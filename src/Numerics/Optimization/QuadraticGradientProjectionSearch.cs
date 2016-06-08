using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public static class QuadraticGradientProjectionSearch
    {
        public static GradientProjectionResult Search(Vector<double> x0, Vector<double> gradient, Matrix<double> hessian, Vector<double> lowerBound, Vector<double> upperBound)
        {
            List<bool> isFixed = new List<bool>(x0.Count);
            List<double> breakpoint = new List<double>(x0.Count);
            for (int ii = 0; ii < x0.Count; ++ii)
            {
                breakpoint.Add(0.0);
                isFixed.Add(false);
                if (gradient[ii] < 0)
                    breakpoint[ii] = (x0[ii] - upperBound[ii]) / gradient[ii];
                else if (gradient[ii] > 0)
                    breakpoint[ii] = (x0[ii] - lowerBound[ii]) / gradient[ii];
                else
                {
                    if (Math.Abs(x0[ii] - upperBound[ii]) < 100 * Double.Epsilon || Math.Abs(x0[ii] - lowerBound[ii]) < 100 * Double.Epsilon)
                        breakpoint[ii] = 0.0;
                    else
                        breakpoint[ii] = Double.PositiveInfinity;
                }
            }

            var orderedBreakpoint = new List<double>(x0.Count);
            orderedBreakpoint.AddRange(breakpoint);
            orderedBreakpoint.Sort();

            // Compute initial state variables
            var d = -gradient;
            for (int ii = 0; ii < d.Count; ++ii)
                if (breakpoint[ii] <= 0.0)
                    d[ii] *= 0.0;


            int jj = -1;
            var x = x0;
            var f1 = gradient * d;
            var f2 = 0.5 * d * hessian * d;
            var sMin = -f1 / f2;
            var maxS = orderedBreakpoint[0];

            if (sMin < maxS)
                return new GradientProjectionResult(x + sMin * d, 0,isFixed);

            // while minimum of the last quadratic piece observed is beyond the interval searched
            while (true)
            {
                // update data to the beginning of the interval we're searching
                jj += 1;
                x = x + d * maxS;
                maxS = orderedBreakpoint[jj+1] - orderedBreakpoint[jj];

                int fixedCount = 0;
                for (int ii = 0; ii < d.Count; ++ii)
                    if (orderedBreakpoint[jj] >= breakpoint[ii])
                    {
                        d[ii] *= 0.0;
                        isFixed[ii] = true;
                        fixedCount += 1;
                    }

                if (Double.IsPositiveInfinity(orderedBreakpoint[jj + 1]))
                    return new GradientProjectionResult(x, fixedCount, isFixed);

                f1 = gradient * d + (x - x0) * hessian * d;
                f2 = d * hessian * d;

                sMin = -f1 / f2;

                if (sMin < maxS)
                    return new GradientProjectionResult(x + sMin * d, fixedCount, isFixed);
                else if (jj + 1 >= orderedBreakpoint.Count - 1)
                {
                    isFixed[isFixed.Count - 1] = true;
                    return new GradientProjectionResult(x + maxS * d, lowerBound.Count, isFixed);
                }
            }
        }

        public struct GradientProjectionResult
        {
            public GradientProjectionResult(Vector<double> cauchyPoint, int fixedCount, List<bool> isFixed)
            {
                CauchyPoint = cauchyPoint;
                FixedCount = fixedCount;
                IsFixed = isFixed;
            }
            public Vector<double> CauchyPoint { get; }
            public int FixedCount { get; }
            public List<bool> IsFixed { get; }
        }
    }
}
