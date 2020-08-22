using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils
{
    public static class Utils
    {

        // Return a random double type value
        public static double GetRand()
        {
            int seed = Environment.TickCount;
            var rnd = new Random(seed++);
            double x, y;
            x = rnd.NextDouble();
            y = rnd.NextDouble();
            return (x - y) / 2;
        }
    }
}
