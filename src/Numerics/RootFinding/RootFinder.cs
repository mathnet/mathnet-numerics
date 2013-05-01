using System;

namespace MathNet.Numerics.RootFinding
{
    public abstract class RootFinder
	{
	    protected const double DoubleAccuracy = 9.99200722162641E-16;
        private const int DefaultMaxIterations = 30;
        private const double DefaultAccuracy = 1e-8;

        int _maxNumIters;
        double _xmin = double.MinValue;
        double _xmax = double.MaxValue;
        Func<double, double> _func;

        public RootFinder() : this(DefaultMaxIterations, DefaultAccuracy)
        {
        }

        public RootFinder(int numIters, double accuracy)
        {
            _maxNumIters = numIters;
            Accuracy = accuracy;
		}

        protected double XMin { get { return _xmin; } }
        protected double XMax { get { return _xmax; } }

        public Func<double, double> Func 
        {
            get { return _func; }
            set { _func = value; }
        }

        public double Accuracy { get; set; }

        public int Iterations
        {
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException();
                _maxNumIters = value;
            }
            protected get { return _maxNumIters; }
        }

		/// <summary>Prototype algorithm for solving the equation f(x)=0.</summary>
		/// <param name="x1">The low value of the range where the root is supposed to be.</param>
		/// <param name="x2">The high value of the range where the root is supposed to be.</param>
		/// <returns>Returns the root with the specified accuracy.</returns>
        public virtual double Solve(double x1, double x2)
        {
            _xmin = x1;
            _xmax = x2;
            return Find();
        }

		protected abstract double Find();
	}
}
