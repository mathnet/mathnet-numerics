// <copyright file="Util.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using System;
using System.Collections.Generic;

namespace MathNet.Numerics.LinearRegression
{
    internal static class Util
    {
        public static (TU[] U, TV[] V) UnpackSinglePass<TU, TV>(this IEnumerable<Tuple<TU, TV>> samples)
        {
            var ux = new List<TU>();
            var vx = new List<TV>();

            foreach (var tuple in samples)
            {
                ux.Add(tuple.Item1);
                vx.Add(tuple.Item2);
            }

            return (ux.ToArray(), vx.ToArray());
        }

        public static (TU[] U, TV[] V) UnpackSinglePass<TU, TV>(this IEnumerable<(TU, TV)> samples)
        {
            var ux = new List<TU>();
            var vx = new List<TV>();

            foreach (var (u, v) in samples)
            {
                ux.Add(u);
                vx.Add(v);
            }

            return (ux.ToArray(), vx.ToArray());
        }
    }
}
