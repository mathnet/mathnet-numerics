// <copyright file="Matrix.Solve.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Solvers;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>
    /// Defines the base class for <c>Matrix</c> classes.
    /// </summary>
    public abstract partial class Matrix<T>
    {

        // Factorizations

        /// <summary>
        /// Computes the Cholesky decomposition for a matrix.
        /// </summary>
        /// <returns>The Cholesky decomposition object.</returns>
        public abstract Cholesky<T> Cholesky();

        /// <summary>
        /// Computes the LU decomposition for a matrix.
        /// </summary>
        /// <returns>The LU decomposition object.</returns>
        public abstract LU<T> LU();

        /// <summary>
        /// Computes the QR decomposition for a matrix.
        /// </summary>
        /// <param name="method">The type of QR factorization to perform.</param>
        /// <returns>The QR decomposition object.</returns>
        public abstract QR<T> QR(QRMethod method = QRMethod.Thin);

        /// <summary>
        /// Computes the QR decomposition for a matrix using Modified Gram-Schmidt Orthogonalization.
        /// </summary>
        /// <returns>The QR decomposition object.</returns>
        public abstract GramSchmidt<T> GramSchmidt();

        /// <summary>
        /// Computes the SVD decomposition for a matrix.
        /// </summary>
        /// <param name="computeVectors">Compute the singular U and VT vectors or not.</param>
        /// <returns>The SVD decomposition object.</returns>
        public abstract Svd<T> Svd(bool computeVectors = true);

        /// <summary>
        /// Computes the EVD decomposition for a matrix.
        /// </summary>
        /// <returns>The EVD decomposition object.</returns>
        public abstract Evd<T> Evd(Symmetricity symmetricity = Symmetricity.Unknown);



        // Direct Solvers: Full

        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>x</b>.</param>
        public void Solve(Vector<T> input, Vector<T> result)
        {
            if (ColumnCount == RowCount)
            {
                LU().Solve(input, result);
                return;
            }

            QR().Solve(input, result);
        }

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <param name="result">The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</param>
        public void Solve(Matrix<T> input, Matrix<T> result)
        {
            if (ColumnCount == RowCount)
            {
                LU().Solve(input, result);
                return;
            }

            QR().Solve(input, result);
        }



        // Direct Solvers: Simple

        /// <summary>
        /// Solves a system of linear equations, <b>AX = B</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix{T}"/>, <b>B</b>.</param>
        /// <returns>The left hand side <see cref="Matrix{T}"/>, <b>X</b>.</returns>
        public Matrix<T> Solve(Matrix<T> input)
        {
            var x = Build.SameAs(this, ColumnCount, input.ColumnCount, fullyMutable: true);
            Solve(input, x);
            return x;
        }


        /// <summary>
        /// Solves a system of linear equations, <b>Ax = b</b>, with A QR factorized.
        /// </summary>
        /// <param name="input">The right hand side vector, <b>b</b>.</param>
        /// <returns>The left hand side <see cref="Vector{T}"/>, <b>x</b>.</returns>
        public Vector<T> Solve(Vector<T> input)
        {
            var x = Vector<T>.Build.SameAs(this, ColumnCount);
            Solve(input, x);
            return x;
        }



        // Iterative Solvers: Full

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix (this matrix), b is the solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="input">The solution vector <c>b</c>.</param>
        /// <param name="result">The result vector <c>x</c>.</param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="iterator">The iterator to use to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        public IterationStatus TrySolveIterative(Vector<T> input, Vector<T> result, IIterativeSolver<T> solver, Iterator<T> iterator = null, IPreconditioner<T> preconditioner = null)
        {
            if (iterator == null)
            {
                iterator = new Iterator<T>(Build.IterativeSolverStopCriteria());
            }

            if (preconditioner == null)
            {
                preconditioner = new UnitPreconditioner<T>();
            }

            solver.Solve(this, input, result, iterator, preconditioner);

            return iterator.Status;
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix (this matrix), B is the solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="input">The solution matrix <c>B</c>.</param>
        /// <param name="result">The result matrix <c>X</c></param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="iterator">The iterator to use to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        public IterationStatus TrySolveIterative(Matrix<T> input, Matrix<T> result, IIterativeSolver<T> solver, Iterator<T> iterator = null, IPreconditioner<T> preconditioner = null)
        {
            if (RowCount != input.RowCount || input.RowCount != result.RowCount || input.ColumnCount != result.ColumnCount)
            {
                throw DimensionsDontMatch<ArgumentException>(this, input, result);
            }

            if (iterator == null)
            {
                iterator = new Iterator<T>(Build.IterativeSolverStopCriteria());
            }

            if (preconditioner == null)
            {
                preconditioner = new UnitPreconditioner<T>();
            }

            for (var column = 0; column < input.ColumnCount; column++)
            {
                var solution = Vector<T>.Build.Dense(RowCount);

                solver.Solve(this, input.Column(column), solution, iterator, preconditioner);

                foreach (var element in solution.EnumerateIndexed(Zeros.AllowSkip))
                {
                    result.At(element.Item1, column, element.Item2);
                }
            }

            return iterator.Status;
        }


        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix (this matrix), b is the solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="input">The solution vector <c>b</c>.</param>
        /// <param name="result">The result vector <c>x</c>.</param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="stopCriteria">Criteria to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        public IterationStatus TrySolveIterative(Vector<T> input, Vector<T> result, IIterativeSolver<T> solver, IPreconditioner<T> preconditioner, params IIterationStopCriterion<T>[] stopCriteria)
        {
            var iterator = new Iterator<T>(stopCriteria.Length == 0 ? Build.IterativeSolverStopCriteria() : stopCriteria);
            return TrySolveIterative(input, result, solver, iterator, preconditioner);
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix (this matrix), B is the solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="input">The solution matrix <c>B</c>.</param>
        /// <param name="result">The result matrix <c>X</c></param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="stopCriteria">Criteria to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        public IterationStatus TrySolveIterative(Matrix<T> input, Matrix<T> result, IIterativeSolver<T> solver, IPreconditioner<T> preconditioner, params IIterationStopCriterion<T>[] stopCriteria)
        {
            var iterator = new Iterator<T>(stopCriteria.Length == 0 ? Build.IterativeSolverStopCriteria() : stopCriteria);
            return TrySolveIterative(input, result, solver, iterator, preconditioner);
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix (this matrix), b is the solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="input">The solution vector <c>b</c>.</param>
        /// <param name="result">The result vector <c>x</c>.</param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="stopCriteria">Criteria to control when to stop iterating.</param>
        public IterationStatus TrySolveIterative(Vector<T> input, Vector<T> result, IIterativeSolver<T> solver, params IIterationStopCriterion<T>[] stopCriteria)
        {
            var iterator = new Iterator<T>(stopCriteria.Length == 0 ? Build.IterativeSolverStopCriteria() : stopCriteria);
            return TrySolveIterative(input, result, solver, iterator);
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix (this matrix), B is the solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="input">The solution matrix <c>B</c>.</param>
        /// <param name="result">The result matrix <c>X</c></param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="stopCriteria">Criteria to control when to stop iterating.</param>
        public IterationStatus TrySolveIterative(Matrix<T> input, Matrix<T> result, IIterativeSolver<T> solver, params IIterationStopCriterion<T>[] stopCriteria)
        {
            var iterator = new Iterator<T>(stopCriteria.Length == 0 ? Build.IterativeSolverStopCriteria() : stopCriteria);
            return TrySolveIterative(input, result, solver, iterator);
        }



        // Iterative Solvers: Simple

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix (this matrix), b is the solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="input">The solution vector <c>b</c>.</param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="iterator">The iterator to use to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        /// <returns>The result vector <c>x</c>.</returns>
        public Vector<T> SolveIterative(Vector<T> input, IIterativeSolver<T> solver, Iterator<T> iterator = null, IPreconditioner<T> preconditioner = null)
        {
            var result = Vector<T>.Build.Dense(RowCount);
            TrySolveIterative(input, result, solver, iterator, preconditioner);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix (this matrix), B is the solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="input">The solution matrix <c>B</c>.</param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="iterator">The iterator to use to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        /// <returns>The result matrix <c>X</c>.</returns>
        public Matrix<T> SolveIterative(Matrix<T> input, IIterativeSolver<T> solver, Iterator<T> iterator = null, IPreconditioner<T> preconditioner = null)
        {
            var result = Build.Dense(input.RowCount, input.ColumnCount);
            TrySolveIterative(input, result, solver, iterator, preconditioner);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix (this matrix), b is the solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="input">The solution vector <c>b</c>.</param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="stopCriteria">Criteria to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        /// <returns>The result vector <c>x</c>.</returns>
        public Vector<T> SolveIterative(Vector<T> input, IIterativeSolver<T> solver, IPreconditioner<T> preconditioner, params IIterationStopCriterion<T>[] stopCriteria)
        {
            var result = Vector<T>.Build.Dense(RowCount);
            TrySolveIterative(input, result, solver, preconditioner, stopCriteria);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix (this matrix), B is the solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="input">The solution matrix <c>B</c>.</param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="stopCriteria">Criteria to control when to stop iterating.</param>
        /// <param name="preconditioner">The preconditioner to use for approximations.</param>
        /// <returns>The result matrix <c>X</c>.</returns>
        public Matrix<T> SolveIterative(Matrix<T> input, IIterativeSolver<T> solver, IPreconditioner<T> preconditioner, params IIterationStopCriterion<T>[] stopCriteria)
        {
            var result = Build.Dense(input.RowCount, input.ColumnCount);
            TrySolveIterative(input, result, solver, preconditioner, stopCriteria);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix (this matrix), b is the solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="input">The solution vector <c>b</c>.</param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="stopCriteria">Criteria to control when to stop iterating.</param>
        /// <returns>The result vector <c>x</c>.</returns>
        public Vector<T> SolveIterative(Vector<T> input, IIterativeSolver<T> solver, params IIterationStopCriterion<T>[] stopCriteria)
        {
            var result = Vector<T>.Build.Dense(RowCount);
            TrySolveIterative(input, result, solver, stopCriteria);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix (this matrix), B is the solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="input">The solution matrix <c>B</c>.</param>
        /// <param name="solver">The iterative solver to use.</param>
        /// <param name="stopCriteria">Criteria to control when to stop iterating.</param>
        /// <returns>The result matrix <c>X</c>.</returns>
        public Matrix<T> SolveIterative(Matrix<T> input, IIterativeSolver<T> solver, params IIterationStopCriterion<T>[] stopCriteria)
        {
            var result = Build.Dense(input.RowCount, input.ColumnCount);
            TrySolveIterative(input, result, solver, stopCriteria);
            return result;
        }
    }
}
