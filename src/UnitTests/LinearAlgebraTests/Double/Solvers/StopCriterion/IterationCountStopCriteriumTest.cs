// <copyright file="IterationCountStopCriterionTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using MathNet.Numerics.LinearAlgebra.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.StopCriterion
{
    /// <summary>
    /// Iteration count stop criterion tests.
    /// </summary>
    [TestFixture, Category("LASolver")]
    public sealed class IterationCountStopCriterionTest
    {
        /// <summary>
        /// Create with illegal minimum iterations throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void CreateWithIllegalMinimumIterationsThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => new IterationCountStopCriterion<double>(-1), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can create.
        /// </summary>
        [Test]
        public void Create()
        {
            var criterion = new IterationCountStopCriterion<double>(10);
            Assert.IsNotNull(criterion, "A criterion should have been created");
        }

        /// <summary>
        /// Can reset maximum iterations.
        /// </summary>
        [Test]
        public void ResetMaximumIterations()
        {
            var criterion = new IterationCountStopCriterion<double>(10);
            Assert.IsNotNull(criterion, "A criterion should have been created");
            Assert.AreEqual(10, criterion.MaximumNumberOfIterations, "Incorrect maximum number of iterations");

            criterion.ResetMaximumNumberOfIterationsToDefault();
            Assert.AreNotEqual(10, criterion.MaximumNumberOfIterations, "Should have reset");
            Assert.AreEqual(IterationCountStopCriterion<double>.DefaultMaximumNumberOfIterations, criterion.MaximumNumberOfIterations, "Reset to the wrong value");
        }

        /// <summary>
        /// Determine status with illegal iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithIllegalIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criterion = new IterationCountStopCriterion<double>(10);
            Assert.IsNotNull(criterion, "A criterion should have been created");

            Assert.That(() => criterion.DetermineStatus(-1, Vector<double>.Build.Dense(3, 1), Vector<double>.Build.Dense(3, 2), Vector<double>.Build.Dense(3, 3)), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can determine status.
        /// </summary>
        [Test]
        public void DetermineStatus()
        {
            var criterion = new IterationCountStopCriterion<double>(10);
            Assert.IsNotNull(criterion, "A criterion should have been created");

            var status = criterion.DetermineStatus(5, Vector<double>.Build.Dense(3, 1), Vector<double>.Build.Dense(3, 2), Vector<double>.Build.Dense(3, 3));
            Assert.AreEqual(IterationStatus.Continue, status, "Should be running");

            var status2 = criterion.DetermineStatus(10, Vector<double>.Build.Dense(3, 1), Vector<double>.Build.Dense(3, 2), Vector<double>.Build.Dense(3, 3));
            Assert.AreEqual(IterationStatus.StoppedWithoutConvergence, status2, "Should be finished");
        }

        /// <summary>
        /// Can reset calculation state.
        /// </summary>
        [Test]
        public void ResetCalculationState()
        {
            var criterion = new IterationCountStopCriterion<double>(10);
            Assert.IsNotNull(criterion, "A criterion should have been created");

            var status = criterion.DetermineStatus(5, Vector<double>.Build.Dense(3, 1), Vector<double>.Build.Dense(3, 2), Vector<double>.Build.Dense(3, 3));
            Assert.AreEqual(IterationStatus.Continue, status, "Should be running");

            criterion.Reset();
            Assert.AreEqual(IterationStatus.Continue, criterion.Status, "Should not have started");
        }

        /// <summary>
        /// Can clone a stop criterion.
        /// </summary>
        [Test]
        public void Clone()
        {
            var criterion = new IterationCountStopCriterion<double>(10);
            Assert.IsNotNull(criterion, "A criterion should have been created");
            Assert.AreEqual(10, criterion.MaximumNumberOfIterations, "Incorrect maximum");

            var clone = criterion.Clone();
            Assert.IsInstanceOf(typeof (IterationCountStopCriterion<double>), clone, "Wrong criterion type");

            var clonedCriterion = clone as IterationCountStopCriterion<double>;
            Assert.IsNotNull(clonedCriterion);

            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(criterion.MaximumNumberOfIterations, clonedCriterion.MaximumNumberOfIterations, "Clone failed");

            // ReSharper restore PossibleNullReferenceException
        }
    }
}
