using System;
using System.Collections.Generic;
using System.IO;

namespace UPLOADER
{
    /// <summary>
    /// Handles physical storage of uploaded files on disk instead of inside the
    /// database. Only a small relative path string + metadata (file_size,
    /// file_type, etc.) is stored in opd_file_manager.system_path — the actual
    /// bytes live under <see cref="Root"/>, bucketed by upload date so no
    /// single folder ever ends up with tens of thousands of files in it.
    ///
    /// This replaces the old design where every file's bytes were stored
    /// directly in opd_file_manager.file_data (VARBINARY(MAX)), which caused
    /// the table to balloon to tens of GB and made every query against it
    /// (even unrelated ones) slower, backups huge, and large uploads prone to
    /// long-held transactions / out-of-memory errors.
    ///
    /// Existing rows that still have data in file_data (uploaded before this
    /// change) keep working — Download/Preview/Print fall back to reading the
    /// blob column when system_path is empty. New uploads always go to disk.
    /// </summary>
    internal static class FileStorage
    {
        private const int CopyBufferSize = 1024 * 1024; // 1 MB — good balance for large files

        /// <summary>Root folder where all uploaded files are physically stored.</summary>
        public static string Root
        {
            get
            {
                var cfg = AppConfig.Load();
                string root = string.IsNullOrWhiteSpace(cfg.StorageRoot)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HIMS_Storage")
                    : cfg.StorageRoot;
                Directory.CreateDirectory(root);
                return root;
            }
        }

        /// <summary>
        /// Streams a local source file into the managed storage tree under a
        /// date-bucketed subfolder (Root/yyyy/MM/dd/{guid}{ext}) and returns the
        /// path to store in the DB (relative to Root, so the whole tree can be
        /// moved to a new drive/server later just by updating StorageRoot).
        /// Uses a buffered stream copy rather than loading the whole file into
        /// memory, so this is safe for very large files.
        /// </summary>
        public static string SaveFile(string sourcePath, string displayName)
        {
            string bucket = Path.Combine(
                DateTime.Now.Year.ToString(),
                DateTime.Now.Month.ToString("D2"),
                DateTime.Now.Day.ToString("D2"));

            string destDir = Path.Combine(Root, bucket);
            Directory.CreateDirectory(destDir);

            // GUID prefix avoids collisions between files that share a display name;
            // the original name is kept separately in opd_file_manager.display_name.
            string ext = Path.GetExtension(displayName);
            string uniqueName = Guid.NewGuid().ToString("N") + ext;
            string destPath = Path.Combine(destDir, uniqueName);

            using (var src = new FileStream(sourcePath, FileMode.Open, FileAccess.Read,
                       FileShare.Read, CopyBufferSize, FileOptions.SequentialScan))
            using (var dst = new FileStream(destPath, FileMode.CreateNew, FileAccess.Write,
                       FileShare.None, CopyBufferSize))
            {
                src.CopyTo(dst, CopyBufferSize);
            }

            return Path.Combine(bucket, uniqueName);
        }

        /// <summary>Full physical path for a relative path stored in the DB.</summary>
        public static string FullPath(string relativePath) => Path.Combine(Root, relativePath);

        /// <summary>Streams a stored file out to a destination chosen by the user (download).</summary>
        public static void CopyOut(string relativePath, string destinationPath)
        {
            string src = FullPath(relativePath);
            using var s = new FileStream(src, FileMode.Open, FileAccess.Read,
                FileShare.Read, CopyBufferSize, FileOptions.SequentialScan);
            using var d = new FileStream(destinationPath, FileMode.Create, FileAccess.Write,
                FileShare.None, CopyBufferSize);
            s.CopyTo(d, CopyBufferSize);
        }

        /// <summary>Opens a stored file for reading (e.g. for image preview/printing).</summary>
        public static FileStream OpenRead(string relativePath) =>
            new(FullPath(relativePath), FileMode.Open, FileAccess.Read, FileShare.Read);

        /// <summary>Deletes a physical file, ignoring errors (missing file, locked file, etc).</summary>
        public static void TryDelete(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;
            try
            {
                string full = FullPath(relativePath);
                if (File.Exists(full)) File.Delete(full);
            }
            catch
            {
                // Best-effort only — a locked/in-use file shouldn't block DB cleanup.
                // It can be picked up later by a storage reconciliation pass.
            }
        }

