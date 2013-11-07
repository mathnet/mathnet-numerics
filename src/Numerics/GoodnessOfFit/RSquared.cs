// <copyright file="RSquared.cs">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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
using System.Linq;

namespace MathNet.Numerics.GoodnessOfFit
{
    public static class RSquared
    {
        /// <summary>
        /// Calculated the R-Squared value given modelled and observed values
        /// </summary>
        /// <param name="modelledValues">The values expected from the modelled</param>
        /// <param name="observedValues">The actual data set values obtained</param>
        /// <returns></returns>
        public static double RSqr(IEnumerable<double> modelledValues, IEnumerable<double> observedValues)
        {
            var modelledData = modelledValues as double[] ?? modelledValues.ToArray();
            var observedData = observedValues as double[] ?? observedValues.ToArray();
            var observedDataCount = observedData.Count();
            if ( modelledData.Count() != observedDataCount)
            {
                throw new ArgumentException("Dataset length mismatch");
            }

            var observedSum = observedData.Sum();
            var modelledSum = modelledData.Sum();

            var sumObservedByModelled = 0d;

            for (var itemIndex = 0; itemIndex < observedDataCount; itemIndex++)
            {
                sumObservedByModelled += (observedData[itemIndex] * modelledData[itemIndex]);
            }

            var sumObservedSquared = observedData.Sum(item => item * item);
            var sumModelledSquared = modelledData.Sum(item => item * item);

            return Math.Pow(( observedDataCount * sumObservedByModelled - observedSum * modelledSum ) /
                Math.Sqrt((observedDataCount * sumObservedSquared - Math.Pow(observedSum, 2)) 
                    * (observedDataCount * sumModelledSquared - Math.Pow(modelledSum, 2))), 2);
        }
    }
}
