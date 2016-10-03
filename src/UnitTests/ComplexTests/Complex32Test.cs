// <copyright file="Complex32Test.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.ComplexTests
{
#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    /// <summary>
    /// Complex32 tests.
    /// </summary>
    [TestFixture]
    public class Complex32Test
    {
        /// <summary>
        /// Can add a complex number and a double using operator.
        /// </summary>
        [Test]
        public void CanAddComplexNumberAndDoubleUsingOperator()
        {
            Assert.That((Complex32.NaN + float.NaN).IsNaN());
            Assert.That((float.NaN + Complex32.NaN).IsNaN());
            Assert.That((float.PositiveInfinity + Complex32.One).IsInfinity());
            Assert.That((Complex32.PositiveInfinity + 1.0f).IsInfinity());
            Assert.That((Complex32.One + 0.0f) == Complex32.One);
            Assert.That((0.0f + Complex32.One) == Complex32.One);
            Assert.That(new Complex32(1.1f, -2.2f) + 1.1f == new Complex32(2.2f, -2.2f));
            Assert.That(-2.2f + new Complex32(-1.1f, 2.2f) == new Complex32(-3.3f, 2.2f));
        }

        /// <summary>
        /// Can add/subtract complex numbers using operator.
        /// </summary>
        [Test]
        public void CanAddSubtractComplexNumbersUsingOperator()
        {
            Assert.That((Complex32.NaN - Complex32.NaN).IsNaN());
            Assert.That((Complex32.PositiveInfinity - Complex32.One).IsInfinity());
            Assert.That((Complex32.One - Complex32.Zero) == Complex32.One);
            Assert.That((new Complex32(1.1f, -2.2f) - new Complex32(1.1f, -2.2f)) == Complex32.Zero);
        }

        /// <summary>
        /// Can add two complex numbers.
        /// </summary>
        [Test]
        public void CanAddTwoComplexNumbers()
        {
            Assert.That(Complex32.Add(Complex32.NaN, (Complex32.NaN)).IsNaN());
            Assert.That(Complex32.Add(Complex32.PositiveInfinity, Complex32.One).IsInfinity());
            Assert.That(Complex32.Add(Complex32.One, Complex32.Zero) == Complex32.One);
            Assert.That(Complex32.Add(new Complex32(1.1f, -2.2f), new Complex32(-1.1f, 2.2f)) == Complex32.Zero);
        }

        /// <summary>
        /// Can add two complex numbers using operator.
        /// </summary>
        [Test]
        public void CanAddTwoComplexNumbersUsingOperator()
        {
            Assert.That((Complex32.NaN + Complex32.NaN).IsNaN());
            Assert.That((Complex32.PositiveInfinity + Complex32.One).IsInfinity());
            Assert.That((Complex32.One + Complex32.Zero) == Complex32.One);
            Assert.That((new Complex32(1.1f, -2.2f) + new Complex32(-1.1f, 2.2f)) == Complex32.Zero);
        }

        /// <summary>
        /// Can get hash code.
        /// </summary>
        [Test]
        public void CanCalculateHashCode()
        {
            Assert.AreEqual(new Complex32(1, 2).GetHashCode(), new Complex32(1, 2).GetHashCode());
            Assert.AreNotEqual(new Complex32(1, 0).GetHashCode(), new Complex32(0, 1).GetHashCode());
            Assert.AreNotEqual(new Complex32(1, 1).GetHashCode(), new Complex32(2, 2).GetHashCode());
            Assert.AreNotEqual(new Complex32(1, 0).GetHashCode(), new Complex32(-1, 0).GetHashCode());
            Assert.AreNotEqual(new Complex32(0, 1).GetHashCode(), new Complex32(0, -1).GetHashCode());
        }

        /// <summary>
        /// Can compute exponential.
        /// </summary>
        /// <param name="real">Real part.</param>
        /// <param name="imag">Imaginary part.</param>
        /// <param name="expectedReal">Expected real part.</param>
        /// <param name="expectedImag">Expected imaginary part.</param>
        [TestCase(0.0f, 0.0f, 1.0f, 0.0f)]
        [TestCase(0.0f, 1.0f, 0.54030230586813977f, 0.8414709848078965f)]
        [TestCase(-1.0f, 1.0f, 0.19876611034641295f, 0.30955987565311222f)]
        [TestCase(-111.0f, 111.0f, -2.3259065941590448e-49f, -5.1181940185795617e-49f)]
        public void CanComputeExponential(float real, float imag, float expectedReal, float expectedImag)
        {
            var value = new Complex32(real, imag);
            var expected = new Complex32(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, value.Exponential(), 6);
        }

        /// <summary>
        /// Can compute natural logarithm.
        /// </summary>
        /// <param name="real">Real part.</param>
        /// <param name="imag">Imaginary part.</param>
        /// <param name="expectedReal">Expected real part.</param>
        /// <param name="expectedImag">Expected imaginary part.</param>
        [TestCase(0.0f, 0.0f, float.NegativeInfinity, 0.0f)]
        [TestCase(0.0f, 1.0f, 0.0f, 1.5707963267948966f)]
        [TestCase(-1.0f, 1.0f, 0.34657359027997264f, 2.3561944901923448f)]
        [TestCase(-111.1f, 111.1f, 5.0570042869255571f, 2.3561944901923448f)]
        [TestCase(111.1f, -111.1f, 5.0570042869255571f, -0.78539816339744828f)]
        public void CanComputeNaturalLogarithm(float real, float imag, float expectedReal, float expectedImag)
        {
            var value = new Complex32(real, imag);
            var expected = new Complex32(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, value.NaturalLogarithm(), 7);
        }

        /// <summary>
        /// Can compute power.
        /// </summary>
        [Test]
        public void CanComputePower()
        {
            var a = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            var b = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqualRelative(
                new Complex32(9.99998047207974718744e-1f, -1.76553541154378695012e-6f), a.Power(b), 6);
            a = new Complex32(0.0f, 1.19209289550780998537e-7f);
            b = new Complex32(0.0f, -1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqualRelative(new Complex32(1.00000018725172576491f, 1.90048076369011843105e-6f), a.Power(b), 6);
            a = new Complex32(0.0f, -1.19209289550780998537e-7f);
            b = new Complex32(0.0f, 0.5f);
            AssertHelpers.AlmostEqualRelative(new Complex32(-2.56488189382693049636e-1f, -2.17823120666116144959f), a.Power(b), 4);
            a = new Complex32(0.0f, 0.5f);
            b = new Complex32(0.0f, -0.5f);
            AssertHelpers.AlmostEqualRelative(new Complex32(2.06287223508090495171f, 7.45007062179724087859e-1f), a.Power(b), 6);
            a = new Complex32(0.0f, -0.5f);
            b = new Complex32(0.0f, 1.0f);
            AssertHelpers.AlmostEqualRelative(new Complex32(3.70040633557002510874f, -3.07370876701949232239f), a.Power(b), 6);
            a = new Complex32(0.0f, 2.0f);
            b = new Complex32(0.0f, -2.0f);
            AssertHelpers.AlmostEqualRelative(new Complex32(4.24532146387429353891f, -2.27479427903521192648e1f), a.Power(b), 5);
            a = new Complex32(0.0f, -8.388608e6f);
            b = new Complex32(1.19209289550780998537e-7f, 0.0f);
            AssertHelpers.AlmostEqualRelative(new Complex32(1.00000190048219620166f, -1.87253870018168043834e-7f), a.Power(b), 6);
            a = new Complex32(0.0f, 0.0f);
            b = new Complex32(0.0f, 0.0f);
            AssertHelpers.AlmostEqualRelative(new Complex32(1.0f, 0.0f), a.Power(b), 6);
            a = new Complex32(0.0f, 0.0f);
            b = new Complex32(1.0f, 0.0f);
            AssertHelpers.AlmostEqualRelative(new Complex32(0.0f, 0.0f), a.Power(b), 6);
            a = new Complex32(0.0f, 0.0f);
            b = new Complex32(-1.0f, 0.0f);
            AssertHelpers.AlmostEqualRelative(new Complex32(float.PositiveInfinity, 0.0f), a.Power(b), 6);
            a = new Complex32(0.0f, 0.0f);
            b = new Complex32(-1.0f, 1.0f);
            AssertHelpers.AlmostEqualRelative(new Complex32(float.PositiveInfinity, float.PositiveInfinity), a.Power(b), 6);
            a = new Complex32(0.0f, 0.0f);
            b = new Complex32(0.0f, 1.0f);
            Assert.That(a.Power(b).IsNaN());
        }

        /// <summary>
        /// Can compute root.
        /// </summary>
        [Test]
        public void CanComputeRoot()
        {
            var a = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            var b = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqualRelative(new Complex32(0.0f, 0.0f), a.Root(b), 6);
            a = new Complex32(0.0f, -1.19209289550780998537e-7f);
            b = new Complex32(0.0f, 0.5f);
            AssertHelpers.AlmostEqualRelative(new Complex32(0.038550761943650161f, 0.019526430428319544f), a.Root(b), 5);
            a = new Complex32(0.0f, 0.5f);
            b = new Complex32(0.0f, -0.5f);
            AssertHelpers.AlmostEqualRelative(new Complex32(0.007927894711475968f, -0.042480480425152213f), a.Root(b), 5);
            a = new Complex32(0.0f, -0.5f);
            b = new Complex32(0.0f, 1.0f);
            AssertHelpers.AlmostEqualRelative(new Complex32(0.15990905692806806f, 0.13282699942462053f), a.Root(b), 6);
            a = new Complex32(0.0f, 2.0f);
            b = new Complex32(0.0f, -2.0f);
            AssertHelpers.AlmostEqualRelative(new Complex32(0.42882900629436788f, 0.15487175246424678f), a.Root(b), 6);
            a = new Complex32(0.0f, -8.388608e6f);
            b = new Complex32(1.19209289550780998537e-7f, 0.0f);
            AssertHelpers.AlmostEqualRelative(new Complex32(float.PositiveInfinity, float.NegativeInfinity), a.Root(b), 6);
        }

        /// <summary>
        /// Can compute square.
        /// </summary>
        [Test]
        public void CanComputeSquare()
        {
            var complex = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqualRelative(new Complex32(0, 2.8421709430403888e-14f), complex.Square(), 7);
            complex = new Complex32(0.0f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqualRelative(new Complex32(-1.4210854715201944e-14f, 0.0f), complex.Square(), 7);
            complex = new Complex32(0.0f, -1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqualRelative(new Complex32(-1.4210854715201944e-14f, 0.0f), complex.Square(), 7);
            complex = new Complex32(0.0f, 0.5f);
            AssertHelpers.AlmostEqualRelative(new Complex32(-0.25f, 0.0f), complex.Square(), 7);
            complex = new Complex32(0.0f, -0.5f);
            AssertHelpers.AlmostEqualRelative(new Complex32(-0.25f, 0.0f), complex.Square(), 7);
            complex = new Complex32(0.0f, -8.388608e6f);
            AssertHelpers.AlmostEqualRelative(new Complex32(-70368744177664.0f, 0.0f), complex.Square(), 7);
        }

        /// <summary>
        /// Can compute square root.
        /// </summary>
        [Test]
        public void CanComputeSquareRoot()
        {
            var complex = new Complex32(1.19209289550780998537e-7f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqualRelative(
                new Complex32(0.00037933934912842666f, 0.00015712750315077684f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqualRelative(
                new Complex32(0.00024414062499999973f, 0.00024414062499999976f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, -1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqualRelative(
                new Complex32(0.00024414062499999973f, -0.00024414062499999976f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, 0.5f);
            AssertHelpers.AlmostEqualRelative(new Complex32(0.5f, 0.5f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, -0.5f);
            AssertHelpers.AlmostEqualRelative(new Complex32(0.5f, -0.5f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, -8.388608e6f);
            AssertHelpers.AlmostEqualRelative(new Complex32(2048.0f, -2048.0f), complex.SquareRoot(), 7);
            complex = new Complex32(8.388608e6f, 1.19209289550780998537e-7f);
            AssertHelpers.AlmostEqualRelative(new Complex32(2896.3093757400989f, 2.0579515874459933e-11f), complex.SquareRoot(), 7);
            complex = new Complex32(0.0f, 0.0f);
            AssertHelpers.AlmostEqualRelative(Complex32.Zero, complex.SquareRoot(), 7);
        }

        /// <summary>
        /// Can convert a double to a complex.
        /// </summary>
        [Test]
        public void CanConvertDoubleToComplex()
        {
            Assert.That(((Complex32)float.NaN).IsNaN());
            Assert.That(((Complex32)float.NegativeInfinity).IsInfinity());
            Assert.AreEqual((Complex32)1.1f, new Complex32(1.1f, 0));
        }

        /// <summary>
        /// Can create a complex number using constructor.
        /// </summary>
        [Test]
        public void CanCreateComplexNumberUsingConstructor()
        {
            var complex = new Complex32(1.1f, -2.2f);
            Assert.AreEqual(1.1f, complex.Real, "Real part is 1.1f.");
            Assert.AreEqual(-2.2f, complex.Imaginary, "Imaginary part is -2.2f.");
        }

        /// <summary>
        /// Can create a complex number with modulus argument.
        /// </summary>
        [Test]
        public void CanCreateComplexNumberWithModulusArgument()
        {
            var complex = Complex32.FromPolarCoordinates(2, (float)-Math.PI / 6);
            Assert.AreEqual((float)Constants.Sqrt3, complex.Real, 1e-7f, "Real part is Sqrt(3).");
            Assert.AreEqual(-1.0f, complex.Imaginary, 1e-7f, "Imaginary part is -1.");
        }

        /// <summary>
        /// Can create a complex number with real imaginary initializer.
        /// </summary>
        [Test]
        public void CanCreateComplexNumberWithRealImaginaryInitializer()
        {
            var complex = new Complex32(1.1f, -2.2f);
            Assert.AreEqual(1.1f, complex.Real, "Real part is 1.1f.");
            Assert.AreEqual(-2.2f, complex.Imaginary, "Imaginary part is -2.2f.");
        }

        /// <summary>
        /// Can determine if imaginary is unit.
        /// </summary>
        [Test]
        public void CanDetermineIfImaginaryUnit()
        {
            var complex = new Complex32(0, 1);
            Assert.IsTrue(complex.IsImaginaryOne(), "Imaginary unit");
        }

        /// <summary>
        /// Can determine if a complex is infinity.
        /// </summary>
        [Test]
        public void CanDetermineIfInfinity()
        {
            var complex = new Complex32(float.PositiveInfinity, 1);
            Assert.IsTrue(complex.IsInfinity(), "Real part is infinity.");
            complex = new Complex32(1, float.NegativeInfinity);
            Assert.IsTrue(complex.IsInfinity(), "Imaginary part is infinity.");
            complex = new Complex32(float.NegativeInfinity, float.PositiveInfinity);
            Assert.IsTrue(complex.IsInfinity(), "Both parts are infinity.");
        }

        /// <summary>
        /// Can determine if a complex is not a number.
        /// </summary>
        [Test]
        public void CanDetermineIfNaN()
        {
            var complex = new Complex32(float.NaN, 1);
            Assert.IsTrue(complex.IsNaN(), "Real part is NaN.");
            complex = new Complex32(1, float.NaN);
            Assert.IsTrue(complex.IsNaN(), "Imaginary part is NaN.");
            complex = new Complex32(float.NaN, float.NaN);
            Assert.IsTrue(complex.IsNaN(), "Both parts are NaN.");
        }

        /// <summary>
        /// Can determine Complex32 number with a value of one.
        /// </summary>
        [Test]
        public void CanDetermineIfOneValueComplexNumber()
        {
            var complex = new Complex32(1, 0);
            Assert.IsTrue(complex.IsOne(), "Complex32 number with a value of one.");
        }

        /// <summary>
        /// Can determine if a complex is a real non-negative number.
        /// </summary>
        [Test]
        public void CanDetermineIfRealNonNegativeNumber()
        {
            var complex = new Complex32(1, 0);
            Assert.IsTrue(complex.IsReal(), "Is a real non-negative number.");
        }

        /// <summary>
        /// Can determine if a complex is a real number.
        /// </summary>
        [Test]
        public void CanDetermineIfRealNumber()
        {
            var complex = new Complex32(-1, 0);
            Assert.IsTrue(complex.IsReal(), "Is a real number.");
        }

        /// <summary>
        /// Can determine if a complex is a zero number.
        /// </summary>
        [Test]
        public void CanDetermineIfZeroValueComplexNumber()
        {
            var complex = new Complex32(0, 0);
            Assert.IsTrue(complex.IsZero(), "Zero complex number.");
        }

        /// <summary>
        /// Can divide a complex number and a double using operators.
        /// </summary>
        [Test]
        public void CanDivideComplexNumberAndDoubleUsingOperators()
        {
            Assert.That((Complex32.NaN * 1.0f).IsNaN());
            Assert.AreEqual(new Complex32(-2, 2), new Complex32(4, -4) / -2);
            Assert.AreEqual(new Complex32(0.25f, 0.25f), 2 / new Complex32(4, -4));
            Assert.AreEqual(Complex32.PositiveInfinity, 2.0f / Complex32.Zero);
            Assert.AreEqual(Complex32.PositiveInfinity, Complex32.One / 0);
        }

        /// <summary>
        /// Can divide two complex numbers.
        /// </summary>
        [Test]
        public void CanDivideTwoComplexNumbers()
        {
            Assert.That(Complex32.Divide(Complex32.NaN, Complex32.One).IsNaN());
            Assert.AreEqual(new Complex32(-2, 0), Complex32.Divide(new Complex32(4, -4), new Complex32(-2, 2)));
            Assert.AreEqual(Complex32.PositiveInfinity, Complex32.Divide(Complex32.One, Complex32.Zero));
        }

        /// <summary>
        /// Can divide two complex numbers using operators.
        /// </summary>
        [Test]
        public void CanDivideTwoComplexNumbersUsingOperators()
        {
            Assert.That((Complex32.NaN / Complex32.One).IsNaN());
            Assert.AreEqual(new Complex32(-2, 0), new Complex32(4, -4) / new Complex32(-2, 2));
            Assert.AreEqual(Complex32.PositiveInfinity, Complex32.One / Complex32.Zero);
        }
        /// <summary>
        /// Can divide without overflow.
        /// </summary>
        [Test]
        public void CanDodgeOverflowDivision()
        {
            var first = new Complex32((float)Math.Pow(10, 37), (float)Math.Pow(10, -37));
            var second = new Complex32((float)Math.Pow(10, 25), (float)Math.Pow(10, -25));
            Assert.AreEqual(new Complex32((float)Math.Pow(10, 12), (float)Math.Pow(10, -38)), first / second);

            first = new Complex32(-(float)Math.Pow(10, 37), (float)Math.Pow(10, -37));
            second = new Complex32((float)Math.Pow(10, 25), (float)Math.Pow(10, -25));
            Assert.AreEqual(new Complex32(-(float)Math.Pow(10, 12), (float)Math.Pow(10, -38)), first / second);

            first = new Complex32((float)Math.Pow(10, -37), (float)Math.Pow(10, 37));
            second = new Complex32((float)Math.Pow(10, -17), -(float)Math.Pow(10, 17));
            Assert.AreEqual(new Complex32(-(float)Math.Pow(10, 20), (float)Math.Pow(10, -14)), first / second);

        }
        /// <summary>
        /// Can divide float/complex without overflow
        /// </summary>
        [Test]
        public void CanDodgeOverflowDivisionFloat()
        {

            var firstComplex = new Complex32((float)Math.Pow(10, 25), (float)Math.Pow(10, -25));
            float firstFloat = (float)Math.Pow(10, -37);
            float secondFloat = (float)Math.Pow(10, 37);

            Assert.AreEqual(new Complex32(0, 0), firstFloat / firstComplex); // it's (10^-62,10^-112) thus overflow to 0
            Assert.AreEqual(new Complex32((float)Math.Pow(10, 12), (float)Math.Pow(10, -38)), secondFloat / firstComplex);

            var secondComplex = new Complex32((float)Math.Pow(10, -25), (float)Math.Pow(10, 25));
            Assert.AreEqual(new Complex32(0, 0), firstFloat / secondComplex);// it's (10^-112,10^-62) thus overflow to 0
            Assert.AreEqual(new Complex32((float)Math.Pow(10, -38), -(float)Math.Pow(10, 12)), secondFloat / secondComplex);



            float thirdFloat = (float)Math.Pow(10, 13);
            var thirdComplex = new Complex32((float)Math.Pow(10, -25), (float)Math.Pow(10, -25));
            Assert.AreEqual(new Complex32(5.0f * (float)Math.Pow(10, 37), -5.0f * (float)Math.Pow(10, 37)), thirdFloat / thirdComplex);

            var fourthFloat = (float)Math.Pow(10, -30);
            var fourthComplex = new Complex32((float)Math.Pow(10, -30), (float)Math.Pow(10, -30));
            Assert.AreEqual(new Complex32(0.5f, -0.5f), fourthFloat / fourthComplex);

        }
        /// <summary>
        /// Can multiple a complex number and a double using operators.
        /// </summary>
        [Test]
        public void CanMultipleComplexNumberAndDoubleUsingOperators()
        {
            Assert.That((Complex32.NaN * 1.0f).IsNaN());
            Assert.AreEqual(new Complex32(8, -8), new Complex32(4, -4) * 2);
            Assert.AreEqual(new Complex32(8, -8), 2 * new Complex32(4, -4));
        }

        /// <summary>
        /// Can multiple two complex numbers.
        /// </summary>
        [Test]
        public void CanMultipleTwoComplexNumbers()
        {
            Assert.That(Complex32.Multiply(Complex32.NaN, Complex32.One).IsNaN());
            Assert.AreEqual(new Complex32(0, 16), Complex32.Multiply(new Complex32(4, -4), new Complex32(-2, 2)));
        }

        /// <summary>
        /// Can multiple two complex numbers using operators.
        /// </summary>
        [Test]
        public void CanMultipleTwoComplexNumbersUsingOperators()
        {
            Assert.That((Complex32.NaN * Complex32.One).IsNaN());
            Assert.AreEqual(new Complex32(0, 16), new Complex32(4, -4) * new Complex32(-2, 2));
        }

        /// <summary>
        /// Can negate.
        /// </summary>
        [Test]
        public void CanNegateValue()
        {
            var complex = new Complex32(1.1f, -2.2f);
            Assert.AreEqual(new Complex32(-1.1f, 2.2f), Complex32.Negate(complex));
        }

        /// <summary>
        /// Can negate using operator.
        /// </summary>
        [Test]
        public void CanNegateValueUsingOperator()
        {
            var complex = new Complex32(1.1f, -2.2f);
            Assert.AreEqual(new Complex32(-1.1f, 2.2f), -complex);
        }

        /// <summary>
        /// Can subtract a complex number and a double using operator.
        /// </summary>
        [Test]
        public void CanSubtractComplexNumberAndDoubleUsingOperator()
        {
            Assert.That((Complex32.NaN - float.NaN).IsNaN());
            Assert.That((float.NaN - Complex32.NaN).IsNaN());
            Assert.That((float.PositiveInfinity - Complex32.One).IsInfinity());
            Assert.That((Complex32.PositiveInfinity - 1.0f).IsInfinity());
            Assert.That((Complex32.One - 0.0f) == Complex32.One);
            Assert.That((0.0f - Complex32.One) == -Complex32.One);
            Assert.That(new Complex32(1.1f, -2.2f) - 1.1f == new Complex32(0.0f, -2.2f));
            Assert.That(-2.2f - new Complex32(-1.1f, 2.2f) == new Complex32(-1.1f, -2.2f));
        }

        /// <summary>
        /// Can subtract two complex numbers.
        /// </summary>
        [Test]
        public void CanSubtractTwoComplexNumbers()
        {
            Assert.That(Complex32.Subtract(Complex32.NaN, Complex32.NaN).IsNaN());
            Assert.That(Complex32.Subtract(Complex32.PositiveInfinity, Complex32.One).IsInfinity());
            Assert.That(Complex32.Subtract(Complex32.One, Complex32.Zero) == Complex32.One);
            Assert.That(Complex32.Subtract(new Complex32(1.1f, -2.2f), new Complex32(1.1f, -2.2f)) == Complex32.Zero);
        }

        /// <summary>
        /// Can test for equality.
        /// </summary>
        [Test]
        public void CanTestForEquality()
        {
            Assert.AreNotEqual(Complex32.NaN, Complex32.NaN);
            Assert.AreEqual(Complex32.PositiveInfinity, Complex32.PositiveInfinity);
            Assert.AreEqual(new Complex32(1.1f, -2.2f), new Complex32(1.1f, -2.2f));
            Assert.AreNotEqual(new Complex32(-1.1f, 2.2f), new Complex32(1.1f, -2.2f));
        }

        /// <summary>
        /// Can test for equality using operators.
        /// </summary>
        [Test]
        public void CanTestForEqualityUsingOperators()
        {
#pragma warning disable 1718
            Assert.That(Complex32.NaN != Complex32.NaN);
            Assert.That(Complex32.PositiveInfinity == Complex32.PositiveInfinity);
#pragma warning restore 1718
            Assert.That(new Complex32(1.1f, -2.2f) == new Complex32(1.1f, -2.2f));
            Assert.That(new Complex32(-1.1f, 2.2f) != new Complex32(1.1f, -2.2f));
        }

        /// <summary>
        /// Can use unary "+" operator.
        /// </summary>
        [Test]
        public void CanUsePlusOperator()
        {
            var complex = new Complex32(1.1f, -2.2f);
            Assert.AreEqual(complex, +complex);
        }

        /// <summary>
        /// Can compute magnitude.
        /// </summary>
        /// <param name="real">Real part.</param>
        /// <param name="imag">Imaginary part.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0f, 0.0f, 0.0f)]
        [TestCase(0.0f, 1.0f, 1.0f)]
        [TestCase(-1.0f, 1.0f, 1.4142135623730951f)]
        [TestCase(-111.1f, 111.1f, 157.11912677965086f)]
        public void CanComputeMagnitude(float real, float imag, float expected)
        {
            Assert.AreEqual(expected, new Complex32(real, imag).Magnitude);
        }
        /// <summary>
        /// Can calculate magnitude without overflow
        /// </summary>
        [Test]
        public void CanDodgeOverflowMagnitude()
        {
            Assert.AreEqual((float)Math.Sqrt(2) * float.Epsilon, new Complex32(float.Epsilon, float.Epsilon).Magnitude);
            Assert.AreEqual(float.Epsilon, new Complex32(0, float.Epsilon).Magnitude);
            Assert.AreEqual((float)(Math.Pow(10, 30) * Math.Sqrt(2)), new Complex32((float)Math.Pow(10, 30), (float)Math.Pow(10, 30)).Magnitude);

        }
        /// <summary>
        /// Can compute sign.
        /// </summary>
        /// <param name="real">Real part.</param>
        /// <param name="imag">Imaginary part.</param>
        /// <param name="expectedReal">Expected real value.</param>
        /// <param name="expectedImag">Expected imaginary value.</param>
        [TestCase(float.PositiveInfinity, float.PositiveInfinity, (float)Constants.Sqrt1Over2, (float)Constants.Sqrt1Over2)]
        [TestCase(float.PositiveInfinity, float.NegativeInfinity, (float)Constants.Sqrt1Over2, (float)-Constants.Sqrt1Over2)]
        [TestCase(float.NegativeInfinity, float.PositiveInfinity, (float)-Constants.Sqrt1Over2, (float)-Constants.Sqrt1Over2)]
        [TestCase(float.NegativeInfinity, float.NegativeInfinity, (float)-Constants.Sqrt1Over2, (float)Constants.Sqrt1Over2)]
        [TestCase(0.0f, 0.0f, 0.0f, 0.0f)]
        [TestCase(-1.0f, 1.0f, -0.70710678118654746f, 0.70710678118654746f)]
        [TestCase(-111.1f, 111.1f, -0.70710678118654746f, 0.70710678118654746f)]
        public void CanComputeSign(float real, float imag, float expectedReal, float expectedImag)
        {
            Assert.AreEqual(new Complex32(expectedReal, expectedImag), new Complex32(real, imag).Sign);
        }

        /// <summary>
        /// Can convert a decimal to a complex.
        /// </summary>
        [Test]
        public void CanConvertDecimalToComplex()
        {
            var orginal = new decimal(1.234567890);
            var complex = (Complex32)orginal;
            Assert.AreEqual((float)1.234567890, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        /// <summary>
        /// Can convert a byte to a complex.
        /// </summary>
        [Test]
        public void CanConvertByteToComplex()
        {
            const byte Orginal = 123;
            var complex = (Complex32)Orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        /// <summary>
        /// Can convert a short to a complex.
        /// </summary>
        [Test]
        public void CanConvertShortToComplex()
        {
            const short Orginal = 123;
            var complex = (Complex32)Orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        /// <summary>
        /// Can convert an int to a complex.
        /// </summary>
        [Test]
        public void CanConvertIntToComplex()
        {
            const int Orginal = 123;
            var complex = (Complex32)Orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        /// <summary>
        /// Can convert a long to a complex.
        /// </summary>
        [Test]
        public void CanConvertLongToComplex()
        {
            const long Orginal = 123;
            var complex = (Complex32)Orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        /// <summary>
        /// Can convert an uint to a complex.
        /// </summary>
        [Test]
        public void CanConvertUIntToComplex()
        {
            const uint Orginal = 123;
            var complex = (Complex32)Orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        /// <summary>
        /// Can convert an ulong to  complex.
        /// </summary>
        [Test]
        public void CanConvertULongToComplex()
        {
            const ulong Orginal = 123;
            var complex = (Complex32)Orginal;
            Assert.AreEqual(123, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        /// <summary>
        /// Can convert a float to a complex.
        /// </summary>
        [Test]
        public void CanConvertFloatToComplex()
        {
            const float Orginal = 123.456789f;
            var complex = (Complex32)Orginal;
            Assert.AreEqual(123.456789f, complex.Real);
            Assert.AreEqual(0.0f, complex.Imaginary);
        }

        /// <summary>
        /// Can convert a complex to a complex32.
        /// </summary>
        [Test]
        public void CanConvertComplexToComplex32()
        {
            var complex32 = new Complex(123.456, -78.9);
            var complex = (Complex32)complex32;
            Assert.AreEqual(123.456f, complex.Real);
            Assert.AreEqual(-78.9f, complex.Imaginary);
        }

        /// <summary>
        /// Can conjugate.
        /// </summary>
        [Test]
        public void CanGetConjugate()
        {
            var complex = new Complex32(123.456f, -78.9f);
            var conjugate = complex.Conjugate();
            Assert.AreEqual(complex.Real, conjugate.Real);
            Assert.AreEqual(-complex.Imaginary, conjugate.Imaginary);
        }
    }
}
