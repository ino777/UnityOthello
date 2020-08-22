using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


using UnityEngine;

namespace Othello
{
    public class OthelloAI
    {
        // The depth to which this AI can search
        private int searchDepth;

        // The limit depth of each iteration in iterative deepening depth-first search
        private int depthMax;

        private int selfColor;

        internal Pos optAction;

        OthelloEvaluator evaluator;

        Dictionary<string, TranspositionTableEntry> transpositionTable;

        // Debug parameter
        int alphaCutCount = 0;
        int betaCutCount = 0;
        int transpositionCutCount = 0;

        public OthelloAI(int depth, int color)
        {
            searchDepth = depth;
            selfColor = color;
            evaluator = new OthelloEvaluator();
            transpositionTable = new Dictionary<string, TranspositionTableEntry>();
            Debug.Log(string.Format("w1{0} w2{1} w3{2}", evaluator.w1, evaluator.w2, evaluator.w3));
        }

        void InitDebugParameter()
        {
            alphaCutCount = 0;
            betaCutCount = 0;
            transpositionCutCount = 0;
        }

        string BoardToHash(int[,] board, int depth)
        {
            return string.Join("", board.Cast<int>()) + "_" + depth.ToString();
        }

        // Acquire the optimal action using alpha beta algorithm
        internal Pos AcquireOptAction(int[,] board, int color)
        {
            InitDebugParameter();
            transpositionTable = new Dictionary<string, TranspositionTableEntry>();
            for (int i=1; i <= searchDepth; i++)
            {
                depthMax = i;
                AlphaBeta(board, color);
            }
            Debug.Log(string.Format("a: {0}, b: {1}, t: {2}", alphaCutCount, betaCutCount, transpositionCutCount));
            return optAction;
        }

        // Alpha beta searching
        private double AlphaBeta(int[,] board, int color, int depth=0,  double alpha=double.NegativeInfinity, double beta=double.PositiveInfinity)
        {

            evaluator.SetBoard(board);

            // Return evaluation value if reaching depth = depthMax
            if (depth >= depthMax)
            {
                return evaluator.Evaluate(selfColor);
            }

            List<Pos> newOptions = evaluator.Availables(color);
            List<Pos> oppNewOptions = evaluator.Availables(StoneColor.OppColor(color));

            // Return evaluation value when the game end (end node)
            if (newOptions.Count == 0 && oppNewOptions.Count == 0)
            {
                return evaluator.Evaluate(selfColor);
            }

            
            // When only the opponent can put stone, go to next layer
            if (newOptions.Count == 0)
            {
                depth += 1;
                color = StoneColor.OppColor(color);
                newOptions = oppNewOptions;
            }
            

            // Expand board and store the all child boards in children list
            // Associate the child and the action of that time
            List<int[,]> children = new List<int[,]>();
            Dictionary<Pos, int[,]> actionChildTable = new Dictionary<Pos, int[,]>();

            foreach (Pos action in newOptions)
            {
                Board childBoard = new Board();
                childBoard.SetBoard(board);
                childBoard.UpdateBoard(action, color);
                children.Add(childBoard.GetBoard());
                actionChildTable.Add(action, childBoard.GetBoard());
            }


            /*
            // Sort children in evaluation value order to speed up alpha beta searching
            // In descending order when self turn and in ascending order when opponent turn

            children = SortByScore(children, depthMax - 1);
            if (color == selfColor)
            {
                children.Reverse();
            }
            */



            // Alpha beta searching
            if (color == selfColor)
            {
                // In self turn, select the max score from children's nodes

                foreach (int[,] child in children)
                {
                    double score;

                    // Check if the child is the same with any boards that came up before
                    // If it matches, set the value for the score
                    // If not, start alpha-beta-searching in next layer and get the score
                    string childHash = BoardToHash(child, depthMax);

                    if (transpositionTable.ContainsKey(childHash))
                    {
                        transpositionCutCount += 1;
                        score = transpositionTable[childHash].Score;

                    }
                    else
                    {
                        score = AlphaBeta(
                            child, StoneColor.OppColor(color), depth + 1,  alpha, beta);
                        TranspositionTableEntry entry = new TranspositionTableEntry(childHash, depthMax, score);
                        transpositionTable.Add(childHash, entry);
                    }
                    // score = AlphaBeta(child, depth - 1, StoneColor.OppColor(color), alpha, beta);


                    if (score > alpha)
                    {
                        alpha = score;

                        // Get best action at root
                        if (depth == 0)
                        {
                            foreach (KeyValuePair<Pos, int[,]> kvp in actionChildTable)
                            {
                                if (kvp.Value.Cast<int>().SequenceEqual(child.Cast<int>()))
                                {
                                    optAction = kvp.Key;
                                }
                            }
                        }

                    }

                    // Beta cut
                    if (alpha >= beta)
                    {
                        betaCutCount += 1;
                        break;
                    }
                }
                return alpha;
            }
            else
            {
                // In the opponent turn, select the minimum score from children's nodes

                foreach (int[,] child in children)
                {
                    double score;
                    
                    string childHash = BoardToHash(child, depthMax);

                    if (transpositionTable.ContainsKey(childHash))
                    {
                        transpositionCutCount += 1;
                        score = transpositionTable[childHash].Score;
                    }
                    else
                    {
                        score = AlphaBeta(
                            child, StoneColor.OppColor(color), depth + 1, alpha, beta);
                        TranspositionTableEntry entry = new TranspositionTableEntry(childHash, depthMax, score);
                        transpositionTable.Add(childHash, entry);
                    }

                    // score = AlphaBeta(child, depth - 1, StoneColor.OppColor(color), alpha, beta);

                    beta = Math.Min(beta, score);

                    // Alpha cut
                    if (beta <= alpha)
                    {
                        alphaCutCount += 1;
                        break;
                    }
                }
                return beta;
            }

        }

        /*
        // Sort by evaluation value in ascending order
        List<int[,]> SortByEval(List<int[,]> list, int color)
        {
            Dictionary<string, double> evalTable = new Dictionary<string, double>();
            foreach (int[,] board in list)
            {
                OthelloEvaluator eval = new OthelloEvaluator();
                eval.SetBoard(board);
                evalTable[BoardToString(board)] = eval.Evaluate(color);
            }

            return _SortByEval(list, evalTable);

        }

        // Quick sort using evaluation table
        List<int[,]> _SortByEval(List<int[,]> list, Dictionary<string, double> evalTable)
        {
            if (list.Count <= 1) { return list; }

            List<int[,]> left = new List<int[,]>();
            List<int[,]> right = new List<int[,]>();
            int[,] refBoard = list[0];
            double refEval = evalTable[BoardToString(refBoard)];
            int refCount = 0;

            foreach (int[,] board in list)
            {
                double boardEval = evalTable[BoardToString(board)];
                if (boardEval < refEval)
                {
                    left.Add(board);
                }
                else if (boardEval > refEval)
                {
                    right.Add(board);
                }
                else
                {
                    refCount += 1;
                }
            }

            left = _SortByEval(left, evalTable);
            right = _SortByEval(right, evalTable);

            List<int[,]> center = new List<int[,]>();
            for (int i=0; i < refCount; i++)
            {
                center.Add(refBoard);
            }
            return left.Concat(center).Concat(right).ToList();
        }
        */
    }
}
