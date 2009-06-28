namespace MathNet.Numerics.UnitTests
{
    using MbUnit.Framework;

    [TestFixture]
    public class ComplexTest
    {
        [Test, MultipleAsserts]
        public void CanCreateAComplexNumberUsingTheConstructor()
        {
            var complex = new Complex(1.1, -2.2);
            AssertEx.That(() => complex.Real == 1.1, "Real Part");
            AssertEx.That(() => complex.Imaginary == -2.2, "Imaginary Part");
        }
    }
}