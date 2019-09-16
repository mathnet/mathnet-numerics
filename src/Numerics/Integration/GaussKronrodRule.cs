// <copyright file="GaussKronrodRule.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2019 Math.NET
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

// This file uses code from the Boost Project.
//  Copyright John Maddock 2017.
//  Copyright Nick Thompson 2017.
//  Use, modification and distribution are subject to the
//  Boost Software License, Version 1.0. (See accompanying file
//  LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)
//  https://github.com/boostorg/math/blob/develop/include/boost/math/quadrature/gauss_kronrod.hpp

using System;
using System.Numerics;

namespace MathNet.Numerics.Integration
{
    public static class GaussKronrodRule
    {
        const double epsilon = 2.2204460492503131e-016;

        /// <summary>
        /// The number of Gauss-Kronrod points. Pre-computed for 15, 31, 41, 51 and 61 points.
        /// </summary>
        static int Order = 15;        

        static double integrate_non_adaptive_m1_1(Func<double, double> f, out double error, out double pL1)
        {
            int gauss_start = 2;
            int kronrod_start = 1;
            int gauss_order = ((int)Order - 1) / 2;

            double kronrod_result = 0d;
            double gauss_result = 0d;
            double fp, fm;

            var KAbscissa = KronrodAbscissa();
            var KWeights = KronrodWeights();
            var GWeights = GaussWeights();

            if ((gauss_order & 1) == 1)
            {
                fp = f(0);
                kronrod_result = fp * KWeights[0];
                gauss_result += fp * GWeights[0];
            }
            else
            {
                fp = f(0);
                kronrod_result = fp * KWeights[0];
                gauss_start = 1;
                kronrod_start = 2;
            }
            double L1 = Math.Abs(kronrod_result);

            for (int i = gauss_start; i < KAbscissa.Length; i += 2)
            {
                fp = f(KAbscissa[i]);
                fm = f(-KAbscissa[i]);
                kronrod_result += (fp + fm) * KWeights[i];
                L1 += (Math.Abs(fp) + Math.Abs(fm)) * KWeights[i];
                gauss_result += (fp + fm) * GWeights[i / 2];
            }
            for (int i = kronrod_start; i < KAbscissa.Length; i += 2)
            {
                fp = f(KAbscissa[i]);
                fm = f(-KAbscissa[i]);
                kronrod_result += (fp + fm) * KWeights[i];
                L1 += (Math.Abs(fp) + Math.Abs(fm)) * KWeights[i];
            }
            pL1 = L1;
            error = Math.Max(Math.Abs(kronrod_result - gauss_result), Math.Abs(kronrod_result * epsilon * 2d));
            return kronrod_result;
        }

        static Complex contour_integrate_non_adaptive_m1_1(Func<double, Complex> f, out double error, out double pL1)
        {
            int gauss_start = 2;
            int kronrod_start = 1;
            int gauss_order = ((int)Order - 1) / 2;

            Complex kronrod_result = new Complex();
            Complex gauss_result = new Complex();
            Complex fp, fm;

            var KAbscissa = KronrodAbscissa();
            var KWeights = KronrodWeights();
            var GWeights = GaussWeights();

            if (gauss_order.IsOdd())
            {
                fp = f(0);
                kronrod_result = fp * KWeights[0];
                gauss_result += fp * GWeights[0];
            }
            else
            {
                fp = f(0);
                kronrod_result = fp * KWeights[0];
                gauss_start = 1;
                kronrod_start = 2;
            }
            double L1 = Complex.Abs(kronrod_result);

            for (int i = gauss_start; i < KAbscissa.Length; i += 2)
            {
                fp = f(KAbscissa[i]);
                fm = f(-KAbscissa[i]);
                kronrod_result += (fp + fm) * KWeights[i];
                L1 += (Complex.Abs(fp) + Complex.Abs(fm)) * KWeights[i];
                gauss_result += (fp + fm) * GWeights[i / 2];
            }
            for (int i = kronrod_start; i < KAbscissa.Length; i += 2)
            {
                fp = f(KAbscissa[i]);
                fm = f(-KAbscissa[i]);
                kronrod_result += (fp + fm) * KWeights[i];
                L1 += (Complex.Abs(fp) + Complex.Abs(fm)) * KWeights[i];
            }
            pL1 = L1;
            error = Math.Max(Complex.Abs(kronrod_result - gauss_result), Complex.Abs(kronrod_result * epsilon * 2d));
            return kronrod_result;
        }

