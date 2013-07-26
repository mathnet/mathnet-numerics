// <copyright file="Broyden.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using System;

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
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached. Default 1e-8.</param>
        /// <param name="maxIterations">Maximum number of iterations. Default 100.</param>
        /// <returns>Returns the root with the specified accuracy.</returns>
        /// <exception cref="NonConvergenceException"></exception>
        public static double[] FindRoot(Func<double[], double[]> f, double[] initialGuess, double accuracy = 1e-8, int maxIterations = 100)
        {
            double[] root;
            if (TryFindRoot(f, initialGuess, accuracy, maxIterations, out root))
            {
                return root;
            }
            throw new NonConvergenceException("The algorithm has exceeded the number of iterations allowed");
        }

        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <param name="f">The function to find roots from.</param>
        /// <param name="initialGuess">The low value of the range where the root is supposed to be.</param>
        /// <param name="accuracy">Desired accuracy. The root will be refined until the accuracy or the maximum number of iterations is reached.</param>
        /// <param name="maxIterations">Maximum number of iterations. Usually 100.</param>
        /// <param name="root">The root that was found, if any. Undefined if the function returns false.</param>
        /// <returns>True if a root with the specified accuracy was found, else false.</returns>
        public static bool TryFindRoot(Func<double[], double[]> f, double[] initialGuess, double accuracy, int maxIterations, out double[] root)
        {
            double[] F = f(initialGuess);
            DenseVector FVect = new DenseVector(F);
            double g = FVect.Norm(2);

            Matrix<double> B = CalculateApproximateJacobian(f, initialGuess, F);

            Vector<double> x = new DenseVector(initialGuess);

            for (int i = 0; i <= maxIterations; i++)
            {
                Vector<double> dx = -B.LU().Solve(FVect);
                Vector<double> xnew = x + dx;
                double[] FNew = f(xnew.ToArray());
                DenseVector FNewVect = new DenseVector(FNew);
                double gNew = FNewVect.Norm(2);
                if (gNew > g)
                {
                    double g2 = g * g;
                    double scale = g2 / (g2 + gNew * gNew);
                    if (scale == 0.0) scale = 1.0e-4;
                    dx = scale * dx;
                    xnew = x + dx;
                    FNew = f(xnew.ToArray());
                    FNewVect = new DenseVector(FNew);
                    gNew = FNewVect.Norm(2);
                }

                if (gNew < accuracy)
                {
                    root = xnew.ToArray();
                    return true;
                }

                // update Jacobian B
                DenseVector dF = FNewVect - FVect;
                Matrix<double> dB = (dF - B.Multiply(dx)).ToColumnMatrix() * dx.Multiply(1.0 / Math.Pow(dx.Norm(2),2)).ToRowMatrix();
                B = B + dB;

                x = xnew;
                FVect = FNewVect;
                g = gNew;
            }

            root = null;
            return false;
        }

        /// <summary>
        /// Helper method to calculate an approxiamtion of the Jacobian.
        /// </summary>
        /// <param name="f">The function.</param>
        /// <param name="x0">The argument.</param>
        /// <returns></returns>
        private static Matrix<double> CalculateApproximateJacobian(Func<double[], double[]> f, double[] x0, double[] F0)
        {
            int dim = x0.Length;
            double[] xpos = new double[dim];
            DenseMatrix B = new DenseMatrix(dim);

            for (int j = 0; j < dim; j++)
            {
                Array.Copy(x0, xpos, dim);

                double h = Math.Abs(x0[j]) * 1.0e-4;
                if (h == 0.0) h = 1.0e-4;
                xpos[j] += h;

                double[] Fpos = f(xpos);

                for (int i = 0; i < dim; i++)
                {
                    B[i, j] = (Fpos[i] - F0[i]) / h;
                }
            }

            return B;
        }
    }
}
