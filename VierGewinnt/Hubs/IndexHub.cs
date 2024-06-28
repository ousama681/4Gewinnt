using Microsoft.AspNetCore.SignalR;
using MQTTBroker;
using VierGewinnt.Services;

namespace VierGewinnt.Hubs
{
    public class IndexHub : Hub
    {

        public async Task SendRobotFeedback()
        {
            Clients.All.SendAsync("ReceiveRobotFeedback", "Robot is done!");

            // erster Move
        }
    }
}
