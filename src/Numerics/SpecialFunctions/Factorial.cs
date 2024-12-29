// <copyright file="Factorial.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using System;
using BigInteger = System.Numerics.BigInteger;

// ReSharper disable once CheckNamespace
namespace MathNet.Numerics
{
    public partial class SpecialFunctions
    {
        static readonly double[] _factorialCache = new double[]
        {
            1, 1, 2, 6, 24, 120, 720, 5040, 40320, 362880, 3628800, 39916800, 479001600, 6227020800, 87178291200, 1307674368000, 20922789888000, 355687428096000, 6402373705728000, 1.21645100408832E+17, 2.43290200817664E+18, 5.109094217170944E+19, 1.1240007277776077E+21, 2.5852016738884978E+22, 6.2044840173323941E+23, 1.5511210043330986E+25, 4.0329146112660565E+26, 1.0888869450418352E+28, 3.0488834461171384E+29, 8.8417619937397008E+30, 2.6525285981219103E+32, 8.2228386541779224E+33, 2.6313083693369352E+35, 8.6833176188118859E+36, 2.9523279903960412E+38, 1.0333147966386144E+40, 3.7199332678990118E+41, 1.3763753091226343E+43, 5.2302261746660104E+44, 2.0397882081197442E+46, 8.1591528324789768E+47, 3.3452526613163803E+49, 1.4050061177528798E+51, 6.0415263063373834E+52, 2.6582715747884485E+54, 1.1962222086548019E+56, 5.5026221598120885E+57, 2.5862324151116818E+59, 1.2413915592536073E+61, 6.0828186403426752E+62, 3.0414093201713376E+64, 1.5511187532873822E+66, 8.0658175170943877E+67, 4.2748832840600255E+69, 2.3084369733924138E+71, 1.2696403353658276E+73, 7.1099858780486348E+74, 4.0526919504877221E+76, 2.3505613312828789E+78, 1.3868311854568986E+80, 8.3209871127413916E+81, 5.0758021387722484E+83, 3.1469973260387939E+85, 1.9826083154044401E+87, 1.2688693218588417E+89, 8.2476505920824715E+90, 5.4434493907744307E+92, 3.6471110918188683E+94, 2.4800355424368305E+96, 1.711224524281413E+98, 1.197857166996989E+100, 8.5047858856786218E+101, 6.1234458376886077E+103, 4.4701154615126834E+105, 3.3078854415193856E+107, 2.4809140811395391E+109, 1.8854947016660498E+111, 1.4518309202828584E+113, 1.1324281178206295E+115, 8.9461821307829729E+116, 7.1569457046263779E+118, 5.7971260207473655E+120, 4.7536433370128398E+122, 3.9455239697206569E+124, 3.314240134565352E+126, 2.8171041143805494E+128, 2.4227095383672724E+130, 2.1077572983795269E+132, 1.8548264225739836E+134, 1.6507955160908452E+136, 1.4857159644817607E+138, 1.3520015276784023E+140, 1.24384140546413E+142, 1.1567725070816409E+144, 1.0873661566567424E+146, 1.0329978488239052E+148, 9.916779348709491E+149, 9.6192759682482062E+151, 9.426890448883242E+153, 9.3326215443944096E+155, 9.3326215443944102E+157, 9.4259477598383536E+159, 9.6144667150351211E+161, 9.9029007164861754E+163, 1.0299016745145622E+166, 1.0813967582402903E+168, 1.1462805637347078E+170, 1.2265202031961373E+172, 1.3246418194518284E+174, 1.4438595832024928E+176, 1.5882455415227421E+178, 1.7629525510902437E+180, 1.9745068572210728E+182, 2.2311927486598123E+184, 2.5435597334721862E+186, 2.9250936934930141E+188, 3.3931086844518965E+190, 3.969937160808719E+192, 4.6845258497542883E+194, 5.5745857612076033E+196, 6.6895029134491239E+198, 8.09429852527344E+200, 9.8750442008335976E+202, 1.2146304367025325E+205, 1.5061417415111404E+207, 1.8826771768889254E+209, 2.3721732428800459E+211, 3.0126600184576582E+213, 3.8562048236258025E+215, 4.9745042224772855E+217, 6.4668554892204716E+219, 8.4715806908788174E+221, 1.1182486511960039E+224, 1.4872707060906852E+226, 1.9929427461615181E+228, 2.6904727073180495E+230, 3.6590428819525472E+232, 5.0128887482749898E+234, 6.9177864726194859E+236, 9.6157231969410859E+238, 1.346201247571752E+241, 1.8981437590761701E+243, 2.6953641378881614E+245, 3.8543707171800706E+247, 5.5502938327393013E+249, 8.0479260574719866E+251, 1.1749972043909099E+254, 1.7272458904546376E+256, 2.5563239178728637E+258, 3.8089226376305671E+260, 5.7133839564458505E+262, 8.6272097742332346E+264, 1.3113358856834518E+267, 2.0063439050956811E+269, 3.0897696138473489E+271, 4.7891429014633912E+273, 7.4710629262828905E+275, 1.1729568794264138E+278, 1.8532718694937338E+280, 2.9467022724950369E+282, 4.714723635992059E+284, 7.5907050539472148E+286, 1.2296942187394488E+289, 2.0044015765453015E+291, 3.2872185855342945E+293, 5.423910666131586E+295, 9.0036917057784329E+297, 1.5036165148649983E+300, 2.5260757449731969E+302, 4.2690680090047027E+304, 7.257415615307994E+306
        };
        static readonly double[] _factorialLnCache = new double[]
        {
                             0.0,                  0.0,0.6931471805599453094, 1.791759469228055001,
             3.17805383034794562, 4.787491742782045994, 6.579251212010100995,   8.5251613610654143,
            10.60460290274525023, 12.80182748008146961,  15.1044125730755153, 17.50230784587388584,
            19.98721449566188615, 22.55216385312342289,  25.1912211827386815, 27.89927138384089157,
             30.6718601060806728, 33.50507345013688888, 36.39544520803305358, 39.33988418719949404,
            42.33561646075348503, 45.38013889847690803, 48.47118135183522388, 51.60667556776437357,
            54.78472939811231919, 58.00360522298051994, 61.26170176100200198, 64.55753862700633106,
            67.88974313718153498, 71.25703896716800901, 74.65823634883016439, 78.09222355331531063,
            81.55795945611503718, 85.05446701758151741,  88.5808275421976788, 92.13617560368709248,
            95.71969454214320248, 99.33061245478742693, 102.9681986145138127, 106.6317602606434591,
            110.3206397147573954, 114.0342117814617032, 117.7718813997450715,  121.533081515438634,
            125.3172711493568951, 129.1239336391272149, 132.9525750356163099, 136.8027226373263685,
            140.6739236482342594,  144.565743946344886, 148.4777669517730321, 152.4095925844973578,
            156.3608363030787852,  160.331128216630907, 164.3201122631951814, 168.3274454484276523,
            172.3527971391628016, 176.3958484069973517,  180.456291417543771, 184.5338288614494905,
            188.6281734236715912, 192.7390472878449024,  196.866181672889994, 201.0093163992815267,
            205.1681994826411985, 209.3425867525368356, 213.5322414945632612, 217.7369341139542272,
             221.956441819130334, 226.1905483237275933, 230.4390435657769523, 234.7017234428182677,
             238.978389561834323, 243.2688490029827142, 247.5729140961868839, 251.8904022097231944,
            256.2211355500095255, 260.5649409718632093,  264.921649798552801, 269.2910976510198225,
            273.6731242856937042, 278.0675734403661429,  282.474292687630396,  286.893133295426994,
            291.3239500942703076,  295.766601350760624, 300.2209486470141318, 304.6868567656687155,
            309.1641935801469219, 313.6528299498790618, 318.1526396202093268, 322.6634991267261769,
            327.1852877037752172, 331.7178871969284731,  336.261181979198477, 340.8150588707990179,
            345.3794070622668541, 349.9541180407702369, 354.5390855194408088, 359.1342053695753988,
            363.7393755555634901, 368.3544960724047496, 372.9794688856890207, 377.6141978739186564,
            382.2585887730600291, 386.9125491232175525, 391.5759882173296196, 396.2488170517915258,
            400.9309482789157455, 405.6222961611448892, 410.3227765269373054, 415.0323067282496396,
            419.7508055995447341, 424.4781934182570747, 429.2143918666515701, 433.9593239950148202,
            438.7129141861211848,  443.475088120918941, 448.2457727453846057, 453.0248962384961351,
            457.8123879812781811, 462.6081785268749222, 467.4121995716081787, 472.2243839269805962,
            477.0446654925856331, 481.8729792298879342, 486.7092611368394122, 491.5534482232980035,
            496.4054784872176207, 501.2652908915792928, 506.1328253420348752, 511.0080226652360267,
            515.8908245878223976, 520.7811737160441514, 525.6790135159950627, 530.5842882944334922,
            535.4969431801695442, 540.4169241059976691, 545.3441777911548738, 550.2786517242855656,
            555.2202941468948698, 560.1690540372730381, 565.1248810948742989, 570.0877257251342061,
            575.0575390247102068, 580.0342727671307812, 585.0178793888391176, 590.0083119756178539,
             595.005524249381969, 600.0094705553274281, 605.0201058494236839, 610.0373856862386082,
            615.0612662070848846,   620.09170412847732, 625.1286567308909492, 630.1720818478101958,
            635.2219378550597329, 640.2781836604080409, 645.3407786934350077, 650.4096828956552392,
            655.4848567108890662, 660.5662610758735292, 665.6538574111059132, 670.7476076119126756,
             675.847474039736874, 680.9534195136374546, 686.0654073019939978,  691.183401114410753,
            696.3073650938140119, 701.4372638087370854, 706.5730622457873471
        };

