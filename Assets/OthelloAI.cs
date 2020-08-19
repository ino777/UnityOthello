using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


using UnityEngine;

namespace Othello
{
    public class OthelloAI
    {
        internal int depthMax;
        private int selfColor;

        internal Pos optAction;

        OthelloEvaluator evaluator = new OthelloEvaluator();

        Dictionary<int, double> transpositionTable = new Dictionary<int, double>();


        public OthelloAI(int depth, int color)
        {
            depthMax = depth;
           selfColor = color;
        }

        // Acquire the optimal action using alpha beta algorithm
        internal Pos AcquireOptAction(int[,] board, int depth, int color)
        {
            transpositionTable = new Dictionary<int, double>();
            AlphaBeta(board, depth, color);
            return optAction;
        }

        // Alpha beta searching
        private double AlphaBeta(int[,] board, int depth, int color, double alpha=double.NegativeInfinity, double beta=double.PositiveInfinity)
        {
            evaluator.SetBoard(board);

            // Return evaluation value if reaching depth=0
            if (depth <= 0)
            {
                return evaluator.Evaluate(selfColor);
            }

            List<Pos> newOptions = evaluator.Availables(selfColor);
            List<Pos> oppNewOptions = evaluator.Availables(StoneColor.OppColor(selfColor));

            // Return evaluation value when the game end (end node)
            if (newOptions.Count == 0 && oppNewOptions.Count == 0)
            {
                return evaluator.Evaluate(selfColor);
            }

            // When only the opponent can put stone
            if (newOptions.Count == 0)
            {
                color = StoneColor.OppColor(color);
                depth -= 1;
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


            // TODO: Sort children


            // Alpha beta searching
            if (selfColor == color)
            {
                Pos bestAction = newOptions[0];
                foreach (int[,] child in children)
                {
                    double score;

                    // Check if the child is the same with any boards that came up before
                    // If it matches, set the value for the score
                    // If not, start alpha-beta-searching in next layer and get the score
                    int childHash = child.GetHashCode();
                    if (transpositionTable.ContainsKey(childHash))
                    {
                        score = transpositionTable[childHash];
                    }
                    else
                    {
                        score = AlphaBeta(
                            child, depth - 1, StoneColor.OppColor(color), alpha, beta);
                        transpositionTable.Add(childHash, score);
                    }


                    if (score > alpha)
                    {
                        alpha = score;

                        // Get best action
                        if (depth == depthMax)
                        {
                            foreach (KeyValuePair<Pos, int[,]> kvp in actionChildTable)
                            {
                                if (kvp.Value.Cast<int>().SequenceEqual(child.Cast<int>()))
                                {
                                    bestAction = kvp.Key;
                                }
                            }
                        }

                    }

                    // Beta cut
                    if (alpha >= beta) { break; }
                }
                optAction = bestAction;
                return alpha;
            }
            else
            {
                foreach (int[,] child in children)
                {
                    double score;

                    int childHash = child.GetHashCode();
                    if (transpositionTable.ContainsKey(childHash))
                    {
                        score = transpositionTable[childHash];
                    }
                    else
                    {
                        score = AlphaBeta(
                            child, depth - 1, StoneColor.OppColor(color), alpha, beta);
                        transpositionTable.Add(childHash, score);
                    }

                    beta = Math.Min(beta, score);

                    // Alpha cut
                    if (beta <= alpha) { break; }
                }
                return beta;
            }

        }
    }
}
