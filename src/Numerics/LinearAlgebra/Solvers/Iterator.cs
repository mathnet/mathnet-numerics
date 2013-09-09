// <copyright file="Iterator.cs" company="Math.NET">
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
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Solvers.Status;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Solvers
{
    /// <summary>
    /// An iterator that is used to check if an iterative calculation should continue or stop.
    /// </summary>
    public sealed class Iterator<T> : IIterator<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// The default status for the iterator.
        /// </summary>
        static readonly ICalculationStatus DefaultStatus = new CalculationIndetermined();

        /// <summary>
        /// The collection that holds all the stop criteria and the flag indicating if they should be added
        /// to the child iterators.
        /// </summary>
        readonly Dictionary<Type, IIterationStopCriterium<T>> _stopCriterias = new Dictionary<Type, IIterationStopCriterium<T>>();

        /// <summary>
        /// The status of the iterator.
        /// </summary>
        ICalculationStatus _status = DefaultStatus;

        /// <summary>
        /// Indicates if the iteration was canceled.
        /// </summary>
        bool _wasIterationCancelled;

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator{T}"/> class.
        /// </summary>
        public Iterator() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator{T}"/> class with the specified stop criteria.
        /// </summary>
        /// <param name="stopCriteria">
        /// The specified stop criteria. Only one stop criterium of each type can be passed in. None
        /// of the stop criteria will be passed on to child iterators.
        /// </param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="stopCriteria"/> contains multiple stop criteria of the same type.</exception>
        public Iterator(IEnumerable<IIterationStopCriterium<T>> stopCriteria)
        {
            // Add the stop criteria
            if (stopCriteria == null)
            {
                return;
            }

            foreach (var stopCriterium in stopCriteria.Where(stopCriterium => stopCriterium != null))
            {
                Add(stopCriterium);
            }
        }

        /// <summary>
        /// Adds an <see cref="IIterationStopCriterium{T}"/> to the internal collection of stop-criteria. Only a 
        /// single stop criterium of each type can be stored.
        /// </summary>
        /// <param name="stopCriterium">The stop criterium to add.</param>
        public void Add(IIterationStopCriterium<T> stopCriterium)
        {
            _stopCriterias[stopCriterium.GetType()] = stopCriterium;
        }

        /// <summary>
        /// Removes the <see cref="IIterationStopCriterium{T}"/> from the internal collection.
        /// </summary>
        /// <param name="stopCriterium">The stop criterium that must be removed.</param>
        public void Remove(IIterationStopCriterium<T> stopCriterium)
        {
            _stopCriterias.Remove(stopCriterium.GetType());
        }

        /// <summary>
        /// Indicates if the specific stop criterium is stored by the <see cref="IIterator{T}"/>.
        /// </summary>
        /// <param name="stopCriterium">The stop criterium.</param>
        /// <returns><c>true</c> if the <see cref="IIterator{T}"/> contains the stop criterium; otherwise <c>false</c>.</returns>
        public bool Contains(IIterationStopCriterium<T> stopCriterium)
        {
            return _stopCriterias.ContainsKey(stopCriterium.GetType());
        }

        /// <summary>
        /// Gets the number of stored stop criteria.
        /// </summary>
        /// <remarks>Used for testing only.</remarks>
        internal int NumberOfCriteria
        {
            get { return _stopCriterias.Count; }
        }

        /// <summary>
        /// Gets an <c>IEnumerator</c> that enumerates over all the stored stop criteria.
        /// </summary>
        /// <remarks>Used for testing only.</remarks>
        internal IEnumerable<IIterationStopCriterium<T>> StoredStopCriteria
        {
            get { return _stopCriterias.Select(criterium => criterium.Value); }
        }

        /// <summary>
        /// Indicates to the iterator that the iterative process has been cancelled.
        /// </summary>
        /// <remarks>
        /// Does not reset the stop-criteria.
        /// </remarks>
        public void IterationCancelled()
        {
            _wasIterationCancelled = true;
            _status = new CalculationCancelled();
        }

        /// <summary>
        /// Determines the status of the iterative calculation based on the stop criteria stored
        /// by the current <see cref="IIterator{T}"/>. Result is set into <c>Status</c> field.
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
        public void DetermineStatus(int iterationNumber, Vector<T> solutionVector, Vector<T> sourceVector, Vector<T> residualVector)
        {
            if (_stopCriterias.Count == 0)
            {
                throw new ArgumentException(Resources.StopCriteriumMissing);
            }

            if (iterationNumber < 0)
            {
                throw new ArgumentOutOfRangeException("iterationNumber");
            }

            // While we're cancelled we don't call on the stop-criteria.
            if (_wasIterationCancelled)
            {
                return;
            }

            foreach (var stopCriterium in _stopCriterias.Select(pair => pair.Value))
            {
                stopCriterium.DetermineStatus(iterationNumber, solutionVector, sourceVector, residualVector);
                var status = stopCriterium.Status;

                // Check if the status is:
                // - Running --> keep going
                // - Indetermined --> keep going
                // Anything else:
                // Stop looping and set that status
                if ((status is CalculationRunning) || (status is CalculationIndetermined))
                {
                    continue;
                }

                _status = status;
                return;
            }

            // Got all the way through
            // So we're running because we had vectors passed to us.
            if (!(_status is CalculationRunning))
            {
                _status = new CalculationRunning();
            }
        }

        /// <summary>
        /// Gets the current calculation status.
        /// </summary>
        public ICalculationStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Resets the <see cref="IIterator{T}"/> to the pre-calculation state.
        /// </summary>
        public void ResetToPrecalculationState()
        {
            // Indicate that we're no longer cancelled.
            _wasIterationCancelled = false;

            // Reset the status.
            _status = DefaultStatus;

            // Reset the stop-criteria
            foreach (var stopCriterium in _stopCriterias.Select(pair => pair.Value))
            {
                stopCriterium.ResetToPrecalculationState();
            }
        }

        /// <summary>
        /// Creates a deep clone of the current iterator.
        /// </summary>
        /// <returns>The deep clone of the current iterator.</returns>
        public IIterator<T> Clone()
        {
            var stopCriteria = _stopCriterias.Select(pair => pair.Value).Select(stopCriterium => stopCriterium.Clone()).ToList();
            return new Iterator<T>(stopCriteria);
        }
    }
}
