// <copyright file="ArrayExtensions.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2011 Math.NET
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

namespace MathNet.Numerics
{
    using System;
    using System.Numerics;

    /// <summary>
    /// Useful extension methods for Arrays.
    /// </summary>
    internal static class ArrayExtensions
    {
        /// <summary>
        /// Copies the values from on array to another.
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="dest">The destination array.</param>
        public static void Copy(this double[] source, double[] dest)
        {
            Buffer.BlockCopy(source, 0, dest, 0, source.Length * Constants.SizeOfDouble);
        }

        /// <summary>
        /// Copies the values from on array to another.
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="dest">The destination array.</param>
        public static void Copy(this float[] source, float[] dest)
        {
            Buffer.BlockCopy(source, 0, dest, 0, source.Length * Constants.SizeOfFloat);
        }

        /// <summary>
        /// Copies the values from on array to another.
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="dest">The destination array.</param>
        public static void Copy(this Complex[] source, Complex[] dest)
        {
            Array.Copy(source, 0, dest, 0, source.Length);
        }
        
        /// <summary>
        /// Copies the values from on array to another.
        /// </summary>
        /// <param name="source">The source array.</param>
        /// <param name="dest">The destination array.</param>
        public static void Copy(this Complex32[] source, Complex32[] dest)
        {
            Array.Copy(source, 0, dest, 0, source.Length);
        }
    }
}
