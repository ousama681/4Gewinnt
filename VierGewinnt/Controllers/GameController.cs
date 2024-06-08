using Microsoft.AspNetCore.Mvc;
using MQTTnet.Client;
using MQTTnet;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Data.Models;
using VierGewinnt.ViewModels;
using System.Text;
using VierGewinnt.Services;
using VierGewinnt.Data.Model;
using System.Diagnostics;
using MQTTBroker;
using Microsoft.AspNetCore.SignalR;
using VierGewinnt.Hubs;
using Microsoft.EntityFrameworkCore;
using VierGewinnt.Data;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        private readonly IGameRepository _gameRepository;
        //private readonly IAccountRepository _accountRepository;
        private readonly IHubContext<BoardEvEHub> _hubContext;
        //private static readonly IList<GameBoard> runningGames;

        private static string connectionstring = "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";

        //static GameController()
        //{
        //    runningGames = new List<GameBoard>();
        //}

        public GameController(IGameRepository gameRepository, IHubContext<BoardEvEHub> hubContext,
            IAccountRepository accountRepository)
        {
            _gameRepository = gameRepository;
            _hubContext = hubContext;
            //_accountRepository = accountRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Board(int gameId)
        {
            GameViewModel gameViewModel = new GameViewModel();
            GameBoard gameBoard = await _gameRepository.GetByIdAsync(new GameBoard() { ID = gameId });
            gameViewModel.Board = gameBoard;
            return View(gameViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> BoardPvE(int gameId)
        {
            GameViewModel gameViewModel = new GameViewModel();
            GameBoard gameBoard = await _gameRepository.GetByIdAsync(new GameBoard() { ID = gameId });
            gameViewModel.Board = gameBoard;
            return View(gameViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> BoardEvE(string robotOneName, string robotTwoName)
        {
            GameViewModel gameViewModel = new GameViewModel();

            GameBoard gameBoard = new GameBoard();

            Robot robotOne = new Robot() { Name = robotOneName };
            Robot robotTwo = new Robot() { Name = robotTwoName };

            RobotVsRobotManager.robotsInGame.Add(robotOneName, 1);
            RobotVsRobotManager.robotsInGame.Add(robotTwoName, 2);

            gameBoard.PlayerOneID = robotOne.Name;
            gameBoard.PlayerTwoID = robotTwo.Name;
            gameViewModel.Board = gameBoard;

            await RobotVsRobotManager.SubscribeToFeedbackTopic();
            RobotVsRobotManager.hubContext = _hubContext;
            RobotVsRobotManager.moves = new Move[7, 6];
            RobotVsRobotManager.currentGame = gameBoard;
            RobotVsRobotManager.currentRobotMove = robotOne.Name;


            RobotVsRobotManager.InitColDepth();

            return View(gameViewModel);
        }

        private async Task<Robot> GetRobotByName(string robotName)
        {

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionstring);


            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    return await dbContext.Robots.Where(r => r.Name.Equals(robotName)).SingleAsync();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            return null;
        }

        private void AnimateMove(string robotName, int columnNR, int gameId, string color)
        {
            _hubContext.Clients.All.SendAsync("AnimateMove", robotName, columnNR, gameId, color);
        }

        private void SaveMoveToDB(string robotName, int columnNR, int gameId)
        {
            Debug.WriteLine("Move Saved To DB");
        }

        private Move[,] CreateMoveArrFromBoard(ICollection<Move> moves)
        {
            try
            {
                Move[,] movesArr = new Move[7, 6];

                Dictionary<string, int> colDepth = new Dictionary<string, int>();
                colDepth.Add("1", 6);
                colDepth.Add("2", 6);
                colDepth.Add("3", 6);
                colDepth.Add("4", 6);
                colDepth.Add("5", 6);
                colDepth.Add("6", 6);
                colDepth.Add("7", 6);

                foreach (Move move in moves)
                {
                    int depth;
                    colDepth.TryGetValue("" + move.Column, out depth);
                    movesArr[(move.Column - 1), depth - 1] = move;
                    colDepth["" + move.Column] = depth - 1;
                }

                return movesArr;
            } catch(IndexOutOfRangeException e)
            {
                Debug.WriteLine(e);
            }
            return null;
        }



        private class BoardParticipants
        {
            public Robot RobotOne {  get; set; }
            public Robot RobotTwo { get; set; }

            public GameBoard Board { get; set; }
        }
    }
}