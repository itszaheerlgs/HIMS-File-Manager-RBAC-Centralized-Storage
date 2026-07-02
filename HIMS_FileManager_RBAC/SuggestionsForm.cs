using Microsoft.Data.SqlClient;

namespace UPLOADER
{
    public partial class SuggestionsForm : Form
    {
        private readonly AdminUser _currentUser;
        private readonly bool _isSA;
        private int? _selectedId = null;

        // ── Constructor ───────────────────────────────────────────────────────
        public SuggestionsForm(AdminUser currentUser)
        {
            _currentUser = currentUser;
            _isSA = _currentUser.Role == "SuperAdmin";
            InitializeComponent();
            BuildReplyPanelLayout();
        }

        // ── Dynamic layout for the right-hand reply panel ───────────────────────
        // This logic is intentionally kept OUT of InitializeComponent() because the
        // WinForms designer's code parser cannot handle local variables or
        // arithmetic inside that method (it caused "the designer cannot process
        // the code" errors). All Location calculations live here instead, and
        // this method is called once from the constructor, right after
        // InitializeComponent().
        private void BuildReplyPanelLayout()
        {
            int y = 8;

            lblPanelTitle.Location = new Point(10, y); y += 24;

            lblComposeTitle.Location = new Point(10, y); y += 22;
            lblNewSuggestion.Location = new Point(10, y); y += 18;
            txtNewSuggestion.Location = new Point(10, y); y += 98;
            lblComposeError.Location = new Point(10, y); y += 18;
            btnSendSuggestion.Location = new Point(10, y); y += 40;

            lblFrom.Location = new Point(10, y);
            lblFromValue.Location = new Point(55, y); y += 20;

            lblDate.Location = new Point(10, y);
            lblDateValue.Location = new Point(55, y); y += 24;

            lblSuggestion.Location = new Point(10, y); y += 18;
            txtSuggestionView.Location = new Point(10, y); y += 108;

            lblReply.Location = new Point(10, y); y += 18;
            txtReply.Location = new Point(10, y); y += 118;

            lblError.Location = new Point(10, y); y += 20;

            btnSendReply.Location = new Point(10, y); y += 36;

            btnClearReply.Location = new Point(10, y);
            btnDelete.Location = new Point(162, y); y += 34;

            btnClose.Location = new Point(10, y);
        }

        // ── Load ──────────────────────────────────────────────────────────────
        private void SuggestionsForm_Load(object sender, EventArgs e)
        {
            ApplyRoleView();
            SetupGrid();
            LoadSuggestions();
            ClearReplyPanel();
        }

        // ── Show admin controls vs. the "submit a suggestion" view ────────────
        private void ApplyRoleView()
        {
            if (_isSA)
            {
                // Unchanged admin experience.
                lblPanelTitle.Text = "Reply";
                lblTitle.Text = "💡  Suggestions";
                chkUnrepliedOnly.Visible = true;
                lblUnreplied.Visible = true;
                lblFrom.Visible = lblFromValue.Visible = true;

                lblComposeTitle.Visible = false;
                lblNewSuggestion.Visible = false;
                txtNewSuggestion.Visible = false;
                lblComposeError.Visible = false;
                btnSendSuggestion.Visible = false;

                lblReply.Text = "Your Reply  (SuperAdmin)";
                txtReply.ReadOnly = false;
                btnSendReply.Visible = true;
                btnClearReply.Visible = true;
                btnDelete.Visible = true;
            }
            else
            {
                // Regular user: can only submit suggestions and view their own + the reply.
                lblPanelTitle.Text = "💡 Suggestion Box";
                lblTitle.Text = "💡  My Suggestions";
                chkUnrepliedOnly.Visible = false;
                lblUnreplied.Visible = false;
                lblFrom.Visible = lblFromValue.Visible = false; // it's always "you"

                lblComposeTitle.Visible = true;
                lblNewSuggestion.Visible = true;
                txtNewSuggestion.Visible = true;
                btnSendSuggestion.Visible = true;

                lblReply.Text = "SuperAdmin's Reply";
                txtReply.ReadOnly = true; // can view but not edit/clear admin's reply
                btnSendReply.Visible = false;
                btnClearReply.Visible = false;
                btnDelete.Visible = false;
            }
        }

