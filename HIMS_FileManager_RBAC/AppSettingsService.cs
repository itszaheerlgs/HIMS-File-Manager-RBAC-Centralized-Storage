using Microsoft.Data.SqlClient;

namespace UPLOADER
{
    /// <summary>
    /// Small DB-backed key/value settings store, backed by the
    /// hims_app_settings table (see db/feature_additions.sql).
    /// Right now this only holds the SuperAdmin-only download/print
    /// watermark toggle, but it's generic enough to grow.
    ///
    /// Reads are cached for a few seconds so every download/print doesn't
    /// need its own round trip, while still picking up Settings changes
    /// almost immediately.
    /// </summary>
    internal static class AppSettingsService
    {
        public const string WatermarkEnabledKey = "watermark_enabled";

        private const int CacheMs = 5000;
        private static readonly Dictionary<string, (string Value, DateTime Expires)> _cache = new();
        private static readonly object _lock = new();

        public static bool GetBool(string key, bool defaultValue = false)
        {
            string? raw = GetString(key, null);
            if (raw == null) return defaultValue;
            return raw == "1" || raw.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public static string? GetString(string key, string? defaultValue = null)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var cached) && cached.Expires > DateTime.UtcNow)
                    return cached.Value;
            }

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "SELECT setting_value FROM hims_app_settings WHERE setting_key = @k", conn);
                cmd.Parameters.AddWithValue("@k", key);
                object? result = cmd.ExecuteScalar();
                string? value = (result == null || result == DBNull.Value) ? null : result.ToString();

                lock (_lock)
                {
                    _cache[key] = (value ?? defaultValue ?? "", DateTime.UtcNow.AddMilliseconds(CacheMs));
                }
                return value ?? defaultValue;
            }
            catch
            {
                // hims_app_settings not migrated yet, or DB unreachable — fall back
                // to the default rather than throwing from inside a download/print click.
                return defaultValue;
            }
        }

        /// <summary>Gated write — throws PermissionDeniedException if the caller's role can't change settings.</summary>
        public static void SetBool(string key, bool value, AdminUser changedBy)
        {
            PermissionService.Require(changedBy, Permission.Settings_Update);

            string strVal = value ? "1" : "0";

            using (var conn = DbConfig.OpenConnection())
            using (var cmd = new SqlCommand(@"
                MERGE hims_app_settings AS target
                USING (SELECT @k AS setting_key) AS src
                ON target.setting_key = src.setting_key
                WHEN MATCHED THEN
                    UPDATE SET setting_value = @v, updated_by = @by, updated_at = SYSDATETIME()
                WHEN NOT MATCHED THEN
                    INSERT (setting_key, setting_value, updated_by, updated_at)
                    VALUES (@k, @v, @by, SYSDATETIME());", conn))
            {
                cmd.Parameters.AddWithValue("@k", key);
                cmd.Parameters.AddWithValue("@v", strVal);
                cmd.Parameters.AddWithValue("@by", changedBy.FullName);
                cmd.ExecuteNonQuery();
            }

            lock (_lock)
            {
                _cache[key] = (strVal, DateTime.UtcNow.AddMilliseconds(CacheMs));
            }

            AuditLogger.Log(changedBy, AuditLogger.ModFileManager, AuditLogger.ToggleWatermark,
                targetName: key, detail: $"Set to {(value ? "ON" : "OFF")}");
        }
    }
}
