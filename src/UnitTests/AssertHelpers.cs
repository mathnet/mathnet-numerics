// <copyright file="AssertHelpers.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests
{
    using System.Collections.Generic;
    using System.Numerics;
    using NUnit.Framework;

    /// <summary>
    /// A class which includes some assertion helper methods particularly for numerical code.
    /// </summary>
    internal class AssertHelpers
    {
        /// <summary>
        /// Asserts that the expected value and the actual value are equal.
        /// </summary>
        public static void AreEqual(Complex expected, Complex actual)
        {
            if (expected.IsNaN() && actual.IsNaN() || expected.IsInfinity() && expected.IsInfinity())
            {
                return;
            }

            if (!expected.Real.AlmostEqual(actual.Real))
            {
                Assert.Fail("Real components are not equal. Expected:{0}; Actual:{1}", expected.Real, actual.Real);
            }

            if (!expected.Imaginary.AlmostEqual(actual.Imaginary))
            {
                Assert.Fail("Imaginary components are not equal. Expected:{0}; Actual:{1}", expected.Imaginary, actual.Imaginary);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal.
        /// </summary>
        public static void AreEqual(Complex32 expected, Complex32 actual)
        {
            if (expected.IsNaN() && actual.IsNaN() || expected.IsInfinity() && expected.IsInfinity())
            {
                return;
            }

            if (!expected.Real.AlmostEqual(actual.Real))
            {
                Assert.Fail("Real components are not equal. Expected:{0}; Actual:{1}", expected.Real, actual.Real);
            }

            if (!expected.Imaginary.AlmostEqual(actual.Imaginary))
            {
                Assert.Fail("Imaginary components are not equal. Expected:{0}; Actual:{1}", expected.Imaginary, actual.Imaginary);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places. If both
        /// <paramref name="expected"/> and <paramref name="actual"/> are NaN then no assert is thrown.
        /// </summary>
        public static void AlmostEqual(double expected, double actual, int decimalPlaces)
        {
            if (double.IsNaN(expected) && double.IsNaN(actual))
            {
                return;
            }

            if (!expected.AlmostEqualInDecimalPlaces(actual, decimalPlaces))
            {
                Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected, actual);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places. If both
        /// <paramref name="expected"/> and <paramref name="actual"/> are NaN then no assert is thrown.
        /// </summary>
        public static void AlmostEqual(float expected, float actual, int decimalPlaces)
        {
            if (float.IsNaN(expected) && float.IsNaN(actual))
            {
                return;
            }

            if (!expected.AlmostEqualInDecimalPlaces(actual, decimalPlaces))
            {
                Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected, actual);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places.
        /// </summary>
        public static void AlmostEqual(Complex expected, Complex actual, int decimalPlaces)
        {
            if (!expected.Real.AlmostEqualInDecimalPlaces(actual.Real, decimalPlaces))
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            if (!expected.Imaginary.AlmostEqualInDecimalPlaces(actual.Imaginary, decimalPlaces))
            {
                Assert.Fail("Imaginary components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Imaginary, actual.Imaginary);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places.
        /// </summary>
        public static void AlmostEqual(Complex32 expected, Complex32 actual, int decimalPlaces)
        {
            if (!expected.Real.AlmostEqualInDecimalPlaces(actual.Real, decimalPlaces))
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            if (!expected.Imaginary.AlmostEqualInDecimalPlaces(actual.Imaginary, decimalPlaces))
            {
                Assert.Fail("Imaginary components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Imaginary, actual.Imaginary);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places. If both
        /// <paramref name="expected"/> and <paramref name="actual"/> are NaN then no assert is thrown.
        /// </summary>
        public static void AlmostEqualAbsolute(double expected, double actual, int decimalPlaces)
        {
            if (double.IsNaN(expected) && double.IsNaN(actual))
            {
                return;
            }

            if (!expected.AlmostEqualWithAbsoluteDecimalPlaces(actual, decimalPlaces))
            {
                Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected, actual);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places. If both
        /// <paramref name="expected"/> and <paramref name="actual"/> are NaN then no assert is thrown.
        /// </summary>
        public static void AlmostEqualAbsolute(float expected, float actual, int decimalPlaces)
        {
            if (float.IsNaN(expected) && float.IsNaN(actual))
            {
                return;
            }

            if (!expected.AlmostEqualWithAbsoluteDecimalPlaces(actual, decimalPlaces))
            {
                Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected, actual);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places.
        /// </summary>
        public static void AlmostEqualAbsolute(Complex expected, Complex actual, int decimalPlaces)
        {
            if (!expected.Real.AlmostEqualWithAbsoluteDecimalPlaces(actual.Real, decimalPlaces))
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            if (!expected.Imaginary.AlmostEqualWithAbsoluteDecimalPlaces(actual.Imaginary, decimalPlaces))
            {
                Assert.Fail("Imaginary components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Imaginary, actual.Imaginary);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places.
        /// </summary>
        public static void AlmostEqualAbsolute(Complex32 expected, Complex32 actual, int decimalPlaces)
        {
            if (!expected.Real.AlmostEqualWithAbsoluteDecimalPlaces(actual.Real, decimalPlaces))
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            if (!expected.Imaginary.AlmostEqualWithAbsoluteDecimalPlaces(actual.Imaginary, decimalPlaces))
            {
                Assert.Fail("Imaginary components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Imaginary, actual.Imaginary);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain
        /// maximum error.
        /// </summary>
        /// <typeparam name="T">The type of the structures. Must implement 
        /// <see cref="IPrecisionSupport{T}"/>.</typeparam>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static void AlmostEqual<T>(T expected, T actual, double maximumError)
            where T : IPrecisionSupport<T>
        {
            if (!actual.AlmostEqualWithError(expected, maximumError))
            {
                Assert.Fail("Not equal within a maximum error {0}. Expected:{1}; Actual:{2}", maximumError, expected, actual);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain
        /// maximum error.
        /// </summary>
        public static void AlmostEqualList(IList<double> expected, IList<double> actual, double maximumError)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqualWithError(expected[i], maximumError))
                {
                    Assert.Fail("Not equal within a maximum error {0}. Expected:{1}; Actual:{2}", maximumError, expected[i], actual[i]);
                }
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain
        /// maximum error.
        /// </summary>
        public static void AlmostEqualList(IList<float> expected, IList<float> actual, double maximumError)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqualWithError(expected[i], maximumError))
                {
                    Assert.Fail("Not equal within a maximum error {0}. Expected:{1}; Actual:{2}", maximumError, expected[i], actual[i]);
                }
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain
        /// maximum error.
        /// </summary>
        /// <typeparam name="T">The type of the structures. Must implement 
        /// <see cref="IPrecisionSupport{T}"/>.</typeparam>
        public static void AlmostEqualList<T>(IList<T> expected, IList<T> actual, double maximumError)
            where T : IPrecisionSupport<T>
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqualWithError(expected[i], maximumError))
                {
                    Assert.Fail("Not equal within a maximum error {0}. Expected:{1}; Actual:{2}", maximumError, expected[i], actual[i]);
                }
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain
        /// maximum error.
        /// </summary>
        public static void AlmostEqualList(IList<Complex> expected, IList<Complex> actual, double maximumError)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqualWithError(expected[i], maximumError))
                {
                    Assert.Fail("Not equal within a maximum error {0}. Expected:{1}; Actual:{2}", maximumError, expected[i], actual[i]);
                }
            }
        }
    }
}