        // ── Send a brand-new suggestion (non-SuperAdmin users) ─────────────────
        private void btnSendSuggestion_Click(object sender, EventArgs e)
        {
            lblComposeError.Visible = false;
            string text = txtNewSuggestion.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                lblComposeError.Text = "❌  Please write your suggestion first.";
                lblComposeError.Visible = true;
                return;
            }

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    INSERT INTO hims_suggestions (user_name, suggestion, created_at)
                    VALUES (@user, @suggestion, GETDATE())", conn);

                cmd.Parameters.AddWithValue("@user", _currentUser.FullName);
                cmd.Parameters.AddWithValue("@suggestion", text);
                cmd.ExecuteNonQuery();
                AuditLogger.Log(_currentUser,
    AuditLogger.ModSuggestions, AuditLogger.SendSuggestion,
    targetName: _currentUser.FullName);

                txtNewSuggestion.Clear();
                MessageBox.Show("Your suggestion has been sent. Thank you!",
                    "Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadSuggestions(txtSearch.Text);
            }
            catch (Exception ex)
            {
                lblComposeError.Text = "❌  Failed to send: " + ex.Message;
                lblComposeError.Visible = true;
            }
        }

        // ── Grid Setup ────────────────────────────────────────────────────────
        private void SetupGrid()
        {
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Clear();
            dgv.Columns.AddRange(
                Col("id", "#", 40),
                Col("user_name", "From", 130),
                Col("suggestion", "Suggestion", 320),
                Col("super_message", "SA Reply", 200),
                Col("created_at", "Date", 130)
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

            // Color rows that have no reply yet
            dgv.RowPrePaint += (s, e) => { };
            dgv.CellFormatting += dgv_CellFormatting;
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

        // ── Highlight rows with no reply ──────────────────────────────────────
        private void dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv.Rows[e.RowIndex].Cells["super_message"].Value is DBNull ||
                string.IsNullOrWhiteSpace(dgv.Rows[e.RowIndex]
                    .Cells["super_message"].Value?.ToString()))
            {
                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 220);
            }
            else
            {
                dgv.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            }
        }

        // ── Load / Refresh ────────────────────────────────────────────────────
        private void LoadSuggestions(string filter = "")
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                string sql = @"
                    SELECT id, user_name, suggestion, super_message, created_at
                    FROM   hims_suggestions
                    WHERE  (@filter = ''
                            OR user_name  LIKE @filter
                            OR suggestion LIKE @filter)
                           AND (@isSA = 1 OR user_name = @myName)
                    ORDER  BY created_at DESC";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@filter",
                    string.IsNullOrWhiteSpace(filter) ? "" : $"%{filter.Trim()}%");
                cmd.Parameters.AddWithValue("@isSA", _isSA ? 1 : 0);
                cmd.Parameters.AddWithValue("@myName", _currentUser.FullName);

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new System.Data.DataTable();
                adapter.Fill(dt);

                dgv.DataSource = dt;
                lblCount.Text = $"{dt.Rows.Count} suggestion(s)";

                // Count unreplied
                int unreplied = 0;
                foreach (System.Data.DataRow row in dt.Rows)
                    if (row["super_message"] == DBNull.Value ||
                        string.IsNullOrWhiteSpace(row["super_message"]?.ToString()))
                        unreplied++;

                lblUnreplied.Text = unreplied > 0
                    ? $"⚠️  {unreplied} unreplied"
                    : "✔  All replied";
                lblUnreplied.ForeColor = unreplied > 0 ? Color.DarkOrange : Color.DarkGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load suggestions: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Grid Selection → fill panel ───────────────────────────────────────
        private void dgv_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) { ClearReplyPanel(); return; }

            var row = dgv.CurrentRow;
            _selectedId = Convert.ToInt32(row.Cells["id"].Value);

            lblFromValue.Text = row.Cells["user_name"].Value?.ToString() ?? "—";
            lblDateValue.Text = row.Cells["created_at"].Value?.ToString() ?? "—";
            txtSuggestionView.Text = row.Cells["suggestion"].Value?.ToString() ?? "";
            txtReply.Text = row.Cells["super_message"].Value?.ToString() ?? "";

            lblError.Visible = false;
            btnSendReply.Enabled = true;
            btnDelete.Enabled = true;
            btnClearReply.Enabled = true;
        }

        // ── Send / Update Reply ───────────────────────────────────────────────
        private void btnSendReply_Click(object sender, EventArgs e)
        {
            if (_selectedId == null) return;
            lblError.Visible = false;

            string reply = txtReply.Text.Trim();
            if (string.IsNullOrWhiteSpace(reply))
            {
                ShowError("Reply cannot be empty.");
                return;
            }

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    UPDATE hims_suggestions
                    SET    super_message = @reply, replied_at = GETDATE()
                    WHERE  id = @id", conn);

                cmd.Parameters.AddWithValue("@reply", reply);
                cmd.Parameters.AddWithValue("@id", _selectedId);
                cmd.ExecuteNonQuery();
                AuditLogger.Log(_currentUser,
    AuditLogger.ModSuggestions, AuditLogger.ReplySuggestion,
    targetId: _selectedId.ToString(),
    targetName: lblFromValue.Text);

                MessageBox.Show("Reply sent successfully.",
                    "Replied", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadSuggestions(txtSearch.Text);
            }
            catch (Exception ex)
            {
                ShowError("Failed to send reply: " + ex.Message);
            }
        }

        // ── Clear Reply ───────────────────────────────────────────────────────
        private void btnClearReply_Click(object sender, EventArgs e)
        {
            if (_selectedId == null) return;

            var confirm = MessageBox.Show(
                "Clear the existing reply for this suggestion?",
                "Confirm Clear",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    UPDATE hims_suggestions
                    SET    super_message = NULL, replied_at = NULL
                    WHERE  id = @id", conn);

                cmd.Parameters.AddWithValue("@id", _selectedId);
                cmd.ExecuteNonQuery();

                txtReply.Clear();
                AuditLogger.Log(_currentUser,
    AuditLogger.ModSuggestions, AuditLogger.ClearReply,
    targetId: _selectedId.ToString());
                LoadSuggestions(txtSearch.Text);
            }
            catch (Exception ex)
            {
                ShowError("Failed to clear reply: " + ex.Message);
            }
        }

        // ── Delete ────────────────────────────────────────────────────────────
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedId == null) return;

            string from = lblFromValue.Text;
            var confirm = MessageBox.Show(
                $"Permanently delete suggestion from '{from}'?\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "DELETE FROM hims_suggestions WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", _selectedId);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Suggestion deleted.",
                    "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AuditLogger.Log(_currentUser,
    AuditLogger.ModSuggestions, AuditLogger.DeleteSuggestion,
    targetId: _selectedId.ToString(),
    targetName: from);

                ClearReplyPanel();
                LoadSuggestions(txtSearch.Text);
            }
            catch (Exception ex)
            {
                ShowError("Delete failed: " + ex.Message);
            }
        }

        // ── Filter: Unreplied only ────────────────────────────────────────────
        private void chkUnrepliedOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUnrepliedOnly.Checked)
                LoadUnrepliedOnly();
            else
                LoadSuggestions(txtSearch.Text);
        }

        private void LoadUnrepliedOnly()
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    SELECT id, user_name, suggestion, super_message, created_at
                    FROM   hims_suggestions
                    WHERE  super_message IS NULL OR super_message = ''
                    ORDER  BY created_at DESC", conn);

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new System.Data.DataTable();
                adapter.Fill(dt);
                dgv.DataSource = dt;
                lblCount.Text = $"{dt.Rows.Count} unreplied";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load failed: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Search ────────────────────────────────────────────────────────────
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            chkUnrepliedOnly.Checked = false;
            LoadSuggestions(txtSearch.Text);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            chkUnrepliedOnly.Checked = false;
            LoadSuggestions();
            ClearReplyPanel();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void ClearReplyPanel()
        {
            _selectedId = null;
            lblFromValue.Text = "—";
            lblDateValue.Text = "—";
            txtSuggestionView.Clear();
            txtReply.Clear();
            lblError.Visible = false;
            btnSendReply.Enabled = false;
            btnDelete.Enabled = false;
            btnClearReply.Enabled = false;
        }

        private void ShowError(string msg)
        {
            lblError.Text = "❌  " + msg;
            lblError.Visible = true;
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
    }
}