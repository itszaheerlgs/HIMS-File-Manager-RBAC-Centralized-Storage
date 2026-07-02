using Microsoft.Data.SqlClient;

namespace UPLOADER
{
    /// <summary>
    /// Static audit helper for HIMS File Manager v3.
    /// Call AuditLogger.Log(...) right after a DB/file operation succeeds.
    /// All failures are silently swallowed — an audit glitch must never
    /// block real work.
    ///
    /// UI helpers (BuildSidebarEntry, GetActionPillColor) are pure helpers
    /// that return ready-made WinForms panels/colors so every form can
    /// render a consistent audit sidebar without duplicating layout code.
    /// </summary>
    internal static class AuditLogger
    {
        // ────────────────────────────────────────────────────────────────────
        // ACTION CONSTANTS
        // ────────────────────────────────────────────────────────────────────

        // FileManager
        public const string CreateFolder    = "CREATE_FOLDER";
        public const string Upload          = "UPLOAD";
        public const string BulkUpload      = "BULK_UPLOAD";
        public const string Download        = "DOWNLOAD";
        public const string Rename          = "RENAME";
        public const string Delete          = "DELETE";
        public const string Lock            = "LOCK";
        public const string Unlock          = "UNLOCK";
        public const string PrintPreview    = "PRINT_PREVIEW";
        public const string ToggleWatermark = "TOGGLE_WATERMARK";

        // Scanner / Scan form
        public const string Scan            = "SCAN";
        public const string Import          = "IMPORT";
        public const string Rotate          = "ROTATE";
        public const string Flip            = "FLIP";
        public const string MovePage        = "MOVE";
        public const string DeletePage      = "DELETE_PAGE";
        public const string ClearPages      = "CLEAR";
        public const string SaveScan        = "SAVE";
        public const string RefreshScanners = "REFRESH";
        public const string CancelScan      = "CANCEL";
        public const string ExportAudit     = "EXPORT";

        // Suggestions
        public const string SendSuggestion  = "SEND_SUGGESTION";
        public const string ReplySuggestion = "REPLY_SUGGESTION";
        public const string ClearReply      = "CLEAR_REPLY";
        public const string DeleteSuggestion= "DELETE_SUGGESTION";

        // Chat
        public const string SendChat        = "SEND_CHAT";
        public const string MentionFile     = "MENTION_FILE";

        // Profile / User management
        public const string UpdateProfile   = "UPDATE_PROFILE";
        public const string UpdatePassword  = "UPDATE_PASSWORD";
        public const string AddUser         = "ADD_USER";
        public const string EditUser        = "EDIT_USER";
        public const string DeleteUser      = "DELETE_USER";
        public const string ToggleActive    = "TOGGLE_USER_ACTIVE";

        // Auth
        public const string Login           = "LOGIN";
        public const string Logout          = "LOGOUT";
        public const string SessionExpired  = "SESSION_EXPIRED";

        // ────────────────────────────────────────────────────────────────────
        // MODULE CONSTANTS
        // ────────────────────────────────────────────────────────────────────
        public const string ModFileManager  = "FileManager";
        public const string ModScanner      = "Scanner";
        public const string ModSuggestions  = "Suggestions";
        public const string ModChat         = "Chat";
        public const string ModUsers        = "Users";
        public const string ModProfile      = "Profile";
        public const string ModAuth         = "Auth";

        // ────────────────────────────────────────────────────────────────────
        // PALETTE (matches dark navy / gold theme)
        // ────────────────────────────────────────────────────────────────────
        private static readonly Color _gold    = Color.FromArgb(201, 168,  76);
        private static readonly Color _green   = Color.FromArgb( 26, 184, 112);
        private static readonly Color _red     = Color.FromArgb(224,  80,  80);
        private static readonly Color _blue    = Color.FromArgb( 59, 159, 224);
        private static readonly Color _card2   = Color.FromArgb( 17,  42,  74);
        private static readonly Color _navy    = Color.FromArgb( 11,  31,  58);
        private static readonly Color _textPri = Color.FromArgb(232, 240, 248);
        private static readonly Color _textMut = Color.FromArgb(104, 136, 164);

        // ────────────────────────────────────────────────────────────────────
        // CORE WRITER — persists to hims_audit_log
        // ────────────────────────────────────────────────────────────────────
        /// <param name="actor">The logged-in user performing the action.</param>
        /// <param name="module">One of the Mod* constants above.</param>
        /// <param name="action">One of the action constants above.</param>
        /// <param name="targetId">PK / id of the affected record (nullable).</param>
        /// <param name="targetName">Human-readable label of the target (nullable).</param>
        /// <param name="detail">Optional extra context — old→new value, file size, etc.</param>
        public static void Log(
            AdminUser actor,
            string    module,
            string    action,
            string?   targetId   = null,
            string?   targetName = null,
            string?   detail     = null)
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd  = new SqlCommand(@"
                    INSERT INTO hims_audit_log
                        (actor_id, actor_name, actor_role,
                         module,   action,
                         target_id, target_name, detail)
                    VALUES
                        (@actorId, @actorName, @actorRole,
                         @module,  @action,
                         @targetId, @targetName, @detail)", conn);

                cmd.Parameters.AddWithValue("@actorId",    actor.Id);
                cmd.Parameters.AddWithValue("@actorName",  actor.FullName);
                cmd.Parameters.AddWithValue("@actorRole",  actor.Role);
                cmd.Parameters.AddWithValue("@module",     module);
                cmd.Parameters.AddWithValue("@action",     action);
                cmd.Parameters.AddWithValue("@targetId",   (object?)targetId   ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@targetName", (object?)targetName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@detail",     (object?)detail     ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Never let an audit failure crash the application.
            }
        }

