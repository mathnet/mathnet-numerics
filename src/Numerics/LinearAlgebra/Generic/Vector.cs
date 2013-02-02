// <copyright file="Vector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime;
    using System.Text;
    using Numerics;
    using Properties;
    using Storage;
    using Threading;

    /// <summary>
    /// Defines the generic class for <c>Vector</c> classes.
    /// </summary>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    [Serializable]
    public abstract class Vector<T> :
#if PORTABLE
    IFormattable, IEnumerable<T>, IEquatable<Vector<T>>
#else
    IFormattable, IEnumerable<T>, IEquatable<Vector<T>>, ICloneable
#endif
    where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// The zero value for type T.
        /// </summary>
        private static readonly T Zero = default(T);

        /// <summary>
        /// The value of 1.0 for type T.
        /// </summary>
        private static readonly T One = Common.SetOne<T>();

        /// <summary>
        /// Initializes a new instance of the Vector class.
        /// </summary>
        protected Vector(VectorStorage<T> storage)
        {
            Storage = storage;
            Count = storage.Length;
        }

        /// <summary>
        /// Gets the raw vector data storage.
        /// </summary>
        public VectorStorage<T> Storage { get; private set; }

        /// <summary>
        /// Gets the number of items.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>Gets or sets the value at the given <paramref name="index"/>.</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns> 
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative or 
        /// greater than the size of the vector.</exception>
        public T this[int index]
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
            get { return Storage[index]; }

            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
            set { Storage[index] = value;}
        }

        /// <summary>Gets the value at the given <paramref name="index"/> without range checking..</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
        public T At(int index)
        {
            return Storage.At(index);
        }

        /// <summary>Sets the <paramref name="value"/> at the given <paramref name="index"/> without range checking..</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <param name="value">The value to set.</param>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] .Net 4.5 only
        public void At(int index, T value)
        {
            Storage.At(index, value);
        }

        /// <summary>
        /// Resets all values to zero.
        /// </summary>
        public void Clear()
        {
            Storage.Clear();
        }

        /// <summary>
        /// Sets all values of a subvector to zero.
        /// </summary>
        public void ClearSubVector(int index, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count", Resources.ArgumentMustBePositive);
            }

            if (index + count > Count || index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            Storage.Clear(index, count);
        }

        /// <summary>
        /// Returns a deep-copy clone of the vector.
        /// </summary>
        /// <returns>
        /// A deep-copy clone of the vector.
        /// </returns>
        public Vector<T> Clone()
        {
            var result = CreateVector(Count);
            Storage.CopyToUnchecked(result.Storage, skipClearing: true);
            return result;
        }

        /// <summary>
        /// Copies the values of this vector into the target vector.
        /// </summary>
        /// <param name="target">
        /// The vector to copy elements into.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="target"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="target"/> is not the same size as this vector.
        /// </exception>
        public void CopyTo(Vector<T> target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            Storage.CopyTo(target.Storage);
        }

        /// <summary>
        /// Copies the requested elements from this vector to another.
        /// </summary>
        /// <param name="destination">
        /// The vector to copy the elements to.
        /// </param>
        /// <param name="sourceIndex">
        /// The element to start copying from.
        /// </param>
        /// <param name="targetIndex">
        /// The element to start copying to.
        /// </param>
        /// <param name="count">
        /// The number of elements to copy.
        /// </param>
        public void CopySubVectorTo(Vector<T> destination, int sourceIndex, int targetIndex, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            // TODO: refactor range checks
            Storage.CopySubVectorTo(destination.Storage, sourceIndex, targetIndex, count);
        }

        [Obsolete("Use CopySubVectorTo instead.")]
        public void CopyTo(Vector<T> destination, int sourceIndex, int targetIndex, int count)
        {
            CopySubVectorTo(destination, sourceIndex, targetIndex, count);
        }

        /// <summary>
        /// Creates a matrix with the given dimensions using the same storage type
        /// as this vector.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <returns>
        /// A matrix with the given dimensions.
        /// </returns>
        public abstract Matrix<T> CreateMatrix(int rows, int columns);

        /// <summary>
        /// Creates a <strong>Vector</strong> of the given size using the same storage type
        /// as this vector.
        /// </summary>
        /// <param name="size">
        /// The size of the <strong>Vector</strong> to create.
        /// </param>
        /// <returns>
        /// The new <c>Vector</c>.
        /// </returns>
        public abstract Vector<T> CreateVector(int size);

        #region Elementary operations

        /// <summary>
        /// Adds a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to add.
        /// </param>
        /// <returns>A copy of the vector with the scalar added.</returns>
        public virtual Vector<T> Add(T scalar)
        {
            if (scalar.Equals(Zero))
            {
                return Clone();
            }

            var result = CreateVector(Count);
            DoAdd(scalar, result);
            return result;
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
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Add(T scalar, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoAdd(scalar, result);
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
        protected abstract void DoAdd(T scalar, Vector<T> result);

        /// <summary>
        /// Returns a copy of this vector.
        /// </summary>
        /// <returns>
        /// This vector.
        /// </returns>
        /// <remarks>
        /// Added as an alternative to the unary addition operator.
        /// </remarks>
        public virtual Vector<T> Plus()
        {
            return Clone();
        }

        /// <summary>
        /// Adds another vector to this vector.
        /// </summary>
        /// <param name="other">
        /// The vector to add to this one.
        /// </param>
        /// <returns>A new vector containing the sum of both vectors.</returns>
        /// <exception cref="ArgumentNullException">
        /// If the other vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="other"/> are not the same size.
        /// </exception>
        public virtual Vector<T> Add(Vector<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = CreateVector(Count);
            DoAdd(other, result);
            return result;
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
        /// <exception cref="ArgumentNullException">
        /// If the other vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="other"/> are not the same size.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Add(Vector<T> other, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoAdd(other, result);
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
        protected abstract void DoAdd(Vector<T> other, Vector<T> result);

        /// <summary>
        /// Subtracts a scalar from each element of the vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to subtract.
        /// </param>
        /// <returns>A new vector containing the subtraction of this vector and the scalar.</returns>
        public virtual Vector<T> Subtract(T scalar)
        {
            if (scalar.Equals(default(T)))
            {
                return Clone();
            }

            var result = CreateVector(Count);
            DoSubtract(scalar, result);
            return result;
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
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Subtract(T scalar, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoSubtract(scalar, result);
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
        protected abstract void DoSubtract(T scalar, Vector<T> result);

        /// <summary>
        /// Returns a negated vector.
        /// </summary>
        /// <returns>
        /// The negated vector.
        /// </returns>
        /// <remarks>
        /// Added as an alternative to the unary negation operator.
        /// </remarks>
        public abstract Vector<T> Negate();

        /// <summary>
        /// Subtracts another vector from this vector.
        /// </summary>
        /// <param name="other">
        /// The vector to subtract from this one.
        /// </param>
        /// <returns>A new vector containing the subtraction of the the two vectors.</returns>
        /// <exception cref="ArgumentNullException">
        /// If the other vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="other"/> are not the same size.
        /// </exception>
        public virtual Vector<T> Subtract(Vector<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = CreateVector(Count);
            DoSubtract(other, result);
            return result;
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
        /// <exception cref="ArgumentNullException">
        /// If the other vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="other"/> are not the same size.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Subtract(Vector<T> other, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoSubtract(other, result);
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
        protected abstract void DoSubtract(Vector<T> other, Vector<T> result);

        /// <summary>
        /// Multiplies a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to multiply.
        /// </param>
        /// <returns>A new vector that is the multiplication of the vector and the scalar.</returns>
        public virtual Vector<T> Multiply(T scalar)
        {
            if (scalar.Equals(One))
            {
                return Clone();
            }

            var result = CreateVector(Count);
            DoMultiply(scalar, result);
            return result;
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
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Multiply(T scalar, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoMultiply(scalar, result);
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
        protected abstract void DoMultiply(T scalar, Vector<T> result);

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">
        /// The other vector to add.
        /// </param>
        /// <returns>s
        /// The result of the addition.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If <paramref name="other"/> is not of the same size.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="other"/> is <see langword="null"/>.
        /// </exception>
        public virtual T DotProduct(Vector<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            return DoDotProduct(other);
        }

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">
        /// The other vector to add.
        /// </param>
        /// <returns>s
        /// The result of the addition.
        /// </returns>
        protected abstract T DoDotProduct(Vector<T> other);

        /// <summary>
        /// Divides each element of the vector by a scalar.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to divide with.
        /// </param>
        /// <returns>A new vector that is the division of the vector and the scalar.</returns>
        public virtual Vector<T> Divide(T scalar)
        {
            if (scalar.Equals(One))
            {
                return Clone();
            }

            var result = CreateVector(Count);
            DoDivide(scalar, result);
            return result;
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
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Divide(T scalar, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoDivide(scalar, result);
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
        protected abstract void DoDivide(T scalar, Vector<T> result);

        /// <summary>
        /// Pointwise multiplies this vector with another vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <returns>A new vector which is the pointwise multiplication of the two vectors.</returns>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public virtual Vector<T> PointwiseMultiply(Vector<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = CreateVector(Count);
            DoPointwiseMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void PointwiseMultiply(Vector<T> other, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoPointwiseMultiply(other, result);
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        protected abstract void DoPointwiseMultiply(Vector<T> other, Vector<T> result);

        /// <summary>
        /// Pointwise divide this vector with another vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <returns>A new vector which is the pointwise division of the two vectors.</returns>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public virtual Vector<T> PointwiseDivide(Vector<T> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = CreateVector(Count);
            DoPointwiseDivide(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void PointwiseDivide(Vector<T> other, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoPointwiseDivide(other, result);
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">
        /// The vector to pointwise divide this one by.
        /// </param>
        /// <param name="result">
        /// The result of the division.
        /// </param>
        protected abstract void DoPointwiseDivide(Vector<T> other, Vector<T> result);

        /// <summary>
        /// Outer product of two vectors
        /// </summary>
        /// <param name="u">First vector</param>
        /// <param name="v">Second vector</param>
        /// <returns>Matrix M[i,j] = u[i]*v[j] </returns>
        /// <exception cref="ArgumentNullException">If the u vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the v vector is <see langword="null" />.</exception> 
        public static Matrix<T> OuterProduct(Vector<T> u, Vector<T> v)
        {
            if (u == null)
            {
                throw new ArgumentNullException("u");
            }

            if (v == null)
            {
                throw new ArgumentNullException("v");
            }

            var matrix = u.CreateMatrix(u.Count, v.Count);

            for (var i = 0; i < u.Count; i++)
            {
                matrix.SetRow(i, v.Multiply(u[i]));
            }
            
            return matrix;
        }

        /// <summary>
        /// Outer product of this and another vector.
        /// </summary>
        /// <param name="v">The vector to operate on.</param>
        /// <returns>
        /// Matrix M[i,j] = this[i] * v[j].
        /// </returns>
        /// <seealso cref="OuterProduct(Vector{T}, Vector{T})"/>
        public Matrix<T> OuterProduct(Vector<T> v)
        {
            return OuterProduct(this, v);
        }

        /// <summary>
        /// Returns the value of the absolute minimum element.
        /// </summary>
        /// <returns>The value of the absolute minimum element.</returns>
        public abstract T AbsoluteMinimum();

        /// <summary>
        /// Returns the index of the absolute minimum element.
        /// </summary>
        /// <returns>The index of absolute minimum element.</returns>   
        public abstract int AbsoluteMinimumIndex();

        /// <summary>
        /// Returns the value of the absolute maximum element.
        /// </summary>
        /// <returns>The value of the absolute maximum element.</returns>
        public abstract T AbsoluteMaximum();

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>   
        public abstract int AbsoluteMaximumIndex();

        /// <summary>
        /// Returns the value of maximum element.
        /// </summary>
        /// <returns>The value of maximum element.</returns>        
        public virtual T Maximum()
        {
            return this[MaximumIndex()];
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>          
        public abstract int MaximumIndex();

        /// <summary>
        /// Returns the value of the minimum element.
        /// </summary>
        /// <returns>The value of the minimum element.</returns>
        public virtual T Minimum()
        {
            return this[MinimumIndex()];
        }

        /// <summary>
        /// Returns the index of the minimum element.
        /// </summary>
        /// <returns>The index of minimum element.</returns>  
        public abstract int MinimumIndex();

        /// <summary>
        /// Computes the sum of the vector's elements.
        /// </summary>
        /// <returns>The sum of the vector's elements.</returns>
        public abstract T Sum();

        /// <summary>
        /// Computes the sum of the absolute value of the vector's elements.
        /// </summary>
        /// <returns>The sum of the absolute value of the vector's elements.</returns>
        public abstract T SumMagnitudes();

        /// <summary>
        /// Computes the modulus for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <returns>A vector containing the result.</returns>
        public virtual Vector<T> Modulus(T divisor)
        {
            var result = CreateVector(Count);
            Modulus(divisor, result);
            return result;
        }

        /// <summary>
        /// Computes the modulus for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        public virtual void Modulus(T divisor, Vector<T> result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            DoModulus(divisor, result);
        }

        /// <summary>
        /// Computes the modulus for each element of the vector for the given divisor.
        /// </summary>
        /// <param name="divisor">The divisor to use.</param>
        /// <param name="result">A vector to store the results in.</param>
        protected abstract void DoModulus(T divisor, Vector<T> result);

        #endregion

        #region Arithmetic Operator Overloading

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the same values of <paramref name="rightSide"/>. 
        /// </summary>
        /// <remarks>This method is included for completeness.</remarks>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<T> operator +(Vector<T> rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.Plus();
        }

        /// <summary>
        /// Adds two <strong>Vectors</strong> together and returns the results.
        /// </summary>
        /// <param name="leftSide">One of the vectors to add.</param>
        /// <param name="rightSide">The other vector to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<T> operator +(Vector<T> leftSide, Vector<T> rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.Count != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rightSide");
            }

            return leftSide.Add(rightSide);
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the negated values of <paramref name="rightSide"/>. 
        /// </summary>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the negated values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<T> operator -(Vector<T> rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.Negate();
        }

        /// <summary>
        /// Subtracts two <strong>Vectors</strong> and returns the results.
        /// </summary>
        /// <param name="leftSide">The vector to subtract from.</param>
        /// <param name="rightSide">The vector to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<T> operator -(Vector<T> leftSide, Vector<T> rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.Count != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rightSide");
            }

            return leftSide.Subtract(rightSide);
        }

        /// <summary>
        /// Multiplies a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The vector to scale.</param>
        /// <param name="rightSide">The scalar value.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Vector<T> operator *(Vector<T> leftSide, T rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return leftSide.Multiply(rightSide);
        }

        /// <summary>
        /// Multiplies a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The scalar value.</param>
        /// <param name="rightSide">The vector to scale.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector<T> operator *(T leftSide, Vector<T> rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.Multiply(leftSide);
        }

        /// <summary>
        /// Computes the dot product between two <strong>Vectors</strong>.
        /// </summary>
        /// <param name="leftSide">The left row vector.</param>
        /// <param name="rightSide">The right column vector.</param>
        /// <returns>The dot product between the two vectors.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static T operator *(Vector<T> leftSide, Vector<T> rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.Count != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rightSide");
            }

            return leftSide.DotProduct(rightSide);
        }

        /// <summary>
        /// Divides a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The vector to divide.</param>
        /// <param name="rightSide">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Vector<T> operator /(Vector<T> leftSide, T rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return leftSide.Divide(rightSide);
        }

        /// <summary>
        /// Computes the modulus of each element of the vector of the given divisor.
        /// </summary>
        /// <param name="leftSide">The vector whose elements we want to compute the modulus of.</param>
        /// <param name="rightSide">The divisor to use,</param>
        /// <returns>The result of the calculation</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Vector<T> operator %(Vector<T> leftSide, T rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            return leftSide.Modulus(rightSide);
        }

        #endregion

        #region Vector Norms

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">
        /// The p value.
        /// </param>
        /// <returns>
        /// <c>Scalar ret = (sum(abs(this[i])^p))^(1/p)</c>
        /// </returns>
        public abstract T Norm(double p);

        /// <summary>
        /// Normalizes this vector to a unit vector with respect to the p-norm.
        /// </summary>
        /// <param name="p">
        /// The p value.
        /// </param>
        /// <returns>
        /// This vector normalized to a unit vector with respect to the p-norm.
        /// </returns>
        public abstract Vector<T> Normalize(double p);

        #endregion

        /// <summary>
        /// Return vector with conjugate values of the source vector
        /// </summary>
        /// <returns>Conjugated vector</returns>
        public Vector<T> Conjugate()
        {
            var retrunVector = CreateVector(Count);
            Conjugate(retrunVector);
            return retrunVector;
        }

        /// <summary>
        /// Conjugates vector and save result to <paramref name="target"/>
        /// </summary>
        /// <param name="target">Target vector</param>
        public virtual void Conjugate(Vector<T> target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (Count != target.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "target");
            }

            DoConjugate(target);
        }

        /// <summary>
        /// Conjugates vector and save result to <paramref name="target"/>
        /// </summary>
        /// <param name="target">Target vector</param>
        protected abstract void DoConjugate(Vector<T> target);

        #region Copying and Conversion

        /// <summary>
        /// Returns the data contained in the vector as an array.
        /// </summary>
        /// <returns>
        /// The vector's data as an array.
        /// </returns>
        public virtual T[] ToArray()
        {
            var ret = new T[Count];
            for (var i = 0; i < ret.Length; i++)
            {
                ret[i] = At(i);
            }

            return ret;
        }

        /// <summary>
        /// Create a matrix based on this vector in column form (one single column).
        /// </summary>
        /// <returns>
        /// This vector as a column matrix.
        /// </returns>
        public virtual Matrix<T> ToColumnMatrix()
        {
            var matrix = CreateMatrix(Count, 1);
            for (var i = 0; i < Count; i++)
            {
                matrix.At(i, 0, this[i]);
            }

            return matrix;
        }

        /// <summary>
        /// Create a matrix based on this vector in row form (one single row).
        /// </summary>
        /// <returns>
        /// This vector as a row matrix.
        /// </returns>
        public virtual Matrix<T> ToRowMatrix()
        {
            var matrix = CreateMatrix(1, Count);
            for (var i = 0; i < Count; i++)
            {
                matrix.At(0, i, this[i]);
            }

            return matrix;
        }

        /// <summary>
        /// Creates a vector containing specified elements.
        /// </summary>
        /// <param name="index">The first element to begin copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>A vector containing a copy of the specified elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><list><item>If <paramref name="index"/> is not positive or
        /// greater than or equal to the size of the vector.</item>
        /// <item>If <paramref name="index"/> + <paramref name="length"/> is greater than or equal to the size of the vector.</item>
        /// </list></exception>
        /// <exception cref="ArgumentException">If <paramref name="length"/> is not positive.</exception>
        public virtual Vector<T> SubVector(int index, int length)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (index + length > Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var result = CreateVector(length);

            CommonParallel.For(
                index, 
                index + length, 
                i => result[i - index] = this[i]);
            return result;
        }

        /// <summary>
        /// Set the values of this vector to the given values.
        /// </summary>
        /// <param name="values">The array containing the values to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="values"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="values"/> is not the same size as this vector.</exception>
        public virtual void SetValues(T[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length != Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "values");
            }

            CommonParallel.For(
                0, 
                values.Length, 
                i => this[i] = values[i]);
        }

        #endregion

        #region Implemented Interfaces

