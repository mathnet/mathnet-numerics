// <copyright file="GammaTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.UnitTests.SpecialFunctionsTests
{
    using System;
    using NUnit.Framework;

    /// <summary>
    /// Gamma functions tests.
    /// </summary>
    [TestFixture]
    public class GammaTests
    {
        /// <summary>
        /// Log gamma.
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(Double.NaN, Double.NaN)]
        [TestCase(0.1, 2.2527126517342059020062379568954763844479865649307379)]
        [TestCase(1.0, 0.0)]
        [TestCase(1.5, -0.12078223763524522234551844578164721225185272790259947)]
        [TestCase(Constants.Pi / 2, -0.11590380084550241329912089415904874214542604767006895)]
        [TestCase(2.0, 0.0)]
        [TestCase(2.5, 0.28468287047291915963249466968270192432013769555989498)]
        [TestCase(3.0, 0.693147180559945309417232121458176568075500134360255)]
        [TestCase(Constants.Pi, 0.82769459232343710152957855845235995115350173412073715)]
        [TestCase(3.5, 1.2009736023470742248160218814507129957702389154681574)]
        [TestCase(4.0, 1.7917594692280550008124773583807022727229906921830034)]
        [TestCase(4.5, 2.4537365708424422205041425034357161573318235106897606)]
        [TestCase(5.0, 3.1780538303479456196469416012970554088739909609035161)]
        [TestCase(5.5, 3.9578139676187162938774008558225909985513044919750065)]
        [TestCase(10.1, 13.02752673863323715481371189614224148681183971709386)]
        public void GammaLn(double x, double f)
        {
            AssertHelpers.AlmostEqual(f, SpecialFunctions.GammaLn(x), 14);
        }

        /// <summary>
        /// Gamma function.
        /// </summary>
        /// <param name="x">Input X value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(Double.NaN, Double.NaN)]
        [TestCase(-1.5, 2.3632718012073547030642233111215269103967326081631802)]
        [TestCase(-0.5, -3.544907701811032054596334966682290365595098912244773)]
        [TestCase(0.1, 9.5135076986687312858079798958252325009137161063903012)]
        [TestCase(1.0, 1.0)]
        [TestCase(1.5, 0.88622692545275801364908374167057259139877472806119326)]
        [TestCase(Constants.Pi / 2, 0.89056089038153932801065963535912100593354196288475879)]
        [TestCase(2.0, 1.0)]
        [TestCase(2.5, 1.3293403881791370204736256125058588870981620920917912)]
        [TestCase(3.0, 2.0)]
        [TestCase(Constants.Pi, 2.2880377953400324179595889090602339228896881533562229)]
        [TestCase(3.5, 3.3233509704478425511840640312646472177454052302294767)]
        [TestCase(4.0, 6.0)]
        [TestCase(4.5, 11.631728396567448929144224109426265262108918305803166)]
        [TestCase(5.0, 24.0)]
        [TestCase(5.5, 52.342777784553520181149008492418193679490132376114268)]
        [TestCase(10.1, 454760.75144158558537612486797710217749925965322893332)]
        public void Gamma(double x, double f)
        {
            AssertHelpers.AlmostEqual(f, SpecialFunctions.Gamma(x), 13);
        }

        /// <summary>
        /// Gamma lower regularized.
        /// </summary>
        /// <param name="a">Value of A parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(double.NaN, Double.NaN, Double.NaN)]
        [TestCase(0.1, 1.0, 0.97587265627367222115949155252812057714751052498477013)]
        [TestCase(0.1, 2.0, 0.99432617602018847196075251078067514034772764693462125)]
        [TestCase(0.1, 8.0, 0.99999507519205198048686442150578226823401842046310854)]
        [TestCase(1.5, 1.0, 0.42759329552912016600095238564127189392715996802703368)]
        [TestCase(1.5, 2.0, 0.73853587005088937779717792402407879809718939080920993)]
        [TestCase(1.5, 8.0, 0.99886601571021467734329986257903021041757398191304284)]
        [TestCase(2.5, 1.0, 0.15085496391539036377410688601371365034788861473418704)]
        [TestCase(2.5, 2.0, 0.45058404864721976739416885516693969548484517509263197)]
        [TestCase(2.5, 8.0, 0.99315592607757956900093935107222761316136944145439676)]
        [TestCase(5.5, 1.0, 0.0015041182825838038421585211353488839717739161316985392)]
        [TestCase(5.5, 2.0, 0.030082976121226050615171484772387355162056796585883967)]
        [TestCase(5.5, 8.0, 0.85886911973294184646060071855669224657735916933487681)]
        public void GammaLowerRegularized(double a, double x, double f)
        {
            AssertHelpers.AlmostEqual(f, SpecialFunctions.GammaLowerRegularized(a, x), 14);
        }

        /// <summary>
        /// Gamma lower incomplete.
        /// </summary>
        /// <param name="a">Value of A parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(double.NaN, Double.NaN, Double.NaN)]
        [TestCase(0.1, 1.0, 9.2839720283798852469443229940217320532607158711056334)]
        [TestCase(0.1, 2.0, 9.4595297305559030536119885480983751098528458886962883)]
        [TestCase(0.1, 8.0, 9.5134608464704033372127589212547718314010339263844976)]
        [TestCase(1.5, 1.0, 0.37894469164098470380394366597039213790868855578083847)]
        [TestCase(1.5, 2.0, 0.65451037345177732033319477475056262302270310457635612)]
        [TestCase(1.5, 8.0, 0.88522195804210983776635107858848816480298923071075222)]
        [TestCase(2.5, 1.0, 0.20053759629003473411039172879412733941722170263949)]
        [TestCase(2.5, 2.0, 0.59897957413602228465664030130712917348327070206302442)]
        [TestCase(2.5, 8.0, 1.3202422842943799358198434659248530581833764879301293)]
        [TestCase(5.5, 1.0, 0.078729729026968321691794205337720556329618007004848672)]
        [TestCase(5.5, 2.0, 1.5746265342113649473739798668921124454837064926448459)]
        [TestCase(5.5, 8.0, 44.955595480196465884619737757794960132425035578313584)]
        public void GammaLowerIncomplete(double a, double x, double f)
        {
            AssertHelpers.AlmostEqual(f, SpecialFunctions.GammaLowerIncomplete(a, x), 14);
        }

        /// <summary>
        /// Gamma upper regularized.
        /// </summary>
        /// <param name="a">Value of A parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(double.NaN, Double.NaN, Double.NaN)]
        [TestCase(0.1, 1.0, 0.024127343726327778840508447471879422852489475015229)]
        [TestCase(0.1, 2.0, 0.0056738239798115280392474892193248596522723530653781)]
        [TestCase(0.1, 8.0, 0.0000049248079480195131355784942177317659815795368919702)]
        [TestCase(1.5, 1.0, 0.57240670447087983399904761435872810607284003197297)]
        [TestCase(1.5, 2.0, 0.26146412994911062220282207597592120190281060919079)]
        [TestCase(1.5, 8.0, 0.0011339842897853226567001374209697895824260180869567)]
        [TestCase(2.5, 1.0, 0.84914503608460963622589311398628634965211138526581)]
        [TestCase(2.5, 2.0, 0.54941595135278023260583114483306030451515482490737)]
        [TestCase(2.5, 8.0, 0.0068440739224204309990606489277723868386305585456026)]
        [TestCase(5.5, 1.0, 0.9984958817174161961578414788646511160282260838683)]
        [TestCase(5.5, 2.0, 0.96991702387877394938482851522761264483794320341412)]
        [TestCase(5.5, 8.0, 0.14113088026705815353939928144330775342264083066512)]
        public void GammaUpperRegularized(double a, double x, double f)
        {
            AssertHelpers.AlmostEqual(f, SpecialFunctions.GammaUpperRegularized(a, x), 14);
        }

        /// <summary>
        /// Gamma upper incomplete.
        /// </summary>
        /// <param name="a">Value of A parameter.</param>
        /// <param name="x">Input X value.</param>
        /// <param name="f">Function result.</param>
        [TestCase(double.NaN, Double.NaN, Double.NaN)]
        [TestCase(0.1, 1.0, 0.22953567028884603886365690180350044765300023528467)]
        [TestCase(0.1, 2.0, 0.053977968112828232195991347726857391060870217694027)]
        [TestCase(0.1, 8.0, 0.000046852198327948595220974570460669512682180005810156)]
        [TestCase(1.5, 1.0, 0.50728223381177330984514007570018045349008617228036)]
        [TestCase(1.5, 2.0, 0.23171655200098069331588896692000996837607162348484)]
        [TestCase(1.5, 8.0, 0.0010049674106481758827326630820844265957854973504417)]
        [TestCase(2.5, 1.0, 1.1288027918891022863632338837117315476809403894523)]
        [TestCase(2.5, 2.0, 0.73036081404311473581698531119872971361489139002877)]
        [TestCase(2.5, 8.0, 0.0090981038847570846537821465810058289147856041616617)]
        [TestCase(5.5, 1.0, 52.264048055526551859457214287080473123160514369109)]
        [TestCase(5.5, 2.0, 50.768151250342155233775028625526081234006425883469)]
        [TestCase(5.5, 8.0, 7.3871823043570542965292707346232335470650967978006)]
        public void GammaUpperIncomplete(double a, double x, double f)
        {
            AssertHelpers.AlmostEqual(f, SpecialFunctions.GammaUpperIncomplete(a, x), 14);
        }
    }
}
