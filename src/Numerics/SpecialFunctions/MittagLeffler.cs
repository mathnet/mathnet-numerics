// <copyright file="MittagLeffler.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
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
//
// <contribution>
// Copyright (c) 2015, Roberto Garrappa
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.   
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution
// * Neither the name of Department of Mathematics - University of Bari - Italy nor the names of its
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </contribution>

using System;
using System.Linq;
using Complex = System.Numerics.Complex;

namespace MathNet.Numerics
{
    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the Mittag-Leffler function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        // Translated from Professor Robert Garrappa's Matlab code[1] by hand.
        //
        // References:
        // [1] Roberto Garrappa (2022).
        //     The Mittag-Leffler function (https://www.mathworks.com/matlabcentral/fileexchange/48154-the-mittag-leffler-function),
        //     MATLAB Central File Exchange. Retrieved December 26, 2022.
        // [2] R. Garrappa,
        //     Numerical evaluation of two and three parameter Mittag-Leffler functions,
        //     SIAM Journal of Numerical Analysis, 2015, 53(3), 1350-1369.
        // [3] https://mathworld.wolfram.com/Mittag-LefflerFunction.html
        // [4] https://en.wikipedia.org/wiki/Mittag-Leffler_function

        /// <summary>
        /// Computes the Mittag-Leffler function, E_(α)(x).
        /// </summary>
        /// <param name="alpha">The </param>
        /// <param name="x">The value to evaluate.</param>
        /// <returns>The Mittag-Leffler function evaluated at given value.</returns>
        /// <remarks>
        /// E_(α)(x) = sum_(k=0)^∞ x^k/Γ(k α + 1),
        ///    where α is any positive real number.
        /// </remarks>
        public static double MittagLefflerE(double alpha, double x)
            => MittagLefflerE(alpha, 1, 1, new Complex(x, 0)).Real;

        /// <summary>
        /// Computes the generalized Mittag-Leffler function, E_(α, β)(x).
        /// </summary>
        /// <param name="x">The value to evaluate.</param>
        /// <returns>The Mittag-Leffler function evaluated at given value.</returns>
        /// <remarks>
        /// E_(α, β)(x) = sum_(k=0)^∞ x^k/Γ(k α + β),
        ///    where α is any positive real numbers.
        /// </remarks>
        public static double MittagLefflerE(double alpha, double beta, double x)
            => MittagLefflerE(alpha, beta, 1, new Complex(x, 0)).Real;

        /// <summary>
        /// Computes the three-parameter Mittag-Leffler function, E_(α, β, γ)(x).
        /// </summary>
        /// <param name="x">The value to evaluate.</param>
        /// <returns>The Mittag-Leffler function evaluated at given value.</returns>
        /// <remarks>
        /// E_(α, β, γ)(x) = sum_(k=0)^∞ (x^k Γ(k + γ))/(k! Γ(γ) Γ(k α + β)),
        ///    where α and γ must be positive real number.
        ///    If γ is not 1, α must be (0, 1) and |arg(z)| > α π.
        /// </remarks>
        public static double MittagLefflerE(double alpha, double beta, double gamma, double x)
            => MittagLefflerE(alpha, beta, gamma, new Complex(x, 0)).Real;

        /// <summary>
        /// Computes the Mittag-Leffler function, E_(α)(z).
        /// </summary>
        /// <param name="z">The value to evaluate.</param>
        /// <returns>The Mittag-Leffler function evaluated at given value.</returns>
        /// <remarks>
        /// E_(α)(z) = sum_(k=0)^∞ z^k/Γ(k α + 1),
        ///    where α is any positive real number.
        /// </remarks>
        public static Complex MittagLefflerE(double alpha, Complex z)
            => MittagLefflerE(alpha, 1, 1, z);

        /// <summary>
        /// Computes the generalized Mittag-Leffler function, E_(α, β)(z).
        /// </summary>
        /// <param name="z">The value to evaluate.</param>
        /// <returns>The Mittag-Leffler function evaluated at given value.</returns>
        /// <remarks>
        /// E_(α, β)(z) = sum_(k=0)^∞ z^k/Γ(k α + β),
        ///    where α is any positive real numbers.
        /// </remarks>
        public static Complex MittagLefflerE(double alpha, double beta, Complex z)
            => MittagLefflerE(alpha, beta, 1, z);

