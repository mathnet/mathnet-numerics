using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using LinearAlgebra.Double;
    using MbUnit.Framework;

    public abstract class MatrixLoader
    {
        protected Dictionary<string, double[,]> testData2D;
        protected Dictionary<string, Matrix> testMatrices;

        protected abstract Matrix CreateMatrix(int rows, int columns);
        protected abstract Matrix CreateMatrix(double[,] data);
        protected abstract Vector CreateVector(int size);
        protected abstract Vector CreateVector(double[] data);

        [SetUp]
        public void SetupMatrices()
        {
            testData2D = new Dictionary<string, double[,]>();
            testData2D.Add("Singular3x3", new double[,] { { 1, 1, 2 }, { 1, 1, 2 }, { 1, 1, 2 } });
            testData2D.Add("Square3x3", new double[,] { { -1.1, -2.2, -3.3 }, { 0, 1.1, 2.2 }, { -4.4, 5.5, 6.6 } });
            testData2D.Add("Square4x4", new double[,] { { -1.1, -2.2, -3.3, -4.4 }, { 0, 1.1, 2.2, 3.3 }, { 1.0, 2.1, 6.2, 4.3 }, { -4.4, 5.5, 6.6, -7.7 } });
            testData2D.Add("Singular4x4", new double[,] { { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 }, { -1.1, -2.2, -3.3, -4.4 } });
            testData2D.Add("Tall3x2", new double[,] { { -1.1, -2.2 }, { 0, 1.1 }, { -4.4, 5.5 } });
            testData2D.Add("Wide2x3", new double[,] { { -1.1, -2.2, -3.3 }, { 0, 1.1, 2.2 } });

            testMatrices = new Dictionary<string, Matrix>();
            foreach (var name in testData2D.Keys)
            {
                testMatrices.Add(name, CreateMatrix(testData2D[name]));
            }
        }

    }
}
