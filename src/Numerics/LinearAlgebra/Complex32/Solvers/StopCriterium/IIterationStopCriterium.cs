// <copyright file="IIterationStopCriterium.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.LinearAlgebra.Complex32.Solvers.StopCriterium
{
    using System;
    using Generic.Solvers.Status;
    using Generic.Solvers.StopCriterium;

    /// <summary>
    /// The base interface for classes that provide stop criteria for iterative calculations. 
    /// </summary>
    public interface IIterationStopCriterium
#if !PORTABLE
 : ICloneable
#endif
    {
        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current <see cref="IIterationStopCriterium"/>. Status is set to <c>Status</c> field of current object.
        /// </summary>
        /// <param name="iterationNumber">The number of iterations that have passed so far.</param>
        /// <param name="solutionVector">The vector containing the current solution values.</param>
        /// <param name="sourceVector">The right hand side vector.</param>
        /// <param name="residualVector">The vector containing the current residual vectors.</param>
        /// <remarks>
        /// The individual stop criteria may internally track the progress of the calculation based
        /// on the invocation of this method. Therefore this method should only be called if the 
        /// calculation has moved forwards at least one step.
        /// </remarks>
        void DetermineStatus(int iterationNumber, Vector solutionVector, Vector sourceVector, Vector residualVector);

        /// <summary>
        /// Gets the current calculation status.
        /// </summary>
        /// <remarks><see langword="null" /> is not a legal value. Status should be set in <see cref="DetermineStatus"/> implementation.</remarks>
        ICalculationStatus Status { get; }

        /// <summary>
        /// Resets the <see cref="IIterationStopCriterium"/> to the pre-calculation state.
        /// </summary>
        /// <remarks>To implementers: Invoking this method should not clear the user defined
        /// property values, only the state that is used to track the progress of the 
        /// calculation.</remarks>
        void ResetToPrecalculationState();

        /// <summary>
        /// Gets the <see cref="StopLevel"/> which indicates what sort of stop criterium this
        /// <see cref="IIterationStopCriterium"/> monitors.
        /// </summary>
        StopLevel StopLevel { get; }

#if PORTABLE
        IIterationStopCriterium Clone();
#endif
    }
}
