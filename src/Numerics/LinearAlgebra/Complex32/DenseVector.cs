﻿// <copyright file="DenseVector.cs" company="Math.NET">
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

using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MathNet.Numerics.LinearAlgebra.Complex32
{
    using Numerics;

    /// <summary>
    /// A vector using dense storage.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("DenseVector {Count}-Complex32")]
    public class DenseVector : Vector
    {
        /// <summary>
        /// Number of elements
        /// </summary>
        readonly int _length;

        /// <summary>
        /// Gets the vector's data.
        /// </summary>
        readonly Complex32[] _values;

        /// <summary>
        /// Create a new dense vector straight from an initialized vector storage instance.
        /// The storage is used directly without copying.
        /// Intended for advanced scenarios where you're working directly with
        /// storage for performance or interop reasons.
        /// </summary>
        public DenseVector(DenseVectorStorage<Complex32> storage)
            : base(storage)
        {
            _length = storage.Length;
            _values = storage.Data;
        }

        /// <summary>
        /// Create a new dense vector with the given length.
        /// All cells of the vector will be initialized to zero.
        /// Zero-length vectors are not supported.
        /// </summary>
        /// <exception cref="ArgumentException">If length is less than one.</exception>
        public DenseVector(int length)
            : this(new DenseVectorStorage<Complex32>(length))
        {
        }

        /// <summary>
        /// Create a new dense vector directly binding to a raw array.
        /// The array is used directly without copying.
        /// Very efficient, but changes to the array and the vector will affect each other.
        /// </summary>
        public DenseVector(Complex32[] storage)
            : this(new DenseVectorStorage<Complex32>(storage.Length, storage))
        {
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given other vector.
        /// This new vector will be independent from the other vector.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static DenseVector OfVector(Vector<Complex32> vector)
        {
            return new DenseVector(DenseVectorStorage<Complex32>.OfVector(vector.Storage));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given array.
        /// This new vector will be independent from the array.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static DenseVector OfArray(Complex32[] array)
        {
            return new DenseVector(DenseVectorStorage<Complex32>.OfVector(new DenseVectorStorage<Complex32>(array.Length, array)));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given enumerable.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static DenseVector OfEnumerable(IEnumerable<Complex32> enumerable)
        {
            return new DenseVector(DenseVectorStorage<Complex32>.OfEnumerable(enumerable));
        }

        /// <summary>
        /// Create a new dense vector as a copy of the given indexed enumerable.
        /// Keys must be provided at most once, zero is assumed if a key is omitted.
        /// This new vector will be independent from the enumerable.
        /// A new memory block will be allocated for storing the vector.
        /// </summary>
        public static DenseVector OfIndexedEnumerable(int length, IEnumerable<Tuple<int, Complex32>> enumerable)
        {
            return new DenseVector(DenseVectorStorage<Complex32>.OfIndexedEnumerable(length, enumerable));
        }

        /// <summary>
        /// Create a new dense vector and initialize each value using the provided value.
        /// </summary>
        public static DenseVector Create(int length, Complex32 value)
        {
            if (value == Complex32.Zero) return new DenseVector(length);
            return new DenseVector(DenseVectorStorage<Complex32>.OfValue(length, value));
        }

        /// <summary>
        /// Create a new dense vector and initialize each value using the provided init function.
        /// </summary>
        public static DenseVector Create(int length, Func<int, Complex32> init)
        {
            return new DenseVector(DenseVectorStorage<Complex32>.OfInit(length, init));
        }

        /// <summary>
        /// Create a new dense vector with values sampled from the provided random distribution.
        /// </summary>
        public static DenseVector CreateRandom(int length, IContinuousDistribution distribution)
        {
            var samples = Generate.RandomComplex32(length, distribution);
            return new DenseVector(new DenseVectorStorage<Complex32>(length, samples));
        }

        /// <summary>
        /// Gets the vector's data.
        /// </summary>
        /// <value>The vector's data.</value>
        public Complex32[] Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Returns a reference to the internal data structure.
        /// </summary>
        /// <param name="vector">The <c>DenseVector</c> whose internal data we are
        /// returning.</param>
        /// <returns>
        /// A reference to the internal date of the given vector.
        /// </returns>
        public static explicit operator Complex32[](DenseVector vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException("vector");
            }

            return vector.Values;
        }

        /// <summary>
        /// Returns a vector bound directly to a reference of the provided array.
        /// </summary>
        /// <param name="array">The array to bind to the <c>DenseVector</c> object.</param>
        /// <returns>
        /// A <c>DenseVector</c> whose values are bound to the given array.
        /// </returns>
        public static implicit operator DenseVector(Complex32[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            return new DenseVector(array);
        }

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        protected override void DoAdd(Complex32 scalar, Vector<Complex32> result)
        {
            var dense = result as DenseVector;
            if (dense == null)
            {
                base.DoAdd(scalar, result);
            }
            else
            {
                CommonParallel.For(0, _values.Length, 4096, (a, b) =>
                    {
                        for (int i = a; i < b; i++)
                        {
                            dense._values[i] = _values[i] + scalar;
                        }
                    });
            }
        }

        /// <summary>
        /// Adds another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        protected override void DoAdd(Vector<Complex32> other, Vector<Complex32> result)
        {
            var otherDense = other as DenseVector;
            var resultDense = result as DenseVector;

            if (otherDense == null || resultDense == null)
            {
                base.DoAdd(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.AddArrays(_values, otherDense._values, resultDense._values);
            }
        }

        /// <summary>
        /// Adds two <strong>Vectors</strong> together and returns the results.
        /// </summary>
        /// <param name="leftSide">One of the vectors to add.</param>
        /// <param name="rightSide">The other vector to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator +(DenseVector leftSide, DenseVector rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Add(rightSide);
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        protected override void DoSubtract(Complex32 scalar, Vector<Complex32> result)
        {
            var dense = result as DenseVector;
            if (dense == null)
            {
                base.DoSubtract(scalar, result);
            }
            else
            {
                CommonParallel.For(0, _values.Length, 4096, (a, b) =>
                    {
                        for (int i = a; i < b; i++)
                        {
                            dense._values[i] = _values[i] - scalar;
                        }
                    });
            }
        }

        /// <summary>
        /// Subtracts another vector from this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        protected override void DoSubtract(Vector<Complex32> other, Vector<Complex32> result)
        {
            var otherDense = other as DenseVector;
            var resultDense = result as DenseVector;

            if (otherDense == null || resultDense == null)
            {
                base.DoSubtract(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.SubtractArrays(_values, otherDense._values, resultDense._values);
            }
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the negated values of <paramref name="rightSide"/>.
        /// </summary>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the negated values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator -(DenseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (DenseVector)rightSide.Negate();
        }

        /// <summary>
        /// Subtracts two <strong>Vectors</strong> and returns the results.
        /// </summary>
        /// <param name="leftSide">The vector to subtract from.</param>
        /// <param name="rightSide">The vector to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator -(DenseVector leftSide, DenseVector rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Negates vector and saves result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoNegate(Vector<Complex32> result)
        {
            var denseResult = result as DenseVector;
            if (denseResult == null)
            {
                base.DoNegate(result);
                return;
            }

            Control.LinearAlgebraProvider.ScaleArray(-Complex32.One, _values, denseResult.Values);
        }

        /// <summary>
        /// Conjugates vector and save result to <paramref name="result"/>
        /// </summary>
        /// <param name="result">Target vector</param>
        protected override void DoConjugate(Vector<Complex32> result)
        {
            var resultDense = result as DenseVector;
            if (resultDense == null)
            {
                base.DoConjugate(result);
                return;
            }

            Control.LinearAlgebraProvider.ConjugateArray(_values, resultDense._values);
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to multiply.</param>
        /// <param name="result">The vector to store the result of the multiplication.</param>
        /// <remarks></remarks>
        protected override void DoMultiply(Complex32 scalar, Vector<Complex32> result)
        {
            var denseResult = result as DenseVector;
            if (denseResult == null)
            {
                base.DoMultiply(scalar, result);
                return;
            }

            Control.LinearAlgebraProvider.ScaleArray(scalar, _values, denseResult.Values);
        }

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>The sum of a[i]*b[i] for all i.</returns>
        protected override Complex32 DoDotProduct(Vector<Complex32> other)
        {
            var denseVector = other as DenseVector;
            return denseVector == null
                ? base.DoDotProduct(other)
                : Control.LinearAlgebraProvider.DotProduct(_values, denseVector.Values);
        }

        /// <summary>
        /// Computes the dot product between the conjugate of this vector and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>The sum of conj(a[i])*b[i] for all i.</returns>
        protected override Complex32 DoConjugateDotProduct(Vector<Complex32> other)
        {
            var denseVector = other as DenseVector;
            if (denseVector == null) return base.DoConjugateDotProduct(other);

            // TODO: provide native cdotc routine
            var dot = Complex32.Zero;
            for (var i = 0; i < _values.Length; i++)
            {
                dot += _values[i].Conjugate() * denseVector._values[i];
            }
            return dot;
        }

        /// <summary>
        /// Multiplies a vector with a complex.
        /// </summary>
        /// <param name="leftSide">The vector to scale.</param>
        /// <param name="rightSide">The Complex32 value.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator *(DenseVector leftSide, Complex32 rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a vector with a complex.
        /// </summary>
        /// <param name="leftSide">The Complex32 value.</param>
        /// <param name="rightSide">The vector to scale.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator *(Complex32 leftSide, DenseVector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return (DenseVector)rightSide.Multiply(leftSide);
        }

        /// <summary>
        /// Computes the dot product between two <strong>Vectors</strong>.
        /// </summary>
        /// <param name="leftSide">The left row vector.</param>
        /// <param name="rightSide">The right column vector.</param>
        /// <returns>The dot product between the two vectors.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Complex32 operator *(DenseVector leftSide, DenseVector rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return leftSide.DotProduct(rightSide);
        }

        /// <summary>
        /// Divides a vector with a complex.
        /// </summary>
        /// <param name="leftSide">The vector to divide.</param>
        /// <param name="rightSide">The Complex32 value.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static DenseVector operator /(DenseVector leftSide, Complex32 rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return (DenseVector)leftSide.Divide(rightSide);
        }

        /// <summary>
        /// Returns the index of the absolute minimum element.
        /// </summary>
        /// <returns>The index of absolute minimum element.</returns>
        public override int AbsoluteMinimumIndex()
        {
            var index = 0;
            var min = _values[index].Magnitude;
            for (var i = 1; i < _length; i++)
            {
                var test = _values[i].Magnitude;
                if (test < min)
                {
                    index = i;
                    min = test;
                }
            }

            return index;
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>
        public override int AbsoluteMaximumIndex()
        {
            var index = 0;
            var max = _values[index].Magnitude;
            for (var i = 1; i < _length; i++)
            {
                var test = _values[i].Magnitude;
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
        public override Complex32 Sum()
        {
            var sum = Complex32.Zero;
            for (var i = 0; i < _length; i++)
            {
                sum += _values[i];
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
            for (var i = 0; i < _length; i++)
            {
                sum += _values[i].Magnitude;
            }
            return sum;
        }

        /// <summary>
        /// Calculates the L2 norm of the vector, also known as Euclidean norm.
        /// </summary>
        /// <returns>The square root of the sum of the squared values.</returns>
        public override double L2Norm()
        {
            // TODO: native provider
            return _values.Aggregate(Complex32.Zero, SpecialFunctions.Hypotenuse).Magnitude;
        }

        /// <summary>
        /// Calculates the infinity norm of the vector.
        /// </summary>
        /// <returns>The maximum absolute value.</returns>
        public override double InfinityNorm()
        {
            return CommonParallel.Aggregate(_values, (i, v) => v.Magnitude, Math.Max, 0f);
        }

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">The p value.</param>
        /// <returns>Scalar <c>ret = ( ∑|this[i]|^p )^(1/p)</c></returns>
        public override double Norm(double p)
        {
            if (p < 0d) throw new ArgumentOutOfRangeException("p");

            if (p == 1d) return L1Norm();
            if (p == 2d) return L2Norm();
            if (double.IsPositiveInfinity(p)) return InfinityNorm();

            double sum = 0d;
            for (var i = 0; i < _length; i++)
            {
                sum += Math.Pow(_values[i].Magnitude, p);
            }
            return Math.Pow(sum, 1.0 / p);
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        protected override void DoPointwiseMultiply(Vector<Complex32> other, Vector<Complex32> result)
        {
            var denseOther = other as DenseVector;
            var denseResult = result as DenseVector;

            if (denseOther == null || denseResult == null)
            {
                base.DoPointwiseMultiply(other, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWiseMultiplyArrays(_values, denseOther._values, denseResult._values);
            }
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="divisor">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        /// <remarks></remarks>
        protected override void DoPointwiseDivide(Vector<Complex32> divisor, Vector<Complex32> result)
        {
            var denseOther = divisor as DenseVector;
            var denseResult = result as DenseVector;

            if (denseOther == null || denseResult == null)
            {
                base.DoPointwiseDivide(divisor, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWiseDivideArrays(_values, denseOther._values, denseResult._values);
            }
        }

        /// <summary>
        /// Pointwise raise this vector to an exponent vector and store the result into the result vector.
        /// </summary>
        /// <param name="exponent">The exponent vector to raise this vector values to.</param>
        /// <param name="result">The vector to store the result of the pointwise power.</param>
        protected override void DoPointwisePower(Vector<Complex32> exponent, Vector<Complex32> result)
        {
            var denseExponent = exponent as DenseVector;
            var denseResult = result as DenseVector;

            if (denseExponent == null || denseResult == null)
            {
                base.DoPointwisePower(exponent, result);
            }
            else
            {
                Control.LinearAlgebraProvider.PointWisePowerArrays(_values, denseExponent._values, denseResult._values);
            }
        }

        #region Parse Functions

        /// <summary>
        /// Creates a Complex32 dense vector based on a string. The string can be in the following formats (without the
        /// quotes): 'n', 'n;n;..', '(n;n;..)', '[n;n;...]', where n is a double.
        /// </summary>
        /// <returns>
        /// A Complex32 dense vector containing the values specified by the given string.
        /// </returns>
        /// <param name="value">
        /// the string to parse.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        public static DenseVector Parse(string value, IFormatProvider formatProvider = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                throw new FormatException();
            }

            // strip out parens
            if (value.StartsWith("(", StringComparison.Ordinal))
            {
                if (!value.EndsWith(")", StringComparison.Ordinal))
                {
                    throw new FormatException();
                }

                value = value.Substring(1, value.Length - 2).Trim();
            }

            if (value.StartsWith("[", StringComparison.Ordinal))
            {
                if (!value.EndsWith("]", StringComparison.Ordinal))
                {
                    throw new FormatException();
                }

                value = value.Substring(1, value.Length - 2).Trim();
            }

            // parsing
            var strongTokens = value.Split(new[] { formatProvider.GetTextInfo().ListSeparator }, StringSplitOptions.RemoveEmptyEntries);
            var data = new List<Complex32>();
            foreach (string strongToken in strongTokens)
            {
                var weakTokens = strongToken.Split(new[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                string current = string.Empty;
                for (int i = 0; i < weakTokens.Length; i++)
                {
                    current += weakTokens[i];
                    if (current.EndsWith("+") || current.EndsWith("-") || current.StartsWith("(") && !current.EndsWith(")"))
                    {
                        continue;
                    }
                    var ahead = i < weakTokens.Length - 1 ? weakTokens[i + 1] : string.Empty;
                    if (ahead.StartsWith("+") || ahead.StartsWith("-"))
                    {
                        continue;
                    }
                    data.Add(current.ToComplex32(formatProvider));
                    current = string.Empty;
                }
                if (current != string.Empty)
                {
                    throw new FormatException();
                }
            }
            if (data.Count == 0)
            {
                throw new FormatException();
            }
            return new DenseVector(data.ToArray());
        }

        /// <summary>
        /// Converts the string representation of a complex dense vector to double-precision dense vector equivalent.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">
        /// A string containing a complex vector to convert.
        /// </param>
        /// <param name="result">
        /// The parsed value.
        /// </param>
        /// <returns>
        /// If the conversion succeeds, the result will contain a complex number equivalent to value.
        /// Otherwise the result will be <c>null</c>.
        /// </returns>
        public static bool TryParse(string value, out DenseVector result)
        {
            return TryParse(value, null, out result);
        }

        /// <summary>
        /// Converts the string representation of a complex dense vector to double-precision dense vector equivalent.
        /// A return value indicates whether the conversion succeeded or failed.
        /// </summary>
        /// <param name="value">
        /// A string containing a complex vector to convert.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information about value.
        /// </param>
        /// <param name="result">
        /// The parsed value.
        /// </param>
        /// <returns>
        /// If the conversion succeeds, the result will contain a complex number equivalent to value.
        /// Otherwise the result will be <c>null</c>.
        /// </returns>
        public static bool TryParse(string value, IFormatProvider formatProvider, out DenseVector result)
        {
            bool ret;
            try
            {
                result = Parse(value, formatProvider);
                ret = true;
            }
            catch (ArgumentNullException)
            {
                result = null;
                ret = false;
            }
            catch (FormatException)
            {
                result = null;
                ret = false;
            }

            return ret;
        }

        #endregion
    }
}
