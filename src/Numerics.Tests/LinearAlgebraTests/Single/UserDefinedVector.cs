// <copyright file="UserDefinedVector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using MathNet.Numerics.LinearAlgebra.Single;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    /// <summary>
    /// User-defined vector implementation (internal class for testing purposes)
    /// </summary>
    internal class UserDefinedVector : Vector
    {
        class UserDefinedVectorStorage : VectorStorage<float>
        {
            public readonly float[] Data;

            public UserDefinedVectorStorage(int size)
                : base(size)
            {
                Data = new float[size];
            }

            public UserDefinedVectorStorage(int size, float[] data)
                : base(size)
            {
                Data = data;
            }

            public override bool IsDense
            {
                get { return true; }
            }

            public override float At(int index)
            {
                return Data[index];
            }

            public override void At(int index, float value)
            {
                Data[index] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedVector"/> class with a given size.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        public UserDefinedVector(int size)
            : base(new UserDefinedVectorStorage(size))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDefinedVector"/> class for an array.
        /// </summary>
        /// <param name="data">The array to create this vector from.</param>
        public UserDefinedVector(float[] data)
            : base(new UserDefinedVectorStorage(data.Length, (float[])data.Clone()))
        {
        }
    }
}
