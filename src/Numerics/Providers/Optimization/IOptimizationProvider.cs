// <copyright file="IOptimizationProvider.cs" company="Math.NET">
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

using MathNet.Numerics.Optimization;

namespace MathNet.Numerics.Providers.Optimization
{
    /// <summary>
    /// Function specifying the model. This takes in model parameters, calculates residuals
    /// and updates the residuals array with these.
    /// </summary>
    /// <param name="parameters">The model parameters. The function must not change these.</param>
    /// <param name="r">The residuals to be updated. The existing array should be updated.</param>
    /// <returns></returns>
    public delegate void LeastSquaresForwardModel(double[] p, double[] r);

    /// <summary>
    /// Function providing the Jacobian matrix in column-major format for a set of model parameter values.
    /// Jacobian is dr_i / dp_i, r being the residuals vector and p the vector of parameters
    /// </summary>
    /// <param name="p">THe model parameters. The function must not change these.</param>
    /// <param name="jacobian">THe Jacobian matrix in column-major format.</param>
    public delegate void Jacobian(double[] p, double[] jacobian);

    /// <summary>
    /// Interface to linear algebra algorithms that work off 1-D arrays.
    /// </summary>
    /// <typeparam name="T">Supported data types are Double, Single, Complex, and Complex32.</typeparam>
    public interface IOptimizationProvider<T>
        where T : struct
    {
        NonLinearLeastSquaresResult NonLinearLeastSquaresUnboundedMinimize(
            int residualsLength, T[] initialGuess, LeastSquaresForwardModel function,
            out T[] parameters, Jacobian jacobianFunction = null, NonLinearLeastSquaresOptions options = null);
    }
}
