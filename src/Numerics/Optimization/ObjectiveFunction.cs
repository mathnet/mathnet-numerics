// <copyright file="ObjectiveFunction.cs" company="Math.NET">
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
using MathNet.Numerics.Optimization.ObjectiveFunctions;

namespace MathNet.Numerics.Optimization
{
    public static class ObjectiveFunction
    {
        /// <summary>
        /// Objective function where neither Gradient nor Hessian is available.
        /// </summary>
        public static IObjectiveFunction Value(Func<Vector<double>, double> function)
        {
            return new ValueObjectiveFunction(function);
        }

        /// <summary>
        /// Objective function where the Gradient is available. Greedy evaluation.
        /// </summary>
        public static IObjectiveFunction Gradient(Func<Vector<double>, Tuple<double, Vector<double>>> function)
        {
            return new GradientObjectiveFunction(function);
        }

        /// <summary>
        /// Objective function where the Gradient is available. Lazy evaluation.
        /// </summary>
        public static IObjectiveFunction Gradient(Func<Vector<double>, double> function, Func<Vector<double>, Vector<double>> gradient)
        {
            return new LazyObjectiveFunction(function, gradient: gradient);
        }

        /// <summary>
        /// Objective function where the Hessian is available. Greedy evaluation.
        /// </summary>
        public static IObjectiveFunction Hessian(Func<Vector<double>, Tuple<double, Matrix<double>>> function)
        {
            return new HessianObjectiveFunction(function);
        }

        /// <summary>
        /// Objective function where the Hessian is available. Lazy evaluation.
        /// </summary>
        public static IObjectiveFunction Hessian(Func<Vector<double>, double> function, Func<Vector<double>, Matrix<double>> hessian)
        {
            return new LazyObjectiveFunction(function, hessian: hessian);
        }

        /// <summary>
        /// Objective function where both Gradient and Hessian are available. Greedy evaluation.
        /// </summary>
        public static IObjectiveFunction GradientHessian(Func<Vector<double>, Tuple<double, Vector<double>, Matrix<double>>> function)
        {
            return new GradientHessianObjectiveFunction(function);
        }

        /// <summary>
        /// Objective function where both Gradient and Hessian are available. Lazy evaluation.
        /// </summary>
        public static IObjectiveFunction GradientHessian(Func<Vector<double>, double> function, Func<Vector<double>, Vector<double>> gradient, Func<Vector<double>, Matrix<double>> hessian)
        {
            return new LazyObjectiveFunction(function, gradient: gradient, hessian: hessian);
        }
    }
}
