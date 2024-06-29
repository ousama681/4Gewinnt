using Microsoft.AspNetCore.SignalR;
using MQTTBroker;
using VierGewinnt.Services;

namespace VierGewinnt.Hubs
{
    public class BoardEvEHub : Hub
    {

        private static string connectionstring = "Server=Koneko\\KONEKO;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

        public async Task MakeFirstMove(string robotOneName)
        {
            await RobotVsRobotManager.MakeNextMove();
        }
    }
}
