// <copyright file="ErfTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.UnitTests.SpecialFunctionTests
{
    using MbUnit.Framework;
    using MathNet.Numerics;

    [TestFixture]
    public class ErfTests
    {
        [Test]
        [Row(0.0, 0.0)]
        [Row(0.1, 0.1124629160182848984047122510143040617233925185058162)]
        [Row(0.2, 0.22270258921047846617645303120925671669511570710081967)]
        [Row(0.3, 0.32862675945912741618961798531820303325847175931290341)]
        [Row(0.4, 0.42839235504666847645410962730772853743532927705981257)]
        [Row(0.5, 0.5204998778130465376827466538919645287364515757579637)]
        [Row(1.0, 0.84270079294971486934122063508260925929606699796630291)]
        [Row(1.5, 0.96610514647531072706697626164594785868141047925763678)]
        [Row(2.0, 0.99532226501895273416206925636725292861089179704006008)]
        [Row(2.5, 0.99959304798255504106043578426002508727965132259628658)]
        [Row(3.0, 0.99997790950300141455862722387041767962015229291260075)]
        [Row(4.0, 0.99999998458274209971998114784032651311595142785474641)]
        [Row(5.0, 0.99999999999846254020557196514981165651461662110988195)]
        [Row(double.PositiveInfinity, 1.0)]
        public void ErfCanMatchLargePrecision(double x, double f)
        {
            AssertHelpers.AlmostEqual(f, SpecialFunctions.Erf(x), 15);
        }

        [Test]
        [Row(0.0, 1.0)]
        [Row(0.1, 0.88753708398171510159528774898569593827660748149418343)]
        [Row(0.2, 0.77729741078952153382354696879074328330488429289918085)]
        [Row(0.3, 0.67137324054087258381038201468179696674152824068709621)]
        [Row(0.4, 0.57160764495333152354589037269227146256467072294018715)]
        [Row(0.5, 0.47950012218695346231725334610803547126354842424203654)]
        [Row(1.0, 0.15729920705028513065877936491739074070393300203369719)]
        [Row(1.5, 0.033894853524689272933023738354052141318589520742363247)]
        [Row(2.0, 0.0046777349810472658379307436327470713891082029599399245)]
        [Row(2.5, 0.00040695201744495893956421573997491272034867740371342016)]
        [Row(3.0, 0.00002209049699858544137277612958232037984770708739924966)]
        [Row(4.0, 0.000000015417257900280018852159673486884048572145253589191167)]
        [Row(5.0, 0.0000000000015374597944280348501883434853833788901180503147233804)]
        [Row(double.PositiveInfinity, 0.0)]
        public void ErfcCanMatchLargePrecision(double x, double f)
        {
            AssertHelpers.AlmostEqual(f, SpecialFunctions.Erfc(x), 13);
        }

        [Test]
        [Row(0.0, double.PositiveInfinity)]
        [Row(1e-100, 15.065574702593)] // From dnA tests.
        [Row(1e-30, 8.1486162231699)] // From dnA tests.
        [Row(1e-20, 6.6015806223551)] // From dnA tests.
        [Row(1e-10, 4.5728249585449249378479309946884581365517663258840893)]
        [Row(1e-5, 3.1234132743415708640270717579666062107939039971365252)]
        [Row(0.1, 1.1630871536766741628440954340547000483801487126688552)]
        [Row(0.2, 0.90619380243682330953597079527631536107443494091638384)]
        [Row(0.5, 0.47693627620446987338141835364313055980896974905947083)]
        [Row(1.0, 0.0)]
        [Row(1.5, -0.47693627620446987338141835364313055980896974905947083)]
        [Row(2.0, double.NegativeInfinity)]
        public void ErfcInvCanMatchLargePrecision(double x, double f)
        {
            AssertHelpers.AlmostEqual(f, SpecialFunctions.ErfcInv(x), 8);
        }
    }
}
