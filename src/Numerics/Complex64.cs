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

#if PORTABLE
namespace System.Numerics
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using MathNet.Numerics;
    using MathNet.Numerics.Properties;

    /// <summary>
    /// 64-bit double precision complex numbers class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class <c>Complex</c> provides all elementary operations
    /// on complex numbers. All the operators <c>+</c>, <c>-</c>,
    /// <c>*</c>, <c>/</c>, <c>==</c>, <c>!=</c> are defined in the
    /// canonical way. Additional complex trigonometric functions
    /// are also provided. Note that the <c>Complex</c> structures
    /// has two special constant values <see cref="Complex.NaN"/> and
    /// <see cref="Complex.PositiveInfinity"/>.
    /// </para>
    /// <para>
    /// <code>
    /// Complex x = new Complex(1d, 2d);
    /// Complex y = Complex.FromPolarCoordinates(1d, Math.Pi);
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
        /// <summary>
        /// The real component of the complex number.
        /// </summary>
        private readonly double _real;

        /// <summary>
        /// The imaginary component of the complex number.
        /// </summary>
        private readonly double _imag;

        /// <summary>
        /// Initializes a new instance of the Complex structure with the given real
        /// and imaginary parts.
        /// </summary>
        /// <param name="real">The value for the real component.</param>
        /// <param name="imaginary">The value for the imaginary component.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Complex(double real, double imaginary)
        {
            _real = real;
            _imag = imaginary;
        }

        /// <summary>
        /// Creates a complex number from a point's polar coordinates.
        /// </summary>
        /// <returns>A complex number.</returns>
        /// <param name="magnitude">The magnitude, which is the distance from the origin (the intersection of the x-axis and the y-axis) to the number.</param>
        /// <param name="phase">The phase, which is the angle from the line to the horizontal axis, measured in radians.</param>
        public static Complex FromPolarCoordinates(double magnitude, double phase)
        {
            return new Complex(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));
        }

        [Obsolete("Use the public constructor instead. Scheduled for removal in v3.0.")]
        public static Complex WithRealImaginary(double real, double imaginary)
        {
            return new Complex(real, imaginary);
        }

        [Obsolete("Use static FromPolarCoordinates instead. Scheduled for removal in v3.0.")]
        public static Complex WithModulusArgument(double modulus, double argument)
        {
            if (modulus < 0.0d)
            {
                throw new ArgumentOutOfRangeException("modulus", Resources.ArgumentNotNegative);
            }

            return new Complex(modulus * Math.Cos(argument), modulus * Math.Sin(argument));
        }

        /// <summary>
        /// Returns a new <see cref="T:System.Numerics.Complex" /> instance
        /// with a real number equal to zero and an imaginary number equal to zero.
        /// </summary>
        public static readonly Complex Zero = new Complex(0.0f, 0.0f);

        /// <summary>
        /// Returns a new <see cref="T:System.Numerics.Complex" /> instance
        /// with a real number equal to one and an imaginary number equal to zero.
        /// </summary>
        public static readonly Complex One = new Complex(1.0f, 0.0f);

        /// <summary>
        /// Returns a new <see cref="T:System.Numerics.Complex" /> instance
        /// with a real number equal to zero and an imaginary number equal to one.
        /// </summary>
        public static readonly Complex ImaginaryOne = new Complex(0, 1);

        /// <summary>
        /// Returns a new <see cref="T:System.Numerics.Complex" /> instance
        /// with real and imaginary numbers positive infinite.
        /// </summary>
        public static readonly Complex PositiveInfinity = new Complex(float.PositiveInfinity, float.PositiveInfinity);

        [Obsolete("Use PositiveInfinity instead. Scheduled for removal in v3.0.")]
        public static readonly Complex Infinity = PositiveInfinity;

        /// <summary>
        /// Returns a new <see cref="T:System.Numerics.Complex" /> instance
        /// with real and imaginary numbers not a number.
        /// </summary>
        public static readonly Complex NaN = new Complex(float.NaN, float.NaN);

        /// <summary>
        /// Gets the real component of the complex number.
        /// </summary>
        /// <value>The real component of the complex number.</value>
        public double Real
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _real; }
        }

        /// <summary>
        /// Gets the real imaginary component of the complex number.
        /// </summary>
        /// <value>The real imaginary component of the complex number.</value>
        public double Imaginary
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return _imag; }
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
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return Math.Atan2(_imag, _real); }
        }

        /// <summary>
        /// Gets the magnitude (or absolute value) of a complex number.
        /// </summary>
        /// <returns>The magnitude of the current instance.</returns>
        public double Magnitude
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return Math.Sqrt((_real * _real) + (_imag * _imag)); }
        }

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
                return PositiveInfinity;
            }

            var modSquared = divisor.MagnitudeSquared();
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
                return PositiveInfinity;
            }

            var zmod = divisor.MagnitudeSquared();
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
                return PositiveInfinity;
            }

            return new Complex(dividend._real / divisor, dividend._imag / divisor);
        }

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
            if (this.IsNaN() || other.IsNaN())
            {
                return false;
            }

            if (this.IsInfinity() && other.IsInfinity())
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
            int hash = 27;
            hash = (13 * hash) + _real.GetHashCode();
            hash = (13 * hash) + _imag.GetHashCode();
            return hash;
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
            return this.MagnitudeSquared();
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
            return (this - otherValue).MagnitudeSquared();
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

            var value = GlobalizationHelper.ParseSingle(ref token);

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
                result = Zero;
                ret = false;
            }
            catch (FormatException)
            {
                result = Zero;
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
        /// Returns the additive inverse of a specified complex number.
        /// </summary>
        /// <returns>The result of the <see cref="P:System.Numerics.Complex.Real" /> and <see cref="P:System.Numerics.Complex.Imaginary" /> components of the <paramref name="value" /> parameter multiplied by -1.</returns>
        /// <param name="value">A complex number.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Negate(Complex value)
        {
            return -value;
        }

        /// <summary>
        /// Computes the conjugate of a complex number and returns the result.
        /// </summary>
        /// <returns>The conjugate of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Conjugate(Complex value)
        {
            return new Complex(value._real, -value._imag);
        }

        /// <summary>
        /// Adds two complex numbers and returns the result.
        /// </summary>
        /// <returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
        /// <param name="left">The first complex number to add.</param>
        /// <param name="right">The second complex number to add.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Add(Complex left, Complex right)
        {
            return left + right;
        }

        /// <summary>
        /// Subtracts one complex number from another and returns the result.
        /// </summary>
        /// <returns>The result of subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
        /// <param name="left">The value to subtract from (the minuend).</param>
        /// <param name="right">The value to subtract (the subtrahend).</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Subtract(Complex left, Complex right)
        {
            return left - right;
        }

        /// <summary>
        /// Returns the product of two complex numbers.
        /// </summary>
        /// <returns>The product of the <paramref name="left" /> and <paramref name="right" /> parameters.</returns>
        /// <param name="left">The first complex number to multiply.</param>
        /// <param name="right">The second complex number to multiply.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Multiply(Complex left, Complex right)
        {
            return left * right;
        }

        /// <summary>
        /// Divides one complex number by another and returns the result.
        /// </summary>
        /// <returns>The quotient of the division.</returns>
        /// <param name="dividend">The complex number to be divided.</param>
        /// <param name="divisor">The complex number to divide by.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Divide(Complex dividend, Complex divisor)
        {
            return dividend / divisor;
        }

        /// <summary>
        /// Returns the multiplicative inverse of a complex number.
        /// </summary>
        /// <returns>The reciprocal of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        public static Complex Reciprocal(Complex value)
        {
            if (value.IsZero())
            {
                return Zero;
            }

            return 1.0d / value;
        }

        /// <summary>
        /// Returns the square root of a specified complex number.
        /// </summary>
        /// <returns>The square root of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        public static Complex Sqrt(Complex value)
        {
            if (value.IsRealNonNegative())
            {
                return new Complex(Math.Sqrt(value.Real), 0.0);
            }

            Complex result;

            var absReal = Math.Abs(value.Real);
            var absImag = Math.Abs(value.Imaginary);
            double w;
            if (absReal >= absImag)
            {
                var ratio = value.Imaginary / value.Real;
                w = Math.Sqrt(absReal) * Math.Sqrt(0.5 * (1.0 + Math.Sqrt(1.0 + (ratio * ratio))));
            }
            else
            {
                var ratio = value.Real / value.Imaginary;
                w = Math.Sqrt(absImag) * Math.Sqrt(0.5 * (Math.Abs(ratio) + Math.Sqrt(1.0 + (ratio * ratio))));
            }

            if (value.Real >= 0.0)
            {
                result = new Complex(w, value.Imaginary / (2.0 * w));
            }
            else if (value.Imaginary >= 0.0)
            {
                result = new Complex(absImag / (2.0 * w), w);
            }
            else
            {
                result = new Complex(absImag / (2.0 * w), -w);
            }

            return result;
        }

        /// <summary>
        /// Gets the absolute value (or magnitude) of a complex number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The absolute value (or magnitude) of a complex number.</returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static double Abs(Complex value)
        {
            return value.Magnitude;
        }

        /// <summary>
        /// Returns e raised to the power specified by a complex number.
        /// </summary>
        /// <returns>The number e raised to the power <paramref name="value" />.</returns>
        /// <param name="value">A complex number that specifies a power.</param>
        public static Complex Exp(Complex value)
        {
            var exp = Math.Exp(value.Real);
            if (value.IsReal())
            {
                return new Complex(exp, 0.0);
            }

            return new Complex(exp * Trig.Cosine(value.Imaginary), exp * Trig.Sine(value.Imaginary));
        }

        /// <summary>
        /// Returns a specified complex number raised to a power specified by a complex number.
        /// </summary>
        /// <returns>The complex number <paramref name="value" /> raised to the power <paramref name="power" />.</returns>
        /// <param name="value">A complex number to be raised to a power.</param>
        /// <param name="power">A complex number that specifies a power.</param>
        public static Complex Pow(Complex value, Complex power)
        {
            if (value.IsZero())
            {
                if (power.IsZero())
                {
                    return One;
                }

                if (power.Real > 0.0)
                {
                    return Zero;
                }

                if (power.Real < 0)
                {
                    if (power.Imaginary == 0.0)
                    {
                        return new Complex(double.PositiveInfinity, 0.0);
                    }

                    return new Complex(double.PositiveInfinity, double.PositiveInfinity);
                }

                return double.NaN;
            }

            return Exp(power * Log(value));
        }

        /// <summary>
        /// Returns a specified complex number raised to a power specified by a double-precision floating-point number.
        /// </summary>
        /// <returns>The complex number <paramref name="value" /> raised to the power <paramref name="power" />.</returns>
        /// <param name="value">A complex number to be raised to a power.</param>
        /// <param name="power">A double-precision floating-point number that specifies a power.</param>
        public static Complex Pow(Complex value, double power)
        {
            if (value.IsZero())
            {
                if (power == 0d)
                {
                    return One;
                }

                return power > 0d
                    ? Zero
                    : new Complex(double.PositiveInfinity, 0.0);
            }

            return Exp(power * Log(value));
        }

        /// <summary>
        /// Returns the natural (base e) logarithm of a specified complex number.
        /// </summary>
        /// <returns>The natural (base e) logarithm of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        public static Complex Log(Complex value)
        {
            if (value.IsRealNonNegative())
            {
                return new Complex(Math.Log(value.Real), 0.0);
            }

            return new Complex(0.5 * Math.Log(value.MagnitudeSquared()), value.Phase);
        }

        /// <summary>
        /// Returns the logarithm of a specified complex number in a specified base.
        /// </summary>
        /// <returns>The logarithm of <paramref name="value" /> in base <paramref name="baseValue" />.</returns>
        /// <param name="value">A complex number.</param>
        /// <param name="baseValue">The base of the logarithm.</param>
        public static Complex Log(Complex value, double baseValue)
        {
            return Log(value) / Math.Log(baseValue);
        }

        /// <summary>
        /// Returns the base-10 logarithm of a specified complex number.
        /// </summary>
        /// <returns>The base-10 logarithm of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        public static Complex Log10(Complex value)
        {
            return Log(value) / Constants.Ln10;
        }

        /// <summary>
        /// Returns the sine of the specified complex number.
        /// </summary>
        /// <returns>The sine of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Sin(Complex value)
        {
            return Trig.Sine(value);
        }

        /// <summary>
        /// Returns the cosine of the specified complex number.
        /// </summary>
        /// <returns>The cosine of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Cos(Complex value)
        {
            return Trig.Cosine(value);
        }

        /// <summary>
        /// Returns the tangent of the specified complex number.
        /// </summary>
        /// <returns>The tangent of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Tan(Complex value)
        {
            return Trig.Tangent(value);
        }

        /// <summary>
        /// Returns the angle that is the arc sine of the specified complex number.
        /// </summary>
        /// <returns>The angle which is the arc sine of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Asin(Complex value)
        {
            return Trig.InverseSine(value);
        }

        /// <summary>
        /// Returns the angle that is the arc cosine of the specified complex number.
        /// </summary>
        /// <returns>The angle, measured in radians, which is the arc cosine of <paramref name="value" />.</returns>
        /// <param name="value">A complex number that represents a cosine.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Acos(Complex value)
        {
            return Trig.InverseCosine(value);
        }

        /// <summary>
        /// Returns the angle that is the arc tangent of the specified complex number.
        /// </summary>
        /// <returns>The angle that is the arc tangent of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Atan(Complex value)
        {
            return Trig.InverseTangent(value);
        }

        /// <summary>
        /// Returns the hyperbolic sine of the specified complex number.
        /// </summary>
        /// <returns>The hyperbolic sine of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Sinh(Complex value)
        {
            return Trig.HyperbolicSine(value);
        }

        /// <summary>
        /// Returns the hyperbolic cosine of the specified complex number.
        /// </summary>
        /// <returns>The hyperbolic cosine of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Cosh(Complex value)
        {
            return Trig.HyperbolicCosine(value);
        }

        /// <summary>
        /// Returns the hyperbolic tangent of the specified complex number.
        /// </summary>
        /// <returns>The hyperbolic tangent of <paramref name="value" />.</returns>
        /// <param name="value">A complex number.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static Complex Tanh(Complex value)
        {
            return Trig.HyperbolicTangent(value);
        }
    }
}
#endif
