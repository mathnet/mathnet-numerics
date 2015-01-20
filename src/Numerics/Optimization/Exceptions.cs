﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    public class OptimizationException : Exception
    {
        public OptimizationException(string message)
            : base(message) { }

        public OptimizationException(string message, Exception inner_exception)
            : base(message, inner_exception) { }
    }

    public class MaximumIterationsException : OptimizationException
    {
        public MaximumIterationsException(string message)
            : base(message) { }
    }

    public class EvaluationException : OptimizationException
    {
        public IEvaluation Evaluation { get; private set; }

        public EvaluationException(string message, IEvaluation eval)
            : base(message)
        {
            this.Evaluation = eval;
        }

        public EvaluationException(string message, IEvaluation eval, Exception inner_exception)
            : base(message, inner_exception)
        {
            this.Evaluation = eval;
        }

        //public EvaluationException(string message, IEvaluation1D eval)
        //    : base(message)
        //{
        //    this.Evaluation = new OneDEvaluationExpander(eval);
        //}

        //public EvaluationException(string message, IEvaluation1D eval, Exception inner_exception)
        //    : base(message, inner_exception)
        //{
        //    this.Evaluation = new OneDEvaluationExpander(eval);
        //}

    }

    public class InnerOptimizationException : OptimizationException
    {
        public InnerOptimizationException(string message)
            : base(message) { }

        public InnerOptimizationException(string message, Exception inner_exception)
            : base(message, inner_exception) { }
    }

    public class IncompatibleObjectiveException : OptimizationException
    {
        public IncompatibleObjectiveException(string message)
            : base(message) { }
    }
}
