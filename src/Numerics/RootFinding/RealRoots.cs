// <copyright file="FloatingPointRoots.cs" company="Math.NET">
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
using MathNet.Numerics.RootFinding.Algorithms;

namespace MathNet.Numerics.RootFinding
{
    public static class RealRoots
    {
        public static double OfFunction(Func<double, double> f, double lowerBound, double upperBound, double accuracy = 1e-8)
        {
            return Brent.FindRoot(f, lowerBound, upperBound, accuracy, 100);
        }

        public static double OfFunctionAndDerivative(Func<double, double> f, Func<double, double> df, double lowerBound, double upperBound, double accuracy = 1e-8)
        {
            double root;
            if (HybridNewtonRaphson.TryFindRoot(f, df, lowerBound, upperBound, accuracy, 100, 20, out root))
            {
                return root;
            }
            if (Brent.TryFindRoot(f, lowerBound, upperBound, accuracy, 100, out root))
            {
                return root;
            }

            throw new NonConvergenceException("The algorithm has exceeded the number of iterations allowed");
        }
    }
}
