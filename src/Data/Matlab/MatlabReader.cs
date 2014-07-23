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
    /// Creates matrices from MATLAB Level-5 Mat files.
    /// </summary>
    public static class MatlabReader
    {
        /// <summary>
        /// List all compatible matrices from a MATLAB file stream.
        /// </summary>
        public static List<MatlabMatrix> List(Stream stream)
        {
            return Parser.ParseFile(stream);
        }

        /// <summary>
        /// List all compatible matrices from a MATLAB file.
        /// </summary>
        public static List<MatlabMatrix> List(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return List(stream);
            }
        }

        /// <summary>
        /// Unpacks the matrix of a MATLAB matrix data object.
        /// </summary>
        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Matrix<T> Unpack<T>(MatlabMatrix matrixData)
            where T : struct, IEquatable<T>, IFormattable
        {
            return Parser.ParseMatrix<T>(matrixData.Data);
        }

        /// <summary>
        /// Read the first or a specific matrix from a MATLAB file stream.
        /// </summary>
        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Matrix<T> Read<T>(Stream stream, string matrixName = null)
            where T : struct, IEquatable<T>, IFormattable
        {
            var matrices = List(stream);

            if (string.IsNullOrEmpty(matrixName))
            {
                return Unpack<T>(matrices.First());
            }

            var matrix = matrices.Find(m => m.Name == matrixName);
            if (matrix == null)
            {
                throw new KeyNotFoundException("Matrix with the provided name was not found.");
            }

            return Unpack<T>(matrix);
        }

        /// <summary>
        /// Read the first or a specific matrix from a MATLAB file.
        /// </summary>
        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Matrix<T> Read<T>(string filePath, string matrixName = null)
            where T : struct, IEquatable<T>, IFormattable
        {
            using (var stream = File.OpenRead(filePath))
            {
                return Read<T>(stream, matrixName);
            }
        }

        /// <summary>
        /// Read all matrices or those with matching name from a MATLAB file stream.
        /// </summary>
        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Dictionary<string, Matrix<T>> ReadAll<T>(Stream stream, params string[] matrixNames)
            where T : struct, IEquatable<T>, IFormattable
        {
            var names = new HashSet<string>(matrixNames);
            return List(stream)
                .Where(m => names.Count == 0 || names.Contains(m.Name))
                .ToDictionary(m => m.Name, Unpack<T>);
        }

        /// <summary>
        /// Read all matrices or those with matching name from a MATLAB file.
        /// </summary>
        /// <typeparam name="T">The data type of the Matrix. It can be either: double, float, Complex, or Complex32.</typeparam>
        public static Dictionary<string, Matrix<T>> ReadAll<T>(string filePath, params string[] matrixNames)
            where T : struct, IEquatable<T>, IFormattable
        {
            using (var stream = File.OpenRead(filePath))
            {
                return ReadAll<T>(stream, matrixNames);
            }
        }
    }
}
