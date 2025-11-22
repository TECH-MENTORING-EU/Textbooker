using System.Collections.Concurrent;
using Booker.Services;

namespace Booker.Services
{
    public class InMemoryChatStore
    {
        private readonly ConcurrentDictionary<string, List<ChatMessageDto>> _messages = new();

        public IReadOnlyList<ChatMessageDto> GetMessages(string dealId)
        {
            if (string.IsNullOrWhiteSpace(dealId)) return Array.Empty<ChatMessageDto>();
            var list = _messages.GetOrAdd(dealId, _ => new List<ChatMessageDto>());
            lock (list)
            {
                return list.OrderBy(m => m.CreatedUtc).ToList();
            }
        }

        public ChatMessageDto AddMessage(string dealId, int userId, string userDisplayName, string content)
        {
            var list = _messages.GetOrAdd(dealId, _ => new List<ChatMessageDto>());
            ChatMessageDto dto;
            lock (list)
            {
                int id = list.Count == 0 ? 1 : list[^1].Id + 1;
                dto = new ChatMessageDto(id, dealId, userId, userDisplayName, content, DateTime.UtcNow);
                list.Add(dto);
            }
            return dto;
        }

        public void SeedIfEmpty(string dealId, IEnumerable<ChatMessageDto> seeds)
        {
            var list = _messages.GetOrAdd(dealId, _ => new List<ChatMessageDto>());
            lock (list)
            {
                if (list.Count == 0)
                {
                    list.AddRange(seeds);
                }
            }
        }
    }
}
