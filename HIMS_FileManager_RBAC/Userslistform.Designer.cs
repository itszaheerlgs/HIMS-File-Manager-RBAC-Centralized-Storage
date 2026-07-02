using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace UPLOADER
{
    partial class UsersListForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UsersListForm));
            pnlTop = new Panel();
            lblTitle = new Label();
            lblSearch = new Label();
            txtSearch = new TextBox();
            btnRefresh = new Button();
            lblCount = new Label();
            dgv = new DataGridView();
            pnlEdit = new Panel();
            lblEditTitle = new Label();
            lblEditFullName = new Label();
            txtEditFullName = new TextBox();
            lblEditUsername = new Label();
            txtEditUsername = new TextBox();
            lblEditEmail = new Label();
            txtEditEmail = new TextBox();
            lblEditRole = new Label();
            cmbEditRole = new ComboBox();
            chkEditActive = new CheckBox();
            lblEditNewPass = new Label();
            txtEditNewPassword = new TextBox();
            lblEditConfirm = new Label();
            txtEditConfirmPassword = new TextBox();
            chkShowPass = new CheckBox();
            lblSelfNote = new Label();
            lblEditError = new Label();
            btnUpdate = new Button();
            btnDelete = new Button();
            btnToggleActive = new Button();
            btnClose = new Button();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            pnlEdit.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = SystemColors.ControlLight;
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblSearch);
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Controls.Add(lblCount);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1020, 46);
            pnlTop.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(10, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(74, 15);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "👥  Users List";
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(210, 15);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(19, 15);
            lblSearch.TabIndex = 1;
            lblSearch.Text = "🔍";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(230, 11);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search name, username, role…";
            txtSearch.Size = new Size(220, 23);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(460, 10);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(80, 26);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "↺ Refresh";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // lblCount
            // 
            lblCount.AutoSize = true;
            lblCount.ForeColor = SystemColors.GrayText;
            lblCount.Location = new Point(556, 15);
            lblCount.Name = "lblCount";
            lblCount.Size = new Size(0, 15);
            lblCount.TabIndex = 4;
            // 
            // dgv
            // 
            dgv.Dock = DockStyle.Fill;
            dgv.Location = new Point(0, 46);
            dgv.Name = "dgv";
            dgv.Size = new Size(720, 534);
            dgv.TabIndex = 0;
            // 
            // pnlEdit
            // 
            pnlEdit.AutoScroll = true;
            pnlEdit.BackColor = SystemColors.Control;
            pnlEdit.Controls.Add(lblEditTitle);
            pnlEdit.Controls.Add(lblEditFullName);
            pnlEdit.Controls.Add(txtEditFullName);
            pnlEdit.Controls.Add(lblEditUsername);
            pnlEdit.Controls.Add(txtEditUsername);
            pnlEdit.Controls.Add(lblEditEmail);
            pnlEdit.Controls.Add(txtEditEmail);
            pnlEdit.Controls.Add(lblEditRole);
            pnlEdit.Controls.Add(cmbEditRole);
            pnlEdit.Controls.Add(chkEditActive);
            pnlEdit.Controls.Add(lblEditNewPass);
            pnlEdit.Controls.Add(txtEditNewPassword);
            pnlEdit.Controls.Add(lblEditConfirm);
            pnlEdit.Controls.Add(txtEditConfirmPassword);
            pnlEdit.Controls.Add(chkShowPass);
            pnlEdit.Controls.Add(lblSelfNote);
            pnlEdit.Controls.Add(lblEditError);
            pnlEdit.Controls.Add(btnUpdate);
            pnlEdit.Controls.Add(btnDelete);
            pnlEdit.Controls.Add(btnToggleActive);
            pnlEdit.Controls.Add(btnClose);
            pnlEdit.Dock = DockStyle.Right;
            pnlEdit.Location = new Point(720, 46);
            pnlEdit.Name = "pnlEdit";
            pnlEdit.Padding = new Padding(10);
            pnlEdit.Size = new Size(300, 534);
            pnlEdit.TabIndex = 1;
            // 
            // lblEditTitle
            // 
            lblEditTitle.AutoSize = true;
            lblEditTitle.Location = new Point(0, 0);
            lblEditTitle.Name = "lblEditTitle";
            lblEditTitle.Size = new Size(53, 15);
            lblEditTitle.TabIndex = 0;
            lblEditTitle.Text = "Edit User";
            // 
            // lblEditFullName
            // 
            lblEditFullName.AutoSize = true;
            lblEditFullName.Location = new Point(0, 0);
            lblEditFullName.Name = "lblEditFullName";
            lblEditFullName.Size = new Size(61, 15);
            lblEditFullName.TabIndex = 1;
            lblEditFullName.Text = "Full Name";
            // 
            // txtEditFullName
            // 
            txtEditFullName.Location = new Point(0, 0);
            txtEditFullName.Name = "txtEditFullName";
            txtEditFullName.Size = new Size(100, 23);
            txtEditFullName.TabIndex = 2;
            // 
            // lblEditUsername
            // 
            lblEditUsername.AutoSize = true;
            lblEditUsername.Location = new Point(0, 0);
            lblEditUsername.Name = "lblEditUsername";
            lblEditUsername.Size = new Size(60, 15);
            lblEditUsername.TabIndex = 3;
            lblEditUsername.Text = "Username";
            // 
            // txtEditUsername
            // 
            txtEditUsername.Location = new Point(0, 0);
            txtEditUsername.Name = "txtEditUsername";
            txtEditUsername.Size = new Size(100, 23);
            txtEditUsername.TabIndex = 4;
            // 
            // lblEditEmail
            // 
            lblEditEmail.AutoSize = true;
            lblEditEmail.Location = new Point(0, 0);
            lblEditEmail.Name = "lblEditEmail";
            lblEditEmail.Size = new Size(36, 15);
            lblEditEmail.TabIndex = 5;
            lblEditEmail.Text = "Email";
            // 
            // txtEditEmail
            // 
            txtEditEmail.Location = new Point(0, 0);
            txtEditEmail.Name = "txtEditEmail";
            txtEditEmail.Size = new Size(100, 23);
            txtEditEmail.TabIndex = 6;
            // 
            // lblEditRole
            // 
            lblEditRole.AutoSize = true;
            lblEditRole.Location = new Point(0, 0);
            lblEditRole.Name = "lblEditRole";
            lblEditRole.Size = new Size(30, 15);
            lblEditRole.TabIndex = 7;
            lblEditRole.Text = "Role";
            // 
            // cmbEditRole
            // 
            cmbEditRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEditRole.Items.AddRange(new object[] { "SuperAdmin", "DataManager", "Auditor", "OPDStaff", "CertificationStaff", "RecordControllScan", "StatisticianStaff" });
            cmbEditRole.Location = new Point(0, 0);
            cmbEditRole.Name = "cmbEditRole";
            cmbEditRole.Size = new Size(121, 23);
            cmbEditRole.TabIndex = 8;
            // 
            // chkEditActive
            // 
            chkEditActive.AutoSize = true;
            chkEditActive.Location = new Point(0, 0);
            chkEditActive.Name = "chkEditActive";
            chkEditActive.Size = new Size(59, 19);
            chkEditActive.TabIndex = 9;
            chkEditActive.Text = "Active";
            // 
            // lblEditNewPass
            // 
            lblEditNewPass.AutoSize = true;
            lblEditNewPass.ForeColor = SystemColors.GrayText;
            lblEditNewPass.Location = new Point(0, 0);
            lblEditNewPass.Name = "lblEditNewPass";
            lblEditNewPass.Size = new Size(142, 15);
            lblEditNewPass.TabIndex = 10;
            lblEditNewPass.Text = "New Password  (optional)";
            // 
            // txtEditNewPassword
            // 
            txtEditNewPassword.Location = new Point(0, 0);
            txtEditNewPassword.Name = "txtEditNewPassword";
            txtEditNewPassword.Size = new Size(100, 23);
            txtEditNewPassword.TabIndex = 11;
            txtEditNewPassword.UseSystemPasswordChar = true;
            // 
            // lblEditConfirm
            // 
            lblEditConfirm.AutoSize = true;
            lblEditConfirm.Location = new Point(0, 0);
            lblEditConfirm.Name = "lblEditConfirm";
            lblEditConfirm.Size = new Size(104, 15);
            lblEditConfirm.TabIndex = 12;
            lblEditConfirm.Text = "Confirm Password";
            // 
            // txtEditConfirmPassword
            // 
            txtEditConfirmPassword.Location = new Point(0, 0);
            txtEditConfirmPassword.Name = "txtEditConfirmPassword";
            txtEditConfirmPassword.Size = new Size(100, 23);
            txtEditConfirmPassword.TabIndex = 13;
            txtEditConfirmPassword.UseSystemPasswordChar = true;
            // 
            // chkShowPass
            // 
            chkShowPass.AutoSize = true;
            chkShowPass.Location = new Point(0, 0);
            chkShowPass.Name = "chkShowPass";
            chkShowPass.Size = new Size(108, 19);
            chkShowPass.TabIndex = 14;
            chkShowPass.Text = "Show password";
            chkShowPass.CheckedChanged += chkShowPass_CheckedChanged;
            // 
            // lblSelfNote
            // 
            lblSelfNote.AutoSize = true;
            lblSelfNote.ForeColor = Color.DarkOrange;
            lblSelfNote.Location = new Point(0, 0);
            lblSelfNote.Name = "lblSelfNote";
            lblSelfNote.Size = new Size(210, 15);
            lblSelfNote.TabIndex = 15;
            lblSelfNote.Text = "ℹ️ Use 'Update Profile' to edit yourself.";
            lblSelfNote.Visible = false;
            // 
            // lblEditError
            // 
            lblEditError.AutoSize = true;
            lblEditError.ForeColor = Color.Red;
            lblEditError.Location = new Point(0, 0);
            lblEditError.Name = "lblEditError";
            lblEditError.Size = new Size(0, 15);
            lblEditError.TabIndex = 16;
            lblEditError.Visible = false;
            // 
            // btnUpdate
            // 
            btnUpdate.Enabled = false;
            btnUpdate.Location = new Point(0, 0);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(130, 30);
            btnUpdate.TabIndex = 17;
            btnUpdate.Text = "💾  Update";
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnDelete
            // 
            btnDelete.Enabled = false;
            btnDelete.ForeColor = Color.DarkRed;
            btnDelete.Location = new Point(0, 0);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(130, 30);
            btnDelete.TabIndex = 18;
            btnDelete.Text = "🗑  Delete";
            btnDelete.Click += btnDelete_Click;
            // 
            // btnToggleActive
            // 
            btnToggleActive.Location = new Point(0, 0);
            btnToggleActive.Name = "btnToggleActive";
            btnToggleActive.Size = new Size(276, 28);
            btnToggleActive.TabIndex = 19;
            btnToggleActive.Text = "⏸  Toggle Active";
            btnToggleActive.Click += btnToggleActive_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(0, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(276, 28);
            btnClose.TabIndex = 20;
            btnClose.Text = "✖  Close";
            btnClose.Click += btnClose_Click;
            // 
            // UsersListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1020, 580);
            Controls.Add(dgv);
            Controls.Add(pnlEdit);
            Controls.Add(pnlTop);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(800, 480);
            Name = "UsersListForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "HIMS : Users List";
            Load += UsersListForm_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            pnlEdit.ResumeLayout(false);
            pnlEdit.PerformLayout();
            ResumeLayout(false);
        }

        // ── Control declarations ───────────────────────────────────────────────
        private Panel pnlTop;
        private Label lblTitle;
        private TextBox txtSearch;
        private Label lblSearch;
        private Button btnRefresh;
        private Label lblCount;

        private DataGridView dgv;

        private Panel pnlEdit;
        private Label lblEditTitle;
        private Label lblEditFullName;
        private TextBox txtEditFullName;
        private Label lblEditUsername;
        private TextBox txtEditUsername;
        private Label lblEditEmail;
        private TextBox txtEditEmail;
        private Label lblEditRole;
        private ComboBox cmbEditRole;
        private CheckBox chkEditActive;
        private Label lblEditNewPass;
        private TextBox txtEditNewPassword;
        private Label lblEditConfirm;
        private TextBox txtEditConfirmPassword;
        private CheckBox chkShowPass;
        private Label lblSelfNote;
        private Label lblEditError;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnToggleActive;
        private Button btnClose;
    }
}