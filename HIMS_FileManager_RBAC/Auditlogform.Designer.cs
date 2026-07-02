using System.Drawing;
using System.Windows.Forms;

namespace UPLOADER
{
    partial class AuditLogForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AuditLogForm));
            pnlTop = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            pnlTopAccent = new Panel();
            pnlFilters = new Panel();
            lblFromDate = new Label();
            dtpFrom = new DateTimePicker();
            lblToDate = new Label();
            dtpTo = new DateTimePicker();
            lblModule = new Label();
            cmbModule = new ComboBox();
            lblAction = new Label();
            cmbAction = new ComboBox();
            lblActor = new Label();
            cmbActor = new ComboBox();
            txtSearch = new TextBox();
            btnSearch = new Button();
            btnRefresh = new Button();
            pnlBottom = new Panel();
            lblCount = new Label();
            btnExport = new Button();
            btnClearAll = new Button();
            btnClose = new Button();
            dgv = new DataGridView();
            pnlTop.SuspendLayout();
            pnlFilters.SuspendLayout();
            pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = Color.FromArgb(13, 34, 71);
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblSubtitle);
            pnlTop.Controls.Add(pnlTopAccent);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1100, 60);
            pnlTop.TabIndex = 3;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(201, 168, 76);
            lblTitle.Location = new Point(14, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(120, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "📋  AUDIT LOG";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 8.5F);
            lblSubtitle.ForeColor = Color.FromArgb(122, 139, 170);
            lblSubtitle.Location = new Point(16, 34);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(211, 15);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "HIMS File Manager v3  —  Activity Trail";
            // 
            // pnlTopAccent
            // 
            pnlTopAccent.BackColor = Color.FromArgb(201, 168, 76);
            pnlTopAccent.Dock = DockStyle.Bottom;
            pnlTopAccent.Location = new Point(0, 58);
            pnlTopAccent.Name = "pnlTopAccent";
            pnlTopAccent.Size = new Size(1100, 2);
            pnlTopAccent.TabIndex = 2;
            // 
            // pnlFilters
            // 
            pnlFilters.BackColor = Color.FromArgb(22, 43, 82);
            pnlFilters.Controls.Add(lblFromDate);
            pnlFilters.Controls.Add(dtpFrom);
            pnlFilters.Controls.Add(lblToDate);
            pnlFilters.Controls.Add(dtpTo);
            pnlFilters.Controls.Add(lblModule);
            pnlFilters.Controls.Add(cmbModule);
            pnlFilters.Controls.Add(lblAction);
            pnlFilters.Controls.Add(cmbAction);
            pnlFilters.Controls.Add(lblActor);
            pnlFilters.Controls.Add(cmbActor);
            pnlFilters.Controls.Add(txtSearch);
            pnlFilters.Controls.Add(btnSearch);
            pnlFilters.Controls.Add(btnRefresh);
            pnlFilters.Dock = DockStyle.Top;
            pnlFilters.Location = new Point(0, 60);
            pnlFilters.Name = "pnlFilters";
            pnlFilters.Size = new Size(1100, 56);
            pnlFilters.TabIndex = 2;
            // 
            // lblFromDate
            // 
            lblFromDate.AutoSize = true;
            lblFromDate.Font = new Font("Segoe UI", 7.5F);
            lblFromDate.ForeColor = Color.FromArgb(122, 139, 170);
            lblFromDate.Location = new Point(10, 8);
            lblFromDate.Name = "lblFromDate";
            lblFromDate.Size = new Size(33, 12);
            lblFromDate.TabIndex = 0;
            lblFromDate.Text = "FROM";
            // 
            // dtpFrom
            // 
            dtpFrom.BackColor = Color.FromArgb(11, 31, 58);
            dtpFrom.CalendarMonthBackground = Color.FromArgb(11, 31, 58);
            dtpFrom.ForeColor = Color.FromArgb(232, 234, 240);
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.Location = new Point(10, 24);
            dtpFrom.Name = "dtpFrom";
            dtpFrom.Size = new Size(115, 23);
            dtpFrom.TabIndex = 1;
            // 
            // lblToDate
            // 
            lblToDate.AutoSize = true;
            lblToDate.Font = new Font("Segoe UI", 7.5F);
            lblToDate.ForeColor = Color.FromArgb(122, 139, 170);
            lblToDate.Location = new Point(134, 8);
            lblToDate.Name = "lblToDate";
            lblToDate.Size = new Size(18, 12);
            lblToDate.TabIndex = 2;
            lblToDate.Text = "TO";
            // 
            // dtpTo
            // 
            dtpTo.BackColor = Color.FromArgb(11, 31, 58);
            dtpTo.CalendarMonthBackground = Color.FromArgb(11, 31, 58);
            dtpTo.ForeColor = Color.FromArgb(232, 234, 240);
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.Location = new Point(134, 24);
            dtpTo.Name = "dtpTo";
            dtpTo.Size = new Size(115, 23);
            dtpTo.TabIndex = 3;
            // 
            // lblModule
            // 
            lblModule.AutoSize = true;
            lblModule.Font = new Font("Segoe UI", 7.5F);
            lblModule.ForeColor = Color.FromArgb(122, 139, 170);
            lblModule.Location = new Point(260, 8);
            lblModule.Name = "lblModule";
            lblModule.Size = new Size(46, 12);
            lblModule.TabIndex = 4;
            lblModule.Text = "MODULE";
            // 
            // cmbModule
            // 
            cmbModule.BackColor = Color.FromArgb(11, 31, 58);
            cmbModule.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbModule.FlatStyle = FlatStyle.Flat;
            cmbModule.ForeColor = Color.FromArgb(232, 234, 240);
            cmbModule.Location = new Point(260, 24);
            cmbModule.Name = "cmbModule";
            cmbModule.Size = new Size(120, 23);
            cmbModule.TabIndex = 5;
            // 
            // lblAction
            // 
            lblAction.AutoSize = true;
            lblAction.Font = new Font("Segoe UI", 7.5F);
            lblAction.ForeColor = Color.FromArgb(122, 139, 170);
            lblAction.Location = new Point(390, 8);
            lblAction.Name = "lblAction";
            lblAction.Size = new Size(40, 12);
            lblAction.TabIndex = 6;
            lblAction.Text = "ACTION";
            // 
            // cmbAction
            // 
            cmbAction.BackColor = Color.FromArgb(11, 31, 58);
            cmbAction.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAction.FlatStyle = FlatStyle.Flat;
            cmbAction.ForeColor = Color.FromArgb(232, 234, 240);
            cmbAction.Location = new Point(390, 24);
            cmbAction.Name = "cmbAction";
            cmbAction.Size = new Size(140, 23);
            cmbAction.TabIndex = 7;
            // 
            // lblActor
            // 
            lblActor.AutoSize = true;
            lblActor.Font = new Font("Segoe UI", 7.5F);
            lblActor.ForeColor = Color.FromArgb(122, 139, 170);
            lblActor.Location = new Point(540, 8);
            lblActor.Name = "lblActor";
            lblActor.Size = new Size(28, 12);
            lblActor.TabIndex = 8;
            lblActor.Text = "USER";
            // 
            // cmbActor
            // 
            cmbActor.BackColor = Color.FromArgb(11, 31, 58);
            cmbActor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbActor.FlatStyle = FlatStyle.Flat;
            cmbActor.ForeColor = Color.FromArgb(232, 234, 240);
            cmbActor.Location = new Point(540, 24);
            cmbActor.Name = "cmbActor";
            cmbActor.Size = new Size(150, 23);
            cmbActor.TabIndex = 9;
            // 
            // txtSearch
            // 
            txtSearch.BackColor = Color.FromArgb(11, 31, 58);
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 9F);
            txtSearch.ForeColor = Color.FromArgb(232, 234, 240);
            txtSearch.Location = new Point(702, 24);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "🔍  keyword…";
            txtSearch.Size = new Size(160, 23);
            txtSearch.TabIndex = 10;
            txtSearch.KeyDown += txtSearch_KeyDown;
            // 
            // btnSearch
            // 
            btnSearch.BackColor = Color.FromArgb(201, 168, 76);
            btnSearch.Cursor = Cursors.Hand;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnSearch.ForeColor = Color.FromArgb(11, 31, 58);
            btnSearch.Location = new Point(870, 22);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(76, 28);
            btnSearch.TabIndex = 11;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnRefresh
            // 
            btnRefresh.BackColor = Color.FromArgb(30, 56, 102);
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 11F);
            btnRefresh.ForeColor = Color.FromArgb(201, 168, 76);
            btnRefresh.Location = new Point(952, 22);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(34, 28);
            btnRefresh.TabIndex = 12;
            btnRefresh.Text = "↺";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = Color.FromArgb(13, 34, 71);
            pnlBottom.Controls.Add(lblCount);
            pnlBottom.Controls.Add(btnExport);
            pnlBottom.Controls.Add(btnClearAll);
            pnlBottom.Controls.Add(btnClose);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 592);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1100, 48);
            pnlBottom.TabIndex = 1;
            // 
            // lblCount
            // 
            lblCount.AutoSize = true;
            lblCount.Font = new Font("Segoe UI", 8.5F);
            lblCount.ForeColor = Color.FromArgb(122, 139, 170);
            lblCount.Location = new Point(12, 16);
            lblCount.Name = "lblCount";
            lblCount.Size = new Size(0, 15);
            lblCount.TabIndex = 0;
            // 
            // btnExport
            // 
            btnExport.BackColor = Color.FromArgb(22, 43, 82);
            btnExport.Cursor = Cursors.Hand;
            btnExport.FlatAppearance.BorderColor = Color.FromArgb(138, 113, 48);
            btnExport.FlatStyle = FlatStyle.Flat;
            btnExport.Font = new Font("Segoe UI", 8.5F);
            btnExport.ForeColor = Color.FromArgb(201, 168, 76);
            btnExport.Location = new Point(170, 9);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(124, 30);
            btnExport.TabIndex = 1;
            btnExport.Text = "📥  Export CSV";
            btnExport.UseVisualStyleBackColor = false;
            btnExport.Click += btnExport_Click;
            // 
            // btnClearAll
            // 
            btnClearAll.BackColor = Color.FromArgb(22, 43, 82);
            btnClearAll.Cursor = Cursors.Hand;
            btnClearAll.FlatAppearance.BorderColor = Color.FromArgb(229, 81, 81);
            btnClearAll.FlatStyle = FlatStyle.Flat;
            btnClearAll.Font = new Font("Segoe UI", 8.5F);
            btnClearAll.ForeColor = Color.FromArgb(229, 81, 81);
            btnClearAll.Location = new Point(302, 9);
            btnClearAll.Name = "btnClearAll";
            btnClearAll.Size = new Size(134, 30);
            btnClearAll.TabIndex = 2;
            btnClearAll.Text = "🗑  Clear All Logs";
            btnClearAll.UseVisualStyleBackColor = false;
            btnClearAll.Click += btnClearAll_Click;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(30, 56, 102);
            btnClose.Cursor = Cursors.Hand;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 8.5F);
            btnClose.ForeColor = Color.FromArgb(122, 139, 170);
            btnClose.Location = new Point(444, 9);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(90, 30);
            btnClose.TabIndex = 3;
            btnClose.Text = "✖  Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // dgv
            // 
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(18, 40, 76);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(232, 234, 240);
            dgv.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.FromArgb(11, 31, 58);
            dgv.BorderStyle = BorderStyle.None;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(13, 34, 71);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(201, 168, 76);
            dataGridViewCellStyle2.Padding = new Padding(6, 0, 0, 0);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(13, 34, 71);
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgv.ColumnHeadersHeight = 34;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(14, 33, 64);
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(232, 234, 240);
            dataGridViewCellStyle3.Padding = new Padding(4, 2, 0, 2);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(22, 43, 82);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(201, 168, 76);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgv.DefaultCellStyle = dataGridViewCellStyle3;
            dgv.Dock = DockStyle.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.GridColor = Color.FromArgb(22, 43, 82);
            dgv.Location = new Point(0, 116);
            dgv.MultiSelect = false;
            dgv.Name = "dgv";
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.RowTemplate.Height = 28;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Size = new Size(1100, 476);
            dgv.TabIndex = 0;
            // 
            // AuditLogForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(11, 31, 58);
            ClientSize = new Size(1100, 640);
            Controls.Add(dgv);
            Controls.Add(pnlBottom);
            Controls.Add(pnlFilters);
            Controls.Add(pnlTop);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(900, 500);
            Name = "AuditLogForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "HIMS : Audit Log";
            Load += AuditLogForm_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlFilters.ResumeLayout(false);
            pnlFilters.PerformLayout();
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            ResumeLayout(false);
        }

        // ── Column factory — static method is fine outside InitializeComponent ──
        private static DataGridViewTextBoxColumn MakeCol(string name, string header, int w)
            => new DataGridViewTextBoxColumn
            {
                DataPropertyName = name,
                HeaderText = header,
                Name = name,
                MinimumWidth = w,
                FillWeight = w
            };

        // ── Control declarations ──────────────────────────────────────────────
        private Panel pnlTop, pnlTopAccent, pnlFilters, pnlBottom;
        private Label lblTitle, lblSubtitle;
        private Label lblFromDate, lblToDate, lblModule, lblAction, lblActor;
        private DateTimePicker dtpFrom, dtpTo;
        private ComboBox cmbModule, cmbAction, cmbActor;
        private TextBox txtSearch;
        private Button btnSearch, btnRefresh;
        private DataGridView dgv;
        private Label lblCount;
        private Button btnExport, btnClearAll, btnClose;
    }
}