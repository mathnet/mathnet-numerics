// <copyright file="SemiDeviationTests.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Financial;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.FinancialTests
{
    [TestFixture, Category("Financial")]
    public class SemiDeviationTests
    {
        [Test]
        public void returns_undefined_with_no_input_data()
        {
            //arrange
            var inputData = new List<double>();
            //act
            var semiDeviation = inputData.SemiDeviation();
            //assert
            Assert.AreEqual(double.NaN, semiDeviation);
        }

        [Test]
        public void returns_undefined_with_single_positive_input()
        {
            //arrange
            var inputData = new[] { 1.0 };
            //act
            var semiDeviation = inputData.SemiDeviation();
            //assert
            Assert.AreEqual(double.NaN, semiDeviation);
        }

        [Test]
        public void returns_undefined_with_single_negative_input()
        {
            //arrange
            var inputData = new[] { -1.0 };
            //act
            var semiDeviation = inputData.SemiDeviation();
            //assert
            Assert.AreEqual(double.NaN, semiDeviation);
        }

        [Test]
        public void only_uses_data_points_below_the_mean_of_all_data()
        {
            //arrange
            var inputData = new[] { 1.0, 2.0, 3.0, 4.0 };
            var mean = inputData.Mean();
            var expectedSemiDeviation = inputData.Where(x => x < mean).StandardDeviation();
            //act
            var semiDeviation = inputData.SemiDeviation();
            //assert
            Assert.AreEqual(expectedSemiDeviation, semiDeviation);
        }

        [Test]
        public void handles_negative_values()
        {
            //arrange
            var inputData = new[] { -1.0, 2.0, 3.0, 4.0 };
            var mean = inputData.Mean();
            var expectedSemiDeviation = inputData.Where(x => x < mean).StandardDeviation();
            //act
            var semiDeviation = inputData.SemiDeviation();
            //assert
            Assert.AreEqual(expectedSemiDeviation, semiDeviation);
        }

        [Test]
        public void throws_when_input_data_is_null()
        {
            //arrange
            List<double> inputData = null;
            //act
// ReSharper disable ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => AbsoluteRiskMeasures.SemiDeviation(inputData));
// ReSharper restore ExpressionIsAlwaysNull
        }
    }
}
