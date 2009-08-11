// <copyright file="AssertHelpers.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests
{
    using MbUnit.Framework;

    /// <summary>
    /// A class which includes some assertion helper methods particularly for numerical code.
    /// </summary>
    class AssertHelpers
    {
        /// <summary>
        /// Asserts that the expected value and the actual value are equal up to a certain number of decimal places.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="decimalPlaces">The number of decimal places to agree on.</param>
        public static void AlmostEqual(double expected, double actual, int decimalPlaces)
        {
            bool pass = Precision.AlmostEqualInDecimalPlaces(expected, actual, decimalPlaces);
            if (!pass)
            {
                //signals Gallio that the test failed.
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
            bool pass = expected.Real.AlmostEqualInDecimalPlaces(actual.Real, decimalPlaces);
            if (!pass)
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            pass = expected.Imaginary.AlmostEqualInDecimalPlaces(actual.Imaginary, decimalPlaces);
            if (!pass)
            {
                Assert.Fail("Imaginary components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Imaginary, actual.Imaginary);
            }
        }
    }
}
