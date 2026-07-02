using Microsoft.Data.SqlClient;

namespace UPLOADER
{
    /// <summary>
    /// LAN Connection Settings form.
    /// Saves to hims_config.json and reloads DbConfig.Current on OK.
    /// </summary>
    public class SettingsForm : Form
    {
        // ── Controls ────────────────────────────────────────────────────────
        private Panel pnlHeader = null!;
        private Label lblTitle = null!, lblSub = null!;
        private Panel pnlBody = null!;
        private TableLayoutPanel mainLayout = null!;

        // Server
        private Label lblServerSection = null!;
        private Label lblServerIP = null!, lblApachePort = null!,
                        lblSslPort = null!, lblMySqlPort = null!;
        private TextBox txtServerIP = null!, txtApachePort = null!,
                        txtSslPort = null!, txtMySqlPort = null!;

        // Database
        private Label lblDbSection = null!;
        private Label lblDatabase = null!, lblDbUser = null!,
                        lblDbPass = null!, lblTimeout = null!;
        private TextBox txtDatabase = null!, txtDbUser = null!,
                        txtDbPass = null!, txtTimeout = null!;
        private CheckBox chkShowPass = null!;

        // Status / URL preview
        private Label lblPreviewSection = null!;
        private Label lblConnStr = null!;

        // Buttons
        private Button btnTest = null!, btnSave = null!, btnCancel = null!;
        private Button btnFixRemote = null!;   // NEW
        private Label lblStatus = null!;

        // Security (SuperAdmin only)
        private CheckBox chkWatermark = null!;
        private Label lblWatermarkStatus = null!;
        private readonly AdminUser? _currentUser;

        // ── Constructor ─────────────────────────────────────────────────────
        public SettingsForm(AdminUser? currentUser = null)
        {
            _currentUser = currentUser;
            BuildUI();
            LoadFromConfig(DbConfig.Current);
            UpdatePreview();
            LoadWatermarkSetting();
        }

        // ── Build UI ────────────────────────────────────────────────────────
        private void BuildUI()
        {
            // Header
            pnlHeader = new Panel
            {
                BackColor = Color.FromArgb(28, 50, 90),
                Dock = DockStyle.Top,
                Height = 78
            };

            lblTitle = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(18, 12),
                Text = "⚙  Connection Settings"
            };

            lblSub = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(170, 195, 230),
                Location = new Point(20, 46),
                Text = "Configure LAN server IP, Apache & SQL Server ports"
            };

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

