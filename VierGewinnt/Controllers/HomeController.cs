using Microsoft.AspNetCore.Mvc;
using MQTTnet.Client;
using MQTTnet;
using System.Diagnostics;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Models;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using VierGewinnt.Hubs;
using VierGewinnt.Data.Models;
using VierGewinnt.Data.Repositories;
using VierGewinnt.Data.Model;

namespace VierGewinnt.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;



        private string username;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
 
        }

        public IActionResult Index()
        {

            return View();
        }

        public async Task<IActionResult> GameLobby()
        {
            return View();
        }

        public IActionResult Leaderboard()
        {
            return View();
        }

        public IActionResult MatchHistory()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
