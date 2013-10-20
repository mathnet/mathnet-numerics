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

using System;
using MathNet.Numerics.LinearAlgebra.Complex32;
using MathNet.Numerics.LinearAlgebra.Complex32.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Solvers.StopCriterium
{
    using Numerics;

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
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResidualStopCriterium<Complex32>(-0.1f));
        }

        /// <summary>
        /// Create with illegal minimum iterations throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithIllegalMinimumIterationsThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ResidualStopCriterium<Complex32>(-1));
        }

        /// <summary>
        /// Can create.
        /// </summary>
        [Test]
        public void Create()
        {
            var criterium = new ResidualStopCriterium<Complex32>(1e-6f, 50);

            Assert.AreEqual(1e-6f, criterium.Maximum, "Incorrect maximum");
            Assert.AreEqual(50, criterium.MinimumIterationsBelowMaximum, "Incorrect iteration count");
        }

        /// <summary>
        /// Determine status with illegal iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithIllegalIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criterium = new ResidualStopCriterium<Complex32>(1e-6f, 50);

            Assert.Throws<ArgumentOutOfRangeException>(() => criterium.DetermineStatus(
                -1,
                DenseVector.Create(3, i => 4),
                DenseVector.Create(3, i => 5),
                DenseVector.Create(3, i => 6)));
        }

        /// <summary>
        /// Determine status with non-matching solution vector throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNonMatchingSolutionVectorThrowsArgumentException()
        {
            var criterium = new ResidualStopCriterium<Complex32>(1e-6f, 50);

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
            var criterium = new ResidualStopCriterium<Complex32>(1e-6f, 50);

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
            var criterium = new ResidualStopCriterium<Complex32>(1e-6f, 50);

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
            var criterium = new ResidualStopCriterium<Complex32>(1e-3f, 10);

            var solution = new DenseVector(new[] {new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1)});
            var source = new DenseVector(new[] {new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(float.NaN, 1)});
            var residual = new DenseVector(new[] {new Complex32(1000.0f, 1), new Complex32(1000.0f, 1), new Complex32(2001.0f, 1)});

            var status = criterium.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Diverged, status, "Should be diverged");
        }

        /// <summary>
        /// Can determine status with residual NaN.
        /// </summary>
        [Test]
        public void DetermineStatusWithResidualNaN()
        {
            var criterium = new ResidualStopCriterium<Complex32>(1e-3f, 10);

            var solution = new DenseVector(new[] {new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1)});
            var source = new DenseVector(new[] {new Complex32(1.0f, 1), new Complex32(1.0f, 1), new Complex32(2.0f, 1)});
            var residual = new DenseVector(new[] {new Complex32(1000.0f, 1), new Complex32(float.NaN, 1), new Complex32(2001.0f, 1)});

            var status = criterium.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Diverged, status, "Should be diverged");
        }

        /// <summary>
        /// Can determine status with convergence at first iteration.
        /// </summary>
        /// <remarks>Bugfix: The unit tests for the BiCgStab solver run with a super simple matrix equation
        /// which converges at the first iteration. The default settings for the
        /// residual stop criterium should be able to handle this.
        /// </remarks>
        [Test]
        public void DetermineStatusWithConvergenceAtFirstIteration()
        {
            var criterium = new ResidualStopCriterium<Complex32>(1e-6);

            var solution = new DenseVector(new[] {Complex32.One, Complex32.One, Complex32.One});
            var source = new DenseVector(new[] {Complex32.One, Complex32.One, Complex32.One});
            var residual = new DenseVector(new[] {Complex32.Zero, Complex32.Zero, Complex32.Zero});

            var status = criterium.DetermineStatus(0, solution, source, residual);
            Assert.AreEqual(IterationStatus.Converged, status, "Should be done");
        }

        /// <summary>
        /// Can determine status.
        /// </summary>
        [Test]
        public void DetermineStatus()
        {
            var criterium = new ResidualStopCriterium<Complex32>(1e-3f, 10);

            // the solution vector isn't actually being used so ...
            var solution = new DenseVector(new[] {new Complex32(float.NaN, float.NaN), new Complex32(float.NaN, float.NaN), new Complex32(float.NaN, float.NaN)});

            // Set the source values
            var source = new DenseVector(new[] {new Complex32(1.000f, 1), new Complex32(1.000f, 1), new Complex32(2.001f, 1)});

            // Set the residual values
            var residual = new DenseVector(new[] {new Complex32(0.001f, 0), new Complex32(0.001f, 0), new Complex32(0.002f, 0)});

            var status = criterium.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Continue, status, "Should still be running");

            var status2 = criterium.DetermineStatus(16, solution, source, residual);
            Assert.AreEqual(IterationStatus.Converged, status2, "Should be done");
        }

        /// <summary>
        /// Can reset calculation state.
        /// </summary>
        [Test]
        public void ResetCalculationState()
        {
            var criterium = new ResidualStopCriterium<Complex32>(1e-3f, 10);

            var solution = new DenseVector(new[] {new Complex32(0.001f, 1), new Complex32(0.001f, 1), new Complex32(0.002f, 1)});
            var source = new DenseVector(new[] {new Complex32(0.001f, 1), new Complex32(0.001f, 1), new Complex32(0.002f, 1)});
            var residual = new DenseVector(new[] {new Complex32(1.000f, 0), new Complex32(1.000f, 0), new Complex32(2.001f, 0)});

            var status = criterium.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Continue, status, "Should be running");

            criterium.Reset();
            Assert.AreEqual(IterationStatus.Continue, criterium.Status, "Should not have started");
        }

        /// <summary>
        /// Can clone stop criterium.
        /// </summary>
        [Test]
        public void Clone()
        {
            var criterium = new ResidualStopCriterium<Complex32>(1e-3f, 10);

            var clone = criterium.Clone();
            Assert.IsInstanceOf(typeof(ResidualStopCriterium<Complex32>), clone, "Wrong criterium type");

            var clonedCriterium = clone as ResidualStopCriterium<Complex32>;
            Assert.IsNotNull(clonedCriterium);

            Assert.AreEqual(criterium.Maximum, clonedCriterium.Maximum, "Clone failed");
            Assert.AreEqual(criterium.MinimumIterationsBelowMaximum, clonedCriterium.MinimumIterationsBelowMaximum, "Clone failed");
        }
    }
}
