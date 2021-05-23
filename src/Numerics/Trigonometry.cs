// <copyright file="Trigonometry.cs" company="Math.NET">
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
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics
{
    /// <summary>
    /// Double-precision trigonometry toolkit.
    /// </summary>
    public static class Trig
    {
        /// <summary>
        /// Constant to convert a degree to grad.
        /// </summary>
        const double DegreeToGradConstant = 10.0 / 9.0;

        /// <summary>
        /// Converts a degree (360-periodic) angle to a grad (400-periodic) angle.
        /// </summary>
        /// <param name="degree">The degree to convert.</param>
        /// <returns>The converted grad angle.</returns>
        public static double DegreeToGrad(double degree)
        {
            return degree * DegreeToGradConstant;
        }

        /// <summary>
        /// Converts a degree (360-periodic) angle to a radian (2*Pi-periodic) angle.
        /// </summary>
        /// <param name="degree">The degree to convert.</param>
        /// <returns>The converted radian angle.</returns>
        public static double DegreeToRadian(double degree)
        {
            return degree * Constants.Degree;
        }

        /// <summary>
        /// Converts a grad (400-periodic) angle to a degree (360-periodic) angle.
        /// </summary>
        /// <param name="grad">The grad to convert.</param>
        /// <returns>The converted degree.</returns>
        public static double GradToDegree(double grad)
        {
            return grad * 0.9;
        }

        /// <summary>
        /// Converts a grad (400-periodic) angle to a radian (2*Pi-periodic) angle.
        /// </summary>
        /// <param name="grad">The grad to convert.</param>
        /// <returns>The converted radian.</returns>
        public static double GradToRadian(double grad)
        {
            return grad * Constants.Grad;
        }

        /// <summary>
        /// Converts a radian (2*Pi-periodic) angle to a degree (360-periodic) angle.
        /// </summary>
        /// <param name="radian">The radian to convert.</param>
        /// <returns>The converted degree.</returns>
        public static double RadianToDegree(double radian)
        {
            return radian / Constants.Degree;
        }

        /// <summary>
        /// Converts a radian (2*Pi-periodic) angle to a grad (400-periodic) angle.
        /// </summary>
        /// <param name="radian">The radian to convert.</param>
        /// <returns>The converted grad.</returns>
        public static double RadianToGrad(double radian)
        {
            return radian / Constants.Grad;
        }


        /// <summary>
        /// Normalized Sinc function. sinc(x) = sin(pi*x)/(pi*x).
        /// </summary>
        public static double Sinc(double x)
        {
            double z = Math.PI*x;
            return z.AlmostEqual(0.0, 15) ? 1.0 : Math.Sin(z)/z;
        }


        /// <summary>
        /// Trigonometric Sine of an angle in radian, or opposite / hypotenuse.
        /// </summary>
        /// <param name="radian">The angle in radian.</param>
        /// <returns>The sine of the radian angle.</returns>
        public static double Sin(double radian)
        {
            return Math.Sin(radian);
        }

        /// <summary>
        /// Trigonometric Sine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The sine of the complex number.</returns>
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
        /// Trigonometric Cosine of an angle in radian, or adjacent / hypotenuse.
        /// </summary>
        /// <param name="radian">The angle in radian.</param>
        /// <returns>The cosine of an angle in radian.</returns>
        public static double Cos(double radian)
        {
            return Math.Cos(radian);
        }

        /// <summary>
        /// Trigonometric Cosine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The cosine of a complex number.</returns>
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
        /// Trigonometric Tangent of an angle in radian, or opposite / adjacent.
        /// </summary>
        /// <param name="radian">The angle in radian.</param>
        /// <returns>The tangent of the radian angle.</returns>
        public static double Tan(double radian)
        {
            return Math.Tan(radian);
        }

        /// <summary>
        /// Trigonometric Tangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The tangent of the complex number.</returns>
        public static Complex Tan(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Tan(value.Real), 0.0);
            }

            // tan(z) = - j*tanh(j*z)

            Complex z = Tanh(new Complex(-value.Imaginary, value.Real));
            return new Complex(z.Imaginary, -z.Real);
        }

        /// <summary>
        /// Trigonometric Cotangent of an angle in radian, or adjacent / opposite. Reciprocal of the tangent.
        /// </summary>
        /// <param name="radian">The angle in radian.</param>
        /// <returns>The cotangent of an angle in radian.</returns>
        public static double Cot(double radian)
        {
            return 1 / Math.Tan(radian);
        }

        /// <summary>
        /// Trigonometric Cotangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The cotangent of the complex number.</returns>
        public static Complex Cot(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Cot(value.Real), 0d);
            }

            // cot(z) = - j*coth(-j*z)

            Complex z = Coth(new Complex(value.Imaginary, -value.Real));
            return new Complex(z.Imaginary, -z.Real);
        }

        /// <summary>
        /// Trigonometric Secant of an angle in radian, or hypotenuse / adjacent. Reciprocal of the cosine.
        /// </summary>
        /// <param name="radian">The angle in radian.</param>
        /// <returns>The secant of the radian angle.</returns>
        public static double Sec(double radian)
        {
            return 1 / Math.Cos(radian);
        }

        /// <summary>
        /// Trigonometric Secant of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The secant of the complex number.</returns>
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
        /// Trigonometric Cosecant of an angle in radian, or hypotenuse / opposite. Reciprocal of the sine.
        /// </summary>
        /// <param name="radian">The angle in radian.</param>
        /// <returns>Cosecant of an angle in radian.</returns>
        public static double Csc(double radian)
        {
            return 1 / Math.Sin(radian);
        }

        /// <summary>
        /// Trigonometric Cosecant of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The cosecant of a complex number.</returns>
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
        /// Trigonometric principal Arc Sine in radian
        /// </summary>
        /// <param name="opposite">The opposite for a unit hypotenuse (i.e. opposite / hypotenuse).</param>
        /// <returns>The angle in radian.</returns>
        public static double Asin(double opposite)
        {
            return Math.Asin(opposite);
        }

        /// <summary>
        /// Trigonometric principal Arc Sine of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc sine of a complex number.</returns>
        public static Complex Asin(this Complex value)
        {
            if (value.Imaginary > 0 || value.Imaginary == 0d && value.Real < 0)
            {
                return -Asin(-value);
            }

            return -Complex.ImaginaryOne * ((1 - value.Square()).SquareRoot() + (Complex.ImaginaryOne * value)).Ln();
        }

        /// <summary>
        /// Trigonometric principal Arc Cosine in radian
        /// </summary>
        /// <param name="adjacent">The adjacent for a unit hypotenuse (i.e. adjacent / hypotenuse).</param>
        /// <returns>The angle in radian.</returns>
        public static double Acos(double adjacent)
        {
            return Math.Acos(adjacent);
        }

        /// <summary>
        /// Trigonometric principal Arc Cosine of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc cosine of a complex number.</returns>
        public static Complex Acos(this Complex value)
        {
            if (value.Imaginary < 0 || value.Imaginary == 0d && value.Real > 0)
            {
                return Constants.Pi - Acos(-value);
            }

            return -Complex.ImaginaryOne * (value + (Complex.ImaginaryOne * (1 - value.Square()).SquareRoot())).Ln();
        }

        /// <summary>
        /// Trigonometric principal Arc Tangent  in radian
        /// </summary>
        /// <param name="opposite">The opposite for a unit adjacent (i.e. opposite / adjacent).</param>
        /// <returns>The angle in radian.</returns>
        public static double Atan(double opposite)
        {
            return Math.Atan(opposite);
        }

        /// <summary>
        /// Trigonometric principal Arc Tangent of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc tangent of a complex number.</returns>
        public static Complex Atan(this Complex value)
        {
            var iz = new Complex(-value.Imaginary, value.Real); // I*this
            return new Complex(0, 0.5) * ((1 - iz).Ln() - (1 + iz).Ln());
        }

        /// <summary>
        /// Trigonometric principal Arc Cotangent in radian
        /// </summary>
        /// <param name="adjacent">The adjacent for a unit opposite (i.e. adjacent / opposite).</param>
        /// <returns>The angle in radian.</returns>
        public static double Acot(double adjacent)
        {
            return Math.Atan(1 / adjacent);
        }

        /// <summary>
        /// Trigonometric principal Arc Cotangent of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc cotangent of a complex number.</returns>
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
        /// Trigonometric principal Arc Secant in radian
        /// </summary>
        /// <param name="hypotenuse">The hypotenuse for a unit adjacent (i.e. hypotenuse / adjacent).</param>
        /// <returns>The angle in radian.</returns>
        public static double Asec(double hypotenuse)
        {
            return Math.Acos(1 / hypotenuse);
        }

        /// <summary>
        /// Trigonometric principal Arc Secant of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc secant of a complex number.</returns>
        public static Complex Asec(this Complex value)
        {
            var inv = 1 / value;
            return -Complex.ImaginaryOne * (inv + (Complex.ImaginaryOne * (1 - inv.Square()).SquareRoot())).Ln();
        }

        /// <summary>
        /// Trigonometric principal Arc Cosecant in radian
        /// </summary>
        /// <param name="hypotenuse">The hypotenuse for a unit opposite (i.e. hypotenuse / opposite).</param>
        /// <returns>The angle in radian.</returns>
        public static double Acsc(double hypotenuse)
        {
            return Math.Asin(1 / hypotenuse);
        }

        /// <summary>
        /// Trigonometric principal Arc Cosecant of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc cosecant of a complex number.</returns>
        public static Complex Acsc(this Complex value)
        {
            var inv = 1 / value;
            return -Complex.ImaginaryOne * ((Complex.ImaginaryOne * inv) + (1 - inv.Square()).SquareRoot()).Ln();
        }


        /// <summary>
        /// Hyperbolic Sine
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic sine of the angle.</returns>
        public static double Sinh(double angle)
        {
            return (Math.Exp(angle) - Math.Exp(-angle)) / 2;
        }

        /// <summary>
        /// Hyperbolic Sine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic sine of a complex number.</returns>
        public static Complex Sinh(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Sinh(value.Real), 0.0);
            }

            // sinh(x + j y) = sinh(x)*cos(y) + j*cosh(x)*sin(y)
            // if x > huge, sinh(x + jy) = sign(x)*exp(|x|)/2*cos(y) + j*exp(|x|)/2*sin(y)

            if (Math.Abs(value.Real) >= 22.0) // Taken from the msun library in FreeBSD
            {
                double h = Math.Exp(Math.Abs(value.Real)) * 0.5;
                return new Complex(
                    Math.Sign(value.Real)*h*Cos(value.Imaginary),
                    h*Sin(value.Imaginary));
            }

            return new Complex(
                Sinh(value.Real) * Cos(value.Imaginary),
                Cosh(value.Real) * Sin(value.Imaginary));
        }

        /// <summary>
        /// Hyperbolic Cosine
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic Cosine of the angle.</returns>
        public static double Cosh(double angle)
        {
            return (Math.Exp(angle) + Math.Exp(-angle)) / 2;
        }

        /// <summary>
        /// Hyperbolic Cosine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic cosine of a complex number.</returns>
        public static Complex Cosh(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Cosh(value.Real), 0.0);
            }

            // cosh(x + j*y) = cosh(x)*cos(y) + j*sinh(x)*sin(y)
            // if x > huge, cosh(x + j*y) = exp(|x|)/2*cos(y) + j*sign(x)*exp(|x|)/2*sin(y)

            if (Math.Abs(value.Real) >= 22.0) // Taken from the msun library in FreeBSD
            {
                double h = Math.Exp(Math.Abs(value.Real)) * 0.5;
                return new Complex(
                    h * Cos(value.Imaginary),
                    Math.Sign(value.Real) * h * Sin(value.Imaginary));
            }

            return new Complex(
                Cosh(value.Real) * Cos(value.Imaginary),
                Sinh(value.Real) * Sin(value.Imaginary));
        }

        /// <summary>
        /// Hyperbolic Tangent in radian
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic tangent of the angle.</returns>
        public static double Tanh(double angle)
        {
            if (angle > 19.1)
            {
                return 1.0;
            }

            if (angle < -19.1)
            {
                return -1;
            }

            var e1 = Math.Exp(angle);
            var e2 = Math.Exp(-angle);
            return (e1 - e2) / (e1 + e2);
        }

        /// <summary>
        /// Hyperbolic Tangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic tangent of a complex number.</returns>
        public static Complex Tanh(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Tanh(value.Real), 0.0);
            }

            // tanh(x + j*y) = (cosh(x)*sinh(x)/cos^2(y) + j*tan(y))/(1 + sinh^2(x)/cos^2(y))
            // if |x| > huge, tanh(z) = sign(x) + j*4*cos(y)*sin(y)*exp(-2*|x|)
            // if exp(-|x|) = 0, tanh(z) = sign(x)
            // if tan(y) = +/- oo or 1/cos^2(y) = 1 + tan^2(y) = oo, tanh(z) = cosh(x)/sinh(x)
            //
            // The algorithm is based on Kahan.

            if (Math.Abs(value.Real) >= 22.0) // Taken from the msun library in FreeBSD
            {
                double e = Math.Exp(-Math.Abs(value.Real));
                return e == 0.0
                    ? new Complex(Math.Sign(value.Real), 0.0)
                    : new Complex(Math.Sign(value.Real), 4.0 * Math.Cos(value.Imaginary) * Math.Sin(value.Imaginary) * e * e);
            }

            double tani = Tan(value.Imaginary);
            double beta = 1 + tani * tani; // beta = 1/cos^2(y) = 1 + t^2
            double sinhr = Sinh(value.Real);
            double coshr = Cosh(value.Real);

            if (double.IsInfinity(tani))
                return new Complex(coshr / sinhr, 0.0);

            double denom = 1.0 + beta * sinhr * sinhr;
            return new Complex(beta * coshr * sinhr / denom, tani / denom);
        }

        /// <summary>
        /// Hyperbolic Cotangent
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic cotangent of the angle.</returns>
        public static double Coth(double angle)
        {
            if (angle > 19.115)
            {
                return 1.0;
            }

            if (angle < -19.115)
            {
                return -1;
            }

            var e1 = Math.Exp(angle);
            var e2 = Math.Exp(-angle);
            return (e1 + e2) / (e1 - e2);
        }

        /// <summary>
        /// Hyperbolic Cotangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic cotangent of a complex number.</returns>
        public static Complex Coth(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Coth(value.Real), 0.0);
            }

            // Coth(z) = 1/tanh(z)

            return Complex.One / Tanh(value);
        }

        /// <summary>
        /// Hyperbolic Secant
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic secant of the angle.</returns>
        public static double Sech(double angle)
        {
            return 1 / Cosh(angle);
        }

        /// <summary>
        /// Hyperbolic Secant of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic secant of a complex number.</returns>
        public static Complex Sech(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Sech(value.Real), 0.0);
            }

            // sech(x + j*y) = (cosh(x)/cos(y) - j*sinh(x)*tan(y)/cos(y))/(1 + sinh^2(x)/cos^2(y))
            // if |x| > huge, sech(z) = 4*cosh(x)*cos(y)*exp(-2*|x|) - j*4*sinh(x)*tan(y)*cos(y)*exp(-2*|x|)
            // if exp(-|x|) = 0, sech(z) = 0
            // if tan(y) = +/- oo or 1/cos^2(y) = 1 + tan^2(y) = oo, sech(z) = -j*sign(tan(y))/sinh(x)
            //
            // The algorithm is based on Kahan.

            double tani = Tan(value.Imaginary);
            double cosi = Cos(value.Imaginary);
            double beta = 1.0 + tani * tani;
            double sinhr = Math.Sinh(value.Real);
            double coshr = Math.Cosh(value.Real);

            if (Math.Abs(value.Real) >= 22.0) // Taken from the msun library in FreeBSD
            {
                double e = Math.Exp(-Math.Abs(value.Real));
                return e == 0.0
                    ? new Complex(0, 0)
                    : new Complex(4.0 * coshr * cosi * e * e, -4.0 * sinhr * tani * cosi * e * e);
            }

            if (double.IsInfinity(tani))
            {
                return new Complex(0.0, -Math.Sign(tani) / sinhr);
            }

            double denom = 1.0 + beta * sinhr * sinhr;
            return new Complex(coshr / cosi / denom, -sinhr * tani / cosi / denom);
        }

        /// <summary>
        /// Hyperbolic Cosecant
        /// </summary>
        /// <param name="angle">The hyperbolic angle, i.e. the area of the hyperbolic sector.</param>
        /// <returns>The hyperbolic cosecant of the angle.</returns>
        public static double Csch(double angle)
        {
            return 1 / Sinh(angle);
        }

        /// <summary>
        /// Hyperbolic Cosecant of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic cosecant of a complex number.</returns>
        public static Complex Csch(this Complex value)
        {
            if (value.IsReal())
            {
                return new Complex(Csch(value.Real), 0.0);
            }

            // csch(x + j*y) = (sinh(x)*cot(y)/sin(y) - j*cosh(x)/sin(y))/(1 + sinh^2(x)/sin^2(y))
            // if |x| > huge, csch(z) = 4*sinh(x)*cot(y)*sin(y)*exp(-2*|x|) - j*4*cosh(x)*sin(y)*exp(-2*|x|)
            // if exp(-|x|) = 0, csch(z) = 0
            // if cot(y) = +/- oo or 1/sin^2(x) = 1 + cot^2(x) = oo, csch(z) = sign(cot(y))/sinh(x)
            //
            // The algorithm is based on Kahan.

            double coti = Cot(value.Imaginary);
            double sini = Sin(value.Imaginary);
            double beta = 1 + coti * coti;
            double sinhr = Sinh(value.Real);
            double coshr = Cosh(value.Real);

            if (Math.Abs(value.Real) >= 22.0) // Taken from the msun library in FreeBSD
            {
                double e = Math.Exp(-Math.Abs(value.Real));
                return e == 0.0
                    ? new Complex(0, 0)
                    : new Complex(4.0 * sinhr * coti * sini * e * e, -4.0 * coshr * sini * e * e);
            }

            if (double.IsInfinity(coti))
            {
                return new Complex(Math.Sign(coti) / sinhr, 0.0);
            }

            double denom = 1.0 + beta * sinhr * sinhr;
            return new Complex(sinhr * coti / sini / denom, -coshr / sini / denom);
        }


        /// <summary>
        /// Hyperbolic Area Sine
        /// </summary>
        /// <param name="value">The real value.</param>
        /// <returns>The hyperbolic angle, i.e. the area of its hyperbolic sector.</returns>
        public static double Asinh(double value)
        {
            // asinh(x) = Sign(x) * ln(|x| + sqrt(x*x + 1))
            // if |x| > huge, asinh(x) ~= Sign(x) * ln(2|x|)

            if (Math.Abs(value) >= 268435456.0) // 2^28, taken from freeBSD
                return Math.Sign(value) * (Math.Log(Math.Abs(value)) + Math.Log(2.0));

            return Math.Sign(value) * Math.Log(Math.Abs(value) + Math.Sqrt((value * value) + 1));
        }

        /// <summary>
        /// Hyperbolic Area Sine of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic arc sine of a complex number.</returns>
        public static Complex Asinh(this Complex value)
        {
            return (value + (value.Square() + 1).SquareRoot()).Ln();
        }

        /// <summary>
        /// Hyperbolic Area Cosine
        /// </summary>
        /// <param name="value">The real value.</param>
        /// <returns>The hyperbolic angle, i.e. the area of its hyperbolic sector.</returns>
        public static double Acosh(double value)
        {
            // acosh(x) = ln(x + sqrt(x*x - 1))
            // if |x| >= 2^28, acosh(x) ~ ln(x) + ln(2)

            if (Math.Abs(value) >= 268435456.0) // 2^28, taken from freeBSD
                return Math.Log(value) + Math.Log(2.0);

            return Math.Log(value + (Math.Sqrt(value - 1) * Math.Sqrt(value + 1)), Math.E);
        }

        /// <summary>
        /// Hyperbolic Area Cosine of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic arc cosine of a complex number.</returns>
        public static Complex Acosh(this Complex value)
        {
            return (value + ((value - 1).SquareRoot() * (value + 1).SquareRoot())).Ln();
        }

        /// <summary>
        /// Hyperbolic Area Tangent
        /// </summary>
        /// <param name="value">The real value.</param>
        /// <returns>The hyperbolic angle, i.e. the area of its hyperbolic sector.</returns>
        public static double Atanh(double value)
        {
            return 0.5 * Math.Log((1 + value) / (1 - value), Math.E);
        }

        /// <summary>
        /// Hyperbolic Area Tangent of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic arc tangent of a complex number.</returns>
        public static Complex Atanh(this Complex value)
        {
            return 0.5 * ((1 + value).Ln() - (1 - value).Ln());
        }

        /// <summary>
        /// Hyperbolic Area Cotangent
        /// </summary>
        /// <param name="value">The real value.</param>
        /// <returns>The hyperbolic angle, i.e. the area of its hyperbolic sector.</returns>
        public static double Acoth(double value)
        {
            return 0.5 * Math.Log((value + 1) / (value - 1), Math.E);
        }

        /// <summary>
        /// Hyperbolic Area Cotangent of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic arc cotangent of a complex number.</returns>
        public static Complex Acoth(this Complex value)
        {
            var inv = 1.0 / value;
            return 0.5 * ((1.0 + inv).Ln() - (1.0 - inv).Ln());
        }

        /// <summary>
        /// Hyperbolic Area Secant
        /// </summary>
        /// <param name="value">The real value.</param>
        /// <returns>The hyperbolic angle, i.e. the area of its hyperbolic sector.</returns>
        public static double Asech(double value)
        {
            return Acosh(1 / value);
        }

        /// <summary>
        /// Hyperbolic Area Secant of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic arc secant of a complex number.</returns>
        public static Complex Asech(this Complex value)
        {
            var inv = 1 / value;
            return (inv + ((inv - 1).SquareRoot() * (inv + 1).SquareRoot())).Ln();
        }

        /// <summary>
        /// Hyperbolic Area Cosecant
        /// </summary>
        /// <param name="value">The real value.</param>
        /// <returns>The hyperbolic angle, i.e. the area of its hyperbolic sector.</returns>
        public static double Acsch(double value)
        {
            return Asinh(1 / value);
        }

        /// <summary>
        /// Hyperbolic Area Cosecant of this <c>Complex</c> number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic arc cosecant of a complex number.</returns>
        public static Complex Acsch(this Complex value)
        {
            var inv = 1 / value;
            return (inv + (inv.Square() + 1).SquareRoot()).Ln();
        }
    }
}
