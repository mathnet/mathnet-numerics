using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Empirical Distribution with uniform distributed steps
    /// </summary>
    public class Empirical : IContinuousDistribution
    {
        public int NrOfSteps => _stepProbability.Count();
        private List<double> _stepProbability = new List<double>();
        private double _min;
        private double _stepsize;
        private System.Random _randomSource;

        /// <summary>
        /// Creates an Empty Empirical Distribution with min = 0 and no steps
        /// </summary>
        public Empirical()
        {
            RandomSource = new System.Random();
        }

        /// <summary>
        /// Creates an empirical distribution with the probability for each step set in an parameter array.
        /// </summary>
        /// <param name="min">Start for the emperical distribution</param>
        /// <param name="stepsize">Size of each uniform distribution step</param>
        /// <param name="stepProbability">An array that defines how many and the probability for each step</param>
        public Empirical(double min, double stepsize, double[] stepProbability) : this()
        {
            _min = min;
            _stepsize = stepsize;
            foreach (var d in stepProbability)
            {
                _stepProbability.Add(d);
            }
        }
        /// <summary>
        /// Creates an Empirical distribution with tha number of steps as a parameter
        /// </summary>
        /// <param name="min">Start for the emperical distribution</param>
        /// <param name="stepsize">Size of each uniform distribution step</param>
        /// <param name="steps">Nr of steps for the empirical distribution</param>
        public Empirical(double min, double stepsize, int steps) : this(min, stepsize, new double[steps])
        {

        }

        /// <summary>
        /// The random number generator
        /// </summary>
        public System.Random RandomSource
        {
            get { return _randomSource; }
            set
            {
                if (value == null)
                    _randomSource = new System.Random();
                else
                    _randomSource = value;
            }
        }

        /// <summary>
        /// The CDF function
        /// </summary>
        /// <param name="x">Paramter for the CDF function</param>
        /// <returns>The CDF function return</returns>
        public double CumulativeDistribution(double x)
        {
            var sum = _stepProbability.Sum();
            if (x < _min)
                return 0;
            if (x > _min + (_stepsize * NrOfSteps))
            {
                return 1;
            }
            var instep = 0;
            var totprob = 0d;
            while (_min + (instep + 1) * _stepsize < x)

            {
                totprob += _stepProbability[instep] / sum;
                instep++;
            }
            totprob += (x - instep * _stepsize - _min) / (_stepsize) * (_stepProbability[instep] / sum);

            return totprob;
        }

        public double Mean => _min + _stepsize * (NrOfSteps / 2d);
        public double Variance => Samples(1000).Variance();
        public double StdDev => Samples(1000).StandardDeviation();
        public double Entropy => Mean;
        public double Skewness => Samples(1000).Skewness();
        public double Median => Mean;
        public double Mode => _stepProbability.FindIndex(t => t == _stepProbability.Max(i => i)) * _stepsize + _min;
        public double Minimum => _min;
        public double Maximum => _min + NrOfSteps * _stepsize + _stepsize;

        /// <summary>
        /// Teh density function for the empirical distribution
        /// </summary>
        /// <param name="x">Parameter for the density function</param>
        /// <returns>Parameter for the density function</returns>
        public double Density(double x)
        {
            if (x < _min)
                return 0;
            if (x > _min + (_stepsize * NrOfSteps))
            {
                return 0;
            }
            var instep = 0;
            while (_min + instep * _stepsize < x)
                instep++;

            return _stepProbability[instep] / _stepProbability.Sum();
        }

        public double DensityLn(double x)
        {
            return Math.Log(Density(x), Math.E);

        }

        /// <summary>
        /// Generates v nr of samples
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private IEnumerable<double> Samples(int v)
        {
            for (int i = 0; i < v; i++)
                yield return Sample();
        }

        /// <summary>
        /// Creates one sample
        /// </summary>
        /// <returns></returns>
        public double Sample()
        {
            var d = RandomSource.NextDouble();
            var instep = 0;
            var sofar = 0d;
            var sum = _stepProbability.Sum();
            while (_stepProbability.Count > instep + 1 && sofar + _stepProbability[instep] / sum < d)
            {
                sofar += _stepProbability[instep] / sum;
                instep++;
            }
            var rest = d - sofar;
            return _min + (instep * _stepsize) + (rest / (_stepProbability[instep] / sum)) * _stepsize;
        }

        /// <summary>
        /// Fill a array of samples
        /// </summary>
        /// <param name="values"></param>
        public void Samples(double[] values)
        {
            for (int index = 0; index < values.Length; index++)
            {
                values[index] = Sample();
            }
        }

        /// <summary>
        /// Returns an infinit array of samples
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> Samples()
        {
            while (true)
                yield return Sample();
        }


    }
}
