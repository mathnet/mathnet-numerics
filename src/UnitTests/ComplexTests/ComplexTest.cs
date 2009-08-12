namespace MathNet.Numerics.UnitTests
{
    using System;
    using System.Globalization;

    using MbUnit.Framework;

    [TestFixture]
    public class ComplexTest
    {
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

        [Test]
        [MultipleAsserts]
        public void CanAddSubtractComplexNumbersUsingOperartor()
        {
            AssertEx.That(() => (Complex.NaN - Complex.NaN).IsNaN);
            AssertEx.That(() => (Complex.Infinity - Complex.One).IsInfinity);
            AssertEx.That(() => (Complex.One - Complex.Zero) == Complex.One);
            AssertEx.That(() => (new Complex(1.1, -2.2) - new Complex(1.1, -2.2)) == Complex.Zero);
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoComplexNumbers()
        {
            AssertEx.That(() => Complex.NaN.Add(Complex.NaN).IsNaN);
            AssertEx.That(() => Complex.Infinity.Add(Complex.One).IsInfinity);
            AssertEx.That(() => Complex.One.Add(Complex.Zero) == Complex.One);
            AssertEx.That(() => new Complex(1.1, -2.2).Add(new Complex(-1.1, 2.2)) == Complex.Zero);
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoComplexNumbersUsingOperartor()
        {
            AssertEx.That(() => (Complex.NaN + Complex.NaN).IsNaN);
            AssertEx.That(() => (Complex.Infinity + Complex.One).IsInfinity);
            AssertEx.That(() => (Complex.One + Complex.Zero) == Complex.One);
            AssertEx.That(() => (new Complex(1.1, -2.2) + new Complex(-1.1, 2.2)) == Complex.Zero);
        }

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
            a = new Complex(0.0, 0.0);
            b = new Complex(0.0, 0.0);
            AssertHelpers.AlmostEqual(new Complex(1.0, 0.0), a.Power(b), 15);
            a = new Complex(0.0, 0.0);
            b = new Complex(1.0, 0.0);
            AssertHelpers.AlmostEqual(new Complex(0.0, 0.0), a.Power(b), 15);
            a = new Complex(0.0, 0.0);
            b = new Complex(-1.0, 0.0);
            AssertHelpers.AlmostEqual(new Complex(double.PositiveInfinity, 0.0), a.Power(b), 15);
            a = new Complex(0.0, 0.0);
            b = new Complex(-1.0, 1.0);
            AssertHelpers.AlmostEqual(new Complex(double.PositiveInfinity, double.PositiveInfinity), a.Power(b), 15);
            a = new Complex(0.0, 0.0);
            b = new Complex(0.0, 1.0);
            AssertEx.That(()=>a.Power(b).IsNaN);
           	
        }

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
            complex = new Complex(0.0, 0.0);
            AssertHelpers.AlmostEqual(Complex.Zero, complex.SquareRoot(), 15);
        }

        [Test]
        [Row(1, -2, "1 -2i")]
        [Row(1, 2, "1 + 2i")]
        [Row(1, 0, "1")]
        [Row(0, -2, "-2i")]
        [Row(0, 2, "2i")]
        public void CanConvertComplexToString(double real, double imag, string expected)
        {
            var a = new Complex(real, imag);
            Assert.AreEqual(expected, a.ToString());
        }

        [Test]
        [MultipleAsserts]
        public void CanConvertDoubleToComplex()
        {
            AssertEx.That(() => ((Complex)double.NaN).IsNaN);
            AssertEx.That(() => ((Complex)double.NegativeInfinity).IsInfinity);
            Assert.AreEqual(1.1, new Complex(1.1, 0));
        }

        [Test]
        [Row("-1", -1, 0)]
        [Row("-i", 0, -1)]
        [Row("i", 0, 1)]
        [Row("2i", 0, 2)]
        [Row("1 + 2i", 1, 2)]
        [Row("1+2i", 1, 2)]
        [Row("1 - 2i", 1, -2)]
        [Row("1-2i", 1, -2)]
        [Row("1,2", 1, 2)]
        [Row("1 , 2", 1, 2)]
        [Row("1,2i", 1, 2)]
        [Row("-1, -2i", -1, -2)]
        [Row("(+1,2i)", 1, 2)]
        [Row("(-1 , -2)", -1, -2)]
        [Row("(-1 , -2i)", -1, -2)]
        [Row("(+1e1 , -2e-2i)", 10, -0.02)]
        [Row("(-1E1 -2e2i)", -10, -200)]
        [Row("(-1e+1 -2e2i)", -10, -200)]
        [Row("(-1e1 -2e+2i)", -10, -200)]
        [Row("(-1e-1  -2E2i)", -0.1, -200)]
        [Row("(-1e1  -2e-2i)", -10, -0.02)]
        [Row("(-1E+1 -2e+2i)", -10, -200)]
        [Row("(-1e-1,-2e-2i)", -0.1, -0.02)]
        [Row("(+1 +2i)", 1, 2)]
        public void CanConvertStringToComplexUsingTryParse(string str, double expectedReal, double expectedImag)
        {
            Complex z;
            var ret = Complex.TryParse(str, out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(expectedReal, z.Real);
            Assert.AreEqual(expectedImag, z.Imaginary);

            ret = Complex.TryParse("(-1E+1 -2e+2i)", out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(-10, z.Real);
            Assert.AreEqual(-200, z.Imaginary);

            ret = Complex.TryParse("(-1e-1,-2e-2i)", out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(-.1, z.Real);
            Assert.AreEqual(-.02, z.Imaginary);

            ret = Complex.TryParse("(+1 +2i)", out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(1, z.Real);
            Assert.AreEqual(2, z.Imaginary);
        }

        [Test]
        public void CanParseStringToComplex()
        {
            var actual = Complex.Parse("-1 -2i");
            Assert.AreEqual(new Complex(-1,-2), actual);
        }

        [Test]
        public void ParseThrowsFormatExceptionIfMissingClosingParen()
        {
            Assert.Throws<FormatException>(() => Complex.Parse("(1,2"));
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateComplexNumberUsingTheConstructor()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(1.1, complex.Real, "Real part is 1.1.");
            Assert.AreEqual(-2.2, complex.Imaginary, "Imaginary part is -2.2.");
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateComplexNumberWithModulusArgument()
        {
            var complex = Complex.WithModulusArgument(2, -Math.PI / 6);
            Assert.AreApproximatelyEqual(Math.Sqrt(3), complex.Real, 1e-15, "Real part is Sqrt(3).");
            Assert.AreApproximatelyEqual(-1, complex.Imaginary, 1e-15, "Imaginary part is -1.");
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateComplexNumberWithRealImaginaryIntializer()
        {
            var complex = Complex.WithRealImaginary(1.1, -2.2);
            Assert.AreEqual(1.1, complex.Real, "Real part is 1.1.");
            Assert.AreEqual(-2.2, complex.Imaginary, "Imaginary part is -2.2.");
        }

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

        [Test]
        public void CanDetermineIfImaginaryUnit()
        {
            var complex = new Complex(0, 1);
            Assert.IsTrue(complex.IsI, "Imaginary unit");
        }

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

        [Test]
        public void CanDetermineIfOneValueComplexNumber()
        {
            var complex = new Complex(1, 0);
            Assert.IsTrue(complex.IsOne, "Complex number with a value of one.");
        }

        [Test]
        public void CanDetermineIfRealNonNegativeNumber()
        {
            var complex = new Complex(1, 0);
            Assert.IsTrue(complex.IsReal, "Is a real non-negative number.");
        }

        [Test]
        public void CanDetermineIfRealNumber()
        {
            var complex = new Complex(-1, 0);
            Assert.IsTrue(complex.IsReal, "Is a real number.");
        }

        [Test]
        public void CanDetermineIfZeroValueComplexNumber()
        {
            var complex = new Complex(0, 0);
            Assert.IsTrue(complex.IsZero, "Zero complex number.");
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideComplexNumberAndDoubleUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN * 1.0).IsNaN);
            Assert.AreEqual(new Complex(-2, 2), new Complex(4, -4) / -2);
            Assert.AreEqual(new Complex(0.25, 0.25), 2 / new Complex(4, -4));
            Assert.AreEqual(Complex.Infinity, 2.0 / Complex.Zero);
            Assert.AreEqual(Complex.Infinity, Complex.One / 0);
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideTwoComplexNumbers()
        {
            AssertEx.That(() => Complex.NaN.Multiply(Complex.One).IsNaN);
            Assert.AreEqual(new Complex(-2, 0), new Complex(4, -4).Divide(new Complex(-2, 2)));
            Assert.AreEqual(Complex.Infinity, Complex.One.Divide(Complex.Zero));
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideTwoComplexNumbersUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN / Complex.One).IsNaN);
            Assert.AreEqual(new Complex(-2, 0), new Complex(4, -4) / new Complex(-2, 2));
            Assert.AreEqual(Complex.Infinity, Complex.One / Complex.Zero);
        }

        [Test]
        [MultipleAsserts]
        public void CanMultipleComplexNumberAndDoubleUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN * 1.0).IsNaN);
            Assert.AreEqual(new Complex(8, -8), new Complex(4, -4) * 2);
            Assert.AreEqual(new Complex(8, -8), 2 * new Complex(4, -4));
        }

        [Test]
        [MultipleAsserts]
        public void CanMultipleTwoComplexNumbers()
        {
            AssertEx.That(() => Complex.NaN.Multiply(Complex.One).IsNaN);
            Assert.AreEqual(new Complex(0, 16), new Complex(4, -4).Multiply(new Complex(-2, 2)));
        }

        [Test]
        [MultipleAsserts]
        public void CanMultipleTwoComplexNumbersUsingOperators()
        {
            AssertEx.That(() => (Complex.NaN * Complex.One).IsNaN);
            Assert.AreEqual(new Complex(0, 16), new Complex(4, -4) * new Complex(-2, 2));
        }

        [Test]
        public void CanNegateValue()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(new Complex(-1.1, 2.2), complex.Negate());
        }

        [Test]
        public void CanNegateValueUsingOperator()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(new Complex(-1.1, 2.2), -complex);
        }

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

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoComplexNumbers()
        {
            AssertEx.That(() => Complex.NaN.Subtract(Complex.NaN).IsNaN);
            AssertEx.That(() => Complex.Infinity.Subtract(Complex.One).IsInfinity);
            AssertEx.That(() => Complex.One.Subtract(Complex.Zero) == Complex.One);
            AssertEx.That(() => new Complex(1.1, -2.2).Subtract(new Complex(1.1, -2.2)) == Complex.Zero);
        }

        [Test]
        [MultipleAsserts]
        public void CanTestForEquality()
        {
            Assert.AreNotEqual(Complex.NaN, Complex.NaN);
            Assert.AreEqual(Complex.Infinity, Complex.Infinity);
            Assert.AreEqual(new Complex(1.1, -2.2), new Complex(1.1, -2.2));
            Assert.AreNotEqual(new Complex(-1.1, 2.2), new Complex(1.1, -2.2));
        }

        [Test]
        [MultipleAsserts]
        public void CanTestForEqualityUsingOperators()
        {
            AssertEx.That(() => Complex.NaN != Complex.NaN);
            AssertEx.That(() => Complex.Infinity == Complex.Infinity);
            AssertEx.That(() => new Complex(1.1, -2.2) == new Complex(1.1, -2.2));
            AssertEx.That(() => new Complex(-1.1, 2.2) != new Complex(1.1, -2.2));
        }

        [Test]
        public void CanUsePlus()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(complex, complex.Plus());
        }

        [Test]
        public void CanUsePlusOperator()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(complex, +complex);
        }

        [Test]
        public void TryParseCanHandleSymbols()
        {
            Complex z;
            var ni = new NumberFormatInfo();
            var ret = Complex.TryParse(ni.NegativeInfinitySymbol + "," + ni.PositiveInfinitySymbol, out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(double.NegativeInfinity, z.Real);
            Assert.AreEqual(double.PositiveInfinity, z.Imaginary);

            ret = Complex.TryParse(ni.NaNSymbol + "," + ni.NaNSymbol, out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(double.NaN, z.Real);
            Assert.AreEqual(double.NaN, z.Imaginary);

            ret = Complex.TryParse(ni.NegativeInfinitySymbol + "+" + ni.PositiveInfinitySymbol + "i", out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(double.NegativeInfinity, z.Real);
            Assert.AreEqual(double.PositiveInfinity, z.Imaginary);

            ret = Complex.TryParse(ni.NaNSymbol + "+" + ni.NaNSymbol + "i", out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(double.NaN, z.Real);
            Assert.AreEqual(double.NaN, z.Imaginary);

            ret = Complex.TryParse(double.MaxValue.ToString("R") + " " + double.MinValue.ToString("R") + "i", out z);
            Assert.IsTrue(ret);
            Assert.AreEqual(double.MaxValue, z.Real);
            Assert.AreEqual(double.MinValue, z.Imaginary);
        }

        [Test]
        [Row("")]
        [Row("+")]
        [Row("1i+2")]
        [Row(null)]
        public void TryParseReturnsFalseWhenGiveBadValue(string str)
        {
            Complex z;
            var ret = Complex.TryParse(str, out z);
            Assert.IsFalse(ret);
            Assert.AreEqual(0, z.Real);
            Assert.AreEqual(0, z.Imaginary);
        }

        [Test]
        public void WithModulusArgumentThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => Complex.WithModulusArgument(-1, 1), "Throws exception because modulus is negative.");
        }

        [Test]
        [Row(0.0, 0.0, 0.0)]
        [Row(0.0, 1.0, 1.0)]
        [Row(-1.0, 1.0, 1.4142135623730951)]
        [Row(-111.1, 111.1, 157.11912677965086)]
        public void CanComputeModulus(double real, double imag, double expected)
        {
            Assert.AreEqual(expected, new Complex(real, imag).Modulus);
        }

        [Test]
        [Row(double.PositiveInfinity, double.PositiveInfinity, Constants.Sqrt1Over2, Constants.Sqrt1Over2)]
        [Row(double.PositiveInfinity, double.NegativeInfinity, Constants.Sqrt1Over2, -Constants.Sqrt1Over2)]
        [Row(double.NegativeInfinity, double.PositiveInfinity, -Constants.Sqrt1Over2, -Constants.Sqrt1Over2)]
        [Row(double.NegativeInfinity, double.NegativeInfinity, -Constants.Sqrt1Over2, Constants.Sqrt1Over2)]
        [Row(0.0, 0.0, 0.0, 0.0)]
        [Row(-1.0, 1.0, -0.70710678118654746, 0.70710678118654746)]
        [Row(-111.1, 111.1, -0.70710678118654746, 0.70710678118654746)]
        public void CanComputeSign(double real, double imag, double expectedReal, double expectedImag)
        {
            Assert.AreEqual(new Complex(expectedReal, expectedImag), new Complex(real, imag).Sign);
        }
    }
}