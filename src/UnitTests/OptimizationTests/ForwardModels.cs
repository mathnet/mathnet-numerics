// <copyright file="ForwardModels.cs" company="Math.NET">
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
using System.Collections.Generic;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    public static class ForwardModels
    {
        /*
         * linear fit function
         *
         * m - number of data points
         * n - number of parameters (2)
         * p - array of fit parameters
         * dy - array of residuals to be returned
         * CustomUserVariable - private data (struct vars_struct *)
         *
         * RETURNS: error code (0 = success)
         */

        public static int LinFunc(double[] p, double[] dy, IList<double>[] dvec, object vars)
        {
            int i;
            double[] x, y, ey;
            double f;

            CustomUserVariable v = (CustomUserVariable)vars;

            x = v.X;
            y = v.Y;
            ey = v.Ey;

            for (i = 0; i < dy.Length; i++)
            {
                f = p[0] - p[1]*x[i]; /* Linear fit function */
                dy[i] = (y[i] - f)/ey[i];
            }

            return 0;
        }

        /*
        * quadratic fit function
        *
        * m - number of data points
        * n - number of parameters (2)
        * p - array of fit parameters
        * dy - array of residuals to be returned
        * CustomUserVariable - private data (struct vars_struct *)
        *
        * RETURNS: error code (0 = success)
        */

        public static int QuadFunc(double[] p, double[] dy, IList<double>[] dvec, object vars)
        {
            int i;
            double[] x, y, ey;

            CustomUserVariable v = (CustomUserVariable)vars;
            x = v.X;
            y = v.Y;
            ey = v.Ey;

            /* Console.Write ("QuadFunc %f %f %f\n", p[0], p[1], p[2]); */

            for (i = 0; i < dy.Length; i++)
            {
                dy[i] = (y[i] - p[0] - p[1]*x[i] - p[2]*x[i]*x[i])/ey[i];
            }

            return 0;
        }


        /*
         * gaussian fit function
         *
         * m - number of data points
         * n - number of parameters (4)
         * p - array of fit parameters
         * dy - array of residuals to be returned
         * CustomUserVariable - private data (struct vars_struct *)
         *
         * RETURNS: error code (0 = success)
         */

        public static int GaussFunc(double[] p, double[] dy, IList<double>[] dvec, object vars)
        {
            int i;
            CustomUserVariable v = (CustomUserVariable)vars;
            double[] x, y, ey;
            double xc, sig2;

            x = v.X;
            y = v.Y;
            ey = v.Ey;

            sig2 = p[3]*p[3];

            for (i = 0; i < dy.Length; i++)
            {
                xc = x[i] - p[2];
                dy[i] = (y[i] - p[1]*Math.Exp(-0.5*xc*xc/sig2) - p[0])/ey[i];
            }

            return 0;
        }
    }
}