        // ────────────────────────────────────────────────────────────────────
        // UI HELPER — builds a sidebar entry Panel for ScanPatientDocsForm
        // (and any other form that renders an in-form audit strip)
        // ────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Creates a self-contained Panel that represents one audit entry
        /// for the in-form sidebar.  Add it to pnlAuditSidebarScroll at index 0
        /// so newest entries appear at the top.
        /// </summary>
        /// <param name="module">Module name (e.g. AuditLogger.ModScanner).</param>
        /// <param name="action">Action string (e.g. AuditLogger.Scan).</param>
        /// <param name="detail">Human-readable detail text.</param>
        /// <param name="width">Width of the target panel minus scrollbar (default 198).</param>
        public static Panel BuildSidebarEntry(
            string module,
            string action,
            string? detail = null,
            int    width   = 198)
        {
            var accent = GetActionAccentColor(action);

            var outer = new Panel
            {
                Width       = width,
                Height      = 58,
                BackColor   = _card2,
                Margin      = new Padding(0, 0, 0, 5),
                Padding     = new Padding(8, 6, 6, 6),
                Cursor      = Cursors.Default,
            };

            // Left accent bar
            var bar = new Panel
            {
                Width       = 3,
                Height      = 58,
                BackColor   = accent,
                Dock        = DockStyle.Left,
            };

            var lblAction = new Label
            {
                AutoSize    = false,
                Location    = new Point(10, 5),
                Size        = new Size(width - 14, 16),
                Text        = action,
                Font        = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor   = _textPri,
                BackColor   = Color.Transparent,
            };

            var lblDetail = new Label
            {
                AutoSize    = false,
                Location    = new Point(10, 22),
                Size        = new Size(width - 14, 16),
                Text        = detail ?? string.Empty,
                Font        = new Font("Segoe UI", 7.5f),
                ForeColor   = _textMut,
                BackColor   = Color.Transparent,
            };

            var lblTime = new Label
            {
                AutoSize    = false,
                Location    = new Point(10, 38),
                Size        = new Size(width - 14, 14),
                Text        = DateTime.Now.ToString("HH:mm:ss"),
                Font        = new Font("Segoe UI", 7f),
                ForeColor   = _textMut,
                BackColor   = Color.Transparent,
            };

            outer.Controls.Add(bar);
            outer.Controls.Add(lblAction);
            outer.Controls.Add(lblDetail);
            outer.Controls.Add(lblTime);
            return outer;
        }

        // ────────────────────────────────────────────────────────────────────
        // UI HELPER — returns the accent Color for a given action string
        // Used by BuildSidebarEntry and by the full audit DataGridView/table
        // ────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Maps an action constant to its sidebar accent color.
        /// Blue = scan/import  |  Gold = edit (rotate/flip/move)  |
        /// Red = delete/clear/cancel  |  Green = save/export  |  Muted = other
        /// </summary>
        public static Color GetActionAccentColor(string action) => action switch
        {
            Scan or Import or RefreshScanners or Login or Upload or BulkUpload
                => _blue,

            Rotate or Flip or MovePage or Rename or Lock or Unlock
                => _gold,

            Delete or DeletePage or ClearPages or CancelScan or DeleteUser
            or DeleteSuggestion or ToggleActive or Logout or SessionExpired
                => _red,

            SaveScan or CreateFolder or Download or ExportAudit
            or SendSuggestion or ReplySuggestion or SendChat
            or UpdateProfile or UpdatePassword or AddUser or EditUser
            or MentionFile
                => _green,

            ToggleWatermark => _gold,

            _ => _textMut,
        };

        // ────────────────────────────────────────────────────────────────────
        // UI HELPER — returns a short human-friendly label for an action
        // (e.g. for a DataGridView pill column)
        // ────────────────────────────────────────────────────────────────────
        public static string GetActionLabel(string action) => action switch
        {
            Scan            => "Scan",
            Import          => "Import",
            Rotate          => "Rotate",
            Flip            => "Flip",
            MovePage        => "Move Page",
            DeletePage      => "Delete Page",
            ClearPages      => "Clear All",
            SaveScan        => "Save",
            RefreshScanners => "Refresh",
            CancelScan      => "Cancel",
            ExportAudit     => "Export",
            CreateFolder    => "Create Folder",
            Upload          => "Upload",
            BulkUpload      => "Bulk Upload",
            Download        => "Download",
            Rename          => "Rename",
            Delete          => "Delete",
            Lock            => "Lock",
            Unlock          => "Unlock",
            PrintPreview    => "Print Preview",
            SendSuggestion  => "Send Suggestion",
            ReplySuggestion => "Reply",
            ClearReply      => "Clear Reply",
            DeleteSuggestion=> "Delete Suggestion",
            SendChat        => "Chat",
            UpdateProfile   => "Update Profile",
            UpdatePassword  => "Change Password",
            AddUser         => "Add User",
            EditUser        => "Edit User",
            DeleteUser      => "Delete User",
            ToggleActive    => "Toggle Active",
            Login           => "Login",
            Logout          => "Logout",
            SessionExpired  => "Session Expired",
            ToggleWatermark => "Toggle Watermark",
            MentionFile     => "Mentioned in Chat",
            _               => action,
        };

