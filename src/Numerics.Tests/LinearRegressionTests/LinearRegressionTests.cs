using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.LinearRegressionTests
{
    [TestFixture, Category("Linear Regression Tests")]
    public class LinearRegressionTests
    {
        [Test]
        public void LinearRegressionsTest()
        {
            /*
                x1 = 3;
                x2 = 8;
                x3 = 39;

                Test solving of system of linear equations:

                x1 + x2 + x3 = 50
                x1 - x2 + x3 = 34
                x1 + x2 - x3 = -23

            */

            Matrix<double> input = DenseMatrix.OfArray(new double[,] {
                {1,1,1},
                {1,-1,1},
                {1,1,-1}});

            var y = Vector<double>.Build.DenseOfArray(new double[] { 50, 34, -23 });
            var weights = LinearRegression.MultipleRegression.DirectMethod(input, y);
            var diff = new double[3];
            diff[0] = 5.499999;
            diff[1] = 8.000000;
            diff[2] = 36.50000;
            Assert.AreEqual(diff[0] - weights[0] < 0.0001, true);
            Assert.AreEqual(diff[1] - weights[1] < 0.0001, true);
            Assert.AreEqual(diff[2] - weights[2] < 0.0001, true);
        }

        private static double[][] GetInputsOutputsAll(out double[] outputs)
        {
            var fileName = Path.GetDirectoryName(typeof(LinearRegressionTests).GetTypeInfo().Assembly.Location) + @"\LinearRegressionTests\bikes_rent.csv";
            var data = File.ReadAllLines(fileName);
            var dataStrings = new List<string>(data.Skip(1));

            var dataFragmentedStrigns = dataStrings.Select(a => a.Split(',')).ToList();
            double[][] inputs = new double[dataFragmentedStrigns.Count][];
            outputs = new double[dataFragmentedStrigns.Count];

            for (int i = 0; i < dataFragmentedStrigns.Count(); i++)
            {
                inputs[i] = new double[dataFragmentedStrigns[0].Length - 1];
            }

            for (int i = 0; i < dataFragmentedStrigns.Count; i++)
            {
                int j;
                for (j = 0; j < dataFragmentedStrigns[0].Length - 1; j++)
                {
                    inputs[i][j] = double.Parse(dataFragmentedStrigns[i][j]);
                }

                outputs[i] = double.Parse(dataFragmentedStrigns[i][j]);
            }

            return inputs;
        }

        [Test]
        public void LinearRegressionsTestRidge()
        {

            var inputs = GetInputsOutputsAll(out var outputs);
            var result0 = LinearRegression.MultipleRegression.RidgeRegression(inputs, outputs, 0);
            var result1 = LinearRegression.MultipleRegression.RidgeRegression(inputs, outputs);
            var result2 = LinearRegression.MultipleRegression.RidgeRegression(inputs, outputs, 10);

            Assert.AreEqual(result0.Length, 13);
            Assert.AreEqual(result1.Length, 13);
            Assert.AreEqual(result2.Length, 13);

            var avg0 = result0.Select(a => Math.Abs(a)).Average();
            var avg1 = result1.Select(a => Math.Abs(a)).Average();
            var avg2 = result2.Select(a => Math.Abs(a)).Average();

            Assert.Greater(avg1, avg2); // The greater is the value of lambda, the smaller weights should be
            Assert.Greater(avg0, avg1); // The greater is the value of lambda, the smaller weights should be

        }

    }
}
