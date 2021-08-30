using System;
using System.Linq;

namespace MathNet.Numerics.LinearAlgebra
{
    /*------------------------------------------------------------------------------------------------------
     * This partial class file(Vector.Indexers.cs) is used for add some indexer for Vector<T> class.
     * I implement these functions mainly through int, int Array, ValueTuple struct, Range struct(.NET5).
     * They can change the usage of indexer function of MathNet.Numerics.LinearAlgebra.Vector.
     * I have simply indicated the use method on each indexer function.
     * So everyone can use these indexers to Get or Set SubVector more easily.
     * Enjoy it!
     * 
     * If you have any questions, suggestions, comments or ideas, please contact the author.
     * Thanks for your feedbacks.
     *                              Author Email: zhouxutao@163.com
     *                              Date: 2021.08.30
     -------------------------------------------------------------------------------------------------------*/
    public abstract partial class Vector<T>
    {
        /// <summary>
        /// Get the SubVector or Set the SubElement by ValueTuple (start, end) struct Indexes.
        /// </summary>
        /// 
        /// <param name="indexes">
        /// ValueTuple (start, end) struct Indexes.
        /// The End Must be Larger than the Start.
        /// </param>
        /// 
        /// <returns>A SubVector.</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneVector[(1, 4)];
        ///     etc.
        /// Set:
        ///     oneVector[(1, 4)] = otherMatrix[(1, 4), 6];
        ///     oneVector[(1, 4)] = otherVector[(2, 5)];
        ///     etc.
        /// </example>
        public Vector<T> this[ValueTuple<int, int> indexes]
        {
            get
            {
                VerifyValueTuple(indexes, Count);

                int sourceIndex = indexes.Item1;
                int length = indexes.Item2 - indexes.Item1 + 1;

                var result = Build.SameAs(this, length);
                Storage.CopySubVectorTo(result.Storage, sourceIndex, 0, length);
                return result;
            }

            set
            {
                VerifyValueTuple(indexes, Count);

                int targetIndex = indexes.Item1;
                int length = indexes.Item2 - indexes.Item1 + 1;

                if (length != value.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(indexes));
                }

                value.Storage.CopySubVectorTo(Storage, 0, targetIndex, length);
            }
        }

        /// <summary>
        /// Get the SubVector or Set the SubElement by ValueTuple (start, step, end) struct Indexes.
        /// </summary>
        /// 
        /// <param name="indexes">
        /// ValueTuple (start, step, end) struct Indexes.
        /// It will be a LinearRange by (start, step, end). The End Must be Larger than the Start.
        /// </param>
        /// 
        /// <returns>A SubVector.</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneVector[(1, 2, 5)];
        ///     etc.
        /// Set:
        ///     oneVector[(1, 2, 5)] = otherMatrix[ 3, (1, 2, 5)];
        ///     oneVector[(1, 2, 5)] = otherVector[(2, 2, 6)];
        ///     etc.
        /// </example>
        public Vector<T> this[ValueTuple<int, int, int> indexes]
        {
            get
            {
                VerifyValueTuple(indexes, Count);

                int[] vectorIndex = Generate.LinearRangeInt32(indexes.Item1, indexes.Item2, indexes.Item3);

                var result = Build.SameAs(this, vectorIndex.Length);
                for (int i = 0; i < vectorIndex.Length; i++)
                {
                    result.Storage[i] = Storage[vectorIndex[i]];
                }
                return result;
            }

            set
            {
                VerifyValueTuple(indexes, Count);

                int[] vectorIndex = Generate.LinearRangeInt32(indexes.Item1, indexes.Item2, indexes.Item3);

                if (vectorIndex.Length != value.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(indexes));
                }

                for (int i = 0; i < vectorIndex.Length; i++)
                {
                    Storage[vectorIndex[i]] = value.Storage[i];
                }
            }
        }

        /// <summary>
        /// Get the SubVector or Set the SubElement by int Array Indexes.
        /// </summary>
        /// 
        /// <param name="indexes">
        /// int Array Indexes.
        /// The Length of it Must be Larger than One.
        /// </param>
        /// 
        /// <returns>A SubVector.</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneVector[new int[]{1, 2, 6, 2}];
        ///     etc.
        /// Set:
        ///     oneVector[new int[]{1, 2, 6, 2}] = otherVector[new int[]{2, 3, 6, 5}];
        ///     etc.
        /// </example>
        public Vector<T> this[int[] indexes]
        {
            get
            {
                VerifyArray(indexes, Count);

                var result = Build.SameAs(this, indexes.Length);
                for (int i = 0; i < indexes.Length; i++)
                {
                    result.Storage[i] = Storage[indexes[i]];
                }
                return result;
            }

            set
            {
                VerifyArray(indexes, Count);

                if (indexes.Length != value.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(indexes));
                }

                for (int i = 0; i < indexes.Length; i++)
                {
                    Storage[indexes[i]] = value.Storage[i];
                }
            }
        }

