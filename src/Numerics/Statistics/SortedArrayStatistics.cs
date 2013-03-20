// <copyright file="SortedArrayStatistics.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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

namespace MathNet.Numerics.Statistics
{
    public enum QuantileCompatibility
    {
        Default=0,
        Nist,Nearest,Excel,
        R1,R2,R3,R4,R5,R6,R7,R8,R9,
        SAS1,SAS2,SAS3,SAS4,SAS5
    }

    public static class SortedArrayStatistics
    {
        const double Third = 1d / 3d;
        const double Half = 1d / 2d;

        /// <remarks>
        /// R-8, SciPy-(1/3,1/3):
        /// Linear interpolation of the approximate medians for order statistics.
        /// When tau &lt; (2/3) / (N + 1/3), use x1. When tau &gt;= (N - 1/3) / (N + 1/3), use xN.
        /// </remarks>
        public static double Quantile(double[] data, double tau)
        {
            if (tau < 0d || tau > 1d || data == null || data.Length == 0) return double.NaN;
            if (tau == 0d || data.Length == 1) return data[0];
            if (tau == 1d) return data[data.Length - 1];

            double h = (data.Length + Third)*tau + Third;
            var hf = (int) h;
            return data[hf - 1] + (h - hf)*(data[hf] - data[hf - 1]);
        }

        public static double QuantileCompatible(double[] data, double tau, QuantileCompatibility compatibility)
        {
            if (tau < 0d || tau > 1d || data == null || data.Length == 0) return double.NaN;
            if (tau == 0d || data.Length == 1) return data[0];
            if (tau == 1d) return data[data.Length - 1];

            switch (compatibility)
            {
                case QuantileCompatibility.R1:
                case QuantileCompatibility.SAS3:
                    {
                        double h = data.Length*tau + Half;
                        return data[(int) Math.Ceiling(h - Half) - 1];
                    }
                case QuantileCompatibility.R2:
                case QuantileCompatibility.SAS5:
                    {
                        double h = data.Length * tau + Half;
                        return (data[(int) Math.Ceiling(h - Half) - 1] + data[(int) (h + Half) - 1])*Half;
                    }
                case QuantileCompatibility.R3:
                case QuantileCompatibility.SAS2:
                case QuantileCompatibility.Nearest:
                    {
                        double h = data.Length*tau;
                        return data[(int) Math.Round(h) - 1];
                    }
                case QuantileCompatibility.R4:
                case QuantileCompatibility.SAS1:
                    {
                        double h = data.Length*tau;
                        var hf = (int)h;
                        return data[hf - 1] + (h - hf) * (data[hf] - data[hf - 1]);
                    }
                case QuantileCompatibility.R5:
                    {
                        double h = data.Length*tau + Half;
                        var hf = (int)h;
                        return data[hf - 1] + (h - hf) * (data[hf] - data[hf - 1]);
                    }
                case QuantileCompatibility.R6:
                case QuantileCompatibility.SAS4:
                case QuantileCompatibility.Nist:
                    {
                        double h = (data.Length + 1)*tau;
                        var hf = (int)h;
                        return data[hf - 1] + (h - hf) * (data[hf] - data[hf - 1]);
                    }
                case QuantileCompatibility.R7:
                case QuantileCompatibility.Excel:
                    {
                        double h = (data.Length - 1)*tau + 1d;
                        var hf = (int)h;
                        return data[hf - 1] + (h - hf) * (data[hf] - data[hf - 1]);
                    }
                case QuantileCompatibility.R8:
                case QuantileCompatibility.Default:
                    {
                        double h = (data.Length + Third) * tau + Third;
                        var hf = (int)h;
                        return data[hf - 1] + (h - hf) * (data[hf] - data[hf - 1]);
                    }
                case QuantileCompatibility.R9:
                    {
                        double h = (data.Length + 1d/4d) * tau + 3d/8d;
                        var hf = (int)h;
                        return data[hf - 1] + (h - hf) * (data[hf] - data[hf - 1]);
                    }
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
