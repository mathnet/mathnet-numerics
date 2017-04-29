// <copyright file="ForwardDifferenceGradientObjectiveFunction.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2017 Math.NET
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
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    /// <summary>
    /// Adapts an objective function with only value implemented 
    /// to provide a gradient as well. Gradient calculation is 
    /// done using the finite difference method, specifically 
    /// forward differences.
    /// 
    /// For each gradient computed, the algorithm requires an 
    /// additional number of function evaluations equal to the 
    /// functions's number of input parameters.
    /// </summary>
    public class ForwardDifferenceGradientObjectiveFunction : IObjectiveFunction
    {
        public IObjectiveFunction InnerObjectiveFunction { get; protected set; }
        protected Vector<double> LowerBound { get; set; }
        protected Vector<double> UpperBound { get; set; }

        protected bool ValueEvaluated { get; set; } = false;
        protected bool GradientEvaluated { get; set; } = false;
        private Vector<double> _gradient;

        public double MinimumIncrement { get; set; }
        public double RelativeIncrement { get; set; }

        public ForwardDifferenceGradientObjectiveFunction(IObjectiveFunction valueOnlyObj, Vector<double> lowerBound, Vector<double> upperBound, double relativeIncrement=1e-5, double minimumIncrement=1e-8)
        {
            InnerObjectiveFunction = valueOnlyObj;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            _gradient = new LinearAlgebra.Double.DenseVector(LowerBound.Count);
            RelativeIncrement = relativeIncrement;
            MinimumIncrement = minimumIncrement;
        }

        protected void EvaluateValue()
        {
            ValueEvaluated = true;
        }

        protected void EvaluateGradient()
        {
            if (!ValueEvaluated)
                EvaluateValue();

            var tmp_point = Point.Clone();
            var tmp_obj = InnerObjectiveFunction.CreateNew();
            for (int ii = 0; ii < _gradient.Count; ++ii)
            {
                var orig_point = tmp_point[ii];
                var rel_incr = orig_point * RelativeIncrement;
                var h = Math.Max(rel_incr, MinimumIncrement);
                var mult = 1;
                if (orig_point + h > UpperBound[ii])
                    mult = -1;

                tmp_point[ii] = orig_point + mult*h;
                tmp_obj.EvaluateAt(tmp_point);
                double bumped_value = tmp_obj.Value;
                _gradient[ii] = (mult * bumped_value - mult * InnerObjectiveFunction.Value) / h;

                tmp_point[ii] = orig_point;
            }
            GradientEvaluated = true;
        }

        public Vector<double> Gradient
        {
            get
            {
                if (!GradientEvaluated)
                    EvaluateGradient();
                return _gradient;
            }
            protected set { _gradient = value; }
        }

        public Matrix<double> Hessian
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsGradientSupported
        {
            get
            {
                return true;
            }
        }

        public bool IsHessianSupported
        {
            get
            {
                return false;
            }
        }

        public Vector<double> Point { get; protected set; }

        public double Value
        {
            get
            {
                if (!ValueEvaluated)
                    EvaluateValue();
                return this.InnerObjectiveFunction.Value;
            }
        }

        public IObjectiveFunction CreateNew()
        {
            var tmp = new ForwardDifferenceGradientObjectiveFunction(this.InnerObjectiveFunction.CreateNew(), LowerBound, UpperBound, this.RelativeIncrement, this.MinimumIncrement);
            return tmp;
        }

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;
            ValueEvaluated = false;
            GradientEvaluated = false;
            InnerObjectiveFunction.EvaluateAt(point);
        }

        public IObjectiveFunction Fork()
        {
            return new ForwardDifferenceGradientObjectiveFunction(this.InnerObjectiveFunction.Fork(), LowerBound, UpperBound, this.RelativeIncrement, this.MinimumIncrement)
            {
                Point = Point?.Clone(),
                GradientEvaluated = GradientEvaluated,
                ValueEvaluated = ValueEvaluated,
                _gradient = _gradient?.Clone()
            };
        }
    }
}
