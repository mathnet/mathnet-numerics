using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    [Flags]
    public enum EvaluationStatus
    {
        None = 0,
        Value = 1,
        Gradient = 2,
        Hessian = 4
    }

    public interface IObjectiveFunction
    {
        void EvaluateAt(Vector<double> point);
        IObjectiveFunction Fork();

        Vector<double> Point { get; }
        double Value { get; }

        bool IsGradientSupported { get; }
        Vector<double> Gradient { get; }

        bool IsHessianSupported { get; }
        Matrix<double> Hessian { get; }
    }
}
