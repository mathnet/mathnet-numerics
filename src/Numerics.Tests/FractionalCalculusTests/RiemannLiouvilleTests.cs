using NUnit.Framework;
using System;

namespace MathNet.Numerics.Tests.FractionalCalculus
{
    [TestFixture, Category("DifferIntegration")]
    public class RiemannLiouvilleTests
    {
        [TestCase(3.0, 3.0, 0.0)]                               // D(3) x^2 = 0
        [TestCase(2.5, 3.0, 0.651470015870559895448512830451)]  // D(5/2) x^2 = Gamma(3)/Gamma(1/2)*x^(-1/2)
        [TestCase(2.0, 3.0, 2.0)]                               // D(2) x^2 = 2
        [TestCase(1.5, 3.0, 3.90882009522335937269107698271)]   // D(3/2) x^2 = Gamma(3)/Gamma(3/2)*x^(1/2)        
        [TestCase(1.0, 3.0, 6.0)]                               // D(1) x^2 = 2 x
        [TestCase(0.5, 3.0, 7.81764019044671874538215396541)]   // D(1/2) x^2 = Gamma(3)/Gamma(5/2)*x^(3/2)        
        [TestCase(0.0, 3.0, 9.0)]                               // D(0) x^2 = x^2
        [TestCase(-0.5, 3.0, 9.38116822853606249445858475850)]  // D(-1/2) x^2 = Gamma(3)/Gamma(7/2)*x^(5/2)        
        [TestCase(-1.0, 3.0, 9.0)]                              // D(-1) x^2 = x^3/3
        [TestCase(-1.5, 3.0, 8.04100133874519642382164407871)]  // D(-3/2) x^2 = Gamma(3)/Gamma(9/2)*x^(7/2)        
        [TestCase(-2.0, 3.0, 6.75)]                             // D(-2) x^2 = x^4/12
        [TestCase(-2.5, 3.0, 5.36066755916346428254776271914)]  // D(-5/2) x^2 = Gamma(3)/Gamma(11/2)*x^(9/2)        
        [TestCase(-3.0, 3.0, 4.05)]                             // D(-3) x^2 = x^5/60
        public void TestPolynomial(double n, double x, double expected)
        {
            Func<double, double> f = (t) => Math.Pow(t, 2);

            Assert.LessOrEqual(
                (expected - DifferIntegrate.DoubleExponential(f, x, n, x0: 0.0)) / expected,
                1E-7,
                "D({0}) x^2 (Double-Exponetial) where x = {1}", n, x);

            Assert.LessOrEqual(
                (expected - DifferIntegrate.GaussLegendre(f, x, n, x0: 0.0)) / expected,
                1E-6,
                "D({0}) x^2 (Gauss-Legendre) where x = {1}", n, x);

            Assert.LessOrEqual(
                (expected - DifferIntegrate.GaussKronrod(f, x, n, x0: 0.0)) / expected,
                1E-4,
                "D({0}) x^2 (Gauss-Kronrod) where x = {1}", n, x);
        }

        [TestCase(3.0, -1.5, 0.278538406900792139127369841989)]     // D(3) exp(π x) = exp(π x) π^3,
        [TestCase(2.5, -1.5, 0.157148467791413402880359987678)]     // D(5/2) exp(π x) = exp(π x) π^(2.5),        
        [TestCase(2.0, -1.5, 0.0886615285984055199686857617477)]    // D(2) exp(π x) = exp(π x) π^2,
        [TestCase(1.5, -1.5, 0.0500219108966418944762345595265)]    // D(3/2) exp(π x) = exp(π x) π^(1.5),        
        [TestCase(1.0, -1.5, 0.0282218410770393627236690193887)]    // D(1) exp(π x) = exp(π x) π,
        [TestCase(0.5, -1.5, 0.0159224687642057998088504370906)]    // D(1/2) exp(π x) = exp(π x) π^(0.5),        
        [TestCase(0.0, -1.5, 0.00898329102112942788966495190793)]   // D(0) exp(π x) = exp(π x)
        [TestCase(-0.5, 3.5, 33631.1952282529497668583487591)]      // D(-1/2) exp(π x) = exp(π x)/π^(0.5)        
        [TestCase(-1.0, 3.5, 18974.3700300413201713375621666)]      // D(-1) exp(π x) = exp(π x)/π
        [TestCase(-1.5, 3.5, 10705.1419253300403750707800464)]      // D(-3/2) exp(π x) = exp(π x)/π^(1.5)        
        [TestCase(-2.0, 3.5, 6039.72956467158140885534431539)]      // D(-2) exp(π x) = exp(π x)/π^2
        [TestCase(-2.5, 3.5, 3407.55250783313088752769495213)]      // D(-5/2) exp(π x) = exp(π x)/π^(2.5) 
        [TestCase(-3.0, 3.5, 1922.50563031148665828996231149)]      // D(-3) exp(π x) = exp(π x)/π^3
        public void TestExponential(double n, double x, double expected)
        {
            Func<double, double> f = (t) => Math.Exp(Math.PI * t);

            Assert.LessOrEqual(
               (expected - DifferIntegrate.DoubleExponential(f, x, n, x0: double.NegativeInfinity)) / expected,
               1E-9, 
               "D({0}) exp(π x) (Double-Exponential) where x = {1}", n, x);

            Assert.LessOrEqual(
               (expected - DifferIntegrate.GaussLegendre(f, x, n, x0: double.NegativeInfinity)) / expected,
               1E-6,
               "D({0}) exp(π x) (Gauss-Legendre) where x = {1}", n, x);

            Assert.LessOrEqual(
               (expected - DifferIntegrate.GaussKronrod(f, x, n, x0: double.NegativeInfinity)) / expected,
               1E-7,
               "D({0}) exp(π x) (Gauss-Kronrod) where x = {1}", n, x);
        }
    }
}
