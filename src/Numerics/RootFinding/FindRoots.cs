using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.RootFinding
{
	public static class FindRoots
	{
	    /// <summary>Find a solution of the equation f(x)=0.</summary>
	    /// <param name="f">The function to find roots from.</param>
	    /// <param name="xmin">The low value of the range where the root is supposed to be.</param>
	    /// <param name="xmax">The high value of the range where the root is supposed to be.</param>
	    /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached.</param>
	    /// <param name="maxIterations">Maximum number of iterations. Usually 100.</param>
	    /// <returns>Returns the root with the specified accuracy.</returns>
        /// <remarks>
        /// Algorithm by by Brent, Van Wijngaarden, Dekker et al.
        /// Implementation inspired by Press, Teukolsky, Vetterling, and Flannery, "Numerical Recipes in C", 2nd edition, Cambridge University Press
        /// </remarks>
	    public static double BrentMethod(Func<double, double> f, double xmin, double xmax, double accuracy = 1e-8, int maxIterations = 100)
        {
	        double xMid = 0;
	        double d = 0.0, e = 0.0;

            double fxmin = f(xmin);
            double fxmax = f(xmax);
            double root = xmax;
            double froot = fxmax;

            for (int i = 0; i <= maxIterations; i++)
            {
                // adjust bounds
                if (Math.Sign(froot) == Math.Sign(fxmax))
                {
                    xmax = xmin;
                    fxmax = fxmin;
                    e = d = root - xmin;
                }

                if (Math.Abs(fxmax) < Math.Abs(froot))
                {
                    xmin = root;
                    root = xmax;
                    xmax = xmin;
                    fxmin = froot;
                    froot = fxmax;
                    fxmax = fxmin;
                }

                // convergence check
                double xAcc1 = 2.0 * Precision.DoubleMachinePrecision * Math.Abs(root) + 0.5 * accuracy;
                xMid = (xmax - root) / 2.0;
                if (Math.Abs(xMid) <= xAcc1 || froot.AlmostEqualWithAbsoluteError(0, froot, accuracy))
                {
                    return root;
                }

                if (Math.Abs(e) >= xAcc1 && Math.Abs(fxmin) > Math.Abs(froot))
                {
                    // Attempt inverse quadratic interpolation
                    double s = froot / fxmin;
                    double p;
                    double q;
                    if (xmin.AlmostEqual(xmax))
                    {
                        p = 2.0 * xMid * s;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = fxmin / fxmax;
                        double r = froot / fxmax;
                        p = s * (2.0 * xMid * q * (q - r) - (root - xmin) * (r - 1.0));
                        q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                    }

                    if (p > 0.0)
                    {
                        // Check whether in bounds
                        q = -q;
                    }
                    p = Math.Abs(p);
                    if (2.0 * p < Math.Min(3.0 * xMid * q - Math.Abs(xAcc1 * q), Math.Abs(e * q)))
                    {
                        // Accept interpolation
                        e = d;
                        d = p / q;
                    }
                    else
                    {
                        // Interpolation failed, use bisection
                        d = xMid;
                        e = d;
                    }
                }
                else
                {
                    // Bounds decreasing too slowly, use bisection
                    d = xMid;
                    e = d;
                }
                xmin = root;
                fxmin = froot;
                if (Math.Abs(d) > xAcc1)
                {
                    root += d;
                }
                else
                {
                    root += Sign(xAcc1, xMid);
                }
                froot = f(root);
            }

            // The algorithm has exceeded the number of iterations allowed
            throw new RootFindingException(Resources.AccuracyNotReached, maxIterations, xmin, xmax, Math.Abs(xMid));
        }

        /// <summary>Helper method useful for preventing rounding errors.</summary>
        /// <returns>a*sign(b)</returns>
        static double Sign(double a, double b)
        {
            return b >= 0 ? (a >= 0 ? a : -a) : (a >= 0 ? -a : a);
        }
	}
}
