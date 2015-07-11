// <copyright file="IndicatorsTests.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Financial;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.FinancialTests
{
    [TestFixture, Category("Financial")]
    public class IndicatorsTests
    {
        [Test]
        public void Atr_ShouldReturnValuesWithoutException()
        {
            var inputBars = GenerateValidBars();
            var atr = inputBars.ATR(14);
        }

        [Test]
        public void Atr_ShouldReturnValue()
        {
            var inputBars = GenerateValidBars();
            var atr = inputBars.ATR(14);

            var expectedValueAtBar20 = 132.33;
            var actual = atr.ElementAt(20);
            Assert.AreEqual(expectedValueAtBar20, actual);
        }

        [Test]
        public void Atr_ShouldRaiseException_IfPeriodIsZero()
        {
            var inputBars = GenerateValidBars();
            var period = 0;

            Assert.That(() => inputBars.ATR(period), Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test, Ignore("Unclear why this should be enforced")]
        public void Atr_ShouldRaiseException_IfPeriodIsGreaterThanBarCount()
        {
            var inputBars = GenerateValidBars();
            inputBars = inputBars.Take(10);
            var period = 11;

            Assert.That(() => inputBars.ATR(period), Throws.Exception.TypeOf<ArgumentException>());
        }

        private IEnumerable<Bar> GenerateValidBars()
        {
            StockDataReader reader = new StockDataReader();
            var data = reader.ReadFile("./data/Finance/DaxHistoricalData.dat");

            List<Bar> inputBars = new List<Bar>();

            foreach (var stockData in data)
            {
                var bar = new Bar(stockData.High, stockData.Low, stockData.Open, stockData.Close);
                inputBars.Add(bar);
            }
            return inputBars;
        }
    }
}
