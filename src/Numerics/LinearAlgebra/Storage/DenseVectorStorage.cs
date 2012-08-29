using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
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

        /// <remarks>Parameters assumed to be validated already.</remarks>
        public override void CopyTo(VectorStorage<T> target, bool skipClearing = false)
        {
            var denseTarget = target as DenseVectorStorage<T>;
            if (denseTarget != null)
            {
                CopyTo(denseTarget);
                return;
            }

            // FALL BACK

            for (int i = 0; i < Length; i++)
            {
                target.At(i, Data[i]);
            }
        }

        void CopyTo(DenseVectorStorage<T> target)
        {
            if (ReferenceEquals(this, target))
            {
                return;
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (Length != target.Length)
            {
                var message = string.Format(Resources.ArgumentMatrixDimensions2, Length, target.Length);
                throw new ArgumentException(message, "target");
            }

            //Buffer.BlockCopy(Data, 0, target.Data, 0, Data.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
            Array.Copy(Data, 0, target.Data, 0, Data.Length);
        }

        public override void CopySubVectorTo(VectorStorage<T> target,
            int sourceIndex, int targetIndex, int count,
            bool skipClearing = false)
        {
            var denseTarget = target as DenseVectorStorage<T>;
            if (denseTarget != null)
            {
                CopySubVectorTo(denseTarget, sourceIndex, targetIndex, count);
                return;
            }

            // FALL BACK

            base.CopySubVectorTo(target, sourceIndex, targetIndex, count, skipClearing);
        }

        void CopySubVectorTo(DenseVectorStorage<T> target,
            int sourceIndex, int targetIndex, int count)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (ReferenceEquals(this, target))
            {
                throw new NotSupportedException();
            }

            ValidateSubVectorRange(target, sourceIndex, targetIndex, count);

            //Buffer.BlockCopy(Data, sourceIndex, target.Data, targetIndex, count * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
            Array.Copy(Data, sourceIndex, target.Data, targetIndex, count);
        }
    }
}
