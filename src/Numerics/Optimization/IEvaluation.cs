using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    [Flags]
    public enum EvaluationStatus { None = 0, Value = 1, Gradient = 2, Hessian = 4 }

    public interface IEvaluation
    {
        Vector<double> Point { get; set; }
        IEvaluation CreateNew();

        // Used by algorithm
        bool GradientSupported { get; }
        bool HessianSupported { get; }
        double Value { get; }
        Vector<double> Gradient { get; }
        Matrix<double> Hessian { get; }
    }
}
