// <copyright file="Precision.cs" company="Math.NET">
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

#if PORTABLE
    using System.Runtime.InteropServices;
#endif

    /// <summary>
    /// Utilities for working with floating point numbers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Useful links:
    /// <list type="bullet">
    /// <item>
    /// http://docs.sun.com/source/806-3568/ncg_goldberg.html#689 - What every computer scientist should know about floating-point arithmetic
    /// </item>
    /// <item>
    /// http://en.wikipedia.org/wiki/Machine_epsilon - Gives the definition of machine epsilon
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public static class Precision
    {
        #region Constants

        /// <summary>
        /// The base number for binary values
        /// </summary>
        private const int BinaryBaseNumber = 2;

        /// <summary>
        /// The number of binary digits used to represent the binary number for a double precision floating
        /// point value. i.e. there are this many digits used to represent the
        /// actual number, where in a number as: 0.134556 * 10^5 the digits are 0.134556 and the exponent is 5.
        /// </summary>
        private const int DoublePrecision = 53;

        /// <summary>
        /// The number of binary digits used to represent the binary number for a single precision floating
        /// point value. i.e. there are this many digits used to represent the
        /// actual number, where in a number as: 0.134556 * 10^5 the digits are 0.134556 and the exponent is 5.
        /// </summary>
        private const int SinglePrecision = 24;

        #endregion
 
        #region Fields

        /// <summary>
        /// The maximum relative precision of a double
        /// </summary>
        private static readonly double _doubleMachinePrecision = Math.Pow(BinaryBaseNumber, -DoublePrecision);

        /// <summary>
        /// The maximum relative precision of a single
        /// </summary>
        private static readonly double _singleMachinePrecision = Math.Pow(BinaryBaseNumber, -SinglePrecision);

        /// <summary>
        /// The number of significant figures that a double-precision floating point has.
        /// </summary>
        private static readonly int _numberOfDecimalPlacesForDoubles;

        /// <summary>
        /// The number of significant figures that a single-precision floating point has.
        /// </summary>
        private static readonly int _numberOfDecimalPlacesForFloats;

        /// <summary>Value representing 10 * 2^(-52)</summary>
        private static readonly double _defaultDoubleRelativeAccuracy = _doubleMachinePrecision * 10;

        /// <summary>Value representing 10 * 2^(-52)</summary>
        private static readonly float _defaultSingleRelativeAccuracy = (float)(_singleMachinePrecision * 10);

        #endregion

        #region Properties
        /// <summary>
        /// Gets the maximum relative precision of a double.
        /// </summary>
        /// <value>The maximum relative precision of a double.</value>
        public static double DoubleMachinePrecision
        {
            get
            {
                return _doubleMachinePrecision;
            }
        }

        /// <summary>
        /// Gets the maximum relative precision of a single.
        /// </summary>
        /// <value>The maximum relative precision of a single.</value>
        public static double SingleMachinePrecision
        {
            get
            {
                return _singleMachinePrecision;
            }
        }
        #endregion

        /// <summary>
        /// Initializes static members of the Precision class.
        /// </summary>
        static Precision()
        {
            _numberOfDecimalPlacesForFloats = (int)Math.Ceiling(Math.Abs(Math.Log10(_singleMachinePrecision)));
            _numberOfDecimalPlacesForDoubles = (int)Math.Ceiling(Math.Abs(Math.Log10(_doubleMachinePrecision)));
        }

        /// <summary>
        /// Gets the number of decimal places for floats.
        /// </summary>
        /// <value>The number of decimal places for floats.</value>
        public static int NumberOfDecimalPlacesForFloats
        {
            get
            {
                return _numberOfDecimalPlacesForFloats;
            }
        }

        /// <summary>
        /// Gets the number of decimal places for doubles.
        /// </summary>
        /// <value>The number of decimal places for doubles.</value>
        public static int NumberOfDecimalPlacesForDoubles
        {
            get
            {
                return _numberOfDecimalPlacesForDoubles;
            }
        }

        /// <summary>
        /// Returns the magnitude of the number.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The magnitude of the number.</returns>
        public static int Magnitude(this double value)
        {
            // Can't do this with zero because the 10-log of zero doesn't exist.
            if (value.Equals(0.0))
            {
                return 0;
            }

            // Note that we need the absolute value of the input because Log10 doesn't
            // work for negative numbers (obviously).
            double magnitude = Math.Log10(Math.Abs(value));

#if PORTABLE
            // To get the right number we need to know if the value is negative or positive
            // truncating a positive number will always give use the correct magnitude
            // truncating a negative number will give us a magnitude that is off by 1
            if (magnitude < 0)
            {

                return (int)Truncate(magnitude - 1);
            }

            return (int)Truncate(magnitude);
#else
            // To get the right number we need to know if the value is negative or positive
            // truncating a positive number will always give use the correct magnitude
            // truncating a negative number will give us a magnitude that is off by 1
            if (magnitude < 0)
            {
                return (int)Math.Truncate(magnitude - 1);
            }

            return (int)Math.Truncate(magnitude);
#endif
        }


        /// <summary>
        /// Returns the magnitude of the number.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The magnitude of the number.</returns>
        public static int Magnitude(this float value)
        {
            // Can't do this with zero because the 10-log of zero doesn't exist.
            if (value.Equals(0.0f))
            {
                return 0;
            }

            // Note that we need the absolute value of the input because Log10 doesn't
            // work for negative numbers (obviously).
            var magnitude = Convert.ToSingle(Math.Log10(Math.Abs(value)));

            // To get the right number we need to know if the value is negative or positive
            // truncating a positive number will always give use the correct magnitude
            // truncating a negative number will give us a magnitude that is off by 1
            if (magnitude < 0)
            {
#if PORTABLE
                return (int)Truncate(magnitude - 1);
#else
                return (int)Math.Truncate(magnitude - 1);
#endif
            }

#if PORTABLE
            return (int)Truncate(magnitude);
#else
            return (int)Math.Truncate(magnitude);
#endif
        }
        /// <summary>
        /// Returns the number divided by it's magnitude, effectively returning a number between -10 and 10.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The value of the number.</returns>
        public static double GetMagnitudeScaledValue(this double value)
        {
            if (value.Equals(0.0))
            {
                return value;
            }

            int magnitude = Magnitude(value);
            return value * Math.Pow(10, -magnitude);
        }

        /// <summary>
        /// Gets the equivalent <c>long</c> value for the given <c>double</c> value.
        /// </summary>
        /// <param name="value">The <c>double</c> value which should be turned into a <c>long</c> value.</param>
        /// <returns>
        /// The resulting <c>long</c> value.
        /// </returns>
        private static long GetLongFromDouble(double value)
        {
#if PORTABLE
            return DoubleToInt64Bits(value);
#else
            return BitConverter.DoubleToInt64Bits(value);
#endif
        }

        /// <summary>
        /// Returns a 'directional' long value. This is a long value which acts the same as a double,
        /// e.g. a negative double value will return a negative double value starting at 0 and going
        /// more negative as the double value gets more negative.
        /// </summary>
        /// <param name="value">The input double value.</param>
        /// <returns>A long value which is roughly the equivalent of the double value.</returns>
        private static long GetDirectionalLongFromDouble(double value)
        {
            // Convert in the normal way.
            long result = GetLongFromDouble(value);
            
            // Now find out where we're at in the range
            // If the value is larger/equal to zero then we can just return the value
            // if the value is negative we subtract long.MinValue from it.
            return (result >= 0) ? result : (long.MinValue - result);
        }

        /// <summary>
        /// Returns a 'directional' int value. This is a int value which acts the same as a float,
        /// e.g. a negative float value will return a negative int value starting at 0 and going
        /// more negative as the float value gets more negative.
        /// </summary>
        /// <param name="value">The input float value.</param>
        /// <returns>An int value which is roughly the equivalent of the double value.</returns>
        private static int GetDirectionalIntFromFloat(float value)
        {
            // Convert in the normal way.
            int result = FloatToInt32Bits(value);

            // Now find out where we're at in the range
            // If the value is larger/equal to zero then we can just return the value
            // if the value is negative we subtract int.MinValue from it.
            return (result >= 0) ? result : (int.MinValue - result);
        }

        /// <summary>
        /// Increments a floating point number to the next bigger number representable by the data type.
        /// </summary>
        /// <param name="value">The value which needs to be incremented.</param>
        /// <remarks>
        /// The incrementation step length depends on the provided value.
        /// Increment(double.MaxValue) will return positive infinity.
        /// </remarks>
        /// <returns>The next larger floating point value.</returns>
        public static double Increment(this double value)
        {
            return Increment(value, 1);
        }

        /// <summary>
        /// Increments a floating point number to the next bigger number representable by the data type.
        /// </summary>
        /// <param name="value">The value which needs to be incremented.</param>
        /// <param name="count">How many times the number should be incremented.</param>
        /// <remarks>
        /// The incrementation step length depends on the provided value.
        /// Increment(double.MaxValue) will return positive infinity.
        /// </remarks>
        /// <returns>The next larger floating point value.</returns>
        public static double Increment(this double value, int count)
        {
            if (double.IsInfinity(value) || double.IsNaN(value) || count == 0)
            {
                return value;
            }

            if (count < 0)
            {
                return Decrement(value, -count);
            }

            // Translate the bit pattern of the double to an integer.
            // Note that this leads to:
            // double > 0 --> long > 0, growing as the double value grows
            // double < 0 --> long < 0, increasing in absolute magnitude as the double 
            //                          gets closer to zero!
            //                          i.e. 0 - double.epsilon will give the largest long value!
            long intValue = GetLongFromDouble(value);
            if (intValue < 0)
            {
                intValue -= count;
            }
            else
            {
                intValue += count;
            }

            // Note that long.MinValue has the same bit pattern as -0.0.
            if (intValue == long.MinValue)
            {
                return 0;
            }

            // Note that not all long values can be translated into double values. There's a whole bunch of them 
            // which return weird values like infinity and NaN
#if PORTABLE
            return Int64BitsToDouble(intValue);
#else
            return BitConverter.Int64BitsToDouble(intValue);
#endif        
        }

        /// <summary>
        /// Decrements a floating point number to the next smaller number representable by the data type.
        /// </summary>
        /// <param name="value">The value which should be decremented.</param>
        /// <remarks>
        /// The decrementation step length depends on the provided value.
        /// Decrement(double.MinValue) will return negative infinity.
        /// </remarks>
        /// <returns>The next smaller floating point value.</returns>
        public static double Decrement(this double value)
        {
            return Decrement(value, 1);
        }

        /// <summary>
        /// Decrements a floating point number to the next smaller number representable by the data type.
        /// </summary>
        /// <param name="value">The value which should be decremented.</param>
        /// <param name="count">How many times the number should be decremented.</param>
        /// <remarks>
        /// The decrementation step length depends on the provided value.
        /// Decrement(double.MinValue) will return negative infinity.
        /// </remarks>
        /// <returns>The next smaller floating point value.</returns>
        public static double Decrement(this double value, int count)
        {
            if (double.IsInfinity(value) || double.IsNaN(value) || count == 0)
            {
                return value;
            }

            if (count < 0)
            {
                return Decrement(value, -count);
            }

            // Translate the bit pattern of the double to an integer.
            // Note that this leads to:
            // double > 0 --> long > 0, growing as the double value grows
            // double < 0 --> long < 0, increasing in absolute magnitude as the double 
            //                          gets closer to zero!
            //                          i.e. 0 - double.epsilon will give the largest long value!
            long intValue = GetLongFromDouble(value);

            // If the value is zero then we'd really like the value to be -0. So we'll make it -0 
            // and then everything else should work out.
            if (intValue == 0)
            {
                // Note that long.MinValue has the same bit pattern as -0.0.
                intValue = long.MinValue;
            }

            if (intValue < 0)
            {
                intValue += count;
            }
            else
            {
                intValue -= count;
            }

            // Note that not all long values can be translated into double values. There's a whole bunch of them 
            // which return weird values like infinity and NaN
#if PORTABLE
            return Int64BitsToDouble(intValue);
#else
            return BitConverter.Int64BitsToDouble(intValue);
#endif   
        }

        /// <summary>
        /// Forces small numbers near zero to zero, according to the specified absolute accuracy.
        /// </summary>
        /// <param name="a">The real number to coerce to zero, if it is almost zero.</param>
        /// <param name="maxNumbersBetween">The maximum count of numbers between the zero and the number <paramref name="a"/>.</param>
        /// <returns>
        ///     Zero if |<paramref name="a"/>| is fewer than <paramref name="maxNumbersBetween"/> numbers from zero, <paramref name="a"/> otherwise.
        /// </returns>
        public static double CoerceZero(this double a, int maxNumbersBetween)
        {
            return CoerceZero(a, (long)maxNumbersBetween);
        }

        /// <summary>
        /// Forces small numbers near zero to zero, according to the specified absolute accuracy.
        /// </summary>
        /// <param name="a">The real number to coerce to zero, if it is almost zero.</param>
        /// <param name="maxNumbersBetween">The maximum count of numbers between the zero and the number <paramref name="a"/>.</param>
        /// <returns>
        ///     Zero if |<paramref name="a"/>| is fewer than <paramref name="maxNumbersBetween"/> numbers from zero, <paramref name="a"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="maxNumbersBetween"/> is smaller than zero.
        /// </exception>
        public static double CoerceZero(this double a, long maxNumbersBetween)
        {
            if (maxNumbersBetween < 0)
            {
                throw new ArgumentOutOfRangeException("maxNumbersBetween");
            }

            if (double.IsInfinity(a) || double.IsNaN(a))
            {
                return a;
            }

            // We allow maxNumbersBetween between 0 and the number so
            // we need to check if there a
            if (NumbersBetween(0.0, a) <= (ulong)maxNumbersBetween)
            {
                return 0.0;
            }

            return a;
        }

        /// <summary>
        /// Forces small numbers near zero to zero, according to the specified absolute accuracy.
        /// </summary>
        /// <param name="a">The real number to coerce to zero, if it is almost zero.</param>
        /// <param name="maximumAbsoluteError">The absolute threshold for <paramref name="a"/> to consider it as zero.</param>
        /// <returns>Zero if |<paramref name="a"/>| is smaller than <paramref name="maximumAbsoluteError"/>, <paramref name="a"/> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="maximumAbsoluteError"/> is smaller than zero.
        /// </exception>
        public static double CoerceZero(this double a, double maximumAbsoluteError)
        {
            if (maximumAbsoluteError < 0)
            {
                throw new ArgumentOutOfRangeException("maximumAbsoluteError");
            }
            
            if (double.IsInfinity(a) || double.IsNaN(a))
            {
                return a;
            }

            if (Math.Abs(a) < maximumAbsoluteError)
            {
                return 0.0;
            }

            return a;
        }

        /// <summary>
        /// Forces small numbers near zero to zero.
        /// </summary>
        /// <param name="a">The real number to coerce to zero, if it is almost zero.</param>
        /// <returns>Zero if |<paramref name="a"/>| is smaller than 2^(-53) = 1.11e-16, <paramref name="a"/> otherwise.</returns>
        public static double CoerceZero(this double a)
        {
            return CoerceZero(a, _doubleMachinePrecision);
        }

        /// <summary>
        /// Determines the range of floating point numbers that will match the specified value with the given tolerance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maxNumbersBetween">The <c>ulps</c> difference.</param>
        /// <param name="bottomRangeEnd">The bottom range end.</param>
        /// <param name="topRangeEnd">The top range end.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="maxNumbersBetween"/> is smaller than zero.
        /// </exception>
        public static void RangeOfMatchingFloatingPointNumbers(this double value, long maxNumbersBetween, out double bottomRangeEnd, out double topRangeEnd)
        {
            // Make sure ulpDifference is non-negative
            if (maxNumbersBetween < 1)
            {
                throw new ArgumentOutOfRangeException("maxNumbersBetween");
            }

            // If the value is infinity (positive or negative) just
            // return the same infinity for the range.
            if (double.IsInfinity(value))
            {
                topRangeEnd = value;
                bottomRangeEnd = value;
                return;
            }

            // If the value is a NaN then the range is a NaN too.
            if (double.IsNaN(value))
            {
                topRangeEnd = double.NaN;
                bottomRangeEnd = double.NaN;
                return;
            }

            // Translate the bit pattern of the double to an integer.
            // Note that this leads to:
            // double > 0 --> long > 0, growing as the double value grows
            // double < 0 --> long < 0, increasing in absolute magnitude as the double 
            //                          gets closer to zero!
            //                          i.e. 0 - double.epsilon will give the largest long value!
            long intValue = GetLongFromDouble(value);

#if PORTABLE
            // We need to protect against over- and under-flow of the intValue when
            // we start to add the ulpsDifference.
            if (intValue < 0)
            {
                // Note that long.MinValue has the same bit pattern as
                // -0.0. Therefore we're working in opposite direction (i.e. add if we want to
                // go more negative and subtract if we want to go less negative)
                topRangeEnd = Math.Abs(long.MinValue - intValue) < maxNumbersBetween
                    // Got underflow, which can be fixed by splitting the calculation into two bits
                    // first get the remainder of the intValue after subtracting it from the long.MinValue
                    // and add that to the ulpsDifference. That way we'll turn positive without underflow
                    ? Int64BitsToDouble(maxNumbersBetween + (long.MinValue - intValue))
                    // No problems here, move along.
                    : Int64BitsToDouble(intValue - maxNumbersBetween);

                bottomRangeEnd = Math.Abs(intValue) < maxNumbersBetween
                    // Underflow, which means we'd have to go further than a long would allow us.
                    // Also we couldn't translate it back to a double, so we'll return -Double.MaxValue
                    ? -double.MaxValue
                    // intValue is negative. Adding the positive ulpsDifference means that it gets less negative.
                    // However due to the conversion way this means that the actual double value gets more negative :-S
                    : Int64BitsToDouble(intValue + maxNumbersBetween);
            }
            else
            {
                // IntValue is positive
                topRangeEnd = long.MaxValue - intValue < maxNumbersBetween
                    // Overflow, which means we'd have to go further than a long would allow us.
                    // Also we couldn't translate it back to a double, so we'll return Double.MaxValue 
                    ? double.MaxValue
                    // No troubles here
                    : Int64BitsToDouble(intValue + maxNumbersBetween);

                // Check the bottom range end for underflows
                bottomRangeEnd = intValue > maxNumbersBetween
                    // No problems here. IntValue is larger than ulpsDifference so we'll end up with a
                    // positive number.
                    ? Int64BitsToDouble(intValue - maxNumbersBetween)
                    // Int value is bigger than zero but smaller than the ulpsDifference. So we'll need to deal with
                    // the reversal at the negative end
                    : Int64BitsToDouble(long.MinValue + (maxNumbersBetween - intValue));
            }
#else
            // We need to protect against over- and under-flow of the intValue when
            // we start to add the ulpsDifference.
            if (intValue < 0)
            {
                // Note that long.MinValue has the same bit pattern as
                // -0.0. Therefore we're working in opposite direction (i.e. add if we want to
                // go more negative and subtract if we want to go less negative)
                topRangeEnd = Math.Abs(long.MinValue - intValue) < maxNumbersBetween
                    // Got underflow, which can be fixed by splitting the calculation into two bits
                    // first get the remainder of the intValue after subtracting it from the long.MinValue
                    // and add that to the ulpsDifference. That way we'll turn positive without underflow
                    ? BitConverter.Int64BitsToDouble(maxNumbersBetween + (long.MinValue - intValue))
                    // No problems here, move along.
                    : BitConverter.Int64BitsToDouble(intValue - maxNumbersBetween);

                bottomRangeEnd = Math.Abs(intValue) < maxNumbersBetween
                    // Underflow, which means we'd have to go further than a long would allow us.
                    // Also we couldn't translate it back to a double, so we'll return -Double.MaxValue
                    ? -double.MaxValue
                    // intValue is negative. Adding the positive ulpsDifference means that it gets less negative.
                    // However due to the conversion way this means that the actual double value gets more negative :-S
                    : BitConverter.Int64BitsToDouble(intValue + maxNumbersBetween);
            }
            else
            {
                // IntValue is positive
                topRangeEnd = long.MaxValue - intValue < maxNumbersBetween
                    // Overflow, which means we'd have to go further than a long would allow us.
                    // Also we couldn't translate it back to a double, so we'll return Double.MaxValue
                    ? double.MaxValue
                    // No troubles here
                    : BitConverter.Int64BitsToDouble(intValue + maxNumbersBetween);

                // Check the bottom range end for underflows
                bottomRangeEnd = intValue > maxNumbersBetween
                    // No problems here. IntValue is larger than ulpsDifference so we'll end up with a
                    // positive number.
                    ? BitConverter.Int64BitsToDouble(intValue - maxNumbersBetween)
                    // Int value is bigger than zero but smaller than the ulpsDifference. So we'll need to deal with
                    // the reversal at the negative end
                    : BitConverter.Int64BitsToDouble(long.MinValue + (maxNumbersBetween - intValue));
            }
#endif
        }

        /// <summary>
        /// Returns the floating point number that will match the value with the tolerance on the maximum size (i.e. the result is
        /// always bigger than the value)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maxNumbersBetween">The <c>ulps</c> difference.</param>
        /// <returns>The maximum floating point number which is <paramref name="maxNumbersBetween"/> larger than the given <paramref name="value"/>.</returns>
        public static double MaximumMatchingFloatingPointNumber(this double value, long maxNumbersBetween)
        {
            double topRangeEnd, bottomRangeEnd;
            RangeOfMatchingFloatingPointNumbers(value, maxNumbersBetween, out bottomRangeEnd, out topRangeEnd);
            return topRangeEnd;
        }

        /// <summary>
        /// Returns the floating point number that will match the value with the tolerance on the minimum size (i.e. the result is
        /// always smaller than the value)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maxNumbersBetween">The <c>ulps</c> difference.</param>
        /// <returns>The minimum floating point number which is <paramref name="maxNumbersBetween"/> smaller than the given <paramref name="value"/>.</returns>
        public static double MinimumMatchingFloatingPointNumber(this double value, long maxNumbersBetween)
        {
            double topRangeEnd, bottomRangeEnd;
            RangeOfMatchingFloatingPointNumbers(value, maxNumbersBetween, out bottomRangeEnd, out topRangeEnd);
            return bottomRangeEnd;
        }

        /// <summary>
        /// Determines the range of <c>ulps</c> that will match the specified value with the given tolerance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="relativeDifference">The relative difference.</param>
        /// <param name="bottomRangeEnd">The number of ULPS between the <c>value</c> and the <c>value - relativeDifference</c>.</param>
        /// <param name="topRangeEnd">The number of ULPS between the <c>value</c> and the <c>value + relativeDifference</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="relativeDifference"/> is smaller than zero.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="value"/> is <c>double.PositiveInfinity</c> or <c>double.NegativeInfinity</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="value"/> is <c>double.NaN</c>.
        /// </exception>
        public static void RangeOfMatchingNumbers(this double value, double relativeDifference, out long bottomRangeEnd, out long topRangeEnd)
        {
            // Make sure the relative is non-negative 
            if (relativeDifference < 0)
            {
                throw new ArgumentOutOfRangeException("relativeDifference");
            }

            // If the value is infinity (positive or negative) then
            // we can't determine the range.
            if (double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException("value");
            }

            // If the value is a NaN then we can't determine the range.
            if (double.IsNaN(value))
            {
                throw new ArgumentOutOfRangeException("value");
            }

            // If the value is zero (0.0) then we can't calculate the relative difference
            // so return the ulps counts for the difference.
            if (value.Equals(0))
            {
                topRangeEnd = GetLongFromDouble(relativeDifference);
                bottomRangeEnd = topRangeEnd;
                return;
            }

            // Calculate the ulps for the maximum and minimum values
            // Note that these can overflow
            long max = GetDirectionalLongFromDouble(value + (relativeDifference * Math.Abs(value)));
            long min = GetDirectionalLongFromDouble(value - (relativeDifference * Math.Abs(value)));

            // Calculate the ulps from the value
            long intValue = GetDirectionalLongFromDouble(value);

            // Determine the ranges
            topRangeEnd = Math.Abs(max - intValue);
            bottomRangeEnd = Math.Abs(intValue - min);
        }

        /// <summary>
        /// Evaluates the count of numbers between two double numbers
        /// </summary>
        /// <param name="a">The first parameter.</param>
        /// <param name="b">The second parameter.</param>
        /// <remarks>The second number is included in the number, thus two equal numbers evaluate to zero and two neighbor numbers evaluate to one. Therefore, what is returned is actually the count of numbers between plus 1.</remarks>
        /// <returns>The number of floating point values between <paramref name="a"/> and <paramref name="b"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="a"/> is <c>double.PositiveInfinity</c> or <c>double.NegativeInfinity</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="a"/> is <c>double.NaN</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="b"/> is <c>double.PositiveInfinity</c> or <c>double.NegativeInfinity</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="b"/> is <c>double.NaN</c>.
        /// </exception>
        [CLSCompliant(false)]
        public static ulong NumbersBetween(this double a, double b)
        {
            if (double.IsNaN(a) || double.IsInfinity(a))
            {
                throw new ArgumentOutOfRangeException("a");
            }

            if (double.IsNaN(b) || double.IsInfinity(b))
            {
                throw new ArgumentOutOfRangeException("b");
            }

            // Calculate the ulps for the maximum and minimum values
            // Note that these can overflow
            long intA = GetDirectionalLongFromDouble(a);
            long intB = GetDirectionalLongFromDouble(b);

            // Now find the number of values between the two doubles. This should not overflow
            // given that there are more long values than there are double values
            return (a >= b) ? (ulong)(intA - intB) : (ulong)(intB - intA);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual(this double a, double b)
        {
            double diff = a - b;
            return AlmostEqualWithError(a, b, diff, _defaultDoubleRelativeAccuracy);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual(this float a, float b)
        {
            double diff = a - b;
            return AlmostEqualWithError(a, b, diff, _defaultSingleRelativeAccuracy);
        }

        /// <summary>
        /// Checks whether two Compex numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual(this Complex a, Complex b)
        {
            double diff = a.NormOfDifference(b);
            return AlmostEqualWithError(a.Norm(), b.Norm(), diff, _defaultDoubleRelativeAccuracy);
        }

        /// <summary>
        /// Checks whether two Compex numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual(this Complex32 a, Complex32 b)
        {
            double diff = ((IPrecisionSupport<Complex32>)a).NormOfDifference(b);
            return AlmostEqualWithError(((IPrecisionSupport<Complex32>)a).Norm(), ((IPrecisionSupport<Complex32>)b).Norm(), diff, _defaultSingleRelativeAccuracy);
        }
        /// <summary>
        /// Checks whether two structures with precision support are almost equal. 
        /// </summary>
        /// <typeparam name="T">The type of the structures. Must implement <see cref="IPrecisionSupport{T}"/>.</typeparam>
        /// <param name="a">The first structure</param>
        /// <param name="b">The second structure</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual<T>(this T a, T b)
            where T : IPrecisionSupport<T>
        {
            double diff = a.NormOfDifference(b);
            return AlmostEqualWithError(a.Norm(), b.Norm(), diff, _defaultDoubleRelativeAccuracy);
        }

        /// <summary>
        /// Compares two complex and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        /// <returns>
        /// <see langword="true" /> if both complex are almost equal up to the
        /// specified maximum error, <see langword="false" /> otherwise.
        /// </returns>
        public static bool AlmostEqualWithError(this Complex a, Complex b, double maximumError)
        {
            double diff = a.NormOfDifference(b);
            return AlmostEqualWithError(a.Norm(), b.Norm(), diff, maximumError);
        }

        /// <summary>
        /// Compares two complex and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        /// <returns>
        /// <see langword="true" /> if both complex are almost equal up to the
        /// specified maximum error, <see langword="false" /> otherwise.
        /// </returns>
        public static bool AlmostEqualWithError(this float a, float b, double maximumError)
        {
            return AlmostEqualWithError(a, b, a - b, maximumError);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        /// <returns>
        /// <see langword="true" /> if both doubles are almost equal up to the
        /// specified maximum error, <see langword="false" /> otherwise.
        /// </returns>
        public static bool AlmostEqualWithError(this double a, double b, double maximumError)
        {
            return AlmostEqualWithError(a, b, a - b, maximumError);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumError">
        /// The accuracy required for being almost equal.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if both doubles are almost equal up to the specified
        /// maximum error, <see langword="false" /> otherwise.
        /// </returns>
        public static bool AlmostEqualListWithError(this IList<double> a, IList<double> b, double maximumError)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null || b == null || a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!AlmostEqualWithError(a[i], b[i], a[i] - b[i], maximumError))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumError">
        /// The accuracy required for being almost equal.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if both doubles are almost equal up to the specified
        /// maximum error, <see langword="false" /> otherwise.
        /// </returns>
        public static bool AlmostEqualListWithError(this IList<Complex> a, IList<Complex> b, double maximumError)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null || b == null || a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!AlmostEqualWithError(a[i].Norm(), b[i].Norm(), a[i].NormOfDifference(b[i]), maximumError))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compares two structure with precision support and determines if they are equal
        /// within the specified maximum relative error.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the structures. Must implement <see cref="IPrecisionSupport{T}"/>.
        /// </typeparam>
        /// <param name="a">The first structure.</param>
        /// <param name="b">The second structure.</param>
        /// <param name="maximumError">
        /// The accuracy required for being almost equal.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if both doubles are almost equal up to the specified
        /// maximum relative error, <see langword="false" /> otherwise.
        /// </returns>
        public static bool AlmostEqualWithError<T>(this T a, T b, double maximumError)
            where T : IPrecisionSupport<T>
        {
            return AlmostEqualWithError(a.Norm(), b.Norm(), a.NormOfDifference(b), maximumError);
        }

        /// <summary>
        /// Compares two lists of structures with precision support and determines if they
        /// are equal within the specified maximum error.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the structures. Must implement <see cref="IPrecisionSupport{T}"/>.
        /// </typeparam>
        /// <param name="a">The first structure list.</param>
        /// <param name="b">The second structure list.</param>
        /// <param name="maximumError">
        /// The accuracy required for being almost equal.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if both doubles are almost equal up to the specified
        /// maximum error, <see langword="false" /> otherwise.
        /// </returns>
        public static bool AlmostEqualListWithError<T>(this IList<T> a, IList<T> b, double maximumError)
            where T : IPrecisionSupport<T>
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null || b == null || a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!AlmostEqualWithError(a[i].Norm(), b[i].Norm(), a[i].NormOfDifference(b[i]), maximumError))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal within the specified
        /// maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="diff">
        /// The difference of the two values (according to some norm).
        /// </param>
        /// <param name="maximumError">
        /// The accuracy required for being almost equal.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if both doubles are almost equal up to the specified
        /// maximum error, <see langword="false" /> otherwise.
        /// </returns>
        public static bool AlmostEqualWithError(this double a, double b, double diff, double maximumError)
        {
            // If A or B are infinity (positive or negative) then
            // only return true if they are exactly equal to each other -
            // that is, if they are both infinities of the same sign.
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a == b;
            }

            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves.
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            if (Math.Abs(a) < _doubleMachinePrecision || Math.Abs(b) < _doubleMachinePrecision)
            {
                return AlmostEqualWithAbsoluteError(a, b, diff, maximumError);
            }

            return AlmostEqualWithRelativeError(a, b, diff, maximumError);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal within the specified
        /// maximum absolute error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="diff">
        /// The difference of the two values (according to some norm).
        /// </param>
        /// <param name="maximumAbsoluteError">
        /// The absolute accuracy required for being almost equal.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if both doubles are almost equal up to the specified
        /// maximum absolute error, <see langword="false" /> otherwise.
        /// </returns>
        public static bool AlmostEqualWithAbsoluteError(this double a, double b, double diff, double maximumAbsoluteError)
        {
            // If A or B are infinity (positive or negative) then
            // only return true if they are exactly equal to each other -
            // that is, if they are both infinities of the same sign.
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a == b;
            }

            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves.
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return Math.Abs(diff) < maximumAbsoluteError;
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal within the specified
        /// maximum relative error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="diff">The difference of the two values (according to some norm).
        /// </param>
        /// <param name="maximumRelativeError">The relative accuracy required for being
        /// almost equal.</param>
        /// <returns>
        /// <see langword="true" /> if both doubles are almost equal up to the specified
        /// maximum relative error, <see langword="false" /> otherwise.
        /// </returns>
        public static bool AlmostEqualWithRelativeError(this double a, double b, double diff, double maximumRelativeError)
        {
            // If A or B are infinity (positive or negative) then
            // only return true if they are exactly equal to each other -
            // that is, if they are both infinities of the same sign.
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a == b;
            }

            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves.
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            if ((a == 0 && Math.Abs(b) < maximumRelativeError)
                || (b == 0 && Math.Abs(a) < maximumRelativeError))
            {
                return true;
            }

            return Math.Abs(diff) < maximumRelativeError * Math.Max(Math.Abs(a), Math.Abs(b));
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not. If the numbers
        /// are very close to zero an absolute difference is compared, otherwise the relative difference is compared.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 10^(-numberOfDecimalPlaces). We divide by 
        /// two so that we have half the range on each side of the numbers, e.g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between 
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><see langword="true" /> if both doubles are equal to each other within the specified number of decimal places; otherwise <see langword="false" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="decimalPlaces"/> is smaller than zero.
        /// </exception>
        public static bool AlmostEqualInDecimalPlaces(this double a, double b, int decimalPlaces)
        {
            if (decimalPlaces <= 0)
            {
                // Can't have a negative number of decimal places
                throw new ArgumentOutOfRangeException("decimalPlaces");
            }
            
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves.
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            // If A or B are infinity (positive or negative) then
            // only return true if they are exactly equal to each other -
            // that is, if they are both infinities of the same sign.
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a == b;
            }

            if (Math.Abs(a) < _doubleMachinePrecision || Math.Abs(b) < _doubleMachinePrecision)
            {
                return AlmostEqualWithAbsoluteDecimalPlaces(a, b, decimalPlaces);
            }
            
            // If both numbers are equal, get out now. This should remove the possibility of both numbers being zero
            // and any problems associated with that.
            if (a.Equals(b))
            {
                return true;
            }

            return AlmostEqualWithRelativeDecimalPlaces(a, b, decimalPlaces);
        }

        /// <summary>
        /// Compares two floats and determines if they are equal to within the specified number of decimal places or not. If the numbers
        /// are very close to zero an absolute difference is compared, otherwise the relative difference is compared.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 10^(-numberOfDecimalPlaces). We divide by 
        /// two so that we have half the range on each side of the numbers, e.g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between 
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><see langword="true" /> if both doubles are equal to each other within the specified number of decimal places; otherwise <see langword="false" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="decimalPlaces"/> is smaller than zero.
        /// </exception>
        public static bool AlmostEqualInDecimalPlaces(this float a, float b, int decimalPlaces)
        {
            if (decimalPlaces <= 0)
            {
                // Can't have a negative number of decimal places
                throw new ArgumentOutOfRangeException("decimalPlaces");
            }

            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves.
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            // If A or B are infinity (positive or negative) then
            // only return true if they are exactly equal to each other -
            // that is, if they are both infinities of the same sign.
            if (float.IsInfinity(a) || float.IsInfinity(b))
            {
                return a == b;
            }

            if (Math.Abs(a) < _singleMachinePrecision || Math.Abs(b) < _singleMachinePrecision)
            {
                return AlmostEqualWithAbsoluteDecimalPlaces(a, b, decimalPlaces);
            }

            // If both numbers are equal, get out now. This should remove the possibility of both numbers being zero
            // and any problems associated with that.
            if (a.Equals(b))
            {
                return true;
            }

            return AlmostEqualWithRelativeDecimalPlaces(a, b, decimalPlaces);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 10^(-numberOfDecimalPlaces). We divide by 
        /// two so that we have half the range on each side of the numbers, e.g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between 
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><see langword="true" /> if both doubles are equal to each other within the specified number of decimal places; otherwise <see langword="false" />.</returns>
        private static bool AlmostEqualWithRelativeDecimalPlaces(this double a, double b, int decimalPlaces)
        {
            // If the magnitudes of the two numbers are equal to within one magnitude the numbers could potentially be equal
            int magnitudeOfFirst = Magnitude(a);
            int magnitudeOfSecond = Magnitude(b);
            if (Math.Max(magnitudeOfFirst, magnitudeOfSecond) > (Math.Min(magnitudeOfFirst, magnitudeOfSecond) + 1))
            {
                return false;
            }

            // Get the power of the number of decimalPlaces
            double decimalPlaceMagnitude = Math.Pow(10, -(decimalPlaces - 1));

            // The values are equal if the difference between the two numbers is smaller than
            // 10^(-numberOfDecimalPlaces). We divide by two so that we have half the range
            // on each side of the numbers, e.g. if decimalPlaces == 2, 
            // then 0.01 will equal between 0.005 and 0.015, but not 0.02 and not 0.00
            double maxDifference = decimalPlaceMagnitude / 2.0;

            return a > b
                ? (a*Math.Pow(10, -magnitudeOfFirst)) - maxDifference < (b*Math.Pow(10, -magnitudeOfFirst))
                : (b*Math.Pow(10, -magnitudeOfSecond)) - maxDifference < (a*Math.Pow(10, -magnitudeOfSecond));
        }

        /// <summary>
        /// Compares two floats and determines if they are equal to within the specified number of decimal places or not. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 10^(-numberOfDecimalPlaces). We divide by 
        /// two so that we have half the range on each side of the numbers, e.g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between 
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><see langword="true" /> if both floats are equal to each other within the specified number of decimal places; otherwise <see langword="false" />.</returns>
        private static bool AlmostEqualWithRelativeDecimalPlaces(this float a, float b, int decimalPlaces)
        {
            // If the magnitudes of the two numbers are equal to within one magnitude the numbers could potentially be equal
            int magnitudeOfFirst = Magnitude(a);
            int magnitudeOfSecond = Magnitude(b);
            if (Math.Max(magnitudeOfFirst, magnitudeOfSecond) > (Math.Min(magnitudeOfFirst, magnitudeOfSecond) + 1))
            {
                return false;
            }

            // Get the power of the number of decimalPlaces
            var decimalPlaceMagnitude = (float)Math.Pow(10, -(decimalPlaces - 1));

            // The values are equal if the difference between the two numbers is smaller than
            // 10^(-numberOfDecimalPlaces). We divide by two so that we have half the range
            // on each side of the numbers, e.g. if decimalPlaces == 2, 
            // then 0.01 will equal between 0.005 and 0.015, but not 0.02 and not 0.00
            float maxDifference = decimalPlaceMagnitude / 2.0f;

            return a > b
                ? (a*(float) Math.Pow(10, -magnitudeOfFirst)) - maxDifference < (b*(float) Math.Pow(10, -magnitudeOfFirst))
                : (b*(float) Math.Pow(10, -magnitudeOfSecond)) - maxDifference < (a*(float) Math.Pow(10, -magnitudeOfSecond));
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not, using the 
        /// number of decimal places as an absolute measure.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 10^(-numberOfDecimalPlaces). We divide by 
        /// two so that we have half the range on each side of the numbers, e.g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between 
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><see langword="true" /> if both doubles are equal to each other within the specified number of decimal places; otherwise <see langword="false" />.</returns>
        private static bool AlmostEqualWithAbsoluteDecimalPlaces(this double a, double b, int decimalPlaces)
        {
            double decimalPlaceMagnitude = Math.Pow(10, -(decimalPlaces - 1));
            
            // The values are equal if the difference between the two numbers is smaller than
            // 10^(-numberOfDecimalPlaces). We divide by two so that we have half the range
            // on each side of the numbers, e.g. if decimalPlaces == 2, 
            // then 0.01 will equal between 0.005 and 0.015, but not 0.02 and not 0.00
            return Math.Abs((a - b)) < decimalPlaceMagnitude / 2.0;
        }

        /// <summary>
        /// Compares two floats and determines if they are equal to within the specified number of decimal places or not, using the 
        /// number of decimal places as an absolute measure.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 10^(-numberOfDecimalPlaces). We divide by 
        /// two so that we have half the range on each side of the numbers, e.g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between 
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><see langword="true" /> if both floats are equal to each other within the specified number of decimal places; otherwise <see langword="false" />.</returns>
        private static bool AlmostEqualWithAbsoluteDecimalPlaces(this float a, float b, int decimalPlaces)
        {
            var decimalPlaceMagnitude = (float)Math.Pow(10, -(decimalPlaces - 1));

            // The values are equal if the difference between the two numbers is smaller than
            // 10^(-numberOfDecimalPlaces). We divide by two so that we have half the range
            // on each side of the numbers, e.g. if decimalPlaces == 2, 
            // then 0.01 will equal between 0.005 and 0.015, but not 0.02 and not 0.00
            return Math.Abs((a - b)) < decimalPlaceMagnitude / 2.0f;
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the tolerance or not. Equality comparison is based on the binary representation.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Determines the 'number' of floating point numbers between two values (i.e. the number of discrete steps 
        /// between the two numbers) and then checks if that is within the specified tolerance. So if a tolerance 
        /// of 1 is passed then the result will be true only if the two numbers have the same binary representation 
        /// OR if they are two adjacent numbers that only differ by one step.
        /// </para>
        /// <para>
        /// The comparison method used is explained in http://www.cygnus-software.com/papers/comparingfloats/comparingfloats.htm . The article
        /// at http://www.extremeoptimization.com/resources/Articles/FPDotNetConceptsAndFormats.aspx explains how to transform the C code to 
        /// .NET enabled code without using pointers and unsafe code.
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum number of floating point values between the two values. Must be 1 or larger.</param>
        /// <returns><see langword="true" /> if both doubles are equal to each other within the specified tolerance; otherwise <see langword="false" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="maxNumbersBetween"/> is smaller than one.
        /// </exception>
        public static bool AlmostEqual(this double a, double b, long maxNumbersBetween)
        {
            // Make sure maxNumbersBetween is non-negative and small enough that the
            // default NAN won't compare as equal to anything.
            if (maxNumbersBetween < 1)
            {
                throw new ArgumentOutOfRangeException("maxNumbersBetween");
            }

            // If A or B are infinity (positive or negative) then
            // only return true if they are exactly equal to each other -
            // that is, if they are both infinities of the same sign.
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a == b;
            }

            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves.
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            // Get the first double and convert it to an integer value (by using the binary representation)
            long firstUlong = GetDirectionalLongFromDouble(a);

            // Get the second double and convert it to an integer value (by using the binary representation)
            long secondUlong = GetDirectionalLongFromDouble(b);

            // Now compare the values. 
            // Note that this comparison can overflow so we'll approach this differently
            // Do note that we could overflow this way too. We should probably check that we don't.
            return (a > b) ? (secondUlong + maxNumbersBetween >= firstUlong) : (firstUlong + maxNumbersBetween >= secondUlong);
        }

        /// <summary>
        /// Compares two floats and determines if they are equal to within the tolerance or not. Equality comparison is based on the binary representation.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum number of floating point values between the two values. Must be 1 or larger.</param>
        /// <returns><see langword="true" /> if both floats are equal to each other within the specified tolerance; otherwise <see langword="false" />.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="maxNumbersBetween"/> is smaller than one.
        /// </exception>
        public static bool AlmostEqual(this float a, float b, int maxNumbersBetween)
        {
            // Make sure maxNumbersBetween is non-negative and small enough that the
            // default NAN won't compare as equal to anything.
            if (maxNumbersBetween < 1)
            {
                throw new ArgumentOutOfRangeException("maxNumbersBetween");
            }

            // If A or B are infinity (positive or negative) then
            // only return true if they are exactly equal to each other -
            // that is, if they are both infinities of the same sign.
            if (float.IsInfinity(a) || float.IsInfinity(b))
            {
                return a == b;
            }

            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves.
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            // Get the first float and convert it to an integer value (by using the binary representation)
            int firstUlong = GetDirectionalIntFromFloat(a);

            // Get the second float and convert it to an integer value (by using the binary representation)
            int secondUlong = GetDirectionalIntFromFloat(b);

            // Now compare the values. 
            // Note that this comparison can overflow so we'll approach this differently
            // Do note that we could overflow this way too. We should probably check that we don't.
            return (a > b) ? (secondUlong + maxNumbersBetween >= firstUlong) : (firstUlong + maxNumbersBetween >= secondUlong);
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is larger than the <c>second</c>
        /// value to within the tolerance or not. Equality comparison is based on the binary representation.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum number of floating point values for which the two values are considered equal. Must be 1 or larger.</param>
        /// <returns><c>true</c> if the first value is larger than the second value; otherwise <c>false</c>.</returns>
        public static bool IsLarger(this double a, double b, long maxNumbersBetween)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, maxNumbersBetween) > 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is larger than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 10^(-numberOfDecimalPlaces). We divide by 
        /// two so that we have half the range on each side of the numbers, e.g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between 
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><c>true</c> if the first value is larger than the second value; otherwise <c>false</c>.</returns>
        public static bool IsLargerWithDecimalPlaces(this double a, double b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareToInDecimalPlaces(a, b, decimalPlaces) > 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the tolerance or not. Equality comparison is based on the binary representation.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum number of floating point values for which the two values are considered equal. Must be 1 or larger.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmaller(this double a, double b, long maxNumbersBetween)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, maxNumbersBetween) < 0;
        }

        /// <summary>
        /// Compares two floats and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the tolerance or not. Equality comparison is based on the binary representation.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum number of floating point values for which the two values are considered equal. Must be 1 or larger.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmaller(this float a, float b, long maxNumbersBetween)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, maxNumbersBetween) < 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 10^(-numberOfDecimalPlaces). We divide by 
        /// two so that we have half the range on each side of th<paramref name="decimalPlaces"/>g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between 
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmallerWithDecimalPlaces(this double a, double b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareToInDecimalPlaces(a, b, decimalPlaces) < 0;
        }
        
        ///<summary>
        /// Compares two floats and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 10^(-numberOfDecimalPlaces). We divide by 
        /// two so that we have half the range on each side of th<paramref name="decimalPlaces"/>g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between 
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmallerWithDecimalPlaces(this float a, float b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareToInDecimalPlaces(a, b, decimalPlaces) < 0;
        }

        /// <summary>
        /// Compares two doubles and determines which double is bigger.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum error in terms of Units in Last Place (<c>ulps</c>), i.e. the maximum number of decimals that may be different. Must be 1 or larger.</param>
        /// <returns>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Return value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term>-1</term>
        ///         <description><paramref name="a"/> is smaller than <paramref name="b"/> by more than the <paramref name="maxNumbersBetween"/> tolerance.</description>
        ///     </item>
        ///     <item>
        ///         <term>0</term>
        ///         <description><paramref name="a"/> is equal to <paramref name="b"/> within the <paramref name="maxNumbersBetween"/> tolerance.</description>
        ///     </item>
        ///     <item>
        ///         <term>1</term>
        ///         <description><paramref name="a"/> is bigger than <paramref name="b"/> by more than the <paramref name="maxNumbersBetween"/> tolerance.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public static int CompareTo(this double a, double b, long maxNumbersBetween)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return a.CompareTo(b);
            }

            // If A or B are infinity (positive or negative) then
            // only return true if first is smaller
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a.CompareTo(b);
            }

            // If the numbers are equal to within the tolerance then
            // there's technically no difference
            if (AlmostEqual(a, b, maxNumbersBetween))
            {
                return 0;
            }

            return a.CompareTo(b);
        }

        /// <summary>
        /// Compares two doubles and determines which double is bigger.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places on which the values must be compared. Must be 1 or larger.</param>
        /// <returns>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Return value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term>-1</term>
        ///         <description><paramref name="a"/> is smaller than <paramref name="b"/> by more than a magnitude equal to <paramref name="decimalPlaces"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>0</term>
        ///         <description><paramref name="a"/> is equal to <paramref name="b"/> within a magnitude equal to <paramref name="decimalPlaces"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>1</term>
        ///         <description><paramref name="a"/> is bigger than <paramref name="b"/> by more than a magnitude equal to <paramref name="decimalPlaces"/>.</description>
        ///     </item>
        /// </list>
        /// </returns>
        public static int CompareToInDecimalPlaces(this double a, double b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return a.CompareTo(b);
            }

            // If A or B are infinity (positive or negative) then
            // only return true if first is smaller
            if (double.IsInfinity(a) || double.IsInfinity(b))
            {
                return a.CompareTo(b);
            }

            // If the numbers are equal to within the number of decimal places
            // then there's technically no difference
            if (AlmostEqualInDecimalPlaces(a, b, decimalPlaces))
            {
                return 0;
            }

            // The numbers differ by more than the decimal places, so
            // we can check the normal way to see if the first is
            // larger than the second.
            return a.CompareTo(b);
        }

        /// <summary>
        /// Evaluates the minimum distance to the next distinguishable number near the argument value.
        /// </summary>
        /// <param name="value">The value used to determine the minimum distance.</param>
        /// <returns>
        /// Relative Epsilon (positive double or NaN).
        /// </returns>
        /// <remarks>Evaluates the <b>negative</b> epsilon. The more common positive epsilon is equal to two times this negative epsilon.</remarks>
        /// <seealso cref="PositiveEpsilonOf(double)"/>
        public static double EpsilonOf(this double value)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                return double.NaN;
            }

