using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Minimizes f(p) where p is a vector of model parameters using the Powell method.
    /// </summary>
    public class PowellMinimizer
    {
        public double PointTolerance { get; set; }
        public double FunctionTolerance { get; set; }
        int? maxIterations, maxFunctionCalls;
        double[] minimumPoint;
        double functionAtMinimum;
        public int FunctionCalls { get; set; }
        public int Iterations { get; set; }
        public int? MaxIterations { get; set; }
        public int? MaxFunctionCalls { get; set; }
        public double[] MinimumPoint { get { return minimumPoint; } }
        public double FunctionValueAtMinimum { get { return functionAtMinimum; } }

        Func<double[], double> function;
        MultiDimensionalBrent powellLineSearch;

        public PowellMinimizer()
        {
            //this.function = function;
            //powellLineSearch = new MultiDimensionalBrent(function);
            PointTolerance = 1e-4;
            FunctionTolerance = 1e-4;
            MaxIterations = null;
            MaxFunctionCalls = null;
            FunctionCalls = 0;
            Iterations = 0;
            minimumPoint = null;
            functionAtMinimum = 0;
        }

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
            this.function = function;
            powellLineSearch = new MultiDimensionalBrent(function);
            this.Minimize(pStart);
            return minimumPoint;
        }

        public int Minimize(double[] p)
        {
            // Set line search valuer to use main valuer: 
            // (this valuer takes starting point, direciton and length and 
            // returns scalar): 
            // 
            int N = p.Length; // number of dimensions 
            powellLineSearch.SetDimension(N);
            double fval;
            FunctionCalls = 0;
            Iterations = 0;
            if (maxIterations == null) maxIterations = N * 1000;
            if (maxFunctionCalls == null) maxFunctionCalls = N * 1000;

            // An array of N directions:
            double[][] direc = new double[N][];
            for (int i = 0; i < N; ++i)
            {
                direc[i] = new double[N];
                direc[i][i] = 1.0;
            }
            double[] x = p;
            double[] x1 = (double[])x.Clone(); 
            powellLineSearch.Tolerance = PointTolerance * 100; // Set tolerance 

            fval = function(x);
            FunctionCalls++;
            double[] x2 = new double[N];
            double fx;
            double[] direc1 = new double[N];
            double[] xnew;
            while (true)
            {; 
                fx = fval;
                int bigind = 0;
                double delta = 0.0;
                double fx2;
                for (int i = 0; i < N; ++i)
                {
                    direc1 = direc[i];
                    fx2 = fval;
                    powellLineSearch.StartingPoint = x;
                    powellLineSearch.Direction = direc1;

                    powellLineSearch.Search(out xnew, out fval);
                    // Do a linesearch with specified starting point and direction. 
                    FunctionCalls += powellLineSearch.FunctionCalls;

                    for (int j = 0; j < N; ++j) x[j] = xnew[j];

                    if ((fx2 - fval) > delta)
                    {
                        delta = fx2 - fval;
                        bigind = i;
                    }
                }
                Iterations++;
                if (2.0 * (fx - fval) <= FunctionTolerance * ((Math.Abs(fx) + Math.Abs(fval)) + 1e-20)) break;
                if (FunctionCalls >= maxFunctionCalls) break;
                if (Iterations >= maxIterations) break;

                // Construct the extrapolated point  
                direc1 = new double[N];
                for (int i = 0; i < N; ++i)
                {
                    direc1[i] = x[i] - x1[i];
                    x2[i] = 2.0 * x[i] - x1[i];
                    x1[i] = x[i];
                }

                fx2 = function(x2);
                FunctionCalls++;
                if (fx > fx2)
                {
                    double t = 2.0 * (fx + fx2 - 2.0 * fval);
                    double temp = (fx - fval - delta);
                    t *= temp * temp;
                    temp = fx - fx2;
                    t -= delta * temp * temp;
                    if (t < 0.0)
                    {
                        powellLineSearch.StartingPoint = x;
                        powellLineSearch.Direction = direc1;
                        powellLineSearch.Search(out xnew, out fval);

                        FunctionCalls += powellLineSearch.FunctionCalls;
                        direc1 = new double[N];
                        for (int i = 0; i < N; ++i)
                        {
                            direc1[i] = xnew[i] - x[i];
                            x[i] = xnew[i];
                        }

                        direc[bigind] = direc[N - 1];
                        direc[N - 1] = direc1;
                    }
                }
            }
            minimumPoint = (double[])x.Clone();
            functionAtMinimum = fx;
            
            // Find out what happened: 
            if (FunctionCalls >= maxFunctionCalls)
            {
                return 1; // Max function calls exceeded 
            }
            if (Iterations >= maxIterations)
            {
                return 2; // Max iterations exceeded 
            }
            return 0; // all good 
        }
    }
}
