// <copyright file="FailureStopCriteriumTest.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.StopCriterium
{
    /// <summary>
    /// Failure stop criterium tests.
    /// </summary>
    [TestFixture]
    public sealed class FailureStopCriteriumTest
    {
        /// <summary>
        /// Can create.
        /// </summary>
        [Test]
        public void Create()
        {
            var criterium = new FailureStopCriterium<double>();
            Assert.IsNotNull(criterium, "Should have a criterium now");
        }

        /// <summary>
        /// Determine status with illegal iteration number throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithIllegalIterationNumberThrowsArgumentOutOfRangeException()
        {
            var criterium = new FailureStopCriterium<double>();
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.Throws<ArgumentOutOfRangeException>(() => criterium.DetermineStatus(-1, Vector<double>.Build.Dense(3, 4), Vector<double>.Build.Dense(3, 5), Vector<double>.Build.Dense(3, 6)));
        }

        /// <summary>
        /// Determine status with non-matching vectors throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void DetermineStatusWithNonMatchingVectorsThrowsArgumentException()
        {
            var criterium = new FailureStopCriterium<double>();
            Assert.IsNotNull(criterium, "There should be a criterium");

            Assert.Throws<ArgumentException>(() => criterium.DetermineStatus(1, Vector<double>.Build.Dense(3, 4), Vector<double>.Build.Dense(3, 6), Vector<double>.Build.Dense(4, 4)));
        }

        /// <summary>
        /// Can determine status with residual NaN.
        /// </summary>
        [Test]
        public void DetermineStatusWithResidualNaN()
        {
            var criterium = new FailureStopCriterium<double>();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] {1.0, 1.0, 2.0});
            var source = new DenseVector(new[] {1001.0, 0, 2003.0});
            var residual = new DenseVector(new[] {1000, double.NaN, 2001});

            var status = criterium.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Failure, status, "Should be failed");
        }

        /// <summary>
        /// Can determine status with solution NaN.
        /// </summary>
        [Test]
        public void DetermineStatusWithSolutionNaN()
        {
            var criterium = new FailureStopCriterium<double>();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] {1.0, 1.0, double.NaN});
            var source = new DenseVector(new[] {1001.0, 0.0, 2003.0});
            var residual = new DenseVector(new[] {1000.0, 1000.0, 2001.0});

            var status = criterium.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Failure, status, "Should be failed");
        }

        /// <summary>
        /// Can determine status.
        /// </summary>
        [Test]
        public void DetermineStatus()
        {
            var criterium = new FailureStopCriterium<double>();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] {3.0, 2.0, 1.0});
            var source = new DenseVector(new[] {1001.0, 0.0, 2003.0});
            var residual = new DenseVector(new[] {1.0, 2.0, 3.0});

            var status = criterium.DetermineStatus(5, solution, source, residual);
            Assert.AreEqual(IterationStatus.Continue, status, "Should be running");
        }

        /// <summary>
        /// Can reset calculation state.
        /// </summary>
        [Test]
        public void ResetCalculationState()
        {
            var criterium = new FailureStopCriterium<double>();
            Assert.IsNotNull(criterium, "There should be a criterium");

            var solution = new DenseVector(new[] {1.0, 1.0, 2.0});
            var source = new DenseVector(new[] {1001.0, 0.0, 2003.0});
            var residual = new DenseVector(new[] {1000.0, 1000.0, 2001.0});

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
            var criterium = new FailureStopCriterium<double>();
            Assert.IsNotNull(criterium, "There should be a criterium");
            var clone = criterium.Clone();
            Assert.IsInstanceOf(typeof(FailureStopCriterium<double>), clone, "Wrong criterium type");
        }
    }
}
