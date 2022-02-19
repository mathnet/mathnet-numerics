// <copyright file="Matrix.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Complex.Factorization;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace MathNet.Numerics.LinearAlgebra.Complex
{
    using Complex = System.Numerics.Complex;

    /// <summary>
    /// <c>Complex</c> version of the <see cref="Matrix{T}"/> class.
    /// </summary>
    [Serializable]
    public abstract class Matrix : Matrix<Complex>
    {
        /// <summary>
        /// Initializes a new instance of the Matrix class.
        /// </summary>
        protected Matrix(MatrixStorage<Complex> storage)
            : base(storage)
        {
        }

        /// <summary>
        /// Set all values whose absolute value is smaller than the threshold to zero.
        /// </summary>
        public override void CoerceZero(double threshold)
        {
            MapInplace(x => x.Magnitude < threshold ? Complex.Zero : x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Returns the conjugate transpose of this matrix.
        /// </summary>
        /// <returns>The conjugate transpose of this matrix.</returns>
        public sealed override Matrix<Complex> ConjugateTranspose()
        {
            var ret = Transpose();
            ret.MapInplace(c => c.Conjugate(), Zeros.AllowSkip);
            return ret;
        }

        /// <summary>
        /// Puts the conjugate transpose of this matrix into the result matrix.
        /// </summary>
        public sealed override void ConjugateTranspose(Matrix<Complex> result)
        {
            Transpose(result);
            result.MapInplace(c => c.Conjugate(), Zeros.AllowSkip);
        }

        /// <summary>
        /// Complex conjugates each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the conjugation.</param>
        protected override void DoConjugate(Matrix<Complex> result)
        {
            Map(Complex.Conjugate, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Negate each element of this matrix and place the results into the result matrix.
        /// </summary>
        /// <param name="result">The result of the negation.</param>
        protected override void DoNegate(Matrix<Complex> result)
        {
            Map(Complex.Negate, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Add a scalar to each element of the matrix and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        protected override void DoAdd(Complex scalar, Matrix<Complex> result)
        {
            Map(x => x + scalar, result, Zeros.Include);
        }

        /// <summary>
        /// Adds another matrix to this matrix.
        /// </summary>
        /// <param name="other">The matrix to add to this matrix.</param>
        /// <param name="result">The matrix to store the result of the addition.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected override void DoAdd(Matrix<Complex> other, Matrix<Complex> result)
        {
            Map2(Complex.Add, other, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The matrix to store the result of the subtraction.</param>
        protected override void DoSubtract(Complex scalar, Matrix<Complex> result)
        {
            Map(x => x - scalar, result, Zeros.Include);
        }

        /// <summary>
        /// Subtracts another matrix from this matrix.
        /// </summary>
        /// <param name="other">The matrix to subtract to this matrix.</param>
        /// <param name="result">The matrix to store the result of subtraction.</param>
        /// <exception cref="ArgumentNullException">If the other matrix is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the two matrices don't have the same dimensions.</exception>
        protected override void DoSubtract(Matrix<Complex> other, Matrix<Complex> result)
        {
            Map2(Complex.Subtract, other, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Multiplies each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="scalar">The scalar to multiply the matrix with.</param>
        /// <param name="result">The matrix to store the result of the multiplication.</param>
        protected override void DoMultiply(Complex scalar, Matrix<Complex> result)
        {
            Map(x => x*scalar, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Multiplies this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Vector<Complex> rightSide, Vector<Complex> result)
        {
            for (var i = 0; i < RowCount; i++)
            {
                var s = Complex.Zero;
                for (var j = 0; j < ColumnCount; j++)
                {
                    s += At(i, j)*rightSide[j];
                }
                result[i] = s;
            }
        }

        /// <summary>
        /// Multiplies this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            for (var i = 0; i < RowCount; i++)
            {
                for (var j = 0; j != other.ColumnCount; j++)
                {
                    var s = Complex.Zero;
                    for (var k = 0; k < ColumnCount; k++)
                    {
                        s += At(i, k)*other.At(k, j);
                    }
                    result.At(i, j, s);
                }
            }
        }

        /// <summary>
        /// Divides each element of the matrix by a scalar and places results into the result matrix.
        /// </summary>
        /// <param name="divisor">The scalar to divide the matrix with.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        protected override void DoDivide(Complex divisor, Matrix<Complex> result)
        {
            Map(x => x/divisor, result, divisor.IsZero() ? Zeros.Include : Zeros.AllowSkip);
        }

        /// <summary>
        /// Divides a scalar by each element of the matrix and stores the result in the result matrix.
        /// </summary>
        /// <param name="dividend">The scalar to divide by each element of the matrix.</param>
        /// <param name="result">The matrix to store the result of the division.</param>
        protected override void DoDivideByThis(Complex dividend, Matrix<Complex> result)
        {
            Map(x => dividend/x, result, Zeros.Include);
        }

        /// <summary>
        /// Multiplies this matrix with transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeAndMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            for (var j = 0; j < other.RowCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    var s = Complex.Zero;
                    for (var k = 0; k < ColumnCount; k++)
                    {
                        s += At(i, k)*other.At(j, k);
                    }
                    result.At(i, j, s);
                }
            }
        }

        /// <summary>
        /// Multiplies this matrix with the conjugate transpose of another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoConjugateTransposeAndMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            for (var j = 0; j < other.RowCount; j++)
            {
                for (var i = 0; i < RowCount; i++)
                {
                    var s = Complex.Zero;
                    for (var k = 0; k < ColumnCount; k++)
                    {
                        s += At(i, k)*other.At(j, k).Conjugate();
                    }
                    result.At(i, j, s);
                }
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeThisAndMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            for (var j = 0; j < other.ColumnCount; j++)
            {
                for (var i = 0; i < ColumnCount; i++)
                {
                    var s = Complex.Zero;
                    for (var k = 0; k < RowCount; k++)
                    {
                        s += At(k, i)*other.At(k, j);
                    }
                    result.At(i, j, s);
                }
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with another matrix and places the results into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoConjugateTransposeThisAndMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            for (var j = 0; j < other.ColumnCount; j++)
            {
                for (var i = 0; i < ColumnCount; i++)
                {
                    var s = Complex.Zero;
                    for (var k = 0; k < RowCount; k++)
                    {
                        s += At(k, i).Conjugate()*other.At(k, j);
                    }
                    result.At(i, j, s);
                }
            }
        }

        /// <summary>
        /// Multiplies the transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoTransposeThisAndMultiply(Vector<Complex> rightSide, Vector<Complex> result)
        {
            for (var j = 0; j < ColumnCount; j++)
            {
                var s = Complex.Zero;
                for (var i = 0; i < RowCount; i++)
                {
                    s += At(i, j)*rightSide[i];
                }
                result[j] = s;
            }
        }

        /// <summary>
        /// Multiplies the conjugate transpose of this matrix with a vector and places the results into the result vector.
        /// </summary>
        /// <param name="rightSide">The vector to multiply with.</param>
        /// <param name="result">The result of the multiplication.</param>
        protected override void DoConjugateTransposeThisAndMultiply(Vector<Complex> rightSide, Vector<Complex> result)
        {
            for (var j = 0; j < ColumnCount; j++)
            {
                var s = Complex.Zero;
                for (var i = 0; i < RowCount; i++)
                {
                    s += At(i, j).Conjugate()*rightSide[i];
                }
                result[j] = s;
            }
        }

        /// <summary>
        /// Pointwise multiplies this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="other">The matrix to pointwise multiply with this one.</param>
        /// <param name="result">The matrix to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Matrix<Complex> other, Matrix<Complex> result)
        {
            Map2(Complex.Multiply, other, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Pointwise divide this matrix by another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="divisor">The matrix to pointwise divide this one by.</param>
        /// <param name="result">The matrix to store the result of the pointwise division.</param>
        protected override void DoPointwiseDivide(Matrix<Complex> divisor, Matrix<Complex> result)
        {
            Map2(Complex.Divide, divisor, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise raise this matrix to an exponent and store the result into the result matrix.
        /// </summary>
        /// <param name="exponent">The exponent to raise this matrix values to.</param>
        /// <param name="result">The matrix to store the result of the pointwise power.</param>
        protected override void DoPointwisePower(Complex exponent, Matrix<Complex> result)
        {
            Map(x => x.Power(exponent), result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise raise this matrix to an exponent and store the result into the result matrix.
        /// </summary>
        /// <param name="exponent">The exponent to raise this matrix values to.</param>
        /// <param name="result">The vector to store the result of the pointwise power.</param>
        protected override void DoPointwisePower(Matrix<Complex> exponent, Matrix<Complex> result)
        {
            Map2(Complex.Pow, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise canonical modulus, where the result has the sign of the divisor,
        /// of this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use</param>
        /// <param name="result">The result of the modulus.</param>
        protected sealed override void DoPointwiseModulus(Matrix<Complex> divisor, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pointwise remainder (% operator), where the result has the sign of the dividend,
        /// of this matrix with another matrix and stores the result into the result matrix.
        /// </summary>
        /// <param name="divisor">The pointwise denominator matrix to use</param>
        /// <param name="result">The result of the modulus.</param>
        protected sealed override void DoPointwiseRemainder(Matrix<Complex> divisor, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for the given divisor each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected sealed override void DoModulus(Complex divisor, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for the given dividend for each element of the matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected sealed override void DoModulusByThis(Complex dividend, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// for the given divisor each element of the matrix.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">Matrix to store the results in.</param>
        protected sealed override void DoRemainder(Complex divisor, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// for the given dividend for each element of the matrix.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected sealed override void DoRemainderByThis(Complex dividend, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pointwise applies the exponential function to each value and stores the result into the result matrix.
        /// </summary>
        /// <param name="result">The matrix to store the result.</param>
        protected override void DoPointwiseExp(Matrix<Complex> result)
        {
            Map(Complex.Exp, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise applies the natural logarithm function to each value and stores the result into the result matrix.
        /// </summary>
        /// <param name="result">The matrix to store the result.</param>
        protected override void DoPointwiseLog(Matrix<Complex> result)
        {
            Map(Complex.Log, result, Zeros.Include);
        }

        protected override void DoPointwiseAbs(Matrix<Complex> result)
        {
            Map(x => Complex.Abs(x), result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseAcos(Matrix<Complex> result)
        {
            Map(Complex.Acos, result, Zeros.Include);
        }
        protected override void DoPointwiseAsin(Matrix<Complex> result)
        {
            Map(Complex.Asin, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseAtan(Matrix<Complex> result)
        {
            Map(Complex.Atan, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseAtan2(Matrix<Complex> other, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseCeiling(Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseCos(Matrix<Complex> result)
        {
            Map(Complex.Cos, result, Zeros.Include);
        }
        protected override void DoPointwiseCosh(Matrix<Complex> result)
        {
            Map(Complex.Cosh, result, Zeros.Include);
        }
        protected override void DoPointwiseFloor(Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseLog10(Matrix<Complex> result)
        {
            Map(Complex.Log10, result, Zeros.Include);
        }
        protected override void DoPointwiseRound(Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseSign(Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseSin(Matrix<Complex> result)
        {
            Map(Complex.Sin, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseSinh(Matrix<Complex> result)
        {
            Map(Complex.Sinh, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseSqrt(Matrix<Complex> result)
        {
            Map(Complex.Sqrt, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseTan(Matrix<Complex> result)
        {
            Map(Complex.Tan, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseTanh(Matrix<Complex> result)
        {
            Map(Complex.Tanh, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Computes the Moore-Penrose Pseudo-Inverse of this matrix.
        /// </summary>
        public override Matrix<Complex> PseudoInverse()
        {
            var svd = Svd(true);
            var w = svd.W;
            var s = svd.S;
            double tolerance = Math.Max(RowCount, ColumnCount) * svd.L2Norm * Precision.DoublePrecision;

            for (int i = 0; i < s.Count; i++)
            {
                s[i] = s[i].Magnitude < tolerance ? 0 : 1/s[i];
            }

            w.SetDiagonal(s);
            return (svd.U * (w * svd.VT)).ConjugateTranspose();
        }

        /// <summary>
        /// Computes the trace of this matrix.
        /// </summary>
        /// <returns>The trace of this matrix</returns>
        /// <exception cref="ArgumentException">If the matrix is not square</exception>
        public override Complex Trace()
        {
            if (RowCount != ColumnCount)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            var sum = Complex.Zero;
            for (var i = 0; i < RowCount; i++)
            {
                sum += At(i, i);
            }

            return sum;
        }

        protected override void DoPointwiseMinimum(Complex scalar, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }

        protected override void DoPointwiseMaximum(Complex scalar, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }

        protected override void DoPointwiseAbsoluteMinimum(Complex scalar, Matrix<Complex> result)
        {
            double absolute = scalar.Magnitude;
            Map(x => Math.Min(absolute, x.Magnitude), result, Zeros.AllowSkip);
        }

        protected override void DoPointwiseAbsoluteMaximum(Complex scalar, Matrix<Complex> result)
        {
            double absolute = scalar.Magnitude;
            Map(x => Math.Max(absolute, x.Magnitude), result, Zeros.Include);
        }

        protected override void DoPointwiseMinimum(Matrix<Complex> other, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }

        protected override void DoPointwiseMaximum(Matrix<Complex> other, Matrix<Complex> result)
        {
            throw new NotSupportedException();
        }

        protected override void DoPointwiseAbsoluteMinimum(Matrix<Complex> other, Matrix<Complex> result)
        {
            Map2((x, y) => Math.Min(x.Magnitude, y.Magnitude), other, result, Zeros.AllowSkip);
        }

        protected override void DoPointwiseAbsoluteMaximum(Matrix<Complex> other, Matrix<Complex> result)
        {
            Map2((x, y) => Math.Max(x.Magnitude, y.Magnitude), other, result, Zeros.AllowSkip);
        }

        /// <summary>Calculates the induced L1 norm of this matrix.</summary>
        /// <returns>The maximum absolute column sum of the matrix.</returns>
        public override double L1Norm()
        {
            var norm = 0d;
            for (var j = 0; j < ColumnCount; j++)
            {
                var s = 0d;
                for (var i = 0; i < RowCount; i++)
                {
                    s += At(i, j).Magnitude;
                }
                norm = Math.Max(norm, s);
            }
            return norm;
        }

        /// <summary>Calculates the induced infinity norm of this matrix.</summary>
        /// <returns>The maximum absolute row sum of the matrix.</returns>
        public override double InfinityNorm()
        {
            var norm = 0d;
            for (var i = 0; i < RowCount; i++)
            {
                var s = 0d;
                for (var j = 0; j < ColumnCount; j++)
                {
                    s += At(i, j).Magnitude;
                }
                norm = Math.Max(norm, s);
            }
            return norm;
        }

        /// <summary>Calculates the entry-wise Frobenius norm of this matrix.</summary>
        /// <returns>The square root of the sum of the squared values.</returns>
        public override double FrobeniusNorm()
        {
            var transpose = ConjugateTranspose();
            var aat = this*transpose;
            var norm = 0d;
            for (var i = 0; i < RowCount; i++)
            {
                norm += aat.At(i, i).Magnitude;
            }
            return Math.Sqrt(norm);
        }

        /// <summary>
        /// Calculates the p-norms of all row vectors.
        /// Typical values for p are 1.0 (L1, Manhattan norm), 2.0 (L2, Euclidean norm) and positive infinity (infinity norm)
        /// </summary>
        public override Vector<double> RowNorms(double norm)
        {
            if (norm <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(norm), "Value must be positive.");
            }

            var ret = new double[RowCount];
            if (norm == 2.0)
            {
                Storage.FoldByRowUnchecked(ret, (s, x) => s + x.MagnitudeSquared(), (x, _) => Math.Sqrt(x), ret, Zeros.AllowSkip);
            }
            else if (norm == 1.0)
            {
                Storage.FoldByRowUnchecked(ret, (s, x) => s + x.Magnitude, (x, _) => x, ret, Zeros.AllowSkip);
            }
            else if (double.IsPositiveInfinity(norm))
            {
                Storage.FoldByRowUnchecked(ret, (s, x) => Math.Max(s, x.Magnitude), (x, _) => x, ret, Zeros.AllowSkip);
            }
            else
            {
                double invnorm = 1.0/norm;
                Storage.FoldByRowUnchecked(ret, (s, x) => s + Math.Pow(x.Magnitude, norm), (x, _) => Math.Pow(x, invnorm), ret, Zeros.AllowSkip);
            }
            return Vector<double>.Build.Dense(ret);
        }

        /// <summary>
        /// Calculates the p-norms of all column vectors.
        /// Typical values for p are 1.0 (L1, Manhattan norm), 2.0 (L2, Euclidean norm) and positive infinity (infinity norm)
        /// </summary>
        public override Vector<double> ColumnNorms(double norm)
        {
            if (norm <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(norm), "Value must be positive.");
            }

            var ret = new double[ColumnCount];
            if (norm == 2.0)
            {
                Storage.FoldByColumnUnchecked(ret, (s, x) => s + x.MagnitudeSquared(), (x, _) => Math.Sqrt(x), ret, Zeros.AllowSkip);
            }
            else if (norm == 1.0)
            {
                Storage.FoldByColumnUnchecked(ret, (s, x) => s + x.Magnitude, (x, _) => x, ret, Zeros.AllowSkip);
            }
            else if (double.IsPositiveInfinity(norm))
            {
                Storage.FoldByColumnUnchecked(ret, (s, x) => Math.Max(s, x.Magnitude), (x, _) => x, ret, Zeros.AllowSkip);
            }
            else
            {
                double invnorm = 1.0/norm;
                Storage.FoldByColumnUnchecked(ret, (s, x) => s + Math.Pow(x.Magnitude, norm), (x, _) => Math.Pow(x, invnorm), ret, Zeros.AllowSkip);
            }
            return Vector<double>.Build.Dense(ret);
        }

        /// <summary>
        /// Normalizes all row vectors to a unit p-norm.
        /// Typical values for p are 1.0 (L1, Manhattan norm), 2.0 (L2, Euclidean norm) and positive infinity (infinity norm)
        /// </summary>
        public sealed override Matrix<Complex> NormalizeRows(double norm)
        {
            var norminv = ((DenseVectorStorage<double>)RowNorms(norm).Storage).Data;
            for (int i = 0; i < norminv.Length; i++)
            {
                norminv[i] = norminv[i] == 0d ? 1d : 1d/norminv[i];
            }

            var result = Build.SameAs(this, RowCount, ColumnCount);
            Storage.MapIndexedTo(result.Storage, (i, _, x) => norminv[i]*x, Zeros.AllowSkip, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Normalizes all column vectors to a unit p-norm.
        /// Typical values for p are 1.0 (L1, Manhattan norm), 2.0 (L2, Euclidean norm) and positive infinity (infinity norm)
        /// </summary>
        public sealed override Matrix<Complex> NormalizeColumns(double norm)
        {
            var norminv = ((DenseVectorStorage<double>)ColumnNorms(norm).Storage).Data;
            for (int i = 0; i < norminv.Length; i++)
            {
                norminv[i] = norminv[i] == 0d ? 1d : 1d/norminv[i];
            }

            var result = Build.SameAs(this, RowCount, ColumnCount);
            Storage.MapIndexedTo(result.Storage, (_, j, x) => norminv[j]*x, Zeros.AllowSkip, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Calculates the value sum of each row vector.
        /// </summary>
        public override Vector<Complex> RowSums()
        {
            var ret = new Complex[RowCount];
            Storage.FoldByRowUnchecked(ret, (s, x) => s + x, (x, _) => x, ret, Zeros.AllowSkip);
            return Vector<Complex>.Build.Dense(ret);
        }

        /// <summary>
        /// Calculates the absolute value sum of each row vector.
        /// </summary>
        public override Vector<Complex> RowAbsoluteSums()
        {
            var ret = new Complex[RowCount];
            Storage.FoldByRowUnchecked(ret, (s, x) => s + x.Magnitude, (x, _) => x, ret, Zeros.AllowSkip);
            return Vector<Complex>.Build.Dense(ret);
        }

        /// <summary>
        /// Calculates the value sum of each column vector.
        /// </summary>
        public override Vector<Complex> ColumnSums()
        {
            var ret = new Complex[ColumnCount];
            Storage.FoldByColumnUnchecked(ret, (s, x) => s + x, (x, _) => x, ret, Zeros.AllowSkip);
            return Vector<Complex>.Build.Dense(ret);
        }

        /// <summary>
        /// Calculates the absolute value sum of each column vector.
        /// </summary>
        public override Vector<Complex> ColumnAbsoluteSums()
        {
            var ret = new Complex[ColumnCount];
            Storage.FoldByColumnUnchecked(ret, (s, x) => s + x.Magnitude, (x, _) => x, ret, Zeros.AllowSkip);
            return Vector<Complex>.Build.Dense(ret);
        }

        /// <summary>
        /// Evaluates whether this matrix is Hermitian (conjugate symmetric).
        /// </summary>
        public override bool IsHermitian()
        {
            if (RowCount != ColumnCount)
            {
                return false;
            }

            for (var k = 0; k < RowCount; k++)
            {
                if (!At(k, k).IsReal())
                {
                    return false;
                }
            }

            for (var row = 0; row < RowCount; row++)
            {
                for (var column = row + 1; column < ColumnCount; column++)
                {
                    if (!At(row, column).Equals(At(column, row).Conjugate()))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override Cholesky<Complex> Cholesky()
        {
            return UserCholesky.Create(this);
        }

        public override LU<Complex> LU()
        {
            return UserLU.Create(this);
        }

        public override QR<Complex> QR(QRMethod method = QRMethod.Thin)
        {
            return UserQR.Create(this, method);
        }

        public override GramSchmidt<Complex> GramSchmidt()
        {
            return UserGramSchmidt.Create(this);
        }

        public override Svd<Complex> Svd(bool computeVectors = true)
        {
            return UserSvd.Create(this, computeVectors);
        }

        public override Evd<Complex> Evd(Symmetricity symmetricity = Symmetricity.Unknown)
        {
            return UserEvd.Create(this, symmetricity);
        }
    }
}
