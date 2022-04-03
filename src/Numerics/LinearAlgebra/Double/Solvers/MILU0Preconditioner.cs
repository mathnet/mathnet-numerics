// <copyright file="Milu0.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace MathNet.Numerics.LinearAlgebra.Double.Solvers
{
    /// <summary>
    /// A simple milu(0) preconditioner.
    /// </summary>
    /// <remarks>
    /// Original Fortran code by Yousef Saad (07 January 2004)
    /// </remarks>
    public sealed class MILU0Preconditioner : IPreconditioner<double>
    {
        // Matrix stored in Modified Sparse Row (MSR) format containing the L and U
        // factors together.

        // The diagonal (stored in alu(0:n-1) ) is inverted. Each i-th row of the matrix
        // contains the i-th row of L (excluding the diagonal entry = 1) followed by
        // the i-th row of U.
        double[] _alu;

        // The row pointers (stored in jlu(0:n) ) and column indices to off-diagonal elements.
        int[] _jlu;

        // Pointer to the diagonal elements in MSR storage (for faster LU solving).
        int[] _diag;

        /// <param name="modified">Use modified or standard ILU(0)</param>
        public MILU0Preconditioner(bool modified = true)
        {
            UseModified = modified;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use modified or standard ILU(0).
        /// </summary>
        public bool UseModified { get; set; }

        /// <summary>
        /// Gets a value indicating whether the preconditioner is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">The matrix upon which the preconditioner is based. </param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square or is not an
        /// instance of SparseCompressedRowMatrixStorage.</exception>
        public void Initialize(Matrix<double> matrix)
        {
            if (!(matrix.Storage is SparseCompressedRowMatrixStorage<double> csr))
            {
                throw new ArgumentException("Matrix must be in sparse storage format", nameof(matrix));
            }

            // Dimension of matrix
            int n = csr.RowCount;
            if (n != csr.ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.", nameof(matrix));
            }

            // Original matrix compressed sparse row storage.
            double[] a = csr.Values;
            int[] ja = csr.ColumnIndices;
            int[] ia = csr.RowPointers;

            _alu = new double[ia[n] + 1];
            _jlu = new int[ia[n] + 1];
            _diag = new int[n];

            int code = Compute(n, a, ja, ia, _alu, _jlu, _diag, UseModified);
            if (code > -1)
            {
                throw new NumericalBreakdownException("Zero pivot encountered on row " + code + " during ILU process");
            }

            IsInitialized = true;
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="input">The right hand side vector b.</param>
        /// <param name="result">The left hand side vector x.</param>
        public void Approximate(Vector<double> input, Vector<double> result)
        {
            if (_alu == null)
            {
                throw new ArgumentException("The requested matrix does not exist.");
            }

            if ((result.Count != input.Count) || (result.Count != _diag.Length))
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            int n = _diag.Length;

            // Forward solve.
            for (int i = 0; i < n; i++)
            {
                result[i] = input[i];
                for (int k = _jlu[i]; k < _diag[i]; k++)
                {
                    result[i] = result[i] - _alu[k] * result[_jlu[k]];
                }
            }

            // Backward solve.
            for (int i = n - 1; i >= 0; i--)
            {
                for (int k = _diag[i]; k < _jlu[i + 1]; k++)
                {
                    result[i] = result[i] - _alu[k] * result[_jlu[k]];
                }
                result[i] = _alu[i] * result[i];
            }
        }

        /// <summary>
        /// MILU0 is a simple milu(0) preconditioner.
        /// </summary>
        /// <param name="n">Order of the matrix.</param>
        /// <param name="a">Matrix values in CSR format (input).</param>
        /// <param name="ja">Column indices (input).</param>
        /// <param name="ia">Row pointers (input).</param>
        /// <param name="alu">Matrix values in MSR format (output).</param>
        /// <param name="jlu">Row pointers and column indices (output).</param>
        /// <param name="ju">Pointer to diagonal elements (output).</param>
        /// <param name="modified">True if the modified/MILU algorithm should be used (recommended)</param>
        /// <returns>Returns 0 on success or k > 0 if a zero pivot was encountered at step k.</returns>
        int Compute(int n, double[] a, int[] ja, int[] ia, double[] alu, int[] jlu, int[] ju, bool modified)
        {
            var iw = new int[n];
            int i;

            // Set initial pointer value.
            int p = n + 1;
            jlu[0] = p;

            // Initialize work vector.
            for (i = 0; i < n; i++)
            {
                iw[i] = -1;
            }

            // The main loop.
            for (i = 0; i < n; i++)
            {
                int pold = p;

                // Generating row i of L and U.
                int j;
                for (j = ia[i]; j < ia[i + 1]; j++)
                {
                    // Copy row i of A, JA, IA into row i of ALU, JLU (LU matrix).
                    int jcol = ja[j];

                    if (jcol == i)
                    {
                        alu[i] = a[j];
                        iw[jcol] = i;
                        ju[i] = p;
                    }
                    else
                    {
                        alu[p] = a[j];
                        jlu[p] = ja[j];
                        iw[jcol] = p;
                        p = p + 1;
                    }
                }

                jlu[i + 1] = p;

                double s = 0.0;

                int k;
                for (j = pold; j < ju[i]; j++)
                {
                    int jrow = jlu[j];
                    double tl = alu[j] * alu[jrow];
                    alu[j] = tl;

                    // Perform linear combination.
                    for (k = ju[jrow]; k < jlu[jrow + 1]; k++)
                    {
                        int jw = iw[jlu[k]];
                        if (jw != -1)
                        {
                            alu[jw] = alu[jw] - tl * alu[k];
                        }
                        else
                        {
                            // Accumulate fill-in values.
                            s = s + tl * alu[k];
                        }
                    }
                }

                if (modified)
                {
                    alu[i] = alu[i] - s;
                }

                if (alu[i] == 0.0)
                {
                    return i;
                }

                // Invert and store diagonal element.
                alu[i] = 1.0 / alu[i];

                // Reset pointers in work array.
                iw[i] = -1;
                for (k = pold; k < p; k++)
                {
                    iw[jlu[k]] = -1;
                }
            }

            return -1;
        }
    }
}
