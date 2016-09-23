using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public class BealeFunction : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new BealeFunction(),
                    InitialGuess = new double[] { 1, 1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 3, 0.5 }, 
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new BealeFunction(),
                    InitialGuess = new double[] { 1, 1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 3, 0.5 },
                    LowerBound = new double[] { -1000, -1000},
                    UpperBound = new double[] { 1000, 1000},
                    CaseName = "loose bounds"
                };
                yield return new TestCase()
                {
                    Function = new BealeFunction(),
                    InitialGuess = new double[] { 1, 1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 3, 0.5 },
                    LowerBound = new double[] { 0.6, 0.5 },
                    UpperBound = new double[] { 10, 100 },
                    CaseName = "tight bounds"
                };
            }
        }

        public BealeFunction() { }

        public override string Description
        {
            get
            {
                return "Beale fun (MGH #5)";
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
            int ii = itemIndex + 1;
            output[0] = -1 + Math.Pow(x[1], ii);
            output[1] = ii * x[0] * Math.Pow(x[1], ii - 1);
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            int ii = itemIndex + 1;
            output[0, 0] = 0;
            output[0, 1] = ii * Math.Pow(x[1], ii - 1);
            output[1, 0] = ii * Math.Pow(x[1], ii - 1);
            output[1, 1] = (ii - 1) * ii * x[0] * Math.Pow(x[1], ii - 2);
        }

        private static readonly double[] y = { 1.5, 2.25, 2.625};

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            int ii = itemIndex + 1;
            return y[itemIndex] - x[0] * (1 - Math.Pow(x[1], ii));
        }
    }
}
