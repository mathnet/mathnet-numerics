using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Integration.GaussRule
{
    /// <summary>
    /// Contains a method to compute the Gauss-Kronrod abscissas/weights and precomputed abscissas/weights for orders 15, 21, 31, 41, 51, 61.
    /// </summary>
    internal static partial class GaussKronrodPoint
    {
        /// <summary>
        /// Precomputed abscissas/weights for orders 15, 21, 31, 41, 51, 61.
        /// </summary>
        internal static readonly Dictionary<int, GaussPointPair> PreComputed = new Dictionary<int, GaussPointPair>
        {
            { 15, new GaussPointPair(15,
                new[] // 15-point Gauss-Kronrod Abscissa
                {
                    0.00000000000000000e+00,
                    2.07784955007898468e-01,
                    4.05845151377397167e-01,
                    5.86087235467691130e-01,
                    7.41531185599394440e-01,
                    8.64864423359769073e-01,
                    9.49107912342758525e-01,
                    9.91455371120812639e-01,
                },
                new[] // 15-point Gauss-Kronrod Weights
                {
                    2.09482141084727828e-01,
                    2.04432940075298892e-01,
                    1.90350578064785410e-01,
                    1.69004726639267903e-01,
                    1.40653259715525919e-01,
                    1.04790010322250184e-01,
                    6.30920926299785533e-02,
                    2.29353220105292250e-02,
                }, 7,
                new[] // 7-point Gauss Weights
                {
                    4.17959183673469388e-01,
                    3.81830050505118945e-01,
                    2.79705391489276668e-01,
                    1.29484966168869693e-01,
                })
            },
            { 21, new GaussPointPair(21,
                new[]  // 21-point Gauss-Kronrod Abscissa
                {
                    0.00000000000000000e+00,
                    1.48874338981631211e-01,
                    2.94392862701460198e-01,
                    4.33395394129247191e-01,
                    5.62757134668604683e-01,
                    6.79409568299024406e-01,
                    7.80817726586416897e-01,
                    8.65063366688984511e-01,
                    9.30157491355708226e-01,
                    9.73906528517171720e-01,
                    9.95657163025808081e-01,
                },
                new[] // 21-point Gauss-Kronrod Weights
                {
                    1.49445554002916906e-01,
                    1.47739104901338491e-01,
                    1.42775938577060081e-01,
                    1.34709217311473326e-01,
                    1.23491976262065851e-01,
                    1.09387158802297642e-01,
                    9.31254545836976055e-02,
                    7.50396748109199528e-02,
                    5.47558965743519960e-02,
                    3.25581623079647275e-02,
                    1.16946388673718743e-02,
                }, 10,
                new[] // 10-point Gauss Weights
                {
                    2.95524224714752870e-01,
                    2.69266719309996355e-01,
                    2.19086362515982044e-01,
                    1.49451349150580593e-01,
                    6.66713443086881376e-02,
                })
            },
            { 31, new GaussPointPair(31,
                new[] // 31-point Gauss-Kronrod Abscissa
                {
                    0.00000000000000000e+00,
                    1.01142066918717499e-01,
                    2.01194093997434522e-01,
                    2.99180007153168812e-01,
                    3.94151347077563370e-01,
                    4.85081863640239681e-01,
                    5.70972172608538848e-01,
                    6.50996741297416971e-01,
                    7.24417731360170047e-01,
                    7.90418501442465933e-01,
                    8.48206583410427216e-01,
                    8.97264532344081901e-01,
                    9.37273392400705904e-01,
                    9.67739075679139134e-01,
                    9.87992518020485428e-01,
                    9.98002298693397060e-01,
                },
                new[] // 31-point Gauss-Kronrod Weights
                {
                    1.01330007014791549e-01,
                    1.00769845523875595e-01,
                    9.91735987217919593e-02,
                    9.66427269836236785e-02,
                    9.31265981708253212e-02,
                    8.85644430562117706e-02,
                    8.30805028231330210e-02,
                    7.68496807577203789e-02,
                    6.98541213187282587e-02,
                    6.20095678006706403e-02,
                    5.34815246909280873e-02,
                    4.45897513247648766e-02,
                    3.53463607913758462e-02,
                    2.54608473267153202e-02,
                    1.50079473293161225e-02,
                    5.37747987292334899e-03,
                }, 15,
                new[] // 15-point Gauss Weights
                {
                    2.02578241925561273e-01,
                    1.98431485327111576e-01,
                    1.86161000015562211e-01,
                    1.66269205816993934e-01,
                    1.39570677926154314e-01,
                    1.07159220467171935e-01,
                    7.03660474881081247e-02,
                    3.07532419961172684e-02,
                })
            },
            { 41, new GaussPointPair(41,
                new[] // 41-point Gauss-Kronrod Abscissa
                {
                    0.00000000000000000e+00,
                    7.65265211334973338e-02,
                    1.52605465240922676e-01,
                    2.27785851141645078e-01,
                    3.01627868114913004e-01,
                    3.73706088715419561e-01,
                    4.43593175238725103e-01,
                    5.10867001950827098e-01,
                    5.75140446819710315e-01,
                    6.36053680726515025e-01,
                    6.93237656334751385e-01,
                    7.46331906460150793e-01,
                    7.95041428837551198e-01,
                    8.39116971822218823e-01,
                    8.78276811252281976e-01,
                    9.12234428251325906e-01,
                    9.40822633831754754e-01,
                    9.63971927277913791e-01,
                    9.81507877450250259e-01,
                    9.93128599185094925e-01,
                    9.98859031588277664e-01,
                },
                new[] // 41-point Gauss-Kronrod Weights
                {
                    7.66007119179996564e-02,
                    7.63778676720807367e-02,
                    7.57044976845566747e-02,
                    7.45828754004991890e-02,
                    7.30306903327866675e-02,
                    7.10544235534440683e-02,
                    6.86486729285216193e-02,
                    6.58345971336184221e-02,
                    6.26532375547811680e-02,
                    5.91114008806395724e-02,
                    5.51951053482859947e-02,
                    5.09445739237286919e-02,
                    4.64348218674976747e-02,
                    4.16688733279736863e-02,
                    3.66001697582007980e-02,
                    3.12873067770327990e-02,
                    2.58821336049511588e-02,
                    2.03883734612665236e-02,
                    1.46261692569712530e-02,
                    8.60026985564294220e-03,
                    3.07358371852053150e-03,
                }, 20,
                new[] // 20-point Gauss Weights
                {
                    1.52753387130725851e-01,
                    1.49172986472603747e-01,
                    1.42096109318382051e-01,
                    1.31688638449176627e-01,
                    1.18194531961518417e-01,
                    1.01930119817240435e-01,
                    8.32767415767047487e-02,
                    6.26720483341090636e-02,
                    4.06014298003869413e-02,
                    1.76140071391521183e-02,
                })
            },
            { 51, new GaussPointPair(51,
                new[] // 51-point Gauss-Kronrod Abscissa
                {
                    0.00000000000000000e+00,
                    6.15444830056850789e-02,
                    1.22864692610710396e-01,
                    1.83718939421048892e-01,
                    2.43866883720988432e-01,
                    3.03089538931107830e-01,
                    3.61172305809387838e-01,
                    4.17885382193037749e-01,
                    4.73002731445714961e-01,
                    5.26325284334719183e-01,
                    5.77662930241222968e-01,
                    6.26810099010317413e-01,
                    6.73566368473468364e-01,
                    7.17766406813084388e-01,
                    7.59259263037357631e-01,
                    7.97873797998500059e-01,
                    8.33442628760834001e-01,
                    8.65847065293275595e-01,
                    8.94991997878275369e-01,
                    9.20747115281701562e-01,
                    9.42974571228974339e-01,
                    9.61614986425842512e-01,
                    9.76663921459517511e-01,
                    9.88035794534077248e-01,
                    9.95556969790498098e-01,
                    9.99262104992609834e-01,
                },
                new[] // 51-point Gauss-Kronrod Weights
                {
                    6.15808180678329351e-02,
                    6.14711898714253167e-02,
                    6.11285097170530483e-02,
                    6.05394553760458629e-02,
                    5.97203403241740600e-02,
                    5.86896800223942080e-02,
                    5.74371163615678329e-02,
                    5.59508112204123173e-02,
                    5.42511298885454901e-02,
                    5.23628858064074759e-02,
                    5.02776790807156720e-02,
                    4.79825371388367139e-02,
                    4.55029130499217889e-02,
                    4.28728450201700495e-02,
                    4.00838255040323821e-02,
                    3.71162714834155436e-02,
                    3.40021302743293378e-02,
                    3.07923001673874889e-02,
                    2.74753175878517378e-02,
                    2.40099456069532162e-02,
                    2.04353711458828355e-02,
                    1.68478177091282982e-02,
                    1.32362291955716748e-02,
                    9.47397338617415161e-03,
                    5.56193213535671376e-03,
                    1.98738389233031593e-03,
                }, 25,
                new[] // 25-point Gauss Weights
                {
                    1.23176053726715451e-01,
                    1.22242442990310042e-01,
                    1.19455763535784772e-01,
                    1.14858259145711648e-01,
                    1.08519624474263653e-01,
                    1.00535949067050644e-01,
                    9.10282619829636498e-02,
                    8.01407003350010180e-02,
                    6.80383338123569172e-02,
                    5.49046959758351919e-02,
                    4.09391567013063127e-02,
                    2.63549866150321373e-02,
                    1.13937985010262879e-02,
                })
            },
            { 61, new GaussPointPair(61,
                new[] // 61-point Gauss-Kronrod Abscissa
                {
                    0.00000000000000000e+00,
                    5.14718425553176958e-02,
                    1.02806937966737030e-01,
                    1.53869913608583547e-01,
                    2.04525116682309891e-01,
                    2.54636926167889846e-01,
                    3.04073202273625077e-01,
                    3.52704725530878113e-01,
                    4.00401254830394393e-01,
                    4.47033769538089177e-01,
                    4.92480467861778575e-01,
                    5.36624148142019899e-01,
                    5.79345235826361692e-01,
                    6.20526182989242861e-01,
                    6.60061064126626961e-01,
                    6.97850494793315797e-01,
                    7.33790062453226805e-01,
                    7.67777432104826195e-01,
                    7.99727835821839083e-01,
                    8.29565762382768397e-01,
                    8.57205233546061099e-01,
                    8.82560535792052682e-01,
                    9.05573307699907799e-01,
                    9.26200047429274326e-01,
                    9.44374444748559979e-01,
                    9.60021864968307512e-01,
                    9.73116322501126268e-01,
                    9.83668123279747210e-01,
                    9.91630996870404595e-01,
                    9.96893484074649540e-01,
                    9.99484410050490638e-01,
                },
                new[] // 61-point Gauss-Kronrod Weights
                {
                    5.14947294294515676e-02,
                    5.14261285374590259e-02,
                    5.12215478492587722e-02,
                    5.08817958987496065e-02,
                    5.04059214027823468e-02,
                    4.97956834270742064e-02,
                    4.90554345550297789e-02,
                    4.81858617570871291e-02,
                    4.71855465692991539e-02,
                    4.60592382710069881e-02,
                    4.48148001331626632e-02,
                    4.34525397013560693e-02,
                    4.19698102151642461e-02,
                    4.03745389515359591e-02,
                    3.86789456247275930e-02,
                    3.68823646518212292e-02,
                    3.49793380280600241e-02,
                    3.29814470574837260e-02,
                    3.09072575623877625e-02,
                    2.87540487650412928e-02,
                    2.65099548823331016e-02,
                    2.41911620780806014e-02,
                    2.18280358216091923e-02,
                    1.94141411939423812e-02,
                    1.69208891890532726e-02,
                    1.43697295070458048e-02,
                    1.18230152534963417e-02,
                    9.27327965951776343e-03,
                    6.63070391593129217e-03,
                    3.89046112709988405e-03,
                    1.38901369867700762e-03,
                }, 30,
                new[] // 30-point Gauss Weights
                {
                    1.02852652893558840e-01,
                    1.01762389748405505e-01,
                    9.95934205867952671e-02,
                    9.63687371746442596e-02,
                    9.21225222377861287e-02,
                    8.68997872010829798e-02,
                    8.07558952294202154e-02,
                    7.37559747377052063e-02,
                    6.59742298821804951e-02,
                    5.74931562176190665e-02,
                    4.84026728305940529e-02,
                    3.87991925696270496e-02,
                    2.87847078833233693e-02,
                    1.84664683110909591e-02,
                    7.96819249616660562e-03,
                })
            },
        };
    }

    /// <summary>
    /// Contains a method to compute the Gauss-Kronrod abscissas/weights.
    /// </summary>
    internal static partial class GaussKronrodPoint
    {
        /// <summary>
        /// Computes the Gauss-Kronrod abscissas/weights and Gauss weights.
        /// </summary>
        /// <param name="order">Defines an Nth order Gauss-Kronrod rule. The order also defines the number of abscissas and weights for the rule.</param>
        /// <param name="eps">Required precision to compute the abscissas/weights.</param>
        /// <returns>Object containing the non-negative abscissas/weights, order.</returns>
        internal static GaussPointPair Generate(int order, double eps)
        {
            int gaussOrder = (order - 1) / 2;
            int gaussStart = gaussOrder.IsOdd() ? 0 : 1;
            int kronrodStart = gaussOrder.IsOdd() ? 1 : 0;

            var gaussPoint = GaussLegendrePointFactory.GetGaussPoint(gaussOrder);
            var gaussAbscissas = gaussPoint.Abscissas;
            var gaussWeights = gaussPoint.Weights;

            // Calculate Kronrod polynomial in terms of Legendre polynomials
            // K(x) = c0*P(0, x) + c1*P(1, x) + ...

            var c = StieltjesP(gaussOrder + 1);

            // Calculate Abscissas for Kronrod polynomial

            int r = gaussOrder.IsOdd() ? (gaussOrder - 1) / 2 + 1 : gaussOrder / 2 + 1;
            var kronrodAbscissas = new double[r];

            for (int k = 1; k <= gaussOrder + 1; k = k + 2)
            {
                var x0 = (1.0 - (1.0 - 1.0 / gaussOrder) / (8 * gaussOrder * gaussOrder)) * Math.Cos((k - 0.5) * Math.PI / (2.0 * gaussOrder + 1.0));
                double dx;
                var j = 1; // iterations

                // Newton iterations
                do
                {
                    var E = LegendreSeries(c, x0);
                    dx = E.Item1 / E.Item2;
                    x0 = x0 - dx;
                    j++;
                }
                while (Math.Abs(dx) > eps && j < 100);

                if (Math.Abs(x0) < Precision.MachineEpsilon) x0 = 0.0;

                kronrodAbscissas[(k - 1) / 2] = x0;
            }

            // Concatenate two abscissas

            var abscissas = new double[gaussAbscissas.Length + kronrodAbscissas.Length];
            gaussAbscissas.CopyTo(abscissas, 0);
            kronrodAbscissas.CopyTo(abscissas, gaussAbscissas.Length);
            abscissas = abscissas.OrderBy(v => v).ToArray();

            // Calculate weights for abscissas

            var weights = new double[gaussAbscissas.Length + kronrodAbscissas.Length];
            for (int i = gaussStart; i < abscissas.Length; i += 2)
            {
                var x = abscissas[i];

                var E = LegendreSeries(c, x);
                var L = LegendreP(gaussOrder, x);

                var p = L.Item2;
                var w2 = 2.0 / ((1.0 - x * x) * p * p); // Gauss weight
                weights[i] = w2 + 2.0 / ((gaussOrder + 1.0) * p * E.Item1);
            }
            for (int i = kronrodStart; i < abscissas.Length; i += 2)
            {
                var x = abscissas[i];

                var E = LegendreSeries(c, x);
                var L = LegendreP(gaussOrder, x);

                weights[i] = 2.0 / ((gaussOrder + 1.0) * L.Item1 * E.Item2);
            }

            return new GaussPointPair(order, abscissas, weights, gaussOrder, gaussWeights);
        }

        /// <summary>
        /// Returns coefficients of a Stieltjes polynomial in terms of Legendre polynomials.
        /// </summary>
        static double[] StieltjesP(int order)
        {
            // Reference:
            // 1. Patterson, Thomas NL. "The optimum addition of points to quadrature formulae." Mathematics of Computation 22.104 (1968): 847-856.
            // 2. Piessens, Robert, and Maria Branders. "A note on the optimal addition of abscissas to quadrature formulas of Gauss and Lobatto type." Mathematics of Computation (1974): 135-139.
            // 3. Legendre-Stieltjes Polynomials, Boost.Math
            //
            // Here, we are using Patterson algorithm, expanding the Stieltjes polynomial in terms of Legendre polynomials.
            //
            // Kronrod Polynomial K[n + 1, x] is expanded in terms of Legendre Polynomial P[n, x].
            //
            //       K[n + 1, x] = sum_(n=1)^r a[i] P[2 * i - 1 - q, x]
            //
            // where P[n, x] is the Legendre polynomial of degree n,
            //       [x] denotes the integer part of x,
            //       q = n - 2[n/2]
            //       r = [(n + 3)/2]
            //
            // The added n + 1 Kronrod abscissae is the roots of the Kronrod polynomial.

            if (order == 1)         // P(1, x)
                return new[] { 0.0, 1.0 };
            if (order == 2)    // -2/5 * P(0, x) +  P(2, x)
                return new[] { -0.4, 0.0, 1.0 };
            if (order == 3)    // -9/14 * P(1, x) + P(3, x)
                return new[] { 0, -0.642857142857142857142857142857, 0.0, 1.0 };
            if (order == 4)    // 14/891 * P(0, x) - 20/27 * P(2, x) + P(4, x)
                return new[] { 0.0157126823793490460157126823793, 0, -0.740740740740740740740740740741, 0.0, 1.0 };
            if (order == 5)    // 135/12584 * P(1, x) - 35/44 * P(3, x) + P(5, x)
                return new[] { 0, 0.0107279084551811824539097266370, 0, -0.795454545454545454545454545455, 0.0, 1.0 };

            int n = order - 1;
            int q = n.IsOdd() ? 1 : 0;
            int r = n.IsOdd() ? (n - 1) / 2 + 2 : n / 2 + 1;

            double[] a = new double[r + 1];

            // Calculate a[i] for i = 1, ..., r
            //
            // a[r] = 1;
            // a[r - 1] = -a[r] * S[r, 1] / S[r - 1, 1];
            // a[r - 2] = -a[r] * S[r, 2] / S[r - 2, 2] - a[r - 1] * S[r - 1, 2] / S[r - 2, 2];
            // ...
            // a[1] = -a[r] * S[r, r - 1] / S[1, r - 1] - a[r - 1] * S[r - 1, r - 1] / S[1, r - 1] - ... - a[2] * S[2, r - 1] / S[1, r - 1];
            //
            // S[i, k] / S[r - k, k] = S[i - 1, k] / S[r - k, k]
            //                         * ((n - q + 2 * (i + k - 1)) * (n + q + 2 * (k - i + 1)) * (n - 1 - q + 2 * (i - k)) * (2 * (k + i - 1) - 1 - q - n))
            //                         / ((n - q + 2 * (i - k)) * (2 * (k + i - 1) - q - n) * (n + 1 + q + 2 * (k - i)) * (n - 1 - q + 2 * (i + k)));

            a[r] = 1.0;
            for (int k = 1; k < r; k++)
            {
                double ratio = 1.0;
                a[r - k] = 0.0;
                for (int i = r + 1 - k; i <= r; i++)
                {
                    double numerator = (n - q + 2 * (i + k - 1)) * (n + q + 2 * (k - i + 1)) * (n - 1 - q + 2 * (i - k)) * (2 * (k + i - 1) - 1 - q - n);
                    double denominator = (n - q + 2 * (i - k)) * (2 * (k + i - 1) - q - n) * (n + 1 + q + 2 * (k - i)) * (n - 1 - q + 2 * (i + k));
                    ratio = ratio * numerator / denominator;
                    a[r - k] -= a[i] * ratio;
                }
            }

            // K = sum c[k] P[k, x]

            double[] c = new double[2 * r - q];
            for (int i = 1; i < a.Length; i++)
            {
                c[2 * i - 1 - q] = a[i];
            }

            return c;
        }

        /// <summary>
        /// Return value and derivative of a Legendre series at given points.
        /// </summary>
        static (double, double) LegendreSeries(double[] a, double x)
        {
            // S = a[0]*P[0, x] + ... + a[k]*P[k, x] + ... + a[n]*P[n, x]
            // where P[k, x] is the Legendre polynomial of order k
            //
            // According to the Clenshaw algorithm, S can be written by
            // S = a[0] + x*b[1, x] - 1/2 * b[2,x]
            //
            // b[n + 1, x] = 0
            // b[n + 2, x] = 0
            // b[k, x] = a[k] + (2k + 1)/(k + 1)*x*b[k + 1, x] - (k + 1)/(k + 2)*b[k + 2, x]
            //
            // Derivative of S is given by
            // S' = b[1, x] + x*b'[1, x] - 1/2 * b'[2,x]
            //
            // b'[k, x] = (2k + 1)/(k + 1)*b[k + 1, x] + (2k + 1)/(k + 1)*x*b'[k + 1, x] - (k + 1)/(k + 2)*b'[k + 2, x]

            if (a.Length == 1)
                return (a[0], 0);
            if (a.Length == 2)
                return (a[0] + a[1] * x, a[1]);

            double b0, b1 = 0.0, b2 = 0.0;
            double p0, p1 = 0.0, p2 = 0.0;

            for (int k = a.Length - 1; k >= 1; k--)
            {
                b0 = a[k] + (2.0 * k + 1.0) / (k + 1.0) * x * b1 - (k + 1.0) / (k + 2.0) * b2;
                p0 = (2.0 * k + 1.0) / (k + 1.0) * (b1 + x * p1) - (k + 1.0) / (k + 2.0) * p2;

                b2 = b1;
                b1 = b0;
                p2 = p1;
                p1 = p0;
            }

            var value = a[0] + b1 * x - 0.5 * b2;
            var derivative = b1 + p1 * x - 0.5 * p2;
            return (value, derivative);
        }

        /// <summary>
        /// Return value and derivative of a Legendre polynomial of order at given points.
        /// </summary>
        static (double, double) LegendreP(int order, double x)
        {
            // The Legendre polynomial, P[n, x], is defined by the recurrence relation:
            //
            // P[0, x] = 1
            // P[1, x] = x
            // (n + 1) * P[n + 1, x] = (2 * n + 1) * x * P[n, x] - n * P[n - 1, x]
            //
            // The derivative of the Legendre polynomial, P'[n, x] is given by
            // P'[0, x] = 0
            // P'[1, x] = 1
            // (n + 1) * P'[n + 1, x] = (2 * n + 1) * P[n, x] + (2 * n + 1) * x * P'[n, x] - n * P'[n - 1, x]
            //                        = (2 * n + 1) * (P[n, x] + x * P'[n, x]) - n * P'[n - 1, x]

            if (order == 0)
                return (1.0, 0.0);
            if (order == 1)
                return (x, 1.0);

            double b0 = 0.0, b1 = 1.0, b2 = 0.0;
            double p0 = 0.0, p1 = 0.0, p2 = 0.0;

            for (int k = 1; k <= order; k++)
            {
                b0 = (2.0 * k - 1.0) / k * x * b1 - (k - 1.0) / k * b2; // L(k, x)
                p0 = (2.0 * k - 1.0) / k * (b1 + x * p1) - (k - 1.0) / k * p2; // L'(k, x)

                b2 = b1;
                b1 = b0;
                p2 = p1;
                p1 = p0;
            }

            var value = b0;
            var derivative = p0;
            return (value, derivative);
        }
    }
}
