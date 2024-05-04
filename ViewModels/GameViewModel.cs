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
        }

        public string PlayerOne {  get; set; }
        public string PlayerTwo { get; set; }
        public int ColumnNr { get; set; }
        public int PlayerNr {  get; set; }
        public GameBoard Board {  get; set; }
    }
}
