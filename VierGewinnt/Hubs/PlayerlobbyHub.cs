using Microsoft.AspNetCore.SignalR;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text;
using VierGewinnt.Data.Interfaces;
using MQTTBroker;
using System.Diagnostics;
using VierGewinnt.Controllers;

namespace VierGewinnt.Hubs
{
    public class PlayerlobbyHub : Hub
    {
        //static readonly IList<string> robots = new List<string>();
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
            await Clients.Client(playerTwoId).SendAsync("ReceiveChallenge", payload, playerOneId, groupId); // send playerTwo a challenge request   
        }

        public async Task ConfirmChallenge(string payload, string playerOneId, string groupId)
        {
            // Challenge got confirmed from playerTwo, now we ask playerOne to accept the game as well
            await Clients.Client(playerOneId).SendAsync("AcceptChallenge", payload, groupId);           
        }

        public async Task StartGame(string payload)
        {
            //PlayerOne also has accepted the Challenge, we can now start the game
            await GameHub.SubscribeToFeedbackAsync("feedback", this.Clients);
            await MQTTBrokerService.PublishAsync("Challenge", payload);
        }

        public async Task AbortChallenge(string groupId, string playerName)
        {
            // Send Message to both players, that the challenge request was not successfull
            await Clients.Group(groupId).SendAsync("ChallengeAborted", playerName, groupId);
        }

        public async Task RemoveFromGroup(string groupId, string connectionId)
        {
            await Groups.RemoveFromGroupAsync(connectionId, groupId);
        }




        // Player vs Robot

        // Zum testen
        public async Task CreateRobot(string id)
        {
            await MQTTBrokerService.PublishAsync("SubscribeRobot", id);
        }

        public async Task AddRobot(string robotID)
        {
            //if (HomeController.robotsInHub.Contains(robotID))
            //{
            //    Debug.WriteLine("ID already exists. Robot could not be added.");
            //    return;
            //}
            //else
            //{
            //    robots.Add(robotID);
            await Clients.All.SendAsync("UpdateRobotLobby", HomeController.robotsInHub);
            //}
            return;
        }

        public async Task RemoveRobot(string robotID)
        {
            //if (robots.Contains(robotID))
            //{
            //    Debug.WriteLine("ID already exists. Robot could not be added.");
            //    return;
            //}
            //else
            //{
                //robots.Add(robotID);
                await Clients.All.SendAsync("UpdateRobotLobby", HomeController.robotsInHub);
            //}
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

        //public async Task LeaveLobbyRobot(string robot)
        //{
        //    if (robots.Contains(robot))
        //    {
        //        robots.Remove(robot);
        //        await Clients.Others.SendAsync("RobotLeft", robot);
        //        return;
        //    }
        //}

        public async Task ShowAvailableRobots()
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("RegisterRobot", "ShowAll");
        }
    }
}
