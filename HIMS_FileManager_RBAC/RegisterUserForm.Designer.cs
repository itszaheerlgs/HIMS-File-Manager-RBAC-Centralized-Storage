namespace UPLOADER
{
    partial class RegisterUserForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegisterUserForm));
            pnlHeader = new Panel();
            lblTitle = new Label();
            lblSub = new Label();
            pnlBody = new Panel();
            lblFullName = new Label();
            txtFullName = new TextBox();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblConfirmPassword = new Label();
            txtConfirmPassword = new TextBox();
            chkShowPass = new CheckBox();
            lblRole = new Label();
            cmbRole = new ComboBox();
            chkIsActive = new CheckBox();
            lblProfilePic = new Label();
            txtProfilePicPath = new TextBox();
            btnBrowsePic = new Button();
            lblError = new Label();
            btnRegister = new Button();
            btnCancel = new Button();
            pnlHeader.SuspendLayout();
            pnlBody.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(28, 50, 90);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(420, 80);
            pnlHeader.TabIndex = 1;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(206, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "👤  Register New User";
            // 
            // lblSub
            // 
            lblSub.AutoSize = true;
            lblSub.Font = new Font("Segoe UI", 8.5F);
            lblSub.ForeColor = Color.FromArgb(180, 200, 230);
            lblSub.Location = new Point(22, 46);
            lblSub.Name = "lblSub";
            lblSub.Size = new Size(279, 15);
            lblSub.TabIndex = 1;
            lblSub.Text = "Create a new admin account for HIMS File Manager";
            // 
            // pnlBody
            // 
            pnlBody.AutoScroll = true;
            pnlBody.BackColor = Color.FromArgb(245, 247, 252);
            pnlBody.Controls.Add(lblFullName);
            pnlBody.Controls.Add(txtFullName);
            pnlBody.Controls.Add(lblUsername);
            pnlBody.Controls.Add(txtUsername);
            pnlBody.Controls.Add(lblEmail);
            pnlBody.Controls.Add(txtEmail);
            pnlBody.Controls.Add(lblPassword);
            pnlBody.Controls.Add(txtPassword);
            pnlBody.Controls.Add(lblConfirmPassword);
            pnlBody.Controls.Add(txtConfirmPassword);
            pnlBody.Controls.Add(chkShowPass);
            pnlBody.Controls.Add(lblRole);
            pnlBody.Controls.Add(cmbRole);
            pnlBody.Controls.Add(chkIsActive);
            pnlBody.Controls.Add(lblProfilePic);
            pnlBody.Controls.Add(txtProfilePicPath);
            pnlBody.Controls.Add(btnBrowsePic);
            pnlBody.Controls.Add(lblError);
            pnlBody.Controls.Add(btnRegister);
            pnlBody.Controls.Add(btnCancel);
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.Location = new Point(0, 80);
            pnlBody.Name = "pnlBody";
            pnlBody.Padding = new Padding(30, 18, 30, 14);
            pnlBody.Size = new Size(420, 500);
            pnlBody.TabIndex = 0;
            // 
            // lblFullName
            // 
            lblFullName.AutoSize = true;
            lblFullName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFullName.ForeColor = Color.FromArgb(28, 50, 90);
            lblFullName.Location = new Point(30, 18);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new Size(70, 15);
            lblFullName.TabIndex = 0;
            lblFullName.Text = "Full Name *";
            // 
            // txtFullName
            // 
            txtFullName.BorderStyle = BorderStyle.FixedSingle;
            txtFullName.Font = new Font("Segoe UI", 10F);
            txtFullName.Location = new Point(30, 40);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(360, 25);
            txtFullName.TabIndex = 1;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(28, 50, 90);
            lblUsername.Location = new Point(30, 78);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(72, 15);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Username *";
            // 
            // txtUsername
            // 
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 10F);
            txtUsername.Location = new Point(30, 100);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(360, 25);
            txtUsername.TabIndex = 3;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblEmail.ForeColor = Color.FromArgb(28, 50, 90);
            lblEmail.Location = new Point(30, 138);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(36, 15);
            lblEmail.TabIndex = 4;
            lblEmail.Text = "Email";
            // 
            // txtEmail
            // 
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.Font = new Font("Segoe UI", 10F);
            txtEmail.Location = new Point(30, 160);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(360, 25);
            txtEmail.TabIndex = 5;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(28, 50, 90);
            lblPassword.Location = new Point(30, 198);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(67, 15);
            lblPassword.TabIndex = 6;
            lblPassword.Text = "Password *";
            // 
            // txtPassword
            // 
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.Location = new Point(30, 220);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(360, 25);
            txtPassword.TabIndex = 7;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // lblConfirmPassword
            // 
            lblConfirmPassword.AutoSize = true;
            lblConfirmPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblConfirmPassword.ForeColor = Color.FromArgb(28, 50, 90);
            lblConfirmPassword.Location = new Point(30, 258);
            lblConfirmPassword.Name = "lblConfirmPassword";
            lblConfirmPassword.Size = new Size(115, 15);
            lblConfirmPassword.TabIndex = 8;
            lblConfirmPassword.Text = "Confirm Password *";
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.BorderStyle = BorderStyle.FixedSingle;
            txtConfirmPassword.Font = new Font("Segoe UI", 10F);
            txtConfirmPassword.Location = new Point(30, 280);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(360, 25);
            txtConfirmPassword.TabIndex = 9;
            txtConfirmPassword.UseSystemPasswordChar = true;
            // 
            // chkShowPass
            // 
            chkShowPass.AutoSize = true;
            chkShowPass.Font = new Font("Segoe UI", 8.5F);
            chkShowPass.ForeColor = Color.Gray;
            chkShowPass.Location = new Point(30, 314);
            chkShowPass.Name = "chkShowPass";
            chkShowPass.Size = new Size(108, 19);
            chkShowPass.TabIndex = 10;
            chkShowPass.Text = "Show password";
            chkShowPass.CheckedChanged += chkShowPass_CheckedChanged;
            // 
            // lblRole
            // 
            lblRole.AutoSize = true;
            lblRole.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRole.ForeColor = Color.FromArgb(28, 50, 90);
            lblRole.Location = new Point(30, 350);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(40, 15);
            lblRole.TabIndex = 11;
            lblRole.Text = "Role *";
            // 
            // cmbRole
            // 
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.Font = new Font("Segoe UI", 10F);
            cmbRole.Location = new Point(30, 372);
            cmbRole.Name = "cmbRole";
            cmbRole.Size = new Size(220, 25);
            cmbRole.TabIndex = 12;
            // 
            // chkIsActive
            // 
            chkIsActive.AutoSize = true;
            chkIsActive.Checked = true;
            chkIsActive.CheckState = CheckState.Checked;
            chkIsActive.Font = new Font("Segoe UI", 9F);
            chkIsActive.ForeColor = Color.FromArgb(50, 60, 80);
            chkIsActive.Location = new Point(264, 376);
            chkIsActive.Name = "chkIsActive";
            chkIsActive.Size = new Size(59, 19);
            chkIsActive.TabIndex = 13;
            chkIsActive.Text = "Active";
            // 
            // lblProfilePic
            // 
            lblProfilePic.AutoSize = true;
            lblProfilePic.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblProfilePic.ForeColor = Color.FromArgb(28, 50, 90);
            lblProfilePic.Location = new Point(30, 410);
            lblProfilePic.Name = "lblProfilePic";
            lblProfilePic.Size = new Size(87, 15);
            lblProfilePic.TabIndex = 14;
            lblProfilePic.Text = "Profile Picture";
            // 
            // txtProfilePicPath
            // 
            txtProfilePicPath.BorderStyle = BorderStyle.FixedSingle;
            txtProfilePicPath.Font = new Font("Segoe UI", 9.5F);
            txtProfilePicPath.Location = new Point(30, 432);
            txtProfilePicPath.Name = "txtProfilePicPath";
            txtProfilePicPath.ReadOnly = true;
            txtProfilePicPath.Size = new Size(270, 24);
            txtProfilePicPath.TabIndex = 15;
            // 
            // btnBrowsePic
            // 
            btnBrowsePic.BackColor = Color.FromArgb(50, 70, 110);
            btnBrowsePic.Cursor = Cursors.Hand;
            btnBrowsePic.FlatStyle = FlatStyle.Flat;
            btnBrowsePic.Font = new Font("Segoe UI", 8.5F);
            btnBrowsePic.ForeColor = Color.White;
            btnBrowsePic.Location = new Point(306, 431);
            btnBrowsePic.Name = "btnBrowsePic";
            btnBrowsePic.Size = new Size(84, 26);
            btnBrowsePic.TabIndex = 16;
            btnBrowsePic.Text = "📁 Browse";
            btnBrowsePic.UseVisualStyleBackColor = false;
            btnBrowsePic.Click += btnBrowsePic_Click;
            // 
            // lblError
            // 
            lblError.Font = new Font("Segoe UI", 8.5F);
            lblError.ForeColor = Color.Firebrick;
            lblError.Location = new Point(30, 468);
            lblError.Name = "lblError";
            lblError.Size = new Size(360, 36);
            lblError.TabIndex = 17;
            lblError.Visible = false;
            // 
            // btnRegister
            // 
            btnRegister.BackColor = Color.FromArgb(28, 50, 90);
            btnRegister.Cursor = Cursors.Hand;
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnRegister.ForeColor = Color.White;
            btnRegister.Location = new Point(30, 512);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(180, 38);
            btnRegister.TabIndex = 18;
            btnRegister.Text = "✅  Register User";
            btnRegister.UseVisualStyleBackColor = false;
            btnRegister.Click += btnRegister_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(130, 50, 50);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(218, 512);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(172, 38);
            btnCancel.TabIndex = 19;
            btnCancel.Text = "✕  Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // RegisterUserForm
            // 
            AcceptButton = btnRegister;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btnCancel;
            ClientSize = new Size(420, 580);
            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RegisterUserForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "HIMS : Register New User";
            Load += RegisterUserForm_Load;
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlBody.ResumeLayout(false);
            pnlBody.PerformLayout();
            ResumeLayout(false);
        }

        private Panel pnlHeader;
        private Label lblTitle, lblSub;
        private Panel pnlBody;

        private Label lblFullName, lblUsername, lblEmail, lblPassword, lblConfirmPassword, lblRole, lblProfilePic, lblError;
        private TextBox txtFullName, txtUsername, txtEmail, txtPassword, txtConfirmPassword, txtProfilePicPath;
        private CheckBox chkShowPass, chkIsActive;
        private ComboBox cmbRole;
        private Button btnBrowsePic, btnRegister, btnCancel;
    }
}