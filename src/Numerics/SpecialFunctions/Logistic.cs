// <copyright file="Logistic.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2011 Math.NET
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

// <contribution>
//    Cephes Math Library, Stephen L. Moshier
//    ALGLIB 2.0.1, Sergey Bochkanov
// </contribution>

// ReSharper disable CheckNamespace
namespace MathNet.Numerics
// ReSharper restore CheckNamespace
{
    using System;
    using Properties;

    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the logistic function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Computes the logistic function. see: http://en.wikipedia.org/wiki/Logistic
        /// </summary>
        /// <param name="p">The parameter for which to compute the logistic function.</param>
        /// <returns>The logistic function of <paramref name="p"/>.</returns>
        public static double Logistic(double p)
        {
            return 1.0 / (Math.Exp(-p) + 1.0);
        }

        /// <summary>
        /// Computes the logit function, the inverse of the sigmoid logistic function. see: http://en.wikipedia.org/wiki/Logit
        /// </summary>
        /// <param name="p">The parameter for which to compute the logit function. This number should be
        /// between 0 and 1.</param>
        /// <returns>The logarithm of <paramref name="p"/> divided by 1.0 - <paramref name="p"/>.</returns>
        public static double Logit(double p)
        {
            if (p < 0.0 || p > 1.0)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentBetween0And1);
            }

            return Math.Log(p / (1.0 - p));
        }
    }
}
