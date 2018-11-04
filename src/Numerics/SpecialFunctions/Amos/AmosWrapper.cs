using System;
using System.Numerics;

namespace MathNet.Numerics
{
    // References:
    // [1] https://github.com/scipy/scipy/blob/master/scipy/special/amos_wrappers.c
    public static partial class SpecialFunctions
    {
        private class AmosWrapper
        {
            #region AiryAi

            // Returns Ai(z)
            public Complex Cairy(Complex z)
            {
                int id = 0;
                int kode = 1;
                int nz = 0;
                int ierr = 0;

                double air = double.NaN;
                double aii = double.NaN;

                var amos = new AmosHelper();
                amos.zairy(z.Real, z.Imaginary, id, kode, ref air, ref aii, ref nz, ref ierr);
                return new Complex(air, aii);
            }

            // Returns Exp(zta) * Ai(z) where zta = (2/3) * z * Sqrt(z)
            public Complex ScaledCairy(Complex z)
            {
                int id = 0;
                int kode = 2;
                int nz = 0;
                int ierr = 0;

                double air = double.NaN;
                double aii = double.NaN;

                var amos = new AmosHelper();
                amos.zairy(z.Real, z.Imaginary, id, kode, ref air, ref aii, ref nz, ref ierr);
                return new Complex(air, aii);
            }

            // Returns Exp(zta) * Ai(z) where zta = (2/3) * z * Sqrt(z)
            public double ScaledCairy(double z)
            {
                if (z < 0)
                {
                    return double.NaN;
                }

                int id = 0;
                int kode = 2;
                int nz = 0;
                int ierr = 0;

                double air = double.NaN;
                double aii = double.NaN;

                var amos = new AmosHelper();
                amos.zairy(z, 0.0, id, kode, ref air, ref aii, ref nz, ref ierr);
                return air;
            }

            // Returns d/dz Ai(z)
            public Complex CairyPrime(Complex z)
            {
                int id = 1;
                int kode = 1;
                int nz = 0;
                int ierr = 0;

                double air = double.NaN;
                double aii = double.NaN;

                var amos = new AmosHelper();
                amos.zairy(z.Real, z.Imaginary, id, kode, ref air, ref aii, ref nz, ref ierr);
                return new Complex(air, aii);
            }

            // Returns Exp(zta) * d/dz Ai(z) where zta = (2/3) * z * Sqrt(z)
            public Complex ScaledCairyPrime(Complex z)
            {
                int id = 1;
                int kode = 2;
                int nz = 0;
                int ierr = 0;

                double air = double.NaN;
                double aii = double.NaN;

                var amos = new AmosHelper();
                amos.zairy(z.Real, z.Imaginary, id, kode, ref air, ref aii, ref nz, ref ierr);
                return new Complex(air, aii);
            }

            // Returns Exp(zta) * d/dz Ai(z) where zta = (2/3) * z * Sqrt(z)
            public double ScaledCairyPrime(double z)
            {
                if (z < 0)
                {
                    return double.NaN;
                }

                int id = 1;
                int kode = 2;
                int nz = 0;
                int ierr = 0;

                double air = double.NaN;
                double aii = double.NaN;

                var amos = new AmosHelper();
                amos.zairy(z, 0.0, id, kode, ref air, ref aii, ref nz, ref ierr);
                return air;
            }

            #endregion

            #region AiryBi

            // Returns Bi(z)
            public Complex Cbiry(Complex z)
            {
                int id = 0;
                int kode = 1;
                int nz = 0;
                int ierr = 0;

                double bir = double.NaN;
                double bii = double.NaN;

                var amos = new AmosHelper();
                amos.zbiry(z.Real, z.Imaginary, id, kode, ref bir, ref bii, ref nz, ref ierr);
                return new Complex(bir, bii);
            }

            // Returns Exp(-axzta) * Bi(z) where zta = (2 / 3) * z * Sqrt(z) and axzta = Abs(zta.Real)
            public Complex ScaledCbiry(Complex z)
            {
                int id = 0;
                int kode = 2;
                int nz = 0;
                int ierr = 0;

                double bir = double.NaN;
                double bii = double.NaN;

                var amos = new AmosHelper();
                amos.zbiry(z.Real, z.Imaginary, id, kode, ref bir, ref bii, ref nz, ref ierr);
                return new Complex(bir, bii);
            }

