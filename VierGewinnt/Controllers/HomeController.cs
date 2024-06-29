using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using VierGewinnt.Data;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using VierGewinnt.Hubs;
using VierGewinnt.Models;
using VierGewinnt.ViewModels;

namespace VierGewinnt.Controllers
{
    public class HomeController : Controller
    {
        private string username;
        private IMqttClient mqttClient = null;
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<PlayerlobbyHub> _hubContext;
        private readonly IPlayerInfoRepository _playerInfoRepo;
        private readonly IGameRepository _gameRepository;
        private readonly IAccountRepository _accountRepository;
        private static List<IMqttClient> connectedMqttClients = new List<IMqttClient>();
        private static IList<string> playersInHub = new List<string>();
        private static IList<string> robotsInHub = new List<string>();
        private static int countInstances = 0;

        private static string connectionstring = "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

        public HomeController(ILogger<HomeController> logger,
            IHubContext<PlayerlobbyHub> hubContext,
            IPlayerInfoRepository playerInfoRepository,
            IGameRepository gameRepository,
            IAccountRepository accountRepository)
        {
            _logger = logger;
            _hubContext = hubContext;
            _playerInfoRepo = playerInfoRepository;
            _gameRepository = gameRepository;
            _accountRepository = accountRepository;
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
            }
            if (countInstances == 0)
            {
                
                await SubscribeRobotAsync("SubscribeRobot");
                await SubscribeAsync("Challenge");
                await SubscribeAsync("ChallengeRobot");

            }
            countInstances++;

            GameLobbyViewModel vm = new GameLobbyViewModel();

            vm.Robots = await _accountRepository.GetAllRegisteredRobots();

            return View(vm);
        }

        public async Task<IActionResult> Test()
        {
            return View("GameLobby");
        }

