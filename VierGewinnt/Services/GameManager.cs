using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VierGewinnt.Controllers;
using VierGewinnt.Data;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using static VierGewinnt.Hubs.GameHub;

namespace VierGewinnt.Services
{
    public class GameManager
    {
        public static string playerOneName = "";
        public static string playerTwoName = "";
        public static int playerOneNr = 1;
        public static int playerTwoNr = 2;
        public static string currentColumn = "";



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

                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseSqlServer(DbUtility.connectionString);
                ApplicationUser user = null;

                using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
                {
                    try
                    {

                        user = DbUtility.GetUser(move.PlayerName, dbContext).Result;

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }

                if (move.PlayerName.Equals(playerOneName))
                {
                    playerNr = playerOneNr;
                }
                else if (move.PlayerName.Equals(playerTwoName))
                {
                    playerNr = playerTwoNr;
                }

                board[depth - 1, column - 1] = playerNr;

            }
            return board;
        }


        public static int CheckForWinOrDraw(int[,] board)
        {
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7 - 3; col++)
                {
                    if (board[row, col] != 0 &&
                        board[row, col] == board[row, col + 1] &&
                        board[row, col] == board[row, col + 2] &&
                        board[row, col] == board[row, col + 3])
                    {
                        return board[row, col];
                    }
                }
            }

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

            foreach (var cell in board)
            {
                if (cell == 0)
                {
                    return 0; 
                }
            }

            return -1;
        }


        public static int CheckForWin(int[,] board)
        {
            return GameManager.CheckForWinOrDraw(board);
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
                    move.PlayerName = boardPlayer.PlayerName;
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

        public static async Task<GameBoard> CreateBoardEntityAsync(string playerOne, string playerTwo)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);

            GameBoard game = new GameBoard();

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    var userOne = DbUtility.GetUser(playerOne, dbContext).Result;
                    var userTwo = DbUtility.GetUser(playerTwo, dbContext).Result;

                    if (userOne != null && userTwo != null)
                    {
                        game.PlayerOneID = userOne.Id;
                        game.PlayerOneName = userOne.UserName;
                        game.PlayerTwoID = userTwo.Id;
                        game.PlayerTwoName = userTwo.UserName;
                    }
                    else if (userOne != null)
                    {
                        game.PlayerOneID = userOne.Id;
                        game.PlayerOneName = userOne.UserName;
                        game.PlayerTwoID = playerTwo;
                        game.PlayerTwoName = playerTwo;
                    }
                    else
                    {
                        game.PlayerOneID = playerOne;
                        game.PlayerOneName = playerOne;
                        game.PlayerTwoID = playerTwo;
                        game.PlayerTwoName = playerTwo;
                    }
                    await dbContext.GameBoards.AddAsync(game);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Game already exists!");
                }
            }
            return game;
        }

        public static async Task<GameBoard> CheckForExistingGame(string playerOne, string playerTwo)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);

            GameBoard game = new GameBoard();

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    //string playerOneID = DbUtility.GetUser(playerOne, dbContext).Result.Id;
                    //string playerTwoID = DbUtility.GetUser(playerTwo, dbContext).Result.Id;



                    GameBoard existingGame = await dbContext.GameBoards.Include(gb => gb.Moves).Where(gb => (gb.PlayerOneName.Equals(playerOne) && gb.PlayerTwoName.Equals(playerTwo) && gb.IsFinished.Equals(false)) ||
                    (gb.PlayerOneName.Equals(playerOne) && gb.PlayerTwoName.Equals(playerTwo) && gb.IsFinished.Equals(false))).FirstOrDefaultAsync();
                    return existingGame;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return game;
        }


        public static async Task UpdatePlayerRanking(string winnerName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);


            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                string playerID = DbUtility.GetUser(winnerName, dbContext).Result.Id;
                try
                {
                    PlayerRanking pr = dbContext.PlayerRankings.Where(pr => pr.PlayerName.Equals(winnerName)).FirstOrDefault();

                    if (pr == null)
                    {
                        PlayerRanking newPr = new PlayerRanking() { PlayerName = winnerName, Wins = 1 };
                        await dbContext.AddAsync(newPr);
                    }
                    else
                    {
                        pr.Wins = pr.Wins + 1;
                    }

                    await dbContext.SaveChangesAsync();
                    return;
                    // Hier mal die Prüfung für den Win machen. 
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public static async Task SetIsFinished(int gameId)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    GameBoard gameboard = dbContext.GameBoards.Include(gb => gb.Moves).Where(gb => gb.ID.Equals(gameId)).Single();
                    gameboard.IsFinished = true;
                    dbContext.GameBoards.Update(gameboard);
                    await dbContext.SaveChangesAsync();
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
    }
}
