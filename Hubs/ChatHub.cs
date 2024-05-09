using Microsoft.AspNetCore.SignalR;

namespace VierGewinnt.Hubs
{
    public class ChatHub : Hub
    {
        static readonly IList<string> players = new List<string>();
        static readonly IDictionary<string, string> onlineUsers = new Dictionary<string, string>();

        public async Task SendNotification(string player)
        {
            await Clients.Others.SendAsync("ReceiveNewUser", player);
        }

        public async Task AddUser(string player)
        {
            if (players.Contains(player))
            {
                return;
            }
            else
            {
                onlineUsers.Add(player, Context.ConnectionId);
                players.Add(player);
            }
            return;
        }

        public async Task GetAvailableUsers()
        {
            await Clients.Caller.SendAsync("ReceiveAvailableUsers", players);
        }

        public async Task LeaveLobby(string userName)
        {
            if (players.Contains(userName))
            {
                players.Remove(userName);
                onlineUsers.Remove(userName);
                await Clients.Others.SendAsync("PlayerLeft", userName);
                return;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName) && players.Contains(userName))
            {
                players.Remove(userName);
                onlineUsers.Remove(userName);
                await Clients.Others.SendAsync("PlayerLeft", userName);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotificateGameStart(string playerOne, string playerTwo)
        {    
            await Clients.All.SendAsync("NavigateToGame", playerOne, playerTwo);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
}
