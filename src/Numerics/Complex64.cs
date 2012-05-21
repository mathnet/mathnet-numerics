// <copyright file="Complex64.cs" company="Math.NET">
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

using MathNet.Numerics;

namespace System.Numerics
{
#if !SYSNUMERICS
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using MathNet.Numerics.Properties;

    /// <summary>
    /// 64-bit Complex numbers class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class <c>Complex</c> provides all elementary operations
    /// on complex numbers. All the operators <c>+</c>, <c>-</c>,
    /// <c>*</c>, <c>/</c>, <c>==</c>, <c>!=</c> are defined in the
    /// canonical way. Additional complex trigonometric functions 
    /// are also provided. Note that the <c>Complex</c> structures 
    /// has two special constant values <see cref="Complex.NaN"/> and 
    /// <see cref="Complex.Infinity"/>.
    /// </para>
    /// <para>
    /// In order to avoid possible ambiguities resulting from a 
    /// <c>Complex(double, double)</c> constructor, the static methods 
    /// <see cref="Complex.WithRealImaginary"/> and <see cref="Complex.WithModulusArgument"/>
    /// are provided instead.
    /// </para>
    /// <para>
    /// <code>
    /// Complex x = Complex.FromRealImaginary(1d, 2d);
    /// Complex y = Complex.FromModulusArgument(1d, Math.Pi);
    /// Complex z = (x + y) / (x - y);
    /// </code>
    /// </para>
    /// <para>
    /// For mathematical details about complex numbers, please
    /// have a look at the <a href="http://en.wikipedia.org/wiki/Complex_number">
    /// Wikipedia</a>
    /// </para>
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Complex : IFormattable, IEquatable<Complex>, IPrecisionSupport<Complex>
    {
    #region fields

        /// <summary>
        /// Represents imaginary unit number.
        /// </summary>
        private static readonly Complex _i = new Complex(0, 1);

        /// <summary>
        /// Represents a infinite complex number
        /// </summary>
        private static readonly Complex _infinity = new Complex(double.PositiveInfinity, double.PositiveInfinity);

        /// <summary>
        /// Represents not-a-number.
        /// </summary>
        private static readonly Complex _nan = new Complex(double.NaN, double.NaN);

        /// <summary>
        /// Representing the one value.
        /// </summary>
        private static readonly Complex _one = new Complex(1.0f, 0.0d);

        /// <summary>
        /// Representing the zero value.
        /// </summary>
        private static readonly Complex _zero = new Complex(0.0d, 0.0d);

        /// <summary>
        /// The real component of the complex number.
        /// </summary>
        private readonly double _real;

        /// <summary>
        /// The imaginary component of the complex number.
        /// </summary>
        private readonly double _imag;

        #endregion fields

    #region Constructor

        /// <summary>
        /// Initializes a new instance of the Complex structure with the given real
        /// and imaginary parts.
        /// </summary>
        /// <param name="real">
        /// The value for the real component.
        /// </param>
        /// <param name="imaginary">
        /// The value for the imaginary component.
        /// </param>
#if !PORTABLE
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
        public Complex(double real, double imaginary)
        {
            _real = real;
            _imag = imaginary;
        }

        #endregion

    #region Properties

        /// <summary>
        /// Gets a value representing the infinity value. This field is constant.
        /// </summary>
        /// <value>The infinity.</value>
        /// <remarks>
        /// The semantic associated to this value is a <c>Complex</c> of
        /// infinite real and imaginary part. If you need more formal complex
        /// number handling (according to the Riemann Sphere and the extended
        /// complex plane C*, or using directed infinity) please check out the
        /// alternative Math.NET symbolics packages instead.
        /// </remarks>
        /// <value>A value representing the infinity value.</value>
        public static Complex Infinity
        {
            get
            {
                return _infinity;
            }
        }

        /// <summary>
        /// Gets a value representing not-a-number. This field is constant.
        /// </summary>
        /// <value>A value representing not-a-number.</value>
        public static Complex NaN
        {
            get
            {
                return _nan;
            }
        }

        /// <summary>
        /// Gets a value representing the imaginary unit number. This field is constant.
        /// </summary>
        /// <value>A value representing the imaginary unit number.</value>
        public static Complex ImaginaryOne
        {
            get
            {
                return _i;
            }
        }

        /// <summary>
        /// Gets a value representing the zero value. This field is constant.
        /// </summary>
        /// <value>A value representing the zero value.</value>
        public static Complex Zero
        {
            get
            {
                return new Complex(0.0d, 0.0d);
            }
        }

        /// <summary>
        /// Gets a value representing the <c>1</c> value. This field is constant.
        /// </summary>
        /// <value>A value representing the <c>1</c> value.</value>
        public static Complex One
        {
            get
            {
                return _one;
            }
        }

        #endregion Properties

        /// <summary>
        /// Gets the real component of the complex number.
        /// </summary>
        /// <value>The real component of the complex number.</value>
        public double Real
        {
#if !PORTABLE
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
            get
            {
                return _real;
            }
        }

        /// <summary>
        /// Gets the real imaginary component of the complex number.
        /// </summary>
        /// <value>The real imaginary component of the complex number.</value>
        public double Imaginary
        {
#if !PORTABLE
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
            get
            {
                return _imag;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <c>Complex</c> is zero.
        /// </summary>
        /// <returns><c>true</c> if this instance is zero; otherwise, <c>false</c>.</returns>
        public bool IsZero()
        {
            return _real == 0.0d && _imag == 0.0d;
        }

        /// <summary>
        /// Gets a value indicating whether the <c>Complex</c> is one.
        /// </summary>
        /// <returns><c>true</c> if this instance is one; otherwise, <c>false</c>.</returns>
        public bool IsOne()
        {
            return _real == 1.0d && _imag == 0.0d;
        }

        /// <summary>
        /// Gets a value indicating whether the <c>Complex</c> is the imaginary unit.
        /// </summary>
        /// <returns><c>true</c> if this instance is ImaginaryOne; otherwise, <c>false</c>.</returns>
        public bool IsImaginaryOne()
        {
            return _real == 0.0d && _imag == 1.0d;
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c>evaluates
        /// to a value that is not a number.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is <see cref="NaN"/>; otherwise,
        /// <c>false</c>.
        /// </returns>
        public bool IsNaN()
        {
            return double.IsNaN(_real) || double.IsNaN(_imag);
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c> evaluates to an
        /// infinite value.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance is infinite; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// True if it either evaluates to a complex infinity
        /// or to a directed infinity.
        /// </remarks>
        public bool IsInfinity()
        {
            return double.IsInfinity(_real) || double.IsInfinity(_imag);
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c> is real.
        /// </summary>
        /// <returns><c>true</c> if this instance is a real number; otherwise, <c>false</c>.</returns>
        public bool IsReal()
        {
            return _imag == 0.0d;
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c> is real and not negative, that is &gt;= 0.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance is real nonnegative number; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRealNonNegative()
        {
            return _imag == 0.0d && _real >= 0;
        }

        /// <summary>
        /// Gets the conjugate of this <c>Complex</c>.
        /// </summary>
        /// <remarks>
        /// The semantic of <i>setting the conjugate</i> is such that
        /// <code>
        /// // a, b of type Complex
        /// a.Conjugate = b;
        /// </code>
        /// is equivalent to
        /// <code>
        /// // a, b of type Complex
        /// a = b.Conjugate
        /// </code>
        /// </remarks>
        /// <returns>The conjugate of this <c>Complex</c></returns>
        public Complex Conjugate()
        {
            return new Complex(_real, -_imag);
        }

        /// <summary>
        /// Gets the magnitude or modulus of this <c>Complex</c>.
        /// </summary>
        /// <returns>The magnitude or modulus of this <c>Complex</c></returns>
        /// <seealso cref="Phase"/>
        public double Magnitude
        {
            get
            {
                return Math.Sqrt((_real * _real) + (_imag * _imag));
            }
        }

        /// <summary>
        /// Gets the squared magnitude of this <c>Complex</c>.
        /// </summary>
        /// <returns>The squared magnitude of this <c>Complex</c></returns>
        public double MagnitudeSquared
        {
            get
            {
                return (_real * _real) + (_imag * _imag);
            }
        }

        /// <summary>
        /// Gets the phase or argument of this <c>Complex</c>.
        /// </summary>
        /// <remarks>
        /// Phase always returns a value bigger than negative Pi and
        /// smaller or equal to Pi. If this <c>Complex</c> is zero, the Complex
        /// is assumed to be positive real with an argument of zero.
        /// </remarks>
        /// <returns>The phase or argument of this <c>Complex</c></returns>
        public double Phase
        {
            get
            {
                if (IsReal() && _real < 0)
                {
                    return Math.PI;
                }

                return IsRealNonNegative() ? 0.0d : Math.Atan2(_imag, _real);
            }
        }

        /// <summary>
        /// Gets the unity of this complex (same argument, but on the unit circle; exp(I*arg))
        /// </summary>
        /// <returns>The unity of this <c>Complex</c>.</returns>
        public Complex Sign
        {
            get
            {
                if (double.IsPositiveInfinity(_real) && double.IsPositiveInfinity(_imag))
                {
                    return new Complex(Constants.Sqrt1Over2, Constants.Sqrt1Over2);
                }

                if (double.IsPositiveInfinity(_real) && double.IsNegativeInfinity(_imag))
                {
                    return new Complex(Constants.Sqrt1Over2, -Constants.Sqrt1Over2);
                }

                if (double.IsNegativeInfinity(_real) && double.IsPositiveInfinity(_imag))
                {
                    return new Complex(-Constants.Sqrt1Over2, -Constants.Sqrt1Over2);
                }

                if (double.IsNegativeInfinity(_real) && double.IsNegativeInfinity(_imag))
                {
                    return new Complex(-Constants.Sqrt1Over2, Constants.Sqrt1Over2);
                }

                // don't replace this with "Magnitude"!
                var mod = SpecialFunctions.Hypotenuse(_real, _imag);
                if (mod == 0.0d)
                {
                    return Zero;
                }

                return new Complex(_real / mod, _imag / mod);
            }
        }

    #region Exponential Functions

        /// <summary>
        /// Exponential of this <c>Complex</c> (exp(x), E^x).
        /// </summary>
        /// <returns>
        /// The exponential of this complex number.
        /// </returns>
        public Complex Exponential()
        {
            var exp = Math.Exp(_real);
            if (IsReal())
            {
                return new Complex(exp, 0.0d);
            }

            return new Complex(exp * Trig.Cosine(_imag), exp * Trig.Sine(_imag));
        }

        /// <summary>
        /// Natural Logarithm of this <c>Complex</c> (Base E).
        /// </summary>
        /// <returns>
        /// The natural logarithm of this complex number.
        /// </returns>
        public Complex NaturalLogarithm()
        {
            if (IsRealNonNegative())
            {
                return new Complex(Math.Log(_real), 0.0d);
            }

            return new Complex(0.5d * Math.Log(MagnitudeSquared), Phase);
        }

        /// <summary>
        /// Raise this <c>Complex</c> to the given value.
        /// </summary>
        /// <param name="exponent">
        /// The exponent.
        /// </param>
        /// <returns>
        /// The complex number raised to the given exponent.
        /// </returns>
        public Complex Power(Complex exponent)
        {
            if (IsZero())
            {
                if (exponent.IsZero())
                {
                    return One;
                }

                if (exponent.Real > 0.0d)
                {
                    return Zero;
                }

                if (exponent.Real < 0)
                {
                    if (exponent.Imaginary == 0.0d)
                    {
                        return new Complex(double.PositiveInfinity, 0.0d);
                    }

                    return new Complex(double.PositiveInfinity, double.PositiveInfinity);
                }

                return NaN;
            }

            return (exponent * NaturalLogarithm()).Exponential();
        }

        /// <summary>
        /// Raise this <c>Complex</c> to the inverse of the given value.
        /// </summary>
        /// <param name="rootExponent">
        /// The root exponent.
        /// </param>
        /// <returns>
        /// The complex raised to the inverse of the given exponent.
        /// </returns>
        public Complex Root(Complex rootExponent)
        {
            return Power(1 / rootExponent);
        }

        /// <summary>
        /// The Square (power 2) of this <c>Complex</c>
        /// </summary>
        /// <returns>
        /// The square of this complex number.
        /// </returns>
        public Complex Square()
        {
            if (IsReal())
            {
                return new Complex(_real * _real, 0.0d);
            }

            return new Complex((_real * _real) - (_imag * _imag), 2 * _real * _imag);
        }

        /// <summary>
        /// The Square Root (power 1/2) of this <c>Complex</c>
        /// </summary>
        /// <returns>
        /// The square root of this complex number.
        /// </returns>
        public Complex SquareRoot()
        {
            if (IsRealNonNegative())
            {
                return new Complex(Math.Sqrt(_real), 0.0d);
            }

            Complex result;

            var absReal = Math.Abs(Real);
            var absImag = Math.Abs(Imaginary);
            double w;
            if (absReal >= absImag)
            {
                var ratio = Imaginary / Real;
                w = Math.Sqrt(absReal) * Math.Sqrt(0.5 * (1.0d + Math.Sqrt(1.0d + (ratio * ratio))));
            }
            else
            {
                var ratio = Real / Imaginary;
                w = Math.Sqrt(absImag) * Math.Sqrt(0.5 * (Math.Abs(ratio) + Math.Sqrt(1.0d + (ratio * ratio))));
            }

            if (Real >= 0.0d)
            {
                result = new Complex(w, Imaginary / (2.0d * w));
            }
            else if (Imaginary >= 0.0d)
            {
                result = new Complex(absImag / (2.0 * w), w);
            }
            else
            {
                result = new Complex(absImag / (2.0 * w), -w);
            }

            return result;
        }

        #endregion

    #region Static Initializers

        /// <summary>
        /// Constructs a <c>Complex</c> from its real
        /// and imaginary parts.
        /// </summary>
        /// <param name="real">
        /// The value for the real component.
        /// </param>
        /// <param name="imaginary">
        /// The value for the imaginary component.
        /// </param>
        /// <returns>
        /// A new <c>Complex</c> with the given values.
        /// </returns>
        public static Complex WithRealImaginary(double real, double imaginary)
        {
            return new Complex(real, imaginary);
        }

        /// <summary>
        /// Constructs a <c>Complex</c> from its modulus and
        /// argument.
        /// </summary>
        /// <param name="modulus">
        /// Must be non-negative.
        /// </param>
        /// <param name="argument">
        /// Real number.
        /// </param>
        /// <returns>
        /// A new <c>Complex</c> from the given values.
        /// </returns>
        public static Complex WithModulusArgument(double modulus, double argument)
        {
            if (modulus < 0.0d)
            {
                throw new ArgumentOutOfRangeException("modulus", Resources.ArgumentNotNegative);
            }

            return new Complex(modulus * Math.Cos(argument), modulus * Math.Sin(argument));
        }

        #endregion

    #region IFormattable Members

        /// <summary>
        /// A string representation of this complex number.
        /// </summary>
        /// <returns>
        /// The string representation of this complex number.
        /// </returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>
        /// A string representation of this complex number.
        /// </summary>
        /// <returns>
        /// The string representation of this complex number formatted as specified by the
        /// format string.
        /// </returns>
        /// <param name="format">
        /// A format specification.
        /// </param>
        public string ToString(string format)
        {
            return ToString(format, null);
        }

        /// <summary>
        /// A string representation of this complex number.
        /// </summary>
        /// <returns>
        /// The string representation of this complex number formatted as specified by the
        /// format provider.
        /// </returns>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        /// <summary>
        /// A string representation of this complex number.
        /// </summary>
        /// <returns>
        /// The string representation of this complex number formatted as specified by the
        /// format string and format provider.
        /// </returns>
        /// <exception cref="FormatException">
        /// if the n, is not a number.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// if s, is <see langword="null"/>.
        /// </exception>
        /// <param name="format">
        /// A format specification.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            var ret = new StringBuilder();
            ret.Append("(").Append(_real.ToString(format, formatProvider)).Append(", ").Append(_imag.ToString(format, formatProvider)).Append(")");
            return ret.ToString();
        }

        #endregion

    #region IEquatable<Complex> Members

        /// <summary>
        /// Checks if two complex numbers are equal. Two complex numbers are equal if their
        /// corresponding real and imaginary components are equal.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the two objects are the same object, or if their corresponding
        /// real and imaginary components are equal, <c>false</c> otherwise.
        /// </returns>
        /// <param name="other">
        /// The complex number to compare to with.
        /// </param>
        public bool Equals(Complex other)
        {
            if (IsNaN() || other.IsNaN())
            {
                return false;
            }

            if (IsInfinity() && other.IsInfinity())
            {
                return true;
            }

            return _real.AlmostEqual(other._real) && _imag.AlmostEqual(other._imag);
        }

        /// <summary>
        /// The hash code for the complex number.
        /// </summary>
        /// <returns>
        /// The hash code of the complex number.
        /// </returns>
        /// <remarks>
        /// The hash code is calculated as
        /// System.Math.Exp(ComplexMath.Absolute(complexNumber)).
        /// </remarks>
        public override int GetHashCode()
        {
            return _real.GetHashCode() ^ (-_imag.GetHashCode());
        }

        /// <summary>
        /// Checks if two complex numbers are equal. Two complex numbers are equal if their
        /// corresponding real and imaginary components are equal.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the two objects are the same object, or if their corresponding
        /// real and imaginary components are equal, <c>false</c> otherwise.
        /// </returns>
        /// <param name="obj">
        /// The complex number to compare to with.
        /// </param>
        public override bool Equals(object obj)
        {
            return (obj is Complex) && Equals((Complex)obj);
        }

        #endregion

    #region Operators

        /// <summary>
        /// Equality test.
        /// </summary>
        /// <param name="complex1">One of complex numbers to compare.</param>
        /// <param name="complex2">The other complex numbers to compare.</param>
        /// <returns><c>true</c> if the real and imaginary components of the two complex numbers are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Complex complex1, Complex complex2)
        {
            return complex1.Equals(complex2);
        }

        /// <summary>
        /// Inequality test.
        /// </summary>
        /// <param name="complex1">One of complex numbers to compare.</param>
        /// <param name="complex2">The other complex numbers to compare.</param>
        /// <returns><c>true</c> if the real or imaginary components of the two complex numbers are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Complex complex1, Complex complex2)
        {
            return !complex1.Equals(complex2);
        }

        /// <summary>
        /// Unary addition.
        /// </summary>
        /// <param name="summand">The complex number to operate on.</param>
        /// <returns>Returns the same complex number.</returns>
        public static Complex operator +(Complex summand)
        {
            return summand;
        }

        /// <summary>
        /// Unary minus.
        /// </summary>
        /// <param name="subtrahend">The complex number to operate on.</param>
        /// <returns>The negated value of the <paramref name="subtrahend"/>.</returns>
        public static Complex operator -(Complex subtrahend)
        {
            return new Complex(-subtrahend._real, -subtrahend._imag);
        }

        /// <summary>Addition operator. Adds two complex numbers together.</summary>
        /// <returns>The result of the addition.</returns>
        /// <param name="summand1">One of the complex numbers to add.</param>
        /// <param name="summand2">The other complex numbers to add.</param>
        public static Complex operator +(Complex summand1, Complex summand2)
        {
            return new Complex(summand1._real + summand2._real, summand1._imag + summand2._imag);
        }

        /// <summary>Subtraction operator. Subtracts two complex numbers.</summary>
        /// <returns>The result of the subtraction.</returns>
        /// <param name="minuend">The complex number to subtract from.</param>
        /// <param name="subtrahend">The complex number to subtract.</param>
        public static Complex operator -(Complex minuend, Complex subtrahend)
        {
            return new Complex(minuend._real - subtrahend._real, minuend._imag - subtrahend._imag);
        }

        /// <summary>Addition operator. Adds a complex number and double together.</summary>
        /// <returns>The result of the addition.</returns>
        /// <param name="summand1">The complex numbers to add.</param>
        /// <param name="summand2">The double value to add.</param>
        public static Complex operator +(Complex summand1, double summand2)
        {
            return new Complex(summand1._real + summand2, summand1._imag);
        }

        /// <summary>Subtraction operator. Subtracts double value from a complex value.</summary>
        /// <returns>The result of the subtraction.</returns>
        /// <param name="minuend">The complex number to subtract from.</param>
        /// <param name="subtrahend">The double value to subtract.</param>
        public static Complex operator -(Complex minuend, double subtrahend)
        {
            return new Complex(minuend._real - subtrahend, minuend._imag);
        }

        /// <summary>Addition operator. Adds a complex number and double together.</summary>
        /// <returns>The result of the addition.</returns>
        /// <param name="summand1">The double value to add.</param>
        /// <param name="summand2">The complex numbers to add.</param>
        public static Complex operator +(double summand1, Complex summand2)
        {
            return new Complex(summand2._real + summand1, summand2._imag);
        }

        /// <summary>Subtraction operator. Subtracts complex value from a double value.</summary>
        /// <returns>The result of the subtraction.</returns>
        /// <param name="minuend">The double vale to subtract from.</param>
        /// <param name="subtrahend">The complex value to subtract.</param>
        public static Complex operator -(double minuend, Complex subtrahend)
        {
            return new Complex(minuend - subtrahend._real, -subtrahend._imag);
        }

        /// <summary>Multiplication operator. Multiplies two complex numbers.</summary>
        /// <returns>The result of the multiplication.</returns>
        /// <param name="multiplicand">One of the complex numbers to multiply.</param>
        /// <param name="multiplier">The other complex number to multiply.</param>
        public static Complex operator *(Complex multiplicand, Complex multiplier)
        {
            return new Complex(
                (multiplicand._real * multiplier._real) - (multiplicand._imag * multiplier._imag),
                (multiplicand._real * multiplier._imag) + (multiplicand._imag * multiplier._real));
        }

        /// <summary>Multiplication operator. Multiplies a complex number with a double value.</summary>
        /// <returns>The result of the multiplication.</returns>
        /// <param name="multiplicand">The double value to multiply.</param>
        /// <param name="multiplier">The complex number to multiply.</param>
        public static Complex operator *(double multiplicand, Complex multiplier)
        {
            return new Complex(multiplier._real * multiplicand, multiplier._imag * multiplicand);
        }

        /// <summary>Multiplication operator. Multiplies a complex number with a double value.</summary>
        /// <returns>The result of the multiplication.</returns>
        /// <param name="multiplicand">The complex number to multiply.</param>
        /// <param name="multiplier">The double value to multiply.</param>
        public static Complex operator *(Complex multiplicand, double multiplier)
        {
            return new Complex(multiplicand._real * multiplier, multiplicand._imag * multiplier);
        }

        /// <summary>Division operator. Divides a complex number by another.</summary>
        /// <returns>The result of the division.</returns>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        public static Complex operator /(Complex dividend, Complex divisor)
        {
            if (dividend.IsZero() && divisor.IsZero())
            {
                return NaN;
            }

            if (divisor.IsZero())
            {
                return Infinity;
            }

            var modSquared = divisor.MagnitudeSquared;
            return new Complex(
                ((dividend._real * divisor._real) + (dividend._imag * divisor._imag)) / modSquared,
                ((dividend._imag * divisor._real) - (dividend._real * divisor._imag)) / modSquared);
        }

        /// <summary>Division operator. Divides a double value by a complex number.</summary>
        /// <returns>The result of the division.</returns>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        public static Complex operator /(double dividend, Complex divisor)
        {
            if (dividend == 0.0d && divisor.IsZero())
            {
                return NaN;
            }

            if (divisor.IsZero())
            {
                return Infinity;
            }

            var zmod = divisor.MagnitudeSquared;
            return new Complex(dividend * divisor._real / zmod, -dividend * divisor._imag / zmod);
        }

        /// <summary>Division operator. Divides a complex number by a double value.</summary>
        /// <returns>The result of the division.</returns>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        public static Complex operator /(Complex dividend, double divisor)
        {
            if (dividend.IsZero() && divisor == 0.0d)
            {
                return NaN;
            }

            if (divisor == 0.0d)
            {
                return Infinity;
            }

            return new Complex(dividend._real / divisor, dividend._imag / divisor);
        }

        /// <summary>
        /// Unary addition.
        /// </summary>
        /// <returns>
        /// Returns the same complex number.
        /// </returns>
#if !PORTABLE
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
        public Complex Plus()
        {
            return this;
        }

        /// <summary>
        /// Unary minus.
        /// </summary>
        /// <returns>
        /// The negated value of this complex number.
        /// </returns>
#if !PORTABLE
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
        public Complex Negate()
        {
            return -this;
        }

        /// <summary>
        /// Adds a complex number to this one.
        /// </summary>
        /// <returns>
        /// The result of the addition.
        /// </returns>
        /// <param name="other">
        /// The other complex number to add.
        /// </param>
#if !PORTABLE
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
        public Complex Add(Complex other)
        {
            return this + other;
        }

        /// <summary>
        /// Subtracts a complex number from this one.
        /// </summary>
        /// <returns>
        /// The result of the subtraction.
        /// </returns>
        /// <param name="other">
        /// The other complex number to subtract from this one.
        /// </param>
#if !PORTABLE
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
        public Complex Subtract(Complex other)
        {
            return this - other;
        }

        /// <summary>
        /// Multiplies this complex number with this one.
        /// </summary>
        /// <returns>
        /// The result of the multiplication.
        /// </returns>
        /// <param name="multiplier">
        /// The complex number to multiply.
        /// </param>
#if !PORTABLE
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
        public Complex Multiply(Complex multiplier)
        {
            return this * multiplier;
        }

        /// <summary>
        /// Divides this complex number by another.
        /// </summary>
        /// <returns>
        /// The result of the division.
        /// </returns>
        /// <param name="divisor">
        /// The divisor.
        /// </param>
#if !PORTABLE
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
#endif
        public Complex Divide(Complex divisor)
        {
            return this / divisor;
        }

        #endregion

    #region IPrecisionSupport<Complex>

        /// <summary>
        /// Returns a Norm of a value of this type, which is appropriate for measuring how
        /// close this value is to zero.
        /// </summary>
        /// <returns>
        /// A norm of this value.
        /// </returns>
        double IPrecisionSupport<Complex>.Norm()
        {
            return MagnitudeSquared;
        }

        /// <summary>
        /// Returns a Norm of the difference of two values of this type, which is
        /// appropriate for measuring how close together these two values are.
        /// </summary>
        /// <param name="otherValue">
        /// The value to compare with.
        /// </param>
        /// <returns>
        /// A norm of the difference between this and the other value.
        /// </returns>
        double IPrecisionSupport<Complex>.NormOfDifference(Complex otherValue)
        {
            return (this - otherValue).MagnitudeSquared;
        }

        #endregion

    #region Parse Functions

        /// <summary>
        /// Creates a complex number based on a string. The string can be in the
        /// following formats (without the quotes): 'n', 'ni', 'n +/- ni',
        /// 'ni +/- n', 'n,n', 'n,ni,' '(n,n)', or '(n,ni)', where n is a double.
        /// </summary>
        /// <returns>
        /// A complex number containing the value specified by the given string.
        /// </returns>
        /// <param name="value">
        /// The string to parse.
        /// </param>
        public static Complex Parse(string value)
        {
            return Parse(value, null);
        }

        /// <summary>
        /// Creates a complex number based on a string. The string can be in the
        /// following formats (without the quotes): 'n', 'ni', 'n +/- ni',
        /// 'ni +/- n', 'n,n', 'n,ni,' '(n,n)', or '(n,ni)', where n is a double.
        /// </summary>
        /// <returns>
        /// A complex number containing the value specified by the given string.
        /// </returns>
        /// <param name="value">
        /// the string to parse.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific
        /// formatting information.
        /// </param>
        public static Complex Parse(string value, IFormatProvider formatProvider)
        {
            if (value == null)
            {
                throw new ArgumentNullException(value);
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                throw new FormatException();
            }

            // strip out parens
            if (value.StartsWith("(", StringComparison.Ordinal))
            {
                if (!value.EndsWith(")", StringComparison.Ordinal))
                {
                    throw new FormatException();
                }

                value = value.Substring(1, value.Length - 2).Trim();
            }

            // keywords
            var numberFormatInfo = formatProvider.GetNumberFormatInfo();
            var textInfo = formatProvider.GetTextInfo();
            var keywords =
                new[]
                {
                    textInfo.ListSeparator, numberFormatInfo.NaNSymbol, 
                    numberFormatInfo.NegativeInfinitySymbol, numberFormatInfo.PositiveInfinitySymbol, 
                    "+", "-", "i", "j"
                };

            // lexing
            var tokens = new LinkedList<string>();
            GlobalizationHelper.Tokenize(tokens.AddFirst(value), keywords, 0);
            var token = tokens.First;

            // parse the left part
            bool isLeftPartImaginary;
            var leftPart = ParsePart(ref token, out isLeftPartImaginary, formatProvider);
            if (token == null)
            {
                return isLeftPartImaginary ? new Complex(0, leftPart) : new Complex(leftPart, 0);
            }

            // parse the right part
            if (token.Value == textInfo.ListSeparator)
            {
                // format: real,imag
                token = token.Next;

                if (isLeftPartImaginary)
                {
                    // left must not contain 'i', right doesn't matter.
                    throw new FormatException();
                }

                bool isRightPartImaginary;
                var rightPart = ParsePart(ref token, out isRightPartImaginary, formatProvider);

                return new Complex(leftPart, rightPart);
            }
            else
            {
                // format: real + imag
                bool isRightPartImaginary;
                var rightPart = ParsePart(ref token, out isRightPartImaginary, formatProvider);

                if (!(isLeftPartImaginary ^ isRightPartImaginary))
                {
                    // either left or right part must contain 'i', but not both.
                    throw new FormatException();
                }

                return isLeftPartImaginary ? new Complex(rightPart, leftPart) : new Complex(leftPart, rightPart);
            }
        }

        /// <summary>
        /// Parse a part (real or complex) from a complex number.
        /// </summary>
        /// <param name="token">Start Token.</param>
        /// <param name="imaginary">Is set to <c>true</c> if the part identified itself as being imaginary.</param>
        /// <param name="format">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific
        /// formatting information.
        /// </param>
        /// <returns>Resulting part as double.</returns>
        /// <exception cref="FormatException"/>
        private static double ParsePart(ref LinkedListNode<string> token, out bool imaginary, IFormatProvider format)
        {
            imaginary = false;
            if (token == null)
            {
                throw new FormatException();
            }

            // handle prefix modifiers
            if (token.Value == "+")
            {
                token = token.Next;

                if (token == null)
                {
                    throw new FormatException();
                }
            }

            var negative = false;
            if (token.Value == "-")
            {
                negative = true;
                token = token.Next;

                if (token == null)
                {
                    throw new FormatException();
                }
            }

            // handle prefix imaginary symbol
            if (String.Compare(token.Value, "i", StringComparison.OrdinalIgnoreCase) == 0
                || String.Compare(token.Value, "j", StringComparison.OrdinalIgnoreCase) == 0)
            {
                imaginary = true;
                token = token.Next;

                if (token == null)
                {
                    return negative ? -1 : 1;
                }
            }

#if PORTABLE
            var value = GlobalizationHelper.ParseSingle(ref token);
#else
            var value = GlobalizationHelper.ParseSingle(ref token, format.GetCultureInfo());
#endif

            // handle suffix imaginary symbol
            if (token != null && (String.Compare(token.Value, "i", StringComparison.OrdinalIgnoreCase) == 0
                                  || String.Compare(token.Value, "j", StringComparison.OrdinalIgnoreCase) == 0))
            {
                if (imaginary)
                {
                    // only one time allowed: either prefix or suffix, or neither.
                    throw new FormatException();
                }

                imaginary = true;
                token = token.Next;
            }

            return negative ? -value : value;
        }

        /// <summary>
        /// Converts the string representation of a complex number to a single-precision complex number equivalent. 
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">
        /// A string containing a complex number to convert. 
        /// </param>
        /// <param name="result">
        /// The parsed value.
        /// </param>
        /// <returns>
        /// If the conversion succeeds, the result will contain a complex number equivalent to value. 
        /// Otherwise the result will contain complex32.Zero.  This parameter is passed uninitialized
        /// </returns>
        public static bool TryParse(string value, out Complex result)
        {
            return TryParse(value, null, out result);
        }

        /// <summary>
        /// Converts the string representation of a complex number to single-precision complex number equivalent.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">
        /// A string containing a complex number to convert.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information about value.
        /// </param>
        /// <param name="result">
        /// The parsed value.
        /// </param>
        /// <returns>
        /// If the conversion succeeds, the result will contain a complex number equivalent to value.
        /// Otherwise the result will contain complex32.Zero.  This parameter is passed uninitialized
        /// </returns>
        public static bool TryParse(string value, IFormatProvider formatProvider, out Complex result)
        {
            bool ret;
            try
            {
                result = Parse(value, formatProvider);
                ret = true;
            }
            catch (ArgumentNullException)
            {
                result = _zero;
                ret = false;
            }
            catch (FormatException)
            {
                result = _zero;
                ret = false;
            }

            return ret;
        }

        #endregion

    #region Conversion

        /// <summary>
        /// Explicit conversion of a real decimal to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The decimal value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Complex(decimal value)
        {
            return new Complex((double)value, 0.0d);
        }

        /// <summary>
        /// Explicit conversion of a <c>Complex</c> to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The decimal value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Complex(Complex32 value)
        {
            return new Complex((double)value.Real, (double)value.Imaginary);
        }

        /// <summary>
        /// Implicit conversion of a real byte to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The byte value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex(byte value)
        {
            return new Complex(value, 0.0d);
        }

        /// <summary>
        /// Implicit conversion of a real short to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The short value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex(short value)
        {
            return new Complex(value, 0.0d);
        }

        /// <summary>
        /// Implicit conversion of a signed byte to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The signed byte value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(sbyte value)
        {
            return new Complex(value, 0.0d);
        }

        /// <summary>
        /// Implicit conversion of a unsgined real short to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The unsgined short value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(ushort value)
        {
            return new Complex(value, 0.0d);
        }

        /// <summary>
        /// Implicit conversion of a real int to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The int value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex(int value)
        {
            return new Complex(value, 0.0d);
        }

        /// <summary>
        /// Implicit conversion of a real long to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The long value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex(long value)
        {
            return new Complex(value, 0.0d);
        }

        /// <summary>
        /// Implicit conversion of a real uint to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The uint value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(uint value)
        {
            return new Complex(value, 0.0d);
        }

        /// <summary>
        /// Implicit conversion of a real ulong to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The ulong value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex(ulong value)
        {
            return new Complex(value, 0.0d);
        }

        /// <summary>
        /// Implicit conversion of a real double to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The double value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex(double value)
        {
            return new Complex(value, 0.0d);
        }

        /// <summary>
        /// Implicit conversion of a real float to a <c>Complex</c>.
        /// </summary>
        /// <param name="value">The double value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Complex(float value)
        {
            return new Complex((double)value, 0.0d);
        }

        /// <summary>
        /// Converts this <c>Complex</c> to a <see cref="Complex"/>.
        /// </summary>
        /// <returns>A <see cref="Complex"/> with the same values as this <c>Complex</c>.</returns>
        public Complex ToComplex()
        {
            return new Complex(_real, _imag);
        }

        #endregion

        /// <summary>
        /// Gets the absolute value (or magnitude) of a complex number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The absolute value (or magnitude) of a complex number.</returns>
        public static double Abs(Complex value)
        {
            return value.Magnitude;
        }

        /// <summary>
        /// Trigonometric Arc Cosine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The arc cosine of a complex number.
        /// </returns>
        public static Complex Acos(Complex value)
        {
            return (Complex)value.ToComplex().InverseCosine();
        }

        /// <summary>
        /// Trigonometric Arc Sine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The arc sine of a complex number.
        /// </returns>
        public static Complex Asin(Complex value)
        {
            return (Complex)value.ToComplex().InverseSine();
        }

        /// <summary>
        /// Trigonometric Arc Tangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The arc tangent of a complex number.
        /// </returns>
        public static Complex Atan(Complex value)
        {
            return (Complex)value.ToComplex().InverseTangent();
        }

        /// <summary>
        /// Trigonometric Cosine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The cosine of a complex number.
        /// </returns>
        public static Complex Cos(Complex value)
        {
            return (Complex)value.ToComplex().Cosine();
        }

        /// <summary>
        /// Trigonometric Sine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The Sine of a complex number.
        /// </returns>
        public static Complex Sin(Complex value)
        {
            return (Complex)value.ToComplex().Sine();
        }

        /// <summary>
        /// Trigonometric Tangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The tangent of a complex number.
        /// </returns>
        public static Complex Tan(Complex value)
        {
            return (Complex)value.ToComplex().Tangent();
        }

        /// <summary>
        /// Trigonometric Hyperbolic Cosine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The hyperbolic cosine of a complex number.
        /// </returns>
        public static Complex Cosh(Complex value)
        {
            return (Complex)value.ToComplex().HyperbolicCosine();
        }

        /// <summary>
        /// Trigonometric Hyperbolic Sine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The hyperbolic sine of a complex number.
        /// </returns>
        public static Complex Sinh(Complex value)
        {
            return (Complex)value.ToComplex().HyperbolicSine();
        }

        /// <summary>
        /// Trigonometric Hyperbolic Tangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The hyperbolic tangent of a complex number.
        /// </returns>
        public static Complex Tanh(Complex value)
        {
            return (Complex)value.ToComplex().HyperbolicTangent();
        }

        /// <summary>
        /// Exponential of a <c>Complex</c> number (exp(x), E^x).
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The exponential of a complex number.
        /// </returns>
        public static Complex Exp(Complex value)
        {
            return (Complex)value.ToComplex().Exponential();
        }

        /// <summary>
        /// Constructs a <c>Complex</c> from its magnitude and phase.
        /// </summary>
        /// <param name="magnitude">
        /// Must be non-negative.
        /// </param>
        /// <param name="phase">
        /// Real number.
        /// </param>
        /// <returns>
        /// A new <c>Complex</c> from the given values.
        /// </returns>
        /// <seealso cref="WithModulusArgument"/>
        public static Complex FromPolarCoordinates(double magnitude, double phase)
        {
            return WithModulusArgument(magnitude, phase);
        }

        /// <summary>
        /// Natural Logarithm  of a <c>Complex</c> number (exp(x), E^x).
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The natural logarithm of a complex number.
        /// </returns>
        public static Complex Log(Complex value)
        {
            return (Complex)value.ToComplex().NaturalLogarithm();
        }

        /// <summary>
        /// Returns the logarithm of a specified complex number in a specified base
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <param name="baseValue">The base of the logarithm.</param>
        /// <returns>The logarithm of value in base baseValue.</returns>
        public static Complex Log(Complex value, double baseValue)
        {
            if (baseValue == 1.0)
            {
                return double.NaN;
            }

            return (Complex)(value.ToComplex().NaturalLogarithm() / Math.Log(baseValue, Math.E));
        }

        /// <summary>
        /// Returns the base-10 logarithm of a specified complex number in a specified base
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The base-10 logarithm of the complex number.</returns>
        public static Complex Log10(Complex value)
        {
            return Log(value, 10);
        }

        /// <summary>
        /// Raise this a <c>Complex</c>number to the given value.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <param name="power">The exponent.</param>
        /// <returns>
        /// The complex number raised to the given exponent.
        /// </returns>
        public static Complex Pow(Complex value, Complex power)
        {
            return value.Power(power);
        }

        /// <summary>
        /// Raise this a <c>Complex</c>number to the given value.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <param name="power">The exponent.</param>
        /// <returns>
        /// The complex number raised to the given exponent.
        /// </returns>
        public static Complex Pow(Complex value, double power)
        {
            return value.Power(power);
        }

        /// <summary>
        /// Returns the multiplicative inverse of a complex number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The reciprocal of value.</returns>
        /// <remarks>If value is <see cref="Zero"/>, the method returns <see cref="Zero"/>. Otherwise, it returns the result of the expression <see cref="One"/> / value. </remarks>
        public static Complex Reciprocal(Complex value)
        {
            if (value.IsZero())
            {
                return _zero;
            }

            return 1.0d / value;
        }

        /// <summary>
        /// The Square Root (power 1/2) of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The square root of a complex number.
        /// </returns>
        public static Complex Sqrt(Complex value)
        {
            return value.SquareRoot();
        }
    }
#endif
}