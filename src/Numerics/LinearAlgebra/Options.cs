// <copyright file="Options.cs" company="Math.NET">
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


namespace MathNet.Numerics.LinearAlgebra
{
    public enum ExistingData
    {
        /// <summary>
        /// Existing data may not be all zeros, so clearing may be necessary
        /// if not all of it will be overwritten anyway.
        /// </summary>
        Clear = 0,

        /// <summary>
        /// If existing data is assumed to be all zeros already,
        /// clearing it may be skipped if applicable.
        /// </summary>
        AssumeZeros = 1
    }

    public enum Zeros
    {
        /// <summary>
        /// Allow skipping zero entries (without enforcing skipping them).
        /// When enumerating sparse matrices this can significantly speed up operations.
        /// </summary>
        AllowSkip = 0,

        /// <summary>
        /// Force applying the operation to all fields even if they are zero.
        /// </summary>
        Include = 1
    }

    public enum Symmetricity
    {
        /// <summary>
        /// It is not known yet whether a matrix is symmetric or not.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A matrix is symmetric
        /// </summary>
        Symmetric = 1,

        /// <summary>
        /// A matrix is Hermitian (conjugate symmetric).
        /// </summary>
        Hermitian = 2,

        /// <summary>
        /// A matrix is not symmetric
        /// </summary>
        Asymmetric = 3
    }
}
