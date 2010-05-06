// <copyright file="FourierOptions.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.IntegralTransforms
{
    using System;

    /// <summary>
    /// Fourier Transform Convention
    /// </summary>
    [Flags]
    public enum FourierOptions
    {
        // FLAGS:

        /// <summary>
        /// Inverse integrand exponent (forward: positive sign; inverse: negative sign).
        /// </summary>
        InverseExponent = 0x01,

        /// <summary>
        /// Only scale by 1/N in the inverse direction; No scaling in forward direction.
        /// </summary>
        AsymmetricScaling = 0x02,

        /// <summary>
        /// Don't scale at all (neither on forward nor on inverse transformation).
        /// </summary>
        NoScaling = 0x04,

        // USABILITY POINTERS:

        /// <summary>
        /// Universal; Symmetric scaling and common exponent (used in Maple).
        /// </summary>
        Default = 0,

        /// <summary>
        /// Only scale by 1/N in the inverse direction; No scaling in forward direction (used in Matlab). [= AsymmetricScaling]
        /// </summary>
        Matlab = AsymmetricScaling,

        /// <summary>
        /// Inverse integrand exponent; No scaling at all (used in all Numerical Recipes based implementations). [= InverseExponent | NoScaling]
        /// </summary>
        NumericalRecipes = InverseExponent | NoScaling
    }
}
