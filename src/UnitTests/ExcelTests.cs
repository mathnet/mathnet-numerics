// <copyright file="ExcelTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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

using System;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace MathNet.Numerics.UnitTests
{
    [TestFixture, Category("Functions")]
    public class ExcelTests
    {
        [Test]
        public void TDIST()
        {
            Assert.That(ExcelFunctions.TDIST(0, 2, 1), Is.EqualTo(0.50000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.TDIST(0.25, 2, 1), Is.EqualTo(0.41296117202).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.TDIST(0.5, 2, 1), Is.EqualTo(0.33333333333).Within(1e-8), "C1");
            Assert.That(ExcelFunctions.TDIST(1, 2, 1), Is.EqualTo(0.21132486541).Within(1e-8), "D1");
            Assert.That(ExcelFunctions.TDIST(2, 2, 1), Is.EqualTo(0.09175170954).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.TDIST(10, 2, 1), Is.EqualTo(0.00492622851).Within(1e-8), "F1");

            Assert.That(ExcelFunctions.TDIST(0, 2, 2), Is.EqualTo(1.00000000000).Within(1e-8), "A2");
            Assert.That(ExcelFunctions.TDIST(0.25, 2, 2), Is.EqualTo(0.82592234404).Within(1e-8), "B2");
            Assert.That(ExcelFunctions.TDIST(0.5, 2, 2), Is.EqualTo(0.66666666667).Within(1e-8), "C2");
            Assert.That(ExcelFunctions.TDIST(1, 2, 2), Is.EqualTo(0.42264973081).Within(1e-8), "D2");
            Assert.That(ExcelFunctions.TDIST(2, 2, 2), Is.EqualTo(0.18350341907).Within(1e-8), "E2");
            Assert.That(ExcelFunctions.TDIST(10, 2, 2), Is.EqualTo(0.00985245702).Within(1e-8), "F2");
        }

        [Test]
        public void TINV()
        {
            Assert.That(ExcelFunctions.TINV(0.01, 2), Is.EqualTo(9.92484320092).Within(1e-6), "A1");
            Assert.That(ExcelFunctions.TINV(0.25, 2), Is.EqualTo(1.60356745147).Within(1e-6), "B1");
            Assert.That(ExcelFunctions.TINV(0.5, 2), Is.EqualTo(0.81649658093).Within(1e-6), "C1");
            Assert.That(ExcelFunctions.TINV(0.75, 2), Is.EqualTo(0.36514837167).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.TINV(0.99, 2), Is.EqualTo(0.01414284278).Within(1e-6), "E1");
            Assert.That(ExcelFunctions.TINV(0.9999, 2), Is.EqualTo(0.00014142136).Within(1e-6), "F1");
            Assert.That(ExcelFunctions.TINV(1.0, 2), Is.EqualTo(0.00000000000).Within(1e-6), "G1");
        }

        [Test]
        public void BETADIST()
        {
            Assert.That(ExcelFunctions.BETADIST(0, 2, 1.5), Is.EqualTo(0.00000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.BETADIST(0.25, 2, 1.5), Is.EqualTo(0.10691130235).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.BETADIST(0.5, 2, 1.5), Is.EqualTo(0.38128156646).Within(1e-8), "C1");
            Assert.That(ExcelFunctions.BETADIST(0.75, 2, 1.5), Is.EqualTo(0.73437500000).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.BETADIST(0.9999, 2, 1.5), Is.EqualTo(0.99999750015).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.BETADIST(0.0001, 2, 1.5), Is.EqualTo(0.00000001875).Within(1e-8), "F1");
        }

        [Test]
        public void BETAINV()
        {
            Assert.That(ExcelFunctions.BETAINV(0.01, 2, 1.5), Is.EqualTo(0.07396024691).Within(1e-6), "A1");
            Assert.That(ExcelFunctions.BETAINV(0.25, 2, 1.5), Is.EqualTo(0.39447722186).Within(1e-6), "B1");
            Assert.That(ExcelFunctions.BETAINV(0.5, 2, 1.5), Is.EqualTo(0.58637250688).Within(1e-6), "C1");
            Assert.That(ExcelFunctions.BETAINV(0.75, 2, 1.5), Is.EqualTo(0.76115477285).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.BETAINV(0.99, 2, 1.5), Is.EqualTo(0.97454166372).Within(1e-6), "E1");
            Assert.That(ExcelFunctions.BETAINV(0.9999, 2, 1.5), Is.EqualTo(0.99882984514).Within(1e-6), "F1");
        }

        [Test]
        public void GAMMADIST()
        {
            Assert.That(ExcelFunctions.GAMMADIST(0, 2, 1.5, true), Is.EqualTo(0.00000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.GAMMADIST(0.25, 2, 1.5, true), Is.EqualTo(0.01243798763).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.GAMMADIST(0.5, 2, 1.5, true), Is.EqualTo(0.04462491923).Within(1e-8), "C1");
            Assert.That(ExcelFunctions.GAMMADIST(1, 2, 1.5, true), Is.EqualTo(0.14430480161).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.GAMMADIST(2, 2, 1.5, true), Is.EqualTo(0.38494001106).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.GAMMADIST(10, 2, 1.5, true), Is.EqualTo(0.99024314086).Within(1e-8), "F1");

            Assert.That(ExcelFunctions.GAMMADIST(0, 2, 1.5, false), Is.EqualTo(0.00000000000).Within(1e-8), "A2");
            Assert.That(ExcelFunctions.GAMMADIST(0.25, 2, 1.5, false), Is.EqualTo(0.09405352499).Within(1e-8), "B2");
            Assert.That(ExcelFunctions.GAMMADIST(0.5, 2, 1.5, false), Is.EqualTo(0.15922918013).Within(1e-8), "C2");
            Assert.That(ExcelFunctions.GAMMADIST(1, 2, 1.5, false), Is.EqualTo(0.22818538624).Within(1e-8), "D2");
            Assert.That(ExcelFunctions.GAMMADIST(2, 2, 1.5, false), Is.EqualTo(0.23430856721).Within(1e-8), "E2");
            Assert.That(ExcelFunctions.GAMMADIST(10, 2, 1.5, false), Is.EqualTo(0.00565615023).Within(1e-8), "F2");
        }

        [Test]
        public void GAMMAINV()
        {
            Assert.That(ExcelFunctions.GAMMAINV(0, 2, 1.5), Is.EqualTo(0.00000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.GAMMAINV(0.01, 2, 1.5), Is.EqualTo(0.22283211038).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.GAMMAINV(0.25, 2, 1.5), Is.EqualTo(1.44191814467).Within(1e-6), "C1");
            Assert.That(ExcelFunctions.GAMMAINV(0.5, 2, 1.5), Is.EqualTo(2.51752048502).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.GAMMAINV(0.75, 2, 1.5), Is.EqualTo(4.03895179333).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.GAMMAINV(0.99, 2, 1.5), Is.EqualTo(9.95752810199).Within(1e-8), "F1");
            Assert.That(ExcelFunctions.GAMMAINV(0.9999, 2, 1.5), Is.EqualTo(17.63455683374).Within(1e-8), "G1");
        }

        [Test]
        public void PERCENTILE()
        {
            var array = new Double[] { 1, 8, 12, 7, 2, 9, 10, 4 };
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.00), Is.EqualTo(1.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.01), Is.EqualTo(1.07000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.10), Is.EqualTo(1.70000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.40), Is.EqualTo(6.40000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.50), Is.EqualTo(7.50000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.90), Is.EqualTo(10.60000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.99), Is.EqualTo(11.86000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 1.00), Is.EqualTo(12.00000000000).Within(1e-8));
            
            array = new Double[] { 1, 9, 12, 7, 2, 9, 10, 2 };
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.00), Is.EqualTo(1.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.01), Is.EqualTo(1.07000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.10), Is.EqualTo(1.70000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.40), Is.EqualTo(6.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.50), Is.EqualTo(8.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.90), Is.EqualTo(10.60000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 0.99), Is.EqualTo(11.86000000000).Within(1e-8));
            Assert.That(ExcelFunctions.PERCENTILE(array, 1.00), Is.EqualTo(12.00000000000).Within(1e-8));
        }

        [Test]
        public void QUARTILE()
        {
            var array = new Double[] { 1, 8, 12, 7, 2, 9, 10, 4 };
            Assert.That(ExcelFunctions.QUARTILE(array, 0), Is.EqualTo(1.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 1), Is.EqualTo(3.50000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 2), Is.EqualTo(7.50000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 3), Is.EqualTo(9.25000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 4), Is.EqualTo(12.00000000000).Within(1e-8));

            array = new Double[] { 1, 9, 12, 7, 2, 9, 10, 2 };
            Assert.That(ExcelFunctions.QUARTILE(array, 0), Is.EqualTo(1.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 1), Is.EqualTo(2.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 2), Is.EqualTo(8.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 3), Is.EqualTo(9.25000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 4), Is.EqualTo(12.00000000000).Within(1e-8));
        }

        [Test]
        public void PERCENTRANK()
        {
            var array = new Double[] { 1, 8, 12, 7, 2, 9, 10, 4 };
            Assert.That(ExcelFunctions.PERCENTRANK(array, 1.0), Is.EqualTo(0.00000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 2.0), Is.EqualTo(0.14285714280).Within(1e-8), "A2");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 3.0), Is.EqualTo(0.21428571420).Within(1e-8), "A3");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 4.0), Is.EqualTo(0.28571428570).Within(1e-8), "A4");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 5.0), Is.EqualTo(0.33333333330).Within(1e-8), "A5");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 6.0), Is.EqualTo(0.38095238090).Within(1e-8), "A6");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 7.0), Is.EqualTo(0.42857142850).Within(1e-8), "A7");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 8.0), Is.EqualTo(0.57142857140).Within(1e-8), "A8");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 9.0), Is.EqualTo(0.71428571420).Within(1e-8), "A9");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 10.0), Is.EqualTo(0.85714285710).Within(1e-8), "A10");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 11.0), Is.EqualTo(0.92857142850).Within(1e-8), "A11");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 12.0), Is.EqualTo(1.00000000000).Within(1e-8), "A12");

            array = new Double[] { 1, 9, 12, 7, 2, 9, 10, 2 };
            Assert.That(ExcelFunctions.PERCENTRANK(array, 1.0), Is.EqualTo(0.00000000000).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 2.0), Is.EqualTo(0.14285714280).Within(1e-8), "B2");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 3.0), Is.EqualTo(0.31428571420).Within(1e-8), "B3");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 4.0), Is.EqualTo(0.34285714280).Within(1e-8), "B4");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 5.0), Is.EqualTo(0.37142857140).Within(1e-8), "B5");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 6.0), Is.EqualTo(0.40000000000).Within(1e-8), "B6");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 7.0), Is.EqualTo(0.42857142850).Within(1e-8), "B7");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 8.0), Is.EqualTo(0.50000000000).Within(1e-8), "B8");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 9.0), Is.EqualTo(0.57142857140).Within(1e-8), "B9");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 10.0), Is.EqualTo(0.85714285710).Within(1e-8), "B10");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 11.0), Is.EqualTo(0.92857142850).Within(1e-8), "B11");
            Assert.That(ExcelFunctions.PERCENTRANK(array, 12.0), Is.EqualTo(1.00000000000).Within(1e-8), "B12");
        }
    }
}
// ReSharper restore InconsistentNaming
