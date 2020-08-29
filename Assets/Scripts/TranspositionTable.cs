using System;
using System.Collections;
using System.Collections.Generic;


namespace Othello
{
    // Transposition table entry class
    public class TranspositionTableEntry
    {
        public int[,] Board { get; private set; }
        public int Depth { get; private set; }
        public double Score { get; private set; }
        public string NodeType { get; private set; }    // EXACT, LOWERBOUND, UPPERBOUND, TEMP
        public Pos BestMove { get; private set; }

        public TranspositionTableEntry(int[,] board, int depth, double score, string nodeType="EXACT")
        {
            Board = board;
            Depth = depth;
            Score = score;
            NodeType = nodeType;
        }

        public void SetEntry(int[,] board, int depth, double score, string nodeType)
        {
            Board = board;
            Depth = depth;
            Score = score;
            NodeType = nodeType;
        }

        public void UpdateEntry(int[,] newBoard, int newDepth, double newScore, string newNodeType)
        {
            if (newDepth > Depth)
            {
                SetEntry(newBoard, newDepth, newScore, newNodeType);

            }
            if (newDepth == Depth)
            {
                if (newNodeType != "EXACT" && NodeType == "EXACT")
                {
                    // Do nothing
                }
                else
                {
                    SetEntry(newBoard, newDepth, newScore, newNodeType);
                }
            }
        }
    }
}
