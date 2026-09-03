using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Booker.Services
{
    /// <summary>
    /// Anti-spam gate for chat messages: per-user sliding-window rate limit,
    /// duplicate-message suppression and link filtering. All checks are local
    /// and in-memory — cheap and dependency-free.
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
        }

        private readonly ConcurrentDictionary<int, SenderState> _senders = new();

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

                if (content.Length > 0 &&
                    string.Equals(content.Trim(), state.LastContent, StringComparison.OrdinalIgnoreCase))
                {
                    return ModerationVerdict.Duplicate;
                }

                if (UrlPattern.IsMatch(content))
                {
                    return ModerationVerdict.LinkBlocked;
                }

                state.Timestamps.Enqueue(now);
                state.LastContent = content.Trim();
            }

            return ModerationVerdict.Accepted;
        }

        public enum ModerationVerdict
        {
            Accepted,
            RateLimited,
            Duplicate,
            LinkBlocked
        }
    }
}
