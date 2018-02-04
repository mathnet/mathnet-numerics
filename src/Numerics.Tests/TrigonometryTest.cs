// <copyright file="TrigonometryTest.cs" company="Math.NET">
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
using System.Numerics;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests
{
    /// <summary>
    /// Trigonometry tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class TrigonometryTest
    {
        /// <summary>
        /// Can compute complex cosine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 1.0, 0.0)]
        [TestCase(8.388608e6, 0.0, -0.90175467375875928, 0.0)]
        [TestCase(-8.388608e6, 0.0, -0.90175467375875928, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 0.99999999999999289, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, 0.99999999999999289, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, -0.90175467375876572, -5.1528001100635277e-8)]
        [TestCase(-1.19209289550780998537e-7, -8.388608e6, double.PositiveInfinity, double.NegativeInfinity)]
        [TestCase(512.0, 32.0, -39356457675156.6, -3139507838262.74)]
        [TestCase(-512.0, -32.0, -39356457675156.6, -3139507838262.74)]
        [TestCase(32.0, -512.0, +9.52855589474963e+221, +6.29843301304523e+221)]
        [TestCase(-32.0, -512.0, +9.52855589474963e+221, -6.29843301304523e+221)]
        [TestCase(355.0, 355.0, -7.4732769989672537746e153, +2.25277102712558920722e149)]
        [TestCase(355.58, 355.58, -1.11645148443766450218e154, +7.31511888973169437082e153)]
        public void CanComputeComplexCosine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Cos();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can compute complex sine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 0.0, 0.0)]
        [TestCase(8.388608e6, 0.0, 0.43224820225679778, 0.0)]
        [TestCase(-8.388608e6, 0.0, -0.43224820225679778, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.19209289550780998537e-7, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.19209289550780998537e-7, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 0.43224820225680083, -1.0749753400787824e-7)]
        [TestCase(-1.19209289550780998537e-7, -8.388608e6, double.NegativeInfinity, double.NegativeInfinity)]
        [TestCase(512.0, 32.0, +3139507838262.74, -39356457675156.6)]
        [TestCase(-512.0, -32.0, -3139507838262.74, +39356457675156.6)]
        [TestCase(32.0, -512.0, +6.29843301304523e+221, -9.52855589474963e+221)]
        [TestCase(-32.0, -512.0, -6.29843301304523e+221, -9.52855589474963e+221)]
        [TestCase(355.0, 355.0, -2.25277102712558920722e149, -7.4732769989672537746e153)]
        [TestCase(355.58, 355.58, -+7.31511888973169437082e153, -1.11645148443766450218e154)]
        public void CanComputeComplexSine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Sin();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can compute complex tangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 0.0, 0.0)]
        [TestCase(8.388608e6, 0.0, -0.47934123862654288, 0.0)]
        [TestCase(-8.388608e6, 0.0, 0.47934123862654288, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.1920928955078157e-7, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.1920928955078157e-7, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, -0.47934123862653449, 1.4659977233982276e-7)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, 0.47934123862653449, -1.4659977233982276e-7)]
        [TestCase(512.0, 32.0, -5.08515122860094E-29, 1)]
        [TestCase(-512.0, -32.0, +5.08515122860094E-29, -1)]
        [TestCase(32.0, 512.0, +3.52598650243739e-445, 1)]
        [TestCase(32.0, -512.0, +3.52598650243739e-445, -1)]
        [TestCase(355.0, 355.0, +5.39739014654622322424e-313, +1.0)]
        [TestCase(355.58, 355.58, +2.57308259096360322163e-309, +1.0)]
        [TestCase(196.0, 16.0, +1.62991625102252204721e-14, +1.00000000000001938715)]
        public void CanComputeComplexTangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Tan();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can compute cosecant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, double.PositiveInfinity)]
        [TestCase(8388608, 2.3134856195559191)]
        [TestCase(1.19209289550780998537e-7, 8388608.0000000376)]
        [TestCase(-8388608, -2.3134856195559191)]
        [TestCase(-1.19209289550780998537e-7, -8388608.0000000376)]
        public void CanComputeCosecant(double value, double expected)
        {
            var actual = Trig.Csc(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can compute cosine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 1.0)]
        [TestCase(8388608, -0.90175467375875928)]
        [TestCase(1.19209289550780998537e-7, 0.99999999999999289)]
        [TestCase(-8388608, -0.90175467375875928)]
        [TestCase(-1.19209289550780998537e-7, 0.99999999999999289)]
        public void CanComputeCosine(double value, double expected)
        {
            var actual = Trig.Cos(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 13);
        }

        /// <summary>
        /// Can compute cotangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, double.PositiveInfinity)]
        [TestCase(8388608, -2.086196470108229)]
        [TestCase(1.19209289550780998537e-7, 8388607.999999978)]
        [TestCase(-8388608, 2.086196470108229)]
        [TestCase(-1.19209289550780998537e-7, -8388607.999999978)]
        public void CanComputeCotangent(double value, double expected)
        {
            var actual = Trig.Cot(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can compute hyperbolic cosecant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, double.PositiveInfinity)]
        [TestCase(8388608, 1.3670377960148449e-3643126)]
        [TestCase(1.19209289550780998537e-7, 8388607.9999999978)]
        [TestCase(-8388608, -1.3670377960148449e-3643126)]
        [TestCase(-1.19209289550780998537e-7, -8388607.9999999978)]
        public void CanComputeHyperbolicCosecant(double value, double expected)
        {
            var actual = Trig.Csch(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute hyperbolic cosine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 1.0)]
        [TestCase(8388608, double.PositiveInfinity)]
        [TestCase(1.19209289550780998537e-7, 1.0000000000000071)]
        [TestCase(-8388608, double.PositiveInfinity)]
        [TestCase(-1.19209289550780998537e-7, 1.0000000000000071)]
        public void CanComputeHyperbolicCosine(double value, double expected)
        {
            var actual = Trig.Cosh(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 15);
        }

        /// <summary>
        /// Can compute hyperbolic cotangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, double.PositiveInfinity)]
        [TestCase(8388608, 1.0)]
        [TestCase(1.19209289550780998537e-7, 8388608.0000000574)]
        [TestCase(-8388608, -1.0)]
        [TestCase(-1.19209289550780998537e-7, -8388608.0000000574)]
        public void CanComputeHyperbolicCotangent(double value, double expected)
        {
            var actual = Trig.Coth(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute hyperbolic secant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 1.0)]
        [TestCase(8388608, 1.3670377960148449e-3643126)]
        [TestCase(1.19209289550780998537e-7, 0.99999999999999289)]
        [TestCase(-8388608, 1.3670377960148449e-3643126)]
        [TestCase(-1.19209289550780998537e-7, 0.99999999999999289)]
        public void CanComputeHyperbolicSecant(double value, double expected)
        {
            var actual = Trig.Sech(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 15);
        }

        /// <summary>
        /// Can compute hyperbolic sine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(8388608, double.PositiveInfinity)]
        [TestCase(1.19209289550780998537e-7, 1.1920928955078128e-7)]
        [TestCase(-8388608, double.NegativeInfinity)]
        [TestCase(-1.19209289550780998537e-7, -1.1920928955078128e-7)]
        public void CanComputeHyperbolicSine(double value, double expected)
        {
            var actual = Trig.Sinh(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 15);
        }

        /// <summary>
        /// Can compute hyperbolic tangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(8388608, 1.0)]
        [TestCase(1.19209289550780998537e-7, 1.1920928955078043e-7)]
        [TestCase(-8388608, -1.0)]
        [TestCase(-1.19209289550780998537e-7, -1.1920928955078043e-7)]
        public void CanComputeHyperbolicTangent(double value, double expected)
        {
            var actual = Trig.Tanh(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 15);
        }

        /// <summary>
        /// Can compute inverse cosecant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(8388608, 1.1920928955078097e-7)]
        [TestCase(-8388608, -1.1920928955078097e-7)]
        [TestCase(1, 1.5707963267948966)]
        [TestCase(-1, -1.5707963267948966)]
        public void CanComputeInverseCosecant(double value, double expected)
        {
            var actual = Trig.Acsc(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 13);
        }

        /// <summary>
        /// Can compute inverse cosine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(1, 0)]
        [TestCase(-1, 3.1415926535897931)]
        [TestCase(1.19209289550780998537e-7, 1.570796207585607)]
        [TestCase(-1.19209289550780998537e-7, 1.5707964460041861)]
        public void CanComputeInverseCosine(double value, double expected)
        {
            var actual = Trig.Acos(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 15);
        }

        /// <summary>
        /// Can compute inverse cotangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 1.5707963267948966)]
        [TestCase(8388608, 1.1920928955078069e-7)]
        [TestCase(-8388608, -1.1920928955078069e-7)]
        [TestCase(1.19209289550780998537e-7, 1.5707962075856071)]
        [TestCase(-1.19209289550780998537e-7, -1.5707962075856071)]
        public void CanComputeInverseCotangent(double value, double expected)
        {
            var actual = Trig.Acot(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic cosecant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, double.PositiveInfinity)]
        [TestCase(8388608, 1.1920928955078097e-7)]
        [TestCase(-8388608, -1.1920928955078097e-7)]
        [TestCase(1.19209289550780998537e-7, 16.635532333438693)]
        [TestCase(-1.19209289550780998537e-7, -16.635532333438693)]
        public void CanComputeInverseHyperbolicCosecant(double value, double expected)
        {
            var actual = Trig.Acsch(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic cosine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(1.0, 0.0)]
        [TestCase(8388608, 16.635532333438682)]
        [TestCase(1.7976931348623157E+308, 710.47586007394394203711)]
        public void CanComputeInverseHyperbolicCosine(double value, double expected)
        {
            var actual = Trig.Acosh(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic cotangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(8388608, 1.1920928955078181e-7)]
        [TestCase(-8388608, -1.1920928955078181e-7)]
        [TestCase(1, double.PositiveInfinity)]
        [TestCase(-1, double.NegativeInfinity)]
        public void CanComputeInverseHyperbolicCotangent(double value, double expected)
        {
            var actual = Trig.Acoth(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 13);
        }

        /// <summary>
        /// Can compute inverse hyperbolic secant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0, double.PositiveInfinity)]
        [TestCase(0.5, 1.3169578969248167)]
        [TestCase(1, 0.0)]
        public void CanComputeInverseHyperbolicSecant(double value, double expected)
        {
            var actual = Trig.Asech(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic sine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(8388608, 16.63553233343869)]
        [TestCase(-8388608, -16.63553233343869)]
        [TestCase(1.19209289550780998537e-7, 1.1920928955078072e-7)]
        [TestCase(-1.19209289550780998537e-7, -1.1920928955078072e-7)]
        [TestCase(1.7976931348623157E+308, 710.47586007394394203711)]
        [TestCase(-1.7976931348623157E+308, -710.47586007394394203711)]
        public void CanComputeInverseHyperbolicSine(double value, double expected)
        {
            var actual = Trig.Asinh(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse hyperbolic tangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(1.0, double.PositiveInfinity)]
        [TestCase(-1.0, double.NegativeInfinity)]
        [TestCase(1.19209289550780998537e-7, 1.19209289550780998537e-7)]
        [TestCase(-1.19209289550780998537e-7, -1.19209289550780998537e-7)]
        public void CanComputeInverseHyperbolicTangent(double value, double expected)
        {
            var actual = Trig.Atanh(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 13);
        }

        /// <summary>
        /// Can compute inverse secant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(8388608, 1.5707962075856071)]
        [TestCase(-8388608, 1.5707964460041862)]
        [TestCase(1.0, 0.0)]
        [TestCase(-1.0, 3.1415926535897932)]
        public void CanComputeInverseSecant(double value, double expected)
        {
            var actual = Trig.Asec(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse sine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(1.0, 1.5707963267948966)]
        [TestCase(-1.0, -1.5707963267948966)]
        [TestCase(1.19209289550780998537e-7, 1.1920928955078128e-7)]
        [TestCase(-1.19209289550780998537e-7, -1.1920928955078128e-7)]
        public void CanComputeInverseSine(double value, double expected)
        {
            var actual = Trig.Asin(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute inverse tangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(8388608, 1.570796207585607)]
        [TestCase(-8388608, -1.570796207585607)]
        [TestCase(1.19209289550780998537e-7, 1.19209289550780998537e-7)]
        [TestCase(-1.19209289550780998537e-7, -1.19209289550780998537e-7)]
        public void CanComputeInverseTangent(double value, double expected)
        {
            var actual = Trig.Atan(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 13);
        }

        /// <summary>
        /// Can compute secant.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 1.0)]
        [TestCase(8388608, -1.1089490624226292)]
        [TestCase(1.19209289550780998537e-7, 1.0000000000000071)]
        [TestCase(-8388608, -1.1089490624226292)]
        [TestCase(-1.19209289550780998537e-7, 1.0000000000000071)]
        public void CanComputeSecant(double value, double expected)
        {
            var actual = Trig.Sec(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 13);
        }

        /// <summary>
        /// Can compute sine.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(8388608, 0.43224820225679778)]
        [TestCase(-8388608, -0.43224820225679778)]
        [TestCase(1.19209289550780998537e-7, 1.1920928955078072e-7)]
        [TestCase(-1.19209289550780998537e-7, -1.1920928955078072e-7)]
        public void CanComputeSine(double value, double expected)
        {
            var actual = Trig.Sin(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can compute tangent.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="expected">Expected value.</param>
        [TestCase(0.0, 0.0)]
        [TestCase(8388608, -0.47934123862654288)]
        [TestCase(-8388608, 0.47934123862654288)]
        [TestCase(1.19209289550780998537e-7, 1.1920928955078157e-7)]
        [TestCase(-1.19209289550780998537e-7, -1.1920928955078157e-7)]
        public void CanComputeTangent(double value, double expected)
        {
            var actual = Trig.Tan(value);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can convert degree to grad.
        /// </summary>
        [Test]
        public void CanConvertDegreeToGrad()
        {
            AssertHelpers.AlmostEqualRelative(90 / .9, Trig.DegreeToGrad(90), 15);
        }

        /// <summary>
        /// Can convert degree to radian.
        /// </summary>
        [Test]
        public void CanConvertDegreeToRadian()
        {
            AssertHelpers.AlmostEqualRelative(Math.PI / 2, Trig.DegreeToRadian(90), 15);
        }

        /// <summary>
        /// Can convert grad to degree.
        /// </summary>
        [Test]
        public void CanConvertGradToDegree()
        {
            AssertHelpers.AlmostEqualRelative(180, Trig.GradToDegree(200), 15);
        }

        /// <summary>
        /// Can convert grad to radian.
        /// </summary>
        [Test]
        public void CanConvertGradToRadian()
        {
            AssertHelpers.AlmostEqualRelative(Math.PI, Trig.GradToRadian(200), 15);
        }

        /// <summary>
        /// Can convert radian to degree.
        /// </summary>
        [Test]
        public void CanConvertRadianToDegree()
        {
            AssertHelpers.AlmostEqualRelative(60.0, Trig.RadianToDegree(Math.PI / 3.0), 14);
        }

        /// <summary>
        /// Can convert radian to grad.
        /// </summary>
        [Test]
        public void CanConvertRadianToGrad()
        {
            AssertHelpers.AlmostEqualRelative(200.0 / 3.0, Trig.RadianToGrad(Math.PI / 3.0), 14);
        }

        /// <summary>
        /// Can compute complex cotangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, double.PositiveInfinity, 0.0)]
        [TestCase(8.388608e6, 0.0, -2.086196470108229, 0.0)]
        [TestCase(-8.388608e6, 0.0, 2.086196470108229, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 8388607.999999978, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -8388607.999999978, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, -2.0861964701080704, -6.3803383253713457e-7)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, 2.0861964701080704, 6.3803383253713457e-7)]
        [TestCase(512.0, 32.0, -5.08515122860094E-29, -1.0)]
        [TestCase(-512.0, -32.0, 5.08515122860094E-29, +1.0)]
        [TestCase(32.0, 512.0, +3.52598650243739e-445, -1.0)]
        [TestCase(32.0, -512.0, +3.52598650243739e-445, +1.0)]
        [TestCase(355.0, 355.0, +5.39739014654622322424e-313, -1.0)]
        [TestCase(355.58, 355.58, +2.57308259096360322163e-309, -1.0)]
        [TestCase(196.0, 16.0, +1.62991625102252204721e-14, -1.00000000000001938715)]
        public void CanComputeComplexCotangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Cot();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can compute complex secant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 1.0, 0.0)]
        [TestCase(8.388608e6, 0.0, -1.1089490624226292, 0.0)]
        [TestCase(-8.388608e6, 0.0, -1.1089490624226292, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.0000000000000071, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, 1.0000000000000071, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, -1.1089490624226177, 6.3367488045143761e-8)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -1.1089490624226177, 6.3367488045143761e-8)]
        [TestCase(512.0, 32.0, -2.52481261731330570134e-14, +2.01407074478744036413e-15)]
        [TestCase(-512.0, -32.0, -2.52481261731330570134e-14, +2.01407074478744036413e-15)]
        [TestCase(32.0, 512.0, +7.30361056703505e-223, +4.82773070945482e-223)]
        [TestCase(32.0, -512.0, +7.30361056703505e-223, -4.82773070945482e-223)]
        [TestCase(355.0, 355.0, -1.33810107564527561878e-154, -4.03361916732891580715e-159)]
        [TestCase(355.58, 355.58, -6.26665948010988663559e-155, -4.10598756663013051681e-155)]
        [TestCase(196.0, 16.0, +7.70790453157574313253e-8, +2.11460357915116320189e-7)]
        public void CanComputeComplexSecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Sec();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can compute complex cosecant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, double.PositiveInfinity, 0.0)]
        [TestCase(8.388608e6, 0.0, 2.3134856195559191, 0.0)]
        [TestCase(-8.388608e6, 0.0, -2.3134856195559191, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 8388608.0000000376, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -8388608.0000000376, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 2.3134856195557596, 5.7534999050657057e-7)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -2.3134856195557596, -5.7534999050657057e-7)]
        [TestCase(512.0, 32.0, 2.01407074478744e-15, 2.52481261731331e-14)]
        [TestCase(-512.0, -32.0, -2.01407074478744e-15, -2.52481261731331e-14)]
        [TestCase(32.0, 512.0, +4.82773070945482e-223, -7.30361056703505e-223)]
        [TestCase(32.0, -512.0, +4.82773070945482e-223, +7.30361056703505e-223)]
        [TestCase(355.0, 355.0, -4.03361916732891580715e-159, +1.33810107564527561878e-154)]
        [TestCase(355.58, 355.58, -4.10598756663013051681e-155, +6.26665948010988663559e-155)]
        [TestCase(196.0, 16.0, +2.11460357915116320189e-7, -7.70790453157574313253e-8)]
        public void CanComputeComplexCosecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Csc();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can compute complex hyperbolic sine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 0.0, 0.0)]
        [TestCase(8.388608e6, 0.0, double.PositiveInfinity, 0.0)]
        [TestCase(-8.388608e6, 0.0, double.NegativeInfinity, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.1920928955078128e-7, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.1920928955078128e-7, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, double.PositiveInfinity, double.PositiveInfinity)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, double.NegativeInfinity, double.NegativeInfinity)]
        [TestCase(0.5, 0.5, 0.45730415318424922, 0.54061268571315335)]
        [TestCase(0.5, -0.5, 0.45730415318424922, -0.54061268571315335)]
        [TestCase(-0.5, 0.5, -0.45730415318424922, 0.54061268571315335)]
        [TestCase(-0.5, -0.5, -0.45730415318424922, -0.54061268571315335)]
        [TestCase(512.0, 32.0, 9.52855589474962752845e221, 6.29843301304522728211e221)]
        [TestCase(-512.0, -32.0, -9.52855589474962752845e221, -6.29843301304522728211e221)]
        [TestCase(-32.0, 512.0, +3.93564576751566e13, +3.13950783826274e12)]
        [TestCase(32.0, -512.0, -3.93564576751566e13, -3.13950783826274e12)]
        [TestCase(709.0, 709.0, +2.22042071181169647963e307, -3.45764185010438140812e307)]
        public void CanComputeComplexHyperbolicSine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Sinh();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex hyperbolic cosine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 1.0, 0.0)]
        [TestCase(8.388608e6, 0.0, double.PositiveInfinity, 0.0)]
        [TestCase(-8.388608e6, 0.0, double.PositiveInfinity, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.0000000000000071, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, 1.0000000000000071, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, double.PositiveInfinity, double.PositiveInfinity)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, double.PositiveInfinity, double.PositiveInfinity)]
        [TestCase(0.5, 0.5, 0.9895848833999199, 0.24982639750046154)]
        [TestCase(0.5, -0.5, 0.9895848833999199, -0.24982639750046154)]
        [TestCase(-0.5, 0.5, 0.9895848833999199, -0.24982639750046154)]
        [TestCase(-0.5, -0.5, 0.9895848833999199, 0.24982639750046154)]
        [TestCase(512.0, 32.0, 9.52855589474962752845e221, 6.29843301304522728211e221)]
        [TestCase(-512.0, -32.0, 9.52855589474962752845e221, 6.29843301304522728211e221)]
        [TestCase(-32.0, 512.0, -3.93564576751566e13, -3.13950783826274e12)]
        [TestCase(32.0, -512.0, -3.93564576751566e13, -3.13950783826274e12)]
        [TestCase(709.0, 709.0, +2.22042071181169647963e307, -3.45764185010438140812e307)]
        public void CanComputeComplexHyperbolicCosine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Cosh();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex hyperbolic tangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 0.0, 0.0)]
        [TestCase(8.388608e6, 0.0, 1.0, 0.0)]
        [TestCase(-8.388608e6, 0.0, -1.0, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.1920928955078043e-7, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.1920928955078043e-7, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.0, 0.0)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -1.0, 0.0)]
        [TestCase(0.5, 0.5, 0.56408314126749848, 0.40389645531602575)]
        [TestCase(0.5, -0.5, 0.56408314126749848, -0.40389645531602575)]
        [TestCase(-0.5, 0.5, -0.56408314126749848, 0.40389645531602575)]
        [TestCase(-0.5, -0.5, -0.56408314126749848, -0.40389645531602575)]
        [TestCase(512.0, 32.0, 1.0, 0.0)]
        [TestCase(-512.0, -32.0, -1.0, 0.0)]
        [TestCase(-32.0, 512.0, -1.0, -5.08515122860093626173e-29)]
        [TestCase(32.0, -512.0, +1.0, +5.08515122860093626173e-29)]
        [TestCase(355.0, 355.0, +1.0, +5.39739014654622322424e-313)]
        [TestCase(355.58, 355.58, +1.0, +2.57308259096360322163e-309)]
        [TestCase(196.0, 16.0, +1.0, +6.29623407730470174659e-171)]
        public void CanComputeComplexHyperbolicTangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Tanh();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex hyperbolic cotangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, double.PositiveInfinity, 0.0)]
        [TestCase(8.388608e6, 0.0, 1.0, 0.0)]
        [TestCase(-8.388608e6, 0.0, -1.0, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 8388608.0000000574, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -8388608.0000000574, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.0, 0.0)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -1.0, 0.0)]
        [TestCase(0.5, 0.5, 1.1719451445243514, -0.8391395790248311)]
        [TestCase(0.5, -0.5, 1.1719451445243514, 0.8391395790248311)]
        [TestCase(-0.5, 0.5, -1.1719451445243514, -0.8391395790248311)]
        [TestCase(-0.5, -0.5, -1.1719451445243514, 0.8391395790248311)]
        [TestCase(512.0, 32.0, 1.0, 0.0)]
        [TestCase(-512.0, -32.0, -1.0, 0.0)]
        [TestCase(-32.0, 512.0, -1.0, 5.08515122860093626173e-29)]
        [TestCase(32.0, -512.0, +1.0, -5.08515122860093626173e-29)]
        [TestCase(355.0, 355.0, +1.0, -5.39739014654622322424e-313)]
        [TestCase(355.58, 355.58, +1.0, -2.57308259096360322163e-309)]
        [TestCase(196.0, 16.0, +1.0, -6.29623407730470174659e-171)]
        public void CanComputeComplexHyperbolicCotangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Coth();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex hyperbolic secant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 1.0, 0.0)]
        [TestCase(8.388608e6, 0.0, 0.0, 0.0)]
        [TestCase(-8.388608e6, 0.0, 0.0, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 0.99999999999999289, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, 0.99999999999999289, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 0.0, 0.0)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -0.0, 0.0)]
        [TestCase(0.5, 0.5, 0.94997886761549463, -0.23982763093808804)]
        [TestCase(0.5, -0.5, 0.94997886761549463, 0.23982763093808804)]
        [TestCase(-0.5, 0.5, 0.94997886761549463, 0.23982763093808804)]
        [TestCase(-0.5, -0.5, 0.94997886761549463, -0.23982763093808804)]
        [TestCase(512.0, 32.0, 7.30361056703505051822e-223, -4.82773070945482080706e-223)]
        [TestCase(-512.0, -32.0, 7.30361056703505051822e-223, -4.82773070945482080706e-223)]
        [TestCase(-32.0, 512.0, -2.52481261731330570134e-14, +2.01407074478744036413e-15)]
        [TestCase(-32.0, -512.0, -2.52481261731330570134e-14, -2.01407074478744036413e-15)]
        [TestCase(355.0, 355.0, -1.33810107564527561878e-154, +4.03361916732891580715e-159)]
        [TestCase(355.58, 355.58, -6.26665948010988663559e-155, +4.10598756663013051681e-155)]
        [TestCase(196.0, 16.0, -1.447180343166980306269e-85, +4.350690711792111509525e-86)]
        public void CanComputeComplexHyperbolicSecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Sech();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex hyperbolic cosecant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, double.PositiveInfinity, 0.0)]
        [TestCase(8.388608e6, 0.0, 0.0, 0.0)]
        [TestCase(-8.388608e6, 0.0, 0.0, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 8388607.9999999978, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -8388607.9999999978, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 0.0, 0.0)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, 0.0, 0.0)]
        [TestCase(0.5, 0.5, 0.91207426403881078, -1.0782296946540223)]
        [TestCase(0.5, -0.5, 0.91207426403881078, 1.0782296946540223)]
        [TestCase(-0.5, 0.5, -0.91207426403881078, -1.0782296946540223)]
        [TestCase(-0.5, -0.5, -0.91207426403881078, 1.0782296946540223)]
        [TestCase(512.0, 32.0, 7.30361056703505051822e-223, -4.82773070945482080706e-223)]
        [TestCase(-512.0, -32.0, -7.30361056703505051822e-223, +4.82773070945482080706e-223)]
        [TestCase(-32.0, 512.0, +2.52481261731330570134e-14, -2.01407074478744036413e-15)]
        [TestCase(32.0, -512.0, -2.52481261731330570134e-14, +2.01407074478744036413e-15)]
        [TestCase(355.0, 355.0, -1.33810107564527561878e-154, +4.03361916732891580715e-159)]
        [TestCase(355.58, 355.58, -6.26665948010988663559e-155, +4.10598756663013051681e-155)]
        [TestCase(196.0, 16.0, -1.447180343166980306269e-85, +4.350690711792111509525e-86)]
        public void CanComputeComplexHyperbolicCosecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Csch();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse sine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 0.0, 0.0)]
        [TestCase(8.388608e6, 0.0, 1.5707963267948966, -16.635532333438682)]
        [TestCase(-8.388608e6, 0.0, -1.5707963267948966, 16.635532333438682)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.1920928955078128e-7, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.1920928955078128e-7, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.5707963267948966, 16.635532333438682)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -1.5707963267948966, -16.635532333438682)]
        [TestCase(0.5, -0.5, 0.4522784471511907, -0.53063753095251787)]
        [TestCase(123400000000d, 0d, 1.57079632679489661923, -26.23184412897764390497)]
        [TestCase(-123400000000d, 0d, -1.57079632679489661923, 26.23184412897764390497)]
        public void CanComputeComplexInverseSine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Asin();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 13);
        }

        /// <summary>
        /// Can compute complex inverse cosine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 1.5707963267948966, 0.0)]
        [TestCase(8.388608e6, 0.0, 0.0, 16.635532333438682)]
        [TestCase(-8.388608e6, 0.0, 3.1415926535897931, -16.635532333438682)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.570796207585607, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, 1.5707964460041861, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.4210854715202073e-14, -16.635532333438682)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, 3.1415926535897789, 16.63553233343868)]
        [TestCase(0.5, -0.5, 1.1185178796437059, 0.53063753095251787)]
        [TestCase(123400000000d, 0d, 0d, 26.23184412897764390497)]
        [TestCase(-123400000000d, 0d, 3.14159265358979323846, -26.23184412897764390497)]
        public void CanComputeComplexInverseCosine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Acos();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse tangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, 0.0, 0.0)]
        [TestCase(8.388608e6, 0.0, 1.570796207585607, 0.0)]
        [TestCase(-8.388608e6, 0.0, -1.570796207585607, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.1920928955078043e-7, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.1920928955078043e-7, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.570796207585607, 0.0)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -1.570796207585607, 0.0)]
        [TestCase(0.5, -0.5, 0.5535743588970452, -0.40235947810852507)]
        public void CanComputeComplexInverseTangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Atan();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse cotangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(0.0, 0.0, Math.PI / 2.0, 0.0)]
        [TestCase(8.388608e6, 0.0, 1.1920928955078069e-7, 0.0)]
        [TestCase(-8.388608e6, 0.0, -1.1920928955078069e-7, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.5707962075856071, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.5707962075856071, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.1920928955078069e-7, -1.6907571720583645e-21)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -1.1920928955078069e-7, 1.6907571720583645e-21)]
        [TestCase(0.5, -0.5, 1.0172219678978514, 0.40235947810852509)]
        public void CanComputeComplexInverseCotangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Acot();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse secant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(8.388608e6, 0.0, 1.5707962075856071, 0.0)]
        [TestCase(-8.388608e6, 0.0, 1.5707964460041862, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 0.0, 16.635532333438686)]
        [TestCase(-1.19209289550780998537e-7, 0.0, 3.1415926535897932, -16.635532333438686)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.5707962075856071, 1.6940658945086007e-21)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, 1.5707964460041862, -1.6940658945086007e-21)]
        [TestCase(0.5, -0.5, 0.90455689430238136, -1.0612750619050357)]
        public void CanComputeComplexInverseSecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Asec();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse cosecant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(8.388608e6, 0.0, 1.1920928955078153e-7, 0.0)]
        [TestCase(-8.388608e6, 0.0, -1.1920928955078153e-7, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.5707963267948966, -16.635532333438686)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.5707963267948966, 16.635532333438686)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.1920928955078153e-7, -1.6940658945086007e-21)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -1.1920928955078153e-7, 1.6940658945086007e-21)]
        [TestCase(0.5, -0.5, 0.66623943249251526, 1.0612750619050357)]
        public void CanComputeComplexInverseCosecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Acsc();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic sine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(8.388608e6, 0.0, 16.63553233343869, 0.0)]
        [TestCase(-8.388608e6, 0.0, -16.63553233343869, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.1920928955078072e-7, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.1920928955078072e-7, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 16.63553233343869, 1.4210854715201873e-14)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -16.63553233343869, -1.4210854715201873e-14)]
        [TestCase(0.5, -0.5, 0.53063753095251787, -0.4522784471511907)]
        public void CanComputeComplexInverseHyperbolicSine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Asinh();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 12);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic cosine.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(8.388608e6, 0.0, 16.635532333438682, 0.0)]
        [TestCase(-8.388608e6, 0.0, 16.635532333438682, 3.1415926535897931)]
        [TestCase(1.19209289550780998537e-7, 0.0, 0.0, 1.570796207585607)]
        [TestCase(-1.19209289550780998537e-7, 0.0, 0.0, 1.5707964460041861)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 16.635532333438682, 1.4210854715202073e-14)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, 16.635532333438682, -3.1415926535897789)]
        [TestCase(0.5, -0.5, 0.53063753095251787, -1.1185178796437059)]
        public void CanComputeComplexInverseHyperbolicCosine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Acosh();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic tangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(8.388608e6, 0.0, 1.1920928955078125e-7, -1.5707963267948966)]
        [TestCase(-8.388608e6, 0.0, -1.1920928955078125e-7, 1.5707963267948966)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.1920928955078157e-7, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.1920928955078157e-7, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.1920928955078125e-7, 1.5707963267948966)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -1.1920928955078125e-7, -1.5707963267948966)]
        [TestCase(0.5, -0.5, 0.40235947810852509, -0.55357435889704525)]
        public void CanComputeComplexInverseHyperbolicTangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Atanh();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic cotangent.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(8.388608e6, 0.0, 1.1920928955078181e-7, 0.0)]
        [TestCase(-8.388608e6, 0.0, -1.1920928955078181e-7, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 1.1920928955078157e-7, -1.5707963267948966)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -1.1920928955078157e-7, 1.5707963267948966)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.1920928955078181e-7, -1.6940658945086212e-21)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -1.1920928955078181e-7, 1.6940658945086212e-21)]
        [TestCase(0.5, -0.5, 0.40235947810852509, 1.0172219678978514)]
        public void CanComputeComplexInverseHyperbolicCotangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Acoth();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic secant.
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(8.388608e6, 0.0, 0.0, 1.5707962075856071)]
        [TestCase(-8.388608e6, 0.0, 0.0, 1.5707964460041862)]
        [TestCase(1.19209289550780998537e-7, 0.0, 16.635532333438686, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, 16.635532333438686, 3.1415926535897932)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.6940658945086007e-21, -1.5707962075856071)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, 1.6940658945086007e-21, 1.5707964460041862)]
        [TestCase(0.5, -0.5, 1.0612750619050357, 0.90455689430238136)]
        public void CanComputeComplexInverseHyperbolicSecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Asech();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 14);
        }

        /// <summary>
        /// Can compute complex inverse hyperbolic cosecant
        /// </summary>
        /// <param name="real">Input complex real part.</param>
        /// <param name="imag">Input complex imaginary part.</param>
        /// <param name="expectedReal">Expected complex real part.</param>
        /// <param name="expectedImag">Expected complex imaginary part.</param>
        [TestCase(8.388608e6, 0.0, 1.1920928955078097e-7, 0.0)]
        [TestCase(-8.388608e6, 0.0, -1.1920928955078097e-7, 0.0)]
        [TestCase(1.19209289550780998537e-7, 0.0, 16.635532333438693, 0.0)]
        [TestCase(-1.19209289550780998537e-7, 0.0, -16.635532333438693, 0.0)]
        [TestCase(8.388608e6, 1.19209289550780998537e-7, 1.1920928955078076e-7, -1.6940658945085851e-21)]
        [TestCase(-8.388608e6, -1.19209289550780998537e-7, -1.1920928955078076e-7, 1.6940658945085851e-21)]
        [TestCase(0.5, -0.5, 1.0612750619050357, 0.66623943249251526)]
        public void CanComputeComplexInverseHyperbolicCosecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Acsch();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqualRelative(expected, actual, 13);
        }
    }
}
