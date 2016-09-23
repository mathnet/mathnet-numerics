using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using DenseVector = MathNet.Numerics.LinearAlgebra.Double.DenseVector;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public class TestCase
    {
        public string CaseName;
        public ITestFunction Function;
        public DenseVector InitialGuess;
        public DenseVector LowerBound;
        public DenseVector UpperBound;
        public double MinimalValue;
        public DenseVector MinimizingPoint;

        public bool IsBounded
        {
            get
            {
                return this.LowerBound != null && this.UpperBound != null;
            }
        }

        public bool IsUnbounded
        {
            get
            {
                return this.IsUnboundedOverride ?? this.LowerBound == null || this.UpperBound == null;
            }
        }

        public bool? IsUnboundedOverride;

        public string FullName
        {
            get
            {
                return $"{this.Function.Description} {this.CaseName}";
            }
        }
    }

    public interface ITestFunction
    {
        string Description { get; }
        int ParameterDimension { get; }
        int ItemDimension { get; }

        double ItemValue(Vector<double> x, int itemIndex);
        Vector<double> ItemGradient(Vector<double> x, int itemIndex);
        void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output);
        Matrix<double> ItemHessian(Vector<double> x, int itemIndex);
        void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output);

        Matrix<double> Jacobian(Vector<double> x);
        void JacobianbyRef(Vector<double> x, Matrix<double> output);

        double SsqValue(Vector<double> x);
        Vector<double> SsqGradient(Vector<double> x);
        void SsqGradientByRef(Vector<double> x, Vector<double> output);
        Matrix<double> SsqHessian(Vector<double> x);
        void SsqHessianByRef(Vector<double> x, Matrix<double> output);
    }
}
