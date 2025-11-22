using Microsoft.AspNetCore.Components;

namespace Booker.Components
{
    public partial class ChatRealtimeIsland : ChatRealtimeIslandBase { }

    public abstract class ChatRealtimeIslandBase : ComponentBase
    {
        [Parameter] public string DealId { get; set; } = string.Empty;
        protected bool _connected; // placeholder state
        protected List<LiveMessage> _liveMessages = new();

        protected override Task OnInitializedAsync()
        {
            // Placeholder: real-time SignalR client removed until package restored properly.
            _connected = true;
            return Task.CompletedTask;
        }

        protected record LiveMessage(string UserDisplayName, string Content);
    }
}