            // Returns d/dz Bi(z)
            public Complex CbiryPrime(Complex z)
            {
                int id = 1;
                int kode = 1;
                int nz = 0;
                int ierr = 0;

                double bipr = double.NaN;
                double bipi = double.NaN;

                var amos = new AmosHelper();
                amos.zbiry(z.Real, z.Imaginary, id, kode, ref bipr, ref bipi, ref nz, ref ierr);
                return new Complex(bipr, bipi);
            }

            // Returns Exp(-axzta) * d/dz Bi(z) where zta = (2 / 3) * z * Sqrt(z) and axzta = Abs(zta.Real)
            public Complex ScaledCbiryPrime(Complex z)
            {
                int id = 1;
                int kode = 2;
                int nz = 0;
                int ierr = 0;

                double bipr = double.NaN;
                double bipi = double.NaN;

                var amos = new AmosHelper();
                amos.zbiry(z.Real, z.Imaginary, id, kode, ref bipr, ref bipi, ref nz, ref ierr);
                return new Complex(bipr, bipi);
            }

            #endregion

            #region BesselJ

            // Return J(v, z)
            public Complex Cbesj(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN);
                }

                int sign = 1;
                if (v < 0)
                {
                    v = -v;
                    sign = -1;
                }

                int n = 1;
                int kode = 1;
                int nz = 0;
                int ierr = 0;
                double[] cyjr = new double[n];
                double[] cyji = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cyjr[i] = double.NaN;
                    cyji[i] = double.NaN;
                }

                var amos = new AmosHelper();
                amos.zbesj(z.Real, z.Imaginary, v, kode, n, cyjr, cyji, ref nz, ref ierr);
                Complex cyj = new Complex(cyjr[0], cyji[0]);

                if (ierr == 2)
                {
                    //overflow
                    cyj = ScaledCbesj(v, z);
                    cyj = new Complex(cyj.Real * double.PositiveInfinity, cyj.Imaginary * double.PositiveInfinity);
                }

