using VierGewinnt.Data.Models;

namespace VierGewinnt.Data.Model
{
    public class PlayerRanking
    {
        public int ID { get; set; }
        public string PlayerName { get; set; }
        public int Wins { get; set; }

        // Falls ich Motivation und Lust habe, eventuell jeden einzelnen Win speichern und dann per Reduce pro Spieler den Wert anzeigenw, so können wir das auch noch üben.
    }
}
