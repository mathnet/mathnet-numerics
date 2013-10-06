using System;

namespace MathNet.Numerics.Random
{
    public static class RandomSeed
    {
        /// <summary>
        /// Provides a time-dependent seed value, matching the default behavior of System.Random.
        /// WARNING: There is no randomness in this seed and quick repeated calls can cause
        /// the same seed value. Do not use for cryptography!
        /// </summary>
        public static int Time()
        {
            return Environment.TickCount;
        }

        /// <summary>
        /// Provides a seed based on time and unique GUIDs.
        /// WARNING: There is only low randomness in this seed, but at least quick repeated
        /// calls will result in different seed values. Do not use for cryptography!
        /// </summary>
        public static int Guid()
        {
            return Environment.TickCount ^ System.Guid.NewGuid().GetHashCode();
        }
    }
}
