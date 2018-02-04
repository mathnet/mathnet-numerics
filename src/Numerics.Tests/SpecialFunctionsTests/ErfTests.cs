// <copyright file="ErfTests.cs" company="Math.NET">
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

using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.SpecialFunctionsTests
{
    /// <summary>
    /// Error functions tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class ErfTests
    {
        /// <summary>
        /// Error function can match large precision.
        /// </summary>
        /// <param name="x">Input value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(double.NaN, double.NaN)]
        [TestCase(-1.0, -0.84270079294971486934122063508260925929606699796630291)]
        [TestCase(0.0, 0.0)]
        [TestCase(1e-15, 0.0000000000000011283791670955126615773132947717431253912942469337536)]
        [TestCase(0.1, 0.1124629160182848984047122510143040617233925185058162)]
        [TestCase(0.2, 0.22270258921047846617645303120925671669511570710081967)]
        [TestCase(0.3, 0.32862675945912741618961798531820303325847175931290341)]
        [TestCase(0.4, 0.42839235504666847645410962730772853743532927705981257)]
        [TestCase(0.5, 0.5204998778130465376827466538919645287364515757579637)]
        [TestCase(1.0, 0.84270079294971486934122063508260925929606699796630291)]
        [TestCase(1.5, 0.96610514647531072706697626164594785868141047925763678)]
        [TestCase(2.0, 0.99532226501895273416206925636725292861089179704006008)]
        [TestCase(2.5, 0.99959304798255504106043578426002508727965132259628658)]
        [TestCase(3.0, 0.99997790950300141455862722387041767962015229291260075)]
        [TestCase(4.0, 0.99999998458274209971998114784032651311595142785474641)]
        [TestCase(5.0, 0.99999999999846254020557196514981165651461662110988195)]
        [TestCase(6.0, 0.99999999999999997848026328750108688340664960081261537)]
        [TestCase(double.PositiveInfinity, 1.0)]
        [TestCase(double.NegativeInfinity, -1.0)]
        public void ErfCanMatchLargePrecision(double x, double f)
        {
            AssertHelpers.AlmostEqualRelative(f, SpecialFunctions.Erf(x), 15);
        }

        /// <summary>
        /// Complementary error function can match large precision.
        /// </summary>
        /// <param name="x">Input value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(double.NaN, double.NaN)]
        [TestCase(-1.0, 1.8427007929497148693412206350826092592960669979663028)]
        [TestCase(0.0, 1.0)]
        [TestCase(0.1, 0.88753708398171510159528774898569593827660748149418343)]
        [TestCase(0.2, 0.77729741078952153382354696879074328330488429289918085)]
        [TestCase(0.3, 0.67137324054087258381038201468179696674152824068709621)]
        [TestCase(0.4, 0.57160764495333152354589037269227146256467072294018715)]
        [TestCase(0.5, 0.47950012218695346231725334610803547126354842424203654)]
        [TestCase(1.0, 0.15729920705028513065877936491739074070393300203369719)]
        [TestCase(1.5, 0.033894853524689272933023738354052141318589520742363247)]
        [TestCase(2.0, 0.0046777349810472658379307436327470713891082029599399245)]
        [TestCase(2.5, 0.00040695201744495893956421573997491272034867740371342016)]
        [TestCase(3.0, 0.00002209049699858544137277612958232037984770708739924966)]
        [TestCase(4.0, 0.000000015417257900280018852159673486884048572145253589191167)]
        [TestCase(5.0, 0.0000000000015374597944280348501883434853833788901180503147233804)]
        [TestCase(6.0, 2.1519736712498913116593350399187384630477514061688559e-17)]
        [TestCase(10.0, 2.0884875837625447570007862949577886115608181193211634e-45)]
        [TestCase(15.0, 7.2129941724512066665650665586929271099340909298253858e-100)]
        [TestCase(20.0, 5.3958656116079009289349991679053456040882726709236071e-176)]
        [TestCase(30.0, 2.5646562037561116000333972775014471465488897227786155e-393)]
        [TestCase(50.0, 2.0709207788416560484484478751657887929322509209953988e-1088)]
        [TestCase(80.0, 2.3100265595063985852034904366341042118385080919280966e-2782)]
        [TestCase(double.PositiveInfinity, 0.0)]
        [TestCase(double.NegativeInfinity, 2.0)]
        public void ErfcCanMatchLargePrecision(double x, double f)
        {
            AssertHelpers.AlmostEqualRelative(f, SpecialFunctions.Erfc(x), 13);
        }

        /// <summary>
        /// Complementary inverse error function can match large precision.
        /// </summary>
        /// <param name="x">Input value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(0.0, double.PositiveInfinity)]
        [TestCase(1e-100, 15.065574702593)]
        [TestCase(1e-30, 8.1486162231699)]
        [TestCase(1e-20, 6.6015806223551)]
        [TestCase(1e-10, 4.5728249585449249378479309946884581365517663258840893)]
        [TestCase(1e-5, 3.1234132743415708640270717579666062107939039971365252)]
        [TestCase(0.1, 1.1630871536766741628440954340547000483801487126688552)]
        [TestCase(0.2, 0.90619380243682330953597079527631536107443494091638384)]
        [TestCase(0.5, 0.47693627620446987338141835364313055980896974905947083)]
        [TestCase(1.0, 0.0)]
        [TestCase(1.5, -0.47693627620446987338141835364313055980896974905947083)]
        [TestCase(2.0, double.NegativeInfinity)]
        public void ErfcInvCanMatchLargePrecision(double x, double f)
        {
            AssertHelpers.AlmostEqualRelative(f, SpecialFunctions.ErfcInv(x), 7);
        }

        /// <summary>
        /// Inverse error function can match large precision.
        /// </summary>
        /// <param name="x">Input value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(double.NaN, double.NaN)]
        [TestCase(-1.0, -0.84270079294971486934122063508260925929606699796630291)]
        [TestCase(0.0, 0.0)]
        [TestCase(1e-15, 0.0000000000000011283791670955126615773132947717431253912942469337536)]
        [TestCase(0.1, 0.1124629160182848984047122510143040617233925185058162)]
        [TestCase(0.2, 0.22270258921047846617645303120925671669511570710081967)]
        [TestCase(0.3, 0.32862675945912741618961798531820303325847175931290341)]
        [TestCase(0.4, 0.42839235504666847645410962730772853743532927705981257)]
        [TestCase(0.5, 0.5204998778130465376827466538919645287364515757579637)]
        [TestCase(1.0, 0.84270079294971486934122063508260925929606699796630291)]
        [TestCase(1.5, 0.96610514647531072706697626164594785868141047925763678)]
        [TestCase(2.0, 0.99532226501895273416206925636725292861089179704006008)]
        [TestCase(2.5, 0.99959304798255504106043578426002508727965132259628658)]
        [TestCase(3.0, 0.99997790950300141455862722387041767962015229291260075)]
        [TestCase(4.0, 0.99999998458274209971998114784032651311595142785474641)]
        [TestCase(5.0, 0.99999999999846254020557196514981165651461662110988195)]
        [TestCase(double.PositiveInfinity, 1.0)]
        [TestCase(double.NegativeInfinity, -1.0)]
        public void ErfInv(double x, double f)
        {
            AssertHelpers.AlmostEqualRelative(x, SpecialFunctions.ErfInv(f), 5);
        }
    }
}
