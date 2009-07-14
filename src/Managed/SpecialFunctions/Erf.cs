// <copyright file="Combinatorics.cs" company="Math.NET">
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

namespace MathNet.Numerics
{
    using System;

    public partial class SpecialFunctions
    {
        /// <summary>Calculates the error function.</summary>
        /// <param name="x">The value to evaluate.</param>
        /// <returns>The error function evaluated at given value.</returns>
        /// <remarks>
        /// 	<list type="bullet">
        /// 		<item>returns 1 if <c>x == Double.PositiveInfinity</c>.</item>
        /// 		<item>returns -1 if <c>x == Double.NegativeInfinity</c>.</item>
        /// 	</list>
        /// </remarks>
        public static double Erf(double x)
        {
            if (x == 0)
            {
                return 0;
            }
            if (Double.IsPositiveInfinity(x))
            {
                return 1;
            }
            if (Double.IsNegativeInfinity(x))
            {
                return -1;
            }
            if (Double.IsNaN(x) || Double.IsNaN(x))
            {
                return Double.NaN;
            }

            return ErfImp(x, false);
        }

        /// <summary>Calculates the complementary error function.</summary>
        /// <param name="x">The value to evaluate.</param>
        /// <returns>The complementary error function evaluated at given value.</returns>
        /// <remarks>
        /// 	<list type="bullet">
        /// 		<item>returns 0 if <c>x == Double.PositiveInfinity</c>.</item>
        /// 		<item>returns 2 if <c>x == Double.NegativeInfinity</c>.</item>
        /// 	</list>
        /// </remarks>
        public static double Erfc(double x)
        {
            if (x == 0)
            {
                return 1;
            }
            if (Double.IsPositiveInfinity(x))
            {
                return 0;
            }
            if (Double.IsNegativeInfinity(x))
            {
                return 2;
            }
            if (Double.IsNaN(x) || Double.IsNaN(x))
            {
                return Double.NaN;
            }

            return ErfImp(x,true);
        }


