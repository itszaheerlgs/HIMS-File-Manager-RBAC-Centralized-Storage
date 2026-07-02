namespace UPLOADER
{
    partial class FileManagerForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileManagerForm));
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            pnlTop = new Panel();
            picProfile = new PictureBox();
            lblFullName = new Label();
            lblUsername = new Label();
            lblRole = new Label();
            btnLogout = new Button();
            pnlSearch = new Panel();
            Search = new PictureBox();
            txtSearch = new TextBox();
            btnClearSearch = new Button();
            chkSearchInFolder = new CheckBox();
            lblBreadcrumb = new Label();
            toolBar = new ToolStrip();
            btnBack = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnNewFolder = new ToolStripButton();
            btnUpload = new ToolStripButton();
            btnBulkUpload = new ToolStripButton();
            btnDownload = new ToolStripButton();
            btnPreview = new ToolStripButton();
            btnRename = new ToolStripButton();
            btnDelete = new ToolStripButton();
            btnRecycleBin2 = new ToolStripButton();
            btnLock = new ToolStripButton();
            btnPrintPreview = new ToolStripButton();
            buttonLogout = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            btnProfile = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            btnChat = new ToolStripButton();
            btnMentionChat = new ToolStripButton();
            btnSuggestions = new ToolStripButton();
            btnSettings = new ToolStripButton();
            btnManageUsers = new ToolStripButton();
            btnUserList = new ToolStripButton();
            btnAuditLog = new ToolStripButton();
            btnDashboard = new ToolStripButton();
            dgv = new DataGridView();
            colIcon = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colType = new DataGridViewTextBoxColumn();
            colSize = new DataGridViewTextBoxColumn();
            colItems = new DataGridViewTextBoxColumn();
            colDate = new DataGridViewTextBoxColumn();
            colBy = new DataGridViewTextBoxColumn();
            pnlProgress = new Panel();
            progressBar = new ProgressBar();
            lblPercent = new Label();
            lblStatus = new Label();
            btnCancelUpload = new Button();
            pnlStatusBar = new Panel();
            lblStatus2 = new Label();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picProfile).BeginInit();
            pnlSearch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)Search).BeginInit();
            toolBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            pnlProgress.SuspendLayout();
            pnlStatusBar.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.FromArgb(28, 50, 90);
            pnlTop.Controls.Add(picProfile);
            pnlTop.Controls.Add(lblFullName);
            pnlTop.Controls.Add(lblUsername);
            pnlTop.Controls.Add(lblRole);
            pnlTop.Controls.Add(btnLogout);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1040, 64);
            pnlTop.TabIndex = 5;
            pnlTop.Paint += pnlTop_Paint;
            // 
            // picProfile
            // 
            picProfile.BackColor = Color.Transparent;
            picProfile.Cursor = Cursors.Hand;
            picProfile.Location = new Point(12, 8);
            picProfile.Name = "picProfile";
            picProfile.Size = new Size(48, 48);
            picProfile.SizeMode = PictureBoxSizeMode.StretchImage;
            picProfile.TabIndex = 6;
            picProfile.TabStop = false;
            picProfile.Click += picProfile_Click;
            picProfile.Paint += picProfile_Paint;
            // 
            // lblFullName
            // 
            lblFullName.AutoSize = true;
            lblFullName.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
            lblFullName.ForeColor = Color.White;
            lblFullName.Location = new Point(68, 8);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new Size(71, 17);
            lblFullName.TabIndex = 0;
            lblFullName.Text = "Full Name";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new Font("Segoe UI", 8.5F);
            lblUsername.ForeColor = Color.FromArgb(164, 173, 189);
            lblUsername.Location = new Point(68, 26);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(59, 15);
            lblUsername.TabIndex = 7;
            lblUsername.Text = "username";
            // 
            // lblRole
            // 
            lblRole.AutoSize = true;
            lblRole.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblRole.ForeColor = Color.LightGray;
            lblRole.Location = new Point(68, 43);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(32, 15);
            lblRole.TabIndex = 8;
            lblRole.Text = "Role";
            // 
            // btnLogout
            // 
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.BackColor = Color.FromArgb(50, 70, 110);
            btnLogout.FlatAppearance.BorderColor = Color.FromArgb(80, 110, 160);
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.Font = new Font("Segoe UI", 8.5F);
            btnLogout.ForeColor = Color.White;
            btnLogout.Location = new Point(1660, 12);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(80, 26);
            btnLogout.TabIndex = 1;
            btnLogout.Text = "\u23fb Logout";
            btnLogout.UseVisualStyleBackColor = false;
            btnLogout.Click += btnLogout_Click;
            // 
            // pnlSearch
            // 
            pnlSearch.BackColor = Color.FromArgb(240, 243, 248);
            pnlSearch.Controls.Add(Search);
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(btnClearSearch);
            pnlSearch.Controls.Add(chkSearchInFolder);
            pnlSearch.Controls.Add(lblBreadcrumb);
            pnlSearch.Dock = DockStyle.Top;
            pnlSearch.Location = new Point(0, 64);
            pnlSearch.Name = "pnlSearch";
            pnlSearch.Size = new Size(1040, 44);
            pnlSearch.TabIndex = 4;
            // 
            // Search
            // 
            Search.BackgroundImage = (Image)resources.GetObject("Search.BackgroundImage");
            Search.BackgroundImageLayout = ImageLayout.Zoom;
            Search.Location = new Point(12, 11);
            Search.Name = "Search";
            Search.Size = new Size(28, 23);
            Search.TabIndex = 6;
            Search.TabStop = false;
            // 
            // txtSearch
            // 
            txtSearch.Font = new Font("Segoe UI", 9.5F);
            txtSearch.Location = new Point(46, 10);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = " Search folders or files…";
            txtSearch.Size = new Size(240, 24);
            txtSearch.TabIndex = 1;
            txtSearch.KeyDown += txtSearch_KeyDown;
            // 
            // btnClearSearch
            // 
            btnClearSearch.FlatAppearance.BorderSize = 0;
            btnClearSearch.FlatStyle = FlatStyle.Flat;
            btnClearSearch.Font = new Font("Segoe UI", 8F);
            btnClearSearch.ForeColor = Color.Gray;
            btnClearSearch.Location = new Point(292, 11);
            btnClearSearch.Name = "btnClearSearch";
            btnClearSearch.Size = new Size(36, 23);
            btnClearSearch.TabIndex = 2;
            btnClearSearch.Text = "✕";
            btnClearSearch.Click += btnClearSearch_Click;
            // 
            // chkSearchInFolder
            // 
            chkSearchInFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkSearchInFolder.AutoSize = true;
            chkSearchInFolder.Checked = true;
            chkSearchInFolder.CheckState = CheckState.Checked;
            chkSearchInFolder.Font = new Font("Segoe UI", 8.5F);
            chkSearchInFolder.ForeColor = Color.DimGray;
            chkSearchInFolder.Location = new Point(885, 13);
            chkSearchInFolder.Name = "chkSearchInFolder";
            chkSearchInFolder.Size = new Size(143, 19);
            chkSearchInFolder.TabIndex = 7;
            chkSearchInFolder.Text = "Search this folder only";
            chkSearchInFolder.UseVisualStyleBackColor = true;
            chkSearchInFolder.CheckedChanged += ChkSearchInFolder_CheckedChanged;
            // 
            // lblBreadcrumb
            // 
            lblBreadcrumb.AutoSize = true;
            lblBreadcrumb.Font = new Font("Segoe UI", 9F);
            lblBreadcrumb.ForeColor = Color.DimGray;
            lblBreadcrumb.Location = new Point(334, 15);
            lblBreadcrumb.Name = "lblBreadcrumb";
            lblBreadcrumb.Size = new Size(47, 15);
            lblBreadcrumb.TabIndex = 3;
            lblBreadcrumb.Text = "📁 Root";
            // 
            // toolBar
            // 
            toolBar.BackColor = Color.FromArgb(50, 70, 110);
            toolBar.GripStyle = ToolStripGripStyle.Hidden;
            toolBar.Items.AddRange(new ToolStripItem[] { btnBack, toolStripSeparator1, btnNewFolder, btnUpload, btnBulkUpload, btnDownload, btnPreview, btnRename, btnDelete, btnRecycleBin2, btnLock, btnPrintPreview, buttonLogout, toolStripSeparator2, btnProfile, toolStripSeparator3, btnChat, btnMentionChat, btnSuggestions, btnSettings, btnManageUsers, btnUserList, btnAuditLog, btnDashboard });
            toolBar.Location = new Point(0, 108);
            toolBar.Name = "toolBar";
            toolBar.Padding = new Padding(4, 0, 0, 0);
            toolBar.Size = new Size(1040, 25);
            toolBar.TabIndex = 3;
            toolBar.ItemClicked += toolBar_ItemClicked;
            // 
            // btnBack
            // 
            btnBack.ForeColor = SystemColors.ButtonFace;
            btnBack.Image = (Image)resources.GetObject("btnBack.Image");
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(23, 22);
            btnBack.ToolTipText = "Back";
            btnBack.Click += btnBack_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // btnNewFolder
            // 
            btnNewFolder.ForeColor = SystemColors.ButtonFace;
            btnNewFolder.Image = (Image)resources.GetObject("btnNewFolder.Image");
            btnNewFolder.Name = "btnNewFolder";
            btnNewFolder.Size = new Size(23, 22);
            btnNewFolder.ToolTipText = "Create New Folder (Current)";
            btnNewFolder.Click += btnNewFolder_Click;
            // 
            // btnUpload
            // 
            btnUpload.ForeColor = SystemColors.ButtonFace;
            btnUpload.Image = (Image)resources.GetObject("btnUpload.Image");
            btnUpload.Name = "btnUpload";
            btnUpload.Size = new Size(23, 22);
            btnUpload.ToolTipText = "Upload";
            btnUpload.Click += btnUpload_Click;
            // 
            // btnBulkUpload
            // 
            btnBulkUpload.ForeColor = SystemColors.ButtonFace;
            btnBulkUpload.Image = (Image)resources.GetObject("btnBulkUpload.Image");
            btnBulkUpload.Name = "btnBulkUpload";
            btnBulkUpload.Size = new Size(23, 22);
            btnBulkUpload.ToolTipText = "Bulk Upload";
            btnBulkUpload.Click += btnBulkUpload_Click;
            // 
            // btnDownload
            // 
            btnDownload.ForeColor = SystemColors.ButtonFace;
            btnDownload.Image = (Image)resources.GetObject("btnDownload.Image");
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(23, 22);
            btnDownload.ToolTipText = "Download";
            btnDownload.Click += btnDownload_Click;
            // 
            // btnPreview
            // 
            btnPreview.AccessibleRole = AccessibleRole.None;
            btnPreview.ForeColor = SystemColors.ButtonFace;
            btnPreview.Image = (Image)resources.GetObject("btnPreview.Image");
            btnPreview.Name = "btnPreview";
            btnPreview.Size = new Size(23, 22);
            btnPreview.ToolTipText = "Preview Image";
            btnPreview.Click += btnPreview_Click;
            // 
            // btnRename
            // 
            btnRename.ForeColor = SystemColors.ButtonFace;
            btnRename.Image = (Image)resources.GetObject("btnRename.Image");
            btnRename.Name = "btnRename";
            btnRename.Size = new Size(23, 22);
            btnRename.ToolTipText = "Rename";
            btnRename.Click += btnRename_Click;
            // 
            // btnDelete
            // 
            btnDelete.Image = (Image)resources.GetObject("btnDelete.Image");
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(23, 22);
            btnDelete.ToolTipText = "Delete";
            btnDelete.Click += btnDelete_Click;
            // 
            // btnRecycleBin2
            // 
            btnRecycleBin2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnRecycleBin2.Image = (Image)resources.GetObject("btnRecycleBin2.Image");
            btnRecycleBin2.ImageTransparentColor = Color.Magenta;
            btnRecycleBin2.Name = "btnRecycleBin2";
            btnRecycleBin2.Size = new Size(23, 22);
            btnRecycleBin2.ToolTipText = "Recycle Bin";
            btnRecycleBin2.Click += btnRecycleBin2_Click;
            // 
            // btnLock
            // 
            btnLock.Image = (Image)resources.GetObject("btnLock.Image");
            btnLock.Name = "btnLock";
            btnLock.Size = new Size(23, 22);
            btnLock.ToolTipText = "Lock";
            btnLock.Click += btnLock_Click;
            // 
            // btnPrintPreview
            // 
            btnPrintPreview.Image = (Image)resources.GetObject("btnPrintPreview.Image");
            btnPrintPreview.Name = "btnPrintPreview";
            btnPrintPreview.Size = new Size(23, 22);
            btnPrintPreview.ToolTipText = "Print Preview";
            btnPrintPreview.Click += btnPrintPreview_Click;
            // 
            // buttonLogout
            // 
            buttonLogout.Alignment = ToolStripItemAlignment.Right;
            buttonLogout.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonLogout.Image = (Image)resources.GetObject("buttonLogout.Image");
            buttonLogout.ImageTransparentColor = Color.Magenta;
            buttonLogout.Name = "buttonLogout";
            buttonLogout.Size = new Size(23, 22);
            buttonLogout.ToolTipText = "Logout";
            buttonLogout.Click += buttonLogout_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // btnProfile
            // 
            btnProfile.Alignment = ToolStripItemAlignment.Right;
            btnProfile.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnProfile.Image = (Image)resources.GetObject("btnProfile.Image");
            btnProfile.ImageTransparentColor = Color.Magenta;
            btnProfile.Name = "btnProfile";
            btnProfile.Size = new Size(23, 22);
            btnProfile.ToolTipText = "Profile";
            btnProfile.Click += btnProfile_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 25);
            // 
            // btnChat
            // 
            btnChat.Alignment = ToolStripItemAlignment.Right;
            btnChat.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnChat.Image = (Image)resources.GetObject("btnChat.Image");
            btnChat.ImageTransparentColor = Color.Magenta;
            btnChat.Name = "btnChat";
            btnChat.Size = new Size(23, 22);
            btnChat.ToolTipText = "Chat";
            btnChat.Click += btnChat_Click;
            // 
            // btnMentionChat
            // 
            btnMentionChat.Alignment = ToolStripItemAlignment.Right;
            btnMentionChat.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnMentionChat.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnMentionChat.Image = (Image)resources.GetObject("btnMentionChat.Image");
            btnMentionChat.Name = "btnMentionChat";
            btnMentionChat.Size = new Size(23, 22);
            btnMentionChat.ToolTipText = "Mention the selected file/folder in chat — notifies whoever you @mention";
            btnMentionChat.Click += btnMentionChat_Click;
            // 
            // btnSuggestions
            // 
            btnSuggestions.Alignment = ToolStripItemAlignment.Right;
            btnSuggestions.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnSuggestions.Image = (Image)resources.GetObject("btnSuggestions.Image");
            btnSuggestions.ImageTransparentColor = Color.Magenta;
            btnSuggestions.Name = "btnSuggestions";
            btnSuggestions.Size = new Size(23, 22);
            btnSuggestions.ToolTipText = "Suggestions";
            btnSuggestions.Click += btnSuggestions_Click;
            // 
            // btnSettings
            // 
            btnSettings.Alignment = ToolStripItemAlignment.Right;
            btnSettings.Image = (Image)resources.GetObject("btnSettings.Image");
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(23, 22);
            btnSettings.ToolTipText = "Settings";
            btnSettings.Click += btnSettings_Click;
            // 
            // btnManageUsers
            // 
            btnManageUsers.Alignment = ToolStripItemAlignment.Right;
            btnManageUsers.Image = (Image)resources.GetObject("btnManageUsers.Image");
            btnManageUsers.Name = "btnManageUsers";
            btnManageUsers.Size = new Size(23, 22);
            btnManageUsers.ToolTipText = "Manage Users";
            btnManageUsers.Click += btnManageUsers_Click;
            // 
            // btnUserList
            // 
            btnUserList.Alignment = ToolStripItemAlignment.Right;
            btnUserList.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnUserList.Image = (Image)resources.GetObject("btnUserList.Image");
            btnUserList.ImageTransparentColor = Color.Magenta;
            btnUserList.Name = "btnUserList";
            btnUserList.Size = new Size(23, 22);
            btnUserList.ToolTipText = "Add New User";
            btnUserList.Click += btnUserList_Click;
            // 
            // btnAuditLog
            // 
            btnAuditLog.Alignment = ToolStripItemAlignment.Right;
            btnAuditLog.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnAuditLog.Image = (Image)resources.GetObject("btnAuditLog.Image");
            btnAuditLog.ImageTransparentColor = Color.Magenta;
            btnAuditLog.Name = "btnAuditLog";
            btnAuditLog.Size = new Size(23, 22);
            btnAuditLog.Text = "toolStripButton1";
            btnAuditLog.ToolTipText = "Audit Log";
            btnAuditLog.Click += btnAuditLog_Click;
            // 
            // btnDashboard
            // 
            btnDashboard.Alignment = ToolStripItemAlignment.Right;
            btnDashboard.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnDashboard.Image = (Image)resources.GetObject("btnDashboard.Image");
            btnDashboard.ImageTransparentColor = Color.Magenta;
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Size = new Size(23, 22);
            btnDashboard.ToolTipText = "Dashboard";
            btnDashboard.Click += btnDashboard_Click;
            // 
            // dgv
            // 
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(245, 248, 255);
            dgv.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(28, 50, 90);
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle4.ForeColor = Color.White;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgv.ColumnHeadersHeight = 32;
            dgv.Columns.AddRange(new DataGridViewColumn[] { colIcon, colName, colType, colSize, colItems, colDate, colBy });
            dgv.Dock = DockStyle.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.Font = new Font("Segoe UI", 9.5F);
            dgv.GridColor = Color.FromArgb(220, 225, 235);
            dgv.Location = new Point(0, 133);
            dgv.Name = "dgv";
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 28;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Size = new Size(1040, 449);
            dgv.TabIndex = 0;
            dgv.CellContentClick += dgv_CellContentClick;
            dgv.CellDoubleClick += dgv_CellDoubleClick;
            dgv.SelectionChanged += dgv_SelectionChanged;
            // 
            // colIcon
            // 
            colIcon.HeaderText = "";
            colIcon.Name = "colIcon";
            colIcon.ReadOnly = true;
            colIcon.Width = 30;
            // 
            // colName
            // 
            colName.HeaderText = "Name";
            colName.Name = "colName";
            colName.ReadOnly = true;
            colName.Width = 280;
            // 
            // colType
            // 
            colType.HeaderText = "Type";
            colType.Name = "colType";
            colType.ReadOnly = true;
            // 
            // colSize
            // 
            colSize.HeaderText = "Size";
            colSize.Name = "colSize";
            colSize.ReadOnly = true;
            colSize.Width = 90;
            // 
            // colItems
            // 
            colItems.HeaderText = "📁/📄";
            colItems.Name = "colItems";
            colItems.ReadOnly = true;
            colItems.Width = 65;
            // 
            // colDate
            // 
            colDate.HeaderText = "Uploaded";
            colDate.Name = "colDate";
            colDate.ReadOnly = true;
            colDate.Width = 140;
            // 
            // colBy
            // 
            colBy.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colBy.HeaderText = "By";
            colBy.Name = "colBy";
            colBy.ReadOnly = true;
            // 
            // pnlProgress
            // 
            pnlProgress.BackColor = Color.FromArgb(240, 243, 248);
            pnlProgress.Controls.Add(progressBar);
            pnlProgress.Controls.Add(lblPercent);
            pnlProgress.Controls.Add(lblStatus);
            pnlProgress.Controls.Add(btnCancelUpload);
            pnlProgress.Dock = DockStyle.Bottom;
            pnlProgress.Location = new Point(0, 582);
            pnlProgress.Name = "pnlProgress";
            pnlProgress.Size = new Size(1040, 52);
            pnlProgress.TabIndex = 1;
            pnlProgress.Visible = false;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(8, 6);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(680, 18);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 0;
            // 
            // lblPercent
            // 
            lblPercent.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPercent.ForeColor = Color.FromArgb(28, 50, 90);
            lblPercent.Location = new Point(695, 6);
            lblPercent.Name = "lblPercent";
            lblPercent.Size = new Size(60, 18);
            lblPercent.TabIndex = 1;
            lblPercent.Text = "0%";
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 8.5F);
            lblStatus.ForeColor = Color.DimGray;
            lblStatus.Location = new Point(8, 28);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(840, 16);
            lblStatus.TabIndex = 2;
            // 
            // btnCancelUpload
            // 
            btnCancelUpload.BackColor = Color.FromArgb(200, 60, 60);
            btnCancelUpload.FlatAppearance.BorderSize = 0;
            btnCancelUpload.FlatStyle = FlatStyle.Flat;
            btnCancelUpload.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnCancelUpload.ForeColor = Color.White;
            btnCancelUpload.Location = new Point(761, 3);
            btnCancelUpload.Name = "btnCancelUpload";
            btnCancelUpload.Size = new Size(82, 22);
            btnCancelUpload.TabIndex = 3;
            btnCancelUpload.Text = "✕ Cancel";
            btnCancelUpload.UseVisualStyleBackColor = false;
            btnCancelUpload.Click += BtnCancelUpload_Click;
            // 
            // pnlStatusBar
            // 
            pnlStatusBar.BackColor = Color.FromArgb(235, 238, 245);
            pnlStatusBar.Controls.Add(lblStatus2);
            pnlStatusBar.Dock = DockStyle.Bottom;
            pnlStatusBar.Location = new Point(0, 634);
            pnlStatusBar.Name = "pnlStatusBar";
            pnlStatusBar.Size = new Size(1040, 26);
            pnlStatusBar.TabIndex = 2;
            // 
            // lblStatus2
            // 
            lblStatus2.AutoSize = true;
            lblStatus2.Font = new Font("Segoe UI", 8.5F);
            lblStatus2.ForeColor = Color.FromArgb(60, 80, 120);
            lblStatus2.Location = new Point(10, 5);
            lblStatus2.Name = "lblStatus2";
            lblStatus2.Size = new Size(0, 15);
            lblStatus2.TabIndex = 0;
            // 
            // FileManagerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1040, 660);
            Controls.Add(dgv);
            Controls.Add(pnlProgress);
            Controls.Add(pnlStatusBar);
            Controls.Add(toolBar);
            Controls.Add(pnlSearch);
            Controls.Add(pnlTop);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FileManagerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "HIMS File Manager : hims_srs";
            Load += FileManagerForm_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picProfile).EndInit();
            pnlSearch.ResumeLayout(false);
            pnlSearch.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)Search).EndInit();
            toolBar.ResumeLayout(false);
            toolBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            pnlProgress.ResumeLayout(false);
            pnlStatusBar.ResumeLayout(false);
            pnlStatusBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        // Moved OUT of InitializeComponent — local functions break the designer
        private static void StyleBtn(ToolStripButton b, string t)
        {
            b.DisplayStyle = ToolStripItemDisplayStyle.Text;
            b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 8.5F);
            b.Text = t;
            b.Padding = new Padding(5, 0, 5, 0);
        }

        // Controls
        private Panel pnlTop, pnlSearch, pnlProgress, pnlStatusBar;
        private PictureBox picProfile;
        private Label lblFullName, lblUsername, lblRole, lblBreadcrumb, lblPercent, lblStatus, lblStatus2;
        private TextBox txtSearch;
        private Button btnLogout, btnClearSearch;
        private CheckBox chkSearchInFolder;
        private ToolStrip toolBar;
        private ToolStripButton btnBack, btnNewFolder, btnUpload, btnBulkUpload,
                                btnDownload, btnPreview, btnRename, btnDelete,
                                btnLock, btnPrintPreview, btnManageUsers;
        private ToolStripButton btnSettings;
        private DataGridView dgv;
        private DataGridViewTextBoxColumn colIcon, colName, colType, colSize,
                                          colItems, colDate, colBy;
        private ProgressBar progressBar;
        private Button btnCancelUpload;
        private ToolStripButton buttonLogout;
        private ToolStripButton btnProfile;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnUserList;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton btnAuditLog;
        private ToolStripButton btnChat;
        private ToolStripButton btnMentionChat;
        private ToolStripButton btnSuggestions;
        private ToolStripButton btnRecycleBin2;
        private ToolStripButton btnDashboard;
        private PictureBox Search;
    }
}