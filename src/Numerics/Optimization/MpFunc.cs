// <copyright file="MpFunc.cs" company="Math.NET">
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

using System.Collections.Generic;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// User-function delegate structure required by MPFit.Solve
    /// </summary>
    /// <param name="a">I - Parameters</param>
    /// <param name="fvec">O - function values</param>
    /// <param name="dvec">
    /// O - function derivatives (optional)
    /// "Array of ILists" to accomodate DelimitedArray IList implementation
    /// </param>
    /// <param name="prv">I/O - function private data (cast to object type in user function)</param>
    public delegate int MpFunc(double[] a, double[] fvec, IList<double>[] dvec, object prv);

    //public delegate int MpFunc(int m, int npar, double[] x, double[] fvec, IList<double>[] dvec, object prv);$
}
