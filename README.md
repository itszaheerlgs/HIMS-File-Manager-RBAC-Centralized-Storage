# HIMS File Manager — RBAC & Centralized Storage

Role-based access control enforced at **two independent layers**, plus centralized
file storage across all clients.

- Module: OPD Document Management

---

## Table of Contents

1. [Summary](#summary)
2. [Architecture](#architecture)
3. [Files changed](#files-changed)
4. [Role permission matrix](#role-permission-matrix)
5. [Deployment steps](#deployment-steps)
6. [`hims_config.json` reference](#hims_configjson-reference)
7. [Centralized file storage setup](#centralized-file-storage-setup)
8. [Verification checklist](#verification-checklist)
9. [⚠️ Known security issue — `RemoteAccessFixForm`](#️-known-security-issue--remoteaccessfixform)
10. [Other known pre-existing issue](#other-known-pre-existing-issue)
11. [Troubleshooting](#troubleshooting)

---

## Summary

1. **App layer (C#)** — every CRUD entry point calls `PermissionService.Require(...)`
   before touching the DB or filesystem, not just before showing/hiding a button.
2. **SQL Server layer** — each app role maps to its own SQL Server login with its
   own `GRANT`/`DENY` permissions, so the database itself refuses actions a role
   shouldn't be able to do — even from SSMS with valid credentials.
3. **Centralized storage** — uploaded file bytes live on a shared network path
   (`StorageRoot`), not on whichever client happened to upload them.

Previously, all clients connected using one shared login (defaulted to `sa`), role
checks only hid buttons in the UI, and uploads could end up stored locally per-client
depending on config. A decompiled/patched client, anyone with the shared credentials,
or a client with a blank `StorageRoot` could bypass restrictions or silently strand
files on one machine.

---

## Architecture

```
┌─────────────┐        1. bootstrap login (hims_auth)         ┌──────────────────┐
│   Client A   │ ─────────────────────────────────────────►   │                  │
│ (WinForms)   │        2. sp_ValidateLogin(username, pw)      │   SQL Server     │
│              │ ◄─────────────────────────────────────────   │   hims_srs       │
│              │        3. role read from admins.role          │                  │
│              │        4. reconnect as role-scoped login       │  role_file_read  │
│              │           (e.g. hims_opdstaff)                │  role_file_write │
└──────┬───────┘ ─────────────────────────────────────────►   │  role_user_admin │
       │                                                       │  role_audit_*    │
       │ 5. file bytes read/write                              └──────────────────┘
       ▼
┌─────────────────────────────┐
│  \\<server>\HIMS_Storage    │  ← shared network folder (StorageRoot)
│  yyyy/MM/dd/{guid}.ext      │     metadata only lives in SQL Server;
└─────────────────────────────┘     bytes live here, shared by all clients
```

Only metadata (`system_path`, `display_name`, `file_size`, `file_type`, timestamps)
is stored in SQL Server. File bytes are streamed to/from `StorageRoot`, bucketed by
upload date.

---

## Files changed

| File | What changed |
|---|---|
| `PermissionService.cs` **(new)** | Single source of truth: `Permission` enum + role→permission matrix + `Require()`/`Can()` |
| `FileManagerForm.cs` | Every CRUD click handler now calls `PermissionService.Require()` first. Button-visibility logic reads from the same matrix. |
| `RegisterUserForm.cs` | `btnRegister_Click` requires `User_Create` |
| `Userslistform.cs` | Update/Delete/ToggleActive gated per action (+ blocks deleting your own logged-in account) |
| `Recyclebinform.cs` | Restore requires `File_Restore`; permanent delete / empty bin requires `File_PermanentDelete` |
| `AppSettingsService.cs` | `SetBool` routes through `PermissionService.Require(Settings_Update)` |
| `AppConfig.cs` | Added `RoleLogins` / `RolePasswords` dictionaries + `hims_auth` bootstrap login as default (replaces `sa`) |
| `DbConfig.cs` | Added `SwitchToRole(role)` (reconnect using role-scoped login post-auth) and `ResetToBootstrap()` on logout |
| `LoginForm.cs` | Uses `sp_ValidateLogin` / `sp_UpdateLastLogin` stored procedures instead of raw `SELECT`/`UPDATE` on `admins`; calls `DbConfig.SwitchToRole(role)` after login |
| `FileStorage.cs` | Files stored on disk under `StorageRoot` (date-bucketed), not as DB blobs. Legacy blob rows still supported as fallback. |
| `hims_rbac_sqlserver.sql` **(new)** | Full SQL Server-side setup: logins, database roles, table/column-level grants, two auth stored procedures |

**Not changed:** `Updateprofileform.cs` — already scopes every query to
`_currentUser.Id`, so every role can rightly edit their own profile.

---

## Role permission matrix

| Role | Files | Users | Settings | Audit Log |
|---|---|---|---|---|
| SuperAdmin | Full CRUD + lock + permanent delete | Full CRUD | Read/write | Read |
| DataManager | Upload/rename/soft-delete/restore | — | — | — |
| RecordControllScan | Upload/rename/soft-delete/restore | — | — | — |
| OPDStaff | View/download only | — | — | — |
| CertificationStaff | View/download only | — | — | — |
| StatisticianStaff | View/download only | — | — | — |
| Auditor | View/download only | — | — | Read |

Everyone can edit their own profile (name, email, photo, own password) regardless
of role.

---

## Deployment steps

1. **Run `hims_rbac_sqlserver.sql`** once, connected with a login that holds the
   `sysadmin` server role, against `hims_srs`.
   - **Before running**, replace every `CHANGE_ME_...` password with a real,
     unique, strong password (8 occurrences — one per `hims_*` login).
   - A database-scoped login (even `db_owner`) is **not** sufficient — creating
     server logins requires `sysadmin` (or `securityadmin` + `CREATE LOGIN`).
2. **Set up centralized storage** — see
   [Centralized file storage setup](#centralized-file-storage-setup) below.
3. **Update `hims_config.json`** on every client (see reference below):
   - `DbUser` / `DbPassword` → the `hims_auth` bootstrap credentials
   - `RolePasswords` → matching password for each of the 7 `hims_*` role logins
   - `StorageRoot` → the shared UNC path, identical on every client
4. **Deploy the updated app build** to every client at the same time you run the
   SQL script — an old build's raw `SELECT ... FROM admins` login query fails
   once the bootstrap login's direct table access is revoked.
5. **Verify** — log in as each role once; confirm expected buttons appear/work,
   denied actions show "Access Denied," and a file uploaded from one client is
   visible from another.
6. **Only after all clients are migrated**, uncomment the last section of the SQL
   script to disable the old shared `sa`-based access path.

---

## `hims_config.json` reference

Deployed identically to every client, next to the executable.

```json
{
  "ServerIP": "<server-ip>",
  "ApachePort": 80,
  "ApacheSsl": 443,
  "MySqlPort": 1433,
  "Database": "hims_srs",

  "DbUser": "hims_auth",
  "DbPassword": "<hims_auth password>",
  "ConnTimeout": 10,

  "RoleLogins": {
    "SuperAdmin": "hims_superadmin",
    "DataManager": "hims_datamanager",
    "RecordControllScan": "hims_recordscan",
    "OPDStaff": "hims_opdstaff",
    "CertificationStaff": "hims_certstaff",
    "StatisticianStaff": "hims_statstaff",
    "Auditor": "hims_auditor"
  },

  "RolePasswords": {
    "SuperAdmin": "<hims_superadmin password>",
    "DataManager": "<hims_datamanager password>",
    "RecordControllScan": "<hims_recordscan password>",
    "OPDStaff": "<hims_opdstaff password>",
    "CertificationStaff": "<hims_certstaff password>",
    "StatisticianStaff": "<hims_statstaff password>",
    "Auditor": "<hims_auditor password>"
  },

  "StorageRoot": "\\\\<server-ip>\\HIMS_Storage"
}
```

- `RoleLogins` keys must **exactly match** the `role` value stored in `admins`
  (case-sensitive).
- If `StorageRoot` is left blank, uploads fall back to a local `HIMS_Storage`
  folder next to the executable — files then only exist on the uploading
  client's disk and are invisible to every other client. Always set it to a
  shared path in a real multi-client deployment.
- **Never commit a filled-in copy of this file to git** — it contains 8 live SQL
  credentials in plaintext. Commit a template with placeholders only (see
  `.gitignore` note below).

---

## Centralized file storage setup

File bytes are streamed to/from `StorageRoot` over SMB; only metadata goes into
SQL Server. This is a **separate access-control surface from the SQL RBAC** —
`PermissionService` only gates the app UI, not raw File Explorer access to the
share.

**On the server:**

1. Create a folder, e.g. `D:\HIMS_Storage` (prefer a dedicated data drive over
   `C:\`).
2. Share it (Properties → Sharing → Advanced Sharing), e.g. as `HIMS_Storage`.
3. Set **share permissions** and matching **NTFS permissions** to a specific
   group of staff accounts — avoid `Everyone: Full Control`.
4. Resulting UNC path, e.g.: `\\<server-ip>\HIMS_Storage`

**On every client**, set `StorageRoot` in `hims_config.json` to that UNC path.

**Verify:**

- Manually create/delete a test file at the UNC path from a client via File
  Explorer, to confirm share/NTFS permissions work before the app touches it.
- Upload a file from Client A in the app; confirm Client B can download/preview
  the same file.

---

## Verification checklist

- [ ] All 8 `hims_*` server logins exist (`SELECT name FROM sys.server_principals WHERE name LIKE 'hims_%'`)
- [ ] All 8 database users exist and are mapped to the correct database roles
- [ ] `sp_ValidateLogin` / `sp_UpdateLastLogin` exist and `hims_auth` can `EXECUTE` them
- [ ] Each of the 7 roles can log in successfully end-to-end
- [ ] A `DataManager`/`RecordControllScan` account can upload/rename/soft-delete but
      **cannot** permanently delete or lock a file
- [ ] A view-only role (OPDStaff/CertificationStaff/StatisticianStaff) cannot see
      write actions in the UI, and a forced write attempt is rejected at the DB level
- [ ] Only SuperAdmin/Auditor can open the Audit Log
- [ ] `hims_audit_log` is receiving new rows for test actions
- [ ] A file uploaded from one client is visible/downloadable from a different client
      (confirms `StorageRoot` is shared, not local)

---

## ⚠️ Known security issue — `RemoteAccessFixForm`

`RemoteAccessFixForm.cs`, launched from a **"Fix Remote Access" button in
Settings**, currently:

- Grants **`db_owner`** on `hims_srs` to whatever login is in the app's current
  `DbUser` config (e.g. `hims_auth`) — full control, bypassing every GRANT/DENY
  set up by `hims_rbac_sqlserver.sql`.
- Hardcodes and recreates a second login, **`himsopdroot`**, with a **hardcoded
  password**, also granted `db_owner`.
- Sets `CHECK_POLICY = OFF` on both logins, disabling password complexity/lockout.
- Prints the plaintext password to the on-screen log.
- **Has no `PermissionService.Require()` gate** on the triggering button — any
  user who can reach Settings can run it.

This defeats the entire RBAC effort described above. **Recommended before any
wider rollout:** remove this form and its Settings button entirely, or at minimum
gate it behind SuperAdmin and restrict it to a one-time, manually-run setup tool
that isn't shipped in the general client build.

---

## Other known pre-existing issue

`admins.real_password` stores the plaintext password alongside the bcrypt hash,
and login accepts a plaintext match against it. This predates the RBAC work and
isn't something the permission system fixes — it's a data-at-rest exposure
regardless of role. Worth a follow-up to drop `real_password` entirely and rely
on the bcrypt hash only, once confirmed nothing else in the app still reads it.

---

## Troubleshooting

**`Login failed for user 'hims_auth'` (or any `hims_*` login) in SSMS or the app**
- Confirm the password in `hims_config.json` matches exactly what was set in
  `hims_rbac_sqlserver.sql`.
- Check for a lockout: `SELECT LOGINPROPERTY('hims_auth', 'IsLocked')`.
- Reset directly if in doubt:
  `ALTER LOGIN hims_auth WITH PASSWORD = '<new password>', CHECK_POLICY = ON;`

**`Msg 15247` / `Msg 15007` / `Msg 15151` when running the SQL script**
- These cascade from one root cause: the connected login lacks the `sysadmin`
  server role. Check with:
  `SELECT IS_SRVROLEMEMBER('sysadmin')`.
- Reconnect using a Windows Authentication account that is sysadmin (commonly a
  local admin on the box that installed SQL Server), or have someone with
  sysadmin rights run the script.

**App login succeeds for one role but fails when switching roles**
- `RolePasswords` in `hims_config.json` is likely blank or mismatched for that
  role — `DbConfig.SwitchToRole` opens a second connection using that password.

**Files uploaded on one PC aren't visible on another**
- `StorageRoot` is blank or points to a local path instead of a shared UNC path.
  See [Centralized file storage setup](#centralized-file-storage-setup).
