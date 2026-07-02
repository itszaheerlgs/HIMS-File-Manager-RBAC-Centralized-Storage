namespace UPLOADER
{
    public partial class NotificationToastForm : Form
    {
        private readonly System.Windows.Forms.Timer _autoCloseTimer = new();
        public event EventHandler? ToastClicked;

        public NotificationToastForm(string title, string message, int autoCloseMs = 4500)
        {
            InitializeComponent();
            lblTitle.Text = title;
            lblMessage.Text = message;

            _autoCloseTimer.Interval = autoCloseMs;
            _autoCloseTimer.Tick += (s, e) => { _autoCloseTimer.Stop(); SafeClose(); };
            _autoCloseTimer.Start();
        }

        private void SafeClose()
        {
            if (IsDisposed) return;
            if (InvokeRequired) { Invoke(SafeClose); return; }
            Close();
        }

        // Show without stealing focus, positioned bottom-right of the working area.
        public void ShowToast()
        {
            var area = Screen.PrimaryScreen!.WorkingArea;
            Location = new Point(area.Right - Width - 16, area.Bottom - Height - 16);
            Show();
            BringToFront();
        }

        private void NotificationToastForm_Click(object sender, EventArgs e) => RaiseClickAndClose();
        private void lblTitle_Click(object sender, EventArgs e) => RaiseClickAndClose();
        private void lblMessage_Click(object sender, EventArgs e) => RaiseClickAndClose();

        private void RaiseClickAndClose()
        {
            ToastClicked?.Invoke(this, EventArgs.Empty);
            _autoCloseTimer.Stop();
            Close();
        }

        private void btnDismiss_Click(object sender, EventArgs e)
        {
            _autoCloseTimer.Stop();
            Close();
        }
    }
}