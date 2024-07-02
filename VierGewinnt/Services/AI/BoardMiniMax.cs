using System.Diagnostics;
using static VierGewinnt.Data.Models.GameBoard;

namespace VierGewinnt.Services.AI
{
    public struct BestCalculatedMove
    {
        public int Moves;
        public int Column;
        public int Points;
        public int TransitionSize;

        public BestCalculatedMove(int column, int points, int moves, int transitionSize = 0)
        {
            Column = column;
            Points = points;
            Moves = moves;
            TransitionSize = transitionSize;
        }
    }

    public class BoardMiniMax
    {
        public int BOARD_DEPTH { get; set; }
        public int moveIterations { get; set; } = 0;
        private BoardTransition boardTransition = new BoardTransition();

        public BoardMiniMax(int boardDepth = 6)
        {
            BOARD_DEPTH = boardDepth;
        }

        private List<int> PossibleMoves(ref AIBoard board)
        {
            List<int> possibleMoves = new List<int>();

            for (int i = 0; i < board.COL_COUNT; i++)
            {
                if (board.board[0, i] == 0) possibleMoves.Add(i + 1);
            }

            return possibleMoves;
        }

        private bool IsMoveEnding(AIBoard board)
        {
            return board.ColumnOfBestMove(board) != 0 || board.IsFull();
        }

        private void AdjustCalculatedPoints(ref int playerOne, ref int playerTwo, ref int points)
        {
            if (playerOne == 4) points += 105;
            else if (playerOne == 3 && playerTwo == 0) points += 5;
            else if (playerOne == 2 && playerTwo == 0) points += 2;
            else if (playerOne == 1 && playerTwo == 0) points += 1;
            else if (playerTwo == 4) points -= 105;
            else if (playerTwo == 3 && playerOne == 0) points -= 5;
            else if (playerTwo == 2 && playerOne == 0) points -= 2;
            else if (playerTwo == 1 && playerOne == 0) points -= 1;
        }

        private int Calculate(AIBoard board)
        {
            int points = 0;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int playerOne = 0, playerTwo = 0;

                    for (int k = 0; k < 4; k++)
                    {
                        if (board.board[i, j + k] == 1) playerOne++;
                        else if (board.board[i, j + k] == 2) playerTwo++;
                    }

                    AdjustCalculatedPoints(ref playerOne, ref playerTwo, ref points);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    int playerOne = 0, playerTwo = 0;

                    for (int k = 0; k < 4; k++)
                    {
                        if (board.board[i + k, j] == 1) playerOne++;
                        else if (board.board[i + k, j] == 2) playerTwo++;
                    }

                    AdjustCalculatedPoints(ref playerOne, ref playerTwo, ref points);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int playerOne = 0, playerTwo = 0;

                    for (int k = 0; k < 4; k++)
                    {
                        if (board.board[i + k, j + k] == 1) playerOne++;
                        else if (board.board[i + k, j + k] == 2) playerTwo++;
                    }

                    AdjustCalculatedPoints(ref playerOne, ref playerTwo, ref points);
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 3; j < 7; j++)
                {
                    int playerOne = 0, playerTwo = 0;

                    for (int k = 0; k < 4; k++)
                    {
                        if (board.board[i + k, j - k] == 1) playerOne++;
                        else if (board.board[i + k, j - k] == 2) playerTwo++;
                    }

                    AdjustCalculatedPoints(ref playerOne, ref playerTwo, ref points);
                }
            }

            return points;
        }

        public BestCalculatedMove GetFastestMove(AIBoard board)
        {
            int bestMove = board.NextBestMove(board);

            if (bestMove != -1) return new BestCalculatedMove(bestMove, 100, bestMove);
            return new BestCalculatedMove(-1, 0, 0);
        }

        private bool IsQuiet(ref AIBoard board)
        {
            if (board.TryFourInALine() > 0) return false;
            if (board.TryThreeInALine() > 0) return false;

            return true;
        }

        private int Minimax(AIBoard board, int depth, int maximizing, int minimizing, bool maximizingPlayer)
        {
            moveIterations++;

            if (depth == 0 || IsMoveEnding(board) || IsQuiet(ref board))
            {
                return Calculate(board);
            }

            int bestScore = maximizingPlayer ? int.MinValue : int.MaxValue;

            List<int> validMoves = PossibleMoves(ref board);

            foreach (int move in validMoves)
            {
                AIBoard newBoard = new AIBoard(board);
                newBoard.PlaceMove(move, maximizingPlayer ? 1 : 2);

                int score = 0;

                ulong key = newBoard.GetCoordinateValue();

                if (boardTransition.GetKeyValue(key) != 0)
                {
                    score = boardTransition.GetKeyValue(key);
                }
                else
                {
                    score = Minimax(newBoard, depth - 1, maximizing, minimizing, !maximizingPlayer);
                    boardTransition.AddEntry(key, score);
                }

                if (maximizingPlayer)
                {
                    if (score > bestScore)
                    {
                        bestScore = score;
                        maximizing = Math.Max(maximizing, bestScore);
                    }
                }
                else
                {
                    if (score < bestScore)
                    {
                        bestScore = score;
                        minimizing = Math.Min(minimizing, bestScore);
                    }
                }

                if (minimizing <= maximizing)
                {
                    break;
                }
            }

            return bestScore;
        }

        public BestCalculatedMove GetBestMove(AIBoard board)
        {
            moveIterations = 0;
            int bestScore = int.MinValue;
            int bestMove = 0;

            BestCalculatedMove quickMove = GetFastestMove(board);
            if (quickMove.Column != -1) return quickMove;

            Parallel.ForEach(PossibleMoves(ref board), move =>
            {
                AIBoard newBoard = new AIBoard(board);
                newBoard.PlaceMove(move, 1);

                int score = Minimax(newBoard, BOARD_DEPTH, int.MinValue, int.MaxValue, false);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            });
            BestCalculatedMove returnMove = new BestCalculatedMove(bestMove, bestScore, moveIterations, boardTransition.Conflicts);

            return returnMove;
        }
    }
}
