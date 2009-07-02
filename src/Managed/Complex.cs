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
        private static readonly Regex parseExpression = new Regex(@"^((?<r>(([-+]?(\d+\.?\d*|\d*\.?\d+)([Ee][-+]?[0-9]+)?)|(NaN)|([-+]?Infinity)))|(?<i>(([-+]?((\d+\.?\d*|\d*\.?\d+)([Ee][-+]?[0-9]+)?)|(NaN)|([-+]?Infinity))?[i]))|(?<r>(([-+]?(\d+\.?\d*|\d*\.?\d+)([Ee][-+]?[0-9]+)?)|(NaN)|([-+]?Infinity)))(?<i>(([-+]((\d+\.?\d*|\d*\.?\d+)([Ee][-+]?[0-9]+)?)|[-+](NaN)|([-+]Infinity))?[i])))$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

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
            get { throw new NotImplementedException(); } // return Number.AlmostZero(real) && Number.AlmostZero(imag); }
        }

        /// <summary>
        /// Gets a value indicating whether the <c>Complex</c> is one.
        /// </summary>
        /// <value><c>true</c> if this instance is one; otherwise, <c>false</c>.</value>
        public bool IsOne
        {
            get { throw new NotImplementedException(); } // return Number.AlmostEqual(real, 1) && Number.AlmostZero(imag); }
        }

        /// <summary>
        /// Gets a value indicating whether the <c>Complex</c> is the imaginary unit.
        /// </summary>
        /// <value><c>true</c> if this instance is I; otherwise, <c>false</c>.</value>
        public bool IsI
        {
            get { throw new NotImplementedException(); } // return Number.AlmostZero(real) && Number.AlmostEqual(imag, 1); }
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c> evaluates to a
        /// value that is not a number.
        /// </summary>
        /// <value><c>true</c> if this instance is NaN; otherwise, <c>false</c>.</value>
        public bool IsNaN
        {
            get { throw new NotImplementedException(); } // return double.IsNaN(real) || double.IsNaN(imag); }
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c> evaluates to an
        /// infinite value.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is infinie; otherwise, <c>false</c>.
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
            get { throw new NotImplementedException(); } // return Number.AlmostZero(imag); }
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex</c> is real and not negative, that is &gt;= 0.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is real nonnegative number; otherwise, <c>false</c>.
        /// </value>
        public bool IsRealNonNegative
        {
            get { throw new NotImplementedException(); } // return Number.AlmostZero(imag) && real >= 0; }
        }

        /// <summary>
        /// Gets a value indicating whetherthe provided <c>Complex</c> is imaginary.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is an imaginary number; otherwise, <c>false</c>.
        /// </value>
        public bool IsImaginary
        {
            get { throw new NotImplementedException(); } // return Number.AlmostZero(real); }
        }

        #region Static Initializers

        /// <summary>
        /// Constructs a <c>Complex</c> from its real
        /// and imaginary parts.
        /// </summary>
        /// <param name="real">The value for the real component.</param>
        /// <param name="imaginary">The value for the imaginary component.</param>
        /// <returns>A new <c>Complex</c> with the given values.</returns>
        public static Complex FromRealImaginary(double real, double imaginary)
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
        public static Complex FromModulusArgument(double modulus, double argument)
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

            ret.Append(_real.ToString(format, formatProvider));
            if (_imag < 0)
            {
                ret.Append(" ");
            }
            else
            {
                ret.Append(" + ");
            }

            ret.Append(_imag.ToString(format, formatProvider)).Append("i");

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
            return Real == other.Real && Imaginary == other.Imaginary;
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
            return (obj is Complex) && Equals((Complex)obj);
        }

        #endregion
    }
}