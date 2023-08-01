// <copyright file="MittagLefflerTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using NUnit.Framework;

namespace MathNet.Numerics.Tests.SpecialFunctionsTests
{
    using Complex = System.Numerics.Complex;

    /// <summary>
    /// Marcum Q functions tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class MittagLefflerTests
    {
        // E_0.5(z) = e^(z^2) erfc(-z)
        [TestCase(0.5, 2.0, 0.0, 108.9409043899779724123554, 0.0, 14)]
        [TestCase(0.5, -1.0, 1.0, 0.3047442052569125924571388, 0.2082189382028316272874373, 14)]
        // E_1(z) = e^z
        [TestCase(1.0, 0.0, 0.0, 1.0, 0.0, 14)]
        [TestCase(1.0, -1.0, 0.0, 0.3678794411714423215955238, 0.0, 13)]
        [TestCase(1.0, 1.0, 0.0, 2.718281828459045235360287, 0.0, 14)]
        [TestCase(1.0, 1.0, 1.0, 1.468693939915885157138968, 2.287355287178842391208172, 14)]
        [TestCase(1.0, 0.0, 1.0, 0.5403023058681397174009366, 0.8414709848078965066525023, 13)]
        [TestCase(1.0, 0.0, -1.0, 0.5403023058681397174009366, -0.8414709848078965066525023, 13)]
        [TestCase(1.0, 100.0, -100.0, 2.318014142308082065958914E43, 1.361170159893859825171534E43, 12)]
        // E_2(z) = cosh(sqrt(z))
        [TestCase(2.0, 1.0, 0.0, 1.543080634815243778477906, 0.0, 14)]
        [TestCase(2.0, -1.0, 0.0, 0.5403023058681397174009366, 0.0, 14)]
        // E_3(z) = 1/3 (e^(z^(1/3)) + 2 e^(-z^(1/3)/2) cos(1/2 sqrt(3) z^(1/3)))
        [TestCase(3.0, 1.0, 1.0, 1.1666611468490602090051780, 0.1694499559052291264367060, 14)]
        [TestCase(3.0, 100.0, -25.0, 33.06086298199622791072148, -13.33913169891245601081196, 14)]
        // E_4(z) = 1/2 (cos(z^(1/4)) + cosh(z^(1/4)))
        [TestCase(4.0, 1.0, -1.0, 1.0416666624911240883556243, -0.0417162740166212371993211, 12)]
        [TestCase(4.0, 10.0, -10.0, 1.416662489403313747387247, -0.421631159478217275004827, 14)]
        public void MittagLeffler(double alpha, double z1, double z2, double exp1, double exp2, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(exp1, exp2),
                SpecialFunctions.MittagLefflerE(alpha, new Complex(z1, z2)),
                decimalPlaces
                );
        }

        [TestCase(0.5, 0.5, 0.5, 0.0, 1.5403698281390346, 0.0, 14)]
        [TestCase(1.5, 0.5, 0.5, 0.0, 1.1448466286155243, 0.0, 14)]
        [TestCase(2.3, 1.0, 0.7, 2.0, 1.201890136368392, 0.7895394560075035, 13)]
        public void GeneralizedMittagLeffler(double alpha, double beta, double z1, double z2, double exp1, double exp2, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(exp1, exp2),
                SpecialFunctions.MittagLefflerE(alpha, beta, new Complex(z1, z2)),
                decimalPlaces
                );
        }

        [TestCase(0.75, 1.0, 1.2, -10, 1.0, 8.72265651199321222E-03, 1.30548998870915031E-03, 13)]
        public void ThreeParameterMittagLeffler(double alpha, double beta, double gamma, double z1, double z2, double exp1, double exp2, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(exp1, exp2),
                SpecialFunctions.MittagLefflerE(alpha, beta, gamma, new Complex(z1, z2)),
                decimalPlaces
                );
        }
    }
}
