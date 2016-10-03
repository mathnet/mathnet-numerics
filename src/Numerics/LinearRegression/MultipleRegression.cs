// <copyright file="MultipleRegression.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearRegression
{
    public static class MultipleRegression
    {
        /// <summary>
        /// Find the model parameters β such that X*β with predictor X becomes as close to response Y as possible, with least squares residuals.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response vector Y</param>
        /// <param name="method">The direct method to be used to compute the regression.</param>
        /// <returns>Best fitting vector for model parameters β</returns>
        public static Vector<T> DirectMethod<T>(Matrix<T> x, Vector<T> y, DirectRegressionMethod method = DirectRegressionMethod.NormalEquations) where T : struct, IEquatable<T>, IFormattable
        {
            switch (method)
            {
                case DirectRegressionMethod.NormalEquations:
                    return NormalEquations(x, y);
                case DirectRegressionMethod.QR:
                    return QR(x, y);
                case DirectRegressionMethod.Svd:
                    return Svd(x, y);
                default:
                    throw new NotSupportedException(method.ToString());
            }
        }

        /// <summary>
        /// Find the model parameters β such that X*β with predictor X becomes as close to response Y as possible, with least squares residuals.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response matrix Y</param>
        /// <param name="method">The direct method to be used to compute the regression.</param>
        /// <returns>Best fitting vector for model parameters β</returns>
        public static Matrix<T> DirectMethod<T>(Matrix<T> x, Matrix<T> y, DirectRegressionMethod method = DirectRegressionMethod.NormalEquations) where T : struct, IEquatable<T>, IFormattable
        {
            switch (method)
            {
                case DirectRegressionMethod.NormalEquations:
                    return NormalEquations(x, y);
                case DirectRegressionMethod.QR:
                    return QR(x, y);
                case DirectRegressionMethod.Svd:
                    return Svd(x, y);
                default:
                    throw new NotSupportedException(method.ToString());
            }
        }

        /// <summary>
        /// Find the model parameters β such that their linear combination with all predictor-arrays in X become as close to their response in Y as possible, with least squares residuals.
        /// </summary>
        /// <param name="x">List of predictor-arrays.</param>
        /// <param name="y">List of responses</param>
        /// <param name="intercept">True if an intercept should be added as first artificial predictor value. Default = false.</param>
        /// <param name="method">The direct method to be used to compute the regression.</param>
        /// <returns>Best fitting list of model parameters β for each element in the predictor-arrays.</returns>
        public static T[] DirectMethod<T>(T[][] x, T[] y, bool intercept = false, DirectRegressionMethod method = DirectRegressionMethod.NormalEquations) where T : struct, IEquatable<T>, IFormattable
        {
            switch (method)
            {
                case DirectRegressionMethod.NormalEquations:
                    return NormalEquations(x, y, intercept);
                case DirectRegressionMethod.QR:
                    return QR(x, y, intercept);
                case DirectRegressionMethod.Svd:
                    return Svd(x, y, intercept);
                default:
                    throw new NotSupportedException(method.ToString());
            }
        }

        /// <summary>
        /// Find the model parameters β such that their linear combination with all predictor-arrays in X become as close to their response in Y as possible, with least squares residuals.
        /// Uses the cholesky decomposition of the normal equations.
        /// </summary>
        /// <param name="samples">Sequence of predictor-arrays and their response.</param>
        /// <param name="intercept">True if an intercept should be added as first artificial predictor value. Default = false.</param>
        /// <param name="method">The direct method to be used to compute the regression.</param>
        /// <returns>Best fitting list of model parameters β for each element in the predictor-arrays.</returns>
        public static T[] DirectMethod<T>(IEnumerable<Tuple<T[], T>> samples, bool intercept = false, DirectRegressionMethod method = DirectRegressionMethod.NormalEquations) where T : struct, IEquatable<T>, IFormattable
        {
            switch (method)
            {
                case DirectRegressionMethod.NormalEquations:
                    return NormalEquations(samples, intercept);
                case DirectRegressionMethod.QR:
                    return QR(samples, intercept);
                case DirectRegressionMethod.Svd:
                    return Svd(samples, intercept);
                default:
                    throw new NotSupportedException(method.ToString());
            }
        }

        /// <summary>
        /// Find the model parameters β such that X*β with predictor X becomes as close to response Y as possible, with least squares residuals.
        /// Uses the cholesky decomposition of the normal equations.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response vector Y</param>
        /// <returns>Best fitting vector for model parameters β</returns>
        public static Vector<T> NormalEquations<T>(Matrix<T> x, Vector<T> y) where T : struct, IEquatable<T>, IFormattable
        {
            if (x.RowCount != y.Count)
            {
                throw new ArgumentException(string.Format(Resources.SampleVectorsSameLength, x.RowCount, y.Count));
            }

            if (x.ColumnCount > y.Count)
            {
                throw new ArgumentException(string.Format(Resources.RegressionNotEnoughSamples, x.ColumnCount, y.Count));
            }

            return x.TransposeThisAndMultiply(x).Cholesky().Solve(x.TransposeThisAndMultiply(y));
        }

        /// <summary>
        /// Find the model parameters β such that X*β with predictor X becomes as close to response Y as possible, with least squares residuals.
        /// Uses the cholesky decomposition of the normal equations.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response matrix Y</param>
        /// <returns>Best fitting vector for model parameters β</returns>
        public static Matrix<T> NormalEquations<T>(Matrix<T> x, Matrix<T> y) where T : struct, IEquatable<T>, IFormattable
        {
            if (x.RowCount != y.RowCount)
            {
                throw new ArgumentException(string.Format(Resources.SampleVectorsSameLength, x.RowCount, y.RowCount));
            }

            if (x.ColumnCount > y.RowCount)
            {
                throw new ArgumentException(string.Format(Resources.RegressionNotEnoughSamples, x.ColumnCount, y.RowCount));
            }

            return x.TransposeThisAndMultiply(x).Cholesky().Solve(x.TransposeThisAndMultiply(y));
        }

        /// <summary>
        /// Find the model parameters β such that their linear combination with all predictor-arrays in X become as close to their response in Y as possible, with least squares residuals.
        /// Uses the cholesky decomposition of the normal equations.
        /// </summary>
        /// <param name="x">List of predictor-arrays.</param>
        /// <param name="y">List of responses</param>
        /// <param name="intercept">True if an intercept should be added as first artificial predictor value. Default = false.</param>
        /// <returns>Best fitting list of model parameters β for each element in the predictor-arrays.</returns>
        public static T[] NormalEquations<T>(T[][] x, T[] y, bool intercept = false) where T : struct, IEquatable<T>, IFormattable
        {
            var predictor = Matrix<T>.Build.DenseOfRowArrays(x);
            if (intercept)
            {
                predictor = predictor.InsertColumn(0, Vector<T>.Build.Dense(predictor.RowCount, Vector<T>.One));
            }

            if (predictor.RowCount != y.Length)
            {
                throw new ArgumentException(string.Format(Resources.SampleVectorsSameLength, predictor.RowCount, y.Length));
            }

            if (predictor.ColumnCount > y.Length)
            {
                throw new ArgumentException(string.Format(Resources.RegressionNotEnoughSamples, predictor.ColumnCount, y.Length));
            }

            var response = Vector<T>.Build.Dense(y);
            return predictor.TransposeThisAndMultiply(predictor).Cholesky().Solve(predictor.TransposeThisAndMultiply(response)).ToArray();
        }

        /// <summary>
        /// Find the model parameters β such that their linear combination with all predictor-arrays in X become as close to their response in Y as possible, with least squares residuals.
        /// Uses the cholesky decomposition of the normal equations.
        /// </summary>
        /// <param name="samples">Sequence of predictor-arrays and their response.</param>
        /// <param name="intercept">True if an intercept should be added as first artificial predictor value. Default = false.</param>
        /// <returns>Best fitting list of model parameters β for each element in the predictor-arrays.</returns>
        public static T[] NormalEquations<T>(IEnumerable<Tuple<T[], T>> samples, bool intercept = false) where T : struct, IEquatable<T>, IFormattable
        {
            var xy = samples.UnpackSinglePass();
            return NormalEquations(xy.Item1, xy.Item2, intercept);
        }

        /// <summary>
        /// Find the model parameters β such that X*β with predictor X becomes as close to response Y as possible, with least squares residuals.
        /// Uses an orthogonal decomposition and is therefore more numerically stable than the normal equations but also slower.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response vector Y</param>
        /// <returns>Best fitting vector for model parameters β</returns>
        public static Vector<T> QR<T>(Matrix<T> x, Vector<T> y) where T : struct, IEquatable<T>, IFormattable
        {
            if (x.RowCount != y.Count)
            {
                throw new ArgumentException(string.Format(Resources.SampleVectorsSameLength, x.RowCount, y.Count));
            }

            if (x.ColumnCount > y.Count)
            {
                throw new ArgumentException(string.Format(Resources.RegressionNotEnoughSamples, x.ColumnCount, y.Count));
            }

            return x.QR().Solve(y);
        }

        /// <summary>
        /// Find the model parameters β such that X*β with predictor X becomes as close to response Y as possible, with least squares residuals.
        /// Uses an orthogonal decomposition and is therefore more numerically stable than the normal equations but also slower.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response matrix Y</param>
        /// <returns>Best fitting vector for model parameters β</returns>
        public static Matrix<T> QR<T>(Matrix<T> x, Matrix<T> y) where T : struct, IEquatable<T>, IFormattable
        {
            if (x.RowCount != y.RowCount)
            {
                throw new ArgumentException(string.Format(Resources.SampleVectorsSameLength, x.RowCount, y.RowCount));
            }

            if (x.ColumnCount > y.RowCount)
            {
                throw new ArgumentException(string.Format(Resources.RegressionNotEnoughSamples, x.ColumnCount, y.RowCount));
            }

            return x.QR().Solve(y);
        }

        /// <summary>
        /// Find the model parameters β such that their linear combination with all predictor-arrays in X become as close to their response in Y as possible, with least squares residuals.
        /// Uses an orthogonal decomposition and is therefore more numerically stable than the normal equations but also slower.
        /// </summary>
        /// <param name="x">List of predictor-arrays.</param>
        /// <param name="y">List of responses</param>
        /// <param name="intercept">True if an intercept should be added as first artificial predictor value. Default = false.</param>
        /// <returns>Best fitting list of model parameters β for each element in the predictor-arrays.</returns>
        public static T[] QR<T>(T[][] x, T[] y, bool intercept = false) where T : struct, IEquatable<T>, IFormattable
        {
            var predictor = Matrix<T>.Build.DenseOfRowArrays(x);
            if (intercept)
            {
                predictor = predictor.InsertColumn(0, Vector<T>.Build.Dense(predictor.RowCount, Vector<T>.One));
            }

            if (predictor.RowCount != y.Length)
            {
                throw new ArgumentException(string.Format(Resources.SampleVectorsSameLength, predictor.RowCount, y.Length));
            }

            if (predictor.ColumnCount > y.Length)
            {
                throw new ArgumentException(string.Format(Resources.RegressionNotEnoughSamples, predictor.ColumnCount, y.Length));
            }

            return predictor.QR().Solve(Vector<T>.Build.Dense(y)).ToArray();
        }

        /// <summary>
        /// Find the model parameters β such that their linear combination with all predictor-arrays in X become as close to their response in Y as possible, with least squares residuals.
        /// Uses an orthogonal decomposition and is therefore more numerically stable than the normal equations but also slower.
        /// </summary>
        /// <param name="samples">Sequence of predictor-arrays and their response.</param>
        /// <param name="intercept">True if an intercept should be added as first artificial predictor value. Default = false.</param>
        /// <returns>Best fitting list of model parameters β for each element in the predictor-arrays.</returns>
        public static T[] QR<T>(IEnumerable<Tuple<T[], T>> samples, bool intercept = false) where T : struct, IEquatable<T>, IFormattable
        {
            var xy = samples.UnpackSinglePass();
            return QR(xy.Item1, xy.Item2, intercept);
        }

        /// <summary>
        /// Find the model parameters β such that X*β with predictor X becomes as close to response Y as possible, with least squares residuals.
        /// Uses a singular value decomposition and is therefore more numerically stable (especially if ill-conditioned) than the normal equations or QR but also slower.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response vector Y</param>
        /// <returns>Best fitting vector for model parameters β</returns>
        public static Vector<T> Svd<T>(Matrix<T> x, Vector<T> y) where T : struct, IEquatable<T>, IFormattable
        {
            if (x.RowCount != y.Count)
            {
                throw new ArgumentException(string.Format(Resources.SampleVectorsSameLength, x.RowCount, y.Count));
            }

            if (x.ColumnCount > y.Count)
            {
                throw new ArgumentException(string.Format(Resources.RegressionNotEnoughSamples, x.ColumnCount, y.Count));
            }

            return x.Svd().Solve(y);
        }

        /// <summary>
        /// Find the model parameters β such that X*β with predictor X becomes as close to response Y as possible, with least squares residuals.
        /// Uses a singular value decomposition and is therefore more numerically stable (especially if ill-conditioned) than the normal equations or QR but also slower.
        /// </summary>
        /// <param name="x">Predictor matrix X</param>
        /// <param name="y">Response matrix Y</param>
        /// <returns>Best fitting vector for model parameters β</returns>
        public static Matrix<T> Svd<T>(Matrix<T> x, Matrix<T> y) where T : struct, IEquatable<T>, IFormattable
        {
            if (x.RowCount != y.RowCount)
            {
                throw new ArgumentException(string.Format(Resources.SampleVectorsSameLength, x.RowCount, y.RowCount));
            }

            if (x.ColumnCount > y.RowCount)
            {
                throw new ArgumentException(string.Format(Resources.RegressionNotEnoughSamples, x.ColumnCount, y.RowCount));
            }

            return x.Svd().Solve(y);
        }

        /// <summary>
        /// Find the model parameters β such that their linear combination with all predictor-arrays in X become as close to their response in Y as possible, with least squares residuals.
        /// Uses a singular value decomposition and is therefore more numerically stable (especially if ill-conditioned) than the normal equations or QR but also slower.
        /// </summary>
        /// <param name="x">List of predictor-arrays.</param>
        /// <param name="y">List of responses</param>
        /// <param name="intercept">True if an intercept should be added as first artificial predictor value. Default = false.</param>
        /// <returns>Best fitting list of model parameters β for each element in the predictor-arrays.</returns>
        public static T[] Svd<T>(T[][] x, T[] y, bool intercept = false) where T : struct, IEquatable<T>, IFormattable
        {
            var predictor = Matrix<T>.Build.DenseOfRowArrays(x);
            if (intercept)
            {
                predictor = predictor.InsertColumn(0, Vector<T>.Build.Dense(predictor.RowCount, Vector<T>.One));
            }

            if (predictor.RowCount != y.Length)
            {
                throw new ArgumentException(string.Format(Resources.SampleVectorsSameLength, predictor.RowCount, y.Length));
            }

            if (predictor.ColumnCount > y.Length)
            {
                throw new ArgumentException(string.Format(Resources.RegressionNotEnoughSamples, predictor.ColumnCount, y.Length));
            }

            return predictor.Svd().Solve(Vector<T>.Build.Dense(y)).ToArray();
        }

        /// <summary>
        /// Find the model parameters β such that their linear combination with all predictor-arrays in X become as close to their response in Y as possible, with least squares residuals.
        /// Uses a singular value decomposition and is therefore more numerically stable (especially if ill-conditioned) than the normal equations or QR but also slower.
        /// </summary>
        /// <param name="samples">Sequence of predictor-arrays and their response.</param>
        /// <param name="intercept">True if an intercept should be added as first artificial predictor value. Default = false.</param>
        /// <returns>Best fitting list of model parameters β for each element in the predictor-arrays.</returns>
        public static T[] Svd<T>(IEnumerable<Tuple<T[], T>> samples, bool intercept = false) where T : struct, IEquatable<T>, IFormattable
        {
            var xy = samples.UnpackSinglePass();
            return Svd(xy.Item1, xy.Item2, intercept);
        }
    }
}
