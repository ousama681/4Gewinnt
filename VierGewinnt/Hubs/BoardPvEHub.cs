using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MQTTnet.Client;
using MQTTnet;
using System.Diagnostics;
using VierGewinnt.Controllers;
using VierGewinnt.Data;
using VierGewinnt.Services;
using static VierGewinnt.Hubs.GameHub;
using System.Text;
using VierGewinnt.Data.Models;
using VierGewinnt.Data.Model;

namespace VierGewinnt.Hubs
{
    public class BoardPvEHub : Hub
    {

        private static IHubContext<BoardPvEHub> _hubContext = null;

        private static BoardPlayer? currentMoveKey;
        private static IDictionary<BoardPlayer, int> playerMoves = new Dictionary<BoardPlayer, int>();


        public async Task MakeFirstMove(string robotOneName)
        {
            await RobotVsRobotManager.MakeNextMove();
        }

        public static async Task CallAnimateHandler(string currentColumn)
        {
            await RobotVsRobotManager.hubContext.Clients.All.SendAsync("AnimateMove", currentColumn);
        }

        public static async Task GameIsOver()
        {
            string text = "Roboter " + RobotVsRobotManager.winner + " hat gewonnen.";
            await RobotVsRobotManager.hubContext.Clients.All.SendAsync("NotificateGameEnd", text);
        }

        public static async Task PublishToCoordinate(string column)
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", column);
        }

        public async Task SendPlayerMove(string playerName, string gameId, string column)
        {

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);
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

            await GameManager.SaveMove(bp, columnNr);
            playerMoves.Add(bp, columnNr);

            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", column);
            //await SubscribeToFeedbackAsync("feedback");
            // TestMethode um nicht mit Postman den RobotStatus zu simulieren
            //await MQTTBrokerService.PublishAsync("feedback", "1");
        }

        public static async Task SubscribeToFeedbackAsync(string topic, IHubContext<BoardPvEHub> hubContextPvE)
        {

            _hubContext = hubContextPvE;
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

        private static async Task ConnectToMQTTBroker(IMqttClient mqttClient, MqttClientOptions options, string topic)
        {
            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                // Subscribe to a topic
                await mqttClient.SubscribeAsync(topic);

                // Die Methode welche ausgeführt wird, wenn de Roboter auf "feedback" published.
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

                    await _hubContext.Clients.All.SendAsync("AnimatePlayerMove", column, bpKey.PlayerName);
                    playerMoves.Remove(bpKey);

                    int winnerNr = GameManager.CheckForWin(bpKey.GameId);

                    if (winnerNr != 0)
                    {
                        string winnername = "";
                        int gameId = 0;
                        if (winnerNr == 1)
                        {
                            winnername = playerOne.PlayerName;
                            gameId = bpKey.GameId;
                        }
                        else if (winnerNr == 2)
                        {
                            winnername = playerTwo.PlayerName;
                            gameId = bpKey.GameId;

                        }
                        await GameIsOver(winnername, gameId);
                        await SendRobotGameFinishedMessage();
                        await SetIsFinished(gameId);
                    }

                    await RobotVsRobotManager.MakeNextMove();
                    //await mqttClient.UnsubscribeAsync(topic);
                    //await mqttClient.DisconnectAsync();
                    await Task.CompletedTask;
                };
            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

        public static async Task GameIsOver(string winnerId, int gameId)
        {
            //runningGames.Remove(gameId);
            await UpdatePlayerRanking(winnerId);
            await _hubContext.Clients.All.SendAsync("NotificateGameEnd", winnerId);
            await RobotVsRobotManager.UnsubscribeAndCloseFromFeedback();
        }

        private static async Task SendRobotGameFinishedMessage()
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", "9");
        }

        private static async Task SetIsFinished(int gameId)
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


        private static async Task UpdatePlayerRanking(string winnerName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);


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

    }
}
