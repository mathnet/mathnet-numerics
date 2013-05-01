// <copyright file="Bracketing.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// 
// Copyright (c) 2009-2013 Math.NET
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

            for (int i = 0; i < maxIterations; i++)
            {
                if (Math.Sign(fmin) != Math.Sign(fmax))
                {
                    return true;
                }

                if (Math.Abs(fmin) < Math.Abs(fmax))
                {
                    xmin += factor*(xmin - xmax);
                    fmin = f(xmin);
                }
                else
                {
                    xmax += factor*(xmax - xmin);
                    fmax = f(xmax);
                }
            }

            return false;
        }
    }
}
