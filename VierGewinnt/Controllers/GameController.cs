using Microsoft.AspNetCore.Mvc;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Data.Models;
using VierGewinnt.ViewModels;
using VierGewinnt.Services;
using Microsoft.AspNetCore.SignalR;
using VierGewinnt.Hubs;
using VierGewinnt.Data;
using VierGewinnt.Services.AI;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        private readonly IGameRepository _gameRepository;
        private readonly IAccountRepository _accountRepository;

        public GameController(IGameRepository gameRepository,
            IAccountRepository accountRepository)
        {
            _gameRepository = gameRepository;
            _accountRepository = accountRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Board(int gameId)
        {
            GameViewModel gameViewModel = new GameViewModel();
            GameBoard gameBoard = await _gameRepository.GetByIdAsync(new GameBoard() { ID = gameId });
            gameBoard.playerNames.PlayerOneName = _accountRepository.GetByIdAsync(new ApplicationUser() { Id = gameBoard.PlayerOneID }).Result.UserName;
            gameBoard.playerNames.PlayerTwoName = _accountRepository.GetByIdAsync(new ApplicationUser() { Id = gameBoard.PlayerTwoID }).Result.UserName;
            gameViewModel.Board = gameBoard;

            GameHub.playerOne = new GameHub.BoardPlayer() { PlayerName = gameBoard.PlayerOneName, PlayerNr = 1 };
            GameHub.playerTwo = new GameHub.BoardPlayer() { PlayerName = gameBoard.PlayerTwoName, PlayerNr = 2 };
            GameHub.currPlayerNr = 1;
            GameHub.InitColDepth();
            GameHub.board = new int[6, 7];

            GameManager.playerOneName = gameBoard.PlayerOneName;
            GameManager.playerTwoName = gameBoard.PlayerTwoName;
            return View(gameViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> BoardPvE(int gameId)
        {
            GameViewModel gameViewModel = new GameViewModel();
            GameBoard gameBoard;

            gameBoard = await _gameRepository.GetByIdAsync(new GameBoard() { ID = gameId });
            gameViewModel.Board = gameBoard;
            BoardPvEHub.currentPlayer = gameBoard.PlayerOneName;
            BoardPvEHub.playerName = gameBoard.PlayerOneName;
            BoardPvEHub.robotName = gameBoard.PlayerTwoName;
            BoardPvEHub.currPlayerNr = 1;
            BoardPvEHub.currGameId = gameId;
            BoardPvEHub.robotMappingReversed.TryAdd(1, gameBoard.PlayerOneName);
            BoardPvEHub.robotMappingReversed.TryAdd(2, gameBoard.PlayerTwoName);
            BoardPvEHub.board = new int[6, 7];
            BoardPvEHub.InitColDepth();

            GameManager.playerOneName = gameBoard.PlayerOneName;
            GameManager.playerTwoName = gameBoard.PlayerTwoName;
            return View(gameViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> BoardEvE(string robotOneName, string robotTwoName)
        {
            GameViewModel gameViewModel = new GameViewModel();

            GameBoard gameBoard = new GameBoard();

            Robot robotOne = new Robot() { Name = robotOneName };
            Robot robotTwo = new Robot() { Name = robotTwoName };

            gameBoard.PlayerOneID = robotOne.Name;
            gameBoard.PlayerTwoID = robotTwo.Name;
            gameBoard.PlayerOneName = robotOne.Name;
            gameBoard.PlayerTwoName = robotTwo.Name;
            gameViewModel.Board = gameBoard;

            BoardEvEHub.board = new int[6, 7];
            BoardEvEHub.currGameId = gameBoard.ID;
            BoardEvEHub.currentRobotMove = robotOne.Name;
            BoardEvEHub.currPlayerNr = 1;
            BoardEvEHub.otherRobotNr = 2;
            BoardEvEHub.robotMappingReversed.TryAdd(1, robotOne.Name);
            BoardEvEHub.robotMappingReversed.TryAdd(2, robotTwo.Name);
            BoardEvEHub.feedBackCounter = 0;
            BoardEvEHub.InitColDepth();

            return View(gameViewModel);
        }
    }
}