// <copyright file="ResidualStopCriteriumTest.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.StopCriterium
{
    using System;
    using LinearAlgebra.Double;
    using LinearAlgebra.Double.Solvers.StopCriterium;
    using LinearAlgebra.Generic.Solvers.Status;
    using NUnit.Framework;

    /// <summary>
    /// Residual stop criterium tests.
    /// </summary>
    [TestFixture]
    public sealed class ResidualStopCriteriumTest
    {
        /// <summary>
        /// Create with negative maximum throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithNegativeMaximumThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResidualStopCriterium(-0.1));
        }

        /// <summary>
        /// Create with illegal minimum iterations throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithIllegalMinimumIterationsThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResidualStopCriterium(-1));
        }

        /// <summary>
        /// Can create.
        /// </summary>
        [Test]
        public void Create()
        {
            var criterium = new ResidualStopCriterium(1e-8, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.AreEqual(1e-8, criterium.Maximum, "Incorrect maximum");
            Assert.AreEqual(50, criterium.MinimumIterationsBelowMaximum, "Incorrect iteration count");
        }

        /// <summary>
        /// Can reset maximum.
        /// </summary>
        [Test]
        public void ResetMaximum()
        {
            var criterium = new ResidualStopCriterium(1e-8, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.ResetMaximumResidualToDefault();
            Assert.AreEqual(ResidualStopCriterium.DefaultMaximumResidual, criterium.Maximum, "Incorrect maximum");
        }

        /// <summary>
        /// Can reset minimum iterations below maximum.
        /// </summary>
        [Test]
        public void ResetMinimumIterationsBelowMaximum()
        {
            var criterium = new ResidualStopCriterium(1e-8, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            criterium.ResetMinimumIterationsBelowMaximumToDefault();
            Assert.AreEqual(ResidualStopCriterium.DefaultMinimumIterationsBelowMaximum, criterium.MinimumIterationsBelowMaximum, "Incorrect iteration count");
        }

        /// <summary>
        /// Determine status with illegal iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithIllegalIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criterium = new ResidualStopCriterium(1e-8, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.Throws<ArgumentOutOfRangeException>(() => criterium.DetermineStatus(
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
            var criterium = new ResidualStopCriterium(1e-8, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.Throws<ArgumentNullException>(() => criterium.DetermineStatus(
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
            var criterium = new ResidualStopCriterium(1e-8, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.Throws<ArgumentNullException>(() => criterium.DetermineStatus(
                1,
                DenseVector.Create(3, i => 4),
                null,
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Determine status with <c>null</c> residual vector throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNullResidualVectorThrowsArgumentNullException()
        {
            var criterium = new ResidualStopCriterium(1e-8, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.Throws<ArgumentNullException>(() => criterium.DetermineStatus(
                1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 5),
                null));
        }

        /// <summary>
        /// Determine status with non-matching solution vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNonMatchingSolutionVectorThrowsArgumentException()
        {
            var criterium = new ResidualStopCriterium(1e-8, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.Throws<ArgumentException>(() => criterium.DetermineStatus(
                1,
                DenseVector.Create(4, i => 4),
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4)));
        }

        /// <summary>
        /// Determine status with non-matching source vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNonMatchingSourceVectorThrowsArgumentException()
        {
            var criterium = new ResidualStopCriterium(1e-8, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.Throws<ArgumentException>(() => criterium.DetermineStatus(
                1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(4, i => 4),
                DenseVector.Create(3, i => 4)));
        }

        /// <summary>
        /// Determine status with non-matching residual vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNonMatchingResidualVectorThrowsArgumentException()
        {
            var criterium = new ResidualStopCriterium(1e-8, 50);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.Throws<ArgumentException>(() => criterium.DetermineStatus(
                1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 4),
                DenseVector.Create(4, i => 4)));
        }

        /// <summary>
        /// Can determine status with source NaN.
        /// </summary>
        [Test]
        public void DetermineStatusWithSourceNaN()
        {
            var criterium = new ResidualStopCriterium(1e-3, 10);
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] {1.0, 1.0, 2.0});
            var source = new DenseVector(new[] {1.0, 1.0, double.NaN});
            var residual = new DenseVector(new[] {1000.0, 1000.0, 2001.0});

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOf(typeof (CalculationDiverged), criterium.Status, "Should be diverged");
        }

        /// <summary>
        /// Can determine status with residual NaN.
        /// </summary>
        [Test]
        public void DetermineStatusWithResidualNaN()
        {
            var criterium = new ResidualStopCriterium(1e-3, 10);
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] {1.0, 1.0, 2.0});
            var source = new DenseVector(new[] {1.0, 1.0, 2.0});
            var residual = new DenseVector(new[] {1000.0, double.NaN, 2001.0});

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOf(typeof (CalculationDiverged), criterium.Status, "Should be diverged");
        }

        /// <summary>
        /// Can determine status with convergence at first iteration.
        /// </summary>
        [Test]
        public void DetermineStatusWithConvergenceAtFirstIteration()
        {
            var criterium = new ResidualStopCriterium();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] {1.0, 1.0, 1.0});
            var source = new DenseVector(new[] {1.0, 1.0, 1.0});
            var residual = new DenseVector(new[] {0.0, 0.0, 0.0});

            criterium.DetermineStatus(0, solution, source, residual);
            Assert.IsInstanceOf(typeof (CalculationConverged), criterium.Status, "Should be done");
        }

        /// <summary>
        /// Can determine status.
        /// </summary>
        [Test]
        public void DetermineStatus()
        {
            var criterium = new ResidualStopCriterium(1e-3, 10);
            Assert.IsNotNull(criterium, "There should be a criterium");

            // the solution vector isn't actually being used so ...
            var solution = new DenseVector(new[] {double.NaN, double.NaN, double.NaN});

            // Set the source values
            var source = new DenseVector(new[] {1.000, 1.000, 2.001});

            // Set the residual values
            var residual = new DenseVector(new[] {0.001, 0.001, 0.002});

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOf(typeof (CalculationRunning), criterium.Status, "Should still be running");

            criterium.DetermineStatus(16, solution, source, residual);
            Assert.IsInstanceOf(typeof (CalculationConverged), criterium.Status, "Should be done");
        }

        /// <summary>
        /// Can reset calculation state.
        /// </summary>
        [Test]
        public void ResetCalculationState()
        {
            var criterium = new ResidualStopCriterium(1e-3, 10);
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] {0.001, 0.001, 0.002});
            var source = new DenseVector(new[] {0.001, 0.001, 0.002});
            var residual = new DenseVector(new[] {1.000, 1.000, 2.001});

            criterium.DetermineStatus(5, solution, source, residual);
            Assert.IsInstanceOf(typeof (CalculationRunning), criterium.Status, "Should be running");

            criterium.ResetToPrecalculationState();
            Assert.IsInstanceOf(typeof (CalculationIndetermined), criterium.Status, "Should not have started");
        }

        /// <summary>
        /// Can clone stop criterium.
        /// </summary>
        [Test]
        public void Clone()
        {
            var criterium = new ResidualStopCriterium(1e-3, 10);
            Assert.IsNotNull(criterium, "There should be a criterium");

            var clone = criterium.Clone();
            Assert.IsInstanceOf(typeof (ResidualStopCriterium), clone, "Wrong criterium type");

            var clonedCriterium = clone as ResidualStopCriterium;
            Assert.IsNotNull(clonedCriterium);

            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(criterium.Maximum, clonedCriterium.Maximum, "Clone failed");
            Assert.AreEqual(criterium.MinimumIterationsBelowMaximum, clonedCriterium.MinimumIterationsBelowMaximum, "Clone failed");

            // ReSharper restore PossibleNullReferenceException
        }
    }
}
