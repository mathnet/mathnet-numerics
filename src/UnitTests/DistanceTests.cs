using NUnit.Framework;

namespace MathNet.Numerics.UnitTests
{
    [TestFixture]
    public class DistanceTests
    {
        [Test]
        public void Hamming()
        {
            Assert.That(Distance.Hamming(new[] { 0.0, 0.0 }, new[] { 0.0, 0.0 }), Is.EqualTo(0.0));
            Assert.That(Distance.Hamming(new[] { 1.0, 0.0 }, new[] { 1.0, 0.0 }), Is.EqualTo(0.0));
            Assert.That(Distance.Hamming(new[] { 0.0, 0.0 }, new[] { 1.0, 0.0 }), Is.EqualTo(1.0));
            Assert.That(Distance.Hamming(new[] { 0.0, 0.0 }, new[] { 0.0, 1.0 }), Is.EqualTo(1.0));
            Assert.That(Distance.Hamming(new[] { 0.0, 0.0 }, new[] { 1.0, 1.0 }), Is.EqualTo(2.0));
            Assert.That(Distance.Hamming(new[] { 1.0, 0.0 }, new[] { 0.0, 1.0 }), Is.EqualTo(2.0));
        }
    }
}
