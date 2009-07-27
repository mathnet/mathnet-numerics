// <copyright file="Complex.cs" company="Math.NET">
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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics
{
    /// <summary>
    /// Complex numbers class.
    /// </summary>
    /// <remarks>
    /// <para>The class <c>Complex</c> provides all elementary operations
    /// on complex numbers. All the operators <c>+</c>, <c>-</c>,
    /// <c>*</c>, <c>/</c>, <c>==</c>, <c>!=</c> are defined in the
    /// canonical way. Additional complex trigonometric functions such 
    /// as <see cref="Complex.Cosine"/>, ... 
    /// are also provided. Note that the <c>Complex</c> structures 
    /// has two special constant values <see cref="Complex.NaN"/> and 
    /// <see cref="Complex.Infinity"/>.</para>
    /// <para>In order to avoid possible ambiguities resulting from a 
    /// <c>Complex(double, double)</c> constructor, the static methods 
    /// <see cref="Complex.FromRealImaginary"/> and <see cref="Complex.FromModulusArgument"/>
    /// are provided instead.</para>
    /// <para><code>
    /// Complex x = Complex.FromRealImaginary(1d, 2d);
    /// Complex y = Complex.FromModulusArgument(1d, Math.Pi);
    /// Complex z = (x + y) / (x - y);
    /// </code></para>
    /// <para>Since there is no canonical order among the complex numbers,
    /// <c>Complex</c> does not implement <c>IComparable</c> but several
    /// lexicographic <c>IComparer</c> implementations are provided, see 
    /// <see cref="Complex.RealImaginaryComparer"/>,
    /// <see cref="Complex.ModulusArgumentComparer"/> and
    /// <see cref="Complex.ArgumentModulusComparer"/>.</para>
    /// <para>For mathematical details about complex numbers, please
    /// have a look at the <a href="http://en.wikipedia.org/wiki/Complex_number">
    /// Wikipedia</a></para>
    /// </remarks>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Complex : IFormattable, IEquatable<Complex>
    {
        #region fields

        /// <summary>
        /// Regular expressionused to parse strings into complex numbers.
        /// </summary>
        private static readonly Regex parseExpression = new Regex(@"^((?<r>(([-+]?(\d+\.?\d*|\d*\.?\d+)([Ee][-+]?[0-9]+)?)|(NaN)|([-+]?Infinity)))|(?<i>(([-+]?((\d+\.?\d*|\d*\.?\d+)([Ee][-+]?[0-9]+)?)|(NaN)|([-+]?Infinity))?[i]))|(?<r>(([-+]?(\d+\.?\d*|\d*\.?\d+)([Ee][-+]?[0-9]+)?)|(NaN)|([-+]?Infinity)))(?<i>(([-+]((\d+\.?\d*|\d*\.?\d+)([Ee][-+]?[0-9]+)?)|[-+](NaN)|([-+]Infinity))?[i])))$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        /// <summary>
        /// Represents imaginary unit number.
        /// </summary>
        private static readonly Complex i = new Complex(0, 1);

        /// <summary>
        /// Represents a infite complex number
        /// </summary>
        private static readonly Complex infinity = new Complex(double.PositiveInfinity, double.PositiveInfinity);

        /// <summary>
        /// Reprensents not-a-number.
        /// </summary>
        private static readonly Complex nan = new Complex(Double.NaN, Double.NaN);

        /// <summary>
        /// Representing the one value.
        /// </summary>
        private static readonly Complex one = new Complex(1.0, 0.0);

        /// <summary>
        /// Representing the zero value.
        /// </summary>
        private static readonly Complex zero = new Complex(0.0, 0.0);

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
        /// Initializes a new instance of the Complex struct with the given real
        /// and imaginary parts.
        /// </summary>
        /// <param name="real">The value for the real component.</param>
        /// <param name="imaginary">The value for the imaginary component.</param>
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
        /// alternative MathNet.PreciseNumerics and MathNet.Symbolics packages
        /// instead.
        /// </remarks>
        /// <value>A value representing the infinity value.</value>
        public static Complex Infinity
        {
            get { return infinity; }
        }

        /// <summary>
        /// Gets a value representing not-a-number. This field is constant.
        /// </summary>
        /// <value>A value representing not-a-number.</value>
        public static Complex NaN
        {
            get { return nan; }
        }

        /// <summary>
        /// Gets a value representing the imaginary unit number. This field is constant.
        /// </summary>
        /// <value>A value representing the imaginary unit number.</value>
        public static Complex I
        {
            get { return i; }
        }

        /// <summary>
        /// Gets a value representing the zero value. This field is constant.
        /// </summary>
        /// <value>A value representing the zero value.</value>
        public static Complex Zero
        {
            get { return new Complex(0.0, 0.0); }
        }

        /// <summary>
        /// Gets a value representing the <c>1</c> value. This field is constant.
        /// </summary>
        /// <value>A value representing the <c>1</c> value.</value>
        public static Complex One
        {
            get { return one; }
        }

        #endregion Properties

        /// <summary>
        /// Gets the real component of the complex number.
        /// </summary>
        /// <value>The real component of the complex number.</value>
        public double Real
        {
            get { return _real; }
        }

        /// <summary>
        /// Gets the real imaginary component of the complex number.
        /// </summary>
        /// <value>The real imaginary component of the complex number.</value>
        public double Imaginary
        {
            get { return _imag; }
        }

        /// <summary>
        /// Gets a value indicating whether whether the <c>Complex</c> is zero.
        /// </summary>
        /// <value><c>true</c> if this instance is zero; otherwise, <c>false</c>.</value>
        public bool IsZero
        {
            get { return _real.AlmostZero() && _imag.AlmostZero(); }
        }

        /// <summary>
        /// Gets a value indicating whether the <c>Complex</c> is one.
        /// </summary>
        /// <value><c>true</c> if this instance is one; otherwise, <c>false</c>.</value>
        public bool IsOne
        {
            get { return _real.AlmostEqual(1.0) && _imag.AlmostZero(); }
        }

        /// <summary>
        /// Gets a value indicating whether the <c>Complex</c> is the imaginary unit.
        /// </summary>
        /// <value><c>true</c> if this instance is I; otherwise, <c>false</c>.</value>
        public bool IsI
        {
            get { return _real.AlmostZero() && _imag.AlmostEqual(1.0); }
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c> evaluates to a
        /// value that is not a number.
        /// </summary>
        /// <value><c>true</c> if this instance is NaN; otherwise, <c>false</c>.</value>
        public bool IsNaN
        {
            get { return double.IsNaN(_real) || double.IsNaN(_imag); }
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c> evaluates to an
        /// infinite value.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is infinite; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// True if it either evaluates to a complex infinity
        /// or to a directed infinity.
        /// </remarks>
        public bool IsInfinity
        {
            get { return double.IsInfinity(_real) || double.IsInfinity(_imag); }
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c> is real.
        /// </summary>
        /// <value><c>true</c> if this instance is a real number; otherwise, <c>false</c>.</value>
        public bool IsReal
        {
            get { return _imag.AlmostZero(); }
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c> is real and not negative, that is &gt;= 0.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is real nonnegative number; otherwise, <c>false</c>.
        /// </value>
        public bool IsRealNonNegative
        {
            get { return _imag.AlmostZero() && _real >= 0; }
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
        public Complex Conjugate
        {
            get { return new Complex(_real, -_imag); }
        }

        /// <summary>
        /// Gets or modulus of this <c>Complex</c>.
        /// </summary>
        /// <seealso cref="Argument"/>
        public double Modulus
        {
            get { return Math.Sqrt((_real * _real) + (_imag * _imag)); }
        }

        /// <summary>
        /// Gets the squared modulus of this <c>Complex</c>.
        /// </summary>
        /// <seealso cref="Argument"/>
        public double ModulusSquared
        {
            get { return (_real * _real) + (_imag * _imag); }
        }

        /// <summary>
        /// Gets argument of this <c>Complex</c>.
        /// </summary>
        /// <remarks>
        /// Argument always returns a value bigger than negative Pi and
        /// smaller or equal to Pi. If this <c>Complex</c> is zero, the Complex
        /// is assumed to be positive _real with an argument of zero.
        /// </remarks>
        public double Argument
        {
            get
            {
                if (IsReal && _real < 0)
                {
                    return Math.PI;
                }

                return IsRealNonNegative ? 0 : Math.Atan2(_imag, _real);
            }
        }

        /// <summary>
        /// Gets the unity of this complex (same argument, but on the unit circle; exp(I*arg))
        /// </summary>
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

                // don't replace this with "Modulus"!
                var mod = SpecialFunctions.Hypotenuse(_real, _imag);
                if (mod.AlmostZero())
                {
                    return Zero;
                }

                return new Complex(_real / mod, _imag / mod);
            }
        }

        #region Static Initializers

        /// <summary>
        /// Constructs a <c>Complex</c> from its real
        /// and imaginary parts.
        /// </summary>
        /// <param name="real">The value for the real component.</param>
        /// <param name="imaginary">The value for the imaginary component.</param>
        /// <returns>A new <c>Complex</c> with the given values.</returns>
        public static Complex WithRealImaginary(double real, double imaginary)
        {
            return new Complex(real, imaginary);
        }

        /// <summary>
        /// Constructs a <c>Complex</c> from its modulus and
        /// argument.
        /// </summary>
        /// <param name="modulus">Must be non-negative.</param>
        /// <param name="argument">Real number.</param>
        /// <returns>A new <c>Complex</c> from the given values.</returns>
        public static Complex WithModulusArgument(double modulus, double argument)
        {
            if (modulus < 0.0)
            {
                throw new ArgumentOutOfRangeException("modulus", modulus, Resources.ArgumentNotNegative);
            }

            return new Complex(modulus * Math.Cos(argument), modulus * Math.Sin(argument));
        }

        #endregion

        #region IFormattable Members

        /// <summary>A string representation of this complex number.</summary>
        /// <returns>The string representation of this complex number.</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>A string representation of this complex number.</summary>
        /// <returns>
        /// The string representation of this complex number formatted as specified by the
        /// format string.
        /// </returns>
        /// <param name="format">A format specification.</param>
        public string ToString(string format)
        {
            return ToString(format, null);
        }

        /// <summary>A string representation of this complex number.</summary>
        /// <returns>
        /// The string representation of this complex number formatted as specified by the
        /// format provider.
        /// </returns>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        /// <summary>A string representation of this complex number.</summary>
        /// <returns>
        /// The string representation of this complex number formatted as specified by the
        /// format string and format provider.
        /// </returns>
        /// <exception cref="FormatException">if the n, is not a number.</exception>
        /// <exception cref="ArgumentNullException">if s, is <see langword="null" />.</exception>
        /// <param name="format">A format specification.</param>
        /// <param name="formatProvider">An IFormatProvider that supplies culture-specific formatting information.</param>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (IsNaN)
            {
                return "NaN";
            }

            if (IsInfinity)
            {
                return "Infinity";
            }

            var ret = new StringBuilder();

            if (!_real.AlmostZero())
            {
                ret.Append(_real.ToString(format, formatProvider));
            }

            if (!_imag.AlmostZero())
            {
                if (!_real.AlmostZero())
                {
                    if (_imag < 0)
                    {
                        ret.Append(" ");
                    }
                    else
                    {
                        ret.Append(" + ");
                    }
                }

                ret.Append(_imag.ToString(format, formatProvider)).Append("i");
            }

            return ret.ToString();
        }

        #endregion

        #region IEquatable<Complex> Members

        /// <summary>
        /// Checks if two complex numbers are equal. Two complex numbers are equal if their
        /// corresponding real and imaginary components are equal.
        /// </summary>
        /// <returns>
        /// Returns true if the two objects are the same object, or if their corresponding
        /// real and imaginary components are equal, false otherwise.
        /// </returns>
        /// <param name="other">The complex number to compare to with.</param>
        public bool Equals(Complex other)
        {
            if (IsNaN || other.IsNaN)
            {
                return false;
            }
            if( IsInfinity && other.IsInfinity )
            {
                return true;
            }
            return _real.AlmostEqual(other._real) && _imag.AlmostEqual(other._imag);
        }

        /// <summary>The hash code for the complex number.</summary>
        /// <returns>The hash code of the complex number.</returns>
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
        /// Returns true if the two objects are the same object, or if their corresponding
        /// real and imaginary components are equal, false otherwise.
        /// </returns>
        /// <param name="obj">The complex number to compare to with.</param>
        public override bool Equals(object obj)
        {
            return (obj is Complex) && Equals((Complex) obj);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Equality test.
        /// </summary>
        /// <param name="complex1">One of complex numbers to compare.</param>
        /// <param name="complex2">The other complex numbers to compare.</param>
        /// <returns>true if the real and imaginary components of the two complex numbers are equal; false otherwise.</returns>
        public static bool operator ==(Complex complex1, Complex complex2)
        {
            return complex1.Equals(complex2);
        }

        /// <summary>
        /// Inequality test.
        /// </summary>
        /// <param name="complex1">One of complex numbers to compare.</param>
        /// <param name="complex2">The other complex numbers to compare.</param>
        /// <returns>true if the real or imaginary components of the two complex numbers are not equal; false otherwise.</returns>
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
            return new Complex((multiplicand._real * multiplier._real) - (multiplicand._imag * multiplier._imag), (multiplicand._real * multiplier._imag) + (multiplicand._imag * multiplier._real));
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
            if (divisor.IsZero)
            {
                return Infinity;
            }

            var modSquared = divisor.ModulusSquared;
            return new Complex(((dividend._real * divisor._real) + (dividend._imag * divisor._imag)) / modSquared, ((dividend._imag * divisor._real) - (dividend._real * divisor._imag)) / modSquared);
        }

        /// <summary>Division operator. Divides a double value by a complex number.</summary>
        /// <returns>The result of the division.</returns>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        public static Complex operator /(double dividend, Complex divisor)
        {
            if (divisor.IsZero)
            {
                return Infinity;
            }

            var zmod = divisor.ModulusSquared;
            return new Complex(dividend * divisor._real / zmod, -dividend * divisor._imag / zmod);
        }

        /// <summary>Division operator. Divides a complex number by a double value.</summary>
        /// <returns>The result of the division.</returns>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        public static Complex operator /(Complex dividend, double divisor)
        {
            if (divisor.AlmostZero())
            {
                return Infinity;
            }

            return new Complex(dividend._real / divisor, dividend._imag / divisor);
        }

        /// <summary>
        /// Implicit conversion of a real double to a real <c>Complex</c>.
        /// </summary>
        /// <param name="number">The double value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex(double number)
        {
            return new Complex(number, 0.0);
        }

        /// <summary>
        /// Unary addition.
        /// </summary>
        /// <returns>Returns the same complex number.</returns>
        public Complex Plus()
        {
            return this;
        }

        /// <summary>
        /// Unary minus.
        /// </summary>
        /// <returns>The negated value of this complex number.</returns>
        public Complex Negate()
        {
            return -this;
        }

        /// <summary>Adds a complex number to this one.</summary>
        /// <returns>The result of the addition.</returns>
        /// <param name="other">The other complex number to add.</param>
        public Complex Add(Complex other)
        {
            return this + other;
        }

        /// <summary>Subtracts a complex number from this one.</summary>
        /// <returns>The result of the subtraction.</returns>
        /// <param name="other">The other complex number to subtract from this one.</param>
        public Complex Subtract(Complex other)
        {
            return this - other;
        }

        /// <summary>Multiplies this complex number with this one.</summary>
        /// <returns>The result of the multiplication.</returns>
        /// <param name="multiplier">The complex number to multiply.</param>
        public Complex Multiply(Complex multiplier)
        {
            return this * multiplier;
        }

        /// <summary>Divides this complex number by another.</summary>
        /// <returns>The result of the division.</returns>
        /// <param name="divisor">The divisor.</param>
        public Complex Divide(Complex divisor)
        {
            return this / divisor;
        }

        #endregion
    }
}