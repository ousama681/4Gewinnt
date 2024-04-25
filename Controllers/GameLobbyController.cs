using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VierGewinnt.Models;
using VierGewinnt.Repositories.Interfaces;
using VierGewinnt.ViewModels.GameLobby;

namespace VierGewinnt.Controllers
{
    public class GameLobbyController : Controller
    {
        private readonly IAccountRepository _accountRepository;


        // Mal überlegen wie oft wir die _users aktualisieren. Eigentlich sobald jemand eingeloggt ist und den Gamelobby button anklickt, hier den User adden. Wenn jemand die Seite verlässt, wieder den User entfernen.
        // Achtung. Die Liste wird für die jeweiligen Viewer nur dann geupdatet wenn sie ihre Seite refreshen. 
        private readonly List<string> _users;

        public GameLobbyController(IAccountRepository accountRepository)
        {
            this._accountRepository = accountRepository;
            _users = new List<string>();
        }

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

        public async Task<IActionResult> GameLobby(string username)
        {
            GameLobbyViewModel vm = new GameLobbyViewModel();
            IEnumerable<string?> enumerable = _accountRepository.GetAllAsync().Result.ToList().Select(u => u.UserName);

            List<string> userNames = new List<string>(enumerable);
            userNames.Remove(username);

            vm.Playernames = userNames;


            //_users.Add(userName);

            // Hier noch eine Methode bauen die nur Spielernamen anzeigt die in keinem Spiel sind
            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> Challenge(string userName, string playerTwoUsername)
        {
            // Wenn ein User nun jemanden herausfordet, diesen User aus der Liste nehmen. Falls die HErausforderung nicht angenommen wird, dann den User wieder einfügen.
            _users.Remove(userName);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AcceptChallenge(string userName)
        {
            _users.Remove(userName);
            // Wenn ein User die Challenge akzeptiert, dann auch ihn aus der UserListe entfernen.
            return View();
        }
    }
}
