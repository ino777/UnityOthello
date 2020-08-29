using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


using UnityEngine;

namespace Othello
{
    public class OthelloAI
    {
        // Limit depth
        private readonly int searchDepth;
        // Alpha beta window size
        private readonly double threshold = 0.5;
        // AI's color
        private readonly int selfColor;

        // Turn
        private int rootTurn;

        // The limit depth of each iteration in iterative deepening depth-first search
        private int currentDepthMax;

        // Best next position
        public Pos bestAction;

        OthelloEvaluator evaluator;

        // Transposition Table
        Dictionary<string, TranspositionTableEntry> transpositionTable;

        // Debug parameters
        int nodeCount = 0;
        int alphaCutCount = 0;
        int betaCutCount = 0;
        int transpositionCutCount = 0;
        int sortAllCount = 0;
        int sortEvalCount = 0;
        System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();

        public OthelloAI(int depth, int color)
        {
            searchDepth = depth;
            selfColor = color;
            currentDepthMax = searchDepth;
            evaluator = new OthelloEvaluator();
            transpositionTable = new Dictionary<string, TranspositionTableEntry>();
            Debug.Log(string.Format("w1{0} w2{1} w3{2}", evaluator.w1, evaluator.w2, evaluator.w3));
        }

        void InitDebugParameter()
        {
            nodeCount = 0;
            alphaCutCount = 0;
            betaCutCount = 0;
            transpositionCutCount = 0;
            sortAllCount = 0;
            sortEvalCount = 0;
            st.Reset();
        }

        // Create a string hash of board
        string BoardToHash(int[,] board)
        {
            return string.Join("", board.Cast<int>());
        }

        // Acquire the optimal action using alpha beta algorithm
        internal Pos AcquireOptAction(int[,] board, int turn)
        {
            InitDebugParameter();
            rootTurn = turn;
            transpositionTable = new Dictionary<string, TranspositionTableEntry>();

            // Iterative deepening (IDAS algorithm)
            currentDepthMax = 3;
            double best = AlphaBeta(board, selfColor);
            for (int i=4; i <= searchDepth; i++)
            {
                // Null window search
                currentDepthMax = i;
                double m = AlphaBeta(board, selfColor, alpha: best - threshold, beta: best + threshold);
                if (m < best - threshold)
                {
                    // fail low
                    m = AlphaBeta(board, selfColor, alpha: double.NegativeInfinity, beta: m);
                }
                else if (m > best + threshold)
                {
                    // fail high
                    m = AlphaBeta(board, selfColor, alpha: m, beta: double.PositiveInfinity);
                }
                best = m;
            }
            Debug.Log(string.Format("node:{0}, a: {1}, b: {2}, t: {3}, sortAll: {4}, sortEval: {5}", nodeCount, alphaCutCount, betaCutCount, transpositionCutCount, sortAllCount, sortEvalCount));
            Debug.Log(string.Format("Sort Time: {0}", st.Elapsed));
            return bestAction;
        }

