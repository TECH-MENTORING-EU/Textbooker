using Booker.Data;

namespace Booker.Services
{
    public interface IChatThreadService
    {
        Task<ChatThread?> GetByChannelIdAsync(string channelId, CancellationToken ct);
        Task<ChatThread> GetOrCreateAsync(int userAId, int userBId, CancellationToken ct);
        Task<IReadOnlyList<ChatThread>> GetThreadsForUserAsync(int userId, CancellationToken ct);
        Task UpdateLastMessageUtcAsync(string channelId, DateTime utcNow, CancellationToken ct);
    }
}
