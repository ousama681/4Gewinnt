using VierGewinnt.Data.Models;

namespace VierGewinnt.ViewModels
{
    public class GameViewModel
    {

        public string PlayerOne { get; set; }
        public string PlayerTwo { get; set; }
        public int Column { get; set; }
        public GameBoard Board {  get; set; }

        public int MoveNr { get; set; }
    }
}
