// <copyright file="Precision.Comparison.cs" company="Math.NET">
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

namespace MathNet.Numerics
{
    public static partial class Precision
    {
        /// <summary>
        /// Compares two doubles and determines which double is bigger.
        /// a &lt; b -> -1; a ~= b (almost equal according to parameter) -> 0; a &gt; b -> +1.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The absolute accuracy required for being almost equal.</param>
        public static int CompareTo(this double a, double b, double maximumAbsoluteError)
        {
            // NANs are equal to nothing,
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
            if (AlmostEqual(a, b, maximumAbsoluteError))
            {
                return 0;
            }

            // The numbers differ by more than the decimal places, so
            // we can check the normal way to see if the first is
            // larger than the second.
            return a.CompareTo(b);
        }

        /// <summary>
        /// Compares two doubles and determines which double is bigger.
        /// a &lt; b -> -1; a ~= b (almost equal according to parameter) -> 0; a &gt; b -> +1.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places on which the values must be compared. Must be 1 or larger.</param>
        public static int CompareTo(this double a, double b, int decimalPlaces)
        {
            // NANs are equal to nothing,
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
            if (AlmostEqual(a, b, decimalPlaces))
            {
                return 0;
            }

            // The numbers differ by more than the decimal places, so
            // we can check the normal way to see if the first is
            // larger than the second.
            return a.CompareTo(b);
        }

        /// <summary>
        /// Compares two doubles and determines which double is bigger.
        /// a &lt; b -> -1; a ~= b (almost equal according to parameter) -> 0; a &gt; b -> +1.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The relative accuracy required for being almost equal.</param>
        public static int CompareToRelative(this double a, double b, double maximumError)
        {
            // NANs are equal to nothing,
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
            if (AlmostEqualRelative(a, b, maximumError))
            {
                return 0;
            }

            // The numbers differ by more than the decimal places, so
            // we can check the normal way to see if the first is
            // larger than the second.
            return a.CompareTo(b);
        }

        /// <summary>
        /// Compares two doubles and determines which double is bigger.
        /// a &lt; b -> -1; a ~= b (almost equal according to parameter) -> 0; a &gt; b -> +1.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places on which the values must be compared. Must be 1 or larger.</param>
        public static int CompareToRelative(this double a, double b, int decimalPlaces)
        {
            // NANs are equal to nothing,
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
            if (AlmostEqualRelative(a, b, decimalPlaces))
            {
                return 0;
            }

            // The numbers differ by more than the decimal places, so
            // we can check the normal way to see if the first is
            // larger than the second.
            return a.CompareTo(b);
        }

