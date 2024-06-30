using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using VierGewinnt.Hubs;
using static Microsoft.EntityFrameworkCore.SqlServer.Query.Internal.SqlServerOpenJsonExpression;

namespace VierGewinnt.Services
{
    public static class RobotVsRobotManager
    {
        private static IMqttClient mqttClient;

        public static IHubContext<BoardPvEHub> hubContextPvE;


        public static IDictionary<string, int> colDepth = new Dictionary<string, int>();
        public static int moveNr = 0;
        public static GameBoard currentGame;
        public static string currentRobotMove;
        //public static IDictionary<string, int> robotsInGame = new Dictionary<string, int>();
        public static IDictionary<string, int> robotMappingNr = new Dictionary<string, int>();
        public static IDictionary<int, string> robotMappingReversed = new Dictionary<int, string>();
        public static string winner;
        public static bool firstMove = false;
        public static string currentColumn;
        public static int[,] board = new int[6, 7];


        public static int playerNr;
        public static int otherRobotNr;
        public static int currPlayerNr;
        public static int FeedBackCounter
        {
            get { return feedBackCounter; }
            set
            {
                feedBackCounter = value;
                if (feedBackCounter >= 2)
                {
                    IsBothFinished = true;
                    feedBackCounter = 0;
                }
            }
        }

        private static int feedBackCounter = 0;


        public static bool IsBothFinished
        {
            get { return isBothFinished; }
            set
            {
                isBothFinished = value;
                if (isBothFinished == true)
                {
                    BoardEvEHub.CallAnimateHandler(currentColumn);
                    int winner = GameManager.CheckForWinOrDraw(board);
                    if (winner != 0)
                    {
                        RobotVsRobotManager.robotMappingReversed.TryGetValue(winner, out RobotVsRobotManager.winner);
                        FinishGame();
                        return;
                    }
                    //MakeNextMove();
                }
            }
        }
        private static bool isBothFinished = false;

        private static async Task FinishGame()
        {
            await BoardPvEHub.GameIsOver();
        }

        public static async Task MakeNextMove()
        {
            // Hier falls noch Zeit ist morgen unbedingt eine AI einbauen, es muss kein ProAI sein
            // Aber immerhin sollte sie fähig sein einen Siegerzug zu verhindern und einfache
            // Steine zu verbinden.

            Game game = new Game();
            game.board.board = board;

            //currentColumn = ConnectFourAIService.GetNextRandomMove(board).ToString();
            currentColumn = game.miniMax.GetBestMove(game.board).Column.ToString();
            BoardPvEHub.currentcolumn = currentColumn;

            // NextMove wird an beide Roboter verschickt.
            await BoardPvEHub.PublishToCoordinate(currentColumn);
            moveNr++;
            await AddMoveToBoard();
        }

        public static async Task AddMoveToBoard()
        {

            int columnInt = Int32.Parse(currentColumn);
            int depth;


            colDepth.TryGetValue(currentColumn, out depth);
            colDepth[currentColumn] = depth - 1;

            board[depth - 1, columnInt - 1] = currPlayerNr;

            if (currPlayerNr == 1)
            {
                currPlayerNr = 2;
            } else if (currPlayerNr == 2)
            {
                currPlayerNr = 1;
            }
        }

        public static async Task SubscribeToFeedbackTopic()
        {
            string broker = "localhost";
            string topic = "feedback";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();
            var factory = new MqttFactory();

            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession(true)
                .Build();

            await ConnectToMQTTBroker(mqttClient, options, topic);
        }

        private static async Task ConnectToMQTTBroker(IMqttClient mqttClient, MqttClientOptions options, string topic)
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
                        FeedBackCounter = FeedBackCounter + 1;
                    }
                };
            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

        public static async Task UnsubscribeAndCloseFromFeedback()
        {
            await mqttClient.UnsubscribeAsync("feedback");
            await mqttClient.DisconnectAsync();
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
    }
}