        public async Task<IActionResult> TestRoboterMove()
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", "3");
            await SubscribeFeedbackAsync("feedback");
            return View("Index");
        }

        public async Task SubscribeFeedbackAsync(string topic)
        {

            string broker = "localhost";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();

            // Create a MQTT client factory
            var factory = new MqttFactory();

            // Create a MQTT client instance
            var mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            // Connect to MQTT broker
            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("Connected to MQTT broker successfully.");

                // Subscribe to a topic
                await mqttClient.SubscribeAsync(topic);

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    var message = e.ApplicationMessage;
                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                    if (payload.Equals("0"))
                    {
                        return Task.CompletedTask;
                    }


                    if (message.Retain) // Ignore retained messages
                    {
                        return Task.CompletedTask;
                    }



                   _hubContext.Clients.All.SendAsync("SendRobotFeedback");


                    Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                    return Task.CompletedTask;
                };
            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GameAborted(GameViewModel model)
        {
            GameBoard board = await _gameRepository.GetByIdAsync(model.Board);
            if (board != null)
            {
                board.IsFinished = true;
                _gameRepository.Update(board);
            }


            if (!playersInHub.Contains(username))
            {
                this.username = username;
                playersInHub.Add(username);
                await SubscribeAsync("Challenge");
                await SubscribeAsync("ChallengeRobot");
            }

            GameLobbyViewModel vm = new GameLobbyViewModel();

            vm.Robots = await _accountRepository.GetAllRegisteredRobots();

            return View("GameLobby", vm);
        }

        public IActionResult Leaderboard()
        {
            LeaderboardViewModel vm = new LeaderboardViewModel();
            vm.PlayerRankings = _playerInfoRepo.GetAllAsync().Result;
            return View(vm);
        }

        public IActionResult MatchHistory(string username)
        {
            // ViewModel für MatchHistory

            // Was brauche ich in MatchHistory

            // Pro Spieler Alle Fertigen Spiele Laden inclusive Moves.

            List<GameBoard> gameHistorie = _gameRepository.FindGamesByPlayerName(username).Result;



            MatchHistoryViewModel vm = new MatchHistoryViewModel();

            vm.GameHistories = gameHistorie;

            return View(vm);
        }

        public IActionResult Replay(int gameId)
        {

            // ViewModel für MatchHistory

            // Was brauche ich in MatchHistory

            // Pro Spieler Alle Fertigen Spiele Laden inclusive Moves.

            GameBoard gb = _gameRepository.GetByIdAsync(new GameBoard() { ID = gameId }).Result;


            SpielRueckblickViewModel vm = new SpielRueckblickViewModel();

            vm.Game = gb;
            vm.MovesLeft = new Stack<Move>();

            for (int i = gb.Moves.Count() - 1; i >= 0; i--)
            {
                vm.MovesLeft.Push(gb.Moves.ElementAt(i));
            }

            vm.MovesPlayed = new Queue<Move>();

            // Moves in VM laden

            // Dann Per JS das Spiel animieren ablaufen / simulieren.

            return View(vm);
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
            if (topic == "Challenge")
            {
                await ConnectToMQTTBroker(mqttClient, options, topic);
            }
            else if (topic == "ChallengeRobot")
            {
                await ReceiveChallengeRobot(mqttClient, options, topic);
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
                    // Hier kommt man nur rein von Messages von /Challenge
                    var message = e.ApplicationMessage;
                    if (message.Retain) // Ignore retained messages
                    {
                        return;
                    }

                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                    string[] players = payload.Split(',');

                    // Search for Gameboard that has both player in it with gamefinished == 0.
                    string playerOne = players[0];
                    string playerTwo = players[1];

                    GameBoard game = await CheckForExistingGame(playerOne, playerTwo);

                    if (game != null)
                    {
                        game.playerNames.PlayerOneName = playerOne;
                        game.playerNames.PlayerTwoName = playerTwo;
                        await _hubContext.Clients.All.SendAsync("NavigateToGame", game.ID);
                        return;
                    }

                    game = await CreateBoardEntityAsync(playerOne, playerTwo);

                    game.playerNames.PlayerOneName = playerOne;
                    game.playerNames.PlayerTwoName = playerTwo;

                    await _hubContext.Clients.All.SendAsync("NavigateToGame", game.ID);
                    await AfterStartingGame(mqttClient, topic);
                };

            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

        private async Task ReceiveChallengeRobot(IMqttClient mqttClient, MqttClientOptions options, string topic)
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
                    string robotID = players[1];

                    GameBoard game = await CheckForExistingGameAgainstRobot(playerOne, robotID);

                    if (game != null)
                    {
                        await _hubContext.Clients.All.SendAsync("NavigateToGameAgainstRobot", game.ID);
                        return;
                    }
                    game = await CreateBoardEntityAgainstRobotAsync(playerOne, robotID);
                    await _hubContext.Clients.All.SendAsync("NavigateToGameAgainstRobot", game.ID);
                    await AfterStartingGame(mqttClient, topic);
                };

            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

        private async Task<GameBoard> CheckForExistingGame(string playerOne, string playerTwo)
        {
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

                    GameBoard existingGame = await dbContext.GameBoards.Include(gb => gb.Moves).Where(gb => (gb.PlayerOneName.Equals(playerOne) && gb.PlayerTwoName.Equals(playerTwo) && gb.IsFinished.Equals(false)) ||
                    (gb.PlayerOneName.Equals(playerOne) && gb.PlayerTwoName.Equals(playerTwo) && gb.IsFinished.Equals(false))).FirstOrDefaultAsync();
                    return existingGame;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return game;
        }

        private async Task<GameBoard> CheckForExistingGameAgainstRobot(string playerOne, string robotID)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionstring);

            // Or you can also instantiate inside using
            GameBoard game = new GameBoard();


            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    string playerOneID = GetUser(playerOne, dbContext).Result.Id;

                    GameBoard existingGame = await dbContext.GameBoards.Include(gb => gb.Moves).Where(gb => (gb.PlayerOneName.Equals(playerOne) && gb.PlayerTwoName.Equals(robotID) && gb.IsFinished.Equals(false)) ||
                    (gb.PlayerOneName.Equals(robotID) && gb.PlayerTwoName.Equals(playerOne) && gb.IsFinished.Equals(false))).FirstOrDefaultAsync();
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
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionstring);

            // Or you can also instantiate inside using
            GameBoard game = new GameBoard();


            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    var userOne = GetUser(playerOne, dbContext).Result;
                    var userTwo = GetUser(playerTwo, dbContext).Result;

                    game.PlayerOneID = userOne.Id;
                    game.PlayerTwoID = userTwo.Id;
                    game.PlayerOneName = userOne.UserName;
                    game.PlayerTwoName = userTwo.UserName;

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
        private static async Task<GameBoard> CreateBoardEntityAgainstRobotAsync(string playerOne, string robotID)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionstring);

            // Or you can also instantiate inside using
            GameBoard game = new GameBoard();


            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    var userOne = GetUser(playerOne, dbContext).Result;

                    game.PlayerOneID = userOne.Id;
                    game.PlayerOneName = userOne.UserName;

                    game.PlayerTwoID = robotID;

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

        public static async Task<ApplicationUser> GetUser(string playerName, AppDbContext context)
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
                };

            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }




    }
}
