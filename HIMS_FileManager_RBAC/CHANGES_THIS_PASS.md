# What changed in this pass

## 0. Designer bug fix — "Missing Form SubType" / "View Designer" disappearing

**Root cause found in `DashboardAdmin.Designer.cs` and `LoginForm.Designer.cs`:**
Both files had real logic — custom method calls, lambdas, LINQ (`.OfType<TextBox>().Any(...)`),
loops, conditionals — living either *inside* `InitializeComponent()` or elsewhere in the
`.Designer.cs` partial. The WinForms out-of-process designer's CodeDom-based reader only
understands a narrow, linear subset of C# (object construction + simple property/field
assignment); anything beyond that makes it fail to host the form, and VS silently drops
the "View Designer" option / SubType metadata for that file — even though the project
still compiles and runs fine.

**Fix:** moved all non-trivial logic out of both `.Designer.cs` files and into their
code-behind `.cs` files, to run *after* `InitializeComponent()` returns:

- `DashboardAdmin.Designer.cs` → `ConfigureCard()`, `Blend()`, `ApplyRoundedRegion()`,
  and the 9 `ConfigureCard(...)` calls + `ApplyRoundedRegion(...)` call that used to sit
  inside `InitializeComponent()`, moved to a new `DashboardAdmin.cs` → `ConfigureDashboardCards()`,
  called once from the constructor right after `InitializeComponent()`.
- `LoginForm.Designer.cs` → `PnlCard_Paint`, `PnlInput_Paint` (the one with the LINQ
  `.OfType<TextBox>().Any(...)`), `BtnLogin_MouseEnter`/`BtnLogin_MouseLeave` moved to
  `LoginForm.cs`. The `+=` event-subscription lines (plain method-group references) are
  designer-safe and stay in the `.Designer.cs`.

Visual output/behavior is identical — same colors, same hover effects, same rounded
badge — just executed one call-frame later. Both forms should show "View Designer" again.

## 1. Inside-folder search (not overall search)

- New checkbox **"Search this folder only"** next to the search box in `FileManagerForm`
  (`chkSearchInFolder`), **checked by default**.
- When checked, `RefreshGrid(search)` uses a recursive CTE (`WITH FolderTree AS (...)`)
  to match only the current folder's direct children **and every descendant beneath it**,
  instead of `LIKE`-matching `display_name` across the entire `opd_file_manager` table.
- Uncheck the box to fall back to the old whole-system search. Toggling it re-runs the
  current search immediately.

## 2. Download/print watermark (username + timestamp), SuperAdmin-only toggle

This was already implemented in the codebase (`WatermarkService.cs`, `AppSettingsService.cs`,
the SuperAdmin-only toggle in `SettingsForm.cs`, and the download watermarking in
`FileManagerForm.btnDownload_Click`) — two gaps were closed:

- **Print watermarking was missing** — `PrintImagePage` now overlays the same
  `WatermarkService.DrawOverlay` (diagonal tiled text + solid footer bar, "Full Name
  (username) • yyyy-MM-dd HH:mm") on the printed page when the SuperAdmin toggle is on,
  matching what downloads already did.
- **`hims_app_settings` table didn't exist in the schema** — added in
  `db/feature_additions.sql` (see below), since `AppSettingsService` reads/writes it.

Only image files (jpg/png/bmp/gif/tiff/webp) are pixel-watermarked — that's this
system's primary document format (scanned docs, X-rays). Non-image downloads are still
fully covered by the existing audit log.

## 3. Bulk upload progress + cancel

Already fully implemented (`_uploadCts` `CancellationTokenSource`, `btnCancelUpload`,
`ShowProgress`/`HideProgress`, cooperative cancellation checked between files in
`UploadFilesBatch`/`BulkUploadFolderOptimized`). No changes needed — verified working.

## 4. @mention chat notifications tied to files/folders

New end-to-end feature:

- New toolbar button **"@"** (`btnMentionChat`) in `FileManagerForm` — select a file or
  folder, click it, and Chat opens with a reference tag (`[📁 FolderName]` /
  `[📄 FileName]`) pre-filled in the compose box. Type `@username your message` and send.
- `FormChat.SendMessage()` parses `@username` tokens out of the message, resolves them
  against `admins.username`, and (skipping unknown names and self-mentions) inserts one
  row per mentioned user into the new `hims_chat_mentions` table, carrying the attached
  file/folder reference if the tag is still present in the sent text.
- `NotificationService` gained `CheckNewMentions` (polled alongside suggestions/chat) and
  a `NewMentions` event: `(count, fromWho, attachedItemId, attachedItemName, isFolder)`.
- `FileManagerForm` shows a **"@ You were mentioned"** toast; clicking it calls the new
  `NavigateToItem(id, isFolder)` — jumps straight into the containing folder (or into the
  folder itself, if the mention was a folder) and highlights the file's row — then opens
  Chat.
- Opening Chat marks all of your unread mentions as read (`MarkMyMentionsRead`).
- New audit action `MENTION_FILE` (constant already existed in `Auditlogger.cs` — now
  actually used) logs who mentioned whom about which file/folder.

## Database migration required

Run **`db/feature_additions.sql`** against `hims_srs` after the existing
`db/hims_srs_sqlserver.sql` (safe/idempotent — every statement is existence-guarded).
It adds:

- `hims_app_settings` (key/value settings store — watermark toggle; defaults to OFF)
- `hims_chat_mentions` (@mention notifications, optionally tied to `opd_file_manager`)

Nothing else changed in the schema.
