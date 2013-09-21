// <copyright file="DivergenceStopCriteriumTest.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Complex.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.Solvers.StopCriterium
{
    using System.Numerics;

    /// <summary>
    /// Divergence stop criterium test.
    /// </summary>
    [TestFixture]
    public sealed class DivergenceStopCriteriumTest
    {
        /// <summary>
        /// Create with negative maximum increase throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithNegativeMaximumIncreaseThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DivergenceStopCriterium(-0.1));
        }

        /// <summary>
        /// Create with illegal minimum iterations throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithIllegalMinimumIterationsThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DivergenceStopCriterium(minimumIterations: 2));
        }

        /// <summary>
        /// Can create stop criterium.
        /// </summary>
        [Test]
        public void Create()
        {
            var criterium = new DivergenceStopCriterium(0.1, 3);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.AreEqual(0.1, criterium.MaximumRelativeIncrease, "Incorrect maximum");
            Assert.AreEqual(3, criterium.MinimumNumberOfIterations, "Incorrect iteration count");
        }

        /// <summary>
        /// Can reset maximum increase.
        /// </summary>
        [Test]
        public void ResetMaximumIncrease()
        {
            var criterium = new DivergenceStopCriterium(0.5, 3);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.AreEqual(0.5, criterium.MaximumRelativeIncrease, "Incorrect maximum");

            criterium.ResetMaximumRelativeIncreaseToDefault();
            Assert.AreEqual(DivergenceStopCriterium.DefaultMaximumRelativeIncrease, criterium.MaximumRelativeIncrease, "Incorrect value");
        }

        /// <summary>
        /// Can reset minimum iterations below maximum.
        /// </summary>
        [Test]
        public void ResetMinimumIterationsBelowMaximum()
        {
            var criterium = new DivergenceStopCriterium(0.5, 15);
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.AreEqual(15, criterium.MinimumNumberOfIterations, "Incorrect iteration count");

            criterium.ResetNumberOfIterationsToDefault();
            Assert.AreEqual(DivergenceStopCriterium.DefaultMinimumNumberOfIterations, criterium.MinimumNumberOfIterations, "Incorrect value");
        }

        /// <summary>
        /// Determine status with illegal iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithIllegalIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criterium = new DivergenceStopCriterium(0.5, 15);
            Assert.Throws<ArgumentOutOfRangeException>(() => criterium.DetermineStatus(
                -1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 5),
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Can determine status with too few iterations.
        /// </summary>
        [Test]
        public void DetermineStatusWithTooFewIterations()
        {
            const double Increase = 0.5;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);

            // Add residuals. We should not diverge because we'll have to few iterations
            for (var i = 0; i < Iterations - 1; i++)
            {
                var status = criterium.DetermineStatus(
                    i,
                    new DenseVector(new[] {new Complex(1.0, 0)}),
                    new DenseVector(new[] {new Complex(1.0, 0)}),
                    new DenseVector(new[] {new Complex((i + 1)*(Increase + 0.1), 0)}));

                Assert.AreEqual(IterationStatus.Continue, status, "Status check fail.");
            }
        }

        /// <summary>
        /// Can determine status with no divergence.
        /// </summary>
        [Test]
        public void DetermineStatusWithNoDivergence()
        {
            const double Increase = 0.5;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);

            // Add residuals. We should not diverge because we won't have enough increase
            for (var i = 0; i < Iterations*2; i++)
            {
                var status = criterium.DetermineStatus(
                    i,
                    new DenseVector(new[] {new Complex(1.0, 0)}),
                    new DenseVector(new[] {new Complex(1.0, 0)}),
                    new DenseVector(new[] {new Complex((i + 1)*(Increase - 0.01), 0)}));

                Assert.AreEqual(IterationStatus.Continue, status, "Status check fail.");
            }
        }

        /// <summary>
        /// Can determine status with divergence through NaN.
        /// </summary>
        [Test]
        public void DetermineStatusWithDivergenceThroughNaN()
        {
            const double Increase = 0.5;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);

            // Add residuals. We should not diverge because we'll have to few iterations
            for (var i = 0; i < Iterations - 5; i++)
            {
                var status = criterium.DetermineStatus(
                    i,
                    new DenseVector(new[] {new Complex(1.0, 0)}),
                    new DenseVector(new[] {new Complex(1.0, 0)}),
                    new DenseVector(new[] {new Complex((i + 1)*(Increase - 0.01), 0)}));

                Assert.AreEqual(IterationStatus.Continue, status, "Status check fail.");
            }

            // Now make it fail by throwing in a NaN
            var status2 = criterium.DetermineStatus(
                Iterations,
                new DenseVector(new[] {new Complex(1.0, 0)}),
                new DenseVector(new[] {new Complex(1.0, 0)}),
                new DenseVector(new[] {new Complex(double.NaN, 0)}));

            Assert.AreEqual(IterationStatus.Diverged, status2, "Status check fail.");
        }

        /// <summary>
        /// Can determine status with divergence.
        /// </summary>
        [Test]
        public void DetermineStatusWithDivergence()
        {
            const double Increase = 0.5;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);

            // Add residuals. We should not diverge because we'll have one to few iterations
            double previous = 1;
            for (var i = 0; i < Iterations - 1; i++)
            {
                previous *= 1 + Increase + 0.01;
                var status = criterium.DetermineStatus(
                    i,
                    new DenseVector(new[] {new Complex(1.0, 0)}),
                    new DenseVector(new[] {new Complex(1.0, 0)}),
                    new DenseVector(new[] {new Complex(previous, 0)}));

                Assert.AreEqual(IterationStatus.Continue, status, "Status check fail.");
            }

            // Add the final residual. Now we should have divergence
            previous *= 1 + Increase + 0.01;
            var status2 = criterium.DetermineStatus(
                Iterations - 1,
                new DenseVector(new[] {new Complex(1.0, 0)}),
                new DenseVector(new[] {new Complex(1.0, 0)}),
                new DenseVector(new[] {new Complex(previous, 0)}));

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

            var criterium = new DivergenceStopCriterium(Increase, Iterations);

            // Add residuals. Blow it up instantly
            var status = criterium.DetermineStatus(
                1,
                new DenseVector(new[] {new Complex(1.0, 0)}),
                new DenseVector(new[] {new Complex(1.0, 0)}),
                new DenseVector(new[] {new Complex(double.NaN, 0)}));

            Assert.AreEqual(IterationStatus.Diverged, status, "Status check fail.");

            // Reset the state
            criterium.Reset();

            Assert.AreEqual(Increase, criterium.MaximumRelativeIncrease, "Incorrect maximum");
            Assert.AreEqual(Iterations, criterium.MinimumNumberOfIterations, "Incorrect iteration count");
            Assert.AreEqual(IterationStatus.Continue, criterium.Status, "Status check fail.");
        }

        /// <summary>
        /// Can clone stop criterium.
        /// </summary>
        [Test]
        public void Clone()
        {
            const double Increase = 0.5;
            const int Iterations = 10;

            var criterium = new DivergenceStopCriterium(Increase, Iterations);
            Assert.IsNotNull(criterium, "There should be a criterium");

            var clone = criterium.Clone();
            Assert.IsInstanceOf(typeof (DivergenceStopCriterium), clone, "Wrong criterium type");

            var clonedCriterium = clone as DivergenceStopCriterium;
            Assert.IsNotNull(clonedCriterium);

            Assert.AreEqual(criterium.MaximumRelativeIncrease, clonedCriterium.MaximumRelativeIncrease, "Incorrect maximum");
            Assert.AreEqual(criterium.MinimumNumberOfIterations, clonedCriterium.MinimumNumberOfIterations, "Incorrect iteration count");
        }
    }
}
