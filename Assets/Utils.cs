using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils
{
    public static class Utils
    {

        // Return a random double type value following normal distrubution
        public static double GetRandn()
        {
            int seed = Environment.TickCount;
            var rnd = new Random(seed++);
            double x, y;
            x = rnd.NextDouble();
            y = rnd.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(x)) * Math.Cos(2.0 * Math.PI * y);
        }
    }
}
