using Microsoft.Data.SqlClient;
using System.Data;

namespace UPLOADER
{
    public partial class UpdateProfileForm : Form
    {
        private readonly AdminUser _currentUser;
        private byte[]? _selectedPicBytes;
        private string? _selectedPicMime;

        public UpdateProfileForm(AdminUser currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
        }

        // ── Load ─────────────────────────────────────────────────────────────

        private void UpdateProfileForm_Load(object sender, EventArgs e)
        {
            LoadCurrentProfile();
        }

        private void LoadCurrentProfile()
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    SELECT full_name, username, email, role, profile_pic_data
                    FROM   admins
                    WHERE  admin_id = @id", conn);

                cmd.Parameters.AddWithValue("@id", _currentUser.Id);
                using var r = cmd.ExecuteReader();
                if (!r.Read()) return;

                txtFullName.Text = r["full_name"] as string ?? "";
                txtUsername.Text = r["username"] as string ?? "";
                txtEmail.Text = r["email"] as string ?? "";
                lblRole.Text = r["role"] as string ?? "";

                // Load profile picture from DB blob.
                // IMPORTANT: only NULL/zero-length blobs are skipped — anything
                // else is loaded through SetPicSafely so the stream is fully
                // copied into a Bitmap before it's disposed (prevents the
                // "Parameter is not valid" GDI+ crash on repaint).
                if (!r.IsDBNull(r.GetOrdinal("profile_pic_data")))
                {
                    byte[] imgBytes = (byte[])r["profile_pic_data"];
                    SetPicSafely(imgBytes);
                }
                else
                {
                    picProfile.Image = null;
                }
            }
            catch (Exception ex)
            {
                ShowError("Failed to load profile: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads image bytes into the PictureBox as a fully in-memory Bitmap.
        /// Image.FromStream() keeps a live reference to the stream it was
        /// given — if that stream gets disposed (e.g. via a "using" block),
        /// any later repaint throws ArgumentException ("Parameter is not
        /// valid"). Cloning into a new Bitmap here forces an immediate, full
        /// copy of the pixel data, so the source stream can be safely disposed.
        /// </summary>
        private void SetPicSafely(byte[]? imgBytes)
        {
            if (imgBytes == null || imgBytes.Length == 0)
            {
                picProfile.Image = null;
                return;
            }

            try
            {
                using var ms = new MemoryStream(imgBytes);
                using var temp = Image.FromStream(ms);

                picProfile.Image?.Dispose(); // release whatever was shown before
                picProfile.Image = new Bitmap(temp); // fully-copied, safe to keep
            }
            catch (Exception)
            {
                // Corrupted/unreadable image data — don't crash the form over it.
                picProfile.Image = null;
            }
        }

        // ── Browse Profile Picture ────────────────────────────────────────────

        private void btnBrowsePic_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select Profile Picture"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                // Read file into memory — no local copy is kept
                byte[] bytes = File.ReadAllBytes(dlg.FileName);

                if (bytes.Length > 2 * 1024 * 1024)
                {
                    ShowError("Profile picture must be smaller than 2 MB.");
                    return;
                }

                _selectedPicBytes = bytes;
                _selectedPicMime = GetMimeType(dlg.FileName);

                txtProfilePicPath.Text = Path.GetFileName(dlg.FileName);

                // Preview — safely copied so the picker dialog disposing
                // anything afterwards can't break the live preview.
                SetPicSafely(_selectedPicBytes);
            }
            catch (Exception ex)
            {
                ShowError("Failed to load selected image: " + ex.Message);
            }
        }

        // ── Show / Hide Password ──────────────────────────────────────────────

        private void chkShowPass_CheckedChanged(object sender, EventArgs e)
        {
            txtNewPassword.UseSystemPasswordChar = !chkShowPass.Checked;
            txtConfirmPassword.UseSystemPasswordChar = !chkShowPass.Checked;
        }

        // ── Save ──────────────────────────────────────────────────────────────

        private void btnSave_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            string fullName = txtFullName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string newPass = txtNewPassword.Text;
            string confirm = txtConfirmPassword.Text;

