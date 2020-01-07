using System;
using System.Collections.Generic;
using System.Text;

namespace SysKit.ODG.Base.Utils
{
    public static class RandomThreadSafeGenerator
    {
        //https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way/
        private static Random _global = new Random();

        [ThreadStatic]
        private static Random _local;

        public static int Next(int maxValue)
        {
            Random inst = _local;
            if (inst == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = inst = new Random(seed);
            }
            return inst.Next(maxValue);
        }
    }
}
