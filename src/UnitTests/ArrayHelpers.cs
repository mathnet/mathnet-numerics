// <copyright file="ArrayHelpers.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

#if PORTABLE
using System.Collections.Generic;
using System.Linq;
#endif

namespace MathNet.Numerics.UnitTests
{
    using System;

    /// <summary>
    /// Array and List helper/extention for Silverlight
    /// </summary>
    internal static class ArrayHelpers
    {
        /// <summary>
        /// Converts an array of one type to an array of another type.
        /// </summary>
        /// <typeparam name="TInput">The type of the elements of the source array.</typeparam>
        /// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
        /// <param name="array">The one-dimensional, zero-based Array to convert to a target type.</param>
        /// <param name="converter">A Converter that converts each element from one type to another type.</param>
        /// <returns>An array of the target type containing the converted elements from the source array.</returns>
        public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
        {
#if PORTABLE
            if (array == null)
                throw new ArgumentException();

            return (from item in array select converter(item)).ToArray();
#else
            return Array.ConvertAll(array, converter);
#endif
        }

#if PORTABLE
    /// <summary>
    /// Determines whether the specified array contains elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the array.</typeparam>
    /// <param name="list">The one-dimensional, zero-based Array to search.</param>
    /// <param name="match">The Predicate that defines the conditions of the elements to search for.</param>
    /// <returns>true if array contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
        public static bool Exists<T>(this List<T> list, Predicate<T> match)
        {
            if (list == null)
                throw new ArgumentException();

            return list.Any(item => match(item));
        }
#endif
    }
}
