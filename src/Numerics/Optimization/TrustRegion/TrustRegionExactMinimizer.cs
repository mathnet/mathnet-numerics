// <copyright file="TrustRegionExactMinimizer.cs" company="Math.NET">
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

namespace MathNet.Numerics.Optimization.TrustRegion
{
    /// <summary>
    /// Implements a trust region minimizer using the Exact subproblem solver with eigenvalue decomposition.
    /// </summary>
    /// <remarks>
    /// The Exact method provides the most accurate solution to the trust region subproblem but is more
    /// computationally expensive. It's particularly useful when high accuracy is required or when
    /// the Hessian has negative eigenvalues.
    /// </remarks>
    public sealed class TrustRegionExactMinimizer : TrustRegionMinimizerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrustRegionExactMinimizer"/> class.
        /// </summary>
        /// <param name="gradientTolerance">The gradient tolerance used to determine convergence.</param>
        /// <param name="stepTolerance">The step size tolerance used to determine convergence.</param>
        /// <param name="functionTolerance">The function value tolerance used to determine convergence.</param>
        /// <param name="radiusTolerance">The trust region radius tolerance used to determine convergence.</param>
        /// <param name="maximumIterations">The maximum number of iterations. -1 means no limit.</param>
        public TrustRegionExactMinimizer(double gradientTolerance = 1E-8, double stepTolerance = 1E-8, double functionTolerance = 1E-8, double radiusTolerance = 1E-8, int maximumIterations = -1)
            : base(TrustRegionSubproblem.Exact(), gradientTolerance, stepTolerance, functionTolerance, radiusTolerance, maximumIterations)
        { }
    }
}
