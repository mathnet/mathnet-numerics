/*using System;
using System.Collections.Generic;
using Pnl.RandomSources;

namespace Pnl.Distributions.Discrete
{
    public class Bernoulli : IDiscreteDistribution
    {
        public Bernoulli(double p)
        {
            throw new NotImplementedException();
        }


        public override string ToString()
        {
            throw new NotImplementedException();
        }


        private static void IsValidParameterSet(double p)
        {
            throw new NotImplementedException();
        }

        public void SetParameters(double p)
        {
            throw new NotImplementedException();
        }

        public double P
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #region IDistribution implementation
        public RandomSource RandomNumberGenerator { get; set; }

        public double Mean { get { throw new NotImplementedException(); } }
        public double Variance { get { throw new NotImplementedException(); } }
        public double StdDev { get { throw new NotImplementedException(); } }
        public double Entropy { get { throw new NotImplementedException(); } }
        public double Skewness { get { throw new NotImplementedException(); } }
        #endregion

        #region IContinuousDistribution implementation
        public int Mode { get { throw new NotImplementedException(); } }
        public int Median { get { throw new NotImplementedException(); } }
        public int Minimum { get { throw new NotImplementedException(); } }
        public int Maximum { get { throw new NotImplementedException(); } }
        public double Probability(int k) { throw new NotImplementedException(); }
        public double ProbabilityLn(int k) { throw new NotImplementedException(); }
        public double CumulativeDistribution(int k) { throw new NotImplementedException(); }

        public int Sample() { throw new NotImplementedException(); }
        public IEnumerable<int> Samples() { throw new NotImplementedException(); }
        #endregion

        public static int Sample(System.Random rng, double p) { throw new NotImplementedException(); }
        public static IEnumerable<int> Samples(System.Random rng, double p) { throw new NotImplementedException(); }
    }
}
*/