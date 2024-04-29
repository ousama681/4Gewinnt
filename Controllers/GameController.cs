using Microsoft.AspNetCore.Mvc;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
