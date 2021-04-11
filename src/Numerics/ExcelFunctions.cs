// <copyright file="ExcelFunctions.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2013 Math.NET
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
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System.Runtime;

// ReSharper disable InconsistentNaming

namespace MathNet.Numerics
{
    /// <summary>
    /// Collection of functions equivalent to those provided by Microsoft Excel
    /// but backed instead by Math.NET Numerics.
    /// We do not recommend to use them except in an intermediate phase when
    /// porting over solutions previously implemented in Excel.
    /// </summary>
    public static class ExcelFunctions
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double NormSDist(double z)
        {
            return Normal.CDF(0d, 1d, z);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double NormSInv(double probability)
        {
            return Normal.InvCDF(0d, 1d, probability);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double NormDist(double x, double mean, double standardDev, bool cumulative)
        {
            return cumulative ? Normal.CDF(mean, standardDev, x) : Normal.PDF(mean, standardDev, x);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double NormInv(double probability, double mean, double standardDev)
        {
            return Normal.InvCDF(mean, standardDev, probability);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double TDist(double x, int degreesFreedom, int tails)
        {
            switch (tails)
            {
                case 1:
                    return 1d - StudentT.CDF(0d, 1d, degreesFreedom, x);
                case 2:
                    return 1d - StudentT.CDF(0d, 1d, degreesFreedom, x) + StudentT.CDF(0d, 1d, degreesFreedom, -x);
                default:
                    throw new ArgumentOutOfRangeException(nameof(tails));
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double TInv(double probability, int degreesFreedom)
        {
            return -StudentT.InvCDF(0d, 1d, degreesFreedom, probability/2);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double FDist(double x, int degreesFreedom1, int degreesFreedom2)
        {
            return 1d - FisherSnedecor.CDF(degreesFreedom1, degreesFreedom2, x);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double FInv(double probability, int degreesFreedom1, int degreesFreedom2)
        {
            return FisherSnedecor.InvCDF(degreesFreedom1, degreesFreedom2, 1d - probability);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double BetaDist(double x, double alpha, double beta)
        {
            return Beta.CDF(alpha, beta, x);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double BetaInv(double probability, double alpha, double beta)
        {
            return Beta.InvCDF(alpha, beta, probability);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double GammaDist(double x, double alpha, double beta, bool cumulative)
        {
            return cumulative ? Gamma.CDF(alpha, 1/beta, x) : Gamma.PDF(alpha, 1/beta, x);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double GammaInv(double probability, double alpha, double beta)
        {
            return Gamma.InvCDF(alpha, 1/beta, probability);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double Quartile(double[] array, int quant)
        {
            switch (quant)
            {
                case 0:
                    return ArrayStatistics.Minimum(array);
                case 1:
                    return array.QuantileCustom(0.25, QuantileDefinition.Excel);
                case 2:
                    return array.QuantileCustom(0.5, QuantileDefinition.Excel);
                case 3:
                    return array.QuantileCustom(0.75, QuantileDefinition.Excel);
                case 4:
                    return ArrayStatistics.Maximum(array);
                default:
                    throw new ArgumentOutOfRangeException(nameof(quant));
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double Percentile(double[] array, double k)
        {
            return array.QuantileCustom(k, QuantileDefinition.Excel);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double PercentRank(double[] array, double x)
        {
            return array.QuantileRank(x, RankDefinition.Min);
        }
    }
}

// ReSharper restore InconsistentNaming