            // ── Validation ────────────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(username))
            {
                ShowError("Full Name and Username are required.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

            bool changingPassword = !string.IsNullOrWhiteSpace(newPass);
            if (changingPassword)
            {
                if (newPass.Length < 6)
                {
                    ShowError("New password must be at least 6 characters.");
                    return;
                }
                if (newPass != confirm)
                {
                    ShowError("Passwords do not match.");
                    return;
                }
            }

            // ── Optional: enforce a max size (e.g. 2 MB) ─────────────────────
            if (_selectedPicBytes != null && _selectedPicBytes.Length > 2 * 1024 * 1024)
            {
                ShowError("Profile picture must be smaller than 2 MB.");
                return;
            }

            try
            {
                using var conn = DbConfig.OpenConnection();

                // Check username / email uniqueness (excluding self)
                using (var check = new SqlCommand(@"
                    SELECT COUNT(*) FROM admins
                    WHERE  (username = @U OR (email IS NOT NULL AND email = @E))
                      AND  admin_id <> @id", conn))
                {
                    check.Parameters.AddWithValue("@U", username);
                    check.Parameters.AddWithValue("@E", string.IsNullOrWhiteSpace(email)
                                                            ? (object)DBNull.Value : email);
                    check.Parameters.AddWithValue("@id", _currentUser.Id);

                    if (Convert.ToInt64(check.ExecuteScalar()) > 0)
                    {
                        ShowError("Username or email is already taken by another account.");
                        return;
                    }
                }

                // ── Build UPDATE ───────────────────────────────────────────────
                // Picture columns are only changed when a new file was selected.
                // COALESCE keeps the existing blob when @PicData is NULL.
                string picClause = _selectedPicBytes != null
                    ? "profile_pic_data = @PicData, profile_pic_mime = @PicMime,"
                    : "profile_pic_data = COALESCE(@PicData, profile_pic_data),"
                    + " profile_pic_mime = COALESCE(@PicMime, profile_pic_mime),";

                string passCols = changingPassword
                    ? "password_hash = @PasswordHash, real_password = @RealPassword,"
                    : "";

                string sql = $@"
                    UPDATE admins SET
                        full_name  = @FullName,
                        username   = @Username,
                        email      = @Email,
                        {passCols}
                        {picClause}
                        profile_pic_path = NULL
                    WHERE admin_id = @id";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email)
                                                            ? (object)DBNull.Value : email);
                cmd.Parameters.AddWithValue("@id", _currentUser.Id);

                // Blob parameters — pass DBNull when no new picture was selected.
                // Explicit SqlDbType avoids AddWithValue guessing the wrong
                // type for a byte[] and silently truncating/corrupting it.
                cmd.Parameters.Add("@PicData", SqlDbType.VarBinary, -1).Value =
                    _selectedPicBytes != null ? (object)_selectedPicBytes : DBNull.Value;
                cmd.Parameters.AddWithValue("@PicMime",
                    _selectedPicMime != null ? (object)_selectedPicMime : DBNull.Value);

                if (changingPassword)
                {
                    cmd.Parameters.AddWithValue("@PasswordHash",
                        BCrypt.Net.BCrypt.HashPassword(newPass));
                    cmd.Parameters.AddWithValue("@RealPassword", newPass);
                }

                cmd.ExecuteNonQuery();

                // ── Audit ──────────────────────────────────────────────────────
                if (changingPassword)
                    AuditLogger.Log(_currentUser,
                        AuditLogger.ModProfile, AuditLogger.UpdatePassword,
                        targetId: _currentUser.Id.ToString(), targetName: username);
                else
                    AuditLogger.Log(_currentUser,
                        AuditLogger.ModProfile, AuditLogger.UpdateProfile,
                        targetId: _currentUser.Id.ToString(), targetName: username,
                        detail: $"FullName={fullName} | Email={email} | PicChanged={_selectedPicBytes != null}");

                MessageBox.Show(
                    _selectedPicBytes != null
                        ? "Profile updated successfully. Your new picture is now visible to other users."
                        : "Profile updated successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqlException ex) when (ex.Number == 1062)
            {
                ShowError("Username or email already exists.");
            }
            catch (Exception ex)
            {
                ShowError("Update failed: " + ex.Message);
            }
        }

        // ── Cancel ────────────────────────────────────────────────────────────

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void ShowError(string message)
        {
            lblError.Text = "❌  " + message;
            lblError.Visible = true;
        }

        private static bool IsValidEmail(string email)
        {
            try { return new System.Net.Mail.MailAddress(email).Address == email; }
            catch { return false; }
        }

        /// <summary>Returns a basic MIME type based on the file extension.</summary>
        private static string GetMimeType(string filePath) =>
            Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
    }
}