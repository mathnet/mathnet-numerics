// <copyright file="ILeastSquaresMinimizer.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Interface for solving nonlinear least squares problems.
    /// This interface unifies the Levenberg-Marquardt and Trust Region minimization algorithms.
    /// </summary>
    public interface ILeastSquaresMinimizer
    {
        /// <summary>
        /// Finds the minimum of a nonlinear least squares problem using the specified objective model and initial guess vector.
        /// </summary>
        /// <param name="objective">The objective function model to be minimized.</param>
        /// <param name="initialGuess">The initial guess vector.</param>
        /// <param name="lowerBound">Optional lower bound for the parameters.</param>
        /// <param name="upperBound">Optional upper bound for the parameters.</param>
        /// <param name="scales">Optional scales for the parameters.</param>
        /// <param name="isFixed">Optional list indicating which parameters are fixed.</param>
        /// <returns>A <see cref="NonlinearMinimizationResult"/> containing the results of the minimization.</returns>
        NonlinearMinimizationResult FindMinimum(IObjectiveModel objective, Vector<double> initialGuess,
            Vector<double> lowerBound = null, Vector<double> upperBound = null, Vector<double> scales = null, List<bool> isFixed = null);

        /// <summary>
        /// Finds the minimum of a nonlinear least squares problem using the specified objective model and initial guess array.
        /// </summary>
        /// <param name="objective">The objective function model to be minimized.</param>
        /// <param name="initialGuess">The initial guess array.</param>
        /// <param name="lowerBound">Optional lower bound array for the parameters.</param>
        /// <param name="upperBound">Optional upper bound array for the parameters.</param>
        /// <param name="scales">Optional scales array for the parameters.</param>
        /// <param name="isFixed">Optional array indicating which parameters are fixed.</param>
        /// <returns>A <see cref="NonlinearMinimizationResult"/> containing the results of the minimization.</returns>
        NonlinearMinimizationResult FindMinimum(IObjectiveModel objective, double[] initialGuess,
            double[] lowerBound = null, double[] upperBound = null, double[] scales = null, bool[] isFixed = null);
    }
}
