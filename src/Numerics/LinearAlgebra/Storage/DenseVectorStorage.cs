using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    [Serializable]
    public class DenseVectorStorage<T> : VectorStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        // [ruegg] public fields are OK here

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
                throw new ArgumentNullException("data");
            }

            if (data.Length != length)
            {
                throw new ArgumentOutOfRangeException("data", string.Format(Resources.ArgumentArrayWrongLength, length));
            }

            Data = data;
        }

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

        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public override void Clear(int index, int count)
        {
            Array.Clear(Data, index, count);
        }

        internal override void CopyToUnchecked(VectorStorage<T> target, bool skipClearing = false)
        {
            var denseTarget = target as DenseVectorStorage<T>;
            if (denseTarget != null)
            {
                CopyToUnchecked(denseTarget);
                return;
            }

            // FALL BACK

            for (int i = 0; i < Length; i++)
            {
                target.At(i, Data[i]);
            }
        }

        void CopyToUnchecked(DenseVectorStorage<T> target)
        {
            if (ReferenceEquals(this, target))
            {
                return;
            }

            if (Length != target.Length)
            {
                var message = string.Format(Resources.ArgumentMatrixDimensions2, Length, target.Length);
                throw new ArgumentException(message, "target");
            }

            Array.Copy(Data, 0, target.Data, 0, Data.Length);
        }

        internal override void CopySubVectorToUnchecked(VectorStorage<T> target,
            int sourceIndex, int targetIndex, int count,
            bool skipClearing = false)
        {
            var denseTarget = target as DenseVectorStorage<T>;
            if (denseTarget != null)
            {
                CopySubVectorToUnchecked(denseTarget, sourceIndex, targetIndex, count);
                return;
            }

            // FALL BACK

            base.CopySubVectorToUnchecked(target, sourceIndex, targetIndex, count, skipClearing);
        }

        void CopySubVectorToUnchecked(DenseVectorStorage<T> target,
            int sourceIndex, int targetIndex, int count)
        {
            Array.Copy(Data, sourceIndex, target.Data, targetIndex, count);
        }
    }
}
