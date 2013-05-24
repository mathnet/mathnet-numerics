// <copyright file="Vector.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex
{
    using System;
    using System.Numerics;
    using Generic;
    using Storage;
    using Threading;

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
            for (var index = 0; index < Count; index++)
            {
                result.At(index, At(index) + scalar);
            }
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
            for (var index = 0; index < Count; index++)
            {
                result.At(index, At(index) + other.At(index));
            }
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
            DoAdd(-scalar, result);
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
            for (var index = 0; index < Count; index++)
            {
                result.At(index, At(index) - other.At(index));
            }
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
            for (var index = 0; index < Count; index++)
            {
                result.At(index, At(index) * scalar);
            }
        }

        /// <summary>
        /// Divides each element of the vector by a scalar and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to divide with.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the division.
        /// </param>
        protected override void DoDivide(Complex scalar, Vector<Complex> result)
        {
            DoMultiply(1 / scalar, result);
        }

        /// <summary>
        /// Divides a scalar by each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to divide.</param>
        /// <param name="result">The vector to store the result of the division.</param>
        protected override void DoDivideByThis(Complex scalar, Vector<Complex> result)
        {
            for (var index = 0; index < Count; index++)
            {
                result.At(index, scalar / At(index));
            }
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        protected override void DoPointwiseMultiply(Vector<Complex> other, Vector<Complex> result)
        {
            for (var index = 0; index < Count; index++)
            {
                result.At(index, At(index) * other.At(index));
            }
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        protected override void DoPointwiseDivide(Vector<Complex> other, Vector<Complex> result)
        {
            for (var index = 0; index < Count; index++)
            {
                result.At(index, At(index) / other.At(index));
            }
        }

        /// <summary>
        /// Pointwise modulus this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise modulus this one by.</param>
        /// <param name="result">The result of the modulus.</param>
        protected override void DoPointwiseModulus(Vector<Complex> other, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">
        /// The other vector to add.
        /// </param>
        /// <returns>
        /// The result of the addition.
        /// </returns>
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
        /// Computes the modulus for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoModulus(Complex divisor, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Computes the modulus for the given dividend for each element of the vector.
        /// </summary>
        /// <param name="scalar">The dividend to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected override void DoModulusByThis(Complex scalar, Vector<Complex> result)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the value of the absolute minimum element.
        /// </summary>
        /// <returns>The value of the absolute minimum element.</returns>
        public override Complex AbsoluteMinimum()
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
        /// Computes the sum of the absolute value of the vector's elements.
        /// </summary>
        /// <returns>The sum of the absolute value of the vector's elements.</returns>
        public override Complex SumMagnitudes()
        {
            var sum = Complex.Zero;

            for (var i = 0; i < Count; i++)
            {
                sum += At(i).Magnitude;
            }

            return sum;
        }

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">
        /// The p value.
        /// </param>
        /// <returns>
        /// <c>Scalar ret = (sum(abs(At(i))^p))^(1/p)</c>
        /// </returns>
        public override Complex Norm(double p)
        {
            if (p < 0.0)
            {
                throw new ArgumentOutOfRangeException("p");
            }

            if (double.IsPositiveInfinity(p))
            {
                return CommonParallel.Aggregate(0, Count, i => At(i).Magnitude, Math.Max, 0d);
            }

            var sum = 0.0;

            for (var index = 0; index < Count; index++)
            {
                sum += Math.Pow(At(index).Magnitude, p);
            }

            return Math.Pow(sum, 1.0 / p);
        }

        /// <summary>
        /// Conjugates vector and save result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoConjugate(Vector<Complex> result)
        {
            for (var index = 0; index < Count; index++)
            {
                result.At(index, At(index).Conjugate());
            }
        }

        /// <summary>
        /// Negates vector and saves result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoNegate(Vector<Complex> result)
        {
            for (var index = 0; index < Count; index++)
            {
                result.At(index, -At(index));
            }
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>          
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
            if (p < 0.0)
            {
                throw new ArgumentOutOfRangeException("p");
            }

            var norm = Norm(p);
            var clone = Clone();
            if (norm.Real == 0.0)
            {
                return clone;
            }

            clone.Multiply(1.0 / norm, clone);

            return clone;
        }
    }
}
