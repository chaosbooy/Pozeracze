using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PozeraczePart1
{
    internal class GameBoard
    {
        int[,] _board;


        public int[,] Board
        {
            get { return _board; }
            set { _board = value; }
        }

        public GameBoard()
        {
            _board = new int[0,0];
        }

        public GameBoard(int size)
        {
            _board = new int[size, size];
        }

        public bool PlacePiece(int horizontal, int vertical, int value)
        {
            if (Math.Abs(Board[horizontal, vertical]) < value && value * Board[horizontal, vertical] <= 0)
            {
                Board[horizontal, vertical] = value;
                return true;
            }

            return false;
        }

        public bool CheckWin()
        {
            int size = Board.GetLength(0);

            for(int i = 0; i < size; i++)
            {
                bool horizontalWin = true, verticalWin = true;
                for(int j = 0; j < size - 1 && (verticalWin || horizontalWin); j++)
                {
                    if (Board[i, j] * Board[i, j + 1] <= 0)
                        horizontalWin = false;
                    if (Board[j,i] * Board[j + 1, i] <= 0)
                        verticalWin = false;
                }

                if (horizontalWin || verticalWin)
                    return true;
            }

            bool diagonalWin1 = true, diagonalWin2 = true;
            for(int i = 0; i < size - 1 && (diagonalWin1 && diagonalWin2); i++)
            {
                if (Board[i,i] * Board[i + 1, i + 1] <= 0)
                    diagonalWin1 = false;

                if (Board[i, size - i - 1] * Board[i + 1, size - i - 2] <= 0)
                    diagonalWin2 = false;
            }

            return diagonalWin1 || diagonalWin2;
        }
    }
}
