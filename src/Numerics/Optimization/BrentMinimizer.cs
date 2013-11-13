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
        public double Tolerance = 1e-4;
    }

    /// <summary>
    /// Result of Brent Minimization.
    /// </summary>
    public class BrentResult
    {
        public int NumberOfIterations;
        public double MinimumPoint;
        public double MinimumFunctionValue;
    }
    
    /// <summary>
    /// Minimizes f(p) where p is a model parameter scalar, i.e. a line-search.
    /// Inspired by the SciPy implementation.
    /// </summary>
    public class BrentMinimizer
    {
        public struct Bracket
        {
            public double PointA;
            public double PointB;
            public double PointC;
            public double FunctionA;
            public double FunctionB;
            public double FunctionC;
        }
        
        public BrentResult Result { get; private set; }

        public readonly BrentOptions Options = new BrentOptions();
        
        const double verySmallNumber = 1e-21, goldenRatio = 1.618034, minimumTolerance = 1.0e-11;
        const double growLimit = 110.0, conjugateGradient = 0.3819660;

        /// <summary>
        /// Find the minimum of the supplied function using the Brent method.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public double Minimize(Func<double, double> function)
        {
            Bracket bracket;
            UpdateBracketInterval(function, new Bracket() { PointA = 0, PointB = 1 }, out bracket);
            int iterations = 0;
            double x, w, v, fx, fw, fv;
            double a, b, deltax, rat;
            double cg = conjugateGradient;
            x = w = v = bracket.PointB; // x is the point with lowest function value encountered
            fw = fv = fx = function(x);
            if (bracket.PointA < bracket.PointC)
            {
                a = bracket.PointA; b = bracket.PointC;
            }
            else
            {
                a = bracket.PointC; b = bracket.PointA;
            }
            deltax = 0.0;
            rat = 0;
            while (iterations < Options.MaximumIterations)
            {
                double tol1, tol2, xmid;
                double temp1, temp2, p;
                double u, fu, dx_temp;
                tol1 = Options.Tolerance * Math.Abs(x) + minimumTolerance;
                tol2 = 2.0 * tol1;
                xmid = 0.5 * (a + b);
                if (Math.Abs(x - xmid) < (tol2 - 0.5 * (b - a))) // check for convergence 
                    break;
                if (Math.Abs(deltax) <= tol1)
                {
                    if (x >= xmid) deltax = a - x; // do a golden section step 
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
                iterations++;
            }
            this.Result = new BrentResult() { NumberOfIterations = iterations, MinimumPoint = x, MinimumFunctionValue = fx };
            return x;            
        }

        /// <summary>
        /// Find the minimum of the supplied function along a specified line, using the Brent method.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="direction">Direction of line.</param>
        /// <param name="startingPoint">Starting point of line.</param>
        /// <returns></returns>
        public double Minimize(Func<double[], double> function, double[] direction, double[] startingPoint, out double[] minimumPoint)
        {
            double[] point = new double[direction.Length];
            Func<double, double> functionAlongLine = (p) =>
                {
                    for (int i = 0; i < point.Length; ++i)
                        point[i] = startingPoint[i] + direction[i] * p;
                    return function(point);
                };
            double result = Minimize(functionAlongLine);
            minimumPoint = point;
            return result;
        }

        /// <summary>
        /// Updates the bracket.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="bracketInitial"></param>
        /// <param name="newBracket"></param>
        /// <returns></returns>
        private static bool UpdateBracketInterval(Func<double, double> function, Bracket bracketInitial, out Bracket newBracket)
        {
            int iterations = 0;
            double pointA = bracketInitial.PointA;
            double pointB = bracketInitial.PointB;
            int maxIterations;
            maxIterations = 1000;
            double functionA = function(pointA);
            double functionB = function(pointB);
            double temp;
            if (functionA < functionB) // Swap points over 
            {
                temp = functionA; functionA = functionB; functionB = temp;
                temp = pointA; pointA = pointB; pointB = temp;
            }
            double pointC = pointB + goldenRatio * (pointB - pointA);
            double functionC = function(pointC);
            iterations = 0;
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
                if (iterations > maxIterations)
                {
                    newBracket = bracketInitial;
                    return false;
                }
                iterations++;
                if ((pointW - pointC) * (pointB - pointW) > 0.0)
                {
                    functionW = function(pointW);
                    if (functionW < functionC)
                    {
                        pointA = pointB; pointB = pointW;
                        functionA = functionB; functionB = functionW;
                        break;
                    }
                    else if (functionW > functionB)
                    {
                        pointC = pointW; functionC = functionW;
                        break;
                    }
                    pointW = pointC + goldenRatio * (pointC - pointB);
                    functionW = function(pointW);
                }
                else if ((pointW - wlim) * (wlim - pointC) >= 0.0)
                {
                    pointW = wlim;
                    functionW = function(pointW);
                }
                else if ((pointW - wlim) * (pointC - pointW) > 0.0)
                {
                    functionW = function(pointW);
                    if (functionW < functionC)
                    {
                        pointB = pointC; pointC = pointW;
                        pointW = pointC + goldenRatio * (pointC - pointB);
                        functionB = functionC; functionC = functionW;
                        functionW = function(pointW);
                    }
                }
                else
                {
                    pointW = pointC + goldenRatio * (pointC - pointB);
                    functionW = function(pointW);
                }
                pointA = pointB; pointB = pointC; pointC = pointW;
                functionA = functionB; functionB = functionC; functionC = functionW;
            }
            newBracket = new Bracket()
            {
                PointA = pointA,
                PointB = pointB,
                PointC = pointC,
                FunctionA = functionA,
                FunctionB = functionB,
                FunctionC = functionC
            };
            return true;
        }

    }
}
