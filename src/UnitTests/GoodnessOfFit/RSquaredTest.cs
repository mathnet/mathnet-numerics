using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.GoodnessOfFit
{
    [TestFixture]
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
                data.Add(i);

            Assert.That(Numerics.GoodnessOfFit.RSquared(data, data), Is.EqualTo(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WhenGivenTwoDatasetsOfDifferentSizeThenThrowsArgumentException()
        {
            var observedData = new List<double>()
            {
                23
                ,9
                ,5
                ,7
                ,10
                ,5
                ,4
                ,1
                ,2
                ,1
            };

            var modelledData = new List<double>()
            {
                8
                ,9
                ,10
            };

            Numerics.GoodnessOfFit.RSquared(modelledData, observedData);

            Assert.Fail("Expected ArgumentException exception wasn't thrown");
        }

        [Test]
        public void WhenCalculatingRSquaredOfUnevenDistributionWithLInearDistributionThenRSquaredIsCalculated()
        {
            var observedData = new List<double>()
            {
                1, 2.3, 3.1, 4.8, 5.6, 6.3
            };

            var modelledData = new List<double>()
            {
                2.6, 2.8, 3.1, 4.7, 5.1, 5.3
            };

            Assert.That(Math.Round(Numerics.GoodnessOfFit.RSquared(modelledData, observedData), 11), Is.EqualTo(Math.Round(0.94878520708673d, 11)));
        }
    }
}
