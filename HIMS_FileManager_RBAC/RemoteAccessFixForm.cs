using Microsoft.Data.SqlClient;

namespace UPLOADER
{
    /// <summary>
    /// Runs on the SQL Server SERVER PC only.
    /// Connects as the SQL Server admin (sa) via localhost and grants remote access to the app login,
    /// creates a dedicated himsopdroot login mapped into the database with db_owner rights.
    /// </summary>
    public class RemoteAccessFixForm : Form
    {
        // ── UI controls ──────────────────────────────────────────────────────
        private TableLayoutPanel? layout;
        private Label lblInfo = null!;
        private Label lblRootUser = null!;
        private TextBox txtRootUser = null!;
        private Label lblRootPass = null!;
        private TextBox txtRootPass = null!;
        private Label lblAppUser = null!;
        private TextBox txtAppUser = null!;
        private Label lblAppPass = null!;
        private TextBox txtAppPass = null!;
        private Label lblPort = null!;
        private TextBox txtPort = null!;
        private Label lblDb = null!;
        private TextBox txtDb = null!;
        private CheckBox chkShowPass = null!;
        private Button btnFix = null!;
        private Button btnClose = null!;
        private RichTextBox rtbLog = null!;
        private ProgressBar pbProgress = null!;

        // ── Dedicated OPD user (hardcoded) ───────────────────────────────────
        private const string OpdUser = "himsopdroot";
        private const string OpdPass = "zahham31";

        // ── Default constructor: reads from DbConfig ─────────────────────────
        public RemoteAccessFixForm()
            : this(
                appUser: DbConfig.Current.DbUser ?? "hims_user",
                appPass: DbConfig.Current.DbPassword ?? "",
                port: DbConfig.Current.MySqlPort > 0 ? DbConfig.Current.MySqlPort : 1433,
                db: DbConfig.Current.Database ?? "hims_srs")
        { }

        // ── Preferred constructor: called from SettingsForm with live values ─
        public RemoteAccessFixForm(string appUser, string appPass, int port, string db)
        {
            BuildUI();
            txtRootUser.Text = "sa";
            txtRootPass.Text = "";        // leave blank if SQL auth uses a known sa password set elsewhere
            txtAppUser.Text = appUser;
            txtAppPass.Text = appPass;
            txtPort.Text = port.ToString();
            txtDb.Text = db;
        }

