// <copyright file="ModifiedStruve.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2012 Math.NET
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
//    CERN - European Laboratory for Particle Physics
//        http://www.docjar.com/html/api/cern/jet/math/Bessel.java.html
//        Copyright 1999 CERN - European Laboratory for Particle Physics.
//        Permission to use, copy, modify, distribute and sell this software and its documentation for any purpose 
//        is hereby granted without fee, provided that the above copyright notice appear in all copies and 
//        that both that copyright notice and this permission notice appear in supporting documentation. 
//        CERN makes no representations about the suitability of this software for any purpose. 
//        It is provided "as is" without expressed or implied warranty.
//    TOMS757 - Uncommon Special Functions (Fortran77) by Allan McLeod
//        http://people.sc.fsu.edu/~jburkardt/f77_src/toms757/toms757.html
//    Wei Wu
//    Cephes Math Library, Stephen L. Moshier
//    ALGLIB 2.0.1, Sergey Bochkanov
// </contribution>

// ReSharper disable CheckNamespace
namespace MathNet.Numerics
// ReSharper restore CheckNamespace
{
    using System;

    /// <summary>
    /// This partial implementation of the SpecialFunctions class contains all methods related to the modified bessel function.
    /// </summary>
    public static partial class SpecialFunctions
    {
        /// <summary>
        /// Returns the modified Struve function of order 0.
        /// </summary>
        /// <param name="x">The value to compute the function of.</param>
        /// <returns></returns>
        public static double StruveL0(double x)
        {
            //*********************************************************************72
            //
            //c STRVL0 calculates the modified Struve function of order 0.
            //
            //   DESCRIPTION:
            //
            //      This function calculates the modified Struve function of
            //      order 0, denoted L0(x), defined as the solution of the
            //      second-order equation
            //
            //                  x*D(Df) + Df - x*f  =  2x/pi
            //
            //      This subroutine is set up to work on IEEE machines.
            //      For other machines, you should retrieve the code
            //      from the general MISCFUN archive.
            //
            //
            //   ERROR RETURNS:
            //
            //      If the value of |XVALUE| is too large, the result
            //      would cause an floating-pt overflow. An error message
            //      is printed and the function returns the value of
            //      sign(XVALUE)*XMAX where XMAX is the largest possible
            //      floating-pt argument.
            //
            //
            //   MACHINE-DEPENDENT PARAMETERS:
            //
            //      NTERM1 - INTEGER - The no. of terms for the array ARL0.
            //                         The recommended value is such that
            //                             ABS(ARL0(NTERM1)) < EPS/100
            //
            //      NTERM2 - INTEGER - The no. of terms for the array ARL0AS.
            //                         The recommended value is such that
            //                             ABS(ARL0AS(NTERM2)) < EPS/100 
            //
            //      NTERM3 - INTEGER - The no. of terms for the array AI0ML0.
            //                         The recommended value is such that
            //                             ABS(AI0ML0(NTERM3)) < EPS/100
            //
            //      XLOW - DOUBLE PRECISION - The value of x below which L0(x) = 2*x/pi
            //                    to machine precision. The recommended value is
            //                             3*SQRT(EPS)
            //
            //      XHIGH1 - DOUBLE PRECISION - The value beyond which the Chebyshev series
            //                      in the asymptotic expansion of I0 - L0 gives
            //                      1.0 to machine precision. The recommended value
            //                      is   SQRT( 30/EPSNEG )
            //
            //      XHIGH2 - DOUBLE PRECISION - The value beyond which the Chebyshev series
            //                      in the asymptotic expansion of I0 gives 1.0
            //                      to machine precision. The recommended value
            //                      is   28 / EPSNEG
            //
            //      XMAX - DOUBLE PRECISION - The value of XMAX, where XMAX is the
            //                    largest possible floating-pt argument.
            //                    This is used to prevent overflow.
            //
            //      For values of EPS, EPSNEG and XMAX the user should refer
            //      to the file MACHCON.TXT
            //
            //      The machine-arithmetic constants are given in DATA
            //      statements.
            //
            //
            //   INTRINSIC FUNCTIONS USED:
            //
            //      EXP , LOG , SQRT
            //
            //
            //   OTHER MISCFUN SUBROUTINES USED:
            //
            //          CHEVAL , ERRPRN
            //
            //
            //   AUTHOR:
            //          DR. ALLAN J. MACLEOD
            //          DEPT. OF MATHEMATICS AND STATISTICS
            //          UNIVERSITY OF PAISLEY
            //          HIGH ST.
            //          PAISLEY
            //          SCOTLAND
            //          PA1 2BE
            //
            //      (e-mail: macl_ms0@paisley.ac.uk )
            //
            //
            //   LATEST REVISION:
            //                   12 JANUARY, 1996
            //
            //

            if (x < 0.0)
            {
                return -StruveL0(-x);
            }

            const double LNR2PI = 0.91893853320467274178;
            const double TWOBPI = 0.63661977236758134308;

            double[] ARL0 = new double[28];
            ARL0[0] = 0.42127458349979924863;
            ARL0[1] = -0.33859536391220612188;
            ARL0[2] = 0.21898994812710716064;
            ARL0[3] = -0.12349482820713185712;
            ARL0[4] = 0.6214209793866958440e-1;
            ARL0[5] = -0.2817806028109547545e-1;
            ARL0[6] = 0.1157419676638091209e-1;
            ARL0[7] = -0.431658574306921179e-2;
            ARL0[8] = 0.146142349907298329e-2;
            ARL0[9] = -0.44794211805461478e-3;
            ARL0[10] = 0.12364746105943761e-3;
            ARL0[11] = -0.3049028334797044e-4;
            ARL0[12] = 0.663941401521146e-5;
            ARL0[13] = -0.125538357703889e-5;
            ARL0[14] = 0.20073446451228e-6;
            ARL0[15] = -0.2588260170637e-7;
            ARL0[16] = 0.241143742758e-8;
            ARL0[17] = -0.10159674352e-9;
            ARL0[18] = -0.1202430736e-10;
            ARL0[19] = 0.262906137e-11;
            ARL0[20] = -0.15313190e-12;
            ARL0[21] = -0.1574760e-13;
            ARL0[22] = 0.315635e-14;
            ARL0[23] = -0.4096e-16;
            ARL0[24] = -0.3620e-16;
            ARL0[25] = 0.239e-17;
            ARL0[26] = 0.36e-18;
            ARL0[27] = -0.4e-19;

            double[] ARL0AS = new double[16];
            ARL0AS[0] = 2.00861308235605888600;
            ARL0AS[1] = 0.403737966500438470e-2;
            ARL0AS[2] = -0.25199480286580267e-3;
            ARL0AS[3] = 0.1605736682811176e-4;
            ARL0AS[4] = -0.103692182473444e-5;
            ARL0AS[5] = 0.6765578876305e-7;
            ARL0AS[6] = -0.444999906756e-8;
            ARL0AS[7] = 0.29468889228e-9;
            ARL0AS[8] = -0.1962180522e-10;
            ARL0AS[9] = 0.131330306e-11;
            ARL0AS[10] = -0.8819190e-13;
            ARL0AS[11] = 0.595376e-14;
            ARL0AS[12] = -0.40389e-15;
            ARL0AS[13] = 0.2651e-16;
            ARL0AS[14] = -0.208e-17;
            ARL0AS[15] = 0.11e-18;

            double[] AI0ML0 = new double[24];
            AI0ML0[0] = 2.00326510241160643125;
            AI0ML0[1] = 0.195206851576492081e-2;
            AI0ML0[2] = 0.38239523569908328e-3;
            AI0ML0[3] = 0.7534280817054436e-4;
            AI0ML0[4] = 0.1495957655897078e-4;
            AI0ML0[5] = 0.299940531210557e-5;
            AI0ML0[6] = 0.60769604822459e-6;
            AI0ML0[7] = 0.12399495544506e-6;
            AI0ML0[8] = 0.2523262552649e-7;
            AI0ML0[9] = 0.504634857332e-8;
            AI0ML0[10] = 0.97913236230e-9;
            AI0ML0[11] = 0.18389115241e-9;
            AI0ML0[12] = 0.3376309278e-10;
            AI0ML0[13] = 0.611179703e-11;
            AI0ML0[14] = 0.108472972e-11;
            AI0ML0[15] = 0.18861271e-12;
            AI0ML0[16] = 0.3280345e-13;
            AI0ML0[17] = 0.565647e-14;
            AI0ML0[18] = 0.93300e-15;
            AI0ML0[19] = 0.15881e-15;
            AI0ML0[20] = 0.2791e-16;
            AI0ML0[21] = 0.389e-17;
            AI0ML0[22] = 0.70e-18;
            AI0ML0[23] = 0.16e-18;

            // MACHINE-DEPENDENT VALUES (Suitable for IEEE-arithmetic machines)
            const int NTERM1 = 25; const int NTERM2 = 14; const int NTERM3 = 21;
            const double XLOW = 4.4703484e-8; const double XMAX = 1.797693e308;
            const double XHIGH1 = 5.1982303e8; const double XHIGH2 = 2.5220158e17;

            // Code for |xvalue| <= 16
            if (x <= 16.0)
            {
                if (x < XLOW)
                {
                    return TWOBPI * x;
                }

                double T = (4.0 * x - 24.0) / (x + 24.0);
                return TWOBPI * x * Evaluate.ChebyshevSum(NTERM1, ARL0, T) * Math.Exp(x);
            }

            // Code for |xvalue| > 16
            double ch1;
            if (x > XHIGH2)
            {
                ch1 = 1.0;
            }
            else
            {
                double T = (x - 28.0) / (4.0 - x);
                ch1 = Evaluate.ChebyshevSum(NTERM2, ARL0AS, T);
            }

            double ch2;
            if (x > XHIGH1)
            {
                ch2 = 1.0;
            }
            else
            {
                double xsq = x * x;
                double T = (800.0 - xsq) / (288.0 + xsq);
                ch2 = Evaluate.ChebyshevSum(NTERM3, AI0ML0, T);
            }

            double test = Math.Log(ch1) - LNR2PI - Math.Log(x) / 2.0 + x;
            if (test > Math.Log(XMAX))
            {
                throw new ArithmeticException("ERROR IN MISCFUN FUNCTION STRVL0: ARGUMENT CAUSES OVERFLOW");
            }

            return Math.Exp(test) - TWOBPI * ch2 / x;
        }

        /// <summary>
        /// Returns the modified Struve function of order 1.
        /// </summary>
        /// <param name="x">The value to compute the function of.</param>
        /// <returns></returns>
        public static double StruveL1(double x)
        {
            //*********************************************************************72
            //
            //c STRVL1 calculates the modified Struve function of order 1.
            //
            //   DESCRIPTION:
            //
            //      This function calculates the modified Struve function of
            //      order 1, denoted L1(x), defined as the solution of
            //
            //               x*x*D(Df) + x*Df - (x*x+1)f = 2*x*x/pi
            //
            //      This subroutine is set up to work on IEEE machines.
            //      For other machines, you should retrieve the code
            //      from the general MISCFUN archive.
            //
            //
            //   ERROR RETURNS:
            //
            //      If the value of |XVALUE| is too large, the result
            //      would cause an floating-pt overflow. An error message
            //      is printed and the function returns the value of
            //      sign(XVALUE)*XMAX where XMAX is the largest possible
            //      floating-pt argument.
            //
            //
            //   MACHINE-DEPENDENT PARAMETERS:
            //
            //      NTERM1 - INTEGER - The no. of terms for the array ARL1.
            //                         The recommended value is such that
            //                             ABS(ARL1(NTERM1)) < EPS/100
            //
            //      NTERM2 - INTEGER - The no. of terms for the array ARL1AS.
            //                         The recommended value is such that
            //                             ABS(ARL1AS(NTERM2)) < EPS/100 
            //
            //      NTERM3 - INTEGER - The no. of terms for the array AI1ML1.
            //                         The recommended value is such that
            //                             ABS(AI1ML1(NTERM3)) < EPS/100
            //
            //      XLOW1 - DOUBLE PRECISION - The value of x below which 
            //                                     L1(x) = 2*x*x/(3*pi)
            //                                 to machine precision. The recommended 
            //                                 value is     SQRT(15*EPS)
            //
            //      XLOW2 - DOUBLE PRECISION - The value of x below which L1(x) set to 0.0.
            //                     This is used to prevent underflow. The
            //                     recommended value is
            //                              SQRT(5*XMIN)
            //
            //      XHIGH1 - DOUBLE PRECISION - The value of |x| above which the Chebyshev
            //                      series in the asymptotic expansion of I1
            //                      equals 1.0 to machine precision. The
            //                      recommended value is  SQRT( 30 / EPSNEG ).
            //
            //      XHIGH2 - DOUBLE PRECISION - The value of |x| above which the Chebyshev
            //                      series in the asymptotic expansion of I1 - L1
            //                      equals 1.0 to machine precision. The recommended
            //                      value is   30 / EPSNEG.
            // 
            //      XMAX - DOUBLE PRECISION - The value of XMAX, where XMAX is the
            //                    largest possible floating-pt argument.
            //                    This is used to prevent overflow.
            //
            //      For values of EPS, EPSNEG, XMIN, and XMAX the user should refer 
            //      to the file MACHCON.TXT
            //
            //      The machine-arithmetic constants are given in DATA
            //      statements.
            //
            //
            //   INTRINSIC FUNCTIONS USED:
            //
            //      EXP , LOG , SQRT
            //
            //
            //   OTHER MISCFUN SUBROUTINES USED:
            //
            //          CHEVAL , ERRPRN
            //
            //
            //   AUTHOR:
            //          DR. ALLAN J. MACLEOD
            //          DEPT. OF MATHEMATICS AND STATISTICS
            //          UNIVERSITY OF PAISLEY
            //          HIGH ST.
            //          PAISLEY
            //          SCOTLAND
            //          PA1 2BE
            //
            //          (e-mail: macl_ms0@paisley.ac.uk )
            //
            //
            //   LATEST UPDATE:
            //                 12 JANUARY, 1996
            //
            //

            if (x < 0.0)
            {
                return StruveL1(-x);
            }

            const double LNR2PI = 0.91893853320467274178;
            const double PI3BY2 = 4.71238898038468985769;
            const double TWOBPI = 0.63661977236758134308;

            double[] ARL1 = new double[27];
            ARL1[0] = 0.38996027351229538208;
            ARL1[1] = -0.33658096101975749366;
            ARL1[2] = 0.23012467912501645616;
            ARL1[3] = -0.13121594007960832327;
            ARL1[4] = 0.6425922289912846518e-1;
            ARL1[5] = -0.2750032950616635833e-1;
            ARL1[6] = 0.1040234148637208871e-1;
            ARL1[7] = -0.350532294936388080e-2;
            ARL1[8] = 0.105748498421439717e-2;
            ARL1[9] = -0.28609426403666558e-3;
            ARL1[10] = 0.6925708785942208e-4;
            ARL1[11] = -0.1489693951122717e-4;
            ARL1[12] = 0.281035582597128e-5;
            ARL1[13] = -0.45503879297776e-6;
            ARL1[14] = 0.6090171561770e-7;
            ARL1[15] = -0.623543724808e-8;
            ARL1[16] = 0.38430012067e-9;
            ARL1[17] = 0.790543916e-11;
            ARL1[18] = -0.489824083e-11;
            ARL1[19] = 0.46356884e-12;
            ARL1[20] = 0.684205e-14;
            ARL1[21] = -0.569748e-14;
            ARL1[22] = 0.35324e-15;
            ARL1[23] = 0.4244e-16;
            ARL1[24] = -0.644e-17;
            ARL1[25] = -0.21e-18;
            ARL1[26] = 0.9e-19;

            double[] ARL1AS = new double[17];
            ARL1AS[0] = 1.97540378441652356868;
            ARL1AS[1] = -0.1195130555088294181e-1;
            ARL1AS[2] = 0.33639485269196046e-3;
            ARL1AS[3] = -0.1009115655481549e-4;
            ARL1AS[4] = 0.30638951321998e-6;
            ARL1AS[5] = -0.953704370396e-8;
            ARL1AS[6] = 0.29524735558e-9;
            ARL1AS[7] = -0.951078318e-11;
            ARL1AS[8] = 0.28203667e-12;
            ARL1AS[9] = -0.1134175e-13;
            ARL1AS[10] = 0.147e-17;
            ARL1AS[11] = -0.6232e-16;
            ARL1AS[12] = -0.751e-17;
            ARL1AS[13] = -0.17e-18;
            ARL1AS[14] = 0.51e-18;
            ARL1AS[15] = 0.23e-18;
            ARL1AS[16] = 0.5e-19;

            double[] AI1ML1 = new double[26];
            AI1ML1[0] = 1.99679361896789136501;
            AI1ML1[1] = -0.190663261409686132e-2;
            AI1ML1[2] = -0.36094622410174481e-3;
            AI1ML1[3] = -0.6841847304599820e-4;
            AI1ML1[4] = -0.1299008228509426e-4;
            AI1ML1[5] = -0.247152188705765e-5;
            AI1ML1[6] = -0.47147839691972e-6;
            AI1ML1[7] = -0.9020819982592e-7;
            AI1ML1[8] = -0.1730458637504e-7;
            AI1ML1[9] = -0.332323670159e-8;
            AI1ML1[10] = -0.63736421735e-9;
            AI1ML1[11] = -0.12180239756e-9;
            AI1ML1[12] = -0.2317346832e-10;
            AI1ML1[13] = -0.439068833e-11;
            AI1ML1[14] = -0.82847110e-12;
            AI1ML1[15] = -0.15562249e-12;
            AI1ML1[16] = -0.2913112e-13;
            AI1ML1[17] = -0.543965e-14;
            AI1ML1[18] = -0.101177e-14;
            AI1ML1[19] = -0.18767e-15;
            AI1ML1[20] = -0.3484e-16;
            AI1ML1[21] = -0.643e-17;
            AI1ML1[22] = -0.118e-17;
            AI1ML1[23] = -0.22e-18;
            AI1ML1[24] = -0.4e-19;
            AI1ML1[25] = -0.1e-19;

            // MACHINE-DEPENDENT VALUES (Suitable for IEEE-arithmetic machines)
            const int NTERM1 = 24; const int NTERM2 = 13; const int NTERM3 = 22;
            const double XLOW1 = 5.7711949e-8; const double XLOW2 = 3.3354714e-154; const double XMAX = 1.797693e308;
            const double XHIGH1 = 5.19823025e8; const double XHIGH2 = 2.7021597e17;

            // CODE FOR |x| <= 16
            if (x <= 16.0)
            {
                if (x <= XLOW2)
                {
                    return 0.0;
                }

                double xsq = x * x;
                if (x < XLOW1)
                {
                    return xsq / PI3BY2;
                }

                double t = (4.0 * x - 24.0) / (x + 24.0);
                return xsq * Evaluate.ChebyshevSum(NTERM1, ARL1, t) * Math.Exp(x) / PI3BY2;
            }

            // CODE FOR |x| > 16
            double ch1;
            if (x > XHIGH2)
            {
                ch1 = 1.0;
            }
            else
            {
                double t = (x - 30.0) / (2.0 - x);
                ch1 = Evaluate.ChebyshevSum(NTERM2, ARL1AS, t);
            }

            double ch2;
            if (x > XHIGH1)
            {
                ch2 = 1.0;
            }
            else
            {
                double xsq = x * x;
                double t = (800.0 - xsq) / (288.0 + xsq);
                ch2 = Evaluate.ChebyshevSum(NTERM3, AI1ML1, t);
            }

            double test = Math.Log(ch1) - LNR2PI - Math.Log(x) / 2.0 + x;
            if (test > Math.Log(XMAX))
            {
                throw new ArithmeticException("ERROR IN MISCFUN FUNCTION STRVL1: ARGUMENT CAUSES OVERFLOW");
            }

            return Math.Exp(test) - TWOBPI * ch2;
        }

        /// <summary>
        /// Returns the difference between the Bessel I0 and Struve L0 functions.
        /// </summary>
        /// <param name="x">The value to compute the function of.</param>
        /// <returns></returns>
        public static double BesselI0MStruveL0(double x)
        {
            // TODO: way off for large x (e.g. 100) - needs direct approximation
            return BesselI0(x) - StruveL0(x);
        }

        /// <summary>
        /// Returns the difference between the Bessel I1 and Struve L1 functions.
        /// </summary>
        /// <param name="x">The value to compute the function of.</param>
        /// <returns></returns>
        public static double BesselI1MStruveL1(double x)
        {
            // TODO: way off for large x (e.g. 100) - needs direct approximation
            return BesselI1(x) - StruveL1(x);
        }
    }
}
