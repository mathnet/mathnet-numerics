// <copyright file="MpConfig.cs" company="Math.NET">
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
    public class MpConfig
    {
        /// <summary>Relative chi-square convergence criterium</summary>
        public double ftol;

        /// <summary>Relative parameter convergence criterium</summary>
        public double xtol;

        /// <summary>Orthogonality convergence criterium</summary>
        public double gtol;

        /// <summary>Finite derivative step size</summary>
        public double epsfcn;

        /// <summary>Initial step bound</summary>
        public double stepfactor;

        /// <summary>Range tolerance for covariance</summary>
        public double covtol;

        /// <summary>
        /// Maximum number of iterations.  If maxiter == 0,
        /// then basic error checking is done, and parameter
        /// errors/covariances are estimated based on input
        /// parameter values, but no fitting iterations are done.
        /// </summary>
        public int MaxIterations;

        /// <summary>Maximum number of function evaluations</summary>
        public int MaxEvaluations;

        /// <summary></summary>
        public int nprint;

        /// <summary>
        /// Scale variables by user values?
        /// 1 = yes, user scale values in diag;
        /// 0 = no, variables scaled internally
        /// </summary>
        public int DoUserScale;

        /// <summary>
        /// Disable check for infinite quantities from user?
        /// 0 = do not perform check
        /// 1 = perform check
        /// </summary>
        public int NoFiniteCheck;
    }
}
