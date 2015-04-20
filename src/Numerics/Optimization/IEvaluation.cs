using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        EvaluationStatus Status { get; }
        double Value { get; }
        Vector<double> Gradient { get; }
        Matrix<double> Hessian { get; }
    }
}
