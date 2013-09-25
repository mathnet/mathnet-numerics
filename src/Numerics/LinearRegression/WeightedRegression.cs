// <copyright file="LocallyWeightedRegression.cs" company="Math.NET">
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

using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace MathNet.Numerics.LinearRegression
{
    public static class WeightedRegression
    {
        /// <summary>
        /// Weighted Linear Regression using normal equations.
        /// </summary>
        public static Vector<T> Weighted<T>(Matrix<T> x, Vector<T> y, Matrix<T> w) where T : struct, IEquatable<T>, IFormattable
        {
            return x.TransposeThisAndMultiply(w * x).Cholesky().Solve(x.Transpose() * (w * y));
        }

        /// <summary>
        /// Weighted Linear Regression using normal equations.
        /// </summary>
        public static Matrix<T> Weighted<T>(Matrix<T> x, Matrix<T> y, Matrix<T> w) where T : struct, IEquatable<T>, IFormattable
        {
            return x.TransposeThisAndMultiply(w * x).Cholesky().Solve(x.Transpose() * (w * y));
        }

        /// <summary>
        /// Weighted Linear Regression using normal equations.
        /// </summary>
        /// <param name="intercept">True if an intercept should be added as first artificial perdictor value. Default = false.</param>
        public static T[] Weighted<T>(T[][] x, T[] y, T[] w, bool intercept = false) where T : struct, IEquatable<T>, IFormattable
        {
            var predictor = Matrix<T>.Build.DenseMatrixOfRowArrays(x);
            if (intercept)
            {
                predictor = predictor.InsertColumn(0, Vector<T>.Build.DenseVector(predictor.RowCount, Vector<T>.One));
            }
            var response = Matrix<T>.Build.DenseVector(y);
            var weights = Matrix<T>.Build.DiagonalMatrix(new DiagonalMatrixStorage<T>(predictor.RowCount, predictor.RowCount, w));
            return predictor.TransposeThisAndMultiply(weights * predictor).Cholesky().Solve(predictor.Transpose() * (weights * response)).ToArray();
        }

        /// <summary>
        /// Weighted Linear Regression using normal equations.
        /// </summary>
        /// <param name="intercept">True if an intercept should be added as first artificial perdictor value. Default = false.</param>
        public static T[] Weighted<T>(IEnumerable<Tuple<T[], T>> samples, T[] w, bool intercept = false) where T : struct, IEquatable<T>, IFormattable
        {
            var xy = samples.UnpackSinglePass();
            return Weighted(xy.Item1, xy.Item2, w, intercept);
        }

        /// <summary>
        /// Locally-Weighted Linear Regression using normal equations.
        /// </summary>
        public static Vector<T> Local<T>(Matrix<T> x, Vector<T> y, Vector<T> t, Func<Vector<T>, Vector<T>, T> kernel) where T : struct, IEquatable<T>, IFormattable
        {
            // TODO: Kernel definition is a bit weird as it includes computing the difference norm
            // We can make this more common once we change the norm to always be of type double around LA.

            var w = Matrix<T>.Build.DenseMatrix(x.RowCount, x.RowCount);
            for (int i = 0; i < x.RowCount; i++)
            {
                w.At(i, i, kernel(t, x.Row(i)));
            }
            return Weighted(x, y, w);
        }

        /// <summary>
        /// Locally-Weighted Linear Regression using normal equations.
        /// </summary>
        public static Matrix<T> Local<T>(Matrix<T> x, Matrix<T> y, Vector<T> t, Func<Vector<T>, Vector<T>, T> kernel) where T : struct, IEquatable<T>, IFormattable
        {
            // TODO: Kernel definition is a bit weird as it includes computing the difference norm
            // We can make this more common once we change the norm to always be of type double around LA.

            var w = Matrix<T>.Build.DenseMatrix(x.RowCount, x.RowCount);
            for (int i = 0; i < x.RowCount; i++)
            {
                w.At(i, i, kernel(t, x.Row(i)));
            }
            return Weighted(x, y, w);
        }

        public static Func<Vector<double>, Vector<double>, double> GaussianKernel(double radius)
        {
            // TODO: see above...
            var d = -2.0*radius*radius;
            return (t, x) => Math.Exp(Distance.SSD(x, t)/d);
        }
    }
}
