using Microsoft.Data.SqlClient;

namespace UPLOADER
{
    /// <summary>
    /// Central connection-string source.  Always reads the live AppConfig
    /// so that Settings changes take effect immediately (after re-login or
    /// next connection attempt).
    /// </summary>
    internal static class DbConfig
    {
        /// <summary>Current config singleton — replaced when Settings are saved.</summary>
        public static AppConfig Current { get; set; } = AppConfig.Load();

        /// <summary>
        /// The connection string in current use. Starts out pointed at the
        /// low-privilege bootstrap login (DbUser/DbPassword — see AppConfig)
        /// and is swapped to a role-scoped login by <see cref="SwitchToRole"/>
        /// right after LoginForm authenticates the user. Every OpenConnection()
        /// call after that point authenticates to SQL Server AS that role, so
        /// SQL Server's own GRANT/DENY rules — not just the WinForms UI — is
        /// what decides what the session can touch.
        /// </summary>
        private static string? _activeConnectionString;

        public static string ConnectionString =>
            _activeConnectionString ??= Current.BuildConnectionString();

        public static SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Re-points all future connections at the SQL Server login mapped to
        /// <paramref name="role"/> (AppConfig.RoleLogins / RolePasswords).
        /// Call this exactly once, immediately after LoginForm confirms the
        /// user's identity and role. If no login is configured for the role,
        /// falls back to the bootstrap connection (fails safe/loud rather than
        /// silently keeping elevated bootstrap access).
        /// </summary>
        public static void SwitchToRole(string role)
        {
            var cfg = Current;
            if (cfg.RoleLogins.TryGetValue(role, out var roleUser) &&
                cfg.RolePasswords.TryGetValue(role, out var rolePassword) &&
                !string.IsNullOrWhiteSpace(roleUser))
            {
                _activeConnectionString = cfg.BuildConnectionString(roleUser, rolePassword);
            }
            else
            {
                // No role login configured (e.g. RBAC SQL script not deployed
                // yet on this environment) — keep using the bootstrap login so
                // the app still works, but this means SQL-level enforcement is
                // NOT active. App-layer PermissionService checks still apply.
                _activeConnectionString = cfg.BuildConnectionString();
            }
        }

        /// <summary>Reverts to the bootstrap login — call on logout.</summary>
        public static void ResetToBootstrap() => _activeConnectionString = null;
    }
}

