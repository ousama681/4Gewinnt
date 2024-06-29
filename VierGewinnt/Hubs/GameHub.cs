using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using MQTTBroker;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Xml.Linq;
using VierGewinnt.Data;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Hubs
{
    public class GameHub : Hub
    {
        private static readonly string connectionString = "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

        private static IDictionary<int, GameInfo> runningGames = new Dictionary<int, GameInfo>();

        private static IHubCallerClients _hubClients = null;
        //Speichert die PlayerMoves die es auszuführen gilt. NAch dem Ausführen aus dem Dictionary entfernen.
        //Key: playerId, gameId Value: column
        private static IDictionary<BoardPlayer, int> playerMoves = new Dictionary<BoardPlayer, int>();




        private BoardPlayer? currentMoveKey;

        public async Task SendPlayerMove(string playerId, string gameId, string column)
        {
            // Save the Moves to execute in Dictionary in Case the MQTTService somehow fails. We probably also need to save it in some File. In case the power goes off.
            BoardPlayer bp = new BoardPlayer(playerId, Int32.Parse(gameId));
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

        private static async Task UpdatePlayerRanking(string winnerId)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(GameHub.connectionString);

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    PlayerRanking pr = await dbContext.PlayerRankings.Include(pr => pr.Player).Where(pr => pr.PlayerID.Equals(winnerId)).SingleAsync();

                    if (pr == null)
                    {
                        PlayerRanking newPr = new PlayerRanking() { PlayerID = winnerId, Wins = 1 };
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
                    await _hubClients.All.SendAsync("AnimatePlayerMove", column, bpKey.PlayerId);

                    playerMoves.Remove(bpKey);

                    if (CheckForWin(bpKey.GameId))
                    {
                        GameInfo gi;
                        runningGames.TryGetValue(bpKey.GameId, out gi);
                        await GameIsOver(gi.GetWinner(), bpKey.GameId);
                        await SetIsFinished(bpKey.GameId);
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

        private bool CheckForWin(int gameId)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(GameHub.connectionString);
            ICollection<Move> moves = new List<Move>();

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    moves = dbContext.GameBoards.Include(gb => gb.Moves).Where(gb => gb.ID.Equals(gameId)).First().Moves;

                    Move[,] board = FillBoard(moves);
                    return CheckForWinOrDraw(board, gameId);
                    // Hier mal die Prüfung für den Win machen. 
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return false;
        }

        private Move[,] FillBoard(ICollection<Move> moves)
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

            Move[,] board = new Move[7, 6];

            foreach (var move in moves)
            {
                int column = move.Column;
                int row;
                colDepth.TryGetValue(column.ToString(), out row);
                // Minus 1 wegen Array startindex 0 bei colDepth aber nicht.
                board[column - 1, row - 1] = move;
                colDepth[column.ToString()] = row - 1;
            }

            return board;
        }

        private bool CheckForWinOrDraw(Move[,] board, int gameId)
        {
            GameInfo gameInfo;
            runningGames.TryGetValue(gameId, out gameInfo);
            // Check horizontal
            for (int col = 0; col < 4; col++)
            {
                // Prüfung einbauen, da hier im if Moves Null sein können.
                for (int row = 0; row < 6; row++)
                {
                    string playerId = board[col, row] == null ? null : board[col, row].PlayerID;
                    if (!playerId.IsNullOrEmpty() && (board[col + 1, row] != null && playerId.Equals(board[col + 1, row].PlayerID)) && (board[col + 2, row] != null && playerId.Equals(board[col + 2, row].PlayerID)) && (board[col + 3, row] != null && playerId.Equals(board[col + 3, row].PlayerID)))
                    {
                        gameInfo.SetWinner(playerId);
                        return true;
                    }
                }
            }

            // Check vertical
            for (int col = 0; col < 7; col++)
            {
                for (int row = 0; row < 3; row++)
                {
                    string playerId = board[col, row] == null ? null : board[col, row].PlayerID;
                    if (!playerId.IsNullOrEmpty() && (board[col, row + 1] != null && playerId.Equals(board[col, row + 1].PlayerID)) && (board[col, row + 2] != null && playerId.Equals(board[col, row + 2].PlayerID)) && (board[col, row + 3] != null && playerId.Equals(board[col, row + 3].PlayerID)))
                    {
                        gameInfo.SetWinner(playerId);
                        return true;
                    }
                }
            }

            // Check diagonal (top-left to bottom-right)
            for (int col = 0; col < 4; col++)
            {
                for (int row = 0; row < 3; row++)
                {
                    string playerId = board[col, row] == null ? null : board[col, row].PlayerID;
                    if (!playerId.IsNullOrEmpty() && (board[col + 1, row + 1] != null && playerId.Equals(board[col + 1, row + 1].PlayerID)) && (board[col + 2, row + 2] != null && playerId.Equals(board[col + 2, row + 2].PlayerID)) && (board[col + 3, row + 3] != null && playerId.Equals(board[col + 3, row + 3].PlayerID)))
                    {
                        gameInfo.SetWinner(playerId);
                        return true;
                    }
                }
            }

            // Check diagonal (bottom-left to top-right)
            for (int col = 0; col < 4; col++)
            {
                for (int row = 3; row < 6; row++)
                {
                    string playerId = board[col, row] == null ? null : board[col, row].PlayerID;
                    if (!playerId.IsNullOrEmpty() && (board[col + 1, row - 1] != null && playerId.Equals(board[col + 1, row - 1].PlayerID)) && (board[col + 2, row - 2] != null && playerId.Equals(board[col + 2, row - 2].PlayerID)) && (board[col + 3, row - 3] != null && playerId.Equals(board[col + 3, row - 3].PlayerID)))
                    {
                        gameInfo.SetWinner(playerId);
                        return true;
                    }
                }
            }

            return false;
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


        private class BoardPlayer
        {
            public readonly string PlayerId;
            public readonly int GameId;

            public BoardPlayer(string playerId, int gameId)
            {
                PlayerId = playerId;
                GameId = gameId;
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
