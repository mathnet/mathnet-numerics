// <copyright file="OnlineMedianFilterTest.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.Filtering
{
    using Numerics.Filtering.Median;
    using NUnit.Framework;

    /// <summary>
    /// OnlineMedian test.
    /// </summary>
    [TestFixture]
    public class OnlineMedianFilterTest
    {
        /// <summary>
        /// Naive transforms real sine correctly.
        /// </summary>
        [Test]
        public void MedianWindowFilling()
        {
            var filter = new OnlineMedianFilter(5);

            Assert.AreEqual(5, filter.ProcessSample(5));
            Assert.AreEqual(3.5, filter.ProcessSample(2));
            Assert.AreEqual(5, filter.ProcessSample(6));
            Assert.AreEqual(4, filter.ProcessSample(3));
            Assert.AreEqual(4, filter.ProcessSample(4));
            Assert.AreEqual(3, filter.ProcessSample(1));
            Assert.AreEqual(4, filter.ProcessSample(7));
        }
    }
}
