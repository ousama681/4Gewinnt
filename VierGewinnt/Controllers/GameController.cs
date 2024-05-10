using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MQTTBroker;
using MQTTnet.Client;
using MQTTnet;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using VierGewinnt.Hubs;
using VierGewinnt.ViewModels;
using System.Text;
using System.Diagnostics;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        private readonly IHubContext<GameHub> _hubContext;
        private readonly IGameRepository _gameRepository;
        private readonly IAccountRepository _accountRepository;
        private IMqttClient mqttClient = null;

        private static bool isFinishedCreatingEntity = false;

        public GameController(IHubContext<GameHub> hubContext, IGameRepository gameRepository, IAccountRepository accountRepository)
        {
            _hubContext = hubContext;
            _gameRepository = gameRepository;
            _accountRepository = accountRepository;
        }

        public async Task SendMessage(string message)
        {
            Move move = new Move();
            _gameRepository.AddMoveAsync(move);

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }

        // Achtung wenn diese Methode von beisden Spielern verwendet werden sollte, dann müssen wir einiges noch anpassen.
        public async Task<IActionResult> SpielzugAusfuehren()
        {
            GameViewModel gvm = new GameViewModel();
            int colNumberYellow = int.Parse(Request.Form["colNumber"]);
            int moveNr = int.Parse(Request.Form["moveNr"]);
            int boardId = int.Parse(Request.Form["boardId"]);
            string playerName = Request.Form["userId"];

            // Move in Database speichern

            moveNr++;

            string playerId = _accountRepository.GetUserByUsername(playerName).Result.Id;

            Move move = new Move();
            move.PlayerID = playerId;
            move.MoveNr = moveNr;
            move.Column = colNumberYellow;
            move.GameBoardID = boardId;

            // Hier gibt es noch eine Exception der DB bzgl GameBoadID Foreign Key. 

            _gameRepository.AddMoveAsync(move);

            // Publish to Robot the next PlayerMove
            await MQTTBroker.MQTTBrokerService.PublishAsync("PlayerMove", colNumberYellow.ToString());

            // Subscribe to wait till Robot is finished.
            await SubscribeAsync("RobotStatus");

            GameBoard board = _gameRepository.GetByIdAsync(new GameBoard() { ID = boardId});

            gvm.Board = board;
            gvm.PlayerOne = playerName;
            gvm.MoveNr = moveNr;

            await _hubContext.Clients.All.SendAsync("SendPlayerMove", colNumberYellow);

            return View("Board", gvm);
        }


        [HttpGet]
        public async Task<IActionResult> Board(string playerOne, string playerTwo)
        {
            GameViewModel gameViewModel = new GameViewModel();
            gameViewModel.PlayerOne = playerOne;
            gameViewModel.PlayerTwo = playerTwo;
            
            while (!isFinishedCreatingEntity)
            {
                // Wait till BoardEntityIsCreated
            }

            GameBoard gameBoard = FindGameBoardByPlayernames(playerOne, playerTwo);

            gameViewModel.Board = gameBoard;

            gameViewModel.MoveNr = 0;

            return View(gameViewModel);
        }

        private GameBoard FindGameBoardByPlayernames(string playerOne, string playerTwo)
        {
            return _gameRepository.FindGameByPlayerNames(playerOne, playerTwo);
        }

        public void CreateGame(string playerone, string playertwo)
        {
            isFinishedCreatingEntity = false;
            CreateBoardEntityAsync(playerone, playertwo);
            Task.Delay(500);
            isFinishedCreatingEntity = true;
        }

        private async Task<GameBoard> CreateBoardEntityAsync(string playerOne, string playerTwo)
        {
            ApplicationUser playerOneEnt = GetUser(playerOne).Result;

            string playerOneId = playerOneEnt.Id;

            ApplicationUser playerTwoEnt = GetUser(playerTwo).Result;
            string playerTwoId = playerTwoEnt.Id;

            GameBoard game = new GameBoard();
            game.PlayerOneID = playerOneEnt.Id;
            game.PlayerTwoID = playerTwoEnt.Id;

            _gameRepository.AddAsync(game);
            return game;
        }
        private async Task<ApplicationUser> GetUser(string playerName)
        {
            return _accountRepository.GetUserByUsername(playerName).Result;
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
                    Debug.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                    _hubContext.Clients.All.SendAsync("AllowNextMove", "Spieler darf den nächsten Zug ausführen.");
                    mqttClient.UnsubscribeAsync("RobotStatus");
                    mqttClient.DisconnectAsync();
                    return Task.CompletedTask;
                };

            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }
    }
}