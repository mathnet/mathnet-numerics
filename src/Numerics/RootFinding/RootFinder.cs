using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.RootFinding
{
    public abstract class RootFinder
	{
	    protected const double DOUBLE_ACCURACY = 9.99200722162641E-16;
        private const int DEFAULT_MAX_ITERATIONS = 30;
        private const double DEFAULT_ACCURACY = 1e-8;

        //protected int _maxNumIters;
        //protected double _xmin = double.MinValue;
        //protected double _xmax = double.MaxValue;
        //protected double _accuracy;
        //protected Func<double, double> _func;
        //protected Func<double, double> m_Of;
        int _maxNumIters;
        double _xmin = double.MinValue;
        double _xmax = double.MaxValue;
        double _accuracy;
        Func<double, double> _func;
        private double bracketingFactor = 1.6;

		/// <summary>Constructor.</summary>
		/// <param name="f">A continuous function.</param>
        public RootFinder() : this(DEFAULT_MAX_ITERATIONS, DEFAULT_ACCURACY)
        {
        }

        public RootFinder(int numIters, double accuracy)
        {
            _maxNumIters = numIters;
            _accuracy = accuracy;
		}

	    #region Properties
        protected double XMin { get { return _xmin; } }
        protected double XMax { get { return _xmax; } }

        public Func<double, double> Func 
        {
            get { return _func; }
            set { _func = value; }
        }
        public double BracketingFactor
        {
            get { return bracketingFactor; }
            set
            {
                if (value <= 0.0) throw new ArgumentOutOfRangeException(); 
                bracketingFactor = value;
            }
        }
        public int Iterations
        {
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException();
                _maxNumIters = value;
            }
            protected get { return _maxNumIters; }
        }
        public double Accuracy
        {
            get { return _accuracy; }
            set { _accuracy = value; }
        }
	    #endregion Properties

		/// <summary>Detect a range containing at least one root.</summary>
		/// <param name="xmin">Lower value of the range.</param>
		/// <param name="xmax">Upper value of the range</param>
		/// <param name="factor">The growing factor of research. Usually 1.6.</param>
		/// <returns>True if the bracketing operation succeeded, else otherwise.</returns>
		/// <remarks>This iterative methods stops when two values with opposite signs are found.</remarks>
        public bool SearchBracketsOutward(ref double xmin, ref double xmax, double factor)
        {
            if (xmin >= xmax)
            {
                throw new RootFindingException(string.Format(Resources.ArgumentOutOfRangeGreater,"xmax","xmin"), 0, xmin, xmax, 0.0);
            }

            double fmin = _func(xmin);
            double fmax = _func(xmax);
            
            int i = 0;
            while (i++ < _maxNumIters) 
            {
                if (Math.Sign(fmin) != Math.Sign(fmax)) return true;
                if (Math.Abs(fmin) < Math.Abs(fmax))
                {
                    xmin += factor * (xmin - xmax);
                    fmin = _func(xmin);
                }
                else
                {
                    xmax += factor * (xmax - xmin);
                    fmax = _func(xmax);
                }
            } 

            throw new RootFindingException(Resources.RootNotFound, i, fmin, fmax, 0.0);
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

		/// <summary>Helper method useful for preventing rounding errors.</summary>
		/// <returns>a*sign(b)</returns>
        protected static double Sign(double a, double b)
        {
            return b >= 0 ? (a >= 0 ? a : -a) : (a >= 0 ? -a : a);
        }

        protected static bool Close(double d1, double d2)
        {
            return Math.Abs(d1 - d2) <= double.Epsilon;
        }
	}
}
