using Microsoft.Data.SqlClient;
using System.Data;

namespace UPLOADER
{
    public partial class RegisterUserForm : Form
    {
        private readonly AdminUser _currentUser;
        private byte[]? _profilePicBytes;   // Tracks image raw data
        private string? _profilePicMime;    // Tracks content type extension string
        private string? _selectedPicSourcePath;

        public RegisterUserForm(AdminUser currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
        }

        private void RegisterUserForm_Load(object sender, EventArgs e)
        {
            cmbRole.Items.AddRange(new object[]
            {
                "SuperAdmin",
                "DataManager",
                "Auditor",
                "OPDStaff",
                "CertificationStaff",
                "RecordControllScan",
                "StatisticianStaff"
            });
            cmbRole.SelectedIndex = 1; // default: DataManager
        }

        private void chkShowPass_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPass.Checked;
            txtConfirmPassword.UseSystemPasswordChar = !chkShowPass.Checked;
        }

        private void btnBrowsePic_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select Profile Picture"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            _selectedPicSourcePath = dlg.FileName;
            txtProfilePicPath.Text = Path.GetFileName(dlg.FileName);

            // ── READ IMAGE DATA INTO MEMORY FOR DATABASE STORAGE ──
            try
            {
                _profilePicBytes = File.ReadAllBytes(dlg.FileName);
                
                // Get extension to dynamically calculate accurate MIME type string
                string ext = Path.GetExtension(dlg.FileName).ToLower();
                _profilePicMime = ext switch
                {
                    ".png" => "image/png",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".gif" => "image/gif",
                    ".bmp" => "image/bmp",
                    _ => "application/octet-stream"
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to read image file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _profilePicBytes = null;
                _profilePicMime = null;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_currentUser, Permission.User_Create); }
            catch (PermissionDeniedException ex) { ShowError(ex.Message); return; }

            lblError.Visible = false;

            // ── Validation ──────────────────────────────────────────────
            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;
            string confirm = txtConfirmPassword.Text;
            string role = cmbRole.SelectedItem?.ToString() ?? "DataManager";

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ShowError("Full Name, Username, and Password are required.");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters.");
                return;
            }

            if (password != confirm)
            {
                ShowError("Passwords do not match.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

            try
            {
                using var conn = DbConfig.OpenConnection();

                // Check for existing username/email before insert
                using (var check = new SqlCommand(
                    "SELECT COUNT(*) FROM admins WHERE username = @U OR (email IS NOT NULL AND email = @E)", conn))
                {
                    check.Parameters.AddWithValue("@U", username);
                    check.Parameters.AddWithValue("@E", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                    long count = Convert.ToInt64(check.ExecuteScalar());
                    if (count > 0)
                    {
                        ShowError("Username or email is already taken.");
                        return;
                    }
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                // Local relative backup save path handler
                string? picPath = null;
                if (!string.IsNullOrWhiteSpace(_selectedPicSourcePath))
                {
                    picPath = SaveProfilePicture(_selectedPicSourcePath, username);
                }

                // Updated to include profile_pic_data (MEDIUMBLOB) and profile_pic_mime (VARCHAR)
                using var cmd = new SqlCommand(@"
                    INSERT INTO admins
                        (username, password_hash, real_password, full_name, email,
                         profile_pic_path, profile_pic_data, profile_pic_mime, role, is_active)
                    VALUES
                        ( @Username, @PasswordHash, @RealPassword, @FullName, @Email,
                         @ProfilePicPath, @ProfilePicData, @ProfilePicMime, @Role, @IsActive )", conn);

                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@RealPassword", password); 
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                cmd.Parameters.AddWithValue("@ProfilePicPath", picPath ?? (object)DBNull.Value);
                
                // ── BIND BLOB DATA AND MIME PARAMETERS SAFELY ──
                cmd.Parameters.Add("@ProfilePicData", SqlDbType.VarBinary, -1).Value = (_profilePicBytes != null) ? (object)_profilePicBytes : DBNull.Value;
                cmd.Parameters.AddWithValue("@ProfilePicMime", string.IsNullOrWhiteSpace(_profilePicMime) ? (object)DBNull.Value : _profilePicMime);

                cmd.Parameters.AddWithValue("@Role", role);
                cmd.Parameters.AddWithValue("@IsActive", chkIsActive.Checked ? 1 : 0);

                cmd.ExecuteNonQuery();
                
                AuditLogger.Log(_currentUser,
                    AuditLogger.ModUsers, AuditLogger.AddUser,
                    targetName: username,
                    detail: $"Role={role} | Active={chkIsActive.Checked}");

                MessageBox.Show($"User '{username}' registered successfully as {role}.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqlException ex) when (ex.Number == 1062)
            {
                ShowError("Username or email already exists.");
            }
            catch (Exception ex)
            {
                ShowError("Registration failed: " + ex.Message);
            }
        }

        private static string SaveProfilePicture(string sourcePath, string username)
        {
            string targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProfilePics");
            Directory.CreateDirectory(targetDir);

            string ext = Path.GetExtension(sourcePath);
            string fileName = $"{username}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            string destPath = Path.Combine(targetDir, fileName);

            File.Copy(sourcePath, destPath, overwrite: true);
            return Path.Combine("ProfilePics", fileName);
        }

        private void ShowError(string message)
        {
            lblError.Text = "❌  " + message;
            lblError.Visible = true;
        }

        private static bool IsValidEmail(string email)
        {
            try { return new System.Net.Mail.MailAddress(email).Address == email; }
            catch { return false; }
        }
    }
}