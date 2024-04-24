using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace VierGewinnt.ViewModels.GameLobby
{
    public class GameLobbyViewModel : Hub
    {
        public IEnumerable<string> Playernames {  get; set; }
        public IdentityUser PlayerOne { get; set; }



        public GameLobbyViewModel()
        {
            Playernames = new List<string>();
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
