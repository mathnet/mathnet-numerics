using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public class RosenbrockFunction2 : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { -1.2, 1 },
                    MinimizingPoint = new double[] { 1, 1 },
                    MinimalValue = 0,
                    LowerBound = new double[] { -1000, -1000 },
                    UpperBound = new double[] { 1000, 1000 },
                    CaseName = "hard start",
                    IsUnboundedOverride = true
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { 1.2, 1.2 },
                    MinimizingPoint = new double[] { 1, 1 },
                    MinimalValue = 0,
                    LowerBound = new double[] { -5, -5 },
                    UpperBound = new double[] { 5, 5 },
                    CaseName = "easy start"
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { -0.9, -0.5 },
                    MinimizingPoint = new double[] { 1, 1 },
                    MinimalValue = 0,
                    LowerBound = new double[] { -5, -5 },
                    UpperBound = new double[] { 5, 5 },
                    CaseName = "Overton start",
                    IsUnboundedOverride = true
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { 1.2, 1.2 },
                    MinimizingPoint = new double[] { 1, 1 },
                    MinimalValue = 0,
                    LowerBound = new double[] { 1, -5 },
                    UpperBound = new double[] { 5, 5 },
                    CaseName = "easy one active bound"
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { 1.2, 1.2 },
                    MinimizingPoint = new double[] { 1, 1 },
                    MinimalValue = 0,
                    LowerBound = new double[] { 1, 1 },
                    UpperBound = new double[] { 5, 5 },
                    CaseName = "easy two active bounds"
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { 2.5, 2.5 },
                    MinimizingPoint = new double[] { 2, 4 },
                    MinimalValue = 1,
                    LowerBound = new double[] { 2, 2 },
                    UpperBound = new double[] { 5, 5 },
                    CaseName = "min on lower bound, not local"
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { -0.9, -0.5 },
                    MinimizingPoint = new double[] { 0.5, 0.25 },
                    MinimalValue = 0.25,
                    LowerBound = new double[] { -2, -2 },
                    UpperBound = new double[] { 0.5, 0.5 },
                    CaseName = "min on upper bound, not local"
                };


            }
        }
        public RosenbrockFunction2() { }

        public override string Description
        {
            get
            {
                return "Rosenbrock fun (MGH #1)";
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
                output[0] = -20 * x[0];
                output[1] = 10;
            } else
            {
                output[0] = -1;
                output[1] = 0;
            }
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            if (itemIndex == 0)
            {
                output[0, 0] = -20;
                output[0, 1] = 0;
                output[1, 0] = 0;
                output[1, 1] = 0;
            } else
            {
                output[0, 0] = 0;
                output[0, 1] = 0;
                output[1, 0] = 0;
                output[1, 1] = 0;
            }
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            if (itemIndex == 0)
                return 10 * (x[1] - x[0] * x[0]);
            else
                return 1 - x[0];
        }
    }
}