        // ── Priority 1: free-space guard ───────────────────────────────────
        /// <summary>
        /// Returns the number of bytes currently free on the drive that hosts
        /// <see cref="Root"/>. Used to pre-check available space before a large
        /// upload starts instead of letting the copy fail/freeze partway through.
        /// </summary>
        public static long GetFreeSpaceBytes()
        {
            string root = Root; // ensures the folder exists and resolves config
            string driveRoot = Path.GetPathRoot(Path.GetFullPath(root))!;
            var drive = new DriveInfo(driveRoot);
            return drive.AvailableFreeSpace;
        }

        /// <summary>
        /// Throws an <see cref="IOException"/> with a clear message if there is not
        /// enough free space on the storage drive to safely hold
        /// <paramref name="requiredBytes"/> worth of new files. A small safety
        /// margin (<paramref name="safetyMarginBytes"/>, default 500 MB) is kept
        /// free on top of the literal byte requirement so the OS/DB never runs the
        /// drive bone dry.
        /// </summary>
        public static void EnsureFreeSpace(long requiredBytes, long safetyMarginBytes = 500L * 1024 * 1024)
        {
            long free = GetFreeSpaceBytes();
            long needed = requiredBytes + safetyMarginBytes;
            if (free < needed)
            {
                string freeStr = FormatBytes(free);
                string neededStr = FormatBytes(needed);
                throw new IOException(
                    $"Not enough free disk space on the storage drive.\n\n" +
                    $"Required (incl. safety margin): {neededStr}\n" +
                    $"Available: {freeStr}\n\n" +
                    $"Free up space or point StorageRoot to a drive with more room before retrying this upload.");
            }
        }

        private static string FormatBytes(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double size = bytes;
            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }
            return $"{size:0.##} {units[unit]}";
        }

        // ── Priority 1/2: resilient save with automatic orphan cleanup ─────
        /// <summary>
        /// Same as <see cref="SaveFile"/>, but if anything goes wrong partway through
        /// the copy (disk full mid-write, drive disconnected, app closed, etc.) the
        /// partially-written destination file is deleted immediately instead of being
        /// left behind as an orphan on disk. Use this from upload pipelines; the plain
        /// <see cref="SaveFile"/> is kept for callers that already do their own cleanup.
        /// </summary>
        public static string SaveFileSafe(string sourcePath, string displayName)
        {
            string bucket = Path.Combine(
                DateTime.Now.Year.ToString(),
                DateTime.Now.Month.ToString("D2"),
                DateTime.Now.Day.ToString("D2"));

            string destDir = Path.Combine(Root, bucket);
            Directory.CreateDirectory(destDir);

            string ext = Path.GetExtension(displayName);
            string uniqueName = Guid.NewGuid().ToString("N") + ext;
            string destPath = Path.Combine(destDir, uniqueName);

            try
            {
                using (var src = new FileStream(sourcePath, FileMode.Open, FileAccess.Read,
                           FileShare.Read, CopyBufferSize, FileOptions.SequentialScan))
                using (var dst = new FileStream(destPath, FileMode.CreateNew, FileAccess.Write,
                           FileShare.None, CopyBufferSize))
                {
                    src.CopyTo(dst, CopyBufferSize);
                }

                return Path.Combine(bucket, uniqueName);
            }
            catch
            {
                // Clean up the partial file immediately so a failed/interrupted
                // upload never leaves an orphaned half-written file on disk.
                try { if (File.Exists(destPath)) File.Delete(destPath); } catch { /* best effort */ }
                throw;
            }
        }

        // ── Priority 3: storage reconciliation ──────────────────────────────
        /// <summary>
        /// Scans the entire storage tree under <see cref="Root"/> and returns the
        /// set of relative paths (in the same format stored in
        /// opd_file_manager.system_path) for every physical file found. The caller
        /// (a reconciliation job) compares this against system_path values in the
        /// database to find orphaned files on disk and DB rows whose file is
        /// missing — making backups and storage trustworthy.
        /// </summary>
        public static List<string> ListAllStoredRelativePaths()
        {
            var result = new List<string>();
            string root = Root;
            if (!Directory.Exists(root)) return result;

            foreach (string full in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(root, full);
                result.Add(rel);
            }
            return result;
        }

        /// <summary>True if a relative path stored in the DB actually exists on disk.</summary>
        public static bool Exists(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return false;
            return File.Exists(FullPath(relativePath));
        }
    }
}
