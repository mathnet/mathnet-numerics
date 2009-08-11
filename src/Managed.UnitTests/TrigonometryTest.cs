namespace MathNet.Numerics.UnitTests
{
    using System;

    using MbUnit.Framework;

    [TestFixture]
    public class TrigonometryTest
    {
        [Test]
        [Row(0.0, 0.0, 1.0, 0.0)]
        [Row(8.388608e6, 0.0, -0.90175467375875928, 0.0)]
        [Row(-8.388608e6, 0.0, -0.90175467375875928, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 0.99999999999999289, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, 0.99999999999999289, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, -0.90175467375876572, -5.1528001100635277e-8)]
        [Row(-1.19209289550780998537e-7, -8.388608e6, double.PositiveInfinity, double.NegativeInfinity)]
        public void CanComputeComplexCosine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Cosine();
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
        public void CanComputeComplexSine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Sine();
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
        public void CanComputeComplexTangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Tangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        [Test]
        [Row(0.0, double.PositiveInfinity)]
        [Row(8388608, 2.3134856195559191)]
        [Row(1.19209289550780998537e-7, 8388608.0000000376)]
        [Row(-8388608, -2.3134856195559191)]
        [Row(-1.19209289550780998537e-7, -8388608.0000000376)]
        public void CanComputeCosecant(double value, double expected)
        {
            var actual = Trig.Cosecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        [Test]
        [Row(0.0, 1.0)]
        [Row(8388608, -0.90175467375875928)]
        [Row(1.19209289550780998537e-7, 0.99999999999999289)]
        [Row(-8388608, -0.90175467375875928)]
        [Row(-1.19209289550780998537e-7, 0.99999999999999289)]
        public void CanComputeCosine(double value, double expected)
        {
            var actual = Trig.Cosine(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, double.PositiveInfinity)]
        [Row(8388608, -2.086196470108229)]
        [Row(1.19209289550780998537e-7, 8388607.999999978)]
        [Row(-8388608, 2.086196470108229)]
        [Row(-1.19209289550780998537e-7, -8388607.999999978)]
        public void CanComputeCotangent(double value, double expected)
        {
            var actual = Trig.Cotangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        [Test]
        [Row(0.0, double.PositiveInfinity)]
        [Row(8388608, 1.3670377960148449e-3643126)]
        [Row(1.19209289550780998537e-7, 8388607.9999999978)]
        [Row(-8388608, -1.3670377960148449e-3643126)]
        [Row(-1.19209289550780998537e-7, -8388607.9999999978)]
        public void CanComputeHyperbolicCosecant(double value, double expected)
        {
            var actual = Trig.HyperbolicCosecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        [Test]
        [Row(0.0, 1.0)]
        [Row(8388608, double.PositiveInfinity)]
        [Row(1.19209289550780998537e-7, 1.0000000000000071)]
        [Row(-8388608, double.PositiveInfinity)]
        [Row(-1.19209289550780998537e-7, 1.0000000000000071)]
        public void CanComputeHyperbolicCosine(double value, double expected)
        {
            var actual = Trig.HyperbolicCosine(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        [Test]
        [Row(0.0, double.PositiveInfinity)]
        [Row(8388608, 1.0)]
        [Row(1.19209289550780998537e-7, 8388608.0000000574)]
        [Row(-8388608, -1.0)]
        [Row(-1.19209289550780998537e-7, -8388608.0000000574)]
        public void CanComputeHyperbolicCotangent(double value, double expected)
        {
            var actual = Trig.HyperbolicCotangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        [Test]
        [Row(0.0, 1.0)]
        [Row(8388608, 1.3670377960148449e-3643126)]
        [Row(1.19209289550780998537e-7, 0.99999999999999289)]
        [Row(-8388608, 1.3670377960148449e-3643126)]
        [Row(-1.19209289550780998537e-7, 0.99999999999999289)]
        public void CanComputeHyperbolicSecant(double value, double expected)
        {
            var actual = Trig.HyperbolicSecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        [Test]
        [Row(8388608, double.PositiveInfinity)]
        [Row(1.19209289550780998537e-7, 1.1920928955078128e-7)]
        [Row(-8388608, double.NegativeInfinity)]
        [Row(-1.19209289550780998537e-7, -1.1920928955078128e-7)]
        public void CanComputeHyperbolicSine(double value, double expected)
        {
            var actual = Trig.HyperbolicSine(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(8388608, 1.0)]
        [Row(1.19209289550780998537e-7, 1.1920928955078043e-7)]
        [Row(-8388608, -1.0)]
        [Row(-1.19209289550780998537e-7, -1.1920928955078043e-7)]
        public void CanComputeHyperbolicTangent(double value, double expected)
        {
            var actual = Trig.HyperbolicTangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        [Test]
        [Row(8388608, 1.1920928955078097e-7)]
        [Row(-8388608, -1.1920928955078097e-7)]
        [Row(1, 1.5707963267948966)]
        [Row(-1, -1.5707963267948966)]
        public void CanComputeInverseCosecant(double value, double expected)
        {
            var actual = Trig.InverseCosecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(1, 0)]
        [Row(-1, 3.1415926535897931)]
        [Row(1.19209289550780998537e-7, 1.570796207585607)]
        [Row(-1.19209289550780998537e-7, 1.5707964460041861)]
        public void CanComputeInverseCosine(double value, double expected)
        {
            var actual = Trig.InverseCosine(value);
            AssertHelpers.AlmostEqual(expected, actual, 15);
        }

        [Test]
        [Row(0.0, 1.5707963267948966)]
        [Row(8388608, 1.1920928955078069e-7)]
        [Row(-8388608, -1.1920928955078069e-7)]
        [Row(1.19209289550780998537e-7, 1.5707962075856071)]
        [Row(-1.19209289550780998537e-7, -1.5707962075856071)]
        public void CanComputeInverseCotangent(double value, double expected)
        {
            var actual = Trig.InverseCotangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, double.PositiveInfinity)]
        [Row(8388608, 1.1920928955078097e-7)]
        [Row(-8388608, -1.1920928955078097e-7)]
        [Row(1.19209289550780998537e-7, 16.635532333438693)]
        [Row(-1.19209289550780998537e-7, -16.635532333438693)]
        public void CanComputeInverseHyperbolicCosecant(double value, double expected)
        {
            var actual = Trig.InverseHyperbolicCosecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(1.0, 0.0)]
        [Row(8388608, 16.635532333438682)]
        public void CanComputeInverseHyperbolicCosine(double value, double expected)
        {
            var actual = Trig.InverseHyperbolicCosine(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(8388608, 1.1920928955078181e-7)]
        [Row(-8388608, -1.1920928955078181e-7)]
        [Row(1, double.PositiveInfinity)]
        [Row(-1, double.NegativeInfinity)]
        public void CanComputeInverseHyperbolicCotangent(double value, double expected)
        {
            var actual = Trig.InverseHyperbolicCotangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0, double.PositiveInfinity)]
        [Row(.5, 1.3169578969248167)]
        [Row(1, 0.0)]
        public void CanComputeInverseHyperbolicSecant(double value, double expected)
        {
            var actual = Trig.InverseHyperbolicSecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(8388608, 16.63553233343869)]
        [Row(-8388608, -16.63553233343869)]
        [Row(1.19209289550780998537e-7, 1.1920928955078072e-7)]
        [Row(-1.19209289550780998537e-7, -1.1920928955078072e-7)]
        public void CanComputeInverseHyperbolicSine(double value, double expected)
        {
            var actual = Trig.InverseHyperbolicSine(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(1.0, double.PositiveInfinity)]
        [Row(-1.0, double.NegativeInfinity)]
        [Row(1.19209289550780998537e-7, 1.19209289550780998537e-7)]
        [Row(-1.19209289550780998537e-7, -1.19209289550780998537e-7)]
        public void CanComputeInverseHyperbolicTangent(double value, double expected)
        {
            var actual = Trig.InverseHyperbolicTangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(8388608, 1.5707962075856071)]
        [Row(-8388608, 1.5707964460041862)]
        [Row(1.0, 0.0)]
        [Row(-1.0, 3.1415926535897932)]
        public void CanComputeInverseSecant(double value, double expected)
        {
            var actual = Trig.InverseSecant(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(1.0, 1.5707963267948966)]
        [Row(-1.0, -1.5707963267948966)]
        [Row(1.19209289550780998537e-7, 1.1920928955078128e-7)]
        [Row(-1.19209289550780998537e-7, -1.1920928955078128e-7)]
        public void CanComputeInverseSine(double value, double expected)
        {
            var actual = Trig.InverseSine(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(8388608, 1.570796207585607)]
        [Row(-8388608, -1.570796207585607)]
        [Row(1.19209289550780998537e-7, 1.19209289550780998537e-7)]
        [Row(-1.19209289550780998537e-7, -1.19209289550780998537e-7)]
        public void CanComputeInverseTangent(double value, double expected)
        {
            var actual = Trig.InverseTangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 1.0)]
        [Row(8388608, -1.1089490624226292)]
        [Row(1.19209289550780998537e-7, 1.0000000000000071)]
        [Row(-8388608, -1.1089490624226292)]
        [Row(-1.19209289550780998537e-7, 1.0000000000000071)]
        public void CanComputeSecant(double value, double expected)
        {
            var actual = Trig.Secant(value);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(8388608, 0.43224820225679778)]
        [Row(-8388608, -0.43224820225679778)]
        [Row(1.19209289550780998537e-7, 1.1920928955078072e-7)]
        [Row(-1.19209289550780998537e-7, -1.1920928955078072e-7)]
        public void CanComputeSine(double value, double expected)
        {
            var actual = Trig.Sine(value);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(8388608, -0.47934123862654288)]
        [Row(-8388608, 0.47934123862654288)]
        [Row(1.19209289550780998537e-7, 1.1920928955078157e-7)]
        [Row(-1.19209289550780998537e-7, -1.1920928955078157e-7)]
        public void CanComputeTangent(double value, double expected)
        {
            var actual = Trig.Tangent(value);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        [Test]
        public void CanConvertDegreeToGrad()
        {
            AssertHelpers.AlmostEqual(90 / .9, Trig.DegreeToGrad(90), 15);
        }

        [Test]
        public void CanConvertDegreeToRadian()
        {
            AssertHelpers.AlmostEqual(Math.PI / 2, Trig.DegreeToRadian(90), 15);
        }

        [Test]
        public void CanConvertGradToDegree()
        {
            AssertHelpers.AlmostEqual(180, Trig.GradToDegree(200), 15);
        }

        [Test]
        public void CanConvertGradToRadian()
        {
            AssertHelpers.AlmostEqual(Math.PI, Trig.GradToRadian(200), 15);
        }

        [Test]
        public void CanConvertRadianToDegree()
        {
            AssertHelpers.AlmostEqual(60.0, Trig.RadianToDegree(Math.PI / 3.0), 15);
        }

        [Test]
        public void CanConvertRadianToGrad()
        {
            AssertHelpers.AlmostEqual(200.0 / 3.0, Trig.RadianToGrad(Math.PI / 3.0), 15);
        }

        [Test]
        [Row(0.0, 0.0, double.PositiveInfinity, 0.0)]
        [Row(8.388608e6, 0.0, -2.086196470108229, 0.0)]
        [Row(-8.388608e6, 0.0, 2.086196470108229, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 8388607.999999978, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -8388607.999999978, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, -2.0861964701080704, -6.3803383253713457e-7)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, 2.0861964701080704, 6.3803383253713457e-7)]
        public void CanComputeComplexCotangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Cotangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        [Test]
        [Row(0.0, 0.0, 1.0, 0.0)]
        [Row(8.388608e6, 0.0, -1.1089490624226292, 0.0)]
        [Row(-8.388608e6, 0.0, -1.1089490624226292, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.0000000000000071, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, 1.0000000000000071, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, -1.1089490624226177, 6.3367488045143761e-8)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.1089490624226177, 6.3367488045143761e-8)]
        public void CanComputeComplexSecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Secant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 13);
        }

        [Test]
        [Row(0.0, 0.0, double.PositiveInfinity, 0.0)]
        [Row(8.388608e6, 0.0, 2.3134856195559191, 0.0)]
        [Row(-8.388608e6, 0.0, -2.3134856195559191, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 8388608.0000000376, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -8388608.0000000376, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 2.3134856195557596, 5.7534999050657057e-7)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -2.3134856195557596, -5.7534999050657057e-7)]
        public void CanComputeComplexCosecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).Cosecant();
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
        public void CanComputeComplexHyperbolicSine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicSine();
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
        public void CanComputeComplexHyperbolicCosine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicCosine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, 0.0, 0.0)]
        [Row(8.388608e6, 0.0, 1.0, 0.0)]
        [Row(-8.388608e6, 0.0, -1.0, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078043e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078043e-7, 0.0)]
        //[Row(8.388608e6, 1.19209289550780998537e-7, 1.0, 0.0)]
        //[Row(-8.388608e6, -1.19209289550780998537e-7, -1.0, 0.0)]
        [Row(0.5, -0.5, 0.56408314126749848, -0.40389645531602575)]
        public void CanComputeComplexHyperbolicTangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicTangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, double.PositiveInfinity, 0.0)]
        [Row(8.388608e6, 0.0, 1.0, 0.0)]
        [Row(-8.388608e6, 0.0, -1.0, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 8388608.0000000574, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -8388608.0000000574, 0.0)]
//        [Row(8.388608e6, 1.19209289550780998537e-7, 1.0, 0.0)]
        //[Row(-8.388608e6, -1.19209289550780998537e-7, -1.0, 0.0)]
        [Row(0.5, -0.5, 1.1719451445243514, -0.8391395790248311)]
        public void CanComputeComplexHyperbolicCotangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicCotangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, 1.0, 0.0)]
        [Row(8.388608e6, 0.0, 0.0, 0.0)]
        [Row(-8.388608e6, 0.0, 0.0, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 0.99999999999999289, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, 0.99999999999999289, 0.0)]
//        [Row(8.388608e6, 1.19209289550780998537e-7, 0.0, 0.0)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -0.0, 0.0)]
        [Row(0.5, -0.5, 0.94997886761549463, 0.23982763093808804)]
        public void CanComputeComplexHyperbolicSecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicSecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, double.PositiveInfinity, 0.0)]
        [Row(8.388608e6, 0.0, 0.0, 0.0)]
        [Row(-8.388608e6, 0.0, 0.0, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 8388607.9999999978, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -8388607.9999999978, 0.0)]
