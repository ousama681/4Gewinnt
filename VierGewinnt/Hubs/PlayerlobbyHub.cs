using Microsoft.AspNetCore.SignalR;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text;
using VierGewinnt.Data.Interfaces;

namespace VierGewinnt.Hubs
{
    public class PlayerlobbyHub : Hub
    {
        static readonly IList<string> players = new List<string>();
        static readonly IDictionary<string, string> onlineUsers = new Dictionary<string, string>();

        public async Task SendNotification(string player)
        {
            await Clients.Others.SendAsync("ReceiveNewUser", player);
        }

        public async Task AddUser(string player)
        {
            if (players.Contains(player))
            {
                return;
            }
            else
            {
                onlineUsers.Add(player, Context.ConnectionId);
                players.Add(player);
            }
            return;
        }

        public async Task GetAvailableUsers()
        {
            await Clients.Caller.SendAsync("ReceiveAvailableUsers", players);
        }

        public async Task LeaveLobby(string userName)
        {
            if (players.Contains(userName))
            {
                players.Remove(userName);
                onlineUsers.Remove(userName);
                await Clients.Others.SendAsync("PlayerLeft", userName);
                return;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName) && players.Contains(userName))
            {
                players.Remove(userName);
                onlineUsers.Remove(userName);
                await Clients.Others.SendAsync("PlayerLeft", userName);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotificateGameStart(string playerOne, string playerTwo)
        {
            //PublishDataForNewGame(playerOne, playerTwo);

            //PublishToNewGameParam(playerOne, playerTwo);

            await Clients.All.SendAsync("NavigateToGame", playerOne, playerTwo);
        }

        //private async void PublishDataForNewGame(string playerOne, string playerTwo)
        //{
        //    string broker = "localhost";
        //    int port = 1883;
        //    string clientId = Guid.NewGuid().ToString();
        //    //string topicForReceive = "RobotStatus";
        //    string topicForPublish = "NewGameParam";

        //    // Create a MQTT client factory
        //    var factory = new MqttFactory();

        //    // Create a MQTT client instance
        //    var mqttClient = factory.CreateMqttClient();

        //    // Create MQTT client options
        //    var options = new MqttClientOptionsBuilder()
        //        .WithTcpServer(broker, port)
        //        .WithClientId(clientId)
        //        .WithCleanSession()
        //        .Build();

        //    // Connect to MQTT broker
        //    var connectResult = await mqttClient.ConnectAsync(options);

        //    if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
        //    {
        //        Console.WriteLine("Connected to MQTT broker successfully.");

        //        // Subscribe to a topic
        //        //await mqttClient.SubscribeAsync(topicForReceive);

        //        // Callback function when a message is received
        //        mqttClient.ApplicationMessageReceivedAsync += e =>
        //        {
        //            Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
        //            return Task.CompletedTask;
        //        };

        //        string payload = "playerone:" + playerOne + ",playertwo:" + playerTwo;


        //        var message = new MqttApplicationMessageBuilder()
        //            .WithTopic(topicForPublish)
        //            .WithPayload(payload)
        //            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
        //            .WithRetainFlag()
        //            .Build();

        //        await mqttClient.PublishAsync(message);

        //        // Delay macht mehr sinn, für Applikationen die viele Nachrichten schicken. Bei uns wahrscheinlich nicht.
        //        await Task.Delay(1000);

        //        //// Unsubscribe and disconnect
        //        //await mqttClient.UnsubscribeAsync(topicForReceive);
        //        //await mqttClient.DisconnectAsync();
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
        //    }
        //}

        private async void PublishToNewGameParam(string playerOne, string playerTwo)
        {
            string broker = "localhost";
            int port = 1883;
            string clientId = Guid.NewGuid().ToString();
            //string topicForReceive = "RobotStatus";
            string topicForPublish = "NewGameParam";

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

                string payload = playerOne + "," + playerTwo;

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topicForPublish)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag()
                    .Build();

                await mqttClient.PublishAsync(message);

                // Delay macht mehr sinn, für Applikationen die viele Nachrichten schicken. Bei uns wahrscheinlich nicht.
                await Task.Delay(1000);
            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }
    }
}
