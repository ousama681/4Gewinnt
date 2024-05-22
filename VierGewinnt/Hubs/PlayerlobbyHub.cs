using Microsoft.AspNetCore.SignalR;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text;
using VierGewinnt.Data.Interfaces;
using MQTTBroker;
using System.Diagnostics;

namespace VierGewinnt.Hubs
{
    public class PlayerlobbyHub : Hub
    {
        static readonly IList<string> players = new List<string>();
        static readonly IList<string> robots = new List<string>();
        static readonly IDictionary<string, string> onlineUsers = new Dictionary<string, string>();

        //Player vs Player
        public async Task SendNotification(string player)
        {
            await Clients.Others.SendAsync("ReceiveNewUser", player);
        }

        public async Task AddUser(string player)
        {
            if (players.Contains(player))
            {
                return;
            }
            else
            {
                onlineUsers.Add(player, Context.ConnectionId);
                players.Add(player);
            }
            return;
        }

        public async Task GetAvailableUsers()
        {
            await Clients.Caller.SendAsync("ReceiveAvailableUsers", players);
        }

        public async Task LeaveLobby(string userName)
        {
            if (players.Contains(userName))
            {
                players.Remove(userName);
                onlineUsers.Remove(userName);
                await Clients.Others.SendAsync("PlayerLeft", userName);
                return;
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userName) && players.Contains(userName))
            {
                players.Remove(userName);
                onlineUsers.Remove(userName);
                await Clients.Others.SendAsync("PlayerLeft", userName);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task ChallengePlayer(string playerOne, string playerTwo)
        {
            string payload = $"{playerOne},{playerTwo}";
            await MQTTBrokerService.PublishAsync("Challenge", payload);
        }

        // Player vs Robot

        // Zum testen
        public async Task CreateRobot(string id)
        {
            await MQTTBrokerService.PublishAsync("SubscribeRobot", id);
        }

        public async Task AddRobot(string robotID)
        {
            if (robots.Contains(robotID))
            {
                return;
                Debug.WriteLine("ID already exists. Robot could not be added.");
            }
            else
            {
                robots.Add(robotID);
                await Clients.All.SendAsync("UpdateRobotLobby", robots);
            }
            return;
        }

        public async Task SendNotificationRobot(string robot)
        {
            await Clients.Others.SendAsync("ReceiveNewRobot", robot);
        }

        public async Task GetAvailableRobots()
        {
            await Clients.Caller.SendAsync("ReceiveAvailableRobots", robots);
        }

        public async Task ChallengeRobot(string playerOne, string robot)
        {
            string payload = $"{playerOne},{robot}";
            await MQTTBrokerService.PublishAsync("ChallengeRobot", payload);
        }

        public async Task LeaveLobbyRobot(string robot)
        {
            if (robots.Contains(robot))
            {
                robots.Remove(robot);
                await Clients.Others.SendAsync("RobotLeft", robot);
                return;
            }
        }




    }
}
