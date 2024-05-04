using Microsoft.AspNetCore.Mvc;
using VierGewinnt.Models;
using VierGewinnt.ViewModels;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Board()
        {
            return View();
        }

    }
}
