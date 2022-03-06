using System;

namespace MathNet.Numerics
{
    public static partial class SpecialFunctions
    {
        // Translated from AMOS fortran codes by hand
        //
        // References:
        // [1] Amos package in netlib. http://www.netlib.org/amos
        static class AmosHelper
        {
            #region Bessel- related functions

            // The Airy function Ai(z) and derivative
            public static int zairy(double zr, double zi, int id, int kode, ref double air, ref double aii, ref int nz, ref int ierr)
            {
                #region Description

                //***BEGIN PROLOGUE  ZAIRY
                //***DATE WRITTEN   830501   (YYMMDD)
                //***REVISION DATE  890801   (YYMMDD)
                //***CATEGORY NO.  B5K
                //***KEYWORDS  AIRY FUNCTION,BESSEL FUNCTIONS OF ORDER ONE THIRD
                //***AUTHOR  AMOS, DONALD E., SANDIA NATIONAL LABORATORIES
                //***PURPOSE  TO COMPUTE AIRY FUNCTIONS AI(Z) AND DAI(Z) FOR COMPLEX Z
                //***DESCRIPTION
                //
                //                      ***A DOUBLE PRECISION ROUTINE***
                //         ON KODE=1, ZAIRY COMPUTES THE COMPLEX AIRY FUNCTION AI(Z) OR
                //         ITS DERIVATIVE DAI(Z)/DZ ON ID=0 OR ID=1 RESPECTIVELY. ON
                //         KODE=2, A SCALING OPTION CEXP(ZTA)*AI(Z) OR CEXP(ZTA)*
                //         DAI(Z)/DZ IS PROVIDED TO REMOVE THE EXPONENTIAL DECAY IN
                //         -PI/3.LT.ARG(Z).LT.PI/3 AND THE EXPONENTIAL GROWTH IN
                //         PI/3.LT.ABS(ARG(Z)).LT.PI WHERE ZTA=(2/3)*Z*CSQRT(Z).
                //
                //         WHILE THE AIRY FUNCTIONS AI(Z) AND DAI(Z)/DZ ARE ANALYTIC IN
                //         THE WHOLE Z PLANE, THE CORRESPONDING SCALED FUNCTIONS DEFINED
                //         FOR KODE=2 HAVE A CUT ALONG THE NEGATIVE REAL AXIS.
                //         DEFINTIONS AND NOTATION ARE FOUND IN THE NBS HANDBOOK OF
                //         MATHEMATICAL FUNCTIONS (REF. 1).
                //
                //         INPUT      ZR,ZI ARE DOUBLE PRECISION
                //           ZR,ZI  - Z=CMPLX(ZR,ZI)
                //           ID     - ORDER OF DERIVATIVE, ID=0 OR ID=1
                //           KODE   - A PARAMETER TO INDICATE THE SCALING OPTION
                //                    KODE= 1  RETURNS
                //                             AI=AI(Z)                ON ID=0 OR
                //                             AI=DAI(Z)/DZ            ON ID=1
                //                        = 2  RETURNS
                //                             AI=CEXP(ZTA)*AI(Z)       ON ID=0 OR
                //                             AI=CEXP(ZTA)*DAI(Z)/DZ   ON ID=1 WHERE
                //                             ZTA=(2/3)*Z*CSQRT(Z)
                //
                //         OUTPUT     AIR,AII ARE DOUBLE PRECISION
                //           AIR,AII- COMPLEX ANSWER DEPENDING ON THE CHOICES FOR ID AND
                //                    KODE
                //           NZ     - UNDERFLOW INDICATOR
                //                    NZ= 0   , NORMAL RETURN
                //                    NZ= 1   , AI=CMPLX(0.0D0,0.0D0) DUE TO UNDERFLOW IN
                //                              -PI/3.LT.ARG(Z).LT.PI/3 ON KODE=1
                //           IERR   - ERROR FLAG
                //                    IERR=0, NORMAL RETURN - COMPUTATION COMPLETED
                //                    IERR=1, INPUT ERROR   - NO COMPUTATION
                //                    IERR=2, OVERFLOW      - NO COMPUTATION, REAL(ZTA)
                //                            TOO LARGE ON KODE=1
                //                    IERR=3, CABS(Z) LARGE      - COMPUTATION COMPLETED
                //                            LOSSES OF SIGNIFCANCE BY ARGUMENT REDUCTION
                //                            PRODUCE LESS THAN HALF OF MACHINE ACCURACY
                //                    IERR=4, CABS(Z) TOO LARGE  - NO COMPUTATION
                //                            COMPLETE LOSS OF ACCURACY BY ARGUMENT
                //                            REDUCTION
                //                    IERR=5, ERROR              - NO COMPUTATION,
                //                            ALGORITHM TERMINATION CONDITION NOT MET
                //
                //***LONG DESCRIPTION
                //
                //         AI AND DAI ARE COMPUTED FOR CABS(Z).GT.1.0 FROM THE K BESSEL
                //         FUNCTIONS BY
                //
                //            AI(Z)=C*SQRT(Z)*K(1/3,ZTA) , DAI(Z)=-C*Z*K(2/3,ZTA)
                //                           C=1.0/(PI*SQRT(3.0))
                //                            ZTA=(2/3)*Z**(3/2)
                //
                //         WITH THE POWER SERIES FOR CABS(Z).LE.1.0.
                //
                //         IN MOST COMPLEX VARIABLE COMPUTATION, ONE MUST EVALUATE ELE-
                //         MENTARY FUNCTIONS. WHEN THE MAGNITUDE OF Z IS LARGE, LOSSES
                //         OF SIGNIFICANCE BY ARGUMENT REDUCTION OCCUR. CONSEQUENTLY, IF
                //         THE MAGNITUDE OF ZETA=(2/3)*Z**1.5 EXCEEDS U1=SQRT(0.5/UR),
                //         THEN LOSSES EXCEEDING HALF PRECISION ARE LIKELY AND AN ERROR
                //         FLAG IERR=3 IS TRIGGERED WHERE UR=DMAX1(D1MACH(4),1.0D-18) IS
                //         DOUBLE PRECISION UNIT ROUNDOFF LIMITED TO 18 DIGITS PRECISION.
                //         ALSO, IF THE MAGNITUDE OF ZETA IS LARGER THAN U2=0.5/UR, THEN
                //         ALL SIGNIFICANCE IS LOST AND IERR=4. IN ORDER TO USE THE INT
                //         FUNCTION, ZETA MUST BE FURTHER RESTRICTED NOT TO EXCEED THE
                //         LARGEST INTEGER, U3=I1MACH(9). THUS, THE MAGNITUDE OF ZETA
                //         MUST BE RESTRICTED BY MIN(U2,U3). ON 32 BIT MACHINES, U1,U2,
                //         AND U3 ARE APPROXIMATELY 2.0E+3, 4.2E+6, 2.1E+9 IN SINGLE
                //         PRECISION ARITHMETIC AND 1.3E+8, 1.8E+16, 2.1E+9 IN DOUBLE
                //         PRECISION ARITHMETIC RESPECTIVELY. THIS MAKES U2 AND U3 LIMIT-
                //         ING IN THEIR RESPECTIVE ARITHMETICS. THIS MEANS THAT THE MAG-
                //         NITUDE OF Z CANNOT EXCEED 3.1E+4 IN SINGLE AND 2.1E+6 IN
                //         DOUBLE PRECISION ARITHMETIC. THIS ALSO MEANS THAT ONE CAN
                //         EXPECT TO RETAIN, IN THE WORST CASES ON 32 BIT MACHINES,
                //         NO DIGITS IN SINGLE PRECISION AND ONLY 7 DIGITS IN DOUBLE
                //         PRECISION ARITHMETIC. SIMILAR CONSIDERATIONS HOLD FOR OTHER
                //         MACHINES.
                //
                //         THE APPROXIMATE RELATIVE ERROR IN THE MAGNITUDE OF A COMPLEX
                //         BESSEL FUNCTION CAN BE EXPRESSED BY P*10**S WHERE P=MAX(UNIT
                //         ROUNDOFF,1.0E-18) IS THE NOMINAL PRECISION AND 10**S REPRE-
                //         SENTS THE INCREASE IN ERROR DUE TO ARGUMENT REDUCTION IN THE
                //         ELEMENTARY FUNCTIONS. HERE, S=MAX(1,ABS(LOG10(CABS(Z))),
                //         ABS(LOG10(FNU))) APPROXIMATELY (I.E. S=MAX(1,ABS(EXPONENT OF
                //         CABS(Z),ABS(EXPONENT OF FNU)) ). HOWEVER, THE PHASE ANGLE MAY
                //         HAVE ONLY ABSOLUTE ACCURACY. THIS IS MOST LIKELY TO OCCUR WHEN
                //         ONE COMPONENT (IN ABSOLUTE VALUE) IS LARGER THAN THE OTHER BY
                //         SEVERAL ORDERS OF MAGNITUDE. IF ONE COMPONENT IS 10**K LARGER
                //         THAN THE OTHER, THEN ONE CAN EXPECT ONLY MAX(ABS(LOG10(P))-K,
                //         0) SIGNIFICANT DIGITS; OR, STATED ANOTHER WAY, WHEN K EXCEEDS
                //         THE EXPONENT OF P, NO SIGNIFICANT DIGITS REMAIN IN THE SMALLER
                //         COMPONENT. HOWEVER, THE PHASE ANGLE RETAINS ABSOLUTE ACCURACY
                //         BECAUSE, IN COMPLEX ARITHMETIC WITH PRECISION P, THE SMALLER
                //         COMPONENT WILL NOT (AS A RULE) DECREASE BELOW P TIMES THE
                //         MAGNITUDE OF THE LARGER COMPONENT. IN THESE EXTREME CASES,
                //         THE PRINCIPAL PHASE ANGLE IS ON THE ORDER OF +P, -P, PI/2-P,
                //         OR -PI/2+P.
                //
                //***REFERENCES  HANDBOOK OF MATHEMATICAL FUNCTIONS BY M. ABRAMOWITZ
                //                 AND I. A. STEGUN, NBS AMS SERIES 55, U.S. DEPT. OF
                //                 COMMERCE, 1955.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 AND LARGE ORDER BY D. E. AMOS, SAND83-0643, MAY, 1983
                //
                //               A SUBROUTINE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, SAND85-
                //                 1018, MAY, 1985
                //
                //               A PORTABLE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, TRANS.
                //                 MATH. SOFTWARE, 1986
                //
                //***ROUTINES CALLED  ZACAI,ZBKNU,ZEXP,ZSQRT,I1MACH,D1MACH
                //***END PROLOGUE  ZAIRY

                #endregion

                const double tth = 0.666666666666666667;
                const double c1 = 0.35502805388781724;
                const double c2 = 0.258819403792806799;
                const double coef = 0.183776298473930683;
                const double zeror = 0.0;
                const double zeroi = 0.0;
                const double coner = 1.0;
                const double conei = 0.0;

                double aa, ad, ak, alim, atrm, az, az3, bk;
                double cc, ck, csqi = 0, csqr = 0, dig;
                double dk, d1, d2, elim, fid, fnu, ptr, rl, r1m5, sfac, sti = 0, str = 0;
                double s1i, s1r, s2i, s2r, tol, trm1i, trm1r, trm2i, trm2r;
                double ztai, ztar, z3i, z3r, alaz, bb;
                int iflag, k, k1, k2, mr, nn = 0;

                double[] cyi = new double[1];
                double[] cyr = new double[1];

                air = 0;
                aii = 0;
                ierr = 0;
                nz = 0;
                if (id < 0 || id > 1) ierr = 1;
                if (kode < 1 || kode > 2) ierr = 1;
                if (ierr != 0) return 0;
                az = zabs(zr, zi);
                tol = Math.Max(d1mach(4), 1.0E-18);
                fid = (double)id;
                if (az > 1.0) goto L70;
                // -----------------------------------------------------------------------
                //     POWER SERIES FOR ABS(Z).LE.1.
                // -----------------------------------------------------------------------
                s1r = coner;
                s1i = conei;
                s2r = coner;
                s2i = conei;
                if (az < tol) goto L170;
                aa = az * az;
                if (aa < tol / az) goto L40;
                trm1r = coner;
                trm1i = conei;
                trm2r = coner;
                trm2i = conei;
                atrm = 1.0;
                str = zr * zr - zi * zi;
                sti = zr * zi + zi * zr;
                z3r = str * zr - sti * zi;
                z3i = str * zi + sti * zr;
                az3 = az * aa;
                ak = 2.0 + fid;
                bk = 3.0 - fid - fid;
                ck = 4.0 - fid;
                dk = 3.0 + fid + fid;
                d1 = ak * dk;
                d2 = bk * ck;
                ad = Math.Min(d1, d2);
                ak = 24.0 + 9.0 * fid;
                bk = 30.0 - 9.0 * fid;
                for (k = 1; k <= 25; k++)
                {
                    str = (trm1r * z3r - trm1i * z3i) / d1;
                    trm1i = (trm1r * z3i + trm1i * z3r) / d1;
                    trm1r = str;
                    s1r += trm1r;
                    s1i += trm1i;
                    str = (trm2r * z3r - trm2i * z3i) / d2;
                    trm2i = (trm2r * z3i + trm2i * z3r) / d2;
                    trm2r = str;
                    s2r += trm2r;
                    s2i += trm2i;
                    atrm = atrm * az3 / ad;
                    d1 += ak;
                    d2 += bk;
                    ad = Math.Min(d1, d2);
                    if (atrm < tol * ad) goto L40;
                    ak += 18.0;
                    bk += 18.0;
                }
            L40:
                if (id == 1) goto L50;
                air = s1r * c1 - c2 * (zr * s2r - zi * s2i);
                aii = s1i * c1 - c2 * (zr * s2i + zi * s2r);
                if (kode == 1) return 0;
                zsqrt(zr, zi, ref str, ref sti);
                ztar = tth * (zr * str - zi * sti);
                ztai = tth * (zr * sti + zi * str);
                zexp(ztar, ztai, ref str, ref sti);
                ptr = air * str - aii * sti;
                aii = air * sti + aii * str;
                air = ptr;
                return 0;
            L50:
                air = -s2r * c2;
                aii = -s2i * c2;
                if (az <= tol) goto L60;
                str = zr * s1r - zi * s1i;
                sti = zr * s1i + zi * s1r;
                cc = c1 / (fid + 1.0);
                air += cc * (str * zr - sti * zi);
                aii += cc * (str * zi + sti * zr);
            L60:
                if (kode == 1) return 0;
                zsqrt(zr, zi, ref str, ref sti);
                ztar = tth * (zr * str - zi * sti);
                ztai = tth * (zr * sti + zi * str);
                zexp(ztar, ztai, ref str, ref sti);
                ptr = str * air - sti * aii;
                aii = str * aii + sti * air;
                air = ptr;
                return 0;
                // -----------------------------------------------------------------------
                //     CASE FOR ABS(Z).GT.1.0
                // -----------------------------------------------------------------------
            L70:
                fnu = (1.0 + fid) / 3.0;
                // -----------------------------------------------------------------------
                //     SET PARAMETERS RELATED TO MACHINE CONSTANTS.
                //     TOL IS THE APPROXIMATE UNIT ROUNDOFF LIMITED TO 1.0D-18.
                //     ELIM IS THE APPROXIMATE EXPONENTIAL OVER- AND UNDERFLOW LIMIT.
                //     EXP(-ELIM).LT.EXP(-ALIM)=EXP(-ELIM)/TOL    AND
                //     EXP(ELIM).GT.EXP(ALIM)=EXP(ELIM)*TOL       ARE INTERVALS NEAR
                //     UNDERFLOW AND OVERFLOW LIMITS WHERE SCALED ARITHMETIC IS DONE.
                //     RL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC EXPANSION FOR LARGE Z.
                //     DIG = NUMBER OF BASE 10 DIGITS IN TOL = 10**(-DIG).
                // -----------------------------------------------------------------------
                k1 = i1mach(15);
                k2 = i1mach(16);
                r1m5 = d1mach(5);
                k = Math.Min(Math.Abs(k1), Math.Abs(k2));
                elim = (k * r1m5 - 3.0) * 2.303;
                k1 = i1mach(14) - 1;
                aa = r1m5 * k1;
                dig = Math.Min(aa, 18.0);
                aa *= 2.303;
                alim = elim + Math.Max(-aa, -41.45);
                rl = 1.2 * dig + 3.0;
                alaz = Math.Log(az);
                // -----------------------------------------------------------------------
                //     TEST FOR PROPER RANGE
                // -----------------------------------------------------------------------
                aa = 0.5 / tol;
                bb = i1mach(9) * 0.5;
                aa = Math.Min(aa, bb);
                aa = Math.Pow(aa, tth);
                if (az > aa) goto L260;
                aa = Math.Sqrt(aa);
                if (az > aa) ierr = 3;
                zsqrt(zr, zi, ref csqr, ref csqi);
                ztar = tth * (zr * csqr - zi * csqi);
                ztai = tth * (zr * csqi + zi * csqr);
                // -----------------------------------------------------------------------
                //     RE(ZTA).LE.0 WHEN RE(Z).LT.0, ESPECIALLY WHEN IM(Z) IS SMALL
                // -----------------------------------------------------------------------
                iflag = 0;
                sfac = 1.0;
                ak = ztai;
                if (zr >= 0.0) goto L80;
                bk = ztar;
                ck = -Math.Abs(bk);
                ztar = ck;
                ztai = ak;
            L80:
                if (zi != 0.0) goto L90;
                if (zr > 0.0) goto L90;
                ztar = 0.0;
                ztai = ak;
            L90:
                aa = ztar;
                if (aa >= 0.0 && zr > 0.0) goto L110;
                if (kode == 2) goto L100;
                // -----------------------------------------------------------------------
                //     OVERFLOW TEST
                // -----------------------------------------------------------------------
                if (aa > -alim) goto L100;
                aa = -aa + alaz * 0.25;
                iflag = 1;
                sfac = tol;
                if (aa > elim) goto L270;
            L100:
                // -----------------------------------------------------------------------
                //     CBKNU AND CACON RETURN EXP(ZTA)*K(FNU,ZTA) ON KODE=2
                // -----------------------------------------------------------------------
                mr = 1;
                if (zi < 0.0) mr = -1;
                zacai(ztar, ztai, fnu, kode, mr, 1, cyr, cyi, ref nn, rl, tol, elim, alim);
                if (nn < 0) goto L280;
                nz += nn;
                goto L130;
            L110:
                if (kode == 2) goto L120;
                // -----------------------------------------------------------------------
                //     UNDERFLOW TEST
                // -----------------------------------------------------------------------
                if (aa < alim) goto L120;
                aa = -aa - 0.25 * alaz;
                iflag = 2;
                sfac = 1.0 / tol;
                if (aa < -elim) goto L210;
            L120:
                zbknu(ztar, ztai, fnu, kode, 1, cyr, cyi, ref nz, tol, elim, alim);
            L130:
                s1r = cyr[0] * coef;
                s1i = cyi[0] * coef;
                if (iflag != 0) goto L150;
                if (id == 1) goto L140;
                air = csqr * s1r - csqi * s1i;
                aii = csqr * s1i + csqi * s1r;
                return 0;
            L140:
                air = -(zr * s1r - zi * s1i);
                aii = -(zr * s1i + zi * s1r);
                return 0;
            L150:
                s1r *= sfac;
                s1i *= sfac;
                if (id == 1) goto L160;
                str = s1r * csqr - s1i * csqi;
                s1i = s1r * csqi + s1i * csqr;
                s1r = str;
                air = s1r / sfac;
                aii = s1i / sfac;
                return 0;
            L160:
                str = -(s1r * zr - s1i * zi);
                s1i = -(s1r * zi + s1i * zr);
                s1r = str;
                air = s1r / sfac;
                aii = s1i / sfac;
                return 0;
            L170:
                aa = 1.0E3 * d1mach(1);
                s1r = zeror;
                s1i = zeroi;
                if (id == 1) goto L190;
                if (az <= aa) goto L180;
                s1r = c2 * zr;
                s1i = c2 * zi;
            L180:
                air = c1 - s1r;
                aii = -s1i;
                return 0;
            L190:
                air = -c2;
                aii = 0.0;
                aa = Math.Sqrt(aa);
                if (az <= aa) goto L200;
                s1r = (zr * zr - zi * zi) * 0.5;
                s1i = zr * zi;
            L200:
                air += c1 * s1r;
                aii += c1 * s1i;
                return 0;
            L210:
                nz = 1;
                air = zeror;
                aii = zeroi;
                return 0;
            L270:
                nz = 0;
                ierr = 2;
                return 0;
            L280:
                if (nn == -1) goto L270;
                nz = 0;
                ierr = 5;
                return 0;
            L260:
                ierr = 4;
                nz = 0;
                return 0;
            }

            // The Airy function Bi(z) and derivative
            public static int zbiry(double zr, double zi, int id, int kode, ref double bir, ref double bii, ref int nz, ref int ierr)
            {
                #region Description

                //***BEGIN PROLOGUE  ZBIRY
                //***DATE WRITTEN   830501   (YYMMDD)
                //***REVISION DATE  890801   (YYMMDD)
                //***CATEGORY NO.  B5K
                //***KEYWORDS  AIRY FUNCTION,BESSEL FUNCTIONS OF ORDER ONE THIRD
                //***AUTHOR  AMOS, DONALD E., SANDIA NATIONAL LABORATORIES
                //***PURPOSE  TO COMPUTE AIRY FUNCTIONS BI(Z) AND DBI(Z) FOR COMPLEX Z
                //***DESCRIPTION
                //
                //                      ***A DOUBLE PRECISION ROUTINE***
                //         ON KODE=1, CBIRY COMPUTES THE COMPLEX AIRY FUNCTION BI(Z) OR
                //         ITS DERIVATIVE DBI(Z)/DZ ON ID=0 OR ID=1 RESPECTIVELY. ON
                //         KODE=2, A SCALING OPTION CEXP(-AXZTA)*BI(Z) OR CEXP(-AXZTA)*
                //         DBI(Z)/DZ IS PROVIDED TO REMOVE THE EXPONENTIAL BEHAVIOR IN
                //         BOTH THE LEFT AND RIGHT HALF PLANES WHERE
                //         ZTA=(2/3)*Z*CSQRT(Z)=CMPLX(XZTA,YZTA) AND AXZTA=ABS(XZTA).
                //         DEFINTIONS AND NOTATION ARE FOUND IN THE NBS HANDBOOK OF
                //         MATHEMATICAL FUNCTIONS (REF. 1).
                //
                //         INPUT      ZR,ZI ARE DOUBLE PRECISION
                //           ZR,ZI  - Z=CMPLX(ZR,ZI)
                //           ID     - ORDER OF DERIVATIVE, ID=0 OR ID=1
                //           KODE   - A PARAMETER TO INDICATE THE SCALING OPTION
                //                    KODE= 1  RETURNS
                //                             BI=BI(Z)                 ON ID=0 OR
                //                             BI=DBI(Z)/DZ             ON ID=1
                //                        = 2  RETURNS
                //                             BI=CEXP(-AXZTA)*BI(Z)     ON ID=0 OR
                //                             BI=CEXP(-AXZTA)*DBI(Z)/DZ ON ID=1 WHERE
                //                             ZTA=(2/3)*Z*CSQRT(Z)=CMPLX(XZTA,YZTA)
                //                             AND AXZTA=ABS(XZTA)
                //
                //         OUTPUT     BIR,BII ARE DOUBLE PRECISION
                //           BIR,BII- COMPLEX ANSWER DEPENDING ON THE CHOICES FOR ID AND
                //                    KODE
                //           IERR   - ERROR FLAG
                //                    IERR=0, NORMAL RETURN - COMPUTATION COMPLETED
                //                    IERR=1, INPUT ERROR   - NO COMPUTATION
                //                    IERR=2, OVERFLOW      - NO COMPUTATION, REAL(Z)
                //                            TOO LARGE ON KODE=1
                //                    IERR=3, CABS(Z) LARGE      - COMPUTATION COMPLETED
                //                            LOSSES OF SIGNIFCANCE BY ARGUMENT REDUCTION
                //                            PRODUCE LESS THAN HALF OF MACHINE ACCURACY
                //                    IERR=4, CABS(Z) TOO LARGE  - NO COMPUTATION
                //                            COMPLETE LOSS OF ACCURACY BY ARGUMENT
                //                            REDUCTION
                //                    IERR=5, ERROR              - NO COMPUTATION,
                //                            ALGORITHM TERMINATION CONDITION NOT MET
                //
                //***LONG DESCRIPTION
                //
                //         BI AND DBI ARE COMPUTED FOR CABS(Z).GT.1.0 FROM THE I BESSEL
                //         FUNCTIONS BY
                //
                //                BI(Z)=C*SQRT(Z)*( I(-1/3,ZTA) + I(1/3,ZTA) )
                //               DBI(Z)=C *  Z  * ( I(-2/3,ZTA) + I(2/3,ZTA) )
                //                               C=1.0/SQRT(3.0)
                //                             ZTA=(2/3)*Z**(3/2)
                //
                //         WITH THE POWER SERIES FOR CABS(Z).LE.1.0.
                //
                //         IN MOST COMPLEX VARIABLE COMPUTATION, ONE MUST EVALUATE ELE-
                //         MENTARY FUNCTIONS. WHEN THE MAGNITUDE OF Z IS LARGE, LOSSES
                //         OF SIGNIFICANCE BY ARGUMENT REDUCTION OCCUR. CONSEQUENTLY, IF
                //         THE MAGNITUDE OF ZETA=(2/3)*Z**1.5 EXCEEDS U1=SQRT(0.5/UR),
                //         THEN LOSSES EXCEEDING HALF PRECISION ARE LIKELY AND AN ERROR
                //         FLAG IERR=3 IS TRIGGERED WHERE UR=DMAX1(D1MACH(4),1.0D-18) IS
                //         DOUBLE PRECISION UNIT ROUNDOFF LIMITED TO 18 DIGITS PRECISION.
                //         ALSO, IF THE MAGNITUDE OF ZETA IS LARGER THAN U2=0.5/UR, THEN
                //         ALL SIGNIFICANCE IS LOST AND IERR=4. IN ORDER TO USE THE INT
                //         FUNCTION, ZETA MUST BE FURTHER RESTRICTED NOT TO EXCEED THE
                //         LARGEST INTEGER, U3=I1MACH(9). THUS, THE MAGNITUDE OF ZETA
                //         MUST BE RESTRICTED BY MIN(U2,U3). ON 32 BIT MACHINES, U1,U2,
                //         AND U3 ARE APPROXIMATELY 2.0E+3, 4.2E+6, 2.1E+9 IN SINGLE
                //         PRECISION ARITHMETIC AND 1.3E+8, 1.8E+16, 2.1E+9 IN DOUBLE
                //         PRECISION ARITHMETIC RESPECTIVELY. THIS MAKES U2 AND U3 LIMIT-
                //         ING IN THEIR RESPECTIVE ARITHMETICS. THIS MEANS THAT THE MAG-
                //         NITUDE OF Z CANNOT EXCEED 3.1E+4 IN SINGLE AND 2.1E+6 IN
                //         DOUBLE PRECISION ARITHMETIC. THIS ALSO MEANS THAT ONE CAN
                //         EXPECT TO RETAIN, IN THE WORST CASES ON 32 BIT MACHINES,
                //         NO DIGITS IN SINGLE PRECISION AND ONLY 7 DIGITS IN DOUBLE
                //         PRECISION ARITHMETIC. SIMILAR CONSIDERATIONS HOLD FOR OTHER
                //         MACHINES.
                //
                //         THE APPROXIMATE RELATIVE ERROR IN THE MAGNITUDE OF A COMPLEX
                //         BESSEL FUNCTION CAN BE EXPRESSED BY P*10**S WHERE P=MAX(UNIT
                //         ROUNDOFF,1.0E-18) IS THE NOMINAL PRECISION AND 10**S REPRE-
                //         SENTS THE INCREASE IN ERROR DUE TO ARGUMENT REDUCTION IN THE
                //         ELEMENTARY FUNCTIONS. HERE, S=MAX(1,ABS(LOG10(CABS(Z))),
                //         ABS(LOG10(FNU))) APPROXIMATELY (I.E. S=MAX(1,ABS(EXPONENT OF
                //         CABS(Z),ABS(EXPONENT OF FNU)) ). HOWEVER, THE PHASE ANGLE MAY
                //         HAVE ONLY ABSOLUTE ACCURACY. THIS IS MOST LIKELY TO OCCUR WHEN
                //         ONE COMPONENT (IN ABSOLUTE VALUE) IS LARGER THAN THE OTHER BY
                //         SEVERAL ORDERS OF MAGNITUDE. IF ONE COMPONENT IS 10**K LARGER
                //         THAN THE OTHER, THEN ONE CAN EXPECT ONLY MAX(ABS(LOG10(P))-K,
                //         0) SIGNIFICANT DIGITS; OR, STATED ANOTHER WAY, WHEN K EXCEEDS
                //         THE EXPONENT OF P, NO SIGNIFICANT DIGITS REMAIN IN THE SMALLER
                //         COMPONENT. HOWEVER, THE PHASE ANGLE RETAINS ABSOLUTE ACCURACY
                //         BECAUSE, IN COMPLEX ARITHMETIC WITH PRECISION P, THE SMALLER
                //         COMPONENT WILL NOT (AS A RULE) DECREASE BELOW P TIMES THE
                //         MAGNITUDE OF THE LARGER COMPONENT. IN THESE EXTREME CASES,
                //         THE PRINCIPAL PHASE ANGLE IS ON THE ORDER OF +P, -P, PI/2-P,
                //         OR -PI/2+P.
                //
                //***REFERENCES  HANDBOOK OF MATHEMATICAL FUNCTIONS BY M. ABRAMOWITZ
                //                 AND I. A. STEGUN, NBS AMS SERIES 55, U.S. DEPT. OF
                //                 COMMERCE, 1955.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 AND LARGE ORDER BY D. E. AMOS, SAND83-0643, MAY, 1983
                //
                //               A SUBROUTINE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, SAND85-
                //                 1018, MAY, 1985
                //
                //               A PORTABLE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, TRANS.
                //                 MATH. SOFTWARE, 1986
                //
                //***ROUTINES CALLED  ZBINU,ZABS,ZDIV,ZSQRT,D1MACH,I1MACH
                //***END PROLOGUE  ZBIRY

                #endregion

                const double coef = 5.77350269189625765E-01;
                const double coner = 1.0;
                const double conei = 0.0;
                const double c1 = 6.14926627446000736E-01;
                const double c2 = 4.48288357353826359E-01;
                const double pi = 3.14159265358979323846264338327950;
                const double tth = 6.66666666666666667E-01;

                double aa, ad, ak, alim, atrm, az, az3, bb;
                double bk, cc, ck, csqi = 0, csqr = 0;
                double dig, dk, d1, d2, eaa, elim, fid, fmr, fnu, fnul, rl, r1m5;
                double sfac, sti = 0, str = 0, s1i, s1r, s2i, s2r, tol, trm1i, trm1r, trm2i;
                double trm2r, ztai, ztar, z3i, z3r;
                int k, k1, k2;

                double[] cyr = new double[2];
                double[] cyi = new double[2];

                ierr = 0;
                nz = 0;
                if (id < 0 || id > 1) ierr = 1;
                if (kode < 1 || kode > 2) ierr = 1;
                if (ierr != 0) return 0;
                az = zabs(zr, zi);
                tol = Math.Max(d1mach(4), 1.0E-18);
                fid = (double)id;
                if (az > 1.0) goto L70;
                //-----------------------------------------------------------------------
                //     POWER SERIES FOR CABS(Z).LE.1.
                //-----------------------------------------------------------------------
                s1r = coner;
                s1i = conei;
                s2r = coner;
                s2i = conei;
                if (az < tol) goto L130;
                aa = az * az;
                if (aa < tol / az) goto L40;
                trm1r = coner;
                trm1i = conei;
                trm2r = coner;
                trm2i = conei;
                atrm = 1.0;
                str = zr * zr - zi * zi;
                sti = zr * zi + zi * zr;
                z3r = str * zr - sti * zi;
                z3i = str * zi + sti * zr;
                az3 = az * aa;
                ak = 2.0 + fid;
                bk = 3.0 - fid - fid;
                ck = 4.0 - fid;
                dk = 3.0 + fid + fid;
                d1 = ak * dk;
                d2 = bk * ck;
                ad = Math.Min(d1, d2);
                ak = 24.0 + 9.0 * fid;
                bk = 30.0 - 9.0 * fid;
                for (k = 1; k <= 25; k++)
                {
                    str = (trm1r * z3r - trm1i * z3i) / d1;
                    trm1i = (trm1r * z3i + trm1i * z3r) / d1;
                    trm1r = str;
                    s1r = s1r + trm1r;
                    s1i = s1i + trm1i;
                    str = (trm2r * z3r - trm2i * z3i) / d2;
                    trm2i = (trm2r * z3i + trm2i * z3r) / d2;
                    trm2r = str;
                    s2r = s2r + trm2r;
                    s2i = s2i + trm2i;
                    atrm = atrm * az3 / ad;
                    d1 = d1 + ak;
                    d2 = d2 + bk;
                    ad = Math.Min(d1, d2);
                    if (atrm < tol * ad) goto L40;
                    ak = ak + 18.0;
                    bk = bk + 18.0;
                }
            L40:
                if (id == 1) goto L50;
                bir = c1 * s1r + c2 * (zr * s2r - zi * s2i);
                bii = c1 * s1i + c2 * (zr * s2i + zi * s2r);
                if (kode == 1) return 0;
                zsqrt(zr, zi, ref str, ref sti);
                ztar = tth * (zr * str - zi * sti);
                ztai = tth * (zr * sti + zi * str);
                aa = ztar;
                aa = -Math.Abs(aa);
                eaa = Math.Exp(aa);
                bir = bir * eaa;
                bii = bii * eaa;
                return 0;
            L50:
                bir = s2r * c2;
                bii = s2i * c2;
                if (az <= tol) goto L60;
                cc = c1 / (1.0 + fid);
                str = s1r * zr - s1i * zi;
                sti = s1r * zi + s1i * zr;
                bir = bir + cc * (str * zr - sti * zi);
                bii = bii + cc * (str * zi + sti * zr);
            L60:
                if (kode == 1) return 0;
                zsqrt(zr, zi, ref str, ref sti);
                ztar = tth * (zr * str - zi * sti);
                ztai = tth * (zr * sti + zi * str);
                aa = ztar;
                aa = -Math.Abs(aa);
                eaa = Math.Exp(aa);
                bir = bir * eaa;
                bii = bii * eaa;
                return 0;
                //-----------------------------------------------------------------------
                //     CASE FOR CABS(Z).GT.1.0
                //-----------------------------------------------------------------------
            L70:
                fnu = (1.0 + fid) / 3.0;
                //-----------------------------------------------------------------------
                //     SET PARAMETERS RELATED TO MACHINE CONSTANTS.
                //     TOL IS THE APPROXIMATE UNIT ROUNDOFF LIMITED TO 1.0E-18.
                //     ELIM IS THE APPROXIMATE EXPONENTIAL OVER- AND UNDERFLOW LIMIT.
                //     EXP(-ELIM).LT.EXP(-ALIM)=EXP(-ELIM)/TOL    AND
                //     EXP(ELIM).GT.EXP(ALIM)=EXP(ELIM)*TOL       ARE INTERVALS NEAR
                //     UNDERFLOW AND OVERFLOW LIMITS WHERE SCALED ARITHMETIC IS DONE.
                //     RL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC EXPANSION FOR LARGE Z.
                //     DIG = NUMBER OF BASE 10 DIGITS IN TOL = 10**(-DIG).
                //     FNUL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC SERIES FOR LARGE FNU.
                //-----------------------------------------------------------------------
                k1 = i1mach(15);
                k2 = i1mach(16);
                r1m5 = d1mach(5);
                k = Math.Min(Math.Abs(k1), Math.Abs(k2));
                elim = 2.303 * ((double)k * r1m5 - 3.0);
                k1 = i1mach(14) - 1;
                aa = r1m5 * (double)k1;
                dig = Math.Min(aa, 18.0);
                aa = aa * 2.303;
                alim = elim + Math.Max(-aa, -41.45);
                rl = 1.2 * dig + 3.0;
                fnul = 10.0 + 6.0d * (dig - 3.0);
                //-----------------------------------------------------------------------
                //     TEST FOR RANGE
                //-----------------------------------------------------------------------
                aa = 0.5 / tol;
                bb = (double)i1mach(9) * 0.5;
                aa = Math.Min(aa, bb);
                aa = Math.Pow(aa, tth);
                if (az > aa) goto L260;
                aa = Math.Sqrt(aa);
                if (az > aa) ierr = 3;
                zsqrt(zr, zi, ref csqr, ref csqi);
                ztar = tth * (zr * csqr - zi * csqi);
                ztai = tth * (zr * csqi + zi * csqr);
                //----------------------------------------------------------------------
                //     RE(ZTA).LE.0 WHEN RE(Z).LT.0, ESPECIALLY WHEN IM(Z) IS SMALL
                //-----------------------------------------------------------------------
                sfac = 1.0;
                ak = ztai;
                if (zr >= 0.0) goto L80;
                bk = ztar;
                ck = -Math.Abs(bk);
                ztar = ck;
                ztai = ak;
            L80:
                if (zi != 0.0 || zr > 0.0) goto L90;
                ztar = 0.0;
                ztai = ak;
            L90:
                aa = ztar;
                if (kode == 2) goto L100;
                //-----------------------------------------------------------------------
                //     OVERFLOW TEST
                //-----------------------------------------------------------------------
                bb = Math.Abs(aa);
                if (bb < alim) goto L100;
                bb = bb + 0.25 * Math.Log(az);
                sfac = tol;
                if (bb > elim) goto L190;
            L100:
                fmr = 0.0;
                if (aa >= 0.0 && zr > 0.0) goto L110;
                fmr = pi;
                if (zi < 0.0) fmr = -pi;
                ztar = -ztar;
                ztai = -ztai;
            L110:
                //-----------------------------------------------------------------------
                //     AA=FACTOR FOR ANALYTIC CONTINUATION OF I(FNU,ZTA)
                //     KODE=2 RETURNS EXP(-ABS(XZTA))*I(FNU,ZTA) FROM CBESI
                //-----------------------------------------------------------------------
                zbinu(ztar, ztai, fnu, kode, 1, cyr, cyi, ref nz, rl, fnul, tol, elim, alim);
                if (nz < 0) goto L200;
                aa = fmr * fnu;
                z3r = sfac;
                str = Math.Cos(aa);
                sti = Math.Sin(aa);
                s1r = (str * cyr[0] - sti * cyi[0]) * z3r;
                s1i = (str * cyi[0] + sti * cyr[0]) * z3r;
                fnu = (2.0 - fid) / 3.0;
                zbinu(ztar, ztai, fnu, kode, 2, cyr, cyi, ref nz, rl, fnul, tol, elim, alim);
                cyr[0] = cyr[0] * z3r;
                cyi[0] = cyi[0] * z3r;
                cyr[1] = cyr[1] * z3r;
                cyi[1] = cyi[1] * z3r;
                //-----------------------------------------------------------------------
                //     BACKWARD RECUR ONE STEP FOR ORDERS -1/3 OR -2/3
                //-----------------------------------------------------------------------
                zdiv(cyr[0], cyi[0], ztar, ztai, ref str, ref sti);
                s2r = (fnu + fnu) * str + cyr[1];
                s2i = (fnu + fnu) * sti + cyi[1];
                aa = fmr * (fnu - 1.0);
                str = Math.Cos(aa);
                sti = Math.Sin(aa);
                s1r = coef * (s1r + s2r * str - s2i * sti);
                s1i = coef * (s1i + s2r * sti + s2i * str);
                if (id == 1) goto L120;
                str = csqr * s1r - csqi * s1i;
                s1i = csqr * s1i + csqi * s1r;
                s1r = str;
                bir = s1r / sfac;
                bii = s1i / sfac;
                return 0;
            L120:
                str = zr * s1r - zi * s1i;
                s1i = zr * s1i + zi * s1r;
                s1r = str;
                bir = s1r / sfac;
                bii = s1i / sfac;
                return 0;
            L130:
                aa = c1 * (1.0 - fid) + fid * c2;
                bir = aa;
                bii = 0.0;
                return 0;
            L190:
                ierr = 2;
                nz = 0;
                return 0;
            L200:
                if (nz == -1) goto L190;
                nz = 0;
                ierr = 5;
                return 0;
            L260:
                ierr = 4;
                nz = 0;
                return 0;
            }

            // The Bessel function of the first kind and derivative
            public static int zbesj(double zr, double zi, double fnu, int kode, int n, double[] cyr, double[] cyi, ref int nz, ref int ierr)
            {
                #region Description

                //***BEGIN PROLOGUE  ZBESJ
                //***DATE WRITTEN   830501   (YYMMDD)
                //***REVISION DATE  890801   (YYMMDD)
                //***CATEGORY NO.  B5K
                //***KEYWORDS  J-BESSEL FUNCTION,BESSEL FUNCTION OF COMPLEX ARGUMENT,
                //             BESSEL FUNCTION OF FIRST KIND
                //***AUTHOR  AMOS, DONALD E., SANDIA NATIONAL LABORATORIES
                //***PURPOSE  TO COMPUTE THE J-BESSEL FUNCTION OF A COMPLEX ARGUMENT
                //***DESCRIPTION
                //
                //                      ***A DOUBLE PRECISION ROUTINE***
                //         ON KODE=1, CBESJ COMPUTES AN N MEMBER  SEQUENCE OF COMPLEX
                //         BESSEL FUNCTIONS CY(I)=J(FNU+I-1,Z) FOR REAL, NONNEGATIVE
                //         ORDERS FNU+I-1, I=1,...,N AND COMPLEX Z IN THE CUT PLANE
                //         -PI.LT.ARG(Z).LE.PI. ON KODE=2, CBESJ RETURNS THE SCALED
                //         FUNCTIONS
                //
                //         CY(I)=EXP(-ABS(Y))*J(FNU+I-1,Z)   I = 1,...,N , Y=AIMAG(Z)
                //
                //         WHICH REMOVE THE EXPONENTIAL GROWTH IN BOTH THE UPPER AND
                //         LOWER HALF PLANES FOR Z TO INFINITY. DEFINITIONS AND NOTATION
                //         ARE FOUND IN THE NBS HANDBOOK OF MATHEMATICAL FUNCTIONS
                //         (REF. 1).
                //
                //         INPUT      ZR,ZI,FNU ARE DOUBLE PRECISION
                //           ZR,ZI  - Z=CMPLX(ZR,ZI),  -PI.LT.ARG(Z).LE.PI
                //           FNU    - ORDER OF INITIAL J FUNCTION, FNU.GE.0.0D0
                //           KODE   - A PARAMETER TO INDICATE THE SCALING OPTION
                //                    KODE= 1  RETURNS
                //                             CY(I)=J(FNU+I-1,Z), I=1,...,N
                //                        = 2  RETURNS
                //                             CY(I)=J(FNU+I-1,Z)EXP(-ABS(Y)), I=1,...,N
                //           N      - NUMBER OF MEMBERS OF THE SEQUENCE, N.GE.1
                //
                //         OUTPUT     CYR,CYI ARE DOUBLE PRECISION
                //           CYR,CYI- DOUBLE PRECISION VECTORS WHOSE FIRST N COMPONENTS
                //                    CONTAIN REAL AND IMAGINARY PARTS FOR THE SEQUENCE
                //                    CY(I)=J(FNU+I-1,Z)  OR
                //                    CY(I)=J(FNU+I-1,Z)EXP(-ABS(Y))  I=1,...,N
                //                    DEPENDING ON KODE, Y=AIMAG(Z).
                //           NZ     - NUMBER OF COMPONENTS SET TO ZERO DUE TO UNDERFLOW,
                //                    NZ= 0   , NORMAL RETURN
                //                    NZ.GT.0 , LAST NZ COMPONENTS OF CY SET  ZERO DUE
                //                              TO UNDERFLOW, CY(I)=CMPLX(0.0D0,0.0D0),
                //                              I = N-NZ+1,...,N
                //           IERR   - ERROR FLAG
                //                    IERR=0, NORMAL RETURN - COMPUTATION COMPLETED
                //                    IERR=1, INPUT ERROR   - NO COMPUTATION
                //                    IERR=2, OVERFLOW      - NO COMPUTATION, AIMAG(Z)
                //                            TOO LARGE ON KODE=1
                //                    IERR=3, CABS(Z) OR FNU+N-1 LARGE - COMPUTATION DONE
                //                            BUT LOSSES OF SIGNIFCANCE BY ARGUMENT
                //                            REDUCTION PRODUCE LESS THAN HALF OF MACHINE
                //                            ACCURACY
                //                    IERR=4, CABS(Z) OR FNU+N-1 TOO LARGE - NO COMPUTA-
                //                            TION BECAUSE OF COMPLETE LOSSES OF SIGNIFI-
                //                            CANCE BY ARGUMENT REDUCTION
                //                    IERR=5, ERROR              - NO COMPUTATION,
                //                            ALGORITHM TERMINATION CONDITION NOT MET
                //
                //***LONG DESCRIPTION
                //
                //         THE COMPUTATION IS CARRIED OUT BY THE FORMULA
                //
                //         J(FNU,Z)=EXP( FNU*PI*I/2)*I(FNU,-I*Z)    AIMAG(Z).GE.0.0
                //
                //         J(FNU,Z)=EXP(-FNU*PI*I/2)*I(FNU, I*Z)    AIMAG(Z).LT.0.0
                //
                //         WHERE I**2 = -1 AND I(FNU,Z) IS THE I BESSEL FUNCTION.
                //
                //         FOR NEGATIVE ORDERS,THE FORMULA
                //
                //              J(-FNU,Z) = J(FNU,Z)*COS(PI*FNU) - Y(FNU,Z)*SIN(PI*FNU)
                //
                //         CAN BE USED. HOWEVER,FOR LARGE ORDERS CLOSE TO INTEGERS, THE
                //         THE FUNCTION CHANGES RADICALLY. WHEN FNU IS A LARGE POSITIVE
                //         INTEGER,THE MAGNITUDE OF J(-FNU,Z)=J(FNU,Z)*COS(PI*FNU) IS A
                //         LARGE NEGATIVE POWER OF TEN. BUT WHEN FNU IS NOT AN INTEGER,
                //         Y(FNU,Z) DOMINATES IN MAGNITUDE WITH A LARGE POSITIVE POWER OF
                //         TEN AND THE MOST THAT THE SECOND TERM CAN BE REDUCED IS BY
                //         UNIT ROUNDOFF FROM THE COEFFICIENT. THUS, WIDE CHANGES CAN
                //         OCCUR WITHIN UNIT ROUNDOFF OF A LARGE INTEGER FOR FNU. HERE,
                //         LARGE MEANS FNU.GT.CABS(Z).
                //
                //         IN MOST COMPLEX VARIABLE COMPUTATION, ONE MUST EVALUATE ELE-
                //         MENTARY FUNCTIONS. WHEN THE MAGNITUDE OF Z OR FNU+N-1 IS
                //         LARGE, LOSSES OF SIGNIFICANCE BY ARGUMENT REDUCTION OCCUR.
                //         CONSEQUENTLY, IF EITHER ONE EXCEEDS U1=SQRT(0.5/UR), THEN
                //         LOSSES EXCEEDING HALF PRECISION ARE LIKELY AND AN ERROR FLAG
                //         IERR=3 IS TRIGGERED WHERE UR=DMAX1(D1MACH(4),1.0D-18) IS
                //         DOUBLE PRECISION UNIT ROUNDOFF LIMITED TO 18 DIGITS PRECISION.
                //         IF EITHER IS LARGER THAN U2=0.5/UR, THEN ALL SIGNIFICANCE IS
                //         LOST AND IERR=4. IN ORDER TO USE THE INT FUNCTION, ARGUMENTS
                //         MUST BE FURTHER RESTRICTED NOT TO EXCEED THE LARGEST MACHINE
                //         INTEGER, U3=I1MACH(9). THUS, THE MAGNITUDE OF Z AND FNU+N-1 IS
                //         RESTRICTED BY MIN(U2,U3). ON 32 BIT MACHINES, U1,U2, AND U3
                //         ARE APPROXIMATELY 2.0E+3, 4.2E+6, 2.1E+9 IN SINGLE PRECISION
                //         ARITHMETIC AND 1.3E+8, 1.8E+16, 2.1E+9 IN DOUBLE PRECISION
                //         ARITHMETIC RESPECTIVELY. THIS MAKES U2 AND U3 LIMITING IN
                //         THEIR RESPECTIVE ARITHMETICS. THIS MEANS THAT ONE CAN EXPECT
                //         TO RETAIN, IN THE WORST CASES ON 32 BIT MACHINES, NO DIGITS
                //         IN SINGLE AND ONLY 7 DIGITS IN DOUBLE PRECISION ARITHMETIC.
                //         SIMILAR CONSIDERATIONS HOLD FOR OTHER MACHINES.
                //
                //         THE APPROXIMATE RELATIVE ERROR IN THE MAGNITUDE OF A COMPLEX
                //         BESSEL FUNCTION CAN BE EXPRESSED BY P*10**S WHERE P=MAX(UNIT
                //         ROUNDOFF,1.0E-18) IS THE NOMINAL PRECISION AND 10**S REPRE-
                //         SENTS THE INCREASE IN ERROR DUE TO ARGUMENT REDUCTION IN THE
                //         ELEMENTARY FUNCTIONS. HERE, S=MAX(1,ABS(LOG10(CABS(Z))),
                //         ABS(LOG10(FNU))) APPROXIMATELY (I.E. S=MAX(1,ABS(EXPONENT OF
                //         CABS(Z),ABS(EXPONENT OF FNU)) ). HOWEVER, THE PHASE ANGLE MAY
                //         HAVE ONLY ABSOLUTE ACCURACY. THIS IS MOST LIKELY TO OCCUR WHEN
                //         ONE COMPONENT (IN ABSOLUTE VALUE) IS LARGER THAN THE OTHER BY
                //         SEVERAL ORDERS OF MAGNITUDE. IF ONE COMPONENT IS 10**K LARGER
                //         THAN THE OTHER, THEN ONE CAN EXPECT ONLY MAX(ABS(LOG10(P))-K,
                //         0) SIGNIFICANT DIGITS; OR, STATED ANOTHER WAY, WHEN K EXCEEDS
                //         THE EXPONENT OF P, NO SIGNIFICANT DIGITS REMAIN IN THE SMALLER
                //         COMPONENT. HOWEVER, THE PHASE ANGLE RETAINS ABSOLUTE ACCURACY
                //         BECAUSE, IN COMPLEX ARITHMETIC WITH PRECISION P, THE SMALLER
                //         COMPONENT WILL NOT (AS A RULE) DECREASE BELOW P TIMES THE
                //         MAGNITUDE OF THE LARGER COMPONENT. IN THESE EXTREME CASES,
                //         THE PRINCIPAL PHASE ANGLE IS ON THE ORDER OF +P, -P, PI/2-P,
                //         OR -PI/2+P.
                //
                //***REFERENCES  HANDBOOK OF MATHEMATICAL FUNCTIONS BY M. ABRAMOWITZ
                //                 AND I. A. STEGUN, NBS AMS SERIES 55, U.S. DEPT. OF
                //                 COMMERCE, 1955.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 BY D. E. AMOS, SAND83-0083, MAY, 1983.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 AND LARGE ORDER BY D. E. AMOS, SAND83-0643, MAY, 1983
                //
                //               A SUBROUTINE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, SAND85-
                //                 1018, MAY, 1985
                //
                //               A PORTABLE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, TRANS.
                //                 MATH. SOFTWARE, 1986
                //
                //***ROUTINES CALLED  ZBINU,I1MACH,D1MACH
                //***END PROLOGUE  ZBESJ

                #endregion

                const double hpi = 1.570796326794896619231321696;

                double aa, alim, arg, cii, csgni, csgnr, dig;
                double elim, fnul, rl, r1m5, str, tol, zni, znr;
                double bb, fn, az, ascle, rtol, atol, sti;
                int i, inu, inuh, ir, k, k1, k2, nl;

                ierr = 0;
                nz = 0;
                if (fnu < 0.0) ierr = 1;
                if (kode < 1 || kode > 2) ierr = 1;
                if (n < 1) ierr = 1;
                if (ierr != 0) return 0;
                //-----------------------------------------------------------------------
                //    SET PARAMETERS RELATED TO MACHINE CONSTANTS.
                //    TOL IS THE APPROXIMATE UNIT ROUNDOFF LIMITED TO 1.0E-18.
                //    ELIM IS THE APPROXIMATE EXPONENTIAL OVER- AND UNDERFLOW LIMIT.
                //    EXP(-ELIM).LT.EXP(-ALIM)=EXP(-ELIM)/TOL    AND
                //    EXP(ELIM).GT.EXP(ALIM)=EXP(ELIM)*TOL       ARE INTERVALS NEAR
                //    UNDERFLOW AND OVERFLOW LIMITS WHERE SCALED ARITHMETIC IS DONE.
                //    RL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC EXPANSION FOR LARGE Z.
                //    DIG = NUMBER OF BASE 10 DIGITS IN TOL = 10**(-DIG).
                //    FNUL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC SERIES FOR LARGE FNU.
                //-----------------------------------------------------------------------
                tol = Math.Max(d1mach(4), 1.0E-18);
                k1 = i1mach(15);
                k2 = i1mach(16);
                r1m5 = d1mach(5);
                k = Math.Min(Math.Abs(k1), Math.Abs(k2));
                elim = (k * r1m5 - 3.0) * 2.303;
                k1 = i1mach(14) - 1;
                aa = r1m5 * k1;
                dig = Math.Min(aa, 18.0);
                aa *= 2.303;
                alim = elim + Math.Max(-aa, -41.45);
                rl = dig * 1.2 + 3.0;
                fnul = (dig - 3.0) * 6.0 + 10.0;
                //-----------------------------------------------------------------------
                //    TEST FOR PROPER RANGE
                //-----------------------------------------------------------------------
                az = zabs(zr, zi);
                fn = fnu + (double)(n - 1);
                aa = 0.5 / tol;
                bb = 0.5 * (double)i1mach(9);
                aa = Math.Min(aa, bb);
                if (az > aa) goto L260;
                if (fn > aa) goto L260;
                aa = Math.Sqrt(aa);
                if (az > aa) ierr = 3;
                if (fn > aa) ierr = 3;
                //-----------------------------------------------------------------------
                //    CALCULATE CSGN=EXP(FNU*HPI*I) TO MINIMIZE LOSSES OF SIGNIFICANCE
                //    WHEN FNU IS LARGE
                //-----------------------------------------------------------------------
                cii = 1.0;
                inu = (int)fnu;
                inuh = inu / 2;
                ir = inu - (inuh << 1);
                arg = (fnu - (inu - ir)) * hpi;
                csgnr = Math.Cos(arg);
                csgni = Math.Sin(arg);
                if (inuh % 2 == 0) goto L40;
                csgnr = -csgnr;
                csgni = -csgni;
            L40:
                //-----------------------------------------------------------------------
                //    ZN IS IN THE RIGHT HALF PLANE
                //-----------------------------------------------------------------------
                znr = zi;
                zni = -zr;
                if (zi >= 0.0) goto L50;
                znr = -znr;
                zni = -zni;
                csgni = -csgni;
                cii = -cii;
            L50:
                zbinu(znr, zni, fnu, kode, n, cyr, cyi, ref nz, rl, fnul, tol, elim, alim);
                if (nz < 0) goto L130;
                nl = n - nz;
                if (nl == 0) return 0;
                rtol = 1.0 / tol;
                ascle = d1mach(1) * rtol * 1.0E3;
                for (i = 1; i <= nl; i++)
                {
                    //      STR = CYR(I)*CSGNR - CYI(I)*CSGNI
                    //      CYI(I) = CYR(I)*CSGNI + CYI(I)*CSGNR
                    //      CYR(I) = STR
                    aa = cyr[i - 1];
                    bb = cyi[i - 1];
                    atol = 1.0;
                    if (Math.Max(Math.Abs(aa), Math.Abs(bb)) > ascle) goto L55;
                    aa *= rtol;
                    bb *= rtol;
                    atol = tol;
            L55:
                    str = aa * csgnr - bb * csgni;
                    sti = aa * csgni + bb * csgnr;
                    cyr[i - 1] = str * atol;
                    cyi[i - 1] = sti * atol;
                    str = -csgni * cii;
                    csgni = csgnr * cii;
                    csgnr = str;
                }
                return 0;
            L130:
                if (nz == -2) goto L140;
                nz = 0;
                ierr = 2;
                return 0;
            L140:
                nz = 0;
                ierr = 5;
                return 0;
            L260:
                nz = 0;
                ierr = 4;
                return 0;
            }

            // The Bessel function of the second kind and derivative
            public static int zbesy(double zr, double zi, double fnu, int kode, int n, double[] cyr, double[] cyi, ref int nz, double[] cwrkr, double[] cwrki, ref int ierr)
            {
                #region Description

                //***BEGIN PROLOGUE  ZBESY
                //***DATE WRITTEN   830501   (YYMMDD)
                //***REVISION DATE  890801   (YYMMDD)
                //***CATEGORY NO.  B5K
                //***KEYWORDS  Y-BESSEL FUNCTION,BESSEL FUNCTION OF COMPLEX ARGUMENT,
                //             BESSEL FUNCTION OF SECOND KIND
                //***AUTHOR  AMOS, DONALD E., SANDIA NATIONAL LABORATORIES
                //***PURPOSE  TO COMPUTE THE Y-BESSEL FUNCTION OF A COMPLEX ARGUMENT
                //***DESCRIPTION
                //
                //                      ***A DOUBLE PRECISION ROUTINE***
                //
                //         ON KODE=1, CBESY COMPUTES AN N MEMBER SEQUENCE OF COMPLEX
                //         BESSEL FUNCTIONS CY(I)=Y(FNU+I-1,Z) FOR REAL, NONNEGATIVE
                //         ORDERS FNU+I-1, I=1,...,N AND COMPLEX Z IN THE CUT PLANE
                //         -PI.LT.ARG(Z).LE.PI. ON KODE=2, CBESY RETURNS THE SCALED
                //         FUNCTIONS
                //
                //         CY(I)=EXP(-ABS(Y))*Y(FNU+I-1,Z)   I = 1,...,N , Y=AIMAG(Z)
                //
                //         WHICH REMOVE THE EXPONENTIAL GROWTH IN BOTH THE UPPER AND
                //         LOWER HALF PLANES FOR Z TO INFINITY. DEFINITIONS AND NOTATION
                //         ARE FOUND IN THE NBS HANDBOOK OF MATHEMATICAL FUNCTIONS
                //         (REF. 1).
                //
                //         INPUT      ZR,ZI,FNU ARE DOUBLE PRECISION
                //           ZR,ZI  - Z=CMPLX(ZR,ZI), Z.NE.CMPLX(0.0D0,0.0D0),
                //                    -PI.LT.ARG(Z).LE.PI
                //           FNU    - ORDER OF INITIAL Y FUNCTION, FNU.GE.0.0D0
                //           KODE   - A PARAMETER TO INDICATE THE SCALING OPTION
                //                    KODE= 1  RETURNS
                //                             CY(I)=Y(FNU+I-1,Z), I=1,...,N
                //                        = 2  RETURNS
                //                             CY(I)=Y(FNU+I-1,Z)*EXP(-ABS(Y)), I=1,...,N
                //                             WHERE Y=AIMAG(Z)
                //           N      - NUMBER OF MEMBERS OF THE SEQUENCE, N.GE.1
                //           CWRKR, - DOUBLE PRECISION WORK VECTORS OF DIMENSION AT
                //           CWRKI    AT LEAST N
                //
                //         OUTPUT     CYR,CYI ARE DOUBLE PRECISION
                //           CYR,CYI- DOUBLE PRECISION VECTORS WHOSE FIRST N COMPONENTS
                //                    CONTAIN REAL AND IMAGINARY PARTS FOR THE SEQUENCE
                //                    CY(I)=Y(FNU+I-1,Z)  OR
                //                    CY(I)=Y(FNU+I-1,Z)*EXP(-ABS(Y))  I=1,...,N
                //                    DEPENDING ON KODE.
                //           NZ     - NZ=0 , A NORMAL RETURN
                //                    NZ.GT.0 , NZ COMPONENTS OF CY SET TO ZERO DUE TO
                //                    UNDERFLOW (GENERALLY ON KODE=2)
                //           IERR   - ERROR FLAG
                //                    IERR=0, NORMAL RETURN - COMPUTATION COMPLETED
                //                    IERR=1, INPUT ERROR   - NO COMPUTATION
                //                    IERR=2, OVERFLOW      - NO COMPUTATION, FNU IS
                //                            TOO LARGE OR CABS(Z) IS TOO SMALL OR BOTH
                //                    IERR=3, CABS(Z) OR FNU+N-1 LARGE - COMPUTATION DONE
                //                            BUT LOSSES OF SIGNIFCANCE BY ARGUMENT
                //                            REDUCTION PRODUCE LESS THAN HALF OF MACHINE
                //                            ACCURACY
                //                    IERR=4, CABS(Z) OR FNU+N-1 TOO LARGE - NO COMPUTA-
                //                            TION BECAUSE OF COMPLETE LOSSES OF SIGNIFI-
                //                            CANCE BY ARGUMENT REDUCTION
                //                    IERR=5, ERROR              - NO COMPUTATION,
                //                            ALGORITHM TERMINATION CONDITION NOT MET
                //
                //***LONG DESCRIPTION
                //
                //         THE COMPUTATION IS CARRIED OUT BY THE FORMULA
                //
                //         Y(FNU,Z)=0.5*(H(1,FNU,Z)-H(2,FNU,Z))/I
                //
                //         WHERE I**2 = -1 AND THE HANKEL BESSEL FUNCTIONS H(1,FNU,Z)
                //         AND H(2,FNU,Z) ARE CALCULATED IN CBESH.
                //
                //         FOR NEGATIVE ORDERS,THE FORMULA
                //
                //              Y(-FNU,Z) = Y(FNU,Z)*COS(PI*FNU) + J(FNU,Z)*SIN(PI*FNU)
                //
                //         CAN BE USED. HOWEVER,FOR LARGE ORDERS CLOSE TO HALF ODD
                //         INTEGERS THE FUNCTION CHANGES RADICALLY. WHEN FNU IS A LARGE
                //         POSITIVE HALF ODD INTEGER,THE MAGNITUDE OF Y(-FNU,Z)=J(FNU,Z)*
                //         SIN(PI*FNU) IS A LARGE NEGATIVE POWER OF TEN. BUT WHEN FNU IS
                //         NOT A HALF ODD INTEGER, Y(FNU,Z) DOMINATES IN MAGNITUDE WITH A
                //         LARGE POSITIVE POWER OF TEN AND THE MOST THAT THE SECOND TERM
                //         CAN BE REDUCED IS BY UNIT ROUNDOFF FROM THE COEFFICIENT. THUS,
                //         WIDE CHANGES CAN OCCUR WITHIN UNIT ROUNDOFF OF A LARGE HALF
                //         ODD INTEGER. HERE, LARGE MEANS FNU.GT.CABS(Z).
                //
                //         IN MOST COMPLEX VARIABLE COMPUTATION, ONE MUST EVALUATE ELE-
                //         MENTARY FUNCTIONS. WHEN THE MAGNITUDE OF Z OR FNU+N-1 IS
                //         LARGE, LOSSES OF SIGNIFICANCE BY ARGUMENT REDUCTION OCCUR.
                //         CONSEQUENTLY, IF EITHER ONE EXCEEDS U1=SQRT(0.5/UR), THEN
                //         LOSSES EXCEEDING HALF PRECISION ARE LIKELY AND AN ERROR FLAG
                //         IERR=3 IS TRIGGERED WHERE UR=DMAX1(D1MACH(4),1.0D-18) IS
                //         DOUBLE PRECISION UNIT ROUNDOFF LIMITED TO 18 DIGITS PRECISION.
                //         IF EITHER IS LARGER THAN U2=0.5/UR, THEN ALL SIGNIFICANCE IS
                //         LOST AND IERR=4. IN ORDER TO USE THE INT FUNCTION, ARGUMENTS
                //         MUST BE FURTHER RESTRICTED NOT TO EXCEED THE LARGEST MACHINE
                //         INTEGER, U3=I1MACH(9). THUS, THE MAGNITUDE OF Z AND FNU+N-1 IS
                //         RESTRICTED BY MIN(U2,U3). ON 32 BIT MACHINES, U1,U2, AND U3
                //         ARE APPROXIMATELY 2.0E+3, 4.2E+6, 2.1E+9 IN SINGLE PRECISION
                //         ARITHMETIC AND 1.3E+8, 1.8E+16, 2.1E+9 IN DOUBLE PRECISION
                //         ARITHMETIC RESPECTIVELY. THIS MAKES U2 AND U3 LIMITING IN
                //         THEIR RESPECTIVE ARITHMETICS. THIS MEANS THAT ONE CAN EXPECT
                //         TO RETAIN, IN THE WORST CASES ON 32 BIT MACHINES, NO DIGITS
                //         IN SINGLE AND ONLY 7 DIGITS IN DOUBLE PRECISION ARITHMETIC.
                //         SIMILAR CONSIDERATIONS HOLD FOR OTHER MACHINES.
                //
                //         THE APPROXIMATE RELATIVE ERROR IN THE MAGNITUDE OF A COMPLEX
                //         BESSEL FUNCTION CAN BE EXPRESSED BY P*10**S WHERE P=MAX(UNIT
                //         ROUNDOFF,1.0E-18) IS THE NOMINAL PRECISION AND 10**S REPRE-
                //         SENTS THE INCREASE IN ERROR DUE TO ARGUMENT REDUCTION IN THE
                //         ELEMENTARY FUNCTIONS. HERE, S=MAX(1,ABS(LOG10(CABS(Z))),
                //         ABS(LOG10(FNU))) APPROXIMATELY (I.E. S=MAX(1,ABS(EXPONENT OF
                //         CABS(Z),ABS(EXPONENT OF FNU)) ). HOWEVER, THE PHASE ANGLE MAY
                //         HAVE ONLY ABSOLUTE ACCURACY. THIS IS MOST LIKELY TO OCCUR WHEN
                //         ONE COMPONENT (IN ABSOLUTE VALUE) IS LARGER THAN THE OTHER BY
                //         SEVERAL ORDERS OF MAGNITUDE. IF ONE COMPONENT IS 10**K LARGER
                //         THAN THE OTHER, THEN ONE CAN EXPECT ONLY MAX(ABS(LOG10(P))-K,
                //         0) SIGNIFICANT DIGITS; OR, STATED ANOTHER WAY, WHEN K EXCEEDS
                //         THE EXPONENT OF P, NO SIGNIFICANT DIGITS REMAIN IN THE SMALLER
                //         COMPONENT. HOWEVER, THE PHASE ANGLE RETAINS ABSOLUTE ACCURACY
                //         BECAUSE, IN COMPLEX ARITHMETIC WITH PRECISION P, THE SMALLER
                //         COMPONENT WILL NOT (AS A RULE) DECREASE BELOW P TIMES THE
                //         MAGNITUDE OF THE LARGER COMPONENT. IN THESE EXTREME CASES,
                //         THE PRINCIPAL PHASE ANGLE IS ON THE ORDER OF +P, -P, PI/2-P,
                //         OR -PI/2+P.
                //
                //***REFERENCES  HANDBOOK OF MATHEMATICAL FUNCTIONS BY M. ABRAMOWITZ
                //                 AND I. A. STEGUN, NBS AMS SERIES 55, U.S. DEPT. OF
                //                 COMMERCE, 1955.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 BY D. E. AMOS, SAND83-0083, MAY, 1983.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 AND LARGE ORDER BY D. E. AMOS, SAND83-0643, MAY, 1983
                //
                //               A SUBROUTINE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, SAND85-
                //                 1018, MAY, 1985
                //
                //               A PORTABLE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, TRANS.
                //                 MATH. SOFTWARE, 1986
                //
                //***ROUTINES CALLED  ZBESH,I1MACH,D1MACH
                //***END PROLOGUE  ZBESY

                #endregion

                double c1i, c1r, c2i, c2r;
                double elim, exi, exr, ey, hcii, sti, str, tay;
                double ascle, rtol, atol, aa, bb, tol, r1m5;
                int i, k, k1, k2, nz1 = 0, nz2 = 0;

                ierr = 0;
                nz = 0;
                if (zr == 0.0 && zi == 0.0) ierr = 1;
                if (fnu < 0.0) ierr = 1;
                if (kode < 1 || kode > 2) ierr = 1;
                if (n < 1) ierr = 1;
                if (ierr != 0) return 0;
                hcii = 0.5;
                zbesh(zr, zi, fnu, kode, 1, n, cyr, cyi, ref nz1, ref ierr);
                if (ierr != 0 && ierr != 3) goto L170;
                zbesh(zr, zi, fnu, kode, 2, n, cwrkr, cwrki, ref nz2, ref ierr);
                if (ierr != 0 && ierr != 3) goto L170;
                nz = Math.Min(nz1, nz2);
                if (kode == 2) goto L60;
                for (i = 1; i <= n; i++)
                {
                    str = cwrkr[i - 1] - cyr[i - 1];
                    sti = cwrki[i - 1] - cyi[i - 1];
                    cyr[i - 1] = -sti * hcii;
                    cyi[i - 1] = str * hcii;
                }
                return 0;
            L60:
                tol = Math.Max(d1mach(4), 1.0E-18);
                k1 = i1mach(15);
                k2 = i1mach(16);
                k = Math.Min(Math.Abs(k1), Math.Abs(k2));
                r1m5 = d1mach(5);
                //-----------------------------------------------------------------------
                //    ELIM IS THE APPROXIMATE EXPONENTIAL UNDER- AND OVERFLOW LIMIT
                //-----------------------------------------------------------------------
                elim = 2.303 * ((double)k * r1m5 - 3.0);
                exr = Math.Cos(zr);
                exi = Math.Sin(zr);
                ey = 0.0;
                tay = Math.Abs(zi + zi);
                if (tay < elim) ey = Math.Exp(-tay);
                if (zi < 0.0) goto L90;
                c1r = exr * ey;
                c1i = exi * ey;
                c2r = exr;
                c2i = -exi;
            L70:
                nz = 0;
                rtol = 1.0 / tol;
                ascle = d1mach(1) * rtol * 1.0E3;
                for (i = 1; i <= n; i++)
                {
                    //      STR = C1R*CYR(I) - C1I*CYI(I)
                    //      STI = C1R*CYI(I) + C1I*CYR(I)
                    //      STR = -STR + C2R*CWRKR(I) - C2I*CWRKI(I)
                    //      STI = -STI + C2R*CWRKI(I) + C2I*CWRKR(I)
                    //      CYR(I) = -STI*HCII
                    //      CYI(I) = STR*HCII
                    aa = cwrkr[i - 1];
                    bb = cwrki[i - 1];
                    atol = 1.0;
                    if (Math.Max(Math.Abs(aa), Math.Abs(bb)) > ascle) goto L75;
                    aa *= rtol;
                    bb *= rtol;
                    atol = tol;
            L75:
                    str = (aa * c2r - bb * c2i) * atol;
                    sti = (aa * c2i + bb * c2r) * atol;
                    aa = cyr[i - 1];
                    bb = cyi[i - 1];
                    atol = 1.0;
                    if (Math.Max(Math.Abs(aa), Math.Abs(bb)) > ascle) goto L85;
                    aa *= rtol;
                    bb *= rtol;
                    atol = tol;
            L85:
                    str -= (aa * c1r - bb * c1i) * atol;
                    sti -= (aa * c1i + bb * c1r) * atol;
                    cyr[i - 1] = -sti * hcii;
                    cyi[i - 1] = str * hcii;
                    if (str == 0.0 && sti == 0.0 && ey == 0.0) nz++;
                }
                return 0;
            L90:
                c1r = exr;
                c1i = exi;
                c2r = exr * ey;
                c2i = -exi * ey;
                goto L70;
            L170:
                nz = 0;
                return 0;
            }

            // The modified Bessel function of the first kind and derivative
            public static int zbesi(double zr, double zi, double fnu, int kode, int n, double[] cyr, double[] cyi, ref int nz, ref int ierr)
            {
                #region Description

                //***BEGIN PROLOGUE  ZBESI
                //***DATE WRITTEN   830501   (YYMMDD)
                //***REVISION DATE  890801   (YYMMDD)
                //***CATEGORY NO.  B5K
                //***KEYWORDS  I-BESSEL FUNCTION,COMPLEX BESSEL FUNCTION,
                //             MODIFIED BESSEL FUNCTION OF THE FIRST KIND
                //***AUTHOR  AMOS, DONALD E., SANDIA NATIONAL LABORATORIES
                //***PURPOSE  TO COMPUTE I-BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //***DESCRIPTION
                //
                //                    ***A DOUBLE PRECISION ROUTINE***
                //         ON KODE=1, ZBESI COMPUTES AN N MEMBER SEQUENCE OF COMPLEX
                //         BESSEL FUNCTIONS CY(J)=I(FNU+J-1,Z) FOR REAL, NONNEGATIVE
                //         ORDERS FNU+J-1, J=1,...,N AND COMPLEX Z IN THE CUT PLANE
                //         -PI.LT.ARG(Z).LE.PI. ON KODE=2, ZBESI RETURNS THE SCALED
                //         FUNCTIONS
                //
                //         CY(J)=EXP(-ABS(X))*I(FNU+J-1,Z)   J = 1,...,N , X=REAL(Z)
                //
                //         WITH THE EXPONENTIAL GROWTH REMOVED IN BOTH THE LEFT AND
                //         RIGHT HALF PLANES FOR Z TO INFINITY. DEFINITIONS AND NOTATION
                //         ARE FOUND IN THE NBS HANDBOOK OF MATHEMATICAL FUNCTIONS
                //         (REF. 1).
                //
                //         INPUT      ZR,ZI,FNU ARE DOUBLE PRECISION
                //           ZR,ZI  - Z=CMPLX(ZR,ZI),  -PI.LT.ARG(Z).LE.PI
                //           FNU    - ORDER OF INITIAL I FUNCTION, FNU.GE.0.0D0
                //           KODE   - A PARAMETER TO INDICATE THE SCALING OPTION
                //                    KODE= 1  RETURNS
                //                             CY(J)=I(FNU+J-1,Z), J=1,...,N
                //                        = 2  RETURNS
                //                             CY(J)=I(FNU+J-1,Z)*EXP(-ABS(X)), J=1,...,N
                //           N      - NUMBER OF MEMBERS OF THE SEQUENCE, N.GE.1
                //
                //         OUTPUT     CYR,CYI ARE DOUBLE PRECISION
                //           CYR,CYI- DOUBLE PRECISION VECTORS WHOSE FIRST N COMPONENTS
                //                    CONTAIN REAL AND IMAGINARY PARTS FOR THE SEQUENCE
                //                    CY(J)=I(FNU+J-1,Z)  OR
                //                    CY(J)=I(FNU+J-1,Z)*EXP(-ABS(X))  J=1,...,N
                //                    DEPENDING ON KODE, X=REAL(Z)
                //           NZ     - NUMBER OF COMPONENTS SET TO ZERO DUE TO UNDERFLOW,
                //                    NZ= 0   , NORMAL RETURN
                //                    NZ.GT.0 , LAST NZ COMPONENTS OF CY SET TO ZERO
                //                              TO UNDERFLOW, CY(J)=CMPLX(0.0D0,0.0D0)
                //                              J = N-NZ+1,...,N
                //           IERR   - ERROR FLAG
                //                    IERR=0, NORMAL RETURN - COMPUTATION COMPLETED
                //                    IERR=1, INPUT ERROR   - NO COMPUTATION
                //                    IERR=2, OVERFLOW      - NO COMPUTATION, REAL(Z) TOO
                //                            LARGE ON KODE=1
                //                    IERR=3, CABS(Z) OR FNU+N-1 LARGE - COMPUTATION DONE
                //                            BUT LOSSES OF SIGNIFCANCE BY ARGUMENT
                //                            REDUCTION PRODUCE LESS THAN HALF OF MACHINE
                //                            ACCURACY
                //                    IERR=4, CABS(Z) OR FNU+N-1 TOO LARGE - NO COMPUTA-
                //                            TION BECAUSE OF COMPLETE LOSSES OF SIGNIFI-
                //                            CANCE BY ARGUMENT REDUCTION
                //                    IERR=5, ERROR              - NO COMPUTATION,
                //                            ALGORITHM TERMINATION CONDITION NOT MET
                //
                //***LONG DESCRIPTION
                //
                //         THE COMPUTATION IS CARRIED OUT BY THE POWER SERIES FOR
                //         SMALL CABS(Z), THE ASYMPTOTIC EXPANSION FOR LARGE CABS(Z),
                //         THE MILLER ALGORITHM NORMALIZED BY THE WRONSKIAN AND A
                //         NEUMANN SERIES FOR IMTERMEDIATE MAGNITUDES, AND THE
                //         UNIFORM ASYMPTOTIC EXPANSIONS FOR I(FNU,Z) AND J(FNU,Z)
                //         FOR LARGE ORDERS. BACKWARD RECURRENCE IS USED TO GENERATE
                //         SEQUENCES OR REDUCE ORDERS WHEN NECESSARY.
                //
                //         THE CALCULATIONS ABOVE ARE DONE IN THE RIGHT HALF PLANE AND
                //         CONTINUED INTO THE LEFT HALF PLANE BY THE FORMULA
                //
                //         I(FNU,Z*EXP(M*PI)) = EXP(M*PI*FNU)*I(FNU,Z)  REAL(Z).GT.0.0
                //                       M = +I OR -I,  I**2=-1
                //
                //         FOR NEGATIVE ORDERS,THE FORMULA
                //
                //              I(-FNU,Z) = I(FNU,Z) + (2/PI)*SIN(PI*FNU)*K(FNU,Z)
                //
                //         CAN BE USED. HOWEVER,FOR LARGE ORDERS CLOSE TO INTEGERS, THE
                //         THE FUNCTION CHANGES RADICALLY. WHEN FNU IS A LARGE POSITIVE
                //         INTEGER,THE MAGNITUDE OF I(-FNU,Z)=I(FNU,Z) IS A LARGE
                //         NEGATIVE POWER OF TEN. BUT WHEN FNU IS NOT AN INTEGER,
                //         K(FNU,Z) DOMINATES IN MAGNITUDE WITH A LARGE POSITIVE POWER OF
                //         TEN AND THE MOST THAT THE SECOND TERM CAN BE REDUCED IS BY
                //         UNIT ROUNDOFF FROM THE COEFFICIENT. THUS, WIDE CHANGES CAN
                //         OCCUR WITHIN UNIT ROUNDOFF OF A LARGE INTEGER FOR FNU. HERE,
                //         LARGE MEANS FNU.GT.CABS(Z).
                //
                //         IN MOST COMPLEX VARIABLE COMPUTATION, ONE MUST EVALUATE ELE-
                //         MENTARY FUNCTIONS. WHEN THE MAGNITUDE OF Z OR FNU+N-1 IS
                //         LARGE, LOSSES OF SIGNIFICANCE BY ARGUMENT REDUCTION OCCUR.
                //         CONSEQUENTLY, IF EITHER ONE EXCEEDS U1=SQRT(0.5/UR), THEN
                //         LOSSES EXCEEDING HALF PRECISION ARE LIKELY AND AN ERROR FLAG
                //         IERR=3 IS TRIGGERED WHERE UR=DMAX1(D1MACH(4),1.0D-18) IS
                //         DOUBLE PRECISION UNIT ROUNDOFF LIMITED TO 18 DIGITS PRECISION.
                //         IF EITHER IS LARGER THAN U2=0.5/UR, THEN ALL SIGNIFICANCE IS
                //         LOST AND IERR=4. IN ORDER TO USE THE INT FUNCTION, ARGUMENTS
                //         MUST BE FURTHER RESTRICTED NOT TO EXCEED THE LARGEST MACHINE
                //         INTEGER, U3=I1MACH(9). THUS, THE MAGNITUDE OF Z AND FNU+N-1 IS
                //         RESTRICTED BY MIN(U2,U3). ON 32 BIT MACHINES, U1,U2, AND U3
                //         ARE APPROXIMATELY 2.0E+3, 4.2E+6, 2.1E+9 IN SINGLE PRECISION
                //         ARITHMETIC AND 1.3E+8, 1.8E+16, 2.1E+9 IN DOUBLE PRECISION
                //         ARITHMETIC RESPECTIVELY. THIS MAKES U2 AND U3 LIMITING IN
                //         THEIR RESPECTIVE ARITHMETICS. THIS MEANS THAT ONE CAN EXPECT
                //         TO RETAIN, IN THE WORST CASES ON 32 BIT MACHINES, NO DIGITS
                //         IN SINGLE AND ONLY 7 DIGITS IN DOUBLE PRECISION ARITHMETIC.
                //         SIMILAR CONSIDERATIONS HOLD FOR OTHER MACHINES.
                //
                //         THE APPROXIMATE RELATIVE ERROR IN THE MAGNITUDE OF A COMPLEX
                //         BESSEL FUNCTION CAN BE EXPRESSED BY P*10**S WHERE P=MAX(UNIT
                //         ROUNDOFF,1.0E-18) IS THE NOMINAL PRECISION AND 10**S REPRE-
                //         SENTS THE INCREASE IN ERROR DUE TO ARGUMENT REDUCTION IN THE
                //         ELEMENTARY FUNCTIONS. HERE, S=MAX(1,ABS(LOG10(CABS(Z))),
                //         ABS(LOG10(FNU))) APPROXIMATELY (I.E. S=MAX(1,ABS(EXPONENT OF
                //         CABS(Z),ABS(EXPONENT OF FNU)) ). HOWEVER, THE PHASE ANGLE MAY
                //         HAVE ONLY ABSOLUTE ACCURACY. THIS IS MOST LIKELY TO OCCUR WHEN
                //         ONE COMPONENT (IN ABSOLUTE VALUE) IS LARGER THAN THE OTHER BY
                //         SEVERAL ORDERS OF MAGNITUDE. IF ONE COMPONENT IS 10**K LARGER
                //         THAN THE OTHER, THEN ONE CAN EXPECT ONLY MAX(ABS(LOG10(P))-K,
                //         0) SIGNIFICANT DIGITS; OR, STATED ANOTHER WAY, WHEN K EXCEEDS
                //         THE EXPONENT OF P, NO SIGNIFICANT DIGITS REMAIN IN THE SMALLER
                //         COMPONENT. HOWEVER, THE PHASE ANGLE RETAINS ABSOLUTE ACCURACY
                //         BECAUSE, IN COMPLEX ARITHMETIC WITH PRECISION P, THE SMALLER
                //         COMPONENT WILL NOT (AS A RULE) DECREASE BELOW P TIMES THE
                //         MAGNITUDE OF THE LARGER COMPONENT. IN THESE EXTREME CASES,
                //         THE PRINCIPAL PHASE ANGLE IS ON THE ORDER OF +P, -P, PI/2-P,
                //         OR -PI/2+P.
                //
                //***REFERENCES  HANDBOOK OF MATHEMATICAL FUNCTIONS BY M. ABRAMOWITZ
                //                 AND I. A. STEGUN, NBS AMS SERIES 55, U.S. DEPT. OF
                //                 COMMERCE, 1955.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 BY D. E. AMOS, SAND83-0083, MAY, 1983.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 AND LARGE ORDER BY D. E. AMOS, SAND83-0643, MAY, 1983
                //
                //               A SUBROUTINE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, SAND85-
                //                 1018, MAY, 1985
                //
                //               A PORTABLE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, TRANS.
                //                 MATH. SOFTWARE, 1986
                //
                //***ROUTINES CALLED  ZBINU,I1MACH,D1MACH
                //***END PROLOGUE  ZBESI

                #endregion

                const double coner = 1.0;
                const double conei = 0.0;
                const double pi = 3.14159265358979323846264338327950;

                double aa, alim, arg, csgni, csgnr;
                double dig, elim, fnul, rl, r1m5, str, tol, zni, znr;
                double az, bb, fn, ascle, rtol, atol, sti;
                int i, inu, k, k1, k2, nn;

                ierr = 0;
                nz = 0;
                if (fnu < 0.0) ierr = 1;
                if (kode < 1 || kode > 2) ierr = 1;
                if (n < 1) ierr = 1;
                if (ierr != 0) return 0;
                //-----------------------------------------------------------------------
                //    SET PARAMETERS RELATED TO MACHINE CONSTANTS.
                //    TOL IS THE APPROXIMATE UNIT ROUNDOFF LIMITED TO 1.0E-18.
                //    ELIM IS THE APPROXIMATE EXPONENTIAL OVER- AND UNDERFLOW LIMIT.
                //    EXP(-ELIM).LT.EXP(-ALIM)=EXP(-ELIM)/TOL    AND
                //    EXP(ELIM).GT.EXP(ALIM)=EXP(ELIM)*TOL       ARE INTERVALS NEAR
                //    UNDERFLOW AND OVERFLOW LIMITS WHERE SCALED ARITHMETIC IS DONE.
                //    RL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC EXPANSION FOR LARGE Z.
                //    DIG = NUMBER OF BASE 10 DIGITS IN TOL = 10**(-DIG).
                //    FNUL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC SERIES FOR LARGE FNU.
                //-----------------------------------------------------------------------
                tol = Math.Max(d1mach(4), 1.0E-18);
                k1 = i1mach(15);
                k2 = i1mach(16);
                r1m5 = d1mach(5);
                k = Math.Min(Math.Abs(k1), Math.Abs(k2));
                elim = 2.303 * ((double)k * r1m5 - 3.0);
                k1 = i1mach(14) - 1;
                aa = r1m5 * (double)k1;
                dig = Math.Min(aa, 18.0);
                aa = aa * 2.303;
                alim = elim + Math.Max(-aa, -41.45);
                rl = dig * 1.2 + 3.0;
                fnul = 10.0 + 6.0 * (dig - 3.0);
                //-----------------------------------------------------------------------
                //    TEST FOR PROPER RANGE
                //-----------------------------------------------------------------------
                az = zabs(zr, zi);
                fn = fnu + (double)(n - 1);
                aa = 0.5 / tol;
                bb = (double)i1mach(9) * 0.5;
                aa = Math.Min(aa, bb);
                if (az > aa) goto L260;
                if (fn > aa) goto L260;
                aa = Math.Sqrt(aa);
                if (az > aa) ierr = 3;
                if (fn > aa) ierr = 3;
                znr = zr;
                zni = zi;
                csgnr = coner;
                csgni = conei;
                if (zr >= 0.0) goto L40;
                znr = -zr;
                zni = -zi;
                //-----------------------------------------------------------------------
                //    CALCULATE CSGN=EXP(FNU*PI*I) TO MINIMIZE LOSSES OF SIGNIFICANCE
                //    WHEN FNU IS LARGE
                //-----------------------------------------------------------------------
                inu = (int)fnu;
                arg = (fnu - (double)inu) * pi;
                if (zi < 0.0) arg = -arg;
                csgnr = Math.Cos(arg);
                csgni = Math.Sin(arg);
                if (inu % 2 == 0) goto L40;
                csgnr = -csgnr;
                csgni = -csgni;
            L40:
                zbinu(znr, zni, fnu, kode, n, cyr, cyi, ref nz, rl, fnul, tol, elim, alim);
                if (nz < 0) goto L120;
                if (zr >= 0.0) return 0;
                //-----------------------------------------------------------------------
                //    ANALYTIC CONTINUATION TO THE LEFT HALF PLANE
                //-----------------------------------------------------------------------
                nn = n - nz;
                if (nn == 0) return 0;
                rtol = 1.0 / tol;
                ascle = d1mach(1) * rtol * 1.0E3;
                for (i = 1; i <= nn; i++)
                {
                    // STR = CYR(I) * CSGNR - CYI(I) * CSGNI
                    // CYI(I) = CYR(I) * CSGNI + CYI(I) * CSGNR
                    // CYR(I) = STR
                    aa = cyr[i - 1];
                    bb = cyi[i - 1];
                    atol = 1.0;
                    if (Math.Max(Math.Abs(aa), Math.Abs(bb)) > ascle) goto L55;
                    aa = aa * rtol;
                    bb = bb * rtol;
                    atol = tol;
            L55:
                    str = aa * csgnr - bb * csgni;
                    sti = aa * csgni + bb * csgnr;
                    cyr[i - 1] = str * atol;
                    cyi[i - 1] = sti * atol;
                    csgnr = -csgnr;
                    csgni = -csgni;
                }
                return 0;
            L120:
                if (nz == -2) goto L130;
                nz = 0;
                ierr = 2;
                return 0;
            L130:
                nz = 0;
                ierr = 5;
                return 0;
            L260:
                nz = 0;
                ierr = 4;
                return 0;
            }

            // The modified Bessel function of the second kind and derivative
            public static int zbesk(double zr, double zi, double fnu, int kode, int n, double[] cyr, double[] cyi, ref int nz, ref int ierr)
            {
                #region Description

                //***BEGIN PROLOGUE  ZBESK
                //***DATE WRITTEN   830501   (YYMMDD)
                //***REVISION DATE  890801   (YYMMDD)
                //***CATEGORY NO.  B5K
                //***KEYWORDS  K-BESSEL FUNCTION,COMPLEX BESSEL FUNCTION,
                //             MODIFIED BESSEL FUNCTION OF THE SECOND KIND,
                //             BESSEL FUNCTION OF THE THIRD KIND
                //***AUTHOR  AMOS, DONALD E., SANDIA NATIONAL LABORATORIES
                //***PURPOSE  TO COMPUTE K-BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //***DESCRIPTION
                //
                //                      ***A DOUBLE PRECISION ROUTINE***
                //
                //         ON KODE=1, CBESK COMPUTES AN N MEMBER SEQUENCE OF COMPLEX
                //         BESSEL FUNCTIONS CY(J)=K(FNU+J-1,Z) FOR REAL, NONNEGATIVE
                //         ORDERS FNU+J-1, J=1,...,N AND COMPLEX Z.NE.CMPLX(0.0,0.0)
                //         IN THE CUT PLANE -PI.LT.ARG(Z).LE.PI. ON KODE=2, CBESK
                //         RETURNS THE SCALED K FUNCTIONS,
                //
                //         CY(J)=EXP(Z)*K(FNU+J-1,Z) , J=1,...,N,
                //
                //         WHICH REMOVE THE EXPONENTIAL BEHAVIOR IN BOTH THE LEFT AND
                //         RIGHT HALF PLANES FOR Z TO INFINITY. DEFINITIONS AND
                //         NOTATION ARE FOUND IN THE NBS HANDBOOK OF MATHEMATICAL
                //         FUNCTIONS (REF. 1).
                //
                //         INPUT      ZR,ZI,FNU ARE DOUBLE PRECISION
                //           ZR,ZI  - Z=CMPLX(ZR,ZI), Z.NE.CMPLX(0.0D0,0.0D0),
                //                    -PI.LT.ARG(Z).LE.PI
                //           FNU    - ORDER OF INITIAL K FUNCTION, FNU.GE.0.0D0
                //           N      - NUMBER OF MEMBERS OF THE SEQUENCE, N.GE.1
                //           KODE   - A PARAMETER TO INDICATE THE SCALING OPTION
                //                    KODE= 1  RETURNS
                //                             CY(I)=K(FNU+I-1,Z), I=1,...,N
                //                        = 2  RETURNS
                //                             CY(I)=K(FNU+I-1,Z)*EXP(Z), I=1,...,N
                //
                //         OUTPUT     CYR,CYI ARE DOUBLE PRECISION
                //           CYR,CYI- DOUBLE PRECISION VECTORS WHOSE FIRST N COMPONENTS
                //                    CONTAIN REAL AND IMAGINARY PARTS FOR THE SEQUENCE
                //                    CY(I)=K(FNU+I-1,Z), I=1,...,N OR
                //                    CY(I)=K(FNU+I-1,Z)*EXP(Z), I=1,...,N
                //                    DEPENDING ON KODE
                //           NZ     - NUMBER OF COMPONENTS SET TO ZERO DUE TO UNDERFLOW.
                //                    NZ= 0   , NORMAL RETURN
                //                    NZ.GT.0 , FIRST NZ COMPONENTS OF CY SET TO ZERO DUE
                //                              TO UNDERFLOW, CY(I)=CMPLX(0.0D0,0.0D0),
                //                              I=1,...,N WHEN X.GE.0.0. WHEN X.LT.0.0
                //                              NZ STATES ONLY THE NUMBER OF UNDERFLOWS
                //                              IN THE SEQUENCE.
                //
                //           IERR   - ERROR FLAG
                //                    IERR=0, NORMAL RETURN - COMPUTATION COMPLETED
                //                    IERR=1, INPUT ERROR   - NO COMPUTATION
                //                    IERR=2, OVERFLOW      - NO COMPUTATION, FNU IS
                //                            TOO LARGE OR CABS(Z) IS TOO SMALL OR BOTH
                //                    IERR=3, CABS(Z) OR FNU+N-1 LARGE - COMPUTATION DONE
                //                            BUT LOSSES OF SIGNIFCANCE BY ARGUMENT
                //                            REDUCTION PRODUCE LESS THAN HALF OF MACHINE
                //                            ACCURACY
                //                    IERR=4, CABS(Z) OR FNU+N-1 TOO LARGE - NO COMPUTA-
                //                            TION BECAUSE OF COMPLETE LOSSES OF SIGNIFI-
                //                            CANCE BY ARGUMENT REDUCTION
                //                    IERR=5, ERROR              - NO COMPUTATION,
                //                            ALGORITHM TERMINATION CONDITION NOT MET
                //
                //***LONG DESCRIPTION
                //
                //         EQUATIONS OF THE REFERENCE ARE IMPLEMENTED FOR SMALL ORDERS
                //         DNU AND DNU+1.0 IN THE RIGHT HALF PLANE X.GE.0.0. FORWARD
                //         RECURRENCE GENERATES HIGHER ORDERS. K IS CONTINUED TO THE LEFT
                //         HALF PLANE BY THE RELATION
                //
                //         K(FNU,Z*EXP(MP)) = EXP(-MP*FNU)*K(FNU,Z)-MP*I(FNU,Z)
                //         MP=MR*PI*I, MR=+1 OR -1, RE(Z).GT.0, I**2=-1
                //
                //         WHERE I(FNU,Z) IS THE I BESSEL FUNCTION.
                //
                //         FOR LARGE ORDERS, FNU.GT.FNUL, THE K FUNCTION IS COMPUTED
                //         BY MEANS OF ITS UNIFORM ASYMPTOTIC EXPANSIONS.
                //
                //         FOR NEGATIVE ORDERS, THE FORMULA
                //
                //                       K(-FNU,Z) = K(FNU,Z)
                //
                //         CAN BE USED.
                //
                //         CBESK ASSUMES THAT A SIGNIFICANT DIGIT SINH(X) FUNCTION IS
                //         AVAILABLE.
                //
                //         IN MOST COMPLEX VARIABLE COMPUTATION, ONE MUST EVALUATE ELE-
                //         MENTARY FUNCTIONS. WHEN THE MAGNITUDE OF Z OR FNU+N-1 IS
                //         LARGE, LOSSES OF SIGNIFICANCE BY ARGUMENT REDUCTION OCCUR.
                //         CONSEQUENTLY, IF EITHER ONE EXCEEDS U1=SQRT(0.5/UR), THEN
                //         LOSSES EXCEEDING HALF PRECISION ARE LIKELY AND AN ERROR FLAG
                //         IERR=3 IS TRIGGERED WHERE UR=DMAX1(D1MACH(4),1.0D-18) IS
                //         DOUBLE PRECISION UNIT ROUNDOFF LIMITED TO 18 DIGITS PRECISION.
                //         IF EITHER IS LARGER THAN U2=0.5/UR, THEN ALL SIGNIFICANCE IS
                //         LOST AND IERR=4. IN ORDER TO USE THE INT FUNCTION, ARGUMENTS
                //         MUST BE FURTHER RESTRICTED NOT TO EXCEED THE LARGEST MACHINE
                //         INTEGER, U3=I1MACH(9). THUS, THE MAGNITUDE OF Z AND FNU+N-1 IS
                //         RESTRICTED BY MIN(U2,U3). ON 32 BIT MACHINES, U1,U2, AND U3
                //         ARE APPROXIMATELY 2.0E+3, 4.2E+6, 2.1E+9 IN SINGLE PRECISION
                //         ARITHMETIC AND 1.3E+8, 1.8E+16, 2.1E+9 IN DOUBLE PRECISION
                //         ARITHMETIC RESPECTIVELY. THIS MAKES U2 AND U3 LIMITING IN
                //         THEIR RESPECTIVE ARITHMETICS. THIS MEANS THAT ONE CAN EXPECT
                //         TO RETAIN, IN THE WORST CASES ON 32 BIT MACHINES, NO DIGITS
                //         IN SINGLE AND ONLY 7 DIGITS IN DOUBLE PRECISION ARITHMETIC.
                //         SIMILAR CONSIDERATIONS HOLD FOR OTHER MACHINES.
                //
                //         THE APPROXIMATE RELATIVE ERROR IN THE MAGNITUDE OF A COMPLEX
                //         BESSEL FUNCTION CAN BE EXPRESSED BY P*10**S WHERE P=MAX(UNIT
                //         ROUNDOFF,1.0E-18) IS THE NOMINAL PRECISION AND 10**S REPRE-
                //         SENTS THE INCREASE IN ERROR DUE TO ARGUMENT REDUCTION IN THE
                //         ELEMENTARY FUNCTIONS. HERE, S=MAX(1,ABS(LOG10(CABS(Z))),
                //         ABS(LOG10(FNU))) APPROXIMATELY (I.E. S=MAX(1,ABS(EXPONENT OF
                //         CABS(Z),ABS(EXPONENT OF FNU)) ). HOWEVER, THE PHASE ANGLE MAY
                //         HAVE ONLY ABSOLUTE ACCURACY. THIS IS MOST LIKELY TO OCCUR WHEN
                //         ONE COMPONENT (IN ABSOLUTE VALUE) IS LARGER THAN THE OTHER BY
                //         SEVERAL ORDERS OF MAGNITUDE. IF ONE COMPONENT IS 10**K LARGER
                //         THAN THE OTHER, THEN ONE CAN EXPECT ONLY MAX(ABS(LOG10(P))-K,
                //         0) SIGNIFICANT DIGITS; OR, STATED ANOTHER WAY, WHEN K EXCEEDS
                //         THE EXPONENT OF P, NO SIGNIFICANT DIGITS REMAIN IN THE SMALLER
                //         COMPONENT. HOWEVER, THE PHASE ANGLE RETAINS ABSOLUTE ACCURACY
                //         BECAUSE, IN COMPLEX ARITHMETIC WITH PRECISION P, THE SMALLER
                //         COMPONENT WILL NOT (AS A RULE) DECREASE BELOW P TIMES THE
                //         MAGNITUDE OF THE LARGER COMPONENT. IN THESE EXTREME CASES,
                //         THE PRINCIPAL PHASE ANGLE IS ON THE ORDER OF +P, -P, PI/2-P,
                //         OR -PI/2+P.
                //
                //***REFERENCES  HANDBOOK OF MATHEMATICAL FUNCTIONS BY M. ABRAMOWITZ
                //                 AND I. A. STEGUN, NBS AMS SERIES 55, U.S. DEPT. OF
                //                 COMMERCE, 1955.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 BY D. E. AMOS, SAND83-0083, MAY, 1983.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 AND LARGE ORDER BY D. E. AMOS, SAND83-0643, MAY, 1983.
                //
                //               A SUBROUTINE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, SAND85-
                //                 1018, MAY, 1985
                //
                //               A PORTABLE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, TRANS.
                //                 MATH. SOFTWARE, 1986
                //
                //***ROUTINES CALLED  ZACON,ZBKNU,ZBUNK,ZUOIK,ZABS,I1MACH,D1MACH
                //***END PROLOGUE  ZBESK

                #endregion

                double aa, alim, aln, arg, az, dig, elim, fn;
                double fnul, rl, r1m5, tol, ufl, bb;
                int k, k1, k2, mr, nn, nuf = 0, nw = 0;

                ierr = 0;
                nz = 0;
                if (zi == 0.0 && zr == 0.0) ierr = 1;
                if (fnu < 0.0) ierr = 1;
                if (kode < 1 || kode > 2) ierr = 1;
                if (n < 1) ierr = 1;
                if (ierr != 0) return 0;
                nn = n;
                // -----------------------------------------------------------------------
                //     SET PARAMETERS RELATED TO MACHINE CONSTANTS.
                //     TOL IS THE APPROXIMATE UNIT ROUNDOFF LIMITED TO 1.0E-18.
                //     ELIM IS THE APPROXIMATE EXPONENTIAL OVER- AND UNDERFLOW LIMIT.
                //     EXP(-ELIM).LT.EXP(-ALIM)=EXP(-ELIM)/TOL    AND
                //     EXP(ELIM).GT.EXP(ALIM)=EXP(ELIM)*TOL       ARE INTERVALS NEAR
                //     UNDERFLOW AND OVERFLOW LIMITS WHERE SCALED ARITHMETIC IS DONE.
                //     RL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC EXPANSION FOR LARGE Z.
                //     DIG = NUMBER OF BASE 10 DIGITS IN TOL = 10**(-DIG).
                //     FNUL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC SERIES FOR LARGE FNU
                // -----------------------------------------------------------------------
                tol = Math.Max(d1mach(4), 1.0E-18);
                k1 = i1mach(15);
                k2 = i1mach(16);
                r1m5 = d1mach(5);
                k = Math.Min(Math.Abs(k1), Math.Abs(k2));
                elim = 2.303 * ((double)k * r1m5 - 3.0);
                k1 = i1mach(14) - 1;
                aa = r1m5 * (double)k1;
                dig = Math.Min(aa, 18.0);
                aa *= 2.303;
                alim = elim + Math.Max(-aa, -41.45);
                fnul = (dig - 3.0) * 6.0 + 10.0;
                rl = 1.2 * dig + 3.0;
                // -----------------------------------------------------------------------
                //     TEST FOR PROPER RANGE
                // -----------------------------------------------------------------------
                az = zabs(zr, zi);
                fn = fnu + (nn - 1);
                aa = 0.5 / tol;
                bb = i1mach(9) * 0.5;
                aa = Math.Min(aa, bb);
                if (az > aa) goto L260;
                if (fn > aa) goto L260;
                aa = Math.Sqrt(aa);
                if (az > aa) ierr = 3;
                if (fn > aa) ierr = 3;
                // -----------------------------------------------------------------------
                //     OVERFLOW TEST ON THE LAST MEMBER OF THE SEQUENCE
                // -----------------------------------------------------------------------
                //     UFL = EXP(-ELIM)
                ufl = d1mach(1) * 1.0E3;
                if (az < ufl) goto L180;
                if (fnu > fnul) goto L80;
                if (fn <= 1.0) goto L60;
                if (fn > 2.0) goto L50;
                if (az > tol) goto L60;
                arg = az * 0.5;
                aln = -fn * Math.Log(arg);
                if (aln > elim) goto L180;
                goto L60;
            L50:
                zuoik(zr, zi, fnu, kode, 2, nn, cyr, cyi, ref nuf, tol, elim, alim);
                if (nuf < 0) goto L180;
                nz += nuf;
                nn -= nuf;
                // -----------------------------------------------------------------------
                //     HERE NN=N OR NN=0 SINCE NUF=0,NN, OR -1 ON RETURN FROM CUOIK
                //     IF NUF=NN, THEN CY(I)=CZERO FOR ALL I
                // -----------------------------------------------------------------------
                if (nn == 0) goto L100;
            L60:
                if (zr < 0.0) goto L70;
                // -----------------------------------------------------------------------
                //     RIGHT HALF PLANE COMPUTATION, REAL(Z).GE.0.
                // -----------------------------------------------------------------------
                zbknu(zr, zi, fnu, kode, nn, cyr, cyi, ref nw, tol, elim, alim);
                if (nw < 0) goto L200;
                nz = nw;
                return 0;
                // -----------------------------------------------------------------------
                //     LEFT HALF PLANE COMPUTATION
                //     PI/2.LT.ARG(Z).LE.PI AND -PI.LT.ARG(Z).LT.-PI/2.
                // -----------------------------------------------------------------------
            L70:
                if (nz != 0) goto L180;
                mr = 1;
                if (zi < 0.0) mr = -1;
                zacon(zr, zi, fnu, kode, mr, nn, cyr, cyi, ref nw, rl, fnul, tol, elim, alim);
                if (nw < 0) goto L200;
                nz = nw;
                return 0;
                // -----------------------------------------------------------------------
                //     UNIFORM ASYMPTOTIC EXPANSIONS FOR FNU.GT.FNUL
                // -----------------------------------------------------------------------
            L80:
                mr = 0;
                if (zr >= 0.0) goto L90;
                mr = 1;
                if (zi < 0.0) mr = -1;
            L90:
                zbunk(zr, zi, fnu, kode, mr, nn, cyr, cyi, ref nw, tol, elim, alim);
                if (nw < 0) goto L200;
                nz += nw;
                return 0;
            L100:
                if (zr < 0.0) goto L180;
                return 0;
            L180:
                nz = 0;
                ierr = 2;
                return 0;
            L200:
                if (nw == -1) goto L180;
                nz = 0;
                ierr = 5;
                return 0;
            L260:
                nz = 0;
                ierr = 4;
                return 0;
            }

            // The Hankel functions or Bessel functions of third kind and derivative
            public static int zbesh(double zr, double zi, double fnu, int kode, int m, int n, double[] cyr, double[] cyi, ref int nz, ref int ierr)
            {
                #region Description

                //***BEGIN PROLOGUE  ZBESH
                //***DATE WRITTEN   830501   (YYMMDD)
                //***REVISION DATE  890801   (YYMMDD)
                //***CATEGORY NO.  B5K
                //***KEYWORDS  H-BESSEL FUNCTIONS,BESSEL FUNCTIONS OF COMPLEX ARGUMENT,
                //             BESSEL FUNCTIONS OF THIRD KIND,HANKEL FUNCTIONS
                //***AUTHOR  AMOS, DONALD E., SANDIA NATIONAL LABORATORIES
                //***PURPOSE  TO COMPUTE THE H-BESSEL FUNCTIONS OF A COMPLEX ARGUMENT
                //***DESCRIPTION
                //
                //                      ***A DOUBLE PRECISION ROUTINE***
                //         ON KODE=1, ZBESH COMPUTES AN N MEMBER SEQUENCE OF COMPLEX
                //         HANKEL (BESSEL) FUNCTIONS CY(J)=H(M,FNU+J-1,Z) FOR KINDS M=1
                //         OR 2, REAL, NONNEGATIVE ORDERS FNU+J-1, J=1,...,N, AND COMPLEX
                //         Z.NE.CMPLX(0.0,0.0) IN THE CUT PLANE -PI.LT.ARG(Z).LE.PI.
                //         ON KODE=2, ZBESH RETURNS THE SCALED HANKEL FUNCTIONS
                //
                //         CY(I)=EXP(-MM*Z*I)*H(M,FNU+J-1,Z)       MM=3-2*M,   I**2=-1.
                //
                //         WHICH REMOVES THE EXPONENTIAL BEHAVIOR IN BOTH THE UPPER AND
                //         LOWER HALF PLANES. DEFINITIONS AND NOTATION ARE FOUND IN THE
                //         NBS HANDBOOK OF MATHEMATICAL FUNCTIONS (REF. 1).
                //
                //         INPUT      ZR,ZI,FNU ARE DOUBLE PRECISION
                //           ZR,ZI  - Z=CMPLX(ZR,ZI), Z.NE.CMPLX(0.0D0,0.0D0),
                //                    -PT.LT.ARG(Z).LE.PI
                //           FNU    - ORDER OF INITIAL H FUNCTION, FNU.GE.0.0D0
                //           KODE   - A PARAMETER TO INDICATE THE SCALING OPTION
                //                    KODE= 1  RETURNS
                //                             CY(J)=H(M,FNU+J-1,Z),   J=1,...,N
                //                        = 2  RETURNS
                //                             CY(J)=H(M,FNU+J-1,Z)*EXP(-I*Z*(3-2M))
                //                                  J=1,...,N  ,  I**2=-1
                //           M      - KIND OF HANKEL FUNCTION, M=1 OR 2
                //           N      - NUMBER OF MEMBERS IN THE SEQUENCE, N.GE.1
                //
                //         OUTPUT     CYR,CYI ARE DOUBLE PRECISION
                //           CYR,CYI- DOUBLE PRECISION VECTORS WHOSE FIRST N COMPONENTS
                //                    CONTAIN REAL AND IMAGINARY PARTS FOR THE SEQUENCE
                //                    CY(J)=H(M,FNU+J-1,Z)  OR
                //                    CY(J)=H(M,FNU+J-1,Z)*EXP(-I*Z*(3-2M))  J=1,...,N
                //                    DEPENDING ON KODE, I**2=-1.
                //           NZ     - NUMBER OF COMPONENTS SET TO ZERO DUE TO UNDERFLOW,
                //                    NZ= 0   , NORMAL RETURN
                //                    NZ.GT.0 , FIRST NZ COMPONENTS OF CY SET TO ZERO DUE
                //                              TO UNDERFLOW, CY(J)=CMPLX(0.0D0,0.0D0)
                //                              J=1,...,NZ WHEN Y.GT.0.0 AND M=1 OR
                //                              Y.LT.0.0 AND M=2. FOR THE COMPLMENTARY
                //                              HALF PLANES, NZ STATES ONLY THE NUMBER
                //                              OF UNDERFLOWS.
                //           IERR   - ERROR FLAG
                //                    IERR=0, NORMAL RETURN - COMPUTATION COMPLETED
                //                    IERR=1, INPUT ERROR   - NO COMPUTATION
                //                    IERR=2, OVERFLOW      - NO COMPUTATION, FNU TOO
                //                            LARGE OR CABS(Z) TOO SMALL OR BOTH
                //                    IERR=3, CABS(Z) OR FNU+N-1 LARGE - COMPUTATION DONE
                //                            BUT LOSSES OF SIGNIFCANCE BY ARGUMENT
                //                            REDUCTION PRODUCE LESS THAN HALF OF MACHINE
                //                            ACCURACY
                //                    IERR=4, CABS(Z) OR FNU+N-1 TOO LARGE - NO COMPUTA-
                //                            TION BECAUSE OF COMPLETE LOSSES OF SIGNIFI-
                //                            CANCE BY ARGUMENT REDUCTION
                //                    IERR=5, ERROR              - NO COMPUTATION,
                //                            ALGORITHM TERMINATION CONDITION NOT MET
                //
                //***LONG DESCRIPTION
                //
                //         THE COMPUTATION IS CARRIED OUT BY THE RELATION
                //
                //         H(M,FNU,Z)=(1/MP)*EXP(-MP*FNU)*K(FNU,Z*EXP(-MP))
                //             MP=MM*HPI*I,  MM=3-2*M,  HPI=PI/2,  I**2=-1
                //
                //         FOR M=1 OR 2 WHERE THE K BESSEL FUNCTION IS COMPUTED FOR THE
                //         RIGHT HALF PLANE RE(Z).GE.0.0. THE K FUNCTION IS CONTINUED
                //         TO THE LEFT HALF PLANE BY THE RELATION
                //
                //         K(FNU,Z*EXP(MP)) = EXP(-MP*FNU)*K(FNU,Z)-MP*I(FNU,Z)
                //         MP=MR*PI*I, MR=+1 OR -1, RE(Z).GT.0, I**2=-1
                //
                //         WHERE I(FNU,Z) IS THE I BESSEL FUNCTION.
                //
                //         EXPONENTIAL DECAY OF H(M,FNU,Z) OCCURS IN THE UPPER HALF Z
                //         PLANE FOR M=1 AND THE LOWER HALF Z PLANE FOR M=2.  EXPONENTIAL
                //         GROWTH OCCURS IN THE COMPLEMENTARY HALF PLANES.  SCALING
                //         BY EXP(-MM*Z*I) REMOVES THE EXPONENTIAL BEHAVIOR IN THE
                //         WHOLE Z PLANE FOR Z TO INFINITY.
                //
                //         FOR NEGATIVE ORDERS,THE FORMULAE
                //
                //               H(1,-FNU,Z) = H(1,FNU,Z)*CEXP( PI*FNU*I)
                //               H(2,-FNU,Z) = H(2,FNU,Z)*CEXP(-PI*FNU*I)
                //                         I**2=-1
                //
                //         CAN BE USED.
                //
                //         IN MOST COMPLEX VARIABLE COMPUTATION, ONE MUST EVALUATE ELE-
                //         MENTARY FUNCTIONS. WHEN THE MAGNITUDE OF Z OR FNU+N-1 IS
                //         LARGE, LOSSES OF SIGNIFICANCE BY ARGUMENT REDUCTION OCCUR.
                //         CONSEQUENTLY, IF EITHER ONE EXCEEDS U1=SQRT(0.5/UR), THEN
                //         LOSSES EXCEEDING HALF PRECISION ARE LIKELY AND AN ERROR FLAG
                //         IERR=3 IS TRIGGERED WHERE UR=DMAX1(D1MACH(4),1.0D-18) IS
                //         DOUBLE PRECISION UNIT ROUNDOFF LIMITED TO 18 DIGITS PRECISION.
                //         IF EITHER IS LARGER THAN U2=0.5/UR, THEN ALL SIGNIFICANCE IS
                //         LOST AND IERR=4. IN ORDER TO USE THE INT FUNCTION, ARGUMENTS
                //         MUST BE FURTHER RESTRICTED NOT TO EXCEED THE LARGEST MACHINE
                //         INTEGER, U3=I1MACH(9). THUS, THE MAGNITUDE OF Z AND FNU+N-1 IS
                //         RESTRICTED BY MIN(U2,U3). ON 32 BIT MACHINES, U1,U2, AND U3
                //         ARE APPROXIMATELY 2.0E+3, 4.2E+6, 2.1E+9 IN SINGLE PRECISION
                //         ARITHMETIC AND 1.3E+8, 1.8E+16, 2.1E+9 IN DOUBLE PRECISION
                //         ARITHMETIC RESPECTIVELY. THIS MAKES U2 AND U3 LIMITING IN
                //         THEIR RESPECTIVE ARITHMETICS. THIS MEANS THAT ONE CAN EXPECT
                //         TO RETAIN, IN THE WORST CASES ON 32 BIT MACHINES, NO DIGITS
                //         IN SINGLE AND ONLY 7 DIGITS IN DOUBLE PRECISION ARITHMETIC.
                //         SIMILAR CONSIDERATIONS HOLD FOR OTHER MACHINES.
                //
                //         THE APPROXIMATE RELATIVE ERROR IN THE MAGNITUDE OF A COMPLEX
                //         BESSEL FUNCTION CAN BE EXPRESSED BY P*10**S WHERE P=MAX(UNIT
                //         ROUNDOFF,1.0D-18) IS THE NOMINAL PRECISION AND 10**S REPRE-
                //         SENTS THE INCREASE IN ERROR DUE TO ARGUMENT REDUCTION IN THE
                //         ELEMENTARY FUNCTIONS. HERE, S=MAX(1,ABS(LOG10(CABS(Z))),
                //         ABS(LOG10(FNU))) APPROXIMATELY (I.E. S=MAX(1,ABS(EXPONENT OF
                //         CABS(Z),ABS(EXPONENT OF FNU)) ). HOWEVER, THE PHASE ANGLE MAY
                //         HAVE ONLY ABSOLUTE ACCURACY. THIS IS MOST LIKELY TO OCCUR WHEN
                //         ONE COMPONENT (IN ABSOLUTE VALUE) IS LARGER THAN THE OTHER BY
                //         SEVERAL ORDERS OF MAGNITUDE. IF ONE COMPONENT IS 10**K LARGER
                //         THAN THE OTHER, THEN ONE CAN EXPECT ONLY MAX(ABS(LOG10(P))-K,
                //         0) SIGNIFICANT DIGITS; OR, STATED ANOTHER WAY, WHEN K EXCEEDS
                //         THE EXPONENT OF P, NO SIGNIFICANT DIGITS REMAIN IN THE SMALLER
                //         COMPONENT. HOWEVER, THE PHASE ANGLE RETAINS ABSOLUTE ACCURACY
                //         BECAUSE, IN COMPLEX ARITHMETIC WITH PRECISION P, THE SMALLER
                //         COMPONENT WILL NOT (AS A RULE) DECREASE BELOW P TIMES THE
                //         MAGNITUDE OF THE LARGER COMPONENT. IN THESE EXTREME CASES,
                //         THE PRINCIPAL PHASE ANGLE IS ON THE ORDER OF +P, -P, PI/2-P,
                //         OR -PI/2+P.
                //
                //***REFERENCES  HANDBOOK OF MATHEMATICAL FUNCTIONS BY M. ABRAMOWITZ
                //                 AND I. A. STEGUN, NBS AMS SERIES 55, U.S. DEPT. OF
                //                 COMMERCE, 1955.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 BY D. E. AMOS, SAND83-0083, MAY, 1983.
                //
                //               COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 AND LARGE ORDER BY D. E. AMOS, SAND83-0643, MAY, 1983
                //
                //               A SUBROUTINE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, SAND85-
                //                 1018, MAY, 1985
                //
                //               A PORTABLE PACKAGE FOR BESSEL FUNCTIONS OF A COMPLEX
                //                 ARGUMENT AND NONNEGATIVE ORDER BY D. E. AMOS, TRANS.
                //                 MATH. SOFTWARE, 1986
                //
                //***ROUTINES CALLED  ZACON,ZBKNU,ZBUNK,ZUOIK,ZABS,I1MACH,D1MACH
                //***END PROLOGUE  ZBESH

                #endregion

                const double hpi = 1.570796326794896619231321696;

                double aa, alim, aln, arg, az, dig, elim;
                double fmm, fn, fnul, rhpi, rl, r1m5, sgn, str, tol, ufl;
                double zni, znr, zti, bb, ascle, rtol, atol, sti;
                double csgnr, csgni;
                int i, inu, inuh, ir, k, k1, k2;
                int mm, mr, nn, nuf = 0, nw = 0;

                ierr = 0;
                nz = 0;
                if (zr == 0.0 && zi == 0.0) ierr = 1;
                if (fnu < 0.0) ierr = 1;
                if (m < 1 || m > 2) ierr = 1;
                if (kode < 1 || kode > 2) ierr = 1;
                if (n < 1) ierr = 1;
                if (ierr != 0) return 0;
                nn = n;
                //-----------------------------------------------------------------------
                //    SET PARAMETERS RELATED TO MACHINE CONSTANTS.
                //    TOL IS THE APPROXIMATE UNIT ROUNDOFF LIMITED TO 1.0E-18.
                //    ELIM IS THE APPROXIMATE EXPONENTIAL OVER- AND UNDERFLOW LIMIT.
                //    EXP(-ELIM).LT.EXP(-ALIM)=EXP(-ELIM)/TOL    AND
                //    EXP(ELIM).GT.EXP(ALIM)=EXP(ELIM)*TOL       ARE INTERVALS NEAR
                //    UNDERFLOW AND OVERFLOW LIMITS WHERE SCALED ARITHMETIC IS DONE.
                //    RL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC EXPANSION FOR LARGE Z.
                //    DIG = NUMBER OF BASE 10 DIGITS IN TOL = 10**(-DIG).
                //    FNUL IS THE LOWER BOUNDARY OF THE ASYMPTOTIC SERIES FOR LARGE FNU
                //-----------------------------------------------------------------------
                tol = Math.Max(d1mach(4), 1.0E-18);
                k1 = i1mach(15);
                k2 = i1mach(16);
                r1m5 = d1mach(5);
                k = Math.Min(Math.Abs(k1), Math.Abs(k2));
                elim = 2.303 * (k * r1m5 - 3.0);
                k1 = i1mach(14) - 1;
                aa = r1m5 * (double)k1;
                dig = Math.Min(aa, 18.0);
                aa *= 2.303;
                alim = elim + Math.Max(-aa, -41.45);
                fnul = (dig - 3.0) * 6.0 + 10.0;
                rl = dig * 1.2 + 3.0;
                fn = fnu + (nn - 1);
                mm = 3 - m - m;
                fmm = (double)mm;
                znr = fmm * zi;
                zni = -fmm * zr;
                //-----------------------------------------------------------------------
                //    TEST FOR PROPER RANGE
                //-----------------------------------------------------------------------
                az = zabs(zr, zi);
                aa = 0.5 / tol;
                bb = i1mach(9) * 0.5;
                aa = Math.Min(aa, bb);
                if (az > aa) goto L260;
                if (fn > aa) goto L260;
                aa = Math.Sqrt(aa);
                if (az > aa) ierr = 3;
                if (fn > aa) ierr = 3;
                //-----------------------------------------------------------------------
                //    OVERFLOW TEST ON THE LAST MEMBER OF THE SEQUENCE
                //-----------------------------------------------------------------------
                ufl = d1mach(1) * 1.0E3;
                if (az < ufl) goto L230;
                if (fnu > fnul) goto L90;
                if (fn <= 1.0) goto L70;
                if (fn > 2.0) goto L60;
                if (az > tol) goto L70;
                arg = 0.5 * az;
                aln = -fn * Math.Log(arg);
                if (aln > elim) goto L230;
                goto L70;
            L60:
                zuoik(znr, zni, fnu, kode, 2, nn, cyr, cyi, ref nuf, tol, elim, alim);
                if (nuf < 0) goto L230;
                nz += nuf;
                nn -= nuf;
                //-----------------------------------------------------------------------
                //    HERE NN=N OR NN=0 SINCE NUF=0,NN, OR -1 ON RETURN FROM CUOIK
                //    IF NUF=NN, THEN CY(I)=CZERO FOR ALL I
                //-----------------------------------------------------------------------
                if (nn == 0) goto L140;
            L70:
                if (znr < 0.0 || znr == 0.0 && zni < 0.0 && m == 2) goto L80;
                //-----------------------------------------------------------------------
                //    RIGHT HALF PLANE COMPUTATION, XN.GE.0. .AND. (XN.NE.0. .OR.
                //    YN.GE.0. .OR. M=1)
                //-----------------------------------------------------------------------
                zbknu(znr, zni, fnu, kode, nn, cyr, cyi, ref nz, tol, elim, alim);
                goto L110;
                //-----------------------------------------------------------------------
                //    LEFT HALF PLANE COMPUTATION
                //-----------------------------------------------------------------------
            L80:
                mr = -mm;
                zacon(znr, zni, fnu, kode, mr, nn, cyr, cyi, ref nw, rl, fnul, tol, elim, alim);
                if (nw < 0) goto L240;
                nz = nw;
                goto L110;
            L90:
                //-----------------------------------------------------------------------
                //    UNIFORM ASYMPTOTIC EXPANSIONS FOR FNU.GT.FNUL
                //-----------------------------------------------------------------------
                mr = 0;
                if (znr >= 0.0 && (znr != 0.0 || zni >= 0.0 || m != 2)) goto L100;
                mr = -mm;
                if (znr != 0.0 || zni >= 0.0) goto L100;
                znr = -znr;
                zni = -zni;
            L100:
                zbunk(znr, zni, fnu, kode, mr, nn, cyr, cyi, ref nw, tol, elim, alim);
                if (nw < 0) goto L240;
                nz += nw;
            L110:
                //-----------------------------------------------------------------------
                //    H(M,FNU,Z) = -FMM*(I/HPI)*(ZT**FNU)*K(FNU,-Z*ZT)

                //    ZT=EXP(-FMM*HPI*I) = CMPLX(0.0,-FMM), FMM=3-2*M, M=1,2
                //-----------------------------------------------------------------------
                sgn = dsign(hpi, -fmm);
                //-----------------------------------------------------------------------
                //    CALCULATE EXP(FNU*HPI*I) TO MINIMIZE LOSSES OF SIGNIFICANCE
                //    WHEN FNU IS LARGE
                //-----------------------------------------------------------------------
                inu = (int)fnu;
                inuh = inu / 2;
                ir = inu - 2 * inuh;
                arg = (fnu - (inu - ir)) * sgn;
                rhpi = 1.0 / sgn;
                //    ZNI = RHPI*COS(ARG)
                //    ZNR = -RHPI*SIN(ARG)
                csgni = rhpi * Math.Cos(arg);
                csgnr = -rhpi * Math.Sin(arg);
                if (inuh % 2 == 0) goto L120;
                //    ZNR = -ZNR
                //    ZNI = -ZNI
                csgnr = -csgnr;
                csgni = -csgni;
            L120:
                zti = -fmm;
                rtol = 1.0 / tol;
                ascle = ufl * rtol;
                for (i = 1; i <= nn; i++)
                {
                    //      STR = CYR(I)*ZNR - CYI(I)*ZNI
                    //      CYI(I) = CYR(I)*ZNI + CYI(I)*ZNR
                    //      CYR(I) = STR
                    //      STR = -ZNI*ZTI
                    //      ZNI = ZNR*ZTI
                    //      ZNR = STR
                    aa = cyr[i - 1];
                    bb = cyi[i - 1];
                    atol = 1.0;
                    //Computing MAX
                    if (Math.Max(Math.Abs(aa), Math.Abs(bb)) > ascle) goto L135;
                    aa *= rtol;
                    bb *= rtol;
                    atol = tol;
            L135:
                    str = aa * csgnr - bb * csgni;
                    sti = aa * csgni + bb * csgnr;
                    cyr[i - 1] = str * atol;
                    cyi[i - 1] = sti * atol;
                    str = -csgni * zti;
                    csgni = csgnr * zti;
                    csgnr = str;
                }
                return 0;
            L140:
                if (znr < 0.0) goto L230;
                return 0;
            L230:
                nz = 0;
                ierr = 2;
                return 0;
            L240:
                if (nw == -1) goto L230;
                nz = 0;
                ierr = 5;
                return 0;
            L260:
                nz = 0;
                ierr = 4;
                return 0;
            }

            #endregion

            #region LnGamma functions

            // The logarithm of the gamma function
            public static double dgamln(double z, ref int ierr)
            {
                #region Description

                //***BEGIN PROLOGUE  DGAMLN
                //***DATE WRITTEN   830501   (YYMMDD)
                //***REVISION DATE  830501   (YYMMDD)
                //***CATEGORY NO.  B5F
                //***KEYWORDS  GAMMA FUNCTION,LOGARITHM OF GAMMA FUNCTION
                //***AUTHOR  AMOS, DONALD E., SANDIA NATIONAL LABORATORIES
                //***PURPOSE  TO COMPUTE THE LOGARITHM OF THE GAMMA FUNCTION
                //***DESCRIPTION
                //
                //               **** A DOUBLE PRECISION ROUTINE ****
                //         DGAMLN COMPUTES THE NATURAL LOG OF THE GAMMA FUNCTION FOR
                //         Z.GT.0.  THE ASYMPTOTIC EXPANSION IS USED TO GENERATE VALUES
                //         GREATER THAN ZMIN WHICH ARE ADJUSTED BY THE RECURSION
                //         G(Z+1)=Z*G(Z) FOR Z.LE.ZMIN.  THE FUNCTION WAS MADE AS
                //         PORTABLE AS POSSIBLE BY COMPUTIMG ZMIN FROM THE NUMBER OF BASE
                //         10 DIGITS IN A WORD, RLN=AMAX1(-ALOG10(R1MACH(4)),0.5E-18)
                //         LIMITED TO 18 DIGITS OF (RELATIVE) ACCURACY.
                //
                //         SINCE INTEGER ARGUMENTS ARE COMMON, A TABLE LOOK UP ON 100
                //         VALUES IS USED FOR SPEED OF EXECUTION.
                //
                //     DESCRIPTION OF ARGUMENTS
                //
                //         INPUT      Z IS D0UBLE PRECISION
                //           Z      - ARGUMENT, Z.GT.0.0D0
                //
                //         OUTPUT      DGAMLN IS DOUBLE PRECISION
                //           DGAMLN  - NATURAL LOG OF THE GAMMA FUNCTION AT Z.NE.0.0D0
                //           IERR    - ERROR FLAG
                //                     IERR=0, NORMAL RETURN, COMPUTATION COMPLETED
                //                     IERR=1, Z.LE.0.0D0,    NO COMPUTATION
                //
                //
                //***REFERENCES  COMPUTATION OF BESSEL FUNCTIONS OF COMPLEX ARGUMENT
                //                 BY D. E. AMOS, SAND83-0083, MAY, 1983.
                //***ROUTINES CALLED  I1MACH,D1MACH
                //***END PROLOGUE  DGAMLN

                #endregion

                const double con = 1.83787706640934548;

                double[] gln = {
                    0.0,0.0,0.693147180559945309,
                    1.791759469228055,3.17805383034794562,4.78749174278204599,
                    6.579251212010101,8.5251613610654143,10.6046029027452502,
                    12.8018274800814696,15.1044125730755153,17.5023078458738858,
                    19.9872144956618861,22.5521638531234229,25.1912211827386815,
                    27.8992713838408916,30.6718601060806728,33.5050734501368889,
                    36.3954452080330536,39.339884187199494,42.335616460753485,
                    45.380138898476908,48.4711813518352239,51.6066755677643736,
                    54.7847293981123192,58.0036052229805199,61.261701761002002,
                    64.5575386270063311,67.889743137181535,71.257038967168009,
                    74.6582363488301644,78.0922235533153106,81.5579594561150372,
                    85.0544670175815174,88.5808275421976788,92.1361756036870925,
                    95.7196945421432025,99.3306124547874269,102.968198614513813,
                    106.631760260643459,110.320639714757395,114.034211781461703,
                    117.771881399745072,121.533081515438634,125.317271149356895,
                    129.123933639127215,132.95257503561631,136.802722637326368,
                    140.673923648234259,144.565743946344886,148.477766951773032,
                    152.409592584497358,156.360836303078785,160.331128216630907,
                    164.320112263195181,168.327445448427652,172.352797139162802,
                    176.395848406997352,180.456291417543771,184.533828861449491,
                    188.628173423671591,192.739047287844902,196.866181672889994,
                    201.009316399281527,205.168199482641199,209.342586752536836,
                    213.532241494563261,217.736934113954227,221.956441819130334,
                    226.190548323727593,230.439043565776952,234.701723442818268,
                    238.978389561834323,243.268849002982714,247.572914096186884,
                    251.890402209723194,256.221135550009525,260.564940971863209,
                    264.921649798552801,269.291097651019823,273.673124285693704,
                    278.067573440366143,282.474292687630396,286.893133295426994,
                    291.323950094270308,295.766601350760624,300.220948647014132,
                    304.686856765668715,309.164193580146922,313.652829949879062,
                    318.152639620209327,322.663499126726177,327.185287703775217,
                    331.717887196928473,336.261181979198477,340.815058870799018,
                    345.379407062266854,349.954118040770237,354.539085519440809,
                    359.134205369575399 };
                double[] cf = {
                    .0833333333333333333,-.00277777777777777778,
                    7.93650793650793651e-4,-5.95238095238095238e-4,
                    8.41750841750841751e-4,-.00191752691752691753,
                    .00641025641025641026,-.0295506535947712418,.179644372368830573,
                    -1.39243221690590112,13.402864044168392,-156.848284626002017,
                    2193.10333333333333,-36108.7712537249894,691472.268851313067,
                    -15238221.5394074162,382900751.391414141,-10882266035.7843911,
                    347320283765.002252,-12369602142269.2745,488788064793079.335,
                    -21320333960919373.9 };

                double fln, fz, rln, s, tlg, trm, tst;
                double t1, wdtol, zdmy, zinc, zm, zmin, zp, zsq;
                int i, i1m, k, mz, nz = 0;

                ierr = 0;
                if (z <= 0.0) goto L70;
                if (z > 101.0) goto L10;
                nz = (int)z;
                fz = z - (double)nz;
                if (fz > 0.0) goto L10;
                if (nz > 100) goto L10;
                return gln[nz - 1];
            L10:
                wdtol = d1mach(4);
                wdtol = Math.Max(wdtol, 5e-19);
                i1m = i1mach(14);
                rln = d1mach(5) * (double)i1m;
                fln = Math.Min(rln, 20.0);
                fln = Math.Max(fln, 3.0);
                fln += -3.0;
                zm = 1.8 + 0.3875 * fln;
                mz = (int)zm + 1;
                zmin = (double)mz;
                zdmy = z;
                zinc = 0.0;
                if (z >= zmin) goto L20;
                zinc = zmin - nz;
                zdmy = z + zinc;
            L20:
                zp = 1.0 / zdmy;
                t1 = cf[0] * zp;
                s = t1;
                if (zp < wdtol) goto L40;
                zsq = zp * zp;
                tst = t1 * wdtol;
                for (k = 2; k <= 22; k++)
                {
                    zp *= zsq;
                    trm = cf[k - 1] * zp;
                    if (Math.Abs(trm) < tst) goto L40;
                    s += trm;
                }
            L40:
                if (zinc != 0.0) goto L50;
                tlg = Math.Log(z);
                return z * (tlg - 1.0) + (con - tlg) * 0.5 + s;
            L50:
                zp = 1.0;
                nz = (int)zinc;
                for (i = 1; i <= nz; i++)
                {
                    zp *= z + (i - 1);
                }
                tlg = Math.Log(zdmy);
                return zdmy * (tlg - 1.0) - Math.Log(zp) + (con - tlg) * 0.5 + s;
            L70:
                ierr = 1;
                return d1mach(2);
            }

            #endregion

            #region Fortran utilities

            static double d1mach(int i)
            {
                #region Description

                //***BEGIN PROLOGUE  D1MACH
                //***DATE WRITTEN   750101   (YYMMDD)
                //***REVISION DATE  890213   (YYMMDD)
                //***CATEGORY NO.  R1
                //***KEYWORDS  LIBRARY=SLATEC,TYPE=DOUBLE PRECISION(R1MACH-S D1MACH-D),
                //             MACHINE CONSTANTS
                //***AUTHOR  FOX, P. A., (BELL LABS)
                //           HALL, A. D., (BELL LABS)
                //           SCHRYER, N. L., (BELL LABS)
                //***PURPOSE  Returns double precision machine dependent constants
                //***DESCRIPTION
                //
                //   D1MACH can be used to obtain machine-dependent parameters
                //   for the local machine environment.  It is a function
                //   subprogram with one (input) argument, and can be called
                //   as follows, for example
                //
                //        D = D1MACH(I)
                //
                //   where I=1,...,5.  The (output) value of D above is
                //   determined by the (input) value of I.  The results for
                //   various values of I are discussed below.
                //
                //   D1MACH( 1) = B**(EMIN-1), the smallest positive magnitude.
                //   D1MACH( 2) = B**EMAX*(1 - B**(-T)), the largest magnitude.
                //   D1MACH( 3) = B**(-T), the smallest relative spacing.
                //   D1MACH( 4) = B**(1-T), the largest relative spacing.
                //   D1MACH( 5) = LOG10(B)
                //
                //   Assume double precision numbers are represented in the T-digit,
                //   base-B form
                //
                //              sign (B**E)*( (X(1)/B) + ... + (X(T)/B**T) )
                //
                //   where 0 .LE. X(I) .LT. B for I=1,...,T, 0 .LT. X(1), and
                //   EMIN .LE. E .LE. EMAX.
                //
                //   The values of B, T, EMIN and EMAX are provided in I1MACH as
                //   follows:
                //   I1MACH(10) = B, the base.
                //   I1MACH(14) = T, the number of base-B digits.
                //   I1MACH(15) = EMIN, the smallest exponent E.
                //   I1MACH(16) = EMAX, the largest exponent E.
                //
                //   To alter this function for a particular environment,
                //   the desired set of DATA statements should be activated by
                //   removing the C from column 1.  Also, the values of
                //   D1MACH(1) - D1MACH(4) should be checked for consistency
                //   with the local operating system.
                //
                //***REFERENCES  FOX P.A., HALL A.D., SCHRYER N.L.,*FRAMEWORK FOR A
                //                 PORTABLE LIBRARY*, ACM TRANSACTIONS ON MATHEMATICAL
                //                 SOFTWARE, VOL. 4, NO. 2, JUNE 1978, PP. 177-188.
                //***ROUTINES CALLED  XERROR
                //***END PROLOGUE  D1MACH

                #endregion

                const int FLT_RADIX = 2; // the radix used by the representation of all floating-point types
                const double DBL_EPSILON = 2.2204460492503130808E-16; // 2^(1 - 53)
                const double DBL_MAX = double.MaxValue; // 2^1024 * (1 - 2^(-53))
                const double DBL_MIN = 2.2250738585072013831E-308; // 2^(-1021 - 1)

                switch (i)
                {
                    case 1: return DBL_MIN; // the smallest positive magnitude.
                    case 2: return DBL_MAX; // the largest magnitude.
                    case 3: return DBL_EPSILON / FLT_RADIX; // return Precision.DoublePrecision; // the smallest relative spacing.
                    case 4: return DBL_EPSILON; // return Precision.PositiveDoublePrecision; // the largest relative spacing.
                    case 5: return Math.Log10(FLT_RADIX);
                }
                return 0;
            }

            static int i1mach(int i)
            {
                #region Description

                //***BEGIN PROLOGUE  I1MACH
                //***DATE WRITTEN   750101   (YYMMDD)
                //***REVISION DATE  890213   (YYMMDD)
                //***CATEGORY NO.  R1
                //***KEYWORDS  LIBRARY=SLATEC,TYPE=INTEGER(I1MACH-I),MACHINE CONSTANTS
                //***AUTHOR  FOX, P. A., (BELL LABS)
                //           HALL, A. D., (BELL LABS)
                //           SCHRYER, N. L., (BELL LABS)
                //***PURPOSE  Returns integer machine dependent constants
                //***DESCRIPTION
                //
                //     I1MACH can be used to obtain machine-dependent parameters
                //     for the local machine environment.  It is a function
                //     subroutine with one (input) argument, and can be called
                //     as follows, for example
                //
                //          K = I1MACH(I)
                //
                //     where I=1,...,16.  The (output) value of K above is
                //     determined by the (input) value of I.  The results for
                //     various values of I are discussed below.
                //
                //  I/O unit numbers.
                //    I1MACH( 1) = the standard input unit.
                //    I1MACH( 2) = the standard output unit.
                //    I1MACH( 3) = the standard punch unit.
                //    I1MACH( 4) = the standard error message unit.
                //
                //  Words.
                //    I1MACH( 5) = the number of bits per integer storage unit.
                //    I1MACH( 6) = the number of characters per integer storage unit.
                //
                //  Integers.
                //    assume integers are represented in the S-digit, base-A form
                //
                //               sign ( X(S-1)*A**(S-1) + ... + X(1)*A + X(0) )
                //
                //               where 0 .LE. X(I) .LT. A for I=0,...,S-1.
                //    I1MACH( 7) = A, the base.
                //    I1MACH( 8) = S, the number of base-A digits.
                //    I1MACH( 9) = A**S - 1, the largest magnitude.
                //
                //  Floating-Point Numbers.
                //    Assume floating-point numbers are represented in the T-digit,
                //    base-B form
                //               sign (B**E)*( (X(1)/B) + ... + (X(T)/B**T) )
                //
                //               where 0 .LE. X(I) .LT. B for I=1,...,T,
                //               0 .LT. X(1), and EMIN .LE. E .LE. EMAX.
                //    I1MACH(10) = B, the base.
                //
                //  Single-Precision
                //    I1MACH(11) = T, the number of base-B digits.
                //    I1MACH(12) = EMIN, the smallest exponent E.
                //    I1MACH(13) = EMAX, the largest exponent E.
                //
                //  Double-Precision
                //    I1MACH(14) = T, the number of base-B digits.
                //    I1MACH(15) = EMIN, the smallest exponent E.
                //    I1MACH(16) = EMAX, the largest exponent E.
                //
                //  To alter this function for a particular environment,
                //  the desired set of DATA statements should be activated by
                //  removing the C from column 1.  Also, the values of
                //  I1MACH(1) - I1MACH(4) should be checked for consistency
                //  with the local operating system.
                //
                //***REFERENCES  FOX P.A., HALL A.D., SCHRYER N.L.,*FRAMEWORK FOR A
                //                 PORTABLE LIBRARY*, ACM TRANSACTIONS ON MATHEMATICAL
                //                 SOFTWARE, VOL. 4, NO. 2, JUNE 1978, PP. 177-188.
                //***ROUTINES CALLED  (NONE)
                //***END PROLOGUE  I1MACH

                #endregion

                switch (i)
                {
                    case 9: return int.MaxValue; // the largest magnitude of integer = 2^31 - 1 = 2147483647
                    case 14: return 53; // return Precision.DoubleWidth; // the number of base-2 digits.
                    case 15: return -1021; // EMIN, the smallest exponent E.
                    case 16: return 1024; // EMAX, the largest exponent E = 2^10
                }
                return 0;
            }

            static double dsign(double a, double b)
            {
                // Returns the absolute value of A times the sign of B
                double x = (a >= 0 ? a : -a);
                return (b >= 0 ? x : -x);
            }

            static double zabs(double zr, double zi)
            {
                #region Description

                //***BEGIN PROLOGUE  ZABS
                //***REFER TO  ZBESH,ZBESI,ZBESJ,ZBESK,ZBESY,ZAIRY,ZBIRY
                //
                //     ZABS COMPUTES THE ABSOLUTE VALUE OR MAGNITUDE OF A DOUBLE
                //     PRECISION COMPLEX VARIABLE CMPLX(ZR,ZI)
                //
                //***ROUTINES CALLED  (NONE)
                //***END PROLOGUE  ZABS

                #endregion

                double u, v, q, s;

                u = Math.Abs(zr);
                v = Math.Abs(zi);
                s = u + v;
                //---------------------------------------------------------------------- -
                //     S * 1.0D0 MAKES AN UNNORMALIZED UNDERFLOW ON CDC MACHINES INTO A
                //     TRUE FLOATING ZERO
                //-----------------------------------------------------------------------
                s = s * 1.0;
                if (s == 0.0) goto L20;
                if (u > v) goto L10;
                q = u / v;
                return v * Math.Sqrt(1.0 + q * q);
            L10:
                q = v / u;
                return u * Math.Sqrt(1.0 + q * q);
            L20:
                return 0.0;
            }

            static int zdiv(double ar, double ai, double br, double bi, ref double cr, ref double ci)
            {
                #region Description

                //***BEGIN PROLOGUE  ZDIV
                //***REFER TO  ZBESH,ZBESI,ZBESJ,ZBESK,ZBESY,ZAIRY,ZBIRY
                //
                //     DOUBLE PRECISION COMPLEX DIVIDE C=A/B.
                //
                //***ROUTINES CALLED  ZABS
                //***END PROLOGUE  ZDIV

                #endregion

                double bm, ca, cb, cc, cd;

                bm = 1.0 / zabs(br, bi);
                cc = br * bm;
                cd = bi * bm;
                ca = (ar * cc + ai * cd) * bm;
                cb = (ai * cc - ar * cd) * bm;
                cr = ca;
                ci = cb;
                return 0;
            }

            static int zexp(double ar, double ai, ref double br, ref double bi)
            {
                #region Description

                //***BEGIN PROLOGUE  ZEXP
                //***REFER TO  ZBESH,ZBESI,ZBESJ,ZBESK,ZBESY,ZAIRY,ZBIRY
                //
                //     DOUBLE PRECISION COMPLEX EXPONENTIAL FUNCTION B=EXP(A)
                //
                //***ROUTINES CALLED  (NONE)
                //***END PROLOGUE  ZEXP

                #endregion

                double zm, ca, cb;

                zm = Math.Exp(ar);
                ca = zm * Math.Cos(ai);
                cb = zm * Math.Sin(ai);
                br = ca;
                bi = cb;
                return 0;
            }

            static int zlog(double ar, double ai, ref double br, ref double bi, ref int ierr)
            {
                #region Description

                //***BEGIN PROLOGUE  ZLOG
                //***REFER TO  ZBESH,ZBESI,ZBESJ,ZBESK,ZBESY,ZAIRY,ZBIRY
                //
                //     DOUBLE PRECISION COMPLEX LOGARITHM B=CLOG(A)
                //     IERR=0,NORMAL RETURN      IERR=1, Z=CMPLX(0.0,0.0)
                //***ROUTINES CALLED  ZABS
                //***END PROLOGUE  ZLOG

                #endregion

                double dhpi = 1.570796326794896619231321696;
                double dpi = 3.141592653589793238462643383;
                double zm, dtheta;

                ierr = 0;
                if (ar == 0.0) goto L10;
                if (ai == 0.0) goto L20;
                dtheta = Math.Atan(ai / ar);
                if (dtheta <= 0.0) goto L40;
                if (ar < 0.0) dtheta = dtheta - dpi;
                goto L50;
            L10:
                if (ai == 0.0) goto L60;
                bi = dhpi;
                br = Math.Log(Math.Abs(ai));
                if (ai < 0.0) bi = -bi;
                return 0;
            L20:
                if (ar > 0.0) goto L30;
                br = Math.Log(Math.Abs(ar));
                bi = dpi;
                return 0;
            L30:
                br = Math.Log(ar);
                bi = 0.0;
                return 0;
            L40:
                if (ar < 0.0) dtheta = dtheta + dpi;
            L50:
                zm = zabs(ar, ai);
                br = Math.Log(zm);
                bi = dtheta;
                return 0;
            L60:
                ierr = 1;
                return 0;
            }

            static int zmlt(double ar, double ai, double br, double bi, ref double cr, ref double ci)
            {
                #region Description

                //***BEGIN PROLOGUE  ZMLT
                //***REFER TO  ZBESH,ZBESI,ZBESJ,ZBESK,ZBESY,ZAIRY,ZBIRY
                //
                //     DOUBLE PRECISION COMPLEX MULTIPLY, C=A*B.
                //
                //***ROUTINES CALLED  (NONE)
                //***END PROLOGUE  ZMLT

                #endregion

                double ca, cb;

                ca = ar * br - ai * bi;
                cb = ar * bi + ai * br;
                cr = ca;
                ci = cb;
                return 0;
            }

            static int zsqrt(double ar, double ai, ref double br, ref double bi)
            {
                #region Description

                //***BEGIN PROLOGUE  ZSQRT
                //***REFER TO  ZBESH,ZBESI,ZBESJ,ZBESK,ZBESY,ZAIRY,ZBIRY
                //
                //     DOUBLE PRECISION COMPLEX SQUARE ROOT, B=CSQRT(A)
                //
                //***ROUTINES CALLED  ZABS
                //***END PROLOGUE  ZSQRT

                #endregion

                const double drt = 0.7071067811865475244008443621; // sqrt(2)
                const double dpi = 3.141592653589793238462643383;
                double zm, dtheta;

                zm = zabs(ar, ai);
                zm = Math.Sqrt(zm);
                if (ar == 0.0) goto L10;
                if (ai == 0.0) goto L20;
                dtheta = Math.Atan(ai / ar);
                if (dtheta <= 0.0) goto L40;
                if (ar < 0.0) dtheta = dtheta - dpi;
                goto L50;
            L10:
                if (ai > 0.0) goto L60;
                if (ai < 0.0) goto L70;
                br = 0.0;
                bi = 0.0;
                return 0;
            L20:
                if (ar > 0.0) goto L30;
                br = 0.0;
                bi = Math.Sqrt(Math.Abs(ar));
                return 0;
            L30:
                br = Math.Sqrt(ar);
                bi = 0.0;
                return 0;
            L40:
                if (ar < 0.0) dtheta = dtheta + dpi;
            L50:
                dtheta = dtheta * 0.5;
                br = zm * Math.Cos(dtheta);
                bi = zm * Math.Sin(dtheta);
                return 0;
            L60:
                br = zm * drt;
                bi = zm * drt;
                return 0;
            L70:
                br = zm * drt;
                bi = -zm * drt;
                return 0;
            }

            #endregion

            #region Subroutines to calculate the Bessel functions

            static int zacai(double zr, double zi, double fnu, int kode, int mr, int n, double[] yr, double[] yi, ref int nz, double rl, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZACAI
                //***REFER TO  ZAIRY
                //
                //     ZACAI APPLIES THE ANALYTIC CONTINUATION FORMULA
                //
                //         K(FNU,ZN*EXP(MP))=K(FNU,ZN)*EXP(-MP*FNU) - MP*I(FNU,ZN)
                //                 MP=PI*MR*CMPLX(0.0,1.0)
                //
                //     TO CONTINUE THE K FUNCTION FROM THE RIGHT HALF TO THE LEFT
                //     HALF Z PLANE FOR USE WITH ZAIRY WHERE FNU=1/3 OR 2/3 AND N=1.
                //     ZACAI IS THE SAME AS ZACON WITH THE PARTS FOR LARGER ORDERS AND
                //     RECURRENCE REMOVED. A RECURSIVE CALL TO ZACON CAN RESULT IF ZACON
                //     IS CALLED FROM ZAIRY.
                //
                //***ROUTINES CALLED  ZASYI,ZBKNU,ZMLRI,ZSERI,ZS1S2,D1MACH,ZABS
                //***END PROLOGUE  ZACAI

                #endregion

                const double pi = 3.14159265358979323846264338327950;
                double arg, ascle, az, csgnr, csgni, cspnr;
                double cspni, c1r, c1i, c2r, c2i, dfnu, fmr;
                double sgn, yy, znr, zni;
                int inu, iuf, nn, nw = 0;

                double[] cyr = new double[2];
                double[] cyi = new double[2];

                nz = 0;
                znr = -zr;
                zni = -zi;
                az = zabs(zr, zi);
                nn = n;
                dfnu = fnu + (double)(n - 1);
                if (az <= 2.0) goto L10;
                if (az * az * 0.25 > dfnu + 1.0) goto L20;
            L10:
                // -----------------------------------------------------------------------
                //     POWER SERIES FOR THE I FUNCTION
                // -----------------------------------------------------------------------
                zseri(znr, zni, fnu, kode, nn, yr, yi, ref nw, tol, elim, alim);
                goto L40;
            L20:
                if (az < rl) goto L30;
                // -----------------------------------------------------------------------
                //     ASYMPTOTIC EXPANSION FOR LARGE Z FOR THE I FUNCTION
                // -----------------------------------------------------------------------
                zasyi(znr, zni, fnu, kode, nn, yr, yi, ref nw, rl, tol, elim, alim);
                if (nw < 0) goto L80;
                goto L40;
            L30:
                // -----------------------------------------------------------------------
                //     MILLER ALGORITHM NORMALIZED BY THE SERIES FOR THE I FUNCTION
                // -----------------------------------------------------------------------
                zmlri(znr, zni, fnu, kode, nn, yr, yi, ref nw, tol);
                if (nw < 0) goto L80;
            L40:
                // -----------------------------------------------------------------------
                //     ANALYTIC CONTINUATION TO THE LEFT HALF PLANE FOR THE K FUNCTION
                // -----------------------------------------------------------------------
                zbknu(znr, zni, fnu, kode, 1, cyr, cyi, ref nw, tol, elim, alim);
                if (nw != 0) goto L80;
                fmr = (double)mr;
                sgn = -dsign(pi, fmr);
                csgnr = 0.0;
                csgni = sgn;
                if (kode == 1) goto L50;
                yy = -zni;
                csgnr = -csgni * Math.Sin(yy);
                csgni = csgni * Math.Cos(yy);
            L50:
                // -----------------------------------------------------------------------
                //     CALCULATE CSPN=EXP(FNU*PI*I) TO MINIMIZE LOSSES OF SIGNIFICANCE
                //     WHEN FNU IS LARGE
                // -----------------------------------------------------------------------
                inu = (int)fnu;
                arg = (fnu - (double)inu) * sgn;
                cspnr = Math.Cos(arg);
                cspni = Math.Sin(arg);
                if (inu % 2 == 0) goto L60;
                cspnr = -cspnr;
                cspni = -cspni;
            L60:
                c1r = cyr[0];
                c1i = cyi[0];
                c2r = yr[0];
                c2i = yi[0];
                if (kode == 1) goto L70;
                iuf = 0;
                ascle = d1mach(1) * 1.0E3 / tol;
                zs1s2(znr, zni, ref c1r, ref c1i, ref c2r, ref c2i, ref nw, ascle, alim, ref iuf);
                nz += nw;
            L70:
                yr[0] = cspnr * c1r - cspni * c1i + csgnr * c2r - csgni * c2i;
                yi[0] = cspnr * c1i + cspni * c1r + csgnr * c2i + csgni * c2r;
                return 0;
            L80:
                nz = -1;
                if (nw == -2) nz = -2;
                return 0;
            }

            static int zacon(double zr, double zi, double fnu, int kode, int mr, int n, double[] yr, double[] yi, ref int nz, double rl, double fnul, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZACON
                //***REFER TO  ZBESK,ZBESH
                //
                //     ZACON APPLIES THE ANALYTIC CONTINUATION FORMULA
                //
                //         K(FNU,ZN*EXP(MP))=K(FNU,ZN)*EXP(-MP*FNU) - MP*I(FNU,ZN)
                //                 MP=PI*MR*CMPLX(0.0,1.0)
                //
                //     TO CONTINUE THE K FUNCTION FROM THE RIGHT HALF TO THE LEFT
                //     HALF Z PLANE
                //
                //***ROUTINES CALLED  ZBINU,ZBKNU,ZS1S2,D1MACH,ZABS,ZMLT
                //***END PROLOGUE  ZACON

                #endregion

                const double coner = 1.0;
                const double pi = 3.14159265358979323846264338327950;
                const double zeror = 0.0;

                double arg, ascle, as2, azn, bscle, cki;
                double ckr, cpn, cscl, cscr, csgni, csgnr, cspni, cspnr;
                double csr, c1i, c1m, c1r, c2i, c2r, fmr;
                double fn, pti = 0, ptr = 0, razn, rzi, rzr, sc1i, sc1r;
                double sc2i = 0, sc2r = 0, sgn, spn, sti = 0, str = 0, s1i, s1r, s2i, s2r;
                double yy, zni, znr;
                int i, inu, iuf, kflag, nn, nw = 0;

                double[] bry = new double[3];
                double[] csrr = new double[3];
                double[] cssr = new double[3];
                double[] cyi = new double[2];
                double[] cyr = new double[2];

                nz = 0;
                znr = -zr;
                zni = -zi;
                nn = n;
                zbinu(znr, zni, fnu, kode, nn, yr, yi, ref nw, rl, fnul, tol, elim, alim);
                if (nw < 0) goto L90;
                // -----------------------------------------------------------------------
                //     ANALYTIC CONTINUATION TO THE LEFT HALF PLANE FOR THE K FUNCTION
                // -----------------------------------------------------------------------
                nn = Math.Min(2, n);
                zbknu(znr, zni, fnu, kode, nn, cyr, cyi, ref nw, tol, elim, alim);
                if (nw != 0) goto L90;
                s1r = cyr[0];
                s1i = cyi[0];
                fmr = (double)mr;
                sgn = -dsign(pi, fmr);
                csgnr = zeror;
                csgni = sgn;
                if (kode == 1) goto L10;
                yy = -zni;
                cpn = Math.Cos(yy);
                spn = Math.Sin(yy);
                zmlt(csgnr, csgni, cpn, spn, ref csgnr, ref csgni);
            L10:
                // -----------------------------------------------------------------------
                //     CALCULATE CSPN=EXP(FNU*PI*I) TO MINIMIZE LOSSES OF SIGNIFICANCE
                //     WHEN FNU IS LARGE
                // -----------------------------------------------------------------------
                inu = (int)fnu;
                arg = (fnu - (double)inu) * sgn;
                cpn = Math.Cos(arg);
                spn = Math.Sin(arg);
                cspnr = cpn;
                cspni = spn;
                if (inu % 2 == 0) goto L20;
                cspnr = -cspnr;
                cspni = -cspni;
            L20:
                iuf = 0;
                c1r = s1r;
                c1i = s1i;
                c2r = yr[0];
                c2i = yi[0];
                ascle = 1.0E3 * d1mach(1) / tol;
                if (kode == 1) goto L30;
                zs1s2(znr, zni, ref c1r, ref c1i, ref c2r, ref c2i, ref nw, ascle, alim, ref iuf);
                nz += nw;
                sc1r = c1r;
                sc1i = c1i;
            L30:
                zmlt(cspnr, cspni, c1r, c1i, ref str, ref sti);
                zmlt(csgnr, csgni, c2r, c2i, ref ptr, ref pti);
                yr[0] = str + ptr;
                yi[0] = sti + pti;
                if (n == 1) return 0;
                cspnr = -cspnr;
                cspni = -cspni;
                s2r = cyr[1];
                s2i = cyi[1];
                c1r = s2r;
                c1i = s2i;
                c2r = yr[1];
                c2i = yi[1];
                if (kode == 1) goto L40;
                zs1s2(znr, zni, ref c1r, ref c1i, ref c2r, ref c2i, ref nw, ascle, alim, ref iuf);
                nz += nw;
                sc2r = c1r;
                sc2i = c1i;
            L40:
                zmlt(cspnr, cspni, c1r, c1i, ref str, ref sti);
                zmlt(csgnr, csgni, c2r, c2i, ref ptr, ref pti);
                yr[1] = str + ptr;
                yi[1] = sti + pti;
                if (n == 2) return 0;
                cspnr = -cspnr;
                cspni = -cspni;
                azn = zabs(znr, zni);
                razn = 1.0 / azn;
                str = znr * razn;
                sti = -zni * razn;
                rzr = (str + str) * razn;
                rzi = (sti + sti) * razn;
                fn = fnu + 1.0;
                ckr = fn * rzr;
                cki = fn * rzi;
                // -----------------------------------------------------------------------
                //     SCALE NEAR EXPONENT EXTREMES DURING RECURRENCE ON K FUNCTIONS
                // -----------------------------------------------------------------------
                cscl = 1.0 / tol;
                cscr = tol;
                cssr[0] = cscl;
                cssr[1] = coner;
                cssr[2] = cscr;
                csrr[0] = cscr;
                csrr[1] = coner;
                csrr[2] = cscl;
                bry[0] = ascle;
                bry[1] = 1.0 / ascle;
                bry[2] = d1mach(2);
                as2 = zabs(s2r, s2i);
                kflag = 2;
                if (as2 > bry[0]) goto L50;
                kflag = 1;
                goto L60;
            L50:
                if (as2 < bry[1]) goto L60;
                kflag = 3;
            L60:
                bscle = bry[kflag - 1];
                s1r *= cssr[kflag - 1];
                s1i *= cssr[kflag - 1];
                s2r *= cssr[kflag - 1];
                s2i *= cssr[kflag - 1];
                csr = csrr[kflag - 1];
                for (i = 3; i <= n; i++)
                {
                    str = s2r;
                    sti = s2i;
                    s2r = ckr * str - cki * sti + s1r;
                    s2i = ckr * sti + cki * str + s1i;
                    s1r = str;
                    s1i = sti;
                    c1r = s2r * csr;
                    c1i = s2i * csr;
                    str = c1r;
                    sti = c1i;
                    c2r = yr[i - 1];
                    c2i = yi[i - 1];
                    if (kode == 1) goto L70;
                    if (iuf < 0) goto L70;
                    zs1s2(znr, zni, ref c1r, ref c1i, ref c2r, ref c2i, ref nw, ascle, alim, ref iuf);
                    nz += nw;
                    sc1r = sc2r;
                    sc1i = sc2i;
                    sc2r = c1r;
                    sc2i = c1i;
                    if (iuf != 3) goto L70;
                    iuf = -4;
                    s1r = sc1r * cssr[kflag - 1];
                    s1i = sc1i * cssr[kflag - 1];
                    s2r = sc2r * cssr[kflag - 1];
                    s2i = sc2i * cssr[kflag - 1];
                    str = sc2r;
                    sti = sc2i;
            L70:
                    ptr = cspnr * c1r - cspni * c1i;
                    pti = cspnr * c1i + cspni * c1r;
                    yr[i - 1] = ptr + csgnr * c2r - csgni * c2i;
                    yi[i - 1] = pti + csgnr * c2i + csgni * c2r;
                    ckr += rzr;
                    cki += rzi;
                    cspnr = -cspnr;
                    cspni = -cspni;
                    if (kflag >= 3) goto L80;
                    ptr = Math.Abs(c1r);
                    pti = Math.Abs(c1i);
                    c1m = Math.Max(ptr, pti);
                    if (c1m <= bscle) goto L80;
                    kflag++;
                    bscle = bry[kflag - 1];
                    s1r *= csr;
                    s1i *= csr;
                    s2r = str;
                    s2i = sti;
                    s1r *= cssr[kflag - 1];
                    s1i *= cssr[kflag - 1];
                    s2r *= cssr[kflag - 1];
                    s2i *= cssr[kflag - 1];
                    csr = csrr[kflag - 1];
            L80:
                    ;
                }
                return 0;
            L90:
                nz = -1;
                if (nw == -2) nz = -2;
                return 0;
            }

            static int zasyi(double zr, double zi, double fnu, int kode, int n, double[] yr, double[] yi, ref int nz, double rl, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZASYI
                //***REFER TO  ZBESI,ZBESK
                //
                //     ZASYI COMPUTES THE I BESSEL FUNCTION FOR REAL(Z).GE.0.0 BY
                //     MEANS OF THE ASYMPTOTIC EXPANSION FOR LARGE CABS(Z) IN THE
                //     REGION CABS(Z).GT.MAX(RL,FNU*FNU/2). NZ=0 IS A NORMAL RETURN.
                //     NZ.LT.0 INDICATES AN OVERFLOW ON KODE=1.
                //
                //***ROUTINES CALLED  D1MACH,ZABS,ZDIV,ZEXP,ZMLT,ZSQRT
                //***END PROLOGUE  ZASYI

                #endregion

                const double rtpi = .159154943091895336;
                const double zeror = 0.0;
                const double zeroi = 0.0;
                const double coner = 1.0;
                const double conei = 0.0;
                const double pi = 3.14159265358979323846264338327950;

                double aa, aez, ak, ak1i, ak1r, arg, arm, atol;
                double az, bb, bk, cki = 0, ckr = 0, cs1i, cs1r, cs2i, cs2r, czi;
                double czr, dfnu, dki, dkr, dnu2, ezi, ezr, fdn, p1i;
                double p1r, raz, rtr1, rzi, rzr, s, sgn, sqk, sti, str, s2i;
                double s2r, tzi, tzr;
                int i, ib, il, inu, j, jl, k, koded, m, nn;

                nz = 0;
                az = zabs(zr, zi);
                arm = d1mach(1) * 1.0E3;
                rtr1 = Math.Sqrt(arm);
                il = Math.Min(2, n);
                dfnu = fnu + (n - il);
                // -----------------------------------------------------------------------
                //     OVERFLOW TEST
                // -----------------------------------------------------------------------
                raz = 1.0 / az;
                str = zr * raz;
                sti = -zi * raz;
                ak1r = rtpi * str * raz;
                ak1i = rtpi * sti * raz;
                zsqrt(ak1r, ak1i, ref ak1r, ref ak1i);
                czr = zr;
                czi = zi;
                if (kode != 2) goto L10;
                czr = zeror;
                czi = zi;
            L10:
                if (Math.Abs(czr) > elim) goto L100;
                dnu2 = dfnu + dfnu;
                koded = 1;
                if (Math.Abs(czr) > alim && n > 2) goto L20;
                koded = 0;
                zexp(czr, czi, ref str, ref sti);
                zmlt(ak1r, ak1i, str, sti, ref ak1r, ref ak1i);
            L20:
                fdn = 0.0;
                if (dnu2 > rtr1) fdn = dnu2 * dnu2;
                ezr = zr * 8.0;
                ezi = zi * 8.0;
                // -----------------------------------------------------------------------
                //     WHEN Z IS IMAGINARY, THE ERROR TEST MUST BE MADE RELATIVE TO THE
                //     FIRST RECIPROCAL POWER SINCE THIS IS THE LEADING TERM OF THE
                //     EXPANSION FOR THE IMAGINARY PART.
                // -----------------------------------------------------------------------
                aez = 8.0 * az;
                s = tol / aez;
                jl = (int)(rl + rl) + 2;
                p1r = zeror;
                p1i = zeroi;
                if (zi == 0.0) goto L30;
                // -----------------------------------------------------------------------
                //     CALCULATE EXP(PI*(0.5+FNU+N-IL)*I) TO MINIMIZE LOSSES OF
                //     SIGNIFICANCE WHEN FNU OR N IS LARGE
                // -----------------------------------------------------------------------
                inu = (int)fnu;
                arg = (fnu - (double)inu) * pi;
                inu = inu + n - il;
                ak = -Math.Sin(arg);
                bk = Math.Cos(arg);
                if (zi < 0.0) bk = -bk;
                p1r = ak;
                p1i = bk;
                if (inu % 2 == 0) goto L30;
                p1r = -p1r;
                p1i = -p1i;
            L30:
                for (k = 1; k <= il; k++)
                {
                    sqk = fdn - 1.0;
                    atol = s * Math.Abs(sqk);
                    sgn = 1.0;
                    cs1r = coner;
                    cs1i = conei;
                    cs2r = coner;
                    cs2i = conei;
                    ckr = coner;
                    cki = conei;
                    ak = 0.0;
                    aa = 1.0;
                    bb = aez;
                    dkr = ezr;
                    dki = ezi;
                    for (j = 1; j <= jl; j++)
                    {
                        zdiv(ckr, cki, dkr, dki, ref str, ref sti);
                        ckr = str * sqk;
                        cki = sti * sqk;
                        cs2r += ckr;
                        cs2i += cki;
                        sgn = -sgn;
                        cs1r += ckr * sgn;
                        cs1i += cki * sgn;
                        dkr += ezr;
                        dki += ezi;
                        aa = aa * Math.Abs(sqk) / bb;
                        bb += aez;
                        ak += 8.0;
                        sqk -= ak;
                        if (aa <= atol) goto L50;
                    }
                    goto L110;
            L50:
                    s2r = cs1r;
                    s2i = cs1i;
                    if (zr + zr >= elim) goto L60;
                    tzr = zr + zr;
                    tzi = zi + zi;
                    zexp(-tzr, -tzi, ref str, ref sti);
                    zmlt(str, sti, p1r, p1i, ref str, ref sti);
                    zmlt(str, sti, cs2r, cs2i, ref str, ref sti);
                    s2r += str;
                    s2i += sti;
            L60:
                    fdn = fdn + dfnu * 8.0 + 4.0;
                    p1r = -p1r;
                    p1i = -p1i;
                    m = n - il + k;
                    yr[m - 1] = s2r * ak1r - s2i * ak1i;
                    yi[m - 1] = s2r * ak1i + s2i * ak1r;
                }
                if (n <= 2) return 0;
                nn = n;
                k = nn - 2;
                ak = (double)k;
                str = zr * raz;
                sti = -zi * raz;
                rzr = (str + str) * raz;
                rzi = (sti + sti) * raz;
                ib = 3;
                for (i = ib; i <= nn; i++)
                {
                    yr[k - 1] = (ak + fnu) * (rzr * yr[k] - rzi * yi[k]) + yr[k + 1];
                    yi[k - 1] = (ak + fnu) * (rzr * yi[k] + rzi * yr[k]) + yi[k + 1];
                    ak = ak - 1.0;
                    k--;
                }
                if (koded == 0) return 0;
                zexp(czr, czi, ref ckr, ref cki);
                for (i = 1; i <= nn; i++)
                {
                    str = yr[i - 1] * ckr - yi[i - 1] * cki;
                    yi[i - 1] = yr[i] * cki + yi[i - 1] * ckr;
                    yr[i - 1] = str;
                }
                return 0;
            L100:
                nz = -1;
                return 0;
            L110:
                nz = -2;
                return 0;
            }

            static int zbinu(double zr, double zi, double fnu, int kode, int n, double[] cyr, double[] cyi, ref int nz, double rl, double fnul, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZBINU
                //***REFER TO  ZBESH,ZBESI,ZBESJ,ZBESK,ZAIRY,ZBIRY
                //
                //     ZBINU COMPUTES THE I FUNCTION IN THE RIGHT HALF Z PLANE
                //
                //***ROUTINES CALLED  ZABS,ZASYI,ZBUNI,ZMLRI,ZSERI,ZUOIK,ZWRSK
                //***END PROLOGUE  ZBINU

                #endregion

                const double zeror = 0.0;
                const double zeroi = 0.0;

                double az, dfnu;
                int i, inw, nlast = 0, nn, nui = 0, nw = 0;

                double[] cwi = new double[2];
                double[] cwr = new double[2];

                nz = 0;
                az = zabs(zr, zi);
                nn = n;
                dfnu = fnu + (double)(n - 1);
                if (az <= 2.0) goto L10;
                if (az * az * 0.25 > dfnu + 1.0) goto L20;
            L10:
                // -----------------------------------------------------------------------
                //     POWER SERIES
                // -----------------------------------------------------------------------
                zseri(zr, zi, fnu, kode, nn, cyr, cyi, ref nw, tol, elim, alim);
                inw = Math.Abs(nw);
                nz = nz + inw;
                nn = nn - inw;
                if (nn == 0) return 0;
                if (nw >= 0) goto L120;
                dfnu = fnu + (double)(nn - 1);
            L20:
                if (az < rl) goto L40;
                if (dfnu <= 1.0) goto L30;
                if (az + az < dfnu * dfnu) goto L50;
                // -----------------------------------------------------------------------
                //     ASYMPTOTIC EXPANSION FOR LARGE Z
                // -----------------------------------------------------------------------
            L30:
                zasyi(zr, zi, fnu, kode, nn, cyr, cyi, ref nw, rl, tol, elim, alim);
                if (nw < 0) goto L130;
                goto L120;
            L40:
                if (dfnu <= 1.0) goto L70;
            L50:
                // -----------------------------------------------------------------------
                //     OVERFLOW AND UNDERFLOW TEST ON I SEQUENCE FOR MILLER ALGORITHM
                // -----------------------------------------------------------------------
                zuoik(zr, zi, fnu, kode, 1, nn, cyr, cyi, ref nw, tol, elim, alim);
                if (nw < 0) goto L130;
                nz = nz + nw;
                nn = nn - nw;
                if (nn == 0) return 0;
                dfnu = fnu + (double)(nn - 1);
                if (dfnu > fnul) goto L110;
                if (az > fnul) goto L110;
            L60:
                if (az > rl) goto L80;
            L70:
                // -----------------------------------------------------------------------
                //     MILLER ALGORITHM NORMALIZED BY THE SERIES
                // -----------------------------------------------------------------------
                zmlri(zr, zi, fnu, kode, nn, cyr, cyi, ref nw, tol);
                if (nw < 0) goto L130;
                goto L120;
            L80:
                // -----------------------------------------------------------------------
                //     MILLER ALGORITHM NORMALIZED BY THE WRONSKIAN
                // -----------------------------------------------------------------------
                // -----------------------------------------------------------------------
                //     OVERFLOW TEST ON K FUNCTIONS USED IN WRONSKIAN
                // -----------------------------------------------------------------------
                zuoik(zr, zi, fnu, kode, 2, 2, cwr, cwi, ref nw, tol, elim, alim);
                if (nw >= 0) goto L100;
                nz = nn;
                for (i = 1; i <= nn; i++)
                {
                    cyr[i - 1] = zeror;
                    cyi[i - 1] = zeroi;
                }
                return 0;
            L100:
                if (nw > 0) goto L130;
                zwrsk(zr, zi, fnu, kode, nn, cyr, cyi, ref nw, cwr, cwi, tol, elim, alim);
                if (nw < 0) goto L130;
                goto L120;
            L110:
                // -----------------------------------------------------------------------
                //     INCREMENT FNU+NN-1 UP TO FNUL, COMPUTE AND RECUR BACKWARD
                // -----------------------------------------------------------------------
                nui = (int)(fnul - dfnu) + 1;
                nui = Math.Max(nui, 0);
                zbuni(zr, zi, fnu, kode, nn, cyr, cyi, ref nw, nui, ref nlast, fnul, tol, elim, alim);
                if (nw < 0) goto L130;
                nz = nz + nw;
                if (nlast == 0) goto L120;
                nn = nlast;
                goto L60;
            L120:
                return 0;
            L130:
                nz = -1;
                if (nw == -2) nz = -2;
                return 0;
            }

            static int zbknu(double zr, double zi, double fnu, int kode, int n, double[] yr, double[] yi, ref int nz, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZBKNU
                //***REFER TO  ZBESI,ZBESK,ZAIRY,ZBESH
                //
                //     ZBKNU COMPUTES THE K BESSEL FUNCTION IN THE RIGHT HALF Z PLANE.
                //
                //***ROUTINES CALLED  DGAMLN,I1MACH,D1MACH,ZKSCL,ZSHCH,ZUCHK,ZABS,ZDIV,
                //                    ZEXP,ZLOG,ZMLT,ZSQRT
                //***END PROLOGUE  ZBKNU

                #endregion

                const int kmax = 30;
                const double czeror = 0.0;
                const double czeroi = 0.0;
                const double coner = 1.0;
                const double conei = 0.0;
                const double ctwor = 2.0;
                const double dpi = 3.14159265358979323846264338327950;
                const double r1 = 2.0;
                const double rthpi = 1.25331413731550025;
                const double spi = 1.90985931710274403; // 6/pi
                const double hpi = 1.570796326794896619231321696;
                const double fpi = 1.89769999331517738;
                const double tth = .666666666666666666;

                double[] cc = {
                    0.577215664901532861,
                    -0.0420026350340952355,
                    -0.0421977345555443367,
                    0.00721894324666309954,
                    -2.15241674114950973e-4,
                    -2.01348547807882387e-5,
                    1.13302723198169588e-6,
                    6.11609510448141582e-9 };

                double aa, ak, ascle, a1, a2, bb, bk, caz;
                double cbi, cbr, cchi = 0, cchr = 0, cki = 0, ckr = 0, coefi = 0, coefr = 0;
                double crscr, csclr, cshi = 0, cshr = 0, csi, csr;
                double czi = 0, czr = 0, dnu, dnu2 = 0, etest, fc, fhs;
                double fi, fk, fks, fmui, fmur, fr, g1, g2, pi, pr, pti = 0;
                double ptr = 0, p1i, p1r, p2i, p2m, p2r, qi, qr, rak, rcaz, rzi;
                double rzr, s, smui = 0, smur = 0, sti, str, s1i, s1r, s2i = 0, s2r = 0, tm;
                double t1, t2, elm;
                double celmr, zdr, zdi, alas, helim;
                int i, idum = 0, iflag, inu, k, kflag, kk, koded;
                int j, ic, inub, nw = 0;

                double[] cyr = new double[2];
                double[] cyi = new double[2];
                double[] cssr = new double[3];
                double[] csrr = new double[3];
                double[] bry = new double[3];

                caz = zabs(zr, zi);
                csclr = 1.0 / tol;
                crscr = tol;
                cssr[0] = csclr;
                cssr[1] = 1.0;
                cssr[2] = crscr;
                csrr[0] = crscr;
                csrr[1] = 1.0;
                csrr[2] = csclr;
                bry[0] = 1.0E3 * d1mach(1) / tol;
                bry[1] = 1.0 / bry[0];
                bry[2] = d1mach(2);
                nz = 0;
                iflag = 0;
                koded = kode;
                rcaz = 1.0 / caz;
                str = zr * rcaz;
                sti = -zi * rcaz;
                rzr = (str + str) * rcaz;
                rzi = (sti + sti) * rcaz;
                inu = (int)(fnu + 0.5);
                dnu = fnu - (double)inu;
                if (Math.Abs(dnu) == 0.5) goto L110;
                dnu2 = 0.0;
                if (Math.Abs(dnu) > tol) dnu2 = dnu * dnu;
                if (caz > r1) goto L110;
                //-----------------------------------------------------------------------
                //    SERIES FOR ABS(Z).LE.R1
                //-----------------------------------------------------------------------
                fc = 1.0;
                zlog(rzr, rzi, ref smur, ref smui, ref idum);
                fmur = smur * dnu;
                fmui = smui * dnu;
                zshch(fmur, fmui, ref cshr, ref cshi, ref cchr, ref cchi);
                if (dnu == 0.0) goto L10;
                fc = dnu * dpi;
                fc = fc / Math.Sin(fc);
                smur = cshr / dnu;
                smui = cshi / dnu;
            L10:
                a2 = 1.0 + dnu;
                //-----------------------------------------------------------------------
                //    GAM(1-Z)*GAM(1+Z)=PI*Z/SIN(PI*Z), T1=1/GAM(1-DNU), T2=1/GAM(1+DNU)
                //-----------------------------------------------------------------------
                t2 = Math.Exp(-dgamln(a2, ref idum));
                t1 = 1.0 / (t2 * fc);
                if (Math.Abs(dnu) > 0.1) goto L40;
                //-----------------------------------------------------------------------
                //    SERIES FOR F0 TO RESOLVE INDETERMINACY FOR SMALL ABS(DNU)
                //-----------------------------------------------------------------------
                ak = 1.0;
                s = cc[0];
                for (k = 2; k <= 8; k++)
                {
                    ak = ak * dnu2;
                    tm = cc[k - 1] * ak;
                    s = s + tm;
                    if (Math.Abs(tm) < tol) goto L30;
                }
            L30:
                g1 = -s;
                goto L50;
            L40:
                g1 = (t1 - t2) / (dnu + dnu);
            L50:
                g2 = (t1 + t2) * 0.5;
                fr = fc * (cchr * g1 + smur * g2);
                fi = fc * (cchi * g1 + smui * g2);
                zexp(fmur, fmui, ref str, ref sti);
                pr = 0.5 * str / t2;
                pi = 0.5 * sti / t2;
                zdiv(0.5, 0.0, str, sti, ref ptr, ref pti);
                qr = ptr / t1;
                qi = pti / t1;
                s1r = fr;
                s1i = fi;
                s2r = pr;
                s2i = pi;
                ak = 1.0;
                a1 = 1.0;
                ckr = coner;
                cki = conei;
                bk = 1.0 - dnu2;
                if (inu > 0 || n > 1) goto L80;
                //-----------------------------------------------------------------------
                //    GENERATE K(FNU,Z), 0.0D0 .LE. FNU .LT. 0.5D0 AND N=1
                //-----------------------------------------------------------------------
                if (caz < tol) goto L70;
                zmlt(zr, zi, zr, zi, ref czr, ref czi);
                czr = 0.25 * czr;
                czi = 0.25 * czi;
                t1 = 0.25 * caz * caz;
            L60:
                fr = (fr * ak + pr + qr) / bk;
                fi = (fi * ak + pi + qi) / bk;
                str = 1.0 / (ak - dnu);
                pr = pr * str;
                pi = pi * str;
                str = 1.0 / (ak + dnu);
                qr = qr * str;
                qi = qi * str;
                str = ckr * czr - cki * czi;
                rak = 1.0 / ak;
                cki = (ckr * czi + cki * czr) * rak;
                ckr = str * rak;
                s1r = ckr * fr - cki * fi + s1r;
                s1i = ckr * fi + cki * fr + s1i;
                a1 = a1 * t1 * rak;
                bk = bk + ak + ak + 1.0;
                ak = ak + 1.0;
                if (a1 > tol) goto L60;
            L70:
                yr[0] = s1r;
                yi[0] = s1i;
                if (koded == 1) return 0;
                zexp(zr, zi, ref str, ref sti);
                zmlt(s1r, s1i, str, sti, ref yr[0], ref yi[0]);
                return 0;
                //-----------------------------------------------------------------------
                //    GENERATE K(DNU,Z) AND K(DNU+1,Z) FOR FORWARD RECURRENCE
                //-----------------------------------------------------------------------
            L80:
                if (caz < tol) goto L100;
                zmlt(zr, zi, zr, zi, ref czr, ref czi);
                czr = 0.25 * czr;
                czi = 0.25 * czi;
                t1 = 0.25 * caz * caz;
            L90:
                fr = (fr * ak + pr + qr) / bk;
                fi = (fi * ak + pi + qi) / bk;
                str = 1.0 / (ak - dnu);
                pr = pr * str;
                pi = pi * str;
                str = 1.0 / (ak + dnu);
                qr = qr * str;
                qi = qi * str;
                str = ckr * czr - cki * czi;
                rak = 1.0 / ak;
                cki = (ckr * czi + cki * czr) * rak;
                ckr = str * rak;
                s1r = ckr * fr - cki * fi + s1r;
                s1i = ckr * fi + cki * fr + s1i;
                str = pr - fr * ak;
                sti = pi - fi * ak;
                s2r = ckr * str - cki * sti + s2r;
                s2i = ckr * sti + cki * str + s2i;
                a1 = a1 * t1 * rak;
                bk = bk + ak + ak + 1.0;
                ak = ak + 1.0;
                if (a1 > tol) goto L90;
            L100:
                kflag = 2;
                a1 = fnu + 1.0;
                ak = a1 * Math.Abs(smur);
                if (ak > alim) kflag = 3;
                str = cssr[kflag - 1];
                p2r = s2r * str;
                p2i = s2i * str;
                zmlt(p2r, p2i, rzr, rzi, ref s2r, ref s2i);
                s1r = s1r * str;
                s1i = s1i * str;
                if (koded == 1) goto L210;
                zexp(zr, zi, ref fr, ref fi);
                zmlt(s1r, s1i, fr, fi, ref s1r, ref s1i);
                zmlt(s2r, s2i, fr, fi, ref s2r, ref s2i);
                goto L210;
                //-----------------------------------------------------------------------
                //    IFLAG=0 MEANS NO UNDERFLOW OCCURRED
                //    IFLAG=1 MEANS AN UNDERFLOW OCCURRED- COMPUTATION PROCEEDS WITH
                //    KODED=2 AND A TEST FOR ON SCALE VALUES IS MADE DURING FORWARD
                //    RECURSION
                //-----------------------------------------------------------------------
            L110:
                zsqrt(zr, zi, ref str, ref sti);
                zdiv(rthpi, czeroi, str, sti, ref coefr, ref coefi);
                kflag = 2;
                if (koded == 2) goto L120;
                if (zr > alim) goto L290;
                //    BLANK LINE
                str = Math.Exp(-zr) * cssr[kflag - 1];
                sti = -str * Math.Sin(zi);
                str = str * Math.Cos(zi);
                zmlt(coefr, coefi, str, sti, ref coefr, ref coefi);
            L120:
                if (Math.Abs(dnu) == 0.5) goto L300;
                //-----------------------------------------------------------------------
                //    MILLER ALGORITHM FOR ABS(Z).GT.R1
                //-----------------------------------------------------------------------
                ak = Math.Cos(dpi * dnu);
                ak = Math.Abs(ak);
                if (ak == czeror) goto L300;
                fhs = Math.Abs(0.25 - dnu2);
                if (fhs == czeror) goto L300;
                //-----------------------------------------------------------------------
                //    COMPUTE R2=F(E). IF ABS(Z).GE.R2, USE FORWARD RECURRENCE TO
                //    DETERMINE THE BACKWARD INDEX K. R2=F(E) IS A STRAIGHT LINE ON
                //    12.LE.E.LE.60. E IS COMPUTED FROM 2**(-E)=B**(1-I1MACH(14))=
                //    TOL WHERE B IS THE BASE OF THE ARITHMETIC.
                //-----------------------------------------------------------------------
                t1 = (double)(i1mach(14) - 1);
                t1 = t1 * d1mach(5) * 3.321928094;
                t1 = Math.Max(t1, 12.0);
                t1 = Math.Min(t1, 60.0);
                t2 = tth * t1 - 6.0;
                if (zr != 0.0) goto L130;
                t1 = hpi;
                goto L140;
            L130:
                t1 = Math.Atan(zi / zr);
                t1 = Math.Abs(t1);
            L140:
                if (t2 > caz) goto L170;
                //-----------------------------------------------------------------------
                //    FORWARD RECURRENCE LOOP WHEN ABS(Z).GE.R2
                //-----------------------------------------------------------------------
                etest = ak / (dpi * caz * tol);
                fk = coner;
                if (etest < coner) goto L180;
                fks = ctwor;
                ckr = caz + caz + ctwor;
                p1r = czeror;
                p2r = coner;
                for (i = 1; i <= kmax; i++)
                {
                    ak = fhs / fks;
                    cbr = ckr / (fk + coner);
                    ptr = p2r;
                    p2r = cbr * p2r - p1r * ak;
                    p1r = ptr;
                    ckr = ckr + ctwor;
                    fks = fks + fk + fk + ctwor;
                    fhs = fhs + fk + fk;
                    fk = fk + coner;
                    str = Math.Abs(p2r) * fk;
                    if (etest < str) goto L160;
                }
                goto L310;
            L160:
                fk = fk + spi * t1 * Math.Sqrt(t2 / caz);
                fhs = Math.Abs(0.25 - dnu2);
                goto L180;
            L170:
                //-----------------------------------------------------------------------
                //    COMPUTE BACKWARD INDEX K FOR ABS(Z).LT.R2
                //-----------------------------------------------------------------------
                a2 = Math.Sqrt(caz);
                ak = fpi * ak / (tol * Math.Sqrt(a2));
                aa = 3.0 * t1 / (1.0 + caz);
                bb = 14.7 * t1 / (28.0 + caz);
                ak = (Math.Log(ak) + caz * Math.Cos(aa) / (1.0 + 0.008 * caz)) / Math.Cos(bb);
                fk = 0.12125 * ak * ak / caz + 1.5;
            L180:
                //-----------------------------------------------------------------------
                //    BACKWARD RECURRENCE LOOP FOR MILLER ALGORITHM
                //-----------------------------------------------------------------------
                k = (int)fk;
                fk = (double)k;
                fks = fk * fk;
                p1r = czeror;
                p1i = czeroi;
                p2r = tol;
                p2i = czeroi;
                csr = p2r;
                csi = p2i;
                for (i = 1; i <= k; i++)
                {
                    a1 = fks - fk;
                    ak = (fks + fk) / (a1 + fhs);
                    rak = 2.0 / (fk + coner);
                    cbr = (fk + zr) * rak;
                    cbi = zi * rak;
                    ptr = p2r;
                    pti = p2i;
                    p2r = (ptr * cbr - pti * cbi - p1r) * ak;
                    p2i = (pti * cbr + ptr * cbi - p1i) * ak;
                    p1r = ptr;
                    p1i = pti;
                    csr = csr + p2r;
                    csi = csi + p2i;
                    fks = a1 - fk + coner;
                    fk = fk - coner;
                }
                //-----------------------------------------------------------------------
                //    COMPUTE (P2/CS)=(P2/ABS(CS))*(CONJG(CS)/ABS(CS)) FOR BETTER
                //    SCALING
                //-----------------------------------------------------------------------
                tm = zabs(csr, csi);
                ptr = 1.0 / tm;
                s1r = p2r * ptr;
                s1i = p2i * ptr;
                csr = csr * ptr;
                csi = -csi * ptr;
                zmlt(coefr, coefi, s1r, s1i, ref str, ref sti);
                zmlt(str, sti, csr, csi, ref s1r, ref s1i);
                if (inu > 0 || n > 1) goto L200;
                zdr = zr;
                zdi = zi;
                if (iflag == 1) goto L270;
                goto L240;
            L200:
                //-----------------------------------------------------------------------
                //    COMPUTE P1/P2=(P1/ABS(P2)*CONJG(P2)/ABS(P2) FOR SCALING
                //-----------------------------------------------------------------------
                tm = zabs(p2r, p2i);
                ptr = 1.0 / tm;
                p1r = p1r * ptr;
                p1i = p1i * ptr;
                p2r = p2r * ptr;
                p2i = -p2i * ptr;
                zmlt(p1r, p1i, p2r, p2i, ref ptr, ref pti);
                str = dnu + 0.5 - ptr;
                sti = -pti;
                zdiv(str, sti, zr, zi, ref str, ref sti);
                str = str + 1.0;
                zmlt(str, sti, s1r, s1i, ref s2r, ref s2i);
                //-----------------------------------------------------------------------
                //    FORWARD RECURSION ON THE THREE TERM RECURSION WITH RELATION WITH
                //    SCALING NEAR EXPONENT EXTREMES ON KFLAG=1 OR KFLAG=3
                //-----------------------------------------------------------------------
            L210:
                str = dnu + 1.0;
                ckr = str * rzr;
                cki = str * rzi;
                if (n == 1) inu = inu - 1;
                if (inu > 0) goto L220;
                if (n > 1) goto L215;
                s1r = s2r;
                s1i = s2i;
            L215:
                zdr = zr;
                zdi = zi;
                if (iflag == 1) goto L270;
                goto L240;
            L220:
                inub = 1;
                if (iflag == 1) goto L261;
            L225:
                p1r = csrr[kflag - 1];
                ascle = bry[kflag - 1];
                for (i = inub; i <= inu; i++)
                {
                    str = s2r;
                    sti = s2i;
                    s2r = ckr * str - cki * sti + s1r;
                    s2i = ckr * sti + cki * str + s1i;
                    s1r = str;
                    s1i = sti;
                    ckr = ckr + rzr;
                    cki = cki + rzi;
                    if (kflag >= 3) goto L230;
                    p2r = s2r * p1r;
                    p2i = s2i * p1r;
                    str = Math.Abs(p2r);
                    sti = Math.Abs(p2i);
                    p2m = Math.Max(str, sti);
                    if (p2m <= ascle) goto L230;
                    kflag = kflag + 1;
                    ascle = bry[kflag - 1];
                    s1r = s1r * p1r;
                    s1i = s1i * p1r;
                    s2r = p2r;
                    s2i = p2i;
                    str = cssr[kflag - 1];
                    s1r = s1r * str;
                    s1i = s1i * str;
                    s2r = s2r * str;
                    s2i = s2i * str;
                    p1r = csrr[kflag - 1];
            L230:
                    ;
                }
                if (n != 1) goto L240;
                s1r = s2r;
                s1i = s2i;
            L240:
                str = csrr[kflag - 1];
                yr[0] = s1r * str;
                yi[0] = s1i * str;
                if (n == 1) return 0;
                yr[1] = s2r * str;
                yi[1] = s2i * str;
                if (n == 2) return 0;
                kk = 2;
            L250:
                kk = kk + 1;
                if (kk > n) return 0;
                p1r = csrr[kflag - 1];
                ascle = bry[kflag - 1];
                for (i = kk; i <= n; i++)
                {
                    p2r = s2r;
                    p2i = s2i;
                    s2r = ckr * p2r - cki * p2i + s1r;
                    s2i = cki * p2r + ckr * p2i + s1i;
                    s1r = p2r;
                    s1i = p2i;
                    ckr = ckr + rzr;
                    cki = cki + rzi;
                    p2r = s2r * p1r;
                    p2i = s2i * p1r;
                    yr[i - 1] = p2r;
                    yi[i - 1] = p2i;
                    if (kflag >= 3) goto L260;
                    str = Math.Abs(p2r);
                    sti = Math.Abs(p2i);
                    p2m = Math.Max(str, sti);
                    if (p2m <= ascle) goto L260;
                    kflag = kflag + 1;
                    ascle = bry[kflag - 1];
                    s1r = s1r * p1r;
                    s1i = s1i * p1r;
                    s2r = p2r;
                    s2i = p2i;
                    str = cssr[kflag - 1];
                    s1r = s1r * str;
                    s1i = s1i * str;
                    s2r = s2r * str;
                    s2i = s2i * str;
                    p1r = csrr[kflag - 1];
            L260:
                    ;
                }
                return 0;
                //-----------------------------------------------------------------------
                //    IFLAG=1 CASES, FORWARD RECURRENCE ON SCALED VALUES ON UNDERFLOW
                //-----------------------------------------------------------------------
            L261:
                helim = 0.5 * elim;
                elm = Math.Exp(-elim);
                celmr = elm;
                ascle = bry[0];
                zdr = zr;
                zdi = zi;
                ic = -1;
                j = 2;
                for (i = 1; i <= inu; i++)
                {
                    str = s2r;
                    sti = s2i;
                    s2r = str * ckr - sti * cki + s1r;
                    s2i = sti * ckr + str * cki + s1i;
                    s1r = str;
                    s1i = sti;
                    ckr = ckr + rzr;
                    cki = cki + rzi;
                    alas = Math.Log(zabs(s2r, s2i)); //as = zabs(s2r, s2i); alas = Math.Log(as);
                    p2r = -zdr + alas;
                    if (p2r < -elim) goto L263;
                    zlog(s2r, s2i, ref str, ref sti, ref idum);
                    p2r = -zdr + str;
                    p2i = -zdi + sti;
                    p2m = Math.Exp(p2r) / tol;
                    p1r = p2m * Math.Cos(p2i);
                    p1i = p2m * Math.Sin(p2i);
                    zuchk(p1r, p1i, ref nw, ascle, tol);
                    if (nw != 0) goto L263;
                    j = 3 - j;
                    cyr[j - 1] = p1r;
                    cyi[j - 1] = p1i;
                    if (ic == i - 1) goto L264;
                    ic = i;
                    goto L262;
            L263:
                    if (alas < helim) goto L262;
                    zdr = zdr - elim;
                    s1r = s1r * celmr;
                    s1i = s1i * celmr;
                    s2r = s2r * celmr;
                    s2i = s2i * celmr;
            L262:
                    ;
                }
                if (n != 1) goto L270;
                s1r = s2r;
                s1i = s2i;
                goto L270;
            L264:
                kflag = 1;
                inub = i + 1;
                s2r = cyr[j - 1];
                s2i = cyi[j - 1];
                j = 3 - j;
                s1r = cyr[j - 1];
                s1i = cyi[j - 1];
                if (inub <= inu) goto L225;
                if (n != 1) goto L240;
                s1r = s2r;
                s1i = s2i;
                goto L240;
            L270:
                yr[0] = s1r;
                yi[0] = s1i;
                if (n == 1) goto L280;
                yr[1] = s2r;
                yi[1] = s2i;
            L280:
                ascle = bry[0];
                zkscl(zdr, zdi, fnu, n, yr, yi, ref nz, rzr, rzi, ascle, tol, elim);
                inu = n - nz;
                if (inu <= 0) return 0;
                kk = nz + 1;
                s1r = yr[kk - 1];
                s1i = yi[kk - 1];
                yr[kk - 1] = s1r * csrr[0];
                yi[kk - 1] = s1i * csrr[0];
                if (inu == 1) return 0;
                kk = nz + 2;
                s2r = yr[kk - 1];
                s2i = yi[kk - 1];
                yr[kk - 1] = s2r * csrr[0];
                yi[kk - 1] = s2i * csrr[0];
                if (inu == 2) return 0;
                t2 = fnu + (double)(kk - 1);
                ckr = t2 * rzr;
                cki = t2 * rzi;
                kflag = 1;
                goto L250;
            L290:
                //-----------------------------------------------------------------------
                //    SCALE BY EXP(Z), IFLAG = 1 CASES
                //-----------------------------------------------------------------------
                koded = 2;
                iflag = 1;
                kflag = 2;
                goto L120;
                //-----------------------------------------------------------------------
                //    FNU=HALF ODD INTEGER CASE, DNU=-0.5
                //-----------------------------------------------------------------------
            L300:
                s1r = coefr;
                s1i = coefi;
                s2r = coefr;
                s2i = coefi;
                goto L210;
            L310:
                nz = -2;
                return 0;
            }

            static int zbuni(double zr, double zi, double fnu, int kode, int n, double[] yr, double[] yi, ref int nz, int nui, ref int nlast, double fnul, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZBUNI
                //***REFER TO  ZBESI,ZBESK
                //
                //     ZBUNI COMPUTES THE I BESSEL FUNCTION FOR LARGE CABS(Z).GT.
                //     FNUL AND FNU+N-1.LT.FNUL. THE ORDER IS INCREASED FROM
                //     FNU+N-1 GREATER THAN FNUL BY ADDING NUI AND COMPUTING
                //     ACCORDING TO THE UNIFORM ASYMPTOTIC EXPANSION FOR I(FNU,Z)
                //     ON IFORM=1 AND THE EXPANSION FOR J(FNU,Z) ON IFORM=2
                //
                //***ROUTINES CALLED  ZUNI1,ZUNI2,ZABS,D1MACH
                //***END PROLOGUE  ZBUNI

                #endregion

                double ax, ay, csclr, cscrr, dfnu;
                double fnui, gnu, raz, rzi, rzr, sti, str, s1i, s1r;
                double s2i, s2r, ascle, c1r, c1i, c1m;
                int i, iflag, iform, k, nl, nw = 0;

                double[] cyi = new double[2];
                double[] cyr = new double[2];
                double[] bry = new double[3];

                nz = 0;
                ax = Math.Abs(zr) * 1.7321;
                ay = Math.Abs(zi);
                iform = 1;
                if (ay > ax) iform = 2;
                if (nui == 0) goto L60;
                fnui = (double)nui;
                dfnu = fnu + (double)(n - 1);
                gnu = dfnu + fnui;
                if (iform == 2) goto L10;
                // -----------------------------------------------------------------------
                //     ASYMPTOTIC EXPANSION FOR I(FNU,Z) FOR LARGE FNU APPLIED IN
                //     -PI/3.LE.ARG(Z).LE.PI/3
                // -----------------------------------------------------------------------
                zuni1(zr, zi, gnu, kode, 2, cyr, cyi, ref nw, ref nlast, fnul, tol, elim, alim);
                goto L20;
            L10:
                // -----------------------------------------------------------------------
                //     ASYMPTOTIC EXPANSION FOR J(FNU,Z*EXP(M*HPI)) FOR LARGE FNU
                //     APPLIED IN PI/3.LT.ABS(ARG(Z)).LE.PI/2 WHERE M=+I OR -I
                //     AND HPI=PI/2
                // -----------------------------------------------------------------------
                zuni2(zr, zi, gnu, kode, 2, cyr, cyi, ref nw, ref nlast, fnul, tol, elim, alim);
            L20:
                if (nw < 0) goto L50;
                if (nw != 0) goto L90;
                str = zabs(cyr[0], cyi[0]);
                // ----------------------------------------------------------------------
                //     SCALE BACKWARD RECURRENCE, BRY(3) IS DEFINED BUT NEVER USED
                // ----------------------------------------------------------------------
                bry[0] = d1mach(1) * 1.0E3 / tol;
                bry[1] = 1.0 / bry[0];
                bry[2] = bry[1];
                iflag = 2;
                ascle = bry[1];
                csclr = 1.0;
                if (str > bry[0]) goto L21;
                iflag = 1;
                ascle = bry[0];
                csclr = 1.0 / tol;
                goto L25;
            L21:
                if (str < bry[1]) goto L25;
                iflag = 3;
                ascle = bry[2];
                csclr = tol;
            L25:
                cscrr = 1.0 / csclr;
                s1r = cyr[1] * csclr;
                s1i = cyi[1] * csclr;
                s2r = cyr[0] * csclr;
                s2i = cyi[0] * csclr;
                raz = 1.0 / zabs(zr, zi);
                str = zr * raz;
                sti = -zi * raz;
                rzr = (str + str) * raz;
                rzi = (sti + sti) * raz;
                for (i = 1; i <= nui; i++)
                {
                    str = s2r;
                    sti = s2i;
                    s2r = (dfnu + fnui) * (rzr * str - rzi * sti) + s1r;
                    s2i = (dfnu + fnui) * (rzr * sti + rzi * str) + s1i;
                    s1r = str;
                    s1i = sti;
                    fnui += -1.0;
                    if (iflag >= 3) goto L30;
                    str = s2r * cscrr;
                    sti = s2i * cscrr;
                    c1r = Math.Abs(str);
                    c1i = Math.Abs(sti);
                    c1m = Math.Max(c1r, c1i);
                    if (c1m <= ascle) goto L30;
                    iflag++;
                    ascle = bry[iflag - 1];
                    s1r *= cscrr;
                    s1i *= cscrr;
                    s2r = str;
                    s2i = sti;
                    csclr *= tol;
                    cscrr = 1.0 / csclr;
                    s1r *= csclr;
                    s1i *= csclr;
                    s2r *= csclr;
                    s2i *= csclr;
            L30:
                    ;
                }
                yr[n - 1] = s2r * cscrr;
                yi[n - 1] = s2i * cscrr;
                if (n == 1) return 0;
                nl = n - 1;
                fnui = (double)nl;
                k = nl;
                for (i = 1; i <= nl; i++)
                {
                    str = s2r;
                    sti = s2i;
                    s2r = (fnu + fnui) * (rzr * str - rzi * sti) + s1r;
                    s2i = (fnu + fnui) * (rzr * sti + rzi * str) + s1i;
                    s1r = str;
                    s1i = sti;
                    str = s2r * cscrr;
                    sti = s2i * cscrr;
                    yr[k - 1] = str;
                    yi[k - 1] = sti;
                    fnui += -1.0;
                    k--;
                    if (iflag >= 3) goto L40;
                    c1r = Math.Abs(str);
                    c1i = Math.Abs(sti);
                    c1m = Math.Max(c1r, c1i);
                    if (c1m <= ascle) goto L40;
                    iflag++;
                    ascle = bry[iflag - 1];
                    s1r *= cscrr;
                    s1i *= cscrr;
                    s2r = str;
                    s2i = sti;
                    csclr *= tol;
                    cscrr = 1.0 / csclr;
                    s1r *= csclr;
                    s1i *= csclr;
                    s2r *= csclr;
                    s2i *= csclr;
            L40:
                    ;
                }
                return 0;
            L50:
                nz = -1;
                if (nw == -2) nz = -2;
                return 0;
            L60:
                if (iform == 2) goto L70;
                // -----------------------------------------------------------------------
                //     ASYMPTOTIC EXPANSION FOR I(FNU,Z) FOR LARGE FNU APPLIED IN
                //     -PI/3.LE.ARG(Z).LE.PI/3
                // -----------------------------------------------------------------------
                zuni1(zr, zi, fnu, kode, n, yr, yi, ref nw, ref nlast, fnul, tol, elim, alim);
                goto L80;
            L70:
                // -----------------------------------------------------------------------
                //     ASYMPTOTIC EXPANSION FOR J(FNU,Z*EXP(M*HPI)) FOR LARGE FNU
                //     APPLIED IN PI/3.LT.ABS(ARG(Z)).LE.PI/2 WHERE M=+I OR -I
                //     AND HPI=PI/2
                // -----------------------------------------------------------------------
                zuni2(zr, zi, fnu, kode, n, yr, yi, ref nw, ref nlast, fnul, tol, elim, alim);
            L80:
                if (nw < 0) goto L50;
                nz = nw;
                return 0;
            L90:
                nlast = n;
                return 0;
            }

            static int zbunk(double zr, double zi, double fnu, int kode, int mr, int n, double[] yr, double[] yi, ref int nz, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZBUNK
                //***REFER TO  ZBESK,ZBESH
                //
                //     ZBUNK COMPUTES THE K BESSEL FUNCTION FOR FNU.GT.FNUL.
                //     ACCORDING TO THE UNIFORM ASYMPTOTIC EXPANSION FOR K(FNU,Z)
                //     IN ZUNK1 AND THE EXPANSION FOR H(2,FNU,Z) IN ZUNK2
                //
                //***ROUTINES CALLED  ZUNK1,ZUNK2
                //***END PROLOGUE  ZBUNK

                #endregion

                double ax, ay;

                nz = 0;
                ax = Math.Abs(zr) * 1.7321;
                ay = Math.Abs(zi);
                if (ay > ax) goto L10;
                //-----------------------------------------------------------------------
                //    ASYMPTOTIC EXPANSION FOR K(FNU,Z) FOR LARGE FNU APPLIED IN
                //    -PI/3.LE.ARG(Z).LE.PI/3
                //-----------------------------------------------------------------------
                zunk1(zr, zi, fnu, kode, mr, n, yr, yi, ref nz, tol, elim, alim);
                goto L20;
            L10:
                //-----------------------------------------------------------------------
                //    ASYMPTOTIC EXPANSION FOR H(2,FNU,Z*EXP(M*HPI)) FOR LARGE FNU
                //    APPLIED IN PI/3.LT.ABS(ARG(Z)).LE.PI/2 WHERE M=+I OR -I
                //    AND HPI=PI/2
                //-----------------------------------------------------------------------
                zunk2(zr, zi, fnu, kode, mr, n, yr, yi, ref nz, tol, elim, alim);
            L20:
                return 0;
            }

            static int zkscl(double zrr, double zri, double fnu, int n, double[] yr, double[] yi, ref int nz, double rzr, double rzi, double ascle, double tol, double elim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZKSCL
                //***REFER TO  ZBESK
                //
                //     SET K FUNCTIONS TO ZERO ON UNDERFLOW, CONTINUE RECURRENCE
                //     ON SCALED FUNCTIONS UNTIL TWO MEMBERS COME ON SCALE, THEN
                //     RETURN WITH MIN(NZ+2,N) VALUES SCALED BY 1/TOL.
                //
                //***ROUTINES CALLED  ZUCHK,ZABS,ZLOG
                //***END PROLOGUE  ZKSCL

                #endregion

                const double zeror = 0.0;
                const double zeroi = 0.0;

                double acs, as_, cki, ckr, csi = 0, csr = 0;
                double fn, str, s1i, s1r, s2i;
                double s2r;
                double zdr, zdi, celmr, elm, helim, alas;
                int i, ic, idum = 0, kk, nn, nw = 0;

                double[] cyi = new double[2];
                double[] cyr = new double[2];

                nz = 0;
                ic = 0;
                nn = Math.Min(2, n);
                for (i = 1; i <= nn; i++)
                {
                    s1r = yr[i - 1];
                    s1i = yi[i - 1];
                    cyr[i - 1] = s1r;
                    cyi[i - 1] = s1i;
                    as_ = zabs(s1r, s1i);
                    acs = -zrr + Math.Log(as_);
                    nz++;
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                    if (acs < -elim) goto L10;
                    zlog(s1r, s1i, ref csr, ref csi, ref idum);
                    csr -= zrr;
                    csi -= zri;
                    str = Math.Exp(csr) / tol;
                    csr = str * Math.Cos(csi);
                    csi = str * Math.Sin(csi);
                    zuchk(csr, csi, ref nw, ascle, tol);
                    if (nw != 0) goto L10;
                    yr[i - 1] = csr;
                    yi[i - 1] = csi;
                    ic = i;
                    nz--;
            L10:
                    ;
                }
                if (n == 1) return 0;
                if (ic > 1) goto L20;
                yr[0] = zeror;
                yi[0] = zeroi;
                nz = 2;
            L20:
                if (n == 2) return 0;
                if (nz == 0) return 0;
                fn = fnu + 1.0;
                ckr = fn * rzr;
                cki = fn * rzi;
                s1r = cyr[0];
                s1i = cyi[0];
                s2r = cyr[1];
                s2i = cyi[1];
                helim = elim * .5;
                elm = Math.Exp(-(elim));
                celmr = elm;
                zdr = zrr;
                zdi = zri;
                //-----------------------------------------------------------------------
                //    FIND TWO CONSECUTIVE Y VALUES ON SCALE. SCALE RECURRENCE IF
                //    S2 GETS LARGER THAN EXP(ELIM/2)
                //-----------------------------------------------------------------------
                for (i = 3; i <= n; i++)
                {
                    kk = i;
                    csr = s2r;
                    csi = s2i;
                    s2r = ckr * csr - cki * csi + s1r;
                    s2i = cki * csr + ckr * csi + s1i;
                    s1r = csr;
                    s1i = csi;
                    ckr += rzr;
                    cki += rzi;
                    as_ = zabs(s2r, s2i);
                    alas = Math.Log(as_);
                    acs = -zdr + alas;
                    nz++;
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                    if (acs < -elim) goto L25;
                    zlog(s2r, s2i, ref csr, ref csi, ref idum);
                    csr -= zdr;
                    csi -= zdi;
                    str = Math.Exp(csr) / tol;
                    csr = str * Math.Cos(csi);
                    csi = str * Math.Sin(csi);
                    zuchk(csr, csi, ref nw, ascle, tol);
                    if (nw != 0) goto L25;
                    yr[i - 1] = csr;
                    yi[i - 1] = csi;
                    nz--;
                    if (ic == kk - 1) goto L40;
                    ic = kk;
                    goto L30;
            L25:
                    if (alas < helim) goto L30;
                    zdr -= elim;
                    s1r *= celmr;
                    s1i *= celmr;
                    s2r *= celmr;
                    s2i *= celmr;
            L30:
                    ;
                }
                nz = n;
                if (ic == n) nz = n - 1;
                goto L45;
            L40:
                nz = kk - 2;
            L45:
                for (i = 1; i <= nz; i++)
                {
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                }
                return 0;
            }

            static int zmlri(double zr, double zi, double fnu, int kode, int n, double[] yr, double[] yi, ref int nz, double tol)
            {
                #region Description

                //***BEGIN PROLOGUE  ZMLRI
                //***REFER TO  ZBESI,ZBESK
                //
                //     ZMLRI COMPUTES THE I BESSEL FUNCTION FOR RE(Z).GE.0.0 BY THE
                //     MILLER ALGORITHM NORMALIZED BY A NEUMANN SERIES.
                //
                //***ROUTINES CALLED  DGAMLN,D1MACH,ZABS,ZEXP,ZLOG,ZMLT
                //***END PROLOGUE  ZMLRI

                #endregion

                const double zeror = 0.0;
                const double zeroi = 0.0;
                const double coner = 1.0;
                const double conei = 0.0;

                double ack, ak, ap, at, az, bk, cki, ckr, cnormi = 0;
                double cnormr = 0, fkap, fkk, flam, fnf, pti, ptr, p1i;
                double p1r, p2i, p2r, raz, rho, rho2, rzi, rzr, scle, sti, str, sumi;
                double sumr, tfnf, tst;
                int i, iaz, idum = 0, ifnu, inu, itime, k, kk, km, m;

                scle = d1mach(1) / tol;
                nz = 0;
                az = zabs(zr, zi);
                iaz = (int)az;
                ifnu = (int)fnu;
                inu = ifnu + n - 1;
                at = (double)iaz + 1.0;
                raz = 1.0 / az;
                str = zr * raz;
                sti = -zi * raz;
                ckr = str * at * raz;
                cki = sti * at * raz;
                rzr = (str + str) * raz;
                rzi = (sti + sti) * raz;
                p1r = zeror;
                p1i = zeroi;
                p2r = coner;
                p2i = conei;
                ack = (at + 1.0) * raz;
                rho = ack + Math.Sqrt(ack * ack - 1.0);
                rho2 = rho * rho;
                tst = (rho2 + rho2) / ((rho2 - 1.0) * (rho - 1.0));
                tst /= tol;
                //-----------------------------------------------------------------------
                //    COMPUTE RELATIVE TRUNCATION ERROR INDEX FOR SERIES
                //-----------------------------------------------------------------------
                ak = at;
                for (i = 1; i <= 80; i++)
                {
                    ptr = p2r;
                    pti = p2i;
                    p2r = p1r - (ckr * ptr - cki * pti);
                    p2i = p1i - (cki * ptr + ckr * pti);
                    p1r = ptr;
                    p1i = pti;
                    ckr += rzr;
                    cki += rzi;
                    ap = zabs(p2r, p2i);
                    if (ap > tst * ak * ak) goto L20;
                    ak += 1.0;
                }
                goto L110;
            L20:
                i++;
                k = 0;
                if (inu < iaz) goto L40;
                //-----------------------------------------------------------------------
                //    COMPUTE RELATIVE TRUNCATION ERROR FOR RATIOS
                //-----------------------------------------------------------------------
                p1r = zeror;
                p1i = zeroi;
                p2r = coner;
                p2i = conei;
                at = inu + 1.0;
                str = zr * raz;
                sti = -zi * raz;
                ckr = str * at * raz;
                cki = sti * at * raz;
                ack = at * raz;
                tst = Math.Sqrt(ack / tol);
                itime = 1;
                for (k = 1; k <= 80; k++)
                {
                    ptr = p2r;
                    pti = p2i;
                    p2r = p1r - (ckr * ptr - cki * pti);
                    p2i = p1i - (ckr * pti + cki * ptr);
                    p1r = ptr;
                    p1i = pti;
                    ckr += rzr;
                    cki += rzi;
                    ap = zabs(p2r, p2i);
                    if (ap < tst) goto L30;
                    if (itime == 2) goto L40;
                    ack = zabs(ckr, cki);
                    flam = ack + Math.Sqrt(ack * ack - 1.0);
                    fkap = ap / zabs(p1r, p1i);
                    rho = Math.Min(flam, fkap);
                    tst *= Math.Sqrt(rho / (rho * rho - 1.0));
                    itime = 2;
            L30:
                    ;
                }
                goto L110;
            L40:
                //-----------------------------------------------------------------------
                //    BACKWARD RECURRENCE AND SUM NORMALIZING RELATION
                //-----------------------------------------------------------------------
                k++;
                kk = Math.Max(i + iaz, k + inu);
                fkk = (double)kk;
                p1r = zeror;
                p1i = zeroi;
                //-----------------------------------------------------------------------
                //    SCALE P2 AND SUM BY SCLE
                //-----------------------------------------------------------------------
                p2r = scle;
                p2i = zeroi;
                fnf = fnu - (double)ifnu;
                tfnf = fnf + fnf;
                bk = dgamln(fkk + tfnf + 1.0, ref idum) - dgamln(fkk + 1.0, ref idum) - dgamln(tfnf + 1.0, ref idum);
                bk = Math.Exp(bk);
                sumr = zeror;
                sumi = zeroi;
                km = kk - inu;
                for (i = 1; i <= km; i++)
                {
                    ptr = p2r;
                    pti = p2i;
                    p2r = p1r + (fkk + fnf) * (rzr * ptr - rzi * pti);
                    p2i = p1i + (fkk + fnf) * (rzi * ptr + rzr * pti);
                    p1r = ptr;
                    p1i = pti;
                    ak = 1.0 - tfnf / (fkk + tfnf);
                    ack = bk * ak;
                    sumr += (ack + bk) * p1r;
                    sumi += (ack + bk) * p1i;
                    bk = ack;
                    fkk += -1.0;
                }
                yr[n - 1] = p2r;
                yi[n - 1] = p2i;
                if (n == 1) goto L70;
                for (i = 2; i <= n; i++)
                {
                    ptr = p2r;
                    pti = p2i;
                    p2r = p1r + (fkk + fnf) * (rzr * ptr - rzi * pti);
                    p2i = p1i + (fkk + fnf) * (rzi * ptr + rzr * pti);
                    p1r = ptr;
                    p1i = pti;
                    ak = 1.0 - tfnf / (fkk + tfnf);
                    ack = bk * ak;
                    sumr += (ack + bk) * p1r;
                    sumi += (ack + bk) * p1i;
                    bk = ack;
                    fkk += -1.0;
                    m = n - i + 1;
                    yr[m - 1] = p2r;
                    yi[m - 1] = p2i;
                }
            L70:
                if (ifnu <= 0) goto L90;
                for (i = 1; i <= ifnu; i++)
                {
                    ptr = p2r;
                    pti = p2i;
                    p2r = p1r + (fkk + fnf) * (rzr * ptr - rzi * pti);
                    p2i = p1i + (fkk + fnf) * (rzr * pti + rzi * ptr);
                    p1r = ptr;
                    p1i = pti;
                    ak = 1.0 - tfnf / (fkk + tfnf);
                    ack = bk * ak;
                    sumr += (ack + bk) * p1r;
                    sumi += (ack + bk) * p1i;
                    bk = ack;
                    fkk += -1.0;
                }
            L90:
                ptr = zr;
                pti = zi;
                if (kode == 2) ptr = zeror;
                zlog(rzr, rzi, ref str, ref sti, ref idum);
                p1r = -fnf * str + ptr;
                p1i = -fnf * sti + pti;
                ap = dgamln(fnf + 1.0, ref idum);
                ptr = p1r - ap;
                pti = p1i;
                //-----------------------------------------------------------------------
                //    THE DIVISION CEXP(PT)/(SUM+P2) IS ALTERED TO AVOID OVERFLOW
                //    IN THE DENOMINATOR BY SQUARING LARGE QUANTITIES
                //-----------------------------------------------------------------------
                p2r += sumr;
                p2i += sumi;
                ap = zabs(p2r, p2i);
                p1r = 1.0 / ap;
                zexp(ptr, pti, ref str, ref sti);
                ckr = str * p1r;
                cki = sti * p1r;
                ptr = p2r * p1r;
                pti = -p2i * p1r;
                zmlt(ckr, cki, ptr, pti, ref cnormr, ref cnormi);
                for (i = 1; i <= n; i++)
                {
                    str = yr[i - 1] * cnormr - yi[i - 1] * cnormi;
                    yi[i - 1] = yr[i - 1] * cnormi + yi[i - 1] * cnormr;
                    yr[i - 1] = str;
                }
                return 0;
            L110:
                nz = -2;
                return 0;
            }

            static int zrati(double zr, double zi, double fnu, int n, double[] cyr, double[] cyi, double tol)
            {
                #region Description

                //***BEGIN PROLOGUE  ZRATI
                //***REFER TO  ZBESI,ZBESK,ZBESH
                //
                //     ZRATI COMPUTES RATIOS OF I BESSEL FUNCTIONS BY BACKWARD
                //     RECURRENCE.  THE STARTING INDEX IS DETERMINED BY FORWARD
                //     RECURRENCE AS DESCRIBED IN J. RES. OF NAT. BUR. OF STANDARDS-B,
                //     MATHEMATICAL SCIENCES, VOL 77B, P111-114, SEPTEMBER, 1973,
                //     BESSEL FUNCTIONS I AND J OF COMPLEX ARGUMENT AND INTEGER ORDER,
                //     BY D. J. SOOKNE.
                //
                //***ROUTINES CALLED  ZABS,ZDIV
                //***END PROLOGUE  ZRATI

                #endregion

                const double czeror = 0.0;
                const double czeroi = 0.0;
                const double coner = 1.0;
                const double conei = 0.0;
                const double rt2 = 1.41421356237309505; // sqrt(2)

                double ak, amagz, ap1, ap2, arg, az, cdfnui, cdfnur;
                double dfnu, fdnu, flam;
                double fnup, pti, ptr, p1i, p1r, p2i, p2r, rak, rap1, rho, rzi;
                double rzr, test, test1, tti, ttr, t1i, t1r;
                int i, id, idnu, inu, itime, k, kk, magz;

                az = zabs(zr, zi);
                inu = (int)fnu;
                idnu = inu + n - 1;
                magz = (int)az;
                amagz = (double)(magz + 1);
                fdnu = (double)idnu;
                fnup = Math.Max(amagz, fdnu);
                id = idnu - magz - 1;
                itime = 1;
                k = 1;
                ptr = 1.0 / az;
                rzr = ptr * (zr + zr) * ptr;
                rzi = -ptr * (zi + zi) * ptr;
                t1r = rzr * fnup;
                t1i = rzi * fnup;
                p2r = -t1r;
                p2i = -t1i;
                p1r = coner;
                p1i = conei;
                t1r += rzr;
                t1i += rzi;
                if (id > 0) id = 0;
                ap2 = zabs(p2r, p2i);
                ap1 = zabs(p1r, p1i);
                //-----------------------------------------------------------------------
                //    THE OVERFLOW TEST ON K(FNU+I-1,Z) BEFORE THE CALL TO CBKNU
                //    GUARANTEES THAT P2 IS ON SCALE. SCALE TEST1 AND ALL SUBSEQUENT
                //    P2 VALUES BY AP1 TO ENSURE THAT AN OVERFLOW DOES NOT OCCUR
                //    PREMATURELY.
                //-----------------------------------------------------------------------
                arg = (ap2 + ap2) / (ap1 * tol);
                test1 = Math.Sqrt(arg);
                test = test1;
                rap1 = 1.0 / ap1;
                p1r *= rap1;
                p1i *= rap1;
                p2r *= rap1;
                p2i *= rap1;
                ap2 *= rap1;
            L10:
                k++;
                ap1 = ap2;
                ptr = p2r;
                pti = p2i;
                p2r = p1r - (t1r * ptr - t1i * pti);
                p2i = p1i - (t1r * pti + t1i * ptr);
                p1r = ptr;
                p1i = pti;
                t1r += rzr;
                t1i += rzi;
                ap2 = zabs(p2r, p2i);
                if (ap1 <= test) goto L10;
                if (itime == 2) goto L20;
                ak = zabs(t1r, t1i) * 0.5;
                flam = ak + Math.Sqrt(ak * ak - 1.0);
                rho = Math.Min(ap2 / ap1, flam);
                test = test1 * Math.Sqrt(rho / (rho * rho - 1.0));
                itime = 2;
                goto L10;
            L20:
                kk = k + 1 - id;
                ak = (double)kk;
                t1r = ak;
                t1i = czeroi;
                dfnu = fnu + (double)(n - 1);
                p1r = 1.0 / ap2;
                p1i = czeroi;
                p2r = czeror;
                p2i = czeroi;
                for (i = 1; i <= kk; i++)
                {
                    ptr = p1r;
                    pti = p1i;
                    rap1 = dfnu + t1r;
                    ttr = rzr * rap1;
                    tti = rzi * rap1;
                    p1r = ptr * ttr - pti * tti + p2r;
                    p1i = ptr * tti + pti * ttr + p2i;
                    p2r = ptr;
                    p2i = pti;
                    t1r -= coner;
                }
                if (p1r != czeror || p1i != czeroi) goto L40;
                p1r = tol;
                p1i = tol;
            L40:
                zdiv(p2r, p2i, p1r, p1i, ref cyr[n - 1], ref cyi[n - 1]);
                if (n == 1) return 0;
                k = n - 1;
                ak = (double)k;
                t1r = ak;
                t1i = czeroi;
                cdfnur = fnu * rzr;
                cdfnui = fnu * rzi;
                for (i = 2; i <= n; i++)
                {
                    ptr = cdfnur + (t1r * rzr - t1i * rzi) + cyr[k];
                    pti = cdfnui + (t1r * rzi + t1i * rzr) + cyi[k];
                    ak = zabs(ptr, pti);
                    if (ak != czeror) goto L50;
                    ptr = tol;
                    pti = tol;
                    ak = tol * rt2;
            L50:
                    rak = coner / ak;
                    cyr[k - 1] = rak * ptr * rak;
                    cyi[k - 1] = -rak * pti * rak;
                    t1r -= coner;
                    k--;
                }
                return 0;
            }

            static int zs1s2(double zrr, double zri, ref double s1r, ref double s1i, ref double s2r, ref double s2i, ref int nz, double ascle, double alim, ref int iuf)
            {
                #region Description

                //*** BEGIN PROLOGUE ZS1S2
                //*** REFER TO ZBESK, ZAIRY
                //
                //     ZS1S2 TESTS FOR A POSSIBLE UNDERFLOW RESULTING FROM THE
                //     ADDITION OF THE I AND K FUNCTIONS IN THE ANALYTIC CON -
                //     TINUATION FORMULA WHERE S1 = K FUNCTION AND S2 = I FUNCTION.
                // ON KODE = 1 THE I AND K FUNCTIONS ARE DIFFERENT ORDERS OF
                //     MAGNITUDE, BUT FOR KODE = 2 THEY CAN BE OF THE SAME ORDER
                //     OF MAGNITUDE AND THE MAXIMUM MUST BE AT LEAST ONE
                // PRECISION ABOVE THE UNDERFLOW LIMIT.
                //
                //*** ROUTINES CALLED ZABS, ZEXP, ZLOG
                //*** END PROLOGUE ZS1S2

                #endregion

                const double zeror = 0.0;
                const double zeroi = 0.0;

                double aa, aln, as1, as2, c1i = 0, c1r = 0, s1di;
                double s1dr;
                int idum = 0;

                nz = 0;
                as1 = zabs(s1r, s1i);
                as2 = zabs(s2r, s2i);
                if (s1r == 0.0 && s1i == 0.0) goto L10;
                if (as1 == 0.0) goto L10;
                aln = -zrr - zrr + Math.Log(as1);
                s1dr = s1r;
                s1di = s1i;
                s1r = zeror;
                s1i = zeroi;
                as1 = zeror;
                if (aln < -alim) goto L10;
                zlog(s1dr, s1di, ref c1r, ref c1i, ref idum);
                c1r = c1r - zrr - zrr;
                c1i = c1i - zri - zri;
                zexp(c1r, c1i, ref s1r, ref s1i);
                as1 = zabs(s1r, s1i);
                iuf++;
            L10:
                aa = Math.Max(as1, as2);
                if (aa > ascle) return 0;
                s1r = zeror;
                s1i = zeroi;
                s2r = zeror;
                s2i = zeroi;
                nz = 1;
                iuf = 0;
                return 0;
            }

            static int zseri(double zr, double zi, double fnu, int kode, int n, double[] yr, double[] yi, ref int nz, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZSERI
                //***REFER TO  ZBESI,ZBESK
                //
                //     ZSERI COMPUTES THE I BESSEL FUNCTION FOR REAL(Z).GE.0.0 BY
                //     MEANS OF THE POWER SERIES FOR LARGE CABS(Z) IN THE
                //     REGION CABS(Z).LE.2*SQRT(FNU+1). NZ=0 IS A NORMAL RETURN.
                //     NZ.GT.0 MEANS THAT THE LAST NZ COMPONENTS WERE SET TO ZERO
                //     DUE TO UNDERFLOW. NZ.LT.0 MEANS UNDERFLOW OCCURRED, BUT THE
                //     CONDITION CABS(Z).LE.2*SQRT(FNU+1) WAS VIOLATED AND THE
                //     COMPUTATION MUST BE COMPLETED IN ANOTHER ROUTINE WITH N=N-ABS(NZ).
                //
                //***ROUTINES CALLED  DGAMLN,D1MACH,ZUCHK,ZABS,ZDIV,ZLOG,ZMLT
                //***END PROLOGUE  ZSERI

                #endregion

                const double zeror = 0.0;
                const double zeroi = 0.0;
                const double coner = 1.0;
                const double conei = 0.0;

                double aa, acz, ak, ak1i, ak1r, arm, ascle = 0, atol;
                double az, cki = 0, ckr = 0, coefi, coefr, crscr, czi, czr, dfnu;
                double fnup, hzi, hzr, raz, rs, rtr1, rzi, rzr, s, ss = 0, sti = 0;
                double str = 0, s1i = 0, s1r, s2i, s2r;
                int i, ib, idum = 0, iflag, il, k, l, m, nn, nw = 0;

                double[] wi = new double[2];
                double[] wr = new double[2];

                nz = 0;
                az = zabs(zr, zi);
                if (az == 0.0) goto L160;
                arm = d1mach(1) * 1.0E3;
                rtr1 = Math.Sqrt(arm);
                crscr = 1.0;
                iflag = 0;
                if (az < arm) goto L150;
                hzr = zr * 0.5;
                hzi = zi * 0.5;
                czr = zeror;
                czi = zeroi;
                if (az <= rtr1) goto L10;
                zmlt(hzr, hzi, hzr, hzi, ref czr, ref czi);
            L10:
                acz = zabs(czr, czi);
                nn = n;
                zlog(hzr, hzi, ref ckr, ref cki, ref idum);
            L20:
                dfnu = fnu + (double)(nn - 1);
                fnup = dfnu + 1.0;
                // -----------------------------------------------------------------------
                //     UNDERFLOW TEST
                // -----------------------------------------------------------------------
                ak1r = ckr * dfnu;
                ak1i = cki * dfnu;
                ak = dgamln(fnup, ref idum);
                ak1r -= ak;
                if (kode == 2) ak1r -= zr;
                if (ak1r > -elim) goto L40;
            L30:
                nz++;
                yr[nn - 1] = zeror;
                yi[nn - 1] = zeroi;
                if (acz > dfnu) goto L190;
                nn--;
                if (nn == 0) return 0;
                goto L20;
            L40:
                if (ak1r > -alim) goto L50;
                iflag = 1;
                ss = 1.0 / tol;
                crscr = tol;
                ascle = arm * ss;
            L50:
                aa = Math.Exp(ak1r);
                if (iflag == 1) aa *= ss;
                coefr = aa * Math.Cos(ak1i);
                coefi = aa * Math.Sin(ak1i);
                atol = tol * acz / fnup;
                il = Math.Min(2, nn);
                for (i = 1; i <= il; i++)
                {
                    dfnu = fnu + (nn - i);
                    fnup = dfnu + 1.0;
                    s1r = coner;
                    s1i = conei;
                    if (acz < tol * fnup) goto L70;
                    ak1r = coner;
                    ak1i = conei;
                    ak = fnup + 2.0;
                    s = fnup;
                    aa = 2.0;
            L60:
                    rs = 1.0 / s;
                    str = ak1r * czr - ak1i * czi;
                    sti = ak1r * czi + ak1i * czr;
                    ak1r = str * rs;
                    ak1i = sti * rs;
                    s1r += ak1r;
                    s1i += ak1i;
                    s += ak;
                    ak += 2.0;
                    aa = aa * acz * rs;
                    if (aa > atol) goto L60;
            L70:
                    s2r = s1r * coefr - s1i * coefi;
                    s2i = s1r * coefi + s1i * coefr;
                    wr[i - 1] = s2r;
                    wi[i - 1] = s2i;
                    if (iflag == 0) goto L80;
                    zuchk(s2r, s2i, ref nw, ascle, tol);
                    if (nw != 0) goto L30;
            L80:
                    m = nn - i + 1;
                    yr[m - 1] = s2r * crscr;
                    yi[m - 1] = s2i * crscr;
                    if (i == il) goto L90;
                    zdiv(coefr, coefi, hzr, hzi, ref str, ref sti);
                    coefr = str * dfnu;
                    coefi = sti * dfnu;
            L90:
                    ;
                }
                if (nn <= 2) return 0;
                k = nn - 2;
                ak = (double)k;
                raz = 1.0 / az;
                str = zr * raz;
                sti = -zi * raz;
                rzr = (str + str) * raz;
                rzi = (sti + sti) * raz;
                if (iflag == 1) goto L120;
                ib = 3;
            L100:
                for (i = ib; i <= nn; i++)
                {
                    yr[k - 1] = (ak + fnu) * (rzr * yr[k] - rzi * yi[k]) + yr[k + 1];
                    yi[k - 1] = (ak + fnu) * (rzr * yi[k] + rzi * yr[k]) + yi[k + 1];
                    ak += -1.0;
                    k--;
                }
                return 0;
                // -----------------------------------------------------------------------
                //     RECUR BACKWARD WITH SCALED VALUES
                // -----------------------------------------------------------------------
            L120:
                // -----------------------------------------------------------------------
                //     EXP(-ALIM)=EXP(-ELIM)/TOL=APPROX. ONE PRECISION ABOVE THE
                //     UNDERFLOW LIMIT = ASCLE = D1MACH(1)*SS*1.0D+3
                // -----------------------------------------------------------------------
                s1r = wr[0];
                s1i = wi[0];
                s2r = wr[1];
                s2i = wi[1];
                for (l = 3; l <= nn; l++)
                {
                    ckr = s2r;
                    cki = s2i;
                    s2r = s1r + (ak + fnu) * (rzr * ckr - rzi * cki);
                    s2i = s1i + (ak + fnu) * (rzr * cki + rzi * ckr);
                    s1r = ckr;
                    s1i = cki;
                    ckr = s2r * crscr;
                    cki = s2i * crscr;
                    yr[k - 1] = ckr;
                    yi[k - 1] = cki;
                    ak += -1.0;
                    k--;
                    if (zabs(ckr, cki) > ascle) goto L140;
                }
                return 0;
            L140:
                ib = l + 1;
                if (ib > nn) return 0;
                goto L100;
            L150:
                nz = n;
                if (fnu == 0.0) nz--;
            L160:
                yr[0] = zeror;
                yi[0] = zeroi;
                if (fnu != 0.0) goto L170;
                yr[0] = coner;
                yi[0] = conei;
            L170:
                if (n == 1) return 0;
                for (i = 2; i <= n; i++)
                {
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                }
                return 0;
                // -----------------------------------------------------------------------
                //     RETURN WITH NZ.LT.0 IF ABS(Z*Z/4).GT.FNU+N-NZ-1 COMPLETE
                //     THE CALCULATION IN CBINU WITH N=N-ABS(NZ)
                // -----------------------------------------------------------------------
            L190:
                nz = -nz;
                return 0;
            }

            static int zshch(double zr, double zi, ref double cshr, ref double cshi, ref double cchr, ref double cchi)
            {
                #region Description

                //*** BEGIN PROLOGUE ZSHCH
                //*** REFER TO ZBESK, ZBESH
                //
                //     ZSHCH COMPUTES THE COMPLEX HYPERBOLIC FUNCTIONS CSH = SINH(X + I * Y)
                // AND CCH = COSH(X + I * Y), WHERE I**2 = -1.
                //
                //*** ROUTINES CALLED(NONE)
                //*** END PROLOGUE ZSHCH

                #endregion

                double sh = Math.Sinh(zr);
                double ch = Math.Cosh(zr);
                double sn = Math.Sin(zi);
                double cn = Math.Cos(zi);
                cshr = sh * cn;
                cshi = ch * sn;
                cchr = ch * cn;
                cchi = sh * sn;
                return 0;
            }

            static int zuchk(double yr, double yi, ref int nz, double ascle, double tol)
            {
                #region Description

                //***BEGIN PROLOGUE  ZUCHK
                //***REFER TO ZSERI,ZUOIK,ZUNK1,ZUNK2,ZUNI1,ZUNI2,ZKSCL
                //
                //      Y ENTERS AS A SCALED QUANTITY WHOSE MAGNITUDE IS GREATER THAN
                //      EXP(-ALIM)=ASCLE=1.0E+3*D1MACH(1)/TOL. THE TEST IS MADE TO SEE
                //      IF THE MAGNITUDE OF THE REAL OR IMAGINARY PART WOULD UNDERFLOW
                //      WHEN Y IS SCALED (BY TOL) TO ITS PROPER VALUE. Y IS ACCEPTED
                //      IF THE UNDERFLOW IS AT LEAST ONE PRECISION BELOW THE MAGNITUDE
                //      OF THE LARGEST COMPONENT; OTHERWISE THE PHASE ANGLE DOES NOT HAVE
                //      ABSOLUTE ACCURACY AND AN UNDERFLOW IS ASSUMED.
                //
                //***ROUTINES CALLED  (NONE)
                //***END PROLOGUE  ZUCHK

                #endregion

                double ss, st, wr, wi;

                nz = 0;
                wr = Math.Abs(yr);
                wi = Math.Abs(yi);
                st = Math.Min(wr, wi);
                if (st > ascle) return 0;
                ss = Math.Max(wr, wi);
                st /= tol;
                if (ss < st) nz = 1;
                return 0;
            }

            static int zunhj(double zr, double zi, double fnu, int ipmtr, double tol, ref double phir, ref double phii, ref double argr, ref double argi, ref double zeta1r, ref double zeta1i, ref double zeta2r, ref double zeta2i, ref double asumr, ref double asumi, ref double bsumr, ref double bsumi)
            {
                #region Description

                //***BEGIN PROLOGUE  ZUNHJ
                //***REFER TO  ZBESI,ZBESK
                //
                //     REFERENCES
                //         HANDBOOK OF MATHEMATICAL FUNCTIONS BY M. ABRAMOWITZ AND I.A.
                //         STEGUN, AMS55, NATIONAL BUREAU OF STANDARDS, 1965, CHAPTER 9.
                //
                //         ASYMPTOTICS AND SPECIAL FUNCTIONS BY F.W.J. OLVER, ACADEMIC
                //         PRESS, N.Y., 1974, PAGE 420
                //
                //     ABSTRACT
                //         ZUNHJ COMPUTES PARAMETERS FOR BESSEL FUNCTIONS C(FNU,Z) =
                //         J(FNU,Z), Y(FNU,Z) OR H(I,FNU,Z) I=1,2 FOR LARGE ORDERS FNU
                //         BY MEANS OF THE UNIFORM ASYMPTOTIC EXPANSION
                //
                //         C(FNU,Z)=C1*PHI*( ASUM*AIRY(ARG) + C2*BSUM*DAIRY(ARG) )
                //
                //         FOR PROPER CHOICES OF C1, C2, AIRY AND DAIRY WHERE AIRY IS
                //         AN AIRY FUNCTION AND DAIRY IS ITS DERIVATIVE.
                //
                //               (2/3)*FNU*ZETA**1.5 = ZETA1-ZETA2,
                //
                //         ZETA1=0.5*FNU*CLOG((1+W)/(1-W)), ZETA2=FNU*W FOR SCALING
                //         PURPOSES IN AIRY FUNCTIONS FROM CAIRY OR CBIRY.
                //
                //         MCONJ=SIGN OF AIMAG(Z), BUT IS AMBIGUOUS WHEN Z IS REAL AND
                //         MUST BE SPECIFIED. IPMTR=0 RETURNS ALL PARAMETERS. IPMTR=
                //         1 COMPUTES ALL EXCEPT ASUM AND BSUM.
                //
                //***ROUTINES CALLED  ZABS,ZDIV,ZLOG,ZSQRT,D1MACH
                //***END PROLOGUE  ZUNHJ


                #endregion

                const double coner = 1.0;
                const double conei = 0.0;
                const double ex1 = .333333333333333333;
                const double ex2 = .666666666666666667;
                const double gpi = 3.14159265358979323846264338327950;
                const double hpi = 1.570796326794896619231321696;
                const double thpi = 4.71238898038468986;
                const double zeror = 0.0;
                const double zeroi = 0.0;

                double[] ar = {
                    1.0,.104166666666666667,.0835503472222222222,
                    .12822657455632716,.291849026464140464,.881627267443757652,
                    3.32140828186276754,14.9957629868625547,78.9230130115865181,
                    474.451538868264323,3207.49009089066193,24086.5496408740049,
                    198923.119169509794,1791902.00777534383 };
                double[] br = { 1.0,-.145833333333333333,
                    -.0987413194444444444,-.143312053915895062,-.317227202678413548,
                    -.942429147957120249,-3.51120304082635426,-15.7272636203680451,
                    -82.2814390971859444,-492.355370523670524,-3316.21856854797251,
                    -24827.6742452085896,-204526.587315129788,-1838444.9170682099 };
                double[] c = { 1.0,-.208333333333333333,.125,
                    .334201388888888889,-.401041666666666667,.0703125,
                    -1.02581259645061728,1.84646267361111111,-.8912109375,.0732421875,
                    4.66958442342624743,-11.2070026162229938,8.78912353515625,
                    -2.3640869140625,.112152099609375,-28.2120725582002449,
                    84.6362176746007346,-91.8182415432400174,42.5349987453884549,
                    -7.3687943594796317,.227108001708984375,212.570130039217123,
                    -765.252468141181642,1059.99045252799988,-699.579627376132541,
                    218.19051174421159,-26.4914304869515555,.572501420974731445,
                    -1919.457662318407,8061.72218173730938,-13586.5500064341374,
                    11655.3933368645332,-5305.64697861340311,1200.90291321635246,
                    -108.090919788394656,1.7277275025844574,20204.2913309661486,
                    -96980.5983886375135,192547.001232531532,-203400.177280415534,
                    122200.46498301746,-41192.6549688975513,7109.51430248936372,
                    -493.915304773088012,6.07404200127348304,-242919.187900551333,
                    1311763.6146629772,-2998015.91853810675,3763271.297656404,
                    -2813563.22658653411,1268365.27332162478,-331645.172484563578,
                    45218.7689813627263,-2499.83048181120962,24.3805296995560639,
                    3284469.85307203782,-19706819.1184322269,50952602.4926646422,
                    -74105148.2115326577,66344512.2747290267,-37567176.6607633513,
                    13288767.1664218183,-2785618.12808645469,308186.404612662398,
                    -13886.0897537170405,110.017140269246738,-49329253.664509962,
                    325573074.185765749,-939462359.681578403,1553596899.57058006,
                    -1621080552.10833708,1106842816.82301447,-495889784.275030309,
                    142062907.797533095,-24474062.7257387285,2243768.17792244943,
                    -84005.4336030240853,551.335896122020586,814789096.118312115,
                    -5866481492.05184723,18688207509.2958249,-34632043388.1587779,
                    41280185579.753974,-33026599749.8007231,17954213731.1556001,
                    -6563293792.61928433,1559279864.87925751,-225105661.889415278,
                    17395107.5539781645,-549842.327572288687,3038.09051092238427,
                    -14679261247.6956167,114498237732.02581,-399096175224.466498,
                    819218669548.577329,-1098375156081.22331,1008158106865.38209,
                    -645364869245.376503,287900649906.150589,-87867072178.0232657,
                    17634730606.8349694,-2167164983.22379509,143157876.718888981,
                    -3871833.44257261262,18257.7554742931747 };
                double[] alfa = { -.00444444444444444444,
                    -9.22077922077922078e-4,-8.84892884892884893e-5,
                    1.65927687832449737e-4,2.4669137274179291e-4,
                    2.6599558934625478e-4,2.61824297061500945e-4,
                    2.48730437344655609e-4,2.32721040083232098e-4,
                    2.16362485712365082e-4,2.00738858762752355e-4,
                    1.86267636637545172e-4,1.73060775917876493e-4,
                    1.61091705929015752e-4,1.50274774160908134e-4,
                    1.40503497391269794e-4,1.31668816545922806e-4,
                    1.23667445598253261e-4,1.16405271474737902e-4,
                    1.09798298372713369e-4,1.03772410422992823e-4,
                    9.82626078369363448e-5,9.32120517249503256e-5,
                    8.85710852478711718e-5,8.42963105715700223e-5,
                    8.03497548407791151e-5,7.66981345359207388e-5,
                    7.33122157481777809e-5,7.01662625163141333e-5,
                    6.72375633790160292e-5,6.93735541354588974e-4,
                    2.32241745182921654e-4,-1.41986273556691197e-5,
                    -1.1644493167204864e-4,-1.50803558053048762e-4,
                    -1.55121924918096223e-4,-1.46809756646465549e-4,
                    -1.33815503867491367e-4,-1.19744975684254051e-4,
                    -1.0618431920797402e-4,-9.37699549891194492e-5,
                    -8.26923045588193274e-5,-7.29374348155221211e-5,
                    -6.44042357721016283e-5,-5.69611566009369048e-5,
                    -5.04731044303561628e-5,-4.48134868008882786e-5,
                    -3.98688727717598864e-5,-3.55400532972042498e-5,
                    -3.1741425660902248e-5,-2.83996793904174811e-5,
                    -2.54522720634870566e-5,-2.28459297164724555e-5,
                    -2.05352753106480604e-5,-1.84816217627666085e-5,
                    -1.66519330021393806e-5,-1.50179412980119482e-5,
                    -1.35554031379040526e-5,-1.22434746473858131e-5,
                    -1.10641884811308169e-5,-3.54211971457743841e-4,
                    -1.56161263945159416e-4,3.0446550359493641e-5,
                    1.30198655773242693e-4,1.67471106699712269e-4,
                    1.70222587683592569e-4,1.56501427608594704e-4,
                    1.3633917097744512e-4,1.14886692029825128e-4,
                    9.45869093034688111e-5,7.64498419250898258e-5,
                    6.07570334965197354e-5,4.74394299290508799e-5,
                    3.62757512005344297e-5,2.69939714979224901e-5,
                    1.93210938247939253e-5,1.30056674793963203e-5,
                    7.82620866744496661e-6,3.59257485819351583e-6,
                    1.44040049814251817e-7,-2.65396769697939116e-6,
                    -4.9134686709848591e-6,-6.72739296091248287e-6,
                    -8.17269379678657923e-6,-9.31304715093561232e-6,
                    -1.02011418798016441e-5,-1.0880596251059288e-5,
                    -1.13875481509603555e-5,-1.17519675674556414e-5,
                    -1.19987364870944141e-5,3.78194199201772914e-4,
                    2.02471952761816167e-4,-6.37938506318862408e-5,
                    -2.38598230603005903e-4,-3.10916256027361568e-4,
                    -3.13680115247576316e-4,-2.78950273791323387e-4,
                    -2.28564082619141374e-4,-1.75245280340846749e-4,
                    -1.25544063060690348e-4,-8.22982872820208365e-5,
                    -4.62860730588116458e-5,-1.72334302366962267e-5,
                    5.60690482304602267e-6,2.313954431482868e-5,
                    3.62642745856793957e-5,4.58006124490188752e-5,
                    5.2459529495911405e-5,5.68396208545815266e-5,
                    5.94349820393104052e-5,6.06478527578421742e-5,
                    6.08023907788436497e-5,6.01577894539460388e-5,
                    5.891996573446985e-5,5.72515823777593053e-5,
                    5.52804375585852577e-5,5.3106377380288017e-5,
                    5.08069302012325706e-5,4.84418647620094842e-5,
                    4.6056858160747537e-5,-6.91141397288294174e-4,
                    -4.29976633058871912e-4,1.83067735980039018e-4,
                    6.60088147542014144e-4,8.75964969951185931e-4,
                    8.77335235958235514e-4,7.49369585378990637e-4,
                    5.63832329756980918e-4,3.68059319971443156e-4,
                    1.88464535514455599e-4,3.70663057664904149e-5,
                    -8.28520220232137023e-5,-1.72751952869172998e-4,
                    -2.36314873605872983e-4,-2.77966150694906658e-4,
                    -3.02079514155456919e-4,-3.12594712643820127e-4,
                    -3.12872558758067163e-4,-3.05678038466324377e-4,
                    -2.93226470614557331e-4,-2.77255655582934777e-4,
                    -2.59103928467031709e-4,-2.39784014396480342e-4,
                    -2.20048260045422848e-4,-2.00443911094971498e-4,
                    -1.81358692210970687e-4,-1.63057674478657464e-4,
                    -1.45712672175205844e-4,-1.29425421983924587e-4,
                    -1.14245691942445952e-4,.00192821964248775885,
                    .00135592576302022234,-7.17858090421302995e-4,
                    -.00258084802575270346,-.00349271130826168475,
                    -.00346986299340960628,-.00282285233351310182,
                    -.00188103076404891354,-8.895317183839476e-4,
                    3.87912102631035228e-6,7.28688540119691412e-4,
                    .00126566373053457758,.00162518158372674427,.00183203153216373172,
                    .00191588388990527909,.00190588846755546138,.00182798982421825727,
                    .0017038950642112153,.00155097127171097686,.00138261421852276159,
                    .00120881424230064774,.00103676532638344962,
                    8.71437918068619115e-4,7.16080155297701002e-4,
                    5.72637002558129372e-4,4.42089819465802277e-4,
                    3.24724948503090564e-4,2.20342042730246599e-4,
                    1.28412898401353882e-4,4.82005924552095464e-5 };
                double[] beta = { .0179988721413553309,
                    .00559964911064388073,.00288501402231132779,.00180096606761053941,
                    .00124753110589199202,9.22878876572938311e-4,
                    7.14430421727287357e-4,5.71787281789704872e-4,
                    4.69431007606481533e-4,3.93232835462916638e-4,
                    3.34818889318297664e-4,2.88952148495751517e-4,
                    2.52211615549573284e-4,2.22280580798883327e-4,
                    1.97541838033062524e-4,1.76836855019718004e-4,
                    1.59316899661821081e-4,1.44347930197333986e-4,
                    1.31448068119965379e-4,1.20245444949302884e-4,
                    1.10449144504599392e-4,1.01828770740567258e-4,
                    9.41998224204237509e-5,8.74130545753834437e-5,
                    8.13466262162801467e-5,7.59002269646219339e-5,
                    7.09906300634153481e-5,6.65482874842468183e-5,
                    6.25146958969275078e-5,5.88403394426251749e-5,
                    -.00149282953213429172,-8.78204709546389328e-4,
                    -5.02916549572034614e-4,-2.94822138512746025e-4,
                    -1.75463996970782828e-4,-1.04008550460816434e-4,
                    -5.96141953046457895e-5,-3.1203892907609834e-5,
                    -1.26089735980230047e-5,-2.42892608575730389e-7,
                    8.05996165414273571e-6,1.36507009262147391e-5,
                    1.73964125472926261e-5,1.9867297884213378e-5,
                    2.14463263790822639e-5,2.23954659232456514e-5,
                    2.28967783814712629e-5,2.30785389811177817e-5,
                    2.30321976080909144e-5,2.28236073720348722e-5,
                    2.25005881105292418e-5,2.20981015361991429e-5,
                    2.16418427448103905e-5,2.11507649256220843e-5,
                    2.06388749782170737e-5,2.01165241997081666e-5,
                    1.95913450141179244e-5,1.9068936791043674e-5,
                    1.85533719641636667e-5,1.80475722259674218e-5,
                    5.5221307672129279e-4,4.47932581552384646e-4,
                    2.79520653992020589e-4,1.52468156198446602e-4,
                    6.93271105657043598e-5,1.76258683069991397e-5,
                    -1.35744996343269136e-5,-3.17972413350427135e-5,
                    -4.18861861696693365e-5,-4.69004889379141029e-5,
                    -4.87665447413787352e-5,-4.87010031186735069e-5,
                    -4.74755620890086638e-5,-4.55813058138628452e-5,
                    -4.33309644511266036e-5,-4.09230193157750364e-5,
                    -3.84822638603221274e-5,-3.60857167535410501e-5,
                    -3.37793306123367417e-5,-3.15888560772109621e-5,
                    -2.95269561750807315e-5,-2.75978914828335759e-5,
                    -2.58006174666883713e-5,-2.413083567612802e-5,
                    -2.25823509518346033e-5,-2.11479656768912971e-5,
                    -1.98200638885294927e-5,-1.85909870801065077e-5,
                    -1.74532699844210224e-5,-1.63997823854497997e-5,
                    -4.74617796559959808e-4,-4.77864567147321487e-4,
                    -3.20390228067037603e-4,-1.61105016119962282e-4,
                    -4.25778101285435204e-5,3.44571294294967503e-5,
                    7.97092684075674924e-5,1.031382367082722e-4,
                    1.12466775262204158e-4,1.13103642108481389e-4,
                    1.08651634848774268e-4,1.01437951597661973e-4,
                    9.29298396593363896e-5,8.40293133016089978e-5,
                    7.52727991349134062e-5,6.69632521975730872e-5,
                    5.92564547323194704e-5,5.22169308826975567e-5,
                    4.58539485165360646e-5,4.01445513891486808e-5,
                    3.50481730031328081e-5,3.05157995034346659e-5,
                    2.64956119950516039e-5,2.29363633690998152e-5,
                    1.97893056664021636e-5,1.70091984636412623e-5,
                    1.45547428261524004e-5,1.23886640995878413e-5,
                    1.04775876076583236e-5,8.79179954978479373e-6,
                    7.36465810572578444e-4,8.72790805146193976e-4,
                    6.22614862573135066e-4,2.85998154194304147e-4,
                    3.84737672879366102e-6,-1.87906003636971558e-4,
                    -2.97603646594554535e-4,-3.45998126832656348e-4,
                    -3.53382470916037712e-4,-3.35715635775048757e-4,
                    -3.04321124789039809e-4,-2.66722723047612821e-4,
                    -2.27654214122819527e-4,-1.89922611854562356e-4,
                    -1.5505891859909387e-4,-1.2377824076187363e-4,
                    -9.62926147717644187e-5,-7.25178327714425337e-5,
                    -5.22070028895633801e-5,-3.50347750511900522e-5,
                    -2.06489761035551757e-5,-8.70106096849767054e-6,
                    1.1369868667510029e-6,9.16426474122778849e-6,
                    1.5647778542887262e-5,2.08223629482466847e-5,
                    2.48923381004595156e-5,2.80340509574146325e-5,
                    3.03987774629861915e-5,3.21156731406700616e-5,
                    -.00180182191963885708,-.00243402962938042533,
                    -.00183422663549856802,-7.62204596354009765e-4,
                    2.39079475256927218e-4,9.49266117176881141e-4,
                    .00134467449701540359,.00148457495259449178,.00144732339830617591,
                    .00130268261285657186,.00110351597375642682,
                    8.86047440419791759e-4,6.73073208165665473e-4,
                    4.77603872856582378e-4,3.05991926358789362e-4,
                    1.6031569459472163e-4,4.00749555270613286e-5,
                    -5.66607461635251611e-5,-1.32506186772982638e-4,
                    -1.90296187989614057e-4,-2.32811450376937408e-4,
                    -2.62628811464668841e-4,-2.82050469867598672e-4,
                    -2.93081563192861167e-4,-2.97435962176316616e-4,
                    -2.96557334239348078e-4,-2.91647363312090861e-4,
                    -2.83696203837734166e-4,-2.73512317095673346e-4,
                    -2.6175015580676858e-4,.00638585891212050914,
                    .00962374215806377941,.00761878061207001043,.00283219055545628054,
                    -.0020984135201272009,-.00573826764216626498,
                    -.0077080424449541462,-.00821011692264844401,
                    -.00765824520346905413,-.00647209729391045177,
                    -.00499132412004966473,-.0034561228971313328,
                    -.00201785580014170775,-7.59430686781961401e-4,
                    2.84173631523859138e-4,.00110891667586337403,
                    .00172901493872728771,.00216812590802684701,.00245357710494539735,
                    .00261281821058334862,.00267141039656276912,.0026520307339598043,
                    .00257411652877287315,.00245389126236094427,.00230460058071795494,
                    .00213684837686712662,.00195896528478870911,.00177737008679454412,
                    .00159690280765839059,.00142111975664438546 };
                double[] gama = { .629960524947436582,.251984209978974633,
                    .154790300415655846,.110713062416159013,.0857309395527394825,
                    .0697161316958684292,.0586085671893713576,.0504698873536310685,
                    .0442600580689154809,.0393720661543509966,.0354283195924455368,
                    .0321818857502098231,.0294646240791157679,.0271581677112934479,
                    .0251768272973861779,.0234570755306078891,.0219508390134907203,
                    .020621082823564624,.0194388240897880846,.0183810633800683158,
                    .0174293213231963172,.0165685837786612353,.0157865285987918445,
                    .0150729501494095594,.0144193250839954639,.0138184805735341786,
                    .0132643378994276568,.0127517121970498651,.0122761545318762767,
                    .0118338262398482403 };

                double ang;
                double atol, aw2, azth, btol;
                double fn13, fn23;
                double pp, przthi, przthr, ptfni, ptfnr, raw, raw2;
                double razth, rfnu, rfnu2, rfn13, rtzti = 0, rtztr = 0, rzthi, rzthr, sti = 0, str = 0;
                double sumai, sumar, sumbi, sumbr, test, tfni, tfnr, tzai;
                double tzar, t2i, t2r, wi = 0, wr = 0, w2i, w2r, zai = 0, zar = 0, zbi, zbr;
                double zci = 0, zcr = 0, zetai, zetar;
                double zthi, zthr, ac;
                int ias, ibs, is1, j, jr, ju, k, kmax, kp1, ks, l, lr;
                int lrp1, l1, l2, m, idum = 0;

                double[] ap = new double[30];
                double[] cri = new double[14];
                double[] crr = new double[14];
                double[] dri = new double[14];
                double[] drr = new double[14];
                double[] pi = new double[30];
                double[] pr = new double[30];
                double[] upi = new double[14];
                double[] upr = new double[14];

                rfnu = 1.0 / fnu;
                //-----------------------------------------------------------------------
                //    OVERFLOW TEST (Z/FNU TOO SMALL)
                //-----------------------------------------------------------------------
                test = d1mach(1) * 1.0E3;
                ac = fnu * test;
                if (Math.Abs(zr) > ac || Math.Abs(zi) > ac) goto L15;
                zeta1r = Math.Abs(Math.Log(test)) * 2.0 + fnu;
                zeta1i = 0.0;
                zeta2r = fnu;
                zeta2i = 0.0;
                phir = 1.0;
                phii = 0.0;
                argr = 1.0;
                argi = 0.0;
                return 0;
            L15:
                zbr = zr * rfnu;
                zbi = zi * rfnu;
                rfnu2 = rfnu * rfnu;
                //-----------------------------------------------------------------------
                //    COMPUTE IN THE FOURTH QUADRANT
                //-----------------------------------------------------------------------
                fn13 = Math.Pow(fnu, ex1);
                fn23 = fn13 * fn13;
                rfn13 = 1.0 / fn13;
                w2r = coner - zbr * zbr + zbi * zbi;
                w2i = conei - zbr * zbi - zbr * zbi;
                aw2 = zabs(w2r, w2i);
                if (aw2 > 0.25) goto L130;
                //-----------------------------------------------------------------------
                //    POWER SERIES FOR ABS(W2).LE.0.25D0
                //-----------------------------------------------------------------------
                k = 1;
                pr[0] = coner;
                pi[0] = conei;
                sumar = gama[0];
                sumai = zeroi;
                ap[0] = 1.0;
                if (aw2 < tol) goto L20;
                for (k = 2; k <= 30; k++)
                {
                    pr[k - 1] = pr[k - 2] * w2r - pi[k - 2] * w2i;
                    pi[k - 1] = pr[k - 2] * w2i + pi[k - 2] * w2r;
                    sumar += pr[k - 1] * gama[k - 1];
                    sumai += pi[k - 1] * gama[k - 1];
                    ap[k - 1] = ap[k - 2] * aw2;
                    if (ap[k - 1] < tol) goto L20;
                }
                k = 30;
            L20:
                kmax = k;
                zetar = w2r * sumar - w2i * sumai;
                zetai = w2r * sumai + w2i * sumar;
                argr = zetar * fn23;
                argi = zetai * fn23;
                zsqrt(sumar, sumai, ref zar, ref zai);
                zsqrt(w2r, w2i, ref str, ref sti);
                zeta2r = str * fnu;
                zeta2i = sti * fnu;
                str = coner + ex2 * (zetar * zar - zetai * zai);
                sti = conei + ex2 * (zetar * zai + zetai * zar);
                zeta1r = str * zeta2r - sti * zeta2i;
                zeta1i = str * zeta2i + sti * zeta2r;
                zar = zar + zar;
                zai = zai + zai;
                zsqrt(zar, zai, ref str, ref sti);
                phir = str * rfn13;
                phii = sti * rfn13;
                if (ipmtr == 1) goto L120;
                //-----------------------------------------------------------------------
                //    SUM SERIES FOR ASUM AND BSUM
                //-----------------------------------------------------------------------
                sumbr = zeror;
                sumbi = zeroi;
                for (k = 1; k <= kmax; k++)
                {
                    sumbr += pr[k - 1] * beta[k - 1];
                    sumbi += pi[k - 1] * beta[k - 1];
                }
                asumr = zeror;
                asumi = zeroi;
                bsumr = sumbr;
                bsumi = sumbi;
                l1 = 0;
                l2 = 30;
                btol = tol * (Math.Abs(bsumr) + Math.Abs(bsumi));
                atol = tol;
                pp = 1.0;
                ias = 0;
                ibs = 0;
                if (rfnu2 < tol) goto L110;
                for (is1 = 2; is1 <= 7; is1++)
                {
                    atol /= rfnu2;
                    pp *= rfnu2;
                    if (ias == 1) goto L60;
                    sumar = zeror;
                    sumai = zeroi;
                    for (k = 1; k <= kmax; k++)
                    {
                        m = l1 + k;
                        sumar += pr[k - 1] * alfa[m - 1];
                        sumai += pi[k - 1] * alfa[m - 1];
                        if (ap[k - 1] < atol) goto L50;
                    }
            L50:
                    asumr += sumar * pp;
                    asumi += sumai * pp;
                    if (pp < tol) ias = 1;
            L60:
                    if (ibs == 1) goto L90;
                    sumbr = zeror;
                    sumbi = zeroi;
                    for (k = 1; k <= kmax; k++)
                    {
                        m = l2 + k;
                        sumbr += pr[k - 1] * beta[m - 1];
                        sumbi += pi[k - 1] * beta[m - 1];
                        if (ap[k - 1] < atol) goto L80;
                    }
            L80:
                    bsumr += sumbr * pp;
                    bsumi += sumbi * pp;
                    if (pp < btol) ibs = 1;
            L90:
                    if (ias == 1 && ibs == 1) goto L110;
                    l1 += 30;
                    l2 += 30;
                }
            L110:
                asumr += coner;
                pp = rfnu * rfn13;
                bsumr *= pp;
                bsumi *= pp;
            L120:
                return 0;
                //-----------------------------------------------------------------------
                //    ABS(W2).GT.0.25D0
                //-----------------------------------------------------------------------
            L130:
                zsqrt(w2r, w2i, ref wr, ref wi);
                if (wr < 0.0) wr = 0.0;
                if (wi < 0.0) wi = 0.0;
                str = coner + wr;
                sti = wi;
                zdiv(str, sti, zbr, zbi, ref zar, ref zai);
                zlog(zar, zai, ref zcr, ref zci, ref idum);
                if (zci < 0.0) zci = 0.0;
                if (zci > hpi) zci = hpi;
                if (zcr < 0.0) zcr = 0.0;
                zthr = (zcr - wr) * 1.5;
                zthi = (zci - wi) * 1.5;
                zeta1r = zcr * fnu;
                zeta1i = zci * fnu;
                zeta2r = wr * fnu;
                zeta2i = wi * fnu;
                azth = zabs(zthr, zthi);
                ang = thpi;
                if (zthr >= 0.0 && zthi < 0.0) goto L140;
                ang = hpi;
                if (zthr == 0.0) goto L140;
                ang = Math.Atan(zthi / zthr);
                if (zthr < 0.0) ang += gpi;
            L140:
                pp = Math.Pow(azth, ex2);
                ang *= ex2;
                zetar = pp * Math.Cos(ang);
                zetai = pp * Math.Sin(ang);
                if (zetai < 0.0) zetai = 0.0;
                argr = zetar * fn23;
                argi = zetai * fn23;
                zdiv(zthr, zthi, zetar, zetai, ref rtztr, ref rtzti);
                zdiv(rtztr, rtzti, wr, wi, ref zar, ref zai);
                tzar = zar + zar;
                tzai = zai + zai;
                zsqrt(tzar, tzai, ref str, ref sti);
                phir = str * rfn13;
                phii = sti * rfn13;
                if (ipmtr == 1) goto L120;
                raw = 1.0 / Math.Sqrt(aw2);
                str = wr * raw;
                sti = -wi * raw;
                tfnr = str * rfnu * raw;
                tfni = sti * rfnu * raw;
                razth = 1.0 / azth;
                str = zthr * razth;
                sti = -zthi * razth;
                rzthr = str * razth * rfnu;
                rzthi = sti * razth * rfnu;
                zcr = rzthr * ar[1];
                zci = rzthi * ar[1];
                raw2 = 1.0 / aw2;
                str = w2r * raw2;
                sti = -w2i * raw2;
                t2r = str * raw2;
                t2i = sti * raw2;
                str = t2r * c[1] + c[2];
                sti = t2i * c[1];
                upr[1] = str * tfnr - sti * tfni;
                upi[1] = str * tfni + sti * tfnr;
                bsumr = upr[1] + zcr;
                bsumi = upi[1] + zci;
                asumr = zeror;
                asumi = zeroi;
                if (rfnu < tol) goto L220;
                przthr = rzthr;
                przthi = rzthi;
                ptfnr = tfnr;
                ptfni = tfni;
                upr[0] = coner;
                upi[0] = conei;
                pp = 1.0;
                btol = tol * (Math.Abs(bsumr) + Math.Abs(bsumi));
                ks = 0;
                kp1 = 2;
                l = 3;
                ias = 0;
                ibs = 0;
                for (lr = 2; lr <= 12; lr += 2)
                {
                    lrp1 = lr + 1;
                    //-----------------------------------------------------------------------
                    //    COMPUTE TWO ADDITIONAL CR, DR, AND UP FOR TWO MORE TERMS IN
                    //    NEXT SUMA AND SUMB
                    //-----------------------------------------------------------------------
                    for (k = lr; k <= lrp1; k++)
                    {
                        ks++;
                        kp1++;
                        l++;
                        zar = c[l - 1];
                        zai = zeroi;
                        for (j = 2; j <= kp1; j++)
                        {
                            l++;
                            str = zar * t2r - t2i * zai + c[l - 1];
                            zai = zar * t2i + zai * t2r;
                            zar = str;
                        }
                        str = ptfnr * tfnr - ptfni * tfni;
                        ptfni = ptfnr * tfni + ptfni * tfnr;
                        ptfnr = str;
                        upr[kp1 - 1] = ptfnr * zar - ptfni * zai;
                        upi[kp1 - 1] = ptfni * zar + ptfnr * zai;
                        crr[ks - 1] = przthr * br[ks];
                        cri[ks - 1] = przthi * br[ks];
                        str = przthr * rzthr - przthi * rzthi;
                        przthi = przthr * rzthi + przthi * rzthr;
                        przthr = str;
                        drr[ks - 1] = przthr * ar[ks + 1];
                        dri[ks - 1] = przthi * ar[ks + 1];
                    }
                    pp *= rfnu2;
                    if (ias == 1) goto L180;
                    sumar = upr[lrp1 - 1];
                    sumai = upi[lrp1 - 1];
                    ju = lrp1;
                    for (jr = 1; jr <= lr; jr++)
                    {
                        ju--;
                        sumar = sumar + crr[jr - 1] * upr[ju - 1] - cri[jr - 1] * upi[ju - 1];
                        sumai = sumai + crr[jr - 1] * upi[ju - 1] + cri[jr - 1] * upr[ju - 1];
                    }
                    asumr += sumar;
                    asumi += sumai;
                    test = Math.Abs(sumar) + Math.Abs(sumai);
                    if (pp < tol && test < tol) ias = 1;
            L180:
                    if (ibs == 1) goto L200;
                    sumbr = upr[lr + 1] + upr[lrp1 - 1] * zcr - upi[lrp1 - 1] * zci;
                    sumbi = upi[lr + 1] + upr[lrp1 - 1] * zci + upi[lrp1 - 1] * zcr;
                    ju = lrp1;
                    for (jr = 1; jr <= lr; jr++)
                    {
                        ju--;
                        sumbr = sumbr + drr[jr - 1] * upr[ju - 1] - dri[jr - 1] * upi[ju - 1];
                        sumbi = sumbi + drr[jr - 1] * upi[ju - 1] + dri[jr - 1] * upr[ju - 1];
                    }
                    bsumr += sumbr;
                    bsumi += sumbi;
                    test = Math.Abs(sumbr) + Math.Abs(sumbi);
                    if (pp < btol && test < btol) ibs = 1;
            L200:
                    if (ias == 1 && ibs == 1) goto L220;
                }
            L220:
                asumr += coner;
                str = -(bsumr) * rfn13;
                sti = -(bsumi) * rfn13;
                zdiv(str, sti, rtztr, rtzti, ref bsumr, ref bsumi);
                goto L120;
            }

            static int zuni1(double zr, double zi, double fnu, int kode, int n, double[] yr, double[] yi, ref int nz, ref int nlast, double fnul, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZUNI1
                //***REFER TO  ZBESI,ZBESK
                //
                //     ZUNI1 COMPUTES I(FNU,Z)  BY MEANS OF THE UNIFORM ASYMPTOTIC
                //     EXPANSION FOR I(FNU,Z) IN -PI/3.LE.ARG Z.LE.PI/3.
                //
                //     FNUL IS THE SMALLEST ORDER PERMITTED FOR THE ASYMPTOTIC
                //     EXPANSION. NLAST=0 MEANS ALL OF THE Y VALUES WERE SET.
                //     NLAST.NE.0 IS THE NUMBER LEFT TO BE COMPUTED BY ANOTHER
                //     FORMULA FOR ORDERS FNU TO FNU+NLAST-1 BECAUSE FNU+NLAST-1.LT.FNUL.
                //     Y(I)=CZERO FOR I=NLAST+1,N
                //
                //***ROUTINES CALLED  ZUCHK,ZUNIK,ZUOIK,D1MACH,ZABS
                //***END PROLOGUE  ZUNI1

                #endregion

                const double zeror = 0.0;
                const double zeroi = 0.0;
                const double coner = 1.0;

                double aphi, ascle, crsc;
                double cscl, c1r, c2i, c2m, c2r, fn;
                double phii = 0, phir = 0, rast, rs1, rzi, rzr, sti, str, sumi = 0;
                double sumr = 0, s1i, s1r, s2i, s2r, zeta1i = 0;
                double zeta1r = 0, zeta2i = 0, zeta2r = 0;
                int i, iflag = 0, init, k, m, nd, nn, nuf = 0, nw = 0;

                double[] bry = new double[3];
                double[] csrr = new double[3];
                double[] cssr = new double[3];
                double[] cwrki = new double[16];
                double[] cwrkr = new double[16];
                double[] cyi = new double[2];
                double[] cyr = new double[2];

                nz = 0;
                nd = n;
                nlast = 0;
                //-----------------------------------------------------------------------
                //    COMPUTED VALUES WITH EXPONENTS BETWEEN ALIM AND ELIM IN MAG-
                //    NITUDE ARE SCALED TO KEEP INTERMEDIATE ARITHMETIC ON SCALE,
                //    EXP(ALIM)=EXP(ELIM)*TOL
                //-----------------------------------------------------------------------
                cscl = 1.0 / tol;
                crsc = tol;
                cssr[0] = cscl;
                cssr[1] = coner;
                cssr[2] = crsc;
                csrr[0] = crsc;
                csrr[1] = coner;
                csrr[2] = cscl;
                bry[0] = d1mach(1) * 1.0E3 / tol;
                //-----------------------------------------------------------------------
                //    CHECK FOR UNDERFLOW AND OVERFLOW ON FIRST MEMBER
                //-----------------------------------------------------------------------
                fn = Math.Max(fnu, 1.0);
                init = 0;
                zunik(zr, zi, fn, 1, 1, tol, ref init, ref phir, ref phii, ref zeta1r, ref zeta1i, ref zeta2r, ref zeta2i, ref sumr, ref sumi, ref cwrkr, ref cwrki);
                if (kode == 1) goto L10;
                str = zr + zeta2r;
                sti = zi + zeta2i;
                rast = fn / zabs(str, sti);
                str = str * rast * rast;
                sti = -sti * rast * rast;
                s1r = -zeta1r + str;
                s1i = -zeta1i + sti;
                goto L20;
            L10:
                s1r = -zeta1r + zeta2r;
                s1i = -zeta1i + zeta2i;
            L20:
                rs1 = s1r;
                if (Math.Abs(rs1) > elim) goto L130;
            L30:
                nn = Math.Min(2, nd);
                for (i = 1; i <= nn; i++)
                {
                    fn = fnu + (nd - i);
                    init = 0;
                    zunik(zr, zi, fn, 1, 0, tol, ref init, ref phir, ref phii, ref zeta1r, ref zeta1i, ref zeta2r, ref zeta2i, ref sumr, ref sumi, ref cwrkr, ref cwrki);
                    if (kode == 1) goto L40;
                    str = zr + zeta2r;
                    sti = zi + zeta2i;
                    rast = fn / zabs(str, sti);
                    str = str * rast * rast;
                    sti = -sti * rast * rast;
                    s1r = -zeta1r + str;
                    s1i = -zeta1i + sti + zi;
                    goto L50;
            L40:
                    s1r = -zeta1r + zeta2r;
                    s1i = -zeta1i + zeta2i;
            L50:
                    //-----------------------------------------------------------------------
                    //    TEST FOR UNDERFLOW AND OVERFLOW
                    //-----------------------------------------------------------------------
                    rs1 = s1r;
                    if (Math.Abs(rs1) > elim) goto L110;
                    if (i == 1) iflag = 2;
                    if (Math.Abs(rs1) < alim) goto L60;
                    //-----------------------------------------------------------------------
                    //    REFINE  TEST AND SCALE
                    //-----------------------------------------------------------------------
                    aphi = zabs(phir, phii);
                    rs1 += Math.Log(aphi);
                    if (Math.Abs(rs1) > elim) goto L110;
                    if (i == 1) iflag = 1;
                    if (rs1 < 0.0) goto L60;
                    if (i == 1) iflag = 3;
            L60:
                    //-----------------------------------------------------------------------
                    //    SCALE S1 IF ABS(S1).LT.ASCLE
                    //-----------------------------------------------------------------------
                    s2r = phir * sumr - phii * sumi;
                    s2i = phir * sumi + phii * sumr;
                    str = Math.Exp(s1r) * cssr[iflag - 1];
                    s1r = str * Math.Cos(s1i);
                    s1i = str * Math.Sin(s1i);
                    str = s2r * s1r - s2i * s1i;
                    s2i = s2r * s1i + s2i * s1r;
                    s2r = str;
                    if (iflag != 1) goto L70;
                    zuchk(s2r, s2i, ref nw, bry[0], tol);
                    if (nw != 0) goto L110;
            L70:
                    cyr[i - 1] = s2r;
                    cyi[i - 1] = s2i;
                    m = nd - i + 1;
                    yr[m - 1] = s2r * csrr[iflag - 1];
                    yi[m - 1] = s2i * csrr[iflag - 1];
                }
                if (nd <= 2) goto L100;
                rast = 1.0 / zabs(zr, zi);
                str = zr * rast;
                sti = -zi * rast;
                rzr = (str + str) * rast;
                rzi = (sti + sti) * rast;
                bry[1] = 1.0 / bry[0];
                bry[2] = d1mach(2);
                s1r = cyr[0];
                s1i = cyi[0];
                s2r = cyr[1];
                s2i = cyi[1];
                c1r = csrr[iflag - 1];
                ascle = bry[iflag - 1];
                k = nd - 2;
                fn = (double)k;
                for (i = 3; i <= nd; i++)
                {
                    c2r = s2r;
                    c2i = s2i;
                    s2r = s1r + (fnu + fn) * (rzr * c2r - rzi * c2i);
                    s2i = s1i + (fnu + fn) * (rzr * c2i + rzi * c2r);
                    s1r = c2r;
                    s1i = c2i;
                    c2r = s2r * c1r;
                    c2i = s2i * c1r;
                    yr[k - 1] = c2r;
                    yi[k - 1] = c2i;
                    k--;
                    fn += -1.0;
                    if (iflag >= 3) goto L90;
                    str = Math.Abs(c2r);
                    sti = Math.Abs(c2i);
                    c2m = Math.Max(str, sti);
                    if (c2m <= ascle) goto L90;
                    iflag++;
                    ascle = bry[iflag - 1];
                    s1r *= c1r;
                    s1i *= c1r;
                    s2r = c2r;
                    s2i = c2i;
                    s1r *= cssr[iflag - 1];
                    s1i *= cssr[iflag - 1];
                    s2r *= cssr[iflag - 1];
                    s2i *= cssr[iflag - 1];
                    c1r = csrr[iflag - 1];
            L90:
                    ;
                }
            L100:
                return 0;
                //-----------------------------------------------------------------------
                //    SET UNDERFLOW AND UPDATE PARAMETERS
                //-----------------------------------------------------------------------
            L110:
                if (rs1 > 0.0)
                {
                    goto L120;
                }
                yr[nd - 1] = zeror;
                yi[nd - 1] = zeroi;
                nz++;
                nd--;
                if (nd == 0)
                {
                    goto L100;
                }
                zuoik(zr, zi, fnu, kode, 1, nd, yr, yi, ref nuf, tol, elim, alim);
                if (nuf < 0)
                {
                    goto L120;
                }
                nd -= nuf;
                nz += nuf;
                if (nd == 0)
                {
                    goto L100;
                }
                fn = fnu + (nd - 1);
                if (fn >= fnul)
                {
                    goto L30;
                }
                nlast = nd;
                return 0;
            L120:
                nz = -1;
                return 0;
            L130:
                if (rs1 > 0.0)
                {
                    goto L120;
                }
                nz = n;
                for (i = 1; i <= n; i++)
                {
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                }
                return 0;
            }

            static int zuni2(double zr, double zi, double fnu, int kode, int n, double[] yr, double[] yi, ref int nz, ref int nlast, double fnul, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZUNI2
                //***REFER TO  ZBESI,ZBESK
                //
                //     ZUNI2 COMPUTES I(FNU,Z) IN THE RIGHT HALF PLANE BY MEANS OF
                //     UNIFORM ASYMPTOTIC EXPANSION FOR J(FNU,ZN) WHERE ZN IS Z*I
                //     OR -Z*I AND ZN IS IN THE RIGHT HALF PLANE ALSO.
                //
                //     FNUL IS THE SMALLEST ORDER PERMITTED FOR THE ASYMPTOTIC
                //     EXPANSION. NLAST=0 MEANS ALL OF THE Y VALUES WERE SET.
                //     NLAST.NE.0 IS THE NUMBER LEFT TO BE COMPUTED BY ANOTHER
                //     FORMULA FOR ORDERS FNU TO FNU+NLAST-1 BECAUSE FNU+NLAST-1.LT.FNUL.
                //     Y(I)=CZERO FOR I=NLAST+1,N
                //
                //***ROUTINES CALLED  ZAIRY,ZUCHK,ZUNHJ,ZUOIK,D1MACH,ZABS
                //***END PROLOGUE  ZUNI2

                #endregion

                const double aic = 1.265512123484645396;
                const double coner = 1.0;
                const double hpi = 1.570796326794896619231321696;
                const double zeroi = 0.0;
                const double zeror = 0.0;

                double aarg, aii = 0, air = 0, ang, aphi, argi = 0;
                double argr = 0, ascle, asumi = 0, asumr = 0, bsumi = 0, bsumr = 0, cidi;
                double crsc, cscl, c1r, c2i, c2m, c2r, daii = 0;
                double dair = 0, fn, phii = 0, phir = 0, rast, raz, rs1, rzi;
                double rzr, sti, str, s1i, s1r, s2i, s2r, zbi, zbr;
                double zeta1i = 0, zeta1r = 0, zeta2i = 0, zeta2r = 0, zni, znr;
                double car, sar;
                int i, iflag = 0, ink, inu, j, k, nai = 0, nd, ndai = 0;
                int nn, nuf = 0, nw = 0, idum = 0;

                double[] cipi = { 0.0, 1.0, 0.0, -1.0 };
                double[] cipr = { 1.0, 0.0, -1.0, 0.0 };
                double[] bry = new double[3];
                double[] csrr = new double[3];
                double[] cssr = new double[3];
                double[] cyi = new double[2];
                double[] cyr = new double[2];

                nz = 0;
                nd = n;
                nlast = 0;
                // -----------------------------------------------------------------------
                //     COMPUTED VALUES WITH EXPONENTS BETWEEN ALIM AND ELIM IN MAG-
                //     NITUDE ARE SCALED TO KEEP INTERMEDIATE ARITHMETIC ON SCALE,
                //     EXP(ALIM)=EXP(ELIM)*TOL
                // -----------------------------------------------------------------------
                cscl = 1.0 / tol;
                crsc = tol;
                cssr[0] = cscl;
                cssr[1] = coner;
                cssr[2] = crsc;
                csrr[0] = crsc;
                csrr[1] = coner;
                csrr[2] = cscl;
                bry[0] = d1mach(1) * 1.0E3 / tol;
                // -----------------------------------------------------------------------
                //     ZN IS IN THE RIGHT HALF PLANE AFTER ROTATION BY CI OR -CI
                // -----------------------------------------------------------------------
                znr = zi;
                zni = -zr;
                zbr = zr;
                zbi = zi;
                cidi = -coner;
                inu = (int)fnu;
                ang = hpi * (fnu - (double)inu);
                c2r = Math.Cos(ang);
                c2i = Math.Sin(ang);
                car = c2r;
                sar = c2i;
                ink = inu + n - 1;
                ink = (ink % 4) + 1;
                str = c2r * cipr[ink - 1] - c2i * cipi[ink - 1];
                c2i = c2r * cipi[ink - 1] + c2i * cipr[ink - 1];
                c2r = str;
                if (zi > 0.0) goto L10;
                znr = -znr;
                zbi = -zbi;
                cidi = -cidi;
                c2i = -c2i;
            L10:
                // -----------------------------------------------------------------------
                //     CHECK FOR UNDERFLOW AND OVERFLOW ON FIRST MEMBER
                // -----------------------------------------------------------------------
                fn = Math.Max(fnu, 1.0);
                zunhj(znr, zni, fn, 1, tol, ref phir, ref phii, ref argr, ref argi, ref zeta1r, ref zeta1i, ref zeta2r, ref zeta2i, ref asumr, ref asumi, ref bsumr, ref bsumi);
                if (kode == 1) goto L20;
                str = zbr + zeta2r;
                sti = zbi + zeta2i;
                rast = fn / zabs(str, sti);
                str = str * rast * rast;
                sti = -sti * rast * rast;
                s1r = -zeta1r + str;
                s1i = -zeta1i + sti;
                goto L30;
            L20:
                s1r = -zeta1r + zeta2r;
                s1i = -zeta1i + zeta2i;
            L30:
                rs1 = s1r;
                if (Math.Abs(rs1) > elim) goto L150;
            L40:
                nn = Math.Min(2, nd);
                for (i = 1; i <= nn; i++)
                {
                    fn = fnu + (nd - i);
                    zunhj(znr, zni, fn, 0, tol, ref phir, ref phii, ref argr, ref argi, ref zeta1r, ref zeta1i, ref zeta2r, ref zeta2i, ref asumr, ref asumi, ref bsumr, ref bsumi);
                    if (kode == 1) goto L50;
                    str = zbr + zeta2r;
                    sti = zbi + zeta2i;
                    rast = fn / zabs(str, sti);
                    str = str * rast * rast;
                    sti = -sti * rast * rast;
                    s1r = -zeta1r + str;
                    s1i = -zeta1i + sti + Math.Abs(zi);
                    goto L60;
            L50:
                    s1r = -zeta1r + zeta2r;
                    s1i = -zeta1i + zeta2i;
            L60:
                    // -----------------------------------------------------------------------
                    //     TEST FOR UNDERFLOW AND OVERFLOW
                    // -----------------------------------------------------------------------
                    rs1 = s1r;
                    if (Math.Abs(rs1) > elim) goto L120;
                    if (i == 1) iflag = 2;
                    if (Math.Abs(rs1) < alim) goto L70;
                    // -----------------------------------------------------------------------
                    //     REFINE  TEST AND SCALE
                    // -----------------------------------------------------------------------
                    // -----------------------------------------------------------------------
                    aphi = zabs(phir, phii);
                    aarg = zabs(argr, argi);
                    rs1 = rs1 + Math.Log(aphi) - Math.Log(aarg) * 0.25 - aic;
                    if (Math.Abs(rs1) > elim) goto L120;
                    if (i == 1) iflag = 1;
                    if (rs1 < 0.0) goto L70;
                    if (i == 1) iflag = 3;
            L70:
                    // -----------------------------------------------------------------------
                    //     SCALE S1 TO KEEP INTERMEDIATE ARITHMETIC ON SCALE NEAR
                    //     EXPONENT EXTREMES
                    // -----------------------------------------------------------------------
                    zairy(argr, argi, 0, 2, ref air, ref aii, ref nai, ref idum);
                    zairy(argr, argi, 1, 2, ref dair, ref daii, ref ndai, ref idum);
                    str = dair * bsumr - daii * bsumi;
                    sti = dair * bsumi + daii * bsumr;
                    str += air * asumr - aii * asumi;
                    sti += air * asumi + aii * asumr;
                    s2r = phir * str - phii * sti;
                    s2i = phir * sti + phii * str;
                    str = Math.Exp(s1r) * cssr[iflag - 1];
                    s1r = str * Math.Cos(s1i);
                    s1i = str * Math.Sin(s1i);
                    str = s2r * s1r - s2i * s1i;
                    s2i = s2r * s1i + s2i * s1r;
                    s2r = str;
                    if (iflag != 1) goto L80;
                    zuchk(s2r, s2i, ref nw, bry[0], tol);
                    if (nw != 0) goto L120;
            L80:
                    if (zi <= 0.0) s2i = -s2i;
                    str = s2r * c2r - s2i * c2i;
                    s2i = s2r * c2i + s2i * c2r;
                    s2r = str;
                    cyr[i - 1] = s2r;
                    cyi[i - 1] = s2i;
                    j = nd - i + 1;
                    yr[j - 1] = s2r * csrr[iflag - 1];
                    yi[j - 1] = s2i * csrr[iflag - 1];
                    str = -c2i * cidi;
                    c2i = c2r * cidi;
                    c2r = str;
                }
                if (nd <= 2) goto L110;
                raz = 1.0 / zabs(zr, zi);
                str = zr * raz;
                sti = -zi * raz;
                rzr = (str + str) * raz;
                rzi = (sti + sti) * raz;
                bry[1] = 1.0 / bry[0];
                bry[2] = d1mach(2);
                s1r = cyr[0];
                s1i = cyi[0];
                s2r = cyr[1];
                s2i = cyi[1];
                c1r = csrr[iflag - 1];
                ascle = bry[iflag - 1];
                k = nd - 2;
                fn = (double)k;
                for (i = 3; i <= nd; i++)
                {
                    c2r = s2r;
                    c2i = s2i;
                    s2r = s1r + (fnu + fn) * (rzr * c2r - rzi * c2i);
                    s2i = s1i + (fnu + fn) * (rzr * c2i + rzi * c2r);
                    s1r = c2r;
                    s1i = c2i;
                    c2r = s2r * c1r;
                    c2i = s2i * c1r;
                    yr[k - 1] = c2r;
                    yi[k - 1] = c2i;
                    k--;
                    fn += -1.0;
                    if (iflag >= 3) goto L100;
                    str = Math.Abs(c2r);
                    sti = Math.Abs(c2i);
                    c2m = Math.Max(str, sti);
                    if (c2m <= ascle) goto L100;
                    iflag++;
                    ascle = bry[iflag - 1];
                    s1r *= c1r;
                    s1i *= c1r;
                    s2r = c2r;
                    s2i = c2i;
                    s1r *= cssr[iflag - 1];
                    s1i *= cssr[iflag - 1];
                    s2r *= cssr[iflag - 1];
                    s2i *= cssr[iflag - 1];
                    c1r = csrr[iflag - 1];
            L100:
                    ;
                }
            L110:
                return 0;
            L120:
                if (rs1 > 0.0) goto L140;
                // -----------------------------------------------------------------------
                //     SET UNDERFLOW AND UPDATE PARAMETERS
                // -----------------------------------------------------------------------
                yr[nd - 1] = zeror;
                yi[nd - 1] = zeroi;
                nz++;
                nd--;
                if (nd == 0) goto L110;
                zuoik(zr, zi, fnu, kode, 1, nd, yr, yi, ref nuf, tol, elim, alim);
                if (nuf < 0) goto L140;
                nd -= nuf;
                nz += nuf;
                if (nd == 0) goto L110;
                fn = fnu + (nd - 1);
                if (fn < fnul) goto L130;
                //      FN = CIDI
                //      J = NUF + 1
                //      K = MOD(J,4) + 1
                //      S1R = CIPR(K)
                //      S1I = CIPI(K)
                //      IF (FN.LT.0.0D0) S1I = -S1I
                //      STR = C2R*S1R - C2I*S1I
                //      C2I = C2R*S1I + C2I*S1R
                //      C2R = STR
                ink = inu + nd - 1;
                ink = (ink % 4) + 1;
                c2r = car * cipr[ink - 1] - sar * cipi[ink - 1];
                c2i = car * cipi[ink - 1] + sar * cipr[ink - 1];
                if (zi <= 0.0) c2i = -c2i;
                goto L40;
            L130:
                nlast = nd;
                return 0;
            L140:
                nz = -1;
                return 0;
            L150:
                if (rs1 > 0.0) goto L140;
                nz = n;
                for (i = 1; i <= n; i++)
                {
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                }
                return 0;
            }

            static int zunik(double zrr, double zri, double fnu, int ikflg, int ipmtr, double tol, ref int init, ref double phir, ref double phii, ref double zeta1r, ref double zeta1i, ref double zeta2r, ref double zeta2i, ref double sumr, ref double sumi, ref double[] cwrkr, ref double[] cwrki)
            {
                #region Description

                //***BEGIN PROLOGUE  ZUNIK
                //***REFER TO  ZBESI,ZBESK
                //
                //        ZUNIK COMPUTES PARAMETERS FOR THE UNIFORM ASYMPTOTIC
                //        EXPANSIONS OF THE I AND K FUNCTIONS ON IKFLG= 1 OR 2
                //        RESPECTIVELY BY
                //
                //        W(FNU,ZR) = PHI*EXP(ZETA)*SUM
                //
                //        WHERE       ZETA=-ZETA1 + ZETA2       OR
                //                          ZETA1 - ZETA2
                //
                //        THE FIRST CALL MUST HAVE INIT=0. SUBSEQUENT CALLS WITH THE
                //        SAME ZR AND FNU WILL RETURN THE I OR K FUNCTION ON IKFLG=
                //        1 OR 2 WITH NO CHANGE IN INIT. CWRK IS A COMPLEX WORK
                //        ARRAY. IPMTR=0 COMPUTES ALL PARAMETERS. IPMTR=1 COMPUTES PHI,
                //        ZETA1,ZETA2.
                //
                //***ROUTINES CALLED  ZDIV,ZLOG,ZSQRT,D1MACH
                //***END PROLOGUE  ZUNIK

                #endregion

                const double conei = 0.0;
                const double coner = 1.0;
                const double zeroi = 0.0;
                const double zeror = 0.0;

                double[] con = { 0.398942280401432678, 1.25331413731550025 };
                double[] c = { 1.0,-.208333333333333333, .125,
                    .334201388888888889,-.401041666666666667,.0703125,
                    -1.02581259645061728,1.84646267361111111,-.8912109375,.0732421875,
                    4.66958442342624743,-11.2070026162229938,8.78912353515625,
                    -2.3640869140625,.112152099609375,-28.2120725582002449,
                    84.6362176746007346,-91.8182415432400174,42.5349987453884549,
                    -7.3687943594796317,.227108001708984375,212.570130039217123,
                    -765.252468141181642,1059.99045252799988,-699.579627376132541,
                    218.19051174421159,-26.4914304869515555,.572501420974731445,
                    -1919.457662318407,8061.72218173730938,-13586.5500064341374,
                    11655.3933368645332,-5305.64697861340311,1200.90291321635246,
                    -108.090919788394656,1.7277275025844574,20204.2913309661486,
                    -96980.5983886375135,192547.001232531532,-203400.177280415534,
                    122200.46498301746,-41192.6549688975513,7109.51430248936372,
                    -493.915304773088012,6.07404200127348304,-242919.187900551333,
                    1311763.6146629772,-2998015.91853810675,3763271.297656404,
                    -2813563.22658653411,1268365.27332162478,-331645.172484563578,
                    45218.7689813627263,-2499.83048181120962,24.3805296995560639,
                    3284469.85307203782,-19706819.1184322269,50952602.4926646422,
                    -74105148.2115326577,66344512.2747290267,-37567176.6607633513,
                    13288767.1664218183,-2785618.12808645469,308186.404612662398,
                    -13886.0897537170405,110.017140269246738,-49329253.664509962,
                    325573074.185765749,-939462359.681578403,1553596899.57058006,
                    -1621080552.10833708,1106842816.82301447,-495889784.275030309,
                    142062907.797533095,-24474062.7257387285,2243768.17792244943,
                    -84005.4336030240853,551.335896122020586,814789096.118312115,
                    -5866481492.05184723,18688207509.2958249,-34632043388.1587779,
                    41280185579.753974,-33026599749.8007231,17954213731.1556001,
                    -6563293792.61928433,1559279864.87925751,-225105661.889415278,
                    17395107.5539781645,-549842.327572288687,3038.09051092238427,
                    -14679261247.6956167,114498237732.02581,-399096175224.466498,
                    819218669548.577329,-1098375156081.22331,1008158106865.38209,
                    -645364869245.376503,287900649906.150589,-87867072178.0232657,
                    17634730606.8349694,-2167164983.22379509,143157876.718888981,
                    -3871833.44257261262,18257.7554742931747,286464035717.679043,
                    -2406297900028.50396,9109341185239.89896,-20516899410934.4374,
                    30565125519935.3206,-31667088584785.1584,23348364044581.8409,
                    -12320491305598.2872,4612725780849.13197,-1196552880196.1816,
                    205914503232.410016,-21822927757.5292237,1247009293.51271032,
                    -29188388.1222208134,118838.426256783253 };

                double ac, crfni, crfnr;
                double rfn, si, sr, sri = 0, srr = 0, sti = 0, str = 0;
                double test, ti, tr, t2i = 0, t2r = 0;
                double zni = 0, znr = 0;
                int i, idum = 0, j, k, l;

                zeta1r = 0.0;
                zeta1i = 0.0;

                if (init != 0) goto L40;
                //-----------------------------------------------------------------------
                //    INITIALIZE ALL VARIABLES
                //-----------------------------------------------------------------------
                rfn = 1.0 / fnu;
                //-----------------------------------------------------------------------
                //    OVERFLOW TEST (ZR/FNU TOO SMALL)
                //-----------------------------------------------------------------------
                test = d1mach(1) * 1.0E3;
                ac = fnu * test;
                if (Math.Abs(zrr) > ac || Math.Abs(zri) > ac) goto L15;
                zeta1r = 2.0 * Math.Abs(Math.Log(test)) + fnu;
                zeta1i = 0.0;
                zeta2r = fnu;
                zeta2i = 0.0;
                phir = 1.0;
                phii = 0.0;
                return 0;
            L15:
                tr = zrr * rfn;
                ti = zri * rfn;
                sr = coner + (tr * tr - ti * ti);
                si = conei + (tr * ti + ti * tr);
                zsqrt(sr, si, ref srr, ref sri);
                str = coner + srr;
                sti = conei + sri;
                zdiv(str, sti, tr, ti, ref znr, ref zni);
                zlog(znr, zni, ref str, ref sti, ref idum);
                zeta1r = fnu * str;
                zeta1i = fnu * sti;
                zeta2r = fnu * srr;
                zeta2i = fnu * sri;
                zdiv(coner, conei, srr, sri, ref tr, ref ti);
                srr = tr * rfn;
                sri = ti * rfn;
                zsqrt(srr, sri, ref cwrkr[15], ref cwrki[15]);
                phir = cwrkr[15] * con[ikflg - 1];
                phii = cwrki[15] * con[ikflg - 1];
                if (ipmtr != 0) return 0;
                zdiv(coner, conei, sr, si, ref t2r, ref t2i);
                cwrkr[0] = coner;
                cwrki[0] = conei;
                crfnr = coner;
                crfni = conei;
                ac = 1.0;
                l = 1;
                for (k = 2; k <= 15; k++)
                {
                    sr = zeror;
                    si = zeroi;
                    for (j = 1; j <= k; j++)
                    {
                        l++;
                        str = sr * t2r - si * t2i + c[l - 1];
                        si = sr * t2i + si * t2r;
                        sr = str;
                    }
                    str = crfnr * srr - crfni * sri;
                    crfni = crfnr * sri + crfni * srr;
                    crfnr = str;
                    cwrkr[k - 1] = crfnr * sr - crfni * si;
                    cwrki[k - 1] = crfnr * si + crfni * sr;
                    ac *= rfn;
                    test = Math.Abs(cwrkr[k - 1]) + Math.Abs(cwrki[k - 1]);
                    if (ac < tol && test < tol) goto L30;
                }
                k = 15;
            L30:
                init = k;
            L40:
                if (ikflg == 2) goto L60;
                //-----------------------------------------------------------------------
                //    COMPUTE SUM FOR THE I FUNCTION
                //-----------------------------------------------------------------------
                sr = zeror;
                si = zeroi;
                for (i = 1; i <= init; i++)
                {
                    sr += cwrkr[i - 1];
                    si += cwrki[i - 1];
                }
                sumr = sr;
                sumi = si;
                phir = cwrkr[15] * con[0];
                phii = cwrki[15] * con[0];
                return 0;
            L60:
                //-----------------------------------------------------------------------
                //    COMPUTE SUM FOR THE K FUNCTION
                //-----------------------------------------------------------------------
                sr = zeror;
                si = zeroi;
                tr = coner;
                for (i = 1; i <= init; i++)
                {
                    sr += tr * cwrkr[i - 1];
                    si += tr * cwrki[i - 1];
                    tr = -tr;
                }
                sumr = sr;
                sumi = si;
                phir = cwrkr[15] * con[1];
                phii = cwrki[15] * con[1];
                return 0;
            }

            static int zunk1(double zr, double zi, double fnu, int kode, int mr, int n, double[] yr, double[] yi, ref int nz, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZUNK1
                //***REFER TO  ZBESK
                //
                //     ZUNK1 COMPUTES K(FNU,Z) AND ITS ANALYTIC CONTINUATION FROM THE
                //     RIGHT HALF PLANE TO THE LEFT HALF PLANE BY MEANS OF THE
                //     UNIFORM ASYMPTOTIC EXPANSION.
                //     MR INDICATES THE DIRECTION OF ROTATION FOR ANALYTIC CONTINUATION.
                //     NZ=-1 MEANS AN OVERFLOW WILL OCCUR
                //
                //***ROUTINES CALLED  ZKSCL,ZS1S2,ZUCHK,ZUNIK,D1MACH,ZABS
                //***END PROLOGUE  ZUNK1


                #endregion

                const double coner = 1.0;
                const double pi = 3.14159265358979323846264338327950;
                const double zeroi = 0.0;
                const double zeror = 0.0;

                double ang, aphi, asc, ascle, cki, ckr;
                double crsc, cscl, csgni, cspni, cspnr, csr;
                double c1i, c1r, c2i, c2m, c2r, fmr, fn = 0;
                double fnf, phidi = 0, phidr = 0, rast, razr, rs1, rzi;
                double rzr, sgn, sti, str, sumdi = 0, sumdr = 0, s1i, s1r, s2i;
                double s2r;
                double zet1di = 0, zet1dr = 0, zet2di = 0, zet2dr = 0, zri, zrr;
                int i, ib, iflag = 0, ifn, il, initd = 0, inu, iuf, k, kdflg, kflag = 0;
                int kk, m, nw = 0, ic, ipard, j;

                double[] bry = new double[3];
                double[] csrr = new double[3];
                double[] cssr = new double[3];
                double[][] cwrki = new double[3][]; //was [16][3]
                cwrki[0] = new double[16];
                cwrki[1] = new double[16];
                cwrki[2] = new double[16];
                double[][] cwrkr = new double[3][]; //was [16][3]
                cwrkr[0] = new double[16];
                cwrkr[1] = new double[16];
                cwrkr[2] = new double[16];
                double[] cyi = new double[2];
                double[] cyr = new double[2];
                double[] phii = new double[2];
                double[] phir = new double[2];
                double[] sumi = new double[2];
                double[] sumr = new double[2];
                double[] zeta1i = new double[2];
                double[] zeta2i = new double[2];
                double[] zeta1r = new double[2];
                double[] zeta2r = new double[2];
                int[] init = new int[2];

                kdflg = 1;
                nz = 0;
                //-----------------------------------------------------------------------
                //    EXP(-ALIM)=EXP(-ELIM)/TOL=APPROX. ONE PRECISION GREATER THAN
                //    THE UNDERFLOW LIMIT
                //-----------------------------------------------------------------------
                cscl = 1.0 / tol;
                crsc = tol;
                cssr[0] = cscl;
                cssr[1] = coner;
                cssr[2] = crsc;
                csrr[0] = crsc;
                csrr[1] = coner;
                csrr[2] = cscl;
                bry[0] = 1.0E3 * d1mach(1) / tol;
                bry[1] = 1.0 / bry[0];
                bry[2] = d1mach(2);
                zrr = zr;
                zri = zi;
                if (zr >= 0.0) goto L10;
                zrr = -zr;
                zri = -zi;
            L10:
                j = 2;
                for (i = 1; i <= n; i++)
                {
                    //-----------------------------------------------------------------------
                    //    J FLIP FLOPS BETWEEN 1 AND 2 IN J = 3 - J
                    //-----------------------------------------------------------------------
                    j = 3 - j;
                    fn = fnu + (i - 1);
                    init[j - 1] = 0;
                    zunik(zrr, zri, fn, 2, 0, tol, ref init[j - 1], ref phir[j - 1],
                        ref phii[j - 1], ref zeta1r[j - 1], ref zeta1i[j - 1], ref zeta2r[j - 1],
                        ref zeta2i[j - 1], ref sumr[j - 1], ref sumi[j - 1],
                        ref cwrkr[j - 1], ref cwrki[j - 1]);
                    if (kode == 1) goto L20;
                    str = zrr + zeta2r[j - 1];
                    sti = zri + zeta2i[j - 1];
                    rast = fn / zabs(str, sti);
                    str = str * rast * rast;
                    sti = -sti * rast * rast;
                    s1r = zeta1r[j - 1] - str;
                    s1i = zeta1i[j - 1] - sti;
                    goto L30;
            L20:
                    s1r = zeta1r[j - 1] - zeta2r[j - 1];
                    s1i = zeta1i[j - 1] - zeta2i[j - 1];
            L30:
                    rs1 = s1r;
                    //-----------------------------------------------------------------------
                    //    TEST FOR UNDERFLOW AND OVERFLOW
                    //-----------------------------------------------------------------------
                    if (Math.Abs(rs1) > elim) goto L60;
                    if (kdflg == 1) kflag = 2;
                    if (Math.Abs(rs1) < alim) goto L40;
                    //-----------------------------------------------------------------------
                    //    REFINE  TEST AND SCALE
                    //-----------------------------------------------------------------------
                    aphi = zabs(phir[j - 1], phii[j - 1]);
                    rs1 += Math.Log(aphi);
                    if (Math.Abs(rs1) > elim) goto L60;
                    if (kdflg == 1) kflag = 1;
                    if (rs1 < 0.0) goto L40;
                    if (kdflg == 1) kflag = 3;
            L40:
                    //-----------------------------------------------------------------------
                    //    SCALE S1 TO KEEP INTERMEDIATE ARITHMETIC ON SCALE NEAR
                    //    EXPONENT EXTREMES
                    //-----------------------------------------------------------------------
                    s2r = phir[j - 1] * sumr[j - 1] - phii[j - 1] * sumi[j - 1];
                    s2i = phir[j - 1] * sumi[j - 1] + phii[j - 1] * sumr[j - 1];
                    str = Math.Exp(s1r) * cssr[kflag - 1];
                    s1r = str * Math.Cos(s1i);
                    s1i = str * Math.Sin(s1i);
                    str = s2r * s1r - s2i * s1i;
                    s2i = s1r * s2i + s2r * s1i;
                    s2r = str;
                    if (kflag != 1) goto L50;
                    zuchk(s2r, s2i, ref nw, bry[0], tol);
                    if (nw != 0) goto L60;
            L50:
                    cyr[kdflg - 1] = s2r;
                    cyi[kdflg - 1] = s2i;
                    yr[i - 1] = s2r * csrr[kflag - 1];
                    yi[i - 1] = s2i * csrr[kflag - 1];
                    if (kdflg == 2) goto L75;
                    kdflg = 2;
                    goto L70;
            L60:
                    if (rs1 > 0.0) goto L300;
                    //-----------------------------------------------------------------------
                    //    FOR ZR.LT.0.0, THE I FUNCTION TO BE ADDED WILL OVERFLOW
                    //-----------------------------------------------------------------------
                    if (zr < 0.0) goto L300;
                    kdflg = 1;
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                    nz++;
                    if (i == 1) goto L70;
                    if (yr[i - 2] == zeror && yi[i - 2] == zeroi) goto L70;
                    yr[i - 2] = zeror;
                    yi[i - 2] = zeroi;
                    nz++;
            L70:
                    ;
                }
                i = n;
            L75:
                razr = 1.0 / zabs(zrr, zri);
                str = zrr * razr;
                sti = -zri * razr;
                rzr = (str + str) * razr;
                rzi = (sti + sti) * razr;
                ckr = fn * rzr;
                cki = fn * rzi;
                ib = i + 1;
                if (n < ib) goto L160;
                //-----------------------------------------------------------------------
                //    TEST LAST MEMBER FOR UNDERFLOW AND OVERFLOW. SET SEQUENCE TO ZERO
                //    ON UNDERFLOW.
                //-----------------------------------------------------------------------
                fn = fnu + (double)(n - 1);
                ipard = 1;
                if (mr != 0) ipard = 0;
                zunik(zrr, zri, fn, 2, ipard, tol, ref initd, ref phidr, ref phidi,
                    ref zet1dr, ref zet1di, ref zet2dr, ref zet2di, ref sumdr, ref sumdi, ref cwrkr[2],
                    ref cwrki[2]);
                if (kode == 1) goto L80;
                str = zrr + zet2dr;
                sti = zri + zet2di;
                rast = fn / zabs(str, sti);
                str = str * rast * rast;
                sti = -sti * rast * rast;
                s1r = zet1dr - str;
                s1i = zet1di - sti;
                goto L90;
            L80:
                s1r = zet1dr - zet2dr;
                s1i = zet1di - zet2di;
            L90:
                rs1 = s1r;
                if (Math.Abs(rs1) > elim) goto L95;
                if (Math.Abs(rs1) < alim) goto L100;
                //-----------------------------------------------------------------------
                //    REFINE ESTIMATE AND TEST
                //-----------------------------------------------------------------------
                aphi = zabs(phidr, phidi);
                rs1 += Math.Log(aphi);
                if (Math.Abs(rs1) < elim) goto L100;
            L95:
                if (Math.Abs(rs1) > 0.0) goto L300;
                //-----------------------------------------------------------------------
                //    FOR ZR.LT.0.0, THE I FUNCTION TO BE ADDED WILL OVERFLOW
                //-----------------------------------------------------------------------
                if (zr < 0.0) goto L300;
                nz = n;
                for (i = 1; i <= n; i++)
                {
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                }
                return 0;
                //-----------------------------------------------------------------------
                //    FORWARD RECUR FOR REMAINDER OF THE SEQUENCE
                //-----------------------------------------------------------------------
            L100:
                s1r = cyr[0];
                s1i = cyi[0];
                s2r = cyr[1];
                s2i = cyi[1];
                c1r = csrr[kflag - 1];
                ascle = bry[kflag - 1];
                for (i = ib; i <= n; i++)
                {
                    c2r = s2r;
                    c2i = s2i;
                    s2r = ckr * c2r - cki * c2i + s1r;
                    s2i = ckr * c2i + cki * c2r + s1i;
                    s1r = c2r;
                    s1i = c2i;
                    ckr += rzr;
                    cki += rzi;
                    c2r = s2r * c1r;
                    c2i = s2i * c1r;
                    yr[i - 1] = c2r;
                    yi[i - 1] = c2i;
                    if (kflag >= 3) goto L120;
                    str = Math.Abs(c2r);
                    sti = Math.Abs(c2i);
                    c2m = Math.Max(str, sti);
                    if (c2m <= ascle) goto L120;
                    kflag++;
                    ascle = bry[kflag - 1];
                    s1r *= c1r;
                    s1i *= c1r;
                    s2r = c2r;
                    s2i = c2i;
                    s1r *= cssr[kflag - 1];
                    s1i *= cssr[kflag - 1];
                    s2r *= cssr[kflag - 1];
                    s2i *= cssr[kflag - 1];
                    c1r = csrr[kflag - 1];
            L120:
                    ;
                }
            L160:
                if (mr == 0) return 0;
                //-----------------------------------------------------------------------
                //    ANALYTIC CONTINUATION FOR RE(Z).LT.0.0D0
                //-----------------------------------------------------------------------
                nz = 0;
                fmr = (double)mr;
                sgn = -dsign(pi, fmr);
                //-----------------------------------------------------------------------
                //    CSPN AND CSGN ARE COEFF OF K AND I FUNCTIONS RESP.
                //-----------------------------------------------------------------------
                csgni = sgn;
                inu = (int)fnu;
                fnf = fnu - (double)inu;
                ifn = inu + n - 1;
                ang = fnf * sgn;
                cspnr = Math.Cos(ang);
                cspni = Math.Sin(ang);
                if (ifn % 2 == 0) goto L170;
                cspnr = -cspnr;
                cspni = -cspni;
            L170:
                asc = bry[0];
                iuf = 0;
                kk = n;
                kdflg = 1;
                ib = ib - 1;
                ic = ib - 1;
                for (k = 1; k <= n; k++)
                {
                    fn = fnu + (double)(kk - 1);
                    //-----------------------------------------------------------------------
                    //    LOGIC TO SORT ref CASES WHOSE PARAMETERS WERE SET FOR THE K
                    //    FUNCTION ABOVE
                    //-----------------------------------------------------------------------
                    m = 3;
                    if (n > 2) goto L175;
            L172:
                    initd = init[j - 1];
                    phidr = phir[j - 1];
                    phidi = phii[j - 1];
                    zet1dr = zeta1r[j - 1];
                    zet1di = zeta1i[j - 1];
                    zet2dr = zeta2r[j - 1];
                    zet2di = zeta2i[j - 1];
                    sumdr = sumr[j - 1];
                    sumdi = sumi[j - 1];
                    m = j;
                    j = 3 - j;
                    goto L180;
            L175:
                    if (kk == n && ib < n) goto L180;
                    if (kk == ib || kk == ic) goto L172;
                    initd = 0;
            L180:
                    zunik(zrr, zri, fn, 1, 0, tol, ref initd, ref phidr, ref phidi,
                        ref zet1dr, ref zet1di, ref zet2dr, ref zet2di, ref sumdr, ref sumdi,
                        ref cwrkr[m - 1], ref cwrki[m - 1]);
                    if (kode == 1) goto L200;
                    str = zrr + zet2dr;
                    sti = zri + zet2di;
                    rast = fn / zabs(str, sti);
                    str = str * rast * rast;
                    sti = -sti * rast * rast;
                    s1r = -zet1dr + str;
                    s1i = -zet1di + sti;
                    goto L210;
            L200:
                    s1r = -zet1dr + zet2dr;
                    s1i = -zet1di + zet2di;
            L210:
                    //-----------------------------------------------------------------------
                    //    TEST FOR UNDERFLOW AND OVERFLOW
                    //-----------------------------------------------------------------------
                    rs1 = s1r;
                    if (Math.Abs(rs1) > elim) goto L260;
                    if (kdflg == 1) iflag = 2;
                    if (Math.Abs(rs1) < alim) goto L220;
                    //-----------------------------------------------------------------------
                    //    REFINE  TEST AND SCALE
                    //-----------------------------------------------------------------------
                    aphi = zabs(phidr, phidi);
                    rs1 += Math.Log(aphi);
                    if (Math.Abs(rs1) > elim) goto L260;
                    if (kdflg == 1) iflag = 1;
                    if (rs1 < 0.0) goto L220;
                    if (kdflg == 1) iflag = 3;
            L220:
                    str = phidr * sumdr - phidi * sumdi;
                    sti = phidr * sumdi + phidi * sumdr;
                    s2r = -csgni * sti;
                    s2i = csgni * str;
                    str = Math.Exp(s1r) * cssr[iflag - 1];
                    s1r = str * Math.Cos(s1i);
                    s1i = str * Math.Sin(s1i);
                    str = s2r * s1r - s2i * s1i;
                    s2i = s2r * s1i + s2i * s1r;
                    s2r = str;
                    if (iflag != 1) goto L230;
                    zuchk(s2r, s2i, ref nw, bry[0], tol);
                    if (nw == 0) goto L230;
                    s2r = zeror;
                    s2i = zeroi;
            L230:
                    cyr[kdflg - 1] = s2r;
                    cyi[kdflg - 1] = s2i;
                    c2r = s2r;
                    c2i = s2i;
                    s2r *= csrr[iflag - 1];
                    s2i *= csrr[iflag - 1];
                    //-----------------------------------------------------------------------
                    //    ADD I AND K FUNCTIONS, K SEQUENCE IN Y(I), I=1,N
                    //-----------------------------------------------------------------------
                    s1r = yr[kk - 1];
                    s1i = yi[kk - 1];
                    if (kode == 1) goto L250;
                    zs1s2(zrr, zri, ref s1r, ref s1i, ref s2r, ref s2i, ref nw, asc, alim, ref iuf);
                    nz += nw;
            L250:
                    yr[kk - 1] = s1r * cspnr - s1i * cspni + s2r;
                    yi[kk - 1] = cspnr * s1i + cspni * s1r + s2i;
                    kk--;
                    cspnr = -cspnr;
                    cspni = -cspni;
                    if (c2r != 0.0 || c2i != 0.0) goto L255;
                    kdflg = 1;
                    goto L270;
            L255:
                    if (kdflg == 2) goto L275;
                    kdflg = 2;
                    goto L270;
            L260:
                    if (rs1 > 0.0) goto L300;
                    s2r = zeror;
                    s2i = zeroi;
                    goto L230;
            L270:
                    ;
                }
                k = n;
            L275:
                il = n - k;
                if (il == 0) return 0;
                //-----------------------------------------------------------------------
                //    RECUR BACKWARD FOR REMAINDER OF I SEQUENCE AND ADD IN THE
                //    K FUNCTIONS, SCALING THE I SEQUENCE DURING RECURRENCE TO KEEP
                //    INTERMEDIATE ARITHMETIC ON SCALE NEAR EXPONENT EXTREMES.
                //-----------------------------------------------------------------------
                s1r = cyr[0];
                s1i = cyi[0];
                s2r = cyr[1];
                s2i = cyi[1];
                csr = csrr[iflag - 1];
                ascle = bry[iflag - 1];
                fn = (double)(inu + il);
                for (i = 1; i <= il; i++)
                {
                    c2r = s2r;
                    c2i = s2i;
                    s2r = s1r + (fn + fnf) * (rzr * c2r - rzi * c2i);
                    s2i = s1i + (fn + fnf) * (rzr * c2i + rzi * c2r);
                    s1r = c2r;
                    s1i = c2i;
                    fn = fn - 1.0;
                    c2r = s2r * csr;
                    c2i = s2i * csr;
                    ckr = c2r;
                    cki = c2i;
                    c1r = yr[kk - 1];
                    c1i = yi[kk - 1];
                    if (kode == 1)
                    {
                        goto L280;
                    }
                    zs1s2(zrr, zri, ref c1r, ref c1i, ref c2r, ref c2i, ref nw, asc, alim, ref iuf);
                    nz += nw;
            L280:
                    yr[kk - 1] = c1r * cspnr - c1i * cspni + c2r;
                    yi[kk - 1] = c1r * cspni + c1i * cspnr + c2i;
                    kk--;
                    cspnr = -cspnr;
                    cspni = -cspni;
                    if (iflag >= 3)
                    {
                        goto L290;
                    }
                    c2r = Math.Abs(ckr);
                    c2i = Math.Abs(cki);
                    c2m = Math.Max(c2r, c2i);
                    if (c2m <= ascle) goto L290;
                    iflag++;
                    ascle = bry[iflag - 1];
                    s1r *= csr;
                    s1i *= csr;
                    s2r = ckr;
                    s2i = cki;
                    s1r *= cssr[iflag - 1];
                    s1i *= cssr[iflag - 1];
                    s2r *= cssr[iflag - 1];
                    s2i *= cssr[iflag - 1];
                    csr = csrr[iflag - 1];
            L290:
                    ;
                }
                return 0;
            L300:
                nz = -1;
                return 0;
            }

            static int zunk2(double zr, double zi, double fnu, int kode, int mr, int n, double[] yr, double[] yi, ref int nz, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZUNK2
                //***REFER TO  ZBESK
                //
                //     ZUNK2 COMPUTES K(FNU,Z) AND ITS ANALYTIC CONTINUATION FROM THE
                //     RIGHT HALF PLANE TO THE LEFT HALF PLANE BY MEANS OF THE
                //     UNIFORM ASYMPTOTIC EXPANSIONS FOR H(KIND,FNU,ZN) AND J(FNU,ZN)
                //     WHERE ZN IS IN THE RIGHT HALF PLANE, KIND=(3-MR)/2, MR=+1 OR
                //     -1. HERE ZN=ZR*I OR -ZR*I WHERE ZR=Z IF Z IS IN THE RIGHT
                //     HALF PLANE OR ZR=-Z IF Z IS IN THE LEFT HALF PLANE. MR INDIC-
                //     ATES THE DIRECTION OF ROTATION FOR ANALYTIC CONTINUATION.
                //     NZ=-1 MEANS AN OVERFLOW WILL OCCUR
                //
                //***ROUTINES CALLED  ZAIRY,ZKSCL,ZS1S2,ZUCHK,ZUNHJ,D1MACH,ZABS
                //***END PROLOGUE  ZUNK2

                #endregion

                const double zeror = 0.0;
                const double aic = 1.26551212348464539;
                const double zeroi = 0.0;
                const double coner = 1.0;
                const double cr1r = 1.0;
                const double cr1i = 1.73205080756887729;
                const double cr2r = -0.5;
                const double cr2i = -0.866025403784438647;
                const double hpi = 1.570796326794896619231321696;
                const double pi = 3.14159265358979323846264338327950;

                double aarg, aii = 0, air = 0, ang, aphi, argdi = 0;
                double argdr = 0, asc, ascle, asumdi = 0, asumdr = 0;
                double bsumdi = 0, bsumdr = 0, car, cki, ckr;
                double crsc, cscl, csgni, csi;
                double cspni, cspnr, csr, c1i, c1r, c2i, c2m;
                double c2r, daii = 0, dair = 0, fmr, fn = 0, fnf, phidi = 0, phidr = 0;
                double pti, ptr, rast, razr, rs1, rzi, rzr, sar, sgn;
                double sti, str, s1i, s1r, s2i, s2r, yy, zbi, zbr;
                double zet1di = 0, zet1dr = 0, zet2di = 0;
                double zet2dr = 0, zni, znr, zri, zrr;
                int i, ib, iflag = 0, ifn, il, inn, inu, iuf, k, kdflg, kflag = 0, kk;
                int nai = 0, ndai = 0, nw = 0, idum = 0, j, ipard, ic;

                double[] argi = new double[2];
                double[] argr = new double[2];
                double[] asumi = new double[2];
                double[] asumr = new double[2];
                double[] bsumi = new double[2];
                double[] bsumr = new double[2];
                double[] bry = new double[3];
                double[] cipi = { 0.0, -1.0, 0.0, 1.0 };
                double[] cipr = { 1.0, 0.0, -1.0, 0.0 };
                double[] csrr = new double[3];
                double[] cssr = new double[3];
                double[] cyi = new double[2];
                double[] cyr = new double[2];
                double[] phii = new double[2];
                double[] phir = new double[2];
                double[] zeta1i = new double[2];
                double[] zeta2i = new double[2];
                double[] zeta1r = new double[2];
                double[] zeta2r = new double[2];

                kdflg = 1;
                nz = 0;
                //-----------------------------------------------------------------------
                //    EXP(-ALIM)=EXP(-ELIM)/TOL=APPROX. ONE PRECISION GREATER THAN
                //    THE UNDERFLOW LIMIT
                //-----------------------------------------------------------------------
                cscl = 1.0 / tol;
                crsc = tol;
                cssr[0] = cscl;
                cssr[1] = coner;
                cssr[2] = crsc;
                csrr[0] = crsc;
                csrr[1] = coner;
                csrr[2] = cscl;
                bry[0] = d1mach(1) * 1.0E3 / tol;
                bry[1] = 1.0 / bry[0];
                bry[2] = d1mach(2);
                zrr = zr;
                zri = zi;
                if (zr >= 0.0) goto L10;
                zrr = -zr;
                zri = -zi;
            L10:
                yy = zri;
                znr = zri;
                zni = -zrr;
                zbr = zrr;
                zbi = zri;
                inu = (int)fnu;
                fnf = fnu - (double)inu;
                ang = -hpi * fnf;
                car = Math.Cos(ang);
                sar = Math.Sin(ang);
                c2r = hpi * sar;
                c2i = -hpi * car;
                kk = (inu % 4) + 1;
                str = c2r * cipr[kk - 1] - c2i * cipi[kk - 1];
                sti = c2r * cipi[kk - 1] + c2i * cipr[kk - 1];
                csr = cr1r * str - cr1i * sti;
                csi = cr1r * sti + cr1i * str;
                if (yy > 0.0) goto L20;
                znr = -znr;
                zbi = -zbi;
            L20:
                //-----------------------------------------------------------------------
                //    K(FNU,Z) IS COMPUTED FROM H(2,FNU,-I*Z) WHERE Z IS IN THE FIRST
                //    QUADRANT. FOURTH QUADRANT VALUES (YY.LE.0.0E0) ARE COMPUTED BY
                //    CONJUGATION SINCE THE K FUNCTION IS REAL ON THE POSITIVE REAL AXIS
                //-----------------------------------------------------------------------
                j = 2;
                for (i = 1; i <= n; i++)
                {
                    //-----------------------------------------------------------------------
                    //    J FLIP FLOPS BETWEEN 1 AND 2 IN J = 3 - J
                    //-----------------------------------------------------------------------
                    j = 3 - j;
                    fn = fnu + (double)(i - 1);
                    zunhj(znr, zni, fn, 0, tol, ref phir[j - 1], ref phii[j - 1], ref argr[j - 1], ref argi[j - 1], ref zeta1r[j - 1], ref zeta1i[j - 1], ref zeta2r[j - 1], ref zeta2i[j - 1], ref asumr[j - 1], ref asumi[j - 1], ref bsumr[j - 1], ref bsumi[j - 1]);
                    if (kode == 1) goto L30;
                    str = zbr + zeta2r[j - 1];
                    sti = zbi + zeta2i[j - 1];
                    rast = fn / zabs(str, sti);
                    str = str * rast * rast;
                    sti = -sti * rast * rast;
                    s1r = zeta1r[j - 1] - str;
                    s1i = zeta1i[j - 1] - sti;
                    goto L40;
            L30:
                    s1r = zeta1r[j - 1] - zeta2r[j - 1];
                    s1i = zeta1i[j - 1] - zeta2i[j - 1];
            L40:
                    //-----------------------------------------------------------------------
                    //    TEST FOR UNDERFLOW AND OVERFLOW
                    //-----------------------------------------------------------------------
                    rs1 = s1r;
                    if (Math.Abs(rs1) > elim) goto L70;
                    if (kdflg == 1) kflag = 2;
                    if (Math.Abs(rs1) < alim) goto L50;
                    //-----------------------------------------------------------------------
                    //    REFINE  TEST AND SCALE
                    //-----------------------------------------------------------------------
                    aphi = zabs(phir[j - 1], phii[j - 1]);
                    aarg = zabs(argr[j - 1], argi[j - 1]);
                    rs1 = rs1 + Math.Log(aphi) - Math.Log(aarg) * 0.25 - aic;
                    if (Math.Abs(rs1) > elim) goto L70;
                    if (kdflg == 1) kflag = 1;
                    if (rs1 < 0.0) goto L50;
                    if (kdflg == 1) kflag = 3;
            L50:
                    //-----------------------------------------------------------------------
                    //    SCALE S1 TO KEEP INTERMEDIATE ARITHMETIC ON SCALE NEAR
                    //    EXPONENT EXTREMES
                    //-----------------------------------------------------------------------
                    c2r = argr[j - 1] * cr2r - argi[j - 1] * cr2i;
                    c2i = argr[j - 1] * cr2i + argi[j - 1] * cr2r;
                    zairy(c2r, c2i, 0, 2, ref air, ref aii, ref nai, ref idum);
                    zairy(c2r, c2i, 1, 2, ref dair, ref daii, ref ndai, ref idum);
                    str = dair * bsumr[j - 1] - daii * bsumi[j - 1];
                    sti = dair * bsumi[j - 1] + daii * bsumr[j - 1];
                    ptr = str * cr2r - sti * cr2i;
                    pti = str * cr2i + sti * cr2r;
                    str = ptr + (air * asumr[j - 1] - aii * asumi[j - 1]);
                    sti = pti + (air * asumi[j - 1] + aii * asumr[j - 1]);
                    ptr = str * phir[j - 1] - sti * phii[j - 1];
                    pti = str * phii[j - 1] + sti * phir[j - 1];
                    s2r = ptr * csr - pti * csi;
                    s2i = ptr * csi + pti * csr;
                    str = Math.Exp(s1r) * cssr[kflag - 1];
                    s1r = str * Math.Cos(s1i);
                    s1i = str * Math.Sin(s1i);
                    str = s2r * s1r - s2i * s1i;
                    s2i = s1r * s2i + s2r * s1i;
                    s2r = str;
                    if (kflag != 1) goto L60;
                    zuchk(s2r, s2i, ref nw, bry[0], tol);
                    if (nw != 0) goto L70;
            L60:
                    if (yy <= 0.0) s2i = -s2i;
                    cyr[kdflg - 1] = s2r;
                    cyi[kdflg - 1] = s2i;
                    yr[i - 1] = s2r * csrr[kflag - 1];
                    yi[i - 1] = s2i * csrr[kflag - 1];
                    str = csi;
                    csi = -csr;
                    csr = str;
                    if (kdflg == 2) goto L85;
                    kdflg = 2;
                    goto L80;
            L70:
                    if (rs1 > 0.0) goto L320;
                    //-----------------------------------------------------------------------
                    //    FOR ZR.LT.0.0, THE I FUNCTION TO BE ADDED WILL OVERFLOW
                    //-----------------------------------------------------------------------
                    if (zr < 0.0) goto L320;
                    kdflg = 1;
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                    nz++;
                    str = csi;
                    csi = -csr;
                    csr = str;
                    if (i == 1) goto L80;
                    if (yr[i - 1] == zeror && yi[i - 1] == zeroi) goto L80;
                    yr[i - 2] = zeror;
                    yi[i - 2] = zeroi;
                    nz++;
            L80:
                    ;
                }
                i = n;
            L85:
                razr = 1.0 / zabs(zrr, zri);
                str = zrr * razr;
                sti = -zri * razr;
                rzr = (str + str) * razr;
                rzi = (sti + sti) * razr;
                ckr = fn * rzr;
                cki = fn * rzi;
                ib = i + 1;
                if (n < ib) goto L180;
                //-----------------------------------------------------------------------
                //    TEST LAST MEMBER FOR UNDERFLOW AND OVERFLOW. SET SEQUENCE TO ZERO
                //    ON UNDERFLOW.
                //-----------------------------------------------------------------------
                fn = fnu + (double)(n - 1);
                ipard = 1;
                if (mr != 0) ipard = 0;
                zunhj(znr, zni, fn, ipard, tol, ref phidr, ref phidi, ref argdr, ref argdi, ref zet1dr, ref zet1di, ref zet2dr, ref zet2di, ref asumdr, ref asumdi, ref bsumdr, ref bsumdi);
                if (kode == 1) goto L90;
                str = zbr + zet2dr;
                sti = zbi + zet2di;
                rast = fn / zabs(str, sti);
                str = str * rast * rast;
                sti = -sti * rast * rast;
                s1r = zet1dr - str;
                s1i = zet1di - sti;
                goto L100;
            L90:
                s1r = zet1dr - zet2dr;
                s1i = zet1di - zet2di;
            L100:
                rs1 = s1r;
                if (Math.Abs(rs1) > elim) goto L105;
                if (Math.Abs(rs1) < alim) goto L120;
                //-----------------------------------------------------------------------
                //    REFINE ESTIMATE AND TEST
                //-----------------------------------------------------------------------
                aphi = zabs(phidr, phidi);
                rs1 += Math.Log(aphi);
                if (Math.Abs(rs1) < elim) goto L120;
            L105:
                if (rs1 > 0.0) goto L320;
                //-----------------------------------------------------------------------
                //    FOR ZR.LT.0.0, THE I FUNCTION TO BE ADDED WILL OVERFLOW
                //-----------------------------------------------------------------------
                if (zr < 0.0) goto L320;
                nz = n;
                for (i = 1; i <= n; i++)
                {
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                }
                return 0;
            L120:
                s1r = cyr[0];
                s1i = cyi[0];
                s2r = cyr[1];
                s2i = cyi[1];
                c1r = csrr[kflag - 1];
                ascle = bry[kflag - 1];
                for (i = ib; i <= n; i++)
                {
                    c2r = s2r;
                    c2i = s2i;
                    s2r = ckr * c2r - cki * c2i + s1r;
                    s2i = ckr * c2i + cki * c2r + s1i;
                    s1r = c2r;
                    s1i = c2i;
                    ckr += rzr;
                    cki += rzi;
                    c2r = s2r * c1r;
                    c2i = s2i * c1r;
                    yr[i - 1] = c2r;
                    yi[i - 1] = c2i;
                    if (kflag >= 3) goto L130;
                    str = Math.Abs(c2r);
                    sti = Math.Abs(c2i);
                    c2m = Math.Max(str, sti);
                    if (c2m <= ascle) goto L130;
                    kflag++;
                    ascle = bry[kflag - 1];
                    s1r *= c1r;
                    s1i *= c1r;
                    s2r = c2r;
                    s2i = c2i;
                    s1r *= cssr[kflag - 1];
                    s1i *= cssr[kflag - 1];
                    s2r *= cssr[kflag - 1];
                    s2i *= cssr[kflag - 1];
                    c1r = csrr[kflag - 1];
            L130:
                    ;
                }
            L180:
                if (mr == 0) return 0;
                //-----------------------------------------------------------------------
                //    ANALYTIC CONTINUATION FOR RE(Z).LT.0.0D0
                //-----------------------------------------------------------------------
                nz = 0;
                fmr = (double)mr;
                sgn = -dsign(pi, fmr);
                //-----------------------------------------------------------------------
                //    CSPN AND CSGN ARE COEFF OF K AND I FUNCTIONS RESP.
                //-----------------------------------------------------------------------
                csgni = sgn;
                if (yy <= 0.0) csgni = -csgni;
                ifn = inu + n - 1;
                ang = fnf * sgn;
                cspnr = Math.Cos(ang);
                cspni = Math.Sin(ang);
                if (ifn % 2 == 0) goto L190;
                cspnr = -cspnr;
                cspni = -cspni;
            L190:
                //-----------------------------------------------------------------------
                //    CS=COEFF OF THE J FUNCTION TO GET THE I FUNCTION. I(FNU,Z) IS
                //    COMPUTED FROM EXP(I*FNU*HPI)*J(FNU,-I*Z) WHERE Z IS IN THE FIRST
                //    QUADRANT. FOURTH QUADRANT VALUES (YY.LE.0.0E0) ARE COMPUTED BY
                //    CONJUGATION SINCE THE I FUNCTION IS REAL ON THE POSITIVE REAL AXIS
                //-----------------------------------------------------------------------
                csr = sar * csgni;
                csi = car * csgni;
                inn = (ifn % 4) + 1;
                c2r = cipr[inn - 1];
                c2i = cipi[inn - 1];
                str = csr * c2r + csi * c2i;
                csi = -csr * c2i + csi * c2r;
                csr = str;
                asc = bry[0];
                iuf = 0;
                kk = n;
                kdflg = 1;
                ib--;
                ic = ib - 1;
                for (k = 1; k <= n; k++)
                {
                    fn = fnu + (kk - 1);
                    //-----------------------------------------------------------------------
                    //    LOGIC TO SORT ref CASES WHOSE PARAMETERS WERE SET FOR THE K
                    //    FUNCTION ABOVE
                    //-----------------------------------------------------------------------
                    if (n > 2) goto L175;
            L172:
                    phidr = phir[j - 1];
                    phidi = phii[j - 1];
                    argdr = argr[j - 1];
                    argdi = argi[j - 1];
                    zet1dr = zeta1r[j - 1];
                    zet1di = zeta1i[j - 1];
                    zet2dr = zeta2r[j - 1];
                    zet2di = zeta2i[j - 1];
                    asumdr = asumr[j - 1];
                    asumdi = asumi[j - 1];
                    bsumdr = bsumr[j - 1];
                    bsumdi = bsumi[j - 1];
                    j = 3 - j;
                    goto L210;
            L175:
                    if (kk == n && ib < n) goto L210;
                    if (kk == ib || kk == ic) goto L172;
                    zunhj(znr, zni, fn, 0, tol, ref phidr, ref phidi, ref argdr, ref argdi, ref zet1dr, ref zet1di, ref zet2dr, ref zet2di, ref asumdr, ref asumdi, ref bsumdr, ref bsumdi);
            L210:
                    if (kode == 1) goto L220;
                    str = zbr + zet2dr;
                    sti = zbi + zet2di;
                    rast = fn / zabs(str, sti);
                    str = str * rast * rast;
                    sti = -sti * rast * rast;
                    s1r = -zet1dr + str;
                    s1i = -zet1di + sti;
                    goto L230;
            L220:
                    s1r = -zet1dr + zet2dr;
                    s1i = -zet1di + zet2di;
            L230:
                    //-----------------------------------------------------------------------
                    //    TEST FOR UNDERFLOW AND OVERFLOW
                    //-----------------------------------------------------------------------
                    rs1 = s1r;
                    if (Math.Abs(rs1) > elim) goto L280;
                    if (kdflg == 1) iflag = 2;
                    if (Math.Abs(rs1) < alim) goto L240;
                    //-----------------------------------------------------------------------
                    //    REFINE  TEST AND SCALE
                    //-----------------------------------------------------------------------
                    aphi = zabs(phidr, phidi);
                    aarg = zabs(argdr, argdi);
                    rs1 = rs1 + Math.Log(aphi) - 0.25 * Math.Log(aarg) - aic;
                    if (Math.Abs(rs1) > elim) goto L280;
                    if (kdflg == 1) iflag = 1;
                    if (rs1 < 0.0) goto L240;
                    if (kdflg == 1) iflag = 3;
            L240:
                    zairy(argdr, argdi, 0, 2, ref air, ref aii, ref nai, ref idum);
                    zairy(argdr, argdi, 1, 2, ref dair, ref daii, ref ndai, ref idum);
                    str = dair * bsumdr - daii * bsumdi;
                    sti = dair * bsumdi + daii * bsumdr;
                    str += air * asumdr - aii * asumdi;
                    sti += air * asumdi + aii * asumdr;
                    ptr = str * phidr - sti * phidi;
                    pti = str * phidi + sti * phidr;
                    s2r = ptr * csr - pti * csi;
                    s2i = ptr * csi + pti * csr;
                    str = Math.Exp(s1r) * cssr[iflag - 1];
                    s1r = str * Math.Cos(s1i);
                    s1i = str * Math.Sin(s1i);
                    str = s2r * s1r - s2i * s1i;
                    s2i = s2r * s1i + s2i * s1r;
                    s2r = str;
                    if (iflag != 1) goto L250;
                    zuchk(s2r, s2i, ref nw, bry[0], tol);
                    if (nw == 0) goto L250;
                    s2r = zeror;
                    s2i = zeroi;
            L250:
                    if (yy <= 0.0) s2i = -s2i;
                    cyr[kdflg - 1] = s2r;
                    cyi[kdflg - 1] = s2i;
                    c2r = s2r;
                    c2i = s2i;
                    s2r *= csrr[iflag - 1];
                    s2i *= csrr[iflag - 1];
                    //-----------------------------------------------------------------------
                    //    ADD I AND K FUNCTIONS, K SEQUENCE IN Y(I), I=1,N
                    //-----------------------------------------------------------------------
                    s1r = yr[kk - 1];
                    s1i = yi[kk - 1];
                    if (kode == 1) goto L270;
                    zs1s2(zrr, zri, ref s1r, ref s1i, ref s2r, ref s2i, ref nw, asc, alim, ref iuf);
                    nz += nw;
            L270:
                    yr[kk - 1] = s1r * cspnr - s1i * cspni + s2r;
                    yi[kk - 1] = s1r * cspni + s1i * cspnr + s2i;
                    kk--;
                    cspnr = -cspnr;
                    cspni = -cspni;
                    str = csi;
                    csi = -csr;
                    csr = str;
                    if (c2r != 0.0 || c2i != 0.0) goto L255;
                    kdflg = 1;
                    goto L290;
            L255:
                    if (kdflg == 2) goto L295;
                    kdflg = 2;
                    goto L290;
            L280:
                    if (rs1 > 0.0) goto L320;
                    s2r = zeror;
                    s2i = zeroi;
                    goto L250;
            L290:
                    ;
                }
                k = n;
            L295:
                il = n - k;
                if (il == 0) return 0;
                //-----------------------------------------------------------------------
                //    RECUR BACKWARD FOR REMAINDER OF I SEQUENCE AND ADD IN THE
                //    K FUNCTIONS, SCALING THE I SEQUENCE DURING RECURRENCE TO KEEP
                //    INTERMEDIATE ARITHMETIC ON SCALE NEAR EXPONENT EXTREMES.
                //-----------------------------------------------------------------------
                s1r = cyr[0];
                s1i = cyi[0];
                s2r = cyr[1];
                s2i = cyi[1];
                csr = csrr[iflag - 1];
                ascle = bry[iflag - 1];
                fn = (double)(inu + il);
                for (i = 1; i <= il; i++)
                {
                    c2r = s2r;
                    c2i = s2i;
                    s2r = s1r + (fn + fnf) * (rzr * c2r - rzi * c2i);
                    s2i = s1i + (fn + fnf) * (rzr * c2i + rzi * c2r);
                    s1r = c2r;
                    s1i = c2i;
                    fn = fn - 1.0;
                    c2r = s2r * csr;
                    c2i = s2i * csr;
                    ckr = c2r;
                    cki = c2i;
                    c1r = yr[kk - 1];
                    c1i = yi[kk - 1];
                    if (kode == 1) goto L300;
                    zs1s2(zrr, zri, ref c1r, ref c1i, ref c2r, ref c2i, ref nw, asc, alim, ref iuf);
                    nz = nz + nw;
            L300:
                    yr[kk - 1] = c1r * cspnr - c1i * cspni + c2r;
                    yi[kk - 1] = c1r * cspni + c1i * cspnr + c2i;
                    kk--;
                    cspnr = -cspnr;
                    cspni = -cspni;
                    if (iflag >= 3) goto L310;
                    c2r = Math.Abs(ckr);
                    c2i = Math.Abs(cki);
                    c2m = Math.Max(c2r, c2i);
                    if (c2m <= ascle) goto L310;
                    iflag++;
                    ascle = bry[iflag - 1];
                    s1r *= csr;
                    s1i *= csr;
                    s2r = ckr;
                    s2i = cki;
                    s1r *= cssr[iflag - 1];
                    s1i *= cssr[iflag - 1];
                    s2r *= cssr[iflag - 1];
                    s2i *= cssr[iflag - 1];
                    csr = csrr[iflag - 1];
            L310:
                    ;
                }
                return 0;
            L320:
                nz = -1;
                return 0;
            }

            static int zuoik(double zr, double zi, double fnu, int kode, int ikflg, int n, double[] yr, double[] yi, ref int nuf, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZUOIK
                //***REFER TO  ZBESI,ZBESK,ZBESH
                //
                //     ZUOIK COMPUTES THE LEADING TERMS OF THE UNIFORM ASYMPTOTIC
                //     EXPANSIONS FOR THE I AND K FUNCTIONS AND COMPARES THEM
                //     (IN LOGARITHMIC FORM) TO ALIM AND ELIM FOR OVER AND UNDERFLOW
                //     WHERE ALIM.LT.ELIM. IF THE MAGNITUDE, BASED ON THE LEADING
                //     EXPONENTIAL, IS LESS THAN ALIM OR GREATER THAN -ALIM, THEN
                //     THE RESULT IS ON SCALE. IF NOT, THEN A REFINED TEST USING OTHER
                //     MULTIPLIERS (IN LOGARITHMIC FORM) IS MADE BASED ON ELIM. HERE
                //     EXP(-ELIM)=SMALLEST MACHINE NUMBER*1.0E+3 AND EXP(-ALIM)=
                //     EXP(-ELIM)/TOL
                //
                //     IKFLG=1 MEANS THE I SEQUENCE IS TESTED
                //          =2 MEANS THE K SEQUENCE IS TESTED
                //     NUF = 0 MEANS THE LAST MEMBER OF THE SEQUENCE IS ON SCALE
                //         =-1 MEANS AN OVERFLOW WOULD OCCUR
                //     IKFLG=1 AND NUF.GT.0 MEANS THE LAST NUF Y VALUES WERE SET TO ZERO
                //             THE FIRST N-NUF VALUES MUST BE SET BY ANOTHER ROUTINE
                //     IKFLG=2 AND NUF.EQ.N MEANS ALL Y VALUES WERE SET TO ZERO
                //     IKFLG=2 AND 0.LT.NUF.LT.N NOT CONSIDERED. Y MUST BE SET BY
                //             ANOTHER ROUTINE
                //
                //***ROUTINES CALLED  ZUCHK,ZUNHJ,ZUNIK,D1MACH,ZABS,ZLOG
                //***END PROLOGUE  ZUOIK

                #endregion

                const double zeror = 0.0;
                const double zeroi = 0.0;
                const double aic = 1.265512123484645396;

                double aarg = 0, aphi, argi = 0, argr = 0, asumi = 0, asumr = 0;
                double ascle, ax, ay, bsumi = 0, bsumr = 0, czi, czr, fnn;
                double gnn, gnu, phii = 0, phir = 0, rcz, str = 0, sti = 0, sumi = 0, sumr = 0;
                double zbi, zbr, zeta1i = 0, zeta1r = 0, zeta2i = 0, zeta2r = 0;
                double zni = 0, znr = 0, zri, zrr;
                int i, idum = 0, iform, init, nn, nw = 0;

                double[] cwrkr = new double[16];
                double[] cwrki = new double[16];

                nuf = 0;
                nn = n;
                zrr = zr;
                zri = zi;
                if (zr >= 0.0) goto L10;
                zrr = -zr;
                zri = -zi;
            L10:
                zbr = zrr;
                zbi = zri;
                ax = Math.Abs(zr) * 1.7321;
                ay = Math.Abs(zi);
                iform = 1;
                if (ay > ax) iform = 2;
                gnu = Math.Max(fnu, 1.0);
                if (ikflg == 1) goto L20;
                fnn = (double)nn;
                gnn = fnu + fnn - 1.0;
                gnu = Math.Max(gnn, fnn);
            L20:
                //-----------------------------------------------------------------------
                //    ONLY THE MAGNITUDE OF ARG AND PHI ARE NEEDED ALONG WITH THE
                //    REAL PARTS OF ZETA1, ZETA2 AND ZB. NO ATTEMPT IS MADE TO GET
                //    THE SIGN OF THE IMAGINARY PART CORRECT.
                //-----------------------------------------------------------------------
                if (iform == 2) goto L30;
                init = 0;
                zunik(zrr, zri, gnu, ikflg, 1, tol, ref init, ref phir, ref phii, ref zeta1r, ref zeta1i, ref zeta2r, ref zeta2i, ref sumr, ref sumi, ref cwrkr, ref cwrki);
                czr = -zeta1r + zeta2r;
                czi = -zeta1i + zeta2i;
                goto L50;
            L30:
                znr = zri;
                zni = -zrr;
                if (zi > 0.0) goto L40;
                znr = -znr;
            L40:
                zunhj(znr, zni, gnu, 1, tol, ref phir, ref phii, ref argr, ref argi, ref zeta1r, ref zeta1i, ref zeta2r, ref zeta2i, ref asumr, ref asumi, ref bsumr, ref bsumi);
                czr = -zeta1r + zeta2r;
                czi = -zeta1i + zeta2i;
                aarg = zabs(argr, argi);
            L50:
                if (kode == 1) goto L60;
                czr -= zbr;
                czi -= zbi;
            L60:
                if (ikflg == 1) goto L70;
                czr = -czr;
                czi = -czi;
            L70:
                aphi = zabs(phir, phii);
                rcz = czr;
                //-----------------------------------------------------------------------
                //    OVERFLOW TEST
                //-----------------------------------------------------------------------
                if (rcz > elim) goto L210;
                if (rcz < alim) goto L80;
                rcz += Math.Log(aphi);
                if (iform == 2) rcz = rcz - Math.Log(aarg) * 0.25 - aic;
                if (rcz > elim) goto L210;
                goto L130;
            L80:
                //-----------------------------------------------------------------------
                //    UNDERFLOW TEST
                //-----------------------------------------------------------------------
                if (rcz < -elim) goto L90;
                if (rcz > -alim) goto L130;
                rcz += Math.Log(aphi);
                if (iform == 2) rcz = rcz - Math.Log(aarg) * .25 - aic;
                if (rcz > -elim) goto L110;
            L90:
                for (i = 1; i <= nn; i++)
                {
                    yr[i - 1] = zeror;
                    yi[i - 1] = zeroi;
                }
                nuf = nn;
                return 0;
            L110:
                ascle = d1mach(1) * 1.0E3 / tol;
                zlog(phir, phii, ref str, ref sti, ref idum);
                czr += str;
                czi += sti;
                if (iform == 1) goto L120;
                zlog(argr, argi, ref str, ref sti, ref idum);
                czr = czr - str * 0.25 - aic;
                czi -= sti * 0.25;
            L120:
                ax = Math.Exp(rcz) / tol;
                ay = czi;
                czr = ax * Math.Cos(ay);
                czi = ax * Math.Sin(ay);
                zuchk(czr, czi, ref nw, ascle, tol);
                if (nw != 0) goto L90;
            L130:
                if (ikflg == 2) return 0;
                if (n == 1) return 0;
                //-----------------------------------------------------------------------
                //    SET UNDERFLOWS ON I SEQUENCE
                //-----------------------------------------------------------------------
            L140:
                gnu = fnu + (nn - 1);
                if (iform == 2) goto L150;
                init = 0;
                zunik(zrr, zri, gnu, ikflg, 1, tol, ref init, ref phir, ref phii, ref zeta1r, ref zeta1i, ref zeta2r, ref zeta2i, ref sumr, ref sumi, ref cwrkr, ref cwrki);
                czr = -zeta1r + zeta2r;
                czi = -zeta1i + zeta2i;
                goto L160;
            L150:
                zunhj(znr, zni, gnu, 1, tol, ref phir, ref phii, ref argr, ref argi, ref zeta1r, ref zeta1i, ref zeta2r, ref zeta2i, ref asumr, ref asumi, ref bsumr, ref bsumi);
                czr = -zeta1r + zeta2r;
                czi = -zeta1i + zeta2i;
                aarg = zabs(argr, argi);
            L160:
                if (kode == 1) goto L170;
                czr -= zbr;
                czi -= zbi;
            L170:
                aphi = zabs(phir, phii);
                rcz = czr;
                if (rcz < -elim) goto L180;
                if (rcz > -alim) return 0;
                rcz += Math.Log(aphi);
                if (iform == 2) rcz = rcz - Math.Log(aarg) * 0.25 - aic;
                if (rcz > -elim) goto L190;
            L180:
                yr[nn - 1] = zeror;
                yi[nn - 1] = zeroi;
                nn--;
                nuf++;
                if (nn == 0) return 0;
                goto L140;
            L190:
                ascle = d1mach(1) * 1.0E3 / tol;
                zlog(phir, phii, ref str, ref sti, ref idum);
                czr += str;
                czi += sti;
                if (iform == 1) goto L200;
                zlog(argr, argi, ref str, ref sti, ref idum);
                czr = czr - str * 0.25 - aic;
                czi -= sti * 0.25;
            L200:
                ax = Math.Exp(rcz) / tol;
                ay = czi;
                czr = ax * Math.Cos(ay);
                czi = ax * Math.Sin(ay);
                zuchk(czr, czi, ref nw, ascle, tol);
                if (nw != 0) goto L180;
                return 0;
            L210:
                nuf = -1;
                return 0;
            }

            static int zwrsk(double zrr, double zri, double fnu, int kode, int n, double[] yr, double[] yi, ref int nz, double[] cwr, double[] cwi, double tol, double elim, double alim)
            {
                #region Description

                //***BEGIN PROLOGUE  ZWRSK
                //***REFER TO  ZBESI,ZBESK
                //
                //     ZWRSK COMPUTES THE I BESSEL FUNCTION FOR RE(Z).GE.0.0 BY
                //     NORMALIZING THE I FUNCTION RATIOS FROM ZRATI BY THE WRONSKIAN
                //
                //***ROUTINES CALLED  D1MACH,ZBKNU,ZRATI,ZABS
                //***END PROLOGUE  ZWRSK

                #endregion

                double act, acw, ascle, cinui, cinur, csclr, cti;
                double ctr, c1i, c1r, c2i, c2r, pti, ptr, ract;
                double sti, str;
                int i, nw = 0;

                //-----------------------------------------------------------------------
                //     I(FNU+I-1,Z) BY BACKWARD RECURRENCE FOR RATIOS
                //     Y(I)=I(FNU+I,Z)/I(FNU+I-1,Z) FROM CRATI NORMALIZED BY THE
                //     WRONSKIAN WITH K(FNU,Z) AND K(FNU+1,Z) FROM CBKNU.
                //-----------------------------------------------------------------------
                nz = 0;
                zbknu(zrr, zri, fnu, kode, 2, cwr, cwi, ref nw, tol, elim, alim);
                if (nw != 0) goto L50;
                zrati(zrr, zri, fnu, n, yr, yi, tol);
                //-----------------------------------------------------------------------
                //    RECUR FORWARD ON I(FNU+1,Z) = R(FNU,Z)*I(FNU,Z),
                //    R(FNU+J-1,Z)=Y(J),  J=1,...,N
                //-----------------------------------------------------------------------
                cinur = 1.0;
                cinui = 0.0;
                if (kode == 1) goto L10;
                cinur = Math.Cos(zri);
                cinui = Math.Sin(zri);
            L10:
                //-----------------------------------------------------------------------
                //    ON LOW EXPONENT MACHINES THE K FUNCTIONS CAN BE CLOSE TO BOTH
                //    THE UNDER AND OVERFLOW LIMITS AND THE NORMALIZATION MUST BE
                //    SCALED TO PREVENT OVER OR UNDERFLOW. CUOIK HAS DETERMINED THAT
                //    THE RESULT IS ON SCALE.
                //-----------------------------------------------------------------------
                acw = zabs(cwr[1], cwi[1]);
                ascle = d1mach(1) * 1.0E3 / tol;
                csclr = 1.0;
                if (acw > ascle) goto L20;
                csclr = 1.0 / tol;
                goto L30;
            L20:
                ascle = 1.0 / ascle;
                if (acw < ascle) goto L30;
                csclr = tol;
            L30:
                c1r = cwr[0] * csclr;
                c1i = cwi[0] * csclr;
                c2r = cwr[1] * csclr;
                c2i = cwi[1] * csclr;
                str = yr[0];
                sti = yi[0];
                //-----------------------------------------------------------------------
                //    CINU=CINU*(CONJG(CT)/ABS(CT))*(1.0D0/ABS(CT) PREVENTS
                //    UNDER- OR OVERFLOW PREMATURELY BY SQUARING ABS(CT)
                //-----------------------------------------------------------------------
                ptr = str * c1r - sti * c1i;
                pti = str * c1i + sti * c1r;
                ptr += c2r;
                pti += c2i;
                ctr = zrr * ptr - zri * pti;
                cti = zrr * pti + zri * ptr;
                act = zabs(ctr, cti);
                ract = 1.0 / act;
                ctr *= ract;
                cti = -cti * ract;
                ptr = cinur * ract;
                pti = cinui * ract;
                cinur = ptr * ctr - pti * cti;
                cinui = ptr * cti + pti * ctr;
                yr[0] = cinur * csclr;
                yi[0] = cinui * csclr;
                if (n == 1) return 0;
                for (i = 2; i <= n; i++)
                {
                    ptr = str * cinur - sti * cinui;
                    cinui = str * cinui + sti * cinur;
                    cinur = ptr;
                    str = yr[i - 1];
                    sti = yi[i - 1];
                    yr[i - 1] = cinur * csclr;
                    yi[i - 1] = cinui * csclr;
                }
                return 0;
            L50:
                nz = -1;
                if (nw == -2) nz = -2;
                return 0;
            }

            #endregion
        }
    }
}
