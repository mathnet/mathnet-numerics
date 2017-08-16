// <copyright file="GoodnessOfFit.cs">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using MathNet.Numerics.Properties;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics
{
    public static class GoodnessOfFit
    {
        /// <summary>
        /// Calculates the R-Squared value, also known as coefficient of determination,
        /// given modelled and observed values
        /// </summary>
        /// <param name="modelledValues">The values expected from the modelled</param>
        /// <param name="observedValues">The actual data set values obtained</param>
        /// <returns>Squared Person product-momentum correlation coefficient.</returns>
        public static double RSquared(IEnumerable<double> modelledValues, IEnumerable<double> observedValues)
        {
            var corr = Correlation.Pearson(modelledValues, observedValues);
            return corr * corr;
        }

        /// <summary>
        /// Calculates the R value, also known as linear correlation coefficient,
        /// given modelled and observed values
        /// </summary>
        /// <param name="modelledValues">The values expected from the modelled</param>
        /// <param name="observedValues">The actual data set values obtained</param>
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
                        throw new ArgumentOutOfRangeException("modelledValues", Resources.ArgumentArraysSameLength);
                    }
                    double currentM = ieM.Current;
                    double currentO = ieO.Current;
                    var diff = currentM - currentO;
                    accumulator += diff * diff;
                    n++;
                }

                if (degreesOfFreedom >= n)
                {
                    throw new ArgumentOutOfRangeException("degreesOfFreedom", Resources.DegreesOfFreedomMustBeLessThanSampleSize);
                }
                return Math.Sqrt(accumulator / (n - degreesOfFreedom));
            }
        }
    }
}
