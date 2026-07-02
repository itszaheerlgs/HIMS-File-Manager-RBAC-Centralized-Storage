namespace UPLOADER
{
    public class ImageViewerForm : Form
    {
        private readonly PictureBox _pic;
        private readonly Label _lblInfo;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageViewerForm));
            SuspendLayout();
            // 
            // ImageViewerForm
            // 
            ClientSize = new Size(284, 261);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ImageViewerForm";
            Text = "HIMS : Image Viewer";
            Load += ImageViewerForm_Load;
            ResumeLayout(false);

        }

        public ImageViewerForm(string fileName, Image img)
        {
            Text = $"Preview — {fileName}";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(900, 680);
            BackColor = Color.FromArgb(30, 30, 30);
            MinimizeBox = false;

            _lblInfo = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Text = $"{fileName}   |   {img.Width} × {img.Height} px"
            };

            _pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(30, 30, 30),
                Image = img
            };

            // Zoom with scroll
            _pic.MouseWheel += (s, e) =>
            {
                if (_pic.SizeMode == PictureBoxSizeMode.Zoom && e.Delta > 0)
                    _pic.SizeMode = PictureBoxSizeMode.AutoSize;
                else
                    _pic.SizeMode = PictureBoxSizeMode.Zoom;
            };

            Controls.Add(_pic);
            Controls.Add(_lblInfo);
        }

        private void ImageViewerForm_Load(object sender, EventArgs e)
        {

        }
    }
}
