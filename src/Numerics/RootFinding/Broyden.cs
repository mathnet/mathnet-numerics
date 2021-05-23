// <copyright file="Broyden.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2020 Math.NET
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
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.RootFinding
{
    /// <summary>
    /// Algorithm by Broyden.
    /// Implementation inspired by Press, Teukolsky, Vetterling, and Flannery, "Numerical Recipes in C", 2nd edition, Cambridge University Press
    /// </summary>
    public static class Broyden
    {
        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="initialGuess">Initial guess of the root.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8. Must be greater than 0.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <param name="jacobianStepSize">Relative step size for calculating the Jacobian matrix at first step. Default 1.0e-4</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="NonConvergenceException"></exception>
        public static double[] FindRoot(Func<double[], double[]> f, double[] initialGuess, double accuracy = 1e-8, int maxIterations = 100, double jacobianStepSize = 1.0e-4)
        {
            if (TryFindRootWithJacobianStep(f, initialGuess, accuracy, maxIterations, jacobianStepSize, out var root))
            {
                return root;
            }

            throw new NonConvergenceException("The algorithm has failed, exceeded the number of iterations allowed or there is no root within the provided bounds.");
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="initialGuess">Initial guess of the root.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Must be greater than 0.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 100.</param>
        /// <param name="jacobianStepSize">Relative step size for calculating the Jacobian matrix at first step.</param>
        /// <param name="root">The root that was found, if any. Undefined if the function returns false.</param>
        /// <returns>True if a root with the specified accuracy was found, else false.</returns>
        public static bool TryFindRootWithJacobianStep(Func<double[], double[]> f, double[] initialGuess, double accuracy, int maxIterations, double jacobianStepSize, out double[] root)
        {
            if (accuracy <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be greater than zero.");
            }

            var x = new DenseVector(initialGuess);

            double[] y0 = f(initialGuess);
            var y = new DenseVector(y0);
            double g = y.L2Norm();

            Matrix<double> B = CalculateApproximateJacobian(f, initialGuess, y0, jacobianStepSize);

            try
            {
                for (int i = 0; i <= maxIterations; i++)
                {
                    var dx = (DenseVector) (-B.LU().Solve(y));
                    var xnew = x + dx;
                    var ynew = new DenseVector(f(xnew.Values));
                    double gnew = ynew.L2Norm();

                    if (gnew > g)
                    {
                        double g2 = g*g;
                        double scale = g2/(g2 + gnew*gnew);
                        if (scale == 0.0)
                        {
                            scale = 1.0e-4;
                        }

                        dx = scale*dx;
                        xnew = x + dx;
                        ynew = new DenseVector(f(xnew.Values));
                        gnew = ynew.L2Norm();
                    }

                    if (gnew < accuracy)
                    {
                        root = xnew.Values;
                        return true;
                    }

                    // update Jacobian B
                    DenseVector dF = ynew - y;
                    Matrix<double> dB = (dF - B.Multiply(dx)).ToColumnMatrix() * dx.Multiply(1.0 / Math.Pow(dx.L2Norm(), 2)).ToRowMatrix();
                    B = B + dB;

                    x = xnew;
                    y = ynew;
                    g = gnew;
                }
            }
            catch (InvalidParameterException)
            {
                root = null;
                return false;
            }

            root = null;
            return false;
        }
        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="initialGuess">Initial guess of the root.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Must be greater than 0.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 100.</param>
        /// <param name="root">The root that was found, if any. Undefined if the function returns false.</param>
        /// <returns>True if a root with the specified accuracy was found, else false.</returns>
        public static bool TryFindRoot(Func<double[], double[]> f, double[] initialGuess, double accuracy, int maxIterations, out double[] root)
        {
            return TryFindRootWithJacobianStep(f, initialGuess, accuracy, maxIterations, 1.0e-4, out root);
        }

        /// <summary>
        /// Helper method to calculate an approximation of the Jacobian.
        /// </summary>
        /// <param name="f">The function.</param>
        /// <param name="x0">The argument (initial guess).</param>
        /// <param name="y0">The result (of initial guess).</param>
        /// <param name="jacobianStepSize">Relative step size for calculating the Jacobian.</param>
        static Matrix<double> CalculateApproximateJacobian(Func<double[], double[]> f, double[] x0, double[] y0, double jacobianStepSize)
        {
            int dim = x0.Length;
            var B = new DenseMatrix(dim);

            var x = new double[dim];
            Array.Copy(x0, 0, x, 0, dim);

            for (int j = 0; j < dim; j++)
            {
                double h = (1.0+Math.Abs(x0[j]))*jacobianStepSize;

                var xj = x[j];
                x[j] = xj + h;
                double[] y = f(x);
                x[j] = xj;

                for (int i = 0; i < dim; i++)
                {
                    B.At(i, j, (y[i] - y0[i])/h);
                }
            }

            return B;
        }
    }
}
