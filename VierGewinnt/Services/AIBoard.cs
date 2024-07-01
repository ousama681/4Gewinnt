using static VierGewinnt.Data.Models.GameBoard;

namespace VierGewinnt.Services
{
    public class AIBoard
    {
        public int[,] board = new int[6, 7];
        public int currentPlayerTurn = 1;
        public int moves = 0;

        public int COL_COUNT
        {
            get => board.GetLength(1);
        }

        public int ROW_COUNT
        {
            get => board.GetLength(0);
        }

        public AIBoard() { }

        public AIBoard(AIBoard b)
        {
            for (int i = 0; i < ROW_COUNT; i++)
            {
                for (int j = 0; j < COL_COUNT; j++) board[i, j] = b.board[i, j];
            }

            currentPlayerTurn = b.currentPlayerTurn;
            moves = b.moves;
        }

        public bool IsFull() => moves == 42;

        public ulong GetCoordinateValue()
        {
            ulong coordinateValue = 0;
            for (int i = 0; i < ROW_COUNT; i++)
            {
                for (int j = 0; j < COL_COUNT; j++)
                {
                    coordinateValue = coordinateValue * 3 + (ulong)board[i, j];
                }
            }
            return coordinateValue;
        }

        public bool PlaceMove(int column, int player)
        {
            if (column < 1 || column > 7) return false;
            if (board[0, column - 1] != 0) return false;

            for (int i = 5; i >= 0; i--)
            {
                if (board[i, column - 1] == 0)
                {
                    board[i, column - 1] = player;
                    break;
                }
            }

            moves++;
            currentPlayerTurn = (currentPlayerTurn == 1) ? 2 : 1;
            return true;
        }

        public int ColumnOfBestMove(AIBoard board)
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (board.board[i, j] != 0 &&
                        board.board[i, j] == board.board[i, j + 1] &&
                        board.board[i, j] == board.board[i, j + 2] &&
                        board.board[i, j] == board.board[i, j + 3])
                        return board.board[i, j];
                }
            }

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (board.board[i, j] != 0 &&
                        board.board[i, j] == board.board[i, j + 1] &&
                        board.board[i, j] == board.board[i, j + 2] &&
                        board.board[i, j] == board.board[i, j + 3])
                        return board.board[i, j];
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (board.board[i, j] != 0 &&
                        board.board[i, j] == board.board[i + 1, j] &&
                        board.board[i, j] == board.board[i + 2, j] &&
                        board.board[i, j] == board.board[i + 3, j])
                        return board.board[i, j];
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (board.board[i, j] != 0 &&
                        board.board[i, j] == board.board[i + 1, j + 1] &&
                        board.board[i, j] == board.board[i + 2, j + 2] &&
                        board.board[i, j] == board.board[i + 3, j + 3])
                        return board.board[i, j];
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 3; j < 7; j++)
                {
                    if (board.board[i, j] != 0 &&
                        board.board[i, j] == board.board[i + 1, j - 1] &&
                        board.board[i, j] == board.board[i + 2, j - 2] &&
                        board.board[i, j] == board.board[i + 3, j - 3])
                        return board.board[i, j];
                }
            }

            if (board.IsFull()) return 3;
            return 0;
        }

        public int NextBestMove(AIBoard board)
        {
            int returnValue = -1;

            Parallel.For(1, 7, (i, state) =>
            {
                for (int j = 1; j <= 2; j++)
                {
                    AIBoard temporaryB = new AIBoard(board);
                    if (temporaryB.PlaceMove(i, j))
                    {
                        if (ColumnOfBestMove(temporaryB) == j)
                        {
                            returnValue = i;
                            state.Stop();
                        }
                    }
                }
            });

            return returnValue;
        }

        public int TryFourInALine()
        {
            int returnValue = 0;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (board[i, j] == 0 &&
                        board[i, j + 1] == 0 &&
                        board[i, j + 2] == 0 &&
                        board[i, j + 3] == 0)
                        returnValue++;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (board[i, j] == 0 &&
                        board[i + 1, j] == 0 &&
                        board[i + 2, j] == 0 &&
                        board[i + 3, j] == 0)
                        returnValue++;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (board[i, j] == 0 &&
                        board[i + 1, j + 1] == 0 &&
                        board[i + 2, j + 2] == 0 &&
                        board[i + 3, j + 3] == 0)
                        returnValue++;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 3; j < 7; j++)
                {
                    if (board[i, j] == 0 &&
                        board[i + 1, j - 1] == 0 &&
                        board[i + 2, j - 2] == 0 &&
                        board[i + 3, j - 3] == 0)
                        returnValue++;
                }
            }

            return returnValue;
        }

        public int TryThreeInALine()
        {
            int returnValue = 0;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (board[i, j] == 0 &&
                        board[i, j + 1] == 0 &&
                        board[i, j + 2] == 0)
                        returnValue++;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    if (board[i, j] == 0 &&
                        board[i + 1, j] == 0 &&
                        board[i + 2, j] == 0)
                        returnValue++;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (board[i, j] == 0 &&
                        board[i + 1, j + 1] == 0 &&
                        board[i + 2, j + 2] == 0)
                        returnValue++;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 2; j < 7; j++)
                {
                    if (board[i, j] == 0 &&
                        board[i + 1, j - 1] == 0 &&
                        board[i + 2, j - 2] == 0)
                        returnValue++;
                }
            }

            return returnValue;
        }
    }
}
