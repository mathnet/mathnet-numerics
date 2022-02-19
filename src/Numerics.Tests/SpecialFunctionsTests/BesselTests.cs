using System;
using NUnit.Framework;
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics.Tests.SpecialFunctionsTests
{
    /// <summary>
    /// Bessel functions tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class BesselTests
    {
        #region Bessel J

        [Test]
        public void BesselJ0Approx([Range(-3, 3, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.4.1
            Assert.AreEqual(Polynomial.Evaluate(x / 3.0, 1.0, 0.0, -2.2499997, 0.0, 1.2656208, 0.0, -0.3163866, 0.0, 0.0444479, 0.0, -0.0039444, 0.0, 0.0002100), SpecialFunctions.BesselJ(0, x), 1e-7);
        }

        [TestCase(0, 0.0, 0.0, 1.0000000000000000000, 0.0000000000000000000, 14)]
        [TestCase(0, 0.25, 0.0, 0.98443592929585270492, 0.0000000000000000000, 14)]
        [TestCase(0, 0.5, 0.0, 0.93846980724081290423, 0.0000000000000000000, 14)]
        [TestCase(0, 1.0, 0.0, 0.76519768655796655145, 0.0000000000000000000, 14)]
        [TestCase(0, -1.0, 0.0, 0.76519768655796655145, 0.0000000000000000000, 14)]
        [TestCase(0, 0.0, 1.0, 1.2660658777520083356, 0.0000000000000000000, 14)]
        [TestCase(0, 0.0, -1.0, 1.2660658777520083356, 0.0000000000000000000, 14)]
        [TestCase(0, 1.0, 1.0, 0.93760847680602927660, -0.49652994760912213217, 14)]
        [TestCase(0, -1.0, -1.0, 0.93760847680602927660, -0.49652994760912213217, 14)]
        [TestCase(0, 10.0, 5.0, -17.789591129450371518, 0.20071161672120485098, 13)]
        [TestCase(0, -10.0, -5.0, -17.789591129450371518, 0.20071161672120485098, 13)]
        [TestCase(0.001, 1.0, 1.0, 0.93830680591878982122, -0.49541402042792734422, 14)]
        [TestCase(1, 0.0, -10.0, 0.0000000000000000000, -2670.9883037012546543, 12)]
        [TestCase(1, 10.0, 5.0, -0.92143143564032744340, -17.436439610477507252, 13)]
        [TestCase(-1, 10.0, 5.0, 0.92143143564032744339, 17.436439610477507252, 13)]
        [TestCase(1, 100.0, 100.0, -7.1708320564209428560E41, 5.4401809306071216972E41, 14)]
        [TestCase(2, 4.0, 0.0, 0.36412814585207280421, 0.0000000000000000000, 14)]
        [TestCase(2, 32.0, -64.0, -2.6838844111822281863E26, -1.0228528267491243961E26, 14)]
        [TestCase(100, 1.0, 1.0, -9.5168076700036928783E-174, 4.7113294059604970982E-176, 14)]
        public void ComplexBesselJnExact(double v, double zr, double zi, double cyr, double cyi, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(cyr, cyi),
                SpecialFunctions.BesselJ(v, new Complex(zr, zi)),
                decimalPlaces
                );
        }

        #endregion

        #region Bessel Y

        [Test]
        public void BesselY0Approx([Range(0.25, 3, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.4.2
            Assert.AreEqual(
                Polynomial.Evaluate(x / 3.0, 2.0 / Math.PI * Math.Log(x / 2.0) * SpecialFunctions.BesselJ(0, x) + 0.36746691, 0.0, 0.60559366, 0.0, -0.74350384, 0.0, 0.25300117, 0.0, -0.04261214, 0.0, 0.00427916, 0.0, -0.00024846),
                SpecialFunctions.BesselY(0, x), 1e-7);
        }

        [TestCase(0, 0.0, 0.0, double.NegativeInfinity, 0.0, 14)]
        [TestCase(0, 1.0, 0.0, 0.088256964215676957983, 0.0, 14)]
        [TestCase(0, 0.0, 1.0, -0.26803248203398854876, 1.2660658777520083356, 14)]
        [TestCase(0, 1.0, 1.0, 0.44547448893603251403, 0.71015858200373452118, 14)]
        [TestCase(0, 10.0, 5.0, -0.20001363358737298600, -17.788152066750989245, 13)]
        [TestCase(0, -10.0, -5.0, 0.20140959985503671596, 17.791030192149753792, 13)]
        [TestCase(0.001, 1.0, 1.0, 0.44400137479127562106, 0.71093732860971252181, 14)]
        [TestCase(1, 10.0, 5.0, 17.437935555053368400, -0.92208733599150420146, 13)]
        [TestCase(-1, 10.0, 5.0, -17.437935555053368400, 0.92208733599150420146, 13)]
        [TestCase(1, 100.0, 100.0, -5.4401809306071216972E41, -7.1708320564209428560E41, 14)]
        [TestCase(2, 4.0, 0.0, 0.21590359460361499453, 0.0, 14)]
        [TestCase(2, 32.0, -64.0, -1.0228528267491243961E26, 2.6838844111822281863E26, 14)]
        [TestCase(100, 1.0, 1.0, 3.3446291442187872047E170, 1.6892209981554826575E168, 10)]
        public void ComplexBesselYnExact(double v, double zr, double zi, double cyr, double cyi, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(cyr, cyi),
                SpecialFunctions.BesselY(v, new Complex(zr, zi)),
                decimalPlaces
                );
        }

        #endregion

        #region Modified Bessel I

        [Test]
        public void BesselI0Approx([Range(-3.75, 3.75, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.8.1
            Assert.AreEqual(Polynomial.Evaluate(x / 3.75, 1.0, 0.0, 3.5156229, 0.0, 3.0899424, 0.0, 1.2067492, 0.0, 0.2659732, 0.0, 0.0360768, 0.0, 0.0045813), SpecialFunctions.BesselI(0, x), 1e-7);
        }

        [TestCase(0.0, 1.0)]
        [TestCase(0.005, 1.000006250009766)]
        [TestCase(0.5, 1.063483370741324)]
        [TestCase(1.5, 1.646723189772891)]
        [TestCase(10.0, 2815.716628466254)]
        [TestCase(100.0, 1.073751707131074e+42)]
        [TestCase(-0.005, 1.000006250009766)]
        [TestCase(-10.0, 2815.716628466254)]
        public void BesselI0Exact(double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselI(0, x), 14);
        }

        [Test]
        public void BesselI1Approx([Range(-3.75, 3.75, 0.25)] double x)
        {
            // Approx by Abramowitz/Stegun 9.8.3
            Assert.AreEqual(Polynomial.Evaluate(x / 3.75, 0.5, 0.0, 0.87890594, 0.0, 0.51498869, 0.0, 0.15084934, 0.0, 0.02658733, 0.0, 0.00301532, 0.0, 0.00032411) * x, SpecialFunctions.BesselI(1, x), 1e-8);
        }

        [TestCase(0.0, 0.0)]
        [TestCase(0.005, 0.002500007812508138)]
        [TestCase(0.5, 0.2578943053908963)]
        [TestCase(1.5, 0.9816664285779076)]
        [TestCase(10.0, 2670.988303701255)]
        [TestCase(100.0, 1.068369390338162e+42)]
        [TestCase(-0.005, -0.002500007812508138)]
        [TestCase(-10.0, -2670.988303701255)]
        public void BesselI1Exact(double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselI(1, x), 14);
        }

        [TestCase(0, +10, 2815.7166284662544715)]
        [TestCase(0, -10, 2815.7166284662544715)]
        [TestCase(1, +10, 2670.9883037012546543)]
        [TestCase(1, -10, -2670.9883037012546543)]
        [TestCase(2, +10, 2281.5189677260035406)]
        [TestCase(2, -10, 2281.5189677260035406)]
        [TestCase(3, +10, 1758.3807166108532381)]
        [TestCase(3, -10, -1758.3807166108532381)]
        [TestCase(4, +10, 1226.4905377594915977)]
        [TestCase(4, -10, 1226.4905377594915977)]
        [TestCase(5, +10, 777.18828640325995991)]
        [TestCase(5, -10, -777.18828640325995991)]
        public void BesselInExact(int n, double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselI(n, x), 14);
        }

        [TestCase(0, 0.0, 0.0, 1.0, 0.0, 14)]
        [TestCase(0, 1.0, 0.0, 1.2660658777520083356, 0.0, 14)]
        [TestCase(0, -1.0, 0.0, 1.2660658777520083356, 0.0, 14)]
        [TestCase(0, 0.0, 1.0, 0.7651976865579665514497, 0.0, 14)]
        [TestCase(0, 0.0, -1.0, 0.7651976865579665514497, 0.0, 14)]
        [TestCase(0, 1.0, 1.0, 0.93760847680602927660, 0.49652994760912213217, 14)]
        [TestCase(0, 10.0, 5.0, 133.59268491136563443, -2651.8719709592179510, 14)]
        [TestCase(0, -10.0, -5.0, 133.59268491136563443, -2651.8719709592179510, 14)]
        [TestCase(0.001, 1.0, 1.0, 0.93752745412589393347, 0.49688729751353492126, 14)]
        [TestCase(1, 10.0, 5.0, 183.59643950073504037, -2541.3865266173166975, 14)]
        [TestCase(-1, 10.0, 5.0, 183.59643950073504037, -2541.3865266173166975, 14)]
        [TestCase(1, 100.0, 100.0, 5.4401809306071216972E41, -7.1708320564209428560E41, 14)]
        [TestCase(2, 4.0, 0.0, 6.4221893752841055416, 0.0, 14)]
        [TestCase(2, 32.0, -64.0, 2.9566228902322313083E12, -2.1928744176387872855E12, 14)]
        [TestCase(100, 1.0, 1.0, -9.5168076700036928783E-174, -4.7113294059604970982E-176, 14)]
        [TestCase(100, -1.0, -1.0, -9.5168076700036928783E-174, -4.7113294059604970982E-176, 14)]
        public void ComplexBesselInExact(double v, double zr, double zi, double cyr, double cyi, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(cyr, cyi),
                SpecialFunctions.BesselI(v, new Complex(zr, zi)),
                decimalPlaces
                );
        }

        #endregion

        #region Modified Bessel K

        [Test]
        public void BesselK0Approx([Range(0.20, 2.0, 0.20)] double x)
        {
            // Approx by Abramowitz/Stegun 9.8.5
            Assert.AreEqual(Polynomial.Evaluate(x / 2.0, -Math.Log(x / 2.0) * SpecialFunctions.BesselI(0, x) - 0.57721566, 0.0, 0.42278420, 0.0, 0.23069756, 0.0, 0.03488590, 0.0, 0.00262698, 0.0, 0.00010750, 0.0, 0.00000740), SpecialFunctions.BesselK(0, x), 1e-8);
        }

        [TestCase(1e-10, 23.14178244559887)]
        [TestCase(1e-5, 11.62885698094436)]
        [TestCase(0.005, 5.414288971329485)]
        [TestCase(0.5, 0.9244190712276659)]
        [TestCase(1.5, 0.2138055626475257)]
        [TestCase(10.0, 0.00001778006231616765)]
        [TestCase(100.0, 4.656628229175902e-45)]
        public void BesselK0Exact(double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselK(0, x), 14);
        }

        [Test]
        public void BesselK1Approx([Range(0.20, 2.0, 0.20)] double x)
        {
            // Approx by Abramowitz/Stegun 9.8.7
            Assert.AreEqual(Polynomial.Evaluate(x / 2.0, x * Math.Log(x / 2.0) * SpecialFunctions.BesselI(1, x) + 1.0, 0.0, 0.15443144, 0.0, -0.67278579, 0.0, -0.18156897, 0.0, -0.01919402, 0.0, -0.00110404, 0.0, -0.00004686), SpecialFunctions.BesselK(1, x) * x, 1e-8);
        }

        [TestCase(1e-10, 1.0e+10)]
        [TestCase(1e-5, 99999.99993935572)]
        [TestCase(0.005, 199.9852143257300)]
        [TestCase(0.5, 1.656441120003301)]
        [TestCase(1.5, 0.2773878004568438)]
        [TestCase(10.0, 0.00001864877345382558)]
        [TestCase(100.0, 4.679853735636909e-45)]
        public void BesselK1Exact(double x, double expected)
        {
            AssertHelpers.AlmostEqualRelative(expected, SpecialFunctions.BesselK(1, x), 14);
        }

        [TestCase(0, 0.0, 0.0, double.PositiveInfinity, 0.0, 14)]
        [TestCase(0, 1.0, 0.0, 0.42102443824070833334, 0.0, 14)]
        [TestCase(0, -1.0, 0.0, 0.42102443824070833334, -3.9774632605064226373, 14)]
        [TestCase(0, 0.0, 1.0, -0.13863371520405399968, -1.2019697153172064991, 14)]
        [TestCase(0, 0.0, -1.0, -0.13863371520405399968, 1.2019697153172064991, 14)]
        [TestCase(0, 1.0, 1.0, 0.080197726946517818727, -0.35727745928533025061, 13)]
        [TestCase(0, -1.0, -1.0, -1.4796971087496251933, 2.5883064433920073708, 14)]
        [TestCase(0, 10.0, 5.0, 8.2975524594259100800E-6, 0.000014668532910602615041, 13)]
        [TestCase(0, -10.0, -5.0, 8331.1015105237170946, 419.69381215941520621, 8)]
        [TestCase(0.001, 1.0, 1.0, 0.080197683816936985794, -0.35727755473115169498, 14)]
        [TestCase(1, 10.0, 5.0, 8.9074008179539983668E-6, 0.000015086785431163546150, 13)]
        [TestCase(-1, 10.0, 5.0, 8.9074008179539983668E-6, 0.000015086785431163546150, 13)]
        [TestCase(1, 100.0, 100.0, 3.8914899953028699000E-45, 5.3411641462767852385E-46, 14)]
        [TestCase(-1, 100.0, 100.0, 3.8914899953028699000E-45, 5.3411641462767852385E-46, 14)]
        [TestCase(2, 4.0, 0.0, 0.017401425529487240005, 0.0, 14)]
        [TestCase(2, 32.0, -64.0, -3.2911230526496297606E-16, 1.8699562198185709108E-15, 14)]
        [TestCase(70, -32.0, 0.0, 1.1846697842374187940E12, -1.7227016660469842981E-14, 14)]
        [TestCase(70, 0.0, -1000.0, -0.019114301898634193541, -0.034775023193538639117, 13)]
        [TestCase(70, -100.0, -100.0, 1.0618924726225417382E37, 1.3273296182243209319E36, 11)]
        [TestCase(100, 1.0, 1.0, -5.2537311742300294807E170, 2.6534221390474409958E168, 10)]
        public void ComplexBesselKnExact(double v, double zr, double zi, double cyr, double cyi, int decimalPlaces)
        {
            AssertHelpers.AlmostEqualRelative(
                new Complex(cyr, cyi),
                SpecialFunctions.BesselK(v, new Complex(zr, zi)),
                decimalPlaces
                );
        }

        #endregion

        #region Ratio of Bessel I : I(n + 1, z) / I(n, z)

        [TestCase(0, 1, 1, 0.57495795977223997079, 0.35054769385125934266, 14)]
        [TestCase(0, 10, 10, 0.97503660846868673171, 0.025655591609138262390, 14)]
        [TestCase(0, 100, 100, 0.99750003174335729017, 0.0025062812447888833561, 12)]
        [TestCase(0, 1E6, 1E6, 0.99999975000000000003, 2.5000006250003125000E-7, 8)] // cf. BesselI(0, 1e6 + 1e6 j) = 7.4E434290 - 6.9E434290 j and BesselI(1, 1e6 + 1e6 j) = 7.4E434290 - 6.9E434290 j
        public void BesselIRatioExact(int n, double zr, double zi, double cyr, double cyi, int decimalPlaces)
        {
            var z = new Complex(zr, zi);
            var actual = SpecialFunctions.BesselIScaled(n + 1, z) / SpecialFunctions.BesselIScaled(n, z);
            AssertHelpers.AlmostEqualRelative(new Complex(cyr, cyi), actual, decimalPlaces);
        }

        #endregion

        #region Ratio of Bessel K : K(n + 1, z) / K(n, z)

        [TestCase(0, 1E-6, 1E-6, 38803.844378426554737, -34562.243460439510287, 14)]
        [TestCase(0, 1, 1, 1.2397012468882428039, -0.20950920530836317445, 14)]
        [TestCase(0, 10, 10, 1.0249731393042359657, -0.024405853995209668702, 14)]
        [TestCase(0, 100, 100, 1.0024999692332050665, -0.0024937812450508461147, 12)]
        [TestCase(0, 1E6, 1E6, 1.0000002500000000000, -2.4999993750003125000E-7, 9)]
        public void BesselKRatioExact(int n, double zr, double zi, double cyr, double cyi, int decimalPlaces)
        {
            var z = new Complex(zr, zi);
            var actual = SpecialFunctions.BesselKScaled(n + 1, z) / SpecialFunctions.BesselKScaled(n, z);
            AssertHelpers.AlmostEqualRelative(new Complex(cyr, cyi), actual, decimalPlaces);
        }

        #endregion
    }
}