        // ── Fix logic ────────────────────────────────────────────────────────
        private async void btnFix_Click(object? sender, EventArgs e)
        {
            btnFix.Enabled = false;
            rtbLog.Clear();
            pbProgress.Value = 0;

            string rootUser = txtRootUser.Text.Trim();
            string rootPass = txtRootPass.Text;           // blank = no password
            string appUser = txtAppUser.Text.Trim();
            string appPass = txtAppPass.Text;
            string database = txtDb.Text.Trim();

            if (!int.TryParse(txtPort.Text.Trim(), out int port)) port = 1433;

            if (string.IsNullOrWhiteSpace(appUser) || string.IsNullOrWhiteSpace(database))
            {
                Log("⚠ App username and database name are required.", Color.Orange);
                btnFix.Enabled = true;
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    // ── Step 1: Connect to localhost as the SQL Server admin (sa) ──
                    Invoke(() => { Log("🔌 Connecting to localhost as admin..."); pbProgress.Value = 10; });

                    string connStr = $"Server=127.0.0.1,{port};User Id={rootUser};Password={rootPass};" +
                                      $"TrustServerCertificate=True;Connect Timeout=8;";
                    using var conn = new SqlConnection(connStr);
                    conn.Open();

                    Invoke(() => { Log("✅ Connected to SQL Server as admin.", Color.Green); pbProgress.Value = 20; });

                    // SQL Server has no per-host wildcard concept — a login is either
                    // SQL Server authentication (works from anywhere TCP/IP reaches it)
                    // or Windows authentication. "Remote access" here really means:
                    // 1) the login exists and has SQL auth enabled,
                    // 2) it's mapped to a database user with rights on `database`,
                    // 3) SQL Server is listening on TCP and the firewall allows the port.

                    // ── Step 2: Create/refresh the existing app login ─────────
                    Invoke(() => Log($"⚙ Creating/refreshing login '{appUser}'..."));

                    bool loginExists = false;
                    using (var chk = new SqlCommand(
                        "SELECT COUNT(*) FROM sys.server_principals WHERE name = @u AND type = 'S'", conn))
                    {
                        chk.Parameters.AddWithValue("@u", appUser);
                        loginExists = Convert.ToInt32(chk.ExecuteScalar()) > 0;
                    }

                    if (loginExists)
                        RunSql(conn, $"ALTER LOGIN [{appUser}] WITH PASSWORD = '{EscapeSql(appPass)}', CHECK_POLICY = OFF;");
                    else
                        RunSql(conn, $"CREATE LOGIN [{appUser}] WITH PASSWORD = '{EscapeSql(appPass)}', CHECK_POLICY = OFF;");
                    pbProgress.Value = 45;

                    EnsureDbUserAndOwner(conn, database, appUser);
                    Invoke(() => { Log($"✅ Login '{appUser}' ready with db_owner on '{database}'", Color.Green); pbProgress.Value = 58; });

                    // ── Step 3: Create dedicated himsopdroot login ────────────
                    Invoke(() =>
                    {
                        Log("─────────────────────────────────────────", Color.Gray);
                        Log($"⚙ Creating dedicated OPD login '{OpdUser}'...");
                        pbProgress.Value = 65;
                    });

                    bool opdExists = false;
                    using (var chk = new SqlCommand(
                        "SELECT COUNT(*) FROM sys.server_principals WHERE name = @u AND type = 'S'", conn))
                    {
                        chk.Parameters.AddWithValue("@u", OpdUser);
                        opdExists = Convert.ToInt32(chk.ExecuteScalar()) > 0;
                    }

                    if (opdExists)
                        RunSql(conn, $"ALTER LOGIN [{OpdUser}] WITH PASSWORD = '{EscapeSql(OpdPass)}', CHECK_POLICY = OFF;");
                    else
                        RunSql(conn, $"CREATE LOGIN [{OpdUser}] WITH PASSWORD = '{EscapeSql(OpdPass)}', CHECK_POLICY = OFF;");

                    EnsureDbUserAndOwner(conn, database, OpdUser);

                    Invoke(() =>
                    {
                        Log($"✅ Created login '{OpdUser}' with db_owner on '{database}'", Color.Green);
                        Log($"   Password : {OpdPass}", Color.FromArgb(0, 100, 180));
                        pbProgress.Value = 80;
                    });

                    // ── Done ─────────────────────────────────────────────────
                    Invoke(() =>
                    {
                        pbProgress.Value = 100;
                        Log("─────────────────────────────────────────", Color.Gray);
                        Log("🎉 Done! Remote LAN clients can now connect.", Color.FromArgb(0, 140, 0));
                        Log("", Color.Gray);
                        Log("  App user summary:", Color.FromArgb(0, 100, 180));
                        Log($"   Login : {appUser}", Color.FromArgb(0, 100, 180));
                        Log($"   DB    : {database}", Color.FromArgb(0, 100, 180));
                        Log("", Color.Gray);
                        Log("  Dedicated OPD login:", Color.FromArgb(0, 100, 180));
                        Log($"   Login : {OpdUser}", Color.FromArgb(0, 130, 60));
                        Log($"   Pass  : {OpdPass}", Color.FromArgb(0, 130, 60));
                        Log($"   DB    : {database}", Color.FromArgb(0, 130, 60));
                        Log("─────────────────────────────────────────", Color.Gray);
                        Log("ℹ Also confirm Mixed Mode auth is enabled (SSMS → Server Properties → Security)", Color.DimGray);
                        Log("ℹ and that the SQL Server TCP port is allowed through the Windows Firewall.", Color.DimGray);
                        Log("ℹ Then distribute the app exe to client PCs and set the Server IP to this machine's IP.", Color.DimGray);
                        btnClose.Text = "Close";
                    });
                }
                catch (Exception ex)
                {
                    Invoke(() =>
                    {
                        Log("❌ Error: " + ex.Message, Color.Firebrick);
                        Log("Make sure SQL Server is running and you're on the server PC.", Color.Orange);
                        btnFix.Enabled = true;
                        pbProgress.Value = 0;
                    });
                }
            });
        }

        /// <summary>Ensures the given login has a database user in `database` and is in db_owner.</summary>
        private static void EnsureDbUserAndOwner(SqlConnection conn, string database, string loginName)
        {
            string sql = $@"
USE [{database}];
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'{loginName}')
BEGIN
    CREATE USER [{loginName}] FOR LOGIN [{loginName}];