        // ────────────────────────────────────────────────────────────────────
        // QUERY HELPER — loads recent entries for the full audit tab
        // Returns a List<AuditEntry> so the caller can bind to a grid or
        // a ListView without duplicating the SQL.
        // ────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Fetches the most recent <paramref name="limit"/> audit rows,
        /// optionally filtered by module and/or action.
        /// Returns an empty list (never throws) on connection failure.
        /// </summary>
        public static List<AuditEntry> LoadRecent(
            int     limit   = 200,
            string? module  = null,
            string? action  = null,
            string? search  = null)
        {
            var result = new List<AuditEntry>();
            try
            {
                using var conn  = DbConfig.OpenConnection();
                var sql = @"
                    SELECT TOP (@limit) id, created_at, actor_name, actor_role,
                           module, action, target_id, target_name, detail
                    FROM   hims_audit_log
                    WHERE  (@module IS NULL OR module = @module)
                      AND  (@action IS NULL OR action = @action)
                      AND  (@search IS NULL
                            OR actor_name LIKE @searchLike
                            OR action     LIKE @searchLike
                            OR detail     LIKE @searchLike)
                    ORDER  BY id DESC";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@module",     (object?)module ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@action",     (object?)action ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@search",     (object?)search ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@searchLike", string.IsNullOrEmpty(search)
                                                               ? DBNull.Value
                                                               : (object)$"%{search}%");
                cmd.Parameters.AddWithValue("@limit", limit);

                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    result.Add(new AuditEntry
                    {
                        Id         = rdr.GetInt64(0),
                        CreatedAt  = rdr.GetDateTime(1),
                        ActorName  = rdr.GetString(2),
                        ActorRole  = rdr.GetString(3),
                        Module     = rdr.GetString(4),
                        Action     = rdr.GetString(5),
                        TargetId   = rdr.IsDBNull(6) ? null : rdr.GetString(6),
                        TargetName = rdr.IsDBNull(7) ? null : rdr.GetString(7),
                        Detail     = rdr.IsDBNull(8) ? null : rdr.GetString(8),
                    });
                }
            }
            catch
            {
                // Return whatever we have so far — never crash.
            }
            return result;
        }

        // ────────────────────────────────────────────────────────────────────
        // EXPORT HELPER — generates CSV bytes ready to write to a file
        // ────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Exports the audit log to a CSV byte array (UTF-8 with BOM).
        /// Typical usage:
        /// <code>
        ///   File.WriteAllBytes("hims_audit_log.csv", AuditLogger.ExportCsv());
        /// </code>
        /// </summary>
        public static byte[] ExportCsv(
            int     limit  = 5000,
            string? module = null,
            string? action = null)
        {
            var rows   = LoadRecent(limit, module, action);
            var sb     = new System.Text.StringBuilder();

            sb.AppendLine("ID,Timestamp,Actor,Role,Module,Action,TargetId,TargetName,Detail");

            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(",",
                    CsvCell(r.Id.ToString()),
                    CsvCell(r.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                    CsvCell(r.ActorName),
                    CsvCell(r.ActorRole),
                    CsvCell(r.Module),
                    CsvCell(r.Action),
                    CsvCell(r.TargetId   ?? ""),
                    CsvCell(r.TargetName ?? ""),
                    CsvCell(r.Detail     ?? "")));
            }

            return System.Text.Encoding.UTF8.GetPreamble()
                   .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                   .ToArray();
        }

        private static string CsvCell(string v)
            => $"\"{v.Replace("\"", "\"\"")}\"";
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DATA TRANSFER OBJECT
    // ──────────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Lightweight DTO returned by AuditLogger.LoadRecent().
    /// No EF/ORM dependency — just plain properties.
    /// </summary>
    internal sealed class AuditEntry
    {
        public long     Id         { get; init; }
        public DateTime CreatedAt  { get; init; }
        public string   ActorName  { get; init; } = "";
        public string   ActorRole  { get; init; } = "";
        public string   Module     { get; init; } = "";
        public string   Action     { get; init; } = "";
        public string?  TargetId   { get; init; }
        public string?  TargetName { get; init; }
        public string?  Detail     { get; init; }

        /// <summary>Accent color for UI pill/sidebar rendering.</summary>
        public Color AccentColor => AuditLogger.GetActionAccentColor(Action);

        /// <summary>Friendly display label.</summary>
        public string Label => AuditLogger.GetActionLabel(Action);
    }
}