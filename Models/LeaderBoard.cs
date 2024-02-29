using System.ComponentModel.DataAnnotations;

namespace VierGewinnt.Models
{
    public class LeaderBoard
    {
        [Display(Name = "Playername")]
        public string? PlayerName { get; set; }

        [Display(Name = "Rang")]
        public string? Rank { get; set; }
    }
}
