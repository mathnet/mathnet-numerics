// <copyright file="NonLinearLeastSquaresMinimizer.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using MathNet.Numerics.Providers.Optimization;

#if NATIVEMKL
using MathNet.Numerics.Providers.Optimization.Mkl;
#endif

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Options for Non-Linear Least Squares Minimization.
    /// </summary>
    public class NonLinearLeastSquaresOptions
    {
        public int MaximumIterations = 1000;
        public int MaximumTrialStepIterations = 100;
        public NonLinearLeastSquaresConvergenceType ConvergenceType;

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
    public enum NonLinearLeastSquaresConvergenceType
    {
        MaxIterationsExceeded,
        Criterion0,
        Criterion1,
        Criterion2,
        Criterion3,
        Criterion4,
        Error
    };

    /// <summary>
    /// Result of Non-Linear Least Squares Minimization.
    /// </summary>
    public class NonLinearLeastSquaresResult
    {
        public int NumberOfIterations;
        public NonLinearLeastSquaresConvergenceType ConvergenceType;
    }

#if NATIVEMKL

    /// <summary>
    /// This class is a special function minimizer that minimizes functions of the form
    /// f(p) = |r(p)|^2 where r is a vector of residuals and p is a vector of model parameters.
    /// </summary>
    public class NonLinearLeastSquaresMinimizer // Note does not implement IMinimizer, since it curve fitting problems can be solved more efficiently.
    {
        public NonLinearLeastSquaresResult Result { get; private set; }

        public readonly NonLinearLeastSquaresOptions Options = new NonLinearLeastSquaresOptions();

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
        public double[] CurveFit(double[] x, double[] y, Func<double, double[], double> f,
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
            if (jacobian != null)
                jacobianFunction = (p, jac) =>
                {
                    for (int i = 0; i < y.Length; ++i)
                    {
                        double[] values = jacobian(x[i], p);
                        for (int j = 0; j < values.Length; ++j)
                            jac[j*y.Length + i] = -values[j];
                    }
                };

            double[] parameters;
            Result = provider.NonLinearLeastSquaresUnboundedMinimize(y.Length, pStart, function, out parameters, jacobianFunction);
            return parameters;
        }
    }

#endif

}
