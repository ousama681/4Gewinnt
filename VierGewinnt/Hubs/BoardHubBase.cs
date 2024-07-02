using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VierGewinnt.Data.Models;
using VierGewinnt.Data;
using VierGewinnt.Services.AI;
using VierGewinnt.Data.Model;

namespace VierGewinnt.Hubs
{
    public abstract class BoardHubBase : Hub
    {


        public async Task GameIsOver(string winnerId, IHubCallerClients callerClients)
        {
            await UpdatePlayerRanking(winnerId);
            await callerClients.All.SendAsync("NotificateGameEnd", winnerId);
        }

        public async Task SendAnimateEvEMove(string currentColumn, IHubCallerClients callerClients)
        {
            await callerClients.All.SendAsync("AnimateMove", currentColumn);
        }

        public async Task SendAnimatePlayerMove(string currentColumn, string playerName, IHubCallerClients callerClients)
        {
            await callerClients.All.SendAsync("AnimatePlayerMove", currentColumn, playerName);
        }

        public static async Task SendRobotGameFinishedMessage()
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", "e");
        }

        public static async Task SetIsFinished(int gameId)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    GameBoard gameboard = dbContext.GameBoards.Include(gb => gb.Moves).Where(gb => gb.ID.Equals(gameId)).Single();
                    gameboard.IsFinished = true;
                    dbContext.GameBoards.Update(gameboard);
                    await dbContext.SaveChangesAsync();
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private static async Task UpdatePlayerRanking(string winnerName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(DbUtility.connectionString);

            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    PlayerRanking pr = dbContext.PlayerRankings.Where(pr => pr.PlayerName.Equals(winnerName)).FirstOrDefault();

                    if (pr == null)
                    {
                        PlayerRanking newPr = new PlayerRanking() { PlayerName = winnerName, Wins = 1 };
                        await dbContext.AddAsync(newPr);
                    }
                    else
                    {
                        pr.Wins = pr.Wins + 1;
                    }

                    await dbContext.SaveChangesAsync();
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }


        public abstract void AddMoveToBoard();
    }
}
