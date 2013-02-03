using System;
using System.Runtime;

namespace MathNet.Numerics.Windowing
{
    /// <summary>
    /// Window which is remains intact even if it propagates to the next window.
    /// Still much mor efficient than creating a new buffer for every window,
    /// but somewhat less efficient than the volatile window.
    /// Newest value is the first one at Offset in BufferA, the next older one at Offset+1,
    /// wrapping around at BufferA.Length to BufferB.
    /// </summary>
    public class PermanentWindow<T>
    {
        // Note: public fields are ok here

        public readonly int WindowSize;
        public readonly T[] BufferA;
        public readonly T[] BufferB;
        public readonly int OffsetA;

        public PermanentWindow(int size, int bufferSize)
        {
            if (bufferSize < size || size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            WindowSize = size;
            BufferA = new T[bufferSize];
            BufferB = new T[bufferSize];
            OffsetA = BufferA.Length - 1;
        }

        private PermanentWindow(int windowSize, T[] bufferA, T[] bufferB, int offsetA)
        {
            WindowSize = windowSize;
            BufferA = bufferA;
            BufferB = bufferB;
            OffsetA = offsetA;
        }

        public PermanentWindow<T> Next(T value)
        {
            if (OffsetA == 0)
            {
                var newBufferA = new T[BufferB.Length];
                var newOffsetA = newBufferA.Length - 1;
                newBufferA[newOffsetA] = value;
                return new PermanentWindow<T>(WindowSize, newBufferA, BufferA, newOffsetA);
            }
            else
            {
                var newOffsetA = OffsetA - 1;
                BufferA[newOffsetA] = value;
                return new PermanentWindow<T>(WindowSize, BufferA, BufferB, newOffsetA);
            }
        }

        public T this[int index]
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                var pos = OffsetA + index;
                return pos <= BufferA.Length ? BufferA[pos] : BufferB[pos - BufferA.Length];
            }
            set
            {
                var pos = OffsetA + index;
                if (pos <= BufferA.Length)
                {
                    BufferA[pos] = value;
                }
                else
                {
                    BufferB[pos - BufferA.Length] = value;
                }
            }
        }

        public T[] ToArray()
        {
            var ret = new T[WindowSize];
            var excess = OffsetA + WindowSize - BufferA.Length;
            if (excess <= 0)
            {
                Array.Copy(BufferA, OffsetA, ret, 0, WindowSize);
            }
            else
            {
                Array.Copy(BufferA, OffsetA, ret, 0, WindowSize - excess);
                Array.Copy(BufferB, 0, ret, WindowSize - excess, excess);
            }
            return ret;
        }

        // TODO: Convolution and other map/fold/reduce like applications
    }
}
