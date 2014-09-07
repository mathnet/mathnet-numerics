using NUnit.Framework;
using System;

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

        [Test]
        public void Jaccard_Double()
        {
            double[] p0 = new double[] { 1, 0.5 };
            double[] q0 = new double[] { 0.5, 1 };

            double[] p1 = new double[] { 4.5, 1 };
            double[] q1 = new double[] { 4, 2 };

            double[] p2 = new double[] { 0, 0, 0 };
            double[] q2 = new double[] { 0, 0, 0 };

            double[] p3 = new double[] { 1, 1, 1 };
            double[] q3 = new double[] { 1, 1, 1 };

            double[] p4 = new double[] { 2.5, 3.5, 3.0, 3.5, 2.5, 3.0 };
            double[] q4 = new double[] { 3.0, 3.5, 1.5, 5.0, 3.5, 3.0 };

            double[] p5 = new double[] { 1, 3, 5, 6, 8, 9, 6, 4, 3, 2 };
            double[] q5 = new double[] { 2, 5, 6, 6, 7, 7, 5, 3, 1, 1 };

            Assert.Throws<ArgumentException>(() => Distance.Jaccard(p0, q4));
            Assert.Throws<ArgumentNullException>(() => Distance.Jaccard(null, q4));
            Assert.Throws<ArgumentNullException>(() => Distance.Jaccard(p0, null));

            Assert.That(Distance.Jaccard(p0, q0), Is.EqualTo(1));
            Assert.That(Distance.Jaccard(p1, q1), Is.EqualTo(1));
            Assert.That(Distance.Jaccard(p2, q2), Is.EqualTo(Double.NaN));
            Assert.That(Distance.Jaccard(p3, q3), Is.EqualTo(0));
            Assert.That(Distance.Jaccard(p4, q4), Is.EqualTo(0.66666).Within(0.00001));
            Assert.That(Distance.Jaccard(p5, q5), Is.EqualTo(0.9).Within(0.1));
        }

        [Test]
        public void Jaccard_Float()
        {
            float[] p0 = new float[] { 1, 0.5f };
            float[] q0 = new float[] { 0.5f, 1 };

            float[] p1 = new float[] { 4.5f, 1 };
            float[] q1 = new float[] { 4, 2 };

            float[] p2 = new float[] { 0, 0, 0 };
            float[] q2 = new float[] { 0, 0, 0 };

            float[] p3 = new float[] { 1, 1, 1 };
            float[] q3 = new float[] { 1, 1, 1 };

            float[] p4 = new float[] { 2.5f, 3.5f, 3.0f, 3.5f, 2.5f, 3.0f };
            float[] q4 = new float[] { 3.0f, 3.5f, 1.5f, 5.0f, 3.5f, 3.0f };

            float[] p5 = new float[] { 1, 3, 5, 6, 8, 9, 6, 4, 3, 2 };
            float[] q5 = new float[] { 2, 5, 6, 6, 7, 7, 5, 3, 1, 1 };

            Assert.Throws<ArgumentException>(() => Distance.Jaccard(p0, q4));
            Assert.Throws<ArgumentNullException>(() => Distance.Jaccard(null, q4));
            Assert.Throws<ArgumentNullException>(() => Distance.Jaccard(p0, null));

            Assert.That(Distance.Jaccard(p0, q0), Is.EqualTo(1));
            Assert.That(Distance.Jaccard(p1, q1), Is.EqualTo(1));
            Assert.That(Distance.Jaccard(p2, q2), Is.EqualTo(float.NaN));
            Assert.That(Distance.Jaccard(p3, q3), Is.EqualTo(0));
            Assert.That(Distance.Jaccard(p4, q4), Is.EqualTo(0.66666).Within(0.00001));
            Assert.That(Distance.Jaccard(p5, q5), Is.EqualTo(0.9).Within(0.1));
        }
    }
}