        /// <summary>
        /// Computes the factorial function x -> x! of an integer number > 0. The function can represent all number up
        /// to 22! exactly, all numbers up to 170! using a double representation. All larger values will overflow.
        /// </summary>
        /// <returns>A value value! for value > 0</returns>
        /// <remarks>
        /// If you need to multiply or divide various such factorials, consider using the logarithmic version
        /// <see cref="FactorialLn"/> instead so you can add instead of multiply and subtract instead of divide, and
        /// then exponentiate the result using <see cref="System.Math.Exp"/>. This will also circumvent the problem that
        /// factorials become very large even for small parameters.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static double Factorial(int x)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Value must be positive (and not zero).");
            }

            if (x < _factorialCache.Length)
            {
                return _factorialCache[x];
            }

            return double.PositiveInfinity;
        }

        /// <summary>
        /// Computes the factorial of an integer.
        /// </summary>
        public static BigInteger Factorial(BigInteger x)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Value must be positive (and not zero).");
            }

            if (x == 0)
            {
                return BigInteger.One;
            }

            BigInteger r = x;
            while (--x > 1)
            {
                r *= x;
            }

            return r;
        }

        /// <summary>
        /// Computes the logarithmic factorial function x -> ln(x!) of an integer number > 0.
        /// </summary>
        /// <returns>A value value! for value > 0</returns>
        public static double FactorialLn(int x)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Value must be positive (and not zero).");
            }

            if (x < _factorialLnCache.Length)
            {
                return _factorialLnCache[x];
            }

            return GammaLn(x + 1.0);
        }

        /// <summary>
        /// Computes the binomial coefficient: n choose k.
        /// </summary>
        /// <param name="n">A nonnegative value n.</param>
        /// <param name="k">A nonnegative value h.</param>
        /// <returns>The binomial coefficient: n choose k.</returns>
        public static double Binomial(int n, int k)
        {
            if (k < 0 || n < 0 || k > n)
            {
                return 0.0;
            }

            return Math.Floor(0.5 + Math.Exp(FactorialLn(n) - FactorialLn(k) - FactorialLn(n - k)));
        }

        /// <summary>
        /// Computes the natural logarithm of the binomial coefficient: ln(n choose k).
        /// </summary>
        /// <param name="n">A nonnegative value n.</param>
        /// <param name="k">A nonnegative value h.</param>
        /// <returns>The logarithmic binomial coefficient: ln(n choose k).</returns>
        public static double BinomialLn(int n, int k)
        {
            if (k < 0 || n < 0 || k > n)
            {
                return double.NegativeInfinity;
            }

            return FactorialLn(n) - FactorialLn(k) - FactorialLn(n - k);
        }

        /// <summary>
        /// Computes the multinomial coefficient: n choose n1, n2, n3, ...
        /// </summary>
        /// <param name="n">A nonnegative value n.</param>
        /// <param name="ni">An array of nonnegative values that sum to <paramref name="n"/>.</param>
        /// <returns>The multinomial coefficient.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="ni"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="n"/> or any of the <paramref name="ni"/> are negative.</exception>
        /// <exception cref="ArgumentException">If the sum of all <paramref name="ni"/> is not equal to <paramref name="n"/>.</exception>
        public static double Multinomial(int n, int[] ni)
        {
            if (n < 0)
            {
                throw new ArgumentException("Value must be positive.", nameof(n));
            }

            if (ni == null)
            {
                throw new ArgumentNullException(nameof(ni));
            }

            int sum = 0;
            double ret = FactorialLn(n);
            for (int i = 0; i < ni.Length; i++)
            {
                if (ni[i] < 0)
                {
                    throw new ArgumentException("Value must be positive.", "ni[" + i + "]");
                }

                ret -= FactorialLn(ni[i]);
                sum += ni[i];
            }

            // Before returning, check that the sum of all elements was equal to n.
            if (sum != n)
            {
                throw new ArgumentException("The chosen parameter set is invalid (probably some value is out of range).", nameof(ni));
            }

            return Math.Floor(0.5 + Math.Exp(ret));
        }
    }
}
