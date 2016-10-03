// <copyright file="FailureStopCriterionTest.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.Solvers.StopCriterion
{
#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;

#endif

    /// <summary>
    /// Failure stop criterion tests.
    /// </summary>
    [TestFixture, Category("LASolver")]
    public sealed class FailureStopCriterionTest
    {
        /// <summary>
        /// Can create.
        /// </summary>
        [Test]
        public void Create()
        {
            var criterion = new FailureStopCriterion<Complex>();
            Assert.IsNotNull(criterion, "Should have a criterion now");
        }

        /// <summary>
        /// Determine status with illegal iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithIllegalIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criterion = new FailureStopCriterion<Complex>();
            Assert.IsNotNull(criterion, "There should be a criterion");

            Assert.That(() => criterion.DetermineStatus(-1, Vector<Complex>.Build.Dense(3, 4), Vector<Complex>.Build.Dense(3, 5), Vector<Complex>.Build.Dense(3, 6)), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Determine status with non-matching vectors throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNonMatchingVectorsThrowsArgumentException()
        {
            var criterion = new FailureStopCriterion<Complex>();
            Assert.IsNotNull(criterion, "There should be a criterion");

            Assert.That(() => criterion.DetermineStatus(1, Vector<Complex>.Build.Dense(3, 4), Vector<Complex>.Build.Dense(3, 6), Vector<Complex>.Build.Dense(4, 4)), Throws.ArgumentException);
        }

        /// <summary>
        /// Can determine status with residual NaN.
        /// </summary>
        [Test]
        public void DetermineStatusWithResidualNaN()
        {
            var criterion = new FailureStopCriterion<Complex>();
            Assert.IsNotNull(criterion, "There should be a criterion");

            var solution = new DenseVector(new[] { new Complex(1.0, 0), new Complex(1.0, 0), new Complex(2.0, 0) });
            var source = new DenseVector(new[] { new Complex(1001.0, 0), Complex.Zero, new Complex(2003.0, 0) });
            var residual = new DenseVector(new[] { new Complex(1000, 0), new Complex(double.NaN, 0), new Complex(2001, 0) });

            var status = criterion.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Failure, status, "Should be failed");
        }

        /// <summary>
        /// Can determine status with solution NaN.
        /// </summary>
        [Test]
        public void DetermineStatusWithSolutionNaN()
        {
            var criterion = new FailureStopCriterion<Complex>();
            Assert.IsNotNull(criterion, "There should be a criterion");

            var solution = new DenseVector(new[] { new Complex(1.0, 0), new Complex(1.0, 0), new Complex(double.NaN, 0) });
            var source = new DenseVector(new[] { new Complex(1001.0, 0), Complex.Zero, new Complex(2003.0, 0) });
            var residual = new DenseVector(new[] { new Complex(1000, 0), new Complex(1000, 0), new Complex(2001, 0) });

            var status = criterion.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Failure, status, "Should be failed");
        }

        /// <summary>
        /// Can determine status.
        /// </summary>
        [Test]
        public void DetermineStatus()
        {
            var criterion = new FailureStopCriterion<Complex>();
            Assert.IsNotNull(criterion, "There should be a criterion");

            var solution = new DenseVector(new[] { new Complex(3.0, 0), new Complex(2.0, 0), new Complex(1, 0) });
            var source = new DenseVector(new[] { new Complex(1001.0, 0), Complex.Zero, new Complex(2003.0, 0) });
            var residual = new DenseVector(new[] { new Complex(1.0, 0), new Complex(2.0, 0), new Complex(3, 0) });

            var status = criterion.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Continue, status, "Should be running");
        }

        /// <summary>
        /// Can reset calculation state.
        /// </summary>
        [Test]
        public void ResetCalculationState()
        {
            var criterion = new FailureStopCriterion<Complex>();
            Assert.IsNotNull(criterion, "There should be a criterion");

            var solution = new DenseVector(new[] { new Complex(1.0, 0), new Complex(1.0, 0), new Complex(2, 0) });
            var source = new DenseVector(new[] { new Complex(1001.0, 0), Complex.Zero, new Complex(2003.0, 0) });
            var residual = new DenseVector(new[] { new Complex(1000, 0), new Complex(1000, 0), new Complex(2001, 0) });

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
            var criterion = new FailureStopCriterion<Complex>();
            Assert.IsNotNull(criterion, "There should be a criterion");
            var clone = criterion.Clone();
            Assert.IsInstanceOf(typeof (FailureStopCriterion<Complex>), clone, "Wrong criterion type");
        }
    }
}