        static double recursive_adaptive_integrate(Func<double, double> f, double a, double b, int max_levels, double rel_tol, double abs_tol, out double error, out double L1)
        {
            double error_local;
            double mean = (b + a) / 2;
            double scale = (b - a) / 2;

            var r1 = integrate_non_adaptive_m1_1((x) => f(scale * x + mean), out error_local, out L1);
            var estimate = scale * r1;

            var tmp = estimate * rel_tol;
            var abs_tol1 = Math.Abs(tmp);
            if (abs_tol == 0)
            {
                abs_tol = abs_tol1;
            }

            if (max_levels > 0 && (abs_tol1 < error_local) && (abs_tol < error_local))
            {
                double mid = (a + b) / 2d;
                double L1_local;
                estimate = recursive_adaptive_integrate(f, a, mid, max_levels - 1, rel_tol, abs_tol / 2, out error, out L1);
                estimate += recursive_adaptive_integrate(f, mid, b, max_levels - 1, rel_tol, abs_tol / 2, out error_local, out L1_local);
                error += error_local;
                L1 += L1_local;
                return estimate;
            }
            L1 *= scale;
            error = error_local;
            return estimate;
        }

        static Complex contour_recursive_adaptive_integrate(Func<double, Complex> f, double a, double b, int max_levels, double rel_tol, double abs_tol, out double error, out double L1)
        {
            double error_local;
            double mean = (b + a) / 2;
            double scale = (b - a) / 2;

            var r1 = contour_integrate_non_adaptive_m1_1((x) => f(scale * x + mean), out error_local, out L1);
            var estimate = scale * r1;

            var tmp = estimate * rel_tol;
            var abs_tol1 = Complex.Abs(tmp);
            if (abs_tol == 0)
            {
                abs_tol = abs_tol1;
            }

            if (max_levels > 0 && (abs_tol1 < error_local) && (abs_tol < error_local))
            {
                double mid = (a + b) / 2d;
                double L1_local;
                estimate = contour_recursive_adaptive_integrate(f, a, mid, max_levels - 1, rel_tol, abs_tol / 2, out error, out L1);
                estimate += contour_recursive_adaptive_integrate(f, mid, b, max_levels - 1, rel_tol, abs_tol / 2, out error_local, out L1_local);
                error += error_local;
                L1 += L1_local;
                return estimate;
            }
            L1 *= scale;
            error = error_local;
            return estimate;
        }

