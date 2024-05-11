using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;

namespace VierGewinnt.Hubs
{
    public class GameHub : Hub
    {

        public string playerOne { get; set; }
        public string playerTwo { get; set; }

        private bool isReadyToDispose = false;

        private static IHubCallerClients _hubClients = null;


        public async Task SendPlayerMove(string column)
        {
            Debug.WriteLine("Inside SendplayeRMove in GameHub");

            // Move in DB speichern

            await MQTTBroker.MQTTBrokerService.PublishAsync("PlayerMove", column);

            await SubscribeAsync("RobotStatus");

            // Hier noch Subscriben und von hier aus die clients steuern.
        }



        public async Task SubscribeAsync(string topic)
        {

            _hubClients = this.Clients;
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
            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                Console.WriteLine("Connected to MQTT broker successfully.");

                // Subscribe to a topic
                await mqttClient.SubscribeAsync(topic);

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    // Animate Move when robot gives O.K.
                   await _hubClients.All.SendAsync("AnimatePlayerMove", "Spielzug wird animiert.");

                    

                    // UI  Freigeben, wenn Player am Zug ist.
                    mqttClient.UnsubscribeAsync(topic);
                    mqttClient.DisconnectAsync();
                    await Task.CompletedTask;
                };

            }
            else
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
            }
        }
    }
}
