using Microsoft.Data.SqlClient;

namespace UPLOADER
{

    public partial class UsersListForm : Form
    {

        private readonly AdminUser _currentUser;
        private int? _selectedAdminId = null;

        // ── Constructor ───────────────────────────────────────────────────────
        public UsersListForm(AdminUser currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            BuildEditPanelLayout();
        }

        // ── Dynamic layout for the right-hand edit panel ────────────────────────
        // Kept out of InitializeComponent() because the WinForms designer's code
        // parser cannot handle local variables or arithmetic inside that method.
        // Called once from the constructor, right after InitializeComponent().
        private void BuildEditPanelLayout()
        {
            int y = 8;
            int lh = 18, th = 23, gap = 8, sec = 14;

            // Title
            lblEditTitle.Location = new Point(10, y);
            y += lh + sec;

            // Full Name
            lblEditFullName.Location = new Point(10, y); y += lh;
            txtEditFullName.Location = new Point(10, y);
            txtEditFullName.Size = new Size(276, th); y += th + gap;

            // Username
            lblEditUsername.Location = new Point(10, y); y += lh;
            txtEditUsername.Location = new Point(10, y);
            txtEditUsername.Size = new Size(276, th); y += th + gap;

            // Email
            lblEditEmail.Location = new Point(10, y); y += lh;
            txtEditEmail.Location = new Point(10, y);
            txtEditEmail.Size = new Size(276, th); y += th + gap;

            // Role
            lblEditRole.Location = new Point(10, y); y += lh;
            cmbEditRole.Location = new Point(10, y);
            cmbEditRole.Size = new Size(276, th);
            y += th + gap;

            // Active
            chkEditActive.Location = new Point(10, y); y += th + gap;

            // Separator
            y += 4;

            // New Password
            lblEditNewPass.Location = new Point(10, y); y += lh;
            txtEditNewPassword.Location = new Point(10, y);
            txtEditNewPassword.Size = new Size(276, th); y += th + gap;

            // Confirm
            lblEditConfirm.Location = new Point(10, y); y += lh;
            txtEditConfirmPassword.Location = new Point(10, y);
            txtEditConfirmPassword.Size = new Size(276, th); y += th + gap;

            // Show pass
            chkShowPass.Location = new Point(10, y); y += th + sec;

            // Self note
            lblSelfNote.Location = new Point(10, y); y += lh + gap;

            // Error
            lblEditError.Location = new Point(10, y); y += lh + sec;

            // Buttons
            btnUpdate.Location = new Point(10, y);
            btnDelete.Location = new Point(148, y);
            y += 36;

            btnToggleActive.Location = new Point(10, y);
            y += 34;

            btnClose.Location = new Point(10, y);
        }

        // ── Load ──────────────────────────────────────────────────────────────
        private void UsersListForm_Load(object sender, EventArgs e)
        {
            SetupGrid();
            LoadUsers();
            ClearEditPanel();
        }

        // ── DataGridView Setup ────────────────────────────────────────────────
        private void SetupGrid()
        {
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Clear();
            dgv.Columns.AddRange(
                Col("admin_id", "#", 45),
                Col("full_name", "Full Name", 160),
                Col("username", "Username", 120),
                Col("email", "Email", 180),
                Col("role", "Role", 140),
                Col("is_active", "Active", 55),
                Col("last_login", "Last Login", 130),
                Col("created_at", "Created", 130)
            );
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = SystemColors.Window;
            dgv.SelectionChanged += dgv_SelectionChanged;
        }

        private static DataGridViewTextBoxColumn Col(string name, string header, int width)
            => new()
            {
                DataPropertyName = name,
                HeaderText = header,
                Name = name,
                MinimumWidth = width,
                FillWeight = width
            };

        // ── Load / Refresh Users ──────────────────────────────────────────────
        private void LoadUsers(string filter = "")
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                string sql = @"
                    SELECT admin_id, full_name, username, email, role,
                           CASE WHEN is_active = 1 THEN N'✔' ELSE N'✘' END AS is_active,
                           last_login, created_at
                    FROM admins
                    WHERE (@filter = ''
                           OR full_name LIKE @filter
                           OR username  LIKE @filter
                           OR email     LIKE @filter
                           OR role      LIKE @filter)
                    ORDER BY admin_id";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@filter",
                    string.IsNullOrWhiteSpace(filter) ? "" : $"%{filter.Trim()}%");

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new System.Data.DataTable();
                adapter.Fill(dt);

