using Microsoft.Data.SqlClient;
using System.Data;

namespace UPLOADER
{
    public partial class FormChat : Form
    {
        private readonly AdminUser _currentUser;

        /// <summary>
        /// P/Invoke shim — lets us send WM_SETREDRAW to freeze/unfreeze the
        /// RichTextBox so multi-bubble appends paint in one single pass (no blink).
        /// </summary>
        internal static class NativeMethods
        {
            internal const int WM_SETREDRAW = 0x000B;

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            internal static extern IntPtr SendMessage(
                IntPtr hWnd, int msg, bool wParam, int lParam);
        }
        /// <summary>
        /// Suppresses WM_ERASEBKGND so the control never paints a white flash
        /// before drawing content. Works together with WM_SETREDRAW in LoadMessages.
        /// </summary>
        internal sealed class SmoothRichTextBox : RichTextBox
        {
            private const int WM_ERASEBKGND = 0x0014;

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_ERASEBKGND) return;
                base.WndProc(ref m);
            }
        }
        // A contact is either the Public Room (Id == null) or another admin user.
        private sealed record ChatContact(int? Id, string Name, DateTime? LastSeen = null)
        {
            public string DisplayName => Id == null ? "Public Room" : Name;

            // Considered "online" if we've heard a heartbeat within the last 45s.
            public bool IsOnline => Id != null && LastSeen.HasValue
                && (DateTime.Now - LastSeen.Value) < TimeSpan.FromSeconds(45);

            public string Subtitle => Id == null
                ? "Visible to everyone"
                : (IsOnline ? "Active now" : FormatLastSeen(LastSeen));

            public string Initial => DisplayName.Length > 0 ? DisplayName[0].ToString().ToUpper() : "?";

            private static string FormatLastSeen(DateTime? lastSeen)
            {
                if (!lastSeen.HasValue) return "Offline";
                var span = DateTime.Now - lastSeen.Value;
                if (span < TimeSpan.FromMinutes(1)) return "Active just now";
                if (span < TimeSpan.FromMinutes(60)) return $"Active {(int)span.TotalMinutes}m ago";
                if (span < TimeSpan.FromHours(24)) return $"Active {(int)span.TotalHours}h ago";
                return $"Active {(int)span.TotalDays}d ago";
            }
        }

        private List<ChatContact> _allContacts = new();
        private ChatContact? _selected;
        private int _lastRowCount = -1; // skip re-rendering when nothing new arrived
                                        // Tracks the highest message id already rendered — only NEW rows are ever
                                        // appended; we never clear+redraw, which eliminates the poll-tick blink.
        private long _lastMessageId = -1;

        // Distinct avatar colors so different people are easy to tell apart at a glance.
        private static readonly Color[] AvatarPalette =
        {
            Color.FromArgb(37, 99, 235),  Color.FromArgb(220, 38, 38),
            Color.FromArgb(5, 150, 105),  Color.FromArgb(217, 119, 6),
            Color.FromArgb(124, 58, 237), Color.FromArgb(219, 39, 119),
            Color.FromArgb(13, 148, 136), Color.FromArgb(71, 85, 105),
        };

        private static Color AvatarColorFor(string name)
        {
            int hash = 0;
            foreach (char c in name) hash = hash * 31 + c;
            int idx = Math.Abs(hash) % AvatarPalette.Length;
            return AvatarPalette[idx];
        }

        // ── Profile-picture cache (adminId → Image loaded from DB blob) ────────
        // Keyed by adminId; null entry means we already checked and there is no pic.
        private readonly Dictionary<int, Image?> _avatarCache = new();

        /// <summary>
        /// Returns the profile picture for <paramref name="adminId"/> from the DB,
        /// caching the result so we only hit the database once per contact per session.
        /// Returns <c>null</c> if the user has no picture stored.
        /// </summary>
        private Image? GetCachedAvatar(int adminId)
        {
            if (_avatarCache.TryGetValue(adminId, out var cached))
                return cached;

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "SELECT profile_pic_data FROM admins WHERE admin_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", adminId);

                using var reader = cmd.ExecuteReader();
                if (!reader.Read() || reader.IsDBNull(0))
                {
                    _avatarCache[adminId] = null;
                    return null;
                }

                byte[] imgBytes = (byte[])reader["profile_pic_data"];
                // Keep the stream open for the lifetime of the Image (GDI+ requirement).
                var ms = new MemoryStream(imgBytes);
                var img = Image.FromStream(ms);
                _avatarCache[adminId] = img;
                return img;
            }
            catch
            {
                _avatarCache[adminId] = null;
                return null;
            }
        }

        /// <summary>
        /// Clears the cached avatar for a specific user so it is reloaded on the next draw.
        /// Call this after a user updates their profile picture.
        /// </summary>
        public void InvalidateAvatarCache(int adminId) => _avatarCache.Remove(adminId);

        public FormChat(AdminUser currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
        }

        // ── @Mention / file-folder reference attachment ─────────────────────
        // Set by FileManagerForm's "@" toolbar button (btnMentionChat_Click) so
        // whoever gets @mentioned in the message that follows receives a
        // notification linking straight back to this specific file/folder
        // (see SendMessage below and NotificationService.CheckNewMentions).
        private int? _attachedItemId;
        private string? _attachedItemName;
        private bool _attachedIsFolder;

        private string AttachedTag => _attachedItemId.HasValue
            ? $"[{(_attachedIsFolder ? "📁" : "📄")} {_attachedItemName}]"
            : "";

        /// <summary>
        /// Pre-attaches a file/folder reference to the next message and drops a
        /// visible reference tag into the compose box so the sender can see
        /// (and, if they want, delete) exactly what they're attaching before
        /// typing "@username" and sending.
        /// </summary>
        public void AttachFileReference(int itemId, string itemName, bool isFolder)
        {
            _attachedItemId = itemId;
            _attachedItemName = itemName;
            _attachedIsFolder = isFolder;

            txtMessage.Text = AttachedTag + " @";
            txtMessage.SelectionStart = txtMessage.Text.Length;
            txtMessage.Focus();
        }

        /// <summary>
        /// Pulls distinct "@username" tokens out of a message body. Matches
        /// against admins.username (word characters, dot, underscore, dash —
        /// the same character set HIMS usernames are created with).
        /// </summary>
        private static List<string> ParseMentionedUsernames(string text)
        {
            var names = new List<string>();
            foreach (System.Text.RegularExpressions.Match m in
                     System.Text.RegularExpressions.Regex.Matches(text, @"@([\w.\-]+)"))
            {
                string name = m.Groups[1].Value;
                if (!names.Contains(name, StringComparer.OrdinalIgnoreCase))
                    names.Add(name);
            }
            return names;
        }

        // ── Load ──────────────────────────────────────────────────────────────
        private void FormChat_Load(object sender, EventArgs e)
        {
            LoadContacts();
            if (lstContacts.Items.Count > 0)
                lstContacts.SelectedIndex = 0; // defaults to Public Room
            tmrPoll.Start();

            SendHeartbeat();           // mark myself online immediately
            tmrHeartbeat.Start();

            // Feature 27 & 29: wire mouse handlers on the message area
            txtMessages.MouseDoubleClick += txtMessages_MouseDoubleClick;
            txtMessages.MouseUp += txtMessages_MouseUp;

            MarkMyMentionsRead();
        }

        /// <summary>Clears the unread flag on every @mention addressed to me.</summary>
        private void MarkMyMentionsRead()
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    UPDATE hims_chat_mentions
                    SET    is_read = 1, read_at = SYSDATETIME()
                    WHERE  mentioned_admin_id = @me AND is_read = 0", conn);
                cmd.Parameters.AddWithValue("@me", _currentUser.Id);
                cmd.ExecuteNonQuery();
            }
            catch { /* table may not be migrated yet — non-critical */ }
        }

        private void FormChat_FormClosed(object? sender, FormClosedEventArgs e)
        {
            tmrPoll.Stop();
            tmrHeartbeat.Stop();
        }

        // ── Presence (online / last-seen) ───────────────────────────────────
        private void tmrHeartbeat_Tick(object? sender, EventArgs e) => SendHeartbeat();

        private void SendHeartbeat()
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(
                    "UPDATE admins SET last_seen = SYSDATETIME() WHERE admin_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", _currentUser.Id);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Non-critical — presence is best-effort.
            }
        }

        // Re-reads last_seen for everyone and redraws the dots/labels without
        // disturbing the current selection or re-querying messages.
        private void RefreshPresence()
        {
            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand("SELECT admin_id, last_seen FROM admins WHERE admin_id <> @me", conn);
                cmd.Parameters.AddWithValue("@me", _currentUser.Id);

                var map = new Dictionary<int, DateTime?>();
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    int id = r.GetInt32("admin_id");
                    int ord = r.GetOrdinal("last_seen");
                    map[id] = r.IsDBNull(ord) ? (DateTime?)null : r.GetDateTime(ord);
                }
                r.Close();

                // 1. FREEZE SIDEBAR RENDERING: Prevents layout flashes on the screen
                NativeMethods.SendMessage(lstContacts.Handle, NativeMethods.WM_SETREDRAW, false, 0);

                try
                {
                    bool needsRedraw = false;

                    for (int i = 0; i < lstContacts.Items.Count; i++)
                    {
                        if (lstContacts.Items[i] is ChatContact c && c.Id.HasValue && map.TryGetValue(c.Id.Value, out var ls))
                        {
                            // Check if the timestamp actually changed
                            if (c.LastSeen != ls)
                            {
                                // Use 'with' to create the clone since it's an init-only property
                                lstContacts.Items[i] = c with { LastSeen = ls };
                                needsRedraw = true;
                            }
                        }
                    }

                    // Update selected user header info safely if changed
                    if (_selected?.Id != null && map.TryGetValue(_selected.Id.Value, out var selLs))
                    {
                        if (_selected.LastSeen != selLs)
                        {
                            _selected = _selected with { LastSeen = selLs };
                            lblChatSubtitle.Text = _selected.Subtitle;
                        }
                    }

                    // 2. ONLY TELL WINDOWS TO DRAW IF AN ONLINE STATUS ACTUALLY SWAPPED
                    if (needsRedraw)
                    {
                        lstContacts.Invalidate();
                    }
                }
                finally
                {
                    // 3. UNFREEZE DRAWING: The UI updates instantly in one smooth pass
                    NativeMethods.SendMessage(lstContacts.Handle, NativeMethods.WM_SETREDRAW, true, 0);
                }
            }
            catch
            {
                // Fail silently on quick disconnects to prevent crashes
            }
        }

        // ── Build the contact list: Public Room + every other active user ──────
        private void LoadContacts()
        {
            _allContacts.Clear();
            _allContacts.Add(new ChatContact(null, "Public Room"));

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    SELECT admin_id, full_name, last_seen
                    FROM   admins
                    WHERE  admin_id <> @me
                           AND is_active = 1
                    ORDER  BY full_name", conn);
                cmd.Parameters.AddWithValue("@me", _currentUser.Id);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32("admin_id");
                    string name = reader.GetString("full_name");
                    int ord = reader.GetOrdinal("last_seen");
                    DateTime? lastSeen = reader.IsDBNull(ord) ? (DateTime?)null : reader.GetDateTime(ord);
                    _allContacts.Add(new ChatContact(id, name, lastSeen));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load users: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ApplyContactFilter(txtSearchContacts.Text);
        }

        // ── Filter contacts as the user types (Public Room always stays put) ──
        private void txtSearchContacts_TextChanged(object sender, EventArgs e)
            => ApplyContactFilter(txtSearchContacts.Text);

        private void ApplyContactFilter(string query)
        {
            var previouslySelected = _selected;
            query = query?.Trim() ?? "";

            lstContacts.Items.Clear();
            foreach (var c in _allContacts)
            {
                if (c.Id == null || string.IsNullOrWhiteSpace(query) ||
                    c.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    lstContacts.Items.Add(c);
                }
            }

            if (previouslySelected != null)
            {
                int idx = lstContacts.Items.IndexOf(previouslySelected);
                lstContacts.SelectedIndex = idx >= 0 ? idx : (lstContacts.Items.Count > 0 ? 0 : -1);
            }
        }

        // ── Custom-drawn contact rows: colored initial avatar + name + subtitle ─
        private void lstContacts_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var contact = (ChatContact)lstContacts.Items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            var rowRect = e.Bounds;
            using var bg = new SolidBrush(selected ? FormChat_ClrSidebarSel : FormChat_ClrSidebarBg);
            e.Graphics.FillRectangle(bg, rowRect);

            // Avatar circle
            int avatarSize = 36;
            var avatarRect = new Rectangle(rowRect.Left + 12, rowRect.Top + (rowRect.Height - avatarSize) / 2,
                avatarSize, avatarSize);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Try to draw the real profile picture (clipped to a circle); fall back to
            // the coloured initial / globe icon when no picture is stored in the DB.
            Image? profilePic = contact.Id.HasValue ? GetCachedAvatar(contact.Id.Value) : null;

            if (profilePic != null)
            {
                // Clip to circle then draw the photo scaled to fit the avatar rect.
                using var circlePath = new System.Drawing.Drawing2D.GraphicsPath();
                circlePath.AddEllipse(avatarRect);
                var oldClip = e.Graphics.Clip;
                e.Graphics.SetClip(circlePath, System.Drawing.Drawing2D.CombineMode.Intersect);
                e.Graphics.DrawImage(profilePic, avatarRect);
                e.Graphics.Clip = oldClip;
            }
            else
            {
                // Fallback: coloured circle with initial letter or globe emoji.
                Color avatarColor = contact.Id == null
                    ? Color.FromArgb(100, 116, 139)
                    : AvatarColorFor(contact.Name);
                using var avatarBrush = new SolidBrush(avatarColor);
                e.Graphics.FillEllipse(avatarBrush, avatarRect);

                string avatarText = contact.Id == null ? "🌐" : contact.Initial;
                using var avatarFont = new Font("Segoe UI", contact.Id == null ? 13F : 12F, FontStyle.Bold);
                var avatarTextSize = e.Graphics.MeasureString(avatarText, avatarFont);
                e.Graphics.DrawString(avatarText, avatarFont, Brushes.White,
                    avatarRect.Left + (avatarSize - avatarTextSize.Width) / 2,
                    avatarRect.Top + (avatarSize - avatarTextSize.Height) / 2);
            }

            // Online / offline presence dot (small, bottom-right of avatar), with a
            // sidebar-colored ring so it reads cleanly against the avatar circle.
            if (contact.Id != null)
            {
                int dotSize = 12;
                var dotRect = new Rectangle(
                    avatarRect.Right - dotSize - 1, avatarRect.Bottom - dotSize - 1, dotSize, dotSize);
                Color dotColor = contact.IsOnline ? Color.FromArgb(34, 197, 94) : Color.FromArgb(148, 163, 184);

                using var ringBrush = new SolidBrush(selected ? FormChat_ClrSidebarSel : FormChat_ClrSidebarBg);
                e.Graphics.FillEllipse(ringBrush, Rectangle.Inflate(dotRect, 2, 2));
                using var dotBrush = new SolidBrush(dotColor);
                e.Graphics.FillEllipse(dotBrush, dotRect);
            }

            // Name + subtitle
            int textLeft = avatarRect.Right + 12;
            using var nameFont = new Font("Segoe UI Semibold", 9.7F, FontStyle.Bold);
            using var subFont = new Font("Segoe UI", 8.2F);
            using var nameBrush = new SolidBrush(Color.White);
            using var subBrush = new SolidBrush(contact.IsOnline
                ? Color.FromArgb(74, 222, 128) : Color.FromArgb(148, 163, 184));

            e.Graphics.DrawString(contact.DisplayName, nameFont, nameBrush,
                textLeft, rowRect.Top + 11);
            e.Graphics.DrawString(contact.Subtitle, subFont, subBrush,
                textLeft, rowRect.Top + 31);
        }

        // Pull-through accessors so the DrawItem handler can use the Designer's palette.
        private static readonly Color FormChat_ClrSidebarBg = Color.FromArgb(30, 41, 59);
        private static readonly Color FormChat_ClrSidebarSel = Color.FromArgb(37, 99, 235);

        // ── Switch conversation ──────────────────────────────────────────────
        private void lstContacts_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstContacts.SelectedItem is not ChatContact contact) return;

            _selected = contact;
            lblChatTitle.Text = contact.DisplayName;
            lblChatSubtitle.Text = contact.Subtitle;
            _lastMessageId = -1; // force a full reload for this conversation

            // Update the header avatar in pnlTop with the selected contact's picture.
            UpdateHeaderAvatar(contact);

            LoadMessages(force: true);
        }

        /// <summary>
        /// Draws (or removes) a small circular profile picture next to the chat title
        /// in the header panel whenever the selected conversation changes.
        /// </summary>
        private PictureBox? _headerAvatar; // created once, reused across contact switches

        private void UpdateHeaderAvatar(ChatContact contact)
        {
            // Lazily create the PictureBox the first time it is needed.
            if (_headerAvatar == null)
            {
                _headerAvatar = new PictureBox
                {
                    Size = new Size(36, 36),
                    Location = new Point(pnlTop.Width - 56, 14),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                };

                // Clip to circle via Paint event.
                _headerAvatar.Paint += (s, pe) =>
                {
                    if (_headerAvatar.Image == null) return;
                    pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using var path = new System.Drawing.Drawing2D.GraphicsPath();
                    path.AddEllipse(0, 0, _headerAvatar.Width, _headerAvatar.Height);
                    pe.Graphics.SetClip(path);
                    pe.Graphics.DrawImage(_headerAvatar.Image,
                        new Rectangle(0, 0, _headerAvatar.Width, _headerAvatar.Height));
                };

                pnlTop.Controls.Add(_headerAvatar);
            }

            if (contact.Id.HasValue)
            {
                Image? pic = GetCachedAvatar(contact.Id.Value);
                _headerAvatar.Image = pic;
                _headerAvatar.Visible = pic != null;
            }
            else
            {
                // Public Room — no avatar needed.
                _headerAvatar.Image = null;
                _headerAvatar.Visible = false;
            }
        }

        // ── Poll for new messages ────────────────────────────────────────────
        private void tmrPoll_Tick(object? sender, EventArgs e)
        {
            LoadMessages();
            RefreshPresence();
        }

        private void btnRefresh_Click(object sender, EventArgs e) => LoadMessages(force: true);

        private void LoadMessages(bool force = false)
        {
            if (_selected == null) return;

            try
            {
                using var conn = DbConfig.OpenConnection();
                SqlCommand cmd;

                // On force (contact switch / manual refresh) reset watermark so
                // everything is re-rendered from scratch — but still append-only.
                long afterId = force ? -1 : _lastMessageId;

                if (_selected.Id == null)
                {
                    // Public room — only rows we haven't seen yet.
                    cmd = new SqlCommand(@"
                SELECT TOP (300) id, sender_id, sender_name, message, created_at
                FROM   hims_chat_messages
                WHERE  recipient_id IS NULL
                  AND  id > @afterId
                ORDER  BY created_at ASC, id ASC", conn);
                    cmd.Parameters.AddWithValue("@afterId", afterId);
                }
                else
                {
                    // Direct messages — only rows we haven't seen yet.
                    cmd = new SqlCommand(@"
                SELECT TOP (300) id, sender_id, sender_name, message, created_at
                FROM   hims_chat_messages
                WHERE  ((sender_id = @me AND recipient_id = @them)
                     OR (sender_id = @them AND recipient_id = @me))
                  AND  id > @afterId
                ORDER  BY created_at ASC, id ASC", conn);
                    cmd.Parameters.AddWithValue("@me", _currentUser.Id);
                    cmd.Parameters.AddWithValue("@them", _selected.Id);
                    cmd.Parameters.AddWithValue("@afterId", afterId);
                }

                using var adapter = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);

                // Nothing new and not a forced reload — nothing to do, zero paint.
                if (dt.Rows.Count == 0 && !force) return;

                if (force)
                {
                    // Full reload: wipe existing content and reset watermark.
                    txtMessages.Clear();
                    _lastMessageId = -1;
                    _attachmentMap.Clear();
                    _lineToMsgId.Clear();
                    _lineIsMine.Clear();
                }

                bool atBottom = IsScrolledToBottom();

                // Freeze repaints while appending to avoid per-line flicker.
                NativeMethods.SendMessage(txtMessages.Handle, NativeMethods.WM_SETREDRAW, false, 0);
                try
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        long msgId = Convert.ToInt64(row["id"]);
                        int senderId = Convert.ToInt32(row["sender_id"]);
                        string senderName = row["sender_name"].ToString() ?? "Unknown";
                        string message = row["message"].ToString() ?? "";
                        DateTime when = Convert.ToDateTime(row["created_at"]);
                        bool isMine = senderId == _currentUser.Id;

                        AppendBubble(senderName, message, when, isMine, msgId);

                        if (msgId > _lastMessageId) _lastMessageId = msgId;
                    }
                }
                finally
                {
                    // Re-enable painting and force a single clean redraw.
                    NativeMethods.SendMessage(txtMessages.Handle, NativeMethods.WM_SETREDRAW, true, 0);
                    txtMessages.Invalidate();
                }

                // Only auto-scroll when the user was already at the bottom.
                if (atBottom || force)
                {
                    txtMessages.SelectionStart = txtMessages.TextLength;
                    txtMessages.ScrollToCaret();
                }

                // Feature 26: mark all visible messages as read
                MarkConversationRead();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load messages: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsScrolledToBottom()
        {
            if (txtMessages.TextLength == 0) return true;
            var pos = txtMessages.GetPositionFromCharIndex(txtMessages.TextLength - 1);
            return pos.Y <= txtMessages.ClientSize.Height;
        }
        // ── Render one chat bubble: right/blue for me, left/gray for others ────
        private void AppendBubble(string sender, string message, DateTime when, bool isMine, long msgId = -1)
        {
            // Feature 29: track which line this bubble starts on → msgId
            int startLine = txtMessages.GetLineFromCharIndex(txtMessages.TextLength);
            if (msgId >= 0)
            {
                _lineToMsgId[startLine] = msgId;
                _lineIsMine[startLine] = isMine;
            }

            // Feature 27: attachments render differently
            if (TryHandleAttachment(message, sender, when, isMine)) return;

            txtMessages.SelectionStart = txtMessages.TextLength;
            txtMessages.SelectionLength = 0;

            var alignment = isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            // Feature 29: show deleted messages as greyed-out placeholder
            bool isDeleted = message == "[DELETED]";
            Color bubbleBack = isDeleted ? Color.FromArgb(226, 232, 240)
                             : isMine ? Color.FromArgb(37, 99, 235)
                                         : Color.White;
            Color bubbleText = isDeleted ? Color.FromArgb(148, 163, 184)
                             : isMine ? Color.White
                                         : Color.FromArgb(15, 23, 42);

            // Sender name + time (small, muted) above the bubble
            txtMessages.SelectionAlignment = alignment;
            txtMessages.SelectionFont = new Font("Segoe UI", 8F, FontStyle.Bold);
            txtMessages.SelectionColor = Color.FromArgb(100, 116, 139);
            txtMessages.SelectionBackColor = txtMessages.BackColor;
            txtMessages.AppendText(isMine ? "You  " : $"{sender}  ");

            txtMessages.SelectionFont = new Font("Segoe UI", 7.5F, FontStyle.Italic);
            txtMessages.AppendText($"{when:MMM d, h:mm tt}\n");

            // The message bubble itself
            txtMessages.SelectionAlignment = alignment;
            txtMessages.SelectionFont = isDeleted
                ? new Font("Segoe UI", 10F, FontStyle.Italic)
                : new Font("Segoe UI", 10F);
            txtMessages.SelectionColor = bubbleText;
            txtMessages.SelectionBackColor = bubbleBack;
            txtMessages.AppendText(isDeleted ? " 🗑 This message was deleted. " : $" {message} ");

            // Reset back-color so the next line isn't tinted, and add spacing between bubbles
            txtMessages.SelectionBackColor = txtMessages.BackColor;
            txtMessages.AppendText("\n\n");
        }

        // ── Send ──────────────────────────────────────────────────────────────
        private void btnSend_Click(object sender, EventArgs e) => SendMessage();

        private void txtMessage_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendMessage();
            }
        }

        private void SendMessage()
        {
            if (_selected == null) return;

            string text = txtMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            // Attachment only carries through if its tag is still present —
            // deleting the "[📁 FolderName]" tag before sending cancels it.
            bool hasAttachment = _attachedItemId.HasValue && text.Contains(AttachedTag);

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    INSERT INTO hims_chat_messages
                        (sender_id, sender_name, recipient_id, recipient_name, message, created_at)
                    OUTPUT INSERTED.id
                    VALUES
                        (@senderId, @senderName, @recipientId, @recipientName, @message, SYSDATETIME())", conn);

                cmd.Parameters.AddWithValue("@senderId", _currentUser.Id);
                cmd.Parameters.AddWithValue("@senderName", _currentUser.FullName);
                cmd.Parameters.AddWithValue("@recipientId", _selected.Id.HasValue ? _selected.Id.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@recipientName", _selected.Id.HasValue ? _selected.Name : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@message", text);
                int newMessageId = (int)cmd.ExecuteScalar();

                AuditLogger.Log(_currentUser,
    AuditLogger.ModChat, AuditLogger.SendChat,
    targetName: _selected!.DisplayName,
    detail: $"IsPublic={_selected.Id == null} | Length={text.Length}");

                // ── @mentions ────────────────────────────────────────────────
                var mentionedNames = ParseMentionedUsernames(text);
                if (mentionedNames.Count > 0)
                    SaveMentions(conn, newMessageId, mentionedNames, hasAttachment);

                txtMessage.Clear();
                _attachedItemId = null;
                _attachedItemName = null;
                _attachedIsFolder = false;
                LoadMessages(force: false); // append-only — shows the message we just sent
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send message: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Resolves each @username to a real admin account and records a
        /// notification row per mentioned user (hims_chat_mentions), carrying
        /// the attached file/folder reference along if one was present. Bad/
        /// unknown usernames are silently ignored rather than failing the send.
        /// </summary>
        private void SaveMentions(SqlConnection conn, int messageId, List<string> usernames, bool hasAttachment)
        {
            using var lookup = new SqlCommand(
                "SELECT admin_id, username FROM admins WHERE username = @u AND is_active = 1", conn);
            var pUsername = lookup.Parameters.Add("@u", SqlDbType.NVarChar, 50);

            using var insert = new SqlCommand(@"
                INSERT INTO hims_chat_mentions
                    (message_id, mentioned_admin_id, mentioned_username,
                     mentioned_by_id, mentioned_by_name,
                     attached_item_id, attached_item_name, attached_is_folder,
                     created_at, is_read)
                VALUES
                    (@msg, @mentionedId, @mentionedName,
                     @byId, @byName,
                     @itemId, @itemName, @isFolder,
                     SYSDATETIME(), 0)", conn);

            var matchedForAudit = new List<string>();

            foreach (string uname in usernames)
            {
                pUsername.Value = uname;
                int? mentionedId = null;
                string? mentionedUsername = null;

                using (var r = lookup.ExecuteReader())
                {
                    if (r.Read())
                    {
                        mentionedId = r.GetInt32(0);
                        mentionedUsername = r.GetString(1);
                    }
                }

                // Skip unknown usernames and self-mentions.
                if (mentionedId == null || mentionedId == _currentUser.Id) continue;

                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@msg", messageId);
                insert.Parameters.AddWithValue("@mentionedId", mentionedId.Value);
                insert.Parameters.AddWithValue("@mentionedName", mentionedUsername!);
                insert.Parameters.AddWithValue("@byId", _currentUser.Id);
                insert.Parameters.AddWithValue("@byName", _currentUser.FullName);
                insert.Parameters.AddWithValue("@itemId", hasAttachment ? _attachedItemId!.Value : (object)DBNull.Value);
                insert.Parameters.AddWithValue("@itemName", hasAttachment ? (object)_attachedItemName! : DBNull.Value);
                insert.Parameters.AddWithValue("@isFolder", hasAttachment ? (object)_attachedIsFolder : DBNull.Value);
                insert.ExecuteNonQuery();

                matchedForAudit.Add(mentionedUsername!);
            }

            if (hasAttachment && matchedForAudit.Count > 0)
            {
                AuditLogger.Log(_currentUser,
                    AuditLogger.ModChat, AuditLogger.MentionFile,
                    targetId: _attachedItemId!.Value.ToString(),
                    targetName: _attachedItemName,
                    detail: $"Mentioned: {string.Join(", ", matchedForAudit)}");
            }
        }
        // ══════════════════════════════════════════════════════════════════════
        // FEATURE 25 — Notification History / Inbox
        // A simple in-memory log of every toast that fired this session.
        // ══════════════════════════════════════════════════════════════════════
        private readonly List<(DateTime When, string Title, string Body)> _notifHistory = new();

        /// <summary>
        /// Call this from FileManagerForm.ShowToast() BEFORE showing the toast
        /// to record every notification in the session inbox.
        /// </summary>
        public static void RecordNotification(
            List<(DateTime, string, string)> history, string title, string body)
                => history.Add((DateTime.Now, title, body));

        /// <summary>Opens the notification inbox dialog.</summary>
        public void ShowNotificationInbox()
        {
            if (_notifHistory.Count == 0)
            {
                MessageBox.Show("No notifications this session.", "Inbox",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dlg = new Form
            {
                Text = "Notification Inbox",
                Size = new Size(520, 400),
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false
            };
            var lv = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9.5F)
            };
            lv.Columns.Add("Time", 110);
            lv.Columns.Add("Title", 140);
            lv.Columns.Add("Message", 220);

            foreach (var (when, title, body) in _notifHistory)
                lv.Items.Add(new ListViewItem(new[] { when.ToString("HH:mm:ss"), title, body }));

            dlg.Controls.Add(lv);
            dlg.ShowDialog(this);
        }

        // ══════════════════════════════════════════════════════════════════════
        // FEATURE 26 — Mark messages as read (read_at flag)
        // Requires DB column: ALTER TABLE hims_chat_messages
        //                     ADD COLUMN read_at DATETIME NULL DEFAULT NULL;
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Marks all messages in the current conversation as read by the current user.
        /// Called automatically after LoadMessages renders new rows.
        /// </summary>
        private void MarkConversationRead()
        {
            if (_selected == null) return;
            try
            {
                using var conn = DbConfig.OpenConnection();
                string sql = _selected.Id == null
                    ? @"UPDATE hims_chat_messages
                        SET    read_at = SYSDATETIME()
                        WHERE  recipient_id IS NULL
                               AND sender_id <> @me
                               AND read_at IS NULL"
                    : @"UPDATE hims_chat_messages
                        SET    read_at = SYSDATETIME()
                        WHERE  sender_id = @them
                               AND recipient_id = @me
                               AND read_at IS NULL";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@me", _currentUser.Id);
                if (_selected.Id.HasValue)
                    cmd.Parameters.AddWithValue("@them", _selected.Id.Value);
                cmd.ExecuteNonQuery();
            }
            catch { /* non-critical */ }
        }

        // ══════════════════════════════════════════════════════════════════════
        // FEATURE 27 — Chat file attachment
        // Stores small files as base64 text in the message column with a
        // [ATTACH:filename|base64data] prefix so no schema change is needed.
        // ══════════════════════════════════════════════════════════════════════

        private void SendAttachment()
        {
            if (_selected == null) return;

            using var dlg = new OpenFileDialog
            {
                Title = "Select a file to send",
                Filter = "Images & Documents|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.pdf;*.docx;*.xlsx;*.txt|All Files|*.*",
                Multiselect = false
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            var fi = new FileInfo(dlg.FileName);
            const long maxBytes = 2 * 1024 * 1024; // 2 MB limit
            if (fi.Length > maxBytes)
            {
                MessageBox.Show("File is too large to send via chat (max 2 MB).",
                    "Too Large", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte[] data = File.ReadAllBytes(dlg.FileName);
            string b64 = Convert.ToBase64String(data);
            string payload = $"[ATTACH:{fi.Name}|{b64}]";

            try
            {
                using var conn = DbConfig.OpenConnection();
                using var cmd = new SqlCommand(@"
                    INSERT INTO hims_chat_messages
                        (sender_id, sender_name, recipient_id, recipient_name, message, created_at)
                    VALUES (@sid, @sname, @rid, @rname, @msg, SYSDATETIME())", conn);

                cmd.Parameters.AddWithValue("@sid", _currentUser.Id);
                cmd.Parameters.AddWithValue("@sname", _currentUser.FullName);
                cmd.Parameters.AddWithValue("@rid", _selected.Id.HasValue ? _selected.Id.Value : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@rname", _selected.Id.HasValue ? _selected.Name : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@msg", payload);
                cmd.ExecuteNonQuery();

                LoadMessages(force: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send attachment: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Detects [ATTACH:...] payloads and offers a Save dialog instead of
        /// rendering raw base64 in the bubble. Called inside AppendBubble.
        /// </summary>
        private bool TryHandleAttachment(string message, string sender, DateTime when, bool isMine)
        {
            if (!message.StartsWith("[ATTACH:") || !message.Contains("|")) return false;

            // Parse  [ATTACH:filename|base64]
            int colon = message.IndexOf(':');
            int pipe = message.IndexOf('|');
            int close = message.LastIndexOf(']');
            if (colon < 0 || pipe < 0 || close < 0) return false;

            string filename = message.Substring(colon + 1, pipe - colon - 1);

            // Render a clickable "📎 filename" link in the bubble
            txtMessages.SelectionStart = txtMessages.TextLength;
            txtMessages.SelectionLength = 0;
            var alignment = isMine ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            txtMessages.SelectionAlignment = alignment;
            txtMessages.SelectionFont = new Font("Segoe UI", 8F, FontStyle.Bold);
            txtMessages.SelectionColor = Color.FromArgb(100, 116, 139);
            txtMessages.SelectionBackColor = txtMessages.BackColor;
            txtMessages.AppendText(isMine ? "You  " : $"{sender}  ");
            txtMessages.SelectionFont = new Font("Segoe UI", 7.5F, FontStyle.Italic);
            txtMessages.AppendText($"{when:MMM d, h:mm tt}\n");

            txtMessages.SelectionAlignment = alignment;
            txtMessages.SelectionFont = new Font("Segoe UI", 10F, FontStyle.Underline);
            txtMessages.SelectionColor = Color.FromArgb(37, 99, 235);
            txtMessages.SelectionBackColor = isMine
                ? Color.FromArgb(37, 99, 235) : Color.White;
            txtMessages.AppendText($" 📎 {filename} ");

            txtMessages.SelectionBackColor = txtMessages.BackColor;
            txtMessages.AppendText("\n\n");

            // Wire up a click handler: when the user double-clicks the attachment
            // line, save it. We store the b64 payload keyed to position.
            _attachmentMap[txtMessages.TextLength] = message;

            return true;
        }

        // Maps character-position → raw attachment payload for double-click save.
        private readonly Dictionary<int, string> _attachmentMap = new();

        private void txtMessages_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            int charIdx = txtMessages.GetCharIndexFromPosition(e.Location);
            // Find the nearest attachment whose position is ≥ charIdx
            foreach (var kv in _attachmentMap.OrderBy(k => k.Key))
            {
                if (charIdx <= kv.Key)
                {
                    SaveAttachment(kv.Value);
                    return;
                }
            }
        }

        private void SaveAttachment(string payload)
        {
            int colon = payload.IndexOf(':');
            int pipe = payload.IndexOf('|');
            int close = payload.LastIndexOf(']');
            if (colon < 0 || pipe < 0 || close < 0) return;

            string filename = payload.Substring(colon + 1, pipe - colon - 1);
            string b64 = payload.Substring(pipe + 1, close - pipe - 1);

            using var dlg = new SaveFileDialog
            {
                FileName = filename,
                Title = "Save attachment"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                File.WriteAllBytes(dlg.FileName, Convert.FromBase64String(b64));
                MessageBox.Show("File saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // FEATURE 28 — Chat message search
        // ══════════════════════════════════════════════════════════════════════

        private string _searchHighlight = "";

        public void OpenMessageSearch()
        {
            string? query = Microsoft.VisualBasic.Interaction.InputBox(
                "Search in this conversation:", "Search Messages", _searchHighlight);
            if (string.IsNullOrWhiteSpace(query)) return;

            _searchHighlight = query.Trim();
            SearchAndHighlight(_searchHighlight);
        }

        private void SearchAndHighlight(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return;

            int start = 0;
            int found = 0;
            string fullText = txtMessages.Text;

            // Clear previous highlights
            txtMessages.SelectAll();
            txtMessages.SelectionBackColor = txtMessages.BackColor;
            txtMessages.DeselectAll();

            while (true)
            {
                int idx = fullText.IndexOf(query, start, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) break;
                txtMessages.Select(idx, query.Length);
                txtMessages.SelectionBackColor = Color.Yellow;
                txtMessages.SelectionColor = Color.Black;
                start = idx + query.Length;
                found++;
            }

            if (found == 0)
                MessageBox.Show($"No results for \"{query}\".", "Search",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
            {
                // Scroll to first match
                int first = fullText.IndexOf(query, StringComparison.OrdinalIgnoreCase);
                if (first >= 0)
                {
                    txtMessages.SelectionStart = first;
                    txtMessages.ScrollToCaret();
                }
                MessageBox.Show($"{found} match(es) found.", "Search",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // FEATURE 29 — Delete / Recall own messages
        // Right-click on a message row → context menu → Delete
        // Requires the message id — we track the last-rendered id per line.
        // ══════════════════════════════════════════════════════════════════════

        // Maps rendered line number → message DB id (populated in AppendBubble)
        private readonly Dictionary<int, long> _lineToMsgId = new();
        private readonly Dictionary<int, bool> _lineIsMine = new();

        private void txtMessages_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            int charIdx = txtMessages.GetCharIndexFromPosition(e.Location);
            int line = txtMessages.GetLineFromCharIndex(charIdx);

            // Walk backwards to find the closest message ID at or before this line
            long msgId = -1;
            bool isMine = false;
            for (int l = line; l >= 0; l--)
            {
                if (_lineToMsgId.TryGetValue(l, out long id))
                {
                    msgId = id;
                    _lineIsMine.TryGetValue(l, out isMine);
                    break;
                }
            }

            if (msgId < 0 || !isMine) return; // can only delete your own messages

            var menu = new ContextMenuStrip();
            var deleteItem = new ToolStripMenuItem("🗑  Delete this message");
            deleteItem.Click += (s, ev) => DeleteMessage(msgId);
            menu.Items.Add(deleteItem);
            menu.Show(txtMessages, e.Location);
        }

        private void DeleteMessage(long msgId)
        {
            if (MessageBox.Show("Delete this message?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                using var conn = DbConfig.OpenConnection();
                // Soft delete: mark with a special prefix so the bubble shows "[deleted]"
                using var cmd = new SqlCommand(@"
                    UPDATE hims_chat_messages
                    SET    message = '[DELETED]',
                           deleted_at = SYSDATETIME()
                    WHERE  id = @id AND sender_id = @me", conn);
                cmd.Parameters.AddWithValue("@id", msgId);
                cmd.Parameters.AddWithValue("@me", _currentUser.Id);
                cmd.ExecuteNonQuery();

                LoadMessages(force: true); // redraw conversation
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}