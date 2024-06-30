using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VierGewinnt.Services;

namespace VierGewinnt.Hubs
{
    public class BoardEvEHub : Hub
    { 
        //public async Task MakeFirstMove(string robotOneName)
        //{
        //    await RobotVsRobotManager.MakeNextMove();
        //}

        public static async Task CallAnimateHandler(string currentColumn)
        {
            await RobotVsRobotManager.hubContext.Clients.All.SendAsync("AnimateMove", currentColumn);
        }

        public static async Task GameIsOver()
        {
            string text = "Roboter " + RobotVsRobotManager.winner + " hat gewonnen.";
            await RobotVsRobotManager.hubContext.Clients.All.SendAsync("NotificateGameEnd", text);
        }

        public static async Task PublishToCoordinate(string column)
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", column);
        }
    }
}