                if (sign == -1)
                {
                    if (!ReflectJY(ref cyj, v))
                    {
                        double[] cyyr = new double[n];
                        double[] cyyi = new double[n];
                        double[] cwrkr = new double[n];
                        double[] cwrki = new double[n];

                        for (int i = 0; i < n; i++)
                        {
                            cyyr[i] = double.NaN;
                            cyyi[i] = double.NaN;
                            cwrkr[i] = double.NaN;
                            cwrki[i] = double.NaN;
                        }

                        amos.zbesy(z.Real, z.Imaginary, v, kode, n, cyyr, cyyi, ref nz, cwrkr, cwrki, ref ierr);
                        Complex cyy = new Complex(cyyr[0], cyyi[0]);

                        cyj = RotateJY(cyj, cyy, v);
                    }
                }
                return cyj;
            }

            // Return J(v, z)
            public double Cbesj(double v, double z)
            {
                if (z < 0 && v != (int)v)
                {
                    return double.NaN;
                }

                return Cbesj(v, new Complex(z, 0)).Real;
            }

            // Return Exp(-Abs(y)) * J(v, z) where y = z.Imaginary
            public Complex ScaledCbesj(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN);
                }

                int sign = 1;
                if (v < 0)
                {
                    v = -v;
                    sign = -1;
                }

                int n = 1;
                int kode = 2;
                int nz = 0;
                int ierr = 0;

                double[] cyjr = new double[n];
                double[] cyji = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cyjr[i] = double.NaN;
                    cyji[i] = double.NaN;
                }

                var amos = new AmosHelper();
                amos.zbesj(z.Real, z.Imaginary, v, kode, n, cyjr, cyji, ref nz, ref ierr);
                Complex cyj = new Complex(cyjr[0], cyji[0]);

                if (sign == -1)
                {
                    if (!ReflectJY(ref cyj, v))
                    {
                        double[] cyyr = new double[n];
                        double[] cyyi = new double[n];
                        double[] cworkr = new double[n];
                        double[] cworki = new double[n];
                        for (int i = 0; i < n; i++)
                        {
                            cyyr[i] = double.NaN;
                            cyyi[i] = double.NaN;
                            cworkr[i] = double.NaN;
                            cworki[i] = double.NaN;
                        }

                        amos.zbesy(z.Real, z.Imaginary, v, kode, n, cyyr, cyyi, ref nz, cworkr, cworki, ref ierr);
                        Complex cyy = new Complex(cyyr[0], cyyi[0]);

                        cyj = RotateJY(cyj, cyy, v);
                    }
                }
                return cyj;
            }

            // Return Exp(-Abs(y)) * J(v, z) where y = z.Imaginary
            public double ScaledCbesj(double v, double z)
            {
                if (z < 0 && v != (int)v)
                {
                    return double.NaN;
                }

                return ScaledCbesj(v, new Complex(z, 0)).Real;
            }

            #endregion

            #region BesselY

            // Return Y(v, z)
            public Complex Cbesy(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN);
                }

                int sign = 1;
                if (v < 0)
                {
                    v = -v;
                    sign = -1;
                }

                int n = 1;
                int kode = 1;
                int nz = 0;
                int ierr = 0;
                Complex cyy;

                var amos = new AmosHelper();
                if (z.Real == 0 && z.Imaginary == 0)
                {
                    //overflow
                    cyy = new Complex(double.NegativeInfinity, 0);
                }
                else
                {
                    double[] cyyr = new double[n];
                    double[] cyyi = new double[n];
                    double[] cworkr = new double[n];
                    double[] cworki = new double[n];
                    for (int i = 0; i < n; i++)
                    {
                        cyyr[i] = double.NaN;
                        cyyi[i] = double.NaN;
                        cworkr[i] = double.NaN;
                        cworki[i] = double.NaN;
                    }

                    amos.zbesy(z.Real, z.Imaginary, v, kode, n, cyyr, cyyi, ref nz, cworkr, cworki, ref ierr);
                    cyy = new Complex(cyyr[0], cyyi[0]);

                    if (ierr == 2)
                    {
                        if (z.Real >= 0 && z.Imaginary == 0)
                        {
                            //overflow
                            cyy = new Complex(double.NegativeInfinity, 0);
                        }
                    }
                }

                if (sign == -1)
                {
                    if (!ReflectJY(ref cyy, v))
                    {
                        double[] cyjr = new double[n];
                        double[] cyji = new double[n];
                        for (int i = 0; i < n; i++)
                        {
                            cyjr[i] = double.NaN;
                            cyji[i] = double.NaN;
                        }

                        amos.zbesj(z.Real, z.Imaginary, v, kode, n, cyjr, cyji, ref nz, ref ierr);
                        Complex cyj = new Complex(cyjr[0], cyji[0]);

                        cyy = RotateJY(cyy, cyj, -v);
                    }
                }
                return cyy;
            }

            // Return Y(v, z)
            public double Cbesy(double v, double x)
            {
                if (x < 0.0)
                {
                    return double.NaN;
                }

                Complex z = new Complex(x, 0.0);
                return Cbesy(v, z).Real;
            }

            // Return Exp(-Abs(y)) * Y(v, z) where y = z.Imaginary
            public Complex ScaledCbesy(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN);
                }

                int sign = 1;
                if (v < 0)
                {
                    v = -v;
                    sign = -1;
                }

                int n = 1;
                int kode = 2;
                int nz = 0;
                int ierr = 0;

                double[] cyyr = new double[n];
                double[] cyyi = new double[n];
                double[] cwrkr = new double[n];
                double[] cwrki = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cyyr[i] = double.NaN;
                    cyyi[i] = double.NaN;
                    cwrkr[i] = double.NaN;
                    cwrki[i] = double.NaN;
                }

                var amos = new AmosHelper();
                amos.zbesy(z.Real, z.Imaginary, v, kode, n, cyyr, cyyi, ref nz, cwrkr, cwrki, ref ierr);
                Complex cyy = new Complex(cyyr[0], cyyi[0]);

                if (ierr == 2)
                {
                    if (z.Real >= 0 && z.Imaginary == 0)
                    {
                        //overflow
                        cyy = new Complex(double.PositiveInfinity, 0);
                    }
                }

                if (sign == -1)
                {
                    if (!ReflectJY(ref cyy, v))
                    {
                        double[] cyjr = new double[n];
                        double[] cyji = new double[n];
                        for (int i = 0; i < n; i++)
                        {
                            cyjr[i] = double.NaN;
                            cyji[i] = double.NaN;
                        }

                        amos.zbesj(z.Real, z.Imaginary, v, kode, n, cyjr, cyji, ref nz, ref ierr);
                        Complex cyj = new Complex(cyjr[0], cyji[0]);
                        cyy = RotateJY(cyy, cyj, -v);
                    }
                }
                return cyy;
            }

            // Return Exp(-Abs(y)) * Y(v, z) where y = z.Imaginary
            public double ScaledCbesy(double v, double x)
            {
                if (x < 0)
                {
                    return double.NaN;
                }

                return ScaledCbesy(v, new Complex(x, 0)).Real;
            }

            #endregion

            #region BesselI

            // Returns I(v, z)
            public Complex Cbesi(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN);
                }

                int sign = 1;
                if (v < 0)
                {
                    v = -v;
                    sign = -1;
                }

                int n = 1;
                int kode = 1;
                int nz = 0;
                int ierr = 0;

                double[] cyir = new double[n];
                double[] cyii = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cyir[i] = double.NaN;
                    cyii[i] = double.NaN;
                }

                var amos = new AmosHelper();
                amos.zbesi(z.Real, z.Imaginary, v, kode, n, cyir, cyii, ref nz, ref ierr);
                Complex cyi = new Complex(cyir[0], cyii[0]);

                if (ierr == 2)
                {
                    //overflow
                    if (z.Imaginary == 0 && (z.Real >= 0 || v == Math.Floor(v)))
                    {
                        if (z.Real < 0 && v / 2 != Math.Floor(v / 2))
                            cyi = new Complex(double.NegativeInfinity, 0);
                        else
                            cyi = new Complex(double.PositiveInfinity, 0);
                    }
                    else
                    {
                        cyi = ScaledCbesi(v * sign, z);
                        cyi = new Complex(cyi.Real * double.PositiveInfinity, cyi.Imaginary * double.PositiveInfinity);
                    }
                }

                if (sign == -1)
                {
                    if (!ReflectI(cyi, v))
                    {
                        double[] cykr = new double[n];
                        double[] cyki = new double[n];
                        amos.zbesk(z.Real, z.Imaginary, v, kode, n, cykr, cyki, ref nz, ref ierr);
                        Complex cyk = new Complex(cykr[0], cyki[0]);

                        cyi = RotateI(cyi, cyk, v);
                    }
                }

                return cyi;
            }

            // Return Exp(-Abs(x)) * I(v, z) where x = z.Real
            public Complex ScaledCbesi(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN);
                }

                int sign = 1;
                if (v < 0)
                {
                    v = -v;
                    sign = -1;
                }

                int n = 1;
                int kode = 2;
                int nz = 0;
                int ierr = 0;

                double[] cyir = new double[n];
                double[] cyii = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cyir[i] = double.NaN;
                    cyii[i] = double.NaN;
                }

                var amos = new AmosHelper();
                amos.zbesi(z.Real, z.Imaginary, v, kode, n, cyir, cyii, ref nz, ref ierr);
                Complex cyi = new Complex(cyir[0], cyii[0]);

                if (sign == -1)
                {
                    if (!ReflectI(cyi, v))
                    {
                        double[] cykr = new double[n];
                        double[] cyki = new double[n];
                        amos.zbesk(z.Real, z.Imaginary, v, kode, n, cykr, cyki, ref nz, ref ierr);
                        Complex cyk = new Complex(cykr[0], cyki[0]);

                        //adjust scaling to match zbesi
                        cyk = Rotate(cyk, -z.Imaginary / Math.PI);
                        if (z.Real > 0)
                        {
                            cyk = new Complex(cyk.Real * Math.Exp(-2 * z.Real), cyk.Imaginary * Math.Exp(-2 * z.Real));
                        }
                        //v -> -v
                        cyi = RotateI(cyi, cyk, v);
                    }
                }

                return cyi;
            }

            // Return Exp(-Abs(x)) * I(v, z) where x = z.Real
            public double ScaledCbesi(double v, double x)
            {
                if (v != Math.Floor(v) && x < 0)
                {
                    return double.NaN;
                }

                return ScaledCbesi(v, new Complex(x, 0)).Real;
            }

            #endregion

            #region BesselK

            // Returns K(v, z)
            public Complex Cbesk(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN);
                }
                if (v < 0)
                {
                    //K_v == K_{-v} even for non-integer v
                    v = -v;
                }

                int n = 1;
                int kode = 1;
                int nz = 0;
                int ierr = 0;

                double[] cykr = new double[n];
                double[] cyki = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cykr[i] = double.NaN;
                    cyki[i] = double.NaN;
                }

                var amos = new AmosHelper();
                amos.zbesk(z.Real, z.Imaginary, v, kode, n, cykr, cyki, ref nz, ref ierr);
                Complex cyk = new Complex(cykr[0], cyki[0]);

                if (ierr == 1)
                {
                    if (z.Real == 0.0 && z.Imaginary == 0.0)
                    {
                        cyk = new Complex(double.PositiveInfinity, 0);
                    }
                }
                else if (ierr == 2)
                {
                    if (z.Real >= 0 && z.Imaginary == 0)
                    {
                        //overflow
                        cyk = new Complex(double.PositiveInfinity, 0);
                    }
                }

                return cyk;
            }

            // Returns K(v, z)
            public double Cbesk(double v, double z)
            {
                if (z < 0)
                {
                    return double.NaN;
                }
                else if (z == 0)
                {
                    return double.PositiveInfinity;
                }
                else if (z > 710 * (1 + Math.Abs(v)))
                {
                    // Underflow. See uniform expansion https://dlmf.nist.gov/10.41
                    // This condition is not a strict bound (it can underflow earlier),
                    // rather, we are here working around a restriction in AMOS.

                    return 0;
                }
                else
                {
                    Complex w = new Complex(z, 0);
                    return Cbesk(v, w).Real;
                }
            }

            // returns Exp(z) * K(v, z)
            public Complex ScaledCbesk(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN);
                }

                if (v < 0)
                {
                    //K_v == K_{-v} even for non-integer v
                    v = -v;
                }

                int n = 1;
                int kode = 2;
                int nz = 0;
                int ierr = 0;

                double[] cykr = new double[n];
                double[] cyki = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cykr[i] = double.NaN;
                    cyki[i] = double.NaN;
                }

                var amos = new AmosHelper();

                amos.zbesk(z.Real, z.Imaginary, v, kode, n, cykr, cyki, ref nz, ref ierr);
                Complex cyk = new Complex(cykr[0], cyki[0]);

                if (ierr == 2)
                {
                    if (z.Real >= 0 && z.Imaginary == 0)
                    {
                        //overflow
                        cyk = new Complex(double.PositiveInfinity, 0);
                    }
                }

                return cyk;
            }

            // returns Exp(z) * K(v, z)
            public double ScaledCbesk(double v, double z)
            {
                if (z < 0)
                {
                    return double.NaN;
                }
                else if (z == 0)
                {
                    return double.PositiveInfinity;
                }
                else
                {
                    Complex w = new Complex(z, 0);
                    return ScaledCbesk(v, w).Real;
                }
            }

            #endregion

            #region HankelH1

            // Returns H1(v, z)
            public Complex Cbesh1(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN); ;
                }

                int n = 1;
                int kode = 1;
                int m = 1;
                int nz = 0;
                int ierr = 0;

                double[] cyhr = new double[n];
                double[] cyhi = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cyhr[i] = double.NaN;
                    cyhi[i] = double.NaN;
                }

                int sign = 1;
                if (v < 0)
                {
                    v = -v;
                    sign = -1;
                }

                var amos = new AmosHelper();
                amos.zbesh(z.Real, z.Imaginary, v, kode, m, n, cyhr, cyhi, ref nz, ref ierr);
                Complex cyh = new Complex(cyhr[0], cyhi[0]);

                if (sign == -1)
                {
                    cyh = Rotate(cyh, v);
                }

                return cyh;
            }

            // Returns Exp(-z * j) * H1(n, z) where j = Sqrt(-1)
            public Complex ScaledCbesh1(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN); ;
                }

                int n = 1;
                int kode = 2;
                int m = 1;
                int nz = 0;
                int ierr = 0;

                double[] cyhr = new double[n];
                double[] cyhi = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cyhr[i] = double.NaN;
                    cyhi[i] = double.NaN;
                }

                int sign = 1;
                if (v < 0)
                {
                    v = -v;
                    sign = -1;
                }

                var amos = new AmosHelper();
                amos.zbesh(z.Real, z.Imaginary, v, kode, m, n, cyhr, cyhi, ref nz, ref ierr);
                Complex cyh = new Complex(cyhr[0], cyhi[0]);

                if (sign == -1)
                {
                    cyh = Rotate(cyh, v);
                }

                return cyh;
            }

            #endregion

            #region HankelH2

            // Returns H2(v, z)
            public Complex Cbesh2(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN);
                }

                if (v == 0 && z.Real == 0 && z.Imaginary == 0)
                {
                    return new Complex(double.NaN, double.NaN); // ComplexInfinity
                }

                int n = 1;
                int kode = 1;
                int m = 2;
                int nz = 0;
                int ierr = 0;

                double[] cyhr = new double[n];
                double[] cyhi = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cyhr[i] = double.NaN;
                    cyhi[i] = double.NaN;
                }

                int sign = 1;
                if (v < 0)
                {
                    v = -v;
                    sign = -1;
                }

                var amos = new AmosHelper();
                amos.zbesh(z.Real, z.Imaginary, v, kode, m, n, cyhr, cyhi, ref nz, ref ierr);
                Complex cyh = new Complex(cyhr[0], cyhi[0]);

                if (sign == -1)
                {
                    cyh = Rotate(cyh, -v);
                }
                return cyh;
            }

            // Returns Exp(z * j) * H2(n, z) where j = Sqrt(-1)
            public Complex ScaledCbesh2(double v, Complex z)
            {
                if (double.IsNaN(v) || double.IsNaN(z.Real) || double.IsNaN(z.Imaginary))
                {
                    return new Complex(double.NaN, double.NaN);
                }

                if (v == 0 && z.Real == 0 && z.Imaginary == 0)
                {
                    return new Complex(double.NaN, double.NaN); // ComplexInfinity
                }

                int n = 1;
                int kode = 2;
                int m = 2;
                int nz = 0;
                int ierr = 0;

                double[] cyhr = new double[n];
                double[] cyhi = new double[n];
                for (int i = 0; i < n; i++)
                {
                    cyhr[i] = double.NaN;
                    cyhi[i] = double.NaN;
                }

                int sign = 1;
                if (v < 0)
                {
                    v = -v;
                    sign = -1;
                }

                var amos = new AmosHelper();
                amos.zbesh(z.Real, z.Imaginary, v, kode, m, n, cyhr, cyhi, ref nz, ref ierr);
                Complex cyh = new Complex(cyhr[0], cyhi[0]);

                if (sign == -1)
                {
                    cyh = Rotate(cyh, -v);
                }
                return cyh;
            }

            #endregion

            #region utilities

            private double SinPi(double x)
            {
                if (Math.Floor(x) == x && Math.Abs(x) < 1.0e14)
                {
                    //Return 0 when at exact zero, as long as the floating point number is
                    //small enough to distinguish integer points from other points.

                    return 0;
                }
                return Math.Sin(Math.PI * x);
            }

            private double CosPi(double x)
            {
                if (Math.Floor(x + 0.5) == x + 0.5 && Math.Abs(x) < 1.0E14)
                {
                    //Return 0 when at exact zero, as long as the floating point number is
                    //small enough to distinguish integer points from other points.

                    return 0;
                }
                return Math.Cos(Math.PI * x);
            }

            private Complex Rotate(Complex z, double v)
            {
                double c = CosPi(v);
                double s = SinPi(v);
                return new Complex(z.Real * c - z.Imaginary * s, z.Real * s + z.Imaginary * c);
            }

            private Complex RotateJY(Complex j, Complex y, double v)
            {
                double c = CosPi(v);
                double s = SinPi(v);
                return new Complex(j.Real * c - y.Real * s, j.Imaginary * c - y.Imaginary * s);
            }

            private bool ReflectJY(ref Complex jy, double v)
            {
                //NB: Y_v may be huge near negative integers -- so handle exact
                //     integers carefully

                if (v != Math.Floor(v))
                {
                    return false;
                }

                int i = (int)(v - 16384.0 * Math.Floor(v / 16384.0));
                if (i % 2 == 1)
                {
                    jy = new Complex(-jy.Real, -jy.Imaginary);
                }

                return true;
            }

            private bool ReflectI(Complex ik, double v)
            {
                if (v != Math.Floor(v))
                {
                    return false;
                }

                return true; //I is symmetric for integer v
            }

            private Complex RotateI(Complex i, Complex k, double v)
            {
                double s = Math.Sin(v * Math.PI) * (2.0 / Math.PI);
                return new Complex(i.Real + s * k.Real, i.Imaginary + s * k.Imaginary);
            }

            #endregion
        }
    }
}
