using Microsoft.AspNetCore.Identity;
using VierGewinnt.Data.Models;

namespace VierGewinnt.ViewModels
{
    public class GameLobbyViewModel
    {
        public IEnumerable<string> Playernames { get; set; }
        public ApplicationUser PlayerOne { get; set; }
        public IEnumerable<Robot> Robots { get; set; }

        public GameLobbyViewModel()
        {
            Playernames = new List<string>();
        }
    }
}
