namespace UPLOADER
{
    partial class SuggestionsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SuggestionsForm));
            pnlTop = new Panel();
            lblTitle = new Label();
            lblSearch = new Label();
            txtSearch = new TextBox();
            btnRefresh = new Button();
            chkUnrepliedOnly = new CheckBox();
            lblCount = new Label();
            lblUnreplied = new Label();
            dgv = new DataGridView();
            pnlReply = new Panel();
            lblPanelTitle = new Label();
            lblComposeTitle = new Label();
            lblNewSuggestion = new Label();
            txtNewSuggestion = new RichTextBox();
            lblComposeError = new Label();
            btnSendSuggestion = new Button();
            lblFrom = new Label();
            lblFromValue = new Label();
            lblDate = new Label();
            lblDateValue = new Label();
            lblSuggestion = new Label();
            txtSuggestionView = new RichTextBox();
            lblReply = new Label();
            txtReply = new RichTextBox();
            lblError = new Label();
            btnSendReply = new Button();
            btnClearReply = new Button();
            btnDelete = new Button();
            btnClose = new Button();
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            pnlReply.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTop
            // 
            pnlTop.BackColor = SystemColors.ControlLight;
            pnlTop.Controls.Add(lblTitle);
            pnlTop.Controls.Add(lblSearch);
            pnlTop.Controls.Add(txtSearch);
            pnlTop.Controls.Add(btnRefresh);
            pnlTop.Controls.Add(chkUnrepliedOnly);
            pnlTop.Controls.Add(lblCount);
            pnlTop.Controls.Add(lblUnreplied);
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Location = new Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new Size(1000, 48);
            pnlTop.TabIndex = 2;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblTitle.Location = new Point(10, 13);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(124, 20);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "💡  Suggestions";
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(190, 16);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(19, 15);
            lblSearch.TabIndex = 1;
            lblSearch.Text = "🔍";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(210, 13);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search from, suggestion…";
            txtSearch.Size = new Size(200, 23);
            txtSearch.TabIndex = 2;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(420, 12);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(80, 26);
            btnRefresh.TabIndex = 3;
            btnRefresh.Text = "↺ Refresh";
            btnRefresh.Click += btnRefresh_Click;
            // 
            // chkUnrepliedOnly
            // 
            chkUnrepliedOnly.AutoSize = true;
            chkUnrepliedOnly.Location = new Point(512, 15);
            chkUnrepliedOnly.Name = "chkUnrepliedOnly";
            chkUnrepliedOnly.Size = new Size(103, 19);
            chkUnrepliedOnly.TabIndex = 4;
            chkUnrepliedOnly.Text = "Unreplied only";
            chkUnrepliedOnly.CheckedChanged += chkUnrepliedOnly_CheckedChanged;
            // 
            // lblCount
            // 
            lblCount.AutoSize = true;
            lblCount.ForeColor = SystemColors.GrayText;
            lblCount.Location = new Point(638, 16);
            lblCount.Name = "lblCount";
            lblCount.Size = new Size(0, 15);
            lblCount.TabIndex = 5;
            // 
            // lblUnreplied
            // 
            lblUnreplied.AutoSize = true;
            lblUnreplied.Location = new Point(730, 16);
            lblUnreplied.Name = "lblUnreplied";
            lblUnreplied.Size = new Size(0, 15);
            lblUnreplied.TabIndex = 6;
            // 
            // dgv
            // 
            dgv.Dock = DockStyle.Fill;
            dgv.Font = new Font("Segoe UI", 9F);
            dgv.Location = new Point(0, 48);
            dgv.Name = "dgv";
            dgv.Size = new Size(680, 512);
            dgv.TabIndex = 0;
            // 
            // pnlReply
            // 
            pnlReply.AutoScroll = true;
            pnlReply.BackColor = SystemColors.Control;
            pnlReply.Controls.Add(lblPanelTitle);
            pnlReply.Controls.Add(lblComposeTitle);
            pnlReply.Controls.Add(lblNewSuggestion);
            pnlReply.Controls.Add(txtNewSuggestion);
            pnlReply.Controls.Add(lblComposeError);
            pnlReply.Controls.Add(btnSendSuggestion);
            pnlReply.Controls.Add(lblFrom);
            pnlReply.Controls.Add(lblFromValue);
            pnlReply.Controls.Add(lblDate);
            pnlReply.Controls.Add(lblDateValue);
            pnlReply.Controls.Add(lblSuggestion);
            pnlReply.Controls.Add(txtSuggestionView);
            pnlReply.Controls.Add(lblReply);
            pnlReply.Controls.Add(txtReply);
            pnlReply.Controls.Add(lblError);
            pnlReply.Controls.Add(btnSendReply);
            pnlReply.Controls.Add(btnClearReply);
            pnlReply.Controls.Add(btnDelete);
            pnlReply.Controls.Add(btnClose);
            pnlReply.Dock = DockStyle.Right;
            pnlReply.Location = new Point(680, 48);
            pnlReply.Name = "pnlReply";
            pnlReply.Padding = new Padding(10);
            pnlReply.Size = new Size(320, 512);
            pnlReply.TabIndex = 1;
            // 
            // lblPanelTitle
            // 
            lblPanelTitle.AutoSize = true;
            lblPanelTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPanelTitle.Location = new Point(0, 0);
            lblPanelTitle.Name = "lblPanelTitle";
            lblPanelTitle.Size = new Size(47, 19);
            lblPanelTitle.TabIndex = 0;
            lblPanelTitle.Text = "Reply";
            // 
            // lblComposeTitle
            // 
            lblComposeTitle.AutoSize = true;
            lblComposeTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblComposeTitle.Location = new Point(0, 0);
            lblComposeTitle.Name = "lblComposeTitle";
            lblComposeTitle.Size = new Size(153, 17);
            lblComposeTitle.TabIndex = 1;
            lblComposeTitle.Text = "Send a New Suggestion";
            lblComposeTitle.Visible = false;
            // 
            // lblNewSuggestion
            // 
            lblNewSuggestion.AutoSize = true;
            lblNewSuggestion.ForeColor = SystemColors.GrayText;
            lblNewSuggestion.Location = new Point(0, 0);
            lblNewSuggestion.Name = "lblNewSuggestion";
            lblNewSuggestion.Size = new Size(95, 15);
            lblNewSuggestion.TabIndex = 2;
            lblNewSuggestion.Text = "Your suggestion:";
            lblNewSuggestion.Visible = false;
            // 
            // txtNewSuggestion
            // 
            txtNewSuggestion.BorderStyle = BorderStyle.FixedSingle;
            txtNewSuggestion.Font = new Font("Segoe UI", 9F);
            txtNewSuggestion.Location = new Point(0, 0);
            txtNewSuggestion.Name = "txtNewSuggestion";
            txtNewSuggestion.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtNewSuggestion.Size = new Size(296, 90);
            txtNewSuggestion.TabIndex = 3;
            txtNewSuggestion.Text = "";
            txtNewSuggestion.Visible = false;
            // 
            // lblComposeError
            // 
            lblComposeError.AutoSize = true;
            lblComposeError.ForeColor = Color.Red;
            lblComposeError.Location = new Point(0, 0);
            lblComposeError.Name = "lblComposeError";
            lblComposeError.Size = new Size(0, 15);
            lblComposeError.TabIndex = 4;
            lblComposeError.Visible = false;
            // 
            // btnSendSuggestion
            // 
            btnSendSuggestion.BackColor = Color.FromArgb(0, 122, 204);
            btnSendSuggestion.FlatStyle = FlatStyle.Flat;
            btnSendSuggestion.ForeColor = Color.White;
            btnSendSuggestion.Location = new Point(0, 0);
            btnSendSuggestion.Name = "btnSendSuggestion";
            btnSendSuggestion.Size = new Size(296, 30);
            btnSendSuggestion.TabIndex = 5;
            btnSendSuggestion.Text = "📨  Send Suggestion";
            btnSendSuggestion.UseVisualStyleBackColor = false;
            btnSendSuggestion.Visible = false;
            btnSendSuggestion.Click += btnSendSuggestion_Click;
            // 
            // lblFrom
            // 
            lblFrom.AutoSize = true;
            lblFrom.ForeColor = SystemColors.GrayText;
            lblFrom.Location = new Point(0, 0);
            lblFrom.Name = "lblFrom";
            lblFrom.Size = new Size(38, 15);
            lblFrom.TabIndex = 6;
            lblFrom.Text = "From:";
            // 
            // lblFromValue
            // 
            lblFromValue.AutoSize = true;
            lblFromValue.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFromValue.Location = new Point(0, 0);
            lblFromValue.Name = "lblFromValue";
            lblFromValue.Size = new Size(19, 15);
            lblFromValue.TabIndex = 7;
            lblFromValue.Text = "—";
            // 
            // lblDate
            // 
            lblDate.AutoSize = true;
            lblDate.ForeColor = SystemColors.GrayText;
            lblDate.Location = new Point(0, 0);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(34, 15);
            lblDate.TabIndex = 8;
            lblDate.Text = "Date:";
            // 
            // lblDateValue
            // 
            lblDateValue.AutoSize = true;
            lblDateValue.Location = new Point(0, 0);
            lblDateValue.Name = "lblDateValue";
            lblDateValue.Size = new Size(19, 15);
            lblDateValue.TabIndex = 9;
            lblDateValue.Text = "—";
            // 
            // lblSuggestion
            // 
            lblSuggestion.AutoSize = true;
            lblSuggestion.Location = new Point(0, 0);
            lblSuggestion.Name = "lblSuggestion";
            lblSuggestion.Size = new Size(66, 15);
            lblSuggestion.TabIndex = 10;
            lblSuggestion.Text = "Suggestion";
            // 
            // txtSuggestionView
            // 
            txtSuggestionView.BackColor = SystemColors.Window;
            txtSuggestionView.BorderStyle = BorderStyle.FixedSingle;
            txtSuggestionView.Font = new Font("Segoe UI", 9F);
            txtSuggestionView.Location = new Point(0, 0);
            txtSuggestionView.Name = "txtSuggestionView";
            txtSuggestionView.ReadOnly = true;
            txtSuggestionView.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtSuggestionView.Size = new Size(296, 100);
            txtSuggestionView.TabIndex = 11;
            txtSuggestionView.Text = "";
            // 
            // lblReply
            // 
            lblReply.AutoSize = true;
            lblReply.Location = new Point(0, 0);
            lblReply.Name = "lblReply";
            lblReply.Size = new Size(143, 15);
            lblReply.TabIndex = 12;
            lblReply.Text = "Your Reply  (SuperAdmin)";
            // 
            // txtReply
            // 
            txtReply.BorderStyle = BorderStyle.FixedSingle;
            txtReply.Font = new Font("Segoe UI", 9F);
            txtReply.Location = new Point(0, 0);
            txtReply.Name = "txtReply";
            txtReply.ScrollBars = RichTextBoxScrollBars.Vertical;
            txtReply.Size = new Size(296, 110);
            txtReply.TabIndex = 13;
            txtReply.Text = "";
            // 
            // lblError
            // 
            lblError.AutoSize = true;
            lblError.ForeColor = Color.Red;
            lblError.Location = new Point(0, 0);
            lblError.Name = "lblError";
            lblError.Size = new Size(0, 15);
            lblError.TabIndex = 14;
            lblError.Visible = false;
            // 
            // btnSendReply
            // 
            btnSendReply.BackColor = Color.FromArgb(0, 122, 204);
            btnSendReply.Enabled = false;
            btnSendReply.FlatStyle = FlatStyle.Flat;
            btnSendReply.ForeColor = Color.White;
            btnSendReply.Location = new Point(0, 0);
            btnSendReply.Name = "btnSendReply";
            btnSendReply.Size = new Size(296, 30);
            btnSendReply.TabIndex = 15;
            btnSendReply.Text = "📨  Send Reply";
            btnSendReply.UseVisualStyleBackColor = false;
            btnSendReply.Click += btnSendReply_Click;
            // 
            // btnClearReply
            // 
            btnClearReply.Enabled = false;
            btnClearReply.Location = new Point(0, 0);
            btnClearReply.Name = "btnClearReply";
            btnClearReply.Size = new Size(144, 28);
            btnClearReply.TabIndex = 16;
            btnClearReply.Text = "\U0001f9f9  Clear Reply";
            btnClearReply.Click += btnClearReply_Click;
            // 
            // btnDelete
            // 
            btnDelete.Enabled = false;
            btnDelete.ForeColor = Color.DarkRed;
            btnDelete.Location = new Point(0, 0);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(144, 28);
            btnDelete.TabIndex = 17;
            btnDelete.Text = "🗑  Delete";
            btnDelete.Click += btnDelete_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(0, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(296, 28);
            btnClose.TabIndex = 18;
            btnClose.Text = "✖  Close";
            btnClose.Click += btnClose_Click;
            // 
            // SuggestionsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 560);
            Controls.Add(dgv);
            Controls.Add(pnlReply);
            Controls.Add(pnlTop);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(780, 460);
            Name = "SuggestionsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "HIMS : Suggestions";
            Load += SuggestionsForm_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            pnlReply.ResumeLayout(false);
            pnlReply.PerformLayout();
            ResumeLayout(false);
        }

        // ── Control Declarations ──────────────────────────────────────────────
        private Panel pnlTop;
        private Label lblTitle;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnRefresh;
        private Label lblCount;
        private Label lblUnreplied;
        private CheckBox chkUnrepliedOnly;

        private DataGridView dgv;

        private Panel pnlReply;
        private Label lblPanelTitle;
        private Label lblComposeTitle;
        private Label lblNewSuggestion;
        private RichTextBox txtNewSuggestion;
        private Label lblComposeError;
        private Button btnSendSuggestion;
        private Label lblFrom;
        private Label lblFromValue;
        private Label lblDate;
        private Label lblDateValue;
        private Label lblSuggestion;
        private RichTextBox txtSuggestionView;
        private Label lblReply;
        private RichTextBox txtReply;
        private Label lblError;
        private Button btnSendReply;
        private Button btnClearReply;
        private Button btnDelete;
        private Button btnClose;
    }
}