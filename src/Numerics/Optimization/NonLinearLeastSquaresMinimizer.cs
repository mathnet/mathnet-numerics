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
        /// <summary>
        /// Criterion0: Δ &lt; eps(0) (trust region solvers only)
        /// Criterion1: ||F(x)||2 &lt; eps(1)
        /// Criterion2: The Jacobian matrix is singular.||J(x)(1:m,j)||2 &lt; eps(2), j = 1, ..., n
        /// Criterion3: ||s||2 &lt; eps(3)
        /// Criterion4: ||F(x)||2 - ||F(x) - J(x)s||2 &lt; eps(4)
        /// </summary>
        public enum ConvergenceType { NoneMaxIterationExceeded, Criterion0, Criterion1, Criterion2, Criterion3, Criterion4, SingularJacobian, Error };
        
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
        /// <param name="pStart"></param>
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
