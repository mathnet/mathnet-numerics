using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public interface IUnconstrainedMinimizer
    {
        MinimizationOutput FindMinimum(IObjectiveFunction objective, Vector<double> initial_guess);
    }

}
