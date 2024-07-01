using static VierGewinnt.Data.Models.GameBoard;

namespace VierGewinnt.Services
{
    public class Game
    {
        public AIBoard board = new AIBoard();
        public BoardMiniMax miniMax = new MiniMaxAlgorithm(11, true, true);
    }
}
