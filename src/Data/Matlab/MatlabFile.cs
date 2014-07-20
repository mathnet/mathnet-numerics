// <copyright file="MatlabFile.cs" company="Math.NET">
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
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Data.Matlab
{
    /// <summary>
    /// Represents a MATLAB file
    /// </summary>
    internal class MatlabFile
    {
        /// <summary>
        /// Matrices in a MATLAB file stored as 1-D arrays
        /// </summary>
        readonly IDictionary<string, byte[]> _matrices = new SortedList<string, byte[]>();

        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        /// <value>The header text.</value>
        internal string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the first name of the matrix.
        /// </summary>
        /// <value>The first name of the matrix.</value>
        internal string FirstMatrixName { get; private set; }

        internal ICollection<string> MatrixNames
        {
            get { return _matrices.Keys; }
        }

        internal void Add(string name, byte[] data)
        {
            if (FirstMatrixName == null)
            {
                FirstMatrixName = name;
            }

            _matrices.Add(name, data);
        }

        internal Matrix<TDataType> ReadMatrix<TDataType>(string name) where TDataType : struct, IEquatable<TDataType>, IFormattable
        {
            return Parser<TDataType>.ReadMatrix(_matrices[name]);
        }
    }
}
