using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace UPLOADER
{
    /// <summary>
    /// Stamps a "username + timestamp" watermark onto images — used for both
    /// file downloads and the Print Preview feature, so a leaked copy of a
    /// downloaded/printed document can always be traced back to who pulled it
    /// and when. Controlled by AppSettingsService.WatermarkEnabledKey, which
    /// only a SuperAdmin can toggle (see SettingsForm).
    ///
    /// Scope note: watermarking works on image files (jpg/png/bmp/gif/tiff),
    /// which are this system's primary document format (scanned documents,
    /// X-rays, etc. — see the TWAIN scanner and Print Preview features, which
    /// are also image-only). Non-image downloads (pdf/docx/xlsx/txt/...) are
    /// still fully covered by the existing audit log (who downloaded what,
    /// and when), just not pixel-watermarked, since doing that safely would
    /// need a PDF/Office manipulation library this project doesn't reference.
    /// </summary>
    internal static class WatermarkService
    {
        private static readonly string[] ImageExtensions =
            { "jpg", "jpeg", "png", "bmp", "gif", "tiff", "tif", "webp" };

        public static bool IsImage(string ext) =>
            Array.IndexOf(ImageExtensions, (ext ?? "").Trim('.').ToLowerInvariant()) >= 0;

        /// <summary>Standard watermark text: who downloaded/printed it, and when.</summary>
        public static string BuildText(AdminUser user) =>
            $"{user.FullName} ({user.Username}) \u2022 {DateTime.Now:yyyy-MM-dd HH:mm}";

        /// <summary>
        /// Draws a repeating diagonal watermark plus a solid, clearly-readable
        /// footer bar across the given area. Works for both an in-memory
        /// Bitmap's Graphics (downloads) and a PrintDocument page's Graphics
        /// (prints) — the caller just passes the area to cover.
        /// </summary>
        public static void DrawOverlay(Graphics g, RectangleF area, string text)
        {
            if (area.Width <= 0 || area.Height <= 0 || string.IsNullOrWhiteSpace(text)) return;

            var oldClip = g.Clip;
            var oldSmoothing = g.SmoothingMode;
            var oldTextRender = g.TextRenderingHint;
            GraphicsState state = g.Save();
            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.SetClip(area);

                float fontSize = Math.Max(9f, Math.Min(area.Width, area.Height) / 22f);
                using var font = new Font("Segoe UI", fontSize, FontStyle.Bold);
                using var tileBrush = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
                using var tileShadow = new SolidBrush(Color.FromArgb(45, 0, 0, 0));

                // ── Repeating diagonal tile (subtle, covers the whole image so
                // cropping out the footer bar doesn't remove the watermark) ──
                g.TranslateTransform(area.X + area.Width / 2f, area.Y + area.Height / 2f);
                g.RotateTransform(-30);

                SizeF textSize = g.MeasureString(text, font);
                float tileW = textSize.Width + 70;
                float tileH = textSize.Height + 60;
                float diag = (float)Math.Sqrt(area.Width * area.Width + area.Height * area.Height);
                int steps = (int)(diag / Math.Min(tileW, tileH)) + 3;

                for (int r = -steps; r <= steps; r++)
                {
                    for (int c = -steps; c <= steps; c++)
                    {
                        float x = c * tileW - textSize.Width / 2f;
                        float y = r * tileH - textSize.Height / 2f;
                        g.DrawString(text, font, tileShadow, x + 1.5f, y + 1.5f);
                        g.DrawString(text, font, tileBrush, x, y);
                    }
                }

                g.ResetTransform();

                // ── Solid footer bar: unambiguous, always-legible copy of the
                // same text, so nobody can claim they "didn't notice" a faint
                // diagonal watermark. ──
                using var footerFont = new Font("Segoe UI", Math.Max(8f, area.Height / 45f), FontStyle.Bold);
                SizeF footerTextSize = g.MeasureString(text, footerFont);
                float footerHeight = footerTextSize.Height + 8;
                using var footerBg = new SolidBrush(Color.FromArgb(170, 0, 0, 0));
                var footerRect = new RectangleF(area.X, area.Bottom - footerHeight, area.Width, footerHeight);
                g.FillRectangle(footerBg, footerRect);
                using var footerTextBrush = new SolidBrush(Color.White);
                g.DrawString(text, footerFont, footerTextBrush, area.X + 6, footerRect.Y + 4);
            }
            finally
            {
                g.Restore(state);
                g.Clip = oldClip;
                g.SmoothingMode = oldSmoothing;
                g.TextRenderingHint = oldTextRender;
            }
        }

        /// <summary>Returns a NEW bitmap with the watermark burned into the pixels.</summary>
        public static Bitmap StampImage(Image source, string text)
        {
            var bmp = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            try
            {
                float hRes = source.HorizontalResolution > 0 ? source.HorizontalResolution : 96f;
                float vRes = source.VerticalResolution > 0 ? source.VerticalResolution : 96f;
                bmp.SetResolution(hRes, vRes);
            }
            catch { /* some codecs don't report resolution — default is fine */ }

            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, 0, 0, source.Width, source.Height);
                DrawOverlay(g, new RectangleF(0, 0, source.Width, source.Height), text);
            }
            return bmp;
        }

        /// <summary>Saves an image using the encoder matching the destination file's extension.</summary>
        public static void SaveWithFormat(Image img, string path)
        {
            string ext = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            ImageFormat fmt = ext switch
            {
                "png" => ImageFormat.Png,
                "bmp" => ImageFormat.Bmp,
                "gif" => ImageFormat.Gif,
                "tif" or "tiff" => ImageFormat.Tiff,
                _ => ImageFormat.Jpeg,
            };
            img.Save(path, fmt);
        }
    }
}
