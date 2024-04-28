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
    }
}
