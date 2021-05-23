// <copyright file="Precision.Equality.cs" company="Math.NET">
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
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics
{
    public static partial class Precision
    {
        /// <summary>
        /// Compares two doubles and determines if they are equal
        /// within the specified maximum absolute error.
        /// </summary>
        /// <param name="a">The norm of the first value (can be negative).</param>
        /// <param name="b">The norm of the second value (can be negative).</param>
        /// <param name="diff">The norm of the difference of the two values (can be negative).</param>
        /// <param name="maximumAbsoluteError">The absolute accuracy required for being almost equal.</param>
        /// <returns>True if both doubles are almost equal up to the specified maximum absolute error, false otherwise.</returns>
        public static bool AlmostEqualNorm(this double a, double b, double diff, double maximumAbsoluteError)
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
        /// Compares two doubles and determines if they are equal
        /// within the specified maximum absolute error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The absolute accuracy required for being almost equal.</param>
        /// <returns>True if both doubles are almost equal up to the specified maximum absolute error, false otherwise.</returns>
        public static bool AlmostEqualNorm<T>(this T a, T b, double maximumAbsoluteError)
            where T : IPrecisionSupport<T>
        {
            return AlmostEqualNorm(a.Norm(), b.Norm(), a.NormOfDifference(b), maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal
        /// within the specified maximum error.
        /// </summary>
        /// <param name="a">The norm of the first value (can be negative).</param>
        /// <param name="b">The norm of the second value (can be negative).</param>
        /// <param name="diff">The norm of the difference of the two values (can be negative).</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        /// <returns>True if both doubles are almost equal up to the specified maximum error, false otherwise.</returns>
        public static bool AlmostEqualNormRelative(this double a, double b, double diff, double maximumError)
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

            // If one is almost zero, fall back to absolute equality
            if (Math.Abs(a) < DoublePrecision || Math.Abs(b) < DoublePrecision)
            {
                return Math.Abs(diff) < maximumError;
            }

            if ((a == 0 && Math.Abs(b) < maximumError) || (b == 0 && Math.Abs(a) < maximumError))
            {
                return true;
            }

            return Math.Abs(diff) < maximumError * Math.Max(Math.Abs(a), Math.Abs(b));
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal
        /// within the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        /// <returns>True if both doubles are almost equal up to the specified maximum error, false otherwise.</returns>
        public static bool AlmostEqualNormRelative<T>(this T a, T b, double maximumError)
            where T : IPrecisionSupport<T>
        {
            return AlmostEqualNormRelative(a.Norm(), b.Norm(), a.NormOfDifference(b), maximumError);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqual(this double a, double b, double maximumAbsoluteError)
        {
            return AlmostEqualNorm(a, b, a - b, maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two complex and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqual(this float a, float b, double maximumAbsoluteError)
        {
            return AlmostEqualNorm(a, b, a - b, maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two complex and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqual(this Complex a, Complex b, double maximumAbsoluteError)
        {
            return AlmostEqualNorm(a.Norm(), b.Norm(), a.NormOfDifference(b), maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two complex and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqual(this Complex32 a, Complex32 b, double maximumAbsoluteError)
        {
            return AlmostEqualNorm(a.Norm(), b.Norm(), a.NormOfDifference(b), maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqualRelative(this double a, double b, double maximumError)
        {
            return AlmostEqualNormRelative(a, b, a - b, maximumError);
        }

        /// <summary>
        /// Compares two complex and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqualRelative(this float a, float b, double maximumError)
        {
            return AlmostEqualNormRelative(a, b, a - b, maximumError);
        }

        /// <summary>
        /// Compares two complex and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqualRelative(this Complex a, Complex b, double maximumError)
        {
            return AlmostEqualNormRelative(a.Norm(), b.Norm(), a.NormOfDifference(b), maximumError);
        }

        /// <summary>
        /// Compares two complex and determines if they are equal within
        /// the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqualRelative(this Complex32 a, Complex32 b, double maximumError)
        {
            return AlmostEqualNormRelative(a.Norm(), b.Norm(), a.NormOfDifference(b), maximumError);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual(this double a, double b)
        {
            return AlmostEqualNorm(a, b, a - b, DefaultDoubleAccuracy);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual(this float a, float b)
        {
            return AlmostEqualNorm(a, b, a - b, DefaultSingleAccuracy);
        }

        /// <summary>
        /// Checks whether two Complex numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual(this Complex a, Complex b)
        {
            return AlmostEqualNorm(a.Norm(), b.Norm(), a.NormOfDifference(b), DefaultDoubleAccuracy);
        }

        /// <summary>
        /// Checks whether two Complex numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqual(this Complex32 a, Complex32 b)
        {
            return AlmostEqualNorm(a.Norm(), b.Norm(), a.NormOfDifference(b), DefaultSingleAccuracy);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqualRelative(this double a, double b)
        {
            return AlmostEqualNormRelative(a, b, a - b, DefaultDoubleAccuracy);
        }

        /// <summary>
        /// Checks whether two real numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqualRelative(this float a, float b)
        {
            return AlmostEqualNormRelative(a, b, a - b, DefaultSingleAccuracy);
        }

        /// <summary>
        /// Checks whether two Complex numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqualRelative(this Complex a, Complex b)
        {
            return AlmostEqualNormRelative(a.Norm(), b.Norm(), a.NormOfDifference(b), DefaultDoubleAccuracy);
        }

        /// <summary>
        /// Checks whether two Complex numbers are almost equal.
        /// </summary>
        /// <param name="a">The first number</param>
        /// <param name="b">The second number</param>
        /// <returns>true if the two values differ by no more than 10 * 2^(-52); false otherwise.</returns>
        public static bool AlmostEqualRelative(this Complex32 a, Complex32 b)
        {
            return AlmostEqualNormRelative(a.Norm(), b.Norm(), a.NormOfDifference(b), DefaultSingleAccuracy);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not, using the
        /// number of decimal places as an absolute measure.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 0.5e-decimalPlaces. We divide by
        /// two so that we have half the range on each side of the numbers, e.g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The norm of the first value (can be negative).</param>
        /// <param name="b">The norm of the second value (can be negative).</param>
        /// <param name="diff">The norm of the difference of the two values (can be negative).</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqualNorm(this double a, double b, double diff, int decimalPlaces)
        {
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

            // The values are equal if the difference between the two numbers is smaller than
            // 10^(-numberOfDecimalPlaces). We divide by two so that we have half the range
            // on each side of the numbers, e.g. if decimalPlaces == 2,
            // then 0.01 will equal between 0.005 and 0.015, but not 0.02 and not 0.00
            return Math.Abs(diff) < Pow10(-decimalPlaces) * 0.5;
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not, using the
        /// number of decimal places as an absolute measure.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values are equal if the difference between the two numbers is smaller than 0.5e-decimalPlaces. We divide by
        /// two so that we have half the range on each side of the numbers, e.g. if <paramref name="decimalPlaces"/> == 2, then 0.01 will equal between
        /// 0.005 and 0.015, but not 0.02 and not 0.00
        /// </para>
        /// </remarks>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqualNorm<T>(this T a, T b, int decimalPlaces)
            where T : IPrecisionSupport<T>
        {
            return AlmostEqualNorm(a.Norm(), b.Norm(), a.NormOfDifference(b), decimalPlaces);
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
        /// <param name="a">The norm of the first value (can be negative).</param>
        /// <param name="b">The norm of the second value (can be negative).</param>
        /// <param name="diff">The norm of the difference of the two values (can be negative).</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="decimalPlaces"/> is smaller than zero.</exception>
        public static bool AlmostEqualNormRelative(this double a, double b, double diff, int decimalPlaces)
        {
            if (decimalPlaces < 0)
            {
                // Can't have a negative number of decimal places
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
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

            // If both numbers are equal, get out now. This should remove the possibility of both numbers being zero
            // and any problems associated with that.
            if (a.Equals(b))
            {
                return true;
            }

            // If one is almost zero, fall back to absolute equality
            if (Math.Abs(a) < DoublePrecision || Math.Abs(b) < DoublePrecision)
            {
                // The values are equal if the difference between the two numbers is smaller than
                // 10^(-numberOfDecimalPlaces). We divide by two so that we have half the range
                // on each side of the numbers, e.g. if decimalPlaces == 2,
                // then 0.01 will equal between 0.005 and 0.015, but not 0.02 and not 0.00
                return Math.Abs(diff) < Pow10(-decimalPlaces) * 0.5;
            }

            // If the magnitudes of the two numbers are equal to within one magnitude the numbers could potentially be equal
            int magnitudeOfFirst = Magnitude(a);
            int magnitudeOfSecond = Magnitude(b);
            int magnitudeOfMax = Math.Max(magnitudeOfFirst, magnitudeOfSecond);
            if (magnitudeOfMax > (Math.Min(magnitudeOfFirst, magnitudeOfSecond) + 1))
            {
                return false;
            }

            // The values are equal if the difference between the two numbers is smaller than
            // 10^(-numberOfDecimalPlaces). We divide by two so that we have half the range
            // on each side of the numbers, e.g. if decimalPlaces == 2,
            // then 0.01 will equal between 0.00995 and 0.01005, but not 0.0015 and not 0.0095
            return Math.Abs(diff) < Pow10(magnitudeOfMax - decimalPlaces) * 0.5;
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
        public static bool AlmostEqualNormRelative<T>(this T a, T b, int decimalPlaces)
            where T : IPrecisionSupport<T>
        {
            return AlmostEqualNormRelative(a.Norm(), b.Norm(), a.NormOfDifference(b), decimalPlaces);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not, using the
        /// number of decimal places as an absolute measure.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqual(this double a, double b, int decimalPlaces)
        {
            return AlmostEqualNorm(a, b, a - b, decimalPlaces);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not, using the
        /// number of decimal places as an absolute measure.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqual(this float a, float b, int decimalPlaces)
        {
            return AlmostEqualNorm(a, b, a - b, decimalPlaces);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not, using the
        /// number of decimal places as an absolute measure.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqual(this Complex a, Complex b, int decimalPlaces)
        {
            return AlmostEqualNorm(a.Norm(), b.Norm(), a.NormOfDifference(b), decimalPlaces);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not, using the
        /// number of decimal places as an absolute measure.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqual(this Complex32 a, Complex32 b, int decimalPlaces)
        {
            return AlmostEqualNorm(a.Norm(), b.Norm(), a.NormOfDifference(b), decimalPlaces);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not. If the numbers
        /// are very close to zero an absolute difference is compared, otherwise the relative difference is compared.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqualRelative(this double a, double b, int decimalPlaces)
        {
            return AlmostEqualNormRelative(a, b, a - b, decimalPlaces);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not. If the numbers
        /// are very close to zero an absolute difference is compared, otherwise the relative difference is compared.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqualRelative(this float a, float b, int decimalPlaces)
        {
            return AlmostEqualNormRelative(a, b, a - b, decimalPlaces);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not. If the numbers
        /// are very close to zero an absolute difference is compared, otherwise the relative difference is compared.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqualRelative(this Complex a, Complex b, int decimalPlaces)
        {
            return AlmostEqualNormRelative(a.Norm(), b.Norm(), a.NormOfDifference(b), decimalPlaces);
        }

        /// <summary>
        /// Compares two doubles and determines if they are equal to within the specified number of decimal places or not. If the numbers
        /// are very close to zero an absolute difference is compared, otherwise the relative difference is compared.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqualRelative(this Complex32 a, Complex32 b, int decimalPlaces)
        {
            return AlmostEqualNormRelative(a.Norm(), b.Norm(), a.NormOfDifference(b), decimalPlaces);
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
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxNumbersBetween"/> is smaller than one.</exception>
        public static bool AlmostEqualNumbersBetween(this double a, double b, long maxNumbersBetween)
        {
            // Make sure maxNumbersBetween is non-negative and small enough that the
            // default NAN won't compare as equal to anything.
            if (maxNumbersBetween < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxNumbersBetween));
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
            long firstUlong = AsDirectionalInt64(a);

            // Get the second double and convert it to an integer value (by using the binary representation)
            long secondUlong = AsDirectionalInt64(b);

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
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxNumbersBetween"/> is smaller than one.</exception>
        public static bool AlmostEqualNumbersBetween(this float a, float b, int maxNumbersBetween)
        {
            // Make sure maxNumbersBetween is non-negative and small enough that the
            // default NAN won't compare as equal to anything.
            if (maxNumbersBetween < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxNumbersBetween));
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
            int firstUlong = AsDirectionalInt32(a);

            // Get the second float and convert it to an integer value (by using the binary representation)
            int secondUlong = AsDirectionalInt32(b);

            // Now compare the values.
            // Note that this comparison can overflow so we'll approach this differently
            // Do note that we could overflow this way too. We should probably check that we don't.
            return (a > b) ? (secondUlong + maxNumbersBetween >= firstUlong) : (firstUlong + maxNumbersBetween >= secondUlong);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool ListAlmostEqual(this IList<double> a, IList<double> b, double maximumAbsoluteError)
        {
            return ListForAll(a, b, AlmostEqual, maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool ListAlmostEqual(this IList<float> a, IList<float> b, double maximumAbsoluteError)
        {
            return ListForAll(a, b, AlmostEqual, maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool ListAlmostEqual(this IList<Complex> a, IList<Complex> b, double maximumAbsoluteError)
        {
            return ListForAll(a, b, AlmostEqual, maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool ListAlmostEqual(this IList<Complex32> a, IList<Complex32> b, double maximumAbsoluteError)
        {
            return ListForAll(a, b, AlmostEqual, maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool ListAlmostEqualRelative(this IList<double> a, IList<double> b, double maximumError)
        {
            return ListForAll(a, b, AlmostEqualRelative, maximumError);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool ListAlmostEqualRelative(this IList<float> a, IList<float> b, double maximumError)
        {
            return ListForAll(a, b, AlmostEqualRelative, maximumError);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool ListAlmostEqualRelative(this IList<Complex> a, IList<Complex> b, double maximumError)
        {
            return ListForAll(a, b, AlmostEqualRelative, maximumError);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool ListAlmostEqualRelative(this IList<Complex32> a, IList<Complex32> b, double maximumError)
        {
            return ListForAll(a, b, AlmostEqualRelative, maximumError);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool ListAlmostEqual(this IList<double> a, IList<double> b, int decimalPlaces)
        {
            return ListForAll(a, b, AlmostEqual, decimalPlaces);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool ListAlmostEqual(this IList<float> a, IList<float> b, int decimalPlaces)
        {
            return ListForAll(a, b, AlmostEqual, decimalPlaces);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool ListAlmostEqual(this IList<Complex> a, IList<Complex> b, int decimalPlaces)
        {
            return ListForAll(a, b, AlmostEqual, decimalPlaces);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool ListAlmostEqual(this IList<Complex32> a, IList<Complex32> b, int decimalPlaces)
        {
            return ListForAll(a, b, AlmostEqual, decimalPlaces);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool ListAlmostEqualRelative(this IList<double> a, IList<double> b, int decimalPlaces)
        {
            return ListForAll(a, b, AlmostEqualRelative, decimalPlaces);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool ListAlmostEqualRelative(this IList<float> a, IList<float> b, int decimalPlaces)
        {
            return ListForAll(a, b, AlmostEqualRelative, decimalPlaces);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool ListAlmostEqualRelative(this IList<Complex> a, IList<Complex> b, int decimalPlaces)
        {
            return ListForAll(a, b, AlmostEqualRelative, decimalPlaces);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool ListAlmostEqualRelative(this IList<Complex32> a, IList<Complex32> b, int decimalPlaces)
        {
            return ListForAll(a, b, AlmostEqualRelative, decimalPlaces);
        }

        /// <summary>
        /// Compares two lists of doubles and determines if they are equal within the
        /// specified maximum error.
        /// </summary>
        /// <param name="a">The first value list.</param>
        /// <param name="b">The second value list.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool ListAlmostEqualNorm<T>(this IList<T> a, IList<T> b, double maximumAbsoluteError)
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
                if (!AlmostEqualNorm(a[i], b[i], maximumAbsoluteError))
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
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool ListAlmostEqualNormRelative<T>(this IList<T> a, IList<T> b, double maximumError)
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
                if (!AlmostEqualNormRelative(a[i], b[i], maximumError))
                {
                    return false;
                }
            }

            return true;
        }

        static bool ListForAll<T,TP>(IList<T> a, IList<T> b, Func<T, T, TP, bool> predicate, TP parameter)
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
                if (!predicate(a[i], b[i], parameter))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compares two vectors and determines if they are equal within the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqual<T>(this Vector<T> a, Vector<T> b, double maximumAbsoluteError)
            where T : struct, IEquatable<T>, IFormattable
        {
            return AlmostEqualNorm(a.L2Norm(), b.L2Norm(), (a - b).L2Norm(), maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two vectors and determines if they are equal within the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqualRelative<T>(this Vector<T> a, Vector<T> b, double maximumError)
            where T : struct, IEquatable<T>, IFormattable
        {
            return AlmostEqualNormRelative(a.L2Norm(), b.L2Norm(), (a - b).L2Norm(), maximumError);
        }

        /// <summary>
        /// Compares two vectors and determines if they are equal to within the specified number
        /// of decimal places or not, using the number of decimal places as an absolute measure.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqual<T>(this Vector<T> a, Vector<T> b, int decimalPlaces)
            where T : struct, IEquatable<T>, IFormattable
        {
            return AlmostEqualNorm(a.L2Norm(), b.L2Norm(), (a - b).L2Norm(), decimalPlaces);
        }

        /// <summary>
        /// Compares two vectors and determines if they are equal to within the specified number of decimal places or not.
        /// If the numbers are very close to zero an absolute difference is compared, otherwise the relative difference is compared.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqualRelative<T>(this Vector<T> a, Vector<T> b, int decimalPlaces)
            where T : struct, IEquatable<T>, IFormattable
        {
            return AlmostEqualNormRelative(a.L2Norm(), b.L2Norm(), (a - b).L2Norm(), decimalPlaces);
        }

        /// <summary>
        /// Compares two matrices and determines if they are equal within the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumAbsoluteError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqual<T>(this Matrix<T> a, Matrix<T> b, double maximumAbsoluteError)
            where T : struct, IEquatable<T>, IFormattable
        {
            return AlmostEqualNorm(a.L2Norm(), b.L2Norm(), (a - b).L2Norm(), maximumAbsoluteError);
        }

        /// <summary>
        /// Compares two matrices and determines if they are equal within the specified maximum error.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static bool AlmostEqualRelative<T>(this Matrix<T> a, Matrix<T> b, double maximumError)
            where T : struct, IEquatable<T>, IFormattable
        {
            return AlmostEqualNormRelative(a.L2Norm(), b.L2Norm(), (a - b).L2Norm(), maximumError);
        }

        /// <summary>
        /// Compares two matrices and determines if they are equal to within the specified number
        /// of decimal places or not, using the number of decimal places as an absolute measure.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqual<T>(this Matrix<T> a, Matrix<T> b, int decimalPlaces)
            where T : struct, IEquatable<T>, IFormattable
        {
            return AlmostEqualNorm(a.L2Norm(), b.L2Norm(), (a - b).L2Norm(), decimalPlaces);
        }

        /// <summary>
        /// Compares two matrices and determines if they are equal to within the specified number of decimal places or not.
        /// If the numbers are very close to zero an absolute difference is compared, otherwise the relative difference is compared.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="decimalPlaces">The number of decimal places.</param>
        public static bool AlmostEqualRelative<T>(this Matrix<T> a, Matrix<T> b, int decimalPlaces)
            where T : struct, IEquatable<T>, IFormattable
        {
            return AlmostEqualNormRelative(a.L2Norm(), b.L2Norm(), (a - b).L2Norm(), decimalPlaces);
        }

        static readonly double[] NegativePowersOf10 = new double[]
        {
            1, 0.1, 0.01, 1e-3, 1e-4, 1e-5, 1e-6, 1e-7, 1e-8, 1e-9,
            1e-10, 1e-11, 1e-12, 1e-13, 1e-14, 1e-15, 1e-16,
            1e-17, 1e-18, 1e-19, 1e-20
        };

        static double Pow10(int y)
        {
            return -NegativePowersOf10.Length < y && y <= 0
               ? NegativePowersOf10[-y]
               : Math.Pow(10.0, y);
        }
    }
}
