// <copyright file="NewtonRaphson.cs" company="Math.NET">
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
    public static class NewtonRaphson
    {
        /// <summary>Find a solution of the equation f(x)=0.</summary>
        /// <remarks>Hybrid Newton-Raphson that when failing falls back to bisection.</remarks>
        /// <exception cref="NonConvergenceException"></exception>
        public static double FindRoot(Func<double, double> f, Func<double, double> df, double lowerBound, double upperBound, double accuracy = 1e-8, int maxIterations = 100)
        {
            double fmin = f(lowerBound);
            double fmax = f(upperBound);

            if (fmin == 0.0) return lowerBound;
            if (fmax == 0.0) return upperBound;

            double root = 0.5*(lowerBound + upperBound);
            double fx = f(root);
            double lastStep = Math.Abs(upperBound - lowerBound);
            for (int i = 0; i < maxIterations; i++)
            {
                double dfx = df(root);

                // Netwon-Raphson step
                double step = fx/dfx;
                root -= step;

                if (root < lowerBound || root > upperBound || Math.Abs(2*fx) > Math.Abs(lastStep*dfx))
                {
                    // Newton-Raphson step failed ->  bisect instead
                    root = 0.5*(upperBound + lowerBound);
                    fx = f(root);
                    lastStep = 0.5*Math.Abs(upperBound - lowerBound);
                    if (Math.Sign(fx) == Math.Sign(fmin))
                    {
                        lowerBound = root;
                        fmin = fx;
                    }
                    else
                    {
                        upperBound = root;
                        fmax = fx;
                    }
                    continue;
                }
                
                if (Math.Abs(step) < accuracy)
                {
                    return root;
                }

                // Evaluation
                fx = f(root);
                lastStep = step;

                // Update Bounds
                if (Math.Sign(fx) != Math.Sign(fmin))
                {
                    upperBound = root;
                    fmax = fx;
                }
                else if (Math.Sign(fx) != Math.Sign(fmax))
                {
                    lowerBound = root;
                    fmin = fx;
                }
                else if (Math.Sign(fmin) != Math.Sign(fmax))
                {
                    return root;
                }
            }

            throw new NonConvergenceException("The algorithm has exceeded the number of iterations allowed");
        }
    }
}
