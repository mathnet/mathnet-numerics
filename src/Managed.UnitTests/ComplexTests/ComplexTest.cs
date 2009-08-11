namespace MathNet.Numerics.UnitTests
{
    using System;
    using System.Globalization;

    using MbUnit.Framework;

    /// <summary>
    /// The complex test.
    /// </summary>
    [TestFixture]
    public class ComplexTest
    {

        /// <summary>
        /// The can add complex number and double using operartor.
        /// </summary>
        [Test]
        [MultipleAsserts]
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

        /// <summary>
        /// The can add subtract complex numbers using operartor.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanAddSubtractComplexNumbersUsingOperartor()
        {
            AssertEx.That(() => (Complex.NaN - Complex.NaN).IsNaN);
            AssertEx.That(() => (Complex.Infinity - Complex.One).IsInfinity);
            AssertEx.That(() => (Complex.One - Complex.Zero) == Complex.One);
            AssertEx.That(() => (new Complex(1.1, -2.2) - new Complex(1.1, -2.2)) == Complex.Zero);
        }

        /// <summary>
        /// The can add two complex numbers.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanAddTwoComplexNumbers()
        {
            AssertEx.That(() => Complex.NaN.Add(Complex.NaN).IsNaN);
            AssertEx.That(() => Complex.Infinity.Add(Complex.One).IsInfinity);
            AssertEx.That(() => Complex.One.Add(Complex.Zero) == Complex.One);
            AssertEx.That(() => new Complex(1.1, -2.2).Add(new Complex(-1.1, 2.2)) == Complex.Zero);
        }

        /// <summary>
        /// The can add two complex numbers using operartor.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanAddTwoComplexNumbersUsingOperartor()
        {
            AssertEx.That(() => (Complex.NaN + Complex.NaN).IsNaN);
            AssertEx.That(() => (Complex.Infinity + Complex.One).IsInfinity);
            AssertEx.That(() => (Complex.One + Complex.Zero) == Complex.One);
            AssertEx.That(() => (new Complex(1.1, -2.2) + new Complex(-1.1, 2.2)) == Complex.Zero);
        }

        /// <summary>
        /// The can calculate hash code.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanCalculateHashCode()
        {
            var complex = new Complex(1, 0);
            Assert.AreEqual(1072693248, complex.GetHashCode());
            complex = new Complex(0, 1);
            Assert.AreEqual(-1072693248, complex.GetHashCode());
            complex = new Complex(1, 1);
            Assert.AreEqual(-2097152, complex.GetHashCode());
        }

        /// <summary>
        /// The can compute exponential.
        /// </summary>
        /// <param name="real">
        /// The real.
        /// </param>
        /// <param name="imag">
        /// The imag.
        /// </param>
        /// <param name="expectedReal">
        /// The expected real.
        /// </param>
        /// <param name="expectedImag">
        /// The expected imag.
        /// </param>
        [Test]
        [Row(0.0, 0.0, 1.0, 0.0)]
        [Row(0.0, 1.0, 0.54030230586813977, 0.8414709848078965)]
        [Row(-1.0, 1.0, 0.19876611034641295, 0.30955987565311222)]
        [Row(-111.1, 111.1, -2.3259065941590448e-49, -5.1181940185795617e-49)]
        public void CanComputeExponential(double real, double imag, double expectedReal, double expectedImag)
        {
            var value = new Complex(real, imag);
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, value.Exponential(), 15);
        }

        /// <summary>
        /// The can compute natural logarithm.
        /// </summary>
        /// <param name="real">
        /// The real.
        /// </param>
        /// <param name="imag">
        /// The imag.
        /// </param>
        /// <param name="expectedReal">
        /// The expected real.
        /// </param>
        /// <param name="expectedImag">
        /// The expected imag.
        /// </param>
        [Test]
        [Row(0.0, 0.0, double.NegativeInfinity, 0.0)]
        [Row(0.0, 1.0, 0.0, 1.5707963267948966)]
        [Row(-1.0, 1.0, 0.34657359027997264, 2.3561944901923448)]
        [Row(-111.1, 111.1, 5.0570042869255571, 2.3561944901923448)]
        [Row(111.1, -111.1, 5.0570042869255571, -0.78539816339744828)]
        public void CanComputeNaturalLogarithm(double real, double imag, double expectedReal, double expectedImag)
        {
            var value = new Complex(real, imag);
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, value.NaturalLogarithm(), 15);
        }

        /// <summary>
        /// The can compute power.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanComputePower()
        {
            var a = new Complex(1.19209289550780998537e-7, 1.19209289550780998537e-7);
            var b = new Complex(1.19209289550780998537e-7, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(
                new Complex(9.99998047207974718744e-1, -1.76553541154378695012e-6), a.Power(b), 15);
            a = new Complex(0.0, 1.19209289550780998537e-7);
            b = new Complex(0.0, -1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(1.00000018725172576491, 1.90048076369011843105e-6), a.Power(b), 15);
            a = new Complex(0.0, -1.19209289550780998537e-7);
            b = new Complex(0.0, 0.5);
            AssertHelpers.AlmostEqual(new Complex(-2.56488189382693049636e-1, -2.17823120666116144959), a.Power(b), 15);
            a = new Complex(0.0, 0.5);
            b = new Complex(0.0, -0.5);
            AssertHelpers.AlmostEqual(new Complex(2.06287223508090495171, 7.45007062179724087859e-1), a.Power(b), 15);
            a = new Complex(0.0, -0.5);
            b = new Complex(0.0, 1.0);
            AssertHelpers.AlmostEqual(new Complex(3.70040633557002510874, -3.07370876701949232239), a.Power(b), 15);
            a = new Complex(0.0, 2.0);
            b = new Complex(0.0, -2.0);
            AssertHelpers.AlmostEqual(new Complex(4.24532146387429353891, -2.27479427903521192648e1), a.Power(b), 15);
            a = new Complex(0.0, -8.388608e6);
            b = new Complex(1.19209289550780998537e-7, 0.0);
            AssertHelpers.AlmostEqual(new Complex(1.00000190048219620166, -1.87253870018168043834e-7), a.Power(b), 15);
        }

        /// <summary>
        /// The can compute root.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanComputeRoot()
        {
            var a = new Complex(1.19209289550780998537e-7, 1.19209289550780998537e-7);
            var b = new Complex(1.19209289550780998537e-7, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(0.0, 0.0), a.Root(b), 15);
            a = new Complex(0.0, -1.19209289550780998537e-7);
            b = new Complex(0.0, 0.5);
            AssertHelpers.AlmostEqual(new Complex(0.038550761943650161, 0.019526430428319544), a.Root(b), 15);
            a = new Complex(0.0, 0.5);
            b = new Complex(0.0, -0.5);
            AssertHelpers.AlmostEqual(new Complex(0.007927894711475968, -0.042480480425152213), a.Root(b), 15);
            a = new Complex(0.0, -0.5);
            b = new Complex(0.0, 1.0);
            AssertHelpers.AlmostEqual(new Complex(0.15990905692806806, 0.13282699942462053), a.Root(b), 15);
            a = new Complex(0.0, 2.0);
            b = new Complex(0.0, -2.0);
            AssertHelpers.AlmostEqual(new Complex(0.42882900629436788, 0.15487175246424678), a.Root(b), 15);
            a = new Complex(0.0, -8.388608e6);
            b = new Complex(1.19209289550780998537e-7, 0.0);
            AssertHelpers.AlmostEqual(new Complex(double.PositiveInfinity, double.NegativeInfinity), a.Root(b), 15);
        }

        /// <summary>
        /// The can compute square.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanComputeSquare()
        {
            var complex = new Complex(1.19209289550780998537e-7, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(0, 2.8421709430403888e-14), complex.Square(), 15);
            complex = new Complex(0.0, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(-1.4210854715201944e-14, 0.0), complex.Square(), 15);
            complex = new Complex(0.0, -1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(-1.4210854715201944e-14, 0.0), complex.Square(), 15);
            complex = new Complex(0.0, 0.5);
            AssertHelpers.AlmostEqual(new Complex(-0.25, 0.0), complex.Square(), 15);
            complex = new Complex(0.0, -0.5);
            AssertHelpers.AlmostEqual(new Complex(-0.25, 0.0), complex.Square(), 15);
            complex = new Complex(0.0, -8.388608e6);
            AssertHelpers.AlmostEqual(new Complex(-70368744177664.0, 0.0), complex.Square(), 15);
        }

        /// <summary>
        /// The can compute square root.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanComputeSquareRoot()
        {
            var complex = new Complex(1.19209289550780998537e-7, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(
                new Complex(0.00037933934912842666, 0.00015712750315077684), complex.SquareRoot(), 15);
            complex = new Complex(0.0, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(
                new Complex(0.00024414062499999973, 0.00024414062499999976), complex.SquareRoot(), 15);
            complex = new Complex(0.0, -1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(
                new Complex(0.00024414062499999973, -0.00024414062499999976), complex.SquareRoot(), 15);
            complex = new Complex(0.0, 0.5);
            AssertHelpers.AlmostEqual(new Complex(0.5, 0.5), complex.SquareRoot(), 15);
            complex = new Complex(0.0, -0.5);
            AssertHelpers.AlmostEqual(new Complex(0.5, -0.5), complex.SquareRoot(), 15);
            complex = new Complex(0.0, -8.388608e6);
            AssertHelpers.AlmostEqual(new Complex(2048.0, -2048.0), complex.SquareRoot(), 15);
            complex = new Complex(8.388608e6, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(2896.3093757400989, 2.0579515874459933e-11), complex.SquareRoot(), 15);
        }

        /// <summary>
        /// The can convert double to complex.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanConvertDoubleToComplex()
        {
            AssertEx.That(() => ((Complex)double.NaN).IsNaN);
            AssertEx.That(() => ((Complex)double.NegativeInfinity).IsInfinity);
            Assert.AreEqual(1.1, new Complex(1.1, 0));
        }

        /// <summary>
        /// The can create complex number using the constructor.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanCreateComplexNumberUsingTheConstructor()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(1.1, complex.Real, "Real part is 1.1.");
            Assert.AreEqual(-2.2, complex.Imaginary, "Imaginary part is -2.2.");
        }

        /// <summary>
        /// The can create complex number with modulus argument.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanCreateComplexNumberWithModulusArgument()
        {
            var complex = Complex.WithModulusArgument(2, -Math.PI / 6);
            Assert.AreApproximatelyEqual(Math.Sqrt(3), complex.Real, 1e-15, "Real part is Sqrt(3).");
            Assert.AreApproximatelyEqual(-1, complex.Imaginary, 1e-15, "Imaginary part is -1.");
        }

        /// <summary>
        /// The can create complex number with real imaginary intializer.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanCreateComplexNumberWithRealImaginaryIntializer()
        {
            var complex = Complex.WithRealImaginary(1.1, -2.2);
            Assert.AreEqual(1.1, complex.Real, "Real part is 1.1.");
            Assert.AreEqual(-2.2, complex.Imaginary, "Imaginary part is -2.2.");
        }

        /// <summary>
        /// The can create string from complex number.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanCreateStringFromComplexNumber()
        {
            Assert.AreEqual("NaN", Complex.NaN.ToString());
            Assert.AreEqual("Infinity", Complex.Infinity.ToString());
            Assert.AreEqual("1.1", new Complex(1.1, 0).ToString());
            Assert.AreEqual("-1.1i", new Complex(0, -1.1).ToString());
            Assert.AreEqual("1.1i", new Complex(0, 1.1).ToString());
            Assert.AreEqual("1.1 + 1.1i", new Complex(1.1, 1.1).ToString());
        }

        /// <summary>
        /// The can create string using format provider.
        /// </summary>
        [Test]
        [MultipleAsserts]
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

        /// <summary>
        /// The can create string using number format.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanCreateStringUsingNumberFormat()
        {
            Assert.AreEqual("NaN", Complex.NaN.ToString("#.000"));
            Assert.AreEqual("Infinity", Complex.Infinity.ToString("#.000"));
            Assert.AreEqual("1.100", new Complex(1.1, 0).ToString("#.000"));
            Assert.AreEqual("-1.100i", new Complex(0, -1.1).ToString("#.000"));
            Assert.AreEqual("1.100i", new Complex(0, 1.1).ToString("#.000"));
            Assert.AreEqual("1.100 + 1.100i", new Complex(1.1, 1.1).ToString("#.000"));
        }

        /// <summary>
        /// The can determine if imaginary unit.
        /// </summary>
        [Test]
        public void CanDetermineIfImaginaryUnit()
        {
            var complex = new Complex(0, 1);
            Assert.IsTrue(complex.IsI, "Imaginary unit");
        }

        /// <summary>
        /// The can determine if infinity.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanDetermineIfInfinity()
        {
            var complex = new Complex(double.PositiveInfinity, 1);
            Assert.IsTrue(complex.IsInfinity, "Real part is infinity.");
            complex = new Complex(1, double.NegativeInfinity);
            Assert.IsTrue(complex.IsInfinity, "Imaginary part is infinity.");
            complex = new Complex(double.NegativeInfinity, double.PositiveInfinity);
            Assert.IsTrue(complex.IsInfinity, "Both parts are infinity.");
        }

        /// <summary>
        /// The can determine if na n.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanDetermineIfNaN()
        {
            var complex = new Complex(double.NaN, 1);
            Assert.IsTrue(complex.IsNaN, "Real part is NaN.");
            complex = new Complex(1, double.NaN);
            Assert.IsTrue(complex.IsNaN, "Imaginary part is NaN.");
            complex = new Complex(double.NaN, double.NaN);
            Assert.IsTrue(complex.IsNaN, "Both parts are NaN.");
        }

        /// <summary>
        /// The can determine if one value complex number.
        /// </summary>
        [Test]
        public void CanDetermineIfOneValueComplexNumber()
        {
            var complex = new Complex(1, 0);
            Assert.IsTrue(complex.IsOne, "Complex number with a value of one.");
        }

        /// <summary>
        /// The can determine if real non negative number.
        /// </summary>
        [Test]
        public void CanDetermineIfRealNonNegativeNumber()
        {
            var complex = new Complex(1, 0);
            Assert.IsTrue(complex.IsReal, "Is a real non-negative number.");
        }

        /// <summary>
        /// The can determine if real number.
        /// </summary>
        [Test]
        public void CanDetermineIfRealNumber()
        {
            var complex = new Complex(-1, 0);
            Assert.IsTrue(complex.IsReal, "Is a real number.");
        }

        /// <summary>
        /// The can determine if zero value complex number.
        /// </summary>
        [Test]
        public void CanDetermineIfZeroValueComplexNumber()
        {
            var complex = new Complex(0, 0);
            Assert.IsTrue(complex.IsZero, "Zero complex number.");
        }

        /// <summary>
        /// The can divide complex number and double using operators.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanDivideComplexNumberAndDoubleUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN * 1.0).IsNaN);
            Assert.AreEqual(new Complex(-2, 2), new Complex(4, -4) / -2);
            Assert.AreEqual(new Complex(0.25, 0.25), 2 / new Complex(4, -4));
            Assert.AreEqual(Complex.Infinity, Complex.One / 0);
        }

        /// <summary>
        /// The can divide two complex numbers.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanDivideTwoComplexNumbers()
        {
            AssertEx.That(() => Complex.NaN.Multiply(Complex.One).IsNaN);
            Assert.AreEqual(new Complex(-2, 0), new Complex(4, -4).Divide(new Complex(-2, 2)));
            Assert.AreEqual(Complex.Infinity, Complex.One.Divide(Complex.Zero));
        }

        /// <summary>
        /// The can divide two complex numbers using operators.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanDivideTwoComplexNumbersUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN / Complex.One).IsNaN);
            Assert.AreEqual(new Complex(-2, 0), new Complex(4, -4) / new Complex(-2, 2));
            Assert.AreEqual(Complex.Infinity, Complex.One / Complex.Zero);
        }

        /// <summary>
        /// The can multiple complex number and double using operators.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanMultipleComplexNumberAndDoubleUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN * 1.0).IsNaN);
            Assert.AreEqual(new Complex(8, -8), new Complex(4, -4) * 2);
            Assert.AreEqual(new Complex(8, -8), 2 * new Complex(4, -4));
        }

        /// <summary>
        /// The can multiple two complex numbers.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanMultipleTwoComplexNumbers()
        {
            AssertEx.That(() => Complex.NaN.Multiply(Complex.One).IsNaN);
            Assert.AreEqual(new Complex(0, 16), new Complex(4, -4).Multiply(new Complex(-2, 2)));
        }

        /// <summary>
        /// The can multiple two complex numbers using operators.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanMultipleTwoComplexNumbersUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN * Complex.One).IsNaN);
            Assert.AreEqual(new Complex(0, 16), new Complex(4, -4) * new Complex(-2, 2));
        }

        /// <summary>
        /// The can negate value.
        /// </summary>
        [Test]
        public void CanNegateValue()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(new Complex(-1.1, 2.2), complex.Negate());
        }

        /// <summary>
        /// The can negate value using operator.
        /// </summary>
        [Test]
        public void CanNegateValueUsingOperator()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(new Complex(-1.1, 2.2), -complex);
        }

        /// <summary>
        /// The can subtract complex number and double using operartor.
        /// </summary>
        [Test]
        [MultipleAsserts]
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

        /// <summary>
        /// The can subtract two complex numbers.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoComplexNumbers()
        {
            AssertEx.That(() => Complex.NaN.Subtract(Complex.NaN).IsNaN);
            AssertEx.That(() => Complex.Infinity.Subtract(Complex.One).IsInfinity);
            AssertEx.That(() => Complex.One.Subtract(Complex.Zero) == Complex.One);
            AssertEx.That(() => new Complex(1.1, -2.2).Subtract(new Complex(1.1, -2.2)) == Complex.Zero);
        }

        /// <summary>
        /// The can test for equality.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanTestForEquality()
        {
            Assert.AreNotEqual(Complex.NaN, Complex.NaN);
            Assert.AreEqual(Complex.Infinity, Complex.Infinity);
            Assert.AreEqual(new Complex(1.1, -2.2), new Complex(1.1, -2.2));
            Assert.AreNotEqual(new Complex(-1.1, 2.2), new Complex(1.1, -2.2));
        }

        /// <summary>
        /// The can test for equality using operators.
        /// </summary>
        [Test]
        [MultipleAsserts]
        public void CanTestForEqualityUsingOperators()
        {
            AssertEx.That(() => Complex.NaN != Complex.NaN);
            AssertEx.That(() => Complex.Infinity == Complex.Infinity);
            AssertEx.That(() => new Complex(1.1, -2.2) == new Complex(1.1, -2.2));
            AssertEx.That(() => new Complex(-1.1, 2.2) != new Complex(1.1, -2.2));
        }

        /// <summary>
        /// The can use plus.
        /// </summary>
        [Test]
        public void CanUsePlus()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(complex, complex.Plus());
        }

        /// <summary>
        /// The can use plus operator.
        /// </summary>
        [Test]
        public void CanUsePlusOperator()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(complex, +complex);
        }

        /// <summary>
        /// The with modulus argument throws argument out of range exception.
        /// </summary>
        [Test]
        public void WithModulusArgumentThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => Complex.WithModulusArgument(-1, 1), "Throws exception because modulus is negative.");
        }
    }
}