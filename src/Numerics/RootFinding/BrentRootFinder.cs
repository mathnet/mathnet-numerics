using System;

namespace MathNet.Numerics.RootFinding
{
	public class BrentRootFinder : RootFinder
	{
        public BrentRootFinder() : base()
        {
        }
        public BrentRootFinder(int numIters, double accuracy) : base(numIters, accuracy)
        {
        }

        protected override double Find()
        {
            /* The implementation of the algorithm was inspired by
               Press, Teukolsky, Vetterling, and Flannery,
               "Numerical Recipes in C", 2nd edition, Cambridge
               University Press
            */

            double min1, min2;
            double p, q, r, s, xAcc1, xMid = 0;
            double d = 0.0, e = 0.0;

            // set up
            double xmin = XMin;
            double fxmin = Func(XMin);
            double xmax = XMax;
            double fxmax = Func(XMax);

            double root = xmax;
            double froot = fxmax;

            // solve
            int i = 0;
            for (; i <= Iterations; i++)
            {
                if (Math.Sign(froot) == Math.Sign(fxmax))
                {
                    // Rename xMin_, root_, xMax_ and adjust bounds
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
                // Convergence check
                xAcc1 = 2.0 * DOUBLE_ACCURACY * Math.Abs(root) + 0.5 * Accuracy;
                xMid = (xmax - root) / 2.0;
                if (Math.Abs(xMid) <= xAcc1 || Close(froot, 0.0))
                {
                    return root;
                }
                if (Math.Abs(e) >= xAcc1 &&
                    Math.Abs(fxmin) > Math.Abs(froot))
                {

                    // Attempt inverse quadratic interpolation
                    s = froot / fxmin;
                    if (Close(xmin, xmax))
                    {
                        p = 2.0 * xMid * s;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = fxmin / fxmax;
                        r = froot / fxmax;
                        p = s * (2.0 * xMid * q * (q - r) - (root - xmin) * (r - 1.0));
                        q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                    }
                    if (p > 0.0) q = -q;  // Check whether in bounds
                    p = Math.Abs(p);
                    min1 = 3.0 * xMid * q - Math.Abs(xAcc1 * q);
                    min2 = Math.Abs(e * q);
                    if (2.0 * p < Math.Min(min1, min2))
                    {
                        e = d;                // Accept interpolation
                        d = p / q;
                    }
                    else
                    {
                        d = xMid;  // Interpolation failed, use bisection
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
                    root += d;
                else
                    root += Sign(xAcc1, xMid);
                froot = Func(root);
            }

            // The algorithm has exceeded the number of iterations allowed
            throw new RootFinderException(ACCURACY_NOT_REACHED, i, new Range(XMin, XMax), Math.Abs(xMid));
        }
	}
}
