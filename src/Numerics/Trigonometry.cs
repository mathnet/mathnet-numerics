// <copyright file="Trigonometry.cs" company="Math.NET">
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

namespace MathNet.Numerics
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Double-precision trigonometry toolkit.
    /// </summary>
    public static class Trig
    {
        /// <summary>
        /// Constant to convert a degree to grad.
        /// </summary>
        private const double DegreeToGradConstant = 10.0 / 9.0;

        /// <summary>
        /// Trigonometric Cosecant of an angle in radian.
        /// </summary>
        /// <param name="radian">
        /// The angle in radian.
        /// </param>
        /// <returns>
        /// Cosecant of an angle in radian.
        /// </returns>
        public static double Cosecant(double radian)
        {
            return 1 / Math.Sin(radian);
        }

        /// <summary>
        /// Trigonometric Cosecant of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The cosecant of a complex number.
        /// </returns>
        public static Complex Cosecant(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Cosecant(value.Real), 0d);
            }

            var sinr = Sine(value.Real);
            var sinhi = HyperbolicSine(value.Imaginary);
            var denom = (sinr * sinr) + (sinhi * sinhi);

            return new Complex(sinr * HyperbolicCosine(value.Imaginary) / denom, -Cosine(value.Real) * sinhi / denom);
        }

        /// <summary>
        /// Trigonometric Cosine of an angle in radian.
        /// </summary>
        /// <param name="radian">
        /// The angle in radian.
        /// </param>
        /// <returns>
        /// The cosine of an angle in radian.
        /// </returns>
        public static double Cosine(double radian)
        {
            return Math.Cos(radian);
        }

        /// <summary>
        /// Trigonometric Cosine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The cosine of a complex number.
        /// </returns>
        public static Complex Cosine(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Cosine(value.Real), 0.0);
            }

            return new Complex(
                Cosine(value.Real) * HyperbolicCosine(value.Imaginary),
                -Sine(value.Real) * HyperbolicSine(value.Imaginary));
        }

        /// <summary>
        /// Trigonometric Cotangent of an angle in radian.
        /// </summary>
        /// <param name="radian">
        /// The angle in radian.
        /// </param>
        /// <returns>
        /// The cotangent of an angle in radian.
        /// </returns>
        public static double Cotangent(double radian)
        {
            return 1 / Math.Tan(radian);
        }

        /// <summary>
        /// Trigonometric Cotangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The cotangent of the complex number.
        /// </returns>
        public static Complex Cotangent(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Cotangent(value.Real), 0d);
            }

            var sinr = Sine(value.Real);
            var sinhi = HyperbolicSine(value.Imaginary);
            var denom = (sinr * sinr) + (sinhi * sinhi);

            return new Complex(sinr * Cosine(value.Real) / denom, -sinhi * HyperbolicCosine(value.Imaginary) / denom);
        }

        /// <summary>
        /// Converts a degree (360-periodic) angle to a grad (400-periodic) angle.
        /// </summary>
        /// <param name="degree">
        /// The degree to convert.
        /// </param>
        /// <returns>
        /// The converted grad angle.
        /// </returns>
        public static double DegreeToGrad(double degree)
        {
            return degree * DegreeToGradConstant;
        }

        /// <summary>
        /// Converts a degree (360-periodic) angle to a radian (2*Pi-periodic) angle.
        /// </summary>
        /// <param name="degree">
        /// The degree to convert.
        /// </param>
        /// <returns>
        /// The converted radian angle.
        /// </returns>
        public static double DegreeToRadian(double degree)
        {
            return degree * Constants.Degree;
        }

        /// <summary>
        /// Converts a grad (400-periodic) angle to a degree (360-periodic) angle.
        /// </summary>
        /// <param name="grad">
        /// The grad to convert.
        /// </param>
        /// <returns>
        /// The converted degree.
        /// </returns>
        public static double GradToDegree(double grad)
        {
            return grad * 0.9;
        }

        /// <summary>
        /// Converts a grad (400-periodic) angle to a radian (2*Pi-periodic) angle.
        /// </summary>
        /// <param name="grad">
        /// The grad to convert.
        /// </param>
        /// <returns>
        /// The converted radian.
        /// </returns>
        public static double GradToRadian(double grad)
        {
            return grad * Constants.Grad;
        }

        /// <summary>
        /// Trigonometric Hyperbolic Cosecant
        /// </summary>
        /// <param name="radian">
        /// The angle in radian.
        /// </param>
        /// <returns>
        /// The hyperbolic cosecant of the radian angle.
        /// </returns>
        public static double HyperbolicCosecant(double radian)
        {
            return 1 / HyperbolicSine(radian);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Cosecant of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic cosecant of a complex number.
        /// </returns>
        public static Complex HyperbolicCosecant(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(HyperbolicCosecant(value.Real), 0.0);
            }

            var exp = value.Exponential();

            if (exp.IsInfinity())
            {
                return Complex.Zero;
            }

            return 2 * exp / (exp.Square() - 1);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Cosine
        /// </summary>
        /// <param name="radian">
        /// The angle in radian.
        /// </param>
        /// <returns>
        /// The hyperbolic Cosine of the radian angle.
        /// </returns>
        public static double HyperbolicCosine(double radian)
        {
            return (Math.Exp(radian) + Math.Exp(-radian)) / 2;
        }

        /// <summary>
        /// Trigonometric Hyperbolic Cosine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic cosine of a complex number.
        /// </returns>
        public static Complex HyperbolicCosine(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(HyperbolicCosine(value.Real), 0.0);
            }

            return new Complex(
                HyperbolicCosine(value.Real) * Cosine(value.Imaginary),
                HyperbolicSine(value.Real) * Sine(value.Imaginary));
        }

        /// <summary>
        /// Trigonometric Hyperbolic Cotangent
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The hyperbolic cotangent of the radian angle.
        /// </returns>
        public static double HyperbolicCotangent(double radian)
        {
            if (radian > 19.115)
            {
                return 1.0;
            }

            if (radian < -19.115)
            {
                return -1;
            }

            var e1 = Math.Exp(radian);
            var e2 = Math.Exp(-radian);
            return (e1 + e2) / (e1 - e2);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Cotangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic cotangent of a complex number.
        /// </returns>
        public static Complex HyperbolicCotangent(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(HyperbolicCotangent(value.Real), 0.0);
            }

            var sini = Sine(value.Imaginary);
            var sinhr = HyperbolicSine(value.Real);

            if (double.IsInfinity(sinhr))
            {
                return new Complex(double.IsPositiveInfinity(sinhr) ? 1 : -1, 0.0);
            }

            var denom = (sini * sini) + (sinhr * sinhr);

            return new Complex(sinhr * HyperbolicCosine(value.Real) / denom, sini * Cosine(value.Imaginary) / denom);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Secant
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The hyperbolic secant of the radian angle.
        /// </returns>
        public static double HyperbolicSecant(double radian)
        {
            return 1 / HyperbolicCosine(radian);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Secant of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic secant of a complex number.
        /// </returns>
        public static Complex HyperbolicSecant(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(HyperbolicSecant(value.Real), 0.0);
            }

            var exp = value.Exponential();

            if (exp.IsInfinity())
            {
                return Complex.Zero;
            }

            return 2 * exp / (exp.Square() + 1);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Sine
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The hyperbolic sine of the radian angle.
        /// </returns>
        public static double HyperbolicSine(double radian)
        {
            return (Math.Exp(radian) - Math.Exp(-radian)) / 2;
        }

        /// <summary>
        /// Trigonometric Hyperbolic Sine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic sine of a complex number.
        /// </returns>
        public static Complex HyperbolicSine(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(HyperbolicSine(value.Real), 0.0);
            }

            return new Complex(
                HyperbolicSine(value.Real) * Cosine(value.Imaginary),
                HyperbolicCosine(value.Real) * Sine(value.Imaginary));
        }

        /// <summary>
        /// Trigonometric Hyperbolic Tangent in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The hyperbolic tangent of the radian angle.
        /// </returns>
        public static double HyperbolicTangent(double radian)
        {
            if (radian > 19.1)
            {
                return 1.0;
            }

            if (radian < -19.1)
            {
                return -1;
            }

            var e1 = Math.Exp(radian);
            var e2 = Math.Exp(-radian);
            return (e1 - e2) / (e1 + e2);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Tangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic tangent of a complex number.
        /// </returns>
        public static Complex HyperbolicTangent(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(HyperbolicTangent(value.Real), 0.0);
            }

            var cosi = Cosine(value.Imaginary);
            var sinhr = HyperbolicSine(value.Real);

            if (double.IsInfinity(sinhr))
            {
                return new Complex(double.IsPositiveInfinity(sinhr) ? 1 : -1, 0.0);
            }

            var denom = (cosi * cosi) + (sinhr * sinhr);

            return new Complex(HyperbolicCosine(value.Real) * sinhr / denom, cosi * Sine(value.Imaginary) / denom);
        }

        /// <summary>
        /// Trigonometric Arc Cosecant in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse cosecant of the radian angle.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// if -1 &lt; <paramref name="radian"/> &lt; 1.
        /// </exception>
        public static double InverseCosecant(double radian)
        {
            return Math.Asin(1 / radian);
        }

        /// <summary>
        /// Trigonometric Arc Cosecant of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The arc cosecant of a complex number.
        /// </returns>
        public static Complex InverseCosecant(this Complex value)
        {
            var inv = 1 / value;
            return -Complex.ImaginaryOne * ((Complex.ImaginaryOne * inv) + (1 - inv.Square()).SquareRoot()).NaturalLogarithm();
        }

        /// <summary>
        /// Trigonometric Arc Cosine in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse cosine of the radian angle.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// if 1 &lt; <paramref name="radian"/>  or  <paramref name="radian"/> &lt; -1.
        /// </exception>
        public static double InverseCosine(double radian)
        {
            return Math.Acos(radian);
        }

        /// <summary>
        /// Trigonometric Arc Cosine of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The arc cosine of a complex number.
        /// </returns>
        public static Complex InverseCosine(this Complex value)
        {
            return -Complex.ImaginaryOne * (value + (Complex.ImaginaryOne * (1 - value.Square()).SquareRoot())).NaturalLogarithm();
        }

        /// <summary>
        /// Trigonometric Arc Cotangent in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse cotangent of the radian angle.
        /// </returns>
        public static double InverseCotangent(double radian)
        {
            return Math.Atan(1 / radian);
        }

        /// <summary>
        /// Trigonometric Arc Cotangent of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The arc cotangent of a complex number.
        /// </returns>
        public static Complex InverseCotangent(this Complex value)
        {
            if (value.IsZero())
            {
                return Math.PI / 2.0;
            }

            var inv = Complex.ImaginaryOne / value;
            return (Complex.ImaginaryOne * 0.5) * ((1.0 - inv).NaturalLogarithm() - (1.0 + inv).NaturalLogarithm());
        }

        /// <summary>
        /// Trigonometric Hyperbolic Arc Cosecant
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse hyperbolic cosecant of the radian angle.
        /// </returns>
        public static double InverseHyperbolicCosecant(double radian)
        {
            return InverseHyperbolicSine(1 / radian);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Arc Cosecant of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic arc cosecant of a complex number.
        /// </returns>
        public static Complex InverseHyperbolicCosecant(this Complex value)
        {
            var inv = 1 / value;
            return (inv + (inv.Square() + 1).SquareRoot()).NaturalLogarithm();
        }

        /// <summary>
        /// Trigonometric Hyperbolic Area Cosine
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse hyperbolic cosine of the radian angle.
        /// </returns>
        public static double InverseHyperbolicCosine(double radian)
        {
            return Math.Log(radian + (Math.Sqrt(radian - 1) * Math.Sqrt(radian + 1)), Math.E);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Arc Cosine of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic arc cosine of a complex number.
        /// </returns>
        public static Complex InverseHyperbolicCosine(this Complex value)
        {
            return (value + ((value - 1).SquareRoot() * (value + 1).SquareRoot())).NaturalLogarithm();
        }

        /// <summary>
        /// Trigonometric Hyperbolic Arc Cotangent
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse hyperbolic cotangent of the radian angle.
        /// </returns>
        public static double InverseHyperbolicCotangent(double radian)
        {
            return 0.5 * Math.Log((radian + 1) / (radian - 1), Math.E);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Arc Cotangent of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic arc cotangent of a complex number.
        /// </returns>
        public static Complex InverseHyperbolicCotangent(this Complex value)
        {
            var inv = 1.0 / value;
            return 0.5 * ((1.0 + inv).NaturalLogarithm() - (1.0 - inv).NaturalLogarithm());
        }

        /// <summary>
        /// Trigonometric Hyperbolic Area Secant
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse hyperbolic secant of the radian angle.
        /// </returns>
        public static double InverseHyperbolicSecant(double radian)
        {
            return InverseHyperbolicCosine(1 / radian);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Arc Secant of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic arc secant of a complex number.
        /// </returns>
        public static Complex InverseHyperbolicSecant(this Complex value)
        {
            var inv = 1 / value;
            return (inv + ((inv - 1).SquareRoot() * (inv + 1).SquareRoot())).NaturalLogarithm();
        }

        /// <summary>
        /// Trigonometric Hyperbolic Area Sine
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse hyperbolic sine of the radian angle.
        /// </returns>
        public static double InverseHyperbolicSine(double radian)
        {
            return Math.Log(radian + Math.Sqrt((radian * radian) + 1), Math.E);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Arc Sine of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic arc sine of a complex number.
        /// </returns>
        public static Complex InverseHyperbolicSine(this Complex value)
        {
            return (value + (value.Square() + 1).SquareRoot()).NaturalLogarithm();
        }

        /// <summary>
        /// Trigonometric Hyperbolic Area Tangent
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse hyperbolic tangent of the radian angle.
        /// </returns>
        public static double InverseHyperbolicTangent(double radian)
        {
            return 0.5 * Math.Log((1 + radian) / (1 - radian), Math.E);
        }

        /// <summary>
        /// Trigonometric Hyperbolic Arc Tangent of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The hyperbolic arc tangent of a complex number.
        /// </returns>
        public static Complex InverseHyperbolicTangent(this Complex value)
        {
            return 0.5 * ((1 + value).NaturalLogarithm() - (1 - value).NaturalLogarithm());
        }

        /// <summary>
        /// Trigonometric Arc Secant in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse secant of the radian angle.
        /// </returns>
        public static double InverseSecant(double radian)
        {
            return Math.Acos(1 / radian);
        }

        /// <summary>
        /// Trigonometric Arc Secant of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The arc secant of a complex number.
        /// </returns>
        public static Complex InverseSecant(this Complex value)
        {
            var inv = 1 / value;
            return -Complex.ImaginaryOne * (inv + (Complex.ImaginaryOne * (1 - inv.Square()).SquareRoot())).NaturalLogarithm();
        }

        /// <summary>
        /// Trigonometric Arc Sine in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse sine of the radian angle.
        /// </returns>
        public static double InverseSine(double radian)
        {
            return Math.Asin(radian);
        }

        /// <summary>
        /// Trigonometric Arc Sine of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The arc sine of a complex number.
        /// </returns>
        public static Complex InverseSine(this Complex value)
        {
            return -Complex.ImaginaryOne * ((1 - value.Square()).SquareRoot() + (Complex.ImaginaryOne * value)).NaturalLogarithm();
        }

        /// <summary>
        /// Trigonometric Arc Tangent  in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian angle.
        /// </param>
        /// <returns>
        /// The inverse tangent of the radian angle.
        /// </returns>
        public static double InverseTangent(double radian)
        {
            return Math.Atan(radian);
        }

        /// <summary>
        /// Trigonometric Arc Tangent of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The arc tangent of a complex number.
        /// </returns>
        public static Complex InverseTangent(this Complex value)
        {
            var iz = new Complex(-value.Imaginary, value.Real); // I*this
            return new Complex(0, 0.5) * ((1 - iz).NaturalLogarithm() - (1 + iz).NaturalLogarithm());
        }

        /// <summary>
        /// Converts a radian (2*Pi-periodic) angle to a degree (360-periodic) angle.
        /// </summary>
        /// <param name="radian">
        /// The radian to convert.
        /// </param>
        /// <returns>
        /// The converted degree.
        /// </returns>
        public static double RadianToDegree(double radian)
        {
            return radian / Constants.Degree;
        }

        /// <summary>
        /// Converts a radian (2*Pi-periodic) angle to a grad (400-periodic) angle.
        /// </summary>
        /// <param name="radian">
        /// The radian to convert.
        /// </param>
        /// <returns>
        /// The converted grad.
        /// </returns>
        public static double RadianToGrad(double radian)
        {
            return radian / Constants.Grad;
        }

        /// <summary>
        /// Trigonometric Secant of an angle in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian.
        /// </param>
        /// <returns>
        /// The secant of the radian angle.
        /// </returns>
        public static double Secant(double radian)
        {
            return 1 / Math.Cos(radian);
        }

        /// <summary>
        /// Trigonometric Secant of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The secant of the complex number.
        /// </returns>
        public static Complex Secant(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Secant(value.Real), 0d);
            }

            var cosr = Cosine(value.Real);
            var sinhi = HyperbolicSine(value.Imaginary);
            var denom = (cosr * cosr) + (sinhi * sinhi);

            return new Complex(cosr * HyperbolicCosine(value.Imaginary) / denom, Sine(value.Real) * sinhi / denom);
        }

        /// <summary>
        /// Trigonometric Sine of an angle in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian.
        /// </param>
        /// <returns>
        /// The sine of the radian angle.
        /// </returns>
        public static double Sine(double radian)
        {
            return Math.Sin(radian);
        }

        /// <summary>
        /// Trigonometric Sine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The sine of the complex number.
        /// </returns>
        public static Complex Sine(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Sine(value.Real), 0.0);
            }

            return new Complex(
                Sine(value.Real) * HyperbolicCosine(value.Imaginary),
                Cosine(value.Real) * HyperbolicSine(value.Imaginary));
        }

        /// <summary>
        /// Trigonometric Tangent of an angle in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian.
        /// </param>
        /// <returns>
        /// The tangent of the radian angle.
        /// </returns>
        public static double Tangent(double radian)
        {
            return Math.Tan(radian);
        }

        /// <summary>
        /// Trigonometric Tangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">
        /// The complex value.
        /// </param>
        /// <returns>
        /// The tangent of the complex number.
        /// </returns>
        public static Complex Tangent(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Tangent(value.Real), 0.0);
            }

            var cosr = Cosine(value.Real);
            var sinhi = HyperbolicSine(value.Imaginary);
            var denom = (cosr * cosr) + (sinhi * sinhi);

            return new Complex(Sine(value.Real) * cosr / denom, sinhi * HyperbolicCosine(value.Imaginary) / denom);
        }
    }
}