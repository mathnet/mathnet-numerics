// <copyright file="Precision.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
using System.Numerics;
using System.Runtime;
using System.Runtime.InteropServices;

namespace MathNet.Numerics
{
    /// <summary>
    /// Support Interface for Precision Operations (like AlmostEquals).
    /// </summary>
    /// <typeparam name="T">Type of the implementing class.</typeparam>
    public interface IPrecisionSupport<in T>
    {
        /// <summary>
        /// Returns a Norm of a value of this type, which is appropriate for measuring how
        /// close this value is to zero.
        /// </summary>
        /// <returns>A norm of this value.</returns>
        double Norm();

        /// <summary>
        /// Returns a Norm of the difference of two values of this type, which is
        /// appropriate for measuring how close together these two values are.
        /// </summary>
        /// <param name="otherValue">The value to compare with.</param>
        /// <returns>A norm of the difference between this and the other value.</returns>
        double NormOfDifference(T otherValue);
    }

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
    public static partial class Precision
    {
        /// <summary>
        /// The number of binary digits used to represent the binary number for a double precision floating
        /// point value. i.e. there are this many digits used to represent the
        /// actual number, where in a number as: 0.134556 * 10^5 the digits are 0.134556 and the exponent is 5.
        /// </summary>
        const int DoubleWidth = 53;

        /// <summary>
        /// The number of binary digits used to represent the binary number for a single precision floating
        /// point value. i.e. there are this many digits used to represent the
        /// actual number, where in a number as: 0.134556 * 10^5 the digits are 0.134556 and the exponent is 5.
        /// </summary>
        const int SingleWidth = 24;

        /// <summary>
        /// Standard epsilon, the maximum relative precision of IEEE 754 double-precision floating numbers (64 bit).
        /// According to the definition of Prof. Demmel and used in LAPACK and Scilab.
        /// </summary>
        public static readonly double DoublePrecision = Math.Pow(2, -DoubleWidth);

        /// <summary>
        /// Standard epsilon, the maximum relative precision of IEEE 754 double-precision floating numbers (64 bit).
        /// According to the definition of Prof. Higham and used in the ISO C standard and MATLAB.
        /// </summary>
        public static readonly double PositiveDoublePrecision = 2*DoublePrecision;

        /// <summary>
        /// Standard epsilon, the maximum relative precision of IEEE 754 single-precision floating numbers (32 bit).
        /// According to the definition of Prof. Demmel and used in LAPACK and Scilab.
        /// </summary>
        public static readonly double SinglePrecision = Math.Pow(2, -SingleWidth);

        /// <summary>
        /// Standard epsilon, the maximum relative precision of IEEE 754 single-precision floating numbers (32 bit).
        /// According to the definition of Prof. Higham and used in the ISO C standard and MATLAB.
        /// </summary>
        public static readonly double PositiveSinglePrecision = 2*SinglePrecision;

        /// <summary>
        /// Actual double precision machine epsilon, the smallest number that can be subtracted from 1, yielding a results different than 1.
        /// This is also known as unit roundoff error. According to the definition of Prof. Demmel.
        /// On a standard machine this is equivalent to `DoublePrecision`.
        /// </summary>
        public static readonly double MachineEpsilon = MeasureMachineEpsilon();

        /// <summary>
        /// Actual double precision machine epsilon, the smallest number that can be added to 1, yielding a results different than 1.
        /// This is also known as unit roundoff error. According to the definition of Prof. Higham.
        /// On a standard machine this is equivalent to `PositiveDoublePrecision`.
        /// </summary>
        public static readonly double PositiveMachineEpsilon = MeasurePositiveMachineEpsilon();

        /// <summary>
        /// The number of significant decimal places of double-precision floating numbers (64 bit).
        /// </summary>
        public static readonly int DoubleDecimalPlaces = (int) Math.Floor(Math.Abs(Math.Log10(DoublePrecision)));

        /// <summary>
        /// The number of significant decimal places of single-precision floating numbers (32 bit).
        /// </summary>
        public static readonly int SingleDecimalPlaces = (int) Math.Floor(Math.Abs(Math.Log10(SinglePrecision)));

        /// <summary>
        /// Value representing 10 * 2^(-53) = 1.11022302462516E-15
        /// </summary>
        static readonly double DefaultDoubleAccuracy = DoublePrecision*10;