        /// <summary>
        /// Performs adaptive Gauss-Kronrod quadrature on function f over the range (a,b)
        /// </summary>
        /// <param name="f">The analytic smooth function to integrate</param>
        /// <param name="intervalBegin">Where the interval starts</param>
        /// <param name="intervalEnd">Where the interval stops</param>
        /// <param name="error">The difference between the (N-1)/2 point Gauss approximation and the N-point Gauss-Kronrod approximation</param>
        /// <param name="L1Norm">The L1 norm of the result, if there is a significant difference between this and the returned value, then the result is likely to be ill-conditioned.</param>
        /// <param name="targetRelativeError">The maximum relative error in the result</param>
        /// <param name="maximumDepth">The maximum number of interval splittings permitted before stopping</param>
        /// <param name="order">The number of Gauss-Kronrod points. Pre-computed for 15, 21, 31, 41, 51 and 61 points</param>
        public static double Integrate(Func<double, double> f, double intervalBegin, double intervalEnd, out double error, out double L1Norm, double targetRelativeError = 1E-10, int maximumDepth = 15, int order = 15)
        {
            // Formula used for variable subsitution from 
            // 1. Shampine, L. F. (2008). Vectorized adaptive quadrature in MATLAB. Journal of Computational and Applied Mathematics, 211(2), 131-140.
            // 2. quadgk.m, GNU Octave

            if (f == null)
            {
                throw new ArgumentNullException(nameof(f));
            }

            Order = order;

            if (intervalBegin > intervalEnd)
            {
                return -Integrate(f, intervalEnd, intervalBegin, out error, out L1Norm, targetRelativeError, maximumDepth, order);
            }

            // (-oo, oo) => [-1, 1]
            //
            // integral_(-oo)^(oo) f(x) dx = integral_(-1)^(1) f(g(t)) g'(t) dt
            // g(t) = t / (1 - t^2)
            // g'(t) = (1 + t^2) / (1 - t^2)^2
            if ((intervalBegin < double.MinValue) && (intervalEnd > double.MaxValue))
            {
                Func<double, double> u = (t) =>
                {
                    return f(t / (1 - t * t)) * (1 + t * t) / ((1 - t * t) * (1 - t * t));
                };
                return recursive_adaptive_integrate(u, -1, 1, maximumDepth, targetRelativeError, 0, out error, out L1Norm);
            }
            // [a, oo) => [0, 1]
            //
            // integral_(a)^(oo) f(x) dx = integral_(0)^(oo) f(a + t^2) 2 t dt
            //                           = integral_(0)^(1) f(a + g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 - s)
            // g'(s) = 1 / (1 - s)^2
            else if (intervalEnd > double.MaxValue)
            {
                Func<double, double> u = (s) =>
                {
                    return 2 * s * f(intervalBegin + (s / (1 - s)) * (s / (1 - s))) / ((1 - s) * (1 - s) * (1 - s));
                };
                return recursive_adaptive_integrate(u, 0, 1, maximumDepth, targetRelativeError, 0, out error, out L1Norm);
            }
            // (-oo, b] => [-1, 0]
            //
            // integral_(-oo)^(b) f(x) dx = -integral_(-oo)^(0) f(b - t^2) 2 t dt
            //                            = -integral_(-1)^(0) f(b - g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 + s)
            // g'(s) = 1 / (1 + s)^2
            else if (intervalBegin < double.MinValue)
            {
                Func<double, double> u = (s) =>
                {
                    return -2 * s * f(intervalEnd - s / (1 + s) * (s / (1 + s))) / ((1 + s) * (1 + s) * (1 + s));
                };
                return recursive_adaptive_integrate(u, -1, 0, maximumDepth, targetRelativeError, 0, out error, out L1Norm);
            }
            // [a, b] => [-1, 1]
            //
            // integral_(a)^(b) f(x) dx = integral_(-1)^(1) f(g(t)) g'(t) dt
            // g(t) = (b - a) * t * (3 - t^2) / 4 + (b + a) / 2
            // g'(t) = 3 / 4 * (b - a) * (1 - t^2)
            else
            {
                Func<double, double> u = (t) =>
                {
                    return f((intervalEnd - intervalBegin) / 4 * t * (3 - t * t) + (intervalEnd + intervalBegin) / 2) * 3 * (intervalEnd - intervalBegin) / 4 * (1 - t * t);
                };
                return recursive_adaptive_integrate(u, -1, 1, maximumDepth, targetRelativeError, 0d, out error, out L1Norm);
            }
        }

