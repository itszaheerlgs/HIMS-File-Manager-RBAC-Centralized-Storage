namespace UPLOADER
{
    partial class RecycleBinForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecycleBinForm));
            dgv = new DataGridView();
            colIcon = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colType = new DataGridViewTextBoxColumn();
            colSize = new DataGridViewTextBoxColumn();
            colDeletedBy = new DataGridViewTextBoxColumn();
            colDeletedAt = new DataGridViewTextBoxColumn();
            panelTop = new Panel();
            progressBarRB = new ProgressBar();
            lblPercent = new Label();
            lblStatus = new Label();
            lblTitle = new Label();
            lblCount = new Label();
            panelBottom = new FlowLayoutPanel();
            btnRestore = new Button();
            btnDeletePermanently = new Button();
            btnEmptyBin = new Button();
            btnRefresh = new Button();
            btnClose = new Button();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            panelTop.SuspendLayout();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // dgv
            // 
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.BackgroundColor = Color.White;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.Columns.AddRange(new DataGridViewColumn[] { colIcon, colName, colType, colSize, colDeletedBy, colDeletedAt });
            dgv.Dock = DockStyle.Fill;
            dgv.Location = new Point(0, 56);
            dgv.Name = "dgv";
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Size = new Size(820, 463);
            dgv.TabIndex = 0;
            dgv.CellContentClick += dgv_CellContentClick;
            dgv.SelectionChanged += dgv_SelectionChanged;
            // 
            // colIcon
            // 
            colIcon.HeaderText = "";
            colIcon.Name = "colIcon";
            colIcon.ReadOnly = true;
            colIcon.Width = 36;
            // 
            // colName
            // 
            colName.HeaderText = "Name";
            colName.Name = "colName";
            colName.ReadOnly = true;
            colName.Width = 260;
            // 
            // colType
            // 
            colType.HeaderText = "Type";
            colType.Name = "colType";
            colType.ReadOnly = true;
            colType.Width = 110;
            // 
            // colSize
            // 
            colSize.HeaderText = "Size";
            colSize.Name = "colSize";
            colSize.ReadOnly = true;
            colSize.Width = 90;
            // 
            // colDeletedBy
            // 
            colDeletedBy.HeaderText = "Deleted By";
            colDeletedBy.Name = "colDeletedBy";
            colDeletedBy.ReadOnly = true;
            colDeletedBy.Width = 140;
            // 
            // colDeletedAt
            // 
            colDeletedAt.HeaderText = "Deleted At";
            colDeletedAt.Name = "colDeletedAt";
            colDeletedAt.ReadOnly = true;
            colDeletedAt.Width = 150;
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(243, 244, 246);
            panelTop.Controls.Add(progressBarRB);
            panelTop.Controls.Add(lblPercent);
            panelTop.Controls.Add(lblStatus);
            panelTop.Controls.Add(lblTitle);
            panelTop.Controls.Add(lblCount);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(820, 56);
            panelTop.TabIndex = 2;
            // 
            // progressBarRB
            // 
            progressBarRB.Location = new Point(238, 9);
            progressBarRB.Name = "progressBarRB";
            progressBarRB.Size = new Size(512, 18);
            progressBarRB.Style = ProgressBarStyle.Continuous;
            progressBarRB.TabIndex = 3;
            // 
            // lblPercent
            // 
            lblPercent.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPercent.ForeColor = Color.FromArgb(28, 50, 90);
            lblPercent.Location = new Point(756, 9);
            lblPercent.Name = "lblPercent";
            lblPercent.Size = new Size(52, 18);
            lblPercent.TabIndex = 4;
            lblPercent.Text = "0%";
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 8.5F);
            lblStatus.ForeColor = Color.DimGray;
            lblStatus.Location = new Point(238, 31);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(570, 16);
            lblStatus.TabIndex = 5;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.Location = new Point(16, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(146, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "🗑️  Recycle Bin";
            // 
            // lblCount
            // 
            lblCount.AutoSize = true;
            lblCount.Font = new Font("Segoe UI", 9F);
            lblCount.ForeColor = Color.FromArgb(100, 116, 139);
            lblCount.Location = new Point(18, 36);
            lblCount.Name = "lblCount";
            lblCount.Size = new Size(129, 15);
            lblCount.TabIndex = 1;
            lblCount.Text = "0 item(s) in Recycle Bin";
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(btnRestore);
            panelBottom.Controls.Add(btnDeletePermanently);
            panelBottom.Controls.Add(btnEmptyBin);
            panelBottom.Controls.Add(btnRefresh);
            panelBottom.Controls.Add(btnClose);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.FlowDirection = FlowDirection.RightToLeft;
            panelBottom.Location = new Point(0, 519);
            panelBottom.Name = "panelBottom";
            panelBottom.Padding = new Padding(10);
            panelBottom.Size = new Size(820, 53);
            panelBottom.TabIndex = 1;
            // 
            // btnRestore
            // 
            btnRestore.AutoSize = true;
            btnRestore.Enabled = false;
            btnRestore.Location = new Point(711, 16);
            btnRestore.Margin = new Padding(6, 6, 0, 6);
            btnRestore.Name = "btnRestore";
            btnRestore.Padding = new Padding(10, 4, 10, 4);
            btnRestore.Size = new Size(89, 33);
            btnRestore.TabIndex = 0;
            btnRestore.Text = "↩ Restore";
            btnRestore.Click += btnRestore_Click;
            // 
            // btnDeletePermanently
            // 
            btnDeletePermanently.AutoSize = true;
            btnDeletePermanently.Enabled = false;
            btnDeletePermanently.ForeColor = Color.Firebrick;
            btnDeletePermanently.Location = new Point(565, 16);
            btnDeletePermanently.Margin = new Padding(6, 6, 0, 6);
            btnDeletePermanently.Name = "btnDeletePermanently";
            btnDeletePermanently.Padding = new Padding(10, 4, 10, 4);
            btnDeletePermanently.Size = new Size(140, 33);
            btnDeletePermanently.TabIndex = 1;
            btnDeletePermanently.Text = "Delete Permanently";
            btnDeletePermanently.Click += btnDeletePermanently_Click;
            // 
            // btnEmptyBin
            // 
            btnEmptyBin.AutoSize = true;
            btnEmptyBin.ForeColor = Color.Firebrick;
            btnEmptyBin.Location = new Point(425, 16);
            btnEmptyBin.Margin = new Padding(6, 6, 0, 6);
            btnEmptyBin.Name = "btnEmptyBin";
            btnEmptyBin.Padding = new Padding(10, 4, 10, 4);
            btnEmptyBin.Size = new Size(134, 33);
            btnEmptyBin.TabIndex = 2;
            btnEmptyBin.Text = "Empty Recycle Bin";
            btnEmptyBin.Click += btnEmptyBin_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.AutoSize = true;
            btnRefresh.Location = new Point(330, 16);
            btnRefresh.Margin = new Padding(6, 6, 0, 6);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Padding = new Padding(10, 4, 10, 4);
            btnRefresh.Size = new Size(89, 33);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "⟳ Refresh";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnClose
            // 
            btnClose.AutoSize = true;
            btnClose.Location = new Point(249, 16);
            btnClose.Margin = new Padding(6, 6, 0, 6);
            btnClose.Name = "btnClose";
            btnClose.Padding = new Padding(10, 4, 10, 4);
            btnClose.Size = new Size(75, 33);
            btnClose.TabIndex = 4;
            btnClose.Text = "Close";
            btnClose.Click += btnClose_Click;
            // 
            // RecycleBinForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(820, 572);
            Controls.Add(dgv);
            Controls.Add(panelBottom);
            Controls.Add(panelTop);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(640, 360);
            Name = "RecycleBinForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "HIMS : Recycle Bin";
            Load += RecycleBinForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private DataGridView dgv;
        private DataGridViewTextBoxColumn colIcon;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colType;
        private DataGridViewTextBoxColumn colSize;
        private DataGridViewTextBoxColumn colDeletedBy;
        private DataGridViewTextBoxColumn colDeletedAt;
        private Panel panelTop;
        private Label lblTitle;
        private Label lblCount;
        private FlowLayoutPanel panelBottom;
        private Button btnRestore;
        private Button btnDeletePermanently;
        private Button btnEmptyBin;
        private Button btnRefresh;
        private Button btnClose;
        private ProgressBar progressBarRB;
        private Label lblPercent;
        private Label lblStatus;
    }
}