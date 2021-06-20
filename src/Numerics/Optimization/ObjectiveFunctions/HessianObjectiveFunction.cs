// <copyright file="HessianObjectiveFunction.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
    internal class HessianObjectiveFunction : IObjectiveFunction
    {
        readonly Func<Vector<double>, (double, Matrix<double>)> _function;

        public HessianObjectiveFunction(Func<Vector<double>, (double, Matrix<double>)> function)
        {
            _function = function;
        }

        public IObjectiveFunction CreateNew()
        {
            return new HessianObjectiveFunction(_function);
        }

        public IObjectiveFunction Fork()
        {
            // no need to deep-clone values since they are replaced on evaluation
            return new HessianObjectiveFunction(_function)
            {
                Point = Point,
                Value = Value,
                Hessian = Hessian
            };
        }

        public bool IsGradientSupported => false;

        public bool IsHessianSupported => true;

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;
            (Value, Hessian) = _function(point);
        }

        public Vector<double> Point { get; private set; }
        public double Value { get; private set; }
        public Matrix<double> Hessian { get; private set; }

        public Vector<double> Gradient => throw new NotSupportedException();
    }
}