//        [Row(8.388608e6, 1.19209289550780998537e-7, 0.0, 0.0)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, 0.0, 0.0)]
        [Row(0.5, -0.5, 0.91207426403881078, 1.0782296946540223)]
        public void CanComputeComplexHyperbolicCosecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).HyperbolicCosecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0,0.0, 0.0 )]
        [Row(8.388608e6, 0.0, 1.5707963267948966, -16.635532333438682)]
        [Row(-8.388608e6, 0.0, -1.5707963267948966, 16.635532333438682)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078128e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078128e-7, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.5707963267948966, 16.635532333438682)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.5707963267948966, -16.635532333438682)]
        [Row(0.5, -0.5, 0.4522784471511907, -0.53063753095251787)]
        public void CanComputeComplexInverseSine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseSine();
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
        [Row(-8.388608e6, -1.19209289550780998537e-7, 3.1415926535897789,16.63553233343868)]
        [Row(0.5, -0.5, 1.1185178796437059,0.53063753095251787)]
        public void CanComputeComplexInverseCosine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseCosine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, 0.0, 0.0)]
        [Row(8.388608e6, 0.0, 1.570796207585607, 0.0 )]
        [Row(-8.388608e6, 0.0, -1.570796207585607,0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078043e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078043e-7, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.570796207585607, 0.0)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.570796207585607, 0.0)]
        [Row(0.5, -0.5, 0.5535743588970452, -0.40235947810852507)]
        public void CanComputeComplexInverseTangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseTangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(0.0, 0.0, Math.PI/2.0, 0.0)]
        [Row(8.388608e6, 0.0, 1.1920928955078069e-7, 0.0)]
        [Row(-8.388608e6, 0.0, -1.1920928955078069e-7, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.5707962075856071, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.5707962075856071, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.1920928955078069e-7, -1.6907571720583645e-21)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.1920928955078069e-7, 1.6907571720583645e-21)]
        [Row(0.5, -0.5, 1.0172219678978514, 0.40235947810852509)]
         public void CanComputeComplexInverseCotangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseCotangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(8.388608e6, 0.0, 1.5707962075856071, 0.0)]
        [Row(-8.388608e6, 0.0, 1.5707964460041862, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 0.0, 16.635532333438686)]
        [Row(-1.19209289550780998537e-7, 0.0, 3.1415926535897932, -16.635532333438686)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.5707962075856071, 1.6940658945086007e-21)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, 1.5707964460041862, -1.6940658945086007e-21)]
        [Row(0.5, -0.5, 0.90455689430238136,-1.0612750619050357)]
        public void CanComputeComplexInverseSecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseSecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(8.388608e6, 0.0, 1.1920928955078153e-7, 0.0)]
        [Row(-8.388608e6, 0.0, -1.1920928955078153e-7, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.5707963267948966, -16.635532333438686)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.5707963267948966, 16.635532333438686)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.1920928955078153e-7, -1.6940658945086007e-21)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.1920928955078153e-7, 1.6940658945086007e-21)]
        [Row(0.5, -0.5, 0.66623943249251526, 1.0612750619050357)]
        public void CanComputeComplexInverseCosecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseCosecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(8.388608e6, 0.0, 16.63553233343869, 0.0)]
        [Row(-8.388608e6, 0.0, -16.63553233343869, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078072e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078072e-7, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 16.63553233343869, 1.4210854715201873e-14)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -16.63553233343869, -1.4210854715201873e-14)]
        [Row(0.5, -0.5, 0.53063753095251787, -0.4522784471511907)]
        public void CanComputeComplexInverseHyperbolicSine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicSine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(8.388608e6, 0.0, 16.635532333438682, 0.0)]
        [Row(-8.388608e6, 0.0, 16.635532333438682, 3.1415926535897931)]
        [Row(1.19209289550780998537e-7, 0.0, 0.0, 1.570796207585607)]
        [Row(-1.19209289550780998537e-7, 0.0, 0.0, 1.5707964460041861)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 16.635532333438682, 1.4210854715202073e-14)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, 16.635532333438682, -3.1415926535897789)]
        [Row(0.5, -0.5, 0.53063753095251787, -1.1185178796437059)]
        public void CanComputeComplexInverseHyperbolicCosine(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicCosine();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(8.388608e6, 0.0, 1.1920928955078125e-7, -1.5707963267948966)]
        [Row(-8.388608e6, 0.0, -1.1920928955078125e-7, 1.5707963267948966)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078157e-7, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078157e-7, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.1920928955078125e-7, 1.5707963267948966)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.1920928955078125e-7, -1.5707963267948966)]
        [Row(0.5, -0.5, 0.40235947810852509, -0.55357435889704525)]
        public void CanComputeComplexInverseHyperbolicTangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicTangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(8.388608e6, 0.0, 1.1920928955078181e-7, 0.0)]
        [Row(-8.388608e6, 0.0, -1.1920928955078181e-7, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 1.1920928955078157e-7, -1.5707963267948966)]
        [Row(-1.19209289550780998537e-7, 0.0, -1.1920928955078157e-7, 1.5707963267948966)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.1920928955078181e-7, -1.6940658945086212e-21)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.1920928955078181e-7, 1.6940658945086212e-21)]
        [Row(0.5, -0.5, 0.40235947810852509, 1.0172219678978514)]
        public void CanComputeComplexInverseHyperbolicCotangent(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicCotangent();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(8.388608e6, 0.0, 0.0, 1.5707962075856071)]
        [Row(-8.388608e6, 0.0, 0.0, 1.5707964460041862)]
        [Row(1.19209289550780998537e-7, 0.0, 16.635532333438686, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, 16.635532333438686, 3.1415926535897932)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.6940658945086007e-21, -1.5707962075856071)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, 1.6940658945086007e-21, 1.5707964460041862)]
        [Row(0.5, -0.5, 1.0612750619050357, 0.90455689430238136)]
        public void CanComputeComplexInverseHyperbolicSecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicSecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }

        [Test]
        [Row(8.388608e6, 0.0, 1.1920928955078097e-7, 0.0)]
        [Row(-8.388608e6, 0.0, -1.1920928955078097e-7, 0.0)]
        [Row(1.19209289550780998537e-7, 0.0, 16.635532333438693, 0.0)]
        [Row(-1.19209289550780998537e-7, 0.0, -16.635532333438693, 0.0)]
        [Row(8.388608e6, 1.19209289550780998537e-7, 1.1920928955078076e-7, -1.6940658945085851e-21)]
        [Row(-8.388608e6, -1.19209289550780998537e-7, -1.1920928955078076e-7, 1.6940658945085851e-21)]
        [Row(0.5, -0.5, 1.0612750619050357, 0.66623943249251526)]
        public void CanComputeComplexInverseHyperbolicCosecant(double real, double imag, double expectedReal, double expectedImag)
        {
            var actual = new Complex(real, imag).InverseHyperbolicCosecant();
            var expected = new Complex(expectedReal, expectedImag);
            AssertHelpers.AlmostEqual(expected, actual, 14);
        }
    }
}