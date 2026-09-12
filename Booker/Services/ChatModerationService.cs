using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace Booker.Services
{
    /// <summary>
    /// Moderation settings. The banned word list lives in configuration
    /// (ChatModeration:BannedWords) so it can be curated per environment
    /// without a code change; the running service picks edits up live.
    /// </summary>
    public class ChatModerationOptions
    {
        public string[] BannedWords { get; set; } = [];
    }

    /// <summary>
    /// Anti-spam and civility gate for chat messages: per-user sliding-window
    /// rate limit, duplicate-message suppression, link filtering and banned
    /// word filtering. All checks are local and in-memory, so they are cheap
    /// and dependency-free.
    /// </summary>
    public class ChatModerationService
    {
        /// <summary>Messages allowed per window per user.</summary>
        public const int MessagesPerWindow = 10;

        /// <summary>Sliding window length.</summary>
        public static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

        private static readonly Regex UrlPattern = new(
            @"(?:https?://|www\.)\S+|\b[a-z0-9.-]+\.(?:com|net|org|pl|io|ru|xyz|info|biz|top)(?:/\S*)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private sealed class SenderState
        {
            public readonly Queue<DateTimeOffset> Timestamps = new();
            public string LastContent = string.Empty;
            public DateTimeOffset LastContentAt;
        }

        private readonly ConcurrentDictionary<int, SenderState> _senders = new();

        // Swapped atomically when the configured word list changes; readers
        // either see the old or the new pattern, both are self-consistent.
        private volatile Regex _bannedWordPattern;

        public ChatModerationService(IOptionsMonitor<ChatModerationOptions> options)
        {
            _bannedWordPattern = BuildBannedPattern(options.CurrentValue.BannedWords);
            options.OnChange(o => _bannedWordPattern = BuildBannedPattern(o.BannedWords));
        }

        /// <summary>Test and explicit-configuration constructor.</summary>
        internal ChatModerationService(IEnumerable<string> bannedWords)
        {
            _bannedWordPattern = BuildBannedPattern(bannedWords);
        }

        public ModerationVerdict Check(int userId, string content, DateTimeOffset now)
        {
            var state = _senders.GetOrAdd(userId, _ => new SenderState());

            lock (state.Timestamps)
            {
                // Sliding window: drop timestamps older than the window.
                while (state.Timestamps.Count > 0 && now - state.Timestamps.Peek() > Window)
                {
                    state.Timestamps.Dequeue();
                }

                if (state.Timestamps.Count >= MessagesPerWindow)
                {
                    return ModerationVerdict.RateLimited;
                }

                // Duplicate suppression is bounded by the same window as the rate
                // limit: a repeated "ok" an hour later is a normal chat message,
                // a burst of identical messages is spam.
                if (content.Length > 0
                    && now - state.LastContentAt <= Window
                    && string.Equals(content.Trim(), state.LastContent, StringComparison.OrdinalIgnoreCase))
                {
                    return ModerationVerdict.Duplicate;
                }

                if (UrlPattern.IsMatch(content))
                {
                    return ModerationVerdict.LinkBlocked;
                }

                if (_bannedWordPattern.IsMatch(content))
                {
                    return ModerationVerdict.ProfanityBlocked;
                }

                state.Timestamps.Enqueue(now);
                state.LastContent = content.Trim();
                state.LastContentAt = now;
            }

            return ModerationVerdict.Accepted;
        }

        /// <summary>
        /// Matches every configured word even when non-letter characters are
        /// scattered between its letters, so asterisk-style masking of single
        /// characters does not sneak a word past the filter. Separators are
        /// any non-letters, so Cyrillic entries are matched the same way as
        /// Latin ones (the marketplace also serves Ukrainian users).
        /// </summary>
        private static Regex BuildBannedPattern(IEnumerable<string> words)
        {
            var list = words?.Where(w => !string.IsNullOrWhiteSpace(w)).ToList() ?? [];
            if (list.Count == 0)
            {
                // Never-matching placeholder: an empty list disables the filter.
                return new Regex("(?!x)x", RegexOptions.Compiled);
            }

            var separator = @"[^\p{L}]*";
            var alternation = string.Join("|",
                list.Select(w => string.Join(separator, w.Trim().Select(c => c.ToString()))));
            return new Regex($@"(?i)\b(?:{alternation})\b", RegexOptions.Compiled);
        }

        public enum ModerationVerdict
        {
            Accepted,
            RateLimited,
            Duplicate,
            LinkBlocked,
            ProfanityBlocked
        }
    }
}
