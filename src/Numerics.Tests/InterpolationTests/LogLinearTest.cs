// <copyright file="CubicSplineTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

using MathNet.Numerics.Interpolation;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.InterpolationTests
{
    [TestFixture, Category("Interpolation")]
    public class LogLinearTest
    {
        readonly double[] _t = { 1.0, 2.0, 3.0, 4.0, 5.0 };
        readonly double[] _y = { 1.0, 4.0, 9.0, 16.0, 25.0 };

        /// <summary>
        /// Verifies that the 3rd derivative matches the given value at all the provided sample points.
        /// </summary>
        [TestCase(0, 0.66604930397785889)]
        [TestCase(1, 2.6641972159114355)]
        [TestCase(2, 2.1330961922715468)]
        [TestCase(3, 1.7142371101090121)]
        [TestCase(4, 1.4222075877044038d)]
        [TestCase(5, 2.2221993557881312d)]
        public void ThirdDerivative(double x, double expected)
        {
            IInterpolation it = LogLinear.Interpolate(_t, _y);
            Assert.AreEqual(expected, it.Differentiate3(x));
        }
    }
}
