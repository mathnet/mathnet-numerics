// <copyright file="MovingStatisticsTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.StatisticsTests
{
#if !PORTABLE
    [TestFixture, Category("Statistics")]
    public class MovingStatisticsTests
    {
        [Test]
        public void QuickTest()
        {
            var data = new double[1000000];
            (new Normal(50, 10, new SystemRandomSource(0))).Samples(data);
            var ms = new MovingStatistics(5, data);
            ms.PushRange(new[] { 11.11, 22.22, 33.33, 44.44, 55.55 });
            Assert.AreEqual(5, ms.Count);
            Assert.AreEqual(11.11, ms.Minimum);
            Assert.AreEqual(55.55, ms.Maximum);

            Assert.AreEqual(33.33, ms.Mean, 1e-11);
            Assert.AreEqual(308.58025, ms.Variance, 1e-10);

            //AssertHelpers.AlmostEqualRelative(stats0.Mean, ms.Mean, 14);
            //AssertHelpers.AlmostEqualRelative(stats0.Variance, ms.Variance, 14);
            //AssertHelpers.AlmostEqualRelative(stats0.StandardDeviation, ms.StandardDeviation, 14);
        }
    }
#endif
}
