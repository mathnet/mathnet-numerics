// <copyright file="GainLossRatioTests.cs" company="Math.NET">
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
    public class GainLossRatioTests
    {
        [Test]
        public void throws_when_input_data_is_null()
        {
            //arrange
            List<double> inputData = null;
            //act
// ReSharper disable ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => AbsoluteRiskMeasures.GainLossRatio(inputData));
// ReSharper restore ExpressionIsAlwaysNull
        }

        [Test]
        //Not sure this is correct.  Undefined may be more correct.
        public void returns_NaN_for_a_single_positive_input()
        {
            //arrange
            var inputData = new[] { 1.0 };
            //act
            var gainLossRatio = inputData.GainLossRatio();
            //assert
            Assert.AreEqual(double.NaN, gainLossRatio);
        }

        [Test]
        //Not sure this is correct.  Undefined may be more correct.
        public void returns_NaN_for_a_single_negative_input()
        {
            //arrange
            var inputData = new[] { -1.0 };
            //act
            var gainLossRatio = inputData.GainLossRatio();
            //assert
            Assert.AreEqual(double.NaN, gainLossRatio);
        }

        [Test]
        //Not sure this is correct.  Undefined may be more correct.
        public void returns_NaN_for_a_set_of_all_positive_numbers()
        {
            //arrange
            var inputData = new[] { 1.0, 2.0, 3.0 };
            //act
            var gainLossRatio = inputData.GainLossRatio();
            //assert
            Assert.AreEqual(double.NaN, gainLossRatio);
        }

        [Test]
        //Not sure this is correct.  Undefined may be more correct.
        public void returns_NaN_for_a_set_of_all_negative_numbers()
        {
            //arrange
            var inputData = new[] { -1.0, -2.0, -3.0 };
            //act
            var gainLossRatio = inputData.GainLossRatio();
            //assert
            Assert.AreEqual(double.NaN, gainLossRatio);
        }

        [Test]
        public void handles_a_value_of_zero_as_a_positive()
        {
            //arrange
            var inputData = new[] { 0.0, -1.0 };
            //act
            var gainLossRatio = inputData.GainLossRatio();
            //assert
            Assert.AreEqual(0.0, gainLossRatio); //0.0 / -1.0 => 0.0
        }

        [Test]
        public void calculates_the_correct_ratio_given_a_set_of_gains_and_losses()
        {
            //arrange
            var inputData = new[] { -2.0, -1.0, 0.0, 1.0, 2.0 };
            var meanOfGains = inputData.Where(x => x >= 0).Mean();
            var meanOfLosses = inputData.Where(x => x < 0).Mean();
            var expectedRatio = Math.Abs(meanOfGains / meanOfLosses);
            //act
            var gainLossRatio = inputData.GainLossRatio();
            //assert
            Assert.AreEqual(expectedRatio, gainLossRatio);
        }
    }
}
