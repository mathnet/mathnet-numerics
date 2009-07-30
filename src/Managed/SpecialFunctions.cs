// <copyright file="SpecialFunctions.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics
{
    /// <summary>
    /// This class implements a collection of special function evaluations for double precision. This class 
    /// has a static constructor which will precompute a small number of values for faster runtime computations.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// The order of the GammaLn approximation.
        /// </summary>
        private const int Gamma_n = 10;

        /// <summary>
        /// Auxiliary variable when evaluating the GammaLn function.
        /// </summary>
        private const double Gamma_r = 10.900511;

        /// <summary>
        /// Polynomial coefficients for the GammaLn approximation.
        /// </summary>
        private static readonly double[] Gamma_dk = new double[]
                                                    {
                                                        2.48574089138753565546e-5,
                                                        1.05142378581721974210,
                                                        -3.45687097222016235469,
                                                        4.51227709466894823700,
                                                        -2.98285225323576655721,
                                                        1.05639711577126713077,
                                                        -1.95428773191645869583e-1,
                                                        1.70970543404441224307e-2,
                                                        -5.71926117404305781283e-4,
                                                        4.63399473359905636708e-6,
                                                        -2.71994908488607703910e-9
                                                    };

        /// <summary>
        /// Computes the hypotenuse of a right angle triangle.
        /// </summary>
        /// <param name="a">The length of side a of the triangle.</param>
        /// <param name="b">The length of side b of the triangle.</param>
        /// <returns>Returns <code>sqrt(a<sup>2</sup> + b<sup>2</sup>)</code> without underflow/overflow.</returns>
        public static double Hypotenuse(double a, double b)
        {
            if (Math.Abs(a) > Math.Abs(b))
            {
                double r = b / a;
                return Math.Abs(a) * Math.Sqrt(1 + (r * r));
            }

            if (!b.AlmostZero())
            {
                double r = a / b;
                return Math.Abs(b) * Math.Sqrt(1 + (r * r));
            }

            return 0d;
        }

        /// <summary>
        /// Computes the logarithm of the Gamma function. 
        /// </summary>
        /// <param name="z">The argument of the gamma function.</param>
        /// <returns>The logarithm of the gamma function.</returns>
        /// <remarks>
        /// <para>This implementation of the computation of the gamma and logarithm of the gamma function follows the derivation in
        ///     "An Analysis Of The Lanczos Gamma Approximation", Glendon Ralph Pugh, 2004.
        /// We use the implementation listed on p. 116 which achieves an accuracy of 16 floating point digits. Although 16 digit accuracy
        /// should be sufficient for double values, improving accuracy is possible (see p. 126 in Pugh).</para>
        /// <para>Our unit tests suggest that the accuracy of the Gamma function is correct up to 14 floating point digits.</para>
        /// </remarks>
        public static double GammaLn(double z)
        {
            if (z < 0.5)
            {
                double s = Gamma_dk[0];
                for (int i = 1; i <= Gamma_n; i++)
                {
                    s += Gamma_dk[i] / (i - z);
                }
                return Constants.LnPi - Math.Log(Math.Sin(Math.PI * z)) - Math.Log(s) - Constants.LogTwoSqrtEOverPi - (0.5 - z) * Math.Log((0.5 - z + Gamma_r) / Math.E);
            }
            else
            {
                double s = Gamma_dk[0];
                for (int i = 1; i <= Gamma_n; i++)
                {
                    s += Gamma_dk[i] / (z + i - 1.0);
                }
                return Math.Log(s) + Constants.LogTwoSqrtEOverPi + (z - 0.5) * Math.Log((z - 0.5 + Gamma_r) / Math.E);
            }
        }

        /// <summary>
        /// Computes the Gamma function. 
        /// </summary>
        /// <param name="z">The argument of the gamma function.</param>
        /// <returns>The logarithm of the gamma function.</returns>
        /// <remarks>
        /// <para>
        /// This implementation of the computation of the gamma and logarithm of the gamma function follows the derivation in
        ///     "An Analysis Of The Lanczos Gamma Approximation", Glendon Ralph Pugh, 2004.
        /// We use the implementation listed on p. 116 which should achieve an accuracy of 16 floating point digits. Although 16 digit accuracy
        /// should be sufficient for double values, improving accuracy is possible (see p. 126 in Pugh).
        /// </para>
        /// <para>Our unit tests suggest that the accuracy of the Gamma function is correct up to 13 floating point digits.</para>
        /// </remarks>
        public static double Gamma(double z)
        {
            if (z < 0.5)
            {
                double s = Gamma_dk[0];
                for (int i = 1; i <= Gamma_n; i++)
                {
                    s += Gamma_dk[i] / (i - z);
                }
                return Math.PI / (Math.Sin(Math.PI * z) * s * Constants.TwoSqrtEOverPi * Math.Pow((0.5 - z + Gamma_r) / Math.E, 0.5 - z));
            }
            else
            {
                double s = Gamma_dk[0];
                for (int i = 1; i <= Gamma_n; i++)
                {
                    s += Gamma_dk[i] / (z + i - 1.0);
                }
                return s * Constants.TwoSqrtEOverPi * Math.Pow((z - 0.5 + Gamma_r) / Math.E, z - 0.5);
            }
        }

        public static double IncompleteGamma(double x, double z, bool reg)
        {
            throw new NotImplementedException();
        }
        public static double BetaLn(double a, double b)
        {
            throw new NotImplementedException();
        }

        public static double BetaRegularized(double a, double b, double x)
        {
            throw new NotImplementedException();
        }

        public static double DiGamma(double x)
        {
            throw new NotImplementedException();
        }
    }
}
