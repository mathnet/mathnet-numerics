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

#if !NOSYSNUMERICS
    using Complex = System.Numerics.Complex;
#endif

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
        /// Trigonometric Sine of an angle in radian
        /// </summary>
        /// <param name="radian">
        /// The angle in radian.
        /// </param>
        /// <returns>
        /// The sine of the radian angle.
        /// </returns>
        public static double Sin(double radian)
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
        public static Complex Sin(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Sin(value.Real), 0.0);
            }

            return new Complex(
                Sin(value.Real) * Cosh(value.Imaginary),
                Cos(value.Real) * Sinh(value.Imaginary));
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
        public static double Cos(double radian)
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
        public static Complex Cos(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Cos(value.Real), 0.0);
            }

            return new Complex(
                Cos(value.Real) * Cosh(value.Imaginary),
                -Sin(value.Real) * Sinh(value.Imaginary));
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
        public static double Tan(double radian)
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
        public static Complex Tan(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Tan(value.Real), 0.0);
            }

            var cosr = Cos(value.Real);
            var sinhi = Sinh(value.Imaginary);
            var denom = (cosr * cosr) + (sinhi * sinhi);

            return new Complex(Sin(value.Real) * cosr / denom, sinhi * Cosh(value.Imaginary) / denom);
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
        public static double Cot(double radian)
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
        public static Complex Cot(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Cot(value.Real), 0d);
            }

            var sinr = Sin(value.Real);
            var sinhi = Sinh(value.Imaginary);
            var denom = (sinr * sinr) + (sinhi * sinhi);

            return new Complex(sinr * Cos(value.Real) / denom, -sinhi * Cosh(value.Imaginary) / denom);
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
        public static double Sec(double radian)
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
        public static Complex Sec(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Sec(value.Real), 0d);
            }

            var cosr = Cos(value.Real);
            var sinhi = Sinh(value.Imaginary);
            var denom = (cosr * cosr) + (sinhi * sinhi);

            return new Complex(cosr * Cosh(value.Imaginary) / denom, Sin(value.Real) * sinhi / denom);
        }

        /// <summary>
        /// Trigonometric Cosecant of an angle in radian.
        /// </summary>
        /// <param name="radian">
        /// The angle in radian.
        /// </param>
        /// <returns>
        /// Cosecant of an angle in radian.
        /// </returns>
        public static double Csc(double radian)
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
        public static Complex Csc(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Csc(value.Real), 0d);
            }

            var sinr = Sin(value.Real);
            var sinhi = Sinh(value.Imaginary);
            var denom = (sinr * sinr) + (sinhi * sinhi);

            return new Complex(sinr * Cosh(value.Imaginary) / denom, -Cos(value.Real) * sinhi / denom);
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
        public static double Asin(double radian)
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
        public static Complex Asin(this Complex value)
        {
            if (value.Imaginary > 0 || value.Imaginary == 0d && value.Real < 0)
            {
                return -Asin(-value);
            }

            return -Complex.ImaginaryOne * ((1 - value.Square()).SquareRoot() + (Complex.ImaginaryOne * value)).Ln();
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
        public static double Acos(double radian)
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
        public static Complex Acos(this Complex value)
        {
            if (value.Imaginary < 0 || value.Imaginary == 0d && value.Real > 0)
            {
                return Constants.Pi - Acos(-value);
            }

            return -Complex.ImaginaryOne * (value + (Complex.ImaginaryOne * (1 - value.Square()).SquareRoot())).Ln();
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
        public static double Atan(double radian)
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
        public static Complex Atan(this Complex value)
        {
            var iz = new Complex(-value.Imaginary, value.Real); // I*this
            return new Complex(0, 0.5) * ((1 - iz).Ln() - (1 + iz).Ln());
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
        public static double Acot(double radian)
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
        public static Complex Acot(this Complex value)
        {
            if (value.IsZero())
            {
                return Constants.PiOver2;
            }

            var inv = Complex.ImaginaryOne / value;
            return (Complex.ImaginaryOne * 0.5) * ((1.0 - inv).Ln() - (1.0 + inv).Ln());
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
        public static double Asec(double radian)
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
        public static Complex Asec(this Complex value)
        {
            var inv = 1 / value;
            return -Complex.ImaginaryOne * (inv + (Complex.ImaginaryOne * (1 - inv.Square()).SquareRoot())).Ln();
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
        public static double Acsc(double radian)
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
        public static Complex Acsc(this Complex value)
        {
            var inv = 1 / value;
            return -Complex.ImaginaryOne * ((Complex.ImaginaryOne * inv) + (1 - inv.Square()).SquareRoot()).Ln();
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
        public static double Sinh(double radian)
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
        public static Complex Sinh(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Sinh(value.Real), 0.0);
            }

            return new Complex(
                Sinh(value.Real) * Cos(value.Imaginary),
                Cosh(value.Real) * Sin(value.Imaginary));
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
        public static double Cosh(double radian)
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
        public static Complex Cosh(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Cosh(value.Real), 0.0);
            }

            return new Complex(
                Cosh(value.Real) * Cos(value.Imaginary),
                Sinh(value.Real) * Sin(value.Imaginary));
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
        public static double Tanh(double radian)
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
        public static Complex Tanh(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Tanh(value.Real), 0.0);
            }

            var cosi = Cos(value.Imaginary);
            var sinhr = Sinh(value.Real);

            if (double.IsInfinity(sinhr))
            {
                return new Complex(double.IsPositiveInfinity(sinhr) ? 1 : -1, 0.0);
            }

            var denom = (cosi * cosi) + (sinhr * sinhr);

            return new Complex(Cosh(value.Real) * sinhr / denom, cosi * Sin(value.Imaginary) / denom);
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
        public static double Coth(double radian)
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
        public static Complex Coth(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Coth(value.Real), 0.0);
            }

            var sini = Sin(value.Imaginary);
            var sinhr = Sinh(value.Real);

            if (double.IsInfinity(sinhr))
            {
                return new Complex(double.IsPositiveInfinity(sinhr) ? 1 : -1, 0.0);
            }

            var denom = (sini * sini) + (sinhr * sinhr);

            return new Complex(sinhr * Cosh(value.Real) / denom, sini * Cos(value.Imaginary) / denom);
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
        public static double Sech(double radian)
        {
            return 1 / Cosh(radian);
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
        public static Complex Sech(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Sech(value.Real), 0.0);
            }

            var exp = value.Exp();

            if (exp.IsInfinity())
            {
                return Complex.Zero;
            }

            return 2 * exp / (exp.Square() + 1);
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
        public static double Csch(double radian)
        {
            return 1 / Sinh(radian);
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
        public static Complex Csch(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Csch(value.Real), 0.0);
            }

            var exp = value.Exp();

            if (exp.IsInfinity())
            {
                return Complex.Zero;
            }

            return 2 * exp / (exp.Square() - 1);
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
        public static double Asinh(double radian)
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
        public static Complex Asinh(this Complex value)
        {
            return (value + (value.Square() + 1).SquareRoot()).Ln();
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
        public static double Acosh(double radian)
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
        public static Complex Acosh(this Complex value)
        {
            return (value + ((value - 1).SquareRoot() * (value + 1).SquareRoot())).Ln();
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
        public static double Atanh(double radian)
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
        public static Complex Atanh(this Complex value)
        {
            return 0.5 * ((1 + value).Ln() - (1 - value).Ln());
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
        public static double Acoth(double radian)
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
        public static Complex Acoth(this Complex value)
        {
            var inv = 1.0 / value;
            return 0.5 * ((1.0 + inv).Ln() - (1.0 - inv).Ln());
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
        public static double Asech(double radian)
        {
            return Acosh(1 / radian);
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
        public static Complex Asech(this Complex value)
        {
            var inv = 1 / value;
            return (inv + ((inv - 1).SquareRoot() * (inv + 1).SquareRoot())).Ln();
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
        public static double Acsch(double radian)
        {
            return Asinh(1 / radian);
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
        public static Complex Acsch(this Complex value)
        {
            var inv = 1 / value;
            return (inv + (inv.Square() + 1).SquareRoot()).Ln();
        }
    }
}