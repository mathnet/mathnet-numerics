using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Options for Powell Minimization.
    /// </summary>
    public class PowellOptions
    {
        public int? MaximumIterations = null;
        public int? MaximumFunctionCalls = null;
        public double PointTolerance = 1e-4; // absolute
        public double FunctionTolerance = 1e-4; // relative
    }

    public enum PowellConvergenceType { Success, MaxIterationsExceeded, MaxFunctionCallsExceeded };

    /// <summary>
    /// Result of Powell Minimization.
    /// </summary>
    public class PowellResult
    {
        public int NumberOfIterations;
        public int NumberOfFunctionCalls;
        public double[] MinimumPoint;
        public double MinimumFunctionValue;
        public PowellConvergenceType ConvergenceType; 
    }
    
    /// <summary>
    /// Minimizes f(p) where p is a vector of model parameters using the Powell method.
    /// </summary>
    public class PowellMinimizer
    {
        public PowellResult Result { get; private set; }

        public readonly PowellOptions Options = new PowellOptions();

        /// <summary>
        /// Non-Linear Least-Squares fitting the points (x,y) to a specified function of y : x -> f(x, p), p being a vector of parameters.
        /// returning its best fitting parameters p.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="f"></param>
        /// <param name="pStart">Initial guess of parameters, p.</param>
        /// <returns></returns>
        public double[] CurveFit(double[] x, double[] y, Func<double, double[], double> f,
            double[] pStart)
        {
            // Need to minimize sum of squares of residuals; create this function:
            Func<double[], double> function = (p) =>
            {
                double sum = 0;
                for (int i = 0; i < x.Length; ++i)
                {
                    double temp = y[i] - f(x[i], p);
                    sum += temp * temp;
                }
                return sum;
            };
            return Minimize(function, pStart);
        }

        /// <summary>
        /// Find the minimum of the supplied function using the Powell method.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="pInitialGuess"></param>
        /// <returns></returns>
        public double[] Minimize(Func<double[], double> function, double[] pInitialGuess)
        {
            BrentMinimizer brentMinimizer = new BrentMinimizer();
            int n = pInitialGuess.Length; // number of dimensions 
            // used in closure:
            double[] point = new double[n];
            double[] startingPoint = new double[n];
            double[] direction = new double[n];
            double lineMiniumum = 0;
            int functionCalls = 0;
            Func<double, double> functionAlongLine = (p) =>
            {
                for (int i = 0; i < point.Length; ++i)
                    point[i] = startingPoint[i] + direction[i] * p;
                lineMiniumum = function(point);
                functionCalls++;
                return lineMiniumum;
            };

            double fval;

            int iterations = 0;
            int maxIterations = (Options.MaximumIterations == null) ? n * 1000 : (int)Options.MaximumIterations;
            int maxFunctionCalls = (Options.MaximumFunctionCalls == null) ? n * 1000 : (int)Options.MaximumFunctionCalls;

            // An array of n directions:
            double[][] directionSet = new double[n][];
            for (int i = 0; i < n; ++i)
            {
                directionSet[i] = new double[n];
                directionSet[i][i] = 1.0;
            }
            double[] x = pInitialGuess;
            double[] x1 = (double[])x.Clone();
            double[] x2 = new double[n];
            double[] direction1 = new double[n];

            brentMinimizer.Options.Tolerance = Options.PointTolerance * 100;

            fval = function(x);

            double fx;

            while (true)
            {
                fx = fval;
                int bigIndex = 0;
                double delta = 0.0;
                double fx2;
                // loop over all directions:
                for (int i = 0; i < n; ++i)
                {
                    fx2 = fval;

                    // Do a linesearch along direction
                    direction = directionSet[i];
                    startingPoint = x;
                    double u = brentMinimizer.Minimize(functionAlongLine);
                    fval = functionAlongLine(u); // updates point

                    for (int j = 0; j < n; ++j) x[j] = point[j];

                    if ((fx2 - fval) > delta)
                    {
                        delta = fx2 - fval;
                        bigIndex = i; // record direction index of the largest decrease seen
                    }
                }
                iterations++;
                if (2.0 * (fx - fval) <= Options.FunctionTolerance * ((Math.Abs(fx) + Math.Abs(fval)) + 1e-20)) break;
                if (functionCalls >= maxFunctionCalls) break;
                if (iterations >= maxIterations) break;

                // Construct the extrapolated point 
                for (int i = 0; i < n; ++i)
                {
                    direction1[i] = x[i] - x1[i];
                    x2[i] = 2.0 * x[i] - x1[i];
                    x1[i] = x[i];
                }

                fx2 = function(x2);
                if (fx > fx2)
                {
                    double t = 2.0 * (fx + fx2 - 2.0 * fval);
                    double temp = (fx - fval - delta);
                    t *= temp * temp;
                    temp = fx - fx2;
                    t -= delta * temp * temp;
                    if (t < 0.0)
                    {
                        // Do a linesearch along direction
                        direction = direction1;
                        startingPoint = x;
                        double u = brentMinimizer.Minimize(functionAlongLine);
                        fval = functionAlongLine(u); // updates point

                        for (int i = 0; i < n; ++i)
                        {
                            directionSet[bigIndex][i] = directionSet[n - 1][i];
                            directionSet[n - 1][i] = point[i] - x[i];
                            x[i] = point[i];
                        }
                    }
                }
            }
            var convergenceType = PowellConvergenceType.Success;
            if (functionCalls >= maxFunctionCalls) 
                convergenceType = PowellConvergenceType.MaxFunctionCallsExceeded;
            else if (iterations > maxIterations) 
                convergenceType = PowellConvergenceType.MaxFunctionCallsExceeded;

            Result = new PowellResult()
            {
                MinimumPoint = (double[])x.Clone(),
                MinimumFunctionValue = fx,
                ConvergenceType = convergenceType,
                NumberOfIterations = iterations,
                NumberOfFunctionCalls = functionCalls
            };
            
            return Result.MinimumPoint;
        }
    }
}
