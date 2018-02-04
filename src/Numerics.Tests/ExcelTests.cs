// <copyright file="ExcelTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
        public void NormSDist()
        {
            Assert.That(ExcelFunctions.NormSDist(0), Is.EqualTo(0.50000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.NormSDist(-0.25), Is.EqualTo(0.40129367432).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.NormSDist(0.5), Is.EqualTo(0.69146246127).Within(1e-8), "C1");
            Assert.That(ExcelFunctions.NormSDist(1), Is.EqualTo(0.84134474607).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.NormSDist(-2), Is.EqualTo(0.02275013195).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.NormSDist(10), Is.EqualTo(1.00000000000).Within(1e-8), "F1");
        }

        [Test]
        public void NormSInv()
        {
            Assert.That(ExcelFunctions.NormSInv(0.01), Is.EqualTo(-2.32634787404).Within(1e-6), "A1");
            Assert.That(ExcelFunctions.NormSInv(0.25), Is.EqualTo(-0.67448975020).Within(1e-6), "B1");
            Assert.That(ExcelFunctions.NormSInv(0.5), Is.EqualTo(0.00000000000).Within(1e-6), "C1");
            Assert.That(ExcelFunctions.NormSInv(0.75), Is.EqualTo(0.67448975020).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.NormSInv(0.99), Is.EqualTo(2.32634787404).Within(1e-6), "E1");
            Assert.That(ExcelFunctions.NormSInv(0.9999), Is.EqualTo(3.71901648546).Within(1e-6), "F1");
        }

        [Test]
        public void NormDist()
        {
            Assert.That(ExcelFunctions.NormDist(0, 2, 3, true), Is.EqualTo(0.25249253755).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.NormDist(-0.25, 2, 3, true), Is.EqualTo(0.22662735238).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.NormDist(0.5, 2, 3, true), Is.EqualTo(0.30853753873).Within(1e-8), "C1");
            Assert.That(ExcelFunctions.NormDist(1, 2, 3, true), Is.EqualTo(0.36944134018).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.NormDist(-2, 2, 3, true), Is.EqualTo(0.09121121973).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.NormDist(10, 2, 3, true), Is.EqualTo(0.99616961943).Within(1e-8), "F1");

            Assert.That(ExcelFunctions.NormDist(0, 2, 3, false), Is.EqualTo(0.106482669).Within(1e-8), "A2");
            Assert.That(ExcelFunctions.NormDist(-0.25, 2, 3, false), Is.EqualTo(0.100379144).Within(1e-8), "B2");
            Assert.That(ExcelFunctions.NormDist(0.5, 2, 3, false), Is.EqualTo(0.117355109).Within(1e-8), "C2");
            Assert.That(ExcelFunctions.NormDist(1, 2, 3, false), Is.EqualTo(0.125794409).Within(1e-6), "D2");
            Assert.That(ExcelFunctions.NormDist(-2, 2, 3, false), Is.EqualTo(0.054670025).Within(1e-8), "E2");
            Assert.That(ExcelFunctions.NormDist(10, 2, 3, false), Is.EqualTo(0.003798662).Within(1e-8), "F2");
        }

        [Test]
        public void NormInv()
        {
            Assert.That(ExcelFunctions.NormInv(0.01, 2, 3), Is.EqualTo(-4.97904362212).Within(1e-6), "A1");
            Assert.That(ExcelFunctions.NormInv(0.25, 2, 3), Is.EqualTo(-0.02346925059).Within(1e-6), "B1");
            Assert.That(ExcelFunctions.NormInv(0.5, 2, 3), Is.EqualTo(2.00000000000).Within(1e-6), "C1");
            Assert.That(ExcelFunctions.NormInv(0.75, 2, 3), Is.EqualTo(4.02346925059).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.NormInv(0.99, 2, 3), Is.EqualTo(8.97904362212).Within(1e-6), "E1");
            Assert.That(ExcelFunctions.NormInv(0.9999, 2, 3), Is.EqualTo(13.15704945637).Within(1e-6), "F1");
        }

        [Test]
        public void TDist()
        {
            Assert.That(ExcelFunctions.TDist(0, 2, 1), Is.EqualTo(0.50000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.TDist(0.25, 2, 1), Is.EqualTo(0.41296117202).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.TDist(0.5, 2, 1), Is.EqualTo(0.33333333333).Within(1e-8), "C1");
            Assert.That(ExcelFunctions.TDist(1, 2, 1), Is.EqualTo(0.21132486541).Within(1e-8), "D1");
            Assert.That(ExcelFunctions.TDist(2, 2, 1), Is.EqualTo(0.09175170954).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.TDist(10, 2, 1), Is.EqualTo(0.00492622851).Within(1e-8), "F1");

            Assert.That(ExcelFunctions.TDist(0, 2, 2), Is.EqualTo(1.00000000000).Within(1e-8), "A2");
            Assert.That(ExcelFunctions.TDist(0.25, 2, 2), Is.EqualTo(0.82592234404).Within(1e-8), "B2");
            Assert.That(ExcelFunctions.TDist(0.5, 2, 2), Is.EqualTo(0.66666666667).Within(1e-8), "C2");
            Assert.That(ExcelFunctions.TDist(1, 2, 2), Is.EqualTo(0.42264973081).Within(1e-8), "D2");
            Assert.That(ExcelFunctions.TDist(2, 2, 2), Is.EqualTo(0.18350341907).Within(1e-8), "E2");
            Assert.That(ExcelFunctions.TDist(10, 2, 2), Is.EqualTo(0.00985245702).Within(1e-8), "F2");
        }

        [Test]
        public void TInv()
        {
            Assert.That(ExcelFunctions.TInv(0.01, 2), Is.EqualTo(9.92484320092).Within(1e-6), "A1");
            Assert.That(ExcelFunctions.TInv(0.25, 2), Is.EqualTo(1.60356745147).Within(1e-6), "B1");
            Assert.That(ExcelFunctions.TInv(0.5, 2), Is.EqualTo(0.81649658093).Within(1e-6), "C1");
            Assert.That(ExcelFunctions.TInv(0.75, 2), Is.EqualTo(0.36514837167).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.TInv(0.99, 2), Is.EqualTo(0.01414284278).Within(1e-6), "E1");
            Assert.That(ExcelFunctions.TInv(0.9999, 2), Is.EqualTo(0.00014142136).Within(1e-6), "F1");
            Assert.That(ExcelFunctions.TInv(1.0, 2), Is.EqualTo(0.00000000000).Within(1e-6), "G1");
        }

        [Test]
        public void FDist()
        {
            Assert.That(ExcelFunctions.FDist(0, 2, 3), Is.EqualTo(1.00000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.FDist(0.25, 2, 3), Is.EqualTo(0.79356008552).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.FDist(0.5, 2, 3), Is.EqualTo(0.64951905284).Within(1e-8), "C1");
            Assert.That(ExcelFunctions.FDist(1, 2, 3), Is.EqualTo(0.46475800154).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.FDist(2, 2, 3), Is.EqualTo(0.28056585887).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.FDist(10, 2, 3), Is.EqualTo(0.04710750773).Within(1e-8), "F1");
        }

        [Test]
        public void FInv()
        {
            Assert.That(ExcelFunctions.FInv(0.01, 2, 3), Is.EqualTo(30.81652035048).Within(1e-6), "A1");
            Assert.That(ExcelFunctions.FInv(0.25, 2, 3), Is.EqualTo(2.27976314968).Within(1e-6), "B1");
            Assert.That(ExcelFunctions.FInv(0.5, 2, 3), Is.EqualTo(0.88110157795).Within(1e-6), "C1");
            Assert.That(ExcelFunctions.FInv(0.75, 2, 3), Is.EqualTo(0.31712059283).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.FInv(0.99, 2, 3), Is.EqualTo(0.01008408093).Within(1e-6), "E1");
            Assert.That(ExcelFunctions.FInv(0.9999, 2, 3), Is.EqualTo(0.00010000833).Within(1e-6), "F1");
            Assert.That(ExcelFunctions.FInv(1.0, 2, 3), Is.EqualTo(0.00000000000).Within(1e-6), "G1");
        }

        [Test]
        public void BetaDist()
        {
            Assert.That(ExcelFunctions.BetaDist(0, 2, 1.5), Is.EqualTo(0.00000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.BetaDist(0.25, 2, 1.5), Is.EqualTo(0.10691130235).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.BetaDist(0.5, 2, 1.5), Is.EqualTo(0.38128156646).Within(1e-8), "C1");
            Assert.That(ExcelFunctions.BetaDist(0.75, 2, 1.5), Is.EqualTo(0.73437500000).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.BetaDist(0.9999, 2, 1.5), Is.EqualTo(0.99999750015).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.BetaDist(0.0001, 2, 1.5), Is.EqualTo(0.00000001875).Within(1e-8), "F1");
        }

        [Test]
        public void BetaInv()
        {
            Assert.That(ExcelFunctions.BetaInv(0.01, 2, 1.5), Is.EqualTo(0.07396024691).Within(1e-6), "A1");
            Assert.That(ExcelFunctions.BetaInv(0.25, 2, 1.5), Is.EqualTo(0.39447722186).Within(1e-6), "B1");
            Assert.That(ExcelFunctions.BetaInv(0.5, 2, 1.5), Is.EqualTo(0.58637250688).Within(1e-6), "C1");
            Assert.That(ExcelFunctions.BetaInv(0.75, 2, 1.5), Is.EqualTo(0.76115477285).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.BetaInv(0.99, 2, 1.5), Is.EqualTo(0.97454166372).Within(1e-6), "E1");
            Assert.That(ExcelFunctions.BetaInv(0.9999, 2, 1.5), Is.EqualTo(0.99882984514).Within(1e-6), "F1");
        }

        [Test]
        public void GammaDist()
        {
            Assert.That(ExcelFunctions.GammaDist(0, 2, 1.5, true), Is.EqualTo(0.00000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.GammaDist(0.25, 2, 1.5, true), Is.EqualTo(0.01243798763).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.GammaDist(0.5, 2, 1.5, true), Is.EqualTo(0.04462491923).Within(1e-8), "C1");
            Assert.That(ExcelFunctions.GammaDist(1, 2, 1.5, true), Is.EqualTo(0.14430480161).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.GammaDist(2, 2, 1.5, true), Is.EqualTo(0.38494001106).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.GammaDist(10, 2, 1.5, true), Is.EqualTo(0.99024314086).Within(1e-8), "F1");

            Assert.That(ExcelFunctions.GammaDist(0, 2, 1.5, false), Is.EqualTo(0.00000000000).Within(1e-8), "A2");
            Assert.That(ExcelFunctions.GammaDist(0.25, 2, 1.5, false), Is.EqualTo(0.09405352499).Within(1e-8), "B2");
            Assert.That(ExcelFunctions.GammaDist(0.5, 2, 1.5, false), Is.EqualTo(0.15922918013).Within(1e-8), "C2");
            Assert.That(ExcelFunctions.GammaDist(1, 2, 1.5, false), Is.EqualTo(0.22818538624).Within(1e-8), "D2");
            Assert.That(ExcelFunctions.GammaDist(2, 2, 1.5, false), Is.EqualTo(0.23430856721).Within(1e-8), "E2");
            Assert.That(ExcelFunctions.GammaDist(10, 2, 1.5, false), Is.EqualTo(0.00565615023).Within(1e-8), "F2");
        }

        [Test]
        public void GammaInv()
        {
            Assert.That(ExcelFunctions.GammaInv(0, 2, 1.5), Is.EqualTo(0.00000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.GammaInv(0.01, 2, 1.5), Is.EqualTo(0.22283211038).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.GammaInv(0.25, 2, 1.5), Is.EqualTo(1.44191814467).Within(1e-6), "C1");
            Assert.That(ExcelFunctions.GammaInv(0.5, 2, 1.5), Is.EqualTo(2.51752048502).Within(1e-6), "D1");
            Assert.That(ExcelFunctions.GammaInv(0.75, 2, 1.5), Is.EqualTo(4.03895179333).Within(1e-8), "E1");
            Assert.That(ExcelFunctions.GammaInv(0.99, 2, 1.5), Is.EqualTo(9.95752810199).Within(1e-8), "F1");
            Assert.That(ExcelFunctions.GammaInv(0.9999, 2, 1.5), Is.EqualTo(17.63455683374).Within(1e-8), "G1");
        }

        [Test]
        public void Percentile()
        {
            var array = new Double[] { 1, 8, 12, 7, 2, 9, 10, 4 };
            Assert.That(ExcelFunctions.Percentile(array, 0.00), Is.EqualTo(1.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.01), Is.EqualTo(1.07000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.10), Is.EqualTo(1.70000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.40), Is.EqualTo(6.40000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.50), Is.EqualTo(7.50000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.90), Is.EqualTo(10.60000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.99), Is.EqualTo(11.86000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 1.00), Is.EqualTo(12.00000000000).Within(1e-8));

            array = new Double[] { 1, 9, 12, 7, 2, 9, 10, 2 };
            Assert.That(ExcelFunctions.Percentile(array, 0.00), Is.EqualTo(1.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.01), Is.EqualTo(1.07000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.10), Is.EqualTo(1.70000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.40), Is.EqualTo(6.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.50), Is.EqualTo(8.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.90), Is.EqualTo(10.60000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 0.99), Is.EqualTo(11.86000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Percentile(array, 1.00), Is.EqualTo(12.00000000000).Within(1e-8));
        }

        [Test]
        public void Quartile()
        {
            var array = new Double[] { 1, 8, 12, 7, 2, 9, 10, 4 };
            Assert.That(ExcelFunctions.Quartile(array, 0), Is.EqualTo(1.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Quartile(array, 1), Is.EqualTo(3.50000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Quartile(array, 2), Is.EqualTo(7.50000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Quartile(array, 3), Is.EqualTo(9.25000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Quartile(array, 4), Is.EqualTo(12.00000000000).Within(1e-8));

            array = new Double[] { 1, 9, 12, 7, 2, 9, 10, 2 };
            Assert.That(ExcelFunctions.Quartile(array, 0), Is.EqualTo(1.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Quartile(array, 1), Is.EqualTo(2.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Quartile(array, 2), Is.EqualTo(8.00000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Quartile(array, 3), Is.EqualTo(9.25000000000).Within(1e-8));
            Assert.That(ExcelFunctions.Quartile(array, 4), Is.EqualTo(12.00000000000).Within(1e-8));
        }

        [Test]
        public void PercentRank()
        {
            var array = new Double[] { 1, 8, 12, 7, 2, 9, 10, 4 };
            Assert.That(ExcelFunctions.PercentRank(array, 1.0), Is.EqualTo(0.00000000000).Within(1e-8), "A1");
            Assert.That(ExcelFunctions.PercentRank(array, 2.0), Is.EqualTo(0.14285714280).Within(1e-8), "A2");
            Assert.That(ExcelFunctions.PercentRank(array, 3.0), Is.EqualTo(0.21428571420).Within(1e-8), "A3");
            Assert.That(ExcelFunctions.PercentRank(array, 4.0), Is.EqualTo(0.28571428570).Within(1e-8), "A4");
            Assert.That(ExcelFunctions.PercentRank(array, 5.0), Is.EqualTo(0.33333333330).Within(1e-8), "A5");
            Assert.That(ExcelFunctions.PercentRank(array, 6.0), Is.EqualTo(0.38095238090).Within(1e-8), "A6");
            Assert.That(ExcelFunctions.PercentRank(array, 7.0), Is.EqualTo(0.42857142850).Within(1e-8), "A7");
            Assert.That(ExcelFunctions.PercentRank(array, 8.0), Is.EqualTo(0.57142857140).Within(1e-8), "A8");
            Assert.That(ExcelFunctions.PercentRank(array, 9.0), Is.EqualTo(0.71428571420).Within(1e-8), "A9");
            Assert.That(ExcelFunctions.PercentRank(array, 10.0), Is.EqualTo(0.85714285710).Within(1e-8), "A10");
            Assert.That(ExcelFunctions.PercentRank(array, 11.0), Is.EqualTo(0.92857142850).Within(1e-8), "A11");
            Assert.That(ExcelFunctions.PercentRank(array, 12.0), Is.EqualTo(1.00000000000).Within(1e-8), "A12");

            array = new Double[] { 1, 9, 12, 7, 2, 9, 10, 2 };
            Assert.That(ExcelFunctions.PercentRank(array, 1.0), Is.EqualTo(0.00000000000).Within(1e-8), "B1");
            Assert.That(ExcelFunctions.PercentRank(array, 2.0), Is.EqualTo(0.14285714280).Within(1e-8), "B2");
            Assert.That(ExcelFunctions.PercentRank(array, 3.0), Is.EqualTo(0.31428571420).Within(1e-8), "B3");
            Assert.That(ExcelFunctions.PercentRank(array, 4.0), Is.EqualTo(0.34285714280).Within(1e-8), "B4");
            Assert.That(ExcelFunctions.PercentRank(array, 5.0), Is.EqualTo(0.37142857140).Within(1e-8), "B5");
            Assert.That(ExcelFunctions.PercentRank(array, 6.0), Is.EqualTo(0.40000000000).Within(1e-8), "B6");
            Assert.That(ExcelFunctions.PercentRank(array, 7.0), Is.EqualTo(0.42857142850).Within(1e-8), "B7");
            Assert.That(ExcelFunctions.PercentRank(array, 8.0), Is.EqualTo(0.50000000000).Within(1e-8), "B8");
            Assert.That(ExcelFunctions.PercentRank(array, 9.0), Is.EqualTo(0.57142857140).Within(1e-8), "B9");
            Assert.That(ExcelFunctions.PercentRank(array, 10.0), Is.EqualTo(0.85714285710).Within(1e-8), "B10");
            Assert.That(ExcelFunctions.PercentRank(array, 11.0), Is.EqualTo(0.92857142850).Within(1e-8), "B11");
            Assert.That(ExcelFunctions.PercentRank(array, 12.0), Is.EqualTo(1.00000000000).Within(1e-8), "B12");
        }
    }
}

// ReSharper restore InconsistentNaming