        /// <summary>
        /// Computes the three-parameter Mittag-Leffler function, E_(α, β, γ)(z).
        /// </summary>
        /// <param name="z">The value to evaluate.</param>
        /// <returns>The Mittag-Leffler function evaluated at given value.</returns>
        /// <remarks>
        /// E_(α, β, γ)(z) = sum_(k=0)^∞ (z^k Γ(k + γ))/(k! Γ(γ) Γ(k α + β)),
        ///    where α and γ must be positive real number.
        ///    If γ is not 1, α must be (0, 1) and |arg(z)| > α π.
        /// </remarks>
        public static Complex MittagLefflerE(double alpha, double beta, double gamma, Complex z)
        {
            const double eps = 2.2204460492503130808E-16; // 2^(1 - 53) = 2.2204E-16
            const double Pi = 3.141592653589793238462643; // π

            // alpha must be positive real.
            if (alpha <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(alpha), $"{nameof(alpha)} must be positive.");
            }

            // gamma must be positive real.
            if (gamma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gamma), $"{nameof(gamma)} must be positive.");
            }

            // if gamma is not 1
            if (Math.Abs(gamma - 1) > eps)
            { 
                if (alpha > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(alpha), $"{nameof(alpha)} must satisfy 0 < {nameof(alpha)} < 1.");
                }
                else if (Math.Abs(z.Phase) <= alpha * Pi)
                {
                    throw new NotSupportedException("This works only when |Arg(z)| > alpha*PI.");
                }
            }

            // Target precision
            const double tolerance = 1E-15;
            var log_epsilon = Math.Log(tolerance);

            return (z.Magnitude < tolerance)
                ? 1.0 / Gamma(beta)
                : LTInversion(1.0, z, alpha, beta, gamma, log_epsilon);
        }

        /// <summary>
        /// Evaluates the Mittag-Leffler function by Laplace transform inversion.
        /// </summary>
        private static Complex LTInversion(double t, Complex lambda, double alpha, double beta, double gamma, double log_epsilon)
        {
            const double eps = 2.220446049250313080847263E-16; // 2^(1 - 53) = 2.2204E-16
            const double Pi2 = 6.283185307179586476925287; // 2 π
            const double Ln10 = 2.302585092994045684017991; // log(10)
            var I = Complex.ImaginaryOne; // 1i

            // Evaluation of the relevant poles
            var theta = lambda.Phase;

            var kmin = Math.Ceiling(-alpha / 2 - theta / Pi2);
            var kmax = Math.Floor(alpha / 2 - theta / Pi2);
            // kmax - kmin < int(α) + 1, where x = θ/2π   
            // so, kmax - kmin <= int(α)
            var k_vett = kmin < kmax
                ? Generate.LinearRange((int)kmin, (int)kmax)
                : kmin == kmax
                    ? new double[] { kmin }
                    : Array.Empty<double>();
            var s_star = k_vett.Select(v => Math.Pow(lambda.Magnitude, 1 / alpha) * Complex.Exp(I * (theta + Pi2 * v) / alpha)).ToArray();

            // Evaluation of phi(s_star) for each pole
            var phi_s_star = s_star.Select(v => (v.Real + v.Magnitude) / 2).ToArray();

            // Sorting of the poles according to the value of phi(s_star)
            var index_s_star = Enumerable.Range(0, phi_s_star.Length).ToArray();
            Sorting.Sort(phi_s_star, index_s_star);

            s_star = index_s_star.Select(v => s_star[v]).ToArray();

            // Deleting possible poles with phi_s_star=0
            var index_save = phi_s_star.Select((v, i) => (v, i)).Where(v => v.v > 1.0e-15).Select(v => v.i);
            s_star = index_save.Select(v => s_star[v]).ToArray();
            phi_s_star = index_save.Select(v => phi_s_star[v]).ToArray();

            // Inserting the origin in the set of the singularities
            s_star = new[] { Complex.Zero }.Concat(s_star).ToArray();
            phi_s_star = new[] { 0d }.Concat(phi_s_star).ToArray();
            var J1 = s_star.Length;
            var J = J1 - 1;

            // Strength of the singularities
            var p = new[] { Math.Max(0, -2 * (alpha * gamma - beta + 1)) }.Concat(Enumerable.Repeat(gamma, J)).ToArray();
            var q = Enumerable.Repeat(gamma, J).Concat(new[] { double.PositiveInfinity }).ToArray();
            phi_s_star = phi_s_star.Concat(new[] { double.PositiveInfinity }).ToArray();

            // Looking for the admissible regions with respect to round-off errors
            var phi_s_star1 = phi_s_star.Take(phi_s_star.Length - 1);
            var phi_s_star2 = phi_s_star.Skip(1);
            var admissible_regions = phi_s_star1
                .Zip(phi_s_star2, (v1, v2) => (v1, v2))
                .Where((v, i) => v.v1 < (log_epsilon - Math.Log(eps)) / t && v.v1 < v.v2)
                .Select((v, i) => i)
                .ToArray();

            // Initializing vectors for optimal parameters
            var JJ1 = admissible_regions.Length;
            var mu_vett = Enumerable.Repeat(double.PositiveInfinity, JJ1).ToArray();
            var N_vett = Enumerable.Repeat(double.PositiveInfinity, JJ1).ToArray();
            var h_vett = Enumerable.Repeat(double.PositiveInfinity, JJ1).ToArray();

            // Evaluation of parameters for inversion of LT in each admissible region
            var find_region = false;
            while (!find_region)
            {
                foreach (var j1 in admissible_regions)
                {
                    (var muj, var hj, var Nj) = (j1 + 1 < J1)
                            ? OptimalParametersInRightBoundedRegion(t, phi_s_star[j1], phi_s_star[j1 + 1], p[j1], q[j1], log_epsilon)
                            : OptimalParametersInRightUnboundedRegion(t, phi_s_star[j1], p[j1], log_epsilon);

                    mu_vett[j1] = muj;
                    h_vett[j1] = hj;
                    N_vett[j1] = Nj;
                }
                if (N_vett.Min() > 200)
                {
                    log_epsilon += Ln10;
                }
                else
                {
                    find_region = true;
                }
            }

            // Selection of the admissible region for integration which involves the minimum number of nodes
            var N = (int)N_vett.Min();
            var iN = Array.IndexOf(N_vett, N);
            var mu = mu_vett[iN];
            var h = h_vett[iN];

            // Evaluation of the inverse Laplace transform
            var integral = Complex.Zero;
            for (var k = -N; k <= N; k++)
            {
                var uk = h * k;
                var zk = mu * Complex.Pow(I * uk + 1, 2);
                var zd = 2 * mu * (I - uk);
                var fk = Complex.Pow(zk, alpha * gamma - beta) / Complex.Pow(Complex.Pow(zk, alpha) - lambda, gamma) * zd;
                integral += fk * Complex.Exp(zk * t);
            }
            integral *= h / Pi2 / I;

            // Evaluation of residues
            var residues = Complex.Zero;
            for (var k = iN + 1; k < s_star.Length; k++)
            {
                residues += 1.0 / alpha * Complex.Pow(s_star[k], 1 - beta) * Complex.Exp(t * s_star[k]);
            }

            // Evaluation of the ML function
            var E = integral + residues;
            if (lambda.Imaginary == 0)
            {
                E = new Complex(E.Real, 0);
            }

            return E;
        }

        /// <summary>
        /// Finds optimal parameters in a right-bounded region.
        /// </summary>
        private static (double muj, double hj, double Nj) OptimalParametersInRightBoundedRegion(double t, double phi_s_star_j, double phi_s_star_j1, double pj, double qj, double log_epsilon)
        {
            const double Pi2 = 6.283185307179586476925287; // 2 π

            // Definition of some constants
            const double log_eps = -36.043653389117154;
            const double fac = 1.01;
            var conservative_error_analysis = true;

            // Maximum value of fbar as the ration between tolerance and round - off unit
            var f_max = Math.Exp(log_epsilon - log_eps);

            // Evaluation of the starting values for sq_phi_star_j and sq_phi_star_j1
            var sq_phi_star_j = Math.Sqrt(phi_s_star_j);
            var threshold = 2 * Math.Sqrt((log_epsilon - log_eps) / t);
            var sq_phi_star_j1 = Math.Min(Math.Sqrt(phi_s_star_j1), threshold - sq_phi_star_j);

            // Zero or negative values of pj and qj
            var sq_phibar_star_j = 0d;
            var sq_phibar_star_j1 = 0d;
            var adm_region = false;
            if (pj < 1.0e-14 && qj < 1.0e-14)
            {
                sq_phibar_star_j = sq_phi_star_j;
                sq_phibar_star_j1 = sq_phi_star_j1;
                adm_region = true;
            }

            // Zero or negative values of just pj
            var f_bar = 0d;
            if (pj < 1.0e-14 && qj >= 1.0e-14)
            {
                sq_phibar_star_j = sq_phi_star_j;
                var f_min = (sq_phi_star_j > 0)
                    ? fac * Math.Pow(sq_phi_star_j / (sq_phi_star_j1 - sq_phi_star_j), qj)
                    : fac;
                if (f_min < f_max)
                {
                    f_bar = f_min + f_min / f_max * (f_max - f_min);
                    var fq = Math.Pow(f_bar, -1 / qj);
                    sq_phibar_star_j1 = (2 * sq_phi_star_j1 - fq * sq_phi_star_j) / (2 + fq);
                    adm_region = true;
                }
                else
                {
                    adm_region = false;
                }
            }

            // Zero or negative values of just qj            
            if (pj >= 1.0e-14 && qj < 1.0e-14)
            {
                sq_phibar_star_j1 = sq_phi_star_j1;
                var f_min = fac * Math.Pow(sq_phi_star_j1 / (sq_phi_star_j1 - sq_phi_star_j), pj);
                if (f_min < f_max)
                {
                    f_bar = f_min + f_min / f_max * (f_max - f_min);
                    var fp = Math.Pow(f_bar, -1 / pj);
                    sq_phibar_star_j = (2 * sq_phi_star_j + fp * sq_phi_star_j1) / (2 - fp);
                    adm_region = true;
                }
                else
                {
                    adm_region = false;
                }
            }

            // Positive values of both pj and qj
            if (pj >= 1.0e-14 && qj >= 1.0e-14)
            {
                var f_min = fac * (sq_phi_star_j + sq_phi_star_j1) / Math.Pow(sq_phi_star_j1 - sq_phi_star_j, Math.Max(pj, qj));
                if (f_min < f_max)
                {
                    f_min = Math.Max(f_min, 1.5);
                    f_bar = f_min + f_min / f_max * (f_max - f_min);
                    var fp = Math.Pow(f_bar, -1 / pj);
                    var fq = Math.Pow(f_bar, -1 / qj);
                    var w = (conservative_error_analysis)
                        ? -2 * phi_s_star_j1 * t / (log_epsilon - phi_s_star_j1 * t)
                        : -phi_s_star_j1 * t / log_epsilon;
                    var den = 2 + w - (1 + w) * fp + fq;
                    sq_phibar_star_j = ((2 + w + fq) * sq_phi_star_j + fp * sq_phi_star_j1) / den;
                    sq_phibar_star_j1 = (-(1 + w) * fq * sq_phi_star_j + (2 + w - (1 + w) * fp) * sq_phi_star_j1) / den;
                    adm_region = true;
                }
                else
                {
                    adm_region = false;
                }
            }

            var muj = 0d;
            var hj = 0d;
            var Nj = double.PositiveInfinity;
            if (adm_region)
            {
                log_epsilon = log_epsilon - Math.Log(f_bar);
                var w = (conservative_error_analysis)
                    ? -2 * Math.Pow(sq_phibar_star_j1, 2) * t / (log_epsilon - Math.Pow(sq_phibar_star_j1, 2) * t)
                    : -Math.Pow(sq_phibar_star_j1, 2) * t / log_epsilon;
                muj = Math.Pow((((1 + w) * sq_phibar_star_j + sq_phibar_star_j1) / (2 + w)), 2);
                hj = -Pi2 / log_epsilon * (sq_phibar_star_j1 - sq_phibar_star_j) / ((1 + w) * sq_phibar_star_j + sq_phibar_star_j1);
                Nj = Math.Ceiling(Math.Sqrt(1 - log_epsilon / t / muj) / hj);
            }

            return (muj, hj, Nj);
        }

        /// <summary>
        /// Finds optimal parameters in a right-unbounded region.
        /// </summary>
        private static (double muj, double hj, double Nj) OptimalParametersInRightUnboundedRegion(double t, double phi_s_star_j, double pj, double log_epsilon)
        {
            const double eps = 2.2204460492503130808E-16; // 2^(1 - 53) = 2.2204E-16
            const double Pi = 3.141592653589793238462643; // π
            const double Pi2 = 6.283185307179586476925287; // 2 π

            // Evaluation of the starting values for sq_phi_star_j
            var sq_phi_s_star_j = Math.Sqrt(phi_s_star_j);
            var phibar_star_j = (phi_s_star_j > 0) ? phi_s_star_j * 1.01 : 0.01;
            var sq_phibar_star_j = Math.Sqrt(phibar_star_j);

            // Definition of some constants
            const double f_min = 1d;
            const double f_max = 10d;
            const double f_tar = 5d;

            // Iterative process to look for fbar in [f_min, f_max]
            var A = 0d;
            var sq_muj = 0d;
            var Nj = 0d;
            bool stop = false;
            while (!stop)
            {
                var phi_t = phibar_star_j * t;
                var log_eps_phi_t = log_epsilon / phi_t;

                Nj = Math.Ceiling(phi_t / Pi * (1 - 3 * log_eps_phi_t / 2 + Math.Sqrt(1 - 2 * log_eps_phi_t)));
                A = Pi * Nj / phi_t;

                sq_muj = sq_phibar_star_j * Math.Abs(4 - A) / Math.Abs(7 - Math.Sqrt(1 + 12 * A));
                var fbar = Math.Pow((sq_phibar_star_j - sq_phi_s_star_j) / sq_muj, -pj);

                stop = (pj < 1.0e-14) || (f_min < fbar && fbar < f_max);
                if (!stop)
                {
                    sq_phibar_star_j = Math.Pow(f_tar, -1.0 / pj) * sq_muj + sq_phi_s_star_j;
                    phibar_star_j = Math.Pow(sq_phibar_star_j, 2);
                }
            }

            var muj = Math.Pow(sq_muj, 2);
            var hj = (-3 * A - 2 + 2 * Math.Sqrt(1 + 12 * A)) / (4 - A) / Nj;

            // Adjusting integration parameters to keep round-off errors under control
            var log_eps = Math.Log(eps);
            var threshold = (log_epsilon - log_eps) / t;
            if (muj > threshold)
            {
                var Q = (Math.Abs(pj) < 1.0e-14) ? 0 : Math.Pow(f_tar, -1 / pj) * Math.Sqrt(muj);
                phibar_star_j = Math.Pow(Q + Math.Sqrt(phi_s_star_j), 2);
                if (phibar_star_j < threshold)
                {
                    var w = Math.Sqrt(log_eps / (log_eps - log_epsilon));
                    var u = Math.Sqrt(-phibar_star_j * t / log_eps);
                    muj = threshold;
                    Nj = Math.Ceiling(w * log_epsilon / Pi2 / (u * w - 1));
                    hj = Math.Sqrt(log_eps / (log_eps - log_epsilon)) / Nj;
                }
                else
                {
                    Nj = double.PositiveInfinity;
                    hj = 0;
                }
            }

            return (muj, hj, Nj);
        }
    }
}
