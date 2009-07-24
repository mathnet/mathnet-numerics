namespace MathNet.Numerics.UnitTests
{
    using MbUnit.Framework;

    [TestFixture]
    public class ComplexTest
    {
        [Test, MultipleAsserts]
        public void CanCreateComplexNumberUsingTheConstructor()
        {
            var complex = new Complex(1.1, -2.2);
            Assert.AreEqual(complex.Real, 1.1, "Real part is 1.1");
            Assert.AreEqual(complex.Imaginary, -2.2, "Imaginary part is -2.2");
        }
    }
}