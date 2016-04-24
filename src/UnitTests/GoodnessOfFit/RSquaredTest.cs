// <copyright file="RSquaredTest.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.GoodnessOfFit
{
    [TestFixture, Category("Regression")]
    public class RSquaredTest
    {
        /// <summary>
        /// Test the R-squared value of a values with itself
        /// </summary>
        [Test]
        public void WhenCalculatingRSquaredOfLinearDistributionWithItselfThenRSquaredIsOne()
        {
            var data = new List<double>();
            for (int i = 1; i <= 10; i++)
            {
                data.Add(i);
            }

            Assert.That(Numerics.GoodnessOfFit.RSquared(data, data), Is.EqualTo(1));
        }

        [Test]
        public void WhenGivenTwoDatasetsOfDifferentSizeThenThrowsArgumentException()
        {
            var observedData = new List<double> { 23, 9, 5, 7, 10, 5, 4, 1, 2, 1 };
            var modelledData = new List<double> { 8, 9, 10 };

            Assert.Throws<ArgumentOutOfRangeException>(() => Numerics.GoodnessOfFit.RSquared(modelledData, observedData));
        }

        [Test]
        public void WhenCalculatingRSquaredOfUnevenDistributionWithLInearDistributionThenRSquaredIsCalculated()
        {
            var observedData = new List<double> { 1, 2.3, 3.1, 4.8, 5.6, 6.3 };
            var modelledData = new List<double> { 2.6, 2.8, 3.1, 4.7, 5.1, 5.3 };

            Assert.That(Math.Round(Numerics.GoodnessOfFit.RSquared(modelledData, observedData), 11), Is.EqualTo(Math.Round(0.94878520708673d, 11)));
        }
    }
}
