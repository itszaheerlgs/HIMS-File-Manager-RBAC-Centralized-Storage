namespace UPLOADER
{
    partial class UpdateProfileForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateProfileForm));
            picProfile = new PictureBox();
            lblFullName = new Label();
            txtFullName = new TextBox();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblRoleCaption = new Label();
            lblRole = new Label();
            lblPicPath = new Label();
            txtProfilePicPath = new TextBox();
            btnBrowsePic = new Button();
            lblNewPass = new Label();
            txtNewPassword = new TextBox();
            lblConfirmPass = new Label();
            txtConfirmPassword = new TextBox();
            chkShowPass = new CheckBox();
            lblError = new Label();
            btnSave = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)picProfile).BeginInit();
            SuspendLayout();
            // 
            // picProfile
            // 
            picProfile.BorderStyle = BorderStyle.FixedSingle;
            picProfile.Location = new Point(24, 20);
            picProfile.Name = "picProfile";
            picProfile.Size = new Size(90, 90);
            picProfile.SizeMode = PictureBoxSizeMode.Zoom;
            picProfile.TabIndex = 0;
            picProfile.TabStop = false;
            // 
            // lblFullName
            // 
            lblFullName.AutoSize = true;
            lblFullName.Location = new Point(130, 20);
            lblFullName.Name = "lblFullName";
            lblFullName.Size = new Size(61, 15);
            lblFullName.TabIndex = 1;
            lblFullName.Text = "Full Name";
            // 
            // txtFullName
            // 
            txtFullName.Location = new Point(130, 38);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(280, 23);
            txtFullName.TabIndex = 2;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(130, 72);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(60, 15);
            lblUsername.TabIndex = 3;
            lblUsername.Text = "Username";
            // 
            // txtUsername
            // 
            txtUsername.Location = new Point(130, 90);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(280, 23);
            txtUsername.TabIndex = 4;
            // 
            // lblEmail
            // 
            lblEmail.AutoSize = true;
            lblEmail.Location = new Point(24, 130);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(36, 15);
            lblEmail.TabIndex = 5;
            lblEmail.Text = "Email";
            // 
            // txtEmail
            // 
            txtEmail.Location = new Point(24, 148);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(386, 23);
            txtEmail.TabIndex = 6;
            // 
            // lblRoleCaption
            // 
            lblRoleCaption.AutoSize = true;
            lblRoleCaption.ForeColor = SystemColors.GrayText;
            lblRoleCaption.Location = new Point(24, 182);
            lblRoleCaption.Name = "lblRoleCaption";
            lblRoleCaption.Size = new Size(30, 15);
            lblRoleCaption.TabIndex = 7;
            lblRoleCaption.Text = "Role";
            // 
            // lblRole
            // 
            lblRole.AutoSize = true;
            lblRole.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRole.Location = new Point(70, 182);
            lblRole.Name = "lblRole";
            lblRole.Size = new Size(19, 15);
            lblRole.TabIndex = 8;
            lblRole.Text = "—";
            // 
            // lblPicPath
            // 
            lblPicPath.AutoSize = true;
            lblPicPath.Location = new Point(24, 210);
            lblPicPath.Name = "lblPicPath";
            lblPicPath.Size = new Size(81, 15);
            lblPicPath.TabIndex = 9;
            lblPicPath.Text = "Profile Picture";
            // 
            // txtProfilePicPath
            // 
            txtProfilePicPath.Location = new Point(24, 228);
            txtProfilePicPath.Name = "txtProfilePicPath";
            txtProfilePicPath.ReadOnly = true;
            txtProfilePicPath.Size = new Size(300, 23);
            txtProfilePicPath.TabIndex = 10;
            // 
            // btnBrowsePic
            // 
            btnBrowsePic.Location = new Point(332, 227);
            btnBrowsePic.Name = "btnBrowsePic";
            btnBrowsePic.Size = new Size(78, 25);
            btnBrowsePic.TabIndex = 11;
            btnBrowsePic.Text = "Browse…";
            btnBrowsePic.Click += btnBrowsePic_Click;
            // 
            // lblNewPass
            // 
            lblNewPass.AutoSize = true;
            lblNewPass.ForeColor = SystemColors.GrayText;
            lblNewPass.Location = new Point(24, 264);
            lblNewPass.Name = "lblNewPass";
            lblNewPass.Size = new Size(240, 15);
            lblNewPass.TabIndex = 12;
            lblNewPass.Text = "New Password  (leave blank to keep current)";
            // 
            // txtNewPassword
            // 
            txtNewPassword.Location = new Point(24, 282);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.Size = new Size(386, 23);
            txtNewPassword.TabIndex = 13;
            txtNewPassword.UseSystemPasswordChar = true;
            // 
            // lblConfirmPass
            // 
            lblConfirmPass.AutoSize = true;
            lblConfirmPass.Location = new Point(24, 316);
            lblConfirmPass.Name = "lblConfirmPass";
            lblConfirmPass.Size = new Size(131, 15);
            lblConfirmPass.TabIndex = 14;
            lblConfirmPass.Text = "Confirm New Password";
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.Location = new Point(24, 334);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.Size = new Size(386, 23);
            txtConfirmPassword.TabIndex = 15;
            txtConfirmPassword.UseSystemPasswordChar = true;
            // 
            // chkShowPass
            // 
            chkShowPass.AutoSize = true;
            chkShowPass.Location = new Point(24, 364);
            chkShowPass.Name = "chkShowPass";
            chkShowPass.Size = new Size(108, 19);
            chkShowPass.TabIndex = 16;
            chkShowPass.Text = "Show password";
            chkShowPass.CheckedChanged += chkShowPass_CheckedChanged;
            // 
            // lblError
            // 
            lblError.AutoSize = true;
            lblError.ForeColor = Color.Red;
            lblError.Location = new Point(24, 394);
            lblError.Name = "lblError";
            lblError.Size = new Size(0, 15);
            lblError.TabIndex = 17;
            lblError.Visible = false;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(254, 420);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(90, 30);
            btnSave.TabIndex = 18;
            btnSave.Text = "💾  Save";
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(352, 420);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(58, 30);
            btnCancel.TabIndex = 19;
            btnCancel.Text = "Cancel";
            btnCancel.Click += btnCancel_Click;
            // 
            // UpdateProfileForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(434, 468);
            Controls.Add(picProfile);
            Controls.Add(lblFullName);
            Controls.Add(txtFullName);
            Controls.Add(lblUsername);
            Controls.Add(txtUsername);
            Controls.Add(lblEmail);
            Controls.Add(txtEmail);
            Controls.Add(lblRoleCaption);
            Controls.Add(lblRole);
            Controls.Add(lblPicPath);
            Controls.Add(txtProfilePicPath);
            Controls.Add(btnBrowsePic);
            Controls.Add(lblNewPass);
            Controls.Add(txtNewPassword);
            Controls.Add(lblConfirmPass);
            Controls.Add(txtConfirmPassword);
            Controls.Add(chkShowPass);
            Controls.Add(lblError);
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UpdateProfileForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "HIMS : Update Profile";
            Load += UpdateProfileForm_Load;
            ((System.ComponentModel.ISupportInitialize)picProfile).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        // ── Controls ──────────────────────────────────────────────────────────
        private PictureBox picProfile;
        private Label lblFullName;
        private TextBox txtFullName;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblEmail;
        private TextBox txtEmail;
        private Label lblRoleCaption;
        private Label lblRole;
        private Label lblPicPath;
        private TextBox txtProfilePicPath;
        private Button btnBrowsePic;
        private Label lblNewPass;
        private TextBox txtNewPassword;
        private Label lblConfirmPass;
        private TextBox txtConfirmPassword;
        private CheckBox chkShowPass;
        private Label lblError;
        private Button btnSave;
        private Button btnCancel;
    }
}