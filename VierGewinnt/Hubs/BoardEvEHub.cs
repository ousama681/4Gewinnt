using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using VierGewinnt.Services;

namespace VierGewinnt.Hubs
{
    public class BoardEvEHub : Hub
    {

        public static IHubContext<BoardEvEHub> hubContext = null;

        public async Task MakeFirstMove()
        {
            await RobotVsRobotManager.MakeNextMoveEvE();
        }

        public static async Task CallAnimateHandler(string currentColumn)
        {
            await hubContext.Clients.All.SendAsync("AnimateMove", currentColumn);
        }

        public static async Task MakeNextMove()
        {
            await RobotVsRobotManager.MakeNextMoveEvE();
        }


        public static async Task GameIsOver()
        {
            string text = "Roboter " + RobotVsRobotManager.winner + " hat gewonnen.";
            await hubContext.Clients.All.SendAsync("NotificateGameEnd", text);
        }

        public static async Task PublishToCoordinate(string column)
        {
            await MQTTBroker.MQTTBrokerService.PublishAsync("coordinate", column);
        }

        public int DoWorkInterlocked(List<string> workToProcess)
        {
            int statusCounter = 0;
            int totalCount = workToProcess.Count;

            Parallel.ForEach(workToProcess, (work) =>
            {
                var r = new Random();
                Thread.Sleep((int)(r.NextDouble() * 10));

                Interlocked.Increment(ref statusCounter);
                Console.WriteLine(String.Format("Completed {0} of {1}  items on thread {2}",
                                                statusCounter,
                                                totalCount,
                                                Thread.CurrentThread.ManagedThreadId));
            });

            Console.WriteLine(String.Format("Completed {0} of {1}  items on thread {2}",
                                            statusCounter,
                                            totalCount,
                                            Thread.CurrentThread.ManagedThreadId));
            return statusCounter;
        }
    }
}
