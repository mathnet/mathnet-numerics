// <copyright file="BroydenTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

using MathNet.Numerics.RootFinding;
using NUnit.Framework;
using System;

namespace MathNet.Numerics.UnitTests.RootFindingTests
{
    [TestFixture, Category("RootFinding")]
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
            Assert.That(() => BroydenFindRoot(f1, -5, 5, 1e-14), Throws.TypeOf<NonConvergenceException>());
            Assert.That(() => BroydenFindRoot(f1, -2, 4, 1e-14), Throws.TypeOf<NonConvergenceException>());
            Assert.AreEqual(0, f1(BroydenFindRoot(f1, -2, -1, 1e-14)), 1e-14);
        }

        [Test]
        public void NoRoot()
        {
            Func<double, double> f1 = x => x * x + 4;
            Assert.That(() => BroydenFindRoot(f1, -5, 5, 1e-14), Throws.TypeOf<NonConvergenceException>());
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
                const double t1 = x1 + x2 * G12;
                const double t2 = x2 + x1 * G21;
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

            Assert.That(() => BroydenFindRoot(f1, 0.1, 1), Throws.TypeOf<NonConvergenceException>());
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
            Assert.That(() => BroydenFindRoot(f1, 0, 0.79, 1e-2), Throws.TypeOf<NonConvergenceException>());
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

            Assert.That(() => BroydenFindRoot(f1, .01, 0.35), Throws.TypeOf<NonConvergenceException>());
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
                const double xx = 1;

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

            Assert.That(() => BroydenFindRoot(f1, 0, 0.00035), Throws.TypeOf<NonConvergenceException>());
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

            Assert.That(() => BroydenFindRoot(f1, -0.5, 1.2), Throws.TypeOf<NonConvergenceException>());
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

            Assert.That(() => BroydenFindRoot(f1, 0.75, 1.02), Throws.TypeOf<NonConvergenceException>());
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

            Assert.That(() => BroydenFindRoot(f1, 0, 0.3), Throws.TypeOf<NonConvergenceException>());
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

            Assert.That(() => BroydenFindRoot(f1, 0, 0.1), Throws.TypeOf<NonConvergenceException>());
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
            Func<double[], double[]> fw = x => new[] { f(x[0]) };
            double[] initialGuess = { (lowerBound + upperBound) * 0.5 };
            return Broyden.FindRoot(fw, initialGuess, accuracy, maxIterations)[0];
        }

        [Test]
        public void Twoeq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq1.htm
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
                double ffF = (Re < 2100) ? (fF - 16 / Re) : (fF - 1 / Math.Pow(4 * Math.Log10(Re * Math.Sqrt(fF)) - 0.4, 2));
                return new[] { fD, ffF };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.04, 0.001 }, 1e-14);
            Assert.AreEqual(0.0389653029101, r[0], 1e-5);
            Assert.AreEqual(0.0045905347283, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
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
                return new[] { fx, fT };
            };

            double[] r = Broyden.FindRoot(fa1, new double[] { 1, 400 }, 1e-6);
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
                return new[] { fx, fT };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.5, 1700 }, 1e-11);
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
                return new[] { fG21, fG12 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.5, 0.5 }, 1e-14);
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
                return new[] { fG21, fG12 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.8, 0.8 }, 1e-14);
            Assert.AreEqual(0.3017535528355, r[0], 1e-5);
            Assert.AreEqual(0.0785888934885, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

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
                double fA = Math.Log10(gamma1) - A * Math.Pow(x2 / (A * x1 / B + x2), 2);
                double fB = Math.Log10(gamma2) - B * Math.Pow(x1 / (x1 + B * x2 / A), 2);
                return new[] { fA, fB };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 1.0, 1.0 }, 1e-14);
            Assert.AreEqual(0.7580768059470, r[0], 1e-5);
            Assert.AreEqual(1.1249034445330, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

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
                double fA = Math.Log10(gamma1) * Math.Pow(A * x1 / B + x2, 2) - A * Math.Pow(x2, 2);
                double fB = Math.Log10(gamma2) * Math.Pow(x1 + B * x2 / A, 2) - B * Math.Pow(x1, 2);
                return new[] { fA, fB };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 1.0, 1.0 }, 1e-14);
            Assert.AreEqual(0.7580768919420, r[0], 1e-5);
            Assert.AreEqual(1.1249035089540, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

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
                return new[] { f1, f2 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.9, 0.5 }, 1e-14);
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
                return new[] { f1, f2 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[] { 0, -1 }, 1e-14);
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
                return new[] { frp, fx };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.0001, 0.01 }, 1e-14);
            Assert.AreEqual(0.0003406054400, r[0], 1e-5);
            Assert.AreEqual(0.0051625241669, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
        }

        [Test]
        public void Twoeq9()
        {
            // Test case from http://www.polymath-software.com/library/nle/Twoeq9.htm
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
                double ffF = fF - 1 / Math.Pow(2.28 - 4 * Math.Log10(eps / D + 4.67 / (Re * Math.Sqrt(fF))), 2);
                double fu = 133.7 - (2 * fF * rho * u * u * L / D + rho * g * 200) / (g * 144);
                return new[] { ffF, fu };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.1, 10.0 }, 1e-14);
            Assert.AreEqual(0.006874616348157, r[0], 1e-5);
            Assert.AreEqual(5.6728221306731, r[1], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-14);
            Assert.AreEqual(0, fa1(r)[1], 1e-14);
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
                return new[] { ft12, ft21 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.1, 0.1 }, 1e-14);
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
                return new[] { ft, fx1, fx2 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 100, 0.2, 0.8 }, 1e-14);
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
                return new[] { fx1, fx2, falpha };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0, 1, 0.5 }, 1e-14);
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
                return new[] { fT, fCa, fTj };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 600, 0.1, 600 }, 1e-11);
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
                return new[] { fCD, fCX, fCZ };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.7, 0.2, 0.4 }, 1e-14);
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
                return new[] { fCD, fCX, fCZ };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.7, 0.2, 0.4 }, 1e-14);
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
                return new[] { fX, fT, fh };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.5, 500, 0.5 }, 1e-13);
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
                return new[] { fCA, fCB, fT };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.5, 0.5, 500 }, 1e-13);
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
                return new[] { fCA, fCB, fT };
            };

            double[] r = Broyden.FindRoot(fa1, new double[] { 3, 0, 300 }, 1e-13);
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
                return new[] { fp4, fQ24, fQ34 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[] { 50, 100, 100 }, 1e-11);
            Assert.AreEqual(57.12556038475, r[0], 1e-5);
            Assert.AreEqual(51.75154563498, r[1], 1e-5);
            Assert.AreEqual(92.91811138918, r[2], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
        }

        [Test]
        public void Foureq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/Foureq1.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double CA = xa[0];
                double CB = xa[1];
                double CC = xa[2];
                double T = xa[3];
                double k1 = 4e6 * Math.Exp(-60000 / (8.314 * T));
                double KA = 17 * Math.Exp(-7000 / (8.314 * T));
                double k2 = 3e4 * Math.Exp(-80000 / (8.314 * T));
                double k2p = 3e4 * Math.Exp(-90000 / (8.314 * T));
                const double T0 = 298;
                const double CA0 = 3;
                const double CB0 = 0;
                const double CC0 = 0;
                const double theta = 300;
                double fCA = CA0 - CA - theta * k1 * CA / (1 + KA * CB);
                double fCB = CB - CB0 - (theta * k1 * CA / (1 + KA * CB) - theta * k2 * CB + theta * k2p * CC);
                double fCC = CC - CC0 - theta * k2 * CB + theta * k2p * CC;
                double fT = 85 * (T - T0) + 0.02 * (T * T - T0 * T0) - ((16000 + 3 * T - 0.002 * T * T) * ((CA0 - CA) / CA0) + (30000 + 4 * T - 0.003 * T * T) * CC / CA0);
                return new[] { fCA, fCB, fCC, fT };
            };

            double[] r = Broyden.FindRoot(fa1, new double[] { 3, 0, 0, 300 }, 1e-14);
            Assert.AreEqual(2.7873203092938, r[0], 1e-5);
            Assert.AreEqual(0.2126796260201, r[1], 1e-5);
            Assert.AreEqual(6.468613E-08, r[2], 1e-14);
            Assert.AreEqual(310.2125563399750, r[3], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
        }

        [Test]
        public void Fiveq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/Fiveq1.htm
            // Constraints (0 <= b <= 1, 0 <= y <= 1) are not met; found an "unphysical" solution.
            Func<double[], double[]> fa1 = xa =>
            {
                double ca = xa[0];
                double t1 = xa[1];
                double tc = xa[2];
                double b = xa[3];
                double y = xa[4];
                double k = 0.0744 * Math.Exp(-1.182e7 / (8314.39 * (t1 + 273.16)));
                const double kc = 1;
                double m = y + kc * (10 / 20 - b);
                double fc = 0.02 * Math.Pow(50, -m);
                const double F = 0.0075;
                const double V = 7.08;
                const double dhr = -9.86e7;
                const double rho = 19.2;
                const double cp = 1.815e5;
                const double u = 3550;
                const double a = 5.4;
                const double taui = 600;
                double fca = F * (2.88 - ca) / V - k * ca * ca;
                double ft1 = F * (66 - t1) / V - dhr * k * ca * ca / (rho * cp) - u * a * (t1 - tc) / (V * rho * cp);
                double ftc = u * a * (t1 - tc) / (1.82 * 1000 * 4184) - fc * (tc - 27) / 1.82;
                double fb = ((t1 - 80) / 20 - b) / 20;
                double fy = (m - y) / taui;
                return new[] { fca, ft1, ftc, fb, fy };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 1, 100, 50, 0.4, 0.25 }, 1e-14);
            Assert.IsFalse(r[3] >= 0);
            Assert.IsFalse(r[4] >= 0);
            //Assert.AreEqual(1.1206138931808, r[0], 1e-5);
            //Assert.AreEqual(90, r[1], 1e-5);
            //Assert.AreEqual(54.8512245178517, r[2], 1e-5);
            //Assert.AreEqual(0.5, r[3], 1e-5);
            //Assert.AreEqual(0.3172119884117, r[4], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
        }

        [Test, Ignore("Known convergence problem on Mono")]
        public void Sixeq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/Sixeq1.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double x1 = xa[0];
                double x2 = xa[1];
                double x3 = xa[2];
                double x4 = xa[3];
                double x5 = xa[4];
                double x6 = xa[5];
                double fx1 = x1 + x2 + x4 - 0.001;
                double fx2 = x5 + x6 - 55;
                double fx3 = x1 + x2 + x3 + 2 * x5 + x6 - 110.001;
                double fx4 = x1 - 0.1 * x2;
                double fx5 = x1 - 1e4 * x3 * x4;
                double fx6 = x5 - 55e14 * x3 * x6;
                return new[] { fx1, fx2, fx3, fx4, fx5, fx6 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[] { 10, 10, 10, 10, 10, 10 }, 1e-2, 1000);
            Assert.AreEqual(0.0000826446329, r[0], 1e-9);
            Assert.AreEqual(0.0008264463286, r[1], 1e-8);
            Assert.AreEqual(0.0000909091485, r[2], 1e-8);
            Assert.AreEqual(0.0000909090385, r[3], 1e-8);
            Assert.AreEqual(54.9999999998900, r[4], 1e-5);
            Assert.AreEqual(0.0000000001100, r[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-10);
            Assert.AreEqual(0, fa1(r)[2], 1e-10);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-7);
            Assert.AreEqual(0, fa1(r)[5], 1e-2);
        }

        [Test]
        public void Sixeq2a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Sixeq2a.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double x1 = xa[0];
                double x2 = xa[1];
                double x3 = xa[2];
                double x4 = xa[3];
                double x5 = xa[4];
                double x6 = xa[5];
                const double k1 = 31.24;
                const double k2 = 2.062;
                const double kr1 = 0.272;
                const double kr2 = 0.02;
                const double k3 = 303.03;
                double fx1 = 1 - x1 - k1 * x1 * x6 + kr1 * x4;
                double fx2 = 1 - x2 - k2 * x2 * x6 + kr2 * x5;
                double fx3 = -x3 + 2 * k3 * x4 * x5;
                double fx4 = k1 * x1 * x6 - kr1 * x4 - k3 * x4 * x5;
                double fx5 = 1.5 * (k2 * x2 * x6 - kr2 * x5) - k3 * x4 * x5;
                double fx6 = 1 - x4 - x5 - x6;
                return new[] { fx1, fx2, fx3, fx4, fx5, fx6 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.99, 0.05, 0.05, 0.99, 0.05, 0 }, 1e-14);
            Assert.AreEqual(0.9700739407529, r[0], 1e-5);
            Assert.AreEqual(0.9800492938353, r[1], 1e-5);
            Assert.AreEqual(0.0598521184942, r[2], 1e-6);
            Assert.AreEqual(0.9900268865935, r[3], 1e-5);
            Assert.AreEqual(0.0000997509107, r[4], 1e-10);
            Assert.AreEqual(0.0098733624958, r[5], 1e-8);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
        }

        [Test]
        public void Sixeq2b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Sixeq2b.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double x1 = xa[0];
                double x2 = xa[1];
                double x3 = xa[2];
                double x4 = xa[3];
                double x5 = xa[4];
                double x6 = xa[5];
                const double k1 = 17.721;
                const double k2 = 3.483;
                const double kr1 = 0.118;
                const double kr2 = 0.033;
                const double k3 = 505.051;
                double fx1 = 1 - x1 - k1 * x1 * x6 + kr1 * x4;
                double fx2 = 1 - x2 - k2 * x2 * x6 + kr2 * x5;
                double fx3 = -x3 + 2 * k3 * x4 * x5;
                double fx4 = k1 * x1 * x6 - kr1 * x4 - k3 * x4 * x5;
                double fx5 = 1.5 * (k2 * x2 * x6 - kr2 * x5) - k3 * x4 * x5;
                double fx6 = 1 - x4 - x5 - x6;
                return new[] { fx1, fx2, fx3, fx4, fx5, fx6 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.99, 0.05, 0.05, 0.99, 0.05, 0 }, 1e-14);
            Assert.AreEqual(0.9499424500947, r[0], 1e-5);
            Assert.AreEqual(0.9666283000631, r[1], 1e-5);
            Assert.AreEqual(0.1001150998106, r[2], 1e-5);
            Assert.AreEqual(0.9899868097778, r[3], 1e-5);
            Assert.AreEqual(0.0001001163356, r[4], 1e-10);
            Assert.AreEqual(0.0099130738866, r[5], 1e-8);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
        }

        [Test]
        public void Sixeq2c()
        {
            // Test case from http://www.polymath-software.com/library/nle/Sixeq2c.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double x1 = xa[0];
                double x2 = xa[1];
                double x3 = xa[2];
                double x4 = xa[3];
                double x5 = xa[4];
                double x6 = xa[5];
                const double k1 = 17.721;
                const double k2 = 6.966;
                const double kr1 = 0.118;
                const double kr2 = 333.333;
                const double k3 = 505.051;
                double fx1 = 1 - x1 - k1 * x1 * x6 + kr1 * x4;
                double fx2 = 1 - x2 - k2 * x2 * x6 + kr2 * x5;
                double fx3 = -x3 + 2 * k3 * x4 * x5;
                double fx4 = k1 * x1 * x6 - kr1 * x4 - k3 * x4 * x5;
                double fx5 = 1.5 * (k2 * x2 * x6 - kr2 * x5) - k3 * x4 * x5;
                double fx6 = 1 - x4 - x5 - x6;
                return new[] { fx1, fx2, fx3, fx4, fx5, fx6 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.99, 0.05, 0.05, 0.99, 0.05, 0 }, 1e-14);
            Assert.AreEqual(0.9499356376871, r[0], 1e-5);
            Assert.AreEqual(0.9666237584581, r[1], 1e-5);
            Assert.AreEqual(0.1001150998106, r[2], 1e-4);
            Assert.AreEqual(0.9899863241315, r[3], 1e-5);
            Assert.AreEqual(0.0001001299968, r[4], 1e-10);
            Assert.AreEqual(0.0099135458718, r[5], 1e-8);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
        }

        [Test]
        public void Sixeq3()
        {
            // Test case from http://www.polymath-software.com/library/nle/Sixeq3.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double x11 = xa[0];
                double x12 = xa[1];
                double x21 = xa[2];
                double x22 = xa[3];
                double t = xa[4];
                double beta1 = xa[5];
                double p1 = Math.Pow(10, 7.62231 - 1417.9 / (191.15 + t));
                double p2 = Math.Pow(10, 8.10765 - 1750.29 / (235 + t));
                const double A = 1.7;
                const double B = 0.7;
                double gamma11 = Math.Pow(10, A * x21 * x21 / Math.Pow(A * x11 / B + x21, 2));
                double gamma21 = Math.Pow(10, B * x11 * x11 / Math.Pow(x11 + B * x21 / A, 2));
                double gamma12 = Math.Pow(10, A * x22 * x22 / Math.Pow(A * x12 / B + x22, 2));
                double gamma22 = Math.Pow(10, B * x12 * x12 / Math.Pow(x12 + B * x22 / A, 2));
                double k11 = gamma11 * p1 / 760;
                double k21 = gamma21 * p2 / 760;
                double k12 = gamma12 * p1 / 760;
                double k22 = gamma22 * p2 / 760;
                double fx11 = x11 - 0.2 / (beta1 + (1 - beta1) * k11 / k12);
                double fx12 = x12 - x11 * k11 / k12;
                double fx21 = x21 - 0.8 / (beta1 + (1 - beta1) * k21 / k22);
                double fx22 = x22 - x21 * k21 / k22;
                double ft = x11 * (1 - k11) + x21 * (1 - k21);
                double fbeta1 = (x11 - x12) + (x21 - x22);
                return new[] { fx11, fx12, fx21, fx22, ft, fbeta1 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0, 1, 1, 0, 100, 0.8 }, 1e-14);
            Assert.AreEqual(0.0226982050031, r[0], 1e-7);
            Assert.AreEqual(0.6867475652564, r[1], 1e-6);
            Assert.AreEqual(0.9773017949969, r[2], 1e-6);
            Assert.AreEqual(0.3132524347436, r[3], 1e-6);
            Assert.AreEqual(88.5378298767092, r[4], 1e-5);
            Assert.AreEqual(0.7329990726454, r[5], 1e-6);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
        }

        [Test]
        public void Sixeq4a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Sixeq4a.htm
            // not solvable with this method
            Func<double[], double[]> fa1 = xa =>
            {
                double CA = xa[0];
                double CB = xa[1];
                double CC = xa[2];
                double CD = xa[3];
                double CE = xa[4];
                double T = xa[5];
                const double R = 1.987;
                double k1B = 0.4 * Math.Exp((20000 / R) * (1.0 / 300.0 - 1 / T));
                double k2C = 10 * Math.Exp((5000 / R) * (1.0 / 310.0 - 1 / T));
                double k3E = 10 * Math.Exp((10000 / R) * (1.0 / 320.0 - 1 / T));
                double r1B = -k1B * CA * CB;
                double r2C = -k2C * CC * CB * CB;
                double r3E = k3E * CD;
                double rA = 2 * r1B;
                double rB = r1B + 2 * r2C;
                double rC = -3 * r1B + r2C;
                double rD = -r3E - r2C;
                double rE = r3E;
                double SRH = -rA * 20000 + 2 * r2C * 10000 + 5000 * r3E;
                const double V = 500;
                const double vo = 75 / 3.3;
                const double CAO = 25 / vo;
                const double CBO = 50 / vo;
                double fCA = V - vo * (CAO - CA) / (-rA);
                double fCB = V - vo * (CBO - CB) / (-rB);
                double fCC = V - vo * CC / rC;
                double fCD = V - vo * CD / rD;
                double fCE = V - vo * CE / rE;
                double fT = 5000 * (350 - T) - 25 * (20 + 40) * (T - 300) + V * SRH;
                return new[] { fCA, fCB, fCC, fCD, fCE, fT };
            };

            Assert.That(() => Broyden.FindRoot(fa1, new[] { 0.5, 0.01, 1, 0.01, 1, 420 }, 1e1), Throws.TypeOf<NonConvergenceException>());
        }

        [Test, Ignore("Known convergence problem")]
        public void Sixeq4b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Sixeq4b.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double CA = xa[0];
                double CB = xa[1];
                double CC = xa[2];
                double CD = xa[3];
                double CE = xa[4];
                double T = xa[5];
                const double R = 1.987;
                double k1B = 0.4 * Math.Exp((20000 / R) * (1.0 / 300.0 - 1 / T));
                double k2C = 10 * Math.Exp((5000 / R) * (1.0 / 310.0 - 1 / T));
                double k3E = 10 * Math.Exp((10000 / R) * (1.0 / 320.0 - 1 / T));
                double r1B = -k1B * CA * CB;
                double r2C = -k2C * CC * CB * CB;
                double r3E = k3E * CD;
                double rA = 2 * r1B;
                double rB = r1B + 2 * r2C;
                double rC = -3 * r1B + r2C;
                double rD = -r3E - r2C;
                double rE = r3E;
                double SRH = -rA * 20000 + 2 * r2C * 10000 + 5000 * r3E;
                const double V = 500;
                const double vo = 75 / 3.3;
                const double CAO = 25 / vo;
                const double CBO = 50 / vo;
                double fCA = V * (-rA) - vo * (CAO - CA);
                double fCB = V * (-rB) - vo * (CBO - CB);
                double fCC = V * rC - vo * CC;
                double fCD = V * rD - vo * CD;
                double fCE = V * rE - vo * CE;
                double fT = 5000 * (350 - T) - 25 * (20 + 40) * (T - 300) + V * SRH;
                return new[] { fCA, fCB, fCC, fCD, fCE, fT };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.5, 0.01, 1, 0.01, 1, 420 }, 1e-7, 10000);
            Assert.AreEqual(0.002666326911334, r[0], 1e-9);
            Assert.AreEqual(0.033464055791589, r[1], 1e-8);
            Assert.AreEqual(0.837065955800961, r[2], 1e-6);
            Assert.AreEqual(0.000396698449814, r[3], 1e-10);
            Assert.AreEqual(0.808537855382225, r[4], 1e-6);
            Assert.AreEqual(372.764586230922, r[5], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-9);
            Assert.AreEqual(0, fa1(r)[1], 1e-9);
            Assert.AreEqual(0, fa1(r)[2], 1e-9);
            Assert.AreEqual(0, fa1(r)[3], 1e-8);
            Assert.AreEqual(0, fa1(r)[4], 1e-8);
            Assert.AreEqual(0, fa1(r)[5], 1e-7);
        }

        [Test]
        public void Seveneq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/Seveneq1.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double x1 = xa[0];
                double x2 = xa[1];
                double x3 = xa[2];
                double x4 = xa[3];
                double x5 = xa[4];
                double x6 = xa[5];
                double x7 = xa[6];
                double fx1 = 0.5 * x1 + x2 + 0.5 * x3 - x6 / x7;
                double fx2 = x3 + x4 + 2 * x5 - 2 / x7;
                double fx3 = x1 + x2 + x5 - 1 / x7;
                double fx4 = -28837 * x1 - 139009 * x2 - 78213 * x3 + 18927 * x4 + 8427 * x5 + 13492 / x7 - 10690 * x6 / x7;
                double fx5 = x1 + x2 + x3 + x4 + x5 - 1;
                double fx6 = 400 * x1 * x4 * x4 * x4 - 1.7837e5 * x3 * x5;
                double fx7 = x1 * x3 - 2.6058 * x2 * x4;
                return new[] { fx1, fx2, fx3, fx4, fx5, fx6, fx7 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.5, 0, 0, 0.5, 0, 0.5, 2 }, 1e-12);
            Assert.AreEqual(0.3228708394765, r[0], 1e-9);
            Assert.AreEqual(0.0092235435392, r[1], 1e-8);
            Assert.AreEqual(0.0460170909606, r[2], 1e-8);
            Assert.AreEqual(0.6181716750708, r[3], 1e-8);
            Assert.AreEqual(0.0037168509528, r[4], 1e-5);
            Assert.AreEqual(0.5767153959355, r[5], 1e-12);
            Assert.AreEqual(2.9778634507910, r[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
        }

        [Test]
        public void Seveneq2a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Seveneq2a.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double A = xa[0];
                double B = xa[1];
                double C = xa[2];
                double D = xa[3];
                double E = xa[4];
                double F = xa[5];
                double G = xa[6];
                double fA = 5.5 * (D - B) / (A - B - C) - B / (B + C);
                double fB = 3.5 * (0.888 * A - C - D) / (A - B - C) - C / (B + C);
                double fC = 5.5 * (0.0986 * A - 0.01 * (1.098 * A - B - 9 * D - E - F + G) - (B + E)) / (1.098 * A - B - 9 * D - E - F + G) - E / (E + F);
                double fD = 3.5 * (0.986 * A - 9 * D - F - 0.0986 * A + 0.01 * (1.098 * A - B - 9 * D - E - F + G)) / (1.098 * A - B - 9 * D - E - F + G) - F / (E + F);
                double fE = 150.2 * 133.7 * 0.32 * 0.32 / (A - B - C) - D * (A - B - C) / Math.Pow(0.0986 * A - D, 2);
                double fF = 446.8 * 133.7 * 0.0032 * 0.0032 / (1.098 * A - B - 9 * D - E - F + G) - (A * 0.0986 - D) / (1.098 * A - B - 9 * D - E - F + G) + 0.01;
                double fG = 0.04 * (74.12 * (0.986 * A - 10 * D) + 222.24 * (0.0986 * A - D) + 18 * (D - B) + 278.84 * D + 98.09 * 0.0136 * A) / 98.01 - 0.0136 * A - G;
                return new[] { fA, fB, fC, fD, fE, fF, fG };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 80, 7, 60, 7, 0.5, 4, 0.05 }, 1e-12);
            Assert.AreEqual(80.5340430736936, r[0], 1e-9);
            Assert.AreEqual(6.9725465115675, r[1], 1e-8);
            Assert.AreEqual(61.1190817582544, r[2], 1e-8);
            Assert.AreEqual(7.2042004491716, r[3], 1e-8);
            Assert.AreEqual(0.5476701839101, r[4], 1e-5);
            Assert.AreEqual(3.6532933311347, r[5], 1e-12);
            Assert.AreEqual(0.0597027236971, r[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
        }

        [Test]
        public void Seveneq2b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Seveneq2b.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double A = xa[0];
                double B = xa[1];
                double C = xa[2];
                double D = xa[3];
                double E = xa[4];
                double F = xa[5];
                double G = xa[6];
                double fA = 5.5 * (D - B) * (B + C) - B * (A - B - C);
                double fB = 3.5 * (0.888 * A - C - D) * (B + C) - C * (A - B - C);
                double fC = 5.5 * (0.0986 * A - 0.01 * (1.098 * A - B - 9 * D - E - F + G) - (B + E)) * (E + F) - E * (1.098 * A - B - 9 * D - E - F + G);
                double fD = 3.5 * (0.986 * A - 9 * D - F - 0.0986 * A + 0.01 * (1.098 * A - B - 9 * D - E - F + G)) * (E + F) - F * (1.098 * A - B - 9 * D - E - F + G);
                double fE = 2056.4 * Math.Pow(0.0986 * A - D, 2) - D * Math.Pow(A - B - C, 2);
                double fF = 0.61177 - (A * 0.0986 - D) + 0.01 * (1.098 * A - B - 9 * D - E - F + G);
                double fG = 0.04 * (74.12 * (0.986 * A - 10 * D) + 222.24 * (0.0986 * A - D) + 18 * (D - B) + 278.84 * D + 98.09 * 0.0136 * A) / 98.01 - 0.0136 * A - G;
                return new[] { fA, fB, fC, fD, fE, fF, fG };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 80, 7, 60, 7, 0.5, 4, 0.06 }, 1e-11);
            Assert.AreEqual(80.5396204210231, r[0], 1e-9);
            Assert.AreEqual(6.9730107905982, r[1], 1e-8);
            Assert.AreEqual(61.1233326471120, r[2], 1e-8);
            Assert.AreEqual(7.2046801639777, r[3], 1e-8);
            Assert.AreEqual(0.5477285762994, r[4], 1e-5);
            Assert.AreEqual(3.6537136469003, r[5], 1e-12);
            Assert.AreEqual(0.0597122208353, r[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-11);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-11);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
        }

        [Test]
        public void Seveneq3a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Seveneq3a.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double q01 = xa[0];
                double q12 = xa[1];
                double q13 = xa[2];
                double q24 = xa[3];
                double q23 = xa[4];
                double q34 = xa[5];
                double q45 = xa[6];
                const double fF = 0.005;
                const double rho = 997.08;
                const double D = 0.154;
                const double D5 = D * D * D * D * D;
                const double pi = 3.1416;
                const double pi2 = pi * pi;
                const double deltaPUMP = -15.0e5;
                const double k01 = 32 * fF * rho * 100 / (pi2 * D5);
                const double k12 = 32 * fF * rho * 300 / (pi2 * D5);
                const double k24 = 32 * fF * rho * 1200 / (pi2 * D5);
                const double k45 = 32 * fF * rho * 300 / (pi2 * D5);
                const double k13 = 32 * fF * rho * 1200 / (pi2 * D5);
                const double k23 = 32 * fF * rho * 300 / (pi2 * D5);
                const double k34 = 32 * fF * rho * 1200 / (pi2 * D5);
                double fq01 = q01 - q12 - q13;
                double fq12 = q12 - q24 - q23;
                double fq13 = q23 + q13 - q34;
                double fq24 = q24 + q34 - q45;
                double fq23 = k01 * q01 * q01 + k12 * q12 * q12 + k24 * q24 * q24 + k45 * q45 * q45 + deltaPUMP;
                double fq34 = k13 * q13 * q13 - k23 * q23 * q23 - k12 * q12 * q12;
                double fq45 = k23 * q23 * q23 + k34 * q34 * q34 - k24 * q24 * q24;
                return new[] { fq01, fq12, fq13, fq24, fq23, fq34, fq45 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 }, 1e-9);
            Assert.AreEqual(0.098136927428176, r[0], 1e-8);
            Assert.AreEqual(0.064819554408160, r[1], 1e-8);
            Assert.AreEqual(0.033317373020015, r[2], 1e-8);
            Assert.AreEqual(0.049372394599739, r[3], 1e-8);
            Assert.AreEqual(0.015447159808421, r[4], 1e-8);
            Assert.AreEqual(0.048764532828436, r[5], 1e-8);
            Assert.AreEqual(0.098136927428176, r[6], 1e-8);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-10);
            Assert.AreEqual(0, fa1(r)[6], 1e-9);
        }

        [Test]
        public void Seveneq3b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Seveneq3b.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double q01 = xa[0];
                double q12 = xa[1];
                double q13 = xa[2];
                double q24 = xa[3];
                double q23 = xa[4];
                double q34 = xa[5];
                double q45 = xa[6];
                const double rho = 997.08;
                const double D = 0.154;
                const double D5 = D * D * D * D * D;
                const double pi = 3.1416;
                const double pi2 = pi * pi;
                const double mu = 8.937e-4;
                const double deltaPUMP = -15.0e5;
                const double ed = 4.6e-5 / D;
                double fF01 = 1 / (16 * Math.Pow(Math.Log10(ed / 3.7 - 5.02 * pi * D * mu / (4 * q01 * rho) * Math.Log10(ed / 3.7 + 14.5 * pi * D * mu / (4 * q01 * mu))), 2));
                double fF12 = 1 / (16 * Math.Pow(Math.Log10(ed / 3.7 - 5.02 * pi * D * mu / (4 * q12 * rho) * Math.Log10(ed / 3.7 + 14.5 * pi * D * mu / (4 * q12 * rho))), 2));
                double fF24 = 1 / (16 * Math.Pow(Math.Log10(ed / 3.7 - 5.02 * pi * D * mu / (4 * q24 * rho) * Math.Log10(ed / 3.7 + 14.5 * pi * D * mu / (4 * q24 * rho))), 2));
                double fF45 = 1 / (16 * Math.Pow(Math.Log10(ed / 3.7 - 5.02 * pi * D * mu / (4 * q45 * rho) * Math.Log10(ed / 3.7 + 14.5 * pi * D * mu / (4 * q45 * rho))), 2));
                double fF13 = 1 / (16 * Math.Pow(Math.Log10(ed / 3.7 - 5.02 * pi * D * mu / (4 * q13 * rho) * Math.Log10(ed / 3.7 + 14.5 * pi * D * mu / (4 * q13 * rho))), 2));
                double fF23 = 1 / (16 * Math.Pow(Math.Log10(ed / 3.7 - 5.02 * pi * D * mu / (4 * q23 * rho) * Math.Log10(ed / 3.7 + 14.5 * pi * D * mu / (4 * q23 * rho))), 2));
                double fF34 = 1 / (16 * Math.Pow(Math.Log10(ed / 3.7 - 5.02 * pi * D * mu / (4 * q34 * rho) * Math.Log10(ed / 3.7 + 14.5 * pi * D * mu / (4 * q34 * rho))), 2));
                double k01 = 32 * fF01 * rho * 100 / (pi2 * D5);
                double k12 = 32 * fF12 * rho * 300 / (pi2 * D5);
                double k24 = 32 * fF24 * rho * 1200 / (pi2 * D5);
                double k45 = 32 * fF45 * rho * 300 / (pi2 * D5);
                double k13 = 32 * fF13 * rho * 1200 / (pi2 * D5);
                double k23 = 32 * fF23 * rho * 300 / (pi2 * D5);
                double k34 = 32 * fF34 * rho * 1200 / (pi2 * D5);
                double fq01 = q01 - q12 - q13;
                double fq12 = q12 - q24 - q23;
                double fq13 = q23 + q13 - q34;
                double fq24 = q24 + q34 - q45;
                double fq23 = k01 * q01 * q01 + k12 * q12 * q12 + k24 * q24 * q24 + k45 * q45 * q45 + deltaPUMP;
                double fq34 = k13 * q13 * q13 - k23 * q23 * q23 - k12 * q12 * q12;
                double fq45 = k23 * q23 * q23 + k34 * q34 * q34 - k24 * q24 * q24;
                return new[] { fq01, fq12, fq13, fq24, fq23, fq34, fq45 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 }, 1e-10);
            Assert.AreEqual(0.110237410418775, r[0], 1e-8);
            Assert.AreEqual(0.073312448014583, r[1], 1e-8);
            Assert.AreEqual(0.036924962404192, r[2], 1e-8);
            Assert.AreEqual(0.055534266884471, r[3], 1e-8);
            Assert.AreEqual(0.017778181130112, r[4], 1e-8);
            Assert.AreEqual(0.054703143534304, r[5], 1e-8);
            Assert.AreEqual(0.110237410418774, r[6], 1e-8);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-10);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
        }

        [Test]
        public void Seveneq4()
        {
            // Test case from http://www.polymath-software.com/library/nle/Seveneq4.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double q12 = xa[0];
                double q14 = xa[1];
                double q23 = xa[2];
                double q43 = xa[3];
                double q35 = xa[4];
                double q65 = xa[5];
                double q46 = xa[6];
                const double fc = 8.7e-4;
                double dp12 = fc * 2000 * q12 * q12 / Math.Pow(12, 5);
                double dp23 = fc * 1000 * q23 * q23 / Math.Pow(8, 5);
                double dp43 = fc * 2000 * q43 * q43 / Math.Pow(8, 5);
                double dp35 = fc * 1000 * q35 * q35 / Math.Pow(6, 5);
                double fq12 = 1400 - q12 - q14;
                double fq14 = dp12 + dp23 - dp43 - fc * q14 * q14 * 1000 / Math.Pow(8, 5);
                double fq23 = q12 - 420 - q23;
                double fq43 = q43 + q23 - 420 - q35;
                double fq35 = q35 + q65 - 420;
                double fq65 = q46 - 140 - q65;
                double fq46 = dp43 + dp35 - fc * 0.2572 * q65 * q65 - fc * 0.1286 * q46 * q46;
                return new[] { fq12, fq14, fq23, fq43, fq35, fq65, fq46 };
            };

            double[] r = Broyden.FindRoot(fa1, new double[] { 1000, 500, 600, 200, 300, 100, 400 }, 1e-12);
            Assert.AreEqual(872.751598594702, r[0], 1e-5);
            Assert.AreEqual(527.248401405298, r[1], 1e-5);
            Assert.AreEqual(452.751598594702, r[2], 1e-5);
            Assert.AreEqual(252.590830181724, r[3], 1e-5);
            Assert.AreEqual(285.342428776427, r[4], 1e-5);
            Assert.AreEqual(134.657571223573, r[5], 1e-5);
            Assert.AreEqual(274.657571223573, r[6], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
        }

        [Test]
        public void Nineq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/Nineq1.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double x11 = xa[0];
                double x12 = xa[1];
                double x13 = xa[2];
                double x21 = xa[3];
                double x22 = xa[4];
                double x23 = xa[5];
                double V = xa[6];
                double L1 = xa[7];
                double L2 = xa[8];
                const double T = 63.7;
                const double A12 = 1.6 - 217 / (T + 273);
                const double A21 = 3.125 - 855 / (T + 273);
                const double A13 = 0.63 + 23.3 / (T + 273);
                const double A31 = 0.33 + 23.3 / (273 + T);
                const double A23 = 1.54128 + 554 / (T + 273);
                const double A32 = -2.2534 + 1449 / (T + 273);
                double g11 = Math.Pow(10, x12 * x12 * (A12 + 2 * x11 * (A21 - A12)) + x13 * x13 * (A13 + 2 * x11 * (A31 - A13)) + x12 * x13 * (0.5 * (A21 + A12 + A31 + A13 - A23 - A32) + x11 * (A21 - A12 + A31 - A13) + (x12 - x13) * (A23 - A32) - (1 - 2 * x11) * 0.25));
                double g21 = Math.Pow(10, x22 * x22 * (A12 + 2 * x21 * (A21 - A12)) + x23 * x23 * (A13 + 2 * x21 * (A31 - A13)) + x22 * x23 * (0.5 * (A21 + A12 + A31 + A13 - A23 - A32) + x21 * (A21 - A12 + A31 - A13) + (x22 - x23) * (A23 - A32) - (1 - 2 * x21) * 0.25));
                double g22 = Math.Pow(10, x23 * x23 * (A23 + 2 * x22 * (A31 - A23)) + x21 * x21 * (A21 + 2 * x22 * (A12 - A21)) + x23 * x21 * (0.5 * (A32 + A23 + A12 + A21 - A31 - A13) + x22 * (A32 - A23 + A12 - A21) + (x23 - x21) * (A31 - A13) - (1 - 2 * x22) * 0.25));
                double g12 = Math.Pow(10, x13 * x13 * (A23 + 2 * x12 * (A32 - A23)) + x11 * x11 * (A21 + 2 * x12 * (A12 - A21)) + x13 * x11 * (0.5 * (A32 + A23 + A12 + A21 - A31 - A13) + x12 * (A32 - A23 + A12 - A21) + (x13 - x11) * (A31 - A13) - (1 - 2 * x12) * 0.25));
                double g13 = Math.Pow(10, x11 * x11 * (A31 + 2 * x13 * (A13 - A31)) + x12 * x12 * (A32 + 2 * x13 * (A23 - A32)) + x11 * x12 * (0.5 * (A13 + A31 + A23 + A32 - A12 - A21) + x13 * (A13 - A31 + A23 - A32) + (x11 - x13) * (A12 - A21) - (1 - 2 * x13) * 0.25));
                double g23 = Math.Pow(10, x21 * x21 * (A31 + 2 * x23 * (A13 - A31)) + x22 * x22 * (A32 + 2 * x23 * (A23 - A32)) + x21 * x22 * (0.5 * (A13 + A31 + A23 + A32 - A12 - A21) + x23 * (A13 - A31 + A23 - A32) + (x21 - x23) * (A12 - A21) - (1 - 2 * x23) * 0.25));
                double k12 = Math.Pow(10, 6.90565 - 1211.033 / (T + 220.79)) * g12 / 760;
                double k22 = Math.Pow(10, 6.90565 - 1211.033 / (T + 220.79)) * g22 / 760;
                double k11 = Math.Pow(10, 8.21337 - 1652.05 / (T + 231.48)) * g11 / 760;
                double k21 = Math.Pow(10, 8.21337 - 1652.05 / (T + 231.48)) * g21 / 760;
                double k13 = Math.Pow(10, 8.10765 - 1750.286 / (T + 235)) * g13 / 760;
                double k23 = Math.Pow(10, 8.10765 - 1750.286 / (T + 235)) * g23 / 760;
                double fx11 = x11 * (L1 + L2 * k11 / k21 + V * k11) - 0.23;
                double fx12 = x12 * (L1 + L2 * k12 / k22 + V * k12) - 0.27;
                double fx13 = x13 * (L1 + L2 * k13 / k23 + V * k13) - 0.5;
                double fx21 = x21 - x11 * k11 / k21;
                double fx22 = x22 - x12 * k12 / k22;
                double fx23 = x23 - x13 * k13 / k23;
                double fV = x11 + x12 + x13 - 1;
                double fL1 = L1 + L2 + V - 1;
                double fL2 = x21 + x22 + x23 - 1;
                return new[] { fx11, fx12, fx13, fx21, fx22, fx23, fV, fL1, fL2 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.3, 0, 0.7, 0, 1, 0, 0.4, 0.55, 0.06 }, 1e-14);
            Assert.AreEqual(0.2253818399616, r[0], 1e-5);
            Assert.AreEqual(0.0042438488738, r[1], 1e-5);
            Assert.AreEqual(0.7703743111646, r[2], 1e-5);
            Assert.AreEqual(0.0960474647746, r[3], 1e-5);
            Assert.AreEqual(0.8916699617303, r[4], 1e-5);
            Assert.AreEqual(0.0122825734951, r[5], 1e-5);
            Assert.AreEqual(0.3838837845197, r[6], 1e-5);
            Assert.AreEqual(0.5483053023942, r[7], 1e-5);
            Assert.AreEqual(0.0678109130861, r[8], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[7], 1e-12);
            Assert.AreEqual(0, fa1(r)[8], 1e-12);
        }

        [Test]
        public void Teneq1a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Teneq1a.htm
            // method fails if started too far away from the root
            Func<double[], double[]> fa1 = xa =>
            {
                double x1 = xa[0];
                double x2 = xa[1];
                double x3 = xa[2];
                double x4 = xa[3];
                double x5 = xa[4];
                double x6 = xa[5];
                double x7 = xa[6];
                double x8 = xa[7];
                double x9 = xa[8];
                double x10 = xa[9];
                const double R = 10;
                double xs = x1 + x2 + x3 + x4 + x5 + x6 + x7 + x8 + x9 + x10;
                double fx1 = x1 + x4 - 3;
                double fx2 = 2 * x1 + x2 + x4 + x7 + x8 + x9 + 2 * x10 - R;
                double fx3 = 2 * x2 + 2 * x5 + x6 + x7 - 8;
                double fx4 = 2 * x3 + x5 - 4 * R;
                double fx5 = x1 * x5 - 0.193 * x2 * x4;
                double fx6 = x6 * Math.Sqrt(x2) - 0.002597 * Math.Sqrt(x2 * x4 * xs);
                double fx7 = x7 * Math.Sqrt(x4) - 0.003448 * Math.Sqrt(x1 * x4 * xs);
                double fx8 = x8 * x4 - 1.799e-5 * x2 * xs;
                double fx9 = x9 * x4 - 0.0002155 * x1 * Math.Sqrt(x3 * xs);
                double fx10 = x10 * x4 * x4 - 3.846e-5 * x4 * x4 * xs;
                return new[] { fx1, fx2, fx3, fx4, fx5, fx6, fx7, fx8, fx9, fx10 };
            };

            Assert.That(() => Broyden.FindRoot(fa1, new double[] { 1, 1, 10, 1, 1, 1, 0, 0, 0, 0 }, 1e-1), Throws.TypeOf<NonConvergenceException>());
            double[] r = Broyden.FindRoot(fa1, new[] { 3, 4, 20, 0.1, 0.1, 0.01, 0.01, 0.01, 0.1, 0.001 }, 1e-14);
            Assert.AreEqual(2.88010599840556, r[0], 1e-5);
            Assert.AreEqual(3.95067493980017, r[1], 1e-5);
            Assert.AreEqual(19.9841296101664, r[2], 1e-5);
            Assert.AreEqual(0.11989400159444, r[3], 1e-5);
            Assert.AreEqual(0.03174077966720, r[4], 1e-5);
            Assert.AreEqual(0.00468458194156, r[5], 1e-5);
            Assert.AreEqual(0.03048397912369, r[6], 1e-5);
            Assert.AreEqual(0.01608812124188, r[7], 1e-5);
            Assert.AreEqual(0.12055939838135, r[8], 1e-5);
            Assert.AreEqual(0.00104378152368, r[9], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[7], 1e-12);
            Assert.AreEqual(0, fa1(r)[8], 1e-12);
            Assert.AreEqual(0, fa1(r)[9], 1e-12);
        }

        [Test]
        public void Teneq1b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Teneq1b.htm
            // method fails if started too far away from the root
            Func<double[], double[]> fa1 = xa =>
            {
                double x1 = xa[0];
                double x2 = xa[1];
                double x3 = xa[2];
                double x4 = xa[3];
                double x5 = xa[4];
                double x6 = xa[5];
                double x7 = xa[6];
                double x8 = xa[7];
                double x9 = xa[8];
                double x10 = xa[9];
                const double R = 40;
                double xs = x1 + x2 + x3 + x4 + x5 + x6 + x7 + x8 + x9 + x10;
                double fx1 = x1 + x4 - 3;
                double fx2 = 2 * x1 + x2 + x4 + x7 + x8 + x9 + 2 * x10 - R;
                double fx3 = 2 * x2 + 2 * x5 + x6 + x7 - 8;
                double fx4 = 2 * x3 + x5 - 4 * R;
                double fx5 = x1 * x5 - 0.193 * x2 * x4;
                double fx6 = x6 * Math.Sqrt(x2) - 0.002597 * Math.Sqrt(x2 * x4 * xs);
                double fx7 = x7 * Math.Sqrt(x4) - 0.003448 * Math.Sqrt(x1 * x4 * xs);
                double fx8 = x8 * x4 - 1.799e-5 * x2 * xs;
                double fx9 = x9 * x4 - 0.0002155 * x1 * Math.Sqrt(x3 * xs);
                double fx10 = x10 * x4 * x4 - 3.846e-5 * x4 * x4 * xs;
                return new[] { fx1, fx2, fx3, fx4, fx5, fx6, fx7, fx8, fx9, fx10 };
            };

            Assert.That(() => Broyden.FindRoot(fa1, new double[] { 2, 5, 80, 1, 0, 0, 0, 0, 20, 5 }, 1e-5), Throws.TypeOf<NonConvergenceException>());
            double[] r = Broyden.FindRoot(fa1, new[] { 3, 4, 80, 0.001, 0.001, 0.001, 0.01, 4, 26, 0.01 }, 1e-14);
            Assert.AreEqual(2.99763549788728, r[0], 1e-5);
            Assert.AreEqual(3.96642685827836, r[1], 1e-5);
            Assert.AreEqual(79.9996980829447, r[2], 1e-5);
            Assert.AreEqual(0.00236450211272, r[3], 1e-5);
            Assert.AreEqual(0.00060383411050, r[4], 1e-5);
            Assert.AreEqual(0.00136594705508, r[5], 1e-5);
            Assert.AreEqual(0.06457266816721, r[6], 1e-5);
            Assert.AreEqual(3.53081557628766, r[7], 1e-5);
            Assert.AreEqual(26.43154979533460, r[8], 1e-5);
            Assert.AreEqual(0.00449980202242, r[9], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[7], 1e-12);
            Assert.AreEqual(0, fa1(r)[8], 1e-12);
            Assert.AreEqual(0, fa1(r)[9], 1e-12);
        }

        [Test]
        public void Teneq2a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Teneq2a.htm
            // method fails if started too far away from the root
            Func<double[], double[]> fa1 = xa =>
            {
                double n1 = xa[0];
                double n2 = xa[1];
                double n3 = xa[2];
                double n4 = xa[3];
                double n5 = xa[4];
                double n6 = xa[5];
                double n7 = xa[6];
                double n8 = xa[7];
                double n9 = xa[8];
                double n10 = xa[9];
                double nt = n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10;
                const double K5 = 0.193;
                const double K6 = 2.597e-3;
                const double K7 = 3.448e-3;
                const double K8 = 1.799e-5;
                const double K9 = 2.155e-4;
                const double K10 = 3.846e-5;
                const double R = 10;
                const double p = 40;
                double fn1 = n1 + n4 - 3;
                double fn2 = 2 * n1 + n2 + n4 + n7 + n8 + n9 + 2 * n10 - R;
                double fn3 = 2 * n2 + 2 * n5 + n6 + n7 - 8;
                double fn4 = 2 * n3 + n9 - 4 * R;
                double fn5 = K5 * n2 * n4 - n1 * n5;
                double fn6 = K6 * Math.Sqrt(n2 * n4) - Math.Sqrt(n1) * n6 * Math.Sqrt(p / nt);
                double fn7 = K7 * Math.Sqrt(n1 * n2) - Math.Sqrt(n4) * n7 * Math.Sqrt(p / nt);
                double fn8 = K8 * n1 - n4 * n8 * (p / nt);
                double fn9 = K9 * n1 * Math.Sqrt(n3) - n4 * n9 * Math.Sqrt(p / nt);
                double fn10 = K10 * n1 * n1 - n4 * n4 * n10 * (p / nt);
                return new[] { fn1, fn2, fn3, fn4, fn5, fn6, fn7, fn8, fn9, fn10 };
            };

            Assert.That(() => Broyden.FindRoot(fa1, new[] { 1.5, 2, 35, 0.5, 0.05, 0.005, 0.04, 0.003, 0.02, 5 }, 1e-5), Throws.TypeOf<NonConvergenceException>());
            double[] r = Broyden.FindRoot(fa1, new[] { 3, 4, 20, 0.1, 0.01, 0.001, 0.04, 0.003, 0.03, 0.03 }, 1e-14);
            Assert.AreEqual(2.91572542389522, r[0], 1e-5);
            Assert.AreEqual(3.96094281080888, r[1], 1e-5);
            Assert.AreEqual(19.9862916465515, r[2], 1e-5);
            Assert.AreEqual(8.42745761047765E-02, r[3], 1e-5);
            Assert.AreEqual(2.20956017698935E-02, r[4], 1e-5);
            Assert.AreEqual(7.22766590884200E-04, r[5], 1e-5);
            Assert.AreEqual(3.32004082515745E-02, r[6], 1e-5);
            Assert.AreEqual(4.21099693391800E-04, r[7], 1e-5);
            Assert.AreEqual(2.74167068969179E-02, r[8], 1e-5);
            Assert.AreEqual(3.11467752270064E-02, r[9], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[7], 1e-12);
            Assert.AreEqual(0, fa1(r)[8], 1e-12);
            Assert.AreEqual(0, fa1(r)[9], 1e-12);
        }

        [Test]
        public void Teneq2b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Teneq2b.htm
            // Found solution disagrees with testcase!? Method fails if started too far away from the root.
            Func<double[], double[]> fa1 = xa =>
            {
                double n1 = xa[0];
                double n2 = xa[1];
                double n3 = xa[2];
                double n4 = xa[3];
                double n5 = xa[4];
                double n6 = xa[5];
                double n7 = xa[6];
                double n8 = xa[7];
                double n9 = xa[8];
                double n10 = xa[9];
                double nt = n1 + n2 + n3 + n4 + n5 + n6 + n7 + n8 + n9 + n10;
                const double K5 = 0.193;
                const double K6 = 2.597e-3;
                const double K7 = 3.448e-3;
                const double K8 = 1.799e-5;
                const double K9 = 2.155e-4;
                const double K10 = 3.846e-5;
                const double R = 40;
                const double p = 40;
                double fn1 = n1 + n4 - 3;
                double fn2 = 2 * n1 + n2 + n4 + n7 + n8 + n9 + 2 * n10 - R;
                double fn3 = 2 * n2 + 2 * n5 + n6 + n7 - 8;
                double fn4 = 2 * n3 + n9 - 4 * R;
                double fn5 = K5 * n2 * n4 - n1 * n5;
                double fn6 = K6 * Math.Sqrt(n2 * n4) - Math.Sqrt(n1) * n6 * Math.Sqrt(p / nt);
                double fn7 = K7 * Math.Sqrt(n1 * n2) - Math.Sqrt(n4) * n7 * Math.Sqrt(p / nt);
                double fn8 = K8 * n1 - n4 * n8 * (p / nt);
                double fn9 = K9 * n1 * Math.Sqrt(n3) - n4 * n9 * Math.Sqrt(p / nt);
                double fn10 = K10 * n1 * n1 - n4 * n4 * n10 * (p / nt);
                return new[] { fn1, fn2, fn3, fn4, fn5, fn6, fn7, fn8, fn9, fn10 };
            };

            Assert.That(() => Broyden.FindRoot(fa1, new[] { 1.5, 2, 35, 0.5, 0.05, 0.005, 0.04, 0.003, 0.02, 5 }, 1e-5), Throws.TypeOf<NonConvergenceException>());
            double[] r = Broyden.FindRoot(fa1, new[] { 3, 4, 80, 0.01, 0.002, 0.0006, 0.1, 0.004, 0.1, 15 }, 1e-14);
            //Assert.AreEqual(2.91572542389522, r[0], 1e-5);
            //Assert.AreEqual(3.96094281080888, r[1], 1e-5);
            //Assert.AreEqual(19.9862916465515, r[2], 1e-5);
            //Assert.AreEqual(8.42745761047765E-02, r[3], 1e-5);
            //Assert.AreEqual(2.20956017698935E-02, r[4], 1e-5);
            //Assert.AreEqual(7.22766590884200E-04, r[5], 1e-5);
            //Assert.AreEqual(3.32004082515745E-02, r[6], 1e-5);
            //Assert.AreEqual(4.21099693391800E-04, r[7], 1e-5);
            //Assert.AreEqual(2.74167068969179E-02, r[8], 1e-5);
            //Assert.AreEqual(3.11467752270064E-02, r[9], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[7], 1e-12);
            Assert.AreEqual(0, fa1(r)[8], 1e-12);
            Assert.AreEqual(0, fa1(r)[9], 1e-12);
        }

        [Test]
        public void Teneq3()
        {
            // Test case from http://www.polymath-software.com/library/nle/Teneq3.htm
            // method fails if started too far away from the root
            Func<double[], double[]> fa1 = xa =>
            {
                double x1 = xa[0];
                double x2 = xa[1];
                double x3 = xa[2];
                double x4 = xa[3];
                double x5 = xa[4];
                double x6 = xa[5];
                double x7 = xa[6];
                double x8 = xa[7];
                double x9 = xa[8];
                double x10 = xa[9];
                double tau = 22.5 / (x3 + x4);
                const double kA = 0.08;
                const double kB = 0.03;
                const double xAinf = kB / (kA + kB);
                const double KA = 0.4409;
                const double KB = 1.8299;
                double fx1 = x3 - x1 - x9;
                double fx2 = x4 - x2 - x10;
                double fx3 = (x3 / (x3 + x4) - xAinf) * Math.Exp(-(kA + kB) * tau) + xAinf - x5 / (x5 + x6);
                double fx4 = x3 + x4 - x5 - x6;
                double fx5 = x7 / (x7 + x8) - KA * x9 / (x9 + x10);
                double fx6 = x8 / (x7 + x8) - KB * x10 / (x9 + x10);
                double fx7 = x5 - x7 - x9;
                double fx8 = x6 - x8 - x10;
                double fx9 = x1 - 0.9;
                double fx10 = x2 - 0.1;
                return new[] { fx1, fx2, fx3, fx4, fx5, fx6, fx7, fx8, fx9, fx10 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.9, 0.1, 1, 0.2, 0.5, 0.8, 0.2, 0.7, 0.2, 0.1 }, 1e-14);
            Assert.AreEqual(0.9, r[0], 1e-5);
            Assert.AreEqual(0.1, r[1], 1e-5);
            Assert.AreEqual(1.1898916711704, r[2], 1e-5);
            Assert.AreEqual(0.2952987508752, r[3], 1e-5);
            Assert.AreEqual(0.5533206920487, r[4], 1e-5);
            Assert.AreEqual(0.9318697299969, r[5], 1e-5);
            Assert.AreEqual(0.2634290208783, r[6], 1e-5);
            Assert.AreEqual(0.7365709791217, r[7], 1e-5);
            Assert.AreEqual(0.2898916711704, r[8], 1e-5);
            Assert.AreEqual(0.1952987508752, r[9], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[7], 1e-12);
            Assert.AreEqual(0, fa1(r)[8], 1e-12);
            Assert.AreEqual(0, fa1(r)[9], 1e-12);
        }

        [Test]
        public void Eleveneq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/11eq1.htm
            // Found solution disagrees with testcase!?
            Func<double[], double[]> fa1 = xa =>
            {
                double x1 = xa[0];
                double x2 = xa[1];
                double x3 = xa[2];
                double V = xa[3];
                double L = xa[4];
                double s11 = xa[5];
                double s12 = xa[6];
                double s13 = xa[7];
                double s61 = xa[8];
                double s62 = xa[9];
                double s63 = xa[10];
                const double s102 = 0;
                double s21 = s11 / (1 + 0.211 * 1.5);
                double s71 = L * x1;
                double s72 = L * x2;
                double s73 = L * x3;
                double s41 = V * 2.523 * x1;
                double s42 = V * 1.57 * x2;
                double s43 = V * 0.0329 * x3;
                double s51 = 0.75 * s41;
                double s52 = 0.75 * s42;
                double s53 = 0.75 * s43;
                double s81 = 0.1 * s71;
                double s101 = 0.9 * 0.1 * s71;
                double s82 = 0.5 * s72;
                double s83 = 0.2 * s73;
                double s103 = 0.8 * 0.4 * s73;
                double s22 = s12 / (1 + 0.101 * 1.5) + 0.211 * s21 * 1.5 / (1 + 0.101 * 1.5);
                double s31 = s21 / (1 + 0.44 * 2);
                double s23 = s13 + 0.101 * 1.5 * s22;
                double s32 = s22 / (1 + 0.219 * 2) + 0.44 * s31 * 2 / (1 + 0.219 * 2);
                double s33 = s23 + 0.219 * 2 * s32;
                double fx1 = s61 - x1 * (L + V * 2.523);
                double fx2 = s62 - x2 * (L + V * 1.57);
                double fx3 = s63 - x3 * (L + V * 0.0329);
                double fV = 2.523 * x1 + 1.57 * x2 + 0.0329 * x3 - 1;
                double fL = x1 + x2 + x3 - 1;
                double fs11 = s11 - 970 - s51;
                double fs12 = s12 - 30 - s52;
                double fs13 = s13 - s53;
                double fs61 = s81 + s101 + s31 - s61;
                double fs62 = s82 + s102 + s32 - s62;
                double fs63 = s83 + s103 + s33 - s63;
                return new[] { fx1, fx2, fx3, V, L, s11, s12, s13, s61, s62, s63 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.1, 0.2, 0, 500, 500, 970, 30, 0, 970, 30, 0 }, 1e-14);
            //Assert.AreEqual(0.16547930319775, r[0], 1e-5);
            //Assert.AreEqual(0.36109556119140, r[1], 1e-5);
            //Assert.AreEqual(0.4734251356109, r[2], 1e-5);
            //Assert.AreEqual(630.763337421609, r[3], 1e-5);
            //Assert.AreEqual(1554.563311908010, r[4], 1e-5);
            //Assert.AreEqual(1167.509795711420, r[5], 1e-5);
            //Assert.AreEqual(298.194278136889, r[6], 1e-5);
            //Assert.AreEqual(7.368429217898, r[7], 1e-5);
            //Assert.AreEqual(520.594447913209, r[8], 1e-5);
            //Assert.AreEqual(918.938282370162, r[9], 1e-5);
            //Assert.AreEqual(745.793919046246, r[10], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[7], 1e-12);
            Assert.AreEqual(0, fa1(r)[8], 1e-12);
            Assert.AreEqual(0, fa1(r)[9], 1e-12);
            Assert.AreEqual(0, fa1(r)[10], 1e-12);
        }

        [Test]
        public void Thirteeneq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/13eq1.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double v1 = xa[0];
                double v2 = xa[1];
                double v3 = xa[2];
                double v4 = xa[3];
                double v5 = xa[4];
                double v6 = xa[5];
                double v7 = xa[6];
                double v8 = xa[7];
                double v9 = xa[8];
                double v10 = xa[9];
                double v11 = xa[10];
                double v12 = xa[11];
                double v13 = xa[12];
                double xs = v1 + v2 + v3 + v4 + v5 + v6 + v7 + v8 + v9 + v10;
                double fv1 = -6.089 + Math.Log(v1 / xs) + v11;
                double fv2 = -17.164 + Math.Log(v2 / xs) + 2 * v11;
                double fv3 = -34.054 + Math.Log(v3 / xs) + 2 * v11 + v13;
                double fv4 = -5.914 + Math.Log(v4 / xs) + v12;
                double fv5 = -24.721 + Math.Log(v5 / xs) + 2 * v12;
                double fv6 = -14.986 + Math.Log(v6 / xs) + v11 + v12;
                double fv7 = -24.1 + Math.Log(v7 / xs) + v12 + v13;
                double fv8 = -10.708 + Math.Log(v8 / xs) + v13;
                double fv9 = -26.662 + Math.Log(v9 / xs) + 2 * v13;
                double fv10 = -22.197 + Math.Log(v10 / xs) + v11 + v13;
                double fv11 = v1 + 2 * v2 + 2 * v3 + v6 + v10 - 2;
                double fv12 = v4 + 2 * v5 + v6 + v7 - 1;
                double fv13 = v3 + v7 + v8 + 2 * v9 + v10 - 1;
                return new[] { fv1, fv2, fv3, fv4, fv5, fv6, fv7, fv8, fv9, fv10, fv11, fv12, fv13 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.05, 0.2, 0.8,0.001, 0.5, 0.0007, 0.03, 0.02, 0.1, 0.1, 10, 10, 10 }, 1e-13);
            Assert.AreEqual(0.04070664967202, r[0], 1e-5);
            Assert.AreEqual(0.14796418434584, r[1], 1e-5);
            Assert.AreEqual(0.78211670254494, r[2], 1e-5);
            Assert.AreEqual(0.00141449528575, r[3], 1e-5);
            Assert.AreEqual(0.48528331804737, r[4], 1e-5);
            Assert.AreEqual(0.00069374665473, r[5], 1e-5);
            Assert.AreEqual(0.02732512196478, r[6], 1e-5);
            Assert.AreEqual(0.01790081773259, r[7], 1e-5);
            Assert.AreEqual(0.03710976393300, r[8], 1e-5);
            Assert.AreEqual(0.09843782989168, r[9], 1e-5);
            Assert.AreEqual(9.78442121489321, r[10], 1e-5);
            Assert.AreEqual(12.9690398976484, r[11], 1e-5);
            Assert.AreEqual(15.2249662814130, r[12], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[7], 1e-12);
            Assert.AreEqual(0, fa1(r)[8], 1e-12);
            Assert.AreEqual(0, fa1(r)[9], 1e-12);
            Assert.AreEqual(0, fa1(r)[10], 1e-12);
            Assert.AreEqual(0, fa1(r)[11], 1e-12);
            Assert.AreEqual(0, fa1(r)[12], 1e-12);
        }

        [Test]
        public void Fourteeneq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/14eq1.htm
            Func<double[], double[]> fa1 = xa =>
            {
                double x11 = xa[0];
                double x12 = xa[1];
                double x13 = xa[2];
                double x21 = xa[3];
                double x22 = xa[4];
                double x23 = xa[5];
                double t1 = xa[6];
                double t2 = xa[7];
                double t3 = xa[8];
                double tf = xa[9];
                double t0 = xa[10];
                double V1 = xa[11];
                double V2 = xa[12];
                double V3 = xa[13];
                const double F = 1;
                const double B = 0.75;
                const double D = 0.25;
                double L0 = V1 - D;
                double L1 = V2 - D;
                double L2 = V3 + F - D;
                const double L3 = B;
                const double P = 760 * 120 / 14.7;
                double k11 = Math.Pow(10 ,6.80776 - 935.77 / ((t1 - 32) * 5 / 9 + 238.789)) / P;
                double k12 = Math.Pow(10, 6.80776 - 935.77 / ((t2 - 32) * 5 / 9 + 238.789)) / P;
                double k13 = Math.Pow(10, 6.80776 - 935.77 / ((t3 - 32) * 5 / 9 + 238.789)) / P;
                double k21 = Math.Pow(10, 6.85296 - 1064.84 / ((t1 - 32) * 5 / 9 + 232.012)) / P;
                double k22 = Math.Pow(10, 6.85296 - 1064.84 / ((t2 - 32) * 5 / 9 + 232.012)) / P;
                double k23 = Math.Pow(10, 6.85296 - 1064.84 / ((t3 - 32) * 5 / 9 + 232.012)) / P;
                double k1f = Math.Pow(10, 6.80776 - 935.77 / ((tf - 32) * 5 / 9 + 238.789)) / P;
                double k2f = Math.Pow(10, 6.85296 - 1064.84 / ((tf - 32) * 5 / 9 + 232.012)) / P;
                double k10 = Math.Pow(10, 6.80776 - 935.77 / ((t0 - 32) * 5 / 9 + 238.789)) / P;
                double k20 = Math.Pow(10, 6.85296 - 1064.84 / ((tf - 32) * 5 / 9 + 232.012)) / P;
                double hl1 = t1 * (29.6 + 0.04 * t1) * x11 + t1 * (38.5 + 0.025 * t1) * x21;
                double hv1 = (8003 + t1 * (43.8 - 0.04 * t1)) * k11 * x11 + (12004 + t1 * (31.7 + 0.007 * t1)) * k21 * x21;
                double hl2 = t2 * (29.6 + 0.04 * t2) * x12 + t2 * (38.5 + 0.025 * t2) * x22;
                double hv2 = (8003 + t2 * (43.8 - 0.04 * t2)) * k12 * x12 + (12004 + t2 * (31.7 + 0.007 * t2)) * k22 * x22;
                double hl3 = t3 * (29.6 + 0.04 * t3) * x13 + t3 * (38.5 + 0.025 * t3) * x23;
                double hv3 = (8003 + t3 * (43.8 - 0.04 * t3)) * k13 * x13 + (12004 + t3 * (31.7 + 0.007 * t3)) * k23 * x23;
                const double z1 = 0.40;
                const double z2 = 1 - z1;
                double hf = tf * (29.6 + 0.04 * tf) * z1 + tf * (38.5 + 0.025 * tf) * z2;
                double h0 = t0 * (29.6 + 0.04 * t0) * k10 * k11 * x11 + t0 * (38.5 + 0.025 * t0) * k20 * k21 * x21;
                const double Q = 10000;
                double fx11 =-((V1-L0)*k11+L1)*x11+V2*k12*x12 ;
                double fx12 = L1 * x11 - (V2 * k12 + L2) * x12 + V3 * k13 * x13 + z1 * F;
                double fx13 = L2 * x12 - (V3 * k13 + B) * x13;
                double fx21 =-((V1-L0)*k21+L1)*x21+V2*k22*x22 ;
                double fx22 = L1 * x21 - (V2 * k22 + L2) * x22 + V3 * k23 * x23 + z2 * F;
                double fx23 = L2 * x22 - (V3 * k23 + B) * x23;
                double ft1 = k11 * x11 + k21 * x21 - 1;
                double ft2 = k12 * x12 + k22 * x22 - 1;
                double ft3 = k13 * x13 + k23 * x23 - 1;
                double ftf = k1f * z1 + k2f * z2 - 1;
                double ft0 = k10 * k11 * x11 + k20 * k21 * x21 - 1;
                double fV1 = -V1 * hv1 + V2 * hv2 - L1 * hl1 + L0 * h0;
                double fV2 = -V2 * hv2 + V3 * hv3 + hf + L1 * hl1 - L2 * hl2;
                double fV3 = -V3 * hv3 + Q + L2 * hl2 - L3 * hl3;
                return new[] { fx11, fx12, fx13, fx21, fx22, fx23, ft1, ft2, ft3, ftf, ft0, fV1, fV2, fV3 };
            };

            double[] r = Broyden.FindRoot(fa1, new[] { 0.5, 0.4, 0.3, 0.3, 0.4, 0.5, 145, 190, 210, 200, 200, 1, 1, 1 }, 1e-10);
            Assert.AreEqual(0.57902594925514, r[0], 1e-5);
            Assert.AreEqual(0.39569120229748, r[1], 1e-5);
            Assert.AreEqual(0.27186578944464, r[2], 1e-5);
            Assert.AreEqual(0.42097405074479, r[3], 1e-5);
            Assert.AreEqual(0.60430879770252, r[4], 1e-5);
            Assert.AreEqual(0.72813421055536, r[5], 1e-5);
            Assert.AreEqual(186.378525897475, r[6], 1e-5);
            Assert.AreEqual(200.526868526979, r[7], 1e-5);
            Assert.AreEqual(211.486095297259, r[8], 1e-5);
            Assert.AreEqual(200.167501447086, r[9], 1e-5);
            Assert.AreEqual(169.064015709595, r[10], 1e-5);
            Assert.AreEqual(1.08137588467567, r[11], 1e-5);
            Assert.AreEqual(1.06683987591090, r[12], 1e-5);
            Assert.AreEqual(1.04952829661125, r[13], 1e-5);
            Assert.AreEqual(0, fa1(r)[0], 1e-12);
            Assert.AreEqual(0, fa1(r)[1], 1e-12);
            Assert.AreEqual(0, fa1(r)[2], 1e-12);
            Assert.AreEqual(0, fa1(r)[3], 1e-12);
            Assert.AreEqual(0, fa1(r)[4], 1e-12);
            Assert.AreEqual(0, fa1(r)[5], 1e-12);
            Assert.AreEqual(0, fa1(r)[6], 1e-12);
            Assert.AreEqual(0, fa1(r)[7], 1e-12);
            Assert.AreEqual(0, fa1(r)[8], 1e-12);
            Assert.AreEqual(0, fa1(r)[9], 1e-12);
            Assert.AreEqual(0, fa1(r)[10], 1e-12);
            Assert.AreEqual(0, fa1(r)[11], 1e-10);
            Assert.AreEqual(0, fa1(r)[12], 1e-10);
            Assert.AreEqual(0, fa1(r)[13], 1e-11);
        }
    }
}
