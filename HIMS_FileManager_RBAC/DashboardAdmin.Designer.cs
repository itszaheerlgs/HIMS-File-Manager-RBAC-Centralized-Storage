using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UPLOADER
{
    partial class DashboardAdmin
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // ── Header ───────────────────────────────────────────────────────────
        private Panel pnlHeader;
        private Panel headerSeparator;
        private Label lblWelcome;
        private Panel pnlRoleBadge;
        private Label lblRoleBadge;
        private Label lblLastUpdated;
        private Button btnRefresh;
        private Button btnLogout;

        // ── Stat cards ───────────────────────────────────────────────────────
        private Panel pnlStats;
        private Label lblSectionTitle;
        private TableLayoutPanel tlpCards;

        private Panel cardTotalUsers;
        private Label lblTotalUsersTitle;
        private Label lblTotalUsersValue;
        private Button btnManageTotalUsers;

        private Panel cardOnlineUsers;
        private Label lblOnlineUsersTitle;
        private Label lblOnlineUsersValue;
        private Button btnManageOnlineUsers;

        private Panel cardTotalFiles;
        private Label lblTotalFilesTitle;
        private Label lblTotalFilesValue;
        private Button btnManageTotalFiles;

        private Panel cardTotalFolders;
        private Label lblTotalFoldersTitle;
        private Label lblTotalFoldersValue;
        private Button btnManageTotalFolders;

        private Panel cardRecycleBin;
        private Label lblRecycleBinTitle;
        private Label lblRecycleBinValue;
        private Button btnManageRecycleBin;

        private Panel cardSuggestions;
        private Label lblSuggestionsTitle;
        private Label lblSuggestionsValue;
        private Button btnManageSuggestions;

        private Panel cardChatToday;
        private Label lblChatTodayTitle;
        private Label lblChatTodayValue;
        private Button btnManageChatToday;

        private Panel cardAuditToday;
        private Label lblAuditTodayTitle;
        private Label lblAuditTodayValue;
        private Button btnManageAuditToday;

        private Panel cardStorage;
        private Label lblStorageTitle;
        private Label lblStorageValue;
        private Button btnManageStorage;

        // ── Footer / navigation ──────────────────────────────────────────────
        private Panel pnlFooter;
        private Panel footerSeparator;
        private Button btnOpenFileManager;
        private Label lblFileManagerHint;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardAdmin));

            // ── Palette ──────────────────────────────────────────────────────
            // One place to tweak the whole theme. Cards share the same dark
            // surface color; each just gets a different accent so the grid
            // reads as one cohesive dashboard rather than 9 random colors.
            Color pageBg = Color.FromArgb(17, 20, 27);
            Color surfaceBg = Color.FromArgb(22, 26, 34);
            Color cardBg = Color.FromArgb(27, 32, 41);
            Color dividerColor = Color.FromArgb(38, 44, 56);
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

            pnlHeader = new Panel();
            headerSeparator = new Panel();
            lblWelcome = new Label();
            pnlRoleBadge = new Panel();
            lblRoleBadge = new Label();
            lblLastUpdated = new Label();
            btnRefresh = new Button();
            btnLogout = new Button();
            pnlStats = new Panel();
            lblSectionTitle = new Label();
            tlpCards = new TableLayoutPanel();
            cardTotalUsers = new Panel();
            cardOnlineUsers = new Panel();
            cardTotalFiles = new Panel();
            cardTotalFolders = new Panel();
            cardRecycleBin = new Panel();
            cardSuggestions = new Panel();
            cardChatToday = new Panel();
            cardAuditToday = new Panel();
            cardStorage = new Panel();
            lblTotalUsersTitle = new Label();
            lblTotalUsersValue = new Label();
            btnManageTotalUsers = new Button();
            lblOnlineUsersTitle = new Label();
            lblOnlineUsersValue = new Label();
            btnManageOnlineUsers = new Button();
            lblTotalFilesTitle = new Label();
            lblTotalFilesValue = new Label();
            btnManageTotalFiles = new Button();
            lblTotalFoldersTitle = new Label();
            lblTotalFoldersValue = new Label();
            btnManageTotalFolders = new Button();
            lblRecycleBinTitle = new Label();
            lblRecycleBinValue = new Label();
            btnManageRecycleBin = new Button();
            lblSuggestionsTitle = new Label();
            lblSuggestionsValue = new Label();
            btnManageSuggestions = new Button();
            lblChatTodayTitle = new Label();
            lblChatTodayValue = new Label();
            btnManageChatToday = new Button();
            lblAuditTodayTitle = new Label();
            lblAuditTodayValue = new Label();
            btnManageAuditToday = new Button();
            lblStorageTitle = new Label();
            lblStorageValue = new Label();
            btnManageStorage = new Button();
            pnlFooter = new Panel();
            footerSeparator = new Panel();
            btnOpenFileManager = new Button();
            lblFileManagerHint = new Label();
            pnlHeader.SuspendLayout();
            pnlRoleBadge.SuspendLayout();
            pnlStats.SuspendLayout();
            tlpCards.SuspendLayout();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            //
            // pnlHeader
            //
            pnlHeader.BackColor = surfaceBg;
            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Controls.Add(pnlRoleBadge);
            pnlHeader.Controls.Add(lblLastUpdated);
            pnlHeader.Controls.Add(btnRefresh);
            pnlHeader.Controls.Add(btnLogout);
            pnlHeader.Controls.Add(headerSeparator);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(1100, 76);
            pnlHeader.TabIndex = 2;
            //
            // headerSeparator — thin 1px divider along the bottom edge of the
            // header, replacing what used to be an unstyled 200x100 stray panel.
            //
            headerSeparator.BackColor = dividerColor;
            headerSeparator.Dock = DockStyle.Bottom;
            headerSeparator.Height = 1;
            headerSeparator.Name = "headerSeparator";
            headerSeparator.TabIndex = 6;
            //
            // lblWelcome
            //
            lblWelcome.AutoSize = true;
            lblWelcome.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblWelcome.ForeColor = Color.White;
            lblWelcome.Location = new Point(28, 14);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(99, 28);
            lblWelcome.TabIndex = 1;
            lblWelcome.Text = "Welcome";
            //
            // pnlRoleBadge — small rounded "pill" chip around the role label,
            // styled like a status badge instead of a plain text label.
            //
            pnlRoleBadge.BackColor = Color.FromArgb(36, 46, 68);
            pnlRoleBadge.Controls.Add(lblRoleBadge);
            pnlRoleBadge.Location = new Point(30, 45);
            pnlRoleBadge.Name = "pnlRoleBadge";
            pnlRoleBadge.Padding = new Padding(10, 3, 10, 3);
            pnlRoleBadge.Size = new Size(90, 22);
            pnlRoleBadge.TabIndex = 7;
            //
            // lblRoleBadge
            //
            lblRoleBadge.AutoSize = false;
            lblRoleBadge.Dock = DockStyle.Fill;
            lblRoleBadge.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblRoleBadge.ForeColor = accentBlue;
            lblRoleBadge.Name = "lblRoleBadge";
            lblRoleBadge.TabIndex = 2;
            lblRoleBadge.Text = "Role";
            lblRoleBadge.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblLastUpdated
            //
            lblLastUpdated.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblLastUpdated.Font = new Font("Segoe UI", 8.5F);
            lblLastUpdated.ForeColor = textMuted;
            lblLastUpdated.Location = new Point(732, 58);
            lblLastUpdated.Name = "lblLastUpdated";
            lblLastUpdated.Size = new Size(340, 16);
            lblLastUpdated.TabIndex = 3;
            lblLastUpdated.Text = "Last updated: —";
            lblLastUpdated.TextAlign = ContentAlignment.MiddleRight;
            //
            // btnRefresh
            //
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.BackColor = Color.FromArgb(44, 50, 62);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatAppearance.MouseOverBackColor = Color.FromArgb(58, 66, 82);
            btnRefresh.FlatAppearance.MouseDownBackColor = Color.FromArgb(34, 39, 49);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.Location = new Point(878, 21);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(92, 34);
            btnRefresh.TabIndex = 4;
            btnRefresh.Text = "⟲  Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            //
            // btnLogout
            //
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.BackColor = accentRed;
            btnLogout.Cursor = Cursors.Hand;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 90, 90);
            btnLogout.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 75, 75);
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLogout.ForeColor = Color.White;
            btnLogout.Location = new Point(980, 21);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(92, 34);
            btnLogout.TabIndex = 5;
            btnLogout.Text = "⏻  Logout";
            btnLogout.UseVisualStyleBackColor = false;
            //
            // pnlStats
            //
            pnlStats.BackColor = pageBg;
            pnlStats.Controls.Add(tlpCards);
            pnlStats.Controls.Add(lblSectionTitle);
            pnlStats.Dock = DockStyle.Fill;
            pnlStats.Location = new Point(0, 76);
            pnlStats.Name = "pnlStats";
            pnlStats.Padding = new Padding(24, 20, 24, 24);
            pnlStats.Size = new Size(1100, 560);
            pnlStats.TabIndex = 0;
            //
            // lblSectionTitle
            //
            lblSectionTitle.AutoSize = false;
            lblSectionTitle.Dock = DockStyle.Top;
            lblSectionTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSectionTitle.ForeColor = textMuted;
            lblSectionTitle.Height = 28;
            lblSectionTitle.Name = "lblSectionTitle";
            lblSectionTitle.Padding = new Padding(2, 0, 0, 0);
            lblSectionTitle.TabIndex = 1;
            lblSectionTitle.Text = "SYSTEM OVERVIEW";
            lblSectionTitle.TextAlign = ContentAlignment.BottomLeft;
            //
            // tlpCards
            //
            tlpCards.BackColor = pageBg;
            tlpCards.ColumnCount = 3;
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpCards.Controls.Add(cardTotalUsers, 0, 0);
            tlpCards.Controls.Add(cardOnlineUsers, 1, 0);
            tlpCards.Controls.Add(cardTotalFiles, 2, 0);
            tlpCards.Controls.Add(cardTotalFolders, 0, 1);
            tlpCards.Controls.Add(cardRecycleBin, 1, 1);
            tlpCards.Controls.Add(cardSuggestions, 2, 1);
            tlpCards.Controls.Add(cardChatToday, 0, 2);
            tlpCards.Controls.Add(cardAuditToday, 1, 2);
            tlpCards.Controls.Add(cardStorage, 2, 2);
            tlpCards.Dock = DockStyle.Fill;
            tlpCards.Location = new Point(24, 48);
            tlpCards.Name = "tlpCards";
            tlpCards.RowCount = 3;
            tlpCards.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));
            tlpCards.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tlpCards.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            tlpCards.Size = new Size(1052, 488);
            tlpCards.TabIndex = 0;
            //
            // cardTotalUsers
            //
            cardTotalUsers.Location = new Point(3, 3);
            cardTotalUsers.Name = "cardTotalUsers";
            cardTotalUsers.Size = new Size(200, 100);
            cardTotalUsers.TabIndex = 0;
            //
            // cardOnlineUsers
            //
            cardOnlineUsers.Location = new Point(353, 3);
            cardOnlineUsers.Name = "cardOnlineUsers";
            cardOnlineUsers.Size = new Size(200, 100);
            cardOnlineUsers.TabIndex = 1;
            //
            // cardTotalFiles
            //
            cardTotalFiles.Location = new Point(703, 3);
            cardTotalFiles.Name = "cardTotalFiles";
            cardTotalFiles.Size = new Size(200, 100);
            cardTotalFiles.TabIndex = 2;
            //
            // cardTotalFolders
            //
            cardTotalFolders.Location = new Point(3, 165);
            cardTotalFolders.Name = "cardTotalFolders";
            cardTotalFolders.Size = new Size(200, 100);
            cardTotalFolders.TabIndex = 3;
            //
            // cardRecycleBin
            //
            cardRecycleBin.Location = new Point(353, 165);
            cardRecycleBin.Name = "cardRecycleBin";
            cardRecycleBin.Size = new Size(200, 100);
            cardRecycleBin.TabIndex = 4;
            //
            // cardSuggestions
            //
            cardSuggestions.Location = new Point(703, 165);
            cardSuggestions.Name = "cardSuggestions";
            cardSuggestions.Size = new Size(200, 100);
            cardSuggestions.TabIndex = 5;
            //
            // cardChatToday
            //
            cardChatToday.Location = new Point(3, 327);
            cardChatToday.Name = "cardChatToday";
            cardChatToday.Size = new Size(200, 100);
            cardChatToday.TabIndex = 6;
            //
            // cardAuditToday
            //
            cardAuditToday.Location = new Point(353, 327);
            cardAuditToday.Name = "cardAuditToday";
            cardAuditToday.Size = new Size(200, 100);
            cardAuditToday.TabIndex = 7;
            //
            // cardStorage
            //
            cardStorage.Location = new Point(703, 327);
            cardStorage.Name = "cardStorage";
            cardStorage.Size = new Size(200, 100);
            cardStorage.TabIndex = 8;
            //
            // KPI card labels / buttons — base field setup. ConfigureCard()
            // below does the real styling (icon title, big value, accent bar,
            // hover effects) and parents them into their card panel.
            //
            lblTotalUsersTitle.Name = "lblTotalUsersTitle";
            lblTotalUsersValue.Name = "lblTotalUsersValue";
            btnManageTotalUsers.Name = "btnManageTotalUsers";
            lblOnlineUsersTitle.Name = "lblOnlineUsersTitle";
            lblOnlineUsersValue.Name = "lblOnlineUsersValue";
            btnManageOnlineUsers.Name = "btnManageOnlineUsers";
            lblTotalFilesTitle.Name = "lblTotalFilesTitle";
            lblTotalFilesValue.Name = "lblTotalFilesValue";
            btnManageTotalFiles.Name = "btnManageTotalFiles";
            lblTotalFoldersTitle.Name = "lblTotalFoldersTitle";
            lblTotalFoldersValue.Name = "lblTotalFoldersValue";
            btnManageTotalFolders.Name = "btnManageTotalFolders";
            lblRecycleBinTitle.Name = "lblRecycleBinTitle";
            lblRecycleBinValue.Name = "lblRecycleBinValue";
            btnManageRecycleBin.Name = "btnManageRecycleBin";
            lblSuggestionsTitle.Name = "lblSuggestionsTitle";
            lblSuggestionsValue.Name = "lblSuggestionsValue";
            btnManageSuggestions.Name = "btnManageSuggestions";
            lblChatTodayTitle.Name = "lblChatTodayTitle";
            lblChatTodayValue.Name = "lblChatTodayValue";
            btnManageChatToday.Name = "btnManageChatToday";
            lblAuditTodayTitle.Name = "lblAuditTodayTitle";
            lblAuditTodayValue.Name = "lblAuditTodayValue";
            btnManageAuditToday.Name = "btnManageAuditToday";
            lblStorageTitle.Name = "lblStorageTitle";
            lblStorageValue.Name = "lblStorageValue";
            btnManageStorage.Name = "btnManageStorage";
            //
            // NOTE: KPI card styling (accent bar, title/value fonts, hover glow)
            // is intentionally NOT done here. Calling a custom helper method
            // from inside InitializeComponent() breaks the WinForms designer's
            // ability to parse/host this file (the "Missing Form SubType" /
            // "View Designer option disappeared" bug). That styling now lives
            // in DashboardAdmin.cs → ConfigureDashboardCards(), called once
            // from the constructor right after InitializeComponent().
            //
            // pnlFooter
            //
            pnlFooter.BackColor = surfaceBg;
            pnlFooter.Controls.Add(footerSeparator);
            pnlFooter.Controls.Add(btnOpenFileManager);
            pnlFooter.Controls.Add(lblFileManagerHint);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 636);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(1100, 84);
            pnlFooter.TabIndex = 1;
            //
            // footerSeparator — thin 1px divider along the top edge of the
            // footer, replacing what used to be an unstyled stray panel.
            //
            footerSeparator.BackColor = dividerColor;
            footerSeparator.Dock = DockStyle.Top;
            footerSeparator.Height = 1;
            footerSeparator.Name = "footerSeparator";
            footerSeparator.TabIndex = 0;
            //
            // btnOpenFileManager
            //
            btnOpenFileManager.BackColor = accentBlue;
            btnOpenFileManager.Cursor = Cursors.Hand;
            btnOpenFileManager.FlatAppearance.BorderSize = 0;
            btnOpenFileManager.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 163, 255);
            btnOpenFileManager.FlatAppearance.MouseDownBackColor = Color.FromArgb(48, 122, 224);
            btnOpenFileManager.FlatStyle = FlatStyle.Flat;
            btnOpenFileManager.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnOpenFileManager.ForeColor = Color.White;
            btnOpenFileManager.Location = new Point(28, 19);
            btnOpenFileManager.Name = "btnOpenFileManager";
            btnOpenFileManager.Size = new Size(230, 46);
            btnOpenFileManager.TabIndex = 1;
            btnOpenFileManager.Text = "Open File Manager   →";
            btnOpenFileManager.UseVisualStyleBackColor = false;
            //
            // lblFileManagerHint
            //
            lblFileManagerHint.AutoSize = true;
            lblFileManagerHint.Font = new Font("Segoe UI", 9.5F);
            lblFileManagerHint.ForeColor = textMuted;
            lblFileManagerHint.Location = new Point(28, 33);
            lblFileManagerHint.Name = "lblFileManagerHint";
            lblFileManagerHint.Size = new Size(347, 17);
            lblFileManagerHint.TabIndex = 2;
            lblFileManagerHint.Text = "🔒 File Manager access is restricted to SuperAdmin accounts.";
            lblFileManagerHint.Visible = false;
            //
            // DashboardAdmin
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = pageBg;
            ClientSize = new Size(1100, 720);
            Controls.Add(pnlStats);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 9F);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(980, 640);
            Name = "DashboardAdmin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HIMS : Dashboard";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlRoleBadge.ResumeLayout(false);
            pnlStats.ResumeLayout(false);
            tlpCards.ResumeLayout(false);
            pnlFooter.ResumeLayout(false);
            pnlFooter.PerformLayout();
            ResumeLayout(false);

            // NOTE: rounded "pill" look for the role badge is applied post-construction
            // in DashboardAdmin.cs (ConfigureDashboardCards), not here — see note above.
        }
    }
}