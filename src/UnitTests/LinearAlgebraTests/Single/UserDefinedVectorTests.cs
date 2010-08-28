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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Distributions;
    using LinearAlgebra.Generic;
    using Properties;
    using Threading;

    internal class UserDefinedVector : Vector<float>
    {
        private readonly float[] _data;

        public UserDefinedVector(int size)
            : base(size)
        {
            _data = new float[size];
        }

        public UserDefinedVector(float[] data)
            : base(data.Length)
        {
            _data = (float[])data.Clone();
        }

        public override float this[int index]
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

        public override Matrix<float> CreateMatrix(int rows, int columns)
        {
            return new UserDefinedMatrix(rows, columns);
        }

        public override Vector<float> CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        public override Vector<float> Negate()
        {
            var result = new UserDefinedVector(Count);
            CommonParallel.For(
                0,
                _data.Length,
                index => result[index] = -_data[index]);

            return result;
        }

        public override Vector<float> Random(int length, IContinuousDistribution randomDistribution)
        {
            if (length < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "length");
            }

            var v = (UserDefinedVector)CreateVector(length);
            for (var index = 0; index < v._data.Length; index++)
            {
                v._data[index] = (float)randomDistribution.Sample();
            }

            return v;
        }

        public override Vector<float> Random(int length, IDiscreteDistribution randomDistribution)
        {
            if (length < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "length");
            }

            var v = (UserDefinedVector)CreateVector(length);
            for (var index = 0; index < v._data.Length; index++)
            {
                v._data[index] = randomDistribution.Sample();
            }

            return v;
        }

        public override int MinimumIndex()
        {
            var index = 0;
            var min = _data[0];
            for (var i = 1; i < Count; i++)
            {
                if (min > _data[i])
                {
                    index = i;
                    min = _data[i];
                }
            }

            return index;
        }

        public override int MaximumIndex()
        {
            var index = 0;
            var max = _data[0];
            for (var i = 1; i < Count; i++)
            {
                if (max < _data[i])
                {
                    index = i;
                    max = _data[i];
                }
            }

            return index;
        }

        public override double Norm(double p)
        {
            if (p < 0.0)
            {
                throw new ArgumentOutOfRangeException("p");
            }

            if (1.0 == p)
            {
                return CommonParallel.Aggregate(
                    0,
                    Count,
                    index => Math.Abs(this[index]));
            }

            if (2.0 == p)
            {
                return _data.Aggregate(0.0f, SpecialFunctions.Hypotenuse);
            }

            if (Double.IsPositiveInfinity(p))
            {
                return CommonParallel.Select(
                    0,
                    Count,
                    (index, localData) => Math.Max(localData, Math.Abs(_data[index])),
                    Math.Max);
            }

            var sum = CommonParallel.Aggregate(
                0,
                Count,
                index => Math.Pow(Math.Abs(_data[index]), p));

            return Math.Pow(sum, 1.0 / p);
        }

        public override Vector<float> Normalize(double p)
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

            clone.Multiply(1.0f / (float)norm, clone);

            return clone;
        }
        
        protected sealed override float AddT(float val1, float val2)
        {
            return val1 + val2;
        }

        protected sealed override float SubtractT(float val1, float val2)
        {
            return val1 - val2;
        }

        protected sealed override float MultiplyT(float val1, float val2)
        {
            return val1 * val2;
        }

        protected sealed override float DivideT(float val1, float val2)
        {
            return val1 / val2;
        }

        protected sealed override double AbsoluteT(float val1)
        {
            return Math.Abs(val1);
        }
    }

    public class UserDefinedVectorTests : VectorTests
    {
        protected override Vector<float> CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        protected override Vector<float> CreateVector(IList<float> data)
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