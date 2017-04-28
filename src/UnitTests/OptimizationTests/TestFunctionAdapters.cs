using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.Optimization.ObjectiveFunctions;
using MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    public class MghObjectiveFunction : LazyObjectiveFunctionBase
    {
        private ITestFunction TestFunction;

        public MghObjectiveFunction(ITestFunction testFunction, bool use_gradient, bool use_hessian)
            : base(use_gradient, use_hessian)
        {
            this.TestFunction = testFunction;
        }

        public override IObjectiveFunction CreateNew()
        {
            return new MghObjectiveFunction(this.TestFunction, this.IsGradientSupported, this.IsHessianSupported);
        }

        protected override void EvaluateValue()
        {
            this.Value = this.TestFunction.SsqValue(this.Point);
        }

        protected override void EvaluateGradient()
        {
            if (this.IsGradientSupported)
            {
                if (this.GradientValue == null)
                    this.Gradient = new DenseVector(this.TestFunction.ParameterDimension);
                this.TestFunction.SsqGradientByRef(this.Point, GradientValue);
            }
        }

        protected override void EvaluateHessian()
        {
            if (this.IsHessianSupported)
            {
                if (this.HessianValue == null)
                    this.Hessian = new DenseMatrix(this.TestFunction.ParameterDimension, this.TestFunction.ParameterDimension);
                this.TestFunction.SsqHessianByRef(this.Point, HessianValue);
            }
        }
    }
}