            // Scrollable body
            pnlBody = new Panel
            {
                AutoScroll = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16),
                BackColor = Color.FromArgb(245, 247, 252)
            };

            mainLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
                Width = 1
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // ── SERVER SECTION ──────────────────────────────────────────────
            AddSection(mainLayout, out lblServerSection, "🌐  Server / Apache");

            AddFieldRow(mainLayout, out lblServerIP, out txtServerIP,
                "Server IP Address", "172.10.102.124", placeholder: "e.g. 172.10.102.124");
            txtServerIP.TextChanged += (s, e) => UpdatePreview();

            AddFieldRowWithNote(mainLayout, out lblApachePort, out txtApachePort,
                "Apache HTTP Port", "80", "(default: 80)", fieldWidth: 80);

            AddFieldRowWithNote(mainLayout, out lblSslPort, out txtSslPort,
                "Apache HTTPS Port", "443", "(default: 443)", fieldWidth: 80);

            AddFieldRowWithNote(mainLayout, out lblMySqlPort, out txtMySqlPort,
                "SQL Server Port", "1433", "(default: 1433)", fieldWidth: 80);

            // ── DATABASE SECTION ────────────────────────────────────────────
            AddSection(mainLayout, out lblDbSection, "🗄  Database Credentials");

            AddFieldRow(mainLayout, out lblDatabase, out txtDatabase, "Database Name", "hims_srs");
            AddFieldRow(mainLayout, out lblDbUser, out txtDbUser, "Username", "root");
            AddFieldRow(mainLayout, out lblDbPass, out txtDbPass, "Password", "");
            txtDbPass.UseSystemPasswordChar = true;

            chkShowPass = new CheckBox
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(130, 0, 0, 8),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.Gray,
                Text = "Show password"
            };
            chkShowPass.CheckedChanged += (s, e) =>
                txtDbPass.UseSystemPasswordChar = !chkShowPass.Checked;
            mainLayout.RowCount++;
            mainLayout.Controls.Add(chkShowPass);

            AddFieldRowWithNote(mainLayout, out lblTimeout, out txtTimeout,
                "Connect Timeout", "10", "seconds", fieldWidth: 60);

            // ── SECURITY SECTION (SuperAdmin only) ──────────────────────────
            AddSection(mainLayout, out var lblSecuritySection, "🔒  Security");

            chkWatermark = new CheckBox
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 40,
                Margin = new Padding(0, 0, 0, 4),
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(50, 60, 80),
                Text = "Stamp downloads & prints with username + timestamp\n(traceability if a file is leaked)"
            };
            chkWatermark.CheckedChanged += ChkWatermark_CheckedChanged;
            mainLayout.RowCount++;
            mainLayout.Controls.Add(chkWatermark);

            lblWatermarkStatus = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 18,
                Margin = new Padding(0, 0, 0, 8),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.Gray,
                Text = ""
            };
            mainLayout.RowCount++;
            mainLayout.Controls.Add(lblWatermarkStatus);

            bool isSuperAdmin = _currentUser?.Role == "SuperAdmin";
            if (!isSuperAdmin)
            {
                chkWatermark.Enabled = false;
                lblWatermarkStatus.Text = _currentUser == null
                    ? "Sign in as SuperAdmin to view or change this setting."
                    : "Only a SuperAdmin can change this setting.";
            }

            // ── CONNECTION PREVIEW ──────────────────────────────────────────
            AddSection(mainLayout, out lblPreviewSection, "🔗  Connection String Preview");

            lblConnStr = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 46,
                Margin = new Padding(0, 0, 0, 14),
                Font = new Font("Consolas", 7.5F),
                ForeColor = Color.FromArgb(50, 80, 130),
                BackColor = Color.FromArgb(230, 235, 248),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(6, 4, 4, 4),
                Text = ""
            };
            mainLayout.RowCount++;
            mainLayout.Controls.Add(lblConnStr);

            // ── STATUS ──────────────────────────────────────────────────────
            lblStatus = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 20,
                Margin = new Padding(0, 0, 0, 8),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.DimGray,
                Text = ""
            };
            mainLayout.RowCount++;
            mainLayout.Controls.Add(lblStatus);

            // ── BUTTONS ROW ──────────────────────────────────────────────────
            var buttonRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 4, 0, 0)
            };

            btnTest = ActionButton("🔌  Test Connection", Color.FromArgb(30, 110, 60), 160);
            btnSave = ActionButton("💾  Save & Apply", Color.FromArgb(28, 50, 90), 140);
            btnCancel = ActionButton("✕  Cancel", Color.FromArgb(130, 50, 50), 100);

            btnTest.Margin = new Padding(0, 0, 8, 0);
            btnSave.Margin = new Padding(0, 0, 8, 0);
            btnCancel.Margin = new Padding(0);

            btnTest.Click += BtnTest_Click;
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            buttonRow.Controls.AddRange(new Control[] { btnTest, btnSave, btnCancel });
            mainLayout.RowCount++;
            mainLayout.Controls.Add(buttonRow);

            // ── FIX REMOTE ACCESS SECTION ─────────────────────────────────
            var lblFixSection = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 22,
                Margin = new Padding(0, 16, 0, 8),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 50, 90),
                Text = "🔧  SQL Server Remote Access",
                BackColor = Color.FromArgb(255, 235, 210),
                Padding = new Padding(4, 3, 0, 0)
            };
            mainLayout.RowCount++;
            mainLayout.Controls.Add(lblFixSection);

            var lblFixNote = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 36,
                Margin = new Padding(0, 0, 0, 8),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 70, 0),
                BackColor = Color.FromArgb(255, 248, 220),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(6, 4, 4, 4),
                Text = "⚠ Run this ONCE on the SERVER PC (where SQL Server is installed)\n" +
                       "to allow other LAN PCs to connect."
            };
            mainLayout.RowCount++;
            mainLayout.Controls.Add(lblFixNote);

            btnFixRemote = ActionButton(
                "🔧  Fix Remote Access (Server PC only)",
                Color.FromArgb(160, 60, 10), 280);
            btnFixRemote.Height = 34;

            var fixRow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 42,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 8)
            };
            fixRow.Controls.Add(btnFixRemote);
            btnFixRemote.Click += BtnFixRemote_Click;

            var tip = new ToolTip();
            tip.SetToolTip(btnFixRemote,
                "Only works when this app is running on the SQL Server PC itself.");

            mainLayout.RowCount++;
            mainLayout.Controls.Add(fixRow);

            pnlBody.Controls.Add(mainLayout);

            // ── Form ────────────────────────────────────────────────────────
            Text = "HIMS File Manager — Connection Settings";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            ClientSize = new Size(500, 640);
            MinimumSize = new Size(420, 520);
            BackColor = Color.White;
            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);

            pnlBody.Resize += (s, e) =>
                mainLayout.Width = pnlBody.ClientSize.Width - pnlBody.Padding.Horizontal;
        }

        // ── Watermark toggle (SuperAdmin only) ──────────────────────────────
        private bool _loadingWatermark = false;

        private void LoadWatermarkSetting()
        {
            _loadingWatermark = true;
            try
            {
                chkWatermark.Checked = AppSettingsService.GetBool(AppSettingsService.WatermarkEnabledKey, defaultValue: false);
                if (chkWatermark.Enabled)
                    lblWatermarkStatus.Text = chkWatermark.Checked
                        ? "Currently ON — downloads and prints are watermarked."
                        : "Currently OFF — downloads and prints are not watermarked.";
            }
            catch
            {
                lblWatermarkStatus.Text = "Could not read this setting (check DB connection).";
            }
            finally
            {
                _loadingWatermark = false;
            }
        }

        private void ChkWatermark_CheckedChanged(object? sender, EventArgs e)
        {
            if (_loadingWatermark || _currentUser == null || _currentUser.Role != "SuperAdmin") return;

            try
            {
                AppSettingsService.SetBool(AppSettingsService.WatermarkEnabledKey, chkWatermark.Checked, _currentUser);
                lblWatermarkStatus.ForeColor = Color.FromArgb(30, 110, 60);
                lblWatermarkStatus.Text = chkWatermark.Checked
                    ? "Saved — downloads and prints are now watermarked."
                    : "Saved — watermarking is now off.";
            }
            catch (Exception ex)
            {
                lblWatermarkStatus.ForeColor = Color.Firebrick;
                lblWatermarkStatus.Text = "Failed to save: " + ex.Message;
                _loadingWatermark = true;
                chkWatermark.Checked = !chkWatermark.Checked; // revert the UI toggle
                _loadingWatermark = false;
            }
        }

        // ── Fix Remote Access ────────────────────────────────────────────────
        private void BtnFixRemote_Click(object? sender, EventArgs e)
        {
            // Pass current form values so RemoteAccessFixForm is pre-filled
            var current = BuildFromForm();
            using var dlg = new RemoteAccessFixForm(
                appUser: current.DbUser,
                appPass: current.DbPassword,
                port: current.MySqlPort,
                db: current.Database);
            dlg.ShowDialog(this);
        }

        // ── Helper builders ─────────────────────────────────────────────────
        private void AddSection(TableLayoutPanel layout, out Label label, string text)
        {
            label = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 22,
                Margin = new Padding(0, 0, 0, 10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 50, 90),
                Text = text,
                BackColor = Color.FromArgb(220, 228, 245),
                Padding = new Padding(4, 3, 0, 0)
            };
            layout.RowCount++;
            layout.Controls.Add(label);
        }

        private void AddFieldRow(TableLayoutPanel layout, out Label label, out TextBox box,
            string labelText, string defaultVal, string? placeholder = null)
        {
            var row = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            label = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(50, 60, 80),
                Text = labelText
            };

            box = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5F),
                Text = defaultVal,
                BorderStyle = BorderStyle.FixedSingle
            };
            if (placeholder != null) box.PlaceholderText = placeholder;

            row.Controls.Add(label, 0, 0);
            row.Controls.Add(box, 1, 0);
            layout.RowCount++;
            layout.Controls.Add(row);
        }

        private void AddFieldRowWithNote(TableLayoutPanel layout, out Label label, out TextBox box,
            string labelText, string defaultVal, string noteText, int fieldWidth)
        {
            var row = new TableLayoutPanel
            {
                ColumnCount = 3,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, fieldWidth));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            label = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(50, 60, 80),
                Text = labelText
            };

            box = new TextBox
            {
                Width = fieldWidth,
                Font = new Font("Segoe UI", 9.5F),
                Text = defaultVal,
                BorderStyle = BorderStyle.FixedSingle
            };

            var note = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(8, 5, 0, 0),
                Font = new Font("Segoe UI", 8F),
                ForeColor = Color.Gray,
                Text = noteText
            };

            row.Controls.Add(label, 0, 0);
            row.Controls.Add(box, 1, 0);
            row.Controls.Add(note, 2, 0);
            layout.RowCount++;
            layout.Controls.Add(row);
        }

        private static Button ActionButton(string text, Color back, int w) =>
            new Button
            {
                Text = text,
                Width = w,
                Height = 32,
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

        // ── Load / Save ─────────────────────────────────────────────────────
        private void LoadFromConfig(AppConfig cfg)
        {
            txtServerIP.Text = cfg.ServerIP;
            txtApachePort.Text = cfg.ApachePort.ToString();
            txtSslPort.Text = cfg.ApacheSsl.ToString();
            txtMySqlPort.Text = cfg.MySqlPort.ToString();
            txtDatabase.Text = cfg.Database;
            txtDbUser.Text = cfg.DbUser;
            txtDbPass.Text = cfg.DbPassword;
            txtTimeout.Text = cfg.ConnTimeout.ToString();
        }

        private AppConfig BuildFromForm()
        {
            return new AppConfig
            {
                ServerIP = txtServerIP.Text.Trim(),
                ApachePort = int.TryParse(txtApachePort.Text, out int ap) ? ap : 80,
                ApacheSsl = int.TryParse(txtSslPort.Text, out int sp) ? sp : 443,
                MySqlPort = int.TryParse(txtMySqlPort.Text, out int mp) ? mp : 1433,
                Database = txtDatabase.Text.Trim(),
                DbUser = txtDbUser.Text.Trim(),
                DbPassword = txtDbPass.Text,
                ConnTimeout = int.TryParse(txtTimeout.Text, out int ct) ? ct : 10
            };
        }

        private void UpdatePreview()
        {
            var cfg = BuildFromForm();
            lblConnStr.Text = cfg.BuildConnectionString();
        }

        // ── Button handlers ─────────────────────────────────────────────────
        private void BtnTest_Click(object? sender, EventArgs e)
        {
            var cfg = BuildFromForm();
            lblStatus.ForeColor = Color.DimGray;
            lblStatus.Text = "⏳ Testing connection…";
            Application.DoEvents();

            try
            {
                using var conn = new SqlConnection(cfg.BuildConnectionString());
                conn.Open();
                lblStatus.ForeColor = Color.FromArgb(20, 120, 50);
                lblStatus.Text = $"✅  Connected!  (Server: {conn.ServerVersion})";
                conn.Close();
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Firebrick;
                lblStatus.Text = "❌  " + ex.Message;
            }
        }

        private void InitializeComponent()
        {

        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var cfg = BuildFromForm();

            if (string.IsNullOrWhiteSpace(cfg.ServerIP))
            {
                lblStatus.ForeColor = Color.Firebrick;
                lblStatus.Text = "❌  Server IP cannot be empty.";
                return;
            }

            try
            {
                cfg.Save();
                DbConfig.Current = cfg;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Firebrick;
                lblStatus.Text = "❌  Save failed: " + ex.Message;
            }
        }
    }
}