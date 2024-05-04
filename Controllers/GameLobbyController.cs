using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Models;
using VierGewinnt.ViewModels;

namespace VierGewinnt.Controllers
{
    public class GameLobbyController : Controller
    {
        public async Task<IActionResult> GameLobby(string username)
        {
            return View();
        }
    }
}
