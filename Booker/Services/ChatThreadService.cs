using Booker.Data;
using Booker.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services
{
    public class ChatThreadService : IChatThreadService
    {
        private readonly DataContext _ctx;
        private readonly ILogger<ChatThreadService> _log;
        public ChatThreadService(DataContext ctx, ILogger<ChatThreadService> log)
        {
            _ctx = ctx; _log = log;
        }

        public async Task<ChatThread?> GetByChannelIdAsync(string channelId, CancellationToken ct)
        {
            return await _ctx.ChatThreads.AsNoTracking().FirstOrDefaultAsync(t => t.ChannelId == channelId, ct);
        }

        public async Task<ChatThread> GetOrCreateAsync(int userAId, int userBId, CancellationToken ct)
        {
            string channelId = ThreadIdBuilder.Create(userAId, userBId);
            var existing = await _ctx.ChatThreads.FirstOrDefaultAsync(t => t.ChannelId == channelId, ct);
            if (existing != null) return existing;

            var thread = new ChatThread
            {
                ChannelId = channelId,
                UserAId = Math.Min(userAId, userBId),
                UserBId = Math.Max(userAId, userBId),
                CreatedUtc = DateTime.UtcNow,
                LastMessageUtc = DateTime.UtcNow
            };
            _ctx.ChatThreads.Add(thread);
            await _ctx.SaveChangesAsync(ct);
            _log.LogInformation("Chat thread {ChannelId} created", channelId);
            return thread;
        }

        public async Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(int userId, CancellationToken ct)
        {
            return await _ctx.ChatThreads.AsNoTracking()
                .Where(t => t.UserAId == userId || t.UserBId == userId)
                .OrderByDescending(t => t.LastMessageUtc)
                .ToListAsync(ct);
        }

        public async Task UpdateLastMessageUtcAsync(string channelId, DateTime utcNow, CancellationToken ct)
        {
            var thread = await _ctx.ChatThreads.FirstOrDefaultAsync(t => t.ChannelId == channelId, ct);
            if (thread == null) return;
            thread.LastMessageUtc = utcNow;
            await _ctx.SaveChangesAsync(ct);
        }
    }
}
