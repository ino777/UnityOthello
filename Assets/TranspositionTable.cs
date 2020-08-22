using System;
using System.Collections;
using System.Collections.Generic;


namespace Othello
{
    public class TranspositionTableEntry
    {
        public string Hash { get; }
        public int Depth { get; }
        public double Score { get; }
        public bool Exact { get; }

        public TranspositionTableEntry(string hash, int depth, double score, bool exact=true)
        {
            Hash = hash;
            Depth = depth;
            Score = score;
            Exact = exact;
        }
    }
}
