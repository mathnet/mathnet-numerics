using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.Optimization
{
    public class OptimizationException : Exception
    {
        public OptimizationException(string message)
            : base(message) {}

        public OptimizationException(string message, Exception inner_exception) 
            : base(message, inner_exception) { }
    }

    public class MaximumIterationsException : OptimizationException
    {
       public MaximumIterationsException(string message)
            : base(message) {}
    }

    public class EvaluationException : OptimizationException
    {
        public Vector<double> Point { get; private set; }
        public EvaluationException(string message, Vector<double> point)
            : base(message) 
        {
            this.Point = point;
        }

        public EvaluationException(string message, double point)
            : base(message)
        {
            this.Point = DenseVector.Create(1, point);
        }

        public EvaluationException(string message, Exception inner_exception, Vector<double> point) 
            : base(message, inner_exception) 
        {
            this.Point = point;
        }
    }

    public class InnerOptimizationException : OptimizationException
    {
        public InnerOptimizationException(string message)
            : base(message) {}

        public InnerOptimizationException(string message, Exception inner_exception) 
            : base(message, inner_exception) { }
    }

    public class IncompatibleObjectiveException : OptimizationException
    {
        public IncompatibleObjectiveException(string message)
            : base(message) {}
    }


}
