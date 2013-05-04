using System;
using System.Collections.Generic;

namespace MathNet.Numerics.RootFinding.Algorithms
{
    public static class ZeroCrossingBracketing
    {
        public static IEnumerable<Tuple<double, double>> FindIntervalsWithin(Func<double, double> f, double lowerBound, double upperBound, int parts)
        {
            // TODO: Consider binary-style search instead of linear scan

            var fmin = f(lowerBound);
            var fmax = f(upperBound);

            if (Math.Sign(fmin) != Math.Sign(fmax))
            {
                yield return new Tuple<double, double>(lowerBound, upperBound);
                yield break;
            }

            double subdiv = (upperBound - lowerBound)/parts;
            var smin = lowerBound;
            int sign = Math.Sign(fmin);

            for (int k = 0; k < parts; k++)
            {
                var smax = smin + subdiv;
                var sfmax = f(smax);
                if (double.IsInfinity(sfmax))
                {
                    // expand interval to include pole
                    smin = smax;
                    continue;
                }
                if (Math.Sign(sfmax) != sign)
                {
                    yield return new Tuple<double, double>(smin, smax);
                    sign = Math.Sign(sfmax);
                }
                smin = smax;
            }
        }
    }
}
