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
        EvaluationStatus Status { get; set; }

        // Used by algorithm
        double Value { get; }
        Vector<double> Gradient { get; }
        Matrix<double> Hessian { get; }

        // Used by ObjectiveFunction
        void Reset(Vector<double> new_point);
        double ValueRaw { get; set; }
        Vector<double> GradientRaw { get; set; }
        Matrix<double> HessianRaw { get; set; }
    }
}
