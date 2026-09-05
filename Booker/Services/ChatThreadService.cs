using Booker.Data;
using Booker.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Booker.Services
{
    public class ChatThreadService(DataContext ctx, ItemManager itemManager, ILogger<ChatThreadService> log) : IChatThreadService
    {
        public async Task<ChatThread?> GetByChannelIdAsync(string channelId, CancellationToken ct)
        {
            return await ctx.ChatThreads.AsNoTracking().FirstOrDefaultAsync(t => t.ChannelId == channelId, ct);
        }

        public async Task<ChatThread> GetOrCreateForItemAsync(int requesterId, int itemId, CancellationToken ct)
        {
            // The requester must be able to see the listing at all: the same
            // visibility and school-isolation rules as the offer page apply,
            // because chat entry bypasses the page's GET authorization.
            var requester = await ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == requesterId, ct)
                ?? throw new InvalidOperationException($"User {requesterId} not found");

            var item = await itemManager.GetItemAsync(itemId, requester)
                ?? throw new InvalidOperationException($"Item {itemId} is not available to user {requesterId}");

            if (item.UserId == requesterId)
            {
                throw new InvalidOperationException("Cannot start a conversation about your own listing");
            }

            // A conversation about a given listing between the requester and its seller
            // is unique: one thread per (item, pair).
            var channelId = ThreadIdBuilder.CreateForItem(requesterId, item.UserId, itemId);
            var existing = await ctx.ChatThreads.FirstOrDefaultAsync(t => t.ChannelId == channelId, ct);
            if (existing != null)
            {
                return existing;
            }

            var thread = new ChatThread
            {
                ChannelId = channelId,
                UserAId = Math.Min(requesterId, item.UserId),
                UserBId = Math.Max(requesterId, item.UserId),
                ItemId = item.Id,
                CreatedUtc = DateTime.UtcNow,
                LastMessageUtc = DateTime.UtcNow
            };
            ctx.ChatThreads.Add(thread);
            try
            {
                await ctx.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // A concurrent "start chat" won the unique ChannelId race; open the same thread.
                var loser = await ctx.ChatThreads.FirstOrDefaultAsync(t => t.ChannelId == channelId, ct);
                if (loser != null)
                {
                    return loser;
                }
                throw;
            }

            log.LogInformation("Chat thread {ChannelId} created for item {ItemId}", channelId, itemId);
            return thread;
        }

        public async Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(int userId, CancellationToken ct)
        {
            return await ctx.ChatThreads.AsNoTracking()
                .Where(t => t.UserAId == userId || t.UserBId == userId)
                .OrderByDescending(t => t.LastMessageUtc)
                .ToListAsync(ct);
        }

        public async Task UpdateLastMessageUtcAsync(string channelId, DateTime utcNow, CancellationToken ct)
        {
            var thread = await ctx.ChatThreads.FirstOrDefaultAsync(t => t.ChannelId == channelId, ct);
            if (thread == null) return;
            thread.LastMessageUtc = utcNow;
            await ctx.SaveChangesAsync(ct);
        }
    }
}
