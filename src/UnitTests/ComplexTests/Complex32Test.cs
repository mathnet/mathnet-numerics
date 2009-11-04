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
    public class Complex32Test
    {
        [Test]
        [MultipleAsserts]
        public void CanAddComplexNumberAndDoubleUsingOperartor()
        {
            AssertEx.That(() => (Complex32.NaN + float.NaN).IsNaN);
            AssertEx.That(() => (float.NaN + Complex32.NaN).IsNaN);
            AssertEx.That(() => (float.PositiveInfinity + Complex32.One).IsInfinity);
            AssertEx.That(() => (Complex32.Infinity + 1.0f).IsInfinity);
            AssertEx.That(() => (Complex32.One + 0.0f) == Complex32.One);
            AssertEx.That(() => (0.0f + Complex32.One) == Complex32.One);
            AssertEx.That(() => (new Complex32(1.1f, -2.2f) + 1.1f == new Complex32(2.2f, -2.2f)));
            AssertEx.That(() => -2.2f + new Complex32(-1.1f, 2.2f) == new Complex32(-3.3f, 2.2f));
        }

        [Test]
        [MultipleAsserts]
        public void CanAddSubtractComplexNumbersUsingOperartor()
        {
            AssertEx.That(() => (Complex32.NaN - Complex32.NaN).IsNaN);
            AssertEx.That(() => (Complex32.Infinity - Complex32.One).IsInfinity);
            AssertEx.That(() => (Complex32.One - Complex32.Zero) == Complex32.One);
            AssertEx.That(() => (new Complex32(1.1f, -2.2f) - new Complex32(1.1f, -2.2f)) == Complex32.Zero);
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoComplexNumbers()
        {
            AssertEx.That(() => Complex32.NaN.Add(Complex32.NaN).IsNaN);
            AssertEx.That(() => Complex32.Infinity.Add(Complex32.One).IsInfinity);
            AssertEx.That(() => Complex32.One.Add(Complex32.Zero) == Complex32.One);
            AssertEx.That(() => new Complex32(1.1f, -2.2f).Add(new Complex32(-1.1f, 2.2f)) == Complex32.Zero);
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoComplexNumbersUsingOperartor()
        {
            AssertEx.That(() => (Complex32.NaN + Complex32.NaN).IsNaN);
            AssertEx.That(() => (Complex32.Infinity + Complex32.One).IsInfinity);
            AssertEx.That(() => (Complex32.One + Complex32.Zero) == Complex32.One);
            AssertEx.That(() => (new Complex32(1.1f, -2.2f) + new Complex32(-1.1f, 2.2f)) == Complex32.Zero);
        }

        [Test]
        [MultipleAsserts]
        public void CanCalculateHashCode()
        {
            var complex = new Complex32(1, 0);
            Assert.AreEqual(1065353216, complex.GetHashCode());
            complex = new Complex32(0, 1);
            Assert.AreEqual(-1065353216, complex.GetHashCode());
            complex = new Complex32(1, 1);
            Assert.AreEqual(-16777216, complex.GetHashCode());
        }

        [Test]
        [Row(0.0f, 0.0f, 1.0f, 0.0f)]
        [Row(0.0f, 1.0f, 0.54030230586813977, 0.8414709848078965)]
        [Row(-1.0f, 1.0f, 0.19876611034641295, 0.30955987565311222)]
        [Row(-111.1, 111.1, -2.3259065941590448e-49, -5.1181940185795617e-49)]
        public void CanComputeExponential(float real, float imag, float expectedReal, float expectedImag)
        {
            var value = new Complex32(real, imag);
            var expected = new Complex32(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, value.Exponential(), 7);
        }


        [Test]
        [Row(0.0f, 0.0f, float.NegativeInfinity, 0.0f)]
        [Row(0.0f, 1.0f, 0.0f, 1.5707963267948966)]
        [Row(-1.0f, 1.0f, 0.34657359027997264, 2.3561944901923448)]
        [Row(-111.1, 111.1, 5.0570042869255571, 2.3561944901923448)]
        [Row(111.1, -111.1, 5.0570042869255571, -0.78539816339744828)]
        public void CanComputeNaturalLogarithm(float real, float imag, float expectedReal, float expectedImag)
        {
            var value = new Complex32(real, imag);
            var expected = new Complex32(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, value.NaturalLogarithm(), 7);
        }

        [Test]
        [MultipleAsserts]
        public void CanComputePower()
        {
            var a = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            var b = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqual(
                new Complex32(9.99998047207974718744e-1f, -1.76553541154378695012e-6f), a.Power(b), 7);
            a = new Complex32(0.0f, 1.19209289550780998537e-7f);
            b = new Complex32(0.0f, -1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqual(new Complex32(1.00000018725172576491f, 1.90048076369011843105e-6f), a.Power(b), 7);
            a = new Complex32(0.0f, -1.19209289550780998537e-7f);
            b = new Complex32(0.0f, 0.5f);
            AssertHelpers.AlmostEqual(new Complex32(-2.56488189382693049636e-1f, -2.17823120666116144959f), a.Power(b), 6);
            a = new Complex32(0.0f, 0.5f);
            b = new Complex32(0.0f, -0.5f);
            AssertHelpers.AlmostEqual(new Complex32(2.06287223508090495171f, 7.45007062179724087859e-1f), a.Power(b), 7);
            a = new Complex32(0.0f, -0.5f);
            b = new Complex32(0.0f, 1.0f);
            AssertHelpers.AlmostEqual(new Complex32(3.70040633557002510874f, -3.07370876701949232239f), a.Power(b), 7);
            a = new Complex32(0.0f, 2.0f);
            b = new Complex32(0.0f, -2.0f);
            AssertHelpers.AlmostEqual(new Complex32(4.24532146387429353891f, -2.27479427903521192648e1f), a.Power(b), 7);
            a = new Complex32(0.0f, -8.388608e6f);
            b = new Complex32(1.19209289550780998537e-7f, 0.0f);
            AssertHelpers.AlmostEqual(new Complex32(1.00000190048219620166f, -1.87253870018168043834e-7f), a.Power(b), 7);
            a = new Complex32(0.0f, 0.0f);
            b = new Complex32(0.0f, 0.0f);
            AssertHelpers.AlmostEqual(new Complex32(1.0f, 0.0f), a.Power(b), 7);
            a = new Complex32(0.0f, 0.0f);
            b = new Complex32(1.0f, 0.0f);
            AssertHelpers.AlmostEqual(new Complex32(0.0f, 0.0f), a.Power(b), 7);
            a = new Complex32(0.0f, 0.0f);
            b = new Complex32(-1.0f, 0.0f);
            AssertHelpers.AlmostEqual(new Complex32(float.PositiveInfinity, 0.0f), a.Power(b), 7);
            a = new Complex32(0.0f, 0.0f);
            b = new Complex32(-1.0f, 1.0f);
            AssertHelpers.AlmostEqual(new Complex32(float.PositiveInfinity, float.PositiveInfinity), a.Power(b), 7);
            a = new Complex32(0.0f, 0.0f);
            b = new Complex32(0.0f, 1.0f);
            AssertEx.That(() => a.Power(b).IsNaN);
        }

        [Test]
        [MultipleAsserts]
        public void CanComputeRoot()
        {
            var a = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            var b = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqual(new Complex32(0.0f, 0.0f), a.Root(b), 7);
            a = new Complex32(0.0f, -1.19209289550780998537e-7f);
            b = new Complex32(0.0f, 0.5f);
            AssertHelpers.AlmostEqual(new Complex32(0.038550761943650161f, 0.019526430428319544f), a.Root(b), 6);
            a = new Complex32(0.0f, 0.5f);
            b = new Complex32(0.0f, -0.5f);
            AssertHelpers.AlmostEqual(new Complex32(0.007927894711475968f, -0.042480480425152213f), a.Root(b), 6);
            a = new Complex32(0.0f, -0.5f);
            b = new Complex32(0.0f, 1.0f);
            AssertHelpers.AlmostEqual(new Complex32(0.15990905692806806f, 0.13282699942462053f), a.Root(b), 7);
            a = new Complex32(0.0f, 2.0f);
            b = new Complex32(0.0f, -2.0f);
            AssertHelpers.AlmostEqual(new Complex32(0.42882900629436788f, 0.15487175246424678f), a.Root(b), 7);
            a = new Complex32(0.0f, -8.388608e6f);
            b = new Complex32(1.19209289550780998537e-7f, 0.0f);
            AssertHelpers.AlmostEqual(new Complex32(float.PositiveInfinity, float.NegativeInfinity), a.Root(b), 7);
        }

        [Test]
        [MultipleAsserts]
        public void CanComputeSquare()
        {
            var complex = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqual(new Complex32(0, 2.8421709430403888e-14f), complex.Square(), 7);
            complex = new Complex32(0.0f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqual(new Complex32(-1.4210854715201944e-14f, 0.0f), complex.Square(), 7);
            complex = new Complex32(0.0f, -1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqual(new Complex32(-1.4210854715201944e-14f, 0.0f), complex.Square(), 7);
            complex = new Complex32(0.0f, 0.5f);
            AssertHelpers.AlmostEqual(new Complex32(-0.25f, 0.0f), complex.Square(), 7);
            complex = new Complex32(0.0f, -0.5f);
            AssertHelpers.AlmostEqual(new Complex32(-0.25f, 0.0f), complex.Square(), 7);
            complex = new Complex32(0.0f, -8.388608e6f);
            AssertHelpers.AlmostEqual(new Complex32(-70368744177664.0f, 0.0f), complex.Square(), 7);
        }

        [Test]
        [MultipleAsserts]
        public void CanComputeSquareRoot()
        {
            var complex = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqual(
                new Complex32(0.00037933934912842666f, 0.00015712750315077684f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqual(
                new Complex32(0.00024414062499999973f, 0.00024414062499999976f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, -1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqual(
                new Complex32(0.00024414062499999973f, -0.00024414062499999976f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, 0.5f);
            AssertHelpers.AlmostEqual(new Complex32(0.5f, 0.5f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, -0.5f);
            AssertHelpers.AlmostEqual(new Complex32(0.5f, -0.5f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, -8.388608e6f);
            AssertHelpers.AlmostEqual(new Complex32(2048.0f, -2048.0f), complex.SquareRoot(), 7);
            complex = new Complex32(8.388608e6f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqual(new Complex32(2896.3093757400989f, 2.0579515874459933e-11f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, 0.0f);
            AssertHelpers.AlmostEqual(Complex32.Zero, complex.SquareRoot(), 7);
        }

        [Test]
        [MultipleAsserts]
        public void CanConvertDoubleToComplex()
        {
            AssertEx.That(() => ((Complex32)float.NaN).IsNaN);
            AssertEx.That(() => ((Complex32)float.NegativeInfinity).IsInfinity);
            Assert.AreEqual(1.1f, new Complex32(1.1f, 0));
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateComplexNumberUsingTheConstructor()
        {
            var complex = new Complex32(1.1f, -2.2f);
            Assert.AreEqual(1.1f, complex.Real, "Real part is 1.1f.");
            Assert.AreEqual(-2.2f, complex.Imaginary, "Imaginary part is -2.2f.");
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateComplexNumberWithModulusArgument()
        {
            var complex = Complex32.WithModulusArgument(2, (float)-Math.PI / 6);
            Assert.AreApproximatelyEqual((float)Math.Sqrt(3), complex.Real, 1e-7f, "Real part is Sqrt(3).");
            Assert.AreApproximatelyEqual(-1.0f, complex.Imaginary, 1e-7f, "Imaginary part is -1.");
        }

 
        [Test]
        [MultipleAsserts]
        public void CanCreateComplexNumberWithRealImaginaryIntializer()
        {
            var complex = Complex32.WithRealImaginary(1.1f, -2.2f);
            Assert.AreEqual(1.1f, complex.Real, "Real part is 1.1f.");
            Assert.AreEqual(-2.2f, complex.Imaginary, "Imaginary part is -2.2f.");
        }

        [Test]
        public void CanDetermineIfImaginaryUnit()
        {
            var complex = new Complex32(0, 1);
            Assert.IsTrue(complex.IsImaginaryOne, "Imaginary unit");
        }

        [Test]
        [MultipleAsserts]
        public void CanDetermineIfInfinity()
        {
            var complex = new Complex32(float.PositiveInfinity, 1);
            Assert.IsTrue(complex.IsInfinity, "Real part is infinity.");
            complex = new Complex32(1, float.NegativeInfinity);
            Assert.IsTrue(complex.IsInfinity, "Imaginary part is infinity.");
            complex = new Complex32(float.NegativeInfinity, float.PositiveInfinity);
            Assert.IsTrue(complex.IsInfinity, "Both parts are infinity.");
        }

        [Test]
        [MultipleAsserts]
        public void CanDetermineIfNaN()
        {
            var complex = new Complex32(float.NaN, 1);
            Assert.IsTrue(complex.IsNaN, "Real part is NaN.");
            complex = new Complex32(1, float.NaN);
            Assert.IsTrue(complex.IsNaN, "Imaginary part is NaN.");
            complex = new Complex32(float.NaN, float.NaN);
            Assert.IsTrue(complex.IsNaN, "Both parts are NaN.");
        }

        [Test]
        public void CanDetermineIfOneValueComplexNumber()
        {
            var complex = new Complex32(1, 0);
            Assert.IsTrue(complex.IsOne, "Complex32 number with a value of one.");
        }

        [Test]
        public void CanDetermineIfRealNonNegativeNumber()
        {
            var complex = new Complex32(1, 0);
            Assert.IsTrue(complex.IsReal, "Is a real non-negative number.");
        }

        [Test]
        public void CanDetermineIfRealNumber()
        {
            var complex = new Complex32(-1, 0);
            Assert.IsTrue(complex.IsReal, "Is a real number.");
        }

        [Test]
        public void CanDetermineIfZeroValueComplexNumber()
        {
            var complex = new Complex32(0, 0);
            Assert.IsTrue(complex.IsZero, "Zero complex number.");
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideComplexNumberAndDoubleUsingOperators()
        {
            AssertEx.That(() => (Complex32.NaN * 1.0f).IsNaN);
            Assert.AreEqual(new Complex32(-2, 2), new Complex32(4, -4) / -2);
            Assert.AreEqual(new Complex32(0.25f, 0.25f), 2 / new Complex32(4, -4));
            Assert.AreEqual(Complex32.Infinity, 2.0f / Complex32.Zero);
            Assert.AreEqual(Complex32.Infinity, Complex32.One / 0);
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideTwoComplexNumbers()
        {
            AssertEx.That(() => Complex32.NaN.Multiply(Complex32.One).IsNaN);
            Assert.AreEqual(new Complex32(-2, 0), new Complex32(4, -4).Divide(new Complex32(-2, 2)));
            Assert.AreEqual(Complex32.Infinity, Complex32.One.Divide(Complex32.Zero));
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideTwoComplexNumbersUsingOperators()
        {
            AssertEx.That(() => (Complex32.NaN / Complex32.One).IsNaN);
            Assert.AreEqual(new Complex32(-2, 0), new Complex32(4, -4) / new Complex32(-2, 2));
            Assert.AreEqual(Complex32.Infinity, Complex32.One / Complex32.Zero);
        }

        [Test]
        [MultipleAsserts]
        public void CanMultipleComplexNumberAndDoubleUsingOperators()
        {
            AssertEx.That(() => (Complex32.NaN * 1.0f).IsNaN);
            Assert.AreEqual(new Complex32(8, -8), new Complex32(4, -4) * 2);
            Assert.AreEqual(new Complex32(8, -8), 2 * new Complex32(4, -4));
        }

        [Test]
        [MultipleAsserts]
        public void CanMultipleTwoComplexNumbers()
        {
            AssertEx.That(() => Complex32.NaN.Multiply(Complex32.One).IsNaN);
            Assert.AreEqual(new Complex32(0, 16), new Complex32(4, -4).Multiply(new Complex32(-2, 2)));
        }

        [Test]
        [MultipleAsserts]
        public void CanMultipleTwoComplexNumbersUsingOperators()
        {
            AssertEx.That(() => (Complex32.NaN * Complex32.One).IsNaN);
            Assert.AreEqual(new Complex32(0, 16), new Complex32(4, -4) * new Complex32(-2, 2));
        }

        [Test]
        public void CanNegateValue()
        {
            var complex = new Complex32(1.1f, -2.2f);
            Assert.AreEqual(new Complex32(-1.1f, 2.2f), complex.Negate());
        }

        [Test]
        public void CanNegateValueUsingOperator()
        {
            var complex = new Complex32(1.1f, -2.2f);
            Assert.AreEqual(new Complex32(-1.1f, 2.2f), -complex);
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractComplexNumberAndDoubleUsingOperartor()
        {
            AssertEx.That(() => (Complex32.NaN - float.NaN).IsNaN);
            AssertEx.That(() => (float.NaN - Complex32.NaN).IsNaN);
            AssertEx.That(() => (float.PositiveInfinity - Complex32.One).IsInfinity);
            AssertEx.That(() => (Complex32.Infinity - 1.0f).IsInfinity);
            AssertEx.That(() => (Complex32.One - 0.0f) == Complex32.One);
            AssertEx.That(() => (0.0f - Complex32.One) == -Complex32.One);
            AssertEx.That(() => (new Complex32(1.1f, -2.2f) - 1.1f == new Complex32(0.0f, -2.2f)));
            AssertEx.That(() => -2.2f - new Complex32(-1.1f, 2.2f) == new Complex32(-1.1f, -2.2f));
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoComplexNumbers()
        {
            AssertEx.That(() => Complex32.NaN.Subtract(Complex32.NaN).IsNaN);
            AssertEx.That(() => Complex32.Infinity.Subtract(Complex32.One).IsInfinity);
            AssertEx.That(() => Complex32.One.Subtract(Complex32.Zero) == Complex32.One);
            AssertEx.That(() => new Complex32(1.1f, -2.2f).Subtract(new Complex32(1.1f, -2.2f)) == Complex32.Zero);
        }

        [Test]
        [MultipleAsserts]
        public void CanTestForEquality()
        {
            Assert.AreNotEqual(Complex32.NaN, Complex32.NaN);
            Assert.AreEqual(Complex32.Infinity, Complex32.Infinity);
            Assert.AreEqual(new Complex32(1.1f, -2.2f), new Complex32(1.1f, -2.2f));
            Assert.AreNotEqual(new Complex32(-1.1f, 2.2f), new Complex32(1.1f, -2.2f));
        }

        [Test]
        [MultipleAsserts]
        public void CanTestForEqualityUsingOperators()
        {
            AssertEx.That(() => Complex32.NaN != Complex32.NaN);
            AssertEx.That(() => Complex32.Infinity == Complex32.Infinity);
            AssertEx.That(() => new Complex32(1.1f, -2.2f) == new Complex32(1.1f, -2.2f));
            AssertEx.That(() => new Complex32(-1.1f, 2.2f) != new Complex32(1.1f, -2.2f));
        }

        [Test]
        public void CanUsePlus()
        {
            var complex = new Complex32(1.1f, -2.2f);
            Assert.AreEqual(complex, complex.Plus());
        }

        [Test]
        public void CanUsePlusOperator()
        {
            var complex = new Complex32(1.1f, -2.2f);
            Assert.AreEqual(complex, +complex);
        }

        [Test]
        public void WithModulusArgumentThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => Complex32.WithModulusArgument(-1, 1), "Throws exception because modulus is negative.");
        }

        [Test]
        [Row(0.0f, 0.0f, 0.0f)]
        [Row(0.0f, 1.0f, 1.0f)]
        [Row(-1.0f, 1.0f, 1.4142135623730951)]
        [Row(-111.1, 111.1, 157.11912677965086)]
        public void CanComputeMagnitude(float real, float imag, float expected)
        {
            Assert.AreEqual(expected, new Complex32(real, imag).Magnitude);
        }

 
        [Test]
        [Row(float.PositiveInfinity, float.PositiveInfinity, Constants.Sqrt1Over2, Constants.Sqrt1Over2)]
        [Row(float.PositiveInfinity, float.NegativeInfinity, Constants.Sqrt1Over2, -Constants.Sqrt1Over2)]
        [Row(float.NegativeInfinity, float.PositiveInfinity, -Constants.Sqrt1Over2, -Constants.Sqrt1Over2)]
        [Row(float.NegativeInfinity, float.NegativeInfinity, -Constants.Sqrt1Over2, Constants.Sqrt1Over2)]
        [Row(0.0f, 0.0f, 0.0f, 0.0f)]
        [Row(-1.0f, 1.0f, -0.70710678118654746, 0.70710678118654746)]
        [Row(-111.1, 111.1, -0.70710678118654746, 0.70710678118654746)]
        public void CanComputeSign(float real, float imag, float expectedReal, float expectedImag)
        {
            Assert.AreEqual(new Complex32(expectedReal, expectedImag), new Complex32(real, imag).Sign);
        }

        [Test]
        public void CanConvertDecimalToComplex()
        {
            var orginal = new decimal(1.234567890);
            var complex = (Complex32)orginal;
            Assert.AreEqual((float)1.234567890, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        [Test]
        public void CanConvertByteToComplex()
        {
            const byte orginal = 123;
            var complex = (Complex32)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        [Test]
        public void CanConvertShortToComplex()
        {
            const short orginal = 123;
            var complex = (Complex32)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        [Test]
        public void CanConvertIntToComplex()
        {
            const int orginal = 123;
            var complex = (Complex32)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        [Test]
        public void CanConvertLongToComplex()
        {
            const long orginal = 123;
            var complex = (Complex32)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        [Test]
        public void CanConvertUIntToComplex()
        {
            const uint orginal = 123;
            var complex = (Complex32)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        [Test]
        public void CanConvertULongToComplex()
        {
            const ulong orginal = 123;
            var complex = (Complex32)orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        [Test]
        public void CanConvertFloatToComplex()
        {
            const float orginal = 123.456789f;
            var complex = (Complex32)orginal;
            Assert.AreEqual(123.456789f, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        [Test]
        public void CanConvertComplexToComplex32()
        {
            var complex32 = new Complex(123.456, -78.9);
            var complex = (Complex32)complex32;
            Assert.AreEqual(123.456f, complex.Real);
            Assert.AreEqual(-78.9f, complex.Imaginary);
        }

        [Test]
        public void CanGetConjugate()
        {
            var complex = new Complex(123.456, -78.9);
            var conjugate = complex.Conjugate;
            Assert.AreEqual(complex.Real, conjugate.Real);
            Assert.AreEqual(-complex.Imaginary, conjugate.Imaginary);
        }
   }
}