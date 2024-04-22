using Microsoft.AspNetCore.Mvc;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Lobby()
        {
            return View();
        }

        public IActionResult Board()
        {
            return View();
        }
        public IActionResult EvE()
        {
            return View();
        }
    }
}
