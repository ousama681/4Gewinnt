using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using VierGewinnt.Hubs;
using VierGewinnt.ViewModels;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        private readonly IHubContext<GameHub> _hubContext;

        bool isSubscribed = false;

        public GameController(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
            if (!isSubscribed)
            {
                SubscribeAsync();
                isSubscribed = true;
            }
        }

        public async Task SendMessage(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }

        [HttpGet]
        public async Task<IActionResult> Board(string playerOne, string playerTwo)
        {
            GameViewModel gameViewModel = new GameViewModel();
            gameViewModel.PlayerOne = playerOne;
            gameViewModel.PlayerTwo = playerTwo;
            gameViewModel.Board = new Data.Models.GameBoard();

            return View(gameViewModel);
        }

        public async Task SubscribeAsync()
        {
            string broker = "localhost";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();
            string topicForReceive = "RobotStatus";
            string topicForPublish = "PlayerMove";
            //string username = "emqx";
            //string password = "public";

            // Create a MQTT client factory
            var factory = new MqttFactory();

            // Create a MQTT client instance
            var mqttClient = factory.CreateMqttClient();

            // Create MQTT client options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(broker, port) // MQTT broker address and port
                                             //.WithCredentials(username, password) // Set username and password
                .WithClientId(clientId)
                .WithCleanSession()
                .Build();

            // Connect to MQTT broker
            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("Connected to MQTT broker successfully.");

                // Subscribe to a topic
                await mqttClient.SubscribeAsync(topicForReceive);

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
                    SendMessage($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");

                    // Javascript function für clients aufrufen


                    return Task.CompletedTask;
                };
            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }
    }


    // TODO: Wenn wir eine Lösung haben, dass Action nicht die Page refreshed, können wir das wieder verwenden
    // ansonsten ist SigtnalR / Hubs die Ausweichlösung

    //public async Task<IActionResult> SpielzugAusfuehren(int colNumberYellow)
    //{
    //    string payload = "column:" + colNumberYellow;
    //    var model = new GameViewModel();
    //    model.Column = colNumberYellow;

    //    await MQTTBrokerService.PublishAsync(payload);

    //    return PartialView("_PartialViewName", model);
    //}
}
