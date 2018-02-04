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

using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;

namespace MathNet.Numerics.Data.UnitTests
{
    /// <summary>
    /// A class which includes some assertion helper methods particularly for numerical code.
    /// </summary>
    internal class AssertHelpers
    {
        /// <summary>
        /// Asserts that the expected value and the actual value are equal.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        public static void AreEqual(Complex expected, Complex actual)
        {
            if (expected.IsNaN() && actual.IsNaN())
            {
                return;
            }

            if (expected.IsInfinity() && expected.IsInfinity())
            {
                return;
            }

            var pass = expected.Real.AlmostEqual(actual.Real);
            if (!pass)
            {
                Assert.Fail("Real components are not equal. Expected:{0}; Actual:{1}", expected.Real, actual.Real);
            }

            pass = expected.Imaginary.AlmostEqual(actual.Imaginary);
            if (!pass)
            {
                Assert.Fail("Imaginary components are not equal. Expected:{0}; Actual:{1}", expected.Imaginary, actual.Imaginary);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        public static void AreEqual(Complex32 expected, Complex32 actual)
        {
            if (expected.IsNaN() && actual.IsNaN())
            {
                return;
            }

            if (expected.IsInfinity() && expected.IsInfinity())
            {
                return;
            }

            var pass = expected.Real.AlmostEqual(actual.Real);
            if (!pass)
            {
                Assert.Fail("Real components are not equal. Expected:{0}; Actual:{1}", expected.Real, actual.Real);
            }

            pass = expected.Imaginary.AlmostEqual(actual.Imaginary);
            if (!pass)
            {
                Assert.Fail("Imaginary components are not equal. Expected:{0}; Actual:{1}", expected.Imaginary, actual.Imaginary);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places. If both
        /// <paramref name="expected"/> and <paramref name="actual"/> are NaN then no assert is thrown.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="decimalPlaces">The number of decimal places to agree on.</param>
        public static void AlmostEqual(double expected, double actual, int decimalPlaces)
        {
            if (double.IsNaN(expected) && double.IsNaN(actual))
            {
                return;
            }

            var pass = expected.AlmostEqualRelative(actual, decimalPlaces);
            if (!pass)
            {
                // signals Gallio that the test failed.
                Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected, actual);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places. If both
        /// <paramref name="expected"/> and <paramref name="actual"/> are NaN then no assert is thrown.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="decimalPlaces">The number of decimal places to agree on.</param>
        public static void AlmostEqual(float expected, float actual, int decimalPlaces)
        {
            if (float.IsNaN(expected) && float.IsNaN(actual))
            {
                return;
            }

            var pass = expected.AlmostEqualRelative(actual, decimalPlaces);
            if (!pass)
            {
                // signals Gallio that the test failed.
                Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected, actual);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="decimalPlaces">The number of decimal places to agree on.</param>
        public static void AlmostEqual(Complex expected, Complex actual, int decimalPlaces)
        {
            var pass = expected.Real.AlmostEqualRelative(actual.Real, decimalPlaces);
            if (!pass)
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            pass = expected.Imaginary.AlmostEqualRelative(actual.Imaginary, decimalPlaces);
            if (!pass)
            {
                Assert.Fail("Imaginary components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Imaginary, actual.Imaginary);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="decimalPlaces">The number of decimal places to agree on.</param>
        public static void AlmostEqual(Complex32 expected, Complex32 actual, int decimalPlaces)
        {
            var pass = expected.Real.AlmostEqualRelative(actual.Real, decimalPlaces);
            if (!pass)
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            pass = expected.Imaginary.AlmostEqualRelative(actual.Imaginary, decimalPlaces);
            if (!pass)
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
            if (!actual.AlmostEqualNorm(expected, maximumError))
            {
                Assert.Fail("Not equal within a maximum error {0}. Expected:{1}; Actual:{2}", maximumError, expected, actual);
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain
        /// maximum error.
        /// </summary>
        /// <param name="expected">The expected value list.</param>
        /// <param name="actual">The actual value list.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static void AlmostEqualList(IList<double> expected, IList<double> actual, double maximumError)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqual(expected[i], maximumError))
                {
                    Assert.Fail("Not equal within a maximum error {0}. Expected:{1}; Actual:{2}", maximumError, expected[i], actual[i]);
                }
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain
        /// maximum error.
        /// </summary>
        /// <param name="expected">The expected value list.</param>
        /// <param name="actual">The actual value list.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static void AlmostEqualList(IList<float> expected, IList<float> actual, double maximumError)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqual(expected[i], maximumError))
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
        /// <param name="expected">The expected value list.</param>
        /// <param name="actual">The actual value list.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static void AlmostEqualList<T>(IList<T> expected, IList<T> actual, double maximumError)
            where T : IPrecisionSupport<T>
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqualNorm(expected[i], maximumError))
                {
                    Assert.Fail("Not equal within a maximum error {0}. Expected:{1}; Actual:{2}", maximumError, expected[i], actual[i]);
                }
            }
        }

        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain
        /// maximum error.
        /// </summary>
        /// <param name="expected">The expected value list.</param>
        /// <param name="actual">The actual value list.</param>
        /// <param name="maximumError">The accuracy required for being almost equal.</param>
        public static void AlmostEqualList(IList<Complex> expected, IList<Complex> actual, double maximumError)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqual(expected[i], maximumError))
                {
                    Assert.Fail("Not equal within a maximum error {0}. Expected:{1}; Actual:{2}", maximumError, expected[i], actual[i]);
                }
            }
        }
    }
}
