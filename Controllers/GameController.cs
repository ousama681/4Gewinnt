using Microsoft.AspNetCore.Mvc;
using VierGewinnt.Models;
using VierGewinnt.ViewModels;

namespace VierGewinnt.Controllers
{
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("/GameLobby/Game")]
        public IActionResult SpielzugAusfuehren(int columnnr, int playernr)
        {
            GameViewModel vm = new GameViewModel();

            // aus Repository aktueller Spielstand laden.

            // Nun muss ein Call zum roboter geschickt werden.

            // Spielzug kommt an mit den Parametern.

            // Hier im nächsten Schritt den Zug per MQTT dem Roboter weiterleiten.

            // Beachte Das ViewModel muss eine Eigenschaft besitzen, die das Board repräsentiert. 

            // ZWei Möglichkeiten

            // Entweder wir führen die Spielstein Bewegung aus während dem der Roboter sie ausführt.

            // Oder

            // Wir führen diese erst aus wenn der Roboter die Response für den ausgeführten Zug schickt.

            return View();
        }
    }
}
