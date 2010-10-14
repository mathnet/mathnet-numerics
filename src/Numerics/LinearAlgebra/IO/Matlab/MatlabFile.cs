// <copyright file="MatlabFile.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.IO.Matlab
{
    using System;
    using System.Collections.Generic;
    using Generic;

    /// <summary>
    /// Represents a Matlab file
    /// </summary>
    /// <typeparam name="TDataType">The data type of the matrix to return.</typeparam>
    internal class MatlabFile<TDataType> where TDataType : struct, IEquatable<TDataType>, IFormattable 
    {
        /// <summary>
        /// Matrices in a matlab file stored as 1-D arrays
        /// </summary>
        private readonly IDictionary<string, Matrix<TDataType>> _matrices = new SortedList<string, Matrix<TDataType>>();
       
        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        /// <value>The header text.</value>
        public string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the first name of the matrix.
        /// </summary>
        /// <value>The first name of the matrix.</value>
        public string FirstMatrixName { get; set; }

        /// <summary>
        /// Gets the first matrix.
        /// </summary>
        /// <value>The first matrix.</value>
        public Matrix<TDataType> FirstMatrix
        {
            get
            {
                if (string.IsNullOrEmpty(FirstMatrixName) || !_matrices.ContainsKey(FirstMatrixName))
                {
                    return null;
                }

                return _matrices[FirstMatrixName];
            }
        }

        /// <summary>
        /// Gets the matrices.
        /// </summary>
        /// <value>The matrices.</value>
        public IDictionary<string, Matrix<TDataType>> Matrices
        {
            get { return _matrices; }
        }
    }
}
