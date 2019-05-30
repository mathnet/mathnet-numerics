using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MathNet.Numerics.Tests
{
    [TestFixture, Category("WindowFunctions")]
    public class WindowTest
    {

        [Test]
        public void TukeyWin0()
        {

            var expected = new[] { 1, 1, 1, 1, 1, 1 };
            var actual = Window.Tukey(6, 0);

            Assert.That(actual, Is.EqualTo(expected).Within(0.00005));

        }

        [Test]
        public void TukeyWin1()
        {

            var expected = new[] { 0, 0.3455, 0.9045, 0.9045, 0.3455, 0 };
            var actual = Window.Tukey(6, 1);

            Assert.That(actual, Is.EqualTo(expected).Within(0.00005));

        }

        [Test]
        public void TukeyWin25()
        {

            var expected = new[] { 0, 0.9698, 1, 1, 1, 1, 1, 1, 0.9698, 0 };
            var actual = Window.Tukey(10, 0.25);

            Assert.That(actual, Is.EqualTo(expected).Within(0.00005));

        }

        [Test]
        public void TukeyWin75()
        {

            var expected = new[] { 0, 0.2014, 0.6434, 0.9698, 1, 1, 0.9698, 0.6434, 0.2014, 0 };
            var actual = Window.Tukey(10, 0.75F);

            Assert.That(actual, Is.EqualTo(expected).Within(0.00005));

        }

    }
}
