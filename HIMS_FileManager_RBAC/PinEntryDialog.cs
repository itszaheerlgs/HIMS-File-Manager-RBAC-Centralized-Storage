namespace UPLOADER
{
    /// <summary>
    /// Simple modal dialog that asks the user to enter the 6-digit Settings PIN.
    /// </summary>
    public class PinEntryDialog : Form
    {
        public string EnteredPin => txtPin.Text.Trim();

        private TextBox txtPin = null!;
        private Button btnOk = null!;
        private Button btnCancel = null!;
        private Label lblPrompt = null!;

        public PinEntryDialog()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            // Form properties
            Text = "Enter Settings PIN";
            ClientSize = new Size(300, 160);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 247, 252);
            ShowInTaskbar = false;
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            // Prompt label
            lblPrompt = new Label
            {
                Text = "Enter the 6-digit PIN sent to your email:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(28, 50, 90),
                Location = new Point(20, 20),
                AutoSize = false,
                Size = new Size(260, 32),
            };

            // PIN textbox
            txtPin = new TextBox
            {
                Font = new Font("Consolas", 14F, FontStyle.Bold),
                Location = new Point(80, 58),
                Size = new Size(140, 28),
                MaxLength = 6,
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle,
            };
            txtPin.KeyPress += (s, e) =>
            {
                // Allow digits and backspace only
                if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                    e.Handled = true;
            };

            // OK button
            btnOk = new Button
            {
                Text = "Confirm",
                DialogResult = DialogResult.OK,
                Location = new Point(60, 108),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(28, 50, 90),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnOk.FlatAppearance.BorderSize = 0;

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(158, 108),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(180, 185, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand,
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            Controls.AddRange(new Control[] { lblPrompt, txtPin, btnOk, btnCancel });

            // Wire AcceptButton after controls are added
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void InitializeComponent()
        {

        }
    }
}