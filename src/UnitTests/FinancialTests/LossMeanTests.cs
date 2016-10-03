// <copyright file="LossMeanTests.cs" company="Math.NET">
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
using MathNet.Numerics.Financial;
using MathNet.Numerics.Statistics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.FinancialTests
{
    [TestFixture, Category("Financial")]
    public class LossMeanTests
    {
        [Test]
        public void returns_NaN_when_zero_is_the_only_input()
        {
            //arrange
            var inputData = new[] { 0.0 };
            //act
            var lossMean = inputData.LossMean();
            //assert
            Assert.AreEqual(double.NaN, lossMean);
        }

        [Test]
        public void returns_NaN_when_all_input_is_positive()
        {
            //arrange
            var inputData = new[] { 0.0, 1.0 };
            //act
            var lossMean = inputData.LossMean();
            //assert
            Assert.AreEqual(double.NaN, lossMean);
        }

        [Test]
        public void returns_the_same_as_mean_when_all_values_are_negative()
        {
            //arrange
            var inputData = new[] { -1.0, -2.0 };
            var mean = inputData.Mean();
            //act
            var lossMean = inputData.LossMean();
            //assert
            Assert.AreEqual(mean, lossMean);
        }

        [Test]
        public void does_not_use_positive_input_values()
        {
            //arrange
            var inputData = new[] { -1.0, 2.0 };
            //act
            var lossMean = inputData.LossMean();
            //assert
            Assert.AreEqual(-1.0, lossMean);
        }

        [Test]
        public void throws_when_input_data_is_null()
        {
            //arrange
            List<double> inputData = null;
            //act
// ReSharper disable ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => AbsoluteReturnMeasures.LossMean(inputData));
// ReSharper restore ExpressionIsAlwaysNull
        }

        [Test]
        public void returns_NaN_with_no_input_data()
        {
            //arrange
            var inputData = new List<double>();
            //act
            var lossMean = inputData.LossMean();
            //assert
            Assert.AreEqual(double.NaN, lossMean);
        }
    }
}
