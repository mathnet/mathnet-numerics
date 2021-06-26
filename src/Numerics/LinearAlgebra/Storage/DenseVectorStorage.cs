// <copyright file="DenseVectorStorage.cs" company="Math.NET">
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
using System.Linq;
using System.Runtime.Serialization;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    [DataContract(Namespace = "urn:MathNet/Numerics/LinearAlgebra")]
    public class DenseVectorStorage<T> : VectorStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

        [DataMember(Order = 1)]
        public readonly T[] Data;

        internal DenseVectorStorage(int length)
            : base(length)
        {
            Data = new T[length];
        }

        internal DenseVectorStorage(int length, T[] data)
            : base(length)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length != length)
            {
                throw new ArgumentOutOfRangeException(nameof(data), $"The given array has the wrong length. Should be {length}.");
            }

            Data = data;
        }

        /// <summary>
        /// True if the vector storage format is dense.
        /// </summary>
        public override bool IsDense => true;

        /// <summary>
        /// Retrieves the requested element without range checking.
        /// </summary>
        public override T At(int index)
        {
            return Data[index];
        }

        /// <summary>
        /// Sets the element without range checking.
        /// </summary>
        public override void At(int index, T value)
        {
            Data[index] = value;
        }

        // CLEARING

        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public override void Clear(int index, int count)
        {
            Array.Clear(Data, index, count);
        }

        // INITIALIZATION

        public static DenseVectorStorage<T> OfVector(VectorStorage<T> vector)
        {
            var storage = new DenseVectorStorage<T>(vector.Length);
            vector.CopyToUnchecked(storage, ExistingData.AssumeZeros);
            return storage;
        }

        public static DenseVectorStorage<T> OfValue(int length, T value)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Value must not be negative (zero is ok).");
            }

            var data = new T[length];
            CommonParallel.For(0, data.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    data[i] = value;
                }
            });
            return new DenseVectorStorage<T>(length, data);
        }

        public static DenseVectorStorage<T> OfInit(int length, Func<int, T> init)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Value must not be negative (zero is ok).");
            }

            var data = new T[length];
            CommonParallel.For(0, data.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    data[i] = init(i);
                }
            });
            return new DenseVectorStorage<T>(length, data);
        }

        public static DenseVectorStorage<T> OfEnumerable(IEnumerable<T> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data is T[] arrayData)
            {
                var copy = new T[arrayData.Length];
                Array.Copy(arrayData, 0, copy, 0, arrayData.Length);
                return new DenseVectorStorage<T>(copy.Length, copy);
            }

            var array = data.ToArray();
            return new DenseVectorStorage<T>(array.Length, array);
        }

        public static DenseVectorStorage<T> OfIndexedEnumerable(int length, IEnumerable<Tuple<int, T>> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var array = new T[length];
            foreach (var (index, value) in data)
            {
                array[index] = value;
            }
            return new DenseVectorStorage<T>(array.Length, array);
        }

        public static DenseVectorStorage<T> OfIndexedEnumerable(int length, IEnumerable<(int, T)> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var array = new T[length];
            foreach (var (index, value) in data)
            {
                array[index] = value;
            }
            return new DenseVectorStorage<T>(array.Length, array);
        }

        // VECTOR COPY

        internal override void CopyToUnchecked(VectorStorage<T> target, ExistingData existingData)
        {
            if (target is DenseVectorStorage<T> denseTarget)
            {
                if (!ReferenceEquals(this, denseTarget))
                {
                    Array.Copy(Data, 0, denseTarget.Data, 0, Data.Length);
                }

                return;
            }

            if (target is SparseVectorStorage<T> sparseTarget)
            {
                var indices = new List<int>();
                var values = new List<T>();

                for (int i = 0; i < Data.Length; i++)
                {
                    var item = Data[i];
                    if (!Zero.Equals(item))
                    {
                        values.Add(item);
                        indices.Add(i);
                    }
                }

                sparseTarget.Indices = indices.ToArray();
                sparseTarget.Values = values.ToArray();
                sparseTarget.ValueCount = values.Count;
                return;
            }

            // FALL BACK

            for (int i = 0; i < Data.Length; i++)
            {
                target.At(i, Data[i]);
            }
        }

        // ROW COPY

        internal override void CopyToRowUnchecked(MatrixStorage<T> target, int rowIndex, ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<T> denseTarget)
            {
                for (int j = 0; j < Data.Length; j++)
                {
                    denseTarget.Data[j*target.RowCount + rowIndex] = Data[j];
                }
                return;
            }

            // FALL BACK

            for (int j = 0; j < Length; j++)
            {
                target.At(rowIndex, j, Data[j]);
            }
        }

        // COLUMN COPY

        internal override void CopyToColumnUnchecked(MatrixStorage<T> target, int columnIndex, ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<T> denseTarget)
            {
                Array.Copy(Data, 0, denseTarget.Data, columnIndex*denseTarget.RowCount, Data.Length);
                return;
            }

            // FALL BACK

            for (int i = 0; i < Length; i++)
            {
                target.At(i, columnIndex, Data[i]);
            }
        }

        // SUB-VECTOR COPY

        internal override void CopySubVectorToUnchecked(VectorStorage<T> target,
            int sourceIndex, int targetIndex, int count, ExistingData existingData)
        {
            if (target is DenseVectorStorage<T> denseTarget)
            {
                Array.Copy(Data, sourceIndex, denseTarget.Data, targetIndex, count);
                return;
            }

            // FALL BACK

            base.CopySubVectorToUnchecked(target, sourceIndex, targetIndex, count, existingData);
        }

        // SUB-ROW COPY

        internal override void CopyToSubRowUnchecked(MatrixStorage<T> target, int rowIndex,
            int sourceColumnIndex, int targetColumnIndex, int columnCount, ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<T> denseTarget)
            {
                for (int j = 0; j < Data.Length; j++)
                {
                    denseTarget.Data[(j + targetColumnIndex)*target.RowCount + rowIndex] = Data[j + sourceColumnIndex];
                }
                return;
            }

            // FALL BACK

            for (int j = sourceColumnIndex, jj = targetColumnIndex; j < sourceColumnIndex + columnCount; j++, jj++)
            {
                target.At(rowIndex, jj, Data[j]);
            }
        }

        // SUB-COLUMN COPY

        internal override void CopyToSubColumnUnchecked(MatrixStorage<T> target, int columnIndex,
            int sourceRowIndex, int targetRowIndex, int rowCount, ExistingData existingData)
        {
            if (target is DenseColumnMajorMatrixStorage<T> denseTarget)
            {
                Array.Copy(Data, sourceRowIndex, denseTarget.Data, columnIndex*denseTarget.RowCount + targetRowIndex, rowCount);
                return;
            }

            // FALL BACK

            for (int i = sourceRowIndex, ii = targetRowIndex; i < sourceRowIndex + rowCount; i++, ii++)
            {
                target.At(ii, columnIndex, Data[i]);
            }
        }

        // EXTRACT

        public override T[] ToArray()
        {
            var ret = new T[Data.Length];
            Array.Copy(Data, 0, ret, 0, Data.Length);
            return ret;
        }

        public override T[] AsArray()
        {
            return Data;
        }

        // ENUMERATION

        public override IEnumerable<T> Enumerate()
        {
            return Data;
        }

        public override IEnumerable<(int, T)> EnumerateIndexed()
        {
            return Data.Select((t, i) => (i, t));
        }

        public override IEnumerable<T> EnumerateNonZero()
        {
            return Data.Where(x => !Zero.Equals(x));
        }

        public override IEnumerable<(int, T)> EnumerateNonZeroIndexed()
        {
            for (var i = 0; i < Data.Length; i++)
            {
                if (!Zero.Equals(Data[i]))
                {
                    yield return (i, Data[i]);
                }
            }
        }

        // FIND

        public override Tuple<int, T> Find(Func<T, bool> predicate, Zeros zeros)
        {
            for (int i = 0; i < Data.Length; i++)
            {
                if (predicate(Data[i]))
                {
                    return new Tuple<int, T>(i, Data[i]);
                }
            }
            return null;
        }

        internal override Tuple<int, T, TOther> Find2Unchecked<TOther>(VectorStorage<TOther> other, Func<T, TOther, bool> predicate, Zeros zeros)
        {
            if (other is DenseVectorStorage<TOther> denseOther)
            {
                TOther[] otherData = denseOther.Data;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (predicate(Data[i], otherData[i]))
                    {
                        return new Tuple<int, T, TOther>(i, Data[i], otherData[i]);

                    }
                }
                return null;
            }

            if (other is SparseVectorStorage<TOther> sparseOther)
            {
                int[] otherIndices = sparseOther.Indices;
                TOther[] otherValues = sparseOther.Values;
                int otherValueCount = sparseOther.ValueCount;
                TOther otherZero = BuilderInstance<TOther>.Matrix.Zero;
                int k = 0;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (k < otherValueCount && otherIndices[k] == i)
                    {
                        if (predicate(Data[i], otherValues[k]))
                        {
                            return new Tuple<int, T, TOther>(i, Data[i], otherValues[k]);
                        }
                        k++;
                    }
                    else
                    {
                        if (predicate(Data[i], otherZero))
                        {
                            return new Tuple<int, T, TOther>(i, Data[i], otherZero);
                        }
                    }
                }
                return null;
            }

            // FALLBACK

            return base.Find2Unchecked(other, predicate, zeros);
        }

        // FUNCTIONAL COMBINATORS: MAP

        public override void MapInplace(Func<T, T> f, Zeros zeros)
        {
            CommonParallel.For(0, Data.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    Data[i] = f(Data[i]);
                }
            });
        }

        public override void MapIndexedInplace(Func<int, T, T> f, Zeros zeros)
        {
            CommonParallel.For(0, Data.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    Data[i] = f(i, Data[i]);
                }
            });
        }

        internal override void MapToUnchecked<TU>(VectorStorage<TU> target, Func<T, TU> f, Zeros zeros, ExistingData existingData)
        {
            if (target is DenseVectorStorage<TU> denseTarget)
            {
                CommonParallel.For(0, Data.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        denseTarget.Data[i] = f(Data[i]);
                    }
                });
                return;
            }

            // FALL BACK

            for (int i = 0; i < Length; i++)
            {
                target.At(i, f(Data[i]));
            }
        }

        internal override void MapIndexedToUnchecked<TU>(VectorStorage<TU> target, Func<int, T, TU> f, Zeros zeros, ExistingData existingData)
        {
            if (target is DenseVectorStorage<TU> denseTarget)
            {
                CommonParallel.For(0, Data.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        denseTarget.Data[i] = f(i, Data[i]);
                    }
                });
                return;
            }

            // FALL BACK

            for (int i = 0; i < Length; i++)
            {
                target.At(i, f(i, Data[i]));
            }
        }

        internal override void Map2ToUnchecked(VectorStorage<T> target, VectorStorage<T> other, Func<T, T, T> f, Zeros zeros, ExistingData existingData)
        {
            if (target is SparseVectorStorage<T>)
            {
                // Recursive to dense target at first, since the operation is
                // effectively dense anyway because at least one operand is dense
                var intermediate = new DenseVectorStorage<T>(target.Length);
                Map2ToUnchecked(intermediate, other, f, zeros, ExistingData.AssumeZeros);
                intermediate.CopyTo(target, existingData);
                return;
            }

            var denseTarget = target as DenseVectorStorage<T>;
            if (denseTarget != null && other is DenseVectorStorage<T> denseOther)
            {
                CommonParallel.For(0, Data.Length, 4096, (a, b) =>
                {
                    for (int i = a; i < b; i++)
                    {
                        denseTarget.Data[i] = f(Data[i], denseOther.Data[i]);
                    }
                });

                return;
            }

            if (denseTarget != null && other is SparseVectorStorage<T> sparseOther)
            {
                T[] targetData = denseTarget.Data;
                int[] otherIndices = sparseOther.Indices;
                T[] otherValues = sparseOther.Values;
                int otherValueCount = sparseOther.ValueCount;

                int k = 0;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (k < otherValueCount && otherIndices[k] == i)
                    {
                        targetData[i] = f(Data[i], otherValues[k]);
                        k++;
                    }
                    else
                    {
                        targetData[i] = f(Data[i], Zero);
                    }
                }

                return;
            }

            base.Map2ToUnchecked(target, other, f, zeros, existingData);
        }

        // FUNCTIONAL COMBINATORS: FOLD

        internal override TState Fold2Unchecked<TOther, TState>(VectorStorage<TOther> other, Func<TState, T, TOther, TState> f, TState state, Zeros zeros)
        {
            if (other is DenseVectorStorage<TOther> denseOther)
            {
                var otherData = denseOther.Data;
                for (int i = 0; i < Data.Length; i++)
                {
                    state = f(state, Data[i], otherData[i]);
                }

                return state;
            }

            if (other is SparseVectorStorage<TOther> sparseOther)
            {
                int[] otherIndices = sparseOther.Indices;
                TOther[] otherValues = sparseOther.Values;
                int otherValueCount = sparseOther.ValueCount;
                TOther otherZero = BuilderInstance<TOther>.Vector.Zero;

                int k = 0;
                for (int i = 0; i < Data.Length; i++)
                {
                    if (k < otherValueCount && otherIndices[k] == i)
                    {
                        state = f(state, Data[i], otherValues[k]);
                        k++;
                    }
                    else
                    {
                        state = f(state, Data[i], otherZero);
                    }
                }

                return state;
            }

            return base.Fold2Unchecked(other, f, state, zeros);
        }
    }
}
