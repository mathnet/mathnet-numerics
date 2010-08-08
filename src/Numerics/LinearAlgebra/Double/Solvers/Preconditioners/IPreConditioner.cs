// <copyright file="IPreConditioner.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Double.Solvers.Preconditioners
{
    /// <summary>
    /// The base interface for preconditioner classes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Preconditioners are used by iterative solvers to improve the convergence
    /// speed of the solving process. Increase in convergence speed
    /// is related to the number of iterations necessary to get a converged solution.
    /// So while in general the use of a preconditioner means that the iterative 
    /// solver will perform fewer iterations it does not guarantee that the actual
    /// solution time decreases given that some preconditioners can be expensive to 
    /// setup and run.
    /// </para>
    /// <para>
    /// Note that in general changes to the matrix will invalidate the preconditioner
    /// if the changes occur after creating the preconditioner.
    /// </para>
    /// </remarks>
    public interface IPreConditioner
    {
        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">The matrix on which the preconditioner is based.</param>
        void Initialize(Matrix matrix);

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Mx = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <returns>The left hand side vector.</returns>
        Vector Approximate(Vector rhs);

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Mx = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <param name="lhs">The left hand side vector. Also known as the result vector.</param>
        void Approximate(Vector rhs, Vector lhs);
    }
}
