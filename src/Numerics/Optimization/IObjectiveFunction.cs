﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public interface IObjectiveFunction
    {
        bool GradientSupported { get; }
        bool HessianSupported { get; }

        IEvaluation CreateEvaluationObject();
        void Evaluate(Vector<double> point, IEvaluation output);   
    }
}
