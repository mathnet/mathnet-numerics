// <copyright file="DataType.cs" company="Math.NET">
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
    /// MATLAB data types
    /// </summary>
    internal enum DataType
    {
        /// <summary>
        /// Unkown type
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// miINT8 type
        /// </summary>
        Int8 = 1,

        /// <summary>
        ///  miUINT8 type
        /// </summary>
        UInt8 = 2,

        /// <summary>
        ///  miINT16 type
        /// </summary>
        Int16 = 3,

        /// <summary>
        ///  miUINT16 type
        /// </summary>
        UInt16 = 4,

        /// <summary>
        ///  miINT32 type
        /// </summary>
        Int32 = 5,

        /// <summary>
        ///  miUINT32 type
        /// </summary>
        UInt32 = 6,

        /// <summary>
        ///  miSINGLE type
        /// </summary>
        Single = 7,

        /// <summary>
        ///  miDOUBLE type
        /// </summary>
        Double = 9,

        /// <summary>
        ///  miINT64 type
        /// </summary>
        Int64 = 12,

        /// <summary>
        ///  miUINT6 4type
        /// </summary>
        UInt64 = 13,

        /// <summary>
        /// miMATRIX type
        /// </summary>
        Matrix = 14,

        /// <summary>
        ///  miCOMPRESSED type
        /// </summary>
        Compressed = 15,

        /// <summary>
        ///  miUTF8 type
        /// </summary>
        Utf8 = 16,

        /// <summary>
        ///  miUTF16 type
        /// </summary>
        Utf16 = 17,

        /// <summary>
        ///  miUTF32 type
        /// </summary>
        Utf32 = 18
    }
}
