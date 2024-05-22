using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MQTTBroker;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using VierGewinnt.Data;
using VierGewinnt.Data.Models;
using VierGewinnt.Hubs;
using VierGewinnt.Models;

namespace VierGewinnt.Controllers
{
    public class HomeController : Controller
    {
        private string username;
        private IMqttClient mqttClient = null;
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<PlayerlobbyHub> _hubContext;
        private static List<IMqttClient> connectedMqttClients = new List<IMqttClient>();
        private static IList<string> playersInHub = new List<string>();
        private static IList<string> robotsInHub = new List<string>();
        private static int countInstances = 0;

        public HomeController(ILogger<HomeController> logger, IHubContext<PlayerlobbyHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }
       
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GameLobby(string username)
        {
            if (!playersInHub.Contains(username))
            {
                this.username = username;
                playersInHub.Add(username);
                await SubscribeAsync("Challenge");
            }
            if(countInstances == 0)
            {
                await SubscribeRobotAsync("SubscribeRobot");
               
            }
            countInstances++;
            return View();
        }


        public IActionResult Leaderboard()
        {
            return View();
        }

        public IActionResult MatchHistory()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // MQTT METHODS

        // Challenge, ChallengeRobot

        public async Task SubscribeAsync(string topic)
        {

            string broker = "localhost";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();

            // Create a MQTT client factory
            var factory = new MqttFactory();

            // Create a MQTT client instance
            this.mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession(true)
                .Build();
            if(topic == "Challenge")
            {
                await ConnectToMQTTBroker(mqttClient, options, topic);
            }
            else if(topic == "ChallengeRobot")
            {
                //await ReceiveRobotSubscription(mqttClient, options, topic);
            }
        }

        private async Task ConnectToMQTTBroker(IMqttClient mqttClient, MqttClientOptions options, string topic)
        {
            // Connect to MQTT broker
            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                // Subscribe to a topic
                await mqttClient.SubscribeAsync(topic);

                // Hier adde ich den Client zur statischen Liste der clients
                connectedMqttClients.Add(mqttClient);

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    var message = e.ApplicationMessage;
                    if (message.Retain) // Ignore retained messages
                    {
                        return;
                    }

                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                    string[] players = payload.Split(',');

                    // Search for Gameboard that has both player in it with gamefinished == 0.

                    // IF found, return that gameId, else create new game.

                    string playerOne = players[0];
                    string playerTwo = players[1];

                    GameBoard game = await CheckForExistingGame(playerOne, playerTwo);

                    if (game != null)
                    {
                        await _hubContext.Clients.All.SendAsync("NavigateToGame", game.ID);
                        return;
                    }


                    if (playerTwo.Equals(this.username))
                    {
                        game = await CreateBoardEntityAsync(playerOne, playerTwo);

                        await _hubContext.Clients.All.SendAsync("NavigateToGame", game.ID);
                        await AfterStartingGame(mqttClient, topic);
                    }
                    else if (playerOne.Equals(this.username))
                    {
                        await AfterStartingGame(mqttClient, topic);
                    }
                    await mqttClient.UnsubscribeAsync(topic);
                    await mqttClient.DisconnectAsync();
                };

            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

        private async Task<GameBoard> CheckForExistingGame(string playerOne, string playerTwo)
        {
            var connectionstring = "Server=Koneko\\KONEKO;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionstring);

            // Or you can also instantiate inside using
            GameBoard game = new GameBoard();


            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    string playerOneID = GetUser(playerOne, dbContext).Result.Id;
                    string playerTwoID = GetUser(playerTwo, dbContext).Result.Id;

                    GameBoard existingGame = await dbContext.GameBoards.Include(gb => gb.Moves).Where(gb => (gb.PlayerOneID.Equals(playerOneID) && gb.PlayerTwoID.Equals(playerTwoID) && gb.IsFinished.Equals(false)) ||
                    (gb.PlayerOneID.Equals(playerTwoID) && gb.PlayerTwoID.Equals(playerOneID) && gb.IsFinished.Equals(false))).FirstOrDefaultAsync();
                    return existingGame;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return game;
        }

        private async Task AfterStartingGame(IMqttClient mqttClient, string topic)
        {

            connectedMqttClients.Remove(mqttClient);
            playersInHub.Remove(this.username);
            this.username = null;
        }

        private static async Task<GameBoard> CreateBoardEntityAsync(string playerOne, string playerTwo)
        {
            // "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"
            // "Server=Koneko\\KONEKO;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"
            var connectionstring = "Server=Koneko\\KONEKO;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionstring);

            // Or you can also instantiate inside using
            GameBoard game = new GameBoard();


            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    game.PlayerOneID = GetUser(playerOne, dbContext).Result.Id;
                    game.PlayerTwoID = GetUser(playerTwo, dbContext).Result.Id;

                    await dbContext.GameBoards.AddAsync(game);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return game;
        }

        private static async Task<ApplicationUser> GetUser(string playerName, AppDbContext context)
        {
            return await context.Accounts.FirstAsync(u => u.UserName.Equals(playerName));
        }

        // SubscribeRobot   

        public async Task SubscribeRobotAsync(string topic)
        {
            string broker = "localhost";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();

            // Create a MQTT client factory
            var factory = new MqttFactory();

            // Create a MQTT client instance
            mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession(true)
                .Build();

            await ReceiveRobotSubscription(mqttClient, options, topic);
        }

        private async Task ReceiveRobotSubscription(IMqttClient mqttClient, MqttClientOptions options, string topic)
        {
            // Connect to MQTT broker
            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                // Subscribe to a topic
                await mqttClient.SubscribeAsync(topic);

                // Hier adde ich den Client zur statischen Liste der clients
                connectedMqttClients.Add(mqttClient);

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    var message = e.ApplicationMessage;
                    if (message.Retain) // Ignore retained messages
                    {
                        return;
                    }

                    string robotID = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment); // nur Roboter ID wird gesendet
                    
                    robotsInHub.Add(robotID);
                    await _hubContext.Clients.All.SendAsync("AddRobot", robotID);
                    //await mqttClient.UnsubscribeAsync(topic);
                    //await mqttClient.DisconnectAsync();
                };

            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

       
    }
}
