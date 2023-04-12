// <copyright file="Vector.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>
    /// Defines the generic class for <c>Vector</c> classes.
    /// </summary>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    [Serializable]
    [DebuggerTypeProxy(typeof(VectorDebuggingView<>))]
    public abstract partial class Vector<T> : IFormattable, IEquatable<Vector<T>>, IList, IList<T>, ICloneable
        where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Initializes a new instance of the Vector class.
        /// </summary>
        protected Vector(VectorStorage<T> storage)
        {
            Storage = storage;
            Count = storage.Length;
        }

        public static readonly VectorBuilder<T> Build = BuilderInstance<T>.Vector;

        /// <summary>
        /// Gets the raw vector data storage.
        /// </summary>
        public VectorStorage<T> Storage { get; private set; }

        /// <summary>
        /// Gets the length or number of dimensions of this vector.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>Gets or sets the value at the given <paramref name="index"/>.</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is negative or
        /// greater than the size of the vector.</exception>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get { return Storage[index]; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            set { Storage[index] = value; }
        }

        /// <summary>Gets the value at the given <paramref name="index"/> without range checking..</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public T At(int index)
        {
            return Storage.At(index);
        }

        /// <summary>Sets the <paramref name="value"/> at the given <paramref name="index"/> without range checking..</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
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
                throw new ArgumentOutOfRangeException(nameof(count), "Value must be positive.");
            }

            if (index + count > Count || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            Storage.Clear(index, count);
        }

        /// <summary>
        /// Set all values whose absolute value is smaller than the threshold to zero, in-place.
        /// </summary>
        public abstract void CoerceZero(double threshold);

        /// <summary>
        /// Set all values that meet the predicate to zero, in-place.
        /// </summary>
        public void CoerceZero(Func<T, bool> zeroPredicate)
        {
            MapInplace(x => zeroPredicate(x) ? Zero : x, Zeros.AllowSkip);
        }

        /// <summary>
        /// Returns a deep-copy clone of the vector.
        /// </summary>
        /// <returns>A deep-copy clone of the vector.</returns>
        public Vector<T> Clone()
        {
            var result = Build.SameAs(this);
            Storage.CopyToUnchecked(result.Storage, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Set the values of this vector to the given values.
        /// </summary>
        /// <param name="values">The array containing the values to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="values"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="values"/> is not the same size as this vector.</exception>
        public void SetValues(T[] values)
        {
            var source = new DenseVectorStorage<T>(Count, values);
            source.CopyTo(Storage);
        }

        /// <summary>
        /// Copies the values of this vector into the target vector.
        /// </summary>
        /// <param name="target">The vector to copy elements into.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="target"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="target"/> is not the same size as this vector.</exception>
        public void CopyTo(Vector<T> target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Storage.CopyTo(target.Storage);
        }

        /// <summary>
        /// Creates a vector containing specified elements.
        /// </summary>
        /// <param name="index">The first element to begin copying from.</param>
        /// <param name="count">The number of elements to copy.</param>
        /// <returns>A vector containing a copy of the specified elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><list><item>If <paramref name="index"/> is not positive or
        /// greater than or equal to the size of the vector.</item>
        /// <item>If <paramref name="index"/> + <paramref name="count"/> is greater than or equal to the size of the vector.</item>
        /// </list></exception>
        /// <exception cref="ArgumentException">If <paramref name="count"/> is not positive.</exception>
        public Vector<T> SubVector(int index, int count)
        {
            var target = Build.SameAs(this, count);
            Storage.CopySubVectorTo(target.Storage, index, 0, count, ExistingData.AssumeZeros);
            return target;
        }

        /// <summary>
        /// Copies the values of a given vector into a region in this vector.
        /// </summary>
        /// <param name="index">The field to start copying to</param>
        /// <param name="count">The number of fields to copy. Must be positive.</param>
        /// <param name="subVector">The sub-vector to copy from.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="subVector"/> is <see langword="null" /></exception>
        public void SetSubVector(int index, int count, Vector<T> subVector)
        {
            if (subVector == null)
            {
                throw new ArgumentNullException(nameof(subVector));
            }

            subVector.Storage.CopySubVectorTo(Storage, 0, index, count);
        }

        /// <summary>
        /// Copies the requested elements from this vector to another.
        /// </summary>
        /// <param name="destination">The vector to copy the elements to.</param>
        /// <param name="sourceIndex">The element to start copying from.</param>
        /// <param name="targetIndex">The element to start copying to.</param>
        /// <param name="count">The number of elements to copy.</param>
        public void CopySubVectorTo(Vector<T> destination, int sourceIndex, int targetIndex, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            // TODO: refactor range checks
            Storage.CopySubVectorTo(destination.Storage, sourceIndex, targetIndex, count);
        }

        /// <summary>
        /// Returns the data contained in the vector as an array.
        /// The returned array will be independent from this vector.
        /// A new memory block will be allocated for the array.
        /// </summary>
        /// <returns>The vector's data as an array.</returns>
        public T[] ToArray()
        {
            return Storage.ToArray();
        }

        /// <summary>
        /// Returns the internal array of this vector if, and only if, this vector is stored by such an array internally.
        /// Otherwise returns null. Changes to the returned array and the vector will affect each other.
        /// Use ToArray instead if you always need an independent array.
        /// </summary>
        public T[] AsArray()
        {
            return Storage.AsArray();
        }

        /// <summary>
        /// Create a matrix based on this vector in column form (one single column).
        /// </summary>
        /// <returns>
        /// This vector as a column matrix.
        /// </returns>
        public Matrix<T> ToColumnMatrix()
        {
            var result = Matrix<T>.Build.SameAs(this, Count, 1);
            Storage.CopyToColumnUnchecked(result.Storage, 0, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Create a matrix based on this vector in row form (one single row).
        /// </summary>
        /// <returns>
        /// This vector as a row matrix.
        /// </returns>
        public Matrix<T> ToRowMatrix()
        {
            var result = Matrix<T>.Build.SameAs(this, 1, Count);
            Storage.CopyToRowUnchecked(result.Storage, 0, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the vector.
        /// </summary>
        /// <remarks>
        /// The enumerator will include all values, even if they are zero.
        /// </remarks>
        public IEnumerable<T> Enumerate()
        {
            return Storage.Enumerate();
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the vector.
        /// </summary>
        /// <remarks>
        /// The enumerator will include all values, even if they are zero.
        /// </remarks>
        public IEnumerable<T> Enumerate(Zeros zeros)
        {
            switch (zeros)
            {
                case Zeros.AllowSkip:
                    return Storage.EnumerateNonZero();
                default:
                    return Storage.Enumerate();
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the vector and their index.
        /// </summary>
        /// <remarks>
        /// The enumerator returns a Tuple with the first value being the element index
        /// and the second value being the value of the element at that index.
        /// The enumerator will include all values, even if they are zero.
        /// </remarks>
        public IEnumerable<(int, T)> EnumerateIndexed()
        {
            return Storage.EnumerateIndexed();
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the vector and their index.
        /// </summary>
        /// <remarks>
        /// The enumerator returns a Tuple with the first value being the element index
        /// and the second value being the value of the element at that index.
        /// The enumerator will include all values, even if they are zero.
        /// </remarks>
        public IEnumerable<(int, T)> EnumerateIndexed(Zeros zeros)
        {
            switch (zeros)
            {
                case Zeros.AllowSkip:
                    return Storage.EnumerateNonZeroIndexed();
                default:
                    return Storage.EnumerateIndexed();
            }
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value with its result.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void MapInplace(Func<T, T> f, Zeros zeros = Zeros.AllowSkip)
        {
            Storage.MapInplace(f, zeros);
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value with its result.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void MapIndexedInplace(Func<int, T, T> f, Zeros zeros = Zeros.AllowSkip)
        {
            Storage.MapIndexedInplace(f, zeros);
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value in the result vector.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void Map(Func<T, T> f, Vector<T> result, Zeros zeros = Zeros.AllowSkip)
        {
            if (ReferenceEquals(this, result))
            {
                Storage.MapInplace(f, zeros);
            }
            else
            {
                Storage.MapTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
            }
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value in the result vector.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void MapIndexed(Func<int, T, T> f, Vector<T> result, Zeros zeros = Zeros.AllowSkip)
        {
            if (ReferenceEquals(this, result))
            {
                Storage.MapIndexedInplace(f, zeros);
            }
            else
            {
                Storage.MapIndexedTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
            }
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value in the result vector.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void MapConvert<TU>(Func<T, TU> f, Vector<TU> result, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            Storage.MapTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
        }

        /// <summary>
        /// Applies a function to each value of this vector and replaces the value in the result vector.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public void MapIndexedConvert<TU>(Func<int, T, TU> f, Vector<TU> result, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            Storage.MapIndexedTo(result.Storage, f, zeros, zeros == Zeros.Include ? ExistingData.AssumeZeros : ExistingData.Clear);
        }

        /// <summary>
        /// Applies a function to each value of this vector and returns the results as a new vector.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public Vector<TU> Map<TU>(Func<T, TU> f, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            var result = Vector<TU>.Build.SameAs(this);
            Storage.MapToUnchecked(result.Storage, f, zeros, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Applies a function to each value of this vector and returns the results as a new vector.
        /// The index of each value (zero-based) is passed as first argument to the function.
        /// If forceMapZero is not set to true, zero values may or may not be skipped depending
        /// on the actual data storage implementation (relevant mostly for sparse vectors).
        /// </summary>
        public Vector<TU> MapIndexed<TU>(Func<int, T, TU> f, Zeros zeros = Zeros.AllowSkip)
            where TU : struct, IEquatable<TU>, IFormattable
        {
            var result = Vector<TU>.Build.SameAs(this);
            Storage.MapIndexedToUnchecked(result.Storage, f, zeros, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Applies a function to each value pair of two vectors and replaces the value in the result vector.
        /// </summary>
        public void Map2(Func<T, T, T> f, Vector<T> other, Vector<T> result, Zeros zeros = Zeros.AllowSkip)
        {
            Storage.Map2To(result.Storage, other.Storage, f, zeros, ExistingData.Clear);
        }

        /// <summary>
        /// Applies a function to each value pair of two vectors and returns the results as a new vector.
        /// </summary>
        public Vector<T> Map2(Func<T, T, T> f, Vector<T> other, Zeros zeros = Zeros.AllowSkip)
        {
            var result = Build.SameAs(this);
            Storage.Map2To(result.Storage, other.Storage, f, zeros, ExistingData.AssumeZeros);
            return result;
        }

        /// <summary>
        /// Applies a function to update the status with each value pair of two vectors and returns the resulting status.
        /// </summary>
        public TState Fold2<TOther, TState>(Func<TState, T, TOther, TState> f, TState state, Vector<TOther> other, Zeros zeros = Zeros.AllowSkip)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            return Storage.Fold2(other.Storage, f, state, zeros);
        }

        /// <summary>
        /// Returns a tuple with the index and value of the first element satisfying a predicate, or null if none is found.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public Tuple<int, T> Find(Func<T, bool> predicate, Zeros zeros = Zeros.AllowSkip)
        {
            return Storage.Find(predicate, zeros);
        }

        /// <summary>
        /// Returns a tuple with the index and values of the first element pair of two vectors of the same size satisfying a predicate, or null if none is found.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public Tuple<int, T, TOther> Find2<TOther>(Func<T, TOther, bool> predicate, Vector<TOther> other, Zeros zeros = Zeros.AllowSkip)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            return Storage.Find2(other.Storage, predicate, zeros);
        }

        /// <summary>
        /// Returns true if at least one element satisfies a predicate.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public bool Exists(Func<T, bool> predicate, Zeros zeros = Zeros.AllowSkip)
        {
            return Storage.Find(predicate, zeros) != null;
        }

        /// <summary>
        /// Returns true if at least one element pairs of two vectors of the same size satisfies a predicate.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public bool Exists2<TOther>(Func<T, TOther, bool> predicate, Vector<TOther> other, Zeros zeros = Zeros.AllowSkip)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            return Storage.Find2(other.Storage, predicate, zeros) != null;
        }

        /// <summary>
        /// Returns true if all elements satisfy a predicate.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public bool ForAll(Func<T, bool> predicate, Zeros zeros = Zeros.AllowSkip)
        {
            return Storage.Find(x => !predicate(x), zeros) == null;
        }

        /// <summary>
        /// Returns true if all element pairs of two vectors of the same size satisfy a predicate.
        /// Zero elements may be skipped on sparse data structures if allowed (default).
        /// </summary>
        public bool ForAll2<TOther>(Func<T, TOther, bool> predicate, Vector<TOther> other, Zeros zeros = Zeros.AllowSkip)
            where TOther : struct, IEquatable<TOther>, IFormattable
        {
            return Storage.Find2(other.Storage, (x, y) => !predicate(x, y), zeros) == null;
        }
    }

    internal class VectorDebuggingView<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        private readonly Vector<T> _vector;

        public VectorDebuggingView(Vector<T> vector)
        {
            _vector = vector;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _vector.ToArray();
    }
}
