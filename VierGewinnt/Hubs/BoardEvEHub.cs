using Microsoft.AspNetCore.SignalR;
using MQTTBroker;
using VierGewinnt.Services;

namespace VierGewinnt.Hubs
{
    public class BoardEvEHub : Hub
    {

        private static string connectionstring = "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

        public async Task MakeFirstMove(string robotOneName)
        {
            await RobotVsRobotManager.MakeNextMove();
        }
    }
}
