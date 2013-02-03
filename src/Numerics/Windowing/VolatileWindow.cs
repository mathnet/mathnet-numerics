using System;
using System.Runtime;

namespace MathNet.Numerics.Windowing
{
    /// <summary>
    /// Window which is destroyed/modified as soon as it propagates to the next window.
    /// Efficient, but needs to be consumed immediately when streaming through windows.
    /// Newest value is the first one at Offset, the next older one at Offset+1,
    /// wrapping around at Buffer.Length.
    /// </summary>
    public class VolatileWindow<T>
    {
        // Note: public fields are ok here

        public readonly int WindowSize;
        public readonly T[] Buffer;
        public int Offset;

        public VolatileWindow(int size, int bufferSize)
        {
            if (bufferSize < size || size <= 0)
            {
                throw new ArgumentOutOfRangeException("size");
            }

            WindowSize = size;
            Buffer = new T[bufferSize];
            Offset = Buffer.Length - 1;
        }

        public void Next(T value)
        {
            Buffer[Offset = (Offset == 0) ? Buffer.Length - 1 : Offset - 1] = value;
        }

        public T this[int index]
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get { return Buffer[(Offset + index) % Buffer.Length]; }
            set { Buffer[(Offset + index) % Buffer.Length] = value; }
        }

        public T[] ToArray()
        {
            var ret = new T[WindowSize];
            var excess = Offset + WindowSize - Buffer.Length;
            if (excess <= 0)
            {
                Array.Copy(Buffer, Offset, ret, 0, WindowSize);
            }
            else
            {
                Array.Copy(Buffer, Offset, ret, 0, WindowSize - excess);
                Array.Copy(Buffer, 0, ret, WindowSize - excess, excess);
            }
            return ret;
        }

        // TODO: Convolution and other map/fold/reduce like applications
    }
}