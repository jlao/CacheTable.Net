using System;
using System.Runtime.CompilerServices;

namespace CacheTable
{
    public class XorShiftRandom
    {
        [ThreadStatic]
        private static readonly Random seedRng = new Random();

        private uint state;

        public XorShiftRandom() : this(GetNonZeroSeed())
        {
        }

        public XorShiftRandom(uint seed)
        {
            this.state = seed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Next(int max)
        {
            uint x = this.state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            this.state = x;
            return (int)(x % max);
        }

        private static uint GetNonZeroSeed()
        {
            while (true)
            {
                uint seed = (uint)seedRng.Next();
                if (seed != 0)
                {
                    return seed;
                }
            }
        }
    }
}
