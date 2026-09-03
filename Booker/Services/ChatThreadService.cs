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

        public async Task<ChatThread> GetOrCreateForItemAsync(int requesterId, int itemId, CancellationToken ct)
        {
            var item = await _ctx.Items.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == itemId, ct)
                ?? throw new InvalidOperationException($"Item {itemId} not found");

            if (item.UserId == requesterId)
                throw new InvalidOperationException("Cannot start a conversation about your own listing");

            // A conversation about a given listing between the requester and its seller
            // is unique: one thread per (item, pair).
            var channelId = ThreadIdBuilder.CreateForItem(requesterId, item.UserId, itemId);
            var existing = await _ctx.ChatThreads
                .FirstOrDefaultAsync(t => t.ChannelId == channelId, ct);
            if (existing != null) return existing;

            var thread = new ChatThread
            {
                ChannelId = channelId,
                UserAId = Math.Min(requesterId, item.UserId),
                UserBId = Math.Max(requesterId, item.UserId),
                ItemId = item.Id,
                CreatedUtc = DateTime.UtcNow,
                LastMessageUtc = DateTime.UtcNow
            };
            _ctx.ChatThreads.Add(thread);
            await _ctx.SaveChangesAsync(ct);
            _log.LogInformation("Chat thread {ChannelId} created for item {ItemId}", channelId, itemId);
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
