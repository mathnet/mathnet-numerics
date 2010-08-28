// <copyright file="UserDefinedMatrixTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System;
    using System.Text;
    using Distributions;
    using LinearAlgebra.Generic;
    using Properties;
    using Threading;

    internal class UserDefinedMatrix : Matrix<double>
    {
        private readonly double[,] _data;

        public UserDefinedMatrix(int order): base(order, order)
        {
            _data = new double[order, order];
        }

        public UserDefinedMatrix(int rows, int columns) : base(rows, columns)
        {
            _data = new double[rows, columns];
        }

        public UserDefinedMatrix(double[,] data) : base(data.GetLength(0), data.GetLength(1))
        {
            _data = (double[,])data.Clone();
        }

        public override double At(int row, int column)
        {
            return _data[row, column];
        }

        public override void At(int row, int column, double value)
        {
            _data[row, column] = value;
        }

        public override Matrix<double> CreateMatrix(int numberOfRows, int numberOfColumns)
        {
            return new UserDefinedMatrix(numberOfRows, numberOfColumns);
        }

        public override Vector<double> CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        public static UserDefinedMatrix Identity(int order)
        {
            var m = new UserDefinedMatrix(order, order);
            for (var i = 0; i < order; i++)
            {
                m[i, i] = 1.0;
            }

            return m;
        }

        public override void Negate()
        {
            Multiply(-1);
        }

        public override Matrix<double> Random(int numberOfRows, int numberOfColumns, IContinuousDistribution distribution)
        {
            if (numberOfRows < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfRows");
            }

            if (numberOfColumns < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfColumns");
            }

            var matrix = CreateMatrix(numberOfRows, numberOfColumns);
            CommonParallel.For(
                0,
                ColumnCount,
                j =>
                {
                    for (var i = 0; i < matrix.RowCount; i++)
                    {
                        matrix[i, j] = distribution.Sample();
                    }
                });

            return matrix;
        }

        public override Matrix<double> Random(int numberOfRows, int numberOfColumns, IDiscreteDistribution distribution)
        {
            if (numberOfRows < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfRows");
            }

            if (numberOfColumns < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "numberOfColumns");
            }

            var matrix = CreateMatrix(numberOfRows, numberOfColumns);
            CommonParallel.For(
                0,
                ColumnCount,
                j =>
                {
                    for (var i = 0; i < matrix.RowCount; i++)
                    {
                        matrix[i, j] = distribution.Sample();
                    }
                });

            return matrix;
        }

        protected sealed override double AddT(double val1, double val2)
        {
            return val1 + val2;
        }

        protected sealed override double SubtractT(double val1, double val2)
        {
            return val1 - val2;
        }

        protected sealed override double MultiplyT(double val1, double val2)
        {
            return val1 * val2;
        }

        protected sealed override double DivideT(double val1, double val2)
        {
            return val1 / val2;
        }

        protected sealed override bool IsOneT(double val1)
        {
            return 1.0.AlmostEqualInDecimalPlaces(val1, 15);
        }

        protected sealed override double AbsoluteT(double val1)
        {
            return Math.Abs(val1);
        }
    }

    public class UserDefinedMatrixTests : MatrixTests
    {
        protected override Matrix<double> CreateMatrix(int rows, int columns)
        {
            return new UserDefinedMatrix(rows, columns);
        }

        protected override Matrix<double> CreateMatrix(double[,] data)
        {
            return new UserDefinedMatrix(data);
        }

        protected override Vector<double> CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        protected override Vector<double> CreateVector(double[] data)
        {
            return new UserDefinedVector(data);
        }
    }
}
