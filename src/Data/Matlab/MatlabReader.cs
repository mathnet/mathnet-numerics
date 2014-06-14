// <copyright file="MatlabReader.cs" company="Math.NET">
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
using System.IO;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Data.Matlab
{
    /// <summary>
    /// Creates matrices from Matlab files.
    /// </summary>
    public static class MatlabMatrixReader
    {
        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Matrix<TDataType> ReadMatrix<TDataType>(Stream stream, string matrixName = null)
            where TDataType : struct, IEquatable<TDataType>, IFormattable
        {
            var names = string.IsNullOrEmpty(matrixName) ? new string[] { } : new[] { matrixName };
            var parser = new MatlabParser<TDataType>(stream, names);
            var file = parser.Parse();

            if (string.IsNullOrEmpty(matrixName))
            {
                return file.FirstMatrix;
            }
            if (!file.Matrices.ContainsKey(matrixName))
            {
                throw new KeyNotFoundException("Matrix with the provided name was not found.");
            }
            return file.Matrices[matrixName];
        }

        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Matrix<TDataType> ReadMatrix<TDataType>(string filePath, string matrixName = null)
            where TDataType : struct, IEquatable<TDataType>, IFormattable
        {
            using (var stream = File.OpenRead(filePath))
            {
                return ReadMatrix<TDataType>(stream, matrixName);
            }
        }

        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Dictionary<string, Matrix<TDataType>> ReadMatrices<TDataType>(Stream stream, params string[] matrixNames)
            where TDataType : struct, IEquatable<TDataType>, IFormattable
        {
            var parser = new MatlabParser<TDataType>(stream, matrixNames);
            var file = parser.Parse();
            return file.Matrices.ToDictionary(matrix => matrix.Key, matrix => matrix.Value);
        }

        /// <typeparam name="TDataType">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Dictionary<string, Matrix<TDataType>> ReadMatrices<TDataType>(string filePath, params string[] matrixNames)
            where TDataType : struct, IEquatable<TDataType>, IFormattable
        {
            using (var stream = File.OpenRead(filePath))
            {
                return ReadMatrices<TDataType>(stream, matrixNames);
            }
        }
    }
}