        /// <summary>
        /// Compares two doubles and determines which double is bigger.
        /// a &lt; b -> -1; a ~= b (almost equal according to parameter) -> 0; a &gt; b -> +1.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum error in terms of Units in Last Place (<c>ulps</c>), i.e. the maximum number of decimals that may be different. Must be 1 or larger.</param>
        public static int CompareToNumbersBetween(this double a, double b, long maxNumbersBetween)
        {
            // NANs are equal to nothing,
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
            if (AlmostEqualNumbersBetween(a, b, maxNumbersBetween))
            {
                return 0;
            }

            return a.CompareTo(b);
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
        public static bool IsLarger(this double a, double b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, decimalPlaces) > 0;
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
        public static bool IsLarger(this float a, float b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, decimalPlaces) > 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is larger than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The absolute accuracy required for being almost equal.</param>
        /// <returns><c>true</c> if the first value is larger than the second value; otherwise <c>false</c>.</returns>
        public static bool IsLarger(this double a, double b, double maximumAbsoluteError)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, maximumAbsoluteError) > 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is larger than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The absolute accuracy required for being almost equal.</param>
        /// <returns><c>true</c> if the first value is larger than the second value; otherwise <c>false</c>.</returns>
        public static bool IsLarger(this float a, float b, double maximumAbsoluteError)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, maximumAbsoluteError) > 0;
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
        public static bool IsLargerRelative(this double a, double b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareToRelative(a, b, decimalPlaces) > 0;
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
        public static bool IsLargerRelative(this float a, float b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareToRelative(a, b, decimalPlaces) > 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is larger than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The relative accuracy required for being almost equal.</param>
        /// <returns><c>true</c> if the first value is larger than the second value; otherwise <c>false</c>.</returns>
        public static bool IsLargerRelative(this double a, double b, double maximumError)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareToRelative(a, b, maximumError) > 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is larger than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The relative accuracy required for being almost equal.</param>
        /// <returns><c>true</c> if the first value is larger than the second value; otherwise <c>false</c>.</returns>
        public static bool IsLargerRelative(this float a, float b, double maximumError)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareToRelative(a, b, maximumError) > 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is larger than the <c>second</c>
        /// value to within the tolerance or not. Equality comparison is based on the binary representation.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum number of floating point values for which the two values are considered equal. Must be 1 or larger.</param>
        /// <returns><c>true</c> if the first value is larger than the second value; otherwise <c>false</c>.</returns>
        public static bool IsLargerNumbersBetween(this double a, double b, long maxNumbersBetween)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareToNumbersBetween(a, b, maxNumbersBetween) > 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is larger than the <c>second</c>
        /// value to within the tolerance or not. Equality comparison is based on the binary representation.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum number of floating point values for which the two values are considered equal. Must be 1 or larger.</param>
        /// <returns><c>true</c> if the first value is larger than the second value; otherwise <c>false</c>.</returns>
        public static bool IsLargerNumbersBetween(this float a, float b, long maxNumbersBetween)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareToNumbersBetween(a, b, maxNumbersBetween) > 0;
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
        public static bool IsSmaller(this double a, double b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, decimalPlaces) < 0;
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
        public static bool IsSmaller(this float a, float b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, decimalPlaces) < 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The absolute accuracy required for being almost equal.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmaller(this double a, double b, double maximumAbsoluteError)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, maximumAbsoluteError) < 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The absolute accuracy required for being almost equal.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmaller(this float a, float b, double maximumAbsoluteError)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareTo(a, b, maximumAbsoluteError) < 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmallerRelative(this double a, double b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareToRelative(a, b, decimalPlaces) < 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmallerRelative(this float a, float b, int decimalPlaces)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareToRelative(a, b, decimalPlaces) < 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The relative accuracy required for being almost equal.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmallerRelative(this double a, double b, double maximumError)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareToRelative(a, b, maximumError) < 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the specified number of decimal places or not.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The relative accuracy required for being almost equal.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmallerRelative(this float a, float b, double maximumError)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareToRelative(a, b, maximumError) < 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the tolerance or not. Equality comparison is based on the binary representation.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum number of floating point values for which the two values are considered equal. Must be 1 or larger.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmallerNumbersBetween(this double a, double b, long maxNumbersBetween)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (double.IsNaN(a) || double.IsNaN(b))
            {
                return false;
            }

            return CompareToNumbersBetween(a, b, maxNumbersBetween) < 0;
        }

        /// <summary>
        /// Compares two doubles and determines if the <c>first</c> value is smaller than the <c>second</c>
        /// value to within the tolerance or not. Equality comparison is based on the binary representation.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maxNumbersBetween">The maximum number of floating point values for which the two values are considered equal. Must be 1 or larger.</param>
        /// <returns><c>true</c> if the first value is smaller than the second value; otherwise <c>false</c>.</returns>
        public static bool IsSmallerNumbersBetween(this float a, float b, long maxNumbersBetween)
        {
            // If A or B are a NAN, return false. NANs are equal to nothing,
            // not even themselves, and thus they're not bigger or
            // smaller than anything either
            if (float.IsNaN(a) || float.IsNaN(b))
            {
                return false;
            }

            return CompareToNumbersBetween(a, b, maxNumbersBetween) < 0;
        }

        /// <summary>
        /// Checks if a given double values is finite, i.e. neither NaN nor inifnity
        /// </summary>
        /// <param name="value">The value to be checked fo finitenes.</param>
        public static bool IsFinite(this double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
