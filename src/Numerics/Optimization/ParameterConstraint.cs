// <copyright file="ParameterConstraint.cs" company="Math.NET">
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
    /// Definition of a parameter constraint structure
    /// </summary>
    public class ParameterConstraint
    {
        /// <summary>1 = fixed; 0 = free</summary>
        public int isFixed;

        /// <summary>1 = low/upper limit; 0 = no limit</summary>
        public int[] limited = new int[2];

        /// <summary>lower/upper limit boundary value</summary>
        public double[] limits = new double[2];

        /// <summary>Name of parameter, or 0 for none</summary>
        public string parname;

        /// <summary>Step size for finite difference</summary>
        public double step; /* */

        /// <summary>Relative step size for finite difference</summary>
        public double relstep;

        /// <summary>
        /// Sidedness of finite difference derivative
        /// 0 - one-sided derivative computed automatically
        /// 1 - one-sided derivative (f(x+h) - f(x)  )/h
        /// -1 - one-sided derivative (f(x)   - f(x-h))/h
        /// 2 - two-sided derivative (f(x+h) - f(x-h))/(2*h)
        /// 3 - user-computed analytical derivatives
        /// </summary>
        public int side;

        /// <summary>
        /// Derivative debug mode: 1 = Yes; 0 = No;
        ///
        /// If yes, compute both analytical and numerical
        /// derivatives and print them to the console for
        /// comparison.
        ///
        /// NOTE: when debugging, do *not* set side = 3,
        /// but rather to the kind of numerical derivative
        /// you want to compare the user-analytical one to
        /// (0, 1, -1, or 2).
        ///</summary>
        public int deriv_debug;

        /// <summary>Relative tolerance for derivative debug printout</summary>
        public double deriv_reltol;

        /// <summary>Absolute tolerance for derivative debug printout</summary>
        public double deriv_abstol;
    }
}
