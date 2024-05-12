using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Diagnostics;
using System.Text;
using VierGewinnt.Data;
using VierGewinnt.Data.Interfaces;
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
                .WithCleanSession()
                .Build();

            await ConnectToMQTTBroker(mqttClient, options, topic);
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
                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                    string[] players = payload.Split(',');

                    string playerOne = players[0];
                    string playerTwo = players[1];

                    GameBoard game = null;

                    if (playerTwo.Equals(this.username))
                    {
                        game = await CreateBoardEntityAsync(playerOne, playerTwo);

                        await _hubContext.Clients.All.SendAsync("NavigateToGame", game.ID);
                        await afterStartingGame(mqttClient, topic);
                    }
                    else if (playerOne.Equals(this.username))
                    {
                        await afterStartingGame(mqttClient, topic);
                    }
                    await Task.CompletedTask;
                };
            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

        private async Task afterStartingGame(IMqttClient mqttClient, string topic)
        {
            await mqttClient.UnsubscribeAsync(topic);
            await mqttClient.DisconnectAsync();
            connectedMqttClients.Remove(mqttClient);
            playersInHub.Remove(this.username);
            this.username = null;
            this.mqttClient = null;
        }

        private static async Task<GameBoard> CreateBoardEntityAsync(string playerOne, string playerTwo)
        {
            // "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"
            // "Server=Koneko\\KONEKO;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"
            var connectionstring = "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

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
    }
}
