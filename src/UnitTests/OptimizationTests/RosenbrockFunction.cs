using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    public static class RosenbrockFunction
    {
        public static double Value(Vector<double> input)
        {
            return Math.Pow((1 - input[0]), 2) + 100 * Math.Pow((input[1] - input[0] * input[0]), 2);
        }

        public static Vector<double> Gradient(Vector<double> input)
        {
            Vector<double> output = new DenseVector(2);
            output[0] = -2 * (1 - input[0]) + 200 * (input[1] - input[0] * input[0]) * (-2 * input[0]);
            output[1] = 2 * 100 * (input[1] - input[0] * input[0]);
            return output;
        }

        public static Matrix<double> Hessian(Vector<double> input)
        {
            Matrix<double> output = new DenseMatrix(2, 2);
            output[0, 0] = 2 - 400 * input[1] + 1200 * input[0] * input[0];
            output[1, 1] = 200;
            output[0, 1] = -400 * input[0];
            output[1, 0] = output[0, 1];
            return output;
        }

        public static Vector<double> Minimum
        {
            get
            {
                return new DenseVector(new double[] { 1, 1 });
            }
        }
    }

    public static class BigRosenbrockFunction
    {
        public static double Value(Vector<double> input)
        {
            return 1000.0 + 100.0 * RosenbrockFunction.Value(input / 100.0);
        }

        public static Vector<double> Gradient(Vector<double> input)
        {
            return 100.0 * RosenbrockFunction.Gradient(input / 100.0);
        }

        public static Matrix<double> Hessian(Vector<double> input)
        {
            return 100.0 * RosenbrockFunction.Hessian(input / 100.0);
        }

        public static Vector<double> Minimum
        {
            get
            {
                return new DenseVector(new double[] { 100, 100 });
            }
        }
    }
}
