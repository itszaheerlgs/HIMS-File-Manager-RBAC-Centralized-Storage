using Microsoft.Data.SqlClient;

namespace UPLOADER
{
    /// <summary>
    /// Polls the database for new suggestions, suggestion replies, and chat messages,
    /// and raises events so a form can show a toast / update toolbar badges.
    /// Counts are session-relative: only items created/updated after this service
    /// started (or after the last successful poll) are reported.
    /// </summary>
    public class NotificationService
    {
        private readonly AdminUser _user;
        private readonly System.Windows.Forms.Timer _timer = new();
        private DateTime _lastSuggestionCheck;
        private DateTime _lastReplyCheck;
        private DateTime _lastChatCheck;
        private DateTime _lastMentionCheck;

        public event Action<int>? NewSuggestions;      // count of new suggestions (SuperAdmin only)
        public event Action<int>? NewSuggestionReplies; // count of new replies to *my* suggestions
        /// <summary>
        /// Fired when new chat messages arrive.
        /// Args: (count, senderName, isPrivate)
        /// senderName = name of the person who sent (or "multiple people" if >1 sender)
        /// isPrivate  = true when all new messages are DMs to me; false if any are public
        /// </summary>
        public event Action<int, string, bool>? NewChatMessages;

        /// <summary>
        /// Fired when someone @mentions me in chat. Args: (count, fromWho, attachedItemId,
        /// attachedItemName, attachedIsFolder). attachedItemId is null when the mention
        /// wasn't tied to a specific file/folder. Only the single most recent mention's
        /// attachment is surfaced when count > 1 — click-through opens Chat either way.
        /// </summary>
        public event Action<int, string, int?, string?, bool>? NewMentions;

        public NotificationService(AdminUser user, int pollIntervalMs = 15000)
        {
            _user = user;
            var now = DateTime.Now;
            _lastSuggestionCheck = now;
            _lastReplyCheck = now;
            _lastChatCheck = now;
            _lastMentionCheck = now;

            _timer.Interval = pollIntervalMs;
            _timer.Tick += (s, e) => Poll();
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        private void Poll()
        {
            SqlConnection? conn = null;
            try
            {
                conn = DbConfig.OpenConnection();
            }
            catch
            {
                // Can't even open a connection — nothing to do this cycle.
                return;
            }

            using (conn)
            {
                // Each check runs in its own try/catch so a bug or transient error in
                // one (e.g. chat) can never silently disable the others (e.g. suggestions).
                try
                {
                    if (_user.Role == "SuperAdmin")
                        CheckNewSuggestions(conn);
                    else
                        CheckSuggestionReplies(conn);
                }
                catch { /* non-critical — skip suggestions this cycle */ }

                try
                {
                    CheckNewChatMessages(conn);
                }
                catch { /* non-critical — skip chat this cycle */ }

                try
                {
                    CheckNewMentions(conn);
                }
                catch { /* non-critical — skip mentions this cycle (table may not be migrated yet) */ }
            }
        }

        private void CheckNewSuggestions(SqlConnection conn)
        {
            // Set up a snapshot threshold right now 
            var currentCheckMoment = DateTime.Now;

            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) 
                FROM   hims_suggestions 
                WHERE  created_at > @t", conn);
            cmd.Parameters.AddWithValue("@t", _lastSuggestionCheck);

            int count = Convert.ToInt32(cmd.ExecuteScalar());

            // 🌟 CRITICAL FIX: Always advance the timestamp immediately 
            // so the next tick 15 seconds from now only looks for brand new entries.
            _lastSuggestionCheck = currentCheckMoment;

            if (count > 0)
            {
                PlayNotificationSound();
                NewSuggestions?.Invoke(count);
            }
        }

        private void CheckSuggestionReplies(SqlConnection conn)
        {
            var checkpoint = _lastReplyCheck;
            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM hims_suggestions
                WHERE  user_name = @me
                       AND super_message IS NOT NULL AND super_message <> ''
                       AND replied_at > @t", conn);
            cmd.Parameters.AddWithValue("@me", _user.FullName);
            cmd.Parameters.AddWithValue("@t", checkpoint);

            int count;
            try { count = Convert.ToInt32(cmd.ExecuteScalar()); }
            catch (SqlException)
            {
                // replied_at column not present yet (migration not applied) — skip silently.
                _lastReplyCheck = DateTime.Now;
                return;
            }

            _lastReplyCheck = DateTime.Now;
            if (count > 0) NewSuggestionReplies?.Invoke(count);
        }

