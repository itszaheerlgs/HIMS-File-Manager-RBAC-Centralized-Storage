using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace UPLOADER
{
    /// <summary>
    /// SuperAdmin / Auditor view of hims_audit_log.
    /// Opened via FileManagerForm.btnAuditLog_Click.
    /// </summary>
    public partial class AuditLogForm : Form
    {
        private readonly AdminUser _currentUser;

        public AuditLogForm(AdminUser currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
        }

        // ── Load ──────────────────────────────────────────────────────────────
        private void AuditLogForm_Load(object sender, EventArgs e)
        {
            // Populate module and action filters with distinct values from DB
            PopulateFilter(cmbModule, "SELECT DISTINCT module FROM hims_audit_log ORDER BY module");
            PopulateFilter(cmbAction, "SELECT DISTINCT action FROM hims_audit_log ORDER BY action");
            PopulateFilter(cmbActor, "SELECT DISTINCT actor_name FROM hims_audit_log ORDER BY actor_name");

            // Default date range: last 30 days
            dtpFrom.Value = DateTime.Today.AddDays(-30);
            dtpTo.Value = DateTime.Today.AddDays(1).AddSeconds(-1);

            LoadLogs();
        }

        private void PopulateFilter(ComboBox cmb, string sql)
        {
            cmb.Items.Clear();
            cmb.Items.Add("(All)");
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(sql, conn);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                    cmb.Items.Add(r.GetString(0));
            }
            catch { /* silently skip */ }
            cmb.SelectedIndex = 0;
        }

        // ── Query ─────────────────────────────────────────────────────────────
        private void LoadLogs()
        {
            try
            {
                string module = cmbModule.SelectedItem?.ToString() ?? "(All)";
                string action = cmbAction.SelectedItem?.ToString() ?? "(All)";
                string actor = cmbActor.SelectedItem?.ToString() ?? "(All)";
                string keyword = txtSearch.Text.Trim();
                DateTime fromDt = dtpFrom.Value.Date;
                DateTime toDt = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);

                var sb = new StringBuilder(@"
                    SELECT TOP (2000) id, performed_at, actor_name, actor_role,
                           module, action, target_name, target_id, detail
                    FROM   hims_audit_log
                    WHERE  performed_at BETWEEN @From AND @To");

                if (module != "(All)") sb.Append(" AND module     = @Module");
                if (action != "(All)") sb.Append(" AND action     = @Action");
                if (actor != "(All)") sb.Append(" AND actor_name = @Actor");
                if (!string.IsNullOrWhiteSpace(keyword))
                    sb.Append(" AND (actor_name LIKE @Kw OR target_name LIKE @Kw OR detail LIKE @Kw OR action LIKE @Kw)");

                sb.Append(" ORDER BY performed_at DESC");

                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(sb.ToString(), conn);
                cmd.Parameters.AddWithValue("@From", fromDt);
                cmd.Parameters.AddWithValue("@To", toDt);
                if (module != "(All)") cmd.Parameters.AddWithValue("@Module", module);
                if (action != "(All)") cmd.Parameters.AddWithValue("@Action", action);
                if (actor != "(All)") cmd.Parameters.AddWithValue("@Actor", actor);
                if (!string.IsNullOrWhiteSpace(keyword))
                    cmd.Parameters.AddWithValue("@Kw", $"%{keyword}%");

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                dgv.DataSource = dt;
                lblCount.Text = $"{dt.Rows.Count} record(s)";

                // Color-code rows by severity / action category
                ApplyRowColors();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load audit log: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Row coloring ──────────────────────────────────────────────────────
        private static readonly Dictionary<string, Color> ActionColors = new()
        {
            { "DELETE",           Color.FromArgb(255, 230, 230) },
            { "DELETE_SUGGESTION",Color.FromArgb(255, 230, 230) },
            { "DELETE_USER",      Color.FromArgb(255, 200, 200) },
            { "LOCK",             Color.FromArgb(255, 245, 210) },
            { "UNLOCK",           Color.FromArgb(230, 255, 235) },
            { "SESSION_EXPIRED",  Color.FromArgb(255, 240, 200) },
            { "LOGOUT",           Color.FromArgb(240, 240, 255) },
            { "LOGIN",            Color.FromArgb(230, 250, 230) },
            { "UPDATE_PASSWORD",  Color.FromArgb(255, 245, 210) },
            { "ADD_USER",         Color.FromArgb(220, 240, 255) },
        };

        private void ApplyRowColors()
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                string act = row.Cells["action"]?.Value?.ToString() ?? "";
                if (ActionColors.TryGetValue(act, out Color c))
                    row.DefaultCellStyle.BackColor = c;
            }
        }

        // ── Export to CSV ──────────────────────────────────────────────────────
        private void btnExport_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "CSV File|*.csv",
                FileName = $"HIMS_AuditLog_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                var sb = new StringBuilder();
                // Header
                var headers = dgv.Columns.Cast<DataGridViewColumn>()
                    .Select(c => $"\"{c.HeaderText}\"");
                sb.AppendLine(string.Join(",", headers));

                // Rows
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    var cells = row.Cells.Cast<DataGridViewCell>()
                        .Select(c => $"\"{c.Value?.ToString()?.Replace("\"", "\"\"") ?? ""}\"");
                    sb.AppendLine(string.Join(",", cells));
                }

                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Exported successfully.", "Done",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Clear All Logs (SuperAdmin only, with double-confirm) ──────────────
        private void btnClearAll_Click(object sender, EventArgs e)
        {
            if (_currentUser.Role != "SuperAdmin")
            {
                MessageBox.Show("Only SuperAdmin can clear audit logs.", "Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var r1 = MessageBox.Show(
                "This will permanently delete ALL audit log entries.\nAre you sure?",
                "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r1 != DialogResult.Yes) return;

            var r2 = MessageBox.Show(
                "FINAL CONFIRMATION — This action cannot be undone.\nProceed?",
                "Are you absolutely sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Stop);
            if (r2 != DialogResult.Yes) return;

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand("DELETE FROM hims_audit_log", conn);
                int rows = cmd.ExecuteNonQuery();

                // Log the clear itself as a new entry
                AuditLogger.Log(_currentUser,
                    AuditLogger.ModAuth, "CLEAR_AUDIT_LOG",
                    detail: $"Cleared {rows} audit log entries");

                MessageBox.Show($"Cleared {rows} entries.", "Done",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadLogs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Clear failed: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Controls ──────────────────────────────────────────────────────────
        private void btnSearch_Click(object sender, EventArgs e) => LoadLogs();
        private void btnRefresh_Click(object sender, EventArgs e) => LoadLogs();
        private void btnClose_Click(object sender, EventArgs e) => Close();

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; LoadLogs(); }
        }
    }
}