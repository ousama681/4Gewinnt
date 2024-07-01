using Microsoft.AspNetCore.SignalR;
using MQTTBroker;
using VierGewinnt.Controllers;

namespace VierGewinnt.Hubs
{
    public class PlayerlobbyHub : Hub
    {
        static readonly IDictionary<string, string> onlineUsers = new Dictionary<string, string>();
        public static readonly IHubContext<GameHub> _hubContextPvP;


        //Player vs Player
        public async Task SendNotification(string player)
        {
            await Clients.Others.SendAsync("ReceiveNewUser", player, Context.ConnectionId);
        }

        public async Task AddUser(string player)
        {
            if (onlineUsers.ContainsKey(player))
            {
                return;
            }
            else
            {
                onlineUsers.Add(player, Context.ConnectionId);
                await SetConnectionId(player);
            }
            return;
        }

        public async Task SetConnectionId(string player)
        {
            await Clients.Caller.SendAsync("SetConID", Context.ConnectionId, player);
        }

        public async Task GetAvailableUsers()
        {
            await Clients.Caller.SendAsync("ReceiveAvailableUsers", onlineUsers);
        }

        public async Task LeaveLobby(string userName)
        {
            if (onlineUsers.ContainsKey(userName))
            {
                onlineUsers.Remove(userName);
                await Clients.Others.SendAsync("PlayerLeft", userName);
                return;
            }
        }

        public async Task ChallengePlayer(string playerOneId, string playerTwoId, string playerOne, string playerTwo)
        {
            string payload = $"{playerOne},{playerTwo}";
            string groupId = $"{playerOneId},{playerTwoId}";
            await Groups.AddToGroupAsync(playerOneId, groupId);
            await Groups.AddToGroupAsync(playerTwoId, groupId);
            await Clients.Client(playerTwoId).SendAsync("ReceiveChallenge", payload, playerOneId, groupId);
        }

        public async Task ConfirmChallenge(string payload, string playerOneId, string groupId)
        {
            await Clients.Client(playerOneId).SendAsync("AcceptChallenge", payload, groupId);
        }

        public async Task StartGame(string payload)
        {
            await MQTTBrokerService.PublishAsync("Challenge", payload);
        }

        public async Task AbortChallenge(string groupId, string playerName)
        {
            await Clients.Group(groupId).SendAsync("ChallengeAborted", playerName, groupId);
        }

        public async Task RemoveFromGroup(string groupId, string connectionId)
        {
            await Groups.RemoveFromGroupAsync(connectionId, groupId);
        }




        // Player vs Robot
        public async Task CreateRobot(string id)
        {
            await MQTTBrokerService.PublishAsync("SubscribeRobot", id);
        }

        public async Task AddRobot(string robotID)
        {
            await Clients.All.SendAsync("UpdateRobotLobby", HomeController.robotsInHub);
            return;
        }

        public async Task RemoveRobot(string robotID)
        {
            await Clients.All.SendAsync("UpdateRobotLobby", HomeController.robotsInHub);
            return;
        }

        public async Task FillRobotLobby()
        {
            await Clients.All.SendAsync("UpdateRobotLobby", HomeController.robotsInHub);
        }

        public async Task SendNotificationRobot(string robot)
        {
            await Clients.Others.SendAsync("ReceiveNewRobot", robot);
        }

        public async Task GetAvailableRobots()
        {
            await Clients.Caller.SendAsync("ReceiveAvailableRobots", HomeController.robotsInHub);
        }

        public async Task ChallengeRobot(string playerOne, string robot)
        {
            string payload = $"{playerOne},{robot}";
            await MQTTBrokerService.PublishAsync("ChallengeRobot", payload);
        }

        public async Task ShowAvailableRobots()
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("RegisterRobot", "ShowAll");
        }
    }
}
