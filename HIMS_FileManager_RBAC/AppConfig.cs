using System.Collections.Generic;
using System.Text.Json;

namespace UPLOADER
{
    /// <summary>
    /// Persisted LAN connection settings stored in hims_config.json
    /// next to the executable.
    /// </summary>
    internal class AppConfig
    {
        // ── Defaults ────────────────────────────────────────────────────────
        public string ServerIP    { get; set; } = "localhost";
        public int    ApachePort  { get; set; } = 80;
        public int    ApacheSsl   { get; set; } = 443;
        public int    MySqlPort   { get; set; } = 1433; // SQL Server default port
        public string Database    { get; set; } = "hims_srs";

        // ── Bootstrap login ─────────────────────────────────────────────────
        // Used ONLY before a user's role is known — i.e. the moment the login
        // screen validates username/password against admins. This account
        // must be low-privilege at the SQL Server level: EXECUTE on
        // dbo.sp_ValidateLogin only (see hims_rbac_sqlserver.sql). It must
        // NOT have direct SELECT on the admins table or any file tables.
        public string DbUser      { get; set; } = "hims_auth";
        public string DbPassword  { get; set; } = "Xk7$mQp2vLr9!Wdz";
        public int    ConnTimeout { get; set; } = 10;

        // ── Per-role SQL Server logins ──────────────────────────────────────
        // Once LoginForm knows the authenticated user's role, the app opens a
        // NEW connection using the SQL Server login mapped to that role
        // (see DbConfig.SwitchToRole). SQL Server itself then enforces what
        // that connection can touch via GRANT/DENY on the matching database
        // role — independent of anything the WinForms client code does.
        // The dictionary key must exactly match the `role` value in admins.
        public Dictionary<string, string> RoleLogins { get; set; } = new()
        {
            ["SuperAdmin"]         = "hims_superadmin",
            ["DataManager"]        = "hims_datamanager",
            ["RecordControllScan"] = "hims_recordscan",
            ["OPDStaff"]           = "hims_opdstaff",
            ["CertificationStaff"] = "hims_certstaff",
            ["StatisticianStaff"]  = "hims_statstaff",
            ["Auditor"]            = "hims_auditor",
        };

        public Dictionary<string, string> RolePasswords { get; set; } = new()
        {
            ["SuperAdmin"]         = "Tn4#hBv8xRq1@Fyc",
            ["DataManager"]        = "Gp9&wLk3zNm6!Vte",
            ["RecordControllScan"] = "Rc2$dXq7yTb4#Hjw",
            ["OPDStaff"]           = "Vy8!fKm1sQz5&Nbr",
            ["CertificationStaff"] = "Lw3#pJv9tCk6@Zqx",
            ["StatisticianStaff"]  = "Fq6&mRz2hLb8!Xdy",
            ["Auditor"]            = "Bt5$vNq4wGp7#Klm",
        };

        // ── File storage ────────────────────────────────────────────────────
        // Root folder where uploaded files are physically stored on disk
        // (instead of inside the database as VARBINARY(MAX) blobs). Point this
        // at a dedicated drive/NAS share for large deployments. Leave blank to
        // use a "HIMS_Storage" folder next to the executable.
        public string StorageRoot { get; set; } = "";

        // ── File path ───────────────────────────────────────────────────────
        private static readonly string _path = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "hims_config.json");

        // ── Load / Save ─────────────────────────────────────────────────────
        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(_path))
                {
                    string json = File.ReadAllText(_path);
                    return JsonSerializer.Deserialize<AppConfig>(json)
                           ?? new AppConfig();
                }
            }
            catch { /* fall through to default */ }
            return new AppConfig();
        }

        public void Save()
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_path, JsonSerializer.Serialize(this, opts));
        }

        // ── Build SQL Server connection string ─────────────────────────────
        public string BuildConnectionString()
            => BuildConnectionString(DbUser, DbPassword);

        /// <summary>Builds a connection string for an explicit login — used to
        /// switch to the role-scoped SQL Server login after authentication.</summary>
        public string BuildConnectionString(string user, string password)
        {
            // Server can be "host" or "host\\InstanceName" — MySqlPort is
            // reused here as the SQL Server TCP port (default 1433).
            return $"Server={ServerIP},{MySqlPort};Database={Database};" +
                   $"User Id={user};Password={password};" +
                   $"TrustServerCertificate=True;" +
                   $"Encrypt=False;" +
                   $"Connect Timeout={ConnTimeout};" +
                   $"Pooling=true;Min Pool Size=0;";
        }
    }
}
