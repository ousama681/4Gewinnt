using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;

namespace VierGewinnt.ViewModels
{
    public class SpielRueckblickViewModel
    {
        public GameBoard Game {  get; set; }
        public Stack<Move> MovesLeft { get; set; }
        public Queue<Move> MovesPlayed { get; set; }
    }
}
