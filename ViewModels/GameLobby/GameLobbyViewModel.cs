using Microsoft.AspNetCore.Identity;

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
