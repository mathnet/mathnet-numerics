// <copyright file="ComplexTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.UnitTests.ComplexTests
{
    using System;
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
        [Row(0.0, 0.0, 1.0, 0.0)]
        [Row(0.0, 1.0, 0.54030230586813977, 0.8414709848078965)]
        [Row(-1.0, 1.0, 0.19876611034641295, 0.30955987565311222)]
        [Row(-111.1, 111.1, -2.3259065941590448e-49, -5.1181940185795617e-49)]
        public void CanComputeExp(double real, double imag, double expectedReal, double expectedImag)
        {
            var value = new Complex(real, imag);
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, Complex.Exp(value), 15);
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
        [Row(0.0, 0.0, double.NegativeInfinity, 0.0)]
        [Row(0.0, 1.0, 0.0, 1.5707963267948966)]
        [Row(-1.0, 1.0, 0.34657359027997264, 2.3561944901923448)]
        [Row(-111.1, 111.1, 5.0570042869255571, 2.3561944901923448)]
        [Row(111.1, -111.1, 5.0570042869255571, -0.78539816339744828)]
        public void CanComputeLog(double real, double imag, double expectedReal, double expectedImag)
        {
            var value = new Complex(real, imag);
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, Complex.Log(value), 15);
        }

        [Test]
        [Row(0.0, 0.0, double.NegativeInfinity, 0.0)]
        [Row(0.0, 1.0, 0.0, 0.68218817692092071)]
        [Row(-1.0, 1.0, 0.1505149978319906, 1.0232822653813811)]
        [Row(-111.1, 111.1, 2.1962290567728582, 1.0232822653813811)]
        [Row(111.1, -111.1, 2.1962290567728582, -0.34109408846046035)]
        public void CanComputeLog10(double real, double imag, double expectedReal, double expectedImag)
        {
            var value = new Complex(real, imag);
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, Complex.Log10(value), 15);
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
            AssertEx.That(() => a.Power(b).IsNaN);
        }

        [Test]
        [MultipleAsserts]
        public void CanComputePow()
        {
            var a = new Complex(1.19209289550780998537e-7, 1.19209289550780998537e-7);
            var b = new Complex(1.19209289550780998537e-7, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(9.99998047207974718744e-1, -1.76553541154378695012e-6), Complex.Pow(a,b), 15);
            a = new Complex(0.0, -8.388608e6);
            AssertHelpers.AlmostEqual(new Complex(1.00000190048219620166, -1.87253870018168043834e-7), Complex.Pow(a, 1.19209289550780998537e-7), 15);
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
        [MultipleAsserts]
        public void CanComputeSqrt()
        {
            var complex = new Complex(1.19209289550780998537e-7, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(0.00037933934912842666, 0.00015712750315077684), Complex.Sqrt(complex), 15);
            complex = new Complex(0.0, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(0.00024414062499999973, 0.00024414062499999976), Complex.Sqrt(complex), 15);
            complex = new Complex(0.0, -1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(0.00024414062499999973, -0.00024414062499999976), Complex.Sqrt(complex), 15);
            complex = new Complex(0.0, 0.5);
            AssertHelpers.AlmostEqual(new Complex(0.5, 0.5), Complex.Sqrt(complex), 15);
            complex = new Complex(0.0, -0.5);
            AssertHelpers.AlmostEqual(new Complex(0.5, -0.5), Complex.Sqrt(complex), 15);
            complex = new Complex(0.0, -8.388608e6);
            AssertHelpers.AlmostEqual(new Complex(2048.0, -2048.0), Complex.Sqrt(complex), 15);
            complex = new Complex(8.388608e6, 1.19209289550780998537e-7);
            AssertHelpers.AlmostEqual(new Complex(2896.3093757400989, 2.0579515874459933e-11), Complex.Sqrt(complex), 15);
            complex = new Complex(0.0, 0.0);
            AssertHelpers.AlmostEqual(Complex.Zero, Complex.Sqrt(complex), 15);
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
        public void CanCreateComplexNumberFromPolarCoordinates()
        {
            var complex = Complex.FromPolarCoordinates(2, -Math.PI / 6);
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
        public void CanDetermineIfImaginaryUnit()
        {
            var complex = new Complex(0, 1);
            Assert.IsTrue(complex.IsImaginaryOne, "Imaginary unit");
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
        public void CanComputeMagnitude(double real, double imag, double expected)
        {
            Assert.AreEqual(expected, new Complex(real, imag).Magnitude);
        }

        [Test]
        [Row(0.0, 0.0, 0.0)]
        [Row(0.0, 1.0, 1.0)]
        [Row(-1.0, 1.0, 1.4142135623730951)]
        [Row(-111.1, 111.1, 157.11912677965086)]
        public void CanComputeAbs(double real, double imag, double expected)
        {
            Assert.AreEqual(expected, Complex.Abs(new Complex(real, imag)));
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

        [Test]
        [Row(0.0, 0.0, 1.0, 0.0)]
        [Row(8.388608e6, 0.0, -0.90175467375875928, 0.0)]
        [Row(-8.388608e6, 0.0, -0.90175467375875928, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 0.99999999999999289, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, 0.99999999999999289, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, -0.90175467375876572, -5.1528001100635277e-8)]
        [Row(-1.19209289550780998537e-7, -8.388608e6, double.PositiveInfinity, double.NegativeInfinity)]
        public void CanComputeCos(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = Complex.Cos(new Complex(real, imag));
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        [Test]
        [Row(0.0, 0.0, 0.0, 0.0)]
        [Row(8.388608e6, 0.0, 0.43224820225679778, 0.0)]
        [Row(-8.388608e6, 0.0, -0.43224820225679778, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.19209289550780998537e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.19209289550780998537e-7, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 0.43224820225680083, -1.0749753400787824e-7)]
        [Row(-1.19209289550780998537e-7, -8.388608e6, double.NegativeInfinity, double.NegativeInfinity)]
        public void CanComputeSin(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = Complex.Sin(new Complex(real, imag));
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        [Test]
        [Row(0.0, 0.0, 0.0, 0.0)]
        [Row(8.388608e6, 0.0, -0.47934123862654288, 0.0)]
        [Row(-8.388608e6, 0.0, 0.47934123862654288, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078157e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078157e-7, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, -0.47934123862653449, 1.4659977233982276e-7)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, 0.47934123862653449, -1.4659977233982276e-7)]
        public void CanComputeTan(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = Complex.Tan(new Complex(real, imag));
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        [Test]
        [Row(0.0, 0.0, 0.0, 0.0)]
        [Row(8.388608e6, 0.0, double.PositiveInfinity, 0.0)]
        [Row(-8.388608e6, 0.0, double.NegativeInfinity, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078128e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078128e-7, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, double.PositiveInfinity, double.PositiveInfinity)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, double.NegativeInfinity, double.NegativeInfinity)]
        [Row(0.5, -0.5, 0.45730415318424922, -0.54061268571315335)]
        public void CanComputeSinh(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = Complex.Sinh(new Complex(real, imag));
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, 1.0, 0.0)]
        [Row(8.388608e6, 0.0, double.PositiveInfinity, 0.0)]
        [Row(-8.388608e6, 0.0, double.PositiveInfinity, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.0000000000000071, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, 1.0000000000000071, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, double.PositiveInfinity, double.PositiveInfinity)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, double.PositiveInfinity, double.PositiveInfinity)]
        [Row(0.5, -0.5, 0.9895848833999199, -0.24982639750046154)]
        public void CanComputeCosh(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = Complex.Cosh(new Complex(real, imag));
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, 0.0, 0.0)]
        [Row(8.388608e6, 0.0, 1.0, 0.0)]
        [Row(-8.388608e6, 0.0, -1.0, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078043e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078043e-7, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.0, 0.0)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.0, 0.0)]
        [Row(0.5, -0.5, 0.56408314126749848, -0.40389645531602575)]
        public void CanComputeTanh(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = Complex.Tanh(new Complex(real, imag));
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }


        [Test]
        [Row(0.0, 0.0, 0.0, 0.0)]
        [Row(8.388608e6, 0.0, 1.5707963267948966, -16.635532333438682)]
        [Row(-8.388608e6, 0.0, -1.5707963267948966, 16.635532333438682)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078128e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078128e-7, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.5707963267948966, 16.635532333438682)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.5707963267948966, -16.635532333438682)]
        [Row(0.5, -0.5, 0.4522784471511907, -0.53063753095251787)]
        public void CanComputeAsin(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = Complex.Asin(new Complex(real, imag));
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, 1.5707963267948966, 0.0)]
        [Row(8.388608e6, 0.0, 0.0, 16.635532333438682)]
        [Row(-8.388608e6, 0.0, 3.1415926535897931, -16.635532333438682)]
        [Row(1.19209289550780998537e-7, 0.0, 1.570796207585607, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, 1.5707964460041861, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.4210854715202073e-14, -16.635532333438682)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, 3.1415926535897789, 16.63553233343868)]
        [Row(0.5, -0.5, 1.1185178796437059, 0.53063753095251787)]
        public void CanComputeAcos(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = Complex.Acos(new Complex(real, imag));
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, 0.0, 0.0)]
        [Row(8.388608e6, 0.0, 1.570796207585607, 0.0)]
        [Row(-8.388608e6, 0.0, -1.570796207585607, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078043e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078043e-7, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.570796207585607, 0.0)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.570796207585607, 0.0)]
        [Row(0.5, -0.5, 0.5535743588970452, -0.40235947810852507)]
        public void CanComputeAtan(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = Complex.Atan(new Complex(real, imag));
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, 0.0, 0.0)]
        [Row(0, 8.388608e6, 0.0, -1.1920928955078125e-7)]
        [Row(-8.388608e6, 8.388608e6, -5.9604644775390625e-8, -5.9604644775390625e-8)]
        public void CanComputeReciprocal(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = Complex.Reciprocal(new Complex(real, imag));
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        [Test]
        public void CanConvertDecimalToComplex()
        {
            var orginal = new decimal(1.234567890);
            var complex = (Complex)orginal;
            Assert.AreEqual(1.234567890, complex.Real);
            Assert.AreEqual(0.0, complex.Imaginary);
        }

        [Test]
        public void CanConvertByteToComplex()
        {
            const byte orginal = 123;
            var complex = (Complex)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0, complex.Imaginary);
        }

        [Test]
        public void CanConvertShortToComplex()
        {
            const short orginal = 123;
            var complex = (Complex)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0, complex.Imaginary);
        }

        [Test]
        public void CanConvertIntToComplex()
        {
            const int orginal = 123;
            var complex = (Complex)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0, complex.Imaginary);
        }

        [Test]
        public void CanConvertLongToComplex()
        {
            const long orginal = 123;
            var complex = (Complex)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0, complex.Imaginary);
        }

        [Test]
        public void CanConvertUIntToComplex()
        {
            const uint orginal = 123;
            var complex = (Complex)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0, complex.Imaginary);
        }

        [Test]
        public void CanConvertULongToComplex()
        {
            const ulong orginal = 123;
            var complex = (Complex)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0, complex.Imaginary);
        }

        [Test]
        public void CanConvertFloatToComplex()
        {
            const float orginal = 123.456789f;
            var complex = (Complex)orginal;
            Assert.AreEqual(123.456789f, (float)complex.Real);
            Assert.AreEqual(0.0, complex.Imaginary);
        }

        [Test]
        public void CanConvertComplex32ToComplex()
        {
            var complex32 = new Complex32(123.456f, -78.9f);
            var complex = (Complex)complex32;
            Assert.AreEqual(123.456f, (float)complex.Real);
            Assert.AreEqual(-78.9f, (float)complex.Imaginary);
        }
   }
}