// <copyright file="Complex32.cs" company="Math.NET">
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
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Text;
    using Properties;

    /// <summary>
    /// 32-bit Complex32 numbers class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class <c>Complex32</c> provides all elementary operations
    /// on complex numbers. All the operators <c>+</c>, <c>-</c>,
    /// <c>*</c>, <c>/</c>, <c>==</c>, <c>!=</c> are defined in the
    /// canonical way. Additional complex trigonometric functions 
    /// are also provided. Note that the <c>Complex32</c> structures 
    /// has two special constant values <see cref="Complex32.NaN"/> and 
    /// <see cref="Complex32.Infinity"/>.
    /// </para>
    /// <para>
    /// In order to avoid possible ambiguities resulting from a 
    /// <c>Complex32(float, float)</c> constructor, the static methods 
    /// <see cref="Complex32.WithRealImaginary"/> and <see cref="Complex32.WithModulusArgument"/>
    /// are provided instead.
    /// </para>
    /// <para>
    /// <code>
    /// Complex32 x = Complex32.FromRealImaginary(1d, 2d);
    /// Complex32 y = Complex32.FromModulusArgument(1d, Math.Pi);
    /// Complex32 z = (x + y) / (x - y);
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
    public struct Complex32 : IFormattable, IEquatable<Complex32>, IPrecisionSupport<Complex32>
    {
        #region fields

        /// <summary>
        /// Represents imaginary unit number.
        /// </summary>
        private static readonly Complex32 _i = new Complex32(0, 1);

        /// <summary>
        /// Represents a infinite complex number
        /// </summary>
        private static readonly Complex32 _infinity = new Complex32(float.PositiveInfinity, float.PositiveInfinity);

        /// <summary>
        /// Represents not-a-number.
        /// </summary>
        private static readonly Complex32 _nan = new Complex32(float.NaN, float.NaN);

        /// <summary>
        /// Representing the one value.
        /// </summary>
        private static readonly Complex32 _one = new Complex32(1.0f, 0.0f);

        /// <summary>
        /// Representing the zero value.
        /// </summary>
        private static readonly Complex32 _zero = new Complex32(0.0f, 0.0f);

        /// <summary>
        /// The real component of the complex number.
        /// </summary>
        private readonly float _real;

        /// <summary>
        /// The imaginary component of the complex number.
        /// </summary>
        private readonly float _imag;

        #endregion fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the Complex32 structure with the given real
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
        public Complex32(float real, float imaginary)
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
        /// The semantic associated to this value is a <c>Complex32</c> of
        /// infinite real and imaginary part. If you need more formal complex
        /// number handling (according to the Riemann Sphere and the extended
        /// complex plane C*, or using directed infinity) please check out the
        /// alternative Math.NET symbolics packages instead.
        /// </remarks>
        /// <value>A value representing the infinity value.</value>
        public static Complex32 Infinity
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
        public static Complex32 NaN
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
        public static Complex32 ImaginaryOne
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
        public static Complex32 Zero
        {
            get
            {
                return new Complex32(0.0f, 0.0f);
            }
        }

        /// <summary>
        /// Gets a value representing the <c>1</c> value. This field is constant.
        /// </summary>
        /// <value>A value representing the <c>1</c> value.</value>
        public static Complex32 One
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
        public float Real
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
        public float Imaginary
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
        /// Gets a value indicating whether the <c>Complex32</c> is zero.
        /// </summary>
        /// <returns><c>true</c> if this instance is zero; otherwise, <c>false</c>.</returns>
        public bool IsZero()
        {
            return _real == 0.0f && _imag == 0.0f;
        }

        /// <summary>
        /// Gets a value indicating whether the <c>Complex32</c> is one.
        /// </summary>
        /// <returns><c>true</c> if this instance is one; otherwise, <c>false</c>.</returns>
        public bool IsOne()
        {
            return _real == 1.0f && _imag == 0.0f;
        }

        /// <summary>
        /// Gets a value indicating whether the <c>Complex32</c> is the imaginary unit.
        /// </summary>
        /// <returns><c>true</c> if this instance is ImaginaryOne; otherwise, <c>false</c>.</returns>
        public bool IsImaginaryOne()
        {
            return _real == 0.0f && _imag == 1.0f;
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex32</c>evaluates
        /// to a value that is not a number.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is <see cref="NaN"/>; otherwise,
        /// <c>false</c>.
        /// </returns>
        public bool IsNaN()
        {
            return float.IsNaN(_real) || float.IsNaN(_imag);
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex32</c> evaluates to an
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
            return float.IsInfinity(_real) || float.IsInfinity(_imag);
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex32</c> is real.
        /// </summary>
        /// <returns><c>true</c> if this instance is a real number; otherwise, <c>false</c>.</returns>
        public bool IsReal()
        {
            return _imag == 0.0f;
        }

        /// <summary>
        /// Gets a value indicating whether the provided <c>Complex32</c> is real and not negative, that is &gt;= 0.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance is real nonnegative number; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRealNonNegative()
        {
            return _imag == 0.0f && _real >= 0;
        }

        /// <summary>
        /// Gets the conjugate of this <c>Complex32</c>.
        /// </summary>
        /// <remarks>
        /// The semantic of <i>setting the conjugate</i> is such that
        /// <code>
        /// // a, b of type Complex32
        /// a.Conjugate = b;
        /// </code>
        /// is equivalent to
        /// <code>
        /// // a, b of type Complex32
        /// a = b.Conjugate
        /// </code>
        /// </remarks>
        /// <returns>The conjugate of this <c>Complex32</c></returns>
        public Complex32 Conjugate()
        {
            return new Complex32(_real, -_imag);
        }

        /// <summary>
        /// Gets the magnitude or modulus of this <c>Complex32</c>.
        /// </summary>
        /// <returns>The magnitude or modulus of this <c>Complex32</c></returns>
        /// <seealso cref="Phase"/>
        public float Magnitude
        {
            get
            {
                return (float)Math.Sqrt((_real * _real) + (_imag * _imag));
            }
        }

        /// <summary>
        /// Gets the squared magnitude of this <c>Complex32</c>.
        /// </summary>
        /// <returns>The squared magnitude of this <c>Complex32</c></returns>
        public float MagnitudeSquared
        {
            get
            {
                return (_real * _real) + (_imag * _imag);
            }
        }

        /// <summary>
        /// Gets the phase or argument of this <c>Complex32</c>.
        /// </summary>
        /// <remarks>
        /// Phase always returns a value bigger than negative Pi and
        /// smaller or equal to Pi. If this <c>Complex32</c> is zero, the Complex32
        /// is assumed to be positive real with an argument of zero.
        /// </remarks>
        /// <returns>The phase or argument of this <c>Complex32</c></returns>
        public float Phase
        {
            get
            {
                if (IsReal() && _real < 0)
                {
                    return (float)Math.PI;
                }

                return IsRealNonNegative() ? 0.0f : (float)Math.Atan2(_imag, _real);
            }
        }

        /// <summary>
        /// Gets the unity of this complex (same argument, but on the unit circle; exp(I*arg))
        /// </summary>
        /// <returns>The unity of this <c>Complex32</c>.</returns>
        public Complex32 Sign
        {
            get
            {
                if (float.IsPositiveInfinity(_real) && float.IsPositiveInfinity(_imag))
                {
                    return new Complex32((float)Constants.Sqrt1Over2, (float)Constants.Sqrt1Over2);
                }

                if (float.IsPositiveInfinity(_real) && float.IsNegativeInfinity(_imag))
                {
                    return new Complex32((float)Constants.Sqrt1Over2, -(float)Constants.Sqrt1Over2);
                }

                if (float.IsNegativeInfinity(_real) && float.IsPositiveInfinity(_imag))
                {
                    return new Complex32(-(float)Constants.Sqrt1Over2, -(float)Constants.Sqrt1Over2);
                }

                if (float.IsNegativeInfinity(_real) && float.IsNegativeInfinity(_imag))
                {
                    return new Complex32(-(float)Constants.Sqrt1Over2, (float)Constants.Sqrt1Over2);
                }

                // don't replace this with "Magnitude"!
                var mod = SpecialFunctions.Hypotenuse(_real, _imag);
                if (mod == 0.0f)
                {
                    return Zero;
                }

                return new Complex32(_real / mod, _imag / mod);
            }
        }

        #region Exponential Functions

        /// <summary>
        /// Exponential of this <c>Complex32</c> (exp(x), E^x).
        /// </summary>
        /// <returns>
        /// The exponential of this complex number.
        /// </returns>
        public Complex32 Exponential()
        {
            var exp = (float)Math.Exp(_real);
            if (IsReal())
            {
                return new Complex32(exp, 0.0f);
            }

            return new Complex32(exp * (float)Trig.Cosine(_imag), exp * (float)Trig.Sine(_imag));
        }

        /// <summary>
        /// Natural Logarithm of this <c>Complex32</c> (Base E).
        /// </summary>
        /// <returns>
        /// The natural logarithm of this complex number.
        /// </returns>
        public Complex32 NaturalLogarithm()
        {
            if (IsRealNonNegative())
            {
                return new Complex32((float)Math.Log(_real), 0.0f);
            }

            return new Complex32(0.5f * (float)Math.Log(MagnitudeSquared), Phase);
        }

        /// <summary>
        /// Raise this <c>Complex32</c> to the given value.
        /// </summary>
        /// <param name="exponent">
        /// The exponent.
        /// </param>
        /// <returns>
        /// The complex number raised to the given exponent.
        /// </returns>
        public Complex32 Power(Complex32 exponent)
        {
            if (IsZero())
            {
                if (exponent.IsZero())
                {
                    return One;
                }

                if (exponent.Real > 0.0f)
                {
                    return Zero;
                }

                if (exponent.Real < 0)
                {
                    if (exponent.Imaginary == 0.0f)
                    {
                        return new Complex32(float.PositiveInfinity, 0.0f);
                    }

                    return new Complex32(float.PositiveInfinity, float.PositiveInfinity);
                }

                return NaN;
            }

            return (exponent * NaturalLogarithm()).Exponential();
        }

        /// <summary>
        /// Raise this <c>Complex32</c> to the inverse of the given value.
        /// </summary>
        /// <param name="rootExponent">
        /// The root exponent.
        /// </param>
        /// <returns>
        /// The complex raised to the inverse of the given exponent.
        /// </returns>
        public Complex32 Root(Complex32 rootExponent)
        {
            return Power(1 / rootExponent);
        }

        /// <summary>
        /// The Square (power 2) of this <c>Complex32</c>
        /// </summary>
        /// <returns>
        /// The square of this complex number.
        /// </returns>
        public Complex32 Square()
        {
            if (IsReal())
            {
                return new Complex32(_real * _real, 0.0f);
            }

            return new Complex32((_real * _real) - (_imag * _imag), 2 * _real * _imag);
        }

        /// <summary>
        /// The Square Root (power 1/2) of this <c>Complex32</c>
        /// </summary>
        /// <returns>
        /// The square root of this complex number.
        /// </returns>
        public Complex32 SquareRoot()
        {
            if (IsRealNonNegative())
            {
                return new Complex32((float)Math.Sqrt(_real), 0.0f);
            }

            Complex32 result;

            var absReal = Math.Abs(Real);
            var absImag = Math.Abs(Imaginary);
            double w;
            if (absReal >= absImag)
            {
                var ratio = Imaginary / Real;
                w = Math.Sqrt(absReal) * Math.Sqrt(0.5 * (1.0f + Math.Sqrt(1.0f + (ratio * ratio))));
            }
            else
            {
                var ratio = Real / Imaginary;
                w = Math.Sqrt(absImag) * Math.Sqrt(0.5 * (Math.Abs(ratio) + Math.Sqrt(1.0f + (ratio * ratio))));
            }

            if (Real >= 0.0f)
            {
                result = new Complex32((float)w, (float)(Imaginary / (2.0f * w)));
            }
            else if (Imaginary >= 0.0f)
            {
                result = new Complex32((float)(absImag / (2.0 * w)), (float)w);
            }
            else
            {
                result = new Complex32((float)(absImag / (2.0 * w)), (float)-w);
            }

            return result;
        }

        #endregion

        #region Static Initializers

        /// <summary>
        /// Constructs a <c>Complex32</c> from its real
        /// and imaginary parts.
        /// </summary>
        /// <param name="real">
        /// The value for the real component.
        /// </param>
        /// <param name="imaginary">
        /// The value for the imaginary component.
        /// </param>
        /// <returns>
        /// A new <c>Complex32</c> with the given values.
        /// </returns>
        public static Complex32 WithRealImaginary(float real, float imaginary)
        {
            return new Complex32(real, imaginary);
        }

        /// <summary>
        /// Constructs a <c>Complex32</c> from its modulus and
        /// argument.
        /// </summary>
        /// <param name="modulus">
        /// Must be non-negative.
        /// </param>
        /// <param name="argument">
        /// Real number.
        /// </param>
        /// <returns>
        /// A new <c>Complex32</c> from the given values.
        /// </returns>
        public static Complex32 WithModulusArgument(float modulus, float argument)
        {
            if (modulus < 0.0f)
            {
                throw new ArgumentOutOfRangeException("modulus", Resources.ArgumentNotNegative);
            }

            return new Complex32(modulus * (float)Math.Cos(argument), modulus * (float)Math.Sin(argument));
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

        #region IEquatable<Complex32> Members

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
        public bool Equals(Complex32 other)
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
            return (obj is Complex32) && Equals((Complex32)obj);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Equality test.
        /// </summary>
        /// <param name="complex1">One of complex numbers to compare.</param>
        /// <param name="complex2">The other complex numbers to compare.</param>
        /// <returns><c>true</c> if the real and imaginary components of the two complex numbers are equal; <c>false</c> otherwise.</returns>
        public static bool operator ==(Complex32 complex1, Complex32 complex2)
        {
            return complex1.Equals(complex2);
        }

        /// <summary>
        /// Inequality test.
        /// </summary>
        /// <param name="complex1">One of complex numbers to compare.</param>
        /// <param name="complex2">The other complex numbers to compare.</param>
        /// <returns><c>true</c> if the real or imaginary components of the two complex numbers are not equal; <c>false</c> otherwise.</returns>
        public static bool operator !=(Complex32 complex1, Complex32 complex2)
        {
            return !complex1.Equals(complex2);
        }

        /// <summary>
        /// Unary addition.
        /// </summary>
        /// <param name="summand">The complex number to operate on.</param>
        /// <returns>Returns the same complex number.</returns>
        public static Complex32 operator +(Complex32 summand)
        {
            return summand;
        }

        /// <summary>
        /// Unary minus.
        /// </summary>
        /// <param name="subtrahend">The complex number to operate on.</param>
        /// <returns>The negated value of the <paramref name="subtrahend"/>.</returns>
        public static Complex32 operator -(Complex32 subtrahend)
        {
            return new Complex32(-subtrahend._real, -subtrahend._imag);
        }

        /// <summary>Addition operator. Adds two complex numbers together.</summary>
        /// <returns>The result of the addition.</returns>
        /// <param name="summand1">One of the complex numbers to add.</param>
        /// <param name="summand2">The other complex numbers to add.</param>
        public static Complex32 operator +(Complex32 summand1, Complex32 summand2)
        {
            return new Complex32(summand1._real + summand2._real, summand1._imag + summand2._imag);
        }

        /// <summary>Subtraction operator. Subtracts two complex numbers.</summary>
        /// <returns>The result of the subtraction.</returns>
        /// <param name="minuend">The complex number to subtract from.</param>
        /// <param name="subtrahend">The complex number to subtract.</param>
        public static Complex32 operator -(Complex32 minuend, Complex32 subtrahend)
        {
            return new Complex32(minuend._real - subtrahend._real, minuend._imag - subtrahend._imag);
        }

        /// <summary>Addition operator. Adds a complex number and float together.</summary>
        /// <returns>The result of the addition.</returns>
        /// <param name="summand1">The complex numbers to add.</param>
        /// <param name="summand2">The float value to add.</param>
        public static Complex32 operator +(Complex32 summand1, float summand2)
        {
            return new Complex32(summand1._real + summand2, summand1._imag);
        }

        /// <summary>Subtraction operator. Subtracts float value from a complex value.</summary>
        /// <returns>The result of the subtraction.</returns>
        /// <param name="minuend">The complex number to subtract from.</param>
        /// <param name="subtrahend">The float value to subtract.</param>
        public static Complex32 operator -(Complex32 minuend, float subtrahend)
        {
            return new Complex32(minuend._real - subtrahend, minuend._imag);
        }

        /// <summary>Addition operator. Adds a complex number and float together.</summary>
        /// <returns>The result of the addition.</returns>
        /// <param name="summand1">The float value to add.</param>
        /// <param name="summand2">The complex numbers to add.</param>
        public static Complex32 operator +(float summand1, Complex32 summand2)
        {
            return new Complex32(summand2._real + summand1, summand2._imag);
        }

        /// <summary>Subtraction operator. Subtracts complex value from a float value.</summary>
        /// <returns>The result of the subtraction.</returns>
        /// <param name="minuend">The float vale to subtract from.</param>
        /// <param name="subtrahend">The complex value to subtract.</param>
        public static Complex32 operator -(float minuend, Complex32 subtrahend)
        {
            return new Complex32(minuend - subtrahend._real, -subtrahend._imag);
        }

        /// <summary>Multiplication operator. Multiplies two complex numbers.</summary>
        /// <returns>The result of the multiplication.</returns>
        /// <param name="multiplicand">One of the complex numbers to multiply.</param>
        /// <param name="multiplier">The other complex number to multiply.</param>
        public static Complex32 operator *(Complex32 multiplicand, Complex32 multiplier)
        {
            return new Complex32(
                (multiplicand._real * multiplier._real) - (multiplicand._imag * multiplier._imag), 
                (multiplicand._real * multiplier._imag) + (multiplicand._imag * multiplier._real));
        }

        /// <summary>Multiplication operator. Multiplies a complex number with a float value.</summary>
        /// <returns>The result of the multiplication.</returns>
        /// <param name="multiplicand">The float value to multiply.</param>
        /// <param name="multiplier">The complex number to multiply.</param>
        public static Complex32 operator *(float multiplicand, Complex32 multiplier)
        {
            return new Complex32(multiplier._real * multiplicand, multiplier._imag * multiplicand);
        }

        /// <summary>Multiplication operator. Multiplies a complex number with a float value.</summary>
        /// <returns>The result of the multiplication.</returns>
        /// <param name="multiplicand">The complex number to multiply.</param>
        /// <param name="multiplier">The float value to multiply.</param>
        public static Complex32 operator *(Complex32 multiplicand, float multiplier)
        {
            return new Complex32(multiplicand._real * multiplier, multiplicand._imag * multiplier);
        }

        /// <summary>Division operator. Divides a complex number by another.</summary>
        /// <returns>The result of the division.</returns>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        public static Complex32 operator /(Complex32 dividend, Complex32 divisor)
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
            return new Complex32(
                ((dividend._real * divisor._real) + (dividend._imag * divisor._imag)) / modSquared, 
                ((dividend._imag * divisor._real) - (dividend._real * divisor._imag)) / modSquared);
        }

        /// <summary>Division operator. Divides a float value by a complex number.</summary>
        /// <returns>The result of the division.</returns>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        public static Complex32 operator /(float dividend, Complex32 divisor)
        {
            if (dividend == 0.0f && divisor.IsZero())
            {
                return NaN;
            }

            if (divisor.IsZero())
            {
                return Infinity;
            }

            var zmod = divisor.MagnitudeSquared;
            return new Complex32(dividend * divisor._real / zmod, -dividend * divisor._imag / zmod);
        }

        /// <summary>Division operator. Divides a complex number by a float value.</summary>
        /// <returns>The result of the division.</returns>
        /// <param name="dividend">The dividend.</param>
        /// <param name="divisor">The divisor.</param>
        public static Complex32 operator /(Complex32 dividend, float divisor)
        {
            if (dividend.IsZero() && divisor == 0.0f)
            {
                return NaN;
            }

            if (divisor == 0.0f)
            {
                return Infinity;
            }

            return new Complex32(dividend._real / divisor, dividend._imag / divisor);
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
        public Complex32 Plus()
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
        public Complex32 Negate()
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
        public Complex32 Add(Complex32 other)
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
        public Complex32 Subtract(Complex32 other)
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
        public Complex32 Multiply(Complex32 multiplier)
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
        public Complex32 Divide(Complex32 divisor)
        {
            return this / divisor;
        }

        #endregion

        #region IPrecisionSupport<Complex32>

        /// <summary>
        /// Returns a Norm of a value of this type, which is appropriate for measuring how
        /// close this value is to zero.
        /// </summary>
        /// <returns>
        /// A norm of this value.
        /// </returns>
        double IPrecisionSupport<Complex32>.Norm()
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
        double IPrecisionSupport<Complex32>.NormOfDifference(Complex32 otherValue)
        {
            return (this - otherValue).MagnitudeSquared;
        }

        #endregion

        #region Parse Functions

        /// <summary>
        /// Creates a complex number based on a string. The string can be in the
        /// following formats (without the quotes): 'n', 'ni', 'n +/- ni',
        /// 'ni +/- n', 'n,n', 'n,ni,' '(n,n)', or '(n,ni)', where n is a float.
        /// </summary>
        /// <returns>
        /// A complex number containing the value specified by the given string.
        /// </returns>
        /// <param name="value">
        /// The string to parse.
        /// </param>
        public static Complex32 Parse(string value)
        {
            return Parse(value, null);
        }

        /// <summary>
        /// Creates a complex number based on a string. The string can be in the
        /// following formats (without the quotes): 'n', 'ni', 'n +/- ni',
        /// 'ni +/- n', 'n,n', 'n,ni,' '(n,n)', or '(n,ni)', where n is a float.
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
        public static Complex32 Parse(string value, IFormatProvider formatProvider)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
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
                return isLeftPartImaginary ? new Complex32(0, leftPart) : new Complex32(leftPart, 0);
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

                return new Complex32(leftPart, rightPart);
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

                return isLeftPartImaginary ? new Complex32(rightPart, leftPart) : new Complex32(leftPart, rightPart);
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
        /// <returns>Resulting part as float.</returns>
        /// <exception cref="FormatException"/>
        private static float ParsePart(ref LinkedListNode<string> token, out bool imaginary, IFormatProvider format)
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
        public static bool TryParse(string value, out Complex32 result)
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
        public static bool TryParse(string value, IFormatProvider formatProvider, out Complex32 result)
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
        /// Explicit conversion of a real decimal to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The decimal value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Complex32(decimal value)
        {
            return new Complex32((float)value, 0.0f);
        }

        /// <summary>
        /// Explicit conversion of a <c>Complex</c> to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The decimal value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Complex32(Complex value)
        {
            return new Complex32((float)value.Real, (float)value.Imaginary);
        }

        /// <summary>
        /// Implicit conversion of a real byte to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The byte value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex32(byte value)
        {
            return new Complex32(value, 0.0f);
        }

        /// <summary>
        /// Implicit conversion of a real short to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The short value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex32(short value)
        {
            return new Complex32(value, 0.0f);
        }

        /// <summary>
        /// Implicit conversion of a signed byte to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The signed byte value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex32(sbyte value)
        {
            return new Complex32(value, 0.0f);
        }

        /// <summary>
        /// Implicit conversion of a unsgined real short to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The unsgined short value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex32(ushort value)
        {
            return new Complex32(value, 0.0f);
        }

        /// <summary>
        /// Implicit conversion of a real int to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The int value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex32(int value)
        {
            return new Complex32(value, 0.0f);
        }

#if SYSNUMERICS
        /// <summary>
        /// Implicit conversion of a BigInteger int to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The BigInteger value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex32(BigInteger value)
        {
            return new Complex32((long)value, 0.0f);
        }
#endif

        /// <summary>
        /// Implicit conversion of a real long to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The long value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex32(long value)
        {
            return new Complex32(value, 0.0f);
        }

        /// <summary>
        /// Implicit conversion of a real uint to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The uint value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex32(uint value)
        {
            return new Complex32(value, 0.0f);
        }

        /// <summary>
        /// Implicit conversion of a real ulong to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The ulong value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        public static implicit operator Complex32(ulong value)
        {
            return new Complex32(value, 0.0f);
        }

        /// <summary>
        /// Implicit conversion of a real float to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The float value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator Complex32(float value)
        {
            return new Complex32(value, 0.0f);
        }

        /// <summary>
        /// Implicit conversion of a real double to a <c>Complex32</c>.
        /// </summary>
        /// <param name="value">The double value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Complex32(double value)
        {
            return new Complex32((float)value, 0.0f);
        }

        /// <summary>
        /// Converts this <c>Complex32</c> to a <see cref="Complex"/>.
        /// </summary>
        /// <returns>A <see cref="Complex"/> with the same values as this <c>Complex32</c>.</returns>
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
        public static double Abs(Complex32 value)
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
        public static Complex32 Acos(Complex32 value)
        {
            return (Complex32)value.ToComplex().InverseCosine();
        }

        /// <summary>
        /// Trigonometric Arc Sine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The arc sine of a complex number.
        /// </returns>
        public static Complex32 Asin(Complex32 value)
        {
            return (Complex32)value.ToComplex().InverseSine();
        }

        /// <summary>
        /// Trigonometric Arc Tangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The arc tangent of a complex number.
        /// </returns>
        public static Complex32 Atan(Complex32 value)
        {
            return (Complex32)value.ToComplex().InverseTangent();
        }

        /// <summary>
        /// Trigonometric Cosine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The cosine of a complex number.
        /// </returns>
        public static Complex32 Cos(Complex32 value)
        {
            return (Complex32)value.ToComplex().Cosine();
        }

        /// <summary>
        /// Trigonometric Sine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The Sine of a complex number.
        /// </returns>
        public static Complex32 Sin(Complex32 value)
        {
            return (Complex32)value.ToComplex().Sine();
        }

        /// <summary>
        /// Trigonometric Tangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The tangent of a complex number.
        /// </returns>
        public static Complex32 Tan(Complex32 value)
        {
            return (Complex32)value.ToComplex().Tangent();
        }

        /// <summary>
        /// Trigonometric Hyperbolic Cosine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The hyperbolic cosine of a complex number.
        /// </returns>
        public static Complex32 Cosh(Complex32 value)
        {
            return (Complex32)value.ToComplex().HyperbolicCosine();
        }

        /// <summary>
        /// Trigonometric Hyperbolic Sine of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The hyperbolic sine of a complex number.
        /// </returns>
        public static Complex32 Sinh(Complex32 value)
        {
            return (Complex32)value.ToComplex().HyperbolicSine();
        }

        /// <summary>
        /// Trigonometric Hyperbolic Tangent of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The hyperbolic tangent of a complex number.
        /// </returns>
        public static Complex32 Tanh(Complex32 value)
        {
            return (Complex32)value.ToComplex().HyperbolicTangent();
        }

        /// <summary>
        /// Exponential of a <c>Complex</c> number (exp(x), E^x).
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The exponential of a complex number.
        /// </returns>
        public static Complex32 Exp(Complex32 value)
        {
            return (Complex32)value.ToComplex().Exponential();
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
        public static Complex32 FromPolarCoordinates(float magnitude, float phase)
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
        public static Complex32 Log(Complex32 value)
        {
            return (Complex32)value.ToComplex().NaturalLogarithm();
        }

        /// <summary>
        /// Returns the logarithm of a specified complex number in a specified base
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <param name="baseValue">The base of the logarithm.</param>
        /// <returns>The logarithm of value in base baseValue.</returns>
        public static Complex32 Log(Complex32 value, float baseValue)
        {
            if (baseValue == 1.0)
            {
                return float.NaN;
            }

            return (Complex32)(value.ToComplex().NaturalLogarithm() / Math.Log(baseValue, Math.E));
        }

        /// <summary>
        /// Returns the base-10 logarithm of a specified complex number in a specified base
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The base-10 logarithm of the complex number.</returns>
        public static Complex32 Log10(Complex32 value)
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
        public static Complex32 Pow(Complex32 value, Complex32 power)
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
        public static Complex32 Pow(Complex32 value, float power)
        {
            return value.Power(power);
        }

        /// <summary>
        /// Returns the multiplicative inverse of a complex number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>The reciprocal of value.</returns>
        /// <remarks>If value is <see cref="Zero"/>, the method returns <see cref="Zero"/>. Otherwise, it returns the result of the expression <see cref="One"/> / value. </remarks>
        public static Complex32 Reciprocal(Complex32 value)
        {
            if (value.IsZero())
            {
                return _zero;
            }

            return 1.0f / value;
        }

        /// <summary>
        /// The Square Root (power 1/2) of a <c>Complex</c> number.
        /// </summary>
        /// <param name="value">A complex number.</param>
        /// <returns>
        /// The square root of a complex number.
        /// </returns>
        public static Complex32 Sqrt(Complex32 value)
        {
            return value.SquareRoot();
        }
    }
}