// <copyright file="Milu0.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2015 Math.NET
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Integer.Solvers
{
    /// <summary>
    /// A simple milu(0) preconditioner.
    /// </summary>
    /// <remarks>
    /// Original Fortran code by Youcef Saad (07 January 2004)
    /// </remarks>
    public sealed class MILU0Preconditioner : IPreconditioner<int>
    {
        /// <param name="modified">Use modified or standard ILU(0)</param>
        public MILU0Preconditioner(bool modified = true)
        {
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use modified or standard ILU(0).
        /// <remarks>Shouldn't be possible as this cannot be constructed</remarks>
        /// </summary>
        public bool UseModified { get; set; }

        /// <summary>
        /// Gets a value indicating whether the preconditioner is initialized.
        /// <remarks>Shouldn't be possible as this cannot be constructed</remarks>
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">The matrix upon which the preconditioner is based. </param>
        /// <exception cref="ArgumentNullException">If <paramref name="matrix"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square or is not an
        /// instance of SparseCompressedRowMatrixStorage.</exception>
        public void Initialize(Matrix<int> matrix)
        {
            // Shouldn't be possible as this cannot be constructed
            throw new NotSupportedException(Resources.NotSupportedForIntegerMatrices);
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="input">The right hand side vector b.</param>
        /// <param name="result">The left hand side vector x.</param>
        public void Approximate(Vector<int> input, Vector<int> result)
        {
            // Shouldn't be possible as this cannot be constructed
            throw new NotSupportedException(Resources.NotSupportedForIntegerVectors);
        }
    }
}
