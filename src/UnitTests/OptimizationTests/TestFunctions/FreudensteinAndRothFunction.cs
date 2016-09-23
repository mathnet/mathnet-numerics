using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using DenseVector = MathNet.Numerics.LinearAlgebra.Double.DenseVector;
using DenseMatrix = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public class FreudensteinAndRothFunction : BaseTestFunction
    {

        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new FreudensteinAndRothFunction(),
                    InitialGuess = new double[] { 0.5, -2 },
                    MinimizingPoint = new double[] { 5, 4 },
                    MinimalValue = 0,
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new FreudensteinAndRothFunction(),
                    InitialGuess = new double[] { 0.5, -2 },
                    MinimizingPoint = new double[] {5, 4},
                    MinimalValue = 0,
                    LowerBound = new double[] { -1000, -1000 },
                    UpperBound = new double[] { 1000, 1000},
                    CaseName = "loose bounds"
                };
            }
        }

        public override string Description {  get { return "Freudenstein & Roth fun (MGH #2)"; } }

        public override int ParameterDimension
        {
            get
            {
                return 2;
            }
        }

        public override int ItemDimension
        {
            get
            {
                return 2;
            }
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            if (itemIndex == 0)
                return -13 + x[0] + ((5 - x[1]) * x[1] - 2) * x[1];
            else 
                return -29 + x[0] + ((x[1] + 1) * x[1] - 14) * x[1];
        }

        public override void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output)
        {
            if (itemIndex == 0)
            {
                output[0] = 1;
                output[1] = -2 + (5 - 2 * x[1]) * x[1] + (5 - x[1]) * x[1];
            }
            else
            {
                output[0] = 1;
                output[1] = -14 + x[1] * (1 + x[1]) + x[1] * (1 + 2 * x[1]);
            }
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            if (itemIndex == 0)
            {
                output[0, 0] = 0;
                output[0, 1] = 0;
                output[1, 0] = 0;
                output[1, 1] = 10 - 6 * x[1];
            }
            else
            {
                output[0, 0] = 0;
                output[0, 1] = 0;
                output[1, 0] = 0;
                output[1, 1] = 2 + 6 * x[1];
            }
        }
    }
}
