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
        public void QUARTILE()
        {
            var array = new Double[] { 1, 8, 12, 7, 2, 9, 10, 4 };
            Assert.That(ExcelFunctions.QUARTILE(array, 0), Is.EqualTo(1.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 1), Is.EqualTo(3.50000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 2), Is.EqualTo(7.50000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 3), Is.EqualTo(9.25000000000).Within(1e-8));
            Assert.That(ExcelFunctions.QUARTILE(array, 4), Is.EqualTo(12.00000000000).Within(1e-8));
        }
    }
}
// ReSharper restore InconsistentNaming
