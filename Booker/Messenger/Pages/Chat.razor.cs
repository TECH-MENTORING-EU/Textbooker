using Booker.Messenger.Models;
using Booker.Messenger.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Booker.Messenger.Pages
{
    public partial class Chat : ComponentBase
    {
        [Inject]
        private IMessengerChatService ChatService { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        private const int LoggedUserId = 1;
        private const string LoggedUserName = "Jan Kowalski";
        private const int WeeksPerPage = 4;

        private IReadOnlyList<ChatConversation> _conversations = Array.Empty<ChatConversation>();
        private ChatConversation? _selectedConversation;
        private List<ChatConversationMessage> _messages = new();
        private bool _hasOlderMessages;
        private DateTime? _oldestLoadedDate;
        private bool _isLoadingOlder;
        private string _newMessageText = string.Empty;
        private ElementReference _messagesContainer;

        protected override async Task OnInitializedAsync()
        {
            _conversations = await ChatService.GetConversationsAsync(LoggedUserId, CancellationToken.None);
        }

        private async Task SelectConversation(ChatConversation conversation)
        {
            _selectedConversation = conversation;
            _messages.Clear();
            _oldestLoadedDate = null;
            _hasOlderMessages = false;

            await LoadMessagesPageAsync(olderThan: null);
            await ScrollToBottom();
        }

        private async Task LoadMessagesPageAsync(DateTime? olderThan)
        {
            if (_selectedConversation == null)
            {
                return;
            }

            _isLoadingOlder = true;
            StateHasChanged();

            ChatMessagesPage page = await ChatService.GetMessagesAsync(
                _selectedConversation.Id,
                LoggedUserId,
                olderThan,
                WeeksPerPage,
                CancellationToken.None);

            if (olderThan.HasValue)
            {
                _messages.InsertRange(0, page.Messages);
            }
            else
            {
                _messages = page.Messages;
            }

            _hasOlderMessages = page.HasOlderMessages;
            _oldestLoadedDate = page.OldestMessageDate ?? olderThan;
            _isLoadingOlder = false;
        }

        private async Task LoadOlderMessages()
        {
            if (!_hasOlderMessages || _oldestLoadedDate == null)
            {
                return;
            }

            await LoadMessagesPageAsync(_oldestLoadedDate);
        }

        private async Task SendMessage()
        {
            if (_selectedConversation == null || string.IsNullOrWhiteSpace(_newMessageText))
            {
                return;
            }

            ChatConversationMessage sent = await ChatService.SendMessageAsync(
                _selectedConversation.Id,
                LoggedUserId,
                _newMessageText.Trim(),
                CancellationToken.None);

            _messages.Add(sent);
            _newMessageText = string.Empty;

            // Refresh conversation list to update last message
            _conversations = await ChatService.GetConversationsAsync(LoggedUserId, CancellationToken.None);

            await ScrollToBottom();
        }

        private async Task HandleKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                await SendMessage();
            }
        }

        private async Task ScrollToBottom()
        {
            await Task.Yield();
            try
            {
                await JS.InvokeVoidAsync("eval", "document.querySelector('.chat-messages')?.scrollTo(0, document.querySelector('.chat-messages')?.scrollHeight)");
            }
            catch
            {
                // JS interop may fail during prerendering
            }
        }

        private bool IsLoggedUser(int senderId) => senderId == LoggedUserId;

        private string FormatDate(DateTime utc) => utc.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        private string FormatShortDate(DateTime utc) => utc.ToLocalTime().ToString("dd.MM HH:mm");
    }
}
