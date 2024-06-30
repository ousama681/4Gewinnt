using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VierGewinnt.Data;
using VierGewinnt.Data.Model;
using VierGewinnt.Hubs;
using static VierGewinnt.Hubs.GameHub;

namespace VierGewinnt.Services
{
    public class GameManager
    {

        public static int[,] FillBoard(ICollection<Move> moves)
        {
            IDictionary<string, int> colDepth = new Dictionary<string, int>
            {
                { "1", 6 },
                { "2", 6 },
                { "3", 6 },
                { "4", 6 },
                { "5", 6 },
                { "6", 6 },
                { "7", 6 }
            };

            int[,] board = new int[6, 7];

            foreach (Move move in moves)
            {
                int depth;
                colDepth.TryGetValue(move.Column.ToString(), out depth);

                colDepth[move.Column.ToString()] = depth - 1;

                int column = move.Column;

                int playerNr = 0;

                if (move.Player.UserName.Equals(GameHub.playerOne.PlayerName))
                {
                    playerNr = GameHub.playerOne.PlayerNr;
                }
                else if (move.Player.UserName.Equals(GameHub.playerTwo.PlayerName))
                {
                    playerNr = GameHub.playerTwo.PlayerNr;
                }

                board[depth - 1, column - 1] = playerNr;

            }
            return board;
        }


        public static int CheckForWinOrDraw(int[,] board)
        {
            // Check horizontal
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7 - 3; col++)
                {
                    if (board[row, col] != 0 &&
                        board[row, col] == board[row, col + 1] &&
                        board[row, col] == board[row, col + 2] &&
                        board[row, col] == board[row, col + 3])
                    {
                        //robotMappingReversed.TryGetValue(board[row, col], out winner);
                        return board[row, col];
                    }
                }
            }

            // Check vertical
            for (int col = 0; col < 7; col++)
            {
                for (int row = 0; row < 6 - 3; row++)
                {
                    if (board[row, col] != 0 &&
                        board[row, col] == board[row + 1, col] &&
                        board[row, col] == board[row + 2, col] &&
                        board[row, col] == board[row + 3, col])
                    {
                        return board[row, col];
                    }
                }
            }

            // Check positive diagonal (bottom-left to top-right)
            for (int col = 0; col < 7 - 3; col++)
            {
                for (int row = 0; row < 6 - 3; row++)
                {
                    if (board[row, col] != 0 &&
                        board[row, col] == board[row + 1, col + 1] &&
                        board[row, col] == board[row + 2, col + 2] &&
                        board[row, col] == board[row + 3, col + 3])
                    {
                        return board[row, col];
                    }
                }
            }

            // Check negative diagonal (top-left to bottom-right)
            for (int col = 0; col < 7 - 3; col++)
            {
                for (int row = 3; row < 6; row++)
                {
                    if (board[row, col] != 0 &&
                        board[row, col] == board[row - 1, col + 1] &&
                        board[row, col] == board[row - 2, col + 2] &&
                        board[row, col] == board[row - 3, col + 3])
                    {
                        return board[row, col];
                    }
                }
            }

            // Check for draw (if no empty cells)
            foreach (var cell in board)
            {
                if (cell == 0)
                {
                    return 0; // No winner yet
                }
            }

            return -1; // Draw
        }


        public static int CheckForWin(int gameId)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);
            ICollection<Move> moves = new List<Move>();

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    moves = dbContext.Moves.Include(m => m.Player).Where(m => m.GameBoardID.Equals(gameId)).ToList();
                    //board = new int[6, 7];
                    //FillBoard(moves);
                    board = GameManager.FillBoard(moves);
                    return GameManager.CheckForWinOrDraw(board);

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return 0;
        }


        public static async Task<Move> SaveMove(BoardPlayer boardPlayer, int column)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);
            Move move = new Move();

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    move.MoveNr = dbContext.Moves.Where(m => m.GameBoardID.Equals(boardPlayer.GameId)).Count() + 1;
                    move.Column = column;
                    move.GameBoardID = boardPlayer.GameId;
                    move.PlayerID = boardPlayer.PlayerId;
                    await dbContext.Moves.AddAsync(move);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return move;
        }


    }
}
