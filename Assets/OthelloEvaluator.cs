using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Othello
{
    public class OthelloEvaluator : Board
    {
        readonly double[,] evaluateValue = new double[8, 8]
        {
            { 45.0, -11.0, 4.0, -1.0, -1.0, 4.0, -11.0, 45.0 },
            { -11.0, -16.0, -1.0, -3.0, -3.0, -1.0, -16.0, -11.0 },
            { 4.0, -1.0, 2.0, -1.0, -1.0, 2.0, -1.0, 4.0 },
            { -1.0, -3.0, -1.0, 0.0, 0.0, -1.0, -3.0, -1.0 },
            { -1.0, -3.0, -1.0, 0.0, 0.0, -1.0, -3.0, -1.0 },
            { 4.0, -1.0, 2.0, -1.0, -1.0, 2.0, -1.0, 4.0 },
            { -11.0, -16.0, -1.0, -3.0, -3.0, -1.0, -16.0, -11.0 },
            { 45.0, -11.0, 4.0, -1.0, -1.0, 4.0, -11.0, 45.0}
        };

        /*
        readonly double[,] evaluateValue = new double[8, 8]
        {
            {30.0, -12.0, 0.0, -1.0, -1.0, 0.0, -12.0, 30.0 },
            {-12.0, -15.0, -3.0, -3.0, -3.0, -3.0, -15.0, -12.0 },
            {0.0, -3.0, 0.0, -1.0, -1.0, 0, -3.0, 0.0 },
            {-1.0, -3.0, -1.0, -1.0, -1.0, -1.0, -3.0, -1.0 },
            {-1.0, -3.0, -1.0, -1.0, -1.0, -1.0, -3.0, -1.0 },
            {0.0, -3.0, 0.0, -1.0, -1.0, 0, -3.0, 0.0 },
            {-12.0, -15.0, -3.0, -3.0, -3.0, -3.0, -15.0, -12.0 },
            {30.0, -12.0, 0.0, -1.0, -1.0, 0.0, -12.0, 30.0 }
        };
        */

        public readonly double w1 = 3.0 + Utils.Utils.GetRand();
        public readonly double w2 = 5.0 + Utils.Utils.GetRand();
        public readonly double w3 = 1.0 + Utils.Utils.GetRand();

        readonly Pos[] corners = new Pos[4]
        {
            new Pos(){ x = 0, y = 0 },
            new Pos(){ x = 0, y = boardSize-1 },
            new Pos(){ x = boardSize-1, y = 0 },
            new Pos(){ x = boardSize-1, y = boardSize-1 }
        };

        readonly Pos[] xPos = new Pos[4]
        {
            new Pos(){ x = 1, y = 1 },
            new Pos(){ x = 1, y = boardSize-2 },
            new Pos(){ x = boardSize-2, y = 1 },
            new Pos(){ x = boardSize-2, y = boardSize-2 },
        };


        // Whether the stone at the position is imreversible
        bool IsConfirmed(Pos pos, int color)
        {
            // Return false if there is not self stone at the position
            if (board[pos.y, pos.x] != color)
            {
                return false;
            }


            // Check if the position is connected to the corners only with own stones

            // Vertical line
            // (0, pos.x) ~ (pos.y-1, pos.x) are self stones　or　(pos.y+1, pos.x) ~ (boarSize-1, pos.x) are self stones
            if (pos.x == 0 || pos.x == boardSize - 1)
            {
                bool upperFlag = true;
                bool lowerFlag = true;
                for (int j=0; j < pos.y; j++)
                {
                    if (board[j, pos.x] != color)
                    {
                        upperFlag = false;
                        break;
                    }
                }
                for(int j=pos.y+1; j < boardSize; j++)
                {
                    if (board[j, pos.x] != color)
                    {
                        lowerFlag = false;
                        break;
                    }
                }
                return upperFlag || lowerFlag;
            }

            // Horizontal line
            // (pos.y, 0) ~ (pos.y, pos.x-1) are self stones  or  (pos.y, pos.x+1) ~ (pos.y, boardSize-1) are self stones
            if (pos.y == 0 || pos.y == boardSize - 1)
            {
                bool leftFlag = true;
                bool rightFlag = true;
                for (int i=0; i < pos.x; i++)
                {
                    if (board[pos.y, i] != color)
                    {
                        leftFlag = false;
                        break;
                    }
                }
                for (int i=pos.x+1 ; i < boardSize; i++)
                {
                    if (board[pos.y, i] != color)
                    {
                        rightFlag = false;
                        break;
                    }
                }
                return leftFlag || rightFlag;
            }

            return false;
        }

        // Get imreversible stone number
        public int Confirms(int color)
        {
            int total = 0;
            Pos top;
            Pos bottom;
            Pos left;
            Pos right;
            for (int i=0; i < boardSize; i++)
            {
                top = new Pos() { x = i, y = 0 };
                bottom = new Pos() { x = i, y = boardSize - 1 };
                left = new Pos() { x = 0, y = i };
                right = new Pos() { x = boardSize - 1, y = i };
                if (IsConfirmed(top, color) || IsConfirmed(bottom, color) || IsConfirmed(left, color) || IsConfirmed(right, color))
                {
                    total += 1;
                }
            }
            return total;
        }

        // Self stone => +1.0, Opponent stone => -1.0, Blank => 0.0
        int[,] GetBoardForEval(int color)
        {
            int[,] arr = new int[8, 8];
            Array.Copy(board, arr, board.Length);

            for (int i=0; i < arr.GetLength(0); i++)
            {
                for (int j=0; j < arr.GetLength(1); j++)
                {
                    if (arr[i, j] == color)
                    {
                        arr[i, j] = 1;
                    }
                    else if (arr[i, j] == StoneColor.OppColor(color))
                    {
                        arr[i, j] = -1;
                    }
                    else
                    {
                        arr[i, j] = 0;
                    }
                }
            }
            return arr;

        }

        // Calculate evaluation value
        internal double Evaluate(int color)
        {
            System.Random r = new System.Random();

            double fs = (Confirms(color) - Confirms(StoneColor.OppColor(color)) + r.NextDouble() * 3) * 11;

            double cn = (Availables(color).Count + r.NextDouble() * 2) * 10;

            double bp = 0.0;
            int[,] tmpBoard = GetBoardForEval(color);
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    bp += evaluateValue[i, j] * tmpBoard[i, j] * r.NextDouble() * 3;
                }
            }

            return w1 * bp + w2 * fs + w3 * cn;
        }
    }
}
