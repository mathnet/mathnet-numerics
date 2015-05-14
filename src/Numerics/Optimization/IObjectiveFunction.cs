using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    [Flags]
    public enum EvaluationStatus { None = 0, Value = 1, Gradient = 2, Hessian = 4 }

    public interface IObjectiveFunction
    {
        Vector<double> Point { get; set; }
        IObjectiveFunction CreateNew();

        // Used by algorithm
        bool GradientSupported { get; }
        bool HessianSupported { get; }
        double Value { get; }
        Vector<double> Gradient { get; }
        Matrix<double> Hessian { get; }
    }
}
