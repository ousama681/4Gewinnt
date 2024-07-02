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
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using VierGewinnt.Hubs;
using VierGewinnt.Models;
using VierGewinnt.Services;
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
        private static IList<string> playersInHub = new List<string>();
        public static IList<string> robotsInHub = new List<string>();
        private static int countInstances = 0;

        private static string connectionstring = DbUtility.connectionString;

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

        public async Task SubscribeFeedbackAsync(string topic)
        {

            string broker = "localhost";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();

            var factory = new MqttFactory();

            var mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("Connected to MQTT broker successfully.");

                await mqttClient.SubscribeAsync(topic);

                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    var message = e.ApplicationMessage;
                    string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                    if (payload.Equals("0") || message.Retain)
                    {
                        return Task.CompletedTask;
                    }

                    _hubContext.Clients.All.SendAsync("SendRobotFeedback");
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
            if (model.Board.ID != 0)
            {
                GameBoard board = await _gameRepository.GetByIdAsync(model.Board);
                if (board != null)
                {
                    board.IsFinished = true;
                    _gameRepository.Update(board);
                }
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
            vm.PlayerRankings = _playerInfoRepo.GetAllAsync().Result.OrderByDescending(pr => pr.Wins);
            return View(vm);
        }

        public IActionResult MatchHistory(string username)
        {
            List<GameBoard> gameHistorie = _gameRepository.FindGamesByPlayerName(username).Result;
            ViewBag.Username = username;
            MatchHistoryViewModel vm = new MatchHistoryViewModel();
            vm.GameHistories = gameHistorie;

            return View(vm);
        }

        public IActionResult Replay(int gameId)
        {
            GameBoard gb = _gameRepository.GetByIdAsync(new GameBoard() { ID = gameId }).Result;
            ViewBag.Username = gb.PlayerOneName;
            ViewBag.Username2 = gb.PlayerTwoName;

            SpielRueckblickViewModel vm = new SpielRueckblickViewModel();
            vm.Game = gb;
            vm.MovesLeft = new Stack<Move>();

            for (int i = gb.Moves.Count() - 1; i >= 0; i--)
            {
                vm.MovesLeft.Push(gb.Moves.ElementAt(i));
            }

            vm.MovesPlayed = new Queue<Move>();

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task SubscribeAsync(string topic)
        {

            string broker = "localhost";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();

            var factory = new MqttFactory();

            this.mqttClient = factory.CreateMqttClient();

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
                    string[] players = payload.Split(',');

                    string playerOne = players[0];
                    string playerTwo = players[1];

                    GameBoard game = await GameManager.CheckForExistingGame(playerOne, playerTwo);

                    ApplicationUser playerOneUser = DbUtility.GetUser(playerOne);
                    ApplicationUser playerTwoUser = DbUtility.GetUser(playerTwo);
                    List<string> userIds = new List<string>();
                    userIds.Add(playerOneUser.Id);
                    userIds.Add(playerTwoUser.Id);


                    if (game != null)
                    {
                        game.playerNames.PlayerOneName = playerOne;
                        game.playerNames.PlayerTwoName = playerTwo;
                        await _hubContext.Clients.Users(userIds).SendAsync("NavigateToGame", game.ID);
                        return;
                    }

                    game = await GameManager.CreateBoardEntityAsync(playerOne, playerTwo);

                    game.playerNames.PlayerOneName = playerOne;
                    game.playerNames.PlayerTwoName = playerTwo;

                    await _hubContext.Clients.Users(userIds).SendAsync("NavigateToGame", game.ID);
                    await AfterStartingGame();
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
                //connectedMqttClients.Add(_mqttClient);

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
                    string playerOne = players[0];
                    string robotID = players[1];

                    GameBoard game = await GameManager.CheckForExistingGame(playerOne, robotID);

                    ApplicationUser playerOneUser = DbUtility.GetUser(playerOne);

                    if (game != null)
                    {

                        if (playerOneUser != null)
                        {
                            await _hubContext.Clients.User(playerOneUser.Id).SendAsync("NavigateToGameAgainstRobot", game.ID);

                        }
                        return;
                    }
                    game = await GameManager.CreateBoardEntityAsync(playerOne, robotID);

                    if (playerOneUser != null)
                    {
                        await _hubContext.Clients.User(playerOneUser.Id).SendAsync("NavigateToGameAgainstRobot", game.ID);
                    }
                    await AfterStartingGame();
                };

            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }

        private async Task AfterStartingGame()
        {
            playersInHub.Remove(this.username);
            this.username = null;
        }

        // SubscribeRobot   

        public async Task SubscribeRobotAsync(string topic)
        {
            string broker = "localhost";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();

            var factory = new MqttFactory();

            mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port)
                .WithClientId(clientId)
                .WithCleanSession(true)
                .Build();

            await ReceiveRobotSubscription(mqttClient, options, topic);
        }

        private async Task ReceiveRobotSubscription(IMqttClient mqttClient, MqttClientOptions options, string topic)
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

                    string robotID = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                    if (!robotsInHub.Contains(robotID))
                    {
                        robotsInHub.Add(robotID);
                        await _hubContext.Clients.All.SendAsync("AddRobot", robotID);
                    }
                    else
                    {
                        robotsInHub.Remove(robotID);
                        await _hubContext.Clients.All.SendAsync("RemoveRobot", robotID);
                    }
                };

            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }




    }
}
