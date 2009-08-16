// <copyright file="ManagedLinearAlgebra.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
// Copyright (c) 2009 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Algorithms.LinearAlgebra
{
    /// <summary>
    /// The managed linear algebra provider.
    /// </summary>
    internal class ManagedLinearAlgebra : ILinearAlgebra
    {
        #region ILinearAlgebra Members

        /// <summary>
        /// Adds the two arrays together: <c>a += c</c>.
        /// </summary>
        /// <param name="a">
        /// One of the arrays to add.
        /// </param>
        /// <param name="b">
        /// The other array to add.
        /// </param>
        public void AddArrays(double[] a, double[] b)
        {
            if (a == null)
            {
                throw new ArgumentNullException("a");
            }

            if (b == null)
            {
                throw new ArgumentNullException("b");
            }

            if (a.Length != b.Length)
            {
                throw new ArgumentException(Properties.Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, a.Length, i => a[i] += b[i]);
        }

        #endregion
    }
}