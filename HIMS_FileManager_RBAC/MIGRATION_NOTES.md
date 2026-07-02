# MySQL/MariaDB → SQL Server Migration Notes

## What changed

1. **NuGet package**: `MySql.Data` → `Microsoft.Data.SqlClient` (5.2.2), in `HIMS_FileManager.csproj`.
2. **Core connection layer**:
   - `DbConfig.cs` — now uses `SqlConnection`.
   - `AppConfig.cs` — `BuildConnectionString()` now builds a SQL Server style
     connection string (`Server=host,port;Database=...;User Id=...;Password=...;
     TrustServerCertificate=True;`). Default port changed 3306 → 1433.
3. **All 14 source files** that referenced `MySqlConnection` / `MySqlCommand` /
   `MySqlDataReader` / `MySqlParameter` now use the `Microsoft.Data.SqlClient`
   equivalents (`SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlParameter`).
4. **MySQL-only SQL syntax rewritten to T-SQL**:
   - `NOW()` → `SYSDATETIME()`
   - `DATE_SUB(NOW(), INTERVAL 2 MINUTE)` → `DATEADD(MINUTE, -2, SYSDATETIME())`
   - `... LIMIT n` → `SELECT TOP (n) ...` (moved to the front of the SELECT)
   - `LIMIT @param` → `SELECT TOP (@param) ...`
   - `LAST_INSERT_ID()` → `SCOPE_IDENTITY()`
   - `RemoteAccessFixForm.cs` was rewritten from MariaDB's host-wildcard
     `CREATE USER ... @'%'` / `GRANT` model to SQL Server's `CREATE LOGIN` +
     `CREATE USER ... FOR LOGIN` + `ALTER ROLE db_owner ADD MEMBER` model,
     since SQL Server doesn't have per-host grants — access is controlled by
     the login + firewall/TCP listener instead.
5. **UI text** referencing MariaDB/XAMPP/MySQL relabeled to SQL Server
   throughout `RemoteAccessFixForm.cs` and `SettingsForm.cs`.

## What you still need to do in SSMS

1. Open `sql/hims_srs_sqlserver.sql` in SSMS and execute it (F5) against your
   target SQL Server instance — this creates the database and all 34 tables
   (`opd_file_manager`, `hims_audit_log`, `hims_chat_messages`, `admins`, etc.)
   that the app code already expects.
2. Run any `ALTER TABLE ... REBUILD WITH (DATA_COMPRESSION = PAGE)` statement
   you need separately, after the table exists.
3. Make sure **Mixed Mode Authentication** is enabled (SSMS → right-click
   server → Properties → Security → "SQL Server and Windows Authentication
   mode"), then restart the SQL Server service — SQL logins (like the app's
   `DbUser`/`DbPassword`, or the `himsopdroot` login the Remote Access tool
   creates) won't work otherwise.
4. Make sure SQL Server is listening on TCP/IP (SQL Server Configuration
   Manager → Network Configuration → enable TCP/IP) and that the Windows
   Firewall allows the port (default 1433) for LAN clients.
5. In the app's Settings screen, set Server IP, Port (1433 default), Database,
   and the SQL login credentials. Use "Fix Remote Access" inside Settings to
   auto-create the app's login and the dedicated `himsopdroot` login with
   `db_owner` rights on the database.

## Files touched
DbConfig.cs, AppConfig.cs, HIMS_FileManager.csproj, FileManagerForm.cs,
Formchat.cs, SuggestionsForm.cs, Recyclebinform.cs, Notificationservice.cs,
Userslistform.cs, RemoteAccessFixForm.cs, DashboardAdmin.cs, Auditlogform.cs,
LoginForm.cs, Updateprofileform.cs, Auditlogger.cs, RegisterUserForm.cs,
SettingsForm.cs
