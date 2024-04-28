using Microsoft.AspNetCore.SignalR;
using System.Numerics;
using VierGewinnt.Models;

namespace VierGewinnt.Hubs
{
    public class ChatHub : Hub
    {
        static readonly IList<string> players = new List<string>();
        static readonly IDictionary<string, string> onlineUsers = new Dictionary<string, string>();

        //public async Task SendNotification(string player, string message)

        // Unser Problem ist, dass wenn jemand schon drinn ist, er dann nicht sieht

        public async Task SendNotification(string player)
        {

            // Entweder wir laden hier die User die i´n der chatLobby drin sind.
            // Aber dann müssen wir eigentlich für jeden User der sich mit dem Hub connected in einer Tabelle in der DB speichern.
            await Clients.Others.SendAsync("ReceiveNewUser", player);

            ////await Clients.All.SendAsync("ReceiveMessage", player, message);
        }

        public async Task AddUser(string player)
        {
            if (players.Contains(player))
            {
                return;
            }
            else
            {
                onlineUsers.Add(Context.ConnectionId, player);
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
                onlineUsers.Remove(Context.ConnectionId);
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
                onlineUsers.Remove(Context.ConnectionId);
                await Clients.Others.SendAsync("PlayerLeft", userName);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
