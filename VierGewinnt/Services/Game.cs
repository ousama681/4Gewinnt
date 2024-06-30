using static VierGewinnt.Data.Models.GameBoard;

namespace VierGewinnt.Services
{
    public class Game
    {
        public AIBoard board = new AIBoard();
        public MiniMaxAlgorithm miniMax = new MiniMaxAlgorithm(11, true, true);
    }
}
