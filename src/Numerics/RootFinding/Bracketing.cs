using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.RootFinding
{
    public static class Bracketing
    {
        /// <summary>Detect a range containing at least one root.</summary>
        /// <param name="f">The function to detect roots from.</param>
        /// <param name="xmin">Lower value of the range.</param>
        /// <param name="xmax">Upper value of the range</param>
        /// <param name="factor">The growing factor of research. Usually 1.6.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 50.</param>
        /// <returns>True if the bracketing operation succeeded, false otherwise.</returns>
        /// <remarks>This iterative methods stops when two values with opposite signs are found.</remarks>
        public static bool SearchOutward(Func<double, double> f, ref double xmin, ref double xmax, double factor = 1.6, int maxIterations = 50)
        {
            if (xmin >= xmax)
            {
                throw new ArgumentOutOfRangeException("xmax", string.Format(Resources.ArgumentOutOfRangeGreater, "xmax", "xmin"));
            }

            double fmin = f(xmin);
            double fmax = f(xmax);

            for(int i=0;i<maxIterations; i++)
            {
                if (Math.Sign(fmin) != Math.Sign(fmax))
                {
                    return true;
                }

                if (Math.Abs(fmin) < Math.Abs(fmax))
                {
                    xmin += factor * (xmin - xmax);
                    fmin = f(xmin);
                }
                else
                {
                    xmax += factor * (xmax - xmin);
                    fmax = f(xmax);
                }
            }

            return false;
        }
    }
}
