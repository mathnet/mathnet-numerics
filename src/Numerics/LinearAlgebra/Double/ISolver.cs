// <copyright file="ISolver.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.LinearAlgebra.Double
{
    /// <summary>
    /// Classes that solves a system of linear equations, <c>AX = B</c>.
    /// </summary>
    public interface ISolver
    {
        /// <summary>
        /// Solves a system of linear equations, <c>AX = B</c>.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix"/>, <c>B</c>.</param>
        /// <returns>The left hand side <see cref="Matrix"/>, <c>X</c>.</returns>
        Matrix Solve(Matrix input);

        /// <summary>
        /// Solves a system of linear equations, <c>AX = B</c>.
        /// </summary>
        /// <param name="input">The right hand side <see cref="Matrix"/>, <c>B</c>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <c>X</c>.</param>
        void Solve(Matrix input, Matrix result);

        /// <summary>
        /// Solves a system of linear equations, <c>Ax = b</c>
        /// </summary>
        /// <param name="input">The right hand side vector, <c>b</c>.</param>
        /// <returns>The left hand side <see cref="Vector"/>, <c>x</c>.</returns>
        Vector Solve(Vector input);

        /// <summary>
        /// Solves a system of linear equations, <c>Ax = b</c>.
        /// </summary>
        /// <param name="input">The right hand side vector, <c>b</c>.</param>
        /// <param name="result">The left hand side <see cref="Matrix"/>, <c>x</c>.</param>
        void Solve(Vector input, Vector result);
    }
}