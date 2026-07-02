namespace UPLOADER
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            pnlLeft = new Panel();
            pnlLogo = new Panel();
            pictureBox1 = new PictureBox();
            lblTitle = new Label();
            lblSub = new Label();
            lblTagline = new Label();
            pnlRight = new Panel();
            pnlCard = new Panel();
            lblWelcome = new Label();
            lblSignIn = new Label();
            pnlDivider = new Panel();
            lblUser = new Label();
            pnlUserBox = new Panel();
            picUserIcon = new PictureBox();
            txtUsername = new TextBox();
            lblPass = new Label();
            pnlPassBox = new Panel();
            picPassIcon = new PictureBox();
            txtPassword = new TextBox();
            chkShow = new CheckBox();
            btnLogin = new Button();
            lblError = new Label();
            pnlFooter = new Panel();
            btnSettings = new Button();
            lblFooter = new Label();
            lblConnInfo = new Label();
            cmbIpSuggest = new ComboBox();
            lblIpSuggest = new Label();
            pnlLeft.SuspendLayout();
            pnlLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            pnlRight.SuspendLayout();
            pnlCard.SuspendLayout();
            pnlUserBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picUserIcon).BeginInit();
            pnlPassBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPassIcon).BeginInit();
            pnlFooter.SuspendLayout();
            SuspendLayout();
            // 
            // pnlLeft
            // 
            pnlLeft.BackColor = Color.FromArgb(15, 37, 75);
            pnlLeft.Controls.Add(pnlLogo);
            pnlLeft.Controls.Add(lblTagline);
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(220, 441);
            pnlLeft.TabIndex = 0;
            // 
            // pnlLogo
            // 
            pnlLogo.BackColor = Color.Transparent;
            pnlLogo.Controls.Add(pictureBox1);
            pnlLogo.Controls.Add(lblTitle);
            pnlLogo.Controls.Add(lblSub);
            pnlLogo.Location = new Point(0, 120);
            pnlLogo.Name = "pnlLogo";
            pnlLogo.Size = new Size(220, 200);
            pnlLogo.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.BackgroundImage = (Image)resources.GetObject("pictureBox1.BackgroundImage");
            pictureBox1.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBox1.Location = new Point(63, 17);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(93, 85);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(0, 101);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(220, 34);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "HIMS File Manager";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSub
            // 
            lblSub.Font = new Font("Segoe UI", 8F);
            lblSub.ForeColor = Color.FromArgb(160, 185, 220);
            lblSub.Location = new Point(0, 137);
            lblSub.Name = "lblSub";
            lblSub.Size = new Size(220, 24);
            lblSub.TabIndex = 0;
            lblSub.Text = "OPD Document Management ";
            lblSub.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTagline
            // 
            lblTagline.Font = new Font("Segoe UI", 7.5F, FontStyle.Italic);
            lblTagline.ForeColor = Color.FromArgb(100, 130, 175);
            lblTagline.Location = new Point(20, 370);
            lblTagline.Name = "lblTagline";
            lblTagline.Size = new Size(180, 60);
            lblTagline.TabIndex = 0;
            lblTagline.Text = "Dr. George Tocao Hofer Medical Center\nHealth Information Management Section";
            lblTagline.TextAlign = ContentAlignment.BottomLeft;
            // 
            // pnlRight
            // 
            pnlRight.BackColor = Color.FromArgb(245, 247, 252);
            pnlRight.Controls.Add(pnlCard);
            pnlRight.Controls.Add(pnlFooter);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Location = new Point(220, 0);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(344, 441);
            pnlRight.TabIndex = 0;
            // 
            // pnlCard
            // 
            pnlCard.BackColor = Color.White;
            pnlCard.Controls.Add(lblWelcome);
            pnlCard.Controls.Add(lblSignIn);
            pnlCard.Controls.Add(pnlDivider);
            pnlCard.Controls.Add(lblUser);
            pnlCard.Controls.Add(pnlUserBox);
            pnlCard.Controls.Add(lblPass);
            pnlCard.Controls.Add(pnlPassBox);
            pnlCard.Controls.Add(chkShow);
            pnlCard.Controls.Add(btnLogin);
            pnlCard.Controls.Add(lblError);
            pnlCard.Location = new Point(23, 12);
            pnlCard.Name = "pnlCard";
            pnlCard.Padding = new Padding(28, 24, 28, 24);
            pnlCard.Size = new Size(300, 370);
            pnlCard.TabIndex = 0;
            pnlCard.Paint += PnlCard_Paint;
            // 
            // lblWelcome
            // 
            lblWelcome.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblWelcome.ForeColor = Color.FromArgb(15, 37, 75);
            lblWelcome.Location = new Point(28, 24);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(244, 28);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "Welcome back";
            // 
            // lblSignIn
            // 
            lblSignIn.Font = new Font("Segoe UI", 9F);
            lblSignIn.ForeColor = Color.FromArgb(130, 145, 170);
            lblSignIn.Location = new Point(28, 54);
            lblSignIn.Name = "lblSignIn";
            lblSignIn.Size = new Size(244, 18);
            lblSignIn.TabIndex = 0;
            lblSignIn.Text = "Sign in to your account";
            // 
            // pnlDivider
            // 
            pnlDivider.BackColor = Color.FromArgb(201, 168, 76);
            pnlDivider.Location = new Point(28, 78);
            pnlDivider.Name = "pnlDivider";
            pnlDivider.Size = new Size(36, 3);
            pnlDivider.TabIndex = 0;
            // 
            // lblUser
            // 
            lblUser.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblUser.ForeColor = Color.FromArgb(60, 80, 120);
            lblUser.Location = new Point(28, 100);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(244, 16);
            lblUser.TabIndex = 0;
            lblUser.Text = "USERNAME";
            // 
            // pnlUserBox
            // 
            pnlUserBox.BackColor = Color.FromArgb(248, 250, 255);
            pnlUserBox.Controls.Add(picUserIcon);
            pnlUserBox.Controls.Add(txtUsername);
            pnlUserBox.Location = new Point(28, 118);
            pnlUserBox.Name = "pnlUserBox";
            pnlUserBox.Padding = new Padding(8, 0, 0, 0);
            pnlUserBox.Size = new Size(244, 36);
            pnlUserBox.TabIndex = 0;
            pnlUserBox.Paint += PnlInput_Paint;
            // 
            // picUserIcon
            // 
            picUserIcon.BackColor = Color.Transparent;
            picUserIcon.BackgroundImage = (Image)resources.GetObject("picUserIcon.BackgroundImage");
            picUserIcon.BackgroundImageLayout = ImageLayout.Zoom;
            picUserIcon.Location = new Point(8, 10);
            picUserIcon.Name = "picUserIcon";
            picUserIcon.Size = new Size(16, 16);
            picUserIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            picUserIcon.TabIndex = 0;
            picUserIcon.TabStop = false;
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.FromArgb(248, 250, 255);
            txtUsername.BorderStyle = BorderStyle.None;
            txtUsername.Font = new Font("Segoe UI", 9.5F);
            txtUsername.ForeColor = Color.FromArgb(20, 40, 80);
            txtUsername.Location = new Point(32, 8);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Enter your username";
            txtUsername.Size = new Size(204, 17);
            txtUsername.TabIndex = 0;
            // 
            // lblPass
            // 
            lblPass.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblPass.ForeColor = Color.FromArgb(60, 80, 120);
            lblPass.Location = new Point(28, 170);
            lblPass.Name = "lblPass";
            lblPass.Size = new Size(244, 16);
            lblPass.TabIndex = 0;
            lblPass.Text = "PASSWORD";
            // 
            // pnlPassBox
            // 
            pnlPassBox.BackColor = Color.FromArgb(248, 250, 255);
            pnlPassBox.Controls.Add(picPassIcon);
            pnlPassBox.Controls.Add(txtPassword);
            pnlPassBox.Location = new Point(28, 188);
            pnlPassBox.Name = "pnlPassBox";
            pnlPassBox.Padding = new Padding(8, 0, 0, 0);
            pnlPassBox.Size = new Size(244, 36);
            pnlPassBox.TabIndex = 0;
            pnlPassBox.Paint += PnlInput_Paint;
            // 
            // picPassIcon
            // 
            picPassIcon.BackColor = Color.Transparent;
            picPassIcon.BackgroundImage = (Image)resources.GetObject("picPassIcon.BackgroundImage");
            picPassIcon.BackgroundImageLayout = ImageLayout.Zoom;
            picPassIcon.Location = new Point(8, 10);
            picPassIcon.Name = "picPassIcon";
            picPassIcon.Size = new Size(16, 16);
            picPassIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            picPassIcon.TabIndex = 0;
            picPassIcon.TabStop = false;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(248, 250, 255);
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Segoe UI", 9.5F);
            txtPassword.ForeColor = Color.FromArgb(20, 40, 80);
            txtPassword.Location = new Point(32, 8);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Enter your password";
            txtPassword.Size = new Size(204, 17);
            txtPassword.TabIndex = 0;
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.KeyDown += txtPassword_KeyDown;
            // 
            // chkShow
            // 
            chkShow.AutoSize = true;
            chkShow.Font = new Font("Segoe UI", 8.5F);
            chkShow.ForeColor = Color.FromArgb(130, 145, 170);
            chkShow.Location = new Point(28, 234);
            chkShow.Name = "chkShow";
            chkShow.Size = new Size(108, 19);
            chkShow.TabIndex = 0;
            chkShow.Text = "Show password";
            chkShow.CheckedChanged += chkShow_CheckedChanged;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(15, 37, 75);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(28, 264);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(244, 40);
            btnLogin.TabIndex = 0;
            btnLogin.Text = "Sign In";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            btnLogin.MouseEnter += BtnLogin_MouseEnter;
            btnLogin.MouseLeave += BtnLogin_MouseLeave;
            // 
            // lblError
            // 
            lblError.BackColor = Color.FromArgb(255, 235, 235);
            lblError.Font = new Font("Segoe UI", 8.5F);
            lblError.ForeColor = Color.FromArgb(192, 40, 40);
            lblError.Location = new Point(28, 310);
            lblError.Name = "lblError";
            lblError.Padding = new Padding(6, 0, 0, 0);
            lblError.Size = new Size(244, 30);
            lblError.TabIndex = 0;
            lblError.TextAlign = ContentAlignment.MiddleLeft;
            lblError.Visible = false;
            // 
            // pnlFooter
            // 
            pnlFooter.BackColor = Color.FromArgb(235, 238, 248);
            pnlFooter.Controls.Add(btnSettings);
            pnlFooter.Controls.Add(lblFooter);
            pnlFooter.Controls.Add(lblConnInfo);
            pnlFooter.Controls.Add(cmbIpSuggest);
            pnlFooter.Controls.Add(lblIpSuggest);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 397);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(344, 44);
            pnlFooter.TabIndex = 1;
            // 
            // btnSettings
            // 
            btnSettings.BackColor = Color.FromArgb(50, 70, 110);
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Segoe UI", 8.5F);
            btnSettings.ForeColor = Color.White;
            btnSettings.Location = new Point(8, 9);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(100, 26);
            btnSettings.TabIndex = 3;
            btnSettings.Text = "⚙  Settings";
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            // 
            // lblFooter
            // 
            lblFooter.Font = new Font("Segoe UI", 7.5F);
            lblFooter.ForeColor = Color.FromArgb(160, 170, 190);
            lblFooter.Location = new Point(120, 9);
            lblFooter.Name = "lblFooter";
            lblFooter.Size = new Size(203, 26);
            lblFooter.TabIndex = 4;
            lblFooter.Text = "DGTHMC · HIMS · OPD | I.T NGANI | Dether/Zaheer Lagos";
            lblFooter.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblConnInfo
            // 
            lblConnInfo.Location = new Point(-100, -100);
            lblConnInfo.Name = "lblConnInfo";
            lblConnInfo.Size = new Size(1, 1);
            lblConnInfo.TabIndex = 2;
            lblConnInfo.Visible = false;
            // 
            // cmbIpSuggest
            // 
            cmbIpSuggest.Location = new Point(-100, -100);
            cmbIpSuggest.Name = "cmbIpSuggest";
            cmbIpSuggest.Size = new Size(1, 23);
            cmbIpSuggest.TabIndex = 13;
            cmbIpSuggest.Visible = false;
            cmbIpSuggest.SelectedIndexChanged += cmbIpSuggest_SelectedIndexChanged;
            // 
            // lblIpSuggest
            // 
            lblIpSuggest.Location = new Point(-100, -100);
            lblIpSuggest.Name = "lblIpSuggest";
            lblIpSuggest.Size = new Size(1, 1);
            lblIpSuggest.TabIndex = 14;
            lblIpSuggest.Visible = false;
            // 
            // LoginForm
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 247, 252);
            ClientSize = new Size(564, 441);
            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximumSize = new Size(580, 480);
            MinimumSize = new Size(580, 480);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login : HIMS File Manager";
            Load += LoginForm_Load;
            pnlLeft.ResumeLayout(false);
            pnlLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            pnlRight.ResumeLayout(false);
            pnlCard.ResumeLayout(false);
            pnlCard.PerformLayout();
            pnlUserBox.ResumeLayout(false);
            pnlUserBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picUserIcon).EndInit();
            pnlPassBox.ResumeLayout(false);
            pnlPassBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPassIcon).EndInit();
            pnlFooter.ResumeLayout(false);
            ResumeLayout(false);
        }

        // NOTE: the PnlCard_Paint / PnlInput_Paint / BtnLogin_MouseEnter /
        // BtnLogin_MouseLeave handlers used to be defined here, but method
        // bodies containing LINQ (.OfType/.Any) or non-trivial logic inside
        // the .Designer.cs file break the WinForms out-of-process designer's
        // ability to host this form (the "Missing Form SubType" bug — "View
        // Designer" silently disappears from the right-click menu). The
        // event wiring above (pnlCard.Paint += PnlCard_Paint; etc.) is a
        // plain method-group subscription, which IS designer-safe and stays
        // here; the handler bodies themselves now live in LoginForm.cs.

        // ── Control declarations ──────────────────────────────────────────────
        private Panel pnlLeft, pnlLogo, pnlRight, pnlCard, pnlDivider,
                            pnlUserBox, pnlPassBox, pnlFooter;
        private PictureBox picUserIcon, picPassIcon;
        private Label lblTitle, lblSub, lblTagline,
                            lblWelcome, lblSignIn,
                            lblUser, lblPass, lblError, lblFooter,
                            lblConnInfo, lblIpSuggest;
        private TextBox txtUsername, txtPassword;
        private CheckBox chkShow;
        private Button btnLogin, btnSettings;
        private ComboBox cmbIpSuggest;
        private TableLayoutPanel layoutBody, pnlFooterRow;   // kept for Dispose safety
        private PictureBox pictureBox1;
    }
}