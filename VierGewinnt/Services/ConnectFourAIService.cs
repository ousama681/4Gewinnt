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
        public static IDictionary<int, bool> columnsFull = new Dictionary<int, bool>();

        static ConnectFourAIService()
        {
            columnsFull.Add(1, false);
            columnsFull.Add(2, false);
            columnsFull.Add(3, false);
            columnsFull.Add(4, false);
            columnsFull.Add(5, false);
            columnsFull.Add(6, false);
            columnsFull.Add(7, false);
        }

        public static int GetNextRandomMove(int[,] board)
        {
            int randomColumn;

            Random random = new Random();


            bool isColumnFull = false;
            do
            {
                //Column + 1 entspricht der UI Kolonne
                randomColumn = random.Next(1, 8);
                if (board[0, randomColumn-1] != 0)
                {
                    isColumnFull = true;
                    columnsFull[randomColumn] = true;
                    if (isAllColumnsFull())
                    {
                        throw new Exception("Alle Kolonnen sind voll");
                    }
                } else
                {
                    isColumnFull = false;
                }
            } while (isColumnFull);

            return randomColumn;
        }

        private static bool isAllColumnsFull()
        {
            foreach (var column in columnsFull)
            {
                if (column.Value == false)
                {
                    return false;
                }
            }

            return true;
        }
    }


   
}
