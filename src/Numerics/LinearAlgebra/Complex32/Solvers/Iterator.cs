// <copyright file="Iterator.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

using MathNet.Numerics.LinearAlgebra.Complex32.Solvers.StopCriterium;
using MathNet.Numerics.LinearAlgebra.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers.StopCriterium;

namespace MathNet.Numerics.LinearAlgebra.Complex32.Solvers
{
    using Numerics;

    /// <summary>
    /// An iterator that is used to check if an iterative calculation should continue or stop.
    /// </summary>
    public static class Iterator
    {
        /// <summary>
        /// Creates a default iterator with all the <see cref="IIterationStopCriterium{T}"/> objects.
        /// </summary>
        /// <returns>A new <see cref="IIterator{T}"/> object.</returns>
        public static IIterator<Complex32> CreateDefault()
        {
            var iterator = new Iterator<Complex32>();
            iterator.Add(new FailureStopCriterium());
            iterator.Add(new DivergenceStopCriterium());
            iterator.Add(new IterationCountStopCriterium<Complex32>());
            iterator.Add(new ResidualStopCriterium());

            return iterator;
        }
    }
}
