// <copyright file="IteratorTest.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Solvers.StopCriterium;
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers.Status;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers
{
    /// <summary>
    /// Iterator tests
    /// </summary>
    [TestFixture]
    public class IteratorTest
    {
        /// <summary>
        /// Determine status without stop criteria throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithoutStopCriteriaThrowsArgumentException()
        {
            var iterator = new Iterator<double>();
            Assert.Throws<ArgumentException>(() => iterator.DetermineStatus(
                0,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 5),
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Determine status with negative iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNegativeIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criteria = new List<IIterationStopCriterium<double>>
                {
                    new FailureStopCriterium(),
                    new DivergenceStopCriterium(),
                    new IterationCountStopCriterium<double>(),
                    new ResidualStopCriterium()
                };
            var iterator = new Iterator<double>(criteria);

            Assert.Throws<ArgumentOutOfRangeException>(() => iterator.DetermineStatus(
                -1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 5),
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Determine status with <c>null</c> solution vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNullSolutionVectorThrowsArgumentNullException()
        {
            var criteria = new List<IIterationStopCriterium<double>>
                {
                    new FailureStopCriterium(),
                    new DivergenceStopCriterium(),
                    new IterationCountStopCriterium<double>(),
                    new ResidualStopCriterium()
                };
            var iterator = new Iterator<double>(criteria);

            Assert.Throws<ArgumentNullException>(() => iterator.DetermineStatus(
                1,
                null,
                DenseVector.Create(3, i => 5),
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Determine status with <c>null</c> source vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNullSourceVectorThrowsArgumentNullException()
        {
            var criteria = new List<IIterationStopCriterium<double>>
                {
                    new FailureStopCriterium(),
                    new DivergenceStopCriterium(),
                    new IterationCountStopCriterium<double>(),
                    new ResidualStopCriterium()
                };
            var iterator = new Iterator<double>(criteria);

            Assert.Throws<ArgumentNullException>(() => iterator.DetermineStatus(
                1,
                DenseVector.Create(3, i => 5),
                null,
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Determine status with <c>null</c> residual vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNullResidualVectorThrowsArgumentNullException()
        {
            var criteria = new List<IIterationStopCriterium<double>>
                {
                    new FailureStopCriterium(),
                    new DivergenceStopCriterium(),
                    new IterationCountStopCriterium<double>(),
                    new ResidualStopCriterium()
                };
            var iterator = new Iterator<double>(criteria);

            Assert.Throws<ArgumentNullException>(() => iterator.DetermineStatus(
                1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 5),
                null));
        }

        /// <summary>
        /// Can determine status.
        /// </summary>
        [Test]
        public void DetermineStatus()
        {
            var criteria = new List<IIterationStopCriterium<double>>
                {
                    new FailureStopCriterium(),
                    new DivergenceStopCriterium(),
                    new IterationCountStopCriterium<double>(1)
                };

            var iterator = new Iterator<double>(criteria);

            // First step, nothing should happen.
            iterator.DetermineStatus(
                0,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4));
            Assert.IsInstanceOf(typeof (CalculationRunning), iterator.Status, "Incorrect status");

            // Second step, should run out of iterations.
            iterator.DetermineStatus(
                1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4));
            Assert.IsInstanceOf(typeof (CalculationStoppedWithoutConvergence), iterator.Status, "Incorrect status");
        }

        /// <summary>
        /// Can reset to precalculation state.
        /// </summary>
        [Test]
        public void ResetToPrecalculationState()
        {
            var criteria = new List<IIterationStopCriterium<double>>
                {
                    new FailureStopCriterium(),
                    new DivergenceStopCriterium(),
                    new IterationCountStopCriterium<double>(1)
                };

            var iterator = new Iterator<double>(criteria);

            // First step, nothing should happen.
            iterator.DetermineStatus(
                0,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4));
            Assert.IsInstanceOf(typeof (CalculationRunning), iterator.Status, "Incorrect status");

            iterator.Reset();
            Assert.IsInstanceOf(typeof (CalculationIndetermined), iterator.Status, "Incorrect status");
            Assert.IsInstanceOf(typeof (CalculationIndetermined), criteria[0].Status, "Incorrect status");
            Assert.IsInstanceOf(typeof (CalculationIndetermined), criteria[1].Status, "Incorrect status");
            Assert.IsInstanceOf(typeof (CalculationIndetermined), criteria[2].Status, "Incorrect status");
        }
    }
}
