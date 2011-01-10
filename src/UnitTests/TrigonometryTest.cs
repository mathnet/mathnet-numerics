// <copyright file="TrigonometryTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests
{
    using System;
    using System.Numerics;
    using NUnit.Framework;

    /// <summary>
    /// Trigonometry tests.
    /// </summary>
    [TestFixture]
    public class TrigonometryTest
    {
        /// <summary>
        /// Can compute complex cosine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexCosine(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -1.19209289550780998537e-7)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -8.388608e6)] double imag, 
            [Values(1.0, -0.90175467375875928, -0.90175467375875928, 0.99999999999999289, 0.99999999999999289, -0.90175467375876572, double.PositiveInfinity)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, -5.1528001100635277e-8, double.NegativeInfinity)] double expectedImag)
        {
            var actual = new Complex(real, imag).Cosine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        /// <summary>
        /// Can compute complex sine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexSine(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -1.19209289550780998537e-7)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -8.388608e6)] double imag, 
            [Values(0.0, 0.43224820225679778, -0.43224820225679778, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 0.43224820225680083, double.NegativeInfinity)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, -1.0749753400787824e-7, double.NegativeInfinity)] double expectedImag)
        {
            var actual = new Complex(real, imag).Sine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        /// <summary>
        /// Can compute complex tangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexTangent(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double imag, 
            [Values(0.0, -0.47934123862654288, 0.47934123862654288, 1.1920928955078157e-7, -1.1920928955078157e-7, -0.47934123862653449, 0.47934123862653449)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.4659977233982276e-7, -1.4659977233982276e-7)] double expectedImag)
        {
            var actual = new Complex(real, imag).Tangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        /// <summary>
        /// Can compute cosecant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeCosecant(
            [Values(0.0, 8388608, 1.19209289550780998537e-7, -8388608, -1.19209289550780998537e-7)] double value, 
            [Values(double.PositiveInfinity, 2.3134856195559191, 8388608.0000000376, -2.3134856195559191, -8388608.0000000376)] double expected)
        {
            var actual = Trig.Cosecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        /// <summary>
        /// Can compute cosine. 
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeCosine(
            [Values(0.0, 8388608, 1.19209289550780998537e-7, -8388608, -1.19209289550780998537e-7)] double value, 
            [Values(1.0, -0.90175467375875928, 0.99999999999999289, -0.90175467375875928, 0.99999999999999289)] double expected)
        {
            var actual = Trig.Cosine(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute cotangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeCotangent(
            [Values(0.0, 8388608, 1.19209289550780998537e-7, -8388608, -1.19209289550780998537e-7)] double value, 
            [Values(double.PositiveInfinity, -2.086196470108229, 8388607.999999978, 2.086196470108229, -8388607.999999978)] double expected)
        {
            var actual = Trig.Cotangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        /// <summary>
        /// Can compute hyperbolic cosecant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeHyperbolicCosecant(
            [Values(0.0, 8388608, 1.19209289550780998537e-7, -8388608, -1.19209289550780998537e-7)] double value, 
            [Values(double.PositiveInfinity, 1.3670377960148449e-3643126, 8388607.9999999978, -1.3670377960148449e-3643126, -8388607.9999999978)] double expected)
        {
            var actual = Trig.HyperbolicCosecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        /// <summary>
        /// Can compute hyperbolic cosine. 
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeHyperbolicCosine(
            [Values(0.0, 8388608, 1.19209289550780998537e-7, -8388608, -1.19209289550780998537e-7)] double value, 
            [Values(1.0, double.PositiveInfinity, 1.0000000000000071, double.PositiveInfinity, 1.0000000000000071)] double expected)
        {
            var actual = Trig.HyperbolicCosine(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        /// <summary>
        /// Can compute hyperbolic cotangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeHyperbolicCotangent(
            [Values(0.0, 8388608, 1.19209289550780998537e-7, -8388608, -1.19209289550780998537e-7)] double value, 
            [Values(double.PositiveInfinity, 1.0, 8388608.0000000574, -1.0, -8388608.0000000574)] double expected)
        {
            var actual = Trig.HyperbolicCotangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        /// <summary>
        /// Can compute hyperbolic secant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeHyperbolicSecant(
            [Values(0.0, 8388608, 1.19209289550780998537e-7, -8388608, -1.19209289550780998537e-7)] double value, 
            [Values(1.0, 1.3670377960148449e-3643126, 0.99999999999999289, 1.3670377960148449e-3643126, 0.99999999999999289)] double expected)
        {
            var actual = Trig.HyperbolicSecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        /// <summary>
        /// Can compute hyperbolic sine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeHyperbolicSine(
            [Values(8388608, 1.19209289550780998537e-7, -8388608, -1.19209289550780998537e-7)] double value, 
            [Values(double.PositiveInfinity, 1.1920928955078128e-7, double.NegativeInfinity, -1.1920928955078128e-7)] double expected)
        {
            var actual = Trig.HyperbolicSine(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        /// <summary>
        /// Can compute hyperbolic tangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeHyperbolicTangent(
            [Values(0.0, 8388608, 1.19209289550780998537e-7, -8388608, -1.19209289550780998537e-7)] double value, 
            [Values(0.0, 1.0, 1.1920928955078043e-7, -1.0, -1.1920928955078043e-7)] double expected)
        {
            var actual = Trig.HyperbolicTangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        /// <summary>
        /// Can compute inverse cosecant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseCosecant(
            [Values(8388608, -8388608, 1, -1)] double value, 
            [Values(1.1920928955078097e-7, -1.1920928955078097e-7, 1.5707963267948966, -1.5707963267948966)] double expected)
        {
            var actual = Trig.InverseCosecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse cosine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseCosine(
            [Values(1, -1, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double value, 
            [Values(0, 3.1415926535897931, 1.570796207585607, 1.5707964460041861)] double expected)
        {
            var actual = Trig.InverseCosine(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        /// <summary>
        /// Can compute inverse cotangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseCotangent(
            [Values(0.0, 8388608, -8388608, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double value, 
            [Values(1.5707963267948966, 1.1920928955078069e-7, -1.1920928955078069e-7, 1.5707962075856071, -1.5707962075856071)] double expected)
        {
            var actual = Trig.InverseCotangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic cosecant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseHyperbolicCosecant(
            [Values(0.0, 8388608, -8388608, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double value, 
            [Values(double.PositiveInfinity, 1.1920928955078097e-7, -1.1920928955078097e-7, 16.635532333438693, -16.635532333438693)] double expected)
        {
            var actual = Trig.InverseHyperbolicCosecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic cosine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseHyperbolicCosine([Values(1.0, 8388608)] double value, [Values(0.0, 16.635532333438682)] double expected)
        {
            var actual = Trig.InverseHyperbolicCosine(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic cotangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseHyperbolicCotangent([Values(8388608, -8388608, 1, -1)] double value, [Values(1.1920928955078181e-7, -1.1920928955078181e-7, double.PositiveInfinity, double.NegativeInfinity)] double expected)
        {
            var actual = Trig.InverseHyperbolicCotangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic secant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseHyperbolicSecant([Values(0, 0.5, 1)] double value, [Values(double.PositiveInfinity, 1.3169578969248167, 0.0)] double expected)
        {
            var actual = Trig.InverseHyperbolicSecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic sine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseHyperbolicSine(
            [Values(0.0, 8388608, -8388608, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double value, 
            [Values(0.0, 16.63553233343869, -16.63553233343869, 1.1920928955078072e-7, -1.1920928955078072e-7)] double expected)
        {
            var actual = Trig.InverseHyperbolicSine(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic tangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseHyperbolicTangent(
            [Values(0.0, 1.0, -1.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double value, 
            [Values(0.0, double.PositiveInfinity, double.NegativeInfinity, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double expected)
        {
            var actual = Trig.InverseHyperbolicTangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse secant. 
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseSecant(
            [Values(8388608, -8388608, 1.0, -1.0)] double value, 
            [Values(1.5707962075856071, 1.5707964460041862, 0.0, 3.1415926535897932)] double expected)
        {
            var actual = Trig.InverseSecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse sine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseSine(
            [Values(0.0, 1.0, -1.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double value, 
            [Values(0.0, 1.5707963267948966, -1.5707963267948966, 1.1920928955078128e-7, -1.1920928955078128e-7)] double expected)
        {
            var actual = Trig.InverseSine(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse tangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeInverseTangent(
            [Values(0.0, 8388608, -8388608, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double value, 
            [Values(0.0, 1.570796207585607, -1.570796207585607, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double expected)
        {
            var actual = Trig.InverseTangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute secant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeSecant(
            [Values(0.0, 8388608, 1.19209289550780998537e-7, -8388608, -1.19209289550780998537e-7)] double value, 
            [Values(1.0, -1.1089490624226292, 1.0000000000000071, -1.1089490624226292, 1.0000000000000071)] double expected)
        {
            var actual = Trig.Secant(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute sine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeSine(
            [Values(0.0, 8388608, -8388608, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double value, 
            [Values(0.0, 0.43224820225679778, -0.43224820225679778, 1.1920928955078072e-7, -1.1920928955078072e-7)] double expected)
        {
            var actual = Trig.Sine(value);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        /// <summary>
        /// Can compute tangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [Test, Sequential]
        public void CanComputeTangent(
            [Values(0.0, 8388608, -8388608, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double value, 
            [Values(0.0, -0.47934123862654288, 0.47934123862654288, 1.1920928955078157e-7, -1.1920928955078157e-7)] double expected)
        {
            var actual = Trig.Tangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        /// <summary>
        /// Can convert degree to grad.
        /// </summary>
        [Test]
        public void CanConvertDegreeToGrad()
        {
            AssertHelpers.AlmostEqual(90 / .9, Trig.DegreeToGrad(90), 15);
        }

        /// <summary>
        /// Can convert degree to radian.
        /// </summary>
        [Test]
        public void CanConvertDegreeToRadian()
        {
            AssertHelpers.AlmostEqual(Math.PI / 2, Trig.DegreeToRadian(90), 15);
        }

        /// <summary>
        /// Can convert grad to degree.
        /// </summary>
        [Test]
        public void CanConvertGradToDegree()
        {
            AssertHelpers.AlmostEqual(180, Trig.GradToDegree(200), 15);
        }

        /// <summary>
        /// Can convert grad to radian. 
        /// </summary>
        [Test]
        public void CanConvertGradToRadian()
        {
            AssertHelpers.AlmostEqual(Math.PI, Trig.GradToRadian(200), 15);
        }

        /// <summary>
        /// Can convert radian to degree.
        /// </summary>
        [Test]
        public void CanConvertRadianToDegree()
        {
            AssertHelpers.AlmostEqual(60.0, Trig.RadianToDegree(Math.PI / 3.0), 15);
        }

        /// <summary>
        /// Can convert radian to grad.
        /// </summary>
        [Test]
        public void CanConvertRadianToGrad()
        {
            AssertHelpers.AlmostEqual(200.0 / 3.0, Trig.RadianToGrad(Math.PI / 3.0), 15);
        }

        /// <summary>
        /// Can compute complex cotangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexCotangent(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double imag, 
            [Values(double.PositiveInfinity, -2.086196470108229, 2.086196470108229, 8388607.999999978, -8388607.999999978, -2.0861964701080704, 2.0861964701080704)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, -6.3803383253713457e-7, 6.3803383253713457e-7)] double expectedImag)
        {
            var actual = new Complex(real, imag).Cotangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        /// <summary>
        /// Can compute complex secant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexSecant(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double imag, 
            [Values(1.0, -1.1089490624226292, -1.1089490624226292, 1.0000000000000071, 1.0000000000000071, -1.1089490624226177, -1.1089490624226177)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 6.3367488045143761e-8, 6.3367488045143761e-8)] double expectedImag)
        {
            var actual = new Complex(real, imag).Secant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        /// <summary>
        /// Can compute complex cosecant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexCosecant(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7)] double imag, 
            [Values(double.PositiveInfinity, 2.3134856195559191, -2.3134856195559191, 8388608.0000000376, -8388608.0000000376, 2.3134856195557596, -2.3134856195557596)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 5.7534999050657057e-7, -5.7534999050657057e-7)] double expectedImag)
        {
            var actual = new Complex(real, imag).Cosecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        /// <summary>
        /// Can compute complex hyperbolic sine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexHyperbolicSine(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(0.0, double.PositiveInfinity, double.NegativeInfinity, 1.1920928955078128e-7, -1.1920928955078128e-7, double.PositiveInfinity, double.NegativeInfinity, 0.45730415318424922)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, double.PositiveInfinity, double.NegativeInfinity, -0.54061268571315335)] double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicSine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex hyperbolic cosine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexHyperbolicCosine(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(1.0, double.PositiveInfinity, double.PositiveInfinity, 1.0000000000000071, 1.0000000000000071, double.PositiveInfinity, double.PositiveInfinity, 0.9895848833999199)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, double.PositiveInfinity, double.PositiveInfinity, -0.24982639750046154)] double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicCosine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex hyperbolic tangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexHyperbolicTangent(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(0.0, 1.0, -1.0, 1.1920928955078043e-7, -1.1920928955078043e-7, 1.0, -1.0, 0.56408314126749848)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.40389645531602575)] double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicTangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex hyperbolic cotangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexHyperbolicCotangent(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(double.PositiveInfinity, 1.0, -1.0, 8388608.0000000574, -8388608.0000000574, 1.0, -1.0, 1.1719451445243514)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.8391395790248311)] double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicCotangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex hyperbolic secant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexHyperbolicSecant(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(1.0, 0.0, 0.0, 0.99999999999999289, 0.99999999999999289, 0.0, -0.0, 0.94997886761549463)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.23982763093808804)] double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicSecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex hyperbolic cosecant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexHyperbolicCosecant(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(double.PositiveInfinity, 0.0, 0.0, 8388607.9999999978, -8388607.9999999978, 0.0, 0.0, 0.91207426403881078)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0782296946540223)] double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicCosecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse sine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseSine(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(0.0, 1.5707963267948966, -1.5707963267948966, 1.1920928955078128e-7, -1.1920928955078128e-7, 1.5707963267948966, -1.5707963267948966, 0.4522784471511907)] double expectedReal, 
            [Values(0.0, -16.635532333438682, 16.635532333438682, 0.0, 0.0, 16.635532333438682, -16.635532333438682, -0.53063753095251787)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseSine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse cosine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseCosine(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(1.5707963267948966, 0.0, 3.1415926535897931, 1.570796207585607, 1.5707964460041861, 1.4210854715202073e-14, 3.1415926535897789, 1.1185178796437059)] double expectedReal, 
            [Values(0.0, 16.635532333438682, -16.635532333438682, 0.0, 0.0, -16.635532333438682, 16.63553233343868, 0.53063753095251787)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseCosine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse tangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseTangent(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(0.0, 1.570796207585607, -1.570796207585607, 1.1920928955078043e-7, -1.1920928955078043e-7, 1.570796207585607, -1.570796207585607, 0.5535743588970452)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, -0.40235947810852507)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseTangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse cotangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseCotangent(
            [Values(0.0, 8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(Math.PI / 2.0, 1.1920928955078069e-7, -1.1920928955078069e-7, 1.5707962075856071, -1.5707962075856071, 1.1920928955078069e-7, -1.1920928955078069e-7, 1.0172219678978514)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 0.0, -1.6907571720583645e-21, 1.6907571720583645e-21, 0.40235947810852509)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseCotangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse secant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseSecant(
            [Values(8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(1.5707962075856071, 1.5707964460041862, 0.0, 3.1415926535897932, 1.5707962075856071, 1.5707964460041862, 0.90455689430238136)] double expectedReal, 
            [Values(0.0, 0.0, 16.635532333438686, -16.635532333438686, 1.6940658945086007e-21, -1.6940658945086007e-21, -1.0612750619050357)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseSecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse cosecant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseCosecant(
            [Values(8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(1.1920928955078153e-7, -1.1920928955078153e-7, 1.5707963267948966, -1.5707963267948966, 1.1920928955078153e-7, -1.1920928955078153e-7, 0.66623943249251526)] double expectedReal, 
            [Values(0.0, 0.0, -16.635532333438686, 16.635532333438686, -1.6940658945086007e-21, 1.6940658945086007e-21, 1.0612750619050357)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseCosecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic sine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseHyperbolicSine(
            [Values(8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(16.63553233343869, -16.63553233343869, 1.1920928955078072e-7, -1.1920928955078072e-7, 16.63553233343869, -16.63553233343869, 0.53063753095251787)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, 1.4210854715201873e-14, -1.4210854715201873e-14, -0.4522784471511907)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicSine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic cosine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseHyperbolicCosine(
            [Values(8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(16.635532333438682, 16.635532333438682, 0.0, 0.0, 16.635532333438682, 16.635532333438682, 0.53063753095251787)] double expectedReal, 
            [Values(0.0, 3.1415926535897931, 1.570796207585607, 1.5707964460041861, 1.4210854715202073e-14, -3.1415926535897789, -1.1185178796437059)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicCosine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic tangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseHyperbolicTangent(
            [Values(8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(1.1920928955078125e-7, -1.1920928955078125e-7, 1.1920928955078157e-7, -1.1920928955078157e-7, 1.1920928955078125e-7, -1.1920928955078125e-7, 0.40235947810852509)] double expectedReal, 
            [Values(-1.5707963267948966, 1.5707963267948966, 0.0, 0.0, 1.5707963267948966, -1.5707963267948966, -0.55357435889704525)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicTangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic cotangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseHyperbolicCotangent(
            [Values(8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(1.1920928955078181e-7, -1.1920928955078181e-7, 1.1920928955078157e-7, -1.1920928955078157e-7, 1.1920928955078181e-7, -1.1920928955078181e-7, 0.40235947810852509)] double expectedReal, 
            [Values(0.0, 0.0, -1.5707963267948966, 1.5707963267948966, -1.6940658945086212e-21, 1.6940658945086212e-21, 1.0172219678978514)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicCotangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic secant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseHyperbolicSecant(
            [Values(8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(0.0, 0.0, 16.635532333438686, 16.635532333438686, 1.6940658945086007e-21, 1.6940658945086007e-21, 1.0612750619050357)] double expectedReal, 
            [Values(1.5707962075856071, 1.5707964460041862, 0.0, 3.1415926535897932, -1.5707962075856071, 1.5707964460041862, 0.90455689430238136)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicSecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic cosecant
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [Test, Sequential]
        public void CanComputeComplexInverseHyperbolicCosecant(
            [Values(8.388608e6, -8.388608e6, 1.19209289550780998537e-7, -1.19209289550780998537e-7, 8.388608e6, -8.388608e6, 0.5)] double real, 
            [Values(0.0, 0.0, 0.0, 0.0, 1.19209289550780998537e-7, -1.19209289550780998537e-7, -0.5)] double imag, 
            [Values(1.1920928955078097e-7, -1.1920928955078097e-7, 16.635532333438693, -16.635532333438693, 1.1920928955078076e-7, -1.1920928955078076e-7, 1.0612750619050357)] double expectedReal, 
            [Values(0.0, 0.0, 0.0, 0.0, -1.6940658945085851e-21, 1.6940658945085851e-21, 0.66623943249251526)] double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicCosecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }
    }
}