        /// <summary>
        /// Performs adaptive Gauss-Kronrod quadrature on function f over the range (a,b)
        /// </summary>
        /// <param name="f">The analytic smooth complex function to integrate, defined on the real axis.</param>
        /// <param name="intervalBegin">Where the interval starts</param>
        /// <param name="intervalEnd">Where the interval stops</param>
        /// <param name="error">The difference between the (N-1)/2 point Gauss approximation and the N-point Gauss-Kronrod approximation</param>
        /// <param name="L1Norm">The L1 norm of the result, if there is a significant difference between this and the returned value, then the result is likely to be ill-conditioned.</param>
        /// <param name="targetRelativeError">The maximum relative error in the result</param>
        /// <param name="maximumDepth">The maximum number of interval splittings permitted before stopping</param>
        /// <param name="order">The number of Gauss-Kronrod points. Pre-computed for 15, 21, 31, 41, 51 and 61 points</param>
        /// <returns></returns>
        public static Complex ContourIntegrate(Func<double, Complex> f, double intervalBegin, double intervalEnd, out double error, out double L1Norm, double targetRelativeError = 1E-10, int maximumDepth = 15, int order = 15)
        {
            // Formula used for variable subsitution from 
            // 1. Shampine, L. F. (2008). Vectorized adaptive quadrature in MATLAB. Journal of Computational and Applied Mathematics, 211(2), 131-140.
            // 2. quadgk.m, GNU Octave

            if (f == null)
            {
                throw new ArgumentNullException(nameof(f));
            }

            Order = order;

            if (intervalBegin > intervalEnd)
            {
                return -ContourIntegrate(f, intervalEnd, intervalBegin, out error, out L1Norm, targetRelativeError, maximumDepth, order);
            }

            // (-oo, oo) => [-1, 1]
            //
            // integral_(-oo)^(oo) f(x) dx = integral_(-1)^(1) f(g(t)) g'(t) dt
            // g(t) = t / (1 - t^2)
            // g'(t) = (1 + t^2) / (1 - t^2)^2
            if ((intervalBegin < double.MinValue) && (intervalEnd > double.MaxValue))
            {
                Func<double, Complex> u = (t) =>
                {
                    return f(t / (1 - t * t)) * (1 + t * t) / ((1 - t * t) * (1 - t * t));
                };
                return contour_recursive_adaptive_integrate(u, -1, 1, maximumDepth, targetRelativeError, 0, out error, out L1Norm);
            }
            // [a, oo) => [0, 1]
            //
            // integral_(a)^(oo) f(x) dx = integral_(0)^(oo) f(a + t^2) 2 t dt
            //                           = integral_(0)^(1) f(a + g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 - s)
            // g'(s) = 1 / (1 - s)^2
            else if (intervalEnd > double.MaxValue)
            {
                Func<double, Complex> u = (s) =>
                {
                    return 2 * s * f(intervalBegin + (s / (1 - s)) * (s / (1 - s))) / ((1 - s) * (1 - s) * (1 - s));
                };
                return contour_recursive_adaptive_integrate(u, 0, 1, maximumDepth, targetRelativeError, 0, out error, out L1Norm);
            }
            // (-oo, b] => [-1, 0]
            //
            // integral_(-oo)^(b) f(x) dx = -integral_(-oo)^(0) f(b - t^2) 2 t dt
            //                            = -integral_(-1)^(0) f(b - g(s)^2) 2 g(s) g'(s) ds
            // g(s) = s / (1 + s)
            // g'(s) = 1 / (1 + s)^2
            else if (intervalBegin < double.MinValue)
            {
                Func<double, Complex> u = (s) =>
                {
                    return -2 * s * f(intervalEnd - s / (1 + s) * (s / (1 + s))) / ((1 + s) * (1 + s) * (1 + s));
                };
                return contour_recursive_adaptive_integrate(u, -1, 0, maximumDepth, targetRelativeError, 0, out error, out L1Norm);
            }
            // [a, b] => [-1, 1]
            //
            // integral_(a)^(b) f(x) dx = integral_(-1)^(1) f(g(t)) g'(t) dt
            // g(t) = (b - a) * t * (3 - t^2) / 4 + (b + a) / 2
            // g'(t) = 3 / 4 * (b - a) * (1 - t^2)
            else
            {
                Func<double, Complex> u = (t) =>
                {
                    return f((intervalEnd - intervalBegin) / 4 * t * (3 - t * t) + (intervalEnd + intervalBegin) / 2) * 3 * (intervalEnd - intervalBegin) / 4 * (1 - t * t);
                };
                return contour_recursive_adaptive_integrate(u, -1, 1, maximumDepth, targetRelativeError, 0d, out error, out L1Norm);
            }
        }

        #region Pre-computed Abscissa and weights

