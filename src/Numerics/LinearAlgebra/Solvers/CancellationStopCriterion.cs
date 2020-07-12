// <copyright file="IterationCountStopCriterion.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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
using System.Diagnostics;
using System.Threading;

namespace MathNet.Numerics.LinearAlgebra.Solvers
{
    /// <summary>
    /// Defines an <see cref="IIterationStopCriterion{T}"/> that uses a cancellation token as stop criterion.
    /// </summary>
    public sealed class CancellationStopCriterion<T> : IIterationStopCriterion<T> where T : struct, IEquatable<T>, IFormattable
    {
        readonly CancellationToken _masterToken;
        CancellationTokenSource _currentTcs;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationCountStopCriterion{T}"/> class.
        /// </summary>
        public CancellationStopCriterion()
        {
            _masterToken = CancellationToken.None;
            _currentTcs = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationCountStopCriterion{T}"/> class.
        /// </summary>
        public CancellationStopCriterion(CancellationToken masterToken)
        {
            _masterToken = masterToken;
            _currentTcs = CancellationTokenSource.CreateLinkedTokenSource(masterToken);
        }

        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current <see cref="IterationCountStopCriterion{T}"/>. Result is set into <c>Status</c> field.
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
        public IterationStatus DetermineStatus(int iterationNumber, Vector<T> solutionVector, Vector<T> sourceVector, Vector<T> residualVector)
        {
            return _currentTcs.Token.IsCancellationRequested ? IterationStatus.Cancelled : IterationStatus.Continue;
        }

        /// <summary>
        /// Gets the current calculation status.
        /// </summary>
        public IterationStatus Status
        {
            [DebuggerStepThrough]
            get => _currentTcs.Token.IsCancellationRequested ? IterationStatus.Cancelled : IterationStatus.Continue;
        }

        public void Cancel()
        {
            _currentTcs.Cancel();
        }

        /// <summary>
        /// Resets the <see cref="IterationCountStopCriterion{T}"/> to the pre-calculation state.
        /// </summary>
        public void Reset()
        {
            _currentTcs = CancellationTokenSource.CreateLinkedTokenSource(_masterToken);
        }

        /// <summary>
        /// Clones the current <see cref="IterationCountStopCriterion{T}"/> and its settings.
        /// </summary>
        /// <returns>A new instance of the <see cref="IterationCountStopCriterion{T}"/> class.</returns>
        public IIterationStopCriterion<T> Clone()
        {
            return new CancellationStopCriterion<T>(_masterToken);
        }
    }
}
