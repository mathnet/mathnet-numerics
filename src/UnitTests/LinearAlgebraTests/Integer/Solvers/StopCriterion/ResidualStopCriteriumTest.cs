// <copyright file="ResidualStopCriterionTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Integer;
using MathNet.Numerics.LinearAlgebra.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Integer.Solvers.StopCriterion
{
    /// <summary>
    /// Residual stop criterion tests.
    /// </summary>
    [TestFixture, Category("LASolver")]
    public sealed class ResidualStopCriterionTest
    {
        /// <summary>
        /// Create with negative maximum throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithNegativeMaximumThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => new ResidualStopCriterion<int>(-0.1f), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Create with illegal minimum iterations throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithIllegalMinimumIterationsThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => new ResidualStopCriterion<int>(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can create.
        /// </summary>
        [Test]
        public void Create()
        {
            var criterion = new ResidualStopCriterion<int>(1e-6f, 50);

            Assert.AreEqual(1e-6f, criterion.Maximum, "Incorrect maximum");
            Assert.AreEqual(50, criterion.MinimumIterationsBelowMaximum, "Incorrect iteration count");
        }

        /// <summary>
        /// Determine status with illegal iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithIllegalIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criterion = new ResidualStopCriterion<int>(1e-6f, 50);

            Assert.That(() => criterion.DetermineStatus(
                -1,
                Vector<int>.Build.Dense(3, 4),
                Vector<int>.Build.Dense(3, 5),
                Vector<int>.Build.Dense(3, 6)), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Determine status with non-matching solution vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNonMatchingSolutionVectorThrowsArgumentException()
        {
            var criterion = new ResidualStopCriterion<int>(1e-6f, 50);

            Assert.That(() => criterion.DetermineStatus(
                1,
                Vector<int>.Build.Dense(4, 4),
                Vector<int>.Build.Dense(3, 4),
                Vector<int>.Build.Dense(3, 4)), Throws.ArgumentException);
        }

        /// <summary>
        /// Determine status with non-matching source vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNonMatchingSourceVectorThrowsArgumentException()
        {
            var criterion = new ResidualStopCriterion<int>(1e-6f, 50);

            Assert.That(() => criterion.DetermineStatus(
                1,
                Vector<int>.Build.Dense(3, 4),
                Vector<int>.Build.Dense(4, 4),
                Vector<int>.Build.Dense(3, 4)), Throws.ArgumentException);
        }

        /// <summary>
        /// Determine status with non-matching residual vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNonMatchingResidualVectorThrowsArgumentException()
        {
            var criterion = new ResidualStopCriterion<int>(1e-6f, 50);

            Assert.That(() => criterion.DetermineStatus(
                1,
                Vector<int>.Build.Dense(3, 4),
                Vector<int>.Build.Dense(3, 4),
                Vector<int>.Build.Dense(4, 4)), Throws.ArgumentException);
        }

		/////// <summary>
		/////// Can determine status with source NaN.
		/////// </summary>
		////[Test]
		////public void DetermineStatusWithSourceNaN()
		////{
		////	var criterion = new ResidualStopCriterion<int>(1e-3f, 10);

		////	var solution = new DenseVector(new[] { 1, 1, 2 });
		////	var source = new DenseVector(new[] { 1, 1, float.NaN });
		////	var residual = new DenseVector(new[] { 1000, 1000, 2001 });

		////	var status = criterion.DetermineStatus(5, solution, source, residual);
		////	Assert.AreEqual(IterationStatus.Diverged, status, "Should be diverged");
		////}

		/////// <summary>
		/////// Can determine status with residual NaN.
		/////// </summary>
		////[Test]
		////public void DetermineStatusWithResidualNaN()
		////{
		////	var criterion = new ResidualStopCriterion<int>(1e-3f, 10);

		////	var solution = new DenseVector(new[] { 1, 1, 2 });
		////	var source = new DenseVector(new[] { 1, 1, 2 });
		////	var residual = new DenseVector(new[] { 1000, float.NaN, 2001 });

		////	var status = criterion.DetermineStatus(5, solution, source, residual);
		////	Assert.AreEqual(IterationStatus.Diverged, status, "Should be diverged");
		////}

        /// <summary>
        /// Can determine status with convergence at first iteration.
        /// </summary>
        [Test]
        public void DetermineStatusWithConvergenceAtFirstIteration()
        {
            var criterion = new ResidualStopCriterion<int>(1e-6);

            var solution = new DenseVector(new[] { 1, 1, 1 });
            var source = new DenseVector(new[] { 1, 1, 1 });
            var residual = new DenseVector(new[] { 0, 0, 0 });

            var status = criterion.DetermineStatus(0, solution, source, residual);
            Assert.AreEqual(IterationStatus.Converged, status, "Should be done");
        }

        /// <summary>
        /// Can determine status.
        /// </summary>
        [Test]
        public void DetermineStatus()
        {
            var criterion = new ResidualStopCriterion<int>(1e-3f, 10);

            // the solution vector isn't actually being used so ...
            var solution = new DenseVector(new[] { 0, 0, 0 });

            // Set the source values
            var source = new DenseVector(new[] { 1000, 1000, 2001 });

            // Set the residual values
            var residual = new DenseVector(new[] { 1, 1, 2 });

            var status = criterion.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Continue, status, "Should still be running");

            var status2 = criterion.DetermineStatus(16, solution, source, residual);
            Assert.AreEqual(IterationStatus.Converged, status2, "Should be done");
        }

        /// <summary>
        /// Can reset calculation state.
        /// </summary>
        [Test]
        public void ResetCalculationState()
        {
            var criterion = new ResidualStopCriterion<int>(1e-3f, 10);

            var solution = new DenseVector(new[] { 1, 1, 2 });
            var source = new DenseVector(new[] { 1, 1, 2 });
            var residual = new DenseVector(new[] { 1000, 1000, 2001 });

            var status = criterion.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Continue, status, "Should be running");

            criterion.Reset();
            Assert.AreEqual(IterationStatus.Continue, criterion.Status, "Should not have started");
        }

        /// <summary>
        /// Can clone stop criterion.
        /// </summary>
        [Test]
        public void Clone()
        {
            var criterion = new ResidualStopCriterion<int>(1e-3f, 10);

            var clone = criterion.Clone();
            Assert.IsInstanceOf(typeof (ResidualStopCriterion<int>), clone, "Wrong criterion type");

            var clonedCriterion = clone as ResidualStopCriterion<int>;
            Assert.IsNotNull(clonedCriterion);

            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(criterion.Maximum, clonedCriterion.Maximum, "Clone failed");
            Assert.AreEqual(criterion.MinimumIterationsBelowMaximum, clonedCriterion.MinimumIterationsBelowMaximum, "Clone failed");

            // ReSharper restore PossibleNullReferenceException
        }
    }
}
