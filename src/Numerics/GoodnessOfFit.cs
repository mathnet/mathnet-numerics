// <copyright file="GoodnessOfFit.cs">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2018 Math.NET
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
using System.Collections.Generic;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics
{
    public static class GoodnessOfFit
    {
        /// <summary>
        /// Calculates r^2, the square of the sample correlation coefficient between
        /// the observed outcomes and the observed predictor values.
        /// Not to be confused with R^2, the coefficient of determination, see <see cref="CoefficientOfDetermination"/>.
        /// </summary>
        /// <param name="modelledValues">The modelled/predicted values</param>
        /// <param name="observedValues">The observed/actual values</param>
        /// <returns>Squared Person product-momentum correlation coefficient.</returns>
        public static double RSquared(IEnumerable<double> modelledValues, IEnumerable<double> observedValues)
        {
            var corr = Correlation.Pearson(modelledValues, observedValues);
            return corr * corr;
        }

        /// <summary>
        /// Calculates r, the sample correlation coefficient between the observed outcomes
        /// and the observed predictor values.
        /// </summary>
        /// <param name="modelledValues">The modelled/predicted values</param>
        /// <param name="observedValues">The observed/actual values</param>
        /// <returns>Person product-momentum correlation coefficient.</returns>
        public static double R(IEnumerable<double> modelledValues, IEnumerable<double> observedValues)
        {
            return Correlation.Pearson(modelledValues, observedValues);
        }

        /// <summary>
        /// Calculates the Standard Error of the regression, given a sequence of
        /// modeled/predicted values, and a sequence of actual/observed values
        /// </summary>
        /// <param name="modelledValues">The modelled/predicted values</param>
        /// <param name="observedValues">The observed/actual values</param>
        /// <returns>The Standard Error of the regression</returns>
        public static double PopulationStandardError(IEnumerable<double> modelledValues, IEnumerable<double> observedValues)
        {
            return StandardError(modelledValues, observedValues, 0);
        }

        /// <summary>
        /// Calculates the Standard Error of the regression, given a sequence of
        /// modeled/predicted values, and a sequence of actual/observed values
        /// </summary>
        /// <param name="modelledValues">The modelled/predicted values</param>
        /// <param name="observedValues">The observed/actual values</param>
        /// <param name="degreesOfFreedom">The degrees of freedom by which the
        /// number of samples is reduced for performing the Standard Error calculation</param>
        /// <returns>The Standard Error of the regression</returns>
        public static double StandardError(IEnumerable<double> modelledValues, IEnumerable<double> observedValues, int degreesOfFreedom)
        {
            using (IEnumerator<double> ieM = modelledValues.GetEnumerator())
            using (IEnumerator<double> ieO = observedValues.GetEnumerator())
            {
                double n = 0;
                double accumulator = 0;
                while (ieM.MoveNext())
                {
                    if (!ieO.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(modelledValues), "The array arguments must have the same length.");
                    }
                    double currentM = ieM.Current;
                    double currentO = ieO.Current;
                    var diff = currentM - currentO;
                    accumulator += diff * diff;
                    n++;
                }

                if (degreesOfFreedom >= n)
                {
                    throw new ArgumentOutOfRangeException(nameof(degreesOfFreedom), "The sample size must be larger than the given degrees of freedom.");
                }
                return Math.Sqrt(accumulator / (n - degreesOfFreedom));
            }
        }

        /// <summary>
        /// Calculates the R-Squared value, also known as coefficient of determination,
        /// given some modelled and observed values.
        /// </summary>
        /// <param name="modelledValues">The values expected from the model.</param>
        /// <param name="observedValues">The actual values obtained.</param>
        /// <returns>Coefficient of determination.</returns>
        public static double CoefficientOfDetermination(IEnumerable<double> modelledValues, IEnumerable<double> observedValues)
        {
            var y = observedValues;
            var f = modelledValues;
            int n = 0;

            double meanY = 0;
            double ssTot = 0;
            double ssRes = 0;

            using (IEnumerator<double> ieY = y.GetEnumerator())
            using (IEnumerator<double> ieF = f.GetEnumerator())
            {
                while (ieY.MoveNext())
                {
                    if (!ieF.MoveNext())
                    {
                        throw new ArgumentOutOfRangeException(nameof(modelledValues), "The array arguments must have the same length.");
                    }

                    double currentY = ieY.Current;
                    double currentF = ieF.Current;

                    // If a large constant C is added to every y value,
                    // then each new y have an error of about C*eps,
                    // thus each new deltaY will change by about C*eps (compared to the old deltaY),
                    // and thus ssTot will change by only C*eps*deltaY on each step
                    // thus C*eps*deltaY*n in total.
                    // (This error cannot be eliminated by a Kahan algorithm,
                    // because it is introduced when C is added to the old Y value).
                    //
                    // This is better than summing the square of y values
                    // and then substracting the correct multiple of the square of the sum of y values,
                    // in this latter case ssTot will change by eps*n*(C^2+2*C*meanY) in total.
                    double deltaY = currentY - meanY;
                    double scaleDeltaY = deltaY / ++n;

                    meanY += scaleDeltaY;
                    ssTot += scaleDeltaY* deltaY* (n - 1);

                    // This calculation is as safe as ssTot
                    // in the case when a constant is added to both y and f.
                    ssRes += (currentY - currentF)* (currentY-currentF);
                }

                if (ieF.MoveNext())
                {
                    throw new ArgumentOutOfRangeException(nameof(observedValues), "The array arguments must have the same length.");
                }
            }
            return 1 - ssRes/ssTot;
        }
    }
}
