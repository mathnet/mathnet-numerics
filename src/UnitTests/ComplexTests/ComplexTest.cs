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

namespace MathNet.Numerics.UnitTests.ComplexExtensionTests
{
    using System;
    using System.Numerics;
    using MbUnit.Framework;

    [TestFixture]
    public class ComplexExtensionTest
    {

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
            AssertEx.That(() => a.Power(b).IsNaN());
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
        public void CanDetermineIfImaginaryUnit()
        {
            var complex = new Complex(0, 1);
            Assert.IsTrue(complex.IsImaginaryOne(), "Imaginary unit");
        }

        [Test]
        [MultipleAsserts]
        public void CanDetermineIfInfinity()
        {
            var complex = new Complex(double.PositiveInfinity, 1);
            Assert.IsTrue(complex.IsInfinity(), "Real part is infinity.");
            complex = new Complex(1, double.NegativeInfinity);
            Assert.IsTrue(complex.IsInfinity(), "Imaginary part is infinity.");
            complex = new Complex(double.NegativeInfinity, double.PositiveInfinity);
            Assert.IsTrue(complex.IsInfinity(), "Both parts are infinity.");
        }

        [Test]
        [MultipleAsserts]
        public void CanDetermineIfNaN()
        {
            var complex = new Complex(double.NaN, 1);
            Assert.IsTrue(complex.IsNaN(), "Real part is NaN.");
            complex = new Complex(1, double.NaN);
            Assert.IsTrue(complex.IsNaN(), "Imaginary part is NaN.");
            complex = new Complex(double.NaN, double.NaN);
            Assert.IsTrue(complex.IsNaN(), "Both parts are NaN.");
        }

        [Test]
        public void CanDetermineIfOneValueComplexNumber()
        {
            var complex = new Complex(1, 0);
            Assert.IsTrue(complex.IsOne(), "Complex number with a value of one.");
        }

        [Test]
        public void CanDetermineIfRealNonNegativeNumber()
        {
            var complex = new Complex(1, 0);
            Assert.IsTrue(complex.IsReal(), "Is a real non-negative number.");
        }

        [Test]
        public void CanDetermineIfRealNumber()
        {
            var complex = new Complex(-1, 0);
            Assert.IsTrue(complex.IsReal(), "Is a real number.");
        }

        [Test]
        public void CanDetermineIfZeroValueComplexNumber()
        {
            var complex = new Complex(0, 0);
            Assert.IsTrue(complex.IsZero(), "Zero complex number.");
        }
  }
}