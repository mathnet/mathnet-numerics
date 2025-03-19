// <copyright file="BrentMinimizer.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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

using MathNet.Numerics.Optimization.ObjectiveFunctions;
using System;

namespace MathNet.Numerics.Optimization
{
    public class BrentMinimizer
    {
        public double XTolerance { get; set; }
        public int MaximumIterations { get; set; }
        public int MaximumExpansionSteps { get; set; }
        public double LowerExpansionFactor { get; set; }
        public double UpperExpansionFactor { get; set; }

        public BrentMinimizer(double xTolerance = 1e-5, int maxIterations = 1000, int maxExpansionSteps = 10, double lowerExpansionFactor = 2.0, double upperExpansionFactor = 2.0)
        {
            XTolerance = xTolerance;
            MaximumIterations = maxIterations;
            MaximumExpansionSteps = maxExpansionSteps;
            LowerExpansionFactor = lowerExpansionFactor;
            UpperExpansionFactor = upperExpansionFactor;
        }

        public ScalarMinimizationResult FindMinimum(IScalarObjectiveFunction objective, double lowerBound, double upperBound)
        {
            return Minimum(objective, lowerBound, upperBound, XTolerance, MaximumIterations, MaximumExpansionSteps, LowerExpansionFactor, UpperExpansionFactor);
        }

        public static ScalarMinimizationResult Minimum(IScalarObjectiveFunction objective, double lowerBound, double upperBound, double xTolerance = 1e-5,
            int maxIterations = 1000, int maxExpansionSteps = 10, double lowerExpansionFactor = 2.0, double upperExpansionFactor = 2.0)
        {
            int maxfun = maxIterations;

            if (lowerBound > upperBound)
                throw new OptimizationException("Lower bound must be lower than upper bound.");

            double sqrt_eps = Math.Sqrt(2.2e-16);

            // This is not the golden_mean, but golden angle. Not sure why.
            //  https://en.wikipedia.org/wiki/Golden_angle
            double golden_angle = 0.5 * (3.0 - Math.Sqrt(5.0));

            double a = lowerBound;
            double b = upperBound;
            double fulc = a + golden_angle * (b - a);

            double nfc = fulc, xf = fulc;
            double rat = 0.0, e = 0.0;
            double x = xf;
            var evaluation = objective.Evaluate(x);
            double fx = evaluation.Value;
            int num = 1;

            double fu = double.PositiveInfinity;
            double ffulc = fx, fnfc = fx;
            double xm = 0.5 * (a + b);
            double tol1 = sqrt_eps * Math.Abs(xf) + xTolerance / 3.0;
            double tol2 = 2.0 * tol1;

            while (Math.Abs(xf - xm) > (tol2 - 0.5 * (b - a)))
            {
                bool golden = true;

                // Check for parabolic fit
                if (Math.Abs(e) > tol1)
                {
                    golden = false;
                    double r = (xf - nfc) * (fx - ffulc);
                    double q = (xf - fulc) * (fx - fnfc);
                    double p = (xf - fulc) * q - (xf - nfc) * r;
                    q = 2.0 * (q - r);
                    if (q > 0.0)
                        p = -p;
                    q = Math.Abs(q);
                    r = e;
                    e = rat;

                    // Check for acceptability of parabola
                    if ((Math.Abs(p) < Math.Abs(0.5 * q * r)) && (p > q * (a - xf)) && (p < q * (b - xf)))
                    {
                        rat = (p + 0.0) / q;
                        x = xf + rat;

                        if (((x - a) < tol2) || ((b - x) < tol2))
                        {
                            int si_2 = Math.Sign(xm - xf) + ((xm - xf) == 0 ? 1 : 0);
                            rat = tol1 * si_2;
                        }
                    }
                    else     // do a golden-section step
                        golden = true;
                }

                if (golden)  // do a golden-section step
                {
                    if (xf >= xm)
                        e = a - xf;
                    else
                        e = b - xf;
                    rat = golden_angle * e;
                }

                int si = Math.Sign(rat) + (rat == 0 ? 1 : 0);
                x = xf + si * Math.Max(Math.Abs(rat), tol1);

                evaluation = objective.Evaluate(x);
                fu = evaluation.Value;
                num += 1;

                if (fu <= fx)
                {
                    if (x >= xf)
                        a = xf;
                    else
                        b = xf;

                    fulc = nfc; ffulc = fnfc;
                    nfc = xf; fnfc = fx;
                    xf = x; fx = fu;
                }
                else
                {
                    if (x < xf)
                        a = x;
                    else
                        b = x;

                    if ((fu <= fnfc) || (nfc == xf))
                    {
                        fulc = nfc; ffulc = fnfc;
                        nfc = x; fnfc = fu;
                    }
                    else if ((fu <= ffulc) || (fulc == xf) || (fulc == nfc))
                    {
                        fulc = x; ffulc = fu;
                    }
                }

                xm = 0.5 * (a + b);
                tol1 = sqrt_eps * Math.Abs(xf) + xTolerance / 3.0;
                tol2 = 2.0 * tol1;

                if (num >= maxfun)
                    break;
            }

            var exitCondition = ExitCondition.BoundTolerance;

            if (num >= maxfun)
                exitCondition = ExitCondition.ExceedIterations;
            else if (double.IsNaN(xf) || double.IsNaN(fx) || double.IsNaN(fu))
                exitCondition = ExitCondition.InvalidValues;

            return new ScalarMinimizationResult(new ScalarValueObjectiveFunctionEvaluation(xf, fx), num, exitCondition);
        }

        static void ValueChecker(double value, double point)
        {
            if (Double.IsNaN(value) || Double.IsInfinity(value))
            {
                throw new Exception("Objective function returned non-finite value.");
            }
        }
    }
}
