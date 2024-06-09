using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using MQTTnet;
using MQTTnet.Client;
using System.Runtime.CompilerServices;
using System.Text;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using VierGewinnt.Hubs;

namespace VierGewinnt.Services
{
    public static class RobotVsRobotManager
    {
        private static IMqttClient mqttClient;

        public static IHubContext<BoardEvEHub> hubContext;

        public static Move[,] moves;
        public static IDictionary<string, int> colDepth = new Dictionary<string, int>();
        public static int moveNr = 0;
        public static GameBoard currentGame;
        public static string currentRobotMove;
        public static IDictionary<string, int> robotsInGame = new Dictionary<string, int>();
        public static string winner;
        public static bool firstMove = false;
        public static string currentColumn;
        public static int FeedBackCounter
        {
            get { return feedBackCounter; }
            set
            {
                feedBackCounter = value;
                if (feedBackCounter == 2)
                {
                    IsBothFinished = true;
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
                    CallAnimateHandler();
                    if (CheckForWin())
                    {
                        FinishGame();
                    }
                    MakeNextMove();
                    FeedBackCounter = 0;
                }
            }
        }
        private static bool isBothFinished = false;

        private static async Task FinishGame()
        {
            await GameIsOver();
        }

        private static async Task MakeNextMove()
        {
            string column = ConnectFourAIService.GetNextRandomMove().ToString();
            currentColumn = column;

            // NextMove wird an beide Roboter verschickt.
            await PublishToCoordinate(column);
            moveNr++;
            await AddMoveToBoard(column);

            string currentRobotMove = RobotVsRobotManager.currentRobotMove;
            string robotOne = RobotVsRobotManager.currentGame.PlayerOneID;
            string robotTwo = RobotVsRobotManager.currentGame.PlayerTwoID;


            currentRobotMove = currentRobotMove.Equals(robotOne) ? robotTwo : robotOne;
        }

        private static void CallAnimateHandler()
        {
            hubContext.Clients.All.SendAsync("AnimateMove", currentColumn) ;
        }

        static RobotVsRobotManager()
        {
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
                    if (message.Retain)
                    {
                        return;
                    }

                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

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

        public static async Task GameIsOver()
        {
            string text = "Roboter " + winner + " hat gewonnen.";
            await hubContext.Clients.All.SendAsync("NotificateGameEnd", text);
        }
        private static async Task SetIsFinished(int gameId)
        {
            //    var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            //    optionsBuilder.UseSqlServer(GameHub.connectionString);

            //    using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            //    {
            //        try
            //        {
            //            GameBoard gameboard = dbContext.GameBoards.Include(gb => gb.Moves).Where(gb => gb.ID.Equals(gameId)).Single();
            //            gameboard.IsFinished = true;
            //            dbContext.GameBoards.Update(gameboard);
            //            await dbContext.SaveChangesAsync();
            //            return;
            //            // Hier mal die Prüfung für den Win machen. 
            //        }
            //        catch (Exception e)
            //        {
            //            Debug.WriteLine(e);
            //        }
            //    }
        }


        public static async Task UnsubscribeAndCloseFromFeedback()
        {
            await mqttClient.UnsubscribeAsync("feedback");
            await mqttClient.DisconnectAsync();
        }

        private static async Task PublishToCoordinate(string column)
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", column);
        }


        public static async Task AddMoveToBoard(string column)
        {
            int depth;
            colDepth.TryGetValue(column, out depth);

            int columnInt = Int32.Parse(column);

            string robotNr = currentRobotMove;

            moves[columnInt - 1, depth - 1] = new Move() { PlayerID = robotNr, MoveNr = moveNr, Column = columnInt };
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


        private static bool CheckForWin()
        {
            return CheckForWinOrDraw(moves);
        }

        private static bool CheckForWinOrDraw(Move[,] board)
        {
            //GameInfo gameInfo;

            // Check horizontal
            for (int col = 0; col < 4; col++)
            {
                // Prüfung einbauen, da hier im if Moves Null sein können.
                for (int row = 0; row < 6; row++)
                {
                    string playerId = board[col, row] == null ? null : board[col, row].PlayerID;
                    if (!playerId.IsNullOrEmpty() && (board[col + 1, row] != null && playerId.Equals(board[col + 1, row].PlayerID)) && (board[col + 2, row] != null && playerId.Equals(board[col + 2, row].PlayerID)) && (board[col + 3, row] != null && playerId.Equals(board[col + 3, row].PlayerID)))
                    {
                        //gameInfo.SetWinner(playerId);
                        winner = playerId;
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
                        //gameInfo.SetWinner(playerId);
                        winner = playerId;
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
                        //gameInfo.SetWinner(playerId);
                        winner = playerId;
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
                    if (!playerId.IsNullOrEmpty() && (board[col + 1, row - 1] != null && playerId.Equals(board[col + 1, row - 1].PlayerID)) && (board[col + 2, row - 2] != null && playerId.Equals(board[col + 2, row - 2].PlayerID)) && (board[col + 2, row - 2] != null && playerId.Equals(board[col + 3, row - 3].PlayerID)))
                    {
                        //gameInfo.SetWinner(playerId);
                        winner = playerId;
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
