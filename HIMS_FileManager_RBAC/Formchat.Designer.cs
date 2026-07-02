namespace UPLOADER
{
    partial class FormChat
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        // ── Theme palette ────────────────────────────────────────────────────
        private static readonly Color ClrSidebarBg = Color.FromArgb(30, 41, 59);    // slate-800
        private static readonly Color ClrSidebarHover = Color.FromArgb(41, 53, 74);    // slate-700ish
        private static readonly Color ClrSidebarSel = Color.FromArgb(37, 99, 235);   // blue-600
        private static readonly Color ClrSidebarText = Color.FromArgb(203, 213, 225); // slate-300
        private static readonly Color ClrSidebarSub = Color.FromArgb(148, 163, 184); // slate-400
        private static readonly Color ClrAccent = Color.FromArgb(37, 99, 235);   // blue-600
        private static readonly Color ClrChatBg = Color.FromArgb(241, 245, 249); // slate-100
        private static readonly Color ClrHeaderBg = Color.White;
        private static readonly Color ClrBorder = Color.FromArgb(226, 232, 240); // slate-200

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormChat));
            pnlLeft = new Panel();
            lstContacts = new ListBox();
            pnlSidebarHeader = new Panel();
            lblContacts = new Label();
            txtSearchContacts = new TextBox();
            pnlTop = new Panel();
            lblChatTitle = new Label();
            lblChatSubtitle = new Label();
            btnRefresh = new Button();
            pnlChatArea = new Panel();
            txtMessages = new SmoothRichTextBox();
            pnlBottom = new Panel();
            txtMessage = new TextBox();
            btnSend = new Button();
            tmrPoll = new System.Windows.Forms.Timer(components);
            tmrHeartbeat = new System.Windows.Forms.Timer(components);
            pnlLeft.SuspendLayout();
            pnlSidebarHeader.SuspendLayout();
            pnlTop.SuspendLayout();
            pnlChatArea.SuspendLayout();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            // 
            // pnlLeft
            // 
            pnlLeft.Controls.Add(lstContacts);
            pnlLeft.Controls.Add(pnlSidebarHeader);
            pnlLeft.Dock = DockStyle.Left;
            pnlLeft.Location = new Point(0, 0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(250, 620);
            pnlLeft.TabIndex = 3;
            // 
            // lstContacts
            // 
            lstContacts.BorderStyle = BorderStyle.None;
            lstContacts.Dock = DockStyle.Fill;
            lstContacts.DrawMode = DrawMode.OwnerDrawFixed;
            lstContacts.Font = new Font("Segoe UI", 10F);
            lstContacts.IntegralHeight = false;
            lstContacts.ItemHeight = 56;
            lstContacts.Location = new Point(0, 84);
            lstContacts.Name = "lstContacts";
            lstContacts.Size = new Size(250, 536);
            lstContacts.TabIndex = 0;
            lstContacts.DrawItem += lstContacts_DrawItem;
            lstContacts.SelectedIndexChanged += lstContacts_SelectedIndexChanged;
            // 
            // pnlSidebarHeader
            // 
            pnlSidebarHeader.Controls.Add(lblContacts);
            pnlSidebarHeader.Controls.Add(txtSearchContacts);
            pnlSidebarHeader.Dock = DockStyle.Top;
            pnlSidebarHeader.Location = new Point(0, 0);
            pnlSidebarHeader.Name = "pnlSidebarHeader";
            pnlSidebarHeader.Padding = new Padding(16, 14, 16, 12);
            pnlSidebarHeader.Size = new Size(250, 84);
            pnlSidebarHeader.TabIndex = 1;
            // 
            // lblContacts
            // 
            lblContacts.AutoSize = true;
            lblContacts.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
            lblContacts.ForeColor = Color.White;
            lblContacts.Location = new Point(16, 12);
            lblContacts.Name = "lblContacts";
            lblContacts.Size = new Size(93, 25);
            lblContacts.TabIndex = 0;
            lblContacts.Text = "Messages";
            // 
            // txtSearchContacts
            // 
            txtSearchContacts.BorderStyle = BorderStyle.FixedSingle;
            txtSearchContacts.Font = new Font("Segoe UI", 9.5F);
            txtSearchContacts.Location = new Point(16, 44);
            txtSearchContacts.Name = "txtSearchContacts";
            txtSearchContacts.PlaceholderText = "🔍  Search people…";
            txtSearchContacts.Size = new Size(218, 24);
            txtSearchContacts.TabIndex = 1;
            txtSearchContacts.TextChanged += txtSearchContacts_TextChanged;
            // 
            // pnlTop
            // 
            pnlTop.Controls.Add(lblChatTitle);
            pnlTop.Controls.Add(lblChatSubtitle);
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(250, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Padding = new Padding(20, 0, 16, 0);
            pnlTop.Size = new Size(750, 64);
            pnlTop.TabIndex = 2;
            // 
            // lblChatTitle
            // 
            lblChatTitle.AutoSize = true;
            lblChatTitle.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblChatTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblChatTitle.Location = new Point(20, 12);
            lblChatTitle.Name = "lblChatTitle";
            lblChatTitle.Size = new Size(102, 21);
            lblChatTitle.TabIndex = 0;
            lblChatTitle.Text = "Public Room";
            // 
            // lblChatSubtitle
            // 
            lblChatSubtitle.AutoSize = true;
            lblChatSubtitle.Font = new Font("Segoe UI", 8.5F);
            lblChatSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblChatSubtitle.Location = new Point(20, 36);
            lblChatSubtitle.Name = "lblChatSubtitle";
            lblChatSubtitle.Size = new Size(106, 15);
            lblChatSubtitle.TabIndex = 1;
            lblChatSubtitle.Text = "Visible to everyone";
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.BackColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Font = new Font("Segoe UI", 9F);
            btnRefresh.ForeColor = Color.FromArgb(71, 85, 105);
            btnRefresh.Location = new Point(1434, 17);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(96, 30);
            btnRefresh.TabIndex = 2;
            btnRefresh.Text = "↺  Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // pnlChatArea
            // 
            pnlChatArea.Controls.Add(txtMessages);
            pnlChatArea.Dock = DockStyle.Fill;
            pnlChatArea.Location = new Point(250, 64);
            pnlChatArea.Name = "pnlChatArea";
            pnlChatArea.Padding = new Padding(16, 12, 16, 12);
            pnlChatArea.Size = new Size(750, 488);
            pnlChatArea.TabIndex = 0;
            // 
            // txtMessages
            // 
            txtMessages.BorderStyle = BorderStyle.None;
            txtMessages.Dock = DockStyle.Fill;
            txtMessages.Font = new Font("Segoe UI", 9.5F);
            txtMessages.Location = new Point(16, 12);
            txtMessages.Name = "txtMessages";
            txtMessages.ReadOnly = true;
            txtMessages.Size = new Size(718, 464);
            txtMessages.TabIndex = 0;
            txtMessages.Text = "";
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = Color.White;
            pnlBottom.Controls.Add(txtMessage);
            pnlBottom.Controls.Add(btnSend);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(250, 552);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Padding = new Padding(16, 14, 16, 14);
            pnlBottom.Size = new Size(750, 68);
            pnlBottom.TabIndex = 1;
            // 
            // txtMessage
            // 
            txtMessage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMessage.BorderStyle = BorderStyle.FixedSingle;
            txtMessage.Font = new Font("Segoe UI", 10.5F);
            txtMessage.Location = new Point(8, 14);
            txtMessage.Name = "txtMessage";
            txtMessage.PlaceholderText = "Type a message…";
            txtMessage.Size = new Size(732, 26);
            txtMessage.TabIndex = 0;
            txtMessage.KeyDown += txtMessage_KeyDown;
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            btnSend.ForeColor = Color.White;
            btnSend.Location = new Point(1350, 16);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(104, 34);
            btnSend.TabIndex = 1;
            btnSend.Text = "Send  ➤";
            btnSend.Click += btnSend_Click;
            // 
            // tmrPoll
            // 
            tmrPoll.Interval = 3000;
            tmrPoll.Tick += tmrPoll_Tick;
            // 
            // tmrHeartbeat
            // 
            tmrHeartbeat.Interval = 20000;
            tmrHeartbeat.Tick += tmrHeartbeat_Tick;
            // 
            // FormChat
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 620);
            Controls.Add(pnlChatArea);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
            Controls.Add(pnlLeft);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(760, 460);
            Name = "FormChat";
            StartPosition = FormStartPosition.CenterParent;
            Text = "HIMS : Chat";
            FormClosed += FormChat_FormClosed;
            Load += FormChat_Load;
            pnlLeft.ResumeLayout(false);
            pnlSidebarHeader.ResumeLayout(false);
            pnlSidebarHeader.PerformLayout();
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            pnlChatArea.ResumeLayout(false);
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            ResumeLayout(false);
        }

        private Panel pnlLeft;
        private Panel pnlSidebarHeader;
        private Label lblContacts;
        private TextBox txtSearchContacts;
        private ListBox lstContacts;

        private Panel pnlTop;
        private Label lblChatTitle;
        private Label lblChatSubtitle;
        private Button btnRefresh;

        private Panel pnlChatArea;
        private SmoothRichTextBox txtMessages;

        private Panel pnlBottom;
        private TextBox txtMessage;
        private Button btnSend;

        private System.Windows.Forms.Timer tmrPoll;
        private System.Windows.Forms.Timer tmrHeartbeat;
    }
}