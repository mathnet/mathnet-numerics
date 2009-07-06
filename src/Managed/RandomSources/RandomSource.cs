using System;

namespace Pnl.RandomSources
{
    // Do we want to inherit from System.Random so people can plugin any RNG not from our library?
    public abstract class RandomSource /* : System.Random */
    {
        protected RandomSource(bool threadSafe) { }
        public abstract int Next();
        public abstract int Next(int maxValue);
        public abstract int Next(int minValue, int maxValue);
        public abstract double NextDouble();
        public abstract double NextDouble(double maxValue);
        public abstract double NextDouble(double minValue, double maxValue);
        public abstract bool NextBoolean();
        public abstract void NextBytes(byte[] buffer);

        // Do we want Reset() or just a SetSeed kind of method?
        public abstract void Reset();
        public abstract bool CanReset
        {
            get;
        }

        public virtual long NextInt64()
        {
            throw new NotImplementedException();
        }

        public virtual decimal NextDecimal()
        {
            throw new NotImplementedException();
        }
    }
}
