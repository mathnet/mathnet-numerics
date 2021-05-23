// <copyright file="Vector.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.LinearAlgebra.Complex
{
    using Complex = System.Numerics.Complex;

    /// <summary>
    /// <c>Complex</c> version of the <see cref="Vector{T}"/> class.
    /// </summary>
    [Serializable]
    public abstract class Vector : Vector<Complex>
    {
        /// <summary>
        /// Initializes a new instance of the Vector class.
        /// </summary>
        protected Vector(VectorStorage<Complex> storage)
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
        /// Conjugates vector and save result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoConjugate(Vector<Complex> result)
        {
            Map(Complex.Conjugate, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Negates vector and saves result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoNegate(Vector<Complex> result)
        {
            Map(Complex.Negate, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to add.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the addition.
        /// </param>
        protected override void DoAdd(Complex scalar, Vector<Complex> result)
        {
            Map(x => x + scalar, result, Zeros.Include);
        }

        /// <summary>
        /// Adds another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">
        /// The vector to add to this one.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the addition.
        /// </param>
        protected override void DoAdd(Vector<Complex> other, Vector<Complex> result)
        {
            Map2(Complex.Add, other, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to subtract.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the subtraction.
        /// </param>
        protected override void DoSubtract(Complex scalar, Vector<Complex> result)
        {
            Map(x => x - scalar, result, Zeros.Include);
        }

        /// <summary>
        /// Subtracts another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">
        /// The vector to subtract from this one.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the subtraction.
        /// </param>
        protected override void DoSubtract(Vector<Complex> other, Vector<Complex> result)
        {
            Map2(Complex.Subtract, other, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to multiply.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the multiplication.
        /// </param>
        protected override void DoMultiply(Complex scalar, Vector<Complex> result)
        {
            Map(x => x*scalar, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Divides each element of the vector by a scalar and stores the result in the result vector.
        /// </summary>
        /// <param name="divisor">
        /// The scalar to divide with.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the division.
        /// </param>
        protected override void DoDivide(Complex divisor, Vector<Complex> result)
        {
            Map(x => x/divisor, result, divisor.IsZero() ? Zeros.Include : Zeros.AllowSkip);
        }

        /// <summary>
        /// Divides a scalar by each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="dividend">The scalar to divide.</param>
        /// <param name="result">The vector to store the result of the division.</param>
        protected override void DoDivideByThis(Complex dividend, Vector<Complex> result)
        {
            Map(x => dividend/x, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Vector<Complex> other, Vector<Complex> result)
        {
            Map2(Complex.Multiply, other, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        protected override void DoPointwiseDivide(Vector<Complex> divisor, Vector<Complex> result)
        {
            Map2(Complex.Divide, divisor, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise raise this vector to an exponent and store the result into the result vector.
        /// </summary>
        /// <param name="exponent">The exponent to raise this vector values to.</param>
        /// <param name="result">The vector to store the result of the pointwise power.</param>
        protected override void DoPointwisePower(Complex exponent, Vector<Complex> result)
        {
            Map(x => x.Power(exponent), result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise raise this vector to an exponent vector and store the result into the result vector.
        /// </summary>
        /// <param name="exponent">The exponent vector to raise this vector values to.</param>
        /// <param name="result">The vector to store the result of the pointwise power.</param>
        protected override void DoPointwisePower(Vector<Complex> exponent, Vector<Complex> result)
        {
            Map2(Complex.Pow, exponent, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise canonical modulus, where the result has the sign of the divisor,
        /// of this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The pointwise denominator vector to use.</param>
        /// <param name="result">The result of the modulus.</param>
        protected sealed override void DoPointwiseModulus(Vector<Complex> divisor, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pointwise remainder (% operator), where the result has the sign of the dividend,
        /// of this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The pointwise denominator vector to use.</param>
        /// <param name="result">The result of the modulus.</param>
        protected sealed override void DoPointwiseRemainder(Vector<Complex> divisor, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Pointwise applies the exponential function to each value and stores the result into the result vector.
        /// </summary>
        /// <param name="result">The vector to store the result.</param>
        protected override void DoPointwiseExp(Vector<Complex> result)
        {
            Map(Complex.Exp, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise applies the natural logarithm function to each value and stores the result into the result vector.
        /// </summary>
        /// <param name="result">The vector to store the result.</param>
        protected override void DoPointwiseLog(Vector<Complex> result)
        {
            Map(Complex.Log, result, Zeros.Include);
        }

        protected override void DoPointwiseAbs(Vector<Complex> result)
        {
            Map(x => Complex.Abs(x), result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseAcos(Vector<Complex> result)
        {
            Map(Complex.Acos, result, Zeros.Include);
        }
        protected override void DoPointwiseAsin(Vector<Complex> result)
        {
            Map(Complex.Asin, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseAtan(Vector<Complex> result)
        {
            Map(Complex.Atan, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseAtan2(Vector<Complex> other, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseAtan2(Complex scalar, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseCeiling(Vector<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseCos(Vector<Complex> result)
        {
            Map(Complex.Cos, result, Zeros.Include);
        }
        protected override void DoPointwiseCosh(Vector<Complex> result)
        {
            Map(Complex.Cosh, result, Zeros.Include);
        }
        protected override void DoPointwiseFloor(Vector<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseLog10(Vector<Complex> result)
        {
            Map(Complex.Log10, result, Zeros.Include);
        }
        protected override void DoPointwiseRound(Vector<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseSign(Vector<Complex> result)
        {
            throw new NotSupportedException();
        }
        protected override void DoPointwiseSin(Vector<Complex> result)
        {
            Map(Complex.Sin, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseSinh(Vector<Complex> result)
        {
            Map(Complex.Sinh, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseSqrt(Vector<Complex> result)
        {
            Map(Complex.Sqrt, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseTan(Vector<Complex> result)
        {
            Map(Complex.Tan, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseTanh(Vector<Complex> result)
        {
            Map(Complex.Tanh, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>The sum of a[i]*b[i] for all i.</returns>
        protected override Complex DoDotProduct(Vector<Complex> other)
        {
            var dot = Complex.Zero;
            for (var i = 0; i < Count; i++)
            {
                dot += At(i) * other.At(i);
            }
            return dot;
        }

        /// <summary>
        /// Computes the dot product between the conjugate of this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>The sum of conj(a[i])*b[i] for all i.</returns>
        protected override Complex DoConjugateDotProduct(Vector<Complex> other)
        {
            var dot = Complex.Zero;
            for (var i = 0; i < Count; i++)
            {
                dot += At(i).Conjugate() * other.At(i);
            }
            return dot;
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected sealed override void DoModulus(Complex divisor, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for the given dividend for each element of the vector.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected sealed override void DoModulusByThis(Complex dividend, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected sealed override void DoRemainder(Complex divisor, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for the given dividend for each element of the vector.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected sealed override void DoRemainderByThis(Complex dividend, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        protected override void DoPointwiseMinimum(Complex scalar, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        protected override void DoPointwiseMaximum(Complex scalar, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        protected override void DoPointwiseAbsoluteMinimum(Complex scalar, Vector<Complex> result)
        {
            double absolute = scalar.Magnitude;
            Map(x => Math.Min(absolute, x.Magnitude), result, Zeros.AllowSkip);
        }

        protected override void DoPointwiseAbsoluteMaximum(Complex scalar, Vector<Complex> result)
        {
            double absolute = scalar.Magnitude;
            Map(x => Math.Max(absolute, x.Magnitude), result, Zeros.Include);
        }

        protected override void DoPointwiseMinimum(Vector<Complex> other, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        protected override void DoPointwiseMaximum(Vector<Complex> other, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        protected override void DoPointwiseAbsoluteMinimum(Vector<Complex> other, Vector<Complex> result)
        {
            Map2((x, y) => Math.Min(x.Magnitude, y.Magnitude), other, result, Zeros.AllowSkip);
        }

        protected override void DoPointwiseAbsoluteMaximum(Vector<Complex> other, Vector<Complex> result)
        {
            Map2((x, y) => Math.Max(x.Magnitude, y.Magnitude), other, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Returns the value of the absolute minimum element.
        /// </summary>
        /// <returns>The value of the absolute minimum element.</returns>
        public sealed override Complex AbsoluteMinimum()
        {
            return At(AbsoluteMinimumIndex()).Magnitude;
        }

        /// <summary>
        /// Returns the index of the absolute minimum element.
        /// </summary>
        /// <returns>The index of absolute minimum element.</returns>
        public override int AbsoluteMinimumIndex()
        {
            var index = 0;
            var min = At(index).Magnitude;
            for (var i = 1; i < Count; i++)
            {
                var test = At(i).Magnitude;
                if (test < min)
                {
                    index = i;
                    min = test;
                }
            }

            return index;
        }

        /// <summary>
        /// Returns the value of the absolute maximum element.
        /// </summary>
        /// <returns>The value of the absolute maximum element.</returns>
        public override Complex AbsoluteMaximum()
        {
            return At(AbsoluteMaximumIndex()).Magnitude;
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>
        public override int AbsoluteMaximumIndex()
        {
            var index = 0;
            var max = At(index).Magnitude;
            for (var i = 1; i < Count; i++)
            {
                var test = At(i).Magnitude;
                if (test > max)
                {
                    index = i;
                    max = test;
                }
            }

            return index;
        }

        /// <summary>
        /// Computes the sum of the vector's elements.
        /// </summary>
        /// <returns>The sum of the vector's elements.</returns>
        public override Complex Sum()
        {
            var sum = Complex.Zero;
            for (var i = 0; i < Count; i++)
            {
                sum += At(i);
            }
            return sum;
        }

        /// <summary>
        /// Calculates the L1 norm of the vector, also known as Manhattan norm.
        /// </summary>
        /// <returns>The sum of the absolute values.</returns>
        public override double L1Norm()
        {
            double sum = 0d;
            for (var i = 0; i < Count; i++)
            {
                sum += At(i).Magnitude;
            }
            return sum;
        }

        /// <summary>
        /// Calculates the L2 norm of the vector, also known as Euclidean norm.
        /// </summary>
        /// <returns>The square root of the sum of the squared values.</returns>
        public override double L2Norm()
        {
            return DoConjugateDotProduct(this).SquareRoot().Real;
        }

        /// <summary>
        /// Calculates the infinity norm of the vector.
        /// </summary>
        /// <returns>The maximum absolute value.</returns>
        public override double InfinityNorm()
        {
            return CommonParallel.Aggregate(0, Count, i => At(i).Magnitude, Math.Max, 0d);
        }

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">
        /// The p value.
        /// </param>
        /// <returns>
        /// <c>Scalar ret = ( ∑|At(i)|^p )^(1/p)</c>
        /// </returns>
        public override double Norm(double p)
        {
            if (p < 0d) throw new ArgumentOutOfRangeException(nameof(p));

            if (p == 1d) return L1Norm();
            if (p == 2d) return L2Norm();
            if (double.IsPositiveInfinity(p)) return InfinityNorm();

            double sum = 0d;
            for (var index = 0; index < Count; index++)
            {
                sum += Math.Pow(At(index).Magnitude, p);
            }
            return Math.Pow(sum, 1.0/p);
        }

        /// <summary>
        /// Returns the index of the maximum element.
        /// </summary>
        /// <returns>The index of maximum element.</returns>
        public override int MaximumIndex()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the index of the minimum element.
        /// </summary>
        /// <returns>The index of minimum element.</returns>
        public override int MinimumIndex()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Normalizes this vector to a unit vector with respect to the p-norm.
        /// </summary>
        /// <param name="p">
        /// The p value.
        /// </param>
        /// <returns>
        /// This vector normalized to a unit vector with respect to the p-norm.
        /// </returns>
        public override Vector<Complex> Normalize(double p)
        {
            if (p < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(p));
            }

            double norm = Norm(p);
            var clone = Clone();
            if (norm == 0d)
            {
                return clone;
            }

            clone.Multiply(1d / norm, clone);

            return clone;
        }
    }
}
