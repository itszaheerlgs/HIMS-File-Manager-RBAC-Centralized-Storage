using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace UPLOADER
{
    public partial class FileManagerForm : Form
    {
        private readonly AdminUser _user;
        private int? _currentFolderId = null;
        private readonly Stack<(int? Id, string Name)> _breadcrumb = new();

        // ── Bulk upload cancellation ─────────────────────────────────────────
        private CancellationTokenSource? _uploadCts;

        // For print preview
        private List<DataGridViewRow> _printRows = new();

        // ── Inactivity Logout ─────────────────────────────────────────────────
        private System.Windows.Forms.Timer _inactivityTimer = new();
        // private const int InactivityLimitMs = 5_000; // 5 sec for testing → change to 300_000 for 5 min
        //private const int InactivityLimitMs = 600_000; // 10 minutes
        private const int InactivityLimitMs = 1_800_000; // 30 minutes


        private void InitInactivityTimer()
        {
            _inactivityTimer.Interval = InactivityLimitMs;
            _inactivityTimer.Tick += OnInactivityTimeout;
            _inactivityTimer.Start();

            this.MouseMove += ResetInactivityTimer;
            this.KeyPress += ResetInactivityTimer;
            this.Click += ResetInactivityTimer;
            dgv.MouseMove += ResetInactivityTimer;
            dgv.Click += ResetInactivityTimer;
            dgv.KeyPress += ResetInactivityTimer;
        }

        private void ResetInactivityTimer(object? sender, EventArgs e)
        {
            _inactivityTimer.Stop();
            _inactivityTimer.Start();
        }

        private void OnInactivityTimeout(object? sender, EventArgs e)
        {
            _inactivityTimer.Stop();
            if (InvokeRequired) { Invoke(OnInactivityTimeout, sender, e); return; }

            MessageBox.Show(
                "You have been logged out due to inactivity.",
                "Session Expired",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            AuditLogger.Log(_user, AuditLogger.ModAuth, AuditLogger.SessionExpired);
            DoLogout();
        }
        // ─────────────────────────────────────────────────────────────────────FileManagerForm_Load

        public FileManagerForm(AdminUser user)
        {
            _user = user;
            InitializeComponent();
            Text = $"HIMS File Manager — {DbConfig.Current.ServerIP}:{DbConfig.Current.MySqlPort}/{DbConfig.Current.Database}";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _notifier?.Stop();
            base.OnFormClosed(e);
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private void FileManagerForm_Load(object sender, EventArgs e)
        {
            UpdateUserHeader();

            // Button visibility is now driven by the same permission matrix that
            // Require() enforces on click — this list can no longer drift out of
            // sync with what's actually allowed to execute (see PermissionService.cs).
            btnNewFolder.Visible = PermissionService.Can(_user, Permission.File_NewFolder);
            btnUpload.Visible = PermissionService.Can(_user, Permission.File_Upload);
            btnBulkUpload.Visible = PermissionService.Can(_user, Permission.File_Upload);
            btnRename.Visible = PermissionService.Can(_user, Permission.File_Rename);
            btnDelete.Visible = PermissionService.Can(_user, Permission.File_Delete);
            btnLock.Visible = PermissionService.Can(_user, Permission.File_Lock);
            btnUserList.Visible = PermissionService.Can(_user, Permission.User_View);
            btnManageUsers.Visible = PermissionService.Can(_user, Permission.User_Create);
            btnSettings.Visible = PermissionService.Can(_user, Permission.Settings_View);
            btnRecycleBin2.Visible = PermissionService.Can(_user, Permission.File_Delete);

            btnSuggestions.Visible = true;
            btnAuditLog.Visible = PermissionService.Can(_user, Permission.AuditLog_View);
            btnChat.Visible = true;
            btnMentionChat.Visible = true;
            btnDashboard.Visible = PermissionService.Can(_user, Permission.Dashboard_View);


            InitInactivityTimer();
            InitNotifications();
            RefreshGrid();
        }

        // ── Header (profile photo + name / username / role) ─────────────────────

        private void UpdateUserHeader()
        {
            lblFullName.Text = _user.FullName;
            lblUsername.Text = $"@{_user.Username}";
            lblRole.Text = FormatRoleLabel(_user.Role);
            lblRole.ForeColor = GetRoleColor(_user.Role);

            LoadProfilePicture();
        }

        // Maps each role to a distinct accent color. Roughly ordered by rank,
        // from most privileged (warm/red) down to least (cool/blue-green).
        private static Color GetRoleColor(string role) => role switch
        {
            "SuperAdmin" => Color.FromArgb(255, 90, 90),     // red
            "DataManager" => Color.FromArgb(255, 183, 77),   // amber
            "Auditor" => Color.FromArgb(186, 142, 255),      // purple
            "RecordControllScan" => Color.FromArgb(255, 138, 200), // pink
            "CertificationStaff" => Color.FromArgb(102, 224, 178), // teal/green
            "StatisticianStaff" => Color.FromArgb(255, 159, 102),  // orange
            "OPDStaff" => Color.FromArgb(108, 183, 255),     // blue
            _ => Color.LightGray
        };

        private static string FormatRoleLabel(string role) => role switch
        {
            "SuperAdmin" => "Super Admin",
            "DataManager" => "Data Manager",
            "OPDStaff" => "OPD Staff",
            "CertificationStaff" => "Certification Staff",
            "RecordControllScan" => "Record Control / Scan",
            "StatisticianStaff" => "Statistician Staff",
            "Auditor" => "Auditor",
            _ => role
        };

        private void LoadProfilePicture()
        {
            Image img;
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "SELECT profile_pic_data FROM admins WHERE admin_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", _user.Id);
                using var r = cmd.ExecuteReader();

                if (r.Read() && !r.IsDBNull(0))
                {
                    byte[] blob = (byte[])r["profile_pic_data"];
                    using var ms = new MemoryStream(blob);
                    using var temp = Image.FromStream(ms);
                    int safeW = picProfile.Width > 0 ? picProfile.Width : 48;
                    int safeH = picProfile.Height > 0 ? picProfile.Height : 48;
                    img = new Bitmap(temp, safeW, safeH);
                }
                else
                {
                    img = CreateInitialsAvatar(_user.FullName, _user.Role, picProfile.Width);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoadProfilePicture failed: " + ex);
                img = CreateInitialsAvatar(_user.FullName, _user.Role, picProfile.Width);
            }

            picProfile.Image?.Dispose();
            picProfile.Image = img;
            MakeCircular(picProfile);
        }

        // Generates a circular "initials" avatar (e.g. "JD") tinted with the
        // user's role color — used whenever no profile photo is on file.
        private static Image CreateInitialsAvatar(string fullName, string role, int size)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var bg = new SolidBrush(GetRoleColor(role));
            g.FillEllipse(bg, 0, 0, size, size);

            string initials = GetInitials(fullName);
            using var font = new Font("Segoe UI", size / 2.6f, FontStyle.Bold);
            var measured = g.MeasureString(initials, font);
            g.DrawString(initials, font, Brushes.White,
                (size - measured.Width) / 2, (size - measured.Height) / 2);

            return bmp;
        }

        private static string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }

        // Clips a square PictureBox into a circle.
        private static void MakeCircular(PictureBox pb)
        {
            var path = new GraphicsPath();
            path.AddEllipse(0, 0, pb.Width, pb.Height);
            pb.Region?.Dispose();
            pb.Region = new Region(path);
        }

        private void picProfile_Paint(object sender, PaintEventArgs e)
        {
            // Thin border ring around the circular photo so it reads
            // clearly against the dark header bar.
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.FromArgb(90, 110, 150), 1.5f);
            e.Graphics.DrawEllipse(pen, 0.75f, 0.75f, picProfile.Width - 1.5f, picProfile.Height - 1.5f);
        }

        private void picProfile_Click(object sender, EventArgs e) => btnProfile_Click(sender, e);

        // ── Logout (shared by ALL logout paths) ───────────────────────────────
        //
        //  ROOT CAUSE OF THE ORIGINAL BUG:
        //  The old btnLogout_Click called next.Show() then Close(), but if the
        //  LoginForm was cancelled it still fell through to Application.Exit().
        //  Worse, the two duplicate logout handlers had slightly different code.
        //
        //  FIX: one DoLogout() method used by every logout path.
        //  Key rule: Show() the NEW form BEFORE Close()-ing THIS one so the
        //  WinForms message loop never sees zero open forms (which auto-exits).

        private void DoLogout()
        {
            _inactivityTimer.Stop();
            _inactivityTimer.Dispose();
            DbConfig.ResetToBootstrap();

            while (true)
            {
                var login = new LoginForm();
                Hide();

                var result = login.ShowDialog();

                if (result == DialogResult.OK && login.LoggedInUser != null)
                {
                    // Successful login → open new session
                    AuditLogger.Log(_user, AuditLogger.ModAuth, AuditLogger.Logout);
                    var next = new FileManagerForm(login.LoggedInUser);
                    next.Show();   // show new form BEFORE closing this one
                    Close();
                    return;
                }

                // Login was cancelled or X'd — ask if they want to try again
                var retry = MessageBox.Show(
                    "You are not logged in.\nClick Yes to go back to login, or No to exit the application.",
                    "Login Required",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (retry == DialogResult.No)
                {
                    Application.Exit();
                    return;
                }

                // Loop back and show login again
            }
        }

        // ── Data ──────────────────────────────────────────────────────────────

        //private void RefreshGrid(string search = "")
        //{
        //    UpdateBreadcrumb();
        //    dgv.Rows.Clear();

        //    string sql;
        //    if (!string.IsNullOrWhiteSpace(search))
        //    {
        //        sql = @"
        //            SELECT id, is_folder, display_name, file_type, file_size,
        //                   is_locked, uploaded_by, uploaded_at, parent_id
        //            FROM   opd_file_manager
        //            WHERE  display_name LIKE @Search
        //                   AND is_deleted = 0
        //            ORDER  BY is_folder DESC, display_name ASC";
        //    }
        //    else
        //    {
        //        sql = @"
        //            SELECT id, is_folder, display_name, file_type, file_size,
        //                   is_locked, uploaded_by, uploaded_at, parent_id
        //            FROM   opd_file_manager
        //            WHERE  (parent_id = @ParentId
        //                    OR (@ParentId IS NULL AND (parent_id IS NULL OR parent_id = 0)))
        //                   AND is_deleted = 0
        //            ORDER  BY is_folder DESC, display_name ASC";
        //    }

        //    try
        //    {
        //        using var conn = DbConfig.OpenConnection();
        //        using var cmd = new SqlCommand(sql, conn);

        //        if (!string.IsNullOrWhiteSpace(search))
        //            cmd.Parameters.AddWithValue("@Search", $"%{search.Trim()}%");
        //        else
        //            SetParentId(cmd, _currentFolderId);

        //        using var r = cmd.ExecuteReader();
        //        while (r.Read())
        //        {
        //            bool isFolder = r.GetBoolean("is_folder");
        //            bool isLocked = r.GetBoolean("is_locked");
        //            string name = r.GetString("display_name");
        //            string rawType = NormalizeFileType(r["file_type"] as string ?? "");
        //            string type = isFolder ? "📁 Folder" : GetFileIcon(rawType) + " " + rawType.ToUpper();
        //            string size = isFolder ? "" : FormatSize(r.GetInt64("file_size"));
        //            string date = r.GetDateTime("uploaded_at").ToString("yyyy-MM-dd HH:mm");
        //            string uploader = r["uploaded_by"] as string ?? "";
        //            string locked = isLocked ? "🔒" : "";
        //            int id = r.GetInt32("id");

        //            string countStr = "";
        //            if (isFolder)
        //            {
        //                var (subFolders, subFiles) = GetChildCounts(id);
        //                countStr = $"{subFolders}📁 {subFiles}📄";
        //            }

        //            int rowIdx = dgv.Rows.Add(
        //                isFolder ? "📁" : GetFileIcon(rawType),
        //                name, type, size, countStr, date, uploader, locked);

        //            dgv.Rows[rowIdx].Tag = new FileEntry(id, isFolder, name,
        //                rawType, isLocked);

        //            if (isFolder)
        //                dgv.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(20, 80, 160);
        //            if (isLocked)
        //                dgv.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.Firebrick;
        //        }

        //        UpdateStatusBar();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("DB Error: " + ex.Message, "Error",
        //            MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}
        //private void RefreshGrid(string search = "")
        //{
        //    UpdateBreadcrumb();

        //    // 1. Suspend layout to stop the DataGridView from flickering/stuttering while loading rows
        //    dgv.SuspendLayout();
        //    dgv.Rows.Clear();

        //    string sql;
        //    if (!string.IsNullOrWhiteSpace(search))
        //    {
        //        sql = @"
        //    SELECT id, is_folder, display_name, file_type, file_size,
        //           is_locked, uploaded_by, uploaded_at, parent_id
        //    FROM   opd_file_manager
        //    WHERE  display_name LIKE @Search
        //           AND is_deleted = 0
        //    ORDER  BY is_folder DESC, display_name ASC";
        //    }
        //    else
        //    {
        //        sql = @"
        //    SELECT id, is_folder, display_name, file_type, file_size,
        //           is_locked, uploaded_by, uploaded_at, parent_id
        //    FROM   opd_file_manager
        //    WHERE  (parent_id = @ParentId
        //            OR (@ParentId IS NULL AND (parent_id IS NULL OR parent_id = 0)))
        //           AND is_deleted = 0
        //    ORDER  BY is_folder DESC, display_name ASC";
        //    }

        //    try
        //    {
        //        using var conn = DbConfig.OpenConnection();

        //        // 2. PRE-FETCH FOLDER COUNTS: Get counts for all folders in ONE query
        //        var folderCounts = new Dictionary<int, (int subFolders, int subFiles)>();
        //        string countSql = @"
        //    SELECT parent_id,
        //           SUM(CASE WHEN is_folder = 1 THEN 1 ELSE 0 END) as subFolders,
        //           SUM(CASE WHEN is_folder = 0 THEN 1 ELSE 0 END) as subFiles
        //    FROM   opd_file_manager
        //    WHERE  is_deleted = 0 AND parent_id IS NOT NULL AND parent_id > 0
        //    GROUP  BY parent_id";

        //        using (var countCmd = new SqlCommand(countSql, conn))
        //        using (var countReader = countCmd.ExecuteReader())
        //        {
        //            while (countReader.Read())
        //            {
        //                int pId = countReader.GetInt32("parent_id");
        //                int subFolders = countReader.IsDBNull(countReader.GetOrdinal("subFolders")) ? 0 : countReader.GetInt32("subFolders");
        //                int subFiles = countReader.IsDBNull(countReader.GetOrdinal("subFiles")) ? 0 : countReader.GetInt32("subFiles");
        //                folderCounts[pId] = (subFolders, subFiles);
        //            }
        //        }

        //        // 3. RUN MAIN QUERY
        //        using var cmd = new SqlCommand(sql, conn);
        //        if (!string.IsNullOrWhiteSpace(search))
        //            cmd.Parameters.AddWithValue("@Search", $"%{search.Trim()}%");
        //        else
        //            SetParentId(cmd, _currentFolderId);

        //        // Pre-cache column ordinals for speed
        //        using var r = cmd.ExecuteReader();
        //        int idxId = r.GetOrdinal("id");
        //        int idxIsFolder = r.GetOrdinal("is_folder");
        //        int idxDisplayName = r.GetOrdinal("display_name");
        //        int idxFileType = r.GetOrdinal("file_type");
        //        int idxFileSize = r.GetOrdinal("file_size");
        //        int idxIsLocked = r.GetOrdinal("is_locked");
        //        int idxUploadedBy = r.GetOrdinal("uploaded_by");
        //        int idxUploadedAt = r.GetOrdinal("uploaded_at");

        //        Color folderColor = Color.FromArgb(20, 80, 160);
        //        Color lockedColor = Color.Firebrick;

        //        // Use a list to buffer rows instead of inserting into the dgv one-by-one
        //        var rowsToAdd = new List<DataGridViewRow>();

        //        while (r.Read())
        //        {
        //            bool isFolder = r.GetBoolean(idxIsFolder);
        //            bool isLocked = r.GetBoolean(idxIsLocked);
        //            string name = r.GetString(idxDisplayName);
        //            string rawType = NormalizeFileType(r.IsDBNull(idxFileType) ? "" : r.GetString(idxFileType));
        //            string type = isFolder ? "📁 Folder" : GetFileIcon(rawType) + " " + rawType.ToUpper();
        //            string size = isFolder ? "" : FormatSize(r.GetInt64(idxFileSize));
        //            string date = r.GetDateTime(idxUploadedAt).ToString("yyyy-MM-dd HH:mm");
        //            string uploader = r.IsDBNull(idxUploadedBy) ? "" : r.GetString(idxUploadedBy);
        //            string locked = isLocked ? "🔒" : "";
        //            int id = r.GetInt32(idxId);

        //            string countStr = "";
        //            if (isFolder)
        //            {
        //                // Instant memory lookup instead of hitting the database!
        //                if (folderCounts.TryGetValue(id, out var counts))
        //                {
        //                    countStr = $"{counts.subFolders}📁 {counts.subFiles}📄";
        //                }
        //                else
        //                {
        //                    countStr = "0📁 0📄";
        //                }
        //            }

        //            var row = new DataGridViewRow();
        //            row.CreateCells(dgv, isFolder ? "📁" : GetFileIcon(rawType), name, type, size, countStr, date, uploader, locked);
        //            row.Tag = new FileEntry(id, isFolder, name, rawType, isLocked);

        //            if (isFolder)
        //                row.DefaultCellStyle.ForeColor = folderColor;
        //            else if (isLocked)
        //                row.DefaultCellStyle.ForeColor = lockedColor;

        //            rowsToAdd.Add(row);
        //        }

        //        // 4. Batch add all items simultaneously
        //        if (rowsToAdd.Count > 0)
        //        {
        //            dgv.Rows.AddRange(rowsToAdd.ToArray());
        //        }

        //        UpdateStatusBar();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("DB Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        // 5. Always resume layout drawing
        //        dgv.ResumeLayout();
        //    }
        //}

        // ── Optimized High-Speed Refresh Grid ─────────────────────────────────
        private void RefreshGrid(string search = "")
        {
            UpdateBreadcrumb();

            // Cache current auto-size modes to prevent internal layout calculation loops
            var oldHeadersMode = dgv.RowHeadersWidthSizeMode;
            dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;

            // 1. Suspend layout drawing routines to eliminate flickering entirely
            dgv.SuspendLayout();
            dgv.Rows.Clear();

            // "Inside this folder" search: when checked (the default), a search
            // is scoped to the folder currently open — the current folder's
            // direct children plus every descendant beneath it — instead of
            // matching display_name across the entire file store. Uncheck the
            // box to fall back to a whole-system search.
            bool scopedToFolder = chkSearchInFolder == null || chkSearchInFolder.Checked;

            string sql;
            if (!string.IsNullOrWhiteSpace(search) && scopedToFolder)
            {
                sql = @"
                    ;WITH FolderTree AS (
                        SELECT id FROM opd_file_manager
                        WHERE  is_deleted = 0
                               AND (parent_id = @ParentId
                                    OR (@ParentId IS NULL AND (parent_id IS NULL OR parent_id = 0)))
                        UNION ALL
                        SELECT f.id
                        FROM   opd_file_manager f
                        INNER JOIN FolderTree ft ON f.parent_id = ft.id
                        WHERE  f.is_deleted = 0
                    )
                    SELECT m.id, m.is_folder, m.display_name, m.file_type, m.file_size,
                           m.is_locked, m.uploaded_by, m.uploaded_at, m.parent_id
                    FROM   opd_file_manager m
                    WHERE  m.id IN (SELECT id FROM FolderTree)
                           AND m.display_name LIKE @Search
                           AND m.is_deleted = 0
                    ORDER  BY m.is_folder DESC, m.display_name ASC
                    OPTION (MAXRECURSION 200)";
            }
            else if (!string.IsNullOrWhiteSpace(search))
            {
                sql = @"
                    SELECT id, is_folder, display_name, file_type, file_size,
                           is_locked, uploaded_by, uploaded_at, parent_id
                    FROM   opd_file_manager
                    WHERE  display_name LIKE @Search
                           AND is_deleted = 0
                    ORDER  BY is_folder DESC, display_name ASC";
            }
            else
            {
                sql = @"
                    SELECT id, is_folder, display_name, file_type, file_size,
                           is_locked, uploaded_by, uploaded_at, parent_id
                    FROM   opd_file_manager
                    WHERE  (parent_id = @ParentId
                            OR (@ParentId IS NULL AND (parent_id IS NULL OR parent_id = 0)))
                           AND is_deleted = 0
                    ORDER  BY is_folder DESC, display_name ASC";
            }

            try
            {
                using var conn = DbConfig.OpenConnection();

                // 2. PRE-FETCH FOLDER COUNTS: Get counts for all folders in ONE query
                var folderCounts = new Dictionary<int, (int subFolders, int subFiles)>();
                string countSql = @"
                    SELECT parent_id,
                           SUM(CASE WHEN is_folder = 1 THEN 1 ELSE 0 END) as subFolders,
                           SUM(CASE WHEN is_folder = 0 THEN 1 ELSE 0 END) as subFiles
                    FROM   opd_file_manager
                    WHERE  is_deleted = 0 AND parent_id IS NOT NULL AND parent_id > 0
                    GROUP  BY parent_id";

                using (var countCmd = new SqlCommand(countSql, conn))
                using (var countReader = countCmd.ExecuteReader())
                {
                    while (countReader.Read())
                    {
                        int pId = countReader.GetInt32("parent_id");
                        int subFolders = countReader.IsDBNull(countReader.GetOrdinal("subFolders")) ? 0 : countReader.GetInt32("subFolders");
                        int subFiles = countReader.IsDBNull(countReader.GetOrdinal("subFiles")) ? 0 : countReader.GetInt32("subFiles");
                        folderCounts[pId] = (subFolders, subFiles);
                    }
                }

                // 3. RUN MAIN QUERY
                using var cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrWhiteSpace(search) && scopedToFolder)
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{search.Trim()}%");
                    SetParentId(cmd, _currentFolderId);
                }
                else if (!string.IsNullOrWhiteSpace(search))
                {
                    cmd.Parameters.AddWithValue("@Search", $"%{search.Trim()}%");
                }
                else
                {
                    SetParentId(cmd, _currentFolderId);
                }

                // Pre-cache column ordinals for speed
                using var r = cmd.ExecuteReader();
                int idxId = r.GetOrdinal("id");
                int idxIsFolder = r.GetOrdinal("is_folder");
                int idxDisplayName = r.GetOrdinal("display_name");
                int idxFileType = r.GetOrdinal("file_type");
                int idxFileSize = r.GetOrdinal("file_size");
                int idxIsLocked = r.GetOrdinal("is_locked");
                int idxUploadedBy = r.GetOrdinal("uploaded_by");
                int idxUploadedAt = r.GetOrdinal("uploaded_at");

                Color folderColor = Color.FromArgb(20, 80, 160);
                Color lockedColor = Color.Firebrick;

                // Use a list to buffer rows instead of inserting into the dgv one-by-one
                var rowsToAdd = new List<DataGridViewRow>();

                while (r.Read())
                {
                    bool isFolder = r.GetBoolean(idxIsFolder);
                    bool isLocked = r.GetBoolean(idxIsLocked);
                    string name = r.GetString(idxDisplayName);
                    string rawType = NormalizeFileType(r.IsDBNull(idxFileType) ? "" : r.GetString(idxFileType));
                    string type = isFolder ? "📁 Folder" : GetFileIcon(rawType) + " " + rawType.ToUpper();
                    string size = isFolder ? "" : FormatSize(r.GetInt64(idxFileSize));
                    string date = r.GetDateTime(idxUploadedAt).ToString("yyyy-MM-dd HH:mm");
                    string uploader = r.IsDBNull(idxUploadedBy) ? "" : r.GetString(idxUploadedBy);
                    string locked = isLocked ? "🔒" : "";
                    int id = r.GetInt32(idxId);

                    string countStr = "";
                    if (isFolder)
                    {
                        // Instant memory lookup instead of hitting the database loop!
                        if (folderCounts.TryGetValue(id, out var counts))
                        {
                            countStr = $"{counts.subFolders}📁 {counts.subFiles}📄";
                        }
                        else
                        {
                            countStr = "0📁 0📄";
                        }
                    }

                    var row = new DataGridViewRow();
                    row.CreateCells(dgv, isFolder ? "📁" : GetFileIcon(rawType), name, type, size, countStr, date, uploader, locked);
                    row.Tag = new FileEntry(id, isFolder, name, rawType, isLocked);

                    if (isFolder)
                        row.DefaultCellStyle.ForeColor = folderColor;
                    else if (isLocked)
                        row.DefaultCellStyle.ForeColor = lockedColor;

                    rowsToAdd.Add(row);
                }

                // 4. Batch add all items simultaneously
                if (rowsToAdd.Count > 0)
                {
                    dgv.Rows.AddRange(rowsToAdd.ToArray());
                }

                UpdateStatusBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("DB Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 5. Restore resizing states and execute final UI painting instructions
                dgv.RowHeadersWidthSizeMode = oldHeadersMode;
                dgv.ResumeLayout();
            }
        }
        private (int folders, int files) GetChildCounts(int folderId)
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    SELECT
                        SUM(CASE WHEN is_folder = 1 THEN 1 ELSE 0 END) AS folders,
                        SUM(CASE WHEN is_folder = 0 THEN 1 ELSE 0 END) AS files
                    FROM opd_file_manager
                    WHERE parent_id = @id AND is_deleted = 0", conn);
                cmd.Parameters.AddWithValue("@id", folderId);
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    int f = r.IsDBNull(0) ? 0 : Convert.ToInt32(r[0]);
                    int fi = r.IsDBNull(1) ? 0 : Convert.ToInt32(r[1]);
                    return (f, fi);
                }
            }
            catch { }
            return (0, 0);
        }

        private void UpdateStatusBar()
        {
            int folders = dgv.Rows.Cast<DataGridViewRow>()
                .Count(r => r.Tag is FileEntry fe && fe.IsFolder);
            int files = dgv.Rows.Count - folders;
            lblStatus2.Text = $"📁 {folders} folder(s)   📄 {files} file(s)   |   Total: {dgv.Rows.Count} items   |   DGTHMC · HIMS · OPD | I.T NGANI | Dether/Zaheer Lagos";
        }

        // ── Navigation ────────────────────────────────────────────────────────

        private void UpdateBreadcrumb()
        {
            var parts = new List<string> { "📁 Root" };
            foreach (var crumb in _breadcrumb.Reverse())
                parts.Add(crumb.Name);
            lblBreadcrumb.Text = string.Join("  ›  ", parts);
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var entry = dgv.Rows[e.RowIndex].Tag as FileEntry;
            if (entry == null || !entry.IsFolder) return;

            _breadcrumb.Push((_currentFolderId, entry.Name));
            _currentFolderId = entry.Id;
            txtSearch.Clear();
            RefreshGrid();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (_breadcrumb.Count == 0) return;
            var (parentId, _) = _breadcrumb.Pop();
            _currentFolderId = parentId;
            txtSearch.Clear();
            RefreshGrid();
        }

        // ── Search ────────────────────────────────────────────────────────────

        //private void txtSearch_TextChanged(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(txtSearch.Text))
        //        RefreshGrid();
        //    else
        //        RefreshGrid(txtSearch.Text);
        //}


        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            RefreshGrid();
        }

        // ── New Folder ────────────────────────────────────────────────────────

        private void btnNewFolder_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.File_NewFolder); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            string? name = Prompt("New Folder", "Enter folder name:");
            if (string.IsNullOrWhiteSpace(name)) return;

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    INSERT INTO opd_file_manager
                        (parent_id, is_folder, display_name, file_size, uploaded_by)
                    VALUES (@ParentId, 1, @Name, 0, @By)", conn);
                SetParentId(cmd, _currentFolderId);
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@By", _user.FullName);
                cmd.ExecuteNonQuery();
                // INSERT: inside catch-free block, after cmd.ExecuteNonQuery(); RefreshGrid();
                AuditLogger.Log(_user,
                    AuditLogger.ModFileManager, AuditLogger.CreateFolder,
                    targetName: name.Trim(),
                    detail: $"ParentFolderId={_currentFolderId?.ToString() ?? "root"}");
                RefreshGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Upload ────────────────────────────────────────────────────────────

        //    private async void btnUpload_Click(object sender, EventArgs e)
        //    {
        //        using var dlg = new OpenFileDialog();
        //        dlg.Multiselect = true;
        //        dlg.Filter = "All Supported|*.pdf;*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.docx;*.xlsx;*.txt|All Files|*.*";
        //        if (dlg.ShowDialog() != DialogResult.OK) return;

        //        SetToolbarEnabled(false);
        //        try { await Task.Run(() => UploadFiles(dlg.FileNames)); }
        //        finally { SetToolbarEnabled(true); }

        //    }

        //    private async void btnBulkUpload_Click(object sender, EventArgs e)
        //    {
        //        using var dlg = new FolderBrowserDialog();
        //        dlg.Description = "Select folder to upload (entire structure mirrored)";
        //        if (dlg.ShowDialog() != DialogResult.OK) return;

        //        int total = Directory.GetFiles(dlg.SelectedPath, "*", SearchOption.AllDirectories).Length;
        //        ShowProgress(0, total, "Preparing…");

        //        SetToolbarEnabled(false);
        //        try
        //        {
        //            await Task.Run(() => BulkUploadFolder(dlg.SelectedPath, _currentFolderId, new int[] { 0 }, total));
        //            HideProgress("Bulk upload complete.");
        //            RefreshGrid();
        //            MessageBox.Show("Bulk upload complete!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        finally { SetToolbarEnabled(true); }
        //    }


        //    private void BulkUploadFolder(string localPath, int? parentId, int[] counter, int total)
        //    {
        //        // Log the whole bulk upload as one entry (individual files are logged in UploadFiles)
        //        AuditLogger.Log(_user,
        //            AuditLogger.ModFileManager, AuditLogger.BulkUpload,
        //            targetName: Path.GetFileName(localPath),
        //            detail: $"LocalPath={localPath} | ParentId={parentId?.ToString() ?? "root"}");
        //        string folderName = Path.GetFileName(localPath);
        //        int newFolderId = InsertFolder(folderName, parentId);
        //        UploadFiles(Directory.GetFiles(localPath), newFolderId, counter, total);
        //        foreach (string sub in Directory.GetDirectories(localPath))
        //            BulkUploadFolder(sub, newFolderId, counter, total);
        //    }

        //    private void UploadFiles(string[] paths, int? overrideParent = null,
        //                              int[]? counter = null, int total = 0)
        //    {
        //        bool own = counter == null;
        //        if (own) { counter = new int[] { 0 }; total = paths.Length; ShowProgress(0, total, "Starting…"); }

        //        using var conn = DbConfig.OpenConnection();

        //        foreach (string path in paths)
        //        {
        //            string fileName = Path.GetFileName(path);
        //            counter![0]++;
        //            ShowProgress(counter[0], total, $"Uploading ({counter[0]}/{total}): {fileName}");
        //            try
        //            {
        //                byte[] data = File.ReadAllBytes(path);
        //                string ext = Path.GetExtension(path).TrimStart('.').ToLower();
        //                using var cmd = new SqlCommand(@"
        //                    INSERT INTO opd_file_manager
        //                        (parent_id, is_folder, display_name, file_type, file_size, file_data, uploaded_by)
        //                    VALUES (@P, 0, @N, @T, @S, @D, @By)", conn);
        //                int? ep = overrideParent ?? _currentFolderId;
        //                SetParentId(cmd, ep, "@P");
        //                cmd.Parameters.AddWithValue("@N", fileName);
        //                cmd.Parameters.AddWithValue("@T", ext);
        //                cmd.Parameters.AddWithValue("@S", data.LongLength);
        //                cmd.Parameters.Add("@D", SqlDbType.LongBlob).Value = data;
        //                cmd.Parameters.AddWithValue("@By", _user.FullName);
        //                cmd.ExecuteNonQuery();
        //                AuditLogger.Log(_user,
        //AuditLogger.ModFileManager, AuditLogger.Upload,
        //targetName: fileName,
        //detail: $"Size={data.LongLength} bytes | ParentId={ep?.ToString() ?? "root"}");
        //            }
        //            catch (Exception ex)
        //            {
        //                Invoke(() => MessageBox.Show($"Failed: {fileName}\n{ex.Message}", "Upload Error",
        //                    MessageBoxButtons.OK, MessageBoxIcon.Warning));
        //            }
        //        }
        //        //if (own) { HideProgress($"Done — {total} file(s) uploaded."); Invoke(RefreshGrid); }
        //        if (own) { HideProgress($"Done — {total} file(s) uploaded."); Invoke(() => RefreshGrid()); }

        //    }
        // ── Upload (High-Speed Asynchronous & Batched) ─────────────────────────

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.File_Upload); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            using var dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.Filter = "All Supported|*.pdf;*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.docx;*.xlsx;*.txt|All Files|*.*";
            if (dlg.ShowDialog() != DialogResult.OK) return;

            // ── Priority 1: pre-check free disk space before starting the copy ──
            try
            {
                long totalBytes = 0;
                foreach (string p in dlg.FileNames)
                {
                    try { totalBytes += new FileInfo(p).Length; } catch { /* skip unreadable */ }
                }
                FileStorage.EnsureFreeSpace(totalBytes);
            }
            catch (Exception spaceEx)
            {
                MessageBox.Show(spaceEx.Message, "Not Enough Disk Space",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetToolbarEnabled(false);
            _uploadCts = new CancellationTokenSource();
            try
            {
                await Task.Run(() => UploadFilesBatch(dlg.FileNames, _currentFolderId, token: _uploadCts.Token));
            }
            finally
            {
                SetToolbarEnabled(true);
                _uploadCts.Dispose();
                _uploadCts = null;
            }
        }

        private async void btnBulkUpload_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.File_Upload); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            using var dlg = new FolderBrowserDialog();
            dlg.Description = "Select folder to upload (entire structure mirrored)";
            if (dlg.ShowDialog() != DialogResult.OK) return;

            string[] allFiles = Directory.GetFiles(dlg.SelectedPath, "*", SearchOption.AllDirectories);
            int total = allFiles.Length;

            // ── Priority 1: pre-check free disk space for the entire folder before starting ──
            try
            {
                long totalBytes = 0;
                foreach (string p in allFiles)
                {
                    try { totalBytes += new FileInfo(p).Length; } catch { /* skip unreadable */ }
                }
                FileStorage.EnsureFreeSpace(totalBytes);
            }
            catch (Exception spaceEx)
            {
                MessageBox.Show(spaceEx.Message, "Not Enough Disk Space",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowProgress(0, total, "Preparing high-speed folder mirror mapping...");

            SetToolbarEnabled(false);
            _uploadCts = new CancellationTokenSource();
            try
            {
                await Task.Run(() => BulkUploadFolderOptimized(dlg.SelectedPath, _currentFolderId, new int[] { 0 }, total, _uploadCts.Token));
                bool wasCancelled = _uploadCts.IsCancellationRequested;
                HideProgress(wasCancelled ? "Bulk upload cancelled." : "Bulk folder upload complete.");
                RefreshGrid();
                if (wasCancelled)
                    MessageBox.Show("Upload cancelled. Files copied before cancelling were kept.", "Cancelled",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("Bulk upload complete!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                SetToolbarEnabled(true);
                _uploadCts.Dispose();
                _uploadCts = null;
            }
        }

        private void BulkUploadFolderOptimized(string localPath, int? parentId, int[] counter, int total, CancellationToken token = default)
        {
            if (token.IsCancellationRequested) return;

            // Log the overall container event 
            AuditLogger.Log(_user,
                AuditLogger.ModFileManager, AuditLogger.BulkUpload,
                targetName: Path.GetFileName(localPath),
                detail: $"LocalPath={localPath} | ParentId={parentId?.ToString() ?? "root"}");

            string folderName = Path.GetFileName(localPath);
            int newFolderId = InsertFolder(folderName, parentId);

            // Directly push the items in the current directory level
            UploadFilesBatch(Directory.GetFiles(localPath), newFolderId, counter, total, token);

            if (token.IsCancellationRequested) return;

            // Drill down safely
            foreach (string sub in Directory.GetDirectories(localPath))
            {
                if (token.IsCancellationRequested) return;
                BulkUploadFolderOptimized(sub, newFolderId, counter, total, token);
            }
        }

        // High-Speed Batched Upload Pipeline
        // Files are streamed to disk via FileStorage (date-bucketed folders) and only
        // a small path + metadata row is written to opd_file_manager. This keeps the
        // database itself small and fast no matter how much total file data is
        // uploaded, and avoids loading entire files into memory or holding one giant
        // transaction open for the whole batch.
        private void UploadFilesBatch(string[] paths, int? targetParentId, int[]? sharedCounter = null, int totalItems = 0, CancellationToken token = default)
        {
            bool isStandAlone = (sharedCounter == null);
            if (isStandAlone)
            {
                sharedCounter = new int[] { 0 };
                totalItems = paths.Length;
                ShowProgress(0, totalItems, "Initializing batch pipeline...");
            }

            if (paths.Length == 0) return;

            using var conn = DbConfig.OpenConnection();

            // Each file is inserted as its own small, fast, auto-committed statement
            // (the file copy to disk already happened by the time we touch the DB),
            // rather than one mega-transaction spanning the whole batch — that keeps
            // no long-held locks/transaction-log growth no matter how large the
            // overall upload is, and lets progress survive a crash mid-batch.
            using var cmd = new SqlCommand(@"
                INSERT INTO opd_file_manager
                    (parent_id, is_folder, display_name, file_type, file_size, system_path, uploaded_by)
                VALUES (@P, 0, @N, @T, @S, @Path, @By)", conn);

            cmd.Parameters.Add("@P", SqlDbType.Int);
            cmd.Parameters.Add("@N", SqlDbType.VarChar, 255);
            cmd.Parameters.Add("@T", SqlDbType.VarChar, 50);
            cmd.Parameters.Add("@S", SqlDbType.BigInt);
            cmd.Parameters.Add("@Path", SqlDbType.NVarChar, 500);
            cmd.Parameters.Add("@By", SqlDbType.VarChar, 255);

            foreach (string path in paths)
            {
                if (token.IsCancellationRequested)
                    break; // stop before starting a new file; whatever already uploaded stays

                string fileName = Path.GetFileName(path);
                sharedCounter![0]++;

                // Periodically notify UI to stay responsive
                ShowProgress(sharedCounter[0], totalItems, $"Uploading ({sharedCounter[0]}/{totalItems}): {fileName}");

                try
                {
                    var fi = new FileInfo(path);
                    string ext = Path.GetExtension(path).TrimStart('.').ToLower();

                    // Stream the file straight to managed storage on disk — no
                    // ReadAllBytes, so multi-GB files don't spike memory. Uses the
                    // "safe" variant so a failure mid-copy (disk full, drive
                    // disconnect, etc.) deletes the partial file immediately
                    // instead of leaving an orphan behind on disk.
                    string relativePath = FileStorage.SaveFileSafe(path, fileName);

                    if (targetParentId.HasValue && targetParentId.Value > 0)
                        cmd.Parameters["@P"].Value = targetParentId.Value;
                    else
                        cmd.Parameters["@P"].Value = DBNull.Value;

                    cmd.Parameters["@N"].Value = fileName;
                    cmd.Parameters["@T"].Value = ext;
                    cmd.Parameters["@S"].Value = fi.Length;
                    cmd.Parameters["@Path"].Value = relativePath;
                    cmd.Parameters["@By"].Value = _user.FullName;

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // The physical file was already saved but the DB row failed —
                        // clean it up immediately so it doesn't become an orphan that
                        // a reconciliation pass has to find later.
                        FileStorage.TryDelete(relativePath);
                        throw;
                    }

                    AuditLogger.Log(_user, AuditLogger.ModFileManager, AuditLogger.Upload,
                        targetName: fileName,
                        detail: $"Size={fi.Length} bytes | ParentId={targetParentId?.ToString() ?? "root"}");
                }
                catch (Exception ex)
                {
                    Invoke(() => MessageBox.Show($"Failed uploading file: {fileName}\n{ex.Message}", "Upload Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning));
                }
            }

            if (isStandAlone)
            {
                if (token.IsCancellationRequested)
                    HideProgress($"Cancelled — {sharedCounter![0] - 1} of {totalItems} file(s) uploaded before stopping.");
                else
                    HideProgress($"Done — {totalItems} file(s) safely uploaded.");
                Invoke(() => RefreshGrid());
            }
        }

        private int InsertFolder(string name, int? parentId)
        {
            using var conn = DbConfig.OpenConnection();
            using var cmd = new SqlCommand(@"
        INSERT INTO opd_file_manager (parent_id, is_folder, display_name, file_size, uploaded_by)
        VALUES (@P, 1, @N, 0, @By);
        SELECT CAST(SCOPE_IDENTITY() AS INT);", conn);
            SetParentId(cmd, parentId, "@P");
            cmd.Parameters.AddWithValue("@N", name);
            cmd.Parameters.AddWithValue("@By", _user.FullName);
            return Convert.ToInt32(cmd.ExecuteScalar()!);
        }

        // ── Download ──────────────────────────────────────────────────────────

        private void btnDownload_Click(object sender, EventArgs e)
        {
            var entry = SelectedEntry();
            if (entry == null) return;
            if (entry.IsFolder) { MessageBox.Show("Cannot download a folder.", "Info"); return; }

            using var dlg = new SaveFileDialog { FileName = entry.Name };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "SELECT system_path, file_data FROM opd_file_manager WHERE id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", entry.Id);
                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show("No file data stored.", "No Data",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string? relPath = reader["system_path"] as string;
                bool hasPath = !string.IsNullOrWhiteSpace(relPath);
                object legacyData = hasPath ? DBNull.Value : reader["file_data"];
                reader.Close();

                bool watermarkOn = AppSettingsService.GetBool(AppSettingsService.WatermarkEnabledKey);
                bool isImage = WatermarkService.IsImage(entry.FileType);

                if (watermarkOn && isImage)
                {
                    // Stamp a "downloaded by <user> at <timestamp>" watermark into
                    // the saved copy so a leaked file can be traced back to who
                    // pulled it. The stored original on the server is untouched —
                    // only the copy going out the door is stamped.
                    using Image original = hasPath
                        ? LoadImageFromDisk(relPath!)
                        : (legacyData is byte[] data && data.Length > 0
                            ? LoadImageFromBytes(data)
                            : throw new InvalidOperationException("No file data stored."));

                    using Bitmap stamped = WatermarkService.StampImage(original, WatermarkService.BuildText(_user));
                    WatermarkService.SaveWithFormat(stamped, dlg.FileName);
                }
                else if (hasPath)
                {
                    // New-style: file lives on disk, stream it out directly.
                    FileStorage.CopyOut(relPath!, dlg.FileName);
                }
                else if (legacyData is byte[] data && data.Length > 0)
                {
                    // Legacy row uploaded before disk storage was added.
                    File.WriteAllBytes(dlg.FileName, data);
                }
                else
                {
                    MessageBox.Show("No file data stored.", "No Data",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AuditLogger.Log(_user,
    AuditLogger.ModFileManager, AuditLogger.Download,
    targetId: entry.Id.ToString(),
    targetName: entry.Name);
                MessageBox.Show("Downloaded successfully.", "Done",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Preview Image ─────────────────────────────────────────────────────

        private void btnPreview_Click(object sender, EventArgs e)
        {
            var entry = SelectedEntry();
            if (entry == null) return;
            if (entry.IsFolder) { MessageBox.Show("Select a file to preview.", "Info"); return; }

            string ext = entry.FileType.ToLower();
            if (!new[] { "jpg", "jpeg", "png", "bmp", "gif", "tiff", "tif", "webp" }.Contains(ext))
            {
                MessageBox.Show("Preview is only available for image files (jpg, png, bmp, gif).",
                    "Not an Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "SELECT system_path, file_data FROM opd_file_manager WHERE id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", entry.Id);
                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show("No image data found.", "No Data"); return;
                }

                string? relPath = reader["system_path"] as string;
                bool hasPath = !string.IsNullOrWhiteSpace(relPath);
                object legacyData = hasPath ? DBNull.Value : reader["file_data"];
                reader.Close();

                Image img;
                if (hasPath)
                {
                    using var fs = FileStorage.OpenRead(relPath!);
                    using var msFromDisk = new MemoryStream();
                    fs.CopyTo(msFromDisk);
                    msFromDisk.Position = 0;
                    img = Image.FromStream(msFromDisk);
                }
                else if (legacyData is byte[] data && data.Length > 0)
                {
                    using var ms = new MemoryStream(data);
                    img = Image.FromStream(ms);
                }
                else
                {
                    MessageBox.Show("No image data found.", "No Data"); return;
                }

                var viewer = new ImageViewerForm(entry.Name, img);
                viewer.ShowDialog(this);
                img.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot preview: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Rename ────────────────────────────────────────────────────────────


        private void btnRename_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.File_Rename); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            var entry = SelectedEntry();
            if (entry == null) return;
            if (entry.IsLocked) { MessageBox.Show("Item is locked.", "Locked"); return; }

            string? newName = Prompt("Rename", "Enter new name:", entry.Name);
            if (string.IsNullOrWhiteSpace(newName)) return;

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "UPDATE opd_file_manager SET display_name = @N WHERE id = @Id", conn);
                cmd.Parameters.AddWithValue("@N", newName.Trim());
                cmd.Parameters.AddWithValue("@Id", entry.Id);
                cmd.ExecuteNonQuery();
                AuditLogger.Log(_user,
    AuditLogger.ModFileManager, "RENAME",
    targetId: entry.Id.ToString(),
    targetName: newName!.Trim(),
    detail: $"OldName={entry.Name}");
                RefreshGrid();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        // ── Delete ────────────────────────────────────────────────────────────

        //    private void btnDelete_Click(object sender, EventArgs e)
        //    {
        //        var rows = dgv.SelectedRows.Cast<DataGridViewRow>()
        //                     .Select(r => r.Tag as FileEntry).Where(f => f != null).ToList();
        //        if (rows.Count == 0) return;

        //        if (MessageBox.Show($"Move {rows.Count} item(s) to the Recycle Bin?",
        //            "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

        //        using var conn = DbConfig.OpenConnection();
        //        foreach (var entry in rows)
        //        {
        //            if (entry!.IsLocked) { MessageBox.Show($"'{entry.Name}' is locked.", "Locked"); continue; }
        //            SoftDelete(entry.Id, conn);
        //            AuditLogger.Log(_user,
        //AuditLogger.ModFileManager, AuditLogger.Delete,
        //targetId: entry!.Id.ToString(),
        //targetName: entry.Name,
        //detail: entry.IsFolder ? "Folder (recursive) -> Recycle Bin" : "File -> Recycle Bin");
        //        }
        //        RefreshGrid();
        //    }
        // ── Optimized High-Speed Delete ──────────────────────────────────────

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.File_Delete); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            var rows = dgv.SelectedRows.Cast<DataGridViewRow>()
                         .Select(r => r.Tag as FileEntry).Where(f => f != null).ToList();
            if (rows.Count == 0) return;

            if (MessageBox.Show($"Move {rows.Count} item(s) to the Recycle Bin?",
                "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            // 1. Lock toolbar UI to prevent double clicking
            SetToolbarEnabled(false);

            int totalSelected = rows.Count;
            ShowProgress(0, totalSelected, "Scanning and preparing items...");

            try
            {
                // 2. Offload the entire process to a background thread
                await Task.Run(() =>
                {
                    using var conn = DbConfig.OpenConnection();

                    // List to gather ALL IDs that need to be soft-deleted (including nested files/folders)
                    var allIdsToDelete = new List<int>();
                    int counter = 0;

                    foreach (var entry in rows)
                    {
                        counter++;

                        // Update progress with what we are scanning right now
                        string typeLabel = entry!.IsFolder ? "Folder" : "File";
                        ShowProgress(counter, totalSelected, $"Processing {typeLabel} ({counter}/{totalSelected}): \"{entry.Name}\"");

                        if (entry.IsLocked)
                        {
                            Invoke(() => MessageBox.Show($"'{entry.Name}' is locked and cannot be deleted.", "Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning));
                            continue;
                        }

                        // Fast recursive ID gathering (No updates yet!)
                        GatherDeleteIdsRecursive(entry.Id, allIdsToDelete, conn);
                    }

                    // 3. BATCH UPDATE: If we found IDs to delete, execute them all in ONE single fast query
                    if (allIdsToDelete.Count > 0)
                    {
                        ShowProgress(allIdsToDelete.Count, allIdsToDelete.Count, $"Moving {allIdsToDelete.Count} total items to Recycle Bin...");

                        // Build: UPDATE opd_file_manager SET is_deleted = 1... WHERE id IN (1, 2, 3...)
                        string idList = string.Join(",", allIdsToDelete);
                        string batchSql = $@"
                            UPDATE opd_file_manager
                            SET    is_deleted = 1, deleted_by = @who, deleted_at = SYSDATETIME()
                            WHERE  id IN ({idList})";

                        using var batchCmd = new SqlCommand(batchSql, conn);
                        batchCmd.Parameters.AddWithValue("@who", _user.FullName);
                        batchCmd.ExecuteNonQuery();

                        // Log top-level selections to Audit Log
                        foreach (var entry in rows)
                        {
                            if (entry!.IsLocked) continue;
                            AuditLogger.Log(_user,
                                AuditLogger.ModFileManager, AuditLogger.Delete,
                                targetId: entry.Id.ToString(),
                                targetName: entry.Name,
                                detail: entry.IsFolder ? $"Folder (Batch recursive) -> Recycle Bin ({allIdsToDelete.Count} items total)" : "File -> Recycle Bin");
                        }
                    }
                });

                // 4. Wrap up and refresh grid seamlessly
                HideProgress($"Done — Items successfully moved to Recycle Bin.");
                RefreshGrid();
            }
            catch (Exception ex)
            {
                HideProgress("An error occurred during deletion.");
                MessageBox.Show("Delete Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 5. Always ensure controls are unlocked
                SetToolbarEnabled(true);
            }
        }

        // Fast helper method to pull descendant IDs down without executing heavy UPDATEs sequentially
        private void GatherDeleteIdsRecursive(int id, List<int> allIds, SqlConnection conn)
        {
            allIds.Add(id); // Add current item

            // Fetch child IDs instantly
            var children = new List<int>();
            using (var cmd = new SqlCommand("SELECT id FROM opd_file_manager WHERE parent_id = @id AND is_deleted = 0", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    children.Add(r.GetInt32(0));
                }
            }

            // Drill down into children recursively
            foreach (int childId in children)
            {
                GatherDeleteIdsRecursive(childId, allIds, conn);
            }
        }
        // Soft delete: flags the row (and all its descendants) as deleted instead
        // of removing it. Items can be restored from the Recycle Bin.
        private void SoftDelete(int id, SqlConnection conn)
        {
            var children = new List<int>();
            using (var cmd = new SqlCommand(
                "SELECT id FROM opd_file_manager WHERE parent_id = @id AND is_deleted = 0", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using var r = cmd.ExecuteReader();
                while (r.Read()) children.Add(r.GetInt32(0));
            }
            foreach (int c in children) SoftDelete(c, conn);

            using var upd = new SqlCommand(@"
                UPDATE opd_file_manager
                SET    is_deleted = 1, deleted_by = @who, deleted_at = SYSDATETIME()
                WHERE  id = @id", conn);
            upd.Parameters.AddWithValue("@who", _user.FullName);
            upd.Parameters.AddWithValue("@id", id);
            upd.ExecuteNonQuery();
        }

        // ── Lock / Unlock ─────────────────────────────────────────────────────

        private void btnLock_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.File_Lock); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            var entry = SelectedEntry();
            if (entry == null) return;
            string sql = entry.IsLocked
                ? "UPDATE opd_file_manager SET is_locked = 0 WHERE id = @Id"
                : "UPDATE opd_file_manager SET is_locked = 1 WHERE id = @Id";
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", entry.Id);
                cmd.ExecuteNonQuery();
                string lockAction = entry.IsLocked ? AuditLogger.Unlock : AuditLogger.Lock;
                AuditLogger.Log(_user,
                    AuditLogger.ModFileManager, lockAction,
                    targetId: entry.Id.ToString(),
                    targetName: entry.Name);
                RefreshGrid();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        // ── Print Preview ─────────────────────────────────────────────────────

        private Image? _printImage;
        private string _printImageName = "";

        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            var entry = SelectedEntry();
            if (entry == null)
            {
                MessageBox.Show("Select an image file to print.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (entry.IsFolder)
            {
                MessageBox.Show("Cannot print a folder. Select an image file.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string ext = entry.FileType.ToLower();
            if (!new[] { "jpg", "jpeg", "png", "bmp", "gif", "tiff", "tif", "webp" }.Contains(ext))
            {
                MessageBox.Show("Printing is only available for image files.", "Not an Image",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "SELECT system_path, file_data FROM opd_file_manager WHERE id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", entry.Id);
                using var reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    MessageBox.Show("No image data found.", "No Data"); return;
                }

                string? relPath = reader["system_path"] as string;
                bool hasPath = !string.IsNullOrWhiteSpace(relPath);
                object legacyData = hasPath ? DBNull.Value : reader["file_data"];
                reader.Close();

                if (hasPath)
                {
                    using var fs = FileStorage.OpenRead(relPath!);
                    using var msFromDisk = new MemoryStream();
                    fs.CopyTo(msFromDisk);
                    msFromDisk.Position = 0;
                    _printImage = Image.FromStream(msFromDisk);
                }
                else if (legacyData is byte[] data && data.Length > 0)
                {
                    using var ms = new MemoryStream(data);
                    _printImage = Image.FromStream(ms);
                }
                else
                {
                    MessageBox.Show("No image data found.", "No Data"); return;
                }

                _printImageName = entry.Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot load image: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var pd = new PrintDocument();
            pd.DocumentName = "HIMS File Manager — " + _printImageName;
            pd.DefaultPageSettings.PaperSize = new PaperSize("8.5x13", 850, 1300);
            pd.DefaultPageSettings.Margins = new Margins(20, 20, 20, 20);
            pd.PrintPage += PrintImagePage;

            using var ppd = new PrintPreviewDialog();
            ppd.Document = pd;
            ppd.Width = 900;
            ppd.Height = 700;
            AuditLogger.Log(_user,
    AuditLogger.ModFileManager, AuditLogger.PrintPreview,
    targetId: entry.Id.ToString(),
    targetName: entry.Name);
            ppd.ShowDialog(this);

            pd.PrintPage -= PrintImagePage;
            _printImage?.Dispose();
            _printImage = null;
        }

        private void PrintImagePage(object sender, PrintPageEventArgs e)
        {
            if (_printImage == null) return;
            var g = e.Graphics!;
            var bounds = e.MarginBounds;

            float scale = Math.Min((float)bounds.Width / _printImage.Width, (float)bounds.Height / _printImage.Height);
            float drawW = _printImage.Width * scale;
            float drawH = _printImage.Height * scale;
            float drawX = bounds.Left + (bounds.Width - drawW) / 2f;
            float drawY = bounds.Top + (bounds.Height - drawH) / 2f;

            g.DrawImage(_printImage, drawX, drawY, drawW, drawH);

            // Same traceability watermark as downloads (username + timestamp),
            // gated by the same SuperAdmin-only toggle in Settings — a printed
            // page can be photographed/photocopied just as easily as a
            // downloaded file can be leaked, so it gets the same treatment.
            if (AppSettingsService.GetBool(AppSettingsService.WatermarkEnabledKey))
            {
                WatermarkService.DrawOverlay(g, new RectangleF(drawX, drawY, drawW, drawH),
                    WatermarkService.BuildText(_user));
            }

            e.HasMorePages = false;
        }

        private int _printPageRow = 0;
        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics!;
            var bounds = e.MarginBounds;
            float y = bounds.Top;
            float rowH = 22f;

            using var hFont = new Font("Segoe UI", 12, FontStyle.Bold);
            using var sFont = new Font("Segoe UI", 7.5f);
            using var tFont = new Font("Segoe UI", 8f, FontStyle.Bold);
            using var dFont = new Font("Segoe UI", 7f);

            if (_printPageRow == 0)
            {
                g.DrawString("HIMS File Manager — OPD Document System",
                    hFont, Brushes.DarkBlue, bounds.Left, y);
                y += 22;
                g.DrawString($"Path: {lblBreadcrumb.Text}   |   Printed: {DateTime.Now:yyyy-MM-dd HH:mm}   |   By: {_user.FullName}",
                    dFont, Brushes.Gray, bounds.Left, y);
                y += 18;
                g.DrawLine(Pens.DarkBlue, bounds.Left, y, bounds.Right, y);
                y += 6;

                float[] cw = { 30, 220, 70, 80, 60, 110, 100 };
                string[] ch = { "#", "Name", "Type", "Size", "Items", "Uploaded", "By" };
                float cx = bounds.Left;
                g.FillRectangle(new SolidBrush(Color.FromArgb(28, 50, 90)),
                    bounds.Left, y, bounds.Width, rowH);
                for (int i = 0; i < ch.Length; i++)
                {
                    g.DrawString(ch[i], tFont, Brushes.White, cx + 2, y + 4);
                    cx += cw[i];
                }
                y += rowH + 2;
            }

            float[] colW = { 30, 220, 70, 80, 60, 110, 100 };
            int rowNum = _printPageRow + 1;
            while (_printPageRow < _printRows.Count)
            {
                var row = _printRows[_printPageRow];
                if (y + rowH > bounds.Bottom) { e.HasMorePages = true; return; }

                var bg = _printPageRow % 2 == 0
                    ? Brushes.White : new SolidBrush(Color.FromArgb(240, 245, 255));
                g.FillRectangle(bg, bounds.Left, y, bounds.Width, rowH);

                float cx = bounds.Left;
                string[] vals =
                {
                    rowNum.ToString(),
                    row.Cells[1].Value?.ToString() ?? "",
                    row.Cells[2].Value?.ToString() ?? "",
                    row.Cells[3].Value?.ToString() ?? "",
                    row.Cells[4].Value?.ToString() ?? "",
                    row.Cells[5].Value?.ToString() ?? "",
                    row.Cells[6].Value?.ToString() ?? "",
                };
                for (int i = 0; i < vals.Length; i++)
                {
                    var rf = new RectangleF(cx + 2, y + 3, colW[i] - 4, rowH - 4);
                    g.DrawString(vals[i], sFont, Brushes.Black, rf,
                        new StringFormat { Trimming = StringTrimming.EllipsisCharacter });
                    cx += colW[i];
                }

                g.DrawLine(new Pen(Color.FromArgb(210, 215, 225)),
                    bounds.Left, y + rowH, bounds.Right, y + rowH);
                y += rowH;
                _printPageRow++;
                rowNum++;
            }

            g.DrawLine(Pens.DarkBlue, bounds.Left, bounds.Bottom - 12, bounds.Right, bounds.Bottom - 12);
            g.DrawString($"Total: {_printRows.Count} items", dFont, Brushes.Gray,
                bounds.Left, bounds.Bottom - 10);
            _printPageRow = 0;
            e.HasMorePages = false;
        }

        // ── Progress ──────────────────────────────────────────────────────────

        private void ShowProgress(int cur, int tot, string detail)
        {
            if (InvokeRequired) { Invoke(() => ShowProgress(cur, tot, detail)); return; }
            pnlProgress.Visible = true;
            progressBar.Maximum = Math.Max(tot, 1);
            progressBar.Value = Math.Min(cur, progressBar.Maximum);
            lblPercent.Text = $"{(tot > 0 ? cur * 100 / tot : 0)}%";
            lblStatus.Text = detail;
            btnCancelUpload.Visible = _uploadCts != null;
            btnCancelUpload.Enabled = _uploadCts != null && !_uploadCts.IsCancellationRequested;
        }

        private void HideProgress(string msg = "")
        {
            if (InvokeRequired) { Invoke(() => HideProgress(msg)); return; }
            progressBar.Value = progressBar.Maximum;
            lblPercent.Text = "100%";
            lblStatus.Text = msg;
            btnCancelUpload.Visible = false;
            Task.Delay(400).Wait();
            pnlProgress.Visible = false;
        }

        // Cancels the in-progress bulk upload/mirror operation. The current
        // file being copied is allowed to finish (so we never leave a
        // half-written file on disk); no further files are processed after
        // that, and whatever was already uploaded stays in place.
        private void BtnCancelUpload_Click(object sender, EventArgs e)
        {
            if (_uploadCts == null || _uploadCts.IsCancellationRequested) return;

            if (MessageBox.Show("Cancel the upload? Files already uploaded will be kept.",
                    "Cancel Upload", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _uploadCts.Cancel();
            btnCancelUpload.Enabled = false;
            lblStatus.Text = "Cancelling — finishing current file…";
        }

        private void SetToolbarEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(() => SetToolbarEnabled(enabled));
                return;
            }

            // 1. Disable/Enable top header panel (Locks Logout button, profile interactions, etc.)
            if (pnlTop != null)
            {
                pnlTop.Enabled = enabled;
            }

            // 2. Disable or enable all actions on the main operational toolbar
            if (toolBar != null)
            {
                foreach (ToolStripItem item in toolBar.Items)
                {
                    item.Enabled = enabled;
                }
            }

            // 3. Disable search input during uploading to prevent user interruptions
            if (txtSearch != null)
            {
                txtSearch.Enabled = enabled;
            }

            // 4. Lock the grid completely to freeze row clicking, scrolling, and double-click navigation
            if (dgv != null)
            {
                dgv.Enabled = enabled;
            }
        }
        // ── Helpers ───────────────────────────────────────────────────────────

        private FileEntry? SelectedEntry()
        {
            if (dgv.CurrentRow == null) return null;
            return dgv.CurrentRow.Tag as FileEntry;
        }

        // Loads an image fully into memory and returns a fully-detached copy
        // (a fresh Bitmap, not tied to the backing stream), since GDI+ can
        // still need the original stream alive later for lazy decode/draw —
        // and here the stream is disposed as soon as this method returns.
        private static Image LoadImageFromDisk(string relativePath)
        {
            using var fs = FileStorage.OpenRead(relativePath);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            ms.Position = 0;
            using var decoded = Image.FromStream(ms);
            return new Bitmap(decoded);
        }

        private static Image LoadImageFromBytes(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var decoded = Image.FromStream(ms);
            return new Bitmap(decoded);
        }

        private static void SetParentId(SqlCommand cmd, int? parentId, string param = "@ParentId")
        {
            if (parentId.HasValue && parentId.Value != 0)
                cmd.Parameters.AddWithValue(param, parentId.Value);
            else
                cmd.Parameters.Add(param, SqlDbType.Int).Value = DBNull.Value;
        }

        /// <summary>
        /// Normalises whatever file_type string the DB contains into a plain
        /// lowercase extension with no dot and no MIME prefix.
        /// Handles old-website values like "image/jpeg", ".JPG", "application/pdf", etc.
        /// </summary>
        private static string NormalizeFileType(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";

            string s = raw.Trim().ToLower();

            // MIME type → extension
            return s switch
            {
                "image/jpeg" or "image/jpg" => "jpg",
                "image/png" => "png",
                "image/bmp" => "bmp",
                "image/gif" => "gif",
                "image/tiff" => "tiff",
                "image/webp" => "webp",
                "application/pdf" => "pdf",
                "application/msword" => "doc",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "docx",
                "application/vnd.ms-excel" => "xls",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => "xlsx",
                "text/plain" => "txt",
                "application/zip" => "zip",
                _ => s.TrimStart('.')   // handles ".jpg", ".PNG", etc.
            };
        }

        private static string GetFileIcon(string ext) => ext.ToLower() switch
        {
            "pdf" => "📄",
            "jpg" or "jpeg" or "png" or "bmp" or "gif" or "tiff" or "webp" => "🖼",
            "docx" or "doc" => "📝",
            "xlsx" or "xls" => "📊",
            "txt" => "📃",
            "zip" or "rar" => "📦",
            _ => "📄"
        };

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1048576) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1073741824) return $"{bytes / 1048576.0:F1} MB";
            return $"{bytes / 1073741824.0:F1} GB";
        }

        private static string? Prompt(string title, string label, string def = "")
        {
            using Form f = new()
            {
                Text = title,
                Width = 380,
                Height = 150,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label l = new() { Left = 12, Top = 15, Width = 340, Text = label };
            TextBox tb = new() { Left = 12, Top = 35, Width = 340, Text = def };
            Button ok = new() { Text = "OK", Left = 192, Top = 72, Width = 80, DialogResult = DialogResult.OK };
            Button cn = new() { Text = "Cancel", Left = 278, Top = 72, Width = 80, DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[] { l, tb, ok, cn });
            f.AcceptButton = ok; f.CancelButton = cn;
            return f.ShowDialog() == DialogResult.OK ? tb.Text : null;
        }

        private void dgv_SelectionChanged(object sender, EventArgs e) => UpdateToolbar();

        private void UpdateToolbar()
        {
            bool has = dgv.CurrentRow != null;
            btnDownload.Enabled = has;
            btnPreview.Enabled = has;
            btnRename.Enabled = has;
            btnDelete.Enabled = has;
            btnLock.Enabled = has;
            btnBack.Enabled = _breadcrumb.Count > 0;

            if (has)
            {
                var entry = SelectedEntry();
                btnLock.Text = entry?.IsLocked == true ? "" : "";
            }
        }

        // ── Settings ──────────────────────────────────────────────────────────

        private void btnSettings_Click(object? sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.Settings_View); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            using var dlg = new SettingsForm(_user);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                Text = $"HIMS File Manager — {DbConfig.Current.ServerIP}:{DbConfig.Current.MySqlPort}/{DbConfig.Current.Database}";
                RefreshGrid();
            }
        }

        // ── Logout buttons (all route to DoLogout) ────────────────────────────

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Logout and return to login?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                DoLogout();
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Logout and return to login?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                DoLogout();
        }

        private void toolBar_ItemClicked(object sender, ToolStripItemClickedEventArgs e) { }

        private void btnManageUsers_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.User_Create); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            using var dlg = new RegisterUserForm(_user);
            dlg.ShowDialog(this);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // stops the beep / newline
                e.Handled = true;
                RefreshGrid(txtSearch.Text);
            }
        }

        // Re-run the current search (if any) as soon as the "search this
        // folder only" scope is toggled, so results update immediately.
        private void ChkSearchInFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                RefreshGrid(txtSearch.Text);
        }

        private void btnUserList_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.User_View); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            using var dlg = new UsersListForm(_user);
            dlg.ShowDialog(this);
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            using var dlg = new UpdateProfileForm(_user);
            if (dlg.ShowDialog(this) == DialogResult.OK)
                RefreshHeaderFromDatabase();
        }

        // _user is an immutable record captured at login, so a profile edit
        // (new name/photo) won't show up on it automatically. Re-read just
        // the display fields from the DB and push them into the header UI.
        private void RefreshHeaderFromDatabase()
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "SELECT full_name, username, role, profile_pic_data FROM admins WHERE admin_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", _user.Id);
                using var r = cmd.ExecuteReader();
                if (!r.Read()) return;

                string fullName = r.GetString("full_name");
                string username = r.GetString("username");
                string role = r.GetString("role");
                byte[]? blob = r.IsDBNull(r.GetOrdinal("profile_pic_data"))
                                    ? null
                                    : (byte[])r["profile_pic_data"];
                r.Close();

                // Update text labels
                lblFullName.Text = fullName;
                lblUsername.Text = $"@{username}";
                lblRole.Text = FormatRoleLabel(role);
                lblRole.ForeColor = GetRoleColor(role);

                // Update profile picture
                Image img;
                if (blob != null)
                {
                    using var ms = new MemoryStream(blob);
                    using var temp = Image.FromStream(ms);
                    int safeW = picProfile.Width > 0 ? picProfile.Width : 48;
                    int safeH = picProfile.Height > 0 ? picProfile.Height : 48;
                    img = new Bitmap(temp, safeW, safeH);
                }
                else
                {
                    img = CreateInitialsAvatar(fullName, role, picProfile.Width);
                }

                picProfile.Image?.Dispose();
                picProfile.Image = img;
                MakeCircular(picProfile);
            }
            catch (Exception ex)
            {
                // Non-critical — header just keeps showing the previous values.
                System.Diagnostics.Debug.WriteLine("RefreshHeaderFromDatabase failed: " + ex);
            }
        }

        private void btnSuggestions_Click(object sender, EventArgs e)
        {
            using var dlg = new SuggestionsForm(_user);
            dlg.ShowDialog(this);
            badgeSuggestions = 0;
            UpdateNotificationBadges();
        }

        private void btnChat_Click(object sender, EventArgs e)
        {
            using var dlg = new FormChat(_user);
            dlg.ShowDialog(this);
            badgeChat = 0;
            UpdateNotificationBadges();
        }

        // Opens Chat with the currently selected file/folder attached as a
        // reference chip, ready for the user to type "@someone" and send —
        // that person then gets a notification linking straight back to this
        // item (see FormChat.SendMessage / NotificationService.CheckNewMentions).
        private void btnMentionChat_Click(object sender, EventArgs e)
        {
            var entry = SelectedEntry();
            if (entry == null)
            {
                MessageBox.Show("Select a file or folder first, then click @ to mention it in chat.",
                    "Nothing Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dlg = new FormChat(_user);
            dlg.AttachFileReference(entry.Id, entry.Name, entry.IsFolder);
            dlg.ShowDialog(this);
            badgeChat = 0;
            UpdateNotificationBadges();
        }

        private void btnRecycleBin_Click(object sender, EventArgs e)
        {

        }

        // ── Notifications ────────────────────────────────────────────────────
        private NotificationService? _notifier;
        private int badgeSuggestions = 0;
        private int badgeChat = 0;

        private void InitNotifications()
        {
            _notifier = new NotificationService(_user);
            _notifier.NewSuggestions += count => Invoke(() => OnNewSuggestions(count));
            _notifier.NewSuggestionReplies += count => Invoke(() => OnNewSuggestionReplies(count));
            _notifier.NewChatMessages += (count, sender, isPrivate) => Invoke(() => OnNewChatMessages(count, sender, isPrivate));
            _notifier.NewMentions += (count, from, itemId, itemName, isFolder) =>
                Invoke(() => OnNewMentions(count, from, itemId, itemName, isFolder));
            _notifier.Start();
        }

        private void OnNewSuggestions(int count)
        {
            badgeSuggestions = count;
            UpdateNotificationBadges();

            // ── Change parameters here ──────────────────────────────────────────
            // Passing 5000 keeps the window visible for exactly 5 seconds before it closes automatically
            var toast = new NotificationToastForm("New Suggestion", $"{count} new suggestion(s) received.", 5000);

            toast.ToastClicked += (s, e) =>
            {
                toast.Dispose();
                using var dlg = new SuggestionsForm(_user);
                dlg.ShowDialog(this);
                badgeSuggestions = 0;
                UpdateNotificationBadges();
            };

            toast.FormClosed += (s, e) => toast.Dispose();
            toast.ShowToast();
        }

        private void OnNewSuggestionReplies(int count)
        {
            badgeSuggestions += count;
            UpdateNotificationBadges();
            ShowToast("Suggestion Reply", $"You have {count} new repl{(count == 1 ? "y" : "ies")}.", () =>
            {
                using var dlg = new SuggestionsForm(_user);
                dlg.ShowDialog(this);
                badgeSuggestions = 0;
                UpdateNotificationBadges();
            });
        }

        private void OnNewChatMessages(int count, string sender, bool isPrivate)
        {
            badgeChat += count;
            UpdateNotificationBadges();

            string toastMsg = isPrivate
                ? $"{sender} sent you a private message."
                : $"{sender} sent a message in the Public Room.";

            ShowToast("💬 New Message", toastMsg, () =>
            {
                using var dlg = new FormChat(_user);
                dlg.ShowDialog(this);
                badgeChat = 0;
                UpdateNotificationBadges();
            });
        }

        // Someone @mentioned me in chat, possibly tied to a specific file/folder.
        // Clicking the toast jumps straight into that folder (and highlights the
        // file, if it was a file) before opening Chat so I can see the message.
        private void OnNewMentions(int count, string fromWho, int? itemId, string? itemName, bool isFolder)
        {
            badgeChat += count;
            UpdateNotificationBadges();

            string toastMsg = itemName != null
                ? $"{fromWho} mentioned you about {(isFolder ? "folder" : "file")} \"{itemName}\"."
                : $"{fromWho} mentioned you in chat.";

            ShowToast("@ You were mentioned", toastMsg, () =>
            {
                if (itemId.HasValue)
                    NavigateToItem(itemId.Value, isFolder);

                using var dlg = new FormChat(_user);
                dlg.ShowDialog(this);
                badgeChat = 0;
                UpdateNotificationBadges();
            });
        }

        /// <summary>
        /// Jumps the file browser straight into the folder containing the given
        /// item (or into the item itself, if it's a folder) and highlights the
        /// row if it's a file — used by the @mention toast's click-through.
        /// </summary>
        private void NavigateToItem(int itemId, bool isFolder)
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "SELECT parent_id, is_deleted FROM opd_file_manager WHERE id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", itemId);
                using var r = cmd.ExecuteReader();
                if (!r.Read()) return;

                bool isDeleted = r.GetBoolean(r.GetOrdinal("is_deleted"));
                int? parentId = r.IsDBNull(r.GetOrdinal("parent_id")) ? null : r.GetInt32("parent_id");
                r.Close();

                if (isDeleted)
                {
                    MessageBox.Show("That item has since been moved to the Recycle Bin.",
                        "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // A folder mention opens *into* the folder; a file mention opens
                // its parent so the file is visible (and selected) in the grid.
                _breadcrumb.Clear();
                _currentFolderId = isFolder ? itemId : parentId;
                txtSearch.Clear();
                RefreshGrid();

                if (!isFolder)
                {
                    foreach (DataGridViewRow row in dgv.Rows)
                    {
                        if (row.Tag is FileEntry fe && fe.Id == itemId)
                        {
                            dgv.ClearSelection();
                            row.Selected = true;
                            dgv.CurrentCell = row.Cells[0];
                            dgv.FirstDisplayedScrollingRowIndex = row.Index;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't open that item: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateNotificationBadges()
        {
            btnSuggestions.Text = badgeSuggestions > 0 ? $" 💡({badgeSuggestions})" : "";
            btnSuggestions.DisplayStyle = badgeSuggestions > 0
                ? ToolStripItemDisplayStyle.ImageAndText
                : ToolStripItemDisplayStyle.Image;
            // Optional: Make suggestions text red too if greater than 0
            btnSuggestions.ForeColor = badgeSuggestions > 0 ? Color.Yellow : SystemColors.ControlText;

            btnChat.Text = badgeChat > 0 ? $"({badgeChat})" : "";
            btnChat.DisplayStyle = badgeChat > 0
                ? ToolStripItemDisplayStyle.ImageAndText
                : ToolStripItemDisplayStyle.Image;

            // 🔴 This turns the chat text RED when there are messages, and resets it when 0
            btnChat.ForeColor = badgeChat > 0 ? Color.Red : SystemColors.ControlText;
        }

        private void ShowToast(string title, string message, Action onClick)
        {
            var toast = new NotificationToastForm(title, message);
            toast.ToastClicked += (s, e) => { toast.Dispose(); onClick(); };
            toast.FormClosed += (s, e) => toast.Dispose();
            toast.ShowToast();
        }

        private void btnAuditLog_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.AuditLog_View); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            using var dlg = new AuditLogForm(_user);
            dlg.ShowDialog(this);
        }

        private void btnRecycleBin2_Click(object sender, EventArgs e)
        {
            using var dlg = new RecycleBinForm(_user);
            dlg.ShowDialog(this);
            RefreshGrid();
        }


        private void btnDashboard_Click(object sender, EventArgs e)
        {
            try { PermissionService.Require(_user, Permission.Dashboard_View); }
            catch (PermissionDeniedException ex) { MessageBox.Show(ex.Message, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Stop); return; }

            using var dsb = new DashboardAdmin(_user);
            dsb.ShowDialog(this);
        }

        private void btnScanDocs_Click(object sender, EventArgs e)
        {

        }

        private void dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }

        private void pnlTop_Paint(object sender, PaintEventArgs e)
        {

        }
    }

    // ── Small model ───────────────────────────────────────────────────────────
    record FileEntry(int Id, bool IsFolder, string Name, string FileType, bool IsLocked);
}