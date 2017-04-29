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