        // Alpha beta searching with transposition table
        private double AlphaBeta(int[,] board, int color, int depth=0,  double alpha=double.NegativeInfinity, double beta=double.PositiveInfinity)
        {
            nodeCount += 1;

            evaluator.SetBoard(board);
            
            // Return evaluation value if reaching depth = depthMax or terminal node
            if (depth >= currentDepthMax)
            {
                return evaluator.Evaluate(selfColor);
            }


            // Return evaluation when game ends
            List<Pos> newOptions = evaluator.Availables(color);
            List<Pos> oppNewOptions = evaluator.Availables(StoneColor.OppColor(color));

            if (newOptions.Count == 0 && oppNewOptions.Count == 0)
            {
                int selfStones = evaluator.CountStones(selfColor);
                int oppStones = evaluator.CountStones(StoneColor.OppColor(selfColor));
                if (selfStones > oppStones)
                {
                    return double.PositiveInfinity;
                }
                else if (selfStones < oppStones)
                {
                    return double.NegativeInfinity;
                }
                else
                {
                    return evaluator.Evaluate(selfColor);
                }  
            }

            
            // When only the opponent can put stone, go to next depth
            if (newOptions.Count == 0)
            {
                depth += 1;
                color = StoneColor.OppColor(color);
                newOptions = oppNewOptions;
            }
            

            // Expand board and store the all child boards in children list
            // Associate the child and the action of that time
            List<int[,]> children = new List<int[,]>();
            Dictionary<int[,], Pos> actionChildTable = new Dictionary<int[,], Pos>();

            foreach (Pos action in newOptions)
            {
                Board childBoard = new Board();
                childBoard.SetBoard(board);
                childBoard.UpdateBoard(action, color);
                children.Add(childBoard.GetBoard());
                actionChildTable.Add(childBoard.GetBoard(), action);
            }


            // Sort children in order of the score
            // In descending order when self turn and in ascending order when opponent turn
            st.Start();
            if (depth <= 3)
            {
                children = OrderBoards(children, color);
            }
            st.Stop();


            // Alpha beta searching
            if (color == selfColor)
            {
                // In self turn, search max value of children

                double score = double.NegativeInfinity;

                foreach (int[,] child in children)
                {

                    // Check if the child is stored in transposition table and the node type is EXACT
                    // If it does, set the value for the score
                    // If not, start alpha-beta-searching in next depth and store the score

                    string childHash = BoardToHash(child);

                    if (transpositionTable.ContainsKey(childHash) && transpositionTable[childHash].Depth >= currentDepthMax && transpositionTable[childHash].NodeType == "EXACT")
                    {
                        transpositionCutCount += 1;
                        score = transpositionTable[childHash].Score;
                    }
                    else
                    {
                        score = AlphaBeta(child, StoneColor.OppColor(color), depth + 1, alpha, beta);
                        transpositionTable[childHash] = new TranspositionTableEntry(child, currentDepthMax, score);
                    }


                    if (score > alpha)
                    {
                        alpha = score;

                        // Get best action
                        if (depth == 0)
                        {
                            foreach (KeyValuePair<int[,], Pos> kvp in actionChildTable)
                            {
                                if (kvp.Key.Cast<int>().SequenceEqual(child.Cast<int>()))
                                {
                                    bestAction = kvp.Value;
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
                // If the opponent turn, search minimum value of children

                double score = double.PositiveInfinity;

                foreach (int[,] child in children)
                {
                    string childHash = BoardToHash(child);

                    if (transpositionTable.ContainsKey(childHash) && transpositionTable[childHash].Depth >= currentDepthMax && transpositionTable[childHash].NodeType == "EXACT")
                    {
                        transpositionCutCount += 1;
                        score = transpositionTable[childHash].Score;
                    }
                    else
                    {
                        score = AlphaBeta(child, StoneColor.OppColor(color), depth + 1, alpha, beta);
                        transpositionTable[childHash] = new TranspositionTableEntry(child, currentDepthMax, score);
                    }

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

        
        // Order boards
        List<int[,]> OrderBoards(List<int[,]> list, int color)
        {
            List<int[,]> sortedList = SortByScore(list, color);
            if (color == selfColor)
            {
                sortedList.Reverse();
            }
            return sortedList;

        }

        // Quick sort using transposition table in ascending order
        List<int[,]> SortByScore(List<int[,]> list, int color)
        {
            if (list.Count <= 1) { return list; }

            List<int[,]> left = new List<int[,]>();
            List<int[,]> center = new List<int[,]>();
            List<int[,]> right = new List<int[,]>();
            int[,] refBoard = list[0];
            string refBoardHash = BoardToHash(refBoard);
            double refScore;
            if (transpositionTable.ContainsKey(refBoardHash))
            {
                refScore = transpositionTable[refBoardHash].Score;
            }
            else
            {
                OthelloEvaluator tmpEvaluator = new OthelloEvaluator();
                tmpEvaluator.SetBoard(refBoard);
                refScore = tmpEvaluator.Evaluate(selfColor);
                transpositionTable[refBoardHash] = new TranspositionTableEntry(refBoard, currentDepthMax, refScore, "TEMP");
            }
            center.Add(refBoard);

            foreach (int[,] board in list)
            {
                sortAllCount += 1;
                string boardHash = BoardToHash(board);
                double boardScore;
                if (transpositionTable.ContainsKey(boardHash))
                {
                    boardScore = transpositionTable[boardHash].Score;
                }
                else
                {
                    sortEvalCount += 1;
                    OthelloEvaluator tmpEvaluator = new OthelloEvaluator();
                    tmpEvaluator.SetBoard(board);
                    boardScore = tmpEvaluator.Evaluate(selfColor);
                    transpositionTable[boardHash] = new TranspositionTableEntry(board, currentDepthMax, boardScore, "TEMP");
                }

                if (boardScore < refScore)
                {
                    left.Add(board);
                }
                else if (boardScore > refScore)
                {
                    right.Add(board);
                }
                else
                {
                    center.Add(board);
                }
            }

            left = SortByScore(left, color);
            right = SortByScore(right, color);

            return left.Concat(center).Concat(right).ToList();
        }
    }
}
