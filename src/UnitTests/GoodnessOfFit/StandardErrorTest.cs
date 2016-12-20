using System;
using System.Linq;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.GoodnessOfFit
{
    [TestFixture, Category("Regression")]
    public class StandardErrorTest
    {
        [Test]
        public void ComputesPopulationStandardErrorOfTheRegression()
        {
            // Definition as described at: http://onlinestatbook.com/lms/regression/accuracy.html
            var xes = new[] { 1.0, 2, 3, 4, 5 };
            var ys = new[] { 1, 2, 1.3, 3.75, 2.25 };
            var fit = Fit.Line(xes, ys);
            var a = fit.Item1;
            var b = fit.Item2;
            var predictedYs = xes.Select(x => a + b * x);
            var standardError = Numerics.GoodnessOfFit.PopulationStandardError(predictedYs, ys);

            Assert.AreEqual(0.747, standardError, 1e-3);
        }

        [Test]
        public void ComputesSampleStandardErrorOfTheRegression()
        {
            // Definition as described at: http://onlinestatbook.com/lms/regression/accuracy.html
            var xes = new[] { 1.0, 2, 3, 4, 5 };
            var ys = new[] { 1, 2, 1.3, 3.75, 2.25 };
            var fit = Fit.Line(xes, ys);
            var a = fit.Item1;
            var b = fit.Item2;
            var predictedYs = xes.Select(x => a + b * x);
            var standardError = Numerics.GoodnessOfFit.SampleStandardError(predictedYs, ys, degreesOfFreedom: 2);

            Assert.AreEqual(0.964, standardError, 1e-3);
        }

        [Test]
        public void PopulationStandardErrorShouldThrowIfInputsSequencesDifferInLength()
        {
            var y1 = new[] { 0.0, 1 };
            var y2 = new[] { 1.0 };

            Assert.Throws<ArgumentOutOfRangeException>(() => Numerics.GoodnessOfFit.PopulationStandardError(y1, y2));
        }

        [Test]
        public void SampleStandardErrorShouldThrowIfSampleSizeIsSmallerThanGivenDegreesOfFreedom()
        {
            var modelled = new[] { 1.0 };
            var observed = new[] { 1.0 };
            Assert.Throws<ArgumentOutOfRangeException>(() => Numerics.GoodnessOfFit.SampleStandardError(modelled, observed, 2));
        }
    }
}
