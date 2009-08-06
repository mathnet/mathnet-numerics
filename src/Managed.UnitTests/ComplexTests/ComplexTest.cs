using System;
using System.Globalization;
using MbUnit.Framework;

namespace MathNet.Numerics.UnitTests
{
    [TestFixture]
    public class ComplexTest
    {
        [Test, MultipleAsserts]
        public void CanCreateComplexNumberUsingTheConstructor()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(1.1, complex.Real, "Real part is 1.1.");
            Assert.AreEqual(-2.2, complex.Imaginary, "Imaginary part is -2.2.");
        }

        [Test, MultipleAsserts]
        public void CanCreateComplexNumberWithRealImaginaryIntializer()
        {
            var complex = Complex.WithRealImaginary(1.1, -2.2);
            Assert.AreEqual(1.1, complex.Real, "Real part is 1.1.");
            Assert.AreEqual(-2.2, complex.Imaginary, "Imaginary part is -2.2.");
        }

        [Test]
        public void WithModulusArgumentThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Complex.WithModulusArgument(-1, 1), "Throws exception because modulus is negative.");
        }

        [Test, MultipleAsserts]
        public void CanCreateComplexNumberWithModulusArgument()
        {
            var complex = Complex.WithModulusArgument(2, -Math.PI / 6);
            Assert.AreApproximatelyEqual(Math.Sqrt(3), complex.Real, 1e-15, "Real part is Sqrt(3).");
            Assert.AreApproximatelyEqual(-1, complex.Imaginary, 1e-15, "Imaginary part is -1.");
        }

        [Test]
        public void CanDetermineIfZeroValueComplexNumber()
        {
            var complex = new Complex(0, 0);
            Assert.IsTrue(complex.IsZero, "Zero complex number.");
        }

        [Test]
        public void CanDetermineIfOneValueComplexNumber()
        {
            var complex = new Complex(1, 0);
            Assert.IsTrue(complex.IsOne, "Complex number with a value of one.");
        }

        [Test]
        public void CanDetermineIfImaginaryUnit()
        {
            var complex = new Complex(0, 1);
            Assert.IsTrue(complex.IsI, "Imaginary unit");
        }

        [Test, MultipleAsserts]
        public void CanDetermineIfNaN()
        {
            var complex = new Complex(double.NaN, 1);
            Assert.IsTrue(complex.IsNaN, "Real part is NaN.");
            complex = new Complex(1, double.NaN);
            Assert.IsTrue(complex.IsNaN, "Imaginary part is NaN.");
            complex = new Complex(double.NaN, double.NaN);
            Assert.IsTrue(complex.IsNaN, "Both parts are NaN.");
        }

        [Test, MultipleAsserts]
        public void CanDetermineIfInfinity()
        {
            var complex = new Complex(double.PositiveInfinity, 1);
            Assert.IsTrue(complex.IsInfinity, "Real part is infinity.");
            complex = new Complex(1, double.NegativeInfinity);
            Assert.IsTrue(complex.IsInfinity, "Imaginary part is infinity.");
            complex = new Complex(double.NegativeInfinity, double.PositiveInfinity);
            Assert.IsTrue(complex.IsInfinity, "Both parts are infinity.");
        }

        [Test]
        public void CanDetermineIfRealNumber()
        {
            var complex = new Complex(-1, 0);
            Assert.IsTrue(complex.IsReal, "Is a real number.");
        }

        [Test]
        public void CanDetermineIfRealNonNegativeNumber()
        {
            var complex = new Complex(1, 0);
            Assert.IsTrue(complex.IsReal, "Is a real non-negative number.");
        }

        [Test, MultipleAsserts]
        public void CanCalculateHashCode()
        {
            var complex = new Complex(1, 0);
            Assert.AreEqual(1072693248, complex.GetHashCode());
            complex = new Complex(0, 1);
            Assert.AreEqual(-1072693248, complex.GetHashCode());
            complex = new Complex(1, 1);
            Assert.AreEqual(-2097152, complex.GetHashCode());
        }

        [Test, MultipleAsserts]
        public void CanCreateStringFromComplexNumber()
        {
            Assert.AreEqual("NaN", Complex.NaN.ToString());
            Assert.AreEqual("Infinity", Complex.Infinity.ToString());
            Assert.AreEqual("1.1", new Complex(1.1, 0).ToString());
            Assert.AreEqual("-1.1i", new Complex(0, -1.1).ToString());
            Assert.AreEqual("1.1i", new Complex(0, 1.1).ToString());
            Assert.AreEqual("1.1 + 1.1i", new Complex(1.1, 1.1).ToString());
        }

        [Test, MultipleAsserts]
        public void CanTestForEquality()
        {
            Assert.AreNotEqual(Complex.NaN, Complex.NaN);
            Assert.AreEqual(Complex.Infinity, Complex.Infinity);
            Assert.AreEqual(new Complex(1.1, -2.2), new Complex(1.1, -2.2));
            Assert.AreNotEqual(new Complex(-1.1, 2.2), new Complex(1.1, -2.2));
        }

        [Test, MultipleAsserts]
        public void CanCreateStringUsingNumberFormat()
        {
            Assert.AreEqual("NaN", Complex.NaN.ToString("#.000"));
            Assert.AreEqual("Infinity", Complex.Infinity.ToString("#.000"));
            Assert.AreEqual("1.100", new Complex(1.1, 0).ToString("#.000"));
            Assert.AreEqual("-1.100i", new Complex(0, -1.1).ToString("#.000"));
            Assert.AreEqual("1.100i", new Complex(0, 1.1).ToString("#.000"));
            Assert.AreEqual("1.100 + 1.100i", new Complex(1.1, 1.1).ToString("#.000"));
        }

        [Test, MultipleAsserts]
        public void CanCreateStringUsingFormatProvider()
        {
            var provider = CultureInfo.GetCultureInfo("tr-TR");
            Assert.AreEqual("NaN", Complex.NaN.ToString(provider));
            Assert.AreEqual("Infinity", Complex.Infinity.ToString(provider));
            Assert.AreEqual("1,1", new Complex(1.1, 0).ToString(provider));
            Assert.AreEqual("-1,1i", new Complex(0, -1.1).ToString(provider));
            Assert.AreEqual("1,1i", new Complex(0, 1.1).ToString(provider));
            Assert.AreEqual("1,1 + 1,1i", new Complex(1.1, 1.1).ToString(provider));
        }

        [Test, MultipleAsserts]
        public void CanTestForEqualityUsingOperators()
        {
            AssertEx.That(() => Complex.NaN != Complex.NaN);
            AssertEx.That(() => Complex.Infinity == Complex.Infinity);
            AssertEx.That(() => new Complex(1.1, -2.2) == new Complex(1.1, -2.2));
            AssertEx.That(() => new Complex(-1.1, 2.2) != new Complex(1.1, -2.2));
        }

        [Test, MultipleAsserts]
        public void CanConvertDoubleToComplex()
        {
            AssertEx.That(() => ((Complex) double.NaN).IsNaN);
            AssertEx.That(() => ((Complex) double.NegativeInfinity).IsInfinity);
            Assert.AreEqual((Complex) 1.1, new Complex(1.1, 0));
        }

        [Test]
        public void CanUsePlusOperator()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(complex, +complex);
        }

        [Test]
        public void CanNegateValueUsingOperator()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(new Complex(-1.1, 2.2), -complex);
        }

        [Test]
        public void CanUsePlus()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(complex, complex.Plus());
        }

        [Test]
        public void CanNegateValue()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(new Complex(-1.1, 2.2), complex.Negate());
        }

        [Test, MultipleAsserts]
        public void CanAddTwoComplexNumbersUsingOperartor()
        {
            AssertEx.That(() => (Complex.NaN + Complex.NaN).IsNaN);
            AssertEx.That(() => (Complex.Infinity + Complex.One).IsInfinity);
            AssertEx.That(() => (Complex.One + Complex.Zero) == Complex.One);
            AssertEx.That(() => (new Complex(1.1, -2.2) + new Complex(-1.1, 2.2)) == Complex.Zero);
        }

        [Test, MultipleAsserts]
        public void CanAddTwoComplexNumbers()
        {
            AssertEx.That(() => (Complex.NaN.Add(Complex.NaN)).IsNaN);
            AssertEx.That(() => (Complex.Infinity.Add(Complex.One).IsInfinity));
            AssertEx.That(() => (Complex.One.Add(Complex.Zero)) == Complex.One);
            AssertEx.That(() => (new Complex(1.1, -2.2).Add(new Complex(-1.1, 2.2))) == Complex.Zero);
        }

        [Test, MultipleAsserts]
        public void CanAddComplexNumberAndDoubleUsingOperartor()
        {
            AssertEx.That(() => (Complex.NaN + double.NaN).IsNaN);
            AssertEx.That(() => (double.NaN + Complex.NaN).IsNaN);
            AssertEx.That(() => (double.PositiveInfinity + Complex.One).IsInfinity);
            AssertEx.That(() => (Complex.Infinity + 1.0).IsInfinity);
            AssertEx.That(() => (Complex.One + 0.0) == Complex.One);
            AssertEx.That(() => (0.0 + Complex.One) == Complex.One);
            AssertEx.That(() => (new Complex(1.1, -2.2) + 1.1 == new Complex(2.2, -2.2)));
            AssertEx.That(() => -2.2 + new Complex(-1.1, 2.2) == new Complex(-3.3, 2.2));
        }

        [Test, MultipleAsserts]
        public void CanAddSubtractComplexNumbersUsingOperartor()
        {
            AssertEx.That(() => (Complex.NaN - Complex.NaN).IsNaN);
            AssertEx.That(() => (Complex.Infinity - Complex.One).IsInfinity);
            AssertEx.That(() => (Complex.One - Complex.Zero) == Complex.One);
            AssertEx.That(() => (new Complex(1.1, -2.2) - new Complex(1.1, -2.2)) == Complex.Zero);
        }

        [Test, MultipleAsserts]
        public void CanSubtractTwoComplexNumbers()
        {
            AssertEx.That(() => (Complex.NaN.Subtract(Complex.NaN)).IsNaN);
            AssertEx.That(() => (Complex.Infinity.Subtract(Complex.One)).IsInfinity);
            AssertEx.That(() => (Complex.One.Subtract(Complex.Zero)) == Complex.One);
            AssertEx.That(() => (new Complex(1.1, -2.2).Subtract(new Complex(1.1, -2.2))) == Complex.Zero);
        }

        [Test, MultipleAsserts]
        public void CanSubtractComplexNumberAndDoubleUsingOperartor()
        {
            AssertEx.That(() => (Complex.NaN - double.NaN).IsNaN);
            AssertEx.That(() => (double.NaN - Complex.NaN).IsNaN);
            AssertEx.That(() => (double.PositiveInfinity - Complex.One).IsInfinity);
            AssertEx.That(() => (Complex.Infinity - 1.0).IsInfinity);
            AssertEx.That(() => (Complex.One - 0.0) == Complex.One);
            AssertEx.That(() => (0.0 - Complex.One) == -Complex.One);
            AssertEx.That(() => (new Complex(1.1, -2.2) - 1.1 == new Complex(0.0, -2.2)));
            AssertEx.That(() => -2.2 - new Complex(-1.1, 2.2) == new Complex(-1.1, -2.2));
        }

        [Test, MultipleAsserts]
        public void CanMultipleTwoComplexNumbersUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN * Complex.One).IsNaN);
            Assert.AreEqual(new Complex(0, 16), new Complex(4, -4) * new Complex(-2, 2));
        }

        [Test, MultipleAsserts]
        public void CanMultipleTwoComplexNumbers()
        {
            AssertEx.That(() => (Complex.NaN.Multiply(Complex.One).IsNaN));
            Assert.AreEqual(new Complex(0, 16), new Complex(4, -4).Multiply(new Complex(-2, 2)));
        }

        [Test, MultipleAsserts]
        public void CanMultipleComplexNumberAndDoubleUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN * 1.0).IsNaN);
            Assert.AreEqual(new Complex(8, -8), new Complex(4, -4) * 2);
            Assert.AreEqual(new Complex(8, -8), 2 * new Complex(4, -4));
        }

        [Test, MultipleAsserts]
        public void CanDivideTwoComplexNumbersUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN / Complex.One).IsNaN);
            Assert.AreEqual(new Complex(-2, 0), new Complex(4, -4) / new Complex(-2, 2));
            Assert.AreEqual(Complex.Infinity, Complex.One / Complex.Zero);
        }

        [Test, MultipleAsserts]
        public void CanDivideTwoComplexNumbers()
        {
            AssertEx.That(() => (Complex.NaN.Multiply(Complex.One).IsNaN));
            Assert.AreEqual(new Complex(-2, 0), new Complex(4, -4).Divide(new Complex(-2, 2)));
            Assert.AreEqual(Complex.Infinity, Complex.One.Divide(Complex.Zero));
        }

        [Test, MultipleAsserts]
        public void CanDivideComplexNumberAndDoubleUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN * 1.0).IsNaN);
            Assert.AreEqual(new Complex(-2, 2), new Complex(4, -4) / -2);
            Assert.AreEqual(new Complex(0.25, 0.25), 2 / new Complex(4, -4));
            Assert.AreEqual(Complex.Infinity, Complex.One / 0);
        }
    }
}