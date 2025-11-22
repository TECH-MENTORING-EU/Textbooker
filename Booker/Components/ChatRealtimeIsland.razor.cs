using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Booker.Services;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Web; // for KeyboardEventArgs

namespace Booker.Components
{
    public partial class ChatRealtimeIsland : ComponentBase, IAsyncDisposable
    {
        [Parameter] public string DealId { get; set; } = string.Empty;
        [Parameter] public IEnumerable<ChatMessageDto> InitialMessages { get; set; } = Enumerable.Empty<ChatMessageDto>();
        [Parameter] public int CurrentUserId { get; set; }

        private enum ConnectionStatus { Connecting, Connected, Reconnecting, Disconnected }

        [Inject] private IJSRuntime Js { get; set; } = default!;

        private HubConnection? _hub;
        private ConnectionStatus _status = ConnectionStatus.Connecting;
        private readonly List<ChatMessageDto> _messages = new();
        private string _draft = string.Empty;
        private bool _sending;
        private int _lastRenderedCount;
        private ElementReference _historyRef;
        private ElementReference _inputRef;

        public bool _canSend => _status == ConnectionStatus.Connected && !string.IsNullOrWhiteSpace(_draft.Trim());

        protected override async Task OnInitializedAsync()
        {
            _messages.AddRange(InitialMessages);

            if (string.IsNullOrWhiteSpace(DealId))
            {
                _status = ConnectionStatus.Disconnected;
                return;
            }

            _hub = new HubConnectionBuilder()
                .WithUrl($"/hubs/chat?dealId={Uri.EscapeDataString(DealId)}")
                .WithAutomaticReconnect()
                .Build();

            _hub.Reconnecting += _ =>
            {
                _status = ConnectionStatus.Reconnecting;
                InvokeAsync(StateHasChanged);
                return Task.CompletedTask;
            };

            _hub.Reconnected += _ =>
            {
                _status = ConnectionStatus.Connected;
                InvokeAsync(StateHasChanged);
                return Task.CompletedTask;
            };

            _hub.Closed += _ =>
            {
                _status = ConnectionStatus.Disconnected;
                InvokeAsync(StateHasChanged);
                return Task.CompletedTask;
            };

            _hub.On<object>("MessageAdded", payload =>
            {
                if (payload is null) return;
                var props = payload.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(payload));

                int userId = int.TryParse(Safe(props, "UserId"), out var parsed) ? parsed : 0;

                _messages.Add(new ChatMessageDto(
                    Id: int.TryParse(Safe(props, "Id"), out var id) ? id : 0,
                    DealId: Safe(props, "DealId"),
                    UserId: userId,
                    UserDisplayName: Safe(props, "UserDisplayName"),
                    Content: Safe(props, "Content"),
                    CreatedUtc: DateTime.TryParse(Safe(props, "CreatedUtc"), out var dt) ? dt : DateTime.UtcNow));

                InvokeAsync(StateHasChanged);
            });

            await _hub.StartAsync();
            _status = ConnectionStatus.Connected;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_messages.Count != _lastRenderedCount)
            {
                _lastRenderedCount = _messages.Count;
                await Js.InvokeVoidAsync("chatIsland.scrollIfNearBottom", _historyRef);
            }

            if (firstRender && _messages.Count > 0)
            {
                await Js.InvokeVoidAsync("chatIsland.scrollToBottom", _historyRef);
            }
        }

        public async Task SendAsync()
        {
            if (!_canSend || _sending || _hub == null) return;
            string toSend = Sanitize(_draft);
            if (string.IsNullOrEmpty(toSend)) return;

            _sending = true;
            try
            {
                await _hub.SendAsync("SendMessage", DealId, toSend);
                _draft = string.Empty;
                await _inputRef.FocusAsync();
            }
            finally
            {
                _sending = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task HandleKey(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                // remove trailing newline that might be inserted before send
                _draft = _draft.TrimEnd('\r', '\n');
                await SendAsync();
            }
        }

        private static string Sanitize(string input)
        {
            string trimmed = input.Trim();
            if (trimmed.Length > 500) trimmed = trimmed[..500];
            trimmed = trimmed.Replace('\r', '\n');
            return trimmed;
        }

        private static string Safe(Dictionary<string, object?> dict, string key) =>
            dict.TryGetValue(key, out var v) ? v?.ToString() ?? string.Empty : string.Empty;

        public async ValueTask DisposeAsync()
        {
            if (_hub is not null)
            {
                await _hub.DisposeAsync();
            }
        }
    }
}
