using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace RandomNumberGenerators
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            PcgRandom rng = new PcgRandom();
            rng.Seed((ulong)random.NextInt64(), (ulong)random.NextInt64(), (ulong)random.NextInt64(), (ulong)random.NextInt64());
            while (true)
            {
                int buckets = 1000000;
                int n = 0;
                int[] array = new int[buckets];
                int tries = 0;
                for (; n < buckets; tries++)
                {
                    long r = rng.Next(buckets);
                    if (array[r] == 0)
                        n += 1;
                    array[r]++;
                }
                Console.WriteLine($"Tries = {tries}.");
            }
            Console.ReadLine();
        }
    }

    public sealed class PcgRandom : Random
    {
        // Default seed values from PCG paper: http://www.pcg-random.org/download.html
        ulong state = 0x853c49e6748fea9b;
        ulong inc = 0xda3e39cb94b95bdb;
        ulong state2 = 0x853c49e6748fea9b;
        ulong inc2 = ~0xda3e39cb94b95bdb;

        /// <summary>
        /// Seed with different starting sequences.
        /// </summary>
        /// <param name="initState">Any 64 bit number.</param>
        /// <param name="initSeq">Any 64 bit number different from initSeq2.</param>
        /// <param name="initState2">Any 64 bit number.</param>
        /// <param name="initSeq2">Any 64 bit number different from initSeq.</param>
        public void Seed(ulong initState, ulong initSeq, ulong initState2 = 42, ulong initSeq2 = 57)
        {
            if (initSeq == initSeq2)
                initSeq2 = ~initSeq2;          // Ensure the two sequences are different
            state = 0;
            state2 = 0;
            inc = (initSeq << 1) | 1;          // State must be seeded with an odd value
            inc2 = (initSeq2 << 1) | 1;        // State must be seeded with an odd value
            NextUInt64();
            state += initState;
            state2 += initState2;
            NextUInt64();
        }

        public override int Next()
        {
            return (int)NextUInt32();
        }

        public override int Next(int maxValue)
        {
            return (int)BoundedNextUInt32((uint)maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than maxValue.");
            return (int)BoundedNextUInt32((uint)(maxValue - minValue)) + minValue;
        }

        public override long NextInt64()
        {
            return (long)NextUInt64();
        }

        public override long NextInt64(long maxValue)
        {
            return (long)BoundedNextUInt64((ulong)maxValue);
        }

        public override long NextInt64(long minValue, long maxValue)
        {
            if (minValue >= maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue), "minValue must be less than maxValue.");
            return (long)BoundedNextUInt64((ulong)(maxValue - minValue)) + minValue;
        }

        public override double NextDouble()
        {
            return Math.ScaleB(NextUInt64(), -64);
        }

        public uint NextUInt32()
        {
            ulong oldstate = state;
            state = oldstate * 6364136223846793005ul + inc;
            uint xorshifted = (uint)((oldstate >> 18 ^ oldstate) >> 27);
            int rot = (int)(oldstate >> 59);
            return xorshifted >> rot | xorshifted << (-rot & 31);
        }

        public ulong NextUInt64()
        {
            ulong oldstate2 = state2;
            // Advance internal state
            state2 = oldstate2 * 6364136223846793005ul + inc2;
            uint xorshifted = (uint)((oldstate2 >> 18 ^ oldstate2) >> 27);
            int rot = (int)(oldstate2 >> 59);
            return (ulong)NextUInt32() << 32 | xorshifted >> rot | xorshifted << (-rot & 31);
        }

        public uint BoundedNextUInt32(uint bound)
        {
            uint threshold = ((uint)-bound) % bound;
            while (true)
            {
                uint r = NextUInt32();
                if (r >= threshold)
                    return r % bound;
            }
        }

        public ulong BoundedNextUInt64(ulong bound)
        {
            ulong threshold = (~bound + 1) % bound;
            while (true)
            {
                ulong r = NextUInt64();
                if (r >= threshold)
                    return r % bound;
            }
        }
    }
}
