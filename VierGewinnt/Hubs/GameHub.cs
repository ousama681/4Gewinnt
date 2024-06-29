using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
using System.Text;
using VierGewinnt.Controllers;
using VierGewinnt.Data;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Hubs
{
    public class GameHub : Hub
    {
        private static readonly string connectionString = DbUtility.connectionString;

        private static IDictionary<int, GameInfo> runningGames = new Dictionary<int, GameInfo>();

        private static IHubCallerClients _hubClients = null;
        //Speichert die PlayerMoves die es auszuführen gilt. NAch dem Ausführen aus dem Dictionary entfernen.
        //Key: playerName, gameId Value: column
        private static IDictionary<BoardPlayer, int> playerMoves = new Dictionary<BoardPlayer, int>();
        public static int[,] board = new int[6, 7];
        public static IDictionary<int, string> playerNrMappingReverse = new Dictionary<int, string>();
        public static IDictionary<string, int> playerNrMapping = new Dictionary<string, int>();
        public static BoardPlayer playerOne = new BoardPlayer();
        public static BoardPlayer playerTwo = new BoardPlayer();




        private BoardPlayer? currentMoveKey;

        public async Task SendPlayerMove(string playerName, string gameId, string column)
        {

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(GameHub.connectionString);
            // Save the Moves to execute in Dictionary in Case the MQTTService somehow fails. We probably also need to save it in some File. In case the power goes off.
                string playerId = null;
            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    playerId = HomeController.GetUser(playerName, dbContext).Result.Id;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }


            BoardPlayer bp = new BoardPlayer()
            {
                PlayerId = playerId,
                PlayerName = playerName,
                GameId = Int32.Parse(gameId)
            };

            int columnNr = Int32.Parse(column);

            currentMoveKey = bp;

            await SaveMove(bp, columnNr);
            playerMoves.Add(bp, columnNr);

            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", column);
            await SubscribeAsync("feedback");
            // TestMethode um nicht mit Postman den RobotStatus zu simulieren
            //await MQTTBrokerService.PublishAsync("feedback", "1");
        }

        public async Task GameIsOver(string winnerId, int gameId)
        {
            runningGames.Remove(gameId);
            await UpdatePlayerRanking(winnerId);
            await _hubClients.All.SendAsync("NotificateGameEnd", winnerId);
        }

        private static async Task UpdatePlayerRanking(string winnerName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(GameHub.connectionString);


            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
            string playerID = HomeController.GetUser(winnerName, dbContext).Result.Id;
                try
                {
                    PlayerRanking pr = dbContext.PlayerRankings.Include(pr => pr.Player).Where(pr => pr.PlayerID.Equals(playerID)).FirstOrDefault();

                    if (pr == null)
                    {
                        PlayerRanking newPr = new PlayerRanking() { PlayerID = playerID, Wins = 1 };
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

        public void RegisterGameInStaticProperty(string playerIdOne, string playerIdTwo, int gameId)
        {
            if (!runningGames.Keys.Contains(gameId))
            {
                runningGames.Add(gameId, new GameInfo(playerIdOne, playerIdTwo));
            }
        }

        public async Task SubscribeAsync(string topic)
        {

            _hubClients = this.Clients;
            string broker = "localhost";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();

            // Create a MQTT client factory
            var factory = new MqttFactory();

            // Create a MQTT client instance
            IMqttClient mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            // Connect to MQTT broker
            await ConnectToMQTTBroker(mqttClient, options, topic);
        }

        private async Task ConnectToMQTTBroker(IMqttClient mqttClient, MqttClientOptions options, string topic)
        {
            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                // Subscribe to a topic
                await mqttClient.SubscribeAsync(topic);

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    var message = e.ApplicationMessage;


                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                    if (payload.Equals("0"))
                    {
                        return;
                    }

                    if (message.Retain) // Ignore retained messages
                    {
                        return;
                    }

                    BoardPlayer bpKey = currentMoveKey;
                    int column = 0;
                    playerMoves.TryGetValue(bpKey, out column);

                    // Robot did his Move, now we can save it do database
                    // Hier könnte ich die playerID mitgeben, dann wissen wir, wer als nächstes dran ist.
                    // Innerhalb der AnimatePlayerMove Methode wird auch enabled wer am Zug ist.
                    await _hubClients.All.SendAsync("AnimatePlayerMove", column, bpKey.PlayerName);
                    playerMoves.Remove(bpKey);

                    int winnerNr = CheckForWin(bpKey.GameId);

                    if (winnerNr != 0)
                    {
                        string winnername = "";
                        int gameId = 0;
                        if (winnerNr == 1)
                        {
                            winnername = playerOne.PlayerName;
                            gameId = bpKey.GameId;
                        } else if (winnerNr == 2)
                        {
                            winnername = playerTwo.PlayerName;
                            gameId = bpKey.GameId;

                        }
                        //GameInfo gi;
                        //runningGames.TryGetValue(bpKey.GameId, out gi);
                        await GameIsOver(winnername, gameId);
                        await SendRobotGameFinishedMessage();
                        await SetIsFinished(gameId);
                    }

                    await mqttClient.UnsubscribeAsync(topic);
                    await mqttClient.DisconnectAsync();
                    await Task.CompletedTask;
                };
            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

        private async Task SendRobotGameFinishedMessage()
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", "9");
        }

        private async Task SetIsFinished(int gameId)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(GameHub.connectionString);

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    GameBoard gameboard = dbContext.GameBoards.Include(gb => gb.Moves).Where(gb => gb.ID.Equals(gameId)).Single();
                    gameboard.IsFinished = true;
                    dbContext.GameBoards.Update(gameboard);
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

        private int CheckForWin(int gameId)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(GameHub.connectionString);
            ICollection<Move> moves = new List<Move>();

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    //moves = dbContext.GameBoards.Include(gb => gb.Moves).Where(gb => gb.ID.Equals(gameId)).First().Moves;
                    moves = dbContext.Moves.Include(m => m.Player).Where(m => m.GameBoardID.Equals(gameId)).ToList();


                    //Move[,] board = FillBoard(moves);
                    board = new int[6, 7];
                    FillBoard(moves);
                    return CheckForWinOrDraw();
                    // Hier mal die Prüfung für den Win machen. 
                    //}
                    //catch (Exception e)
                    //{
                    //    Debug.WriteLine(e);
                    //}
                } catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
                }
            return 0;
        }


        private int[,] FillBoard(ICollection<Move> moves)
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

                if (move.Player.UserName.Equals(playerOne.PlayerName))
                {
                    playerNr = playerOne.PlayerNr;
                }
                else if (move.Player.UserName.Equals(playerTwo.PlayerName))
                {
                    playerNr = playerTwo.PlayerNr;
                }

                GameHub.board[depth - 1, column - 1] = playerNr;

            }
            return board;
        }

        public static int CheckForWinOrDraw()
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


        private static async Task<Move> SaveMove(BoardPlayer boardPlayer, int column)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(GameHub.connectionString);
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


        public class BoardPlayer
        {
            public string PlayerId;
            public int GameId;
            public string PlayerName;
            public int PlayerNr;

            public BoardPlayer(string playerId, int gameId)
            {
                PlayerId = playerId;
                GameId = gameId;
            }

            public BoardPlayer()
            {

            }
        }


        private class GameInfo
        {
            private readonly string playerOneId;
            private readonly string playerTwoId;

            private string winner;

            public GameInfo(string playerOneId, string playerTwoId)
            {
                this.playerOneId = playerOneId;
                this.playerTwoId = playerTwoId;
            }


            public void SetWinner(string playerId)
            {
                winner = playerId;
            }

            public string GetWinner()
            {
                return winner;
            }
        }

    }
}
