using Microsoft.AspNetCore.SignalR;
using MQTTBroker;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Hubs
{
    public class BoardEvEHub : Hub
    {


        public async Task MakeFirstMove(string robotOneName)
        {
            await MQTTBrokerService.PublishAsync("NextRobotMove", robotOneName);
        }

        //public async Task AnimateMove(string robotName, int columnNR, int gameId)
        //{
        //    Clients.All.
        //}
    }
}