        static double[] KronrodAbscissa()
        {
            switch (Order)
            {
                default:
                case 15:
                    return PrecomputedKronrodAbscissas[0];
                case 21:
                    return PrecomputedKronrodAbscissas[1];
                case 31:
                    return PrecomputedKronrodAbscissas[2];
                case 41:
                    return PrecomputedKronrodAbscissas[3];
                case 51:
                    return PrecomputedKronrodAbscissas[4];
                case 61:
                    return PrecomputedKronrodAbscissas[5];
            }
        }

        static double[] KronrodWeights()
        {
            switch (Order)
            {
                default:
                case 15:
                    return PrecomputedKronrodWeights[0];
                case 21:
                    return PrecomputedKronrodWeights[1];
                case 31:
                    return PrecomputedKronrodWeights[2];
                case 41:
                    return PrecomputedKronrodWeights[3];
                case 51:
                    return PrecomputedKronrodWeights[4];
                case 61:
                    return PrecomputedKronrodWeights[5];
            }
        }

        static double[] GaussWeights()
        {
            switch (Order)
            {
                default:
                case 15:
                    return PrecomputedGaussWeights[0];
                case 21:
                    return PrecomputedGaussWeights[1];
                case 31:
                    return PrecomputedGaussWeights[2];
                case 41:
                    return PrecomputedGaussWeights[3];
                case 51:
                    return PrecomputedGaussWeights[4];
                case 61:
                    return PrecomputedGaussWeights[5];
            }
        }

        /// <summary>
        /// precomputed abscissa vector per order 15, 21, 31, 41, 51 and 61
        /// </summary>
        static readonly double[][] PrecomputedKronrodAbscissas =
        {
            new[] // 15-point Gauss-Kronrod
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
            new[]  // 21-point Gauss-Kronrod
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
            new[] // 31-point Gauss-Kronrod
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
            new[] // 41-point Gauss-Kronrod
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
            new[] // 51-point Gauss-Kronrod
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
            new[] // 61-point Gauss-Kronrod
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
            }
        };

        /// <summary>
        /// precomputed weight vector per order 15, 21, 31, 41, 51 and 61
        /// </summary>
        static readonly double[][] PrecomputedKronrodWeights =
        {
            new[] // 15-point Gauss-Kronrod integration
            {
                2.09482141084727828e-01,
                2.04432940075298892e-01,
                1.90350578064785410e-01,
                1.69004726639267903e-01,
                1.40653259715525919e-01,
                1.04790010322250184e-01,
                6.30920926299785533e-02,
                2.29353220105292250e-02,
            },
            new[] // 21-point Gauss-Kronrod integration
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
            },
            new[] // 31-point Gauss-Kronrod integration
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
            },
            new[] // 41-point Gauss-Kronrod integration
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
            },
            new[] // 51-point Gauss-Kronrod integration
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
            },
            new[] // 61-point Gauss-Kronrod integration
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
            },
        };

        /// <summary>
        /// precomputed Gauss weight vector per order 7, 10, 15, 20, 25 and 30
        /// </summary>
        static readonly double[][] PrecomputedGaussWeights =
        {
            new [] // 7-point Gauss
            {
                4.17959183673469388e-01,
                3.81830050505118945e-01,
                2.79705391489276668e-01,
                1.29484966168869693e-01,
            },
            new[] // 10-point Gauss
            {
                2.95524224714752870e-01,
                2.69266719309996355e-01,
                2.19086362515982044e-01,
                1.49451349150580593e-01,
                6.66713443086881376e-02,
            },
            new[] // 15-point Gauss
            {
                2.02578241925561273e-01,
                1.98431485327111576e-01,
                1.86161000015562211e-01,
                1.66269205816993934e-01,
                1.39570677926154314e-01,
                1.07159220467171935e-01,
                7.03660474881081247e-02,
                3.07532419961172684e-02,
            },
            new[] // 20-point Gauss
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
            },
            new[] // 25-point Gauss
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
            },
            new[] // 30-point Gauss
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
            }
        };

        #endregion Pre-computed Abscissa and weights
    }
}
