using static VierGewinnt.Data.Models.GameBoard;

namespace VierGewinnt.Services
{
    public class ConnectFourAIService
    {
        //public void CalculateBestMove(GameBoard board, string color)
        //{

        //}

        public static int MinMax(int depth, Board board, bool maximizingPlayer)
        {
            if (depth <= 0)
                return 0;

            var winner = board.Winner;
            if (winner == 2)
                return depth;
            if (winner == 1)
                return -depth;
            if (board.IsFull)
                return 0;


            int bestValue = maximizingPlayer ? -1 : 1;
            for (int i = 0; i < board.Columns; i++)
            {
                if (!board.DropCoin(maximizingPlayer ? 2 : 1, i))
                    continue;
                int v = MinMax(depth - 1, board, !maximizingPlayer);
                bestValue = maximizingPlayer ? Math.Max(bestValue, v) : Math.Min(bestValue, v);
                board.RemoveTopCoin(i);
            }

            return bestValue;
        }
    }
}
