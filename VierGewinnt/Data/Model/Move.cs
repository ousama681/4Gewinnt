using Microsoft.AspNetCore.Identity;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Data.Model
{
    public class Move
    {
        public int ID { get; set; }
        public int MoveNr { get; set; }
        public string PlayerID { get; set; }
        public int GameBoardID { get; set; }
        public int Column {  get; set; }
        public GameBoard GameBoard { get; set; }
        public ApplicationUser Player { get; set; }
    }
}
