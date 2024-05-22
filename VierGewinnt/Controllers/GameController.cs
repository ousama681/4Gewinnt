using Microsoft.AspNetCore.Mvc;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Data.Models;
using VierGewinnt.ViewModels;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        private readonly IGameRepository _gameRepository;

        public GameController(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Board(int gameId)
        {
            GameViewModel gameViewModel = new GameViewModel();
            GameBoard gameBoard = await _gameRepository.GetByIdAsync(new GameBoard() { ID = gameId });
            gameViewModel.Board = gameBoard;
            return View(gameViewModel);
        }
    }
}