        /// <summary>
        /// Value representing 10 * 2^(-24) = 5.96046447753906E-07
        /// </summary>
        static readonly float DefaultSingleAccuracy = (float) (SinglePrecision*10);

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
            var truncated = (int)Truncate(magnitude);

            // To get the right number we need to know if the value is negative or positive
            // truncating a positive number will always give use the correct magnitude
            // truncating a negative number will give us a magnitude that is off by 1 (unless integer)
            return magnitude < 0d && truncated != magnitude
                ? truncated - 1
                : truncated;
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
            var truncated = (int)Truncate(magnitude);

            // To get the right number we need to know if the value is negative or positive
            // truncating a positive number will always give use the correct magnitude
            // truncating a negative number will give us a magnitude that is off by 1 (unless integer)
            return magnitude < 0f && truncated != magnitude
                ? truncated - 1
                : truncated;
        }

        /// <summary>
        /// Returns the number divided by it's magnitude, effectively returning a number between -10 and 10.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The value of the number.</returns>
        public static double ScaleUnitMagnitude(this double value)
        {
            if (value.Equals(0.0))
            {
                return value;
            }

            int magnitude = Magnitude(value);
            return value*Math.Pow(10, -magnitude);
        }

        /// <summary>
        /// Returns a 'directional' long value. This is a long value which acts the same as a double,
        /// e.g. a negative double value will return a negative double value starting at 0 and going
        /// more negative as the double value gets more negative.
        /// </summary>
        /// <param name="value">The input double value.</param>
        /// <returns>A long value which is roughly the equivalent of the double value.</returns>
        static long AsDirectionalInt64(double value)
        {
            // Convert in the normal way.
            long result = BitConverter.DoubleToInt64Bits(value);

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
        static int AsDirectionalInt32(float value)
        {
            // Convert in the normal way.
            int result = SingleToInt32Bits(value);

            // Now find out where we're at in the range
            // If the value is larger/equal to zero then we can just return the value
            // if the value is negative we subtract int.MinValue from it.
            return (result >= 0) ? result : (int.MinValue - result);
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
        public static double Increment(this double value, int count = 1)
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
            long intValue = BitConverter.DoubleToInt64Bits(value);
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
            return BitConverter.Int64BitsToDouble(intValue);
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
        public static double Decrement(this double value, int count = 1)
        {
            if (double.IsInfinity(value) || double.IsNaN(value) || count == 0)
            {
                return value;
            }

            if (count < 0)
            {
                return Increment(value, -count);
            }

            // Translate the bit pattern of the double to an integer.
            // Note that this leads to:
            // double > 0 --> long > 0, growing as the double value grows
            // double < 0 --> long < 0, increasing in absolute magnitude as the double
            //                          gets closer to zero!
            //                          i.e. 0 - double.epsilon will give the largest long value!
            long intValue = BitConverter.DoubleToInt64Bits(value);

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
            return BitConverter.Int64BitsToDouble(intValue);
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
            return CoerceZero(a, (long) maxNumbersBetween);
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
                throw new ArgumentOutOfRangeException(nameof(maxNumbersBetween));
            }

            if (double.IsInfinity(a) || double.IsNaN(a))
            {
                return a;
            }

            // We allow maxNumbersBetween between 0 and the number so
            // we need to check if there a
            if (NumbersBetween(0.0, a) <= (ulong) maxNumbersBetween)
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
                throw new ArgumentOutOfRangeException(nameof(maximumAbsoluteError));
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
            return CoerceZero(a, DoublePrecision);
        }

        /// <summary>
        /// Determines the range of floating point numbers that will match the specified value with the given tolerance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maxNumbersBetween">The <c>ulps</c> difference.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="maxNumbersBetween"/> is smaller than zero.
        /// </exception>
        /// <returns>Tuple of the bottom and top range ends.</returns>
        public static (double, double) RangeOfMatchingFloatingPointNumbers(this double value, long maxNumbersBetween)
        {
            // Make sure ulpDifference is non-negative
            if (maxNumbersBetween < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxNumbersBetween));
            }

            // If the value is infinity (positive or negative) just
            // return the same infinity for the range.
            if (double.IsInfinity(value))
            {
                return (value, value);
            }

            // If the value is a NaN then the range is a NaN too.
            if (double.IsNaN(value))
            {
                return (double.NaN, double.NaN);
            }

