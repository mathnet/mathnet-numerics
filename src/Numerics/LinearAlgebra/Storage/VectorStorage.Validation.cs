using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    // ReSharper disable UnusedParameter.Global
    public partial class VectorStorage<T>
    {
        protected void ValidateRange(int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        protected void ValidateSubVectorRange(VectorStorage<T> target,
            int sourceIndex, int targetIndex, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException("count", Resources.ArgumentMustBePositive);
            }

            // Verify Source

            if (sourceIndex >= Length || sourceIndex < 0)
            {
                throw new ArgumentOutOfRangeException("sourceIndex");
            }

            var sourceMax = sourceIndex + count;

            if (sourceMax > Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // Verify Target

            if (targetIndex >= target.Length || targetIndex < 0)
            {
                throw new ArgumentOutOfRangeException("targetIndex");
            }

            var targetMax = targetIndex + count;

            if (targetMax > target.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
        }
    }
    // ReSharper restore UnusedParameter.Global
}
