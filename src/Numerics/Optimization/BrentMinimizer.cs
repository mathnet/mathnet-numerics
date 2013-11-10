using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Options for Brent Minimization.
    /// </summary>
    public class BrentOptions
    {
        public int MaximumIterations = 1000;

        public double FunctionTolerance = 1e-4;
    }
    
    /// <summary>
    /// Minimizes f(p) where p is a model parameter scalar, i.e. a line-search.
    /// </summary>
    public class BrentMinimizer
    {
        const double verySmallNumber = 1e-21, goldenRatio = 1.618034, minimumTolerance = 1.0e-11;
        const double growLimit = 110.0, conjugateGradient = 0.3819660;
        const int maxIterations = 500;

        int bracketFunctionCalls;
        public int FunctionCalls, Iterations;
        double minPoint, minFunction;
        Func<double, double> function;
        double pointA, pointB, pointC;
        double functionA, functionB, functionC;

        public double Tolerance { get; set; }

        public BrentMinimizer(Func<double, double> function)
        {
            this.function = function;
            Tolerance = 1e-4;
        }

        public int Search(out double minPoint, out double minFunction)
        {
            bracketFunctionCalls = 0;
            UpdateBracketInterval(0, 1);
            FunctionCalls = 0;
            Iterations = 0;
            int result = BrentMinimize();
            FunctionCalls += bracketFunctionCalls;
            minPoint = this.minPoint;
            minFunction = this.minFunction;
            return result;
        }

        private int UpdateBracketInterval(double pointAStart, double pointBStart)
        {
            int iterations = 0;
            pointA = pointAStart;
            pointB = pointBStart;
            int maxIterations;
            maxIterations = 1000;
            functionA = function(pointA);
            functionB = function(pointB);
            double temp;
            if (functionA < functionB) // Swap points over 
            {
                temp = functionA; functionA = functionB; functionB = temp;
                temp = pointA; pointA = pointB; pointB = temp;
            }
            pointC = pointB + goldenRatio * (pointB - pointA);
            functionC = function(pointC);
            bracketFunctionCalls = 3; iterations = 0;
            double temp1, temp2, value, denom;
            while (functionC < functionB)
            {
                double pointW, functionW, wlim;
                temp1 = (pointB - pointA) * (functionB - functionC);
                temp2 = (pointB - pointC) * (functionB - functionA);
                value = temp2 - temp1;
                if (Math.Abs(value) < verySmallNumber) denom = 2.0 * verySmallNumber;
                else denom = 2.0 * value;
                pointW = pointB - ((pointB - pointC) * temp2 - (pointB - pointA) * temp1) / denom;
                wlim = pointB + growLimit * (pointC - pointB);
                if (iterations > maxIterations) return 1;
                iterations++;
                if ((pointW - pointC) * (pointB - pointW) > 0.0)
                {
                    functionW = function(pointW);
                    bracketFunctionCalls++;
                    if (functionW < functionC)
                    {
                        pointA = pointB; pointB = pointW;
                        functionA = functionB; functionB = functionW;
                        return 0;
                    }
                    else if (functionW > functionB)
                    {
                        pointC = pointW; functionC = functionW;
                        return 0;
                    }
                    pointW = pointC + goldenRatio * (pointC - pointB);
                    functionW = function(pointW);
                    bracketFunctionCalls++;
                }
                else if ((pointW - wlim) * (wlim - pointC) >= 0.0)
                {
                    pointW = wlim;
                    functionW = function(pointW);
                    bracketFunctionCalls++;
                }
                else if ((pointW - wlim) * (pointC - pointW) > 0.0)
                {
                    functionW = function(pointW);
                    bracketFunctionCalls++;
                    if (functionW < functionC)
                    {
                        pointB = pointC; pointC = pointW;
                        pointW = pointC + goldenRatio * (pointC - pointB);
                        functionB = functionC; functionC = functionW;
                        functionW = function(pointW);
                        bracketFunctionCalls++;
                    }
                }
                else
                {
                    pointW = pointC + goldenRatio * (pointC - pointB);
                    functionW = function(pointW);
                    bracketFunctionCalls++;
                }
                pointA = pointB; pointB = pointC; pointC = pointW;
                functionA = functionB; functionB = functionC; functionC = functionW;
            }
            return 0;
        }

        // Find the minimum of the function using the Brent method with the current 
        // bracketing interval. 
        private int BrentMinimize()
        {
            int result = 0;
            double x, w, v, fx, fw, fv;
            double a, b, deltax, rat;
            double cg = conjugateGradient;
            x = w = v = pointB; // x is the point with lowest function value encountered
            fw = fv = fx = function(x);
            if (pointA < pointC)
            {
                a = pointA; b = pointC;
            }
            else
            {
                a = pointC; b = pointA;
            }
            deltax = 0.0;
            FunctionCalls = 1;
            Iterations = 0;
            rat = 0;
            while (Iterations < maxIterations)
            {
                double tol1, tol2, xmin, fval, xmid;
                double temp1, temp2, p;
                double u, fu, dx_temp;
                tol1 = Tolerance * Math.Abs(x) + minimumTolerance;
                tol2 = 2.0 * tol1;
                xmid = 0.5 * (a + b);
                if (Math.Abs(x - xmid) < (tol2 - 0.5 * (b - a)))        // check for convergence 
                {
                    xmin = x; fval = fx;
                    result = 1;
                    break;
                }
                if (Math.Abs(deltax) <= tol1)
                {
                    if (x >= xmid) deltax = a - x;        // do a golden section step 
                    else deltax = b - x;
                    rat = cg * deltax;
                }
                else // do a parabolic step 
                {
                    temp1 = (x - w) * (fx - fv);
                    temp2 = (x - v) * (fx - fw);
                    p = (x - v) * temp2 - (x - w) * temp1;
                    temp2 = 2.0 * (temp2 - temp1);
                    if (temp2 > 0.0) p = -p;
                    temp2 = Math.Abs(temp2);
                    dx_temp = deltax;
                    deltax = rat;
                    // check parabolic fit 
                    if ((p > temp2 * (a - x)) && (p < temp2 * (b - x))
                                        && (Math.Abs(p) < Math.Abs(0.5 * temp2 * dx_temp)))
                    {
                        rat = p * 1.0 / temp2; // if parabolic step is useful. 
                        u = x + rat;
                        if (((u - a) < tol2) || ((b - u) < tol2))
                        {
                            if ((xmid - x) >= 0) rat = tol1;
                            else rat = -tol1;
                        }
                    }
                    else
                    {
                        if (x >= xmid) deltax = a - x; // if it's not do a golden section step 
                        else deltax = b - x;
                        rat = cg * deltax;
                    }
                }

                if (Math.Abs(rat) < tol1) // update by at least tol1 
                {
                    if (rat >= 0) u = x + tol1;
                    else u = x - tol1;
                }
                else
                {
                    u = x + rat;
                }
                fu = function(u);
                FunctionCalls++;
                if (fu > fx)        // if it's bigger than current 
                {
                    if (u < x) a = u;
                    else b = u;
                    if ((fu <= fw) || (w == x))
                    {
                        v = w; w = u; fv = fw; fw = fu;
                    }
                    else if ((fu <= fv) || (v == x) || (v == w))
                    {
                        v = u; fv = fu;
                    }
                }
                else
                {
                    if (u >= x) a = x;
                    else b = x;
                    v = w; w = x; x = u;
                    fv = fw; fw = fx; fx = fu;
                }
                Iterations++;
            }
            this.minPoint = x;
            this.minFunction = fx;
            return result;
        }
    }

    /// <summary>
    /// Minimizes f(p u) where p is a model parameter scalar and u is a direction vector.
    /// </summary>
    public class MultiDimensionalBrent
    {
        private Func<double[], double> function;
        private int functionCalls;
        double[] point;
        BrentMinimizer lineSearch;

        public MultiDimensionalBrent(Func<double[], double> function)
        {
            this.function = function;
            lineSearch = new BrentMinimizer(this.PointAlongLine);
            functionCalls = 0;
        }

        public double Tolerance
        {
            get { return lineSearch.Tolerance; }
            set { lineSearch.Tolerance = value; }
        }

        public int FunctionCalls
        {
            get { return lineSearch.FunctionCalls; }
        }

        public double[] StartingPoint { get; set; }
        public double[] Direction { get; set; }


        public void SetDimension(int N)
        {
            point = new double[N];
        }

        public int Search(out double[] minPoint, out double minFunction)
        {
            double path;
            int result = lineSearch.Search(out path, out minFunction);
            for (int i = 0; i < StartingPoint.Length; ++i)
            {
                point[i] = StartingPoint[i] + path * Direction[i];
            }
            minPoint = point;
            return result;
        }

        public double PointAlongLine(double path)
        {
            for (int i = 0; i < StartingPoint.Length; ++i)
            {
                point[i] = StartingPoint[i] + path * Direction[i];
            }
            return function(point);
        }
    }
}