        private static double ErfImp(double z, bool invert)
        {
            if (z < 0)
            {
                if (!invert)
                {
                    return -ErfImp(-z, invert);
                }
                if (z < -0.5)
                {
                    return 2 - ErfImp((-z), invert);
                }
                return 1 + ErfImp(-z, false);
            }

            double result;

            //
            // Big bunch of selection statements now to pick which
            // implementation to use, try to put most likely options
            // first:
            //
            if (z < 0.5)
            {
                //
                // We're going to calculate erf:
                //
                if (z < 1e-10)
                {
                    result = z * 1.125 + z * 0.003379167095512573896158903121545171688;
                }
                else
                {
                    // Worst case absolute error found: 6.688618532e-21
                    double[] n = new double[] { 0.00337916709551257388990745, -0.00073695653048167948530905, -0.374732337392919607868241, 0.0817442448733587196071743, -0.0421089319936548595203468, 0.0070165709512095756344528, -0.00495091255982435110337458, 0.000871646599037922480317225 };
                    double[] d = new double[] { 1, -0.218088218087924645390535, 0.412542972725442099083918, -0.0841891147873106755410271, 0.0655338856400241519690695, -0.0120019604454941768171266, 0.00408165558926174048329689, -0.000615900721557769691924509 };

                    result = z * 1.125 + z * evaluate_polynomial(n, z) / evaluate_polynomial(d, z);
                }
            }
            else if ((z < 110) || ((z < 110) && invert))
            {
                //
                // We'll be calculating erfc:
                //
                invert = !invert;
                double r, b;
                if (z < 0.75)
                {
                    // Worst case absolute error found: 5.582813374e-21
                    double[] n = new double[] { -0.0361790390718262471360258, 0.292251883444882683221149, 0.281447041797604512774415, 0.125610208862766947294894, 0.0274135028268930549240776, 0.00250839672168065762786937 };
                    double[] d = new double[] { 1, 1.8545005897903486499845, 1.43575803037831418074962, 0.582827658753036572454135, 0.124810476932949746447682, 0.0113724176546353285778481 };
                    r = evaluate_polynomial(n, z - 0.5) / evaluate_polynomial(d, z - 0.5);
                    b = 0.3440242112F;
                }
                else if (z < 1.25)
                {
                    // Worst case absolute error found: 4.01854729e-21
                    double[] n = new double[] { -0.0397876892611136856954425, 0.153165212467878293257683, 0.191260295600936245503129, 0.10276327061989304213645, 0.029637090615738836726027, 0.0046093486780275489468812, 0.000307607820348680180548455 };
                   double[] d = new double[] { 1, 1.95520072987627704987886, 1.64762317199384860109595, 0.768238607022126250082483, 0.209793185936509782784315, 0.0319569316899913392596356, 0.00213363160895785378615014 };
                    r = evaluate_polynomial(n, z - 0.75) / evaluate_polynomial(d, z - 0.75);
                    b = 0.419990927F;
                }
                else if (z < 2.25)
                {
                    // Worst case absolute error found: 2.866005373e-21
                    double[] n = new double[] { -0.0300838560557949717328341, 0.0538578829844454508530552, 0.0726211541651914182692959, 0.0367628469888049348429018, 0.00964629015572527529605267, 0.00133453480075291076745275, 0.778087599782504251917881e-4 };
                    double[] d = new double[] { 1, 1.75967098147167528287343, 1.32883571437961120556307, 0.552528596508757581287907, 0.133793056941332861912279, 0.0179509645176280768640766, 0.00104712440019937356634038, -0.106640381820357337177643e-7 };
                    r = evaluate_polynomial(n, z - 1.25) / evaluate_polynomial(d, z - 1.25);
                    b = 0.4898625016F; ;
                }
                else if (z < 3.5)
                {
                    // Worst case absolute error found: 1.045355789e-21
                    double[] n = new double[] { -0.0117907570137227847827732, 0.014262132090538809896674, 0.0202234435902960820020765, 0.00930668299990432009042239, 0.00213357802422065994322516, 0.00025022987386460102395382, 0.120534912219588189822126e-4 };
                    double[] d = new double[] { 1, 1.50376225203620482047419, 0.965397786204462896346934, 0.339265230476796681555511, 0.0689740649541569716897427, 0.00771060262491768307365526, 0.000371421101531069302990367 };
                    r = evaluate_polynomial(n, z - 2.25) / evaluate_polynomial(d, z - 2.25);
                    b = 0.5317370892F;
                }
                else if (z < 5.25)
                {
                    // Worst case absolute error found: 8.300028706e-22
                    double[] n = new double[] { -0.00546954795538729307482955, 0.00404190278731707110245394, 0.0054963369553161170521356, 0.00212616472603945399437862, 0.000394984014495083900689956, 0.365565477064442377259271e-4, 0.135485897109932323253786e-5 };
                    double[] d = new double[] { 1, 1.21019697773630784832251, 0.620914668221143886601045, 0.173038430661142762569515, 0.0276550813773432047594539, 0.00240625974424309709745382, 0.891811817251336577241006e-4, -0.465528836283382684461025e-11 };
                    r = evaluate_polynomial(n, z - 3.5) / evaluate_polynomial(d, z - 3.5);
                    b = 0.5489973426F;
                }
                else if (z < 8)
                {
                    // Worst case absolute error found: 1.700157534e-21
                    double[] n = new double[] { -0.00270722535905778347999196, 0.0013187563425029400461378, 0.00119925933261002333923989, 0.00027849619811344664248235, 0.267822988218331849989363e-4, 0.923043672315028197865066e-6 };
                    double[] d = new double[] { 1, 0.814632808543141591118279, 0.268901665856299542168425, 0.0449877216103041118694989, 0.00381759663320248459168994, 0.000131571897888596914350697, 0.404815359675764138445257e-11 };
                    r = evaluate_polynomial(n, z - 5.25) / evaluate_polynomial(d, z - 5.25);
                    b = 0.5571740866F;
                }
                else if (z < 11.5)
                {
                    //Worst case absolute error found: 3.002278011e-22
                    double[] n = new double[] { -0.00109946720691742196814323, 0.000406425442750422675169153, 0.000274499489416900707787024, 0.465293770646659383436343e-4, 0.320955425395767463401993e-5, 0.778286018145020892261936e-7 };
                    double[] d = new double[] { 1, 0.588173710611846046373373, 0.139363331289409746077541, 0.0166329340417083678763028, 0.00100023921310234908642639, 0.24254837521587225125068e-4 };
                    r = evaluate_polynomial(n, z - 8) / evaluate_polynomial(d, z - 8);
                    b = 0.5609807968F;
                }
                else if (z < 17)
                {
                    //Worst case absolute error found: 6.741114695e-21
                    double[] n = new double[] { -0.00056907993601094962855594, 0.000169498540373762264416984, 0.518472354581100890120501e-4, 0.382819312231928859704678e-5, 0.824989931281894431781794e-7 };
                    double[] d = new double[] { 1, 0.339637250051139347430323, 0.043472647870310663055044, 0.00248549335224637114641629, 0.535633305337152900549536e-4, -0.117490944405459578783846e-12 };
                    r = evaluate_polynomial(n, z - 11.5) / evaluate_polynomial(d, z - 11.5);
                    b = 0.5626493692F;
                }
                else if (z < 24)
                {
                    // Worst case absolute error found: 7.802346984e-22
                    double[] n = new double[] { -0.000241313599483991337479091, 0.574224975202501512365975e-4, 0.115998962927383778460557e-4, 0.581762134402593739370875e-6, 0.853971555085673614607418e-8 };
                    double[] d = new double[] { 1, 0.233044138299687841018015, 0.0204186940546440312625597, 0.000797185647564398289151125, 0.117019281670172327758019e-4 };
                    r = evaluate_polynomial(n, z - 17) / evaluate_polynomial(d, z - 17);
                    b = 0.5634598136F;
                }
                else if (z < 38)
                {
                    // Worst case absolute error found: 2.414228989e-22
                    double[] n = new double[] { -0.000146674699277760365803642, 0.162666552112280519955647e-4, 0.269116248509165239294897e-5, 0.979584479468091935086972e-7, 0.101994647625723465722285e-8 };
                    double[] d = new double[] { 1, 0.165907812944847226546036, 0.0103361716191505884359634, 0.000286593026373868366935721, 0.298401570840900340874568e-5 };
                    r = evaluate_polynomial(n, z - 24) / evaluate_polynomial(d, z - 24);
                    b = 0.5638477802F;
                }
                else if (z < 60)
                {
                    // Worst case absolute error found: 5.896543869e-24
                    double[] n = new double[] { -0.583905797629771786720406e-4, 0.412510325105496173512992e-5, 0.431790922420250949096906e-6, 0.993365155590013193345569e-8, 0.653480510020104699270084e-10 };
                    double[] d = new double[] { 1, 0.105077086072039915406159, 0.00414278428675475620830226, 0.726338754644523769144108e-4, 0.477818471047398785369849e-6 };
                    r = evaluate_polynomial(n, z - 38) / evaluate_polynomial(d, z - 38);
                    b = 0.5640528202F;
                }
                else if (z < 85)
                {
                    // Worst case absolute error found: 3.080612264e-21
                    double[] n = new double[] { -0.196457797609229579459841e-4, 0.157243887666800692441195e-5, 0.543902511192700878690335e-7, 0.317472492369117710852685e-9 };
                    double[] d = new double[] { 1, 0.052803989240957632204885, 0.000926876069151753290378112, 0.541011723226630257077328e-5, 0.535093845803642394908747e-15 };
                    r = evaluate_polynomial(n, z - 60) / evaluate_polynomial(d, z - 60);
                    b = 0.5641309023F;
                }
                else
                {
                    // Worst case absolute error found: 8.094633491e-22
                    double[] n = new double[] { -0.789224703978722689089794e-5, 0.622088451660986955124162e-6, 0.145728445676882396797184e-7, 0.603715505542715364529243e-10 };
                    double[] d = new double[] { 1, 0.0375328846356293715248719, 0.000467919535974625308126054, 0.193847039275845656900547e-5 };
                    r = evaluate_polynomial(n, z - 85) / evaluate_polynomial(d, z - 85);
                    b = 0.5641584396F;
                }
                double g = System.Math.Exp(-z * z) / z;
                result = g * b + g * r;
            }
            else
            {
                //
                // Any value of z larger than 28 will underflow to zero:
                //
                result = 0;
                invert = !invert;
            }

            if (invert)
            {
                result = 1 - result;
            }

            return result;
        }
    }
}
