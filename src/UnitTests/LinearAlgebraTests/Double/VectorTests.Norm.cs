using MbUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    public abstract partial class VectorTests
    {
        [Test]
        public void CanComputeNorm()
        {
            var vector = CreateVector(_data);
            AssertHelpers.AlmostEqual(7.416198487095663, vector.Norm(), 15);
        }

        [Test]
        public void CanComputeNorm1()
        {
            var vector = CreateVector(_data);
            AssertHelpers.AlmostEqual(15.0, vector.Norm1(), 15);
        }

        [Test]
        public void CanComputeSquareNorm()
        {
            var vector = CreateVector(_data);
            AssertHelpers.AlmostEqual(55.0, vector.SquaredNorm(), 15);
        }

        [Test]
        [Row(1, 15.0)]
        [Row(2, 7.416198487095663)]
        [Row(3, 6.0822019955734001)]
        [Row(10, 5.0540557845353753)]
        public void CanComputeNormP(int p, double expected)
        {
            var vector = CreateVector(_data);
            AssertHelpers.AlmostEqual(expected, vector.NormP(p), 15);
        }
        
        [Test]
        public void CanComputeNormInfinity()
        {
            var vector = CreateVector(_data);
            AssertHelpers.AlmostEqual(5.0, vector.NormInfinity(), 15);
        }

        [Test]
        [MultipleAsserts]
        public void CanNormalizeVector()
        {
            var vector = CreateVector(_data);
            var result = vector.Normalize();
            AssertHelpers.AlmostEqual(0.134839972492648, result[0], 14);
            AssertHelpers.AlmostEqual(0.269679944985297, result[1], 14);
            AssertHelpers.AlmostEqual(0.404519917477945, result[2], 14);
            AssertHelpers.AlmostEqual(0.539359889970594, result[3], 14);
            AssertHelpers.AlmostEqual(0.674199862463242, result[4], 14);
        }
    }
}