#if !PORTABLE

        #region ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

#endif

        #region IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEnumerable<T>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            for (var index = 0; index < Count; index++)
            {
                yield return this[index];
            }
        }

        #endregion

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that contains the position and value of the element.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> over this vector that contains the position and value of each
        /// element.
        /// </returns>
        /// <remarks>
        /// The enumerator returns a 
        /// <seealso cref="Tuple{T,K}"/>
        /// with the first value being the element index and the second value 
        /// being the value of the element at that index. For sparse vectors, the enumerator will exclude all elements
        /// with a zero value.
        /// </remarks>
        public virtual IEnumerable<Tuple<int, T>> GetIndexedEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return new Tuple<int, T>(i, this[i]);
            }
        }

        #region IEquatable<Vector>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Equals(Vector<T> other)
        {
            // Reject equality when the argument is null or has a different length.
            if (other == null)
            {
                return false;
            }

            if (Count != other.Count)
            {
                return false;
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // If all else fails, perform element wise comparison.
            for (var index = 0; index < Count; index++)
            {
                if (!this[index].Equals(other[index]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IFormattable

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">
        /// The format to use.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < Count; index++)
            {
                stringBuilder.Append(this[index].ToString(format, formatProvider));
                if (index != Count - 1)
                {
                    stringBuilder.Append(formatProvider.GetTextInfo().ListSeparator);
                }
            }

            return stringBuilder.ToString();
        }
        #endregion

        #endregion

        #region System.Object overrides

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="System.Object"/> to compare with this instance.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Vector<T>);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(Count, 20);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
#if PORTABLE
                hash ^= Precision.DoubleToInt64Bits(this[i].GetHashCode());
#else
                hash ^= BitConverter.DoubleToInt64Bits(this[i].GetHashCode());
#endif
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(hash), 4);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        #endregion
    }
}
