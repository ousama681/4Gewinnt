using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace VierGewinnt.ViewModels.GameLobby
{
    public class GameLobbyViewModel
    {
        public IEnumerable<string> Playernames {  get; set; }
        public IdentityUser PlayerOne { get; set; }



        public GameLobbyViewModel()
        {
            Playernames = new List<string>();
        }
    }
}
