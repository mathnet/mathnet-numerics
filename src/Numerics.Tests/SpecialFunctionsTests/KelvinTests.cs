using NUnit.Framework;
using System;

namespace MathNet.Numerics.Tests.SpecialFunctionsTests
{
    /// <summary>
    /// Kelvin functions tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class KelvinTests
    {
        [Test]
        public void KelvinBerApprox([Range(-8, 8, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.11.1
            Assert.AreEqual(Polynomial.Evaluate(x / 8.0,
                1.0,
                0.0, 0.0, 0.0, -64.0, 0.0,
                0.0, 0.0, 113.77777774, 0.0, 0.0,
                0.0, -32.36345652, 0.0, 0.0, 0.0,
                2.64191397, 0.0, 0.0, 0.0, -0.08349609,
                0.0, 0.0, 0.0, 0.00122552, 0.0,
                0.0, 0.0, -0.00000901), SpecialFunctions.KelvinBer(x), 1e-9);
        }

        [Test]
        public void KelvinBeiApprox([Range(-8, 8, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.11.2
            Assert.AreEqual(Polynomial.Evaluate(x / 8.0,
                0.0,
                0.0, 16.0, 0.0, 0.0, 0.0,
                -113.77777774, 0.0, 0.0, 0.0, 72.81777742,
                0.0, 0.0, 0.0, -10.56765779, 0.0,
                0.0, 0.0, 0.52185615, 0.0, 0.0,
                0.0, -0.01103667, 0.0, 0.0, 0.0,
                0.00011346),
                SpecialFunctions.KelvinBei(x), 6e-9);
        }

        [Test]
        public void KelvinKerApprox([Range(0.25, 8, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.11.3
            Assert.AreEqual(
                Polynomial.Evaluate(x / 8.0,
                -Math.Log(x / 2.0) * SpecialFunctions.KelvinBer(x) + SpecialFunctions.KelvinBei(x) * Constants.PiOver4 - 0.57721566,
                0.0, 0.0, 0.0, -59.05819744, 0.0,
                0.0, 0.0, 171.36272133, 0.0, 0.0,
                0.0, -60.60977451, 0.0, 0.0, 0.0,
                5.65539121, 0.0, 0.0, 0.0, -0.19636347,
                0.0, 0.0, 0.0, 0.00309699, 0.0,
                0.0, 0.0, -0.00002458),
                SpecialFunctions.KelvinKer(x), 1e-8);
        }

        [Test]
        public void KelvinKeiApprox([Range(0.25, 8, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.11.4
            Assert.AreEqual(
                -Math.Log(x / 2.0) * SpecialFunctions.KelvinBei(x) - Constants.PiOver4 * SpecialFunctions.KelvinBer(x)
                + Polynomial.Evaluate(x / 8.0,
                0.0,
                0.0, 6.76454936, 0.0, 0.0, 0.0,
                -142.91827687, 0.0, 0.0, 0.0, 124.23569650,
                0.0, 0.0, 0.0, -21.30060904, 0.0,
                0.0, 0.0, 1.17509064, 0.0, 0.0,
                0.0, -0.02695875, 0.0, 0.0, 0.0,
                0.00029532),
                SpecialFunctions.KelvinKei(x), 3e-9);
        }

        [Test]
        public void KelvinBerPrimeApprox([Range(-8, 8, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.11.5
            Assert.AreEqual(x * Polynomial.Evaluate(x / 8.0,
                0.0,
                0.0, -4.0, 0.0, 0.0, 0.0,
                14.22222222, 0.0, 0.0, 0.0, -6.06814810,
                0.0, 0.0, 0.0, 0.66047849, 0.0,
                0.0, 0.0, -0.02609253, 0.0, 0.0,
                0.0, 0.00045957, 0.0, 0.0, 0.0,
                -0.00000394), SpecialFunctions.KelvinBerPrime(x), 2.1e-8);
        }

        [Test]
        public void KelvinBeiPrimeApprox([Range(-8, 8, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.11.6
            Assert.AreEqual(x * Polynomial.Evaluate(x / 8.0,
                0.5,
                0.0, 0.0, 0.0, -10.66666666, 0.0,
                0.0, 0.0, 11.37777772, 0.0, 0.0,
                0.0, -2.31167514, 0.0, 0.0, 0.0,
                0.14677204, 0.0, 0.0, 0.0, -0.00379386,
                0.0, 0.0, 0.0, 0.00004609),
                SpecialFunctions.KelvinBeiPrime(x), 7e-8);
        }

        [Test]
        public void KelvinKerPrimeApprox([Range(0.25, 8, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.11.7
            Assert.AreEqual(
                -Math.Log(x / 2.0) * SpecialFunctions.KelvinBerPrime(x) - SpecialFunctions.KelvinBer(x) / x + Constants.PiOver4 * SpecialFunctions.KelvinBeiPrime(x)
                + x * Polynomial.Evaluate(x / 8.0,
                0.0,
                0.0, -3.69113734, 0.0, 0.0, 0.0,
                21.42034017, 0.0, 0.0, 0.0, -11.36433272,
                0.0, 0.0, 0.0, 1.41384780, 0.0,
                0.0, 0.0, -0.06136358, 0.0, 0.0,
                0.0, 0.00116137, 0.0, 0.0, 0.0,
                -0.00001075),
                SpecialFunctions.KelvinKerPrime(x), 8e-8);
        }

        [Test]
        public void KelvinKeiPrimeApprox([Range(0.25, 8, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.11.8
            Assert.AreEqual(
                -Math.Log(x / 2.0) * SpecialFunctions.KelvinBeiPrime(x) - SpecialFunctions.KelvinBei(x) / x - Constants.PiOver4 * SpecialFunctions.KelvinBerPrime(x)
                + x * Polynomial.Evaluate(x / 8.0,
                0.21139217,
                0.0, 0.0, 0.0, -13.39858846, 0.0,
                0.0, 0.0, 19.41182758, 0.0, 0.0,
                0.0, -4.65950823, 0.0, 0.0, 0.0,
                0.33049424, 0.0, 0.0, 0.0, -0.00926707,
                0.0, 0.0, 0.0, 0.00011997),
                SpecialFunctions.KelvinKeiPrime(x), 7e-8);
        }
    }
}
