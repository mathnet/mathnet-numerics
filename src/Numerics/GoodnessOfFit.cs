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

using System.Collections.Generic;
using MathNet.Numerics.Statistics;
using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics
{
    public static class GoodnessOfFit
    {
        /// <summary>
        /// Calculated the R-Squared value, also known as coefficient of determination,
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
        /// Calculated the R-Squared value, also known as coefficient of determination,
        /// given modelled and observed values
        /// </summary>
        /// <param name="modelledValues">The values expected from the modelled</param>
        /// <param name="observedValues">The actual data set values obtained</param>
        /// <returns>Squared Person product-momentum correlation coefficient.</returns>
        public static double CoefficentOfDetermination(IEnumerable<double> modelledValues, IEnumerable<double> observedValues)
        {
            var y = observedValues;
            var f = modelledValues;
            int n = 0;

            double meanY = 0;
            double ssTot = 0;
            double ssRes = 0;

            using (IEnumerator<double> ieY = y.GetEnumerator())
            using (IEnumerator<double> ieF = f.GetEnumerator()) {
                while (ieY.MoveNext()) {
                    if (!ieF.MoveNext()) {
                        throw new ArgumentOutOfRangeException("modelledValues", Resources.ArgumentArraysSameLength);
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
                    double scaleDeltaY = deltaY/++n;

                    meanY += scaleDeltaY;
                    ssTot += scaleDeltaY*deltaY*(n - 1);

                    // This calculation is as safe as ssTot
                    // in the case when a constant is added to both y and f.
                    ssRes += (currentY - currentF)*(currentY-currentF);
                }

                if (ieF.MoveNext()) {
                    throw new ArgumentOutOfRangeException("observedValues", Resources.ArgumentArraysSameLength);
                }
            }
            return 1 - ssRes/ssTot;
        }

        /// <summary>
        /// Calculated the R value, also known as linear correlation coefficient,
        /// given modelled and observed values
        /// </summary>
        /// <param name="modelledValues">The values expected from the modelled</param>
        /// <param name="observedValues">The actual data set values obtained</param>
        /// <returns>Person product-momentum correlation coefficient.</returns>
        public static double R(IEnumerable<double> modelledValues, IEnumerable<double> observedValues)
        {
            return Correlation.Pearson(modelledValues, observedValues);
        }
    }
}
