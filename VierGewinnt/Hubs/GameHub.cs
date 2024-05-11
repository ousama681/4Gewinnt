using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;

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
            await MQTTBroker.MQTTBrokerService.PublishAsync("PlayerMove", column);
            await SubscribeAsync("RobotStatus");
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
            await ConnectToMQTTBroker(mqttClient, options, topic);
        }

        private async Task ConnectToMQTTBroker(IMqttClient mqttClient, MqttClientOptions options, string topic)
        {
            var connectResult = await mqttClient.ConnectAsync(options);

            if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
            {
                // Subscribe to a topic
                await mqttClient.SubscribeAsync(topic);

                // Callback function when a message is received
                mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    await _hubClients.All.SendAsync("AnimatePlayerMove", "Spielzug wird animiert.");
                    await mqttClient.UnsubscribeAsync(topic);
                    await mqttClient.DisconnectAsync();
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
