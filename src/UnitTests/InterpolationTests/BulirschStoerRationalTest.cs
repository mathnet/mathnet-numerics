// <copyright file="BulirschStoerRationalTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2002-2011 Math.NET
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

namespace MathNet.Numerics.UnitTests.InterpolationTests
{
    using Interpolation;
    using Interpolation.Algorithms;
    using NUnit.Framework;

    [TestFixture]
    public class BulirschStoerRationalTest
    {
        readonly double[] _t = new[] { 0d, 1, 3, 4, 5 };
        readonly double[] _x = new[] { 0d, 3, 1000, -1000, 3 };

        /// <summary>
        /// Verifies that the interpolation matches the given value at all the provided sample points.
        /// </summary>
        [Test]
        public void FitsAtSamplePoints()
        {
            IInterpolation interpolation = new BulirschStoerRationalInterpolation(_t, _x);

            for (int i = 0; i < _x.Length; i++)
            {
                Assert.AreEqual(_x[i], interpolation.Interpolate(_t[i]), "A Exact Point " + i);
            }
        }

        /// <summary>
        /// Verifies that at points other than the provided sample points, the interpolation matches the one computed by Maple as a reference.
        /// </summary>
        /// <remarks>
        /// Maple:
        /// with(CurveFitting);
        /// evalf(subs({x=0.1},RationalInterpolation([[0,0],[1,3],[3,1000],[4,-1000], [5,3]], x)),20);
        /// </remarks>
        [Test, Sequential]
        public void FitsAtArbitraryPointsWithMaple(
            [Values(0.1, 0.4, 1.1, 3.01, 3.02, 3.03, 3.1, 3.2, 4.5, 10.0, -10.0)] double t,
            [Values(.19389203383553566255, .88132900698869875369, 3.5057665681580626913, 1548.7666642693586902, 3362.2564334253633516, -22332.603641443806014, -440.30323769822443789, -202.42421196280566349, 21.208249625210155439, -4.8936986959784751517, -3.6017584308603731307)] double x,
            [Values(1e-14, 1e-14, 1e-15, 1e-10, 1e-10, 1e-8, 1e-11, 1e-12, 1e-12, 1e-13, 1e-13)] double maxAbsoluteError)
        {
            IInterpolation interpolation = new BulirschStoerRationalInterpolation(_t, _x);

            Assert.AreEqual(x, interpolation.Interpolate(t), maxAbsoluteError, "Interpolation at {0}", t);
        }

        // NOTE: No test for the linear case because this algorithms is incredibly bad at this.
    }
}
