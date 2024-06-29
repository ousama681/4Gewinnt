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

        public static IHubContext<BoardEvEHub> hubContext;

        //public static Move[,] board;
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


        public static int otherRobotNr;
        public static int currRobotNr;
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
                    CallAnimateHandler();
                    int winner = CheckForWin();
                    if (winner != 0)
                    {
                        RobotVsRobotManager.robotMappingReversed.TryGetValue(winner, out RobotVsRobotManager.winner);
                        FinishGame();
                        return;
                    }
                    MakeNextMove();
                }
            }
        }
        private static bool isBothFinished = false;

        private static async Task FinishGame()
        {
            await GameIsOver();
        }

        public static async Task MakeNextMove()
        {
            currentColumn = ConnectFourAIService.GetNextRandomMove(board).ToString();

            //currentColumn = FindBestMove().ToString();

            // NextMove wird an beide Roboter verschickt.
            await PublishToCoordinate(currentColumn);
            moveNr++;
            await AddMoveToBoard();
        }

        public static async Task AddMoveToBoard()
        {

            int columnInt = Int32.Parse(currentColumn);
            int depth;


            colDepth.TryGetValue(currentColumn, out depth);
            colDepth[currentColumn] = depth - 1;

            board[depth - 1, columnInt - 1] = currRobotNr;

            if (currRobotNr == 1)
            {
                currRobotNr = 2;
            } else if (currRobotNr == 2)
            {
                currRobotNr = 1;
            }

        }

        private static void CallAnimateHandler()
        {
            hubContext.Clients.All.SendAsync("AnimateMove", currentColumn);
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

        public static async Task GameIsOver()
        {
            string text = "Roboter " + winner + " hat gewonnen.";
            await hubContext.Clients.All.SendAsync("NotificateGameEnd", text);
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

        private static int CheckForWin()
        {
            return CheckForWinOrDraw();
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

        public static int FindBestMove()
        {
            int bestScore = int.MinValue;
            int bestCol = -1;

            for (int col = 0; col < 7; col++)
            {
                int row = GetNextOpenRow(col);
                if (row != -1)
                {
                    board[row, col] = otherRobotNr;
                    // Hier aufpassen eventuell überschreibe ich alles
                    int score = Minimax(board, 5, false);
                    board[row, col] = 0;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestCol = col;
                    }
                }
            }

            return bestCol+1;
        }

        static int Minimax(int[,] board, int depth, bool isMaximizing)
        {


            int score = EvaluateBoard(board);
            if (Math.Abs(score) == 1000 || depth == 0)
            {
                return score;
            }

            if (isMaximizing)
            {
                int bestScore = int.MinValue;
                for (int col = 0; col < 7; col++)
                {
                    int row = GetNextOpenRow(col);
                    if (row != -1)
                    {
                        //int otherRobotNr;
                        //string otherRobot = currentRobotMove.Equals(currentGame.PlayerOneName) ? currentGame.PlayerOneName : currentGame.PlayerTwoName;
                        //robotMappingNr.TryGetValue(otherRobot, out otherRobotNr);
                        board[row, col] = otherRobotNr;
                        int newScore = Minimax(board, depth - 1, false);
                        board[row, col] = 0;
                        bestScore = Math.Max(bestScore, newScore);
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                for (int col = 0; col < 7; col++)
                {
                    int row = GetNextOpenRow(col);
                    if (row != -1)
                    {
                        board[row, col] = currRobotNr;
                        int newScore = Minimax(board, depth - 1, true);
                        board[row, col] = 0;
                        bestScore = Math.Min(bestScore, newScore);
                    }
                }
                return bestScore;
            }
        }

        static int EvaluateBoard(int[,] board)
        {
            // Simple evaluation: +1000 for AI win, -1000 for player win
            if (CheckWin(otherRobotNr)) return 1000;
            if (CheckWin(currRobotNr)) return -1000;
            return 0; // Neutral
        }

        static bool CheckWin(int player)
        {
            // Check for 4 in a row for the given player
            // Horizontal, vertical, diagonal checks
            // Simplified for brevity
            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 7; c++)
                {
                    if (c + 3 < 7 && board[r, c] == player && board[r, c + 1] == player && board[r, c + 2] == player && board[r, c + 3] == player)
                        return true;
                    if (r + 3 < 6 && board[r, c] == player && board[r + 1, c] == player && board[r + 2, c] == player && board[r + 3, c] == player)
                        return true;
                    if (r + 3 < 6 && c + 3 < 7 && board[r, c] == player && board[r + 1, c + 1] == player && board[r + 2, c + 2] == player && board[r + 3, c + 3] == player)
                        return true;
                    if (r - 3 >= 0 && c + 3 < 7 && board[r, c] == player && board[r - 1, c + 1] == player && board[r - 2, c + 2] == player && board[r - 3, c + 3] == player)
                        return true;
                }
            }
            return false;
        }

        static int GetNextOpenRow(int col)
        {
            for (int row = 5; row >= 0; row--)
            {
                if (board[row, col] == 0)
                {
                    return row;
                }
            }
            return -1; // Column is full
        }
    }
}
