using Booker.Messenger.Models;

namespace Booker.Messenger.Services
{
    public class MockMessengerChatService : IMessengerChatService
    {
        private const int LoggedUserId = 1;
        private const string LoggedUserName = "Jan Kowalski";
        private const int WeeksPerPage = 4;

        private readonly List<ChatConversation> _conversations;
        private readonly List<ChatConversationMessage> _messages;

        public MockMessengerChatService()
        {
            _conversations = GenerateConversations();
            _messages = GenerateMessages();
        }

        public Task<IReadOnlyList<ChatConversation>> GetConversationsAsync(int loggedUserId, CancellationToken ct)
        {
            IReadOnlyList<ChatConversation> result = _conversations
                .OrderByDescending(c => c.LastMessageUtc)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<ChatMessagesPage> GetMessagesAsync(int conversationId, int loggedUserId, DateTime? olderThan, int weeksSpan, CancellationToken ct)
        {
            DateTime cutoffEnd = olderThan ?? DateTime.UtcNow;
            DateTime cutoffStart = cutoffEnd.AddDays(-weeksSpan * 7);

            List<ChatConversationMessage> filtered = _messages
                .Where(m => m.ConversationId == conversationId)
                .Where(m => m.CreatedUtc >= cutoffStart && m.CreatedUtc < cutoffEnd)
                .OrderBy(m => m.CreatedUtc)
                .ToList();

            bool hasOlder = _messages
                .Any(m => m.ConversationId == conversationId && m.CreatedUtc < cutoffStart);

            DateTime? oldest = filtered.Count > 0 ? filtered.First().CreatedUtc : null;

            var page = new ChatMessagesPage
            {
                Messages = filtered,
                HasOlderMessages = hasOlder,
                OldestMessageDate = oldest
            };

            return Task.FromResult(page);
        }

        public Task<ChatConversationMessage> SendMessageAsync(int conversationId, int loggedUserId, string content, CancellationToken ct)
        {
            int newId = _messages.Count > 0 ? _messages.Max(m => m.Id) + 1 : 1;
            var message = new ChatConversationMessage
            {
                Id = newId,
                ConversationId = conversationId,
                SenderId = loggedUserId,
                SenderName = LoggedUserName,
                Content = content,
                CreatedUtc = DateTime.UtcNow
            };
            _messages.Add(message);

            ChatConversation? conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conversation != null)
            {
                conversation.LastMessage = content;
                conversation.LastMessageUtc = message.CreatedUtc;
            }

            return Task.FromResult(message);
        }

        private static List<ChatConversation> GenerateConversations()
        {
            string[] names = { "Anna Nowak", "Piotr Wi?niewski", "Marta Kami?ska", "Tomek Zieli?ski", "Kasia Lewandowska", "Micha? Szyma?ski", "Ola D?browska", "Bartek Wójcik" };

            return names.Select((name, index) => new ChatConversation
            {
                Id = index + 1,
                UserName = name,
                UserId = index + 100,
                LastMessage = $"Ostatnia wiadomo?? od {name.Split(' ')[0]}...",
                LastMessageUtc = DateTime.UtcNow.AddHours(-(index * 3 + 1))
            }).ToList();
        }

