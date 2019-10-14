using System;

namespace MathNet.Numerics
{
    public static class DifferIntegrate
    {
        /// <summary>
        /// Evaluates the Riemann-Liouville fractional derivative that uses the double exponential integration.
        /// </summary>
        /// <remarks>
        /// <para>order = 1.0 : normal derivative</para>
        /// <para>order = 0.5 : semi-derivative</para>
        /// <para>order = -0.5 : semi-integral</para>
        /// <para>order = -1.0 : normal integral</para>
        /// </remarks>
        /// <param name="f">The analytic smooth function to differintegrate.</param>
        /// <param name="x">The evaluation point.</param>
        /// <param name="order">The order of fractional derivative.</param>
        /// <param name="x0">The reference point of integration.</param>
        /// <param name="targetAbsoluteError">The expected relative accuracy of the Double-Exponential integration.</param>
        /// <returns>Approximation of the differintegral of order n at x.</returns>
        public static double DoubleExponential(Func<double, double> f, double x, double order, double x0 = 0, double targetAbsoluteError = 1E-10)
        {
            // The Riemann–Liouville fractional derivative of f(x) of order n is defined as
            //    \,_{x_0}{\mathbb{D}}^n_xf(x) = \frac{1}{\Gamma(m-n)} \frac{d^m}{dx^m} \int_{x_0}^{x} (x-t)^{m-n-1} f(t) dt
            //    where m is the smallest interger greater than n.
            // see https://en.wikipedia.org/wiki/Differintegral

            if (Math.Abs(order) < double.Epsilon)
            {
                return f(x);
            }
            else if (order > 0 && Math.Abs(order - (int)order) < double.Epsilon)
            {
                return Differentiate.Derivative(f, x, (int)order);
            }
            else
            {
                int m = (int)Math.Ceiling(order) + 1;
                if (m < 1) m = 1;
                double r = m - order - 1;
                Func<double, double> g = (v) => Integrate.DoubleExponential((t) => Math.Pow(v - t, r) * f(t), x0, v, targetAbsoluteError: targetAbsoluteError);
                double numerator = Differentiate.Derivative(g, x, m);
                double denominator = SpecialFunctions.Gamma(m - order);
                return numerator / denominator;
            }
        }

        /// <summary>
        /// Evaluates the Riemann-Liouville fractional derivative that uses the Gauss-Legendre integration.
        /// </summary>
        /// <remarks>
        /// <para>order = 1.0 : normal derivative</para>
        /// <para>order = 0.5 : semi-derivative</para>
        /// <para>order = -0.5 : semi-integral</para>
        /// <para>order = -1.0 : normal integral</para>
        /// </remarks>
        /// <param name="f">The analytic smooth function to differintegrate.</param>
        /// <param name="x">The evaluation point.</param>
        /// <param name="order">The order of fractional derivative.</param>
        /// <param name="x0">The reference point of integration.</param>
        /// <param name="gaussLegendrePoints">The number of Gauss-Legendre points.</param>
        /// <returns>Approximation of the differintegral of order n at x.</returns>
        public static double GaussLegendre(Func<double, double> f, double x, double order, double x0 = 0, int gaussLegendrePoints = 128)
        {
            // The Riemann–Liouville fractional derivative of f(x) of order n is defined as
            //    \,_{x_0}{\mathbb{D}}^n_xf(x) = \frac{1}{\Gamma(m-n)} \frac{d^m}{dx^m} \int_{x_0}^{x} (x-t)^{m-n-1} f(t) dt
            //    where m is the smallest interger greater than n.
            // see https://en.wikipedia.org/wiki/Differintegral

            if (Math.Abs(order) < double.Epsilon)
            {
                return f(x);
            }
            else if (order > 0 && Math.Abs(order - (int)order) < double.Epsilon) 
            {
                return Differentiate.Derivative(f, x, (int)order);
            }
            else
            {
                int m = (int)Math.Ceiling(order) + 1;
                if (m < 1) m = 1;
                double r = m - order - 1;
                Func<double, double> g = (v) => Integrate.GaussLegendre((t) => Math.Pow(v - t, r) * f(t), x0, v, order: gaussLegendrePoints);
                double numerator = Differentiate.Derivative(g, x, m);
                double denominator = SpecialFunctions.Gamma(m - order);
                return numerator / denominator;
            }
        }

        /// <summary>
        /// Evaluates the Riemann-Liouville fractional derivative that uses the Gauss-Kronrod integration.
        /// </summary>
        /// <remarks>
        /// <para>order = 1.0 : normal derivative</para>
        /// <para>order = 0.5 : semi-derivative</para>
        /// <para>order = -0.5 : semi-integral</para>
        /// <para>order = -1.0 : normal integral</para>
        /// </remarks>
        /// <param name="f">The analytic smooth function to differintegrate.</param>
        /// <param name="x">The evaluation point.</param>
        /// <param name="order">The order of fractional derivative.</param>
        /// <param name="x0">The reference point of integration.</param>
        /// <param name="targetRelativeError">The expected relative accuracy of the Gauss-Kronrod integration.</param>
        /// <param name="gaussKronrodPoints">The number of Gauss-Kronrod points. Pre-computed for 15, 21, 31, 41, 51 and 61 points.</param>
        /// <returns>Approximation of the differintegral of order n at x.</returns>
        public static double GaussKronrod(Func<double, double> f, double x, double order, double x0 = 0, double targetRelativeError = 1E-10, int gaussKronrodPoints = 15)
        {
            // The Riemann–Liouville fractional derivative of f(x) of order n is defined as
            //    \,_{x_0}{\mathbb{D}}^n_xf(x) = \frac{1}{\Gamma(m-n)} \frac{d^m}{dx^m} \int_{x_0}^{x} (x-t)^{m-n-1} f(t) dt
            //    where m is the smallest interger greater than n.
            // see https://en.wikipedia.org/wiki/Differintegral

            if (Math.Abs(order) < double.Epsilon)
            {
                return f(x);
            }
            else if (order > 0 && Math.Abs(order - (int)order) < double.Epsilon) 
            {
                return Differentiate.Derivative(f, x, (int)order);
            }
            else
            {
                int m = (int)Math.Ceiling(order) + 1;
                if (m < 1) m = 1;
                double r = m - order - 1;
                Func<double, double> g = (v) => Integrate.GaussKronrod((t) => Math.Pow(v - t, r) * f(t), x0, v, targetRelativeError: targetRelativeError, order: gaussKronrodPoints);
                double numerator = Differentiate.Derivative(g, x, m);
                double denominator = SpecialFunctions.Gamma(m - order);
                return numerator / denominator;
            }
        }
    }
}
