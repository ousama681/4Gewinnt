using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Diagnostics;
using System.Text;
using VierGewinnt.Controllers;
using VierGewinnt.Data;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using VierGewinnt.Services;
using VierGewinnt.Services.AI;
using static VierGewinnt.Hubs.GameHub;

namespace VierGewinnt.Hubs
{
    public class BoardPvEHub : BoardHubBase
    {
        private static BoardPlayer? currentMoveKey;
        private static IDictionary<BoardPlayer, int> playerMoves = new Dictionary<BoardPlayer, int>();
        public static string currentPlayer = "";
        public static string currentcolumn = "";
        public static string robotName = "";
        public static string playerName = "";
        public static int currGameId = 0;
        public static int moveNr = 0;

        public static int currPlayerNr = 0;
        public static IDictionary<string, int> colDepth = new Dictionary<string, int>();
        public static IDictionary<int, string> robotMappingReversed = new Dictionary<int, string>();

        public static int[,] board = new int[6, 7];
        public static IHubCallerClients _hubcontextPvE;

        public async Task PublishToCoordinate(string column)
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", column);
        }

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

            currentcolumn = column;
            int columnNr = Int32.Parse(column);

            currentMoveKey = bp;

            await GameManager.SaveMove(bp, columnNr);
            playerMoves.Add(bp, columnNr);

            AddMoveToBoard();
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", column);
            await SubscribeToFeedbackAsync("feedback");
        }

        public async Task MakeNextMove()
        {
            BoardGame game = new BoardGame();
            game.board.board = board;

             currentcolumn = game.miniMax.GetBestMove(game.board).Column.ToString();

            // NextMove wird an beide Roboter verschickt.
            await PublishToCoordinate(currentcolumn);
            await SubscribeToFeedbackAsync("feedback");
            moveNr++;
            AddMoveToBoard();
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

                    if (payload.Equals("0") || message.Retain)
                    {
                        return;
                    }

                    BoardPlayer bpKey = currentMoveKey;
                    int column = 0;
                    playerMoves.TryGetValue(bpKey, out column);

                    //AddMoveToBoard();

                    if (currentPlayer.Equals(playerName))
                    {
                        currentcolumn = column.ToString();
                        //AddMoveToBoard();
                        await SendAnimatePlayerMove(currentcolumn, currentPlayer,_hubcontextPvE);
                    }
                    else
                    {
                        await GameManager.SaveMove(new BoardPlayer()
                        {
                            GameId = currGameId,
                            PlayerId = robotName,
                            PlayerName = robotName,
                            PlayerNr = 0

                        }, Int32.Parse(currentcolumn));
                        await SendAnimatePlayerMove(currentcolumn, currentPlayer, _hubcontextPvE);
                    }
                    playerMoves.Remove(bpKey);

                    // noch schauen warum board nicht stimmt.

                    int winnerNr = GameManager.CheckForWin(board);

                    if (winnerNr != 0)
                    {
                        string winnername = "";
                        int gameId = 0;
                        if (winnerNr == 1)
                        {
                            winnername = playerName;
                            gameId = bpKey.GameId;
                        }
                        else if (winnerNr == 2)
                        {
                            winnername = robotName;
                            gameId = bpKey.GameId;

                        }
                        await GameIsOver(winnername, _hubcontextPvE);
                        await SendRobotGameFinishedMessage();
                        await SetIsFinished(gameId);
                    }



                    if (!currentPlayer.Equals(robotName))
                    {
                        await MakeNextMove();
                    }

                    if (currentPlayer.Equals(playerName))
                    {
                        currentPlayer = robotName;
                    }
                    else
                    {
                        currentPlayer = playerName;
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

        public override Task OnConnectedAsync()
        {
            _hubcontextPvE = this.Clients;
            return base.OnConnectedAsync();
        }
    }
}
