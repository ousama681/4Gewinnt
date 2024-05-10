using Microsoft.AspNetCore.SignalR;
using MQTTBroker;
using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace VierGewinnt.Hubs
{
    public class GameHub : Hub
    {

        public string playerOne { get; set; }
        public string playerTwo { get; set; }

        //public async Task SendPlayerMove(string column)
        //{
        //    string payload = "column:" + column;
        //    await MQTTBrokerService.PublishAsync(payload);
        //}






        //public async Task SendMessageToRobot(string column) => SendPlayerMove(column);

        //public void PlaceYellowChip(int col) { 

        //}

        //public void PlaceRedChip(int col)
        //{

        //}

        //public async Task SendViewUpdate()
        //{
        //    await Clients.All.SendAsync("ReceiveViewUpdate");
        //}

        //public async Task SendPlayerMove(string column)
        //{
        //    Console.WriteLine("sjhfsbjahjfhajhf" + column);

        //    SubscribeAsync();



        //    // Publish cho für roboter
        //}



        //public override Task OnConnectedAsync()
        //{
        //    return base.OnConnectedAsync();
        //}

        //public async Task SubscribeAsync()
        //{
        //    string broker = "localhost";
        //    int port = 1883;
        //    string clientId = Guid.NewGuid().ToString();
        //    string topicForReceive = "RobotStatus";
        //    string topicForPublish = "PlayerMove";
        //    //string username = "emqx";
        //    //string password = "public";

        //    // Create a MQTT client factory
        //    var factory = new MqttFactory();

        //    // Create a MQTT client instance
        //    var mqttClient = factory.CreateMqttClient();

        //    // Create MQTT client options
        //    var options = new MqttClientOptionsBuilder()
        //        .WithTcpServer(broker, port) // MQTT broker address and port
        //                                     //.WithCredentials(username, password) // Set username and password
        //        .WithClientId(clientId)
        //        .WithCleanSession()
        //        .Build();

        //    // Connect to MQTT broker
        //    var connectResult = await mqttClient.ConnectAsync(options);

        //    if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
        //    {
        //        Console.WriteLine("Connected to MQTT broker successfully.");

        //        // Subscribe to a topic
        //        await mqttClient.SubscribeAsync(topicForReceive);

        //        // Callback function when a message is received
        //        mqttClient.ApplicationMessageReceivedAsync += e =>
        //        {
        //            Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");
        //            SendMessage($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment)}");

        //            // Javascript function für clients aufrufen






        //            //GameViewModel gameViewModel = new GameViewModel();
        //            //gameViewModel.Column = 3;
        //            //gameViewModel.RoboterStatus = "Roboter hat seinen Zug ausgeführt.";
        //            //PartialView("_PartialViewName", gameViewModel);

        //            return Task.CompletedTask;
        //        };

        //        //// Publish a message 10 times
        //        //for (int i = 0; i < 10; i++)
        //        //{
        //        //var message = new MqttApplicationMessageBuilder()
        //        //    .WithTopic(topicForPublish)
        //        //    .WithPayload(payload)
        //        //    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
        //        //    .WithRetainFlag()
        //        //    .Build();

        //        //await mqttClient.PublishAsync(message);
        //        await Task.Delay(1000); // Wait for 1 second
        //                                //}

        //        // Unsubscribe and disconnect
        //        //await mqttClient.UnsubscribeAsync(topicForReceive);
        //        //await mqttClient.DisconnectAsync();
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
        //    }
        //}
    }


}
