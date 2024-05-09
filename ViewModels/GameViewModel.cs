using VierGewinnt.Data.Models;

namespace VierGewinnt.ViewModels
{
    public class GameViewModel
    {
        public GameViewModel(string playerOne, string playerTwo)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
        }

        public GameViewModel()
        {
            RoboterStatus = "Roboter ist nicht ready";
        }

        public string PlayerOne {  get; set; }
        public string PlayerTwo { get; set; }
        public string RoboterStatus { get; set; }
        public int Column { get; set; }
        public int PlayerNr {  get; set; }
        public GameBoard Board {  get; set; }
    }
}
