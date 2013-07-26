// <copyright file="BroydenTest.cs" company="Math.NET">
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
using MathNet.Numerics.RootFinding;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.RootFindingTests
{
    [TestFixture]
    internal class BroydenTest
    {
        [Test]
        public void MultipleRoots()
        {
            // Roots at -2, 2
            Func<double, double> f1 = x => x * x - 4;
            Assert.AreEqual(0, f1(BroydenFindRoot(f1, 0, 5, 1e-14)));
            Assert.AreEqual(-2, BroydenFindRoot(f1, -5, -1, 1e-14));
            Assert.AreEqual(2, BroydenFindRoot(f1, 1, 4, 1e-14));
            Assert.AreEqual(0, f1(BroydenFindRoot(x => -f1(x), 0, 5, 1e-14)));
            Assert.AreEqual(-2, BroydenFindRoot(x => -f1(x), -5, -1, 1e-14));
            Assert.AreEqual(2, BroydenFindRoot(x => -f1(x), 1, 4, 1e-14));

            // Roots at 3, 4
            Func<double, double> f2 = x => (x - 3) * (x - 4);
            Assert.AreEqual(0, f2(BroydenFindRoot(f2, 3.5, 5, 1e-14)), 1e-14);
            Assert.AreEqual(3, BroydenFindRoot(f2, -5, 3.5, 1e-14), 1e-14); // slightly less accurate then Bisection
            Assert.AreEqual(4, BroydenFindRoot(f2, 3.2, 5, 1e-14));
            Assert.AreEqual(3, BroydenFindRoot(f2, 2.1, 3.9, 0.001), 0.001);
            Assert.AreEqual(3, BroydenFindRoot(f2, 2.1, 3.4, 0.001), 0.001);
        }

        [Test]
        public void LocalMinima()
        {
            // Method cannot get out of local minima.
            Func<double, double> f1 = x => x * x * x - 2 * x + 2;
            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, -5, 5, 1e-14));
            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, -2, 4, 1e-14));
            Assert.AreEqual(0, f1(BroydenFindRoot(f1, -2, -1, 1e-14)), 1e-14);
        }

        [Test]
        public void NoRoot()
        {
            Func<double, double> f1 = x => x * x + 4;
            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, -5, 5, 1e-14));
        }

        [Test]
        public void Oneeq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq1.htm
            Func<double, double> f1 = z => 8 * Math.Pow((4 - z) * z, 2) / (Math.Pow(6 - 3 * z, 2) * (2 - z)) - 0.186;
            double x = BroydenFindRoot(f1, 0.1, 0.9, accuracy: 1e-14, maxIterations: 80);
            Assert.AreEqual(0.277759543089215, x, 1e-9);
            Assert.AreEqual(0, f1(x), 2e-16); // slightly less accurate then Bisection
        }

        [Test]
        public void Oneeq2a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq2a.htm
            Func<double, double> f1 = T =>
            {
                const double x1 = 0.332112;
                const double x2 = 1 - x1;
                const double G12 = 0.07858889;
                const double G21 = 0.30175355;
                double P2 = Math.Pow(10, 6.87776 - 1171.53 / (224.366 + T));
                double P1 = Math.Pow(10, 8.04494 - 1554.3 / (222.65 + T));
                double t1 = x1 + x2 * G12;
                double t2 = x2 + x1 * G21;
                double gamma2 = Math.Exp(-Math.Log(t2) - (x1 * (G12 * t2 - G21 * t1)) / (t1 * t2));
                double gamma1 = Math.Exp(-Math.Log(t1) + (x2 * (G12 * t2 - G21 * t1)) / (t1 * t2));
                double k1 = gamma1 * P1 / 760;
                double k2 = gamma2 * P2 / 760;
                return 1 - k1 * x1 - k2 * x2;
            };

            double x = BroydenFindRoot(f1, 0, 100, 1e-14);
            Assert.AreEqual(58.7000023925600, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq2b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq2b.htm
            Func<double, double> f1 = T =>
            {
                const double x1 = 0.6763397;
                const double x2 = 1 - x1;
                const double A = 0.75807689;
                const double B = 1.1249035;
                double P2 = Math.Pow(10, 6.9024 - 1268.115 / (216.9 + T));
                double P1 = Math.Pow(10, 8.04494 - 1554.3 / (222.65 + T));
                double gamma2 = Math.Pow(10, B * Math.Pow(x1, 2) / Math.Pow(x1 + B * x2 / A, 2));
                double gamma1 = Math.Pow(10, A * Math.Pow(x2, 2) / Math.Pow(A * x1 / B + x2, 2));
                double k1 = gamma1 * P1 / 760;
                double k2 = gamma2 * P2 / 760;
                return 1 - k1 * x1 - k2 * x2;
            };

            double x = BroydenFindRoot(f1, 0, 100, 1e-14);
            Assert.AreEqual(70.9000000710300, x, 1e-9);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq3()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq3.htm
            Func<double, double> f1 = T => Math.Exp(21000 / T) / (T * T) - 1.11e11;
            double x = BroydenFindRoot(f1, 550, 560, 1e-2);
            Assert.AreEqual(551.773822885233, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-2);
        }

        [Test]
        public void Oneeq4()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq4.htm
            Func<double, double> f1 = x =>
            {
                const double A = 0.38969;
                const double B = 0.55954;
                return A * B * (B * Math.Pow(1 - x, 2) - A * x * x) / Math.Pow(x * (A - B) + B, 2) + 0.14845;
            };

            double r = BroydenFindRoot(f1, 0, 1, 1e-14);
            Assert.AreEqual(0.691473747542323, r, 1e-5);
            Assert.AreEqual(0, f1(r), 1e-14);
        }

        [Test]
        public void Oneeq5()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq5.htm
            Func<double, double> f1 = TR =>
            {
                const double ALPHA = 7.256;
                const double BETA = 2.298E-3;
                const double GAMMA = 0.283E-6;
                const double DH = -57798.0;
                return DH + TR * (ALPHA + TR * (BETA / 2 + TR * GAMMA / 3)) - 298.0 * (ALPHA + 298.0 * (BETA / 2 + 298.0 * GAMMA / 3));
            };

            double x = BroydenFindRoot(f1, 3000, 5000);
            Assert.AreEqual(4305.30991366774, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-10);
        }

        [Test]
        public void Oneeq6a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq6a.htm
            // not solvable with this method
            Func<double, double> f1 = V =>
            {
                const double R = 0.08205;
                const double T = 273.0;
                const double B0 = 0.05587;
                const double A0 = 2.2769;
                const double C = 128300.0;
                const double A = 0.01855;
                const double B = -0.01587;
                const double P = 100.0;
                const double Beta = R * T * B0 - A0 - R * C / (T * T);
                const double Gama = -R * T * B0 * B + A0 * A - R * C * B0 / (T * T);
                const double Delta = R * B0 * B * C / (T * T);

                return R * T / V + Beta / (V * V) + Gama / (V * V * V) + Delta / (V * V * V * V) - P;
            };

            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, 0.1, 1));
        }

        [Test]
        public void Oneeq6b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq6b.htm
            Func<double, double> f1 = V =>
            {
                const double R = 0.08205;
                const double T = 273.0;
                const double B0 = 0.05587;
                const double A0 = 2.2769;
                const double C = 128300.0;
                const double A = 0.01855;
                const double B = -0.01587;
                const double P = 100.0;
                const double Beta = R * T * B0 - A0 - R * C / (T * T);
                const double Gama = -R * T * B0 * B + A0 * A - R * C * B0 / (T * T);
                const double Delta = R * B0 * B * C / (T * T);

                return R * T * V * V * V + Beta * V * V + Gama * V + Delta - P * V * V * V * V;
            };

            double x = BroydenFindRoot(f1, 0.1, 1, 1e-14);
            Assert.AreEqual(0.174749600398905, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-13);
        }

        [Test]
        public void Oneeq7()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq7.htm
            // not solvable with this method
            Func<double, double> f1 = x => x / (1 - x) - 5 * Math.Log(0.4 * (1 - x) / (0.4 - 0.5 * x)) + 4.45977;
            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, 0, 0.79, 1e-2));
        }

        [Test]
        public void Oneeq8()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq8.htm
            Func<double, double> f1 = v =>
            {
                const double a = 240;
                const double b = 40;
                const double c = 200;

                return a * v * v + b * Math.Pow(v, 7 / 4) - c;
            };

            double x = BroydenFindRoot(f1, 0.01, 1, 1e-12);
            Assert.AreEqual(0.842524411168525, x, 1e-2);
            Assert.AreEqual(0, f1(x), 1e-13);
        }

        [Test]
        public void Oneeq9()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq9.htm
            Func<double, double> f1 = P =>
            {
                const double At = 0.1;
                const double A = 0.12;
                const double gama = 1.41;
                const double Pd = 100;

                return Math.Pow((gama + 1) / 2, (gama + 1) / (gama - 1)) * (2 / (gama - 1)) * (Math.Pow(P / Pd, 2 / gama) - Math.Pow(P / Pd, (gama + 1) / gama)) - Math.Pow(At / A, 2);
            };

            double x = BroydenFindRoot(f1, 1, 50, 1e-14);
            Assert.AreEqual(25.743977294088, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = BroydenFindRoot(f1, 50, 100, 1e-14);
            Assert.AreEqual(78.905412035983, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq10()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq10.htm
            // method fails if started too far away from the root
            Func<double, double> f1 = x => (1.0 / 63.0) * Math.Log(x) + (64.0 / 63.0) * Math.Log(1 / (1 - x)) + Math.Log(0.95 - x) - Math.Log(0.9);

            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, .01, 0.35));
            double r = BroydenFindRoot(f1, .01, 0.04, 1e-14);
            Assert.AreEqual(0.036210083704, r, 1e-5);
            Assert.AreEqual(0, f1(r), 1e-14);
            r = BroydenFindRoot(f1, 0.35, 0.7, 1e-14);
            Assert.AreEqual(0.5, r, 1e-5);
            Assert.AreEqual(0, f1(r), 1e-14);
        }

        [Test]
        public void Oneeq11a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq11a.htm
            Func<double, double> f1 = x =>
            {
                const double a = 0.5;
                const double b = 0.8;
                const double c = 0.3;
                const double kp = 604500;
                const double P = 0.00243;

                return a - Math.Pow((c + 2 * x) * (a + b + c - 2 * x), 2) / (kp * P * P * Math.Pow(b - 3 * x, 3)) - x;
            };

            double r = BroydenFindRoot(f1, 0, 0.25, 1e-14);
            Assert.AreEqual(0.058654581076661, r, 1e-5);
            Assert.AreEqual(0, f1(r), 1e-14);
            r = BroydenFindRoot(f1, 0.3, 1, 1e-14);
            Assert.AreEqual(0.600323116977323, r, 1e-5);
            Assert.AreEqual(0, f1(r), 1e-14);
        }

        [Test]
        public void Oneeq11b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq11b.htm
            Func<double, double> f1 = x =>
            {
                const double a = 0.5;
                const double b = 0.8;
                const double c = 0.3;
                const double kp = 604500;
                const double P = 0.00243;

                return (b - Math.Pow(Math.Pow((c + 2 * x) * (a + b + c - 2 * x), 2) / (kp * P * P * (a - x)), 1.0 / 3.0)) / 3.0 - x;
            };

            double r = BroydenFindRoot(f1, 0.01, 0.45, 1e-14);
            Assert.AreEqual(0.058654571042804, r, 1e-5);
            Assert.AreEqual(0, f1(r), 1e-14);
            // Could not test the following root, since Math.Pow(y,1/3) does not work for y<0  
            // r = BroydenFindRoot(f1, 0.55, 0.7);
            // Assert.AreEqual(0.600323117488527, r, 1e-5);
            // Assert.AreEqual(0, f1(r), 1e-14);
        }

        [Test]
        public void Oneeq12()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq12.htm
            Func<double, double> f1 = v =>
            {
                const double b = -159;
                const double c = 9000;
                const double P = 75;
                const double T = 430.85;
                const double R = 82.05;

                return (R * T / P) * (1 + b / v + c / (v * v)) - v;
            };

            double x = BroydenFindRoot(f1, 100, 300, 1e-14);
            Assert.AreEqual(213.001818272273, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq13()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq13.htm
            Func<double, double> f1 = V =>
            {
                const double Pc = 45.8;
                const double Tc = 191.0;
                const double P = 100;
                const double T = 210.0;
                const double R = 0.08205;
                double A = 0.42748 * (R * R * Math.Pow(Tc, 2.5)) / Pc;
                const double B = 0.08664 * R * Tc / Pc;

                return R * Math.Pow(T, 1.5) * V * (V + B) / (P * Math.Sqrt(T) * V * (V + B) + A) + B - V;
            };

            double x = BroydenFindRoot(f1, .01, .3, 1e-14);
            Assert.AreEqual(0.075699587993613, x, 1e-9);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq14()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq14.htm
            Func<double, double> f1 = Q =>
            {
                const double M = 43800.0;
                const double m = 149000.0;
                const double Cp = 0.605;
                const double U = 55.0;
                const double A = 662.0;
                const double T1 = 390.0;
                const double t1 = 100.0;
                const double cp = 0.49;
                double DT1 = T1 - t1 - Q / (M * Cp);
                double DT2 = T1 - t1 - Q / (m * cp);

                return U * A * (DT2 - DT1) / Math.Log(DT2 / DT1) - Q;
            };

            double x = BroydenFindRoot(f1, 1000000, 7000000);
            Assert.AreEqual(5281024.23857737, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-9);
        }

        [Test]
        public void Oneeq15a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq15a.htm
            Func<double, double> f1 = h => h * (1 + h * (3 - h)) - 2.4 - h;

            double x = BroydenFindRoot(f1, -2, 0, 1e-14);
            Assert.AreEqual(-0.795219754733581, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = BroydenFindRoot(f1, 0, 2, 1e-14);
            Assert.AreEqual(1.13413784566692, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = BroydenFindRoot(f1, 2, 5, 1e-14);
            Assert.AreEqual(2.66108192383197, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq15b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq15b.htm
            Func<double, double> f1 = h => 2.4 / (h * (3 - h)) - h;

            double x = BroydenFindRoot(f1, -2, -0.5, 1e-14);
            Assert.AreEqual(-0.795219745444464, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = BroydenFindRoot(f1, 0.5, 2, 1e-14);
            Assert.AreEqual(1.134137843811, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = BroydenFindRoot(f1, 2, 2.9, 1e-14);
            Assert.AreEqual(2.66108190354552, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq16()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq16.htm
            Func<double, double> f1 = T =>
            {
                const double y = 0.7;
                const double x = 1.7;
                const double z = 1 - y - 0.02;
                const double CH4 = 0;
                const double C2H6 = 0;
                const double COtwo = y + 2 * z;
                const double H2O = 2 * y + 3 * z;
                const double N2 = 0.02 + 3.76 * (2 * y + 7 * z / 2) * x;
                const double O2 = (2 * y + 7 * z / 2) * (x - 1);
                const double alp = 3.381 * CH4 + 2.247 * C2H6 + 6.214 * COtwo + 7.256 * H2O + 6.524 * N2 + 6.148 * O2;
                const double bet = 18.044 * CH4 + 38.201 * C2H6 + 10.396 * COtwo + 2.298 * H2O + 1.25 * N2 + 3.102 * O2;
                const double gam = -4.3 * CH4 - 11.049 * C2H6 - 3.545 * COtwo + 0.283 * H2O - 0.001 * N2 - 0.923 * O2;
                const double H0 = alp * 298 + bet * 0.001 * 298 * 298 / 2 + gam * 1e-6 * 298 * 298 * 298 / 3;
                double Hf = alp * T + bet * 0.001 * T * T / 2 + gam * 1e-6 * T * T * T / 3;
                double xx = 1;

                return 212798 * y * xx + 372820 * z * xx + H0 - Hf;
            };

            double r = BroydenFindRoot(f1, 1000, 3000, 1e-14);
            Assert.AreEqual(1779.48406483697, r, 1e-5);
            Assert.AreEqual(0, f1(r), 1e-9);
        }

        [Test]
        public void Oneeq17()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq17.htm
            // method fails if started too far away from the root
            Func<double, double> f1 = rp => rp - 0.327 * Math.Pow(0.06 - 161 * rp, 0.804) * Math.Exp(-5230 / (1.987 * (373 + 1.84e6 * rp)));

            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, 0, 0.00035));
            double x = BroydenFindRoot(f1, 0.0003, 0.00035, 1e-14);
            Assert.AreEqual(0.000340568862275, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq18a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq18a.htm
            Func<double, double> f1 = T =>
            {
                double Tp = (269.267 * T - 1752) / 266.667;
                double k = 0.0006 * Math.Exp(20.7 - 15000 / Tp);
                double Pp = -(0.1 / (321 * (320.0 / 321.0 - (1 + k))));
                double P = (320 * Pp + 0.1) / 321;

                return 1.296 * (T - Tp) + 10369 * k * Pp;
            };

            double x = BroydenFindRoot(f1, 500, 725, 1e-14);
            Assert.AreEqual(690.4486645013260, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-12);
            x = BroydenFindRoot(f1, 725, 850, 1e-12);
            Assert.AreEqual(758.3286948959860, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-12);
            x = BroydenFindRoot(f1, 850, 1000, 1e-12);
            Assert.AreEqual(912.7964911381540, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-12);
        }

        [Test]
        public void Oneeq18b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq18b.htm
            Func<double, double> f1 = T =>
            {
                double Tp = (269 * T - 1752) / 267;
                double k = 0.0006 * Math.Exp(20.7 - 15000 / Tp);
                double Pp = -(0.1 / (321 * (320.0 / 321.0 - (1 + k))));
                double P = (320 * Pp + 0.1) / 321;

                return 1.3 * (T - Tp) + 1.04e4 * k * Pp;
            };

            double x = BroydenFindRoot(f1, 500, 1500, 1e-12);
            Assert.AreEqual(1208.2863599396200, x, 1e-4);
            Assert.AreEqual(0, f1(x), 1e-12);
        }

        [Test]
        public void Oneeq19a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq19a.htm
            Func<double, double> f1 = z =>
            {
                const double P = 200;
                const double Pc = 33.5;
                const double T = 631 * 2;
                const double Tc = 126.2;
                const double Pr = P / Pc;
                const double Tr = T / Tc;
                double Asqr = 0.4278 * Pr / Math.Pow(Tr, 2.5);
                const double B = 0.0867 * Pr / Tr;
                double Q = B * B + B - Asqr;
                double r = Asqr * B;
                double p = (-3 * Q - 1) / 3;
                double q = (-27 * r - 9 * Q - 2) / 27;
                double R = Math.Pow(p / 3, 3) + Math.Pow(q / 2, 2);
                double V = (R > 0) ? Math.Pow(-q / 2 + Math.Sqrt(R), 1 / 3) : 0;
                double WW = (R > 0) ? (-q / 2 - Math.Sqrt(R)) : (0);
                double psi1 = (R < 0) ? (Math.Acos(Math.Sqrt((q * q / 4) / (-p * p * p / 27)))) : (0);
                double W = (R > 0) ? (Math.Sign(WW) * Math.Pow(Math.Abs(WW), 1 / 3)) : (0);
                double z1 = (R < 0) ? (2 * Math.Sqrt(-p / 3) * Math.Cos((psi1 / 3) + 2 * 3.1416 * 0 / 3) + 1 / 3) : (0);
                double z2 = (R < 0) ? (2 * Math.Sqrt(-p / 3) * Math.Cos((psi1 / 3) + 2 * 3.1416 * 1 / 3) + 1 / 3) : (0);
                double z3 = (R < 0) ? (2 * Math.Sqrt(-p / 3) * Math.Cos((psi1 / 3) + 2 * 3.1416 * 2 / 3) + 1 / 3) : (0);
                double z0 = (R > 0) ? (V + W + 1 / 3) : (0);

                return z * z * z - z * z - Q * z - r;
            };

            double x = BroydenFindRoot(f1, -0.5, -0.02, 1e-14);
            Assert.AreEqual(-0.03241692071380, x, 1e-7);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = BroydenFindRoot(f1, -0.02, 0.5, 1e-14);
            Assert.AreEqual(-0.01234360708030, x, 1e-7);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = BroydenFindRoot(f1, 0.5, 1.2, 1e-14);
            Assert.AreEqual(1.04476050281300, x, 1e-7);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq19b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq19b.htm
            // method fails if started too far away from the root
            Func<double, double> f1 = z =>
            {
                const double P = 200;
                const double Pc = 33.5;
                const double T = 631;
                const double Tc = 126.2;
                const double Pr = P / Pc;
                const double Tr = T / Tc;
                double Asqr = 0.4278 * Pr / Math.Pow(Tr, 2.5);
                const double B = 0.0867 * Pr / Tr;
                double Q = B * B + B - Asqr;
                double r = Asqr * B;
                double p = (-3 * Q - 1) / 3;
                double q = (-27 * r - 9 * Q - 2) / 27;
                double R = Math.Pow(p / 3, 3) + Math.Pow(q / 2, 2);
                double V = (R > 0) ? Math.Pow(-q / 2 + Math.Sqrt(R), 1 / 3) : 0;
                double WW = (R > 0) ? (-q / 2 - Math.Sqrt(R)) : (0);
                double psi1 = (R < 0) ? (Math.Acos(Math.Sqrt((q * q / 4) / (-p * p * p / 27)))) : (0);
                double W = (R > 0) ? (Math.Sign(WW) * Math.Pow(Math.Abs(WW), 1 / 3)) : (0);
                double z1 = (R < 0) ? (2 * Math.Sqrt(-p / 3) * Math.Cos((psi1 / 3) + 2 * 3.1416 * 0 / 3) + 1 / 3) : (0);
                double z2 = (R < 0) ? (2 * Math.Sqrt(-p / 3) * Math.Cos((psi1 / 3) + 2 * 3.1416 * 1 / 3) + 1 / 3) : (0);
                double z3 = (R < 0) ? (2 * Math.Sqrt(-p / 3) * Math.Cos((psi1 / 3) + 2 * 3.1416 * 2 / 3) + 1 / 3) : (0);
                double z0 = (R > 0) ? (V + W + 1 / 3) : (0);

                return z * z * z - z * z - Q * z - r;
            };

            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, -0.5, 1.2));
            double x = BroydenFindRoot(f1, 0.5, 1.2, 1e-14);
            Assert.AreEqual(1.06831213384200, x, 1e-7);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq20a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq20a.htm
            // method fails if started too far away from the root
            Func<double, double> f1 = xa =>
            {
                const double T = 313;
                const double P0 = 10;
                const double FA0 = 20 * P0 / (0.082 * 450);
                double k1 = 1.277 * 1.0e9 * Math.Exp(-90000 / (8.31 * T));
                double k2 = 1.29 * 1.0e11 * Math.Exp(-135000 / (8.31 * T));
                double den = 1 + 7 * P0 * (1 - xa) / (1 + xa);
                double xa1 = 1 + xa;
                double ra = (-k1 * P0 * (1 - xa) / xa1 + k2 * P0 * P0 * xa * xa / (xa1 * xa1)) / den;

                return -ra / FA0;
            };

            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, 0.75, 1.02));
            double x = BroydenFindRoot(f1, 0.98, 1.02, 1e-14);
            Assert.AreEqual(0.999251497006000, x, 1e-3);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq20b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq20b.htm
            Func<double, double> f1 = xa =>
            {
                const double T = 313;
                const double P0 = 10;
                const double FA0 = 20 * P0 / (0.082 * 450);
                double k1 = 1.277 * 1.0e9 * Math.Exp(-90000 / (8.31 * T));
                double k2 = 1.29 * 1.0e11 * Math.Exp(-135000 / (8.31 * T));
                double xa1 = 1 + xa;
                double ra = (-k1 * P0 * (1 - xa) / xa1 + k2 * P0 * P0 * xa * xa / (xa1 * xa1));

                return -ra / FA0;
            };

            double x = BroydenFindRoot(f1, 0.75, 1.02, 1e-14);
            Assert.AreEqual(0.999984253901100, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq21a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq21a.htm
            // method fails if started too far away from the root
            Func<double, double> f1 = h =>
            {
                double sg = Math.Acos(2 * h - 1);
                double si = Math.Pow(1 - Math.Pow(2 * h - 1, 2), 0.5);
                double ag = (sg - (2 * h - 1) * si) / 4;
                double al = 3.1415927 / 4 - ag;
                const double d = 1.44;
                double dg = 4 * ag / (sg + si) * d;
                const double ugs = -0.0295;
                double ug = ugs * 3.1415927 / 4 / ag;
                const double uls = -0.00001;
                double ul = uls * 3.1415927 / 4 / al;
                double sl = 3.1415927 - sg;
                double dl = 4 * al / (sl) * d;
                const double rol = 1;
                const double rog = 0.835;
                const double bt = .6;
                double g = 980 * Math.Sin(bt * 3.1415927 / 180);
                double dpg = (rol * al + rog * ag) * g / 3.1415927 * 4;
                const double vig = 1.0;
                double reg = Math.Abs(ug) * rog * dg / vig;
                double tg = -16 / reg * (.5 * rog * ug * Math.Abs(ug));
                const double vil = .01;
                double rel = Math.Abs(ul) * rol * dl / vil;
                double tl = -16 / rel * (.5 * rol * ul * Math.Abs(ul));
                const double it = 1;
                double ti = -16 / reg * rog * (.5 * (ug - ul) * Math.Abs(ug - ul)) * it;
                double dpf = (tl * sl + tg * sg) * d / (al + ag) / (d * d) * 4;
                double dp = dpg + dpf;
                double tic = (Math.Abs(ul) < Math.Abs(ug)) ? (rog / reg) : (rol / rel);
                double y = (rol - rog) * g * Math.Pow(d / 2, 2) / (8 * vig * ugs);

                return -tg * sg * al + tl * sl * ag - ti * si * (al + ag) + d * (rol - rog) * al * ag * g;
            };

            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, 0, 0.3));
            double x = BroydenFindRoot(f1, 0, 0.01, 1e-14);
            Assert.AreEqual(0.00596133169486, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq21b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq21b.htm
            // method fails if started too far away from the root
            Func<double, double> f1 = h =>
            {
                double sg = Math.Acos(2 * h - 1);
                double si = Math.Pow(1 - Math.Pow(2 * h - 1, 2), 0.5);
                double ag = (sg - (2 * h - 1) * si) / 4;
                double al = 3.1415927 / 4 - ag;
                const double d = 1.44;
                double dg = 4 * ag / (sg + si) * d;
                const double ugs = -0.029;
                double ug = ugs * 3.1415927 / 4 / ag;
                const double uls = -0.00001;
                double ul = uls * 3.1415927 / 4 / al;
                double sl = 3.1415927 - sg;
                double dl = 4 * al / (sl) * d;
                const double rol = 1;
                const double rog = 0.835;
                const double bt = .6;
                double g = 980 * Math.Sin(bt * 3.1415927 / 180);
                double dpg = (rol * al + rog * ag) * g / 3.1415927 * 4;
                const double vig = 1.0;
                double reg = Math.Abs(ug) * rog * dg / vig;
                double tg = -16 / reg * (.5 * rog * ug * Math.Abs(ug));
                const double vil = .01;
                double rel = Math.Abs(ul) * rol * dl / vil;
                double tl = -16 / rel * (.5 * rol * ul * Math.Abs(ul));
                const double it = 1;
                double ti = -16 / reg * rog * (.5 * (ug - ul) * Math.Abs(ug - ul)) * it;
                double dpf = (tl * sl + tg * sg) * d / (al + ag) / (d * d) * 4;
                double dp = dpg + dpf;
                double tic = (Math.Abs(ul) < Math.Abs(ug)) ? (rog / reg) : (rol / rel);
                double y = (rol - rog) * g * Math.Pow(d / 2, 2) / (8 * vig * ugs);

                return -tg * sg * al + tl * sl * ag - ti * si * (al + ag) + d * (rol - rog) * al * ag * g;
            };

            Assert.Throws<NonConvergenceException>(() => BroydenFindRoot(f1, 0, 0.1));
            double x = BroydenFindRoot(f1, 0, 0.01, 1e-14);
            Assert.AreEqual(0.00602268958797, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = BroydenFindRoot(f1, 0.1, 0.24, 1e-14);
            Assert.AreEqual(0.19925189173123, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = BroydenFindRoot(f1, 0.24, 0.3, 1e-14);
            Assert.AreEqual(0.26405262123160, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        private static double BroydenFindRoot(Func<double, double> f, double lowerBound, double upperBound, double accuracy = 1e-8, int maxIterations = 100)
        {
            Func<double[], double[]> fw = x => new double[1] { f(x[0]) };
            double[] initialGuess = new double[1] { (lowerBound + upperBound) * 0.5 };
            return Broyden.FindRoot(fw, initialGuess, accuracy, maxIterations)[0];
        }

        [Test]
        public void Twoeq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq1.htm
            // not solvable with this method
            Func<double[], double[]> fa1 = x =>
            {
                double D = x[0];
                double fF = x[1];
                const double dp = 103000;
                const double L = 100;
                const double T = 25 + 273.15;
                const double Q = 0.0025;
                const double pi = 3.1416;
                const double rho = 46.048 + T * (9.418 + T * (-0.0329 + T * (4.882e-5 - T * 2.895e-8)));
                double vis = Math.Exp(-10.547 + 541.69 / (T - 144.53));
                double v = Q / (pi * D * D / 4);
                double kvis = vis / rho;
                double Re = v * D / kvis;
                double fD = -dp / rho + 2 * fF * v * v * L / D;
                double ffF = (Re < 2100) ? (fF - 16 / Re) : (fF - 1 / Math.Pow(4 * Math.Log(Re * Math.Sqrt(fF)) - 0.4, 2));
                return new double[2] { fD, ffF };
            };

            Assert.Throws<NonConvergenceException>(() => Broyden.FindRoot(fa1, new double[2] { 0.04, 0.001 }, 1e-14));
        }

        [Test]
        public void Twoeq2()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq2.htm
            // with this method this test is only solvable with reduced accuracy
            Func<double[], double[]> fa1 = xa =>
            {
                double x = xa[0];
                double T = xa[1];
                double k = 0.12 * Math.Exp(12581 * (T - 298) / (298 * T));
                double fx = 120 * x - 75 * k * (1 - x);
                double fT = -x * (873 - T) + 11.0 * (T - 300);
                return new double[2] { fx, fT };
            };

            double[] r = Broyden.FindRoot(fa1, new double[2] { 1, 400 }, 1e-6);
            Assert.AreEqual(0.9638680512795, r[0], 1e-5);
            Assert.AreEqual(346.16369814640, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-6);
            Assert.AreEqual(0, fa1(r)[1], 1e-7);
        }

        [Test]
        public void Twoeq3()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq3.htm
            // with this method this test is only solvable with slightly reduced accuracy
            Func<double[], double[]> fa1 = xa =>
            {
                double x = xa[0];
                double T = xa[1];
                double k = Math.Exp(-149750 / T + 92.5);
                double Kp = Math.Exp(42300 / T - 24.2 + 0.17 * Math.Log(T));
                double fx = k * Math.Sqrt(1 - x) * ((0.91 - 0.5 * x) / (9.1 - 0.5 * x) - x * x / ((1 - x) * (1 - x) * Kp));
                double fT = T * (1.84 * x + 77.3) - 43260 * x - 105128;
                return new double[2] { fx, fT };
            };

            double[] r = Broyden.FindRoot(fa1, new double[2] { 0.5, 1700 }, 1e-11);
            Assert.AreEqual(0.5333728995523, r[0], 1e-5);
            Assert.AreEqual(1637.70322946500, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-11);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

        [Test]
        public void Twoeq4a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq4a.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double G21 = xa[0];
                double G12 = xa[1];
                const double t = 58.7;
                const double pw1 = 21;
                const double x1 = (pw1 / 46.07) / (pw1 / 46.07 + (100 - pw1) / 86.18);
                double P2 = Math.Pow(10, 6.87776 - 1171.53 / (224.366 + t));
                double P1 = Math.Pow(10, 8.04494 - 1554.3 / (222.65 + t));
                double gamma2 = 760 / P2;
                double gamma1 = 760 / P1;
                const double x2 = 1 - x1;
                double t1 = x1 + x2 * G12;
                double t2 = x2 + x1 * G21;
                double g2calc = Math.Exp(-Math.Log(t2) - x1 * (G12 * t2 - G21 * t1) / (t1 * t2));
                double g1calc = Math.Exp(-Math.Log(t1) + x2 * (G12 * t2 - G21 * t1) / (t1 * t2));
                double fG21 = Math.Log(gamma2) + Math.Log(t2) + x1 * (G12 * t2 - G21 * t1) / (t1 * t2);
                double fG12 = Math.Log(gamma1) + Math.Log(t1) - x2 * (G12 * t2 - G21 * t1) / (t1 * t2);
                return new double[2] { fG21, fG12 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[2] { 0.5, 0.5 }, 1e-14);
            Assert.AreEqual(0.3017535930592, r[0], 1e-5);
            Assert.AreEqual(0.0785888476379, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

        [Test]
        public void Twoeq4b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq4b.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double G21 = xa[0];
                double G12 = xa[1];
                const double t = 58.7;
                const double pw1 = 21;
                const double x1 = (pw1 / 46.07) / (pw1 / 46.07 + (100 - pw1) / 86.18);
                double P2 = Math.Pow(10, 6.87776 - 1171.53 / (224.366 + t));
                double P1 = Math.Pow(10, 8.04494 - 1554.3 / (222.65 + t));
                double gamma2 = 760 / P2;
                double gamma1 = 760 / P1;
                const double x2 = 1 - x1;
                double t1 = x1 + x2 * G12;
                double t2 = x2 + x1 * G21;
                double g2calc = Math.Exp(-Math.Log(t2) - x1 * (G12 * t2 - G21 * t1) / (t1 * t2));
                double g1calc = Math.Exp(-Math.Log(t1) + x2 * (G12 * t2 - G21 * t1) / (t1 * t2));
                double fG21 = t1 * t2 * (Math.Log(gamma2) + Math.Log(t2)) + x1 * (G12 * t2 - G21 * t1);
                double fG12 = t1 * t2 * (Math.Log(gamma1) + Math.Log(t1)) - x2 * (G12 * t2 - G21 * t1);
                return new double[2] { fG21, fG12 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[2] { 0.8, 0.8 }, 1e-14);
            Assert.AreEqual(0.3017535528355, r[0], 1e-5);
            Assert.AreEqual(0.0785888934885, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

        /*
         * Test converges, but to a point at A=1.7455, B=2.59. Error in test case?
        [Test]
        public void Twoeq5a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq5a.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double A = xa[0];
                double B = xa[1];
                const double t = 70.9;
                const double pw1 = 49;
                const double x1 = (pw1 / 46.07) / (pw1 / 46.07 + (100 - pw1) / 100.2);
                double P2 = Math.Pow(10, 6.9024 - 1268.115 / (216.9 + t));
                double P1 = Math.Pow(10, 8.04494 - 1554.3 / (222.65 + t));
                double gamma1 = 760 / P1;
                const double x2 = 1 - x1;
                double gamma2 = 760 / P2;
                double g1calc = Math.Pow(10, A * Math.Pow(x2 / (A * x1 / B + x2), 2));
                double g2calc = Math.Pow(10, B * Math.Pow(x1 / (x1 + B * x2 / A), 2));
                double fA = Math.Log(gamma1) - A * Math.Pow(x2 / (A * x1 / B + x2), 2);
                double fB = Math.Log(gamma2) - B * Math.Pow(x1 / (x1 + B * x2 / A), 2);
                return new double[2] { fA, fB };
            };

            double[] r = Broyden.FindRoot(fa1, new double[2] { 1.0, 1.0 }, 1e-14);
            Assert.AreEqual(0.7580768059470, r[0], 1e-5);
            Assert.AreEqual(1.1249034445330, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }        
        */

        /*
         * Test converges, but to a point at A=1.7455, B=2.59. Error in test case?
        [Test]
        public void Twoeq5b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq5b.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double A = xa[0];
                double B = xa[1];
                const double t = 70.9;
                const double pw1 = 49;
                const double x1 = (pw1 / 46.07) / (pw1 / 46.07 + (100 - pw1) / 100.2);
                double P2 = Math.Pow(10, 6.9024 - 1268.115 / (216.9 + t));
                double P1 = Math.Pow(10, 8.04494 - 1554.3 / (222.65 + t));
                double gamma1 = 760 / P1;
                const double x2 = 1 - x1;
                double gamma2 = 760 / P2;
                double g1calc = Math.Pow(10, A * Math.Pow(x2 / (A * x1 / B + x2), 2));
                double g2calc = Math.Pow(10, B * Math.Pow(x1 / (x1 + B * x2 / A), 2));
                double fA = Math.Log(gamma1) * Math.Pow(A * x1 / B + x2, 2) - A * Math.Pow(x2, 2);
                double fB = Math.Log(gamma2) * Math.Pow(x1 + B * x2 / A, 2) - B * Math.Pow(x1, 2);
                return new double[2] { fA, fB };
            };

            double[] r = Broyden.FindRoot(fa1, new double[2] { 1.0, 1.0 }, 1e-14);
            Assert.AreEqual(0.7580768919420, r[0], 1e-5);
            Assert.AreEqual(1.1249035089540, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }        
        */

        [Test]
        public void Twoeq6()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq7.htm
            Func<double[], double[]> fa1 = x =>
            {
                double x1 = x[0];
                double x2 = x[1];
                double f1 = x1 / (1 - x1) - 5 * Math.Log(0.4 * (1 - x1) / x2) + 4.45977;
                double f2 = x2 - (0.4 - 0.5 * x1);
                return new double[2] { f1, f2 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[2] { 0.9, 0.5 }, 1e-14);
            Assert.AreEqual(0.7573962468236, r[0], 1e-5);
            Assert.AreEqual(0.0213018765882, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

        [Test]
        public void Twoeq7()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq7.htm
            Func<double[], double[]> fa1 = x =>
            {
                double x1 = x[0];
                double x2 = x[1];
                const double a = 0.5;
                const double b = 0.8;
                const double c = 0.3;
                const double kp = 604500;
                const double P = 0.00243;
                double f1 = a - Math.Pow((c + 2 * x1) * (a + b + c - 2 * x1), 2) / (x2) - x1;
                double f2 = x2 - kp * P * P * Math.Pow(b - 3 * x1, 3);
                return new double[2] { f1, f2 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[2] { 0, -1 }, 1e-14);
            Assert.AreEqual(0.6003231171445, r[0], 1e-5);
            Assert.AreEqual(-3.57990244801, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

        [Test]
        public void Twoeq8()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq8.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double rp = xa[0];
                double x = xa[1];
                double frp = rp - 0.327 * Math.Pow(x, 0.804) * Math.Exp(-5230 / (1.987 * (373 + 1.84e6 * rp)));
                double fx = x - (0.06 - 161 * rp);
                return new double[2] { frp, fx };
            };

            double[] r = Broyden.FindRoot(fa1, new double[2] { 0.0001, 0.01 }, 1e-14);
            Assert.AreEqual(0.0003406054400, r[0], 1e-5);
            Assert.AreEqual(0.0051625241669, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

        [Test]
        public void Twoeq9()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq9.htm
            // not solvable with this method
            Func<double[], double[]> fa1 = x =>
            {
                double fF = x[0];
                double u = x[1];
                const double L = 6000;
                const double D = 0.505;
                const double rho = 53;
                const double g = 32.2;
                const double eps = .00015;
                double Re = rho * D * u / (13.2 * 0.000672);
                double ffF = fF - 1 / Math.Pow(2.28 - 4 * Math.Log(eps / D + 4.67 / (Re * Math.Sqrt(fF))), 2);
                double fu = 133.7 - (2 * fF * rho * u * u * L / D + rho * g * 200) / (g * 144);
                return new double[2] { ffF, fu };
            };

            Assert.Throws<NonConvergenceException>(() => Broyden.FindRoot(fa1, new double[2] { 0.1, 10.0 }, 1e-14));
        }

        [Test]
        public void Twoeq10()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq10.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double t12 = xa[0];
                double t21 = xa[1];
                const double alpha = 0.4;
                double c1 = Math.Exp(-alpha * t21);
                double c2 = Math.Exp(-2 * alpha * t21);
                double c3 = Math.Exp(-alpha * t12);
                double c4 = Math.Exp(-2 * alpha * t12);
                const double x1 = 0.5;
                const double x2 = 1 - x1;

                double ft12 = 1 / (x1 * x2) - 2 * t21 * c2 / Math.Pow(x1 + x2 * c1, 3) - 2 * t12 * c4 / Math.Pow(x2 + x1 * c3, 3);
                double ft21 = (x1 - x2) / Math.Pow(x1 * x2, 2) + 6 * t21 * c2 * (1 - c1) / Math.Pow(x1 + x2 * c1, 4) + 6 * t12 * c4 * (c3 - 1) / Math.Pow(x2 + x1 * c3, 4);
                return new double[2] { ft12, ft21 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[2] { 0.1, 0.1 }, 1e-14);
            Assert.AreEqual(1.6043843214350, r[0], 1e-5);
            Assert.AreEqual(1.6043843214350, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

        [Test]
        public void Threeq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/Threeq1.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double t = xa[0];
                double x1 = xa[1];
                double x2 = xa[2];
                const double y2 = 0.8;
                const double y1 = 0.2;
                double p1 = Math.Pow(10, 7.62231 - 1417.9 / (191.15 + t));
                double p2 = Math.Pow(10, 8.10765 - 1750.29 / (235 + t));
                const double B = 0.7;
                const double A = 1.7;
                double gamma2 = Math.Pow(10, B * x1 * x1 / Math.Pow(x1 + B * x2 / A, 2));
                double gamma1 = Math.Pow(10, A * x2 * x2 / Math.Pow(A * x1 / B + x2, 2));
                double k2 = gamma2 * p2 / 760;
                double k1 = gamma1 * p1 / 760;
                double ft = x1 + x2 - 1;
                double fx1 = x1 - y1 / k1;
                double fx2 = x2 - y2 / k2;
                return new double[3] { ft, fx1, fx2 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[3] { 100, 0.2, 0.8 }, 1e-14);
            Assert.AreEqual(93.96706523770, r[0], 1e-5);
            Assert.AreEqual(0.0078754574659, r[1], 1e-5);
            Assert.AreEqual(0.9921245425339, r[2], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
            Assert.AreEqual(0, fa1(r)[2], 1e-14);
        }

        [Test]
        public void Threeq2()
        {
            // Test case from http://www.polymath-software.com/library/nle/Threeq2.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double x1 = xa[0];
                double x2 = xa[1];
                double alpha = xa[2];
                const double t = 88.538;
                const double B = 0.7;
                const double A = 1.7;
                const double z1 = 0.2;
                const double z2 = 0.8;
                double p1 = Math.Pow(10, 7.62231 - 1417.9 / (191.15 + t));
                double p2 = Math.Pow(10, 8.10765 - 1750.29 / (235 + t));
                double gamma2 = Math.Pow(10, B * x1 * x1 / Math.Pow(x1 + B * x2 / A, 2));
                double gamma1 = Math.Pow(10, A * x2 * x2 / Math.Pow(A * x1 / B + x2, 2));
                double k1 = gamma1 * p1 / 760;
                double k2 = gamma2 * p2 / 760;
                double y1 = k1 * x1;
                double y2 = k2 * x2;
                double fx1 = x1 - z1 / (1 + alpha * (k1 - 1));
                double fx2 = x2 - z2 / (1 + alpha * (k2 - 1));
                double falpha = x1 + x2 - (y1 + y2);
                return new double[3] { fx1, fx2, falpha };
            };

            double[] r = Broyden.FindRoot(fa1, new double[3] { 0, 1, 0.5 }, 1e-14);
            Assert.AreEqual(0.0226974766367, r[0], 1e-5);
            Assert.AreEqual(0.9773025233633, r[1], 1e-5);
            Assert.AreEqual(0.5322677863643, r[2], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
            Assert.AreEqual(0, fa1(r)[2], 1e-14);
        }

        [Test]
        public void Threeq3()
        {
            // Test case from http://www.polymath-software.com/library/nle/Threeq3.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double T = xa[0];
                double Ca = xa[1];
                double Tj = xa[2];
                double k = 7.08e10 * Math.Exp(-30000 / 1.9872 / T);
                const double rhocp = 50 * 0.75;
                const double F = 40;
                const double T0 = 530;
                const double V = 48;
                const double U = 150;
                const double A = 250;
                const double Ca0 = 0.55;
                const double Fj = 49.9;
                const double Tj0 = 530;
                double fT = F * (T0 - T) / V + 30000 * k * Ca / rhocp - U * A * (T - Tj) / (rhocp * V);
                double fCa = F * (Ca0 - Ca) / V - k * Ca;
                double fTj = Fj * (Tj0 - Tj) / 3.85 + U * A * (T - Tj) / (62.3 * 1.0 * 3.85);
                return new double[3] { fT, fCa, fTj };
            };

            double[] r = Broyden.FindRoot(fa1, new double[3] { 600, 0.1, 600 }, 1e-11);
            Assert.AreEqual(590.34979512380, r[0], 1e-5);
            Assert.AreEqual(0.3301868979161, r[1], 1e-5);
            Assert.AreEqual(585.72976766210, r[2], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
            Assert.AreEqual(0, fa1(r)[2], 1e-11);
        }

        [Test]
        public void Threeq4a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Threeq4a.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double CD = xa[0];
                double CX = xa[1];
                double CZ = xa[2];
                double CY = CX + CZ;
                double CC = CD - CY;
                const double CA0 = 1.5;
                const double CB0 = 1.5;
                double CA = CA0 - CD - CZ;
                double CB = CB0 - CD - CY;
                const double KC1 = 1.06;
                const double KC2 = 2.63;
                const double KC3 = 5;
                double fCD = CC * CD / (CA * CB) - KC1;
                double fCX = CX * CY / (CB * CC) - KC2;
                double fCZ = CZ / (CA * CX) - KC3;
                return new double[3] { fCD, fCX, fCZ };
            };

            double[] r = Broyden.FindRoot(fa1, new double[3] { 0.7, 0.2, 0.4 }, 1e-14);
            Assert.AreEqual(0.7053344059695, r[0], 1e-5);
            Assert.AreEqual(0.1777924200537, r[1], 1e-5);
            Assert.AreEqual(0.3739765850146, r[2], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
            Assert.AreEqual(0, fa1(r)[2], 1e-14);
        }

        [Test]
        public void Threeq4b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Threeq4b.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double CD = xa[0];
                double CX = xa[1];
                double CZ = xa[2];
                double CY = CX + CZ;
                double CC = CD - CY;
                const double CA0 = 1.5;
                const double CB0 = 1.5;
                double CA = CA0 - CD - CZ;
                double CB = CB0 - CD - CY;
                const double KC1 = 1.06;
                const double KC2 = 2.63;
                const double KC3 = 5;
                double fCD = CC * CD - KC1 * (CA * CB);
                double fCX = CX * CY - KC2 * (CB * CC);
                double fCZ = CZ - KC3 * (CA * CX);
                return new double[3] { fCD, fCX, fCZ };
            };

            double[] r = Broyden.FindRoot(fa1, new double[3] { 0.7, 0.2, 0.4 }, 1e-14);
            Assert.AreEqual(0.7053344059695, r[0], 1e-5);
            Assert.AreEqual(0.1777924200537, r[1], 1e-5);
            Assert.AreEqual(0.3739765850146, r[2], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
            Assert.AreEqual(0, fa1(r)[2], 1e-14);
        }

        [Test]
        public void Threeq5()
        {
            // Test case from http://www.polymath-software.com/library/nle/Threeq5.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double X = xa[0];
                double T = xa[1];
                double h = xa[2];
                double k1 = 3e5 * Math.Exp(-5000 / T);
                double k2 = 6e7 * Math.Exp(-7500 / T);
                const double F0 = 1;
                const double T0 = 300;
                double fX = -0.16 * X * F0 / h + k1 * (1 - X) - k2 * X;
                double fT = 0.16 * F0 * T0 / h - 0.16 * T * F0 / h + 5 * (k1 * (1 - X) - k2 * X);
                double fh = 0.16 * F0 - 0.4 * Math.Sqrt(h);
                return new double[3] { fX, fT, fh };
            };

            double[] r = Broyden.FindRoot(fa1, new double[3] { 0.5, 500, 0.5 }, 1e-13);
            Assert.AreEqual(0.0171035426725, r[0], 1e-5);
            Assert.AreEqual(300.08551771340, r[1], 1e-5);
            Assert.AreEqual(0.16, r[2], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-13);
            Assert.AreEqual(0, fa1(r)[2], 1e-14);
        }

        [Test]
        public void Threeq6()
        {
            // Test case from http://www.polymath-software.com/library/nle/Threeq6.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double CA = xa[0];
                double CB = xa[1];
                double T = xa[2];
                double k1 = 11 * Math.Exp(-4180 / (8.314 * (T + 273.16)));
                double k2 = 172.2 * Math.Exp(-34833 / (8.314 * (T + 273.16)));
                const double Q = 5.1E6;
                double fCA = 0.1 * (1 - CA) - k1 * CA * CA;
                double fCB = -0.1 * CB + k1 * CA * CA - k2 * CB;
                double fT = 0.1 * (25 - T) - 418 * k1 * CA * CA - 418 * k2 * CB + Q * 1e-5;
                return new double[3] { fCA, fCB, fT };
            };

            double[] r = Broyden.FindRoot(fa1, new double[3] { 0.5, 0.5, 500 }, 1e-13);
            Assert.AreEqual(0.1578109142617, r[0], 1e-5);
            Assert.AreEqual(0.77071354919, r[1], 1e-5);
            Assert.AreEqual(153.09, r[2], 1e-2);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
            Assert.AreEqual(0, fa1(r)[2], 1e-13);
        }

        [Test]
        public void Threeq7()
        {
            // Test case from http://www.polymath-software.com/library/nle/Threeq7.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double CA = xa[0];
                double CB = xa[1];
                double T = xa[2];
                const double T0 = 298;
                const double CA0 = 3;
                const double CB0 = 0;
                const double CC0 = 0;
                const double theta = 300;
                double k1 = 4e6 * Math.Exp(-60000 / (8.314 * T));
                double KA = 17 * Math.Exp(-7000 / (8.314 * T));
                double k2 = 3e4 * Math.Exp(-80000 / (8.314 * T));
                double k2p = 3e4 * Math.Exp(-90000 / (8.314 * T));
                double CC = (CC0 + theta * k2 * CB) / (1 + theta * k2p);
                double fCA = CA0 - CA - theta * k1 * CA / (1 + KA * CB);
                double fCB = CB - CB0 - (theta * k1 * CA / (1 + KA * CB) - theta * k2 * CB + theta * k2p * CC);
                double fT = 85 * (T - T0) + 0.02 * (T * T - T0 * T0) - ((16000 + 3 * T - 0.002 * T * T) * ((CA0 - CA) / CA0) + (30000 + 4 * T - 0.003 * T * T) * CC / CA0);
                return new double[3] { fCA, fCB, fT };
            };

            double[] r = Broyden.FindRoot(fa1, new double[3] { 3, 0, 300 }, 1e-13);
            Assert.AreEqual(2.7873203092940, r[0], 1e-5);
            Assert.AreEqual(0.2126796260201, r[1], 1e-5);
            Assert.AreEqual(310.212556340, r[2], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
            Assert.AreEqual(0, fa1(r)[2], 1e-14);
        }

        [Test]
        public void Threeq8()
        {
            // Test case from http://www.polymath-software.com/library/nle/Threeq8.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double p4 = xa[0];
                double Q24 = xa[1];
                double Q34 = xa[2];
                double p2 = 156.6 - 0.00752 * Q24 * Q24;
                double p3 = 117.1 - 0.00427 * Q34 * Q34;
                const double D34 = 2.067 / 12;
                const double D24 = 1.278 / 12;
                const double D45 = 2.469 / 12;
                const double fF = 0.015;
                const double pi = 3.1416;
                double k24 = 2 * fF * 125 / (Math.Pow((60 * 7.48) * (pi * D24 * D24 / 4), 2) * D24);
                double k34 = 2 * fF * 125 / (Math.Pow((60 * 7.48) * (pi * D34 * D34 / 4), 2) * D34);
                double k45 = 2 * fF * 145 / (Math.Pow((60 * 7.48) * (pi * D45 * D45 / 4), 2) * D45);
                double fp4 = 70 * 32.3 - p4 * (144 * 32.2 / 62.35) + k45 * Math.Pow(Q24 + Q34, 2);
                double fQ24 = (p4 - p2) * (144 * 32.2 / 62.35) + k24 * Q24 * Q24;
                double fQ34 = (p4 - p3) * (144 * 32.2 / 62.35) + k34 * Q34 * Q34;
                return new double[3] { fp4, fQ24, fQ34 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[3] { 50, 100, 100 }, 1e-11);
            Assert.AreEqual(57.12556038475, r[0], 1e-5);
            Assert.AreEqual(51.75154563498, r[1], 1e-5);
            Assert.AreEqual(92.91811138918, r[2], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
        }
    }
}
