namespace MathNet.Numerics.UnitTests.QuaternionsTests
{
    using System.Security.Cryptography.X509Certificates;
    using NUnit.Framework;
    /// <summary>
    /// Complex32 tests.
    /// </summary>
    /// <remarks>
    /// Tests are based on http://web.cs.iastate.edu/~cs577/handouts/quaternion.pdf and my own calculations
    /// </remarks>
    [TestFixture]
    public class Quaternions32Tests
    {
        /// <summary>
        /// Can add a quaternions using operator.
        /// </summary>
        [Test]
        public void CanAddQuaternionsUsingOperator()
        {
            Assert.AreEqual(new Quaternion32(1, 2, 3, 4) + new Quaternion32(1, 2, 3, 4), new Quaternion32(2, 4, 6, 8));
            Assert.AreEqual(new Quaternion32(0, 0, 0, 0) + new Quaternion32(1, 2, 3, 4), new Quaternion32(1, 2, 3, 4));
            Assert.AreEqual(new Quaternion32(0, 0, 0, 0) + Quaternion32.Zero, Quaternion32.Zero);
            Assert.AreEqual(new Quaternion32(0, 0, 0, 0) + Quaternion32.One, Quaternion32.One);
        }
        /// <summary>
        /// Can substract quaternions using operator.
        /// </summary>
        [Test]
        public void CanSubstractQuaternionsUsingOperator()
        {
            Assert.AreEqual(Quaternion32.Zero,new Quaternion32(1, 2, 3, 4) - new Quaternion32(1, 2, 3, 4));
            Assert.AreEqual(new Quaternion32(-1, -2, -3, -4),new Quaternion32(0, 0, 0, 0) - new Quaternion32(1, 2, 3, 4));
            Assert.AreEqual(Quaternion32.Zero,new Quaternion32(0, 0, 0, 0) - Quaternion32.Zero);
            //Assert.AreEqual(new Quaternion32(0, 0, 0, 0) - Quaternion32.One, -Quaternion32.One);
        }

        [Test]
        public void CanMultiplyQuaternionsUsingOperator()
        {
            Assert.AreEqual(new Quaternion32(8, -9, -2, 11), new Quaternion32(3, 1, -2, 1) * new Quaternion32(2, -1, 2, 3));
        }

        [Test]
        public void CanCalculateDotProduct()
        {
            Assert.AreEqual(new Quaternion32(0, -8, -4, 0),Quaternion32.DotProduct(new Quaternion32(3, 1, -2, 1), new Quaternion32(2, -1, 2, 3)));
        }
    }
}
