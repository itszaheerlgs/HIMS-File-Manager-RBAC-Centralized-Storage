using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace UPLOADER
{
    public partial class LoginForm : Form
    {
        public AdminUser? LoggedInUser { get; private set; }

        // ── PIN state ────────────────────────────────────────────────────────
        private string? _pendingPin;
        private DateTime _pinExpiry;
        private const int PinValidMinutes = 10;
        private bool _suppressIpEvent;
        private const string PinEmail = "detherslagos@gmail.com";

        // ── SMTP credentials ─────────────────────────────────────────────────
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SmtpUser = "justmedether@gmail.com";
        private const string SmtpPass = "hfuv njhx vvkv kzcb";

        public LoginForm()
        {
            InitializeComponent();

            // Repaint input borders on focus change so the highlight works
            txtUsername.Enter += (s, e) => pnlUserBox.Invalidate();
            txtUsername.Leave += (s, e) => pnlUserBox.Invalidate();
            txtPassword.Enter += (s, e) => pnlPassBox.Invalidate();
            txtPassword.Leave += (s, e) => pnlPassBox.Invalidate();

            lblConnInfo.Visible = false;
            cmbIpSuggest.Visible = false;
            PopulateIpDropdown();
        }

        // ── Detect local IPv4 addresses ──────────────────────────────────────
        private List<string> DetectLocalIPs()
        {
            var ips = new List<string>();

            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up) continue;
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                            continue;
                        string ip = ua.Address.ToString();
                        if (!ip.StartsWith("127.")) ips.Add(ip);
                    }
                }
            }
            catch { }

            // Fallback: ipconfig
            try
            {
                var psi = new ProcessStartInfo("ipconfig")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                    var matches = Regex.Matches(output,
                        @"IPv4 Address[\.\s]+:\s*([\d\.]+)",
                        RegexOptions.IgnoreCase);
                    foreach (Match m in matches)
                    {
                        string ip = m.Groups[1].Value
                            .Trim()
                            .TrimEnd('(', 'P', 'r', 'e', 'f', 'e', 'r', 'd', ')')
                            .Trim();
                        if (!ips.Contains(ip) && !ip.StartsWith("127."))
                            ips.Add(ip);
                    }
                }
            }
            catch { }

            return ips
                .Distinct()
                .OrderBy(ip => ip.StartsWith("192.168.") ? 0 :
                               ip.StartsWith("10.") ? 1 :
                               ip.StartsWith("172.") ? 2 : 3)
                .ToList();
        }

        private void PopulateIpDropdown()
        {
            _suppressIpEvent = true;
            try
            {
                var ips = DetectLocalIPs();
                cmbIpSuggest.Items.Clear();

                if (ips.Count == 0)
                {
                    cmbIpSuggest.Items.Add("No local IPs found");
                    cmbIpSuggest.SelectedIndex = 0;
                    cmbIpSuggest.Enabled = false;
                    return;
                }

                string saved = DbConfig.Current.ServerIP ?? "";
                if (!string.IsNullOrEmpty(saved) && !ips.Contains(saved))
                    ips.Insert(0, saved);

                foreach (var ip in ips)
                    cmbIpSuggest.Items.Add(ip);

                int savedIdx = ips.IndexOf(saved);
                cmbIpSuggest.SelectedIndex = savedIdx >= 0 ? savedIdx : 0;
                cmbIpSuggest.Enabled = true;
            }
            finally
            {
                _suppressIpEvent = false;
            }
        }

        // RefreshConnInfo intentionally does nothing on the login screen.
        // Connection details are confidential and only shown inside SettingsForm.
        private void RefreshConnInfo()
        {
            lblConnInfo.Visible = false;
        }

        private void cmbIpSuggest_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressIpEvent) return;
            string selected = cmbIpSuggest.SelectedItem?.ToString() ?? "";
            if (IPAddress.TryParse(selected, out _))
            {
                DbConfig.Current.ServerIP = selected;
                DbConfig.Current.Save();
                // RefreshConnInfo intentionally not called — label stays hidden
            }
        }

        // ── Settings button ──────────────────────────────────────────────────
        private void btnSettings_Click(object? sender, EventArgs e)
        {
            bool dbReachable = TryDbConnection(out string dbError);

            if (dbReachable)
                TrySettingsViaLogin();
            else
                PromptPinBypass(IsHostNotAllowedError(dbError));
        }

        private bool TryDbConnection(out string errorMessage)
        {
            errorMessage = "";
            try
            {
                using var conn = DbConfig.OpenConnection();
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        private static bool IsHostNotAllowedError(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage)) return false;
            return errorMessage.Contains("not allowed to connect", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("Host '", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("1130", StringComparison.OrdinalIgnoreCase);
        }

        // ── Settings via normal SuperAdmin login ─────────────────────────────
        private void TrySettingsViaLogin()
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                PromptPinBypass(false);
                return;
            }

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand("dbo.sp_ValidateLogin", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Username", username);
                using var r = cmd.ExecuteReader();

                if (!r.Read()) { ShowError("Invalid username or password."); return; }

                string storedHash = r.GetString("password_hash");
                string realPassword = r["real_password"] as string ?? "";
                string role = r.GetString("role");
                int adminId = r.GetInt32("admin_id");
                string fullName = r.GetString("full_name");
                r.Close();

                bool ok = password == realPassword
                       || VerifyMd5(password, storedHash)
                       || VerifyPlain(password, storedHash);

                if (!ok) { ShowError("Invalid username or password."); return; }

                if (role != "SuperAdmin")
                {
                    ShowError("Only a SuperAdmin can change connection settings.");
                    AuditLogger.Log(new AdminUser(adminId, username, fullName, role),
                        AuditLogger.ModAuth, "SETTINGS_DENIED",
                        detail: $"Role={role} tried to open Settings.");
                    return;
                }

                lblError.Visible = false;
                DbConfig.SwitchToRole(role);
                OpenSettingsForm();
                AuditLogger.Log(new AdminUser(adminId, username, fullName, role),
                    AuditLogger.ModAuth, "SETTINGS_OPENED");
            }
            catch (Exception ex)
            {
                ShowError("Connection error: " + ex.Message);
            }
        }

        // ── PIN bypass flow ──────────────────────────────────────────────────
        private void PromptPinBypass(bool isHostDenied)
        {
            string extraMsg = isHostDenied
                ? "\n\n⚠ Detected: MariaDB is blocking this PC's hostname.\n" +
                  "After entering Settings, use \"Fix Remote Access\"\n" +
                  "if you are currently on the SERVER PC."
                : "";

            var choice = MessageBox.Show(
                "Database is unreachable or credentials were not entered.\n\n" +
                $"Send a one-time PIN to:\n  {PinEmail}\n\n" +
                "Click Yes to send the PIN, then enter it to open Settings." +
                extraMsg,
                "Settings Access — PIN Required",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (choice == DialogResult.Yes) SendPin();
        }

        private void SendPin()
        {
            _pendingPin = Random.Shared.Next(100000, 999999).ToString();
            _pinExpiry = DateTime.Now.AddMinutes(PinValidMinutes);

            bool sent = false;
            string sendError = "";

            try
            {
                using var mail = new MailMessage();
                mail.From = new MailAddress(SmtpUser, "HIMS File Manager");
                mail.To.Add(PinEmail);
                mail.Subject = "HIMS Settings Access PIN";

                // 1. Tell Gmail to allow colors and fonts
                mail.IsBodyHtml = true;

                // 2. Simple text with basic font and color tags
                mail.Body = $@"
        <div style='font-family: Arial, sans-serif; font-size: 15px; color: #333333; line-height: 1.5;'>
            <p>Your one-time Settings access PIN is:</p>
            
            <p style='font-size: 28px; font-weight: bold; color: #0056b3; letter-spacing: 4px; margin: 15px 0;'>
                {_pendingPin}
            </p>
            
            <p style='color: #c92a2a; font-weight: bold;'>
                This PIN expires in {PinValidMinutes} minutes.
            </p>
            
            <br>
            <span style='font-size: 12px; color: #666666;'>
                <strong>Machine:</strong> {Environment.MachineName}<br>
                <strong>Time:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}
            </span>
        </div>";

                using var smtp = new SmtpClient(SmtpHost, SmtpPort);
                smtp.EnableSsl = true;
                smtp.Credentials = new System.Net.NetworkCredential(SmtpUser, SmtpPass);
                smtp.Timeout = 10000;
                smtp.Send(mail);
                sent = true;
            }
            catch (Exception ex) { sendError = ex.Message; }

            if (!sent)
                MessageBox.Show(
                    $"Could not send email: {sendError}\n\n" +
                    $"Fallback PIN (visible only because email failed):\n\n" +
                    $"  {_pendingPin}\n\nExpires: {_pinExpiry:HH:mm:ss}",
                    "PIN (Email Failed)", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show(
                    $"PIN sent to {PinEmail}.\nIt expires at {_pinExpiry:HH:mm:ss}.",
                    "PIN Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);

            AskForPin();
        }
        private void AskForPin()
        {
            using var dlg = new PinEntryDialog();
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            string entered = dlg.EnteredPin.Trim();

            if (_pendingPin == null || DateTime.Now > _pinExpiry)
            {
                MessageBox.Show("PIN has expired. Please try again.",
                    "Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _pendingPin = null;
                return;
            }

            if (entered != _pendingPin)
            {
                MessageBox.Show("Incorrect PIN.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _pendingPin = null;
            lblError.Visible = false;
            OpenSettingsForm();
        }

        private void OpenSettingsForm()
        {
            using var dlg = new SettingsForm();
            dlg.ShowDialog(this);
            // Do NOT call RefreshConnInfo here — label stays hidden on login screen
            PopulateIpDropdown(); // silently keep DbConfig in sync
        }

        // ── Login ────────────────────────────────────────────────────────────
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter username and password.");
                return;
            }

            try
            {
                using var conn = DbConfig.OpenConnection();
                // Uses a stored procedure rather than a raw SELECT against
                // admins: the bootstrap login (used before we know the role)
                // only has EXECUTE on this proc, not direct table access —
                // see hims_rbac_sqlserver.sql.
                using var cmd = new SqlCommand("dbo.sp_ValidateLogin", conn)
                { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@Username", username);
                using var r = cmd.ExecuteReader();

                if (!r.Read()) { ShowError("Invalid username or password."); return; }

                string storedHash = r.GetString("password_hash");
                string realPassword = r["real_password"] as string ?? "";
                string fullName = r.GetString("full_name");
                string role = r.GetString("role");
                int adminId = r.GetInt32("admin_id");
                string? profilePic = r["profile_pic_path"] as string;
                r.Close();

                bool ok = password == realPassword
                       || VerifyMd5(password, storedHash)
                       || VerifyPlain(password, storedHash);

                if (!ok)
                {
                    ShowError("Invalid username or password.");
                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                using (var upd = new SqlCommand("dbo.sp_UpdateLastLogin", conn)
                       { CommandType = CommandType.StoredProcedure })
                {
                    upd.Parameters.AddWithValue("@AdminId", adminId);
                    upd.ExecuteNonQuery();
                }

                LoggedInUser = new AdminUser(adminId, username, fullName, role)
                { ProfilePicPath = profilePic };

                // From this point on, every OpenConnection() call authenticates
                // to SQL Server as this role's own login — SQL Server's GRANT/
                // DENY rules for that role now govern every query, independent
                // of the WinForms UI.
                DbConfig.SwitchToRole(role);

                AuditLogger.Log(
                    new AdminUser(adminId, username, fullName, role),
                    AuditLogger.ModAuth, AuditLogger.Login,
                    targetName: username);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqlException ex) when (IsHostNotAllowedError(ex.Message))
            {
                ShowError("⚠ This PC is blocked by MariaDB. Click Settings to fix.");
                btnSettings.PerformClick();
            }
            catch (Exception ex)
            {
                ShowError("Connection error: " + ex.Message);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private static bool VerifyMd5(string password, string hash)
        {
            if (hash.Length != 32) return false;
            using var md5 = MD5.Create();
            string computed = Convert.ToHexString(
                md5.ComputeHash(Encoding.UTF8.GetBytes(password))).ToLower();
            return computed == hash.ToLower();
        }

        private static bool VerifyPlain(string password, string hash)
            => string.Equals(password, hash, StringComparison.Ordinal);

        private void ShowError(string msg)
        {
            lblError.Text = msg;
            lblError.Visible = true;
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) btnLogin_Click(sender, e);
        }

        private void chkShow_CheckedChanged(object sender, EventArgs e)
            => txtPassword.UseSystemPasswordChar = !chkShow.Checked;

        private void LoginForm_Load(object sender, EventArgs e) { }

        private void pnlRight_Paint(object sender, PaintEventArgs e)
        {

        }

        private void picUserIcon_Click(object sender, EventArgs e)
        {

        }

        private void lblAppIcon_Click(object sender, EventArgs e)
        {

        }

        private void picUserIcon_Click_1(object sender, EventArgs e)
        {

        }

        // ── Paint handlers (drawn in code so no image assets needed) ─────────
        // Moved here from LoginForm.Designer.cs — see the note in that file.

        // Card: white with a subtle shadow border + gold top accent bar
        private void PnlCard_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var r = pnlCard.ClientRectangle;

            // Gold top bar
            using var goldBrush = new SolidBrush(Color.FromArgb(201, 168, 76));
            g.FillRectangle(goldBrush, 0, 0, r.Width, 4);

            // Subtle border
            using var pen = new Pen(Color.FromArgb(220, 225, 240), 1f);
            g.DrawRectangle(pen, 0, 0, r.Width - 1, r.Height - 1);
        }

        // Input box: border that highlights on focus (repaint triggered by focus events)
        private void PnlInput_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel p) return;
            bool focused = p.Controls.OfType<TextBox>().Any(t => t.Focused);
            var pen = new Pen(focused
                ? Color.FromArgb(15, 37, 75)
                : Color.FromArgb(210, 215, 230), 1.5f);
            e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
            pen.Dispose();
        }

        // Hover effect on the login button
        private void BtnLogin_MouseEnter(object? sender, EventArgs e)
            => btnLogin.BackColor = Color.FromArgb(28, 55, 110);

        private void BtnLogin_MouseLeave(object? sender, EventArgs e)
            => btnLogin.BackColor = Color.FromArgb(15, 37, 75);
    }

    public record AdminUser(int Id, string Username, string FullName, string Role)
    {
        public string? ProfilePicPath { get; init; }
    }
}