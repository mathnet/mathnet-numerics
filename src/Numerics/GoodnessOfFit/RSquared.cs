using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.GoodnessOfFit
{
    public class RSquared
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
