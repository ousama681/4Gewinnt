using Microsoft.AspNetCore.Mvc;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        public IActionResult PvP()
        {
            return View();
        }
        public IActionResult PvE()
        {
            return View();
        }
        public IActionResult EvE()
        {
            return View();
        }
    }
}
