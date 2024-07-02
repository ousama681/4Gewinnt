using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Text;
using VierGewinnt.Data.Models;
using VierGewinnt.Services;
using VierGewinnt.Services.AI;
using static VierGewinnt.Hubs.GameHub;

namespace VierGewinnt.Hubs
{
    public class BoardEvEHub : BoardHubBase
    {

        public static IHubCallerClients _hubContextEvE = null;


        private static BoardPlayer? currentMoveKey;
        private static IDictionary<BoardPlayer, int> playerMoves = new Dictionary<BoardPlayer, int>();
        public static IDictionary<int, string> robotMappingReversed = new Dictionary<int, string>();
        public static string currentPlayer = "";
        public static string currentcolumn = "";
        public static string robotName = "";
        public static string playerName = "";
        public static int currGameId = 0;
        public static int moveNr = 0;
        public static int currPlayerNr = 0;
        public static IDictionary<string, int> colDepth = new Dictionary<string, int>();

        public static int[,] board = new int[6, 7];



        public static int FeedBackCounter
        {
            get { return feedBackCounter; }
        }

        public static int feedBackCounter = 0;
        internal static GameBoard currentGame;
        internal static int otherRobotNr;
        internal static string currentRobotMove;

        public async Task MakeFirstMove()
        {
            await MakeNextMoveEvE();
        }


        public async Task PublishToCoordinate(string column)
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", column);
        }


        public async Task MakeNextMoveEvE()
        {
            BoardGame game = new BoardGame();
            game.board.board = board;
            currentcolumn = game.miniMax.GetBestMove(game.board).Column.ToString();

            await PublishToCoordinate(currentcolumn);
            await SubscribeToFeedbackTopic();
            moveNr++;
            AddMoveToBoard();
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


        public async Task SubscribeToFeedbackTopic()
        {
            string broker = "localhost";
            string topic = "feedback";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();
            var factory = new MqttFactory();

            IMqttClient mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession(true)
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

                    if (payload.Equals("1"))
                    {
                        feedBackCounter = feedBackCounter + 1;


                        if (FeedBackCounter == 2)
                        {
                            await SendAnimateEvEMove(currentcolumn, _hubContextEvE);
                            int winnerNr = GameManager.CheckForWin(board);

                            if (winnerNr != 0)
                            {
                                string winnername = "";
                                int gameId = 0;
                                if (winnerNr == 1)
                                {
                                    winnername = playerName;
                                    gameId = currGameId;
                                }
                                else if (winnerNr == 2)
                                {
                                    winnername = robotName;
                                    gameId = currGameId;

                                }
                                await GameIsOver(winnername, _hubContextEvE);
                                await SendRobotGameFinishedMessage();
                                await SetIsFinished(gameId);
                                return;
                            }

                            feedBackCounter = 0;
                            await mqttClient.UnsubscribeAsync(topic);

                            if (currentPlayer.Equals(playerName))
                            {
                                currentPlayer = robotName;
                            }
                            else
                            {
                                currentPlayer = playerName;
                            }
                            await MakeNextMoveEvE();
                        }
                        return;
                    }
                };
            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

        public static async Task UnsubscribeAndCloseFromFeedback(IMqttClient mqttClient)
        {
            await mqttClient.UnsubscribeAsync("feedback");
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

        public override Task OnConnectedAsync()
        {
            _hubContextEvE = this.Clients;
            return base.OnConnectedAsync();
        }
    }
}



