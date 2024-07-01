
using System;
using System.Diagnostics;

namespace VierGewinnt.Services
{
    public class MoveCache
    {
        private static Dictionary<string, ReturnMove> boardCache = new Dictionary<string, ReturnMove>();

        //public static string CalculateBoard(AIBoard board)
        //{
        //    string calculatedValue = "";
        //    for (int i = 0; i < board.ROW_COUNT; i++)
        //    {
        //        for (int j = 0; j < board.COL_COUNT; j++)
        //        {
        //            calculatedValue += board.board[i, j];
        //        }
        //    }

        //    return calculatedValue;
        //}

        //private static AIBoard MirrorBoard(AIBoard board)
        //{
        //    AIBoard newBoard = new AIBoard(board);
        //    for (int i = 0; i < board.ROW_COUNT; i++)
        //    {
        //        for (int j = 0; j < board.COL_COUNT; j++)
        //        {
        //            if (board.board[i, j] == 1) newBoard.board[i, j] = 2;
        //            else if (board.board[i, j] == 2) newBoard.board[i, j] = 1;
        //        }
        //    }

        //    return newBoard;
        //}

        //private static bool AppendToFile(string board, ReturnMove move)
        //{
        //    try
        //    {
        //        using (StreamWriter sw = File.AppendText("cache.txt"))
        //        {
        //            sw.WriteLine(board + "," + move.Column + "," + move.Score + "," + move.Iterations);
        //        }

        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        return false;
        //    }
        //}

        //public static void LoadCache()
        //{
        //    try
        //    {
        //        using (StreamReader sr = new StreamReader("cache.txt"))
        //        {
        //            string? line = "";
        //            while ((line = sr.ReadLine()) != null)
        //            {
        //                //Ignores comments
        //                if (line.StartsWith("#")) continue;

        //                string[] split = line.Split(',');
        //                boardCache.Add(split[0], new ReturnMove(int.Parse(split[1]), int.Parse(split[2]), int.Parse(split[3])));
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.WriteLine(e.Message);
        //    }

        //    boardCache = boardCache.OrderByDescending(x => x.Key.Count(c => c == '0')).ToDictionary(x => x.Key, x => x.Value);
        //}

        //public static void AddToCache(string board, ReturnMove move)
        //{
        //    if (boardCache.ContainsKey(board)) return;
        //    boardCache.Add(board, move);
        //    AppendToFile(board, move);
        //    Debug.WriteLine("Appended to cache", "CACHE", ConsoleColor.Green);
        //}

        //public static void AddToCache(AIBoard board, ReturnMove move)
        //{
        //    AddToCache(CalculateBoard(board), move);
        //}

        //public static ReturnMove CacheLookup(AIBoard board)
        //{
        //    if (boardCache.Count == 0) LoadCache();

        //    ReturnMove returnMove = new ReturnMove(-1, 0, 0);
        //    string hash = CalculateBoard(board), reverseHash = CalculateBoard(MirrorBoard(board));

        //    Parallel.ForEach(boardCache, (move) =>
        //    {
        //        if (move.Key == hash || move.Key == reverseHash)
        //        {
        //            if (board.ValidMove(move.Value.Column))
        //            {
        //                returnMove = move.Value;
        //                return;
        //            }
        //        }
        //    });

        //    return returnMove;
        //}
    }
}
