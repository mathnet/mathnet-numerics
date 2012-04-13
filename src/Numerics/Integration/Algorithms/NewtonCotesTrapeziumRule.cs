// <copyright file="NewtonCotesTrapeziumRule.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.Integration.Algorithms
{
    using System;
    using System.Collections.Generic;
    using Properties;

    /// <summary>
    /// Approximation algorithm for definite integrals by the Trapezium rule of the Newton-Cotes family.
    /// </summary>
    /// <remarks>
    /// <a href="http://en.wikipedia.org/wiki/Trapezium_rule">Wikipedia - Trapezium Rule</a>
    /// </remarks>
    public static class NewtonCotesTrapeziumRule
    {
        /// <summary>
        /// Direct 2-point approximation of the definite integral in the provided interval by the trapezium rule.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double IntegrateTwoPoint(
            Func<double, double> f,
            double intervalBegin,
            double intervalEnd)
        {
            if (f == null)
            {
                throw new ArgumentNullException("f");
            }

            return (intervalEnd - intervalBegin) / 2 * (f(intervalBegin) + f(intervalEnd));
        }

        /// <summary>
        /// Composite N-point approximation of the definite integral in the provided interval by the trapezium rule.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="numberOfPartitions">Number of composite subdivision partitions.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double IntegrateComposite(
            Func<double, double> f,
            double intervalBegin,
            double intervalEnd,
            int numberOfPartitions)
        {
            if (f == null)
            {
                throw new ArgumentNullException("f");
            }

            if (numberOfPartitions <= 0)
            {
                throw new ArgumentOutOfRangeException("numberOfPartitions", Resources.ArgumentPositive);
            }

            double step = (intervalEnd - intervalBegin) / numberOfPartitions;

            double offset = step;
            double sum = 0.5 * (f(intervalBegin) + f(intervalEnd));
            for (int i = 0; i < numberOfPartitions - 1; i++)
            {
                // NOTE (ruegg, 2009-01-07): Do not combine intervalBegin and offset (numerical stability!)
                sum += f(intervalBegin + offset);
                offset += step;
            }

            return step * sum;
        }

        /// <summary>
        /// Adaptive approximation of the definite integral in the provided interval by the trapezium rule.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="targetError">The expected accuracy of the approximation.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double IntegrateAdaptive(
            Func<double, double> f,
            double intervalBegin,
            double intervalEnd,
            double targetError)
        {
            if (f == null)
            {
                throw new ArgumentNullException("f");
            }

            int numberOfPartitions = 1;
            double step = intervalEnd - intervalBegin;
            double sum = 0.5 * step * (f(intervalBegin) + f(intervalEnd));
            for (int k = 0; k < 20; k++)
            {
                double midpointsum = 0;
                for (int i = 0; i < numberOfPartitions; i++)
                {
                    midpointsum += f(intervalBegin + ((i + 0.5) * step));
                }

                midpointsum *= step;
                sum = 0.5 * (sum + midpointsum);
                step *= 0.5;
                numberOfPartitions *= 2;

                if (sum.AlmostEqualWithError(midpointsum, targetError))
                {
                    break;
                }
            }

            return sum;
        }

        /// <summary>
        /// Adaptive approximation of the definite integral by the trapezium rule.
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate.</param>
        /// <param name="intervalBegin">Where the interval starts, inclusive and finite.</param>
        /// <param name="intervalEnd">Where the interval stops, inclusive and finite.</param>
        /// <param name="levelAbscissas">Abscissa vector per level provider.</param>
        /// <param name="levelWeights">Weight vector per level provider.</param>
        /// <param name="levelOneStep">First Level Step</param>
        /// <param name="targetRelativeError">The expected relative accuracy of the approximation.</param>
        /// <returns>Approximation of the finite integral in the given interval.</returns>
        public static double IntegrateAdaptiveTransformedOdd(
            Func<double, double> f,
            double intervalBegin,
            double intervalEnd,
            IEnumerable<double[]> levelAbscissas,
            IEnumerable<double[]> levelWeights,
            double levelOneStep,
            double targetRelativeError)
        {
            if (f == null)
            {
                throw new ArgumentNullException("f");
            }

            if (levelAbscissas == null)
            {
                throw new ArgumentNullException("levelAbscissas");
            }

            if (levelWeights == null)
            {
                throw new ArgumentNullException("levelWeights");
            }

            double linearSlope = 0.5 * (intervalEnd - intervalBegin);
            double linearOffset = 0.5 * (intervalEnd + intervalBegin);
            targetRelativeError /= 5 * linearSlope;

            using (var abcissasIterator = levelAbscissas.GetEnumerator())
            using (var weightsIterator = levelWeights.GetEnumerator())
            {
                double step = levelOneStep;

                // First Level
                abcissasIterator.MoveNext();
                weightsIterator.MoveNext();
                double[] abcissasL1 = abcissasIterator.Current;
                double[] weightsL1 = weightsIterator.Current;

                double sum = f(linearOffset) * weightsL1[0];
                for (int i = 1; i < abcissasL1.Length; i++)
                {
                    sum += weightsL1[i] * (f((linearSlope * abcissasL1[i]) + linearOffset) + f(-(linearSlope * abcissasL1[i]) + linearOffset));
                }

                sum *= step;

                // Additional Levels
                double previousDelta = double.MaxValue;
                for (int level = 1; abcissasIterator.MoveNext() && weightsIterator.MoveNext(); level++)
                {
                    double[] abcissas = abcissasIterator.Current;
                    double[] weights = weightsIterator.Current;

                    double midpointsum = 0;
                    for (int i = 0; i < abcissas.Length; i++)
                    {
                        midpointsum += weights[i] * (f((linearSlope * abcissas[i]) + linearOffset) + f(-(linearSlope * abcissas[i]) + linearOffset));
                    }

                    midpointsum *= step;
                    sum = 0.5 * (sum + midpointsum);
                    step *= 0.5;

                    double delta = Math.Abs(sum - midpointsum);

                    if (level == 1)
                    {
                        previousDelta = delta;
                        continue;
                    }

                    double r = Math.Log(delta) / Math.Log(previousDelta);
                    previousDelta = delta;

                    if (r > 1.9 && r < 2.1)
                    {
                        // convergence region
                        delta = Math.Sqrt(delta);
                    }

                    if (sum.AlmostEqualWithRelativeError(midpointsum, delta, targetRelativeError))
                    {
                        break;
                    }
                }

                return sum * linearSlope;
            }
        }
    }
}
