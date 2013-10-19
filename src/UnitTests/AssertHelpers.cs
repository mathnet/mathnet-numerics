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
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests
{
#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    /// <summary>
    /// A class which includes some assertion helper methods particularly for numerical code.
    /// </summary>
    internal class AssertHelpers
    {
        public static void AlmostEqual(Complex expected, Complex actual)
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

        public static void AlmostEqual(Complex32 expected, Complex32 actual)
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


        public static void AlmostEqual(double expected, double actual, int decimalPlaces)
        {
            if (double.IsNaN(expected) && double.IsNaN(actual))
            {
                return;
            }

            if (!expected.AlmostEqual(actual, decimalPlaces))
            {
                Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected, actual);
            }
        }

        public static void AlmostEqual(float expected, float actual, int decimalPlaces)
        {
            if (float.IsNaN(expected) && float.IsNaN(actual))
            {
                return;
            }

            if (!expected.AlmostEqual(actual, decimalPlaces))
            {
                Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected, actual);
            }
        }

        public static void AlmostEqual(Complex expected, Complex actual, int decimalPlaces)
        {
            if (!expected.Real.AlmostEqual(actual.Real, decimalPlaces))
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            if (!expected.Imaginary.AlmostEqual(actual.Imaginary, decimalPlaces))
            {
                Assert.Fail("Imaginary components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Imaginary, actual.Imaginary);
            }
        }

        public static void AlmostEqual(Complex32 expected, Complex32 actual, int decimalPlaces)
        {
            if (!expected.Real.AlmostEqual(actual.Real, decimalPlaces))
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            if (!expected.Imaginary.AlmostEqual(actual.Imaginary, decimalPlaces))
            {
                Assert.Fail("Imaginary components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Imaginary, actual.Imaginary);
            }
        }

        public static void AlmostEqualRelative(double expected, double actual, int decimalPlaces)
        {
            if (double.IsNaN(expected) && double.IsNaN(actual))
            {
                return;
            }

            if (!expected.AlmostEqualRelative(actual, decimalPlaces))
            {
                Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected, actual);
            }
        }

        public static void AlmostEqualRelative(float expected, float actual, int decimalPlaces)
        {
            if (float.IsNaN(expected) && float.IsNaN(actual))
            {
                return;
            }

            if (!expected.AlmostEqualRelative(actual, decimalPlaces))
            {
                Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected, actual);
            }
        }

        public static void AlmostEqualRelative(Complex expected, Complex actual, int decimalPlaces)
        {
            if (!expected.Real.AlmostEqualRelative(actual.Real, decimalPlaces))
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            if (!expected.Imaginary.AlmostEqualRelative(actual.Imaginary, decimalPlaces))
            {
                Assert.Fail("Imaginary components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Imaginary, actual.Imaginary);
            }
        }

        public static void AlmostEqualRelative(Complex32 expected, Complex32 actual, int decimalPlaces)
        {
            if (!expected.Real.AlmostEqualRelative(actual.Real, decimalPlaces))
            {
                Assert.Fail("Real components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Real, actual.Real);
            }

            if (!expected.Imaginary.AlmostEqualRelative(actual.Imaginary, decimalPlaces))
            {
                Assert.Fail("Imaginary components are not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected.Imaginary, actual.Imaginary);
            }
        }


        public static void ListAlmostEqual(IList<double> expected, IList<double> actual, int decimalPlaces)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqual(expected[i], decimalPlaces))
                {
                    Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected[i], actual[i]);
                }
            }
        }

        public static void ListAlmostEqual(IList<float> expected, IList<float> actual, int decimalPlaces)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqual(expected[i], decimalPlaces))
                {
                    Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected[i], actual[i]);
                }
            }
        }

        public static void ListAlmostEqual(IList<Complex> expected, IList<Complex> actual, int decimalPlaces)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqual(expected[i], decimalPlaces))
                {
                    Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected[i], actual[i]);
                }
            }
        }

        public static void ListAlmostEqual(IList<Complex32> expected, IList<Complex32> actual, int decimalPlaces)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqual(expected[i], decimalPlaces))
                {
                    Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected[i], actual[i]);
                }
            }
        }

        public static void ListAlmostEqualRelative(IList<double> expected, IList<double> actual, int decimalPlaces)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqualRelative(expected[i], decimalPlaces))
                {
                    Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected[i], actual[i]);
                }
            }
        }

        public static void ListAlmostEqualRelative(IList<float> expected, IList<float> actual, int decimalPlaces)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqualRelative(expected[i], decimalPlaces))
                {
                    Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected[i], actual[i]);
                }
            }
        }

        public static void ListAlmostEqualRelative(IList<Complex> expected, IList<Complex> actual, int decimalPlaces)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqualRelative(expected[i], decimalPlaces))
                {
                    Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected[i], actual[i]);
                }
            }
        }

        public static void ListAlmostEqualRelative(IList<Complex32> expected, IList<Complex32> actual, int decimalPlaces)
        {
            for (var i = 0; i < expected.Count; i++)
            {
                if (!actual[i].AlmostEqualRelative(expected[i], decimalPlaces))
                {
                    Assert.Fail("Not equal within {0} places. Expected:{1}; Actual:{2}", decimalPlaces, expected[i], actual[i]);
                }
            }
        }
    }
}