#if NET5_0_OR_GREATER

        /// <summary>
        /// Get the SubVector or Set the SubElement by Range struct Indexes.
        /// </summary>
        /// 
        /// <param name="indexes">
        /// Range struct Indexes.
        /// he End Must be Larger than the Start. The End Range Index will be contain in the Indexes.
        /// </param>
        /// 
        /// <returns>A SubVector.</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneVector[1..5];
        ///     var vec = oneVector[..5];
        ///     var vec = oneVector[1..]
        ///     var vec = oneVector[^3..^1];
        ///     var vec = oneVector[..^1];
        ///     etc.
        /// Set:
        ///     oneVector[1..5] = otherVector[2..6];
        ///     oneVector[^3..^1] = otherVector[^3..^1];
        ///     etc.
        /// </example>
        public Vector<T> this[Range indexes]
        {
            get
            {
                var (startIndex, endIndex) = VerifyRange(indexes, Count);

                int length = endIndex - startIndex + 1;

                var result = Build.SameAs(this, length);
                Storage.CopySubVectorTo(result.Storage, startIndex, 0, length);
                return result;
            }

            set
            {
                var (startIndex, endIndex) = VerifyRange(indexes, Count);

                int length = endIndex - startIndex + 1;

                if (length != value.Count)
                {
                    throw new ArgumentException($"The argument Row Range:{indexes} is wrong!");
                }

                value.Storage.CopySubVectorTo(Storage, 0, startIndex, length);

            }
        }

#endif

        #region Used for Verifying the availability of Indexers

#if NET5_0_OR_GREATER

        /// <summary>
        /// Used for Verifying the availability of Range struct Indexes. 
        /// </summary>
        /// <param name="range">Range struct Indexes.</param>
        /// <param name="count">The Count of the Vector.</param>
        /// <returns name="startIndex">The Start of the Range.</returns>
        /// <returns name="endIndex">The End of the Range.</returns>
        private (int startIndex, int endIndex) VerifyRange(Range range, int count)
        {
            int startIndex = range.Start.IsFromEnd ? count - range.Start.Value - 1 : range.Start.Value;
            int endIndex = range.End.IsFromEnd ? count - range.End.Value - 1 : range.End.Value;

            if (startIndex < 0 || endIndex >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            return (startIndex, endIndex);
        }

#endif

        /// <summary>
        /// Used for Verifying the availability of int Array Indexes.
        /// </summary>
        /// <param name="array">int Array Indexes.</param>
        /// <param name="count">The Count of the Vector.</param>
        private void VerifyArray(int[] array, int count)
        {
            if (array.Length <= 1)
            {
                throw new ArgumentException("The Length of int array must Larger than One!");
            }

            if (array.Min() < 0 || array.Max() >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(array));
            }
        }

        /// <summary>
        /// Used for Verifying the availability of ValueTuple (start, end) struct Indexes.
        /// </summary>
        /// <param name="vt">ValueTuple (start, end) struct Indexes.</param>
        /// <param name="count">The Count of the Vector.</param>
        private void VerifyValueTuple(ValueTuple<int, int> vt, int count)
        {
            if (vt.Item1 < 0 || vt.Item2 >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(vt));
            }

            if (vt.Item2 <= vt.Item1)
            {
                throw new ArgumentException("The Length of ValueTuple must Larger than One!");
            }
        }

        /// <summary>
        /// Used for Verifying the availability of ValueTuple (start, step, end) struct Indexes.
        /// </summary>
        /// <param name="vt">ValueTuple (start, step, end) struct Indexes.</param>
        /// <param name="count">The Count of the Vector.</param>
        private void VerifyValueTuple(ValueTuple<int, int, int> vt, int count)
        {
            if (vt.Item1 < 0 || vt.Item3 >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(vt));
            }

            if (vt.Item3 <= vt.Item1)
            {
                throw new ArgumentException("The Length of ValueTuple must Larger than One!");
            }
        }

        /// <summary>
        /// Used for Verifying the availability of int Single Index.
        /// </summary>
        /// <param name="index">int Single Index.</param>
        /// <param name="count">The Count of the Vector.</param>
        private void VerifySingleIndex(int index, int count)
        {
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        #endregion
    }
}

