using System;
using System.Collections;
using System.Collections.Generic;

namespace Othello
{
    public struct Pos
    {
        public int x;
        public int y;
    }

    public static class StoneColor
    {
        public const int black = 1;
        public const int white = -1;

        public static int OppColor(int color)
        {
            switch(color)
            {
                case black:
                    return white;
                case white:
                    return black;
                default:
                    throw new ArgumentException("Invalid color");
            }
        }
    }

    // Gereral Othello Board Class
    // Contains a numerical array representing othello board
    public class Board
    {
        internal const int boardSize = 8;
        protected int[,] board = new int[8, 8];


        // Initialize the numeric array board
        internal void Init()
        {
            board = new int[8, 8]
            {
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, StoneColor.black, StoneColor.white, 0, 0, 0 },
                { 0, 0, 0, StoneColor.white, StoneColor.black, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0 }
            };
        }

        // Get a numeric array board passing by value
        public int[,] GetBoard()
        {
            int[,] tmp = new int[boardSize, boardSize];
            Array.Copy(board, tmp, board.Length);
            return tmp;
        }

        // Set a numeric array board passing by value
        internal void SetBoard(int[,] source)
        {
            if (source.Length != board.Length)
            {
                throw new ArgumentException("Invalid array size");
            }
            Array.Copy(source, board, source.Length);
        }

        void Put(Pos pos, int color)
        {
            board[pos.y, pos.x] = color;
        }

        void Reverse(Pos pos)
        {
            board[pos.y, pos.x] = StoneColor.OppColor(board[pos.y, pos.x]);
        }

        bool IsBlank(Pos pos)
        {
            return board[pos.y, pos.x] == 0;
        }

        // Count the stones of the color in the whole board
        internal int CountStones(int color)
        {
            int count = 0;
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    if (board[i, j] == color)
                    {
                        count += 1;
                    }
                }
            }
            return count;
        }

        // Return the 2 dimentional array of the position where the stone is reversible 
        internal List<List<Pos>> GetReversibles(Pos pos, int color)
        {
            int[,] directionXY = new int[8, 2]
            {
            {-1, -1 }, {0, -1}, {1, -1},
            {-1, 0 },           {1, 0},
            {-1, 1 }, {0, 1}, {1, 1}
            };

            List<List<Pos>> reversibleStones = new List<List<Pos>>();
            for (int i = 0; i < directionXY.GetLength(0); i++)
            {
                reversibleStones.Add(new List<Pos>());
            }

            for (int i = 0; i < directionXY.GetLength(0); i++)
            {
                int dx = directionXY[i, 0];
                int dy = directionXY[i, 1];
                int x = pos.x;
                int y = pos.y;
                for (int j = 0; j < boardSize; j++)
                {
                    x += dx;
                    y += dy;
                    if (!(0 <= x && x < boardSize && 0 <= y && y < boardSize))
                    {
                        reversibleStones[i] = new List<Pos>();
                        break;
                    }
                    if (board[y, x] == StoneColor.OppColor(color))
                    {
                        reversibleStones[i].Add(new Pos() { x = x, y = y });
                    }
                    else if (board[y, x] == color)
                    {
                        break;
                    }
                    else
                    {
                        reversibleStones[i] = new List<Pos>();
                        break;
                    }
                }
            }
            return reversibleStones;
        }

        // Whether you can put a stone on this position
        internal bool IsAvailable(Pos pos, int color)
        {
            if (!IsBlank(pos))
            {
                return false;
            }
            List<List<Pos>> reversibleStones = GetReversibles(pos, color);

            foreach (List<Pos> item in reversibleStones)
            {
                if (item.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        // Return the 1 dimentional array of position where you can put a stone
        internal List<Pos> Availables(int color)
        {
            List<Pos> availables = new List<Pos>();
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    Pos pos = new Pos() { x = x, y = y };
                    if (IsAvailable(pos, color))
                    {
                        availables.Add(pos);
                    }
                }
            }
            return availables;
        }

        // Update the numeric array board
        internal void UpdateBoard(Pos pos, int color)
        {
            Put(pos, color);
            List<List<Pos>> reversibles = GetReversibles(pos, color);
            for (int i = 0; i < reversibles.Count; i++)
            {
                foreach (Pos reversible in reversibles[i])
                {
                    Reverse(reversible);
                }
            }
        }
    }
}
