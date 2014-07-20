// <copyright file="ArrayClass.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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

namespace MathNet.Numerics.Data.Matlab
{
    /// <summary>
    /// Enumeration for the MATLAB array types
    /// </summary>
    internal enum ArrayClass : byte
    {
        /// <summary>
        /// mxUNKNOWN CLASS
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// mxCELL CLASS
        /// </summary>
        Cell = 1,

        /// <summary>
        ///  mxSTRUCT CLASS
        /// </summary>
        Structure = 2,

        /// <summary>
        ///  mxOBJECT CLASS
        /// </summary>
        Object = 3,

        /// <summary>
        /// mxCHAR CLASS
        /// </summary>
        Character = 4,

        /// <summary>
        /// mxSPARSE CLASS
        /// </summary>
        Sparse = 5,

        /// <summary>
        /// mxDOUBLE CLASS
        /// </summary>
        Double = 6,

        /// <summary>
        /// mxSINGLE CLASS
        /// </summary>
        Single = 7,

        /// <summary>
        /// mxINT8 CLASS
        /// </summary>
        Int8 = 8,

        /// <summary>
        /// mxUINT8 CLASS
        /// </summary>
        UInt8 = 9,

        /// <summary>
        /// mxINT16 CLASS
        /// </summary>
        Int16 = 10,

        /// <summary>
        /// mxUINT16 CLASS
        /// </summary>
        UInt16 = 11,

        /// <summary>
        /// mxINT32 CLASS
        /// </summary>
        Int32 = 12,

        /// <summary>
        /// mxUINT32 CLASS
        /// </summary>
        UInt32 = 13,

        /// <summary>
        ///  mxINT64 CLASS
        /// </summary>
        Int64 = 14,

        /// <summary>
        /// mxUINT64 CLASS
        /// </summary>
        UInt64 = 15,

        /// <summary>
        ///  mxFUNCTION CLASS
        /// </summary>
        Function = 16
    }
}
