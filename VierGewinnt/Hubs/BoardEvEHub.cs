using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MQTTBroker;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using VierGewinnt.Services;

namespace VierGewinnt.Hubs
{
    public class BoardEvEHub : Hub
    {

        private static string connectionstring = "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

        public async Task MakeFirstMove(string robotOneName)
        {
            string column = ConnectFourAIService.GetNextRandomMove().ToString();
            /**
             * Eigentlich dürften wir nur den zwei Robotern die Daten schicken, da sonst alle sich bewegen würden.
             */
            await MQTTBrokerService.PublishAsync("coordinate", column);

            //await Clients.All.SendAsync("AnimateMove", column);

            RobotVsRobotManager.moveNr++;
            RobotVsRobotManager.AddMoveToBoard(column);

            string currentRobotMove = RobotVsRobotManager.currentRobotMove;
            string robotOne = RobotVsRobotManager.currentGame.PlayerOneID;
            string robotTwo = RobotVsRobotManager.currentGame.PlayerTwoID;
            //RobotVsRobotManager.firstMove = true;


            string robotNextMove = currentRobotMove.Equals(robotOne) ? robotTwo : robotOne;



            // erster Move
        }
    } 
}
