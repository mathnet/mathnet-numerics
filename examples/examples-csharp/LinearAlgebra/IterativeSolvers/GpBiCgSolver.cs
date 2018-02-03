// <copyright file="GpBiCgSolver.cs" company="Math.NET">
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
using System.Globalization;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace Examples.LinearAlgebra.IterativeSolversExamples
{
    /// <summary>
    /// GpBiCg Iterative solver
    /// </summary>
    public class GpBiCgSolver : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get { return "Generalized Product Bi-Conjugate Gradient iterative solver"; }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get { return "Solve linear equation using Generalized Product Bi-Conjugate Gradient (GPBiCG) solver"; }
        }

        /// <summary>
        /// Run example
        /// </summary>
        public void Run()
        {
            // Format matrix output to console
            var formatProvider = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            formatProvider.TextInfo.ListSeparator = " ";

            // Solve next system of linear equations (Ax=b):
            // 5*x + 2*y - 4*z = -7
            // 3*x - 7*y + 6*z = 38
            // 4*x + 1*y + 5*z = 43

            // Create matrix "A" with coefficients 
            var matrixA = DenseMatrix.OfArray(new[,] { { 5.00, 2.00, -4.00 }, { 3.00, -7.00, 6.00 }, { 4.00, 1.00, 5.00 } });
            Console.WriteLine(@"Matrix 'A' with coefficients");
            Console.WriteLine(matrixA.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Create vector "b" with the constant terms.
            var vectorB = new DenseVector(new[] { -7.0, 38.0, 43.0 });
            Console.WriteLine(@"Vector 'b' with the constant terms");
            Console.WriteLine(vectorB.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // Create stop criteria to monitor an iterative calculation. There are next available stop criteria:
            // - DivergenceStopCriterion: monitors an iterative calculation for signs of divergence;
            // - FailureStopCriterion: monitors residuals for NaN's;
            // - IterationCountStopCriterion: monitors the numbers of iteration steps;
            // - ResidualStopCriterion: monitors residuals if calculation is considered converged;

            // Stop calculation if 1000 iterations reached during calculation
            var iterationCountStopCriterion = new IterationCountStopCriterion<double>(1000);

            // Stop calculation if residuals are below 1E-10 --> the calculation is considered converged
            var residualStopCriterion = new ResidualStopCriterion<double>(1e-10);

            // Create monitor with defined stop criteria
            var monitor = new Iterator<double>(iterationCountStopCriterion, residualStopCriterion);

            // Create Generalized Product Bi-Conjugate Gradient solver
            var solver = new GpBiCg();

            // 1. Solve the matrix equation
            var resultX = matrixA.SolveIterative(vectorB, solver, monitor);
            Console.WriteLine(@"1. Solve the matrix equation");
            Console.WriteLine();

            // 2. Check solver status of the iterations. 
            // Solver has property IterationResult which contains the status of the iteration once the calculation is finished.
            // Possible values are:
            // - CalculationCancelled: calculation was cancelled by the user;
            // - CalculationConverged: calculation has converged to the desired convergence levels;
            // - CalculationDiverged: calculation diverged;
            // - CalculationFailure: calculation has failed for some reason;
            // - CalculationIndetermined: calculation is indetermined, not started or stopped;
            // - CalculationRunning: calculation is running and no results are yet known;
            // - CalculationStoppedWithoutConvergence: calculation has been stopped due to reaching the stopping limits, but that convergence was not achieved;
            Console.WriteLine(@"2. Solver status of the iterations");
            Console.WriteLine(monitor.Status);
            Console.WriteLine();

            // 3. Solution result vector of the matrix equation
            Console.WriteLine(@"3. Solution result vector of the matrix equation");
            Console.WriteLine(resultX.ToString("#0.00\t", formatProvider));
            Console.WriteLine();

            // 4. Verify result. Multiply coefficient matrix "A" by result vector "x"
            var reconstructVecorB = matrixA*resultX;
            Console.WriteLine(@"4. Multiply coefficient matrix 'A' by result vector 'x'");
            Console.WriteLine(reconstructVecorB.ToString("#0.00\t", formatProvider));
            Console.WriteLine();
        }
    }
}
