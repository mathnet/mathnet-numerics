namespace MathNet.Numerics.UnitTests.QuaternionsTests
{
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
        //TODO : dotproduct, equality operator,infs,nans,tostring,norm,conjugation
        /// <summary>
        /// Can add a quaternions using operator.
        /// </summary>
        [Test]
        public void CanAddQuaternionsAndFloatUsingOperator()
        {
            Assert.AreEqual(new Quaternion32(1, 2, 3, 4) + new Quaternion32(1, 2, 3, 4), new Quaternion32(2, 4, 6, 8));
            Assert.AreEqual(new Quaternion32(0, 0, 0, 0) + new Quaternion32(1, 2, 3, 4), new Quaternion32(1, 2, 3, 4));
            Assert.AreEqual(1 + new Quaternion32(1, 2, 3, 4), new Quaternion32(2, 2, 3, 4));
            Assert.AreEqual(new Quaternion32(1, 2, 3, 4) + 1, new Quaternion32(2, 2, 3, 4));
            Assert.AreEqual(new Quaternion32(0, 0, 0, 0) + Quaternion32.Zero, Quaternion32.Zero);
            Assert.AreEqual(new Quaternion32(0, 0, 0, 0) + Quaternion32.One, Quaternion32.One);
        }
        /// <summary>
        /// Can substract quaternions using operator.
        /// </summary>
        [Test]
        public void CanSubstractQuaternionsAndFloatUsingOperator()
        {
            Assert.AreEqual(Quaternion32.Zero, new Quaternion32(1, 2, 3, 4) - new Quaternion32(1, 2, 3, 4));
            Assert.AreEqual(new Quaternion32(-1, -2, -3, -4), new Quaternion32(0, 0, 0, 0) - new Quaternion32(1, 2, 3, 4));
            Assert.AreEqual(Quaternion32.Zero, new Quaternion32(0, 0, 0, 0) - Quaternion32.Zero);
            Assert.AreEqual(new Quaternion32(0, 0, 0, 0) - Quaternion32.One, -Quaternion32.One);
            Assert.AreEqual(1 - new Quaternion32(1, 2, 3, 4), new Quaternion32(0, 2, 3, 4));
            Assert.AreEqual(new Quaternion32(1, 2, 3, 4) - 1, new Quaternion32(0, 2, 3, 4));
        }

        [Test]
        public void CanMultiplyQuaternionsAndFloatUsingOperator()
        {
            Assert.AreEqual(new Quaternion32(8, -9, -2, 11), new Quaternion32(3, 1, -2, 1) * new Quaternion32(2, -1, 2, 3));
            Assert.AreEqual(new Quaternion32(3, 1, -2, 1), new Quaternion32(3, 1, -2, 1) * 1);
            Assert.AreEqual(new Quaternion32(6, 2, -4, 2), new Quaternion32(3, 1, -2, 1) * 2);
            Assert.AreEqual(new Quaternion32(1.5f, 0.5f, -1, 0.5f), new Quaternion32(3, 1, -2, 1) * 0.5f);
        }

    }
}
