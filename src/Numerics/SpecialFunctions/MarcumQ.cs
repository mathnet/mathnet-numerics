// <copyright file="MarcumQ.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2022 Math.NET
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

// ReSharper disable once CheckNamespace
namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Marcum-Q functions.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Returns the Marcum Q-function Q[ν](a,b). <a href="https://en.wikipedia.org/wiki/Marcum_Q-function">Marcum Q-function (Wikipedia)</a>
        /// <para>References: A. Gil, J. Segura and N.M. Temme. Efficient and accurate algorithms for the
        /// computation and inversion of the incomplete gamma function ratios. SIAM J Sci Comput. (2012) 34(6), A2965-A2981</para>
        /// </summary>
        /// <param name="nu">The order of generalized Marcum Q-function. Range: 1≦ν≦10000</param>
        /// <param name="a">The value to compute the Marcum Q-function of. Range: 0≦a≦10000</param>
        /// <param name="b">The value to compute the Marcum Q-function of. Range: 0≦b≦10000</param>
        /// <returns>The Marcum Q-function Q[ν](a,b)</returns>
        public static double MarcumQ(double nu,double a,double b)
        {
            MarcumQFunction.Marcum(nu, a, b, out _, out double q, out _);

            return q;
        }

        /// <summary>
        /// Returns the Marcum Q-function Q[ν](a,b). <a href="https://en.wikipedia.org/wiki/Marcum_Q-function">Marcum Q-function (Wikipedia)</a>
        /// <para>References: A. Gil, J. Segura and N.M. Temme. Efficient and accurate algorithms for the
        /// computation and inversion of the incomplete gamma function ratios. SIAM J Sci Comput. (2012) 34(6), A2965-A2981</para>
        /// </summary>
        /// <param name="nu">The order of generalized Marcum Q-function. Range: 1≦ν≦10000</param>
        /// <param name="a">The value to compute the Marcum Q-function of. Range: 0≦a≦10000</param>
        /// <param name="b">The value to compute the Marcum Q-function of. Range: 0≦b≦10000</param>
        /// <param name="err">Error flag
        /// <para>0: Computation succesful.</para>
        /// <para>1: Underflow problems. The function value is set to one.</para>
        /// <para>2: Any of the arguments of the function is out of range.The function value is set to zero.</para>
        /// </param>
        /// <returns>The Marcum Q-function Q[ν](a,b)</returns>
        public static double MarcumQ(double nu, double a, double b, out int err)
        {
            MarcumQFunction.Marcum(nu, a, b, out _, out double q, out err);

            return q;
        }

        /// <summary>
        /// <para>Marcum Q-functions</para>
        /// <para>References</para>
        /// <para>[1] A. Gil, J. Segura and N.M. Temme, Accompanying paper in ACM Trans Math Soft</para>
        /// <para>[2] A. Gil, J. Segura and N.M. Temme. Efficient and accurate algorithms for the
        /// computation and inversion of the incomplete gamma function ratios. SIAM J Sci Comput.
        /// (2012) 34(6), A2965-A2981</para>
        /// </summary>
        internal static class MarcumQFunction
        {
            #region Constants
            /// <summary>
            /// Fortran 95 TINY(0.0d0)
            /// </summary>
            static readonly double Tiny = 2.225073858507201e-308;
            /// <summary>
            /// Fortran 95 HUGE(0.0d0)
            /// </summary>
            static readonly double Huge = 1.7976931348623157e+308;
            /// <summary>
            /// The number 2**(1/4)
            /// </summary>
            static readonly double TwoExp1Over4 = 1.18920711500272106671749997;
            /// <summary>
            /// machine-epsilon
            /// </summary>
            static readonly double MachTol = 2.220446049250313e-16;
            /// <summary>
            /// The number log[e](sqrt(2*pi))
            /// </summary>
            static readonly double LnSqrt2Pi = 0.9189385332046727418;
            /// <summary>
            /// Safe underflow limit
            /// </summary>
            static readonly double Dwarf = Tiny * 10;
            /// <summary>
            /// Safe overflow limit
            /// </summary>
            static readonly double Giant = Huge / 1000;
            /// <summary>
            /// Lower limit of exponential
            /// </summary>
            static readonly double ExpLow = -300;
            /// <summary>
            /// Demanded accuracy
            /// </summary>
            static readonly double Epss = 1e-15;
            #endregion

            /// <summary>
            /// Calculation of the Marcum Q-functions P_mu(x,y) and Q_mu(x,y). <a href="https://en.wikipedia.org/wiki/Marcum_Q-function">Marcum Q-function (Wikipedia)</a>
            /// </summary>
            /// <remarks>
            /// <para>
            /// In order to avoid, overflow/underflow problems in IEEE double precision arithmetic, the admissible parameter ranges for computation are:
            /// </para>
            /// <para>
            /// 0≦x≦10000, 0≦y≦10000, 1≦mu≦10000
            /// </para>
            /// <para>
            /// The aimed relative accuracy is close to 1.0e-11 in the previous parameter domain.
            /// </para>
            /// </remarks>
            /// <param name="mu">argument of the functions</param>
            /// <param name="x">argument of the functions</param>
            /// <param name="y">argument of the functions</param>
            /// <param name="p">function P_mu(a,x)</param>
            /// <param name="q">function Q_mu(a,x)</param>
            /// <param name="ierr">error flag
            /// <para>0: Computation succesful</para>
            /// <para>1: Underflow problems. The function values are set to zero and one.</para>
            /// <para>2: Any of the arguments of the function is out of range.The function values (P_mu(a, x) and Q_mu(a, x)) are set to zero.</para>
            /// </param>
            public static void Marcum(double mu, double x, double y, out double p, out double q, out int ierr)
            {
                ierr = 0;
                p = 0;
                q = 0;

                if ((x > 10000) || (y > 10000) || (mu > 10000))
                {
                    ierr = 2;
                }

                if ((x < 0) || (y < 0) || (mu < 1))
                {
                    ierr = 2;
                }

                ierr = 0;

                if (ierr == 0)
                {
                    double mulim = 135.0;
                    double b = 1.0;
                    double w = b * Math.Sqrt(4 * x + 2 * mu);
                    double xi = 2 * Math.Sqrt(x * y);
                    double y0 = x + mu - w;
                    double y1 = x + mu + w;

                    if ((y > x + mu) && (x < 30))
                    {
                        // Series for Q in terms of ratios of Gamma functions
                        Qser(mu, x, y, out p, out q, out ierr);
                    }
                    else if ((y <= x + mu) && (x < 30))
                    {
                        // Series for P in terms of ratios of Gamma functions
                        Pser(mu, x, y, out p, out q, out ierr);
                    }
                    else if ((mu * mu < 2 * xi) && (xi > 30))
                    {
                        // Asymptotic expansion for xy large
                        PQasyxy(mu, x, y, out p, out q, out ierr);
                    }
                    else if (((mu >= mulim) && (y0 <= y)) && (y <= y1))
                    {
                        // Asymptotic expansion for mu large
                        PQasymu(mu, x, y, out p, out q, out ierr);
                    }
                    else if (((y <= y1) && (y > x + mu)) && (mu < mulim))
                    {
                        // Recurrence relation for Q
                        Qrec(mu, x, y, out p, out q, out ierr);
                    }
                    else if (((y >= y0) && (y <= x + mu)) && (mu < mulim))
                    {
                        // Recurrence relation for P
                        Prec(mu, x, y, out p, out q, out ierr);
                    }
                    else
                    {
                        // Integral representation
                        MarcumPQtrap(mu, x, y, out p, out q, ref ierr);
                    }
                }

                if (ierr == 0)
                {
                    if (p < 1e-290)
                    {
                        p = 0;
                        q = 1;
                        ierr = 1;
                    }

                    if (q < 1e-290)
                    {
                        p = 1;
                        q = 0;
                        ierr = 1;
                    }
                }

            }

            #region Local Functions

            // Evaluation of the cf for the ratio Ipnu(z)/Ipnu-1(z)
            // We use Lentz-Thompson algorithm.
            static double Fc(double pnu, double z)
            {
                int m = 0;
                double b = 2 * pnu / z;
                double a = 1d;
                double fc = Dwarf;
                double c0 = fc;
                double d0 = 0d;
                double delta = 0d;

                while (Math.Abs(delta - 1) > Epss)
                {
                    d0 = b + a * d0;

                    if (Math.Abs(d0) < Dwarf)
                    {
                        d0 = Dwarf;
                    }

                    c0 = b + a / c0;

                    if (Math.Abs(c0) < Dwarf)
                    {
                        c0 = Dwarf;
                    }

                    d0 = 1 / d0;
                    delta = c0 * d0;
                    fc *= delta;
                    m++;
                    a = 1.0;
                    b = 2.0 * (pnu + m) / z;
                }

                return fc;
            }

            // Math.Power??
            static double Factor(double x, int n)
            {
                double facto = 1;

                for (int i = 1; i <= n; i++)
                {
                    facto *= x / i;
                }

                return facto;
            }

            static double Pol(double[] fjkm, int d, double v)
            {
                double s = fjkm[d];
                int m = d;
                while (m > 0)
                {
                    m--;
                    s = s * v + fjkm[m];
                }

                return s;
            }

            static void Fjkproc16(double u, double[,] fjk)
            {
                double[] fjkm = new double[33];
                double[] un = new double[65];

                un[1] = u;
                double v = u * u;
                un[2] = v;

                for (int n = 2; n <= 64; n++)
                {
                    un[n] = u * un[n - 1];
                }

                #region k: 0
                #region [0, 0]
                fjk[0, 0] = 1;
                #endregion
                #region [1, 0]
                fjkm[0] = 0.50000000000000000000;
                fjkm[1] = 0.16666666666666666667;
                SetFjk16(fjk, j: 1, k: 0, un, fjkm, v);
                #endregion
                #region [2, 0]
                fjkm[0] = -0.12500000000000000000;
                fjkm[1] = 0.0;
                fjkm[2] = 0.20833333333333333333;
                SetFjk16(fjk, j: 2, k: 0, un, fjkm, v);
                #endregion
                #region [3, 0]
                fjkm[0] = 0.62500000000000000000e-1;
                fjkm[1] = -0.54166666666666666667e-1;
                fjkm[2] = -0.31250000000000000000;
                fjkm[3] = 0.28935185185185185185;
                SetFjk16(fjk, j: 3, k: 0, un, fjkm, v);
                #endregion
                #region [4, 0]
                fjkm[0] = -0.39062500000000000000e-1;
                fjkm[1] = 0.83333333333333333333e-1;
                fjkm[2] = 0.36631944444444444444;
                fjkm[3] = -0.83333333333333333333;
                fjkm[4] = 0.42390046296296296296;
                SetFjk16(fjk, j: 4, k: 0, un, fjkm, v);
                #endregion
                #region [5, 0]
                fjkm[0] = 0.27343750000000000000e-1;
                fjkm[1] = -0.10145089285714285714;
                fjkm[2] = -0.38281250000000000000;
                fjkm[3] = 1.6061921296296296296;
                fjkm[4] = -1.7903645833333333333;
                fjkm[5] = 0.64144483024691358025;
                SetFjk16(fjk, j: 5, k: 0, un, fjkm, v);
                #endregion
                #region [6, 0]
                fjkm[0] = -0.20507812500000000000e-1;
                fjkm[1] = 0.11354166666666666667;
                fjkm[2] = 0.36983072916666666667;
                fjkm[3] = -2.5763888888888888889;
                fjkm[4] = 4.6821108217592592593;
                fjkm[5] = -3.5607638888888888889;
                fjkm[6] = 0.99199861754115226337;
                SetFjk16(fjk, j: 6, k: 0, un, fjkm, v);
                #endregion
                #region [7, 0]
                fjkm[0] = 0.16113281250000000000e-1;
                fjkm[1] = -0.12196955605158730159;
                fjkm[2] = -0.33297526041666666667;
                fjkm[3] = 3.7101836350859788360;
                fjkm[4] = -9.7124626253858024691;
                fjkm[5] = 11.698143727494855967;
                fjkm[6] = -6.8153513213734567901;
                fjkm[7] = 1.5583573120284636488;
                SetFjk16(fjk, j: 7, k: 0, un, fjkm, v);
                #endregion
                #region [8, 0]
                fjkm[0] = -0.13092041015625000000e-1;
                fjkm[1] = 0.12801339285714285714;
                fjkm[2] = 0.27645252046130952381;
                fjkm[3] = -4.9738777281746031746;
                fjkm[4] = 17.501935105096726190;
                fjkm[5] = -29.549479166666666667;
                fjkm[6] = 26.907133829250257202;
                fjkm[7] = -12.754267939814814815;
                fjkm[8] = 2.4771798425577632030;
                SetFjk16(fjk, j: 8, k: 0, un, fjkm, v);
                #endregion
                #region [9, 0]
                fjkm[0] = 0.10910034179687500000e-1;
                fjkm[1] = -0.13242874035415539322;
                fjkm[2] = -0.20350690569196428571;
                fjkm[3] = 6.3349384739790013228;
                fjkm[4] = -28.662114811111800044;
                fjkm[5] = 63.367483364421434083;
                fjkm[6] = -79.925485618811085391;
                fjkm[7] = 58.757341382271304870;
                fjkm[8] = -23.521455678429623200;
                fjkm[9] = 3.9743166454849898231;
                SetFjk16(fjk, j: 9, k: 0, un, fjkm, v);
                #endregion
                #region [10, 0]
                fjkm[0] = -0.92735290527343750000e-2;
                fjkm[1] = 0.13569064670138888889;
                fjkm[2] = 0.11668911254144670021;
                fjkm[3] = -7.7625075954861111111;
                fjkm[4] = 43.784562625335567773;
                fjkm[5] = -121.31910738398368607;
                fjkm[6] = 198.20121981295421734;
                fjkm[7] = -200.43673900016432327;
                fjkm[8] = 123.80342757950794259;
                fjkm[9] = -42.937783937667895519;
                fjkm[10] = 6.4238224989853211488;
                SetFjk16(fjk, j: 10, k: 0, un, fjkm, v);
                #endregion
                #region [11, 0]
                fjkm[0] = 0.80089569091796875000e-2;
                fjkm[1] = -0.13811212730852318255;
                fjkm[2] = -0.18036238655211433532e-1;
                fjkm[3] = 9.2275853445797140866;
                fjkm[4] = -63.433189058657045718;
                fjkm[5] = 213.60596888977804302;
                fjkm[6] = -432.96183396641609600;
                fjkm[7] = 563.58282810729226948;
                fjkm[8] = -476.64858951490111802;
                fjkm[9] = 254.12602383553942414;
                fjkm[10] = -77.797248335368675787;
                fjkm[11] = 10.446593930548512362;
                SetFjk16(fjk, j: 11, k: 0, un, fjkm, v);
                #endregion
                #region [12, 0]
                fjkm[0] = -0.70078372955322265625e-2;
                fjkm[1] = 0.13990718736965074856;
                fjkm[2] = -0.90802493534784075207e-1;
                fjkm[3] = -10.703046719402920575;
                fjkm[4] = 88.139055705916160082;
                fjkm[5] = -352.55365414896970073;
                fjkm[6] = 860.26747669490580229;
                fjkm[7] = -1381.3884907075539460;
                fjkm[8] = 1497.5262381375579615;
                fjkm[9] = -1089.5695395426785795;
                fjkm[10] = 511.32054028583482617;
                fjkm[11] = -140.15612725058882506;
                fjkm[12] = 17.075450695147740963;
                SetFjk16(fjk, j: 12, k: 0, un, fjkm, v);
                #endregion
                #region [13, 0]
                fjkm[0] = 0.61992406845092773438e-2;
                fjkm[1] = -0.14122658948520402530;
                fjkm[2] = 0.20847570003254474385;
                fjkm[3] = 12.163573370672875144;
                fjkm[4] = -118.39689039212288489;
                fjkm[5] = 552.67487991757989471;
                fjkm[6] = -1587.5976792806460534;
                fjkm[7] = 3052.8623335041016490;
                fjkm[8] = -4067.0706975337409188;
                fjkm[9] = 3781.4312193762993828;
                fjkm[10] = -2415.5306966669781670;
                fjkm[11] = 1012.7298787459738084;
                fjkm[12] = -251.37116645382357870;
                fjkm[13] = 28.031797071713952493;
                SetFjk16(fjk, j: 13, k: 0, un, fjkm, v);
                #endregion
                #region [14, 0]
                fjkm[0] = -0.55350363254547119141e-2;
                fjkm[1] = 0.14217921713372686883;
                fjkm[2] = -0.33386405994128191388;
                fjkm[3] = -13.585546133738642981;
                fjkm[4] = 154.66282442015249412;
                fjkm[5] = -830.71069930083407076;
                fjkm[6] = 2761.0291182562342601;
                fjkm[7] = -6219.8351157050681259;
                fjkm[8] = 9888.1927799238643295;
                fjkm[9] = -11266.694472611704499;
                fjkm[10] = 9175.5017581920039296;
                fjkm[11] = -5225.7429703251833306;
                fjkm[12] = 1980.4053574007652015;
                fjkm[13] = -449.21570290311749301;
                fjkm[14] = 46.189888661376921323;
                SetFjk16(fjk, j: 14, k: 0, un, fjkm, v);
                #endregion
                #region [15, 0]
                fjkm[0] = 0.49815326929092407227e-2;
                fjkm[1] = -0.14284537645361756992;
                fjkm[2] = 0.46603144339556328292;
                fjkm[3] = 14.946922390174830635;
                fjkm[4] = -197.35300817536964730;
                fjkm[5] = 1205.6532423474201960;
                fjkm[6] = -4572.9473467250314847;
                fjkm[7] = 11865.183572985041892;
                fjkm[8] = -22026.784993357215819;
                fjkm[9] = 29873.206689727991728;
                fjkm[10] = -29749.925047590507307;
                fjkm[11] = 21561.076414337110462;
                fjkm[12] = -11081.438701085531999;
                fjkm[13] = 3832.1051284526998677;
                fjkm[14] = -800.40791995840375064;
                fjkm[15] = 76.356879052900946470;
                SetFjk16(fjk, j: 15, k: 0, un, fjkm, v);
                #endregion
                #region [16, 0]
                fjkm[0] = -0.45145140029489994049e-2;
                fjkm[1] = 0.14328537051348750262;
                fjkm[2] = -0.60418804953366830816;
                fjkm[3] = -16.227111168548122706;
                fjkm[4] = 246.84286168977111296;
                fjkm[5] = -1698.7528990888950203;
                fjkm[6] = 7270.2387180078329370;
                fjkm[7] = -21434.839860240815288;
                fjkm[8] = 45694.866035689911070;
                fjkm[9] = -72195.530107556687632;
                fjkm[10] = 85409.022842807474925;
                fjkm[11] = -75563.234444869051891;
                fjkm[12] = 49344.501227769590532;
                fjkm[13] = -23110.149147008741710;
                fjkm[14] = 7349.7909384681412957;
                fjkm[15] = -1422.6485707704091767;
                fjkm[16] = 126.58493346342458430;
                SetFjk16(fjk, j: 16, k: 0, un, fjkm, v);
                #endregion
                #endregion k: 0
                #region k: 1
                #region [0, 1]
                fjkm[0] = 0.12500000000000000000;
                fjkm[1] = 0.0;
                fjkm[2] = -0.20833333333333333333;
                SetFjk16(fjk, j: 0, k: 1, un, fjkm, v);
                #endregion
                #region [1, 1]
                fjkm[0] = -0.62500000000000000000e-1;
                fjkm[1] = 0.14583333333333333333;
                fjkm[2] = 0.52083333333333333333;
                fjkm[3] = -0.65972222222222222222;
                SetFjk16(fjk, j: 1, k: 1, un, fjkm, v);
                #endregion
                #region [2, 1]
                fjkm[0] = 0.46875000000000000000e-1;
                fjkm[1] = -0.25000000000000000000;
                fjkm[2] = -0.69791666666666666667;
                fjkm[3] = 2.5000000000000000000;
                fjkm[4] = -1.6059027777777777778;
                SetFjk16(fjk, j: 2, k: 1, un, fjkm, v);
                #endregion
                #region [3, 1]
                fjkm[0] = -0.39062500000000000000e-1;
                fjkm[1] = 0.34218750000000000000;
                fjkm[2] = 0.72916666666666666667;
                fjkm[3] = -5.6712962962962962963;
                fjkm[4] = 8.1640625000000000000;
                fjkm[5] = -3.5238233024691358025;
                SetFjk16(fjk, j: 3, k: 1, un, fjkm, v);
                #endregion
                #region [4, 1]
                fjkm[0] = 0.34179687500000000000e-1;
                fjkm[1] = -0.42708333333333333333;
                fjkm[2] = -0.59798177083333333333;
                fjkm[3] = 10.208333333333333333;
                fjkm[4] = -24.385308159722222222;
                fjkm[5] = 22.482638888888888889;
                fjkm[6] = -7.3148750964506172840;
                SetFjk16(fjk, j: 4, k: 1, un, fjkm, v);
                #endregion
                #region [5, 1]
                fjkm[0] = -0.30761718750000000000e-1;
                fjkm[1] = 0.50665457589285714286;
                fjkm[2] = 0.29326171875000000000;
                fjkm[3] = -16.044663008432539683;
                fjkm[4] = 56.156774450231481481;
                fjkm[5] = -82.372823832947530864;
                fjkm[6] = 56.160933883101851852;
                fjkm[7] = -14.669405462319958848;
                SetFjk16(fjk, j: 5, k: 1, un, fjkm, v);
                #endregion
                #region [6, 1]
                fjkm[0] = 0.28198242187500000000e-1;
                fjkm[1] = -0.58203125000000000000;
                fjkm[2] = 0.19236328125000000000;
                fjkm[3] = 23.032335069444444444;
                fjkm[4] = -110.33599717881944444;
                fjkm[5] = 227.74508101851851852;
                fjkm[6] = -243.01300676761831276;
                fjkm[7] = 131.66775173611111111;
                fjkm[8] = -28.734679254811814129;
                SetFjk16(fjk, j: 6, k: 1, un, fjkm, v);
                #endregion
                #region [7, 1]
                fjkm[0] = -0.26184082031250000000e-1;
                fjkm[1] = 0.65396069723462301587;
                fjkm[2] = -0.86386369977678571429;
                fjkm[3] = -30.956497628348214286;
                fjkm[4] = 194.54890778287588183;
                fjkm[5] = -527.74348743041776896;
                fjkm[6] = 780.79702721113040123;
                fjkm[7] = -656.29672278886959877;
                fjkm[8] = 295.22178492918917181;
                fjkm[9] = -55.334928257039108939;
                SetFjk16(fjk, j: 7, k: 1, un, fjkm, v);
                #endregion
                #region [8, 1]
                fjkm[0] = 0.24547576904296875000e-1;
                fjkm[1] = -0.72297712053571428571;
                fjkm[2] = 1.7246239871070498512;
                fjkm[3] = 39.546341145833333333;
                fjkm[4] = -316.99617299397786458;
                fjkm[5] = 1081.5824590773809524;
                fjkm[6] = -2074.4037171674994144;
                fjkm[7] = 2398.8177766525205761;
                fjkm[8] = -1664.7533222350236974;
                fjkm[9] = 640.37285196437757202;
                fjkm[10] = -105.19241070496638071;
                SetFjk16(fjk, j: 8, k: 1, un, fjkm, v);
                #endregion
                #region [9, 1]
                fjkm[0] = -0.23183822631835937500e-1;
                fjkm[1] = 0.78948182770700165720;
                fjkm[2] = -2.7769457196432446677;
                fjkm[3] = -48.483511725054617511;
                fjkm[4] = 486.26944746794524016;
                fjkm[5] = -2023.8687997794445650;
                fjkm[6] = 4819.5203340475451309;
                fjkm[7] = -7173.8455521540386687;
                fjkm[8] = 6815.5497547693867415;
                fjkm[9] = -4029.1488859965138299;
                fjkm[10] = 1353.9765257894256969;
                fjkm[11] = -197.95866455017786514;
                SetFjk16(fjk, j: 9, k: 1, un, fjkm, v);
                #endregion
                #region [10, 1]
                fjkm[0] = 0.22024631500244140625e-1;
                fjkm[1] = -0.85378706190321180556;
                fjkm[2] = 4.0223697649378354858;
                fjkm[3] = 57.408728524667245370;
                fjkm[4] = -711.17788174487525874;
                fjkm[5] = 3529.7027186963924024;
                fjkm[6] = -10126.360073656459287;
                fjkm[7] = 18593.571833843032106;
                fjkm[8] = -22636.974191769862737;
                fjkm[9] = 18256.758136740546277;
                fjkm[10] = -9401.7390963482140877;
                fjkm[11] = 2805.1309521324368189;
                fjkm[12] = -369.51173382133760772;
                SetFjk16(fjk, j: 10, k: 1, un, fjkm, v);
                #endregion
                #region [11, 1]
                fjkm[0] = -0.21023511886596679688e-1;
                fjkm[1] = 0.91614229084450603921;
                fjkm[2] = -5.4618897365663646792;
                fjkm[3] = -65.927090730899790043;
                fjkm[4] = 1000.5846459432155138;
                fjkm[5] = -5819.5104958244309663;
                fjkm[6] = 19669.512303383909626;
                fjkm[7] = -43248.109984766956219;
                fjkm[8] = 64686.833219925562541;
                fjkm[9] = -66644.721592787005810;
                fjkm[10] = 46700.224876576248105;
                fjkm[11] = -21305.241783200054197;
                fjkm[12] = 5716.0564388863858560;
                fjkm[13] = -685.13376643364457482;
                SetFjk16(fjk, j: 11, k: 1, un, fjkm, v);
                #endregion
                #region [12, 1]
                fjkm[0] = 0.20147532224655151367e-1;
                fjkm[1] = -0.97675104265089158888;
                fjkm[2] = 7.0960977732520869186;
                fjkm[3] = 73.612404446931816840;
                fjkm[4] = -1363.2529599857205103;
                fjkm[5] = 9163.5749605651064785;
                fjkm[6] = -35864.832027517679572;
                fjkm[7] = 92376.947946883135629;
                fjkm[8] = -164834.83784046136147;
                fjkm[9] = 208040.74214864238663;
                fjkm[10] = -185789.85543054601897;
                fjkm[11] = 115101.05980498683057;
                fjkm[12] = -47134.497747406170101;
                fjkm[13] = 11488.455724263405622;
                fjkm[14] = -1263.2564781342309572;
                SetFjk16(fjk, j: 12, k: 1, un, fjkm, v);
                #endregion
                #region [13, 1]
                fjkm[0] = -0.19372627139091491699e-1;
                fjkm[1] = 1.0357822247248985273;
                fjkm[2] = -8.9252867409545555669;
                fjkm[3] = -80.010760848208515926;
                fjkm[4] = 1807.7010112763722305;
                fjkm[5] = -13886.239779926939526;
                fjkm[6] = 62074.025784691064765;
                fjkm[7] = -184145.36258906485362;
                fjkm[8] = 383582.03973738516554;
                fjkm[9] = -576038.14802241650407;
                fjkm[10] = 628833.57177487054480;
                fjkm[11] = -495573.34466362548232;
                fjkm[12] = 275136.26556037481059;
                fjkm[13] = -102207.94135045741701;
                fjkm[14] = 22823.518594320784899;
                fjkm[15] = -2318.1664194368241801;
                SetFjk16(fjk, j: 13, k: 1, un, fjkm, v);
                #endregion
                #region [14, 1]
                fjkm[0] = 0.18680747598409652710e-1;
                fjkm[1] = -1.0933780262877533843;
                fjkm[2] = 10.949523517716476548;
                fjkm[3] = 84.643531757174997082;
                fjkm[4] = -2342.0651134541361650;
                fjkm[5] = 20369.770814493525849;
                fjkm[6] = -102837.36670370684414;
                fjkm[7] = 346648.70123159565251;
                fjkm[8] = -828961.75261733863230;
                fjkm[9] = 1449716.2069081751493;
                fjkm[10] = -1879301.2063630471735;
                fjkm[11] = 1807331.1927210534022;
                fjkm[12] = -1274496.0479553760313;
                fjkm[13] = 641015.88735632964269;
                fjkm[14] = -217895.35698164739918;
                fjkm[15] = 44894.191761385746325;
                fjkm[16] = -4236.6734164587391641;
                SetFjk16(fjk, j: 14, k: 1, un, fjkm, v);
                #endregion
                #region [15, 1]
                fjkm[0] = -0.18058056011795997620e-1;
                fjkm[1] = 1.1496596058234620572;
                fjkm[2] = -13.168702574894088581;
                fjkm[3] = -87.009904621222723801;
                fjkm[4] = 2973.9704746271364310;
                fjkm[5] = -29057.863221639334277;
                fjkm[6] = 164134.77797509230729;
                fjkm[7] = -621775.71008157787326;
                fjkm[8] = 1684395.4811437516714;
                fjkm[9] = -3374103.7733925392782;
                fjkm[10] = 5084254.0101088150007;
                fjkm[11] = -5797111.7426650438469;
                fjkm[12] = 4981685.5893221501493;
                fjkm[13] = -3178510.0651440248351;
                fjkm[14] = 1461172.7927229457558;
                fjkm[15] = -457795.06653758949338;
                fjkm[16] = 87552.178162658627517;
                fjkm[17] = -7715.5318619797584859;
                SetFjk16(fjk, j: 15, k: 1, un, fjkm, v);
                #endregion
                #endregion k: 1
                #region k: 2
                #region [0, 2]
                fjkm[0] = 0.70312500000000000000e-1;
                fjkm[1] = 0.0;
                fjkm[2] = -0.40104166666666666667;
                fjkm[3] = 0.0;
                fjkm[4] = 0.33420138888888888889;
                SetFjk16(fjk, j: 0, k: 2, un, fjkm, v);
                #endregion
                #region [1, 2]
                fjkm[0] = -0.10546875000000000000;
                fjkm[1] = 0.15234375000000000000;
                fjkm[2] = 1.4036458333333333333;
                fjkm[3] = -1.6710069444444444444;
                fjkm[4] = -1.8381076388888888889;
                fjkm[5] = 2.0609085648148148148;
                SetFjk16(fjk, j: 1, k: 2, un, fjkm, v);
                #endregion
                #region [2, 2]
                fjkm[0] = 0.13183593750000000000;
                fjkm[1] = -0.42187500000000000000;
                fjkm[2] = -2.8623046875000000000;
                fjkm[3] = 8.0208333333333333333;
                fjkm[4] = 1.0777994791666666667;
                fjkm[5] = -14.036458333333333333;
                fjkm[6] = 8.0904586226851851852;
                SetFjk16(fjk, j: 2, k: 2, un, fjkm, v);
                #endregion
                #region [3, 2]
                fjkm[0] = -0.15380859375000000000;
                fjkm[1] = 0.79892578125000000000;
                fjkm[2] = 4.5903320312500000000;
                fjkm[3] = -22.751985677083333333;
                fjkm[4] = 14.934624565972222222;
                fjkm[5] = 42.526662567515432099;
                fjkm[6] = -65.691460503472222222;
                fjkm[7] = 25.746658387988683128;
                SetFjk16(fjk, j: 3, k: 2, un, fjkm, v);
                #endregion
                #region [4, 2]
                fjkm[0] = 0.17303466796875000000;
                fjkm[1] = -1.2773437500000000000;
                fjkm[2] = -6.3615722656250000000;
                fjkm[3] = 50.011935763888888889;
                fjkm[4] = -73.559339735243055556;
                fjkm[5] = -70.026331018518518519;
                fjkm[6] = 271.34066056616512346;
                fjkm[7] = -242.71375868055555556;
                fjkm[8] = 72.412718470695087449;
                SetFjk16(fjk, j: 4, k: 2, un, fjkm, v);
                #endregion
                #region [5, 2]
                fjkm[0] = -0.19033813476562500000;
                fjkm[1] = 1.8524126325334821429;
                fjkm[2] = 7.9220947265625000000;
                fjkm[3] = -94.174715169270833333;
                fjkm[4] = 221.09830050998263889;
                fjkm[5] = 13.578712293836805556;
                fjkm[6] = -765.03722541714891975;
                fjkm[7] = 1204.5108913845486111;
                fjkm[8] = -777.22725008740837191;
                fjkm[9] = 187.66711848589945559;
                SetFjk16(fjk, j: 5, k: 2, un, fjkm, v);
                #endregion
                #region [6, 2]
                fjkm[0] = 0.20619964599609375000;
                fjkm[1] = -2.5202636718750000000;
                fjkm[2] = -8.9979495239257812500;
                fjkm[3] = 159.65856119791666667;
                fjkm[4] = -527.02200527615017361;
                fjkm[5] = 337.21907552083333333;
                fjkm[6] = 1618.7873626708984375;
                fjkm[7] = -4211.0382245852623457;
                fjkm[8] = 4434.1363497656886306;
                fjkm[9] = -2259.2768162856867284;
                fjkm[10] = 458.84770992088928362;
                SetFjk16(fjk, j: 6, k: 2, un, fjkm, v);
                #endregion
                #region [7, 2]
                fjkm[0] = -0.22092819213867187500;
                fjkm[1] = 3.2776113237653459821;
                fjkm[2] = 9.3003209795270647321;
                fjkm[3] = -250.77115683984504175;
                fjkm[4] = 1087.8174260457356771;
                fjkm[5] = -1404.3028911260910976;
                fjkm[6] = -2563.9444452795962738;
                fjkm[7] = 11622.495969086321293;
                fjkm[8] = -17934.344163614699543;
                fjkm[9] = 14479.313178892270800;
                fjkm[10] = -6122.6397406024697386;
                fjkm[11] = 1074.0188194633057124;
                SetFjk16(fjk, j: 7, k: 2, un, fjkm, v);
                #endregion
                #region [8, 2]
                fjkm[0] = 0.23473620414733886719;
                fjkm[1] = -4.1216033935546875000;
                fjkm[2] = -8.5290844099862234933;
                fjkm[3] = 371.57630452473958333;
                fjkm[4] = -2030.4431650042155432;
                fjkm[5] = 3928.2498148600260417;
                fjkm[6] = 2472.6031768756442600;
                fjkm[7] = -26784.706192883150077;
                fjkm[8] = 57707.467479758203765;
                fjkm[9] = -65779.375284558624561;
                fjkm[10] = 43428.755429000357378;
                fjkm[11] = -15731.921483001918296;
                fjkm[12] = 2430.2098720207426574;
                SetFjk16(fjk, j: 8, k: 2, un, fjkm, v);
                #endregion
                #region [9, 2]
                fjkm[0] = -0.24777710437774658203;
                fjkm[1] = 5.0497246547178788619;
                fjkm[2] = 6.3753494648706345331;
                fjkm[3] = -525.77763195628211612;
                fjkm[4] = 3515.3901490500364354;
                fjkm[5] = -9098.9465854134383025;
                fjkm[6] = 1501.4499341175879961;
                fjkm[7] = 52968.402569427411743;
                fjkm[8] = -156962.17039551999834;
                fjkm[9] = 237710.55444046526561;
                fjkm[10] = -217889.76091367761240;
                fjkm[11] = 122183.02599420558225;
                fjkm[12] = -38765.366765003690558;
                fjkm[13] = 5352.0219072834891857;
                SetFjk16(fjk, j: 9, k: 2, un, fjkm, v);
                #endregion
                #region [10, 2]
                fjkm[0] = 0.26016595959663391113;
                fjkm[1] = -6.0597300529479980469;
                fjkm[2] = -2.5233336282830660035;
                fjkm[3] = 716.61609867398701017;
                fjkm[4] = -5739.3531518108567233;
                fjkm[5] = 18710.056678357368214;
                fjkm[6] = -15227.052778022872591;
                fjkm[7] = -90410.693429278463984;
                fjkm[8] = 374173.78606362103489;
                fjkm[9] = -726678.85908967426430;
                fjkm[10] = 867690.63613487545936;
                fjkm[11] = -669330.97296360068003;
                fjkm[12] = 326923.43164094028666;
                fjkm[13] = -92347.975136788220980;
                fjkm[14] = 11528.702830431737704;
                SetFjk16(fjk, j: 10, k: 2, un, fjkm, v);
                #endregion
                #region [11, 2]
                fjkm[0] = -0.27199168503284454346;
                fjkm[1] = 7.1495960926884537810;
                fjkm[2] = -3.3482232633271774688;
                fjkm[3] = -946.77886875050071077;
                fjkm[4] = 8937.5223755513871158;
                fjkm[5] = -35337.290730230231910;
                fjkm[6] = 49459.110954680523755;
                fjkm[7] = 130172.10642681313787;
                fjkm[8] = -799759.43622037315810;
                fjkm[9] = 1951258.3781149473349;
                fjkm[10] = -2917568.5809228727915;
                fjkm[11] = 2902364.7717490216619;
                fjkm[12] = -1939009.9106363828308;
                fjkm[13] = 839993.29007310418975;
                fjkm[14] = -213947.07574365748020;
                fjkm[15] = 24380.364047003816130;
                SetFjk16(fjk, j: 11, k: 2, un, fjkm, v);
                #endregion
                #region [12, 2]
                fjkm[0] = 0.28332467190921306610;
                fjkm[1] = -8.3174842023230218268;
                fjkm[2] = 11.564968830087189329;
                fjkm[3] = 1218.3176746274854345;
                fjkm[4] = -13385.506466376132701;
                fjkm[5] = 62540.301444639907312;
                fjkm[6] = -122382.91365675340023;
                fjkm[7] = -143089.29056273054521;
                fjkm[8] = 1554707.5273250397863;
                fjkm[9] = -4718379.1202435790866;
                fjkm[10] = 8605382.3882172381886;
                fjkm[11] = -10599895.885891960945;
                fjkm[12] = 9077838.6492033673420;
                fjkm[13] = -5358306.8036736424269;
                fjkm[14] = 2087192.8797093735160;
                fjkm[15] = -484205.51887813298356;
                fjkm[16] = 50761.444989589644273;
                SetFjk16(fjk, j: 12, k: 2, un, fjkm, v);
                #endregion
                #region [13, 2]
                fjkm[0] = -0.29422177467495203018;
                fjkm[1] = 9.5617123863640260863;
                fjkm[2] = -22.456083202924761739;
                fjkm[3] = -1532.5752010328979216;
                fjkm[4] = 19400.899387993296528;
                fjkm[5] = -105087.87893355958488;
                fjkm[6] = 262967.92353732330762;
                fjkm[7] = 61613.709412718183861;
                fjkm[8] = -2770349.5737553264845;
                fjkm[9] = 10454840.708341905254;
                fjkm[10] = -22841191.197018135498;
                fjkm[11] = 33883152.513403664925;
                fjkm[12] = -35702419.891311431439;
                fjkm[13] = 26908321.887260639130;
                fjkm[14] = -14241918.449935481857;
                fjkm[15] = 5042187.1772326832703;
                fjkm[16] = -1074259.7908211056482;
                fjkm[17] = 104287.72699173731366;
                SetFjk16(fjk, j: 13, k: 2, un, fjkm, v);
                #endregion
                #region [14, 2]
                fjkm[0] = 0.30472969519905745983;
                fjkm[1] = -10.880732823367957230;
                fjkm[2] = 36.353598566849979929;
                fjkm[3] = 1890.1183163422473995;
                fjkm[4] = -27344.503966626558254;
                fjkm[5] = 169206.00701162634518;
                fjkm[6] = -515265.57929420780237;
                fjkm[7] = 248217.18376755761876;
                fjkm[8] = 4532205.9690912962460;
                fjkm[9] = -21492951.405026820809;
                fjkm[10] = 55557247.108151935296;
                fjkm[11] = -97285892.160889723465;
                fjkm[12] = 122607925.76741209887;
                fjkm[13] = -113234217.41453912066;
                fjkm[14] = 76312885.330872101297;
                fjkm[15] = -36634431.269047918012;
                fjkm[16] = 11891540.519643040965;
                fjkm[17] = -2342835.9225964451203;
                fjkm[18] = 211794.47349942484210;
                SetFjk16(fjk, j: 14, k: 2, un, fjkm, v);
                #endregion
                #endregion k: 2
                #region k: 3
                #region [0, 3]
                fjkm[0] = 0.73242187500000000000e-1;
                fjkm[1] = 0.0;
                fjkm[2] = -0.89121093750000000000;
                fjkm[3] = 0.0;
                fjkm[4] = 1.8464626736111111111;
                fjkm[5] = 0.0;
                fjkm[6] = -1.0258125964506172840;
                SetFjk16(fjk, j: 0, k: 3, un, fjkm, v);
                #endregion
                #region [1, 3]
                fjkm[0] = -0.18310546875000000000;
                fjkm[1] = 0.23193359375000000000;
                fjkm[2] = 4.0104492187500000000;
                fjkm[3] = -4.6045898437500000000;
                fjkm[4] = -12.002007378472222222;
                fjkm[5] = 13.232982494212962963;
                fjkm[6] = 8.7194070698302469136;
                fjkm[7] = -9.4032821341306584362;
                SetFjk16(fjk, j: 1, k: 3, un, fjkm, v);
                #endregion
                #region [2, 3]
                fjkm[0] = 0.32043457031250000000;
                fjkm[1] = -0.87890625000000000000;
                fjkm[2] = -10.464160156250000000;
                fjkm[3] = 26.736328125000000000;
                fjkm[4] = 29.225667317708333333;
                fjkm[5] = -103.40190972222222222;
                fjkm[6] = 17.131070360725308642;
                fjkm[7] = 92.323133680555555556;
                fjkm[8] = -50.991434481899434156;
                SetFjk16(fjk, j: 2, k: 3, un, fjkm, v);
                #endregion
                #region [3, 3]
                fjkm[0] = -0.48065185546875000000;
                fjkm[1] = 2.1109008789062500000;
                fjkm[2] = 21.025415039062500000;
                fjkm[3] = -89.876334092881944444;
                fjkm[4] = -15.284450954861111111;
                fjkm[5] = 411.04911024305555556;
                fjkm[6] = -389.32152566792052469;
                fjkm[7] = -293.92095419801311728;
                fjkm[8] = 567.72315884813850309;
                fjkm[9] = -213.02470796338270176;
                SetFjk16(fjk, j: 3, k: 3, un, fjkm, v);
                #endregion
                #region [4, 3]
                fjkm[0] = 0.66089630126953125000;
                fjkm[1] = -4.0893554687500000000;
                fjkm[2] = -36.043276468912760417;
                fjkm[3] = 229.67792968750000000;
                fjkm[4] = -150.64704827202690972;
                fjkm[5] = -1115.0236545138888889;
                fjkm[6] = 2175.9328758333936150;
                fjkm[7] = -176.78170412165637860;
                fjkm[8] = -2817.5643744553721654;
                fjkm[9] = 2651.5545930587705761;
                fjkm[10] = -757.67687847693870206;
                SetFjk16(fjk, j: 4, k: 3, un, fjkm, v);
                #endregion
                #region [5, 3]
                fjkm[0] = -0.85916519165039062500;
                fjkm[1] = 6.9690023149762834821;
                fjkm[2] = 55.378133392333984375;
                fjkm[3] = -495.03058466109018477;
                fjkm[4] = 733.55991770426432292;
                fjkm[5] = 2262.8678469622576678;
                fjkm[6] = -7898.5588740407684703;
                fjkm[7] = 5695.1407000317985629;
                fjkm[8] = 7718.7923309734328784;
                fjkm[9] = -16089.784052072184022;
                fjkm[10] = 10424.896645958040967;
                fjkm[11] = -2413.3719004256171872;
                SetFjk16(fjk, j: 5, k: 3, un, fjkm, v);
                #endregion
                #region [6, 3]
                fjkm[0] = 1.0739564895629882812;
                fjkm[1] = -10.898971557617187500;
                fjkm[2] = -78.354169082641601562;
                fjkm[3] = 948.65802978515625000;
                fjkm[4] = -2219.4645106141832140;
                fjkm[5] = -3394.0875061035156250;
                fjkm[6] = 22215.581371235788604;
                fjkm[7] = -30531.682191548916538;
                fjkm[8] = -6945.0954169277301051;
                fjkm[9] = 63170.236743335697713;
                fjkm[10] = -72433.473359744190134;
                fjkm[11] = 36368.490166893057699;
                fjkm[12] = -7090.9841426397698721;
                SetFjk16(fjk, j: 6, k: 3, un, fjkm, v);
                #endregion
                #region [7, 3]
                fjkm[0] = -1.3040900230407714844;
                fjkm[1] = 16.023568312327067057;
                fjkm[2] = 103.72332413083031064;
                fjkm[3] = -1667.3156506674630301;
                fjkm[4] = 5406.6561977448034539;
                fjkm[5] = 2872.9832533515445770;
                fjkm[6] = -52104.157882492630570;
                fjkm[7] = 110439.44251210509668;
                fjkm[8] = -46659.137282813036883;
                fjkm[9] = -173333.53977304078369;
                fjkm[10] = 339551.86235951279889;
                fjkm[11] = -281181.85781749484440;
                fjkm[12] = 116143.52270798282713;
                fjkm[13] = -19586.901426503340492;
                SetFjk16(fjk, j: 7, k: 3, un, fjkm, v);
                #endregion
                #region [8, 3]
                fjkm[0] = 1.5486069023609161377;
                fjkm[1] = -22.482862472534179688;
                fjkm[2] = -129.63729095714432853;
                fjkm[3] = 2741.6385669817243304;
                fjkm[4] = -11510.430102675971531;
                fjkm[5] = 3157.6331450774177672;
                fjkm[6] = 105738.58606273177860;
                fjkm[7] = -320687.78222286271460;
                fjkm[8] = 312110.04133755429291;
                fjkm[9] = 306706.99777237116391;
                fjkm[10] = -1199602.2626751819183;
                fjkm[11] = 1489876.7581807900958;
                fjkm[12] = -983301.03812460934208;
                fjkm[13] = 346445.22525468589947;
                fjkm[14] = -51524.795648340968513;
                SetFjk16(fjk, j: 8, k: 3, un, fjkm, v);
                #endregion
                #region [9, 3]
                fjkm[0] = -1.8067080527544021606;
                fjkm[1] = 30.413155585075869705;
                fjkm[2] = 153.62532423010894230;
                fjkm[3] = -4275.6818557748761872;
                fjkm[4] = 22280.234407631843178;
                fjkm[5] = -22294.360392132099974;
                fjkm[6] = -188892.07658652729984;
                fjkm[7] = 800265.57729686724177;
                fjkm[8] = -1211192.8548070343556;
                fjkm[9] = -77428.408184713489979;
                fjkm[10] = 3343683.8379703140094;
                fjkm[11] = -6075462.1554119391419;
                fjkm[12] = 5742939.8025630234344;
                fjkm[13] = -3178262.5289756645302;
                fjkm[14] = 978732.98065558879521;
                fjkm[15] = -130276.59845140693203;
                SetFjk16(fjk, j: 9, k: 3, un, fjkm, v);
                #endregion
                #region [10, 3]
                fjkm[0] = 2.0777142606675624847;
                fjkm[1] = -39.947360754013061523;
                fjkm[2] = -172.57638879468381540;
                fjkm[3] = 6386.1869555628867376;
                fjkm[4] = -40128.233950133856041;
                fjkm[5] = 67957.947814703914434;
                fjkm[6] = 297268.90885718166849;
                fjkm[7] = -1779345.5277624794845;
                fjkm[8] = 3703482.9515239098067;
                fjkm[9] = -1986101.2546910898185;
                fjkm[10] = -7335848.9571808003709;
                fjkm[11] = 20236466.148311260729;
                fjkm[12] = -26009069.048248407006;
                fjkm[13] = 20168378.697199375155;
                fjkm[14] = -9655403.7938215681211;
                fjkm[15] = 2644939.5099481697170;
                fjkm[16] = -318773.08892039496616;
                SetFjk16(fjk, j: 10, k: 3, un, fjkm, v);
                #endregion
                #region [11, 3]
                fjkm[0] = -2.3610389325767755508;
                fjkm[1] = 51.215318048867833364;
                fjkm[2] = 182.72462206625770697;
                fjkm[3] = -9201.6026100350086393;
                fjkm[4] = 68268.256270370096570;
                fjkm[5] = -162141.74207057405274;
                fjkm[6] = -402709.29424480088095;
                fjkm[7] = 3603004.6646962551842;
                fjkm[8] = -9740822.7436915944519;
                fjkm[9] = 10107345.074822039354;
                fjkm[10] = 11498843.003383757375;
                fjkm[11] = -56841651.345428293012;
                fjkm[12] = 97219891.251414595951;
                fjkm[13] = -99519441.572391483730;
                fjkm[14] = 65942266.170395132237;
                fjkm[15] = -27893470.527717198286;
                fjkm[16] = 6888375.1431181415309;
                fjkm[17] = -758786.31484749532876;
                SetFjk16(fjk, j: 11, k: 3, un, fjkm, v);
                #endregion
                #region [12, 3]
                fjkm[0] = 2.6561687991488724947;
                fjkm[1] = -64.344060508074698510;
                fjkm[2] = -179.63738093291836674;
                fjkm[3] = 12860.884276726402663;
                fjkm[4] = -110864.10416322604021;
                fjkm[5] = 338921.66960863282529;
                fjkm[6] = 430704.46372470858983;
                fjkm[7] = -6738355.6173354573243;
                fjkm[8] = 22959038.837731227338;
                fjkm[9] = -34599901.818598601926;
                fjkm[10] = -5491093.1482636375735;
                fjkm[11] = 136348100.13563343194;
                fjkm[12] = -311391327.48073317359;
                fjkm[13] = 406747852.87490380879;
                fjkm[14] = -350465400.65634558429;
                fjkm[15] = 203621539.64411279745;
                fjkm[16] = -77285292.825523293342;
                fjkm[17] = 17387623.032021543609;
                fjkm[18] = -1764164.5657772609975;
                SetFjk16(fjk, j: 12, k: 3, un, fjkm, v);
                #endregion
                #region [13, 3]
                fjkm[0] = -2.9626498144352808595;
                fjkm[1] = 79.458041370067243966;
                fjkm[2] = 158.20533868869907341;
                fjkm[3] = -17512.092404496780068;
                fjkm[4] = 173186.23779115767550;
                fjkm[5] = -648825.90238130558651;
                fjkm[6] = -226118.57798798103100;
                fjkm[7] = 11744385.639221317992;
                fjkm[8] = -49655262.440949257658;
                fjkm[9] = 97992370.806143206674;
                fjkm[10] = -45563229.252833811893;
                fjkm[11] = -276725901.55879139753;
                fjkm[12] = 874903955.44068001049;
                fjkm[13] = -1431639430.0141678479;
                fjkm[14] = 1544324559.7308983592;
                fjkm[15] = -1156507831.0309397511;
                fjkm[16] = 599846625.33396072076;
                fjkm[17] = -206711172.70469868149;
                fjkm[18] = 42729154.354849580701;
                fjkm[19] = -4019188.6691200667599;
                SetFjk16(fjk, j: 13, k: 3, un, fjkm, v);
                #endregion
                #endregion k: 3
                #region k: 4
                #region [0, 4]
                fjkm[0] = 0.11215209960937500000;
                fjkm[1] = 0.0;
                fjkm[2] = -2.3640869140625000000;
                fjkm[3] = 0.0;
                fjkm[4] = 8.7891235351562500000;
                fjkm[5] = 0.0;
                fjkm[6] = -11.207002616222993827;
                fjkm[7] = 0.0;
                fjkm[8] = 4.6695844234262474280;
                SetFjk16(fjk, j: 0, k: 4, un, fjkm, v);
                #endregion
                #region [1, 4]
                fjkm[0] = -0.39253234863281250000;
                fjkm[1] = 0.46730041503906250000;
                fjkm[2] = 13.002478027343750000;
                fjkm[3] = -14.578535970052083333;
                fjkm[4] = -65.918426513671875000;
                fjkm[5] = 71.777842203776041667;
                fjkm[6] = 106.46652485411844136;
                fjkm[7] = -113.93785993160043724;
                fjkm[8] = -53.700220869401845422;
                fjkm[9] = 56.813277151686010374;
                SetFjk16(fjk, j: 1, k: 4, un, fjkm, v);
                #endregion
                #region [2, 4]
                fjkm[0] = 0.88319778442382812500;
                fjkm[1] = -2.2430419921875000000;
                fjkm[2] = -40.888863372802734375;
                fjkm[3] = 99.291650390625000000;
                fjkm[4] = 222.92270863850911458;
                fjkm[5] = -632.81689453125000000;
                fjkm[6] = -205.55324667471426505;
                fjkm[7] = 1232.7702877845293210;
                fjkm[8] = -339.12856875133121946;
                fjkm[9] = -728.45517005449459877;
                fjkm[10] = 393.21792165601858550;
                SetFjk16(fjk, j: 2, k: 4, un, fjkm, v);
                #endregion
                #region [3, 4]
                fjkm[0] = -1.6191959381103515625;
                fjkm[1] = 6.5174388885498046875;
                fjkm[2] = 97.292139053344726562;
                fjkm[3] = -384.72013047112358941;
                fjkm[4] = -422.46132278442382812;
                fjkm[5] = 2925.8162224946198640;
                fjkm[6] = -1437.2810672241964458;
                fjkm[7] = -5929.6163575631600839;
                fjkm[8] = 6678.9649706318545243;
                fjkm[9] = 1992.7516382310725152;
                fjkm[10] = -5560.5995012212682653;
                fjkm[11] = 2034.9551693024277015;
                SetFjk16(fjk, j: 3, k: 4, un, fjkm, v);
                #endregion
                #region [4, 4]
                fjkm[0] = 2.6311933994293212891;
                fjkm[1] = -14.813423156738281250;
                fjkm[2] = -194.76567316055297852;
                fjkm[3] = 1116.2957621256510417;
                fjkm[4] = 214.74175742997063531;
                fjkm[5] = -9500.2007904052734375;
                fjkm[6] = 12733.852428636433166;
                fjkm[7] = 15619.117721871584041;
                fjkm[8] = -43856.442195416477973;
                fjkm[9] = 16041.189890575016477;
                fjkm[10] = 30538.376827393703173;
                fjkm[11] = -31457.433732481486840;
                fjkm[12] = 8757.4502329231489542;
                SetFjk16(fjk, j: 4, k: 4, un, fjkm, v);
                #endregion
                #region [5, 4]
                fjkm[0] = -3.9467900991439819336;
                fjkm[1] = 28.973631262779235840;
                fjkm[2] = 345.96240515708923340;
                fjkm[3] = -2698.2640026051657540;
                fjkm[4] = 1749.1663194396760729;
                fjkm[5] = 24230.291604531833104;
                fjkm[6] = -57186.682706525590685;
                fjkm[7] = -12268.269917924904529;
                fjkm[8] = 179642.68522044022878;
                fjkm[9] = -184075.59647969633791;
                fjkm[10] = -55836.464134952713487;
                fjkm[11] = 219854.46366368396092;
                fjkm[12] = -146898.32628401899970;
                fjkm[13] = 33116.007471226346158;
                SetFjk16(fjk, j: 5, k: 4, un, fjkm, v);
                #endregion
                #region [6, 4]
                fjkm[0] = 5.5912859737873077393;
                fjkm[1] = -51.149418354034423828;
                fjkm[2] = -562.32073248028755188;
                fjkm[3] = 5740.3790382385253906;
                fjkm[4] = -8505.8885195685227712;
                fjkm[5] = -51098.161945523156060;
                fjkm[6] = 189688.56368664133696;
                fjkm[7] = -98986.676113505422333;
                fjkm[8] = -524313.33320720157996;
                fjkm[9] = 1006412.2572891519230;
                fjkm[10] = -338656.53584266578056;
                fjkm[11] = -879242.30314162838225;
                fjkm[12] = 1184856.1356540792633;
                fjkm[13] = -599009.59593194338847;
                fjkm[14] = 113723.03789882673771;
                SetFjk16(fjk, j: 6, k: 4, un, fjkm, v);
                #endregion
                #region [7, 4]
                fjkm[0] = -7.5881738215684890747;
                fjkm[1] = 83.791322236259778341;
                fjkm[2] = 852.65149199664592743;
                fjkm[3] = -11106.114063047180100;
                fjkm[4] = 25906.550896742895797;
                fjkm[5] = 90628.038560826472504;
                fjkm[6] = -518627.00526554010007;
                fjkm[7] = 625235.13813439073187;
                fjkm[8] = 1093177.6471805288836;
                fjkm[9] = -3890056.2699064931220;
                fjkm[10] = 3443257.5304835279133;
                fjkm[11] = 1688397.8063561002636;
                fjkm[12] = -6072278.7538110165925;
                fjkm[13] = 5368522.3053911687123;
                fjkm[14] = -2206353.9977704553128;
                fjkm[15] = 362368.26917284610367;
                SetFjk16(fjk, j: 7, k: 4, un, fjkm, v);
                #endregion
                #region [8, 4]
                fjkm[0] = 9.9594781408086419106;
                fjkm[1] = -129.64064240455627441;
                fjkm[2] = -1221.6473410353064537;
                fjkm[3] = 19961.318612462793078;
                fjkm[4] = -64135.377206358956439;
                fjkm[5] = -132491.34865988838862;
                fjkm[6] = 1231383.3446542421759;
                fjkm[7] = -2354589.9435372119185;
                fjkm[8] = -1264272.3547066582332;
                fjkm[9] = 11829877.236705665039;
                fjkm[10] = -17640228.849921599961;
                fjkm[11] = 3679036.5985685346058;
                fjkm[12] = 21328290.463957701877;
                fjkm[13] = -31765842.068457922539;
                fjkm[14] = 21552604.862053478433;
                fjkm[15] = -7505720.5013225646891;
                fjkm[16] = 1087467.9477654193166;
                SetFjk16(fjk, j: 8, k: 4, un, fjkm, v);
                #endregion
                #region [9, 4]
                fjkm[0] = -12.725999846588820219;
                fjkm[1] = 191.72191139924424616;
                fjkm[2] = 1668.3300609576205413;
                fjkm[3] = -33822.376037367149478;
                fjkm[4] = 139670.53397437636223;
                fjkm[5] = 142413.44221576977052;
                fjkm[6] = -2615894.9488370680107;
                fjkm[7] = 7028008.7411020993735;
                fjkm[8] = -1692308.6869349767646;
                fjkm[9] = -29470781.812749969812;
                fjkm[10] = 66963160.307047821471;
                fjkm[11] = -48089009.180540108686;
                fjkm[12] = -47220345.652008289171;
                fjkm[13] = 138176622.72569342840;
                fjkm[14] = -141477318.49446414033;
                fjkm[15] = 78991076.066288083382;
                fjkm[16] = -23950277.790642797164;
                fjkm[17] = 3106959.7999206284827;
                SetFjk16(fjk, j: 9, k: 4, un, fjkm, v);
                #endregion
                #region [10, 4]
                fjkm[0] = 15.907499808236025274;
                fjkm[1] = -273.33609867841005325;
                fjkm[2] = -2184.4474298407246048;
                fjkm[3] = 54603.014533953543693;
                fjkm[4] = -277734.45331034886641;
                fjkm[5] = -41109.662796960089573;
                fjkm[6] = 5064705.8054347585059;
                fjkm[7] = -18090940.629099192262;
                fjkm[8] = 15917528.891696545807;
                fjkm[9] = 60220437.637860917599;
                fjkm[10] = -208561974.26419501420;
                fjkm[11] = 250941409.40257928779;
                fjkm[12] = 12084648.536915454989;
                fjkm[13] = -458976381.31741616556;
                fjkm[14] = 699975914.26074700776;
                fjkm[15] = -563757375.34166870146;
                fjkm[16] = 269426949.85344351478;
                fjkm[17] = -72497863.184361287773;
                fjkm[18] = 8519623.3256649401434;
                SetFjk16(fjk, j: 10, k: 4, un, fjkm, v);
                #endregion
                #region [11, 4]
                fjkm[0] = -19.522840673744212836;
                fjkm[1] = 378.05442703043402114;
                fjkm[2] = 2752.8286152184838570;
                fjkm[3] = -84659.009316768248795;
                fjkm[4] = 515277.57789757005885;
                fjkm[5] = -327436.12858763123432;
                fjkm[6] = -9039645.4021864355266;
                fjkm[7] = 41795541.145581633576;
                fjkm[8] = -61523469.344794095368;
                fjkm[9] = -95072522.336003533273;
                fjkm[10] = 557312492.76991978150;
                fjkm[11] = -963898631.17215744429;
                fjkm[12] = 480963077.05278739701;
                fjkm[13] = 1131925258.2353560419;
                fjkm[14] = -2762861092.1224474487;
                fjkm[15] = 3066633312.6192085228;
                fjkm[16] = -2065683582.7903266052;
                fjkm[17] = 866733914.71761334168;
                fjkm[18] = -209952808.47963646972;
                fjkm[19] = 22561861.306890567863;
                SetFjk16(fjk, j: 11, k: 4, un, fjkm, v);
                #endregion
                #region [12, 4]
                fjkm[0] = 23.590099147440923844;
                fjkm[1] = -509.71270865877158940;
                fjkm[2] = -3345.7051051560481552;
                fjkm[3] = 126830.08496875773140;
                fjkm[4] = -904536.17796320887184;
                fjkm[5] = 1241459.9239568200231;
                fjkm[6] = 14964746.535519588726;
                fjkm[7] = -88697323.877097900818;
                fjkm[8] = 182496348.04247934870;
                fjkm[9] = 82548357.373675412652;
                fjkm[10] = -1305701152.6740455738;
                fjkm[11] = 3075905875.9322221769;
                fjkm[12] = -2915784132.1314453282;
                fjkm[13] = -1631935529.3260648957;
                fjkm[14] = 8923557290.4467172403;
                fjkm[15] = -13522339111.332256776;
                fjkm[16] = 12138202400.912393639;
                fjkm[17] = -7081644626.2422883477;
                fjkm[18] = 2655510812.9195669082;
                fjkm[19] = -585530475.83660861349;
                fjkm[20] = 57986597.253985419492;
                SetFjk16(fjk, j: 12, k: 4, un, fjkm, v);
                #endregion
                #endregion k: 4
                #region k: 5
                #region [0, 5]
                fjkm[0] = 0.22710800170898437500;
                fjkm[1] = 0.0;
                fjkm[2] = -7.3687943594796316964;
                fjkm[3] = 0.0;
                fjkm[4] = 42.534998745388454861;
                fjkm[5] = 0.0;
                fjkm[6] = -91.818241543240017361;
                fjkm[7] = 0.0;
                fjkm[8] = 84.636217674600734632;
                fjkm[9] = 0.0;
                fjkm[10] = -28.212072558200244877;
                SetFjk16(fjk, j: 0, k: 5, un, fjkm, v);
                #endregion
                #region [1, 5]
                fjkm[0] = -1.0219860076904296875;
                fjkm[1] = 1.1733913421630859375;
                fjkm[2] = 47.897163336617606027;
                fjkm[3] = -52.809692909604027158;
                fjkm[4] = -361.54748933580186632;
                fjkm[5] = 389.90415516606083623;
                fjkm[6] = 964.09153620402018229;
                fjkm[7] = -1025.3036972328468605;
                fjkm[8] = -1057.9527209325091829;
                fjkm[9] = 1114.3768660489096727;
                fjkm[10] = 409.07505209390355072;
                fjkm[11] = -427.88310046603704731;
                SetFjk16(fjk, j: 1, k: 5, un, fjkm, v);
                #endregion
                #region [2, 5]
                fjkm[0] = 2.8104615211486816406;
                fjkm[1] = -6.8132400512695312500;
                fjkm[2] = -175.59265831538609096;
                fjkm[3] = 412.65248413085937500;
                fjkm[4] = 1483.6983865298922100;
                fjkm[5] = -3828.1498870849609375;
                fjkm[6] = -3429.1824372044316045;
                fjkm[7] = 12120.007883707682292;
                fjkm[8] = 557.04779563126740632;
                fjkm[9] = -15403.791616777333703;
                fjkm[10] = 5099.3321148946942616;
                fjkm[11] = 6770.8974139680587706;
                fjkm[12] = -3602.9167662868229396;
                SetFjk16(fjk, j: 2, k: 5, un, fjkm, v);
                #endregion
                #region [3, 5]
                fjkm[0] = -6.0893332958221435547;
                fjkm[1] = 23.218954324722290039;
                fjkm[2] = 480.30594648633684431;
                fjkm[3] = -1808.5316684886387416;
                fjkm[4] = -3878.5356589824434311;
                fjkm[5] = 19896.567837257637549;
                fjkm[6] = -442.43697992960611979;
                fjkm[7] = -68889.990792852959025;
                fjkm[8] = 51994.291933598341765;
                fjkm[9] = 82310.686911069807202;
                fjkm[10] = -107791.27622674358562;
                fjkm[11] = -11421.030640241631355;
                fjkm[12] = 61775.622629784098705;
                fjkm[13] = -22242.802900370862046;
                SetFjk16(fjk, j: 3, k: 5, un, fjkm, v);
                #endregion
                #region [4, 5]
                fjkm[0] = 11.417499929666519165;
                fjkm[1] = -60.543208122253417969;
                fjkm[2] = -1092.4312783437115805;
                fjkm[3] = 5864.8337360927036830;
                fjkm[4] = 6502.6598836863797808;
                fjkm[5] = -73117.673620733634505;
                fjkm[6] = 62464.986490075687042;
                fjkm[7] = 248344.19895160816334;
                fjkm[8] = -446788.55343178424816;
                fjkm[9] = -141685.28980603760980;
                fjkm[10] = 805685.00855625677338;
                fjkm[11] = -411181.55351158250234;
                fjkm[12] = -353321.84981767407842;
                fjkm[13] = 410732.51135669781511;
                fjkm[14] = -112357.72180097660343;
                SetFjk16(fjk, j: 4, k: 5, un, fjkm, v);
                #endregion
                #region [5, 5]
                fjkm[0] = -19.409749880433082581;
                fjkm[1] = 133.60695303976535797;
                fjkm[2] = 2184.1347855359315872;
                fjkm[3] = -15685.927751365060709;
                fjkm[4] = -3330.0494749048683378;
                fjkm[5] = 213065.39140775687165;
                fjkm[6] = -371035.73548135295431;
                fjkm[7] = -595658.10351999312306;
                fjkm[8] = 2217706.3121620208025;
                fjkm[9] = -928359.76150830112939;
                fjkm[10] = -3462387.4565158783367;
                fjkm[11] = 4492508.5831562094441;
                fjkm[12] = -105953.60990151918538;
                fjkm[13] = -3174045.4228780972346;
                fjkm[14] = 2222890.1148558130258;
                fjkm[15] = -492012.66653936007240;
                SetFjk16(fjk, j: 5, k: 5, un, fjkm, v);
                #endregion
                #region [6, 5]
                fjkm[0] = 30.732103977352380753;
                fjkm[1] = -262.68730902671813965;
                fjkm[2] = -3966.7030987024307251;
                fjkm[3] = 36616.605837684018271;
                fjkm[4] = -23288.020921949948583;
                fjkm[5] = -522073.19210540329968;
                fjkm[6] = 1445873.6105443313563;
                fjkm[7] = 729826.91993359621660;
                fjkm[8] = -8027322.7404775209228;
                fjkm[9] = 9022069.9722413070898;
                fjkm[10] = 8528377.7669713558429;
                fjkm[11] = -26111911.326974580072;
                fjkm[12] = 15072848.600502055062;
                fjkm[13] = 11547035.062352154444;
                fjkm[14] = -20141460.694713124158;
                fjkm[15] = 10381853.494410238157;
                fjkm[16] = -1934247.3992962518385;
                SetFjk16(fjk, j: 6, k: 5, un, fjkm, v);
                #endregion
                #region [7, 5]
                fjkm[0] = -46.098155966028571129;
                fjkm[1] = 474.28179555572569370;
                fjkm[2] = 6683.6986173737261977;
                fjkm[3] = -77185.928108340199369;
                fjkm[4] = 113831.12007369411134;
                fjkm[5] = 1111273.3131467255535;
                fjkm[6] = -4487654.8822124313622;
                fjkm[7] = 1363280.0193113290821;
                fjkm[8] = 22934079.022569534587;
                fjkm[9] = -45086888.891430235790;
                fjkm[10] = -1310272.8912292084566;
                fjkm[11] = 103829405.25391139096;
                fjkm[12] = -124354846.65650933807;
                fjkm[13] = 7129538.3762123397968;
                fjkm[14] = 107358912.41929520843;
                fjkm[15] = -104902766.60439072992;
                fjkm[16] = 43358616.238781106380;
                fjkm[17] = -6986431.7916780392488;
                SetFjk16(fjk, j: 7, k: 5, un, fjkm, v);
                #endregion
                #region [8, 5]
                fjkm[0] = 66.266099201166070998;
                fjkm[1] = -801.85159036517143250;
                fjkm[2] = -10599.673972529735017;
                fjkm[3] = 150238.26124378282197;
                fjkm[4] = -349014.85897753891605;
                fjkm[5] = -2089251.7495501184712;
                fjkm[6] = 11932847.810754978267;
                fjkm[7] = -12233248.989355019522;
                fjkm[8] = -52996346.810335384350;
                fjkm[9] = 167552806.49381000405;
                fjkm[10] = -104151238.67453537869;
                fjkm[11] = -295472139.38679802840;
                fjkm[12] = 638921130.05027917750;
                fjkm[13] = -364575119.55069248400;
                fjkm[14] = -321938848.50186568760;
                fjkm[15] = 670099675.87405621186;
                fjkm[16] = -477068925.07477830810;
                fjkm[17] = 165792634.22539301473;
                fjkm[18] = -23563863.859185525714;
                SetFjk16(fjk, j: 8, k: 5, un, fjkm, v);
                #endregion
                #region [9, 5]
                fjkm[0] = -92.036248890508431941;
                fjkm[1] = 1286.5459820115674202;
                fjkm[2] = 15984.246642014751810;
                fjkm[3] = -274251.94134559012498;
                fjkm[4] = 875242.48789234256927;
                fjkm[5] = 3479369.5294348938478;
                fjkm[6] = -28243123.382389417092;
                fjkm[7] = 48978753.833933594038;
                fjkm[8] = 96403146.255130900766;
                fjkm[9] = -511553591.82361285211;
                fjkm[10] = 629980523.20384634815;
                fjkm[11] = 530948403.41019250637;
                fjkm[12] = -2455930387.0192782105;
                fjkm[13] = 2650615136.5125114389;
                fjkm[14] = -13787083.107153494438;
                fjkm[15] = -2934135354.5042859293;
                fjkm[16] = 3427343175.1586684272;
                fjkm[17] = -1959752975.9949664819;
                fjkm[18] = 590135160.40330437780;
                fjkm[19] = -75099321.778257988527;
                SetFjk16(fjk, j: 9, k: 5, un, fjkm, v);
                #endregion
                #region [10, 5]
                fjkm[0] = 124.24893600218638312;
                fjkm[1] = -1977.9098049255553633;
                fjkm[2] = -23091.371137548782696;
                fjkm[3] = 474842.97883978903678;
                fjkm[4] = -1940160.8026042800714;
                fjkm[5] = -5051698.1764753913085;
                fjkm[6] = 60910728.961121054906;
                fjkm[7] = -150451158.23316204445;
                fjkm[8] = -115862597.99920025974;
                fjkm[9] = 1340568362.9608932867;
                fjkm[10] = -2534626023.8559564861;
                fjkm[11] = 57223508.996001180928;
                fjkm[12] = 7462542511.1368897210;
                fjkm[13] = -12803010436.906857334;
                fjkm[14] = 6529418551.9917203148;
                fjkm[15] = 8333347633.6309463361;
                fjkm[16] = -17879701023.370781023;
                fjkm[17] = 15384544080.383156452;
                fjkm[18] = -7429525037.6309918827;
                fjkm[19] = 1979364564.1715841600;
                fjkm[20] = -228201703.20311712289;
                SetFjk16(fjk, j: 10, k: 5, un, fjkm, v);
                #endregion
                #region [11, 5]
                fjkm[0] = -163.78268836651841411;
                fjkm[1] = 2934.5753597465244149;
                fjkm[2] = 32133.674298098814287;
                fjkm[3] = -786448.50173515116761;
                fjkm[4] = 3940574.6676366506081;
                fjkm[5] = 6023482.0215492578926;
                fjkm[6] = -121576300.80985078356;
                fjkm[7] = 396478314.88388992406;
                fjkm[8] = -28867608.104982563453;
                fjkm[9] = -3079357130.2226958330;
                fjkm[10] = 8249384124.8426910359;
                fjkm[11] = -5275292901.0490022350;
                fjkm[12] = -17864885603.458945318;
                fjkm[13] = 48480263067.875486448;
                fjkm[14] = -46292296797.993831733;
                fjkm[15] = -6808810264.3074322272;
                fjkm[16] = 70205428541.430035549;
                fjkm[17] = -89866265315.798467190;
                fjkm[18] = 62728346454.981427111;
                fjkm[19] = -26379975109.497266807;
                fjkm[20] = 6313975478.5070403854;
                fjkm[21] = -665761463.93251599995;
                SetFjk16(fjk, j: 11, k: 5, un, fjkm, v);
                #endregion
                #endregion k: 5
                #region k: 6
                #region [0, 6]
                fjkm[0] = 0.57250142097473144531;
                fjkm[1] = 0.0;
                fjkm[2] = -26.491430486951555525;
                fjkm[3] = 0.0;
                fjkm[4] = 218.19051174421159048;
                fjkm[5] = 0.0;
                fjkm[6] = -699.57962737613254123;
                fjkm[7] = 0.0;
                fjkm[8] = 1059.9904525279998779;
                fjkm[9] = 0.0;
                fjkm[10] = -765.25246814118164230;
                fjkm[11] = 0.0;
                fjkm[12] = 212.57013003921712286;
                SetFjk16(fjk, j: 0, k: 6, un, fjkm, v);
                #endregion
                #region [1, 6]
                fjkm[0] = -3.1487578153610229492;
                fjkm[1] = 3.5304254293441772461;
                fjkm[2] = 198.68572865213666643;
                fjkm[3] = -216.34668231010437012;
                fjkm[4] = -2072.8098615700101096;
                fjkm[5] = 2218.2702027328178365;
                fjkm[6] = 8045.1657148255242242;
                fjkm[7] = -8511.5521330762792517;
                fjkm[8] = -14309.871109127998352;
                fjkm[9] = 15016.531410813331604;
                fjkm[10] = 11861.413256188315456;
                fjkm[11] = -12371.581568282436550;
                fjkm[12] = -3719.9772756862996501;
                fjkm[13] = 3861.6906957124443986;
                SetFjk16(fjk, j: 1, k: 6, un, fjkm, v);
                #endregion
                #region [2, 6]
                fjkm[0] = 10.233462899923324585;
                fjkm[1] = -24.045059680938720703;
                fjkm[2] = -830.55504153881754194;
                fjkm[3] = 1907.3829950605119978;
                fjkm[4] = 9817.0755057463759468;
                fjkm[5] = -24000.956291863274953;
                fjkm[6] = -37145.398656393453558;
                fjkm[7] = 109134.42187067667643;
                fjkm[8] = 44836.131085879493643;
                fjkm[9] = -222597.99503087997437;
                fjkm[10] = 21083.102663859050460;
                fjkm[11] = 208148.67133440140671;
                fjkm[12] = -75945.993209761297570;
                fjkm[13] = -72698.984473412256018;
                fjkm[14] = 38306.908850817252349;
                SetFjk16(fjk, j: 2, k: 6, un, fjkm, v);
                #endregion
                #region [3, 6]
                fjkm[0] = -25.583657249808311462;
                fjkm[1] = 94.002347901463508606;
                fjkm[2] = 2561.4464542163269860;
                fjkm[3] = -9323.5958246609994343;
                fjkm[4] = -30925.007683879997995;
                fjkm[5] = 137806.75057795568307;
                fjkm[6] = 66832.114908046586804;
                fjkm[7] = -695211.42408942898710;
                fjkm[8] = 297044.24306208789349;
                fjkm[9] = 1456689.0310313083630;
                fjkm[10] = -1349408.4412036920976;
                fjkm[11] = -1160751.9107670259288;
                fjkm[12] = 1778986.1326650806502;
                fjkm[13] = 2732.4118798791034334;
                fjkm[14] = -771855.42780552482418;
                fjkm[15] = 274755.25810395356345;
                SetFjk16(fjk, j: 3, k: 6, un, fjkm, v);
                #endregion
                #region [4, 6]
                fjkm[0] = 54.365271655842661858;
                fjkm[1] = -276.51818633079528809;
                fjkm[2] = -6505.3076391667127609;
                fjkm[3] = 33393.909001989024026;
                fjkm[4] = 70377.367684318314469;
                fjkm[5] = -559956.42247611134141;
                fjkm[6] = 214189.08434628603635;
                fjkm[7] = 2932546.1434609688359;
                fjkm[8] = -3873550.9334950489425;
                fjkm[9] = -5169809.5626455059758;
                fjkm[10] = 12515387.720161636405;
                fjkm[11] = -485287.10103841189331;
                fjkm[12] = -14696506.049911874334;
                fjkm[13] = 8973612.6112505443054;
                fjkm[14] = 4358025.7113579717181;
                fjkm[15] = -5899263.9630258568617;
                fjkm[16] = 1593568.9458830170786;
                SetFjk16(fjk, j: 4, k: 6, un, fjkm, v);
                #endregion
                #region [5, 6]
                fjkm[0] = -103.29401614610105753;
                fjkm[1] = 679.49573871586471796;
                fjkm[2] = 14406.592158034443855;
                fjkm[3] = -97833.485413427407644;
                fjkm[4] = -109874.73447139263153;
                fjkm[5] = 1806898.4781330669971;
                fjkm[6] = -2228617.5688649368428;
                fjkm[7] = -9039218.0698707036945;
                fjkm[8] = 22913970.357558080752;
                fjkm[9] = 6014580.7747564135778;
                fjkm[10] = -67365551.082731008652;
                fjkm[11] = 49347945.008100392566;
                fjkm[12] = 61291772.218955972273;
                fjkm[13] = -101851641.61990357673;
                fjkm[14] = 18812228.438435360796;
                fjkm[15] = 48878028.306514326695;
                fjkm[16] = -36319210.680616361669;
                fjkm[17] = 7931540.8655372143613;
                SetFjk16(fjk, j: 5, k: 6, un, fjkm, v);
                #endregion
                #region [6, 6]
                fjkm[0] = 180.76452825567685068;
                fjkm[1] = -1472.1516226977109909;
                fjkm[2] = -28789.438034501904622;
                fjkm[3] = 248412.49281514968191;
                fjkm[4] = 57720.374439080618322;
                fjkm[5] = -4919773.7285477433167;
                fjkm[6] = 10556839.574755387407;
                fjkm[7] = 20546468.524262875642;
                fjkm[8] = -95782021.413901062575;
                fjkm[9] = 47859753.794019524423;
                fjkm[10] = 248947938.76422519634;
                fjkm[11] = -397302112.77505450021;
                fjkm[12] = -60373613.593196943601;
                fjkm[13] = 619895830.67946774788;
                fjkm[14] = -460542977.57691540068;
                fjkm[15] = -133881283.62288371220;
                fjkm[16] = 360816116.57296145253;
                fjkm[17] = -191228273.50596204944;
                fjkm[18] = 35131056.264643928958;
                SetFjk16(fjk, j: 6, k: 6, un, fjkm, v);
                #endregion
                #region [7, 6]
                fjkm[0] = -296.97029642004054040;
                fjkm[1] = 2903.8678610353963450;
                fjkm[2] = 53085.574017544759304;
                fjkm[3] = -566162.21564224131681;
                fjkm[4] = 351303.05079310148650;
                fjkm[5] = 11717363.296337798053;
                fjkm[6] = -37248885.401731669600;
                fjkm[7] = -29556393.543845627395;
                fjkm[8] = 317963019.15514055453;
                fjkm[9] = -391675101.18705789558;
                fjkm[10] = -632571201.89067213266;
                fjkm[11] = 2001265322.6458411313;
                fjkm[12] = -1008064787.2696644192;
                fjkm[13] = -2363398838.2753953344;
                fjkm[14] = 3743079200.5912006268;
                fjkm[15] = -1091651847.4629542050;
                fjkm[16] = -1902208028.0358040356;
                fjkm[17] = 2133928476.4409109596;
                fjkm[18] = -893289789.98112876745;
                fjkm[19] = 141870657.61208999896;
                SetFjk16(fjk, j: 7, k: 6, un, fjkm, v);
                #endregion
                #region [8, 6]
                fjkm[0] = 464.01608815631334437;
                fjkm[1] = -5325.3068480961956084;
                fjkm[2] = -91725.505981153156193;
                fjkm[3] = 1185316.6466349283157;
                fjkm[4] = -1734253.4162956622921;
                fjkm[5] = -24965643.657687719930;
                fjkm[6] = 109865301.06964371257;
                fjkm[7] = -7842536.5990090553082;
                fjkm[8] = -882004248.16533042144;
                fjkm[9] = 1812569161.1229405097;
                fjkm[10] = 796190115.17482420785;
                fjkm[11] = -7547640676.2891254543;
                fjkm[12] = 8606208381.3162846761;
                fjkm[13] = 4634326771.3840251843;
                fjkm[14] = -19504767652.151161478;
                fjkm[15] = 15458811432.485826518;
                fjkm[16] = 3233232293.8508888375;
                fjkm[17] = -14292761227.388542723;
                fjkm[18] = 10872211035.524945516;
                fjkm[19] = -3794154076.5815443275;
                fjkm[20] = 531367092.46942384383;
                SetFjk16(fjk, j: 8, k: 6, un, fjkm, v);
                #endregion
                #region [9, 6]
                fjkm[0] = -696.02413223447001656;
                fjkm[1] = 9211.7329484450783639;
                fjkm[2] = 150176.80785295595602;
                fjkm[3] = -2316795.9016969799608;
                fjkm[4] = 5353301.7267099139240;
                fjkm[5] = 48240128.320645392607;
                fjkm[6] = -285088568.66606943443;
                fjkm[7] = 236539658.57255855849;
                fjkm[8] = 2091063546.0424140229;
                fjkm[9] = -6478408107.9479218188;
                fjkm[10] = 2114947617.4171696310;
                fjkm[11] = 22484222108.370436724;
                fjkm[12] = -43504182331.312579142;
                fjkm[13] = 9073306866.7430378915;
                fjkm[14] = 72378013713.663446159;
                fjkm[15] = -104755002543.75143509;
                fjkm[16] = 35288894490.708636111;
                fjkm[17] = 58426452630.062379587;
                fjkm[18] = -83650899080.625595930;
                fjkm[19] = 49569134901.994243944;
                fjkm[20] = -14909719423.420583328;
                fjkm[21] = 1869289195.4875346262;
                SetFjk16(fjk, j: 9, k: 6, un, fjkm, v);
                #endregion
                #region [10, 6]
                fjkm[0] = 1009.2349917399815240;
                fjkm[1] = -15188.489623096204014;
                fjkm[2] = -234912.74468029359442;
                fjkm[3] = 4278033.8338821994157;
                fjkm[4] = -13570280.995019535225;
                fjkm[5] = -85095459.392767211454;
                fjkm[6] = 669951675.09023006543;
                fjkm[7] = -1041016026.5703218480;
                fjkm[8] = -4241350570.5261899700;
                fjkm[9] = 19536358548.670235798;
                fjkm[10] = -19158789931.456781880;
                fjkm[11] = -52588961930.566950783;
                fjkm[12] = 168223251386.27505861;
                fjkm[13] = -131543443714.17931735;
                fjkm[14] = -183278984759.32788630;
                fjkm[15] = 500731039137.68584765;
                fjkm[16] = -395175402811.80868032;
                fjkm[17] = -83866621459.668331313;
                fjkm[18] = 443918936157.05017610;
                fjkm[19] = -420471377306.84165961;
                fjkm[20] = 207052924562.97132855;
                fjkm[21] = -54907932888.507130529;
                fjkm[22] = 6236056730.2635893277;
                SetFjk16(fjk, j: 10, k: 6, un, fjkm, v);
                #endregion
                #endregion k: 6
                #region k: 7
                #region [0, 7]
                fjkm[0] = 1.7277275025844573975;
                fjkm[1] = 0.0;
                fjkm[2] = -108.09091978839465550;
                fjkm[3] = 0.0;
                fjkm[4] = 1200.9029132163524628;
                fjkm[5] = 0.0;
                fjkm[6] = -5305.6469786134031084;
                fjkm[7] = 0.0;
                fjkm[8] = 11655.393336864533248;
                fjkm[9] = 0.0;
                fjkm[10] = -13586.550006434137439;
                fjkm[11] = 0.0;
                fjkm[12] = 8061.7221817373093845;
                fjkm[13] = 0.0;
                fjkm[14] = -1919.4576623184069963;
                SetFjk16(fjk, j: 0, k: 7, un, fjkm, v);
                #endregion
                #region [1, 7]
                fjkm[0] = -11.230228766798973083;
                fjkm[1] = 12.382047101855278015;
                fjkm[2] = 918.77281820135457175;
                fjkm[3] = -990.83343139361767542;
                fjkm[4] = -12609.480588771700859;
                fjkm[5] = 13410.082530915935834;
                fjkm[6] = 66320.587232667538855;
                fjkm[7] = -69857.685218409807594;
                fjkm[8] = -169003.20338453573209;
                fjkm[9] = 176773.46560911208759;
                fjkm[10] = 224178.07510616326774;
                fjkm[11] = -233235.77511045269270;
                fjkm[12] = -149141.86036214022361;
                fjkm[13] = 154516.34181663176320;
                fjkm[14] = 39348.882077527343424;
                fjkm[15] = -40628.520519072948089;
                SetFjk16(fjk, j: 1, k: 7, un, fjkm, v);
                #endregion
                #region [2, 7]
                fjkm[0] = 42.113357875496149063;
                fjkm[1] = -96.752740144729614258;
                fjkm[2] = -4309.3875268953187125;
                fjkm[3] = 9728.1827809555189950;
                fjkm[4] = 67131.493914289162272;
                fjkm[5] = -158519.18454455852509;
                fjkm[6] = -361549.21741861661275;
                fjkm[7] = 965627.75010763936573;
                fjkm[8] = 791368.90269480066167;
                fjkm[9] = -2797294.4008474879795;
                fjkm[10] = -473067.29978352049251;
                fjkm[11] = 4157484.3019688460562;
                fjkm[12] = -742925.21875958646140;
                fjkm[13] = -3063454.4290601775661;
                fjkm[14] = 1186992.6183777028865;
                fjkm[15] = 886789.43999110403230;
                fjkm[16] = -463948.91246287829107;
                SetFjk16(fjk, j: 2, k: 7, un, fjkm, v);
                #endregion
                #region [3, 7]
                fjkm[0] = -119.32118064723908901;
                fjkm[1] = 426.72709654457867146;
                fjkm[2] = 14774.672950860112906;
                fjkm[3] = -52455.440737222809167;
                fjkm[4] = -242280.57068559899926;
                fjkm[5] = 994102.67395229967167;
                fjkm[6] = 1032233.4449197463691;
                fjkm[7] = -6741567.1085603777512;
                fjkm[8] = 646495.83215296654790;
                fjkm[9] = 20680668.518424851831;
                fjkm[10] = -13425393.794712164658;
                fjkm[11] = -29950004.715897617246;
                fjkm[12] = 32133326.488920312047;
                fjkm[13] = 16877471.905929211282;
                fjkm[14] = -30879035.210339582752;
                fjkm[15] = 1961435.1350279426026;
                fjkm[16] = 10741165.112229910651;
                fjkm[17] = -3791244.3495002070744;
                SetFjk16(fjk, j: 3, k: 7, un, fjkm, v);
                #endregion
                #region [4, 7]
                fjkm[0] = 283.38780403719283640;
                fjkm[1] = -1397.7315495908260345;
                fjkm[2] = -41380.460209263255820;
                fjkm[3] = 205570.88069305533455;
                fjkm[4] = 656958.69278146053058;
                fjkm[5] = -4406726.3053560412498;
                fjkm[6] = -460386.15389300137896;
                fjkm[7] = 31745963.553354091997;
                fjkm[8] = -30185144.899501059008;
                fjkm[9] = -92885893.761842946947;
                fjkm[10] = 159671874.67639934532;
                fjkm[11] = 89037317.391947147287;
                fjkm[12] = -324112057.62064508289;
                fjkm[13] = 75417289.288447113978;
                fjkm[14] = 275169534.60077554540;
                fjkm[15] = -191141129.57979682342;
                fjkm[16] = -56632059.761422146328;
                fjkm[17] = 92789782.492575658213;
                fjkm[18] = -24828398.690560814574;
                SetFjk16(fjk, j: 4, k: 7, un, fjkm, v);
                #endregion
                #region [5, 7]
                fjkm[0] = -595.11438847810495645;
                fjkm[1] = 3784.5764889410929754;
                fjkm[2] = 100370.50080364884343;
                fjkm[3] = -654419.09384311857985;
                fjkm[4] = -1371381.6192330607878;
                fjkm[5] = 15498343.812751787009;
                fjkm[6] = -11665144.592299357907;
                fjkm[7] = -112601561.49426607138;
                fjkm[8] = 219262318.25667364074;
                fjkm[9] = 254104121.37964987144;
                fjkm[10] = -1006396481.7103226659;
                fjkm[11] = 253036623.98729691889;
                fjkm[12] = 1831393810.4978639927;
                fjkm[13] = -1774529065.4266491466;
                fjkm[14] = -1003472637.9292914864;
                fjkm[15] = 2290280875.9786741918;
                fjkm[16] = -651246794.10887221841;
                fjkm[17] = -803784902.98109705890;
                fjkm[18] = 640483342.29369123263;
                fjkm[19] = -138440607.21363135318;
                SetFjk16(fjk, j: 5, k: 7, un, fjkm, v);
                #endregion
                #region [6, 7]
                fjkm[0] = 1140.6359112497011665;
                fjkm[1] = -8957.2682584379799664;
                fjkm[2] = -218407.33629125532461;
                fjkm[3] = 1794837.6930232931017;
                fjkm[4] = 2046161.0563529025012;
                fjkm[5] = -45986041.828079905965;
                fjkm[6] = 74544641.814482732603;
                fjkm[7] = 314850611.45725349592;
                fjkm[8] = -1044136171.4521382041;
                fjkm[9] = -187106978.33355739111;
                fjkm[10] = 4438953626.0956817754;
                fjkm[11] = -4408582954.0483059788;
                fjkm[12] = -6264272163.4397192983;
                fjkm[13] = 14149702487.577580987;
                fjkm[14] = -2713149740.6519136094;
                fjkm[15] = -14233558941.890554329;
                fjkm[16] = 12753481214.719287040;
                fjkm[17] = 934842459.40433410209;
                fjkm[18] = -6845048171.9341116529;
                fjkm[19] = 3754053882.0127951637;
                fjkm[20] = -682202534.28377278514;
                SetFjk16(fjk, j: 6, k: 7, un, fjkm, v);
                #endregion
                #region [7, 7]
                fjkm[0] = -2036.8498415173235117;
                fjkm[1] = 19163.284363303755526;
                fjkm[2] = 436370.36938456366105;
                fjkm[3] = -4395730.5804739226086;
                fjkm[4] = -1112960.9840974107984;
                fjkm[5] = 119491789.40923461317;
                fjkm[6] = -304912988.84344882540;
                fjkm[7] = -691963837.01336538937;
                fjkm[8] = 3885386169.7360802683;
                fjkm[9] = -2313847981.3260245039;
                fjkm[10] = -14816925759.772210986;
                fjkm[11] = 28337891715.905708458;
                fjkm[12] = 7872387353.1924133326;
                fjkm[13] = -72435569735.091307437;
                fjkm[14] = 59410896148.189366465;
                fjkm[15] = 46141874867.831978016;
                fjkm[16] = -105411711029.14681739;
                fjkm[17] = 45764643115.283153298;
                fjkm[18] = 33619493138.706044340;
                fjkm[19] = -45524891856.627263052;
                fjkm[20] = 19398990085.810093364;
                fjkm[21] = -3046176001.4829695650;
                SetFjk16(fjk, j: 7, k: 7, un, fjkm, v);
                #endregion
                #region [8, 7]
                fjkm[0] = 3437.1841075604834259;
                fjkm[1] = -37884.316448339552153;
                fjkm[2] = -813572.82790964112831;
                fjkm[3] = 9844700.1196742646463;
                fjkm[4] = -5932665.5737596173789;
                fjkm[5] = -278602941.80981757775;
                fjkm[6] = 999702331.75599910068;
                fjkm[7] = 1082145758.5676864833;
                fjkm[8] = -12097872233.934345656;
                fjkm[9] = 16269568192.896168430;
                fjkm[10] = 37442906272.629884505;
                fjkm[11] = -127678482632.39934914;
                fjkm[12] = 56533169829.379619075;
                fjkm[13] = 263705962704.22363017;
                fjkm[14] = -430880513772.40015440;
                fjkm[15] = 28685028305.645455607;
                fjkm[16] = 544657720548.16430202;
                fjkm[17] = -543385080747.85240194;
                fjkm[18] = 33319140131.863860742;
                fjkm[19] = 311025095335.01861076;
                fjkm[20] = -257492440682.78870598;
                fjkm[21] = 90635479554.844098597;
                fjkm[22] = -12545989968.390205031;
                SetFjk16(fjk, j: 8, k: 7, un, fjkm, v);
                #endregion
                #region [9, 7]
                fjkm[0] = -5537.6855066252232973;
                fjkm[1] = 70275.288364441469184;
                fjkm[2] = 1432194.8403133915934;
                fjkm[3] = -20502591.084346145880;
                fjkm[4] = 29691158.739889488095;
                fjkm[5] = 592508323.24904278254;
                fjkm[6] = -2831998971.7799303680;
                fjkm[7] = -487772628.07968000257;
                fjkm[8] = 32638786024.059947790;
                fjkm[9] = -70496897875.469508762;
                fjkm[10] = -62531875035.153643030;
                fjkm[11] = 456706039700.77564846;
                fjkm[12] = -503779673552.18388727;
                fjkm[13] = -650638245764.84731771;
                fjkm[14] = 2119375307389.9958522;
                fjkm[15] = -1373635599107.5234068;
                fjkm[16] = -1758554601545.7817139;
                fjkm[17] = 3640007756944.3988399;
                fjkm[18] = -1912987782878.0613666;
                fjkm[19] = -1044942776057.9478291;
                fjkm[20] = 2082243082925.6114167;
                fjkm[21] = -1292209352199.4454475;
                fjkm[22] = 389815335189.77376153;
                fjkm[23] = -48292926381.689492854;
                SetFjk16(fjk, j: 9, k: 7, un, fjkm, v);
                #endregion
                #endregion k: 7
                #region k: 8
                #region [0, 8]
                fjkm[0] = 6.0740420012734830379;
                fjkm[1] = 0.0;
                fjkm[2] = -493.91530477308801242;
                fjkm[3] = 0.0;
                fjkm[4] = 7109.5143024893637214;
                fjkm[5] = 0.0;
                fjkm[6] = -41192.654968897551298;
                fjkm[7] = 0.0;
                fjkm[8] = 122200.46498301745979;
                fjkm[9] = 0.0;
                fjkm[10] = -203400.17728041553428;
                fjkm[11] = 0.0;
                fjkm[12] = 192547.00123253153236;
                fjkm[13] = 0.0;
                fjkm[14] = -96980.598388637513489;
                fjkm[15] = 0.0;
                fjkm[16] = 20204.291330966148643;
                SetFjk16(fjk, j: 0, k: 8, un, fjkm, v);
                #endregion
                #region [1, 8]
                fjkm[0] = -45.555315009551122785;
                fjkm[1] = 49.604676343733444810;
                fjkm[2] = 4692.1953953443361180;
                fjkm[3] = -5021.4722651930614596;
                fjkm[4] = -81759.414478627682797;
                fjkm[5] = 86499.090680287258611;
                fjkm[6] = 556100.84208011694252;
                fjkm[7] = -583562.61205938197672;
                fjkm[8] = -1894107.2072367706267;
                fjkm[9] = 1975574.1838921155999;
                fjkm[10] = 3559503.1024072718499;
                fjkm[11] = -3695103.2205942155394;
                fjkm[12] = -3754666.5240343648810;
                fjkm[13] = 3883031.1915227192359;
                fjkm[14] = 2085082.8653557065400;
                fjkm[15] = -2149736.5976147982157;
                fjkm[16] = -474800.84627770449312;
                fjkm[17] = 488270.37383168192555;
                SetFjk16(fjk, j: 1, k: 8, un, fjkm, v);
                #endregion
                #region [2, 8]
                fjkm[0] = 193.61008879059227183;
                fjkm[1] = -437.33102409169077873;
                fjkm[2] = -24389.798720089893322;
                fjkm[3] = 54330.683525039681367;
                fjkm[4] = 481258.52318321001006;
                fjkm[5] = -1109084.2311883407405;
                fjkm[6] = -3433050.7548587226633;
                fjkm[7] = 8650457.5434684857726;
                fjkm[8] = 11004225.300068311602;
                fjkm[9] = -33238526.475380749062;
                fjkm[10] = -15303078.309507955098;
                fjkm[11] = 69562860.629902112723;
                fjkm[12] = 1830924.9239440239572;
                fjkm[13] = -80869740.517663243591;
                fjkm[14] = 18943271.994495349280;
                fjkm[15] = 49072182.784650581825;
                fjkm[16] = -19806771.899029389669;
                fjkm[17] = -12122574.798579689186;
                fjkm[18] = 6307948.1226220563244;
                SetFjk16(fjk, j: 2, k: 8, un, fjkm, v);
                #endregion
                #region [3, 8]
                fjkm[0] = -613.09861450354219414;
                fjkm[1] = 2147.8571771753195208;
                fjkm[2] = 91956.399098661058815;
                fjkm[3] = -320284.80771632295052;
                fjkm[4] = -1938565.2131506522862;
                fjkm[5] = 7533108.6348155997448;
                fjkm[6] = 12364512.209251265036;
                fjkm[7] = -65358277.938386196419;
                fjkm[8] = -16530840.331176116137;
                fjkm[9] = 269292234.00676317160;
                fjkm[10] = -105780946.98529899276;
                fjkm[11] = -575008738.51905591292;
                fjkm[12] = 462747211.13824444405;
                fjkm[13] = 619754381.87898314676;
                fjkm[14] = -755460241.94008607635;
                fjkm[15] = -252322946.06096464110;
                fjkm[16] = 569416784.91969405599;
                fjkm[17] = -61357261.820865861243;
                fjkm[18] = -164974352.55837953060;
                fjkm[19] = 57850732.229668052938;
                SetFjk16(fjk, j: 3, k: 8, un, fjkm, v);
                #endregion
                #region [4, 8]
                fjkm[0] = 1609.3838630717982596;
                fjkm[1] = -7751.9961041252827272;
                fjkm[2] = -281313.57426896920515;
                fjkm[3] = 1362914.7891508336068;
                fjkm[4] = 5966457.1081110733990;
                fjkm[5] = -36097318.956064416329;
                fjkm[6] = -22309783.293551422257;
                fjkm[7] = 336158425.14225148967;
                fjkm[8] = -205028522.03506721003;
                fjkm[9] = -1388339192.4100137759;
                fjkm[10] = 1836112835.6997822732;
                fjkm[11] = 2556726458.1368042032;
                fjkm[12] = -5652778880.4580993178;
                fjkm[13] = -1072688790.0156425156;
                fjkm[14] = 8223828086.6764744334;
                fjkm[15] = -2962125430.6175380614;
                fjkm[16] = -5363095697.4586512056;
                fjkm[17] = 4151384158.2283357977;
                fjkm[18] = 758617226.29066830002;
                fjkm[19] = -1589602926.9007581937;
                fjkm[20] = 422197436.26031767718;
                SetFjk16(fjk, j: 4, k: 8, un, fjkm, v);
                #endregion
                #region [5, 8]
                fjkm[0] = -3701.5828850651359971;
                fjkm[1] = 22929.423816498228916;
                fjkm[2] = 740942.45462626213339;
                fjkm[3] = -4683368.4794967535629;
                fjkm[4] = -14688082.295896812171;
                fjkm[5] = 136985890.98054216279;
                fjkm[6] = -37063877.251336542111;
                fjkm[7] = -1319571104.7402945920;
                fjkm[8] = 2005453574.1427636671;
                fjkm[9] = 4917316117.1324589056;
                fjkm[10] = -13428166320.891447862;
                fjkm[11] = -3828355991.3598264145;
                fjkm[12] = 37778220592.815991623;
                fjkm[13] = -20635506272.066951931;
                fjkm[14] = -47082248514.737817855;
                fjkm[15] = 56649997707.122923550;
                fjkm[16] = 14169411429.388481941;
                fjkm[17] = -52510965464.207838905;
                fjkm[18] = 18673363121.781645026;
                fjkm[19] = 14082226269.939926879;
                fjkm[20] = -12159500780.523353877;
                fjkm[21] = 2607014902.9539700770;
                SetFjk16(fjk, j: 5, k: 8, un, fjkm, v);
                #endregion
                #region [6, 8]
                fjkm[0] = 7711.6310105523666607;
                fjkm[1] = -58858.321154496479721;
                fjkm[2] = -1741907.2159207465870;
                fjkm[3] = 13794087.929804007718;
                fjkm[4] = 28916480.065464689455;
                fjkm[5] = -437983696.24595980730;
                fjkm[6] = 494945307.55363422412;
                fjkm[7] = 4180226129.2961517207;
                fjkm[8] = -10954467146.581302332;
                fjkm[9] = -11060935369.012122216;
                fjkm[10] = 67274495072.654389414;
                fjkm[11] = -33385597946.547532147;
                fjkm[12] = -168738543918.69430001;
                fjkm[13] = 236353638119.12099768;
                fjkm[14] = 123426482948.35286349;
                fjkm[15] = -460569493399.15116382;
                fjkm[16] = 183685386812.60549147;
                fjkm[17] = 323426896220.20397235;
                fjkm[18] = -345059927788.03296106;
                fjkm[19] = 18066712637.598243570;
                fjkm[20] = 137638821647.93765226;
                fjkm[21] = -78528723144.419087956;
                fjkm[22] = 14147149999.271829167;
                SetFjk16(fjk, j: 6, k: 8, un, fjkm, v);
                #endregion
                #region [7, 8]
                fjkm[0] = -14872.431234636707131;
                fjkm[1] = 135739.23591067179473;
                fjkm[2] = 3743563.4066693378186;
                fjkm[3] = -36117123.586437547311;
                fjkm[4] = -41857416.064054280688;
                fjkm[5] = 1225475415.8400151136;
                fjkm[6] = -2467842162.9074118554;
                fjkm[7] = -10944143126.183310329;
                fjkm[8] = 45219896350.687109257;
                fjkm[9] = 3706147531.3941538260;
                fjkm[10] = -259638349679.49799671;
                fjkm[11] = 331511238218.36097150;
                fjkm[12] = 503674442974.85219625;
                fjkm[13] = -1481164218255.8010841;
                fjkm[14] = 369277357070.34485189;
                fjkm[15] = 2339646237464.7011180;
                fjkm[16] = -2569885298111.3265694;
                fjkm[17] = -667794049596.58740652;
                fjkm[18] = 2906153064933.9250617;
                fjkm[19] = -1575977334606.5289263;
                fjkm[20] = -578377418663.53101806;
                fjkm[21] = 1021516684285.5146634;
                fjkm[22] = -444821917816.52114438;
                fjkm[23] = 69214137882.703873029;
                SetFjk16(fjk, j: 7, k: 8, un, fjkm, v);
                #endregion
                #region [8, 8]
                fjkm[0] = 26956.281612779031676;
                fjkm[1] = -287752.74431752909550;
                fjkm[2] = -7479028.3293311244821;
                fjkm[3] = 86134990.009811768863;
                fjkm[4] = 23679004.644932133834;
                fjkm[5] = -3077161905.3885988089;
                fjkm[6] = 9103660424.3905427045;
                fjkm[7] = 23518972784.280263949;
                fjkm[8] = -154703115855.86457830;
                fjkm[9] = 108277078595.12757644;
                fjkm[10] = 805587125662.51652400;
                fjkm[11] = -1805165807562.2091925;
                fjkm[12] = -672890155245.49490410;
                fjkm[13] = 6669143665397.1378050;
                fjkm[14] = -6132703262892.6427237;
                fjkm[15] = -7384886877294.0818151;
                fjkm[16] = 17989877009001.983669;
                fjkm[17] = -6680625953666.2171081;
                fjkm[18] = -14315112877022.322823;
                fjkm[19] = 17853687377733.207721;
                fjkm[20] = -3800827233430.3173356;
                fjkm[21] = -6918410846679.1440993;
                fjkm[22] = 6365772360949.3812603;
                fjkm[23] = -2267586042740.4274751;
                fjkm[24] = 310920009576.22258316;
                SetFjk16(fjk, j: 8, k: 8, un, fjkm, v);
                #endregion
                #endregion k: 8
                #region k: 9
                #region [0, 9]
                fjkm[0] = 24.380529699556063861;
                fjkm[1] = 0.0;
                fjkm[2] = -2499.8304818112096241;
                fjkm[3] = 0.0;
                fjkm[4] = 45218.768981362726273;
                fjkm[5] = 0.0;
                fjkm[6] = -331645.17248456357783;
                fjkm[7] = 0.0;
                fjkm[8] = 1268365.2733216247816;
                fjkm[9] = 0.0;
                fjkm[10] = -2813563.2265865341107;
                fjkm[11] = 0.0;
                fjkm[12] = 3763271.2976564039964;
                fjkm[13] = 0.0;
                fjkm[14] = -2998015.9185381067501;
                fjkm[15] = 0.0;
                fjkm[16] = 1311763.6146629772007;
                fjkm[17] = 0.0;
                fjkm[18] = -242919.18790055133346;
                SetFjk16(fjk, j: 0, k: 9, un, fjkm, v);
                #endregion
                #region [1, 9]
                fjkm[0] = -207.23450244622654282;
                fjkm[1] = 223.48818891259725206;
                fjkm[2] = 26248.220059017701053;
                fjkm[3] = -27914.773713558507469;
                fjkm[4] = -565234.61226703407842;
                fjkm[5] = 595380.45825460922926;
                fjkm[6] = 4808855.0010261718786;
                fjkm[7] = -5029951.7826825475971;
                fjkm[8] = -20928027.009806808897;
                fjkm[9] = 21773603.858687892085;
                fjkm[10] = 52050919.691850881048;
                fjkm[11] = -53926628.509575237122;
                fjkm[12] = -77147061.601956281926;
                fjkm[13] = 79655909.133727217924;
                fjkm[14] = 67455358.167107401877;
                fjkm[15] = -69454035.446132806377;
                fjkm[16] = -32138208.559242941417;
                fjkm[17] = 33012717.635684926217;
                fjkm[18] = 6437358.4793646103367;
                fjkm[19] = -6599304.6046316445590;
                SetFjk16(fjk, j: 1, k: 9, un, fjkm, v);
                #endregion
                #region [2, 9]
                fjkm[0] = 984.36388661957607837;
                fjkm[1] = -2194.2476729600457475;
                fjkm[2] = -149715.34984220301505;
                fjkm[3] = 329977.62359907967038;
                fjkm[4] = 3636074.9553359345392;
                fjkm[5] = -8229815.9546080161817;
                fjkm[6] = -32850375.705398849013;
                fjkm[7] = 79594841.396295258680;
                fjkm[8] = 140766384.09976010426;
                fjkm[9] = -388119773.63641718318;
                fjkm[10] = -302391232.58882834949;
                fjkm[11] = 1069154026.1028829621;
                fjkm[12] = 267438889.51147761435;
                fjkm[13] = -1738631339.5172586463;
                fjkm[14] = 117013574.77418801057;
                fjkm[15] = 1654904787.0330349261;
                fjkm[16] = -452792004.09905362650;
                fjkm[17] = -852646349.53093518044;
                fjkm[18] = 354479824.94387953335;
                fjkm[19] = 183646906.05281680809;
                fjkm[20] = -95153470.227211795243;
                SetFjk16(fjk, j: 2, k: 9, un, fjkm, v);
                #endregion
                #region [3, 9]
                fjkm[0] = -3445.2736031685162743;
                fjkm[1] = 11875.044917870854988;
                fjkm[2] = 615370.50617503436198;
                fjkm[3] = -2111012.2880743031723;
                fjkm[4] = -16085470.193130487741;
                fjkm[5] = 60142798.465327082670;
                fjkm[6] = 138071565.42237886418;
                fjkm[7] = -645362876.57211229968;
                fjkm[8] = -403042168.77936625406;
                fjkm[9] = 3392353592.8461412643;
                fjkm[10] = -459521271.54126358493;
                fjkm[11] = -9731832243.4128846845;
                fjkm[12] = 5664098916.1399176504;
                fjkm[13] = 15628587453.068137370;
                fjkm[14] = -14586127233.110452133;
                fjkm[15] = -13112286301.064475188;
                fjkm[16] = 18076138583.500999115;
                fjkm[17] = 3815036344.9012045022;
                fjkm[18] = -11188129037.135692765;
                fjkm[19] = 1563031125.3210441483;
                fjkm[20] = 2774182673.1720275815;
                fjkm[21] = -967769239.01720317824;
                SetFjk16(fjk, j: 3, k: 9, un, fjkm, v);
                #endregion
                #region [4, 9]
                fjkm[0] = 9905.1616091094842886;
                fjkm[1] = -46820.775577189124306;
                fjkm[2] = -2040487.1579820312439;
                fjkm[3] = 9691735.5725892393225;
                fjkm[4] = 54815060.692073363805;
                fjkm[5] = -309390686.57090219229;
                fjkm[6] = -360017948.08027628199;
                fjkm[7] = 3579817310.0615042649;
                fjkm[8] = -975311608.49901937973;
                fjkm[9] = -19324823653.635730322;
                fjkm[10] = 19615966368.548239543;
                fjkm[11] = 52313543472.729543241;
                fjkm[12] = -87319509031.594965641;
                fjkm[13] = -62880701946.001201560;
                fjkm[14] = 187577712375.82047424;
                fjkm[15] = -5014883987.8038807144;
                fjkm[16] = -210187382319.75343210;
                fjkm[17] = 95721572764.807326435;
                fjkm[18] = 109424352395.85297165;
                fjkm[19] = -93578868051.240421500;
                fjkm[20] = -10054064393.166929203;
                fjkm[21] = 29497575770.435656525;
                fjkm[22] = -7788016225.4016704591;
                SetFjk16(fjk, j: 4, k: 9, un, fjkm, v);
                #endregion
                #region [5, 9]
                fjkm[0] = -24762.904022773710722;
                fjkm[1] = 150202.12250011534820;
                fjkm[2] = 5795836.3854749992775;
                fjkm[3] = -35748503.915727151813;
                fjkm[4] = -151894819.53780369009;
                fjkm[5] = 1257834576.5897312294;
                fjkm[6] = 283767191.88351472079;
                fjkm[7] = -15254386392.322461649;
                fjkm[8] = 17551211231.567450237;
                fjkm[9] = 79224659176.160664514;
                fjkm[10] = -169127446206.19948897;
                fjkm[11] = -159965663833.05011647;
                fjkm[12] = 667618088176.46828695;
                fjkm[13] = -96638419442.062469711;
                fjkm[14] = -1307530021252.0141810;
                fjkm[15] = 998989571129.40670193;
                fjkm[16] = 1171444346499.5230642;
                fjkm[17] = -1737201461305.1621935;
                fjkm[18] = -114746520425.41058268;
                fjkm[19] = 1243665816084.6793073;
                fjkm[20] = -512525557301.93515073;
                fjkm[21] = -261796114655.33666678;
                fjkm[22] = 247688439610.96543843;
                fjkm[23] = -52756420815.901269809;
                SetFjk16(fjk, j: 5, k: 9, un, fjkm, v);
                #endregion
                #region [6, 9]
                fjkm[0] = 55716.534051240849124;
                fjkm[1] = -415613.72155461745024;
                fjkm[2] = -14629090.289521179515;
                fjkm[3] = 112517365.15692088569;
                fjkm[4] = 349569807.61470724572;
                fjkm[5] = -4300749690.4008170962;
                fjkm[6] = 2780201489.2284702260;
                fjkm[7] = 53008111096.391616511;
                fjkm[8] = -112833892526.88952297;
                fjkm[9] = -236800084652.33408957;
                fjkm[10] = 948058960807.18952080;
                fjkm[11] = 15893204800.580178374;
                fjkm[12] = -3467524908336.7843972;
                fjkm[13] = 3164309467171.3659456;
                fjkm[14] = 5582435675279.1817146;
                fjkm[15] = -10541249137249.730392;
                fjkm[16] = -1168774181536.2029753;
                fjkm[17] = 14413943369875.125772;
                fjkm[18] = -7960463497916.1060492;
                fjkm[19] = -7323822211977.2888688;
                fjkm[20] = 9405979314966.6882022;
                fjkm[21] = -1275663773372.9196006;
                fjkm[22] = -2930439830152.3544240;
                fjkm[23] = 1747630840980.1348510;
                fjkm[24] = -312613977240.16973767;
                SetFjk16(fjk, j: 6, k: 9, un, fjkm, v);
                #endregion
                #region [7, 9]
                fjkm[0] = -115412.82053471318747;
                fjkm[1] = 1027788.8260207103833;
                fjkm[2] = 33623754.973679064933;
                fjkm[3] = -313584768.22511895708;
                fjkm[4] = -659891574.79280520771;
                fjkm[5] = 12850764070.127691360;
                fjkm[6] = -19440345035.583477731;
                fjkm[7] = -155138555406.99154257;
                fjkm[8] = 516911193990.92260102;
                fjkm[9] = 451041007491.51124256;
                fjkm[10] = -4074544650661.9635765;
                fjkm[11] = 3101195098601.7687528;
                fjkm[12] = 13186035652463.635190;
                fjkm[13] = -24971415775329.260965;
                fjkm[14] = -10847664957177.134063;
                fjkm[15] = 66205224018265.472705;
                fjkm[16] = -37500185472771.584184;
                fjkm[17] = -69777795804669.385671;
                fjkm[18] = 99311193873506.569397;
                fjkm[19] = -492168373279.20159968;
                fjkm[20] = -80134019127845.984412;
                fjkm[21] = 51160427044311.739411;
                fjkm[22] = 9045641917569.2467301;
                fjkm[23] = -24123027505367.165512;
                fjkm[24] = 10768904399045.846700;
                fjkm[25] = -1663085461560.5466581;
                SetFjk16(fjk, j: 7, k: 9, un, fjkm, v);
                #endregion
                #endregion k: 9
                #region k: 10
                #region [0, 10]
                fjkm[0] = 110.01714026924673817;
                fjkm[1] = 0.0;
                fjkm[2] = -13886.089753717040532;
                fjkm[3] = 0.0;
                fjkm[4] = 308186.40461266239848;
                fjkm[5] = 0.0;
                fjkm[6] = -2785618.1280864546890;
                fjkm[7] = 0.0;
                fjkm[8] = 13288767.166421818329;
                fjkm[9] = 0.0;
                fjkm[10] = -37567176.660763351308;
                fjkm[11] = 0.0;
                fjkm[12] = 66344512.274729026665;
                fjkm[13] = 0.0;
                fjkm[14] = -74105148.211532657748;
                fjkm[15] = 0.0;
                fjkm[16] = 50952602.492664642206;
                fjkm[17] = 0.0;
                fjkm[18] = -19706819.118432226927;
                fjkm[19] = 0.0;
                fjkm[20] = 3284469.8530720378211;
                SetFjk16(fjk, j: 0, k: 10, un, fjkm, v);
                #endregion
                #region [1, 10]
                fjkm[0] = -1045.1628325578440126;
                fjkm[1] = 1118.5075927373418381;
                fjkm[2] = 159690.03216774596612;
                fjkm[3] = -168947.42533689065981;
                fjkm[4] = -4160516.4622709423795;
                fjkm[5] = 4365974.0653460506451;
                fjkm[6] = 43177080.985340047679;
                fjkm[7] = -45034159.737397684138;
                fjkm[8] = -232553425.41238182077;
                fjkm[9] = 241412603.52332969965;
                fjkm[10] = 732559944.88488535051;
                fjkm[11] = -757604729.32539425138;
                fjkm[12] = -1426407013.9066740733;
                fjkm[13] = 1470636688.7564934244;
                fjkm[14] = 1741470982.9710174571;
                fjkm[15] = -1790874415.1120392289;
                fjkm[16] = -1299291363.5629483763;
                fjkm[17] = 1333259765.2247248044;
                fjkm[18] = 541937525.75688624049;
                fjkm[19] = -555075405.16917439177;
                fjkm[20] = -96891860.665625115724;
                fjkm[21] = 99081507.234339807604;
                SetFjk16(fjk, j: 1, k: 10, un, fjkm, v);
                #endregion
                #region [2, 10]
                fjkm[0] = 5487.1048709286810663;
                fjkm[1] = -12101.885429617141199;
                fjkm[2] = -991438.75239470139087;
                fjkm[3] = 2166230.0015798583230;
                fjkm[4] = 28994419.876786743130;
                fjkm[5] = -64719144.968659103681;
                fjkm[6] = -321629835.31147623339;
                fjkm[7] = 757688130.83951567540;
                fjkm[8] = 1749409837.5100643555;
                fjkm[9] = -4544758370.9162618687;
                fjkm[10] = -5113992851.9544763313;
                fjkm[11] = 15778214197.520607549;
                fjkm[12] = 7774473545.9444870052;
                fjkm[13] = -33570323211.012887492;
                fjkm[14] = -3804246527.4759322626;
                fjkm[15] = 44463088926.919594649;
                fjkm[16] = -5920634247.3331925357;
                fjkm[17] = -35768726949.850578829;
                fjkm[18] = 10834752690.813605970;
                fjkm[19] = 16001937124.166968265;
                fjkm[20] = -6803368741.9070923418;
                fjkm[21] = -3054556963.3569951737;
                fjkm[22] = 1577229794.0273014954;
                SetFjk16(fjk, j: 2, k: 10, un, fjkm, v);
                #endregion
                #region [3, 10]
                fjkm[0] = -21033.902005226610754;
                fjkm[1] = 71551.022388357981754;
                fjkm[2] = 4410889.4214898588524;
                fjkm[3] = -14945521.653227529678;
                fjkm[4] = -139310283.22771754330;
                fjkm[5] = 506112018.78208167499;
                fjkm[6] = 1519598088.8133635683;
                fjkm[7] = -6552067582.0564506220;
                fjkm[8] = -6692370095.4282068536;
                fjkm[9] = 42444451633.414257582;
                fjkm[10] = 5560288488.7766793217;
                fjkm[11] = -155035909620.11651130;
                fjkm[12] = 57543525805.054081844;
                fjkm[13] = 335136386281.40047184;
                fjkm[14] = -242028870673.91835183;
                fjkm[15] = -425158049601.57076545;
                fjkm[16] = 447281929473.23048294;
                fjkm[17] = 285539009675.26705395;
                fjkm[18] = -446532174700.75534828;
                fjkm[19] = -56114815650.416797381;
                fjkm[20] = 234199738711.39910784;
                fjkm[21] = -38367666879.807869654;
                fjkm[22] = -50717346515.577689017;
                fjkm[23] = 17618025541.849480837;
                SetFjk16(fjk, j: 3, k: 10, un, fjkm, v);
                #endregion
                #region [4, 10]
                fjkm[0] = 65730.943766333158607;
                fjkm[1] = -305976.00327882005331;
                fjkm[2] = -15752484.913133094466;
                fjkm[3] = 73626048.766219891723;
                fjkm[4] = 517727813.38636845015;
                fjkm[5] = -2779778621.0311819654;
                fjkm[6] = -4848484539.3006943808;
                fjkm[7] = 38867748248.486701209;
                fjkm[8] = 2816113040.3833130041;
                fjkm[9] = -261989764937.19714608;
                fjkm[10] = 193832179511.35043238;
                fjkm[11] = 941830771354.52418439;
                fjkm[12] = -1252062600825.4805385;
                fjkm[13] = -1788586580037.9230755;
                fjkm[14] = 3715406759129.6400037;
                fjkm[15] = 1338049108553.0784173;
                fjkm[16] = -6078517409833.2670243;
                fjkm[17] = 1046877894952.0462047;
                fjkm[18] = 5489626110236.1661278;
                fjkm[19] = -2928719118680.3861544;
                fjkm[20] = -2340856193318.6421567;
                fjkm[21] = 2206204676067.3123808;
                fjkm[22] = 119173943641.45927347;
                fjkm[23] = -589883942966.21075926;
                fjkm[24] = 154983207892.81174977;
                SetFjk16(fjk, j: 4, k: 10, un, fjkm, v);
                #endregion
                #region [5, 10]
                fjkm[0] = -177473.54816909952824;
                fjkm[1] = 1058104.4704696212045;
                fjkm[2] = 47977824.897952560186;
                fjkm[3] = -290115715.66147843083;
                fjkm[4] = -1577194622.2444813090;
                fjkm[5] = 12040320836.774941282;
                fjkm[6] = 8938496432.7761692719;
                fjkm[7] = -177729149538.54760085;
                fjkm[8] = 142764258530.36399735;
                fjkm[9] = 1190892990607.9998843;
                fjkm[10] = -2059313111949.9121859;
                fjkm[11] = -3727699663610.1911429;
                fjkm[12] = 10889256225145.522967;
                fjkm[13] = 3246312869985.9515053;
                fjkm[14] = -29702334678008.056483;
                fjkm[15] = 12354361209407.777556;
                fjkm[16] = 43335047349400.917011;
                fjkm[17] = -41504037524268.002306;
                fjkm[18] = -28336804589555.309295;
                fjkm[19] = 52945932725332.519399;
                fjkm[20] = -2984104941157.1313565;
                fjkm[21] = -30618403352283.411013;
                fjkm[22] = 14099792752244.009722;
                fjkm[23] = 5138575314225.4923364;
                fjkm[24] = -5394419195595.0379138;
                fjkm[25] = 1142750145697.5795143;
                SetFjk16(fjk, j: 5, k: 10, un, fjkm, v);
                #endregion
                #region [6, 10]
                fjkm[0] = 428894.40807532385991;
                fjkm[1] = -3139491.0587579525708;
                fjkm[2] = -129346362.91769878378;
                fjkm[3] = 971658116.23636815026;
                fjkm[4] = 4056885717.6650654269;
                fjkm[5] = -43776518263.068506188;
                fjkm[6] = 6886509445.7336568338;
                fjkm[7] = 666192636622.63242699;
                fjkm[8] = -1146605162893.2335873;
                fjkm[9] = -4150515189210.3819757;
                fjkm[10] = 12899393468582.902633;
                fjkm[11] = 7742402532790.6606612;
                fjkm[12] = -63390570866544.420475;
                fjkm[13] = 31574846906545.697293;
                fjkm[14] = 155509830706127.21122;
                fjkm[15] = -197328514020298.70121;
                fjkm[16] = -161495194791368.91656;
                fjkm[17] = 431593958485279.11169;
                fjkm[18] = -57955664355845.420187;
                fjkm[19] = -444813800230214.83356;
                fjkm[20] = 304547176185415.82363;
                fjkm[21] = 164529366533754.02760;
                fjkm[22] = -262168848376474.26253;
                fjkm[23] = 51430539333046.601717;
                fjkm[24] = 65933562346693.241986;
                fjkm[25] = -41287526582645.050139;
                fjkm[26] = 7341963962580.3111652;
                SetFjk16(fjk, j: 6, k: 10, un, fjkm, v);
                #endregion
                #endregion k: 10
                #region k: 11
                #region [0, 11]
                fjkm[0] = 551.33589612202058561;
                fjkm[1] = 0.0;
                fjkm[2] = -84005.433603024085289;
                fjkm[3] = 0.0;
                fjkm[4] = 2243768.1779224494292;
                fjkm[5] = 0.0;
                fjkm[6] = -24474062.725738728468;
                fjkm[7] = 0.0;
                fjkm[8] = 142062907.79753309519;
                fjkm[9] = 0.0;
                fjkm[10] = -495889784.27503030925;
                fjkm[11] = 0.0;
                fjkm[12] = 1106842816.8230144683;
                fjkm[13] = 0.0;
                fjkm[14] = -1621080552.1083370752;
                fjkm[15] = 0.0;
                fjkm[16] = 1553596899.5705800562;
                fjkm[17] = 0.0;
                fjkm[18] = -939462359.68157840255;
                fjkm[19] = 0.0;
                fjkm[20] = 325573074.18576574902;
                fjkm[21] = 0.0;
                fjkm[22] = -49329253.664509961973;
                SetFjk16(fjk, j: 0, k: 11, un, fjkm, v);
                #endregion
                #region [1, 11]
                fjkm[0] = -5789.0269092812161489;
                fjkm[1] = 6156.5841733625632060;
                fjkm[2] = 1050067.9200378010661;
                fjkm[3] = -1106071.5424398171230;
                fjkm[4] = -32534638.579875516724;
                fjkm[5] = 34030484.031823816343;
                fjkm[6] = 403822034.97468901972;
                fjkm[7] = -420138076.79184817203;
                fjkm[8] = -2628163794.2543622609;
                fjkm[9] = 2722872399.4527176577;
                fjkm[10] = 10165740577.638121340;
                fjkm[11] = -10496333767.154808213;
                fjkm[12] = -24903963378.517825536;
                fjkm[13] = 25641858589.733168515;
                fjkm[14] = 39716473526.654258344;
                fjkm[15] = -40797193894.726483060;
                fjkm[16] = -41170317838.620371488;
                fjkm[17] = 42206049105.000758192;
                fjkm[18] = 26774677250.924984473;
                fjkm[19] = -27400985490.712703408;
                fjkm[20] = -9929978762.6658553451;
                fjkm[21] = 10147027478.789699178;
                fjkm[22] = 1603200744.0965737641;
                fjkm[23] = -1636086913.2062470721;
                SetFjk16(fjk, j: 1, k: 11, un, fjkm, v);
                #endregion
                #region [2, 11]
                fjkm[0] = 33286.904728366992856;
                fjkm[1] = -72776.338288106717300;
                fjkm[2] = -7048423.0820374073034;
                fjkm[3] = 15288988.915750383523;
                fjkm[4] = 243935418.08573977628;
                fjkm[5] = -538504362.70138786302;
                fjkm[6] = -3246894911.6396827767;
                fjkm[7] = 7489063194.0760509112;
                fjkm[8] = 21666937100.705365161;
                fjkm[9] = -53983904963.062576171;
                fjkm[10] = -80910564664.877465851;
                fjkm[11] = 229101080335.06400288;
                fjkm[12] = 172760876423.44066571;
                fjkm[13] = -610977234886.30398648;
                fjkm[14] = -187937135374.72033958;
                fjkm[15] = 1053702358870.4190989;
                fjkm[16] = 18639458829.443774842;
                fjkm[17] = -1174519256075.3585225;
                fjkm[18] = 213630362751.48244186;
                fjkm[19] = 817332252922.97321022;
                fjkm[20] = -266086886489.81593243;
                fjkm[21] = -322968489592.27962303;
                fjkm[22] = 139744842706.19027127;
                fjkm[23] = 55347422611.580177333;
                fjkm[24] = -28497920919.101275948;
                SetFjk16(fjk, j: 2, k: 11, un, fjkm, v);
                #endregion
                #region [3, 11]
                fjkm[0] = -138695.43636819580357;
                fjkm[1] = 466698.94436858890046;
                fjkm[2] = 33739004.101605793511;
                fjkm[3] = -113151831.10727233791;
                fjkm[4] = -1262494179.8855194512;
                fjkm[5] = 4485679097.1221702841;
                fjkm[6] = 16876412838.414698204;
                fjkm[7] = -68734331237.685231727;
                fjkm[8] = -99319007190.867271605;
                fjkm[9] = 535143925100.64569760;
                fjkm[10] = 219559829042.68087919;
                fjkm[11] = -2402169771537.2297740;
                fjkm[12] = 385225420494.61274582;
                fjkm[13] = 6605716163805.3629860;
                fjkm[14] = -3536807746028.4626688;
                fjkm[15] = -11320974870399.200872;
                fjkm[16] = 9487604757721.1762395;
                fjkm[17] = 11727696199159.893852;
                fjkm[18] = -13727281916428.067929;
                fjkm[19] = -6409955842808.2983124;
                fjkm[20] = 11468445778721.253331;
                fjkm[21] = 723160235150.79794197;
                fjkm[22] = -5214971540434.5399686;
                fjkm[23] = 952560905703.62661138;
                fjkm[24] = 1001898723474.6755508;
                fjkm[25] = -346817425242.52751961;
                SetFjk16(fjk, j: 3, k: 11, un, fjkm, v);
                #endregion
                #region [4, 11]
                fjkm[0] = 468097.09774266083704;
                fjkm[1] = -2151404.5559841446618;
                fjkm[2] = -129076990.59865050915;
                fjkm[3] = 595316208.51428773472;
                fjkm[4] = 5064273602.5906671292;
                fjkm[5] = -26185510869.073114681;
                fjkm[6] = -61837037264.905576505;
                fjkm[7] = 433386998010.23271090;
                fjkm[8] = 186443883288.37312122;
                fjkm[9] = -3537220973754.0018335;
                fjkm[10] = 1681730347233.8501195;
                fjkm[11] = 15988837394678.875714;
                fjkm[12] = -16993822946683.547492;
                fjkm[13] = -41340811227869.288548;
                fjkm[14] = 67591520066179.679649;
                fjkm[15] = 56621717984129.588648;
                fjkm[16] = -149792388677379.47570;
                fjkm[17] = -20183328379251.847125;
                fjkm[18] = 196651762467137.74506;
                fjkm[19] = -54964390301576.370525;
                fjkm[20] = -147653284026647.88789;
                fjkm[21] = 88925712863728.440410;
                fjkm[22] = 52472318964377.745533;
                fjkm[23] = -54571161032830.893735;
                fjkm[24] = -776744894101.81078918;
                fjkm[25] = 12653076888080.966521;
                fjkm[26] = -3310861678129.4432166;
                SetFjk16(fjk, j: 4, k: 11, un, fjkm, v);
                #endregion
                #region [5, 11]
                fjkm[0] = -1357481.5834537164274;
                fjkm[1] = 7977851.1535147665703;
                fjkm[2] = 419517728.92817665197;
                fjkm[3] = -2495517054.7161424403;
                fjkm[4] = -16720780562.050493234;
                fjkm[5] = 120296564778.19436333;
                fjkm[6] = 154404724838.89607546;
                fjkm[7] = -2110123804399.9206379;
                fjkm[8] = 974123695127.88869692;
                fjkm[9] = 17443881430511.946630;
                fjkm[10] = -24448434409173.656268;
                fjkm[11] = -73513952038406.365892;
                fjkm[12] = 169708094306922.00526;
                fjkm[13] = 139392884698962.95542;
                fjkm[14] = -607701552209729.47437;
                fjkm[15] = 42904363912061.406899;
                fjkm[16] = 1241046770899817.3524;
                fjkm[17] = -768125556097572.10187;
                fjkm[18] = -1403171934960438.0242;
                fjkm[19] = 1622188475923667.7103;
                fjkm[20] = 657623934547413.88722;
                fjkm[21] = -1631882122398223.2875;
                fjkm[22] = 237083343503533.13124;
                fjkm[23] = 785835171244720.50574;
                fjkm[24] = -396417689594574.31797;
                fjkm[25] = -105868095076065.58561;
                fjkm[26] = 125179414423474.77661;
                fjkm[27] = -26396909127729.654202;
                SetFjk16(fjk, j: 5, k: 11, un, fjkm, v);
                #endregion
                #endregion k: 11
                #region k: 12
                #region [0, 12]
                fjkm[0] = 3038.0905109223842686;
                fjkm[1] = 0.0;
                fjkm[2] = -549842.32757228868713;
                fjkm[3] = 0.0;
                fjkm[4] = 17395107.553978164538;
                fjkm[5] = 0.0;
                fjkm[6] = -225105661.88941527780;
                fjkm[7] = 0.0;
                fjkm[8] = 1559279864.8792575133;
                fjkm[9] = 0.0;
                fjkm[10] = -6563293792.6192843320;
                fjkm[11] = 0.0;
                fjkm[12] = 17954213731.155600080;
                fjkm[13] = 0.0;
                fjkm[14] = -33026599749.800723140;
                fjkm[15] = 0.0;
                fjkm[16] = 41280185579.753973955;
                fjkm[17] = 0.0;
                fjkm[18] = -34632043388.158777923;
                fjkm[19] = 0.0;
                fjkm[20] = 18688207509.295824922;
                fjkm[21] = 0.0;
                fjkm[22] = -5866481492.0518472276;
                fjkm[23] = 0.0;
                fjkm[24] = 814789096.11831211495;
                SetFjk16(fjk, j: 0, k: 12, un, fjkm, v);
                #endregion
                #region [1, 12]
                fjkm[0] = -34938.040875607419089;
                fjkm[1] = 36963.434549555675268;
                fjkm[2] = 7422871.4222258972763;
                fjkm[3] = -7789432.9739407564011;
                fjkm[4] = -269624167.08666155034;
                fjkm[5] = 281220905.45598032670;
                fjkm[6] = 3939349083.0647673616;
                fjkm[7] = -4089419524.3243775468;
                fjkm[8] = -30405957365.145521510;
                fjkm[9] = 31445477275.065026519;
                fjkm[10] = 141110816541.31461314;
                fjkm[11] = -145486345736.39413603;
                fjkm[12] = -421924022682.15660188;
                fjkm[13] = 433893498502.92700194;
                fjkm[14] = 842178293619.91844007;
                fjkm[15] = -864196026786.45225550;
                fjkm[16] = -1135205103443.2342838;
                fjkm[17] = 1162725227163.0702664;
                fjkm[18] = 1021645279950.6839487;
                fjkm[19] = -1044733308876.1231340;
                fjkm[20] = -588678536542.81848505;
                fjkm[21] = 601137341549.01570167;
                fjkm[22] = 196527129983.73688212;
                fjkm[23] = -200438117645.10478028;
                fjkm[24] = -28925012912.200080081;
                fjkm[25] = 29468205642.945621491;
                SetFjk16(fjk, j: 1, k: 12, un, fjkm, v);
                #endregion
                #region [2, 12]
                fjkm[0] = 218362.75547254636931;
                fjkm[1] = -473942.11970389194590;
                fjkm[2] = -53559985.272697166145;
                fjkm[3] = 115466888.79018062430;
                fjkm[4] = 2162702487.2919505639;
                fjkm[5] = -4731469254.6820607544;
                fjkm[6] = -33930459549.835830283;
                fjkm[7] = 76986136366.180025009;
                fjkm[8] = 271095146839.75321729;
                fjkm[9] = -654897543249.28815561;
                fjkm[10] = -1244130265844.5028996;
                fjkm[11] = 3321026659065.3578720;
                fjkm[12] = 3434492363731.4649585;
                fjkm[13] = -10772528238693.360048;
                fjkm[14] = -5553407245149.3813559;
                fjkm[15] = 23184673024360.107644;
                fjkm[16] = 4148109873524.0835034;
                fjkm[17] = -33519510690760.226852;
                fjkm[18] = 1766187462911.1875877;
                fjkm[19] = 32207800350987.663468;
                fjkm[20] = -7064569616534.6127663;
                fjkm[21] = -19734747129816.391118;
                fjkm[22] = 6780185269401.9041713;
                fjkm[23] = 6981112975541.6982009;
                fjkm[24] = -3063627371132.2565100;
                fjkm[25] = -1085299076029.5917371;
                fjkm[26] = 557485489473.28346831;
                SetFjk16(fjk, j: 2, k: 12, un, fjkm, v);
                #endregion
                #region [3, 12]
                fjkm[0] = -982632.39962645866188;
                fjkm[1] = 3276416.0527937831379;
                fjkm[2] = 274430595.86767397376;
                fjkm[3] = -912438377.61048048104;
                fjkm[4] = -11979589299.502912885;
                fjkm[5] = 41816043571.570359444;
                fjkm[6] = 191330729548.58232223;
                fjkm[7] = -746943185635.96708583;
                fjkm[8] = -1416198224023.6464721;
                fjkm[9] = 6856956393274.5884258;
                fjkm[10] = 4829515891796.9969935;
                fjkm[11] = -36877687475484.603376;
                fjkm[12] = -2050957082915.0942624;
                fjkm[13] = 124378250253033.78820;
                fjkm[14] = -44312193894090.533579;
                fjkm[15] = -271207689501172.62955;
                fjkm[16] = 179586202876957.77361;
                fjkm[17] = 381618869449173.21877;
                fjkm[18] = -359490061117882.96881;
                fjkm[19] = -330393612742248.46582;
                fjkm[20] = 427952236370799.92495;
                fjkm[21] = 148052565489017.04595;
                fjkm[22] = -307266140839707.15754;
                fjkm[23] = -4687264091266.0864760;
                fjkm[24] = 123260326898726.61625;
                fjkm[25] = -24376238900981.871642;
                fjkm[26] = -21272360948501.370513;
                fjkm[27] = 7341892911307.8818418;
                SetFjk16(fjk, j: 3, k: 12, un, fjkm, v);
                #endregion
                #region [4, 12]
                fjkm[0] = 3562042.4486459126493;
                fjkm[1] = -16196313.687936474068;
                fjkm[2] = -1119546446.8249808309;
                fjkm[3] = 5105896697.6570577310;
                fjkm[4] = 51476060928.630380287;
                fjkm[5] = -258447407741.23544355;
                fjkm[6] = -779649080899.64056288;
                fjkm[7] = 4982179421789.6942804;
                fjkm[8] = 4004696303571.8542110;
                fjkm[9] = -48152095258426.146318;
                fjkm[10] = 10206156605278.263985;
                fjkm[11] = 264344253278752.35082;
                fjkm[12] = -218798857956960.84499;
                fjkm[13] = -868622560677829.56823;
                fjkm[14] = 1161601525559524.2658;
                fjkm[15] = 1687979001191903.2190;
                fjkm[16] = -3349241418127553.8805;
                fjkm[17] = -1649878672532120.5740;
                fjkm[18] = 5897451215285124.6365;
                fjkm[19] = -125952827188585.92633;
                fjkm[20] = -6433283969334751.7469;
                fjkm[21] = 2343935079955608.5977;
                fjkm[22] = 4106725553395393.7642;
                fjkm[23] = -2735980005540132.5807;
                fjkm[24] = -1230142327980830.8915;
                fjkm[25] = 1417490532431040.0017;
                fjkm[26] = -23384761374805.900562;
                fjkm[27] = -289892454526107.40352;
                fjkm[28] = 75592403781851.468191;
                SetFjk16(fjk, j: 4, k: 12, un, fjkm, v);
                #endregion
                #endregion k: 12
                #region k: 13
                #region [0, 13]
                fjkm[0] = 18257.755474293174691;
                fjkm[1] = 0.0;
                fjkm[2] = -3871833.4425726126206;
                fjkm[3] = 0.0;
                fjkm[4] = 143157876.71888898129;
                fjkm[5] = 0.0;
                fjkm[6] = -2167164983.2237950935;
                fjkm[7] = 0.0;
                fjkm[8] = 17634730606.834969383;
                fjkm[9] = 0.0;
                fjkm[10] = -87867072178.023265677;
                fjkm[11] = 0.0;
                fjkm[12] = 287900649906.15058872;
                fjkm[13] = 0.0;
                fjkm[14] = -645364869245.37650328;
                fjkm[15] = 0.0;
                fjkm[16] = 1008158106865.3820948;
                fjkm[17] = 0.0;
                fjkm[18] = -1098375156081.2233068;
                fjkm[19] = 0.0;
                fjkm[20] = 819218669548.57732864;
                fjkm[21] = 0.0;
                fjkm[22] = -399096175224.46649796;
                fjkm[23] = 0.0;
                fjkm[24] = 114498237732.02580995;
                fjkm[25] = 0.0;
                fjkm[26] = -14679261247.695616661;
                SetFjk16(fjk, j: 0, k: 13, un, fjkm, v);
                #endregion
                #region [1, 13]
                fjkm[0] = -228221.94342866468364;
                fjkm[1] = 240393.78041152680010;
                fjkm[2] = 56141584.917302882999;
                fjkm[3] = -58722807.212351291413;
                fjkm[4] = -2362104965.8616681913;
                fjkm[5] = 2457543550.3409275122;
                fjkm[6] = 40092552189.640209230;
                fjkm[7] = -41537328845.122739292;
                fjkm[8] = -361511977440.11687235;
                fjkm[9] = 373268464511.34018528;
                fjkm[10] = 1977009124005.5234777;
                fjkm[11] = -2035587172124.2056548;
                fjkm[12] = -7053565922700.6894237;
                fjkm[13] = 7245499689304.7898162;
                fjkm[14] = 17102169035002.477337;
                fjkm[15] = -17532412281166.061672;
                fjkm[16] = -28732506045663.389701;
                fjkm[17] = 29404611450240.311097;
                fjkm[18] = 33500442260477.310858;
                fjkm[19] = -34232692364531.459729;
                fjkm[20] = -26624606760328.763181;
                fjkm[21] = 27170752540027.814733;
                fjkm[22] = 13768818045244.094179;
                fjkm[23] = -14034882162060.405178;
                fjkm[24] = -4179185677218.9420633;
                fjkm[25] = 4255517835706.9592699;
                fjkm[26] = 565151558036.28124143;
                fjkm[27] = -574937732201.41165254;
                SetFjk16(fjk, j: 1, k: 13, un, fjkm, v);
                #endregion
                #region [2, 13]
                fjkm[0] = 1540498.1181434866146;
                fjkm[1] = -3322911.4963213577938;
                fjkm[2] = -433313348.25129661430;
                fjkm[3] = 929240026.21742702895;
                fjkm[4] = 20173953055.394385937;
                fjkm[5] = -43806310275.980028275;
                fjkm[6] = -367752562201.24170098;
                fjkm[7] = 823522693625.04213554;
                fjkm[8] = 3453452850623.2709660;
                fjkm[9] = -8147245540357.7558550;
                fjkm[10] = -18967395863304.498472;
                fjkm[11] = 48502623842268.842654;
                fjkm[12] = 64652876623215.013090;
                fjkm[13] = -187135422438997.88267;
                fjkm[14] = -137928375585894.45832;
                fjkm[15] = 487895841149504.63648;
                fjkm[16] = 171009666849543.97695;
                fjkm[17] = -877097552972882.42245;
                fjkm[18] = -74254863627598.106482;
                fjkm[19] = 1089588154832573.5204;
                fjkm[20] = -116085557257555.85969;
                fjkm[21] = -919163347233503.76274;
                fjkm[22] = 228872931917376.68922;
                fjkm[23] = 502861180782827.78742;
                fjkm[24] = -180138187046491.99093;
                fjkm[25] = -160984522251228.28879;
                fjkm[26] = 71472589051967.572740;
                fjkm[27] = 22899647546405.161991;
                fjkm[28] = -11739127546959.248774;
                SetFjk16(fjk, j: 2, k: 13, un, fjkm, v);
                #endregion
                #region [3, 13]
                fjkm[0] = -7445740.9043601853037;
                fjkm[1] = 24634048.351746637287;
                fjkm[2] = 2366020806.0262900637;
                fjkm[3] = -7808648260.6425043412;
                fjkm[4] = -118977141825.00262846;
                fjkm[5] = 409347654830.37623610;
                fjkm[6] = 2227886871742.0341399;
                fjkm[7] = -8418196051278.7614582;
                fjkm[8] = -19993114336865.499162;
                fjkm[9] = 89749606285710.604304;
                fjkm[10] = 91027650219177.795567;
                fjkm[11] = -567320456048460.00800;
                fjkm[12] = -158541319151866.40491;
                fjkm[13] = 2286994298876364.5224;
                fjkm[14] = -406108119093234.18714;
                fjkm[15] = -6109006264284954.9274;
                fjkm[16] = 3061509093597577.3617;
                fjkm[17] = 10949569443954370.550;
                fjkm[18] = -8409227264715248.2782;
                fjkm[19] = -12972671111828071.122;
                fjkm[20] = 13506857028733485.383;
                fjkm[21] = 9542099548494263.3202;
                fjkm[22] = -13665198589678693.875;
                fjkm[23] = -3500771616752033.2515;
                fjkm[24] = 8599505926079281.8598;
                fjkm[25] = -192584364085979.71888;
                fjkm[26] = -3085105840164255.6689;
                fjkm[27] = 648294322883069.58159;
                fjkm[28] = 483163296698761.31750;
                fjkm[29] = -166336791576720.83219;
                SetFjk16(fjk, j: 3, k: 13, un, fjkm, v);
                #endregion
                #endregion k: 13
                #region k: 14
                #region [0, 14]
                fjkm[0] = 118838.42625678325312;
                fjkm[1] = 0.0;
                fjkm[2] = -29188388.122220813403;
                fjkm[3] = 0.0;
                fjkm[4] = 1247009293.5127103248;
                fjkm[5] = 0.0;
                fjkm[6] = -21822927757.529223729;
                fjkm[7] = 0.0;
                fjkm[8] = 205914503232.41001569;
                fjkm[9] = 0.0;
                fjkm[10] = -1196552880196.1815990;
                fjkm[11] = 0.0;
                fjkm[12] = 4612725780849.1319668;
                fjkm[13] = 0.0;
                fjkm[14] = -12320491305598.287160;
                fjkm[15] = 0.0;
                fjkm[16] = 23348364044581.840938;
                fjkm[17] = 0.0;
                fjkm[18] = -31667088584785.158403;
                fjkm[19] = 0.0;
                fjkm[20] = 30565125519935.320612;
                fjkm[21] = 0.0;
                fjkm[22] = -20516899410934.437391;
                fjkm[23] = 0.0;
                fjkm[24] = 9109341185239.8989559;
                fjkm[25] = 0.0;
                fjkm[26] = -2406297900028.5039611;
                fjkm[27] = 0.0;
                fjkm[28] = 286464035717.67904299;
                SetFjk16(fjk, j: 0, k: 14, un, fjkm, v);
                #endregion
                #region [1, 14]
                fjkm[0] = -1604318.7544665739172;
                fjkm[1] = 1683544.3719710960859;
                fjkm[2] = 452420015.89442260775;
                fjkm[3] = -471878941.30923648336;
                fjkm[4] = -21822662636.472430684;
                fjkm[5] = 22654002165.480904234;
                fjkm[6] = 425547091271.81986272;
                fjkm[7] = -440095709776.83934521;
                fjkm[8] = -4427161819496.8153373;
                fjkm[9] = 4564438154985.0886811;
                fjkm[10] = 28118992684610.267576;
                fjkm[11] = -28916694604741.055309;
                fjkm[12] = -117624507411652.86515;
                fjkm[13] = 120699657932218.95313;
                fjkm[14] = 338813510903952.89689;
                fjkm[15] = -347027171774351.75500;
                fjkm[16] = -688776739315164.30766;
                fjkm[17] = 704342315344885.53495;
                fjkm[18] = 997513290420732.48968;
                fjkm[19] = -1018624682810589.2619;
                fjkm[20] = -1023931704917833.2405;
                fjkm[21] = 1044308455264456.7876;
                fjkm[22] = 728349929088172.52737;
                fjkm[23] = -742027862028795.48563;
                fjkm[24] = -341600294446496.21085;
                fjkm[25] = 347673188569989.47682;
                fjkm[26] = 95048767051125.906463;
                fjkm[27] = -96652965651144.909104;
                fjkm[28] = -11888257482283.680284;
                fjkm[29] = 12079233506095.466313;
                SetFjk16(fjk, j: 1, k: 14, un, fjkm, v);
                #endregion
                #region [2, 14]
                fjkm[0] = 11631310.969882660899;
                fjkm[1] = -24956069.513924483156;
                fjkm[2] = -3719130469.3827566264;
                fjkm[3] = 7939241569.2440612457;
                fjkm[4] = 197650420583.57805736;
                fjkm[5] = -426477178381.34693109;
                fjkm[6] = -4137136219101.0505865;
                fjkm[7] = 9165629658162.2739663;
                fjkm[8] = 44999979919399.924736;
                fjkm[9] = -104192738635599.46794;
                fjkm[10] = -290053332678279.44824;
                fjkm[11] = 717931728117708.95938;
                fjkm[12] = 1184950942733150.9332;
                fjkm[13] = -3238133498156090.6407;
                fjkm[14] = -3148099361614567.8423;
                fjkm[15] = 10004238940145809.174;
                fjkm[16] = 5326672157182975.4416;
                fjkm[17] = -21713978561461112.072;
                fjkm[18] = -4997511985428331.4237;
                fjkm[19] = 33440445545533127.273;
                fjkm[20] = 429328409587666.98618;
                fjkm[21] = -36372499368723031.528;
                fjkm[22] = 5419838346824587.4483;
                fjkm[23] = 27328510015364670.604;
                fjkm[24] = -7462027883028047.7909;
                fjkm[25] = -13500043636525530.253;
                fjkm[26] = 5000259547410615.2462;
                fjkm[27] = 3946328556046746.4962;
                fjkm[28] = -1769166076587921.0596;
                fjkm[29] = -517354048506128.35163;
                fjkm[30] = 264752449010576.61885;
                SetFjk16(fjk, j: 2, k: 14, un, fjkm, v);
                #endregion
                #endregion k: 14
                #region k: 15
                #region [0, 15]
                fjkm[0] = 832859.30401628929898;
                fjkm[1] = 0.0;
                fjkm[2] = -234557963.52225152478;
                fjkm[3] = 0.0;
                fjkm[4] = 11465754899.448237157;
                fjkm[5] = 0.0;
                fjkm[6] = -229619372968.24646817;
                fjkm[7] = 0.0;
                fjkm[8] = 2485000928034.0853236;
                fjkm[9] = 0.0;
                fjkm[10] = -16634824724892.480519;
                fjkm[11] = 0.0;
                fjkm[12] = 74373122908679.144941;
                fjkm[13] = 0.0;
                fjkm[14] = -232604831188939.92523;
                fjkm[15] = 0.0;
                fjkm[16] = 523054882578444.65558;
                fjkm[17] = 0.0;
                fjkm[18] = -857461032982895.05140;
                fjkm[19] = 0.0;
                fjkm[20] = 1026955196082762.4888;
                fjkm[21] = 0.0;
                fjkm[22] = -889496939881026.44181;
                fjkm[23] = 0.0;
                fjkm[24] = 542739664987659.72270;
                fjkm[25] = 0.0;
                fjkm[26] = -221349638702525.19597;
                fjkm[27] = 0.0;
                fjkm[28] = 54177510755106.049005;
                fjkm[29] = 0.0;
                fjkm[30] = -6019723417234.0054450;
                SetFjk16(fjk, j: 0, k: 15, un, fjkm, v);
                #endregion
                #region [1, 15]
                fjkm[0] = -12076459.908236194835;
                fjkm[1] = 12631699.444247054368;
                fjkm[2] = 3870206398.1171501588;
                fjkm[3] = -4026578373.7986511753;
                fjkm[4] = -212116465639.79238740;
                fjkm[5] = 219760302239.42454551;
                fjkm[6] = 4707197145849.0525974;
                fjkm[7] = -4860276727827.8835762;
                fjkm[8] = -55912520880766.919782;
                fjkm[9] = 57569188166122.976664;
                fjkm[10] = 407553205759865.77271;
                fjkm[11] = -418643088909794.09305;
                fjkm[12] = -1970887757079997.3409;
                fjkm[13] = 2020469839019116.7709;
                fjkm[14] = 6629237688884787.8691;
                fjkm[15] = -6784307576344081.1526;
                fjkm[16] = -15953173918642561.995;
                fjkm[17] = 16301877173694858.432;
                fjkm[18] = 27867483571944089.170;
                fjkm[19] = -28439124260599352.538;
                fjkm[20] = -35429954264855305.864;
                fjkm[21] = 36114591062243814.190;
                fjkm[22] = 32466638305657465.126;
                fjkm[23] = -33059636265578149.421;
                fjkm[24] = -20895477102024899.324;
                fjkm[25] = 21257303545350005.806;
                fjkm[26] = 8964660367452270.4366;
                fjkm[27] = -9112226793253953.9006;
                fjkm[28] = -2302544207092007.0827;
                fjkm[29] = 2338662547595411.1154;
                fjkm[30] = 267877692066913.24230;
                fjkm[31] = -271890841011735.91260;
                SetFjk16(fjk, j: 1, k: 15, un, fjkm, v);
                #endregion
                #endregion k: 15
                #region k: 16
                #region [0, 16]
                fjkm[0] = 6252951.4934347970025;
                fjkm[1] = 0.0;
                fjkm[2] = -2001646928.1917763315;
                fjkm[3] = 0.0;
                fjkm[4] = 110997405139.17901279;
                fjkm[5] = 0.0;
                fjkm[6] = -2521558474912.8546213;
                fjkm[7] = 0.0;
                fjkm[8] = 31007436472896.461417;
                fjkm[9] = 0.0;
                fjkm[10] = -236652530451649.25168;
                fjkm[11] = 0.0;
                fjkm[12] = 1212675804250347.4165;
                fjkm[13] = 0.0;
                fjkm[14] = -4379325838364015.4378;
                fjkm[15] = 0.0;
                fjkm[16] = 11486706978449752.110;
                fjkm[17] = 0.0;
                fjkm[18] = -22268225133911142.562;
                fjkm[19] = 0.0;
                fjkm[20] = 32138275268586241.200;
                fjkm[21] = 0.0;
                fjkm[22] = -34447226006485144.698;
                fjkm[23] = 0.0;
                fjkm[24] = 27054711306197081.241;
                fjkm[25] = 0.0;
                fjkm[26] = -15129826322457681.181;
                fjkm[27] = 0.0;
                fjkm[28] = 5705782159023670.8096;
                fjkm[29] = 0.0;
                fjkm[30] = -1301012723549699.4268;
                fjkm[31] = 0.0;
                fjkm[32] = 135522158703093.69029;
                SetFjk16(fjk, j: 0, k: 16, un, fjkm, v);
                #endregion
                #endregion k: 16
            }

            static void SetFjk16(double[,] fjk, int j, int k, double[] un, double[] fjkm, double v)
            {
                int d = j + 2 * k;
                fjk[j, k] = un[d] * Pol(fjkm, d, v);
            }

            static int Startingpser(double mu, double x, double y)
            {
                double mulnmu = mu * Math.Log(mu);
                double lnx = Math.Log(x);
                double lny = Math.Log(y);

                double n = x < 2 ? x + 5 : 1.5 * x;

                double n1 = 0;
                int a = 0;
                int b = 0;

                while (Math.Abs(n - n1) > 1)
                {
                    n1 = n;
                    n = Ps(mu, mulnmu, lnx, y, lny, n, a, b);
                }

                n += 1;

                if (mu + n > y)
                {
                    if (y > mu)
                    {
                        a = 1;
                    }
                    else
                    {
                        b = 1;
                    }

                    n1 = 0;

                    while (Math.Abs(n - n1) > 1)
                    {
                        n1 = n;
                        n = Ps(mu, mulnmu, lnx, y, lny, n, a, b);
                    }
                }

                return (int)Math.Round(n) + 1;
            }

            static double Ps(double mu, double mulnmu, double lnx,double y, double lny, double n, int a, int b)
            {
                double lneps = Math.Log(Epss);

                if ((a == 0) && (b == 0))
                {
                    return (n - lneps) / (Math.Log(n) - lnx);
                }
                else if ((a == 0) && (b == 1))
                {
                    return (2 * n - lneps + mulnmu - mu * Math.Log(mu + n)) / (Math.Log(n) - lnx - lny + Math.Log(mu + n));
                }
                else if ((a == 1) && (b == 0))
                {
                    return (2 * n - lneps - y + mu * lny - mu * Math.Log(mu + n) + mu) / (Math.Log(n) - lnx - lny + Math.Log(mu + n));
                }
                else
                {
                    throw new ArgumentException("(a,b) must be (a==0&b==0)||(a==1&b==0)||(a==0&b==1)");
                }

            }

            static void Hypfun(double x, out double sinh, out double cosh)
            {
                double ax = Math.Abs(x);
                double y, f, f2, ss;

                if (ax < 0.21)
                {
                    y = ax < 0.07 ? x * x : x * x / 9;

                    f = 2.0 + y * (y * 28 + 2520.0) / (y * (y + 420) + 15120.0);
                    f2 = f * f;
                    sinh = 2 * x * f / (f2 - y);
                    cosh = (f2 + y) / (f2 - y);

                    if (ax >= 0.07)
                    {
                        ss = 2.0 * sinh / 3.0;
                        f = ss * ss;

                        sinh = sinh * (1.0 + f / 3.0);
                        cosh = cosh * (1.0 + f);
                    }
                }
                else
                {
                    y = Math.Exp(x);
                    f = 1.0 / y;
                    cosh = (y + f) / 2.0;
                    sinh = (y - f) / 2.0;
                }
            }

            // Computes the Incomplete Gama(1/2-n,x), x >= 0, n=0,1,2, ...
            static double Ignega(int n, double x)
            {
                double a = 0.5 - n;
                double delta = Epss / 100.0;
                double g;

                if (x > 1.5)
                {
                    double p = 0.0;
                    double q = (x - 1 - a) * (x + 1 - a);
                    double r = 4 * (x + 1 - a);
                    double s = 1 - a;
                    double ro = 0.0;
                    double t = 1.0;
                    g = 1.0;

                    while ((t / g) > delta)
                    {
                        p += s;
                        q += r;
                        r += 8;
                        s += 2;
                        double tau = p * (1 + ro);
                        ro = tau / (q - tau);
                        t *= ro;
                        g += t;
                    }

                    g *= Math.Exp(a * Math.Log(x)) / (x + 1 - a);
                }
                else
                {
                    double t = 1;
                    double s = 1.0 / a;
                    int k = 1;

                    while (Math.Abs(t / s) > delta)
                    {
                        t *= -x / k;
                        s += t / (k + a);
                        k++;
                    }

                    g = Constants.SqrtPi;

                    for (int i = 1; i <= n; i++)
                    {
                        g /= (0.5 - i);
                    }

                    g = Math.Exp(x) * (g - Math.Exp(a * Math.Log(x)) * s);
                }

                return g;
            }

            static double Alfinv(int t, double r)
            {
                double a, b;

                if (t + r < 2.7)
                {
                    if (t == 0)
                    {
                        a = Math.Exp(Math.Log(3.0 * r) / 3.0);
                        double a2 = a * a;
                        b = a * (1.0 + a2 * (-1.0 / 30.0 + 0.004312 * a2));
                    }
                    else
                    {
                        a = Math.Sqrt(2.0 * (1.0 + r));
                        double a2 = a * a;
                        b = a / (1.0 + a2 / 8.0);
                    }
                }
                else
                {
                    a = Math.Log(0.7357589 * (r + t));
                    double lna = Math.Log(a) / a;
                    b = 1.0 + a + Math.Log(a) * (1.0 / a - 1.0) + 0.5 * lna * lna;
                }

                while (Math.Abs(a / b - 1) > 1e-2)
                {
                    a = b;
                    b = Fi(a, r, t);
                }

                return b;
            }

            static double Falfa(double al, double r, int t, out double df)
            {
                Hypfun(al, out double sh, out double ch);

                double falfa;

                if (t == 1)
                {
                    falfa = al * sh / ch - 1.0 - r / ch;
                    df = (sh + (al + r * sh) / ch) / ch;
                }
                else
                {
                    falfa = al - (sh + r) / ch;
                    df = al - (sh + r) / ch;
                }

                return falfa;
            }

            static double Fi(double al, double r, int t)
            {
                double p = Falfa(al, r, t, out var q);
                double fi = al - p / q;

                return fi;
            }

            static double Recipgam(double x, out double q, out double r)
            {
                if (x == 0)
                {
                    q = 0.5772156649015328606e-0;
                    r = -0.6558780715202538811e-0;
                }
                else
                {
                    double tx = 2 * x;
                    double t = 2 * tx * tx - 1;

                    double[] c = {
                            +1.142022680371167841,
                            -6.5165112670736881e-3,
                            -3.087090173085368e-4,
                            +3.4706269649043e-6,
                            -6.9437664487e-9,
                            -3.67795399e-11,
                            +1.356395e-13,
                            +3.68e-17,
                            -5.5e-19,
                    };
                    q = Chepolsum(8, t, c);

                    c = new[]
                    {
                            -1.270583625778727532,
                            +2.05083241859700357e-2,
                            -7.84761097993185e-5,
                            -5.377798984020e-7,
                            +3.8823289907e-9,
                            -2.6758703e-12,
                            -2.39860e-14,
                            +3.80e-17,
                            +4e-20,
                    };
                    r = Chepolsum(8, t, c);
                }

                double recipgam = 1 + x * (q + x * r);

                return recipgam;
            }

            static double Errorfunction(double x, bool erfcc, bool expo)
            {
                double y;

                if (erfcc)
                {
                    if (x < -6.5)
                    {
                        y = 2;
                    }
                    else if (x < 0)
                    {
                        y = 2 - Errorfunction(-x, true, false);
                    }
                    else if (x == 0)
                    {
                        y = 1;
                    }
                    else if (x < 0.5)
                    {
                        y = expo ? Math.Exp(x * x) : 1;

                        y = y * (1 - Errorfunction(x, false, false));
                    }
                    else if (x < 4)
                    {
                        y = expo ? 1 : Math.Exp(-x * x);

                        double[] r = {
                                1.230339354797997253e3,
                                2.051078377826071465e3,
                                1.712047612634070583e3,
                                8.819522212417690904e2,
                                2.986351381974001311e2,
                                6.611919063714162948e1,
                                8.883149794388375941,
                                5.641884969886700892e-1,
                                2.153115354744038463e-8
                        };
                        double[] s = {
                                1.230339354803749420e3,
                                3.439367674143721637e3,
                                4.362619090143247158e3,
                                3.290799235733459627e3,
                                1.621389574566690189e3,
                                5.371811018620098575e2,
                                1.176939508913124993e2,
                                1.574492611070983473e1
                        };

                        y = y * Fractio(x, 8, r, s);
                    }
                    else
                    {
                        double z = x * x;

                        y = expo ? 1 : Math.Exp(-z);

                        z = 1 / z;

                        double[] r = {
                                6.587491615298378032e-4,
                                1.608378514874227663e-2,
                                1.257817261112292462e-1,
                                3.603448999498044394e-1,
                                3.053266349612323440e-1,
                                1.631538713730209785e-2
                        };
                        double[] s = {
                                2.335204976268691854e-3,
                                6.051834131244131912e-2,
                                5.279051029514284122e-1,
                                1.872952849923460472,
                                2.568520192289822421
                        };

                        y = y * (Constants.InvSqrtPi - z * Fractio(z, 5, r, s)) / x;
                    }
                }
                else
                {
                    if (x == 0)
                    {
                        y = 0;
                    }
                    else if (Math.Abs(x) > 6.5)
                    {
                        y = x / Math.Abs(x);
                    }
                    else if (x > 0.5)
                    {
                        y = 1 - Errorfunction(x, true, false);
                    }
                    else if (x < -0.5)
                    {
                        y = Errorfunction(-x, true, false) - 1;
                    }
                    else
                    {
                        double[] r = {
                                3.209377589138469473e3,
                                3.774852376853020208e2,
                                1.138641541510501556e2,
                                3.161123743870565597e0,
                                1.857777061846031527e-1
                        };
                        double[] s = {
                                2.844236833439170622e3,
                                1.282616526077372276e3,
                                2.440246379344441733e2,
                                2.360129095234412093e1
                        };

                        double z = x * x;

                        y = x * Fractio(z, 4, r, s);
                    }
                }

                return y;
            }

            static double Fractio(double x, int n, double[] r, double[] s)
            {
                double a = r[n];
                double b = 1;

                for (int k = n - 1; k >= 0; k--)
                {
                    a = a * x + r[k];
                    b = b * x + s[k];
                }

                return a / b;
            }

            static double Zetaxy(double x, double y)
            {
                double zeta;

                double z = (y - x - 1);
                double x2 = Math.Pow(x, 2);// x * x;
                double x3 = Math.Pow(x, 3);//x2 * x;
                double x4 = Math.Pow(x, 4);//x3 * x;
                double x5 = Math.Pow(x, 5);//x4 * x;
                double x6 = Math.Pow(x, 6);//x5 * x;
                double x7 = Math.Pow(x, 7);//x6 * x;
                double x8 = Math.Pow(x, 8);//x7 * x;
                double x9 = Math.Pow(x, 9);//x8 * x;
                double x10 = Math.Pow(x, 10);//x9 * x;
                double x2p1 = 2 * x + 1;

                if (Math.Abs(z) < 0.05)
                {
                    double[] ck = new double[11];

                    ck[0] = 1.0;
                    ck[1] = -(1.0 / 3.0) * (3 * x + 1);
                    ck[2] = (1.0 / 36.0) * (72 * x2 + 42 * x + 7);
                    ck[3] = -(1.0 / 540.0) * (2700 * x3 + 2142 * x2 + 657 * x + 73);
                    ck[4] = (1.0 / 12960.0) * (1331 + 15972 * x + 76356 * x2 + 177552 * x3 + 181440.0 * x4);
                    ck[5] = -(1.0 / 272160.0) * (22409 + 336135.0 * x + 2115000.0 * x2 + 7097868.0 * x3 + 13105152.0 * x4
                        + 11430720.0 * x5);
                    ck[6] = (1.0 / 5443200.0) * (6706278.0 * x + 52305684.0 * x2 + 228784392.0 * x3
                        + 602453376.0 * x4 + 935038080.0 * x5 + 718502400.0 * x6 + 372571.0);
                    ck[7] = -(1.0 / 16329600.0) * (953677.0 + 20027217.0 * x + 186346566.0 * x2 + 1003641768.0 * x3
                        + 3418065864.0 * x4 + 7496168976.0 * x5 + 10129665600.0 * x6 + 7005398400.0 * x7);
                    ck[8] = (1.0 / 783820800.0) * (39833047.0 + 955993128.0 * x + 1120863744000.0 * x8
                        + 10332818424.0 * x2 + 66071604672.0 * x3 + 275568952176.0 * x4
                        + 776715910272.0 * x5 + 1472016602880.0 * x6 + 1773434373120.0 * x7);
                    ck[9] = -(1.0 / 387991296000.0) * (17422499659.0 + 470407490793.0 * x + 3228423729868800.0 * x8
                        + 1886413681152000.0 * x9 + 5791365522720.0 * x2 + 42859969263000.0 * x3 + 211370902874640.0 * x4
                        + 726288467241168.0 * x5 + 1759764571151616.0 * x6 + 2954947944510720.0 * x7);
                    ck[10] = (1.0 / 6518253772800.0) * (261834237251.0 + 7855027117530.0 * x
                        + 200149640441008128.0 * x8 + 200855460151664640.0 * x9 + 109480590367948800.0 * x10
                        + 108506889674064.0 * x2 + 912062714644368.0 * x3 + 5189556987668592.0 * x4
                        + 21011917557260448.0 * x5 + 61823384007654528.0 * x6 + 132131617757148672.0 * x7);

                    double z2 = z / (x2p1 * x2p1);
                    double S = 1;
                    double t = 1;
                    int k = 1;

                    while ((Math.Abs(t) > 1e-15) && (k < 11))
                    {
                        t = ck[k] * Math.Pow(z2, k);
                        S += t;
                        k++;
                    }

                    zeta = -z / Math.Sqrt(x2p1) * S;
                }
                else
                {
                    double w = Math.Sqrt(1.0 + 4 * x * y);
                    zeta = Math.Sqrt(2.0 * (x + y - w - Math.Log(2.0 * y / (1.0 + w))));

                    if (x + 1 < y)
                    {
                        zeta = -zeta;
                    }
                }

                return zeta;
            }

            static double Chepolsum(int n, double t, double[] ak)
            {
                double u0 = 0;
                double u1 = 0;
                double u2 = 0;
                double tt = t + t;
                int k = n;

                while (k >= 0)
                {
                    u2 = u1;
                    u1 = u0;
                    u0 = tt * u1 - u2 + ak[k];
                    k--;
                }

                double s = (u0 - u2) / 2;

                return s;
            }

            static double Oddchepolsum(int n, double x, double[] ak)
            {
                double s;

                if (n == 0)
                {
                    s = ak[0] * x;
                }
                else if (n == 1)
                {
                    s = x * (ak[0] + ak[1] * (4 * x * x - 3));
                }
                else
                {
                    double y = 2 * (2 * x * x - 1);
                    double r = ak[n];
                    double h = ak[n - 1] + r * y;
                    int k = n - 2;

                    while (k >= 0)
                    {
                        s = r;
                        r = h;
                        h = ak[k] + r * y - s;
                        k--;
                    }

                    s = x * (h - r);
                }

                return s;
            }

            static double Logoneplusx(double t)
            {
                double y;

                if ((-0.2928 < t) && (t < 0.4142))
                {
                    double[] ck = new double[101];
                    double p = TwoExp1Over4;
                    p = (p - 1) / (p + 1);

                    double pj = p;
                    ck[0] = pj;
                    double p2 = p * p;
                    double c = 1;
                    int j = 1;

                    while (Math.Abs(c) > 1e-20)
                    {
                        pj *= p2;
                        c = pj / (2.0 * j + 1.0);
                        ck[j] = c;
                        j++;
                    }

                    double x = t / (2.0 + t) * (1.0 + p2) / (2.0 * p);
                    y = 4 * Oddchepolsum(j - 1, x, ck);
                }
                else
                {
                    y = Math.Log(1 + t);
                }

                return y;
            }

            static double Xminsinx(double x)
            {
                double f;

                if (Math.Abs(x) > 1)
                {
                    f = 6 * (x - Math.Sin(x)) / (x * x * x);
                }
                else
                {
                    double[] fk = new double[]
                    {
                            1.95088260487819821294e-0,
                            -0.244124470324439564863e-1,
                            0.14574198156365500e-3,
                            -0.5073893903402518e-6,
                            0.11556455068443e-8,
                            -0.185522118416e-11,
                            0.22117315e-14,
                            -0.2035e-17,
                            0.15e-20,
                    };

                    double t = 2 * x * x - 1.0;

                    f = Chepolsum(8, t, fk);
                }

                return f;
            }

            static double Trapsum(double a, double b, double h, double d, double xis2, double mu, double wxis, double ys)
            {
                double s = 0;
                double b0 = b;
                double inte, aa, bb;

                if (d == 0)
                {
                    Integrand(a, ref b0, out inte, xis2, mu, wxis, ys);

                    s = inte / 2.0;
                    aa = a + h;
                    bb = b - h / 2.0;
                }
                else
                {
                    aa = a + d;
                    bb = b;
                }

                while ((aa < bb) && (aa < b0))
                {
                    Integrand(aa, ref b0, out inte, xis2, mu, wxis, ys);
                    s += inte;
                    aa += h;
                }

                return s * h;
            }

            static double Trap(double a, double b, double e, double xis2, double mu, double wxis, double ys)
            {
                double h = (b - a) / 8d;
                double p = Trapsum(a, b, h, 0, xis2, mu, wxis, ys);
                double nc = 0; // double? int?
                double v = 1;

                while (((v > e) && (nc < 10)) || (nc <= 2))
                {
                    nc += 1;
                    double q = Trapsum(a, b, h, h / 2, xis2, mu, wxis, ys);
                    v = Math.Abs(q) > 0 ? Math.Abs(p / q - 1) : 0;
                    h /= 2;
                    p = (p + q) / 2;
                }

                return p;
            }

            static void Integrand(double theta, ref double b0, out double inte, double xis2, double mu, double wxis, double ys)
            {
                double eps = 1e-16;
                double lneps = Math.Log(eps);
                double f;

                if (theta > b0)
                {
                    f = 0;
                }
                else if (Math.Abs(theta) < 1e-10)
                {
                    double rtheta = (1.0 + wxis) / (2 * ys);
                    double theta2 = theta * theta;
                    double psitheta = -wxis * theta2 * 0.5;
                    f = rtheta / (1.0 - rtheta) * Math.Exp(mu * psitheta);
                }
                else
                {
                    double theta2 = theta * theta;
                    double sintheta = Math.Sin(theta);
                    double costheta = Math.Cos(theta);
                    double ts = theta / sintheta;
                    double s2 = sintheta * sintheta;
                    double wx = Math.Sqrt(ts * ts + xis2);
                    double xminsinxtheta = Xminsinx(theta);
                    double p = xminsinxtheta * theta2 * ts / 6.0;
                    double term1 = (p * (ts + 1) - theta2 - s2 * xis2) / (costheta * wx + wxis);
                    p *= (1.0 + (ts + 1) / (wx + wxis)) / (1 + wxis);
                    double term2 = -Logoneplusx(p);
                    p = term1 + term2;
                    double psitheta = p;

                    f = mu * psitheta;

                    if (f > lneps)
                    {
                        f = Math.Exp(f);
                    }
                    else
                    {
                        f = 0;
                        b0 = Math.Min(theta, b0);
                    }

                    double rtheta = (ts + wx) / (2 * ys);
                    double sinth = Math.Sin(theta / 2.0);
                    p = (2 * theta * sinth * sinth - xminsinxtheta * theta2 * theta / 6) / (2 * ys * s2);
                    double dr = p * (1 + ts / wx);
                    p = (dr * sintheta + (costheta - rtheta) * rtheta) / (rtheta * (rtheta - 2 * costheta) + 1);
                    double ft = p;

                    f *= ft;
                }

                inte = f;
            }

            /* Computes the series expansion for Q.
             * For computing the incomplete gamma functions we use the routine incgam included in the module IncgamFI.
             * Reference: A. Gil, J. Segura and NM Temme, Efficient and accurate algorithms for
             * the computation and inversion of the incomplete gamma function ratios. SIAM J Sci Comput.*/
            static void Qser(double mu, double x, double y, out double p, out double q, out int ierro)
            {
                ierro = 0;

                IncompleteGamma.Incgam(mu, y, out p, out q, out var ierr);

                double q0 = q;
                double lh0 = mu * Math.Log(y) - y - IncompleteGamma.Loggam(mu + 1);

                if ((lh0 > Math.Log(Dwarf)) && (x < 100))
                {
                    double h0 = Math.Exp(lh0);
                    double xy = x * y;
                    double delta = Epss / 100;

                    for (int n = 0; (q0 / q > delta) && (n < 1000); n++)
                    {
                        q0 = x * (q0 + h0) / (n + 1.0);
                        h0 = xy * h0 / ((n + 1.0) * (mu + n + 1));
                        q = q + q0;
                    }

                    q = Math.Exp(-x) * q;
                    p = 1 - q;
                }
                else
                {
                    // Computing Q forward
                    double x1 = y;
                    double s = 0;
                    double t;
                    double k = 0;
                    int m = 0;

                    while ((k < 10000) && (m == 0))
                    {
                        double a = mu + k;

                        IncompleteGamma.Incgam(a, x1, out _, out double q1, out ierr);

                        t = IncompleteGamma.Dompart(k, x, false) * q1;
                        s += t;
                        k += 1;

                        if ((s == 0) && (k < 150))
                        {
                            m = 1;
                        }

                        if (s > 0)
                        {
                            if (((t / s) < 1e-16) && (k > 10))
                            {
                                m = 1;
                            }
                        }
                    }

                    if (ierr == 0)
                    {
                        q = s;
                        p = 1 - q;
                    }
                    else
                    {
                        q = 0;
                        p = 1;
                        ierro = 1;
                    }
                }
            }

            /* Computes backward the series expansion for P.
             * For computing the incomplete gamma functions we use the routine incgam included in the module IncgamFI.
             * Reference: A. Gil, J. Segura and NM Temme, Efficient and accurate algorithms for
             * the computation and inversion of the incomplete gamma function ratios. SIAM J Sci Comput. */
            static void Pser(double mu, double x, double y, out double p, out double q, out int ierro)
            {
                ierro = 0;
                int ierr = 0;
                double xy = x * y;
                int nnmax = Startingpser(mu, x, y);
                int n = 1 + nnmax;
                double lh0 = -x - y + n * Math.Log(x) + (n + mu) * Math.Log(y) - IncompleteGamma.Loggam(mu + n + 1.0) - IncompleteGamma.Loggam(n + 1.0);
                double p1;

                if (lh0 < Math.Log(Dwarf))
                {
                    double x1 = y;
                    double expo = Math.Exp(-x);
                    double facto;
                    double S = 0;
                    double t;
                    for (int k = Startingpser(mu, x, y) + 1; (k > 0) && (ierr == 0); k--)
                    {
                        double a = mu + k;
                        facto = Factor(x, k);
                        IncompleteGamma.Incgam(a, x1, out p1, out _, out ierr);
                        t = facto * p1;
                        S = S + t;
                        k = k - 1;
                    }

                    if (ierr == 0)
                    {
                        IncompleteGamma.Incgam(mu, x1, out p1, out _, out ierr);
                        S = S + p1;
                        p = S * expo;
                        q = 1 - p;
                    }
                    else
                    {
                        ierro = 1;
                        p = 0;
                        q = 1;
                    }
                }
                else
                {
                    double h0 = Math.Exp(lh0);

                    IncompleteGamma.Incgam(mu + n, y, out p, out q, out ierr);

                    if (ierr == 0)
                    {
                        p1 = p * Math.Exp(-x + n * Math.Log(x) - IncompleteGamma.Loggam(n + 1));
                        p = 0;
                        for (int k = n; k > 0; k--)
                        {
                            h0 = h0 * k * (mu + k) / xy;
                            p1 = k * p1 / x + h0;
                            p = p + p1;
                        }

                        q = 1 - p;
                    }
                    else
                    {
                        ierro = 1;
                        p = 0;
                        q = 1;
                    }
                }

            }

            static void Prec(double mu, double x, double y, out double p, out double q, out int ierro)
            {
                ierro = 0;
                int ierr = 0;
                double b = 1d;
                double nu = y - x + b * b + b * Math.Sqrt(2 * (x + y) + b * b);
                int n1 = (int)Math.Round(mu);
                int n2 = (int)Math.Round(nu) + 2;
                int n3 = n2 - n1;
                double mur = mu + n3;
                double xi = 2 * Math.Sqrt(x * y);
                double cmu = Math.Sqrt(y / x) * Fc(mur, xi);

                // Numerical quadrature
                MarcumPQtrap(mur + 1, x, y, out double p1, out q, ref ierr);
                MarcumPQtrap(mur + 0, x, y, out double p0, out q, ref ierr);

                if (ierr == 0)
                {
                    p = 0;  // initialize

                    for (int n = 0; n < n3; n++)
                    {
                        p = ((1 + cmu) * p0 - p1) / cmu;
                        p1 = p0;
                        p0 = p;
                        cmu = y / (mur - n - 1 + x * cmu);
                    }

                    q = 1 - p;
                }
                else
                {
                    p = 0;
                    q = 1;
                    ierro = 1;
                }

            }

            static void Qrec(double mu, double x, double y, out double p, out double q, out int ierro)
            {
                ierro = 0;
                int ierr = 0;
                double b = 1.0;
                double nu = y - x + b * (b - Math.Sqrt(2 * (x + y) + b * b));

                if (nu < 5)
                {
                    if (x < 200.0)
                    {
                        Qser(mu, x, y, out p, out q, out ierr);
                    }
                    else
                    {
                        Prec(mu, x, y, out p, out q, out ierr);
                    }
                }
                else
                {
                    int n1 = (int)Math.Round(mu);
                    int n2 = (int)Math.Round(nu) - 1;
                    int n3 = n1 - n2;
                    double mur = mu - n3;
                    double xi = 2 * Math.Sqrt(x * y);
                    double[] cmu = new double[301];
                    cmu[0] = Math.Sqrt(y / x) * Fc(mu, xi);

                    for (int n = 1; n <= n3; n++)
                    {
                        cmu[n] = y / (mu - n + x * cmu[n - 1]);
                    }

                    // Numerical quadrature
                    MarcumPQtrap(mur - 1, x, y, out p, out double q0, ref ierr);
                    MarcumPQtrap(mur + 0, x, y, out p, out double q1, ref ierr);

                    if (ierr == 0)
                    {
                        q = 0;  // initialize

                        for (int n = 1; n <= n3; n++)
                        {
                            double c = cmu[n3 + 1 - n];
                            q = (1.0 + c) * q1 - c * q0;
                            q0 = q1;
                            q1 = q;
                        }

                        p = 1 - q;
                    }
                    else
                    {
                        q = 0.0;
                        p = 1.0;
                        ierro = 1;
                    }
                }
            }

            static void PQasyxy(double mu, double x, double y, out double p, out double q, out int ierro)
            {
                ierro = 0;
                double s = y >= x ? 1.0 : -1.0;
                double delta = Epss / 100.0;
                double xi = 2 * Math.Sqrt(x * y);
                double sqxi = Math.Sqrt(xi);
                double rho = Math.Sqrt(y / x);
                double sigmaxi = ((y - x) * (y - x)) / (x + y + xi);
                double mulrho = mu * Math.Log(rho);

                if ((mulrho < Math.Log(Dwarf)) || (mulrho > Math.Log(Giant)))
                {
                    q = s == 1.0 ? 0.0 : 1.0;
                    p = s == 1.0 ? 1.0 : 0.0;
                    ierro = 1;
                }
                else
                {
                    double rhomu = Math.Exp(mulrho);
                    double er = Errorfunction(Math.Sqrt(sigmaxi), true, true);
                    double psi0 = 0.5 * rhomu * er / Math.Sqrt(rho);
                    double nu = 2 * mu - 1;
                    double rhom = nu * (rho - 1);
                    double rhop = 2 * (rho + 1);
                    double mu2 = 4 * mu * mu;
                    double c = s * rhomu / Math.Sqrt(8.0 * Math.PI);
                    double an = sqxi;
                    int n = 0;
                    int n0 = 100;

                    double[] bn = new double[101];
                    bn[0] = 1;

                    while ((Math.Abs(bn[n]) > delta) && (n < n0))
                    {
                        n++;
                        int tnm1 = 2 * n - 1;
                        an = (mu2 - tnm1 * tnm1) * an / (8 * n * xi);
                        bn[n] = an * (rhom - n * rhop) / (rho * (nu + 2 * n));
                    }

                    n0 = n;
                    int nrec = Math.Min(n0, (int)Math.Round(sigmaxi) + 1);

                    double[] phin = new double[101];
                    phin[nrec] = Math.Exp((nrec - 0.5) * Math.Log(sigmaxi)) * Ignega(nrec, sigmaxi);

                    for (int m = nrec + 1; m <= n0; m++)
                    {
                        phin[m] = (-sigmaxi * phin[m - 1] + 1) / (m - 0.5);
                    }

                    for (int m = nrec - 1; m >= 1; m--)
                    {
                        phin[m] = (1 - (m + 0.5) * phin[m + 1]) / sigmaxi;
                    }

                    double pq = psi0;

                    for (int m = 1; m <= n0; m++)
                    {
                        c = -c;
                        double psi = c * bn[m] * phin[m];
                        pq = pq + psi;
                    }

                    pq = pq * Math.Exp(-sigmaxi);

                    if (s == 1.0)
                    {
                        q = pq;
                        p = 1.0 - q;
                    }
                    else
                    {
                        p = pq;
                        q = 1.0 - p;
                    }
                }
            }

            static void PQasymu(double mu0, double x0, double y0, out double p, out double q, out int ierro)
            {
                ierro = 0;
                double mu = mu0 - 1.0;
                double x = x0 / mu;
                double y = y0 / mu;
                double zeta = Zetaxy(x, y);
                int a = zeta < 0.0 ? 1 : -1;
                double u = 1.0 / Math.Sqrt(2.0 * x + 1.0);
                double[,] fjk = new double[17, 17];
                Fjkproc16(u, fjk);

                zeta = a * zeta;
                double r = zeta * Math.Sqrt(mu / 2.0);

                double[] psik = new double[18];
                int psikofst = 1;  // offset psik start index
                psik[0 + psikofst] = Math.Sqrt(Math.PI / (2.0 * mu)) * Errorfunction(-r, true, false);
                double s = psik[0 + psikofst];
                double lexpor = -mu * 0.5 * zeta * zeta;

                if ((lexpor < Math.Log(Dwarf)) || (lexpor > Math.Log(Giant)))
                {
                    if (a == 1)
                    {
                        q = 0.0;
                        p = 1.0;
                    }
                    else
                    {
                        p = 0.0;
                        q = 1.0;
                    }

                    ierro = 1;
                }
                else
                {
                    r = Math.Exp(lexpor);
                    psik[-1 + psikofst] = 0.0;

                    double[] muk = new double[17];
                    muk[0] = 1.0;
                    double bk = s;
                    int k = 1;
                    double zetaj = 1.0;

                    while ((Math.Abs(bk / s) > 1e-30) && (k <= 16))
                    {
                        muk[k] = mu * muk[k - 1];
                        psik[k + psikofst] = ((k - 1) * psik[k - 2 + psikofst] + r * zetaj) / mu;
                        bk = 0;
                        int b = 1;
                        zetaj = -zeta * zetaj;

                        for (int j = 0; j <= k; j++)
                        {
                            int t = (a == -1) && (b == -1) ? -1 : 1;

                            b = -b;
                            bk = bk + t * fjk[j, k - j] * psik[j + psikofst] / muk[k - j];
                        }

                        s = s + bk;
                        k = k + 1;
                    }

                    r = Math.Sqrt(mu / Constants.Pi2) * s;

                    if (a == 1)
                    {
                        q = r;
                        p = 1 - q;
                    }
                    else
                    {
                        p = r;
                        q = 1 - p;
                    }
                }
            }

            static void MarcumPQtrap(double mu, double x, double y, out double p, out double q, ref int ierr)
            {
                double xs = x / mu;
                double ys = y / mu;
                double xis2 = 4 * xs * ys;
                double wxis = Math.Sqrt(1.0 + xis2);
                double a = 0.0;
                double b = 3.0;
                double epstrap = 1.0e-13;
                double pq = Trap(a, b, epstrap, xis2, mu, wxis, ys);
                double zeta = Zetaxy(xs, ys);

                if ((-mu * 0.5 * zeta * zeta) < Math.Log(Dwarf))
                {
                    if (y > x + mu)
                    {
                        p = 1.0;
                        q = 0.0;
                    }
                    else
                    {
                        p = 0.0;
                        q = 1.0;
                    }

                    ierr = 1;
                }
                else
                {
                    pq = pq * Math.Exp(-mu * 0.5 * zeta * zeta) / Math.PI;

                    if (zeta < 0)
                    {
                        q = pq;
                        p = 1 - q;
                    }
                    else
                    {
                        p = -pq;
                        q = 1 - p;
                    }
                }
            }

            #endregion Local Functions

            /// <summary>
            /// computation of the incomplete gamma function ratios
            /// </summary>
            static class IncompleteGamma
            {
                /// <summary>
                /// Calculation of the incomplete gamma functions ratios P(a,x) and Q(a,x).
                /// </summary>
                /// <param name="a">argument of the functions</param>
                /// <param name="x">argument of the functions</param>
                /// <param name="p">function P(a,x)</param>
                /// <param name="q">function Q(a,x)</param>
                /// <param name="ierr">error flag</param>
                internal static void Incgam(double a, double x, out double p, out double q, out int ierr)
                {
                    ierr = 0;
                    p = 0;
                    q = 0;

                    double lnx = x < Dwarf ? Math.Log(Dwarf) : Math.Log(x);
                    double dp;

                    if (a > Alfa(x))
                    {
                        dp = Dompart(a, x, false);

                        if (dp < 0)
                        {
                            ierr = 1;
                            p = 0; q = 0;
                        }
                        else
                        {
                            if ((x < 0.3 * a) || (a < 12))
                            {
                                p = Ptaylor(a, x, dp);
                            }
                            else
                            {
                                p = PQasymp(a, x, dp, true);
                            }

                            q = 1.0 - p;
                        }
                    }
                    else
                    {
                        if (a < -Dwarf / lnx)
                        {
                            q = 0.0;
                        }
                        else
                        {
                            if (x < 1.0)
                            {
                                dp = Dompart(a, x, true);

                                if (dp < 0)
                                {
                                    ierr = 1;
                                    q = 0; p = 0;
                                }
                                else
                                {
                                    q = Qtaylor(a, x, dp);
                                    p = 1.0 - q;
                                }
                            }
                            else
                            {
                                dp = Dompart(a, x, false);

                                if (dp < 0)
                                {
                                    ierr = 1;
                                    p = 0; q = 0;
                                }
                                else
                                {
                                    if ((x > 1.5 * a) || (a < 12.0))
                                    {
                                        q = Qfraction(a, x, dp);
                                    }
                                    else
                                    {
                                        q = PQasymp(a, x, dp, false);
                                        if (dp == 0.0)
                                        {
                                            q = 0.0;
                                        }
                                    }

                                    p = 1.0 - q;
                                }
                            }
                        }
                    }
                }

                /// <summary>
                /// Computes xr in the equations P(a,xr)=p and Q(a,xr)=q with a as a given positive parameter. In most cases, we invert the equation with min(p,q)
                /// </summary>
                /// <param name="a">argument of the functions</param>
                /// <param name="p">function P(a,x)</param>
                /// <param name="q">function Q(a,x)</param>
                /// <param name="xr">soluction of the equations P(a,xr)=p and Q(a,xr)=q with a as a given positive parameter.</param>
                /// <param name="ierr">error flag</param>
                internal static void Invincgam(double a, double p, double q, out double xr, out int ierr)
                {
                    ierr = 0;
                    bool pcase;
                    double porq, s, x0, a2, a3, eta = 0;
                    int m;

                    if (p < 0.5)
                    {
                        pcase = true;
                        porq = p;
                        s = -1;
                    }
                    else
                    {
                        pcase = false;
                        porq = q;
                        s = 1;
                    }

                    double logr = (1.0 / a) * (Math.Log(p) + Loggam(a + 1.0));

                    double[] ck = new double[6];

                    if (logr < Math.Log(0.2 * (1 + a)))
                    {
                        double r = Math.Exp(logr);
                        m = 0;
                        a2 = a * a;
                        a3 = a2 * a;
                        double a4 = a3 * a;
                        double ap1 = a + 1.0;
                        double ap12 = (a + 1.0) * ap1;
                        double ap13 = (a + 1.0) * ap12;
                        double ap14 = ap12 * ap12;
                        double ap2 = a + 2;
                        double ap22 = ap2 * ap2;
                        ck[1] = 1.0;
                        ck[2] = 1.0 / (1.0 + a);
                        ck[3] = 0.5 * (3 * a + 5) / (ap12 * (a + 2));
                        ck[4] = (1.0 / 3.0) * (31 + 8 * a2 + 33 * a) / (ap13 * ap2 * (a + 3));
                        ck[5] = (1.0 / 24.0) * (2888 + 1179 * a3 + 125 * a4 + 3971 * a2 + 5661 * a) / (ap14 * ap22 * (a + 3) * (a + 4));
                        x0 = r * (1 + r * (ck[2] + r * (ck[3] + r * (ck[4] + r * ck[5]))));
                    }
                    else if ((q < Math.Min(0.02, Math.Exp(-1.5 * a) / Gamma(a))) && (a < 10.0))
                    {
                        m = 0;
                        double b = 1.0 - a;
                        double b2 = b * b;
                        double b3 = b2 * b;
                        eta = Math.Sqrt(-2 / a * Math.Log(q * Gamstar(a) * Constants.Sqrt2Pi / Math.Sqrt(a)));
                        x0 = a * Lambdaeta(eta);
                        double L = Math.Log(x0);
                        if ((a > 0.12) || (x0 > 5))
                        {
                            double L2 = L * L;
                            double L3 = L2 * L;
                            double L4 = L3 * L;
                            double r = 1.0 / x0;
                            ck[1] = L - 1;
                            ck[2] = (3 * b - 2 * b * L + L2 - 2 * L + 2) / 2.0;
                            ck[3] = (24 * b * L - 11 * b2 - 24 * b - 6 * L2 + 12 * L - 12 - 9 * b * L2 + 6 * b2 * L + 2 * L3) / 6.0;
                            ck[4] = (-12 * b3 * L + 84 * b * L2 - 114 * b2 * L + 72 + 36 * L2 + 3 * L4 - 72 * L + 162 * b - 168 * b * L - 12 * L3 + 25 * b3 - 22 * b * L3 + 36 * b2 * L2 + 120 * b2) / 12.0;
                            x0 = x0 - L + b * r * (ck[1] + r * (ck[2] + r * (ck[3] + r * ck[4])));
                        }
                        else
                        {
                            double r = 1.0 / x0;
                            ck[1] = L - 1;
                            x0 = x0 - L + b * r * ck[1];
                        }
                    }
                    else if (Math.Abs(porq - 0.5) < 1.0e-5)
                    {
                        m = 0;
                        x0 = a - 1.0 / 3.0 + (8.0 / 405.0 + 184.0 / 25515.0 / a) / a;
                    }
                    else if (Math.Abs(a - 1) < 1.0e-4)
                    {
                        m = 0;
                        if (pcase)
                        {
                            x0 = -Math.Log(1.0 - p);
                        }
                        else
                        {
                            x0 = -Math.Log(q);
                        }
                    }
                    else if (a < 1.0)
                    {
                        m = 0;
                        if (pcase)
                        {
                            x0 = Math.Exp((1.0 / a) * (Math.Log(porq) + Loggam(a + 1.0)));
                        }
                        else
                        {
                            x0 = Math.Exp((1.0 / a) * (Math.Log(1.0 - porq) + Loggam(a + 1.0)));
                        }
                    }
                    else
                    {
                        m = 1;
                        double r = Inverfc(2 * porq);
                        eta = s * r / Math.Sqrt(a * 0.5);
                        eta = eta + (Eps1(eta) + (Eps2(eta) + Eps3(eta) / a) / a) / a;
                        x0 = a * Lambdaeta(eta);
                    }

                    double t = 1;
                    double x = x0;
                    int n = 1;
                    a2 = a * a;

                    // Implementation of the high order Newton-like method
                    while ((t > 1.0e-15) && (n < 15))
                    {
                        x = x0;
                        double x2 = x * x;
                        if (m == 0)
                        {
                            double dlnr = (1.0 - a) * Math.Log(x) + x + Loggam(a);
                            if (dlnr > Math.Log(Giant))
                            {
                                n = 20;
                                ierr = -1;
                            }
                            else
                            {
                                double r = Math.Exp(dlnr);
                                Incgam(a, x, out double px, out double qx, out _);

                                ck[1] = pcase ? -r * (px - p) : r * (qx - q);

                                ck[2] = (x - a + 1.0) / (2.0 * x);
                                ck[3] = (2 * x2 - 4 * x * a + 4 * x + 2 * a2 - 3 * a + 1) / (6 * x2);
                                r = ck[1];
                                if (a > 0.1)
                                {
                                    x0 = x + r * (1 + r * (ck[2] + r * ck[3]));
                                }
                                else
                                {
                                    if (a > 0.05)
                                    {
                                        x0 = x + r * (1 + r * (ck[2]));
                                    }
                                    else
                                    {
                                        x0 = x + r;
                                    }
                                }
                            }
                        }
                        else
                        {
                            double y = eta;
                            double fp = -Math.Sqrt(a / Constants.Pi2) * Math.Exp(-0.5 * a * y * y) / (Gamstar(a));
                            double r = -(1.0 / fp) * x;
                            Incgam(a, x, out double px, out double qx, out _);
                            ck[1] = pcase ? -r * (px - p) : r * (qx - q);
                            ck[2] = (x - a + 1.0) / (2.0 * x);
                            ck[3] = (2 * x2 - 4 * x * a + 4 * x + 2 * a2 - 3 * a + 1) / (6 * x2);
                            r = ck[1];

                            if (a > 0.1)
                            {
                                x0 = x + r * (1 + r * (ck[2] + r * ck[3]));
                            }
                            else
                            {
                                if (a > 0.05)
                                {
                                    x0 = x + r * (1 + r * (ck[2]));
                                }
                                else
                                {
                                    x0 = x + r;
                                }
                            }
                        }

                        t = Math.Abs(x / x0 - 1.0);
                        n = n + 1;
                        x = x0;
                    }
                    if (n == 15)
                    {
                        ierr = -2;
                    }

                    xr = x;
                }

                // to compute hyperbolic function sinh (x)
                static double Sinh(double x, double eps)
                {
                    double y;

                    double ax = Math.Abs(x);

                    if (x == 0.0)
                    {
                        y = 0;
                    }
                    else if (ax < 0.12)
                    {
                        double e = eps / 10.0;
                        double x2 = x * x;
                        y = 1;
                        double t = 1;
                        int u = 0;
                        int k = 1;

                        while (t > e)
                        {
                            u = u + 8 * k - 2;
                            k = k + 1;
                            t = t * x2 / u;
                            y = y + t;
                        }

                        y = x * y;
                    }
                    else if (ax < 0.36)
                    {
                        double t = Sinh(x / 3.0, eps);
                        y = t * (3 + 4 * t * t);
                    }
                    else
                    {
                        double t = Math.Exp(x);
                        y = (t - 1.0 / t) / 2.0;
                    }

                    return y;
                }

                static double Exmin1(double x, double eps)
                {
                    double y;

                    if (x == 0.0)
                    {
                        y = 1.0;
                    }
                    else if ((x < -0.69) || (x > 0.4))
                    {
                        y = (Math.Exp(x) - 1.0) / x;
                    }
                    else
                    {
                        double t = x / 2.0;
                        y = Math.Exp(t) * Sinh(t, eps) / t;
                    }
                    return y;
                }

                static double Exmin1minx(double x, double eps)
                {
                    double y;

                    if (x == 0.0)
                    {
                        y = 1.0;
                    }
                    else if (Math.Abs(x) > 0.9)
                    {
                        y = (Math.Exp(x) - 1 - x) / (x * x / 2.0);
                    }
                    else
                    {
                        double t = Sinh(x / 2.0, eps);
                        double t2 = t * t;
                        y = (2 * t2 + (2 * t * Math.Sqrt(1.0 + t2) - x)) / (x * x / 2.0);
                    }

                    return y;
                }

                // x >-1; computes ln(1+x) with good relative precision when |x| is small
                static double Logoneplusx(double x)
                {
                    double y0 = Math.Log(1.0 + x);

                    if ((-0.2928 < x) && (x < 0.4142))
                    {
                        double s = y0 * Exmin1(y0, MachTol);
                        double r = (s - x) / (s + 1.0);
                        y0 = y0 - r * (6 - r) / (6 - 4 * r);
                    }

                    return y0;
                }

                static double Lnec(double x)
                {
                    double z = Logoneplusx(x);
                    double y0 = z - x;
                    double e2 = Exmin1minx(z, MachTol);
                    double s = e2 * z * z / 2;
                    double r = (s + y0) / (s + 1 + z);
                    double ln1 = y0 - r * (6 - r) / (6 - 4 * r);

                    return ln1;
                }

                static double Alfa(double x)
                {
                    double alfa;
                    double lnx = Math.Log(x);

                    if (x > 0.25)
                    {
                        alfa = x + 0.25;
                    }
                    else if (x >= Dwarf)
                    {
                        alfa = -0.6931 / lnx;
                    }
                    else
                    {
                        alfa = -0.6931 / Math.Log(Dwarf);
                    }

                    return alfa;
                }

                /// <summary>
                /// dompart is approx. of  x^a * exp(-x) / gamma(a+1)
                /// </summary>
                /// <param name="a">argument of the functions</param>
                /// <param name="x">argument of the functions</param>
                /// <param name="qt">argument of the functions</param>
                /// <returns></returns>
                internal static double Dompart(double a, double x, bool qt)
                {
                    double dompart;
                    double lnx = Math.Log(x);
                    double r;

                    if (a <= 1.0)
                    {
                        r = -x + a * lnx;
                    }
                    else
                    {
                        if (x == a)
                        {
                            r = 0;
                        }
                        else
                        {
                            double la = x / a;
                            r = a * (1.0 - la + Math.Log(la));
                        }

                        r = r - 0.5 * Math.Log(6.2832 * a);
                    }

                    double dp = r < ExpLow ? 0.0 : Math.Exp(r);

                    if (qt)
                    {
                        dompart = dp;
                    }
                    else
                    {
                        if ((a < 3.0) || (x < 0.2))
                        {
                            dompart = Math.Exp(a * lnx - x) / Gamma(a + 1.0);
                        }
                        else
                        {
                            double mu = (x - a) / a;
                            double c = Lnec(mu);

                            if ((a * c) > Math.Log(Giant))
                            {
                                dompart = -100;
                            }
                            else if ((a * c) < Math.Log(Dwarf))
                            {
                                dompart = 0.0;
                            }
                            else
                            {
                                dompart = Math.Exp(a * c) / (Math.Sqrt(a * 2 * Math.PI) * Gamstar(a));
                            }
                        }
                    }

                    return dompart;
                }

                // a[0]/2+a[1]T1(x)+...a[n]Tn(x); series of Chebychev polynomials
                static double Chepolsum(int n, double x, double[] a)
                {
                    double chepolsum;

                    if (n == 0)
                    {
                        chepolsum = a[0] / 2;
                    }
                    else if (n == 1)
                    {
                        chepolsum = a[0] / 2 + a[1] + x;
                    }
                    else
                    {
                        double tx = x + x;
                        double r = a[n];
                        double h = a[n - 1] + r * tx;

                        for (int k = n - 2; k >= 1; k--)
                        {
                            double s = r;
                            r = h;
                            h = a[k] + r * tx - s;
                        }

                        chepolsum = a[0] / 2 - r + h * x;
                    }

                    return chepolsum;
                }

                // function g in ln(Gamma(1+x))=x*(1-x)*g(x), 0<=x<=1
                static double Auxloggam(double x)
                {
                    double g;

                    if (x < -1)
                    {
                        g = Giant;
                    }
                    else if (Math.Abs(x) <= Dwarf)
                    {
                        g = -Constants.EulerMascheroni;
                    }
                    else if (Math.Abs(x - 1) <= MachTol)
                    {
                        g = Constants.EulerMascheroni - 1;
                    }
                    else if (x < 0)
                    {
                        g = -(x * (1 + x) * Auxloggam(x + 1) + Logoneplusx(x)) / (x * (1 - x));
                    }
                    else if (x < 1)
                    {
                        double[] ak = {
                            -0.98283078605877425496,
                            0.7611416167043584304e-1,
                            -0.843232496593277796e-2,
                            0.107949372632860815e-2,
                            -0.14900748003692965e-3,
                            0.2151239988855679e-4,
                            -0.319793298608622e-5,
                            0.48516930121399e-6,
                            -0.7471487821163e-7,
                            0.1163829670017e-7,
                            -0.182940043712e-8,
                            0.28969180607e-9,
                            -0.4615701406e-10,
                            0.739281023e-11,
                            -0.118942800e-11,
                            0.19212069e-12,
                            -0.3113976e-13,
                            0.506284e-14,
                            -0.82542e-15,
                            0.13491e-15,
                            -0.2210e-16,
                            0.363e-17,
                            -0.60e-18,
                            0.98e-19,
                            -0.2e-19,
                            0.3e-20
                        };
                        double t = 2 * x - 1;

                        g = Chepolsum(25, t, ak);
                    }
                    else if (x < 1.5)
                    {
                        g = (Logoneplusx(x - 1) + (x - 1) * (2 - x) * Auxloggam(x - 1)) / (x * (1 - x));
                    }
                    else
                    {
                        g = (Math.Log(x) + (x - 1) * (2 - x) * Auxloggam(x - 1)) / (x * (1 - x));
                    }

                    return g;
                }

                /// <summary>
                /// Computation of ln(gamma(x)), x>0
                /// </summary>
                /// <param name="x">argument of the functions</param>
                /// <returns></returns>
                internal static double Loggam(double x)
                {
                    //return SpecialFunctions.GammaLn(x);

                    double loggam;

                    if (x >= 3.0)
                    {
                        loggam = (x - 0.5) * Math.Log(x) - x + LnSqrt2Pi + Stirling(x);
                    }
                    else if (x >= 2.0)
                    {
                        loggam = (x - 2) * (3 - x) * Auxloggam(x - 2.0) + Logoneplusx(x - 2.0);
                    }
                    else if (x >= 1.0)
                    {
                        loggam = (x - 1.0) * (2.0 - x) * Auxloggam(x - 1.0);
                    }
                    else if (x > 0.5)
                    {
                        loggam = x * (1.0 - x) * Auxloggam(x) - Logoneplusx(x - 1.0);
                    }
                    else if (x > 0.0)
                    {
                        loggam = x * (1 - x) * Auxloggam(x) - Math.Log(x);
                    }
                    else
                    {
                        loggam = Giant;
                    }

                    return loggam;
                }

                // function g in 1/gamma(x+1)=1+x*(x-1)*g(x), -1<=x<=1
                static double Auxgam(double x)
                {
                    double auxgam;

                    if (x < 0)
                    {
                        auxgam = -(1 + (1 + x) * (1 + x) * Auxgam(1 + x)) / (1 - x);
                    }
                    else
                    {
                        double[] dr = {
                            -1.013609258009865776949,
                            0.784903531024782283535e-1,
                            0.67588668743258315530e-2,
                            -0.12790434869623468120e-2,
                            0.462939838642739585e-4,
                            0.43381681744740352e-5,
                            -0.5326872422618006e-6,
                            0.172233457410539e-7,
                            0.8300542107118e-9,
                            -0.10553994239968e-9,
                            0.39415842851e-11,
                            0.362068537e-13,
                            -0.107440229e-13,
                            0.5000413e-15,
                            -0.62452e-17,
                            -0.5185e-18,
                            0.347e-19,
                            -0.9e-21,
                        };
                        double t = 2 * x - 1;

                        auxgam = Chepolsum(17, t, dr);
                    }

                    return auxgam;
                }

                // ln(gamma(1+x)), -1<=x<=1
                static double Lngam1(double x)
                {
                    return -Logoneplusx(x * (x - 1) * Auxgam(x));
                }

                // Stirling series, function corresponding with asymptotic series for Math.Log(gamma(x)) that is:  1/(12x)-1/(360x**3)...; x>= 3
                static double Stirling(double x)
                {
                    double stirling;

                    if (x < Dwarf)
                    {
                        stirling = Giant;
                    }
                    else if (x < 1.0)
                    {
                        stirling = Lngam1(x) - (x + 0.5) * Math.Log(x) + x - LnSqrt2Pi;
                    }
                    else if (x < 2.0)
                    {
                        stirling = Lngam1(x - 1) - (x - 0.5) * Math.Log(x) + x - LnSqrt2Pi;
                    }
                    else if (x < 3.0)
                    {
                        stirling = Lngam1(x - 2) - (x - 0.5) * Math.Log(x) + x - LnSqrt2Pi + Math.Log(x - 1);
                    }
                    else if (x < 12.0)
                    {
                        double[] a = {
                            1.996379051590076518221,
                            -0.17971032528832887213e-2,
                            0.131292857963846713e-4,
                            -0.2340875228178749e-6,
                            0.72291210671127e-8,
                            -0.3280997607821e-9,
                            0.198750709010e-10,
                            -0.15092141830e-11,
                            0.1375340084e-12,
                            -0.145728923e-13,
                            0.17532367e-14,
                            -0.2351465e-15,
                            0.346551e-16,
                            -0.55471e-17,
                            0.9548e-18,
                            -0.1748e-18,
                            0.332e-19,
                            -0.58e-20
                        };
                        double z = 18.0 / (x * x) - 1.0;

                        stirling = Chepolsum(17, z, a) / (12.0 * x);
                    }
                    else
                    {
                        double z = 1.0 / (x * x);

                        if (x < 1000)
                        {
                            double[] c = {
                                0.25721014990011306473e-1,
                                0.82475966166999631057e-1,
                                -0.25328157302663562668e-2,
                                0.60992926669463371e-3,
                                -0.33543297638406e-3,
                                0.250505279903e-3,
                                0.30865217988013567769,
                            };

                            stirling = ((((((c[5] * z + c[4]) * z + c[3]) * z + c[2]) * z + c[1]) * z + c[0]) / (c[6] + z) / x);
                        }
                        else
                        {
                            stirling = (((-z / 1680.0 + 1.0 / 1260.0) * z - 1.0 / 360.0) * z + 1.0 / 12.0) / x;
                        }
                    }

                    return stirling;
                }

                // Euler gamma function Gamma(x), x real
                static double Gamma(double x)
                {
                    double gam;
                    int k = (int)Math.Round(x);
                    int k1 = k - 1;
                    double dw = k == 0 ? Dwarf : MachTol;

                    if ((k <= 0) && (Math.Abs(k - x) <= dw))
                    {
                        if (k % 2 > 0)
                        {
                            // k is odd
                            gam = k - x < 0 ? -Giant : Giant;
                        }
                        else
                        {
                            // k is even
                            gam = x - k < 0 ? -Giant : Giant;
                        }
                    }
                    else if (x < 0.45)
                    {
                        gam = Math.PI / (Math.Sin(Math.PI * x) * Gamma(1 - x));
                    }
                    else if ((Math.Abs(k - x) < dw) && (x < 21.0))
                    {
                        gam = 1;

                        for (int n = 2; n <= k1; n++)
                        {
                            gam = gam * n;
                        }
                    }
                    else if ((Math.Abs(k - x - 0.5) < dw) && (x < 21.0))
                    {
                        gam = Constants.SqrtPi;

                        for (int n = 1; n <= k1; n++)
                        {
                            gam = gam * (n - 0.5);
                        }
                    }
                    else if (x < 3.0)
                    {
                        if (k > x)
                        {
                            k = k1;
                        }

                        k1 = 3 - k;

                        double z = k1 + x;
                        gam = Gamma(z);

                        for (int n = 1; n <= k1; n++)
                        {
                            gam = gam / (z - n);
                        }
                    }
                    else
                    {
                        gam = Constants.Sqrt2Pi * Math.Exp(-x + (x - 0.5) * Math.Log(x) + Stirling(x));
                    }

                    return gam;
                }

                // gamstar(x)=exp(stirling(x)), x>0; or
                // gamma(x)/(exp(-x+(x-0.5)*ln(x))/sqrt(2pi)
                static double Gamstar(double x)
                {
                    double gamstar;

                    if (x >= 3.0)
                    {
                        gamstar = Math.Exp(Stirling(x));
                    }
                    else if (x > 0.0)
                    {
                        gamstar = Gamma(x) / (Math.Exp(-x + (x - 0.5) * Math.Log(x)) * Constants.Sqrt2Pi);
                    }
                    else
                    {
                        gamstar = Giant;
                    }

                    return gamstar;
                }

                // coefficients are from Cody (1969), Math. Comp., 23, 631-637
                static double Errorfunction(double x, bool erfcc, bool expo)
                {
                    double y;

                    if (erfcc)
                    {
                        if (x < -6.5)
                        {
                            y = 2.0;
                        }
                        else if (x < 0)
                        {
                            y = 2.0 - Errorfunction(-x, true, false);
                        }
                        else if (x == 0.0)
                        {
                            y = 1.0;
                        }
                        else if (x < 0.5)
                        {
                            y = expo ? Math.Exp(x * x) : 1.0;
                            y *= 1.0 - Errorfunction(x, false, false);
                        }
                        else if (x < 4.0)
                        {
                            y = expo ? 1 : Math.Exp(-x * x);

                            double[] r = {
                                1.230339354797997253e3,
                                2.051078377826071465e3,
                                1.712047612634070583e3,
                                8.819522212417690904e2,
                                2.986351381974001311e2,
                                6.611919063714162948e1,
                                8.883149794388375941,
                                5.641884969886700892e-1,
                                2.153115354744038463e-8
                            };
                            double[] s = {
                                1.230339354803749420e3,
                                3.439367674143721637e3,
                                4.362619090143247158e3,
                                3.290799235733459627e3,
                                1.621389574566690189e3,
                                5.371811018620098575e2,
                                1.176939508913124993e2,
                                1.574492611070983473e1
                            };

                            y *= Fractio(x, 8, r, s);
                        }
                        else
                        {
                            double z = x * x;
                            y = expo ? 1 : Math.Exp(-z);
                            z = 1 / z;

                            double[] r = {
                                6.587491615298378032e-4,
                                1.608378514874227663e-2,
                                1.257817261112292462e-1,
                                3.603448999498044394e-1,
                                3.053266349612323440e-1,
                                1.631538713730209785e-2
                            };
                            double[] s = {
                                2.335204976268691854e-3,
                                6.051834131244131912e-2,
                                5.279051029514284122e-1,
                                1.872952849923460472,
                                2.568520192289822421
                            };

                            y *= (Constants.InvSqrtPi - z * Fractio(z, 5, r, s)) / x;
                        }
                    }
                    else
                    {
                        if (x == 0.0)
                        {
                            y = 0;
                        }
                        else if (Math.Abs(x) > 6.5)
                        {
                            y = x / Math.Abs(x);
                        }
                        else if (x > 0.5)
                        {
                            y = 1.0 - Errorfunction(x, true, false);
                        }
                        else if (x < -0.5)
                        {
                            y = Errorfunction(-x, true, false) - 1.0;
                        }
                        else
                        {
                            double[] r = {
                                3.209377589138469473e3,
                                3.774852376853020208e2,
                                1.138641541510501556e2,
                                3.161123743870565597e0,
                                1.857777061846031527e-1
                            };
                            double[] s = {
                                2.844236833439170622e3,
                                1.282616526077372276e3,
                                2.440246379344441733e2,
                                2.360129095234412093e1
                            };

                            double z = x * x;
                            y = x * Fractio(z, 4, r, s);
                        }
                    }

                    return y;
                }

                static double Fractio(double x, int n, double[] r, double[] s)
                {
                    double a = r[n];
                    double b = 1;

                    for (int k = n - 1; k >= 0; k--)
                    {
                        a = a * x + r[k];
                        b = b * x + s[k];
                    }

                    return a / b;
                }

                static double PQasymp(double a, double x, double dp, bool p)
                {
                    double pqasymp;

                    if (dp == 0.0)
                    {
                        pqasymp = p ? 0.0 : 1.0;
                    }
                    else
                    {

                        double s = p ? -1 : 1;

                        double mu = (x - a) / a;
                        double y = -Lnec(mu);
                        double eta = y < 0.0 ? 0.0 : Math.Sqrt(2.0 * y);
                        y = y * a;
                        double v = Math.Sqrt(Math.Abs(y));

                        if (mu < 0.0)
                        {
                            eta = -eta;
                            v = -v;
                        }

                        double u = 0.5 * Errorfunction(s * v, true, false);
                        v = s * Math.Exp(-y) * Saeta(a, eta) / Math.Sqrt(2.0 * Math.PI * a);

                        pqasymp = u + v;
                    }

                    return pqasymp;
                }

                static double Saeta(double a, double eta)
                {
                    double eps = Epss;

                    double[] fm = {
                        1.0,
                        -1.0 / 3.0,
                        1.0 / 12.0,
                        -2.0 / 135.0,
                        1.0 / 864.0,
                        1.0 / 2835.0,
                        -139.0 / 777600.0,
                        1.0 / 25515.0,
                        -571.0 / 261273600.0,
                        -281.0 / 151559100.0,
                        8.29671134095308601e-7,
                        -1.76659527368260793e-7,
                        6.70785354340149857e-9,
                        1.02618097842403080e-8,
                        -4.38203601845335319e-9,
                        9.14769958223679023e-10,
                        -2.55141939949462497e-11,
                        -5.83077213255042507e-11,
                        2.43619480206674162e-11,
                        -5.02766928011417559e-12,
                        1.10043920319561347e-13,
                        3.37176326240098538e-13,
                        -1.39238872241816207e-13,
                        2.85348938070474432e-14,
                        -5.13911183424257258e-16,
                        -1.97522882943494428e-15,
                        8.09952115670456133e-16
                    };
                    double[] bm = new double[26];

                    bm[25] = fm[26];
                    bm[24] = fm[25];

                    for (int m = 24; m >= 1; m--)
                    {
                        bm[m - 1] = fm[m] + (m + 1) * bm[m + 1] / a;
                    }

                    double s = bm[0];
                    double t = s;
                    double y = eta;
                    int i = 1;

                    while ((Math.Abs(t / s) > eps) && (i < 25))
                    {
                        t = bm[i] * y;
                        s = s + t;
                        i++;
                        y = y * eta;
                    }

                    double saeta = s / (1 + bm[1] / a);

                    return saeta;
                }

                static double Qfraction(double a, double x, double dp)
                {
                    double eps = Epss;
                    double p, q;

                    if (dp == 0.0)
                    {
                        q = 0.0;
                    }
                    else
                    {
                        p = 0.0;
                        q = (x - 1.0 - a) * (x + 1.0 - a);
                        double r = 4 * (x + 1.0 - a);
                        double s = 1.0 - a;
                        double ro = 0.0;
                        double t = 1.0;
                        double g = 1.0;

                        while (Math.Abs(t / g) >= eps)
                        {
                            p = p + s;
                            q = q + r;
                            r = r + 8;
                            s = s + 2;
                            double tau = p * (1.0 + ro);
                            ro = tau / (q - tau);
                            t = ro * t;
                            g = g + t;
                        }

                        q = (a / (x + 1.0 - a)) * g * dp;
                    }

                    return q;
                }

                static double Qtaylor(double a, double x, double dp)
                {
                    double eps = Epss;
                    double lnx = Math.Log(x);
                    double q;

                    if (dp == 0.0)
                    {
                        q = 0.0;
                    }
                    else
                    {
                        double r = a * lnx;
                        q = r * Exmin1(r, eps); // q = x ^ a - 1
                        double s = a * (1.0 - a) * Auxgam(a); // s = 1 - 1 / Gamma(1 + a)
                        q *= 1 - s;
                        double u = s - q; // u = 1 - x ^ a / Gamma(1 + a)
                        double p = a * x;
                        q = a + 1;
                        r = a + 3;
                        double t = 1.0;
                        double v = 1.0;

                        while (Math.Abs(t / v) > eps)
                        {
                            p += x;
                            q += r;
                            r += 2;
                            t = -p * t / q;
                            v += t;
                        }

                        v = a * (1 - s) * Math.Exp((a + 1.0) * lnx) * v / (a + 1.0);
                        q = u + v;
                    }
                    return q;
                }

                static double Ptaylor(double a, double x, double dp)
                {
                    double eps = Epss;
                    double p;

                    if (dp == 0.0)
                    {
                        p = 0.0;
                    }
                    else
                    {
                        p = 1.0;
                        double c = 1.0;
                        double r = a;

                        while ((c / p) > eps)
                        {
                            r += 1;
                            c *= x / r;
                            p += c;
                        }

                        p *= dp;
                    }

                    return p;
                }

                static double Eps1(double eta)
                {
                    double eps1;

                    if (Math.Abs(eta) < 1.0)
                    {
                        double[] ak = {
                            -3.333333333438e-1,
                            -2.070740359969e-1,
                            -5.041806657154e-2,
                            -4.923635739372e-3,
                            -4.293658292782e-5
                        };
                        double[] bk = {
                            1.000000000000e+0,
                            7.045554412463e-1,
                            2.118190062224e-1,
                            3.048648397436e-2,
                            1.605037988091e-3
                        };
                        eps1 = Ratfun(eta, ak, bk);
                    }
                    else
                    {
                        double la = Lambdaeta(eta);
                        eps1 = Math.Log(eta / (la - 1.0)) / eta;
                    }

                    return eps1;
                }

                static double Eps2(double eta)
                {
                    double eps2;
                    double[] ak = new double[5];
                    double[] bk = new double[5];

                    if (eta < -5.0)
                    {
                        double x = eta * eta;
                        double lnmeta = Math.Log(-eta);
                        eps2 = (12.0 - x - 6.0 * (lnmeta * lnmeta)) / (12.0 * x * eta);
                    }
                    else if (eta < -2.0)
                    {
                        ak[0] = -1.72847633523e-2; bk[0] = 1.00000000000e+0;
                        ak[1] = -1.59372646475e-2; bk[1] = 7.64050615669e-1;
                        ak[2] = -4.64910887221e-3; bk[2] = 2.97143406325e-1;
                        ak[3] = -6.06834887760e-4; bk[3] = 5.79490176079e-2;
                        ak[4] = -6.14830384279e-6; bk[4] = 5.74558524851e-3;
                        eps2 = Ratfun(eta, ak, bk);
                    }
                    else if (eta < 2.0)
                    {
                        ak[0] = -1.72839517431e-2; bk[0] = 1.00000000000e+0;
                        ak[1] = -1.46362417966e-2; bk[1] = 6.90560400696e-1;
                        ak[2] = -3.57406772616e-3; bk[2] = 2.49962384741e-1;
                        ak[3] = -3.91032032692e-4; bk[3] = 4.43843438769e-2;
                        ak[4] = 2.49634036069e-6; bk[4] = 4.24073217211e-3;
                        eps2 = Ratfun(eta, ak, bk);
                    }
                    else if (eta < 1000.0)
                    {
                        ak[0] = 9.99944669480e-1; bk[0] = 1.00000000000e+0;
                        ak[1] = 1.04649839762e+2; bk[1] = 1.04526456943e+2;
                        ak[2] = 8.57204033806e+2; bk[2] = 8.23313447808e+2;
                        ak[3] = 7.31901559577e+2; bk[3] = 3.11993802124e+3;
                        ak[4] = 4.55174411671e+1; bk[4] = 3.97003311219e+3;
                        double x = 1.0 / eta;
                        eps2 = Ratfun(x, ak, bk) / (-12.0 * eta);
                    }
                    else
                    {
                        eps2 = -1.0 / (12.0 * eta);
                    }

                    return eps2;
                }

                static double Eps3(double eta)
                {
                    double eps3;
                    double[] ak = new double[5];
                    double[] bk = new double[5];

                    if (eta < -8.0)
                    {
                        double x = eta * eta;
                        double y = Math.Log(-eta) / eta;
                        eps3 = (-30.0 + eta * y * (6.0 * x * y * y - 12.0 + x)) / (12.0 * eta * x * x);
                    }
                    else if (eta < -4.0)
                    {
                        ak[0] = 4.95346498136e-2; bk[0] = 1.00000000000e+0;
                        ak[1] = 2.99521337141e-2; bk[1] = 7.59803615283e-1;
                        ak[2] = 6.88296911516e-3; bk[2] = 2.61547111595e-1;
                        ak[3] = 5.12634846317e-4; bk[3] = 4.64854522477e-2;
                        ak[4] = -2.01411722031e-5; bk[4] = 4.03751193496e-3;
                        eps3 = Ratfun(eta, ak, bk) / (eta * eta);
                    }
                    else if (eta < -2.0)
                    {
                        ak[0] = 4.52313583942e-3; bk[0] = 1.00000000000e+0;
                        ak[1] = 1.20744920113e-3; bk[1] = 9.12203410349e-1;
                        ak[2] = -7.89724156582e-5; bk[2] = 4.05368773071e-1;
                        ak[3] = -5.04476066942e-5; bk[3] = 9.01638932349e-2;
                        ak[4] = -5.35770949796e-6; bk[4] = 9.48935714996e-3;
                        eps3 = Ratfun(eta, ak, bk);
                    }
                    else if (eta < 2.0)
                    {
                        ak[0] = 4.39937562904e-3; bk[0] = 1.00000000000e+0;
                        ak[1] = 4.87225670639e-4; bk[1] = 7.94435257415e-1;
                        ak[2] = -1.28470657374e-4; bk[2] = 3.33094721709e-1;
                        ak[3] = 5.29110969589e-6; bk[3] = 7.03527806143e-2;
                        ak[4] = 1.57166771750e-7; bk[4] = 8.06110846078e-3;
                        eps3 = Ratfun(eta, ak, bk);
                    }
                    else if (eta < 10.0)
                    {
                        ak[0] = -1.14811912320e-3; bk[0] = 1.00000000000e+0;
                        ak[1] = -1.12850923276e-1; bk[1] = 1.42482206905e+1;
                        ak[2] = 1.51623048511e+0; bk[2] = 6.97360396285e+1;
                        ak[3] = -2.18472031183e-1; bk[3] = 2.18938950816e+2;
                        ak[4] = 7.30002451555e-2; bk[4] = 2.77067027185e+2;
                        double x = 1.0 / eta;
                        eps3 = Ratfun(x, ak, bk) / (eta * eta);
                    }
                    else if (eta < 100.0)
                    {
                        ak[0] = -1.45727889667e-4; bk[0] = 1.00000000000e+0;
                        ak[1] = -2.90806748131e-1; bk[1] = 1.39612587808e+2;
                        ak[2] = -1.33085045450e+1; bk[2] = 2.18901116348e+3;
                        ak[3] = 1.99722374056e+2; bk[3] = 7.11524019009e+3;
                        ak[4] = -1.14311378756e+1; bk[4] = 4.55746081453e+4;
                        double x = 1.0 / eta;
                        eps3 = Ratfun(x, ak, bk) / (eta * eta);
                    }
                    else
                    {
                        double eta3 = eta * eta * eta;
                        eps3 = -Math.Log(eta) / (12.0 * eta3);
                    }
                    return eps3;
                }

                static double Lambdaeta(double eta)
                {
                    double la, r;

                    double s = eta * eta * 0.5;
                    double[] ak = new double[6];

                    if (eta == 0.0)
                    {
                        la = 1;
                    }
                    else if (eta < -1.0)
                    {
                        r = Math.Exp(-1 - s);
                        ak[1] = 1.0;
                        ak[2] = 1.0;
                        ak[3] = 3.0 / 2.0;
                        ak[4] = 8.0 / 3.0;
                        ak[5] = 125.0 / 24.0;
                        ak[6] = 54.0 / 5.0;
                        la = r * (ak[1] + r * (ak[2] + r * (ak[3] + r * (ak[4] + r * (ak[5] + r * ak[6])))));
                    }
                    else if (eta < 1.0)
                    {
                        ak[1] = 1.0;
                        ak[2] = 1.0 / 3.0;
                        ak[3] = 1.0 / 36.0;
                        ak[4] = -1.0 / 270.0;
                        ak[5] = 1.0 / 4320.0;
                        ak[6] = 1.0 / 17010.0;
                        r = eta;
                        la = 1 + r * (ak[1] + r * (ak[2] + r * (ak[3] + r * (ak[4] + r * (ak[5] + r * ak[6])))));
                    }
                    else
                    {
                        r = 11 + s;
                        double L = Math.Log(r);
                        la = r + L; r = 1.0 / r;
                        double L2 = L * L;
                        double L3 = L2 * L;
                        double L4 = L3 * L;
                        double L5 = L4 * L;
                        ak[1] = 1;
                        ak[2] = (2 - L) * 0.5;
                        ak[3] = (-9 * L + 6 + 2 * L2) / 6.0;
                        ak[4] = -(3 * L3 + 36 * L - 22 * L2 - 12) / 12.0;
                        ak[5] = (60 + 350 * L2 - 300 * L - 125 * L3 + 12 * L4) / 60.0;
                        ak[6] = -(-120 - 274 * L4 + 900 * L - 1700 * L2 + 1125 * L3 + 20 * L5) / 120.0;
                        la = la + L * r * (ak[1] + r * (ak[2] + r * (ak[3] + r * (ak[4] + r * (ak[5] + r * ak[6])))));
                    }

                    if (((eta > -3.5) && (eta < -0.03)) || ((eta > 0.03) && (eta < 40.0)))
                    {
                        r = 1;
                        double q = la;

                        while (r > 1.0e-8)
                        {
                            la = q * (s + Math.Log(q)) / (q - 1.0);
                            r = Math.Abs(q / la - 1);
                            q = la;
                        }
                    }

                    return la;
                }

                // Abramowitx & Stegun 26.2.23;
                static double Invq(double x)
                {
                    double t = Math.Sqrt(-2 * Math.Log(x));
                    t = t - (2.515517 + t * (0.802853 + t * 0.010328)) / (1.0 + t * (1.432788 + t * (0.189269 + t * 0.001308)));
                    return t;
                }

                static double Inverfc(double x)
                {
                    double y;

                    if (x > 1.0)
                    {
                        y = -Inverfc(2 - x);
                    }
                    else
                    {
                        double y0 = 0.70710678 * Invq(x / 2.0);
                        var f = Errorfunction(y0, true, false) - x;
                        double y02 = y0 * y0;
                        double fp = -2.0 / Constants.SqrtPi * Math.Exp(-y02);
                        double c1 = -1.0 / fp;
                        double c2 = y0;
                        double c3 = (4 * y02 + 1) / 3.0;
                        double c4 = y0 * (12 * y02 + 7) / 6.0;
                        double c5 = (8 * y02 + 7) * (12 * y02 + 1) / 30.0;
                        double r = f * c1;
                        double h = r * (1 + r * (c2 + r * (c3 + r * (c4 + r * c5))));
                        y = y0 + h;
                    }

                    return y;
                }

                static double Ratfun(double x, double[] ak, double[] bk)
                {
                    double p = ak[0] + x * (ak[1] + x * (ak[2] + x * (ak[3] + x * ak[4])));
                    double q = bk[0] + x * (bk[1] + x * (bk[2] + x * (bk[3] + x * bk[4])));
                    return p / q;
                }

                static double InvGam(double a, double q, bool pgam)
                {
                    double z;
                    double x = 0d;
                    double f;
                    double fp;
                    double a1;
                    double a2;
                    double a3;
                    double a4;
                    double y = 0d;
                    double y2;
                    double y3;
                    double y4;
                    double y5;
                    double y6;
                    double mu;
                    double mu2;
                    double mu3;
                    double mu4;
                    double q0 = pgam ? 1 - q : q;
                    double t = 2 * q0;

                    if (Math.Abs(t - 1) < 1.0e-10)
                    {
                        x = a - 1.0 / 3.0 + (8.0 / 405.0 + 184.0 / 25515.0 / a) / a;
                    }
                    else
                    {
                        if (t == 2.0)
                        {
                            z = -6.0;
                        }
                        else if (t < 1.0e-50)
                        {
                            z = 15.0;
                        }
                        else
                        {
                            z = Inverfc(t);
                            y = z / Math.Sqrt(a / 2.0);
                            y2 = y * y;
                            y3 = y * y2;
                            y4 = y2 * y2;
                            y5 = y * y4;
                            y6 = y3 * y3;
                            double sq2 = Constants.Sqrt2;

                            if (Math.Abs(y) < 0.3)
                            {
                                a1 = -1.0 / 3.0 + 1.0 / 36.0 * y + 1.0 / 1620.0 * y2 - 7.0 / 6480.0 * y3 + 5.0 / 18144.0 * y4 - 11.0 / 382725.0 * y5 - 101.0 / 16329600.0 * y6;
                                a2 = -7.0 / 405.0 - 7.0 / 2592.0 * y + 533.0 / 204120.0 * y2 - 1579.0 / 2099520.0 * y3 + 109.0 / 1749600.0 * y4 + 10217.0 / 251942400.0 * y5;
                                a3 = 449.0 / 102060.0 - 63149.0 / 20995200.0 * y + 29233.0 / 36741600.0 * y2 + 346793.0 / 5290790400.0 * y3 - 18442139.0 / 130947062400.0 * y4;
                            }
                            else
                            {
                                f = Inveta(y / sq2);
                                mu = f - 1.0;
                                mu2 = mu * mu;
                                mu3 = mu * mu2;
                                double mup = (mu + 1.0) * y / mu;
                                f = y / mu;
                                double f2 = f * f;
                                fp = f * (1.0 - f2 - y * f) / y;
                                double fpp = -f * (3 * f * fp + f + 2 * y * fp) / y;
                                a1 = Math.Log(f) / y;
                                double a12 = a1 * a1;
                                double a1p = -a1 / y + 1.0 / y2 - mup / (mu * y);
                                double a1pp = a1 / y2 - a1p / y - 2.0 / y3 + mup * (2.0 + mu) / mu3;
                                a2 = -(-12 * a1p * f - 12 * fp * a1 + f + 6 * a12 * f) / (12 * f * y);
                                double a2p = -a2 / y - a2 * fp / f + (12 * (a1pp * f + 2 * a1p * fp + fpp * a1) - fp - 12 * f * a1 * a1p - 6 * fp * a12) / (12 * f * y);
                                a3 = (6 * ((2 * a1 - a12 * y) * fp * fp + a12 * (fpp * f * y + a1 * f2) - a1p * a1p * f2 * y) + 12 * ((a2p * y - a1 * a1p) * f2 + fp * a1p * f) + a1 * f2 - f * (fp + 18 * fp * a12)) / (12 * f2 * y2);
                            }

                            y = y + (a1 + (a2 + a3 / a) / a) / a;
                            x = a * Inveta(y / sq2);
                        }

                        Incgam(a, x, out _, out f, out _);

                        fp = -Math.Sqrt(a / Constants.Pi2) * Math.Exp(-0.5 * y * y) / (Gamstar(a));
                        y = (f - q0) / fp;
                        double x2 = x * x;
                        double x3 = x * x2;
                        double x4 = x * x3;
                        y2 = y * y;
                        y3 = y * y2;
                        y4 = y * y3;
                        a2 = a * a;
                        a3 = a * a2;
                        a4 = a * a3;
                        mu = 60 * (-x + a - 1.0);
                        mu2 = 20 * (2 * x2 - 4 * a * x + 4 * x + 2 * a2 - 3 * a + 1);
                        mu3 = 5 * (6 * a + 6 * a3 - 6 * x3 - 11 * x - 1 + 29 * a * x - 11 * a2 - 18 * x2 - 18 * a2 * x + 18 * a * x2);
                        mu4 = (24 * x4 - 10 * a - 50 * a3 + 96 * x3 + 26 * x + 24 * a4 + 144 * a2 * x2 - 96 * a3 * x - 126 * a * x - 96 * a * x3 + 35 * a2 + 98 * x2 + 196 * a2 * x - 242 * a * x2 + 1);
                        x = x * (1.0 - y * (120 + mu * y + mu2 * y2 + mu3 * y3 + mu4 * y4) / 120);
                    }

                    return x;
                }

                static double Inveta(double x)
                {

                    if (x < -26.0)
                    {
                        return 0.0;
                    }

                    if (x == 0.0)
                    {
                        return 1.0;
                    }

                    double z = x * x, t, p, q, mu;
                    double x2 = x * Constants.Pi2;

                    if (x2 > 2.0)
                    {
                        p = z + 1;
                        q = Math.Log(p);
                        double a = 1.0 / q;
                        double b = 1.0 / 3.0 + a * (a - 1.5);
                        double r = q / p;
                        mu = z + q + r * (1 + r * (a - 0.5 + b * r));
                        t = mu + 1;
                    }
                    else if (x2 > -1.5)
                    {
                        mu = x2 * (1.0 + x2 * (1.0 / 3.0 + x2 * (1.0 / 36.0 + x2 * (-1.0 / 270.0 + x2 * (1.0 / 4320.0 + x2 / 17010.0)))));
                        t = mu + 1;
                    }
                    else
                    {
                        p = Math.Exp(-z - 1.0);
                        t = p * (1.0 + p * (1.0 + p * (1.5 + p * (8.0 / 3.0 + p * 125.0 / 24.0))));
                        mu = t - 1;
                    }

                    bool ready = false;
                    int k = 0;

                    while (!ready)
                    {
                        ready = true;
                        p = Lnec(mu);
                        double r = -p - z;

                        if (Math.Abs(r) > 1.0e-18)
                        {
                            r = r * t / mu;
                            p = r / t / mu;
                            q = r * (1.0 - p * (4 * t - 1.0) / 6.0) / (1.0 - p * (2 * t + 1.0) / 3.0);
                            mu = mu - q;
                            t = t - q;
                            k = k + 1;

                            if ((t <= 0) || (mu <= -1))
                            {
                                t = 0;
                                mu = -1;
                            }
                            else
                            {
                                ready = (k > 5) || (Math.Abs(q) < (1.0e-10) * (Math.Abs(mu) + 1));
                            }
                        }
                    }
                    return t;
                }
            }
        }
    }
}
