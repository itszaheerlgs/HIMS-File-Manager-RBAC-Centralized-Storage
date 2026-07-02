using Microsoft.VisualBasic.Devices;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace UPLOADER
{
    partial class NotificationToastForm
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
            lblTitle = new Label();
            lblMessage = new Label();
            btnDismiss = new Button();
            iconBar = new Panel();
            SuspendLayout();

            // ── iconBar (accent strip) ───────────────────────────────────────
            iconBar.BackColor = Color.FromArgb(37, 99, 235);
            iconBar.Dock = DockStyle.Left;
            iconBar.Name = "iconBar";
            iconBar.Width = 6;

            // ── lblTitle ─────────────────────────────────────────────────────
            lblTitle.AutoSize = false;
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(20, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(280, 22);
            lblTitle.Text = "Notification";
            lblTitle.Cursor = Cursors.Hand;
            lblTitle.Click += lblTitle_Click;

            // ── lblMessage ───────────────────────────────────────────────────
            lblMessage.AutoSize = false;
            lblMessage.ForeColor = Color.FromArgb(71, 85, 105);
            lblMessage.Location = new Point(20, 36);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new Size(280, 38);
            lblMessage.Text = "";
            lblMessage.Cursor = Cursors.Hand;
            lblMessage.Click += lblMessage_Click;

            // ── btnDismiss ───────────────────────────────────────────────────
            btnDismiss.FlatStyle = FlatStyle.Flat;
            btnDismiss.FlatAppearance.BorderSize = 0;
            btnDismiss.ForeColor = Color.FromArgb(148, 163, 184);
            btnDismiss.Location = new Point(294, 8);
            btnDismiss.Name = "btnDismiss";
            btnDismiss.Size = new Size(24, 24);
            btnDismiss.Text = "✕";
            btnDismiss.UseVisualStyleBackColor = true;
            btnDismiss.Click += btnDismiss_Click;

            // ── NotificationToastForm ───────────────────────────────────────
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.White;
            ClientSize = new Size(326, 84);
            Controls.Add(iconBar);
            Controls.Add(lblTitle);
            Controls.Add(lblMessage);
            Controls.Add(btnDismiss);
            FormBorderStyle = FormBorderStyle.None;
            Name = "NotificationToastForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Text = "NotificationToastForm";
            Click += NotificationToastForm_Click;
            ResumeLayout(false);
        }

        #endregion

        private Label lblTitle;
        private Label lblMessage;
        private Button btnDismiss;
        private Panel iconBar;
    }
}