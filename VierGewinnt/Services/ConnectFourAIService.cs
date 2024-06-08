using MQTTnet.Client;
using MQTTnet;
using System;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;
using static VierGewinnt.Data.Models.GameBoard;
using System.Text;

namespace VierGewinnt.Services
{
    public static class ConnectFourAIService
    {
        static ConnectFourAIService()
        {
            
        }

        public static int GetNextRandomMove()
        {
            Random random = new Random();
            return random.Next(1, 7);
        }
    }


   
}
