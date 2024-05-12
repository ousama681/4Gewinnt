using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using System.Diagnostics;
using VierGewinnt.Data;
using VierGewinnt.Data.Model;

namespace VierGewinnt.Hubs
{
    public class GameHub : Hub
    {

        public string playerOne { get; set; }
        public string playerTwo { get; set; }

        private bool isReadyToDispose = false;

        private static IHubCallerClients _hubClients = null;
        //Speichert die PlayerMoves die es auszuführen gilt. NAch dem Ausführen aus dem Dictionary entfernen.
        //Key: playerId, gameId Value: column
        private static IDictionary<BoardPlayer, int> playerMoves = new Dictionary<BoardPlayer, int>();


        private BoardPlayer? currentMoveKey;

        public async Task SendPlayerMove(string playerId, string gameId, string column)
        {
            // Save the Moves to execute in Dictionary in Case the MQTTService somehow fails. We probably also need to save it in some File. In case the power goes off.
            BoardPlayer bp = new BoardPlayer(playerId, Int32.Parse(gameId));
            int columnNr = Int32.Parse(column);

            currentMoveKey = bp;


            await SaveMove(bp, columnNr);
            playerMoves.Add(bp, columnNr);

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
                    var message = e.ApplicationMessage;
                    if (message.Retain) // Ignore retained messages
                    {
                        return;
                    }

                    BoardPlayer bpKey = currentMoveKey;
                    int column = 0;
                    playerMoves.TryGetValue(bpKey, out column);

                    // Robot did his Move, now we can save it do database
                    // Hier könnte ich die playerID mitgeben, dann wissen wir, wer als nächstes dran ist.
                    // Innerhalb der AnimatePlayerMove Methode wird auch enabled wer am Zug ist.
                    await _hubClients.All.SendAsync("AnimatePlayerMove", column, bpKey.PlayerId);

                    playerMoves.Remove(bpKey);

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

        private static async Task<Move> SaveMove(BoardPlayer boardPlayer, int column)
        {
            // "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"
            // "Server=Koneko\\KONEKO;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"
            var connectionstring = "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionstring);
            Move move = new Move();

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    move.MoveNr = dbContext.Moves.Where(m => m.GameBoardID.Equals(boardPlayer.GameId)).Count() + 1;
                    move.Column = column;
                    move.GameBoardID = boardPlayer.GameId;
                    move.PlayerID = boardPlayer.PlayerId;
                    await dbContext.Moves.AddAsync(move);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return move;
        }


        private class BoardPlayer
        {
            public readonly string PlayerId;
            public readonly int GameId;

            public BoardPlayer(string playerId, int gameId)
            {
                PlayerId = playerId;
                GameId = gameId;
            }
        }
    }
}
