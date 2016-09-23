using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public class BrownBadlyScaledFunction : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new BrownBadlyScaledFunction(),
                    InitialGuess = new double[] { 1, 1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 1e6, 2e-6 },
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new BrownBadlyScaledFunction(),
                    InitialGuess = new double[] { 1, 1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 1e6, 2e-6 },
                    LowerBound = new double[] { -1e8, -1e8 },
                    UpperBound = new double[] { 1e8, 1e8 },
                    CaseName = "loose bounds"
                };
                yield return new TestCase()
                {
                    Function = new BrownBadlyScaledFunction(),
                    InitialGuess = new double[] { 1, 1 },
                    MinimalValue = 0.784e3,
                    MinimizingPoint = new double[] { 1e6, 2e-6 },
                    LowerBound = new double[] { 0, 3e-5 },
                    UpperBound = new double[] { 1e6, 100 },
                    CaseName = "tight bounds"                 
                };
            }
        }

        public BrownBadlyScaledFunction() { }

        public override string Description
        {
            get
            {
                return "Brown badly scaled fun (MGH #4)";
            }
        }

        public override int ItemDimension
        {
            get
            {
                return 3;
            }
        }

        public override int ParameterDimension
        {
            get
            {
                return 2;
            }
        }

        public override void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output)
        {
            switch (itemIndex)
            {
                case 0:
                    output[0] = 1;
                    output[1] = 0;
                    break;
                case 1:
                    output[0] = 0;
                    output[1] = 1;
                    break;
                case 2:
                    output[0] = x[1];
                    output[1] = x[0];
                    break;
                default:
                    throw new ArgumentException("itemIndex must be <= 2");
            }
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            switch (itemIndex)
            {
                case 0:
                case 1:
                    output[0, 0] = 0;
                    output[0, 1] = 0;
                    output[1, 0] = 0;
                    output[1, 1] = 0;
                    break;
                case 2:
                    output[0, 0] = 0;
                    output[0, 1] = 1;
                    output[1, 0] = 1;
                    output[1, 1] = 0;
                    break;
                default:
                    throw new ArgumentException("itemIndex must be <= 2");
            }
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            switch (itemIndex)
            {
                case 0:
                    return x[0] - 1e6;
                case 1:
                    return x[1] - 2e-6;
                case 2:
                    return x[0] * x[1] - 2;
                default:
                    throw new ArgumentException("itemIndex must be <= 2");
            }
        }
    }
}
