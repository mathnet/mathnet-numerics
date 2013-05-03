// <copyright file="Bisection.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// 
// Copyright (c) 2009-2013 Math.NET
// 
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;

namespace MathNet.Numerics.RootFinding.Algorithms
{
    public class Bisection
    {
        public Bisection(double objective_tolerance = 1e-5, double x_tolerance = 1e-5, double lower_expansion_factor = -1.0, double upper_expansion_factor = -1.0, int max_expansion_steps = 10)
        {
            ObjectiveTolerance = objective_tolerance;
            XTolerance = x_tolerance;
            LowerExpansionFactor = lower_expansion_factor;
            UpperExpansionFactor = upper_expansion_factor;
            MaxExpansionSteps = max_expansion_steps;
        }

        public double ObjectiveTolerance { get; set; }
        public double XTolerance { get; set; }
        public double LowerExpansionFactor { get; set; }
        public double UpperExpansionFactor { get; set; }
        public int MaxExpansionSteps { get; set; }

        public double FindRoot(Func<double, double> objective_function, double lower_bound, double upper_bound)
        {
            double lower_val = objective_function(lower_bound);
            double upper_val = objective_function(upper_bound);

            if (lower_val == 0.0)
                return lower_bound;
            if (upper_val == 0.0)
                return upper_bound;

            ValidateEvaluation(lower_val, lower_bound);
            ValidateEvaluation(upper_val, upper_bound);

            if (Math.Sign(lower_val) == Math.Sign(upper_val) && LowerExpansionFactor <= 1.0 && UpperExpansionFactor <= 1.0)
                throw new Exception("Bounds do not necessarily span a root, and StepExpansionFactor is not set to expand the interval in this case.");

            int expansion_steps = 0;
            while (Math.Sign(lower_val) == Math.Sign(upper_val) && expansion_steps < MaxExpansionSteps)
            {
                double range = upper_bound - lower_bound;
                if (UpperExpansionFactor <= 0.0 || (LowerExpansionFactor > 0.0 && Math.Abs(lower_val) < Math.Abs(upper_val)))
                {
                    lower_bound = upper_bound - LowerExpansionFactor*range;
                    lower_val = objective_function(lower_bound);
                    ValidateEvaluation(lower_val, lower_bound);
                }
                else
                {
                    upper_bound = lower_bound + UpperExpansionFactor*range;
                    upper_val = objective_function(upper_bound);
                    ValidateEvaluation(upper_val, upper_bound);
                }
                expansion_steps += 1;
            }

            if (expansion_steps == MaxExpansionSteps)
                throw new NonConvergenceException();

            while (Math.Abs(upper_val - lower_val) > 0.5*ObjectiveTolerance || Math.Abs(upper_bound - lower_bound) > 0.5*XTolerance)
            {
                double midpoint = 0.5*(upper_bound + lower_bound);
                double midval = objective_function(midpoint);
                ValidateEvaluation(midval, midpoint);

                if (Math.Sign(midval) == Math.Sign(lower_val))
                {
                    lower_bound = midpoint;
                    lower_val = midval;
                }
                else if (Math.Sign(midval) == Math.Sign(upper_val))
                {
                    upper_bound = midpoint;
                    upper_val = midval;
                }
                else
                {
                    return midpoint;
                }
            }

            return 0.5*(lower_bound + upper_bound);
        }

        void ValidateEvaluation(double output, double input)
        {
            if (!IsFinite(output))
                throw new Exception(String.Format("Objective function returned non-finite result: f({0}) = {1}", input, output));
        }

        static bool IsFinite(double x)
        {
            return !(Double.IsInfinity(x) || Double.IsNaN(x));
        }
    }
}