END
IF IS_ROLEMEMBER('db_owner', N'{loginName}') = 0
BEGIN
    ALTER ROLE db_owner ADD MEMBER [{loginName}];
END";
            RunSql(conn, sql);
        }

        private static void RunSql(SqlConnection conn, string sql)
        {
            using var cmd = new SqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        /// <summary>Escape single quotes in SQL string literals (not for parameterized use).</summary>
        private static string EscapeSql(string s) => s.Replace("'", "''");

        private void Log(string msg, Color? color = null)
        {
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color ?? rtbLog.ForeColor;
            rtbLog.AppendText(msg + "\n");
            rtbLog.ScrollToCaret();
        }

        private void chkShowPass_CheckedChanged(object? sender, EventArgs e)
        {
            txtRootPass.UseSystemPasswordChar = !chkShowPass.Checked;
            txtAppPass.UseSystemPasswordChar = !chkShowPass.Checked;
        }

        // ── UI builder ───────────────────────────────────────────────────────
        private void BuildUI()
        {
            Text = "Fix Remote Access — SQL Server";
            ClientSize = new Size(480, 580);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 247, 252);
            ShowInTaskbar = false;

            // ── Banner ───────────────────────────────────────────────────────
            var banner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.FromArgb(28, 50, 90),
                Padding = new Padding(14, 0, 0, 0),
            };
            var bannerTitle = new Label
            {
                Text = "Fix Remote Access",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(14, 8),
            };
            var bannerSub = new Label
            {
                Text = "Run this ONCE on the SQL Server PC to allow LAN clients",
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(180, 200, 230),
                AutoSize = true,
                Location = new Point(16, 34),
            };
            banner.Controls.AddRange(new Control[] { bannerTitle, bannerSub });

            // ── Body ─────────────────────────────────────────────────────────
            var body = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 12, 20, 8),
                AutoScroll = false,
            };

            lblInfo = MakeLabel(
                "⚠  Run this on the SERVER PC (where SQL Server is installed).\n" +
                "It will create/configure SQL logins so LAN clients can connect.",
                bold: false, color: Color.FromArgb(120, 80, 0));
            lblInfo.BackColor = Color.FromArgb(255, 248, 220);
            lblInfo.BorderStyle = BorderStyle.FixedSingle;
            lblInfo.Padding = new Padding(8, 6, 8, 6);
            lblInfo.Size = new Size(436, 42);
            lblInfo.Location = new Point(0, 4);

            int y = 54;
            Control MakeRow(string labelText, out TextBox box, bool isPass = false, string? hint = null)
            {
                var lbl = MakeLabel(labelText, bold: true);
                lbl.Location = new Point(0, y);
                box = new TextBox
                {
                    Location = new Point(130, y - 2),
                    Size = new Size(306, 24),
                    Font = new Font("Segoe UI", 9.5F),
                    BorderStyle = BorderStyle.FixedSingle,
                    UseSystemPasswordChar = isPass,
                };
                if (hint != null)
                {
                    var h = MakeLabel(hint, bold: false, color: Color.Gray);
                    h.Font = new Font("Segoe UI", 7.5F);
                    h.Location = new Point(130, y + 24);
                    body.Controls.Add(h);
                    y += 10;
                }
                y += 34;
                body.Controls.AddRange(new Control[] { lbl, box });
                return lbl;
            }

            MakeRow("Admin Username:", out txtRootUser);
            MakeRow("Admin Password:", out txtRootPass, isPass: true, hint: "The sa / admin login password");
            MakeRow("App Username:", out txtAppUser, hint: "The user your app connects with");
            MakeRow("App Password:", out txtAppPass, isPass: true);
            MakeRow("SQL Server Port:", out txtPort);
            MakeRow("Database:", out txtDb);

            // Dummy label assignments to satisfy field declarations
            lblRootUser = MakeLabel("Admin Username:", bold: true);
            lblRootPass = MakeLabel("Admin Password:", bold: true);
            lblAppUser = MakeLabel("App Username:", bold: true);
            lblAppPass = MakeLabel("App Password:", bold: true);
            lblPort = MakeLabel("SQL Server Port:", bold: true);
            lblDb = MakeLabel("Database:", bold: true);

            // ── himsopdroot info badge ────────────────────────────────────────
            var lblOpdBadge = new Label
            {
                AutoSize = false,
                Size = new Size(436, 28),
                Location = new Point(0, y),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(0, 100, 50),
                BackColor = Color.FromArgb(220, 248, 230),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(8, 5, 4, 4),
                Text = $"ℹ Will also create dedicated OPD user:  {OpdUser}  /  {OpdPass}",
            };
            y += 34;

            chkShowPass = new CheckBox
            {
                Text = "Show passwords",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.Gray,
                Location = new Point(130, y),
                AutoSize = true,
            };
            chkShowPass.CheckedChanged += chkShowPass_CheckedChanged;
            y += 26;

            // ── Log box ──────────────────────────────────────────────────────
            var lblLog = MakeLabel("Output:", bold: true);
            lblLog.Location = new Point(0, y);
            y += 18;

            rtbLog = new RichTextBox
            {
                Location = new Point(0, y),
                Size = new Size(436, 110),
                Font = new Font("Consolas", 8F),
                BackColor = Color.FromArgb(22, 30, 46),
                ForeColor = Color.FromArgb(180, 220, 255),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical,
            };
            y += 118;

            pbProgress = new ProgressBar
            {
                Location = new Point(0, y),
                Size = new Size(436, 14),
                Minimum = 0,
                Maximum = 100,
                Style = ProgressBarStyle.Continuous,
            };
            y += 22;

            // ── Buttons ──────────────────────────────────────────────────────
            btnFix = new Button
            {
                Text = "▶ Run Fix",
                Location = new Point(0, y),
                Size = new Size(210, 32),
                BackColor = Color.FromArgb(20, 120, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnFix.FlatAppearance.BorderSize = 0;
            btnFix.Click += btnFix_Click;

            btnClose = new Button
            {
                Text = "Cancel",
                Location = new Point(220, y),
                Size = new Size(216, 32),
                BackColor = Color.FromArgb(160, 165, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel,
            };
            btnClose.FlatAppearance.BorderSize = 0;

            body.Controls.AddRange(new Control[]
            {
                lblInfo, lblOpdBadge, chkShowPass, lblLog, rtbLog, pbProgress, btnFix, btnClose
            });

            Controls.Add(body);
            Controls.Add(banner);
            CancelButton = btnClose;
        }

        private void InitializeComponent()
        {

        }

        private static Label MakeLabel(string text, bool bold = false, Color? color = null)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = color ?? Color.FromArgb(28, 50, 90),
                AutoSize = true,
            };
        }
    }
}