using Microsoft.Data.SqlClient;
using System.Data;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace UPLOADER
{
    public partial class RecycleBinForm : Form
    {
        private readonly AdminUser _user;

        public RecycleBinForm(AdminUser user)
        {
            _user = user;
            InitializeComponent();
        }

        private void RecycleBinForm_Load(object sender, EventArgs e)
        {
            LoadDeleted();
        }

        // ── Load deleted items ──────────────────────────────────────────────
        private void LoadDeleted()
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    SELECT id, display_name, is_folder, file_type, file_size,
                           deleted_by, deleted_at, parent_id
                    FROM   opd_file_manager
                    WHERE  is_deleted = 1
                    ORDER  BY deleted_at DESC", conn);

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                dgv.Rows.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    bool isFolder = Convert.ToBoolean(row["is_folder"]);
                    string name = row["display_name"].ToString() ?? "";
                    string type = isFolder ? "📁 Folder" : (row["file_type"]?.ToString()?.ToUpper() ?? "");
                    string size = isFolder ? "" : FormatSize(row["file_size"] == DBNull.Value ? 0 : Convert.ToInt64(row["file_size"]));
                    string deletedBy = row["deleted_by"]?.ToString() ?? "";
                    string deletedAt = row["deleted_at"] == DBNull.Value
                        ? "" : Convert.ToDateTime(row["deleted_at"]).ToString("yyyy-MM-dd HH:mm");

                    int rowIdx = dgv.Rows.Add(isFolder ? "📁" : "📄", name, type, size, deletedBy, deletedAt);
                    dgv.Rows[rowIdx].Tag = Convert.ToInt32(row["id"]);
                }

                lblCount.Text = $"{dgv.Rows.Count} item(s) in Recycle Bin";
                btnRestore.Enabled = btnDeletePermanently.Enabled = dgv.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Recycle Bin: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string FormatSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB" };
            double size = bytes;
            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1) { size /= 1024; unit++; }
            return $"{size:0.##} {units[unit]}";
        }

        private List<int> SelectedIds() =>
            dgv.SelectedRows.Cast<DataGridViewRow>()
               .Select(r => r.Tag is int id ? id : -1)
               .Where(id => id != -1)
               .ToList();

        // ── Restore (Optimized High-Speed Asynchronous) ──────────────────────
        private async void btnRestore_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.File_Restore); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            var ids = SelectedIds();
            if (ids.Count == 0)
            {
                MessageBox.Show("Select at least one item to restore.", "Info");
                return;
            }

            if (MessageBox.Show($"Restore {ids.Count} item(s) back to the File Manager?",
                "Confirm Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            SetControlsEnabled(false);
            int totalSelected = ids.Count;
            UpdateProgress(0, totalSelected, "Preparing database restoration map...");

            try
            {
                await Task.Run(() =>
                {
                    using var conn = DbConfig.OpenConnection();
                    var allIdsToRestore = new List<int>();
                    int counter = 0;

                    // 1. Map out target item IDs recursively
                    foreach (int id in ids)
                    {
                        counter++;
                        UpdateProgress(counter, totalSelected, $"Scanning hierarchy data ({counter}/{totalSelected})...");
                        GatherIdsRecursive(id, allIdsToRestore, conn, targetDeletedState: 1);
                    }

                    // 2. Perform bulk high speed restore update 
                    if (allIdsToRestore.Count > 0)
                    {
                        UpdateProgress(allIdsToRestore.Count, allIdsToRestore.Count, $"Restoring {allIdsToRestore.Count} total items...");

                        string idList = string.Join(",", allIdsToRestore);
                        string sql = $"UPDATE opd_file_manager SET is_deleted = 0, deleted_by = NULL, deleted_at = NULL WHERE id IN ({idList})";

                        using var cmd = new SqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();

                        AuditLogger.Log(_user, AuditLogger.ModFileManager, "RESTORE",
                            detail: $"Restored {ids.Count} item(s) from Recycle Bin ({allIdsToRestore.Count} items impacted)");
                    }
                });

                ResetProgress("Restoration finished successfully.");
                LoadDeleted();
            }
            catch (Exception ex)
            {
                ResetProgress("An error occurred during restoration.");
                MessageBox.Show("Restore failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        // ── Permanent Delete (Optimized High-Speed Asynchronous) ──────────────
        private async void btnDeletePermanently_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.File_PermanentDelete); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            var ids = SelectedIds();
            if (ids.Count == 0)
            {
                MessageBox.Show("Select at least one item to delete.", "Info");
                return;
            }

            if (MessageBox.Show(
                $"PERMANENTLY delete {ids.Count} item(s)? This cannot be undone.",
                "Confirm Permanent Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            SetControlsEnabled(false);
            int totalSelected = ids.Count;
            UpdateProgress(0, totalSelected, "Assembling permanent removal target table...");

            try
            {
                await Task.Run(() =>
                {
                    using var conn = DbConfig.OpenConnection();
                    var allIdsToHardDelete = new List<int>();
                    int counter = 0;

                    // 1. Map out target item IDs recursively
                    foreach (int id in ids)
                    {
                        counter++;
                        UpdateProgress(counter, totalSelected, $"Analyzing dependencies ({counter}/{totalSelected})...");
                        GatherIdsRecursive(id, allIdsToHardDelete, conn, targetDeletedState: 1);
                    }

                    // 2. Clear rows instantly via Batch Delete
                    if (allIdsToHardDelete.Count > 0)
                    {
                        UpdateProgress(allIdsToHardDelete.Count, allIdsToHardDelete.Count, $"Purging {allIdsToHardDelete.Count} records safely from database...");

                        string idList = string.Join(",", allIdsToHardDelete);

                        // Grab the on-disk paths of every file (not folder) being purged
                        // BEFORE deleting the rows, so we can remove the physical files too —
                        // otherwise they'd sit on disk forever as orphaned space.
                        var pathsToDelete = new List<string>();
                        using (var pathCmd = new SqlCommand(
                            $"SELECT system_path FROM opd_file_manager WHERE id IN ({idList}) AND is_folder = 0 AND system_path IS NOT NULL", conn))
                        using (var r = pathCmd.ExecuteReader())
                        {
                            while (r.Read()) pathsToDelete.Add(r.GetString(0));
                        }

                        string sql = $"DELETE FROM opd_file_manager WHERE id IN ({idList})";

                        using var cmd = new SqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();

                        // Only remove the physical files once the DB rows are confirmed gone,
                        // so a failed DB delete never leaves dangling references to missing files.
                        foreach (string p in pathsToDelete) FileStorage.TryDelete(p);

                        AuditLogger.Log(_user, AuditLogger.ModFileManager, "PERMANENT_DELETE",
                            detail: $"Permanently deleted {ids.Count} item(s) from Recycle Bin ({allIdsToHardDelete.Count} items wiped)");
                    }
                });

                ResetProgress("Selected item(s) permanently removed.");
                LoadDeleted();
            }
            catch (Exception ex)
            {
                ResetProgress("An error occurred during permanent deletion.");
                MessageBox.Show("Permanent delete failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        // ── Empty Recycle Bin (Optimized High-Speed Asynchronous) ─────────────
        private async void btnEmptyBin_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.File_PermanentDelete); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            if (dgv.Rows.Count == 0) return;

            if (MessageBox.Show(
                $"PERMANENTLY delete all {dgv.Rows.Count} item(s) in the Recycle Bin? This cannot be undone.",
                "Empty Recycle Bin", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            SetControlsEnabled(false);
            var topLevelRows = dgv.Rows.Cast<DataGridViewRow>().ToList();
            int totalSelected = topLevelRows.Count;
            UpdateProgress(0, totalSelected, "Scanning all items inside Recycle Bin...");

            try
            {
                await Task.Run(() =>
                {
                    using var conn = DbConfig.OpenConnection();
                    var allIdsToHardDelete = new List<int>();
                    int counter = 0;

                    // 1. Build a map of all nested structures
                    foreach (var row in topLevelRows)
                    {
                        counter++;
                        int id = (int)row.Tag!;
                        UpdateProgress(counter, totalSelected, $"Mapping item trees ({counter}/{totalSelected})...");
                        GatherIdsRecursive(id, allIdsToHardDelete, conn, targetDeletedState: 1);
                    }

                    // 2. Drop all entities instantly via Batch Delete
                    if (allIdsToHardDelete.Count > 0)
                    {
                        UpdateProgress(allIdsToHardDelete.Count, allIdsToHardDelete.Count, $"Purging all {allIdsToHardDelete.Count} items instantly...");

                        string idList = string.Join(",", allIdsToHardDelete);

                        // Same as permanent-delete: capture file paths before the rows are gone.
                        var pathsToDelete = new List<string>();
                        using (var pathCmd = new SqlCommand(
                            $"SELECT system_path FROM opd_file_manager WHERE id IN ({idList}) AND is_folder = 0 AND system_path IS NOT NULL", conn))
                        using (var r = pathCmd.ExecuteReader())
                        {
                            while (r.Read()) pathsToDelete.Add(r.GetString(0));
                        }

                        string sql = $"DELETE FROM opd_file_manager WHERE id IN ({idList})";

                        using var cmd = new SqlCommand(sql, conn);
                        cmd.ExecuteNonQuery();

                        foreach (string p in pathsToDelete) FileStorage.TryDelete(p);

                        AuditLogger.Log(_user, AuditLogger.ModFileManager, "EMPTY_RECYCLE_BIN",
                            detail: $"Emptied Recycle Bin completely ({allIdsToHardDelete.Count} total items wiped)");
                    }
                });

                ResetProgress("Recycle Bin emptied successfully.");
                LoadDeleted();
            }
            catch (Exception ex)
            {
                ResetProgress("An error occurred while emptying the Recycle Bin.");
                MessageBox.Show("Failed to empty Recycle Bin: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        // ── High Speed Dependency Data Collector ──────────────────────────────
        private void GatherIdsRecursive(int id, List<int> allIds, SqlConnection conn, int targetDeletedState)
        {
            allIds.Add(id);
            var children = new List<int>();

            using (var cmd = new SqlCommand("SELECT id FROM opd_file_manager WHERE parent_id = @id AND is_deleted = @state", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@state", targetDeletedState);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    children.Add(reader.GetInt32(0));
                }
            }

            foreach (int childId in children)
            {
                GatherIdsRecursive(childId, allIds, conn, targetDeletedState);
            }
        }

        // ── Progress UI Utilities ─────────────────────────────────────────────
        private void UpdateProgress(int current, int total, string statusText)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateProgress(current, total, statusText));
                return;
            }

            if (total <= 0) total = 1;
            int percentage = (int)(((double)current / total) * 100);
            if (percentage > 100) percentage = 100;

            progressBarRB.Maximum = total;
            progressBarRB.Value = current;
            lblPercent.Text = $"{percentage}%";
            lblStatus.Text = statusText;
        }

        private void ResetProgress(string finalMessage)
        {
            if (InvokeRequired)
            {
                Invoke(() => ResetProgress(finalMessage));
                return;
            }

            progressBarRB.Value = 0;
            lblPercent.Text = "0%";
            lblStatus.Text = finalMessage;
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetControlsEnabled(enabled));
                return;
            }

            btnRestore.Enabled = enabled;
            btnDeletePermanently.Enabled = enabled;
            btnEmptyBin.Enabled = enabled;
            btnRefresh.Enabled = enabled;
            btnClose.Enabled = enabled;
            dgv.Enabled = enabled;
        }

        private void btnRefresh_Click(object sender, EventArgs e) => LoadDeleted();

        private void btnClose_Click(object sender, EventArgs e) => Close();

        private void dgv_SelectionChanged(object sender, EventArgs e)
        {
            bool has = dgv.SelectedRows.Count > 0;
            btnRestore.Enabled = has;
            btnDeletePermanently.Enabled = has;
        }

        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}