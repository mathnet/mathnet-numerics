// <copyright file="MatlabStructure.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
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

using MathNet.Numerics.LinearAlgebra;
using OneOf;
using System.Collections.Generic;
using System.Text;

namespace MathNet.Numerics.Data.Matlab
{
    public class MatlabStructure : Dictionary<string, NestedObject>
    {
    }

    public class MatlabCellMatrix
    {
        public NestedObject[,] Data { get; private set; }

        public MatlabCellMatrix(int rows, int cols)
        {
            Data = new NestedObject[rows, cols];
        }
    }

    public class MatlabCharMatrix
    {
        /// <summary>
        /// Typically not that relevant, for UTF32 encoding however each single char is actually 2 chars (hence the string type)
        /// </summary>
        public Encoding Encoding { get; private set; }
        public string[,] Data { get; private set; }

        public MatlabCharMatrix(int rows, int cols, Encoding encoding)
        {
            Encoding = encoding;

            Data = new string[rows, cols];
        }

        /// <summary>
        /// Returns each row as a single string
        /// </summary>
        /// <returns></returns>
        public string[] ConcatRows()
        {
            string[] result = new string[Data.GetLength(0)];
            for (int col = 0; col < Data.GetLength(1); col++)
            {
                for (int row = 0; row < Data.GetLength(0); row++)

                {
                    result[row] += Data[row, col];
                }
            }

            return result;
        }
    }

    public class NestedObject : OneOfBase<MatlabStructure, MatlabCellMatrix, MatlabCharMatrix, MatlabMatrix>
    {
        public NestedObject(OneOf<MatlabStructure, MatlabCellMatrix, MatlabCharMatrix, MatlabMatrix> input) : base(input)
        {
        }
    }
}