                dgv.DataSource = dt;
                lblCount.Text = $"{dt.Rows.Count} user(s)";
            }
            catch (Exception ex)
            {
                ShowError("Failed to load users: " + ex.Message);
            }
        }

        // ── Grid Selection → Fill Edit Panel ──────────────────────────────────
        private void dgv_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) { ClearEditPanel(); return; }
            var row = dgv.CurrentRow;

            _selectedAdminId = Convert.ToInt32(row.Cells["admin_id"].Value);
            txtEditFullName.Text = row.Cells["full_name"].Value?.ToString() ?? "";
            txtEditUsername.Text = row.Cells["username"].Value?.ToString() ?? "";
            txtEditEmail.Text = row.Cells["email"].Value?.ToString() ?? "";
            cmbEditRole.Text = row.Cells["role"].Value?.ToString() ?? "";
            chkEditActive.Checked = row.Cells["is_active"].Value?.ToString() == "✔";

            txtEditNewPassword.Clear();
            txtEditConfirmPassword.Clear();
            lblEditError.Visible = false;

            // Prevent SA from editing themselves in this form (use Update Profile instead)
            bool isSelf = _selectedAdminId == _currentUser.Id;
            btnUpdate.Enabled = true;
            btnDelete.Enabled = !isSelf;   // can't delete yourself
            lblSelfNote.Visible = isSelf;
        }

        // ── Update ────────────────────────────────────────────────────────────
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_currentUser, Permission.User_Update); }
            catch (PermissionDeniedException ex) { ShowEditError(ex.Message); return; }

            lblEditError.Visible = false;

            if (_selectedAdminId == null) { ShowEditError("Select a user first."); return; }

            string fullName = txtEditFullName.Text.Trim();
            string username = txtEditUsername.Text.Trim();
            string email = txtEditEmail.Text.Trim();
            string role = cmbEditRole.Text.Trim();
            bool isActive = chkEditActive.Checked;
            string newPass = txtEditNewPassword.Text;
            string confirm = txtEditConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username))
            { ShowEditError("Full Name and Username are required."); return; }

            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
            { ShowEditError("Invalid email address."); return; }

            bool changingPassword = !string.IsNullOrWhiteSpace(newPass);
            if (changingPassword)
            {
                if (newPass.Length < 6)
                { ShowEditError("Password must be at least 6 characters."); return; }
                if (newPass != confirm)
                { ShowEditError("Passwords do not match."); return; }
            }

            try
            {
                using var conn = DbConfig.OpenConnection();

                // Uniqueness check (exclude self)
                using (var chk = new SqlCommand(@"
                    SELECT COUNT(*) FROM admins
                    WHERE (username=@U OR (email IS NOT NULL AND email=@E))
                      AND admin_id <> @id", conn))
                {
                    chk.Parameters.AddWithValue("@U", username);
                    chk.Parameters.AddWithValue("@E", string.IsNullOrWhiteSpace(email)
                                                        ? (object)DBNull.Value : email);
                    chk.Parameters.AddWithValue("@id", _selectedAdminId);
                    if (Convert.ToInt64(chk.ExecuteScalar()) > 0)
                    { ShowEditError("Username or email already taken."); return; }
                }

                string sql = changingPassword
                    ? @"UPDATE admins SET
                            full_name     = @FullName,
                            username      = @Username,
                            email         = @Email,
                            role          = @Role,
                            is_active     = @IsActive,
                            password_hash = @PwHash,
                            real_password = @RealPw
                        WHERE admin_id = @id"
                    : @"UPDATE admins SET
                            full_name = @FullName,
                            username  = @Username,
                            email     = @Email,
                            role      = @Role,
                            is_active = @IsActive
                        WHERE admin_id = @id";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(email)
                                                          ? (object)DBNull.Value : email);
                cmd.Parameters.AddWithValue("@Role", role);
                cmd.Parameters.AddWithValue("@IsActive", isActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", _selectedAdminId);

                if (changingPassword)
                {
                    cmd.Parameters.AddWithValue("@PwHash", BCrypt.Net.BCrypt.HashPassword(newPass));
                    cmd.Parameters.AddWithValue("@RealPw", newPass);
                }

                cmd.ExecuteNonQuery();
                string updateAction = changingPassword
    ? AuditLogger.UpdatePassword
    : AuditLogger.EditUser;

                AuditLogger.Log(_currentUser,
                    AuditLogger.ModUsers, updateAction,
                    targetId: _selectedAdminId.ToString(),
                    targetName: username,
                    detail: $"Role={role} | Active={isActive} | PasswordChanged={changingPassword}");

                MessageBox.Show($"User '{username}' updated successfully.",
                    "Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadUsers(txtSearch.Text);
            }
            catch (SqlException ex) when (ex.Number == 1062)
            { ShowEditError("Username or email already exists."); }
            catch (Exception ex)
            { ShowEditError("Update failed: " + ex.Message); }
        }

        // ── Delete ────────────────────────────────────────────────────────────
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_currentUser, Permission.User_Delete); }
            catch (PermissionDeniedException ex) { ShowEditError(ex.Message); return; }

            if (_selectedAdminId == null) return;

            if (_selectedAdminId == _currentUser.Id)
            {
                ShowEditError("You cannot delete your own account while logged in.");
                return;
            }

            string name = txtEditFullName.Text.Trim();
            var confirm = MessageBox.Show(
                $"Permanently delete user '{name}'?\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "DELETE FROM admins WHERE admin_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", _selectedAdminId);
                cmd.ExecuteNonQuery();
                AuditLogger.Log(_currentUser,
    AuditLogger.ModUsers, AuditLogger.DeleteUser,
    targetId: _selectedAdminId.ToString(),
    targetName: name);

                MessageBox.Show($"User '{name}' deleted.",
                    "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearEditPanel();
                LoadUsers(txtSearch.Text);
            }
            catch (Exception ex)
            { ShowEditError("Delete failed: " + ex.Message); }
        }

        // ── Toggle Active / Deactivate ─────────────────────────────────────────
        private void btnToggleActive_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_currentUser, Permission.User_ToggleActive); }
            catch (PermissionDeniedException ex) { ShowEditError(ex.Message); return; }

            if (_selectedAdminId == null) return;

            bool makeActive = !chkEditActive.Checked;
            string action = makeActive ? "activate" : "deactivate";
            string name = txtEditFullName.Text.Trim();

            var confirm = MessageBox.Show(
                $"Are you sure you want to {action} '{name}'?",
                "Confirm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "UPDATE admins SET is_active = @v WHERE admin_id = @id", conn);
                cmd.Parameters.AddWithValue("@v", makeActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", _selectedAdminId);
                cmd.ExecuteNonQuery();

                chkEditActive.Checked = makeActive;
                AuditLogger.Log(_currentUser,
    AuditLogger.ModUsers, AuditLogger.ToggleActive,
    targetId: _selectedAdminId.ToString(),
    targetName: name,
    detail: $"SetActive={makeActive}");
                LoadUsers(txtSearch.Text);
            }
            catch (Exception ex)
            { ShowEditError("Toggle failed: " + ex.Message); }
        }

        // ── Search ────────────────────────────────────────────────────────────
        private void txtSearch_TextChanged(object sender, EventArgs e)
            => LoadUsers(txtSearch.Text);

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadUsers();
            ClearEditPanel();
        }

        // ── Show/Hide password ────────────────────────────────────────────────
        private void chkShowPass_CheckedChanged(object sender, EventArgs e)
        {
            txtEditNewPassword.UseSystemPasswordChar = !chkShowPass.Checked;
            txtEditConfirmPassword.UseSystemPasswordChar = !chkShowPass.Checked;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void ClearEditPanel()
        {
            _selectedAdminId = null;
            txtEditFullName.Clear();
            txtEditUsername.Clear();
            txtEditEmail.Clear();
            cmbEditRole.SelectedIndex = -1;
            chkEditActive.Checked = true;
            txtEditNewPassword.Clear();
            txtEditConfirmPassword.Clear();
            lblEditError.Visible = false;
            lblSelfNote.Visible = false;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void ShowError(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowEditError(string msg)
        {
            lblEditError.Text = "❌  " + msg;
            lblEditError.Visible = true;
        }

        private static bool IsValidEmail(string email)
        {
            try { return new System.Net.Mail.MailAddress(email).Address == email; }
            catch { return false; }
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}