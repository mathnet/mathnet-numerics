// <copyright file="DivergenceStopCriterionTest.cs" company="Math.NET">
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
    /// Divergence stop criterion test.
    /// </summary>
    [TestFixture, Category("LASolver")]
    public sealed class DivergenceStopCriterionTest
    {
        /// <summary>
        /// Create with negative maximum increase throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithNegativeMaximumIncreaseThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => new DivergenceStopCriterion<int>(-0.1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Create with illegal minimum iterations throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithIllegalMinimumIterationsThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => new DivergenceStopCriterion<int>(minimumIterations: 2), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can create stop criterion.
        /// </summary>
        [Test]
        public void Create()
        {
            var criterion = new DivergenceStopCriterion<int>(0.1, 3);
            Assert.IsNotNull(criterion, "There should be a criterion");

            Assert.AreEqual(0.1, criterion.MaximumRelativeIncrease, "Incorrect maximum");
            Assert.AreEqual(3, criterion.MinimumNumberOfIterations, "Incorrect iteration count");
        }

        /// <summary>
        /// Determine status with illegal iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithIllegalIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criterion = new DivergenceStopCriterion<int>(0.5, 15);
            Assert.That(() => criterion.DetermineStatus(
                -1,
                Vector<int>.Build.Dense(3, 4),
                Vector<int>.Build.Dense(3, 5),
                Vector<int>.Build.Dense(3, 6)), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can determine status with too few iterations.
        /// </summary>
        [Test]
        public void DetermineStatusWithTooFewIterations()
        {
			// for Integer use 10x the values for Increase and the residuals as used in Single
            const int Increase = 5;
            const int Iterations = 10;

            var criterion = new DivergenceStopCriterion<int>(Increase, Iterations);

            // Add residuals. We should not diverge because we'll have to few iterations
            for (var i = 0; i < Iterations - 1; i++)
            {
                var status = criterion.DetermineStatus(
                    i,
                    new DenseVector(new[] { 1 }),
                    new DenseVector(new[] { 1 }),
                    new DenseVector(new[] { (i + 1)*(Increase + 1) }));

                Assert.AreEqual(IterationStatus.Continue, status, "Status check fail.");
            }
        }

        /// <summary>
        /// Can determine status with no divergence.
        /// </summary>
        [Test]
        public void DetermineStatusWithNoDivergence()
        {
			// for Integer use 100x the values for Increase and the residuals as used in Single
            const int Increase = 50;
            const int Iterations = 10;

            var criterion = new DivergenceStopCriterion<int>(Increase, Iterations);

            // Add residuals. We should not diverge because we won't have enough increase
            for (var i = 0; i < Iterations*2; i++)
            {
                var status = criterion.DetermineStatus(
                    i,
                    new DenseVector(new[] { 1 }),
                    new DenseVector(new[] { 1 }),
                    new DenseVector(new[] { (i + 1)*(Increase - 1) }));

                Assert.AreEqual(IterationStatus.Continue, status, "Status check fail.");
            }
        }

		/////// <summary>
		/////// Can determine status with divergence through NaN.
		/////// </summary>
		////[Test]
		////public void DetermineStatusWithDivergenceThroughNaN()
		////{
		////	const float Increase = 0.5f;
		////	const int Iterations = 10;

		////	var criterion = new DivergenceStopCriterion<int>(Increase, Iterations);

		////	// Add residuals. We should not diverge because we'll have to few iterations
		////	for (var i = 0; i < Iterations - 5; i++)
		////	{
		////		var status = criterion.DetermineStatus(
		////			i,
		////			new DenseVector(new[] { 1 }),
		////			new DenseVector(new[] { 1 }),
		////			new DenseVector(new[] { (i + 1)*(Increase - 0.01f) }));

		////		Assert.AreEqual(IterationStatus.Continue, status, "Status check fail.");
		////	}

		////	// Now make it fail by throwing in a NaN
		////	var status2 = criterion.DetermineStatus(
		////		Iterations,
		////		new DenseVector(new[] { 1 }),
		////		new DenseVector(new[] { 1 }),
		////		new DenseVector(new[] { float.NaN }));

		////	Assert.AreEqual(IterationStatus.Diverged, status2, "Status check fail.");
		////}

        /// <summary>
        /// Can determine status with divergence.
        /// </summary>
        [Test]
        public void DetermineStatusWithDivergence()
        {
			// for Integer scale the values up to be integral for Increase and the residuals (previous) as used in Single
			const int Increase = 5;
			const int Iterations = 10;

            var criterion = new DivergenceStopCriterion<int>(Increase, Iterations);

            // Add residuals. We should not diverge because we'll have one to few iterations
            int previous = 1;
            for (var i = 0; i < Iterations - 1; i++)
            {
                previous *= 1 + Increase + 1;
                var status = criterion.DetermineStatus(
                    i,
                    new DenseVector(new[] { 1 }),
                    new DenseVector(new[] { 1 }),
                    new DenseVector(new[] { previous }));

                Assert.AreEqual(IterationStatus.Continue, status, "Status check fail.");
            }

            // Add the final residual. Now we should have divergence
            previous *= 1 + Increase + 1;
            var status2 = criterion.DetermineStatus(
                Iterations - 1,
                new DenseVector(new[] { 1 }),
                new DenseVector(new[] { 1 }),
                new DenseVector(new[] { previous }));

            Assert.AreEqual(IterationStatus.Diverged, status2, "Status check fail.");
        }

        /// <summary>
        /// Can reset calculation state.
        /// </summary>
        [Test]
        public void ResetCalculationState()
        {
            const double Increase = 0.5;
            const int Iterations = 10;

            var criterion = new DivergenceStopCriterion<int>(Increase, Iterations);

            // Add residuals. Blow it up instantly
            var status = criterion.DetermineStatus(
                1,
                new DenseVector(new[] { 1 }),
                new DenseVector(new[] { 1 }),
                new DenseVector(new[] { int.MaxValue }));

            Assert.AreEqual(IterationStatus.Diverged, status, "Status check fail.");

            // Reset the state
            criterion.Reset();

            Assert.AreEqual(Increase, criterion.MaximumRelativeIncrease, "Incorrect maximum");
            Assert.AreEqual(Iterations, criterion.MinimumNumberOfIterations, "Incorrect iteration count");
            Assert.AreEqual(IterationStatus.Continue, criterion.Status, "Status check fail.");
        }

        /// <summary>
        /// Can clone stop criterion.
        /// </summary>
        [Test]
        public void Clone()
        {
            const double Increase = 0.5;
            const int Iterations = 10;

            var criterion = new DivergenceStopCriterion<int>(Increase, Iterations);
            Assert.IsNotNull(criterion, "There should be a criterion");

            var clone = criterion.Clone();
            Assert.IsInstanceOf(typeof (DivergenceStopCriterion<int>), clone, "Wrong criterion type");

            var clonedCriterion = clone as DivergenceStopCriterion<int>;
            Assert.IsNotNull(clonedCriterion);

            Assert.AreEqual(criterion.MaximumRelativeIncrease, clonedCriterion.MaximumRelativeIncrease, "Incorrect maximum");
            Assert.AreEqual(criterion.MinimumNumberOfIterations, clonedCriterion.MinimumNumberOfIterations, "Incorrect iteration count");
        }
    }
}
