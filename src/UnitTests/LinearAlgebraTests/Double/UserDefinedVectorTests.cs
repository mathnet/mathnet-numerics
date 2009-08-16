namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System;
    using System.Collections.Generic;

    using MathNet.Numerics.LinearAlgebra.Double;

    internal class UserDefinedVector : Vector
    {
        private readonly double[] _data;

        public UserDefinedVector(int size)
            : base(size)
        {
            _data = new double[size];
        }

        public override double this[int index]
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

        public override Matrix CreateMatrix(int rows, int columns)
        {
            throw new NotImplementedException();
        }

        public override Vector CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }
    }

    public class UserDefinedVectorTests : VectorTests
    {
        protected override Vector CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        protected override Vector CreateVector(IList<double> data)
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