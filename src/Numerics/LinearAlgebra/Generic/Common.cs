// <copyright file="Common.cs" company="Math.NET">
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


namespace MathNet.Numerics.LinearAlgebra
{
    using System;

    /// <summary>
    /// A setup functions to help simplify the generic code.
    /// </summary>
    internal static class Common
    {
        /// <summary>
        /// Sets the value of <c>1.0</c> for type T.
        /// </summary>
        /// <typeparam name="T">The type to return the value of 1.0 of.</typeparam>
        /// <returns>The value of <c>1.0</c> for type T.</returns>
        public static T OneOf<T>()
        {
            if (typeof(T) == typeof(System.Numerics.Complex))
            {
                return (T)(object)System.Numerics.Complex.One;
            }

            if (typeof(T) == typeof(Numerics.Complex32))
            {
                return (T)(object)Numerics.Complex32.One;
            }

            if (typeof(T) == typeof(double))
            {
                return (T)(object)1.0d;
            }

            if (typeof(T) == typeof(float))
            {
                return (T)(object)1.0f;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the value of <c>0.0</c> for type T.
        /// </summary>
        /// <typeparam name="T">The type to return the value of 0.0 of.</typeparam>
        /// <returns>The value of <c>0.0</c> for type T.</returns>
        public static T ZeroOf<T>()
        {
            if (typeof(T) == typeof(System.Numerics.Complex))
            {
                return (T)(object)System.Numerics.Complex.Zero;
            }

            if (typeof(T) == typeof(Numerics.Complex32))
            {
                return (T)(object)Numerics.Complex32.Zero;
            }

            if (typeof(T) == typeof(double))
            {
                return (T)(object)0.0d;
            }

            if (typeof(T) == typeof(float))
            {
                return (T)(object)0.0f;
            }

            return default(T);
        }
    }
}