        private void CheckNewChatMessages(SqlConnection conn)
        {
            var checkpoint = _lastChatCheck;
            using var cmd = new SqlCommand(@"
                SELECT sender_name,
                       CASE WHEN recipient_id IS NULL THEN 1 ELSE 0 END AS is_public
                FROM   hims_chat_messages
                WHERE  created_at > @t
                       AND sender_id <> @me
                       AND (recipient_id = @me OR recipient_id IS NULL)
                ORDER  BY id ASC", conn);
            cmd.Parameters.AddWithValue("@t", checkpoint);
            cmd.Parameters.AddWithValue("@me", _user.Id);

            var senders = new List<string>();
            bool anyPublic = false;

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                string sn = r.GetString("sender_name");
                if (!senders.Contains(sn)) senders.Add(sn);
                if (Convert.ToBoolean(r["is_public"])) anyPublic = true;
            }
            r.Close();

            _lastChatCheck = DateTime.Now;
            if (senders.Count == 0) return;

            // 👇 PUT THIS HERE
            PlayNotificationSound();

            string senderLabel = senders.Count == 1 ? senders[0] : "multiple people";
            bool isPrivate = !anyPublic;
            NewChatMessages?.Invoke(senders.Count, senderLabel, isPrivate);
        }

        // Looks for unread @mentions of me created since the last poll. Reports
        // the most recent one's file/folder attachment (if any) for the toast's
        // click-through; older unread mentions are still all counted, and all
        // get marked read together once the person opens Chat and sees them.
        private void CheckNewMentions(SqlConnection conn)
        {
            var checkpoint = _lastMentionCheck;
            using var cmd = new SqlCommand(@"
                SELECT TOP (1) mentioned_by_name, attached_item_id, attached_item_name, attached_is_folder,
                       (SELECT COUNT(*) FROM hims_chat_mentions
                        WHERE mentioned_admin_id = @me AND created_at > @t) AS total
                FROM   hims_chat_mentions
                WHERE  mentioned_admin_id = @me AND created_at > @t
                ORDER  BY id DESC", conn);
            cmd.Parameters.AddWithValue("@me", _user.Id);
            cmd.Parameters.AddWithValue("@t", checkpoint);

            string? fromWho = null;
            int? itemId = null;
            string? itemName = null;
            bool isFolder = false;
            int total = 0;

            using (var r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    fromWho = r.GetString("mentioned_by_name");
                    itemId = r.IsDBNull(r.GetOrdinal("attached_item_id")) ? null : r.GetInt32("attached_item_id");
                    itemName = r.IsDBNull(r.GetOrdinal("attached_item_name")) ? null : r.GetString("attached_item_name");
                    isFolder = !r.IsDBNull(r.GetOrdinal("attached_is_folder")) && r.GetBoolean("attached_is_folder");
                    total = Convert.ToInt32(r["total"]);
                }
            }

            _lastMentionCheck = DateTime.Now;
            if (total == 0 || fromWho == null) return;

            PlayNotificationSound();
            NewMentions?.Invoke(total, fromWho, itemId, itemName, isFolder);
        }

        // ── NuGet Audio Player (NAudio Engine) ────────────────────────────────
        // The notification sound is embedded as a resource inside this assembly
        // (Build Action = "Embedded Resource" on the .mp3 file), so it travels
        // with the exe/dll wherever it's copied — no external file path needed,
        // and nothing to forget when distributing to other machines.
        //
        // Resource name = "<DefaultNamespace>.<FolderPath>.<FileName>" with dots
        // replacing path separators. If you put the file at Resources/notifyms.mp3
        // inside the UPLOADER project, the name below is correct. If your actual
        // default namespace or folder differs, update SoundResourceName to match
        // (see the comment under PlayNotificationSound for how to verify it).
        private const string SoundResourceName = "UPLOADER.Resources.notifyms.mp3";

        private static void PlayNotificationSound()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var asm = System.Reflection.Assembly.GetExecutingAssembly();

                    // Uncomment temporarily to print all embedded resource names and
                    // confirm the exact string to use for SoundResourceName:
                    // foreach (var n in asm.GetManifestResourceNames())
                    //     System.Diagnostics.Debug.WriteLine(n);

                    using var resourceStream = asm.GetManifestResourceStream(SoundResourceName);
                    if (resourceStream == null) return; // resource not found / build action not set

                    // NAudio's Mp3FileReader can read directly from any Stream,
                    // so we never need to touch the filesystem.
                    using var reader = new NAudio.Wave.Mp3FileReader(resourceStream);
                    using var outputDevice = new NAudio.Wave.WaveOutEvent();

                    outputDevice.Init(reader);
                    outputDevice.Play();

                    while (outputDevice.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
                catch
                {
                    // Fail silently if windows audio mixer is busy
                }
            });
        }
    }
}