// <copyright file="UserDefinedVectorTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
    using System;
    using System.Numerics;
    using System.Collections.Generic;
    using Distributions;
    using LinearAlgebra.Complex;
    using LinearAlgebra.Generic;
    using Threading;

    internal class UserDefinedVector : Vector
    {
        private readonly Complex[] _data;

        public UserDefinedVector(int size)
            : base(size)
        {
            _data = new Complex[size];
        }

        public UserDefinedVector(Complex[] data)
            : base(data.Length)
        {
            _data = (Complex[])data.Clone();
        }

        public override Matrix<Complex> CreateMatrix(int rows, int columns)
        {
            return new UserDefinedMatrix(rows, columns);
        }

        public override Vector<Complex> CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        protected internal override Complex At(int index)
        {
            return _data[index];
        }

        protected internal override void At(int index, Complex value)
        {
            _data[index] = value;
        }
       }

    public class UserDefinedVectorTests : VectorTests
    {
        protected override Vector<Complex> CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        protected override Vector<Complex> CreateVector(IList<Complex> data)
        {
            var vector = new UserDefinedVector(data.Count);
            for (var index = 0; index < data.Count; index++)
            {
                vector[index] = data[index];
            }

            return vector;
        }
    }
}