            // Translate the bit pattern of the double to an integer.
            // Note that this leads to:
            // double > 0 --> long > 0, growing as the double value grows
            // double < 0 --> long < 0, increasing in absolute magnitude as the double
            //                          gets closer to zero!
            //                          i.e. 0 - double.epsilon will give the largest long value!
            long intValue = BitConverter.DoubleToInt64Bits(value);

            // We need to protect against over- and under-flow of the intValue when
            // we start to add the ulpsDifference.
            if (intValue < 0)
            {
                // Note that long.MinValue has the same bit pattern as
                // -0.0. Therefore we're working in opposite direction (i.e. add if we want to
                // go more negative and subtract if we want to go less negative)
                var topRangeEnd = Math.Abs(long.MinValue - intValue) < maxNumbersBetween
                    // Got underflow, which can be fixed by splitting the calculation into two bits
                    // first get the remainder of the intValue after subtracting it from the long.MinValue
                    // and add that to the ulpsDifference. That way we'll turn positive without underflow
                    ? BitConverter.Int64BitsToDouble(maxNumbersBetween + (long.MinValue - intValue))
                    // No problems here, move along.
                    : BitConverter.Int64BitsToDouble(intValue - maxNumbersBetween);

                var bottomRangeEnd = Math.Abs(intValue) < maxNumbersBetween
                    // Underflow, which means we'd have to go further than a long would allow us.
                    // Also we couldn't translate it back to a double, so we'll return -Double.MaxValue
                    ? -double.MaxValue
                    // intValue is negative. Adding the positive ulpsDifference means that it gets less negative.
                    // However due to the conversion way this means that the actual double value gets more negative :-S
                    : BitConverter.Int64BitsToDouble(intValue + maxNumbersBetween);

                return (bottomRangeEnd, topRangeEnd);
            }
            else
            {
                // IntValue is positive
                var topRangeEnd = long.MaxValue - intValue < maxNumbersBetween
                    // Overflow, which means we'd have to go further than a long would allow us.
                    // Also we couldn't translate it back to a double, so we'll return Double.MaxValue
                    ? double.MaxValue
                    // No troubles here
                    : BitConverter.Int64BitsToDouble(intValue + maxNumbersBetween);

                // Check the bottom range end for underflows
                var bottomRangeEnd = intValue > maxNumbersBetween
                    // No problems here. IntValue is larger than ulpsDifference so we'll end up with a
                    // positive number.
                    ? BitConverter.Int64BitsToDouble(intValue - maxNumbersBetween)
                    // Int value is bigger than zero but smaller than the ulpsDifference. So we'll need to deal with
                    // the reversal at the negative end
                    : BitConverter.Int64BitsToDouble(long.MinValue + (maxNumbersBetween - intValue));

                return (bottomRangeEnd, topRangeEnd);
            }
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
            return RangeOfMatchingFloatingPointNumbers(value, maxNumbersBetween).Item2;
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
            return RangeOfMatchingFloatingPointNumbers(value, maxNumbersBetween).Item1;
        }

        /// <summary>
        /// Determines the range of <c>ulps</c> that will match the specified value with the given tolerance.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="relativeDifference">The relative difference.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="relativeDifference"/> is smaller than zero.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="value"/> is <c>double.PositiveInfinity</c> or <c>double.NegativeInfinity</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="value"/> is <c>double.NaN</c>.
        /// </exception>
        /// <returns>
        /// Tuple with the number of ULPS between the <c>value</c> and the <c>value - relativeDifference</c> as first,
        /// and the number of ULPS between the <c>value</c> and the <c>value + relativeDifference</c> as second value.
        /// </returns>
        public static (long, long) RangeOfMatchingNumbers(this double value, double relativeDifference)
        {
            // Make sure the relative is non-negative
            if (relativeDifference < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(relativeDifference));
            }

            // If the value is infinity (positive or negative) then
            // we can't determine the range.
            if (double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            // If the value is a NaN then we can't determine the range.
            if (double.IsNaN(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            // If the value is zero (0.0) then we can't calculate the relative difference
            // so return the ulps counts for the difference.
            if (value.Equals(0))
            {
                var v = BitConverter.DoubleToInt64Bits(relativeDifference);
                return (v, v);
            }

            // Calculate the ulps for the maximum and minimum values
            // Note that these can overflow
            long max = AsDirectionalInt64(value + (relativeDifference*Math.Abs(value)));
            long min = AsDirectionalInt64(value - (relativeDifference*Math.Abs(value)));

            // Calculate the ulps from the value
            long intValue = AsDirectionalInt64(value);

            // Determine the ranges
            return (Math.Abs(intValue - min), Math.Abs(max - intValue));
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
                throw new ArgumentOutOfRangeException(nameof(a));
            }

            if (double.IsNaN(b) || double.IsInfinity(b))
            {
                throw new ArgumentOutOfRangeException(nameof(b));
            }

            // Calculate the ulps for the maximum and minimum values
            // Note that these can overflow
            long intA = AsDirectionalInt64(a);
            long intB = AsDirectionalInt64(b);

            // Now find the number of values between the two doubles. This should not overflow
            // given that there are more long values than there are double values
            return (a >= b) ? (ulong) (intA - intB) : (ulong) (intB - intA);
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
        }

        /// <summary>
        /// Evaluates the minimum distance to the next distinguishable number near the argument value.
        /// </summary>
        /// <param name="value">The value used to determine the minimum distance.</param>
        /// <returns>
        /// Relative Epsilon (positive float or NaN).
        /// </returns>
        /// <remarks>Evaluates the <b>negative</b> epsilon. The more common positive epsilon is equal to two times this negative epsilon.</remarks>
        /// <seealso cref="PositiveEpsilonOf(float)"/>
        public static float EpsilonOf(this float value)
        {
            if (float.IsInfinity(value) || float.IsNaN(value))
            {
                return float.NaN;
            }

            int signed32 = SingleToInt32Bits(value);
            if (signed32 == 0)
            {
                signed32++;
                return Int32BitsToSingle(signed32) - value;
            }
            if (signed32-- < 0)
            {
                return Int32BitsToSingle(signed32) - value;
            }
            return value - Int32BitsToSingle(signed32);
        }

        /// <summary>
        /// Evaluates the minimum distance to the next distinguishable number near the argument value.
        /// </summary>
        /// <param name="value">The value used to determine the minimum distance.</param>
        /// <returns>Relative Epsilon (positive double or NaN)</returns>
        /// <remarks>Evaluates the <b>positive</b> epsilon. See also <see cref="EpsilonOf(double)"/></remarks>
        /// <seealso cref="EpsilonOf(double)"/>
        public static double PositiveEpsilonOf(this double value)
        {
            return 2*EpsilonOf(value);
        }

        /// <summary>
        /// Evaluates the minimum distance to the next distinguishable number near the argument value.
        /// </summary>
        /// <param name="value">The value used to determine the minimum distance.</param>
        /// <returns>Relative Epsilon (positive float or NaN)</returns>
        /// <remarks>Evaluates the <b>positive</b> epsilon. See also <see cref="EpsilonOf(float)"/></remarks>
        /// <seealso cref="EpsilonOf(float)"/>
        public static float PositiveEpsilonOf(this float value)
        {
            return 2 * EpsilonOf(value);
        }

        /// <summary>
        /// Calculates the actual (negative) double precision machine epsilon - the smallest number that can be subtracted from 1, yielding a results different than 1.
        /// This is also known as unit roundoff error. According to the definition of Prof. Demmel.
        /// </summary>
        /// <returns>Positive Machine epsilon</returns>
        static double MeasureMachineEpsilon()
        {
            double eps = 1.0d;

            while ((1.0d - (eps / 2.0d)) < 1.0d)
                eps /= 2.0d;

            return eps;
        }

        /// <summary>
        /// Calculates the actual positive double precision machine epsilon - the smallest number that can be added to 1, yielding a results different than 1.
        /// This is also known as unit roundoff error. According to the definition of Prof. Higham.
        /// </summary>
        /// <returns>Machine epsilon</returns>
        static double MeasurePositiveMachineEpsilon()
        {
            double eps = 1.0d;

            while ((1.0d + (eps / 2.0d)) > 1.0d)
                eps /= 2.0d;

            return eps;
        }

        /// <summary>
        /// Round to a multiple of the provided positive basis.
        /// </summary>
        /// <param name="number">Number to be rounded.</param>
        /// <param name="basis">The basis to whose multiples to round to. Must be positive.</param>
        public static double RoundToMultiple(this double number, double basis)
        {
            return Math.Round(number / basis, MidpointRounding.AwayFromZero) * basis;
        }

        /// <summary>
        /// Round to a multiple of the provided positive basis.
        /// </summary>
        /// <param name="number">Number to be rounded.</param>
        /// <param name="basis">The basis to whose multiples to round to. Must be positive.</param>
        public static float RoundToMultiple(this float number, float basis)
        {
            return (float) RoundToMultiple((double) number, basis);
        }

        /// <summary>
        /// Round to a multiple of the provided positive basis.
        /// </summary>
        /// <param name="number">Number to be rounded.</param>
        /// <param name="basis">The basis to whose multiples to round to. Must be positive.</param>
        public static decimal RoundToMultiple(this decimal number, decimal basis)
        {
            return Math.Round(number / basis, MidpointRounding.AwayFromZero) * basis;
        }

        /// <summary>
        /// Round to a multiple of the provided positive basis.
        /// </summary>
        /// <param name="number">Number to be rounded.</param>
        /// <param name="basis">The basis to whose powers to round to. Must be positive.</param>
        public static double RoundToPower(this double number, double basis)
        {
            return number < 0.0
                ? -Math.Pow(basis, Math.Round(Math.Log(-number, basis), MidpointRounding.AwayFromZero))
                : Math.Pow(basis, Math.Round(Math.Log(number, basis), MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// Round to a multiple of the provided positive basis.
        /// </summary>
        /// <param name="number">Number to be rounded.</param>
        /// <param name="basis">The basis to whose powers to round to. Must be positive.</param>
        public static float RoundToPower(this float number, float basis)
        {
            return (float) RoundToPower((double) number, basis);
        }

        /// <summary>
        /// Round to the number closest to 10^(-decimals). Negative decimals to round within the integer part.
        /// </summary>
        /// <param name="number">Number to be rounded</param>
        /// <param name="digits">If positive the number of decimals to round to. If negative the number of digits within the integer part to round, e.g. -3 will wound to the closes 1000.</param>
        /// <example>To round 123456789 to hundreds Round(123456789, -2) = 123456800 </example>
        /// <returns>Rounded number</returns>
        public static double Round(this double number, int digits)
        {
            return digits >= 0
                ? Math.Round(number, digits, MidpointRounding.AwayFromZero)
                : RoundToMultiple(number, Math.Pow(10.0, -digits));
        }

        /// <summary>
        /// Round to the number closest to 10^(-decimals). Negative decimals to round within the integer part.
        /// </summary>
        /// <param name="number">Number to be rounded</param>
        /// <param name="digits">If positive the number of decimals to round to. If negative the number of digits within the integer part to round, e.g. -3 will wound to the closes 1000.</param>
        /// <example>To round 123456789 to hundreds Round(123456789, -2) = 123456800 </example>
        /// <returns>Rounded number</returns>
        public static float Round(this float number, int digits)
        {
            return (float) Round((double) number, digits);
        }

        /// <summary>
		/// Round to the number closest to 10^(-decimals). Negative decimals to round within the integer part.
		/// </summary>
		/// <param name="number">Number to be rounded</param>
        /// <param name="digits">If positive the number of decimals to round to. If negative the number of digits within the integer part to round, e.g. -3 will wound to the closes 1000.</param>
		/// <example>To round 123456789 to hundreds Round(123456789, -2) = 123456800 </example>
		/// <returns>Rounded number</returns>
		public static decimal Round(this decimal number, int digits)
        {
            return digits >= 0
                ? Math.Round(number, digits, MidpointRounding.AwayFromZero)
                : RoundToMultiple(number, (decimal) Math.Pow(10.0, -digits));
        }

		/// <summary>
		/// Round to the number closest to 10^(-decimals). Negative decimals to round within the integer part.
		/// </summary>
		/// <param name="number">Number to be rounded</param>
        /// <param name="digits">If positive the number of decimals to round to. If negative the number of digits within the integer part to round, e.g. -3 will wound to the closes 1000.</param>
		/// <example>To round 123456789 to hundreds Round(123456789, -2) = 123456800 </example>
		/// <returns>Rounded number</returns>
		public static int Round(this int number, int digits)
        {
            return digits >= 0
                ? number
                : (int) Round((decimal) number, digits);
        }

        /// <summary>
        /// Round to the number closest to 10^(-decimals). Negative decimals to round within the integer part.
        /// </summary>
        /// <param name="number">Number to be rounded</param>
        /// <param name="digits">If positive the number of decimals to round to. If negative the number of digits within the integer part to round, e.g. -3 will wound to the closes 1000.</param>
        /// <example>To round 123456789 to hundreds Round(123456789, -2) = 123456800 </example>
        /// <returns>Rounded number</returns>
        [CLSCompliant(false)]
        public static uint Round(this uint number, int digits)
        {
            return digits >= 0
                ? number
                : (uint) Round((decimal) number, digits);
        }

        /// <summary>
        /// Round to the number closest to 10^(-decimals). Negative decimals to round within the integer part.
        /// </summary>
        /// <param name="number">Number to be rounded</param>
        /// <param name="digits">If positive the number of decimals to round to. If negative the number of digits within the integer part to round, e.g. -3 will wound to the closes 1000.</param>
        /// <example>To round 123456789 to hundreds Round(123456789, -2) = 123456800 </example>
        /// <returns>Rounded number</returns>
        public static long Round(this long number, int digits)
        {
            return digits >= 0
                ? number
                : (long) Round((decimal) number, digits);
        }

        /// <summary>
        /// Round to the number closest to 10^(-decimals). Negative decimals to round within the integer part.
        /// </summary>
        /// <param name="number">Number to be rounded</param>
        /// <param name="digits">If positive the number of decimals to round to. If negative the number of digits within the integer part to round, e.g. -3 will wound to the closes 1000.</param>
        /// <example>To round 123456789 to hundreds Round(123456789, -2) = 123456800 </example>
        /// <returns>Rounded number</returns>
        [CLSCompliant(false)]
        public static ulong Round(this ulong number, int digits)
        {
            return digits >= 0
                ? number
                : (ulong) Round((decimal) number, digits);
        }

        /// <summary>
        /// Round to the number closest to 10^(-decimals). Negative decimals to round within the integer part.
        /// </summary>
        /// <param name="number">Number to be rounded</param>
        /// <param name="digits">If positive the number of decimals to round to. If negative the number of digits within the integer part to round, e.g. -3 will wound to the closes 1000.</param>
        /// <example>To round 123456789 to hundreds Round(123456789, -2) = 123456800 </example>
        /// <returns>Rounded number</returns>
        public static short Round(this short number, int digits)
        {
            return digits >= 0
                ? number
                : (short) Round((decimal) number, digits);
        }

        /// <summary>
        /// Round to the number closest to 10^(-decimals). Negative decimals to round within the integer part.
        /// </summary>
        /// <param name="number">Number to be rounded</param>
        /// <param name="digits">If positive the number of decimals to round to. If negative the number of digits within the integer part to round, e.g. -3 will wound to the closes 1000.</param>
        /// <example>To round 123456789 to hundreds Round(123456789, -2) = 123456800 </example>
        /// <returns>Rounded number</returns>
        [CLSCompliant(false)]
        public static ushort Round(this ushort number, int digits)
        {
            return digits >= 0
                ? number
                : (ushort) Round((decimal) number, digits);
        }

        /// <summary>
        /// Round to the number closest to 10^(-decimals). Negative decimals to round within the integer part.
        /// </summary>
        /// <param name="number">Number to be rounded</param>
        /// <param name="digits">If positive the number of decimals to round to. If negative the number of digits within the integer part to round, e.g. -3 will wound to the closes 1000.</param>
        /// <example>To round 123456789 to hundreds Round(123456789, -2) = 123456800 </example>
        /// <returns>Rounded number</returns>
        public static BigInteger Round(this BigInteger number, int digits)
        {
            if (digits >= 0)
            {
                return number;
            }

            var onelarger = number / BigInteger.Pow(10, (-digits)-1);
            var divided = onelarger / 10;
            var lastDigit = onelarger - divided * 10;
            if (lastDigit >= 5)
            {
                divided += 1;
            }

            return divided * BigInteger.Pow(10, -digits);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        static double Truncate(double value)
        {
            return Math.Truncate(value);
        }

        static int SingleToInt32Bits(float value)
        {
            var union = new SingleIntUnion { Single = value };
            return union.Int32;
        }

        static float Int32BitsToSingle(int value)
        {
            var union = new SingleIntUnion { Int32 = value };
            return union.Single;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct SingleIntUnion
        {
            [FieldOffset(0)]
            public float Single;

            [FieldOffset(0)]
            public int Int32;
        }
    }
}
