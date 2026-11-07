using System;

namespace WXProvincialFreeCompany
{
    /// <summary>
    /// Pure, deterministic recruitment simulation used by the wx_pfc.sample
    /// diagnostic command. Uses its own fixed-seed System.Random instance, so
    /// it never consumes or influences the campaign RNG.
    /// </summary>
    public static class RegionalRecruitmentSimulation
    {
        public sealed class Result
        {
            public Result(double chance, int count, int seed, int hits)
            {
                Chance = chance;
                Count = count;
                Seed = seed;
                Hits = hits;
            }

            public double Chance { get; }

            public int Count { get; }

            public int Seed { get; }

            public int Hits { get; }

            public double ObservedRatio => (double)Hits / Count;

            /// <summary>Observed minus configured chance, in percentage points.</summary>
            public double DeviationPoints => (ObservedRatio - Chance) * 100.0;
        }

        public static Result Simulate(double chance, int count, int seed)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Sample count must be at least 1.");
            }

            if (chance < 0.0 || chance > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(chance), "Chance must be within [0, 1].");
            }

            // Deliberately not MBRandom: the frozen design requires an
            // independent, reproducible random source for diagnostics.
            var random = new Random(seed);
            int hits = 0;
            for (int i = 0; i < count; i++)
            {
                if (random.NextDouble() < chance)
                {
                    hits++;
                }
            }

            return new Result(chance, count, seed, hits);
        }
    }
}
