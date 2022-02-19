using System;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.LinearAlgebraTests
{
    [TestFixture]
    public class VectorToStringTests
    {
        Vector<double> v1 = Vector<double>.Build.Dense(1, i => (i + 1)*Constants.Pi);
        Vector<double> v2 = Vector<double>.Build.Dense(2, i => (i + 1)*Constants.Pi);
        Vector<double> v100 = Vector<double>.Build.Dense(100, i => (i + 1)*Constants.Pi);
        readonly string NL = Environment.NewLine;

        [Test]
        public void MinimumLimits()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Assert.That(v1.ToVectorString(3, 16, "G6"), Is.EqualTo("3.14159" + NL));
            Assert.That(v2.ToVectorString(3, 16, "G6"), Is.EqualTo("3.14159" + NL + "6.28319" + NL));
            Assert.That(v100.ToVectorString(3, 16, "G6"), Is.EqualTo("3.14159" + NL + "     .." + NL + "314.159" + NL));
        }

        [Test]
        public void GitHubIssue387()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Vector<double> v = Vector<double>.Build.DenseOfArray(new[]
                {
                    0.607142857142857,
                    1.17857142857143
                });

            Assert.That(v.ToVectorString(12, 12, "..", "  ", "\n", x => x.ToString()), Is.EqualTo("0.607142857142857\n 1.17857142857143\n"));
        }
    }
}
