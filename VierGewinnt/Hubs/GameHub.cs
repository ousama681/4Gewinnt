using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
using System.Text;
using VierGewinnt.Controllers;
using VierGewinnt.Data;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using VierGewinnt.Services;
using VierGewinnt.Services.AI;

namespace VierGewinnt.Hubs
{
    public class GameHub : BoardHubBase
    {
        //private static IDictionary<int, GameInfo> runningGames = new Dictionary<int, GameInfo>();

        //private static IHubCallerClients _hubContextPvP = null;

        public static IHubCallerClients _hubcontextPvP = null;

        private static IDictionary<BoardPlayer, int> playerMoves = new Dictionary<BoardPlayer, int>();
        public static int[,] board = new int[6, 7];
        public static IDictionary<int, string> playerNrMappingReverse = new Dictionary<int, string>();
        public static IDictionary<string, int> playerNrMapping = new Dictionary<string, int>();
        public static BoardPlayer playerOne = new BoardPlayer();
        public static BoardPlayer playerTwo = new BoardPlayer();


        public static string currentcolumn = "";
        public static int currPlayerNr = 0;
        public static IDictionary<string, int> colDepth = new Dictionary<string, int>();


        private static IMqttClient hubMqttClient = null;



        private static BoardPlayer? currentMoveKey;



        public async Task SendPlayerMove(string playerName, string gameId, string column)
        {

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);
            string playerId = null;
            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    playerId = DbUtility.GetUser(playerName, dbContext).Result.Id;
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

            await GameManager.SaveMove(bp, columnNr);
            playerMoves.Add(bp, columnNr);

            BoardGame game = new BoardGame();
            game.board.board = board;
            currentcolumn = column;

            AddMoveToBoard();

            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", column);
            await SubscribeToFeedbackAsync("feedback");
        }

        public static async Task GameIsOver(string winnerId, int gameId)
        {
            await GameManager.UpdatePlayerRanking(winnerId);
            await _hubcontextPvP.All.SendAsync("NotificateGameEnd", winnerId);
        }

        public override Task OnConnectedAsync()
        {

            _hubcontextPvP = this.Clients;
            return base.OnConnectedAsync();
        }

        public async Task SubscribeToFeedbackAsync(string topic)
        {

            string broker = "localhost";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();
            var factory = new MqttFactory();

            IMqttClient mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            await ConnectToMQTTBroker(mqttClient, options, topic);
        }

        private async Task ConnectToMQTTBroker(IMqttClient mqttClient, MqttClientOptions options, string topic)
        {
            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                await mqttClient.SubscribeAsync(topic);

                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    var message = e.ApplicationMessage;
                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                    if (payload.Equals("0"))
                    {
                        return;
                    }

                    if (message.Retain)
                    {
                        return;
                    }

                    BoardPlayer bpKey = currentMoveKey;
                    int column = 0;
                    playerMoves.TryGetValue(bpKey, out column);
                    await _hubcontextPvP.All.SendAsync("AnimatePlayerMove", column, bpKey.PlayerName);
                    playerMoves.Remove(bpKey);

                    int winnerNr = GameManager.CheckForWin(board);

                    if (winnerNr != 0)
                    {
                        string winnername = "";
                        int gameId = 0;
                        if (winnerNr == 1)
                        {
                            winnername = GameManager.playerOneName;
                            gameId = bpKey.GameId;
                        }
                        else if (winnerNr == 2)
                        {
                            winnername = GameManager.playerTwoName;
                            gameId = bpKey.GameId;

                        }
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

        public override void AddMoveToBoard()
        {
            int columnInt = int.Parse(currentcolumn);
            int depth;


            colDepth.TryGetValue(currentcolumn, out depth);
            colDepth[currentcolumn] = depth - 1;

            board[depth - 1, columnInt - 1] = currPlayerNr;

            if (currPlayerNr == 1)
            {
                currPlayerNr = 2;
            }
            else if (currPlayerNr == 2)
            {
                currPlayerNr = 1;
            }

            return;
        }


        internal static void InitColDepth()
        {
            colDepth = new Dictionary<string, int>();

            colDepth.Add("1", 6);
            colDepth.Add("2", 6);
            colDepth.Add("3", 6);
            colDepth.Add("4", 6);
            colDepth.Add("5", 6);
            colDepth.Add("6", 6);
            colDepth.Add("7", 6);
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

    }
}
