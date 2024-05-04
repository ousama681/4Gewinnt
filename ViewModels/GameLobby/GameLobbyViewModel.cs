using Microsoft.AspNetCore.Identity;
using VierGewinnt.Data.Models;

namespace VierGewinnt.ViewModels.GameLobby
{
    public class GameLobbyViewModel
    {
        public IEnumerable<string> Playernames {  get; set; }
        public ApplicationUser PlayerOne { get; set; }



        public GameLobbyViewModel()
        {
            Playernames = new List<string>();
        }
    }
}
