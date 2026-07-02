using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace UPLOADER
{
    /// <summary>
    /// Landing dashboard shown right after login. Shows system-wide stats
    /// pulled from hims_srs, and gates navigation to the File Manager to
    /// SuperAdmin only. Each KPI card also exposes a "Manage" button that
    /// jumps straight into the relevant module.
    /// </summary>
    public partial class DashboardAdmin : Form
    {
        private readonly AdminUser _user;

        public DashboardAdmin(AdminUser user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            InitializeComponent();

            // Card styling / rounded badge / hover effects are applied here,
            // AFTER InitializeComponent() returns — not from inside it. See the
            // note in DashboardAdmin.Designer.cs for why (WinForms designer
            // "Missing Form SubType" bug caused by custom method calls inside
            // InitializeComponent()).
            ConfigureDashboardCards();

            Load += DashboardAdmin_Load;
            btnOpenFileManager.Click += BtnOpenFileManager_Click;
            btnRefresh.Click += (s, e) => LoadStats();
            btnLogout.Click += BtnLogout_Click;

            // ── KPI card "Manage" buttons ───────────────────────────────────
            // File-related cards reuse the existing File Manager (SuperAdmin gated).
            btnManageTotalFiles.Click += (s, e) => OpenFileManagerFromCard("Files");
            btnManageTotalFolders.Click += (s, e) => OpenFileManagerFromCard("Folders");
            btnManageRecycleBin.Click += (s, e) => OpenFileManagerFromCard("Recycle Bin");
            btnManageStorage.Click += (s, e) => OpenFileManagerFromCard("Storage");

            // These modules aren't part of this file's scope — wire each one up
            // to your actual management form when it's ready (see ShowModulePlaceholder).
            btnManageTotalUsers.Click += (s, e) => ShowModulePlaceholder("User");
            btnManageOnlineUsers.Click += (s, e) => ShowModulePlaceholder("User");
            btnManageSuggestions.Click += (s, e) => ShowModulePlaceholder("Suggestions");
            btnManageChatToday.Click += (s, e) => ShowModulePlaceholder("Chat");
            btnManageAuditToday.Click += (s, e) => ShowModulePlaceholder("Audit Log");
        }

        // ── Load ─────────────────────────────────────────────────────────────
        private void DashboardAdmin_Load(object? sender, EventArgs e)
        {
            lblWelcome.Text = $"Welcome, {_user.FullName}";
            lblRoleBadge.Text = _user.Role;

            // Only SuperAdmin can jump straight into the File Manager from here.
            bool isSuperAdmin = _user.Role == "SuperAdmin";
            btnOpenFileManager.Visible = isSuperAdmin;
            btnOpenFileManager.Enabled = isSuperAdmin;
            lblFileManagerHint.Visible = !isSuperAdmin;

            // File-manager-backed "Manage" buttons stay visible for context but
            // are disabled (greyed out) for non-SuperAdmin accounts.
            btnManageTotalFiles.Enabled = isSuperAdmin;
            btnManageTotalFolders.Enabled = isSuperAdmin;
            btnManageRecycleBin.Enabled = isSuperAdmin;
            btnManageStorage.Enabled = isSuperAdmin;

            LoadStats();

            AuditLogger.Log(_user, "Dashboard", "ViewDashboard",
                targetName: _user.FullName);
        }

        // ── Stats ────────────────────────────────────────────────────────────
        private void LoadStats()
        {
            try
            {
                using var conn = DbConfig.OpenConnection();

                lblTotalUsersValue.Text = ScalarCount(conn,
                    "SELECT COUNT(*) FROM admins WHERE is_active = 1");

                lblOnlineUsersValue.Text = ScalarCount(conn,
                    "SELECT COUNT(*) FROM admins WHERE last_seen >= DATEADD(MINUTE, -2, SYSDATETIME())");

                lblTotalFilesValue.Text = ScalarCount(conn,
                    "SELECT COUNT(*) FROM opd_file_manager WHERE is_folder = 0 AND is_deleted = 0");

                lblTotalFoldersValue.Text = ScalarCount(conn,
                    "SELECT COUNT(*) FROM opd_file_manager WHERE is_folder = 1 AND is_deleted = 0");

                lblRecycleBinValue.Text = ScalarCount(conn,
                    "SELECT COUNT(*) FROM opd_file_manager WHERE is_deleted = 1");

                lblSuggestionsValue.Text = ScalarCount(conn,
                    "SELECT COUNT(*) FROM hims_suggestions WHERE super_message IS NULL OR super_message = ''");

                lblChatTodayValue.Text = ScalarCount(conn,
                    "SELECT COUNT(*) FROM hims_chat_messages WHERE CAST(created_at AS DATE) = CAST(GETDATE() AS DATE)");

                lblAuditTodayValue.Text = ScalarCount(conn,
                    "SELECT COUNT(*) FROM hims_audit_log WHERE CAST(performed_at AS DATE) = CAST(GETDATE() AS DATE)");

                lblStorageValue.Text = FormatSize(ScalarLong(conn,
                    "SELECT COALESCE(SUM(file_size),0) FROM opd_file_manager WHERE is_folder = 0 AND is_deleted = 0"));

                lblLastUpdated.Text = "Last updated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load dashboard stats: " + ex.Message,
                    "Dashboard Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string ScalarCount(SqlConnection conn, string sql)
        {
            using var cmd = new SqlCommand(sql, conn);
            object? result = cmd.ExecuteScalar();
            return Convert.ToInt64(result ?? 0).ToString("N0");
        }

        private static long ScalarLong(SqlConnection conn, string sql)
        {
            using var cmd = new SqlCommand(sql, conn);
            object? result = cmd.ExecuteScalar();
            return Convert.ToInt64(result ?? 0);
        }

        private static string FormatSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double size = bytes;
            int unitIndex = 0;
            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }
            return $"{size:0.##} {units[unitIndex]}";
        }

        // ── Navigation ───────────────────────────────────────────────────────
        private void BtnOpenFileManager_Click(object? sender, EventArgs e)
        {
            if (_user.Role != "SuperAdmin")
            {
                MessageBox.Show("Only SuperAdmin accounts can open the File Manager from here.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            AuditLogger.Log(_user, "Dashboard", "OpenFileManager",
                targetName: _user.FullName);

            using var fm = new FileManagerForm(_user);
            Hide();
            fm.ShowDialog();
            Show();
            LoadStats();
        }

        /// <summary>
        /// Shared entry point for the four file-backed KPI cards (Files, Folders,
        /// Recycle Bin, Storage). Gated to SuperAdmin the same way the footer button is.
        /// </summary>
        private void OpenFileManagerFromCard(string section)
        {
            if (_user.Role != "SuperAdmin")
            {
                MessageBox.Show("Only SuperAdmin accounts can manage this section.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            AuditLogger.Log(_user, "Dashboard", "OpenFileManager", targetName: section);

            using var fm = new FileManagerForm(_user);
            Hide();
            fm.ShowDialog();
            Show();
            LoadStats();
        }

        /// <summary>
        /// Stand-in for modules not covered by this file (Users, Suggestions, Chat,
        /// Audit Log). Swap this out for a real navigation call once those forms exist,
        /// e.g. `using var f = new UserManagementForm(_user); Hide(); f.ShowDialog(); Show();`
        /// </summary>
        private void ShowModulePlaceholder(string moduleName)
        {
            MessageBox.Show(
                $"The {moduleName} management screen isn't wired up yet.\n\n" +
                $"Hook this button's Click handler up to your {moduleName} form.",
                "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            AuditLogger.Log(_user, "Auth", "Logout", targetName: _user.FullName);
            Application.Restart();
        }

        // ── KPI card styling ────────────────────────────────────────────────
        // Moved here from DashboardAdmin.Designer.cs. Calling custom helper
        // methods (and using lambdas/LINQ) from inside InitializeComponent()
        // is what breaks the WinForms out-of-process designer's ability to
        // host the form ("Missing Form SubType" / "View Designer" disappears
        // from the right-click menu even though the file still compiles fine).
        // The designer's CodeDom-based parser only understands a narrow,
        // linear subset of C# (object construction + simple property/field
        // assignment); everything else — helper method calls, conditionals,
        // loops, lambdas — must live in the regular code-behind file and run
        // AFTER InitializeComponent() has returned, exactly like this.
        private void ConfigureDashboardCards()
        {
            Color cardBg = Color.FromArgb(27, 32, 41);
            Color textMuted = Color.FromArgb(150, 158, 172);
            Color accentBlue = Color.FromArgb(64, 145, 255);
            Color accentGreen = Color.FromArgb(46, 204, 113);
            Color accentIndigo = Color.FromArgb(142, 124, 255);
            Color accentAmber = Color.FromArgb(255, 183, 77);
            Color accentRed = Color.FromArgb(255, 107, 107);
            Color accentCyan = Color.FromArgb(38, 198, 218);
            Color accentPink = Color.FromArgb(255, 122, 198);
            Color accentViolet = Color.FromArgb(179, 136, 255);
            Color accentMint = Color.FromArgb(102, 217, 166);

            ConfigureCard(cardTotalUsers, lblTotalUsersTitle, lblTotalUsersValue, btnManageTotalUsers,
                "👥 Total Users", accentBlue, Color.White, cardBg, textMuted);

            ConfigureCard(cardOnlineUsers, lblOnlineUsersTitle, lblOnlineUsersValue, btnManageOnlineUsers,
                "🟢 Online Now", accentGreen, accentGreen, cardBg, textMuted);

            ConfigureCard(cardTotalFiles, lblTotalFilesTitle, lblTotalFilesValue, btnManageTotalFiles,
                "📄 Total Files", accentIndigo, Color.White, cardBg, textMuted);

            ConfigureCard(cardTotalFolders, lblTotalFoldersTitle, lblTotalFoldersValue, btnManageTotalFolders,
                "📁 Total Folders", accentAmber, Color.White, cardBg, textMuted);

            ConfigureCard(cardRecycleBin, lblRecycleBinTitle, lblRecycleBinValue, btnManageRecycleBin,
                "🗑 Recycle Bin", accentRed, accentRed, cardBg, textMuted);

            ConfigureCard(cardSuggestions, lblSuggestionsTitle, lblSuggestionsValue, btnManageSuggestions,
                "💡 Suggestions", accentCyan, Color.White, cardBg, textMuted);

            ConfigureCard(cardChatToday, lblChatTodayTitle, lblChatTodayValue, btnManageChatToday,
                "💬 Chat Today", accentPink, Color.White, cardBg, textMuted);

            ConfigureCard(cardAuditToday, lblAuditTodayTitle, lblAuditTodayValue, btnManageAuditToday,
                "🛡 Audit Log Today", accentViolet, Color.White, cardBg, textMuted);

            ConfigureCard(cardStorage, lblStorageTitle, lblStorageValue, btnManageStorage,
                "💾 Storage Used", accentMint, Color.White, cardBg, textMuted);

            // Rounded "pill" look for the role badge — applied last, once the
            // panel has its final size.
            ApplyRoundedRegion(pnlRoleBadge, 11);
        }

        /// <summary>
        /// Builds one KPI card: a colored accent bar, a muted caps title, a large value
        /// label, and a bottom-right outlined "Manage" button that the form wires up
        /// to a click handler. Also adds a subtle hover-lighten effect on the whole
        /// card so the grid feels interactive, not just static text on panels.
        /// </summary>
        private static void ConfigureCard(Panel card, Label title, Label value, Button manage,
            string titleText, Color accentColor, Color valueColor, Color cardBg, Color textMuted)
        {
            const int nominalWidth = 320;
            const int nominalHeight = 158;

            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(8);
            card.BackColor = cardBg;
            card.MinimumSize = new Size(220, 130);

            var accentBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 3,
                BackColor = accentColor
            };

            title.AutoSize = false;
            title.Dock = DockStyle.Top;
            title.Height = 38;
            title.Padding = new Padding(18, 14, 14, 0);
            title.ForeColor = textMuted;
            title.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            title.Text = titleText.ToUpperInvariant();

            value.AutoSize = false;
            value.Dock = DockStyle.Fill;
            value.Padding = new Padding(18, 0, 14, 30);
            value.ForeColor = valueColor;
            value.Font = new Font("Segoe UI", 27F, FontStyle.Bold);
            value.TextAlign = ContentAlignment.MiddleLeft;
            value.Text = "0";

            manage.Size = new Size(96, 28);
            manage.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            manage.Location = new Point(nominalWidth - 96 - 14, nominalHeight - 28 - 12);
            manage.Text = "Manage  →";
            manage.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            manage.FlatStyle = FlatStyle.Flat;
            manage.BackColor = cardBg;
            manage.ForeColor = accentColor;
            manage.Cursor = Cursors.Hand;
            manage.FlatAppearance.BorderSize = 1;
            manage.FlatAppearance.BorderColor = accentColor;
            manage.FlatAppearance.MouseOverBackColor = Blend(cardBg, accentColor, 0.18);

            // Order matters: docked children first, the Fill value label next,
            // and the anchored manage button last so it paints above the value text.
            card.Controls.Add(accentBar);
            card.Controls.Add(title);
            card.Controls.Add(value);
            card.Controls.Add(manage);

            // Whole-card hover feedback: lighten slightly on mouse-over so the
            // grid doesn't feel like static, non-interactive text on boxes.
            Color hoverBg = Blend(cardBg, accentColor, 0.06);
            EventHandler enter = (s, e) => card.BackColor = hoverBg;
            EventHandler leave = (s, e) => card.BackColor = cardBg;
            card.MouseEnter += enter;
            card.MouseLeave += leave;
            title.MouseEnter += enter;
            title.MouseLeave += leave;
            value.MouseEnter += enter;
            value.MouseLeave += leave;

            // Disabled "Manage" buttons (e.g. non-SuperAdmin viewing file-backed
            // cards) should look obviously inactive rather than just stop reacting.
            manage.EnabledChanged += (s, e) =>
            {
                manage.ForeColor = manage.Enabled ? accentColor : Color.FromArgb(90, 96, 106);
                manage.FlatAppearance.BorderColor = manage.Enabled ? accentColor : Color.FromArgb(70, 76, 86);
            };
        }

        /// <summary>Linearly blends two colors — used for simple hover-state shading.</summary>
        private static Color Blend(Color a, Color b, double t)
        {
            int r = (int)(a.R + (b.R - a.R) * t);
            int g = (int)(a.G + (b.G - a.G) * t);
            int bl = (int)(a.B + (b.B - a.B) * t);
            return Color.FromArgb(
                Math.Max(0, Math.Min(255, r)),
                Math.Max(0, Math.Min(255, g)),
                Math.Max(0, Math.Min(255, bl)));
        }

        /// <summary>Clips a control into a rounded-rectangle "pill" shape (used for the role badge).</summary>
        private static void ApplyRoundedRegion(Control c, int radius)
        {
            int w = c.Width, h = c.Height;
            if (w <= 0 || h <= 0) return;

            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d = radius * 2;
            path.AddArc(0, 0, d, d, 180, 90);
            path.AddArc(w - d, 0, d, d, 270, 90);
            path.AddArc(w - d, h - d, d, d, 0, 90);
            path.AddArc(0, h - d, d, d, 90, 90);
            path.CloseFigure();

            c.Region?.Dispose();
            c.Region = new Region(path);
        }
    }
}