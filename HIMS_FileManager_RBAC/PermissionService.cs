using System.Collections.Generic;

namespace UPLOADER
{
    /// <summary>
    /// Every distinct CRUD-level action a user can attempt, across every module.
    /// Add new actions here as the app grows — never gate a new feature with a
    /// raw role string comparison scattered in a form; add it to this enum and
    /// to the matrix in <see cref="PermissionService"/> instead.
    /// </summary>
    public enum Permission
    {
        // Files (opd_file_manager / certification_file_manager)
        File_View,
        File_Download,
        File_Upload,
        File_NewFolder,
        File_Rename,
        File_Delete,        // soft delete -> recycle bin
        File_Restore,       // restore from recycle bin
        File_PermanentDelete,
        File_Lock,

        // User management (admins table)
        User_View,
        User_Create,
        User_Update,
        User_Delete,
        User_ToggleActive,

        // App settings (hims_app_settings)
        Settings_View,
        Settings_Update,

        // Audit log
        AuditLog_View,

        // Dashboard / stats
        Dashboard_View,
    }

    /// <summary>
    /// Thrown when an action is attempted by a role that isn't permitted to
    /// perform it. Callers should catch this only to show a friendly message —
    /// never to silently continue the action.
    /// </summary>
    public sealed class PermissionDeniedException : System.Exception
    {
        public Permission Permission { get; }
        public string Role { get; }

        public PermissionDeniedException(Permission permission, string role)
            : base($"Role '{role}' is not permitted to perform '{permission}'.")
        {
            Permission = permission;
            Role = role;
        }
    }

    /// <summary>
    /// Single source of truth for "who can do what". This mirrors — and now
    /// replaces the authority of — the old scattered `_user.Role == "..."`
    /// checks that only ever controlled button *visibility*. Every CRUD method
    /// (not just the UI) must call <see cref="Require"/> before touching the
    /// database or filesystem. Hiding a button is a UX nicety; Require() is
    /// the actual gate.
    ///
    /// NOTE: this is app-layer, defense-in-depth enforcement. It stops a
    /// modified/patched client from executing code paths it shouldn't reach.
    /// It does NOT stop someone with the raw SQL Server credentials from using
    /// SSMS directly — that's what the SQL Server-level roles/logins/stored
    /// procedures (hims_rbac_sqlserver.sql) are for. Use both.
    /// </summary>
    internal static class PermissionService
    {
        private const string SuperAdmin         = "SuperAdmin";
        private const string DataManager        = "DataManager";
        private const string OPDStaff           = "OPDStaff";
        private const string CertificationStaff = "CertificationStaff";
        private const string RecordControllScan = "RecordControllScan";
        private const string StatisticianStaff  = "StatisticianStaff";
        private const string Auditor            = "Auditor";

        // Roles that may write/modify/delete files (mirrors the original
        // canWrite/canDelete/canRename flags in FileManagerForm).
        private static readonly HashSet<string> FileWriteRoles = new()
        {
            SuperAdmin, DataManager, RecordControllScan
        };

        // Roles that may only view/download files.
        private static readonly HashSet<string> FileReadOnlyRoles = new()
        {
            OPDStaff, CertificationStaff, StatisticianStaff, Auditor
        };

        private static readonly Dictionary<Permission, HashSet<string>> Matrix = new()
        {
            [Permission.File_View]            = Union(FileWriteRoles, FileReadOnlyRoles),
            [Permission.File_Download]         = Union(FileWriteRoles, FileReadOnlyRoles),
            [Permission.File_Upload]           = new HashSet<string>(FileWriteRoles),
            [Permission.File_NewFolder]        = new HashSet<string>(FileWriteRoles),
            [Permission.File_Rename]           = new HashSet<string>(FileWriteRoles),
            [Permission.File_Delete]           = new HashSet<string>(FileWriteRoles),
            [Permission.File_Restore]          = new HashSet<string>(FileWriteRoles),
            [Permission.File_PermanentDelete]  = new HashSet<string> { SuperAdmin },
            [Permission.File_Lock]             = new HashSet<string> { SuperAdmin },

            [Permission.User_View]             = new HashSet<string> { SuperAdmin },
            [Permission.User_Create]           = new HashSet<string> { SuperAdmin },
            [Permission.User_Update]           = new HashSet<string> { SuperAdmin },
            [Permission.User_Delete]           = new HashSet<string> { SuperAdmin },
            [Permission.User_ToggleActive]     = new HashSet<string> { SuperAdmin },

            [Permission.Settings_View]         = new HashSet<string> { SuperAdmin },
            [Permission.Settings_Update]       = new HashSet<string> { SuperAdmin },

            [Permission.AuditLog_View]         = new HashSet<string> { SuperAdmin, Auditor },

            [Permission.Dashboard_View]        = new HashSet<string> { SuperAdmin },
        };

        private static HashSet<string> Union(params HashSet<string>[] sets)
        {
            var result = new HashSet<string>();
            foreach (var s in sets) result.UnionWith(s);
            return result;
        }

        /// <summary>Non-throwing check — use for UI (button visibility, menu items).</summary>
        public static bool Can(AdminUser user, Permission permission)
        {
            if (user is null) return false;
            return Matrix.TryGetValue(permission, out var allowedRoles)
                   && allowedRoles.Contains(user.Role);
        }

        /// <summary>
        /// Enforcing check — call this as the FIRST line of every CRUD method,
        /// before any DB or filesystem mutation. Throws and audit-logs the
        /// attempt if the user's role isn't permitted. This is what actually
        /// stops the action; UI hiding is just a courtesy on top of it.
        /// </summary>
        public static void Require(AdminUser user, Permission permission)
        {
            if (Can(user, permission)) return;

            // Log the denied attempt itself — a role probing for access it
            // doesn't have is exactly the kind of thing an audit trail should
            // catch, even though the action never actually executed.
            AuditLogger.Log(
                user,
                AuditLogger.ModAuth,
                "PERMISSION_DENIED",
                targetId: null,
                targetName: permission.ToString(),
                detail: $"Role '{user.Role}' attempted '{permission}' and was blocked.");

            throw new PermissionDeniedException(permission, user.Role);
        }
    }
}
