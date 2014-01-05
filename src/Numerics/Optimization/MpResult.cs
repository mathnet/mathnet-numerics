// <copyright file="MpResult.cs" company="Math.NET">
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

// <contribution>
//    MINPACK-1 Least Squares Fitting Library
//        Original public domain version by B. Garbow, K. Hillstrom, J. More'
//        (Argonne National Laboratory, MINPACK project, March 1980)
//
//        Tranlation to C Language by S. Moshier (http://moshier.net)
//        Translation to C# Language by D. Cuccia (http://davidcuccia.wordpress.com)
//
//        Enhancements and packaging by C. Markwardt
//        (comparable to IDL fitting routine MPFIT see http://cow.physics.wisc.edu/~craigm/idl/idl.html
// </contribution>

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Definition of results structure, for when fit completes
    /// </summary>
    public class MpResult
    {
        /// <summary>Final chi^2</summary>
        public double bestnorm;

        /// <summary>Starting value of chi^2</summary>
        public double orignorm;

        /// <summary>Number of iterations</summary>
        public int niter;

        /// <summary>Number of function evaluations</summary>
        public int nfev;

        /// <summary>Fitting status code</summary>
        public int status;

        /// <summary>Total number of parameters</summary>
        public int npar;

        /// <summary>Number of free parameters</summary>
        public int nfree;

        /// <summary>Number of pegged parameters</summary>
        public int npegged;

        /// <summary>Number of residuals (= num. of data points)</summary>
        public int nfunc;

        /// <summary>Final residuals nfunc-vector, or 0 if not desired</summary>
        public double[] resid;

        /// <summary>Final parameter uncertainties (1-sigma) npar-vector, or 0 if not desired</summary>
        public double[] xerror;

        /// <summary>Final parameter covariance matrix npar x npar array, or 0 if not desired</summary>
        public double[] covar;

        /// <summary>MPFIT version string</summary>
        public string version;

        public MpResult(int numParameters)
        {
            xerror = new double[numParameters];
        }
    }
}
