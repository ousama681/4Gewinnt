using Microsoft.AspNetCore.SignalR;
using MQTTBroker;
using VierGewinnt.Services;

namespace VierGewinnt.Hubs
{
    public class BoardEvEHub : Hub
    {

        private static string connectionstring = Program.connectionString;

        public async Task MakeFirstMove(string robotOneName)
        {
            await RobotVsRobotManager.MakeNextMove();
        }
    }
}
