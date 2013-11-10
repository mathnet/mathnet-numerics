using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.Providers.Optimization;
using MathNet.Numerics.Providers.Optimization.Mkl;

namespace MathNet.Numerics.Optimization
{   
    /// <summary>
    /// This class is a special function minimizer that minimizes functions of the form
    /// f(p) = |r(p)|^2 where r is a vector of residuals and p is a vector of model parameters.  
    /// </summary>
    public class NonLinearLeastSquaresMinimizer
    {
        public class Options
        {
            public int MaximumIterations = 1000;

            public int MaximumTrialStepIterations = 100;

            public ConvergenceType ConvergenceType;

            /// <summary>
            ///  Convergence if Δ &lt; Criterion0, Δ is trust region size. 
            /// </summary>
            public double Criterion0 = 1e-7;

            /// <summary>
            /// Convergence if |r|2 &lt; Criterion1, r is residuals vector.
            /// </summary>
            public double Criterion1 = 1e-7;

            /// <summary>
            /// Jacobian considered singular if |J(:,j)|2 &lt; Criterion2 for any j.
            /// </summary>
            public double Criterion2 = 1e-7;

            /// <summary>
            /// Jacobian considered singular if |s|2 &lt; Criterion3. s is trial step.
            /// </summary>
            public double Criterion3 = 1e-7;

            /// <summary>
            /// |r|2 - |r - Js|2 &lt; Criterion4
            /// </summary>
            public double Criterion4 = 1e-7;

            public double TrialStepPrecision = 1e-10;

            /// <summary>
            /// Only if Jacobian calculated by central differences.
            /// </summary>
            public double JacobianPrecision = 1e-8;
        }
        
        /// <summary>
        /// For details of convergence criteria, see Options.
        /// </summary>
        public enum ConvergenceType { NoneMaxIterationExceeded, Criterion0, Criterion1, Criterion2, Criterion3, Criterion4, Error };
        
        public class Result
        {
            public int NumberOfIterations;

            public ConvergenceType ConvergenceType;
        }
        
        /// <summary>
        /// Non-Linear Least-Squares fitting the points (x,y) to a specified function of y : x -> f(x, p), p being a vector of parameters.
        /// returning its best fitting parameters p.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="f"></param>
        /// <param name="pStart">Initial guess of parameters, p.</param>
        /// <param name="jacobian">jac_j(x, p) = df / dp_j</param>
        /// <returns></returns>
        public static double[] CurveFit(double[] x, double[] y, Func<double, double[], double> f,
            double[] pStart, Func<double, double[], double[]> jacobian = null)
        {
            if (x.Length != y.Length) throw new ArgumentException("x and y lengths different");
            var provider = new MklOptimizationProvider();
            LeastSquaresForwardModel function = (p, r) =>
            {
                for (int i = 0; i < r.Length; ++i)
                    r[i] = y[i] - f(x[i], p);
            };

            // jac is df_i / dp_j

            Jacobian jacobianFunction = null;
            if (jacobian != null) jacobianFunction = (p, jac) =>
            {
                for (int i = 0; i < y.Length; ++i)
                {
                    double[] values = jacobian(x[i], p);
                    for (int j = 0; j < values.Length; ++j)
                        jac[j * y.Length + i] = -values[j];
                }
            };

            double[] parameters;
            Result result = provider.NonLinearLeastSquaresUnboundedMinimize(y.Length, pStart, function, out parameters, jacobianFunction);
            return parameters;
        }
    }
}