#if PORTABLE
            long signed64 = DoubleToInt64Bits(value);
            if (signed64 == 0)
            {
                signed64++;
                return Int64BitsToDouble(signed64) - value;
            }
            if (signed64-- < 0)
            {
                return Int64BitsToDouble(signed64) - value;
            }
            return value - Int64BitsToDouble(signed64);
#else
            long signed64 = BitConverter.DoubleToInt64Bits(value);
            if (signed64 == 0)
            {
                signed64++;
                return BitConverter.Int64BitsToDouble(signed64) - value;
            }
            if (signed64-- < 0)
            {
                return BitConverter.Int64BitsToDouble(signed64) - value;
            }
            return value - BitConverter.Int64BitsToDouble(signed64);
#endif
        }

        /// <summary>
        /// Evaluates the minimum distance to the next distinguishable number near the argument value.
        /// </summary>
        /// <param name="value">The value used to determine the minimum distance.</param>
        /// <returns>Relative Epsilon (positive double or NaN)</returns>
        /// <remarks>Evaluates the <b>positive</b> epsilon. See also <see cref="EpsilonOf"/></remarks>
        /// <seealso cref="EpsilonOf(double)"/>
        public static double PositiveEpsilonOf(this double value)
        {
            return 2 * EpsilonOf(value);
        }

        /// <summary>
        /// Converts a float valut to a bit array stored in an int.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The bit array.</returns>
        internal static int FloatToInt32Bits(float value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

#if PORTABLE
        internal static long DoubleToInt64Bits(double value)
        {
            var union = new DoubleLongUnion {Double = value};
            return union.Int64;
        }

        internal static double Int64BitsToDouble(long value)
        {
            var union = new DoubleLongUnion {Int64 = value};
            return union.Double;
        }

        internal static double Truncate(double value)
        {
            return value >= 0.0 ? Math.Floor(value) : Math.Ceiling(value);
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DoubleLongUnion
        {
            [FieldOffset(0)]
            public double Double;

            [FieldOffset(0)]
            public long Int64;
        }
#endif
    }
}