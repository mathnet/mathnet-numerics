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
    using LinearAlgebra.Generic;
    using Threading;

    internal class UserDefinedVector : Vector<Complex>
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

        public override Complex this[int index]
        {
            get
            {
                return _data[index];
            }

            set
            {
                _data[index] = value;
            }
        }

        public override Matrix<Complex> CreateMatrix(int rows, int columns)
        {
            return new UserDefinedMatrix(rows, columns);
        }

        public override Vector<Complex> CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        public override Vector<Complex> Negate()
        {
            var result = new UserDefinedVector(Count);
            CommonParallel.For(
                0,
                _data.Length,
                index => result[index] = -_data[index]);

            return result;
        }

        public override Vector<Complex> Random(int length, IContinuousDistribution randomDistribution)
        {
            if (length < 1)
            {
                throw new ArgumentException("length");
            }

            var v = (UserDefinedVector)CreateVector(length);
            for (var index = 0; index < v._data.Length; index++)
            {
                v._data[index] = new Complex(randomDistribution.Sample(), randomDistribution.Sample());
            }

            return v;
        }

        public override Vector<Complex> Random(int length, IDiscreteDistribution randomDistribution)
        {
            if (length < 1)
            {
                throw new ArgumentException("length");
            }

            var v = (UserDefinedVector)CreateVector(length);
            for (var index = 0; index < v._data.Length; index++)
            {
                v._data[index] = new Complex(randomDistribution.Sample(), randomDistribution.Sample());
            }

            return v;
        }

        public override int MinimumIndex()
        {
            throw new NotSupportedException();
        }

        public override int MaximumIndex()
        {
            throw new NotSupportedException();
        }

        public override Vector<Complex> Normalize(double p)
        {
            if (p < 0.0)
            {
                throw new ArgumentOutOfRangeException("p");
            }

            var norm = Norm(p);
            var clone = Clone();
            if (norm == 0.0)
            {
                return clone;
            }

            clone.Multiply(1.0 / norm, clone);

            return clone;
        }

        protected sealed override Complex AddT(Complex val1, Complex val2)
        {
            return val1 + val2;
        }

        protected sealed override Complex SubtractT(Complex val1, Complex val2)
        {
            return val1 - val2;
        }

        protected sealed override Complex MultiplyT(Complex val1, Complex val2)
        {
            return val1 * val2;
        }

        protected sealed override Complex DivideT(Complex val1, Complex val2)
        {
            return val1 / val2;
        }

        protected sealed override bool IsOneT(Complex val1)
        {
            return Complex.One.AlmostEqual(val1);
        }

        protected sealed override double AbsoluteT(Complex val1)
        {
            return val1.Magnitude;
        }

        public override void Conjugate(Vector<Complex> target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (Count != target.Count)
            {
                throw new ArgumentException("target");
            }

            if (ReferenceEquals(this, target))
            {
                var tmp = CreateVector(Count);
                Conjugate(tmp);
                tmp.CopyTo(target);
            }

            CommonParallel.For(
                0,
                Count,
                index => target[index] = this[index].Conjugate());
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