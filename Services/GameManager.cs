using static VierGewinnt.Data.Models.GameBoard;

namespace VierGewinnt.Services
{
    public class GameManager
    {
        public void ManageGame()
        {
            var board = new Board(7, 6);
            var random = new Random();

            while (true)
            {
                Console.WriteLine("Pick a column 1 - 7");
                int move;
                if (!int.TryParse(Console.ReadLine(), out move) || move < 1 || move > 7)
                {
                    Console.WriteLine("Must enter a number 1 - 7");
                    continue;
                }

                if (!board.DropCoin(1, move - 1))
                {
                    Console.WriteLine("That column is full, pick another one");
                    continue;
                }

                if (board.Winner == 1)
                {
                    Console.WriteLine(board);
                    Console.WriteLine("You win!");
                    break;
                }

                if (board.IsFull)
                {
                    Console.WriteLine(board);
                    Console.WriteLine("Tie!");
                    break;
                }

                var moves = new List<Tuple<int, int>>();
                for (int i = 0; i < board.Columns; i++)
                {
                    if (!board.DropCoin(2, i))
                        continue;
                    moves.Add(Tuple.Create(i, ConnectFourAIService.MinMax(5, board, false)));
                    board.RemoveTopCoin(i);
                }

                int maxMoveScore = moves.Max(t => t.Item2);
                var bestMoves = moves.Where(t => t.Item2 == maxMoveScore).ToList();
                board.DropCoin(2, bestMoves[random.Next(0, bestMoves.Count)].Item1);
                Console.WriteLine(board);

                if (board.Winner == 2)
                {
                    Console.WriteLine("You lost!");
                    break;
                }

                if (board.IsFull)
                {
                    Console.WriteLine("Tie!");
                    break;
                }
            }

            Console.WriteLine("DONE");
            Console.ReadKey();
        }
    }
}
