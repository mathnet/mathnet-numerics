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

namespace MathNet.Numerics.LinearAlgebra.Double
{
    /// <summary>
    /// <c>double</c> version of the <see cref="Vector{T}"/> class.
    /// </summary>
    [Serializable]
    public abstract class Vector : Vector<double>
    {
        /// <summary>
        /// Initializes a new instance of the Vector class.
        /// </summary>
        protected Vector(VectorStorage<double> storage)
            : base(storage)
        {
        }

        /// <summary>
        /// Set all values whose absolute value is smaller than the threshold to zero.
        /// </summary>
        public override void CoerceZero(double threshold)
        {
            MapInplace(x => Math.Abs(x) < threshold ? 0d : x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Conjugates vector and save result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected sealed override void DoConjugate(Vector<double> result)
        {
            if (ReferenceEquals(this, result))
            {
                return;
            }

            CopyTo(result);
        }

        /// <summary>
        /// Negates vector and saves result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoNegate(Vector<double> result)
        {
            Map(x => -x, result, Zeros.AllowSkip);
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
        protected override void DoAdd(double scalar, Vector<double> result)
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
        protected override void DoAdd(Vector<double> other, Vector<double> result)
        {
            Map2((x, y) => x + y, other, result, Zeros.AllowSkip);
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
        protected override void DoSubtract(double scalar, Vector<double> result)
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
        protected override void DoSubtract(Vector<double> other, Vector<double> result)
        {
            Map2((x, y) => x - y, other, result, Zeros.AllowSkip);
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
        protected override void DoMultiply(double scalar, Vector<double> result)
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
        protected override void DoDivide(double divisor, Vector<double> result)
        {
            Map(x => x/divisor, result, divisor == 0.0 ? Zeros.Include : Zeros.AllowSkip);
        }

        /// <summary>
        /// Divides a scalar by each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="dividend">The scalar to divide.</param>
        /// <param name="result">The vector to store the result of the division.</param>
        protected override void DoDivideByThis(double dividend, Vector<double> result)
        {
            Map(x => dividend/x, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Vector<double> other, Vector<double> result)
        {
            Map2((x, y) => x*y, other, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        protected override void DoPointwiseDivide(Vector<double> divisor, Vector<double> result)
        {
            Map2((x, y) => x/y, divisor, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise raise this vector to an exponent and store the result into the result vector.
        /// </summary>
        /// <param name="exponent">The exponent to raise this vector values to.</param>
        /// <param name="result">The vector to store the result of the pointwise power.</param>
        protected override void DoPointwisePower(double exponent, Vector<double> result)
        {
            Map(x => Math.Pow(x, exponent), result, exponent > 0.0 ? Zeros.AllowSkip : Zeros.Include);
        }

        /// <summary>
        /// Pointwise raise this vector to an exponent vector and store the result into the result vector.
        /// </summary>
        /// <param name="exponent">The exponent vector to raise this vector values to.</param>
        /// <param name="result">The vector to store the result of the pointwise power.</param>
        protected override void DoPointwisePower(Vector<double> exponent, Vector<double> result)
        {
            Map2(Math.Pow, exponent, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise canonical modulus, where the result has the sign of the divisor,
        /// of this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The pointwise denominator vector to use.</param>
        /// <param name="result">The result of the modulus.</param>
        protected override void DoPointwiseModulus(Vector<double> divisor, Vector<double> result)
        {
            Map2(Euclid.Modulus, divisor, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise remainder (% operator), where the result has the sign of the dividend,
        /// of this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The pointwise denominator vector to use.</param>
        /// <param name="result">The result of the modulus.</param>
        protected override void DoPointwiseRemainder(Vector<double> divisor, Vector<double> result)
        {
            Map2(Euclid.Remainder, divisor, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise applies the exponential function to each value and stores the result into the result vector.
        /// </summary>
        /// <param name="result">The vector to store the result.</param>
        protected override void DoPointwiseExp(Vector<double> result)
        {
            Map(Math.Exp, result, Zeros.Include);
        }

        /// <summary>
        /// Pointwise applies the natural logarithm function to each value and stores the result into the result vector.
        /// </summary>
        /// <param name="result">The vector to store the result.</param>
        protected override void DoPointwiseLog(Vector<double> result)
        {
            Map(Math.Log, result, Zeros.Include);
        }

        protected override void DoPointwiseAbs(Vector<double> result)
        {
            Map(Math.Abs, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseAcos(Vector<double> result)
        {
            Map(Math.Acos, result, Zeros.Include);
        }
        protected override void DoPointwiseAsin(Vector<double> result)
        {
            Map(Math.Asin, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseAtan(Vector<double> result)
        {
            Map(Math.Atan, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseAtan2(Vector<double> other, Vector<double> result)
        {
            Map2(Math.Atan2, other, result, Zeros.Include);
        }
        protected override void DoPointwiseAtan2(double scalar, Vector<double> result)
        {
            Map(x => Math.Atan2(x, scalar), result, Zeros.Include);
        }
        protected override void DoPointwiseCeiling(Vector<double> result)
        {
            Map(Math.Ceiling, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseCos(Vector<double> result)
        {
            Map(Math.Cos, result, Zeros.Include);
        }
        protected override void DoPointwiseCosh(Vector<double> result)
        {
            Map(Math.Cosh, result, Zeros.Include);
        }
        protected override void DoPointwiseFloor(Vector<double> result)
        {
            Map(Math.Floor, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseLog10(Vector<double> result)
        {
            Map(Math.Log10, result, Zeros.Include);
        }
        protected override void DoPointwiseRound(Vector<double> result)
        {
            Map(Math.Round, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseSign(Vector<double> result)
        {
            Map(x => Math.Sign(x), result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseSin(Vector<double> result)
        {
            Map(Math.Sin, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseSinh(Vector<double> result)
        {
            Map(Math.Sinh, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseSqrt(Vector<double> result)
        {
            Map(Math.Sqrt, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseTan(Vector<double> result)
        {
            Map(Math.Tan, result, Zeros.AllowSkip);
        }
        protected override void DoPointwiseTanh(Vector<double> result)
        {
            Map(Math.Tanh, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>The sum of a[i]*b[i] for all i.</returns>
        protected override double DoDotProduct(Vector<double> other)
        {
            var dot = 0.0;
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
        protected sealed override double DoConjugateDotProduct(Vector<double> other)
        {
            return DoDotProduct(other);
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoModulus(double divisor, Vector<double> result)
        {
            Map(x => Euclid.Modulus(x, divisor), result, Zeros.Include);
        }

        /// <summary>
        /// Computes the canonical modulus, where the result has the sign of the divisor,
        /// for the given dividend for each element of the vector.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoModulusByThis(double dividend, Vector<double> result)
        {
            Map(x => Euclid.Modulus(dividend, x), result, Zeros.Include);
        }

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The scalar denominator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoRemainder(double divisor, Vector<double> result)
        {
            Map(x => Euclid.Remainder(x, divisor), result, Zeros.Include);
        }

        /// <summary>
        /// Computes the remainder (% operator), where the result has the sign of the dividend,
        /// for the given dividend for each element of the vector.
        /// </summary>
        /// <param name="dividend">The scalar numerator to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoRemainderByThis(double dividend, Vector<double> result)
        {
            Map(x => Euclid.Remainder(dividend, x), result, Zeros.Include);
        }

        protected override void DoPointwiseMinimum(double scalar, Vector<double> result)
        {
            Map(x => Math.Min(scalar, x), result, scalar >= 0d ? Zeros.AllowSkip : Zeros.Include);
        }

        protected override void DoPointwiseMaximum(double scalar, Vector<double> result)
        {
            Map(x => Math.Max(scalar, x), result, scalar <= 0d ? Zeros.AllowSkip : Zeros.Include);
        }

        protected override void DoPointwiseAbsoluteMinimum(double scalar, Vector<double> result)
        {
            double absolute = Math.Abs(scalar);
            Map(x => Math.Min(absolute, Math.Abs(x)), result, Zeros.AllowSkip);
        }

        protected override void DoPointwiseAbsoluteMaximum(double scalar, Vector<double> result)
        {
            double absolute = Math.Abs(scalar);
            Map(x => Math.Max(absolute, Math.Abs(x)), result, Zeros.Include);
        }

        protected override void DoPointwiseMinimum(Vector<double> other, Vector<double> result)
        {
            Map2(Math.Min, other, result, Zeros.AllowSkip);
        }

        protected override void DoPointwiseMaximum(Vector<double> other, Vector<double> result)
        {
            Map2(Math.Max, other, result, Zeros.AllowSkip);
        }

        protected override void DoPointwiseAbsoluteMinimum(Vector<double> other, Vector<double> result)
        {
            Map2((x, y) => Math.Min(Math.Abs(x), Math.Abs(y)), other, result, Zeros.AllowSkip);
        }

        protected override void DoPointwiseAbsoluteMaximum(Vector<double> other, Vector<double> result)
        {
            Map2((x, y) => Math.Max(Math.Abs(x), Math.Abs(y)), other, result, Zeros.AllowSkip);
        }

        /// <summary>
        /// Returns the value of the absolute minimum element.
        /// </summary>
        /// <returns>The value of the absolute minimum element.</returns>
        public override double AbsoluteMinimum()
        {
            return Math.Abs(At(AbsoluteMinimumIndex()));
        }

        /// <summary>
        /// Returns the index of the absolute minimum element.
        /// </summary>
        /// <returns>The index of absolute minimum element.</returns>
        public override int AbsoluteMinimumIndex()
        {
            var index = 0;
            var min = Math.Abs(At(index));
            for (var i = 1; i < Count; i++)
            {
                var test = Math.Abs(At(i));
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
        public override double AbsoluteMaximum()
        {
            return Math.Abs(At(AbsoluteMaximumIndex()));
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>
        public override int AbsoluteMaximumIndex()
        {
            var index = 0;
            var max = Math.Abs(At(index));
            for (var i = 1; i < Count; i++)
            {
                var test = Math.Abs(At(i));
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
        public override double Sum()
        {
            var sum = 0.0;
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
            var sum = 0.0;
            for (var i = 0; i < Count; i++)
            {
                sum += Math.Abs(At(i));
            }
            return sum;
        }

        /// <summary>
        /// Calculates the L2 norm of the vector, also known as Euclidean norm.
        /// </summary>
        /// <returns>The square root of the sum of the squared values.</returns>
        public override double L2Norm()
        {
            return Math.Sqrt(DoDotProduct(this));
        }

        /// <summary>
        /// Calculates the infinity norm of the vector.
        /// </summary>
        /// <returns>The maximum absolute value.</returns>
        public override double InfinityNorm()
        {
            return CommonParallel.Aggregate(0, Count, i => Math.Abs(At(i)), Math.Max, 0d);
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

            var sum = 0d;
            for (var index = 0; index < Count; index++)
            {
                sum += Math.Pow(Math.Abs(At(index)), p);
            }
            return Math.Pow(sum, 1.0/p);
        }

        /// <summary>
        /// Returns the index of the maximum element.
        /// </summary>
        /// <returns>The index of maximum element.</returns>
        public override int MaximumIndex()
        {
            var index = 0;
            var max = At(index);
            for (var i = 1; i < Count; i++)
            {
                var test = At(i);
                if (test > max)
                {
                    index = i;
                    max = test;
                }
            }

            return index;
        }

        /// <summary>
        /// Returns the index of the minimum element.
        /// </summary>
        /// <returns>The index of minimum element.</returns>
        public override int MinimumIndex()
        {
            var index = 0;
            var min = At(index);
            for (var i = 1; i < Count; i++)
            {
                var test = At(i);
                if (test < min)
                {
                    index = i;
                    min = test;
                }
            }

            return index;
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
        public override Vector<double> Normalize(double p)
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