        private static List<ChatConversationMessage> GenerateMessages()
        {
            var messages = new List<ChatConversationMessage>();
            int id = 1;

            string[] otherNames = { "Anna Nowak", "Piotr Wi?niewski", "Marta Kami?ska", "Tomek Zieli?ski", "Kasia Lewandowska", "Micha? Szyma?ski", "Ola D?browska", "Bartek Wójcik" };

            string[][] sampleTexts =
            {
                new[] { "Cze??, masz jeszcze t? ksi??k??", "Tak, jest dost?pna!", "Ile chcesz za ni??", "150 z?, stan bardzo dobry.", "Mog? odebra? jutro?", "Jasne, pasuje mi po 15.", "Super, to do zobaczenia!", "Dzi?ki, do jutra!" },
                new[] { "Hej, widzia?em twoje og?oszenie.", "Tak, co Ci? interesuje?", "Podr?cznik do matematyki.", "Mam go, wydanie z 2023.", "Jest co? podkre?lone?", "Nie, czysty jak nowy.", "To bior?, kiedy mog? odebra??", "Jutro na uczelni?" },
                new[] { "Czy ten atlas jest jeszcze dost?pny?", "Tak, mog? go przes?a? poczt?.", "Ile kosztuje wysy?ka?", "Mog? nada? za 12 z?.", "OK, bior?. Podaj numer konta.", "Wy?l? Ci na priv.", "Dzi?ki, czekam na paczk?.", "Nada?am dzisiaj, ?ledzenie wy?l? wieczorem." },
                new[] { "Siema, masz co? z fizyki?", "Mam Hallidaya, 4 wydanie.", "O, to szukam! Jaki stan?", "Lekko przetarte rogi, w ?rodku OK.", "Za ile oddasz?", "80 z?, bo kupowa?em za 120.", "Stoi, spotkajmy si? w czwartek.", "Czwartek jest git, napisz o 12." },
                new[] { "Hej, potrzebuj? lektur na polski.", "Mam Lalk? i Ferdydurke.", "Obie bior?! Ile razem?", "30 z? za obie.", "Super, mog? dzi? odebra??", "Dzisiaj nie dam rady, jutro?", "OK, jutro po zaj?ciach.", "Pasuje, napisz jak b?dziesz blisko." },
                new[] { "Cze??, masz jeszcze tamten s?ownik?", "Tak, angielsko-polski Oxford.", "Idealne. Ile?", "40 z?.", "Troch? du?o, dasz za 30?", "Dobra, 30 niech b?dzie.", "Dzi?ki! Kiedy si? spotkamy?", "W pi?tek na kampusie?" },
                new[] { "Hej, widz? ?e sprzedajesz zeszyty z wyk?adów.", "Tak, z algebry i analizy.", "Z analizy poprosz?!", "Jasne, s? z zesz?ego semestru.", "To super, w?a?nie tego szukam.", "Mog? je przynie?? jutro.", "?wietnie, ile p?ac??", "Daj 15 z? i jest OK." },
                new[] { "Siema, potrzebuj? ksi??ki do programowania.", "Mam Clean Code i Pragmatic Programmer.", "Clean Code mnie interesuje.", "Stan idealny, nie otwierana prawie.", "Ile chcesz?", "60 z?.", "Bior?, spotkajmy si? w tym tygodniu.", "?roda Ci pasuje?" }
            };

            for (int convIndex = 0; convIndex < otherNames.Length; convIndex++)
            {
                int conversationId = convIndex + 1;
                int otherUserId = convIndex + 100;
                string otherName = otherNames[convIndex];
                string[] texts = sampleTexts[convIndex];

                // Generate messages spread across ~6 weeks so we can test paging
                DateTime startDate = DateTime.UtcNow.AddDays(-42);

                for (int msgIndex = 0; msgIndex < texts.Length; msgIndex++)
                {
                    bool isLoggedUser = msgIndex % 2 == 1;
                    messages.Add(new ChatConversationMessage
                    {
                        Id = id++,
                        ConversationId = conversationId,
                        SenderId = isLoggedUser ? LoggedUserId : otherUserId,
                        SenderName = isLoggedUser ? LoggedUserName : otherName,
                        Content = texts[msgIndex],
                        CreatedUtc = startDate.AddDays(msgIndex * 6).AddHours(msgIndex * 2)
                    });
                }

                // Add some recent messages (last 2 weeks)
                for (int recent = 0; recent < 4; recent++)
                {
                    bool isLoggedUser = recent % 2 == 0;
                    messages.Add(new ChatConversationMessage
                    {
                        Id = id++,
                        ConversationId = conversationId,
                        SenderId = isLoggedUser ? LoggedUserId : otherUserId,
                        SenderName = isLoggedUser ? LoggedUserName : otherName,
                        Content = isLoggedUser
                            ? $"Jasne, dogadamy si? {otherName.Split(' ')[0]}."
                            : $"Hej, wracam do tematu z ksi??k?.",
                        CreatedUtc = DateTime.UtcNow.AddDays(-10 + recent * 3).AddHours(recent)
                    });
                }
            }

            return messages;
        }
    }
}
