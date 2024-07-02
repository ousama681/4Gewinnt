using static VierGewinnt.Data.Models.GameBoard;

namespace VierGewinnt.Services.AI
{
    public class BoardGame
    {
        public AIBoard board = new AIBoard();
        public BoardMiniMax miniMax = new BoardMiniMax(11);
    }
}
