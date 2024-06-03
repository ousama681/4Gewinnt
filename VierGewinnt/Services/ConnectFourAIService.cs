using System;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using static VierGewinnt.Data.Models.GameBoard;

namespace VierGewinnt.Services
{
    public class ConnectFourAIService
    {
        public static int GetNextRandomMove()
        {
            Random random = new Random();
            return random.Next(1, 7);
        }
    }
}
