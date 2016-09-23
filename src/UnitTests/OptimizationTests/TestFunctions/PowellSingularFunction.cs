using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public class PowellSingularFunction : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new PowellSingularFunction(),
                    InitialGuess = new double[] { 3, -1, 0, 1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] {0,0,0,0},
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new PowellSingularFunction(),
                    InitialGuess = new double[] { 3, -1, 0, 1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 0, 0, 0, 0 },
                    LowerBound = new double[] {-1000, -1000, -1000, -1000},
                    UpperBound = new double[] { 1000, 1000, 1000, 1000 },
                    CaseName = "loose bounds"
                };
            }
        }


        public PowellSingularFunction() { }

        public override string Description
        {
            get
            {
                return "Powell singular fun (MGH #13)";
            }
        }

        public override int ItemDimension
        {
            get
            {
                return 4;
            }
        }

        public override int ParameterDimension
        {
            get
            {
                return 4;
            }
        }

        public override void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output)
        {
            switch (itemIndex)
            {
                case 0:
                    output[0] = 1;
                    output[1] = 10;
                    output[2] = 0;
                    output[3] = 0;
                    break;
                case 1:
                    output[0] = 0;
                    output[1] = 0;
                    output[2] = Math.Sqrt(5);
                    output[3] = -Math.Sqrt(5);
                    break;
                case 2:
                    output[0] = 0;
                    output[1] = 2*(x[1]-2*x[2]);
                    output[2] = -4*x[1] + 8*x[2];
                    output[3] = 0;
                    break;
                case 3:
                    output[0] = 2*Math.Sqrt(10)*(x[0] - x[3]);
                    output[1] = 0;
                    output[2] = 0;
                    output[3] = -2*Math.Sqrt(10)*(x[0] - x[3]);
                    break;
                default:
                    throw new ArgumentException("itemIndex must be <= 3");
            }
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            for (int ii = 0; ii < 4; ++ii)
                for (int jj = 0; jj < 4; ++jj)
                    output[ii, jj] = 0;
            switch(itemIndex)
            {
                case 0:
                case 1:
                    break;
                case 2:
                    output[1, 1] = 2;
                    output[1, 2] = -4;
                    output[2, 1] = -4;
                    output[2, 2] = 8;
                    break;
                case 3:
                    output[0, 0] = 2 * Math.Sqrt(10);
                    output[0, 3] = -2 * Math.Sqrt(10);
                    output[3, 0] = -2 * Math.Sqrt(10);
                    output[3, 3] = 2 * Math.Sqrt(10);
                    break;
                default:
                    throw new ArgumentException("itemIndex must be <= 3");
            }
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            switch (itemIndex)
            {
                case 0:
                    return x[0] + 10 * x[1];
                case 1:
                    return Math.Sqrt(5) * (x[2] - x[3]);
                case 2:
                    return Math.Pow(x[1] - 2 * x[2], 2);
                case 3:
                    return Math.Sqrt(10.0) * Math.Pow(x[0] - x[3], 2);
                default:
                    throw new ArgumentException("itemIndex must be <= 3");
            }
        }
    }
}
