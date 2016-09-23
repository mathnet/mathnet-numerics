using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public class PowellBadlyScaledFunction : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new PowellBadlyScaledFunction(),
                    InitialGuess = new double[] { 0, 1 },
                    MinimizingPoint = new double[] { 1.098e-5, 9.106 },
                    MinimalValue = 0,
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new PowellBadlyScaledFunction(),
                    InitialGuess = new double[] { 0, 1 },
                    MinimizingPoint = new double[] { 1.098e-5, 9.106 },
                    MinimalValue = 0,
                    LowerBound = new double[] { -1000, -1000 },
                    UpperBound = new double[] { 1000, 1000 },
                    CaseName = "loose bounds"
                };
                yield return new TestCase()
                {
                    Function = new PowellBadlyScaledFunction(),
                    LowerBound = new double[] { 0, 1 },
                    UpperBound = new double[] { 1, 9 },
                    InitialGuess = new double[] { 0, 1 },
                    MinimalValue = 0.15125900e-9,
                    CaseName = "tight bounds"
                };
            }
        }

        public override string Description
        {
            get
            {
                return "Powell badly scaled fun (MGH #3)";
            }
        }

        public override int ItemDimension
        {
            get
            {
                return 2;
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
            if (itemIndex == 0)
            {
                output[0] = 10000 * x[1];
                output[1] = 10000 * x[0];
            }
            else if (itemIndex == 1)
            {
                output[0] = -Math.Exp(-x[0]);
                output[1] = -Math.Exp(-x[1]);
            }
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            if (itemIndex == 0)
            {
                output[0, 0] = 0;
                output[0, 1] = 10000;
                output[1, 0] = 10000;
                output[1, 1] = 0;
            }
            else
            {
                output[0, 0] = Math.Exp(-x[0]);
                output[0, 1] = 0;
                output[1, 0] = 0;
                output[1,1] = Math.Exp(-x[1]);
            }
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            if (itemIndex == 0)
                return 10000.0 * x[0] * x[1] - 1;
            else
                return Math.Exp(-x[0]) + Math.Exp(-x[1]) - 1.0001;
        }
    }
}
