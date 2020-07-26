// <copyright file="Iterator.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.LinearAlgebra.Solvers
{
    /// <summary>
    /// An iterator that is used to check if an iterative calculation should continue or stop.
    /// </summary>
    public sealed class Iterator<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// The collection that holds all the stop criteria and the flag indicating if they should be added
        /// to the child iterators.
        /// </summary>
        readonly List<IIterationStopCriterion<T>> _stopCriteria;

        /// <summary>
        /// The status of the iterator.
        /// </summary>
        IterationStatus _status = IterationStatus.Continue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator{T}"/> class with the default stop criteria.
        /// </summary>
        public Iterator()
        {
            _stopCriteria = new List<IIterationStopCriterion<T>>(Matrix<T>.Build.IterativeSolverStopCriteria());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator{T}"/> class with the specified stop criteria.
        /// </summary>
        /// <param name="stopCriteria">
        /// The specified stop criteria. Only one stop criterion of each type can be passed in. None
        /// of the stop criteria will be passed on to child iterators.
        /// </param>
        public Iterator(params IIterationStopCriterion<T>[] stopCriteria)
        {
            _stopCriteria = new List<IIterationStopCriterion<T>>(stopCriteria);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator{T}"/> class with the specified stop criteria.
        /// </summary>
        /// <param name="stopCriteria">
        /// The specified stop criteria. Only one stop criterion of each type can be passed in. None
        /// of the stop criteria will be passed on to child iterators.
        /// </param>
        public Iterator(IEnumerable<IIterationStopCriterion<T>> stopCriteria)
        {
            _stopCriteria = new List<IIterationStopCriterion<T>>(stopCriteria);
        }

        /// <summary>
        /// Gets the current calculation status.
        /// </summary>
        public IterationStatus Status => _status;

        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current <see cref="Iterator{T}"/>. Result is set into <c>Status</c> field.
        /// </summary>
        /// <param name="iterationNumber">The number of iterations that have passed so far.</param>
        /// <param name="solutionVector">The vector containing the current solution values.</param>
        /// <param name="sourceVector">The right hand side vector.</param>
        /// <param name="residualVector">The vector containing the current residual vectors.</param>
        /// <remarks>
        /// The individual iterators may internally track the progress of the calculation based
        /// on the invocation of this method. Therefore this method should only be called if the
        /// calculation has moved forwards at least one step.
        /// </remarks>
        public IterationStatus DetermineStatus(int iterationNumber, Vector<T> solutionVector, Vector<T> sourceVector, Vector<T> residualVector)
        {
            if (_stopCriteria.Count == 0)
            {
                throw new ArgumentException("There is no stop criterion in the collection.");
            }

            if (iterationNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(iterationNumber));
            }

            // While we're cancelled we don't call on the stop-criteria.
            if (_status == IterationStatus.Cancelled)
            {
                return _status;
            }

            foreach (var stopCriterion in _stopCriteria)
            {
                var status = stopCriterion.DetermineStatus(iterationNumber, solutionVector, sourceVector, residualVector);
                if (status == IterationStatus.Continue)
                {
                    continue;
                }

                _status = status;
                return _status;
            }

            // Got all the way through
            // So we're running because we had vectors passed to us.
            _status = IterationStatus.Continue;

            return _status;
        }

        /// <summary>
        /// Indicates to the iterator that the iterative process has been cancelled.
        /// </summary>
        /// <remarks>
        /// Does not reset the stop-criteria.
        /// </remarks>
        public void Cancel()
        {
            _status = IterationStatus.Cancelled;
        }

        /// <summary>
        /// Resets the <see cref="Iterator{T}"/> to the pre-calculation state.
        /// </summary>
        public void Reset()
        {
            _status = IterationStatus.Continue;

            foreach (var stopCriterion in _stopCriteria)
            {
                stopCriterion.Reset();
            }
        }

        /// <summary>
        /// Creates a deep clone of the current iterator.
        /// </summary>
        /// <returns>The deep clone of the current iterator.</returns>
        public Iterator<T> Clone()
        {
            return new Iterator<T>(_stopCriteria.Select(sc => sc.Clone()));
        }
    }
}
