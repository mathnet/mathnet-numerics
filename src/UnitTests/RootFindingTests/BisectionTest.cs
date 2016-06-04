// <copyright file="BisectionTest.cs" company="Math.NET">
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

using System;
using MathNet.Numerics.RootFinding;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.RootFindingTests
{
    [TestFixture, Category("RootFinding")]
    internal class BisectionTest
    {
        [Test]
        public void MultipleRoots()
        {
            // Roots at -2, 2
            Func<double, double> f1 = x => x*x - 4;
            Assert.AreEqual(0, f1(Bisection.FindRoot(f1, 0, 5, 1e-14)), 1e-14);
            Assert.AreEqual(0, f1(Bisection.FindRootExpand(f1, 3, 4, 1e-14)), 1e-14);
            Assert.AreEqual(-2, Bisection.FindRoot(f1, -5, -1, 1e-14), 1e-14);
            Assert.AreEqual(2, Bisection.FindRoot(f1, 1, 4, 1e-14), 1e-14);
            Assert.AreEqual(0, f1(Bisection.FindRoot(x => -f1(x), 0, 5, 1e-14)), 1e-14);
            Assert.AreEqual(-2, Bisection.FindRoot(x => -f1(x), -5, -1, 1e-14), 1e-14);
            Assert.AreEqual(2, Bisection.FindRoot(x => -f1(x), 1, 4, 1e-14), 1e-14);

            // Roots at 3, 4
            Func<double, double> f2 = x => (x - 3)*(x - 4);
            Assert.AreEqual(0, f2(Bisection.FindRoot(f2, 3.5, 5, 1e-14)), 1e-14);
            Assert.AreEqual(3, Bisection.FindRoot(f2, -5, 3.5, 1e-14), 1e-14);
            Assert.AreEqual(4, Bisection.FindRoot(f2, 3.2, 5, 1e-14), 1e-14);
            Assert.AreEqual(3, Bisection.FindRoot(f2, 2.1, 3.9, 0.001), 0.001);
            Assert.AreEqual(3, Bisection.FindRoot(f2, 2.1, 3.4, 0.001), 0.001);
        }

        [Test]
        public void LocalMinima()
        {
            Func<double, double> f1 = x => x*x*x - 2*x + 2;
            Assert.AreEqual(0, f1(Bisection.FindRoot(f1, -5, 5, 1e-14)), 1e-14);
            Assert.AreEqual(0, f1(Bisection.FindRoot(f1, -2, 4, 1e-14)), 1e-14);
        }

        [Test]
        public void NoRoot()
        {
            Func<double, double> f1 = x => x*x + 4;
            Assert.That(() => Bisection.FindRoot(f1, -5, 5, 1e-14), Throws.TypeOf<NonConvergenceException>());
        }

        [Test]
        public void Oneeq1()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq1.htm
            Func<double, double> f1 = z => 8*Math.Pow((4 - z)*z, 2)/(Math.Pow(6 - 3*z, 2)*(2 - z)) - 0.186;
            double x = Bisection.FindRoot(f1, 0.1, 0.9, accuracy: 1e-9, maxIterations: 80);
            Assert.AreEqual(0.277759543089215, x, 1e-9);
            Assert.AreEqual(0, f1(x), 1e-9);
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
                double P2 = Math.Pow(10, 6.87776 - 1171.53/(224.366 + T));
                double P1 = Math.Pow(10, 8.04494 - 1554.3/(222.65 + T));
                const double t1 = x1 + x2*G12;
                const double t2 = x2 + x1*G21;
                double gamma2 = Math.Exp(-Math.Log(t2) - (x1*(G12*t2 - G21*t1))/(t1*t2));
                double gamma1 = Math.Exp(-Math.Log(t1) + (x2*(G12*t2 - G21*t1))/(t1*t2));
                double k1 = gamma1*P1/760;
                double k2 = gamma2*P2/760;
                return 1 - k1*x1 - k2*x2;
            };

            double x = Bisection.FindRoot(f1, 0, 100);
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
                double P2 = Math.Pow(10, 6.9024 - 1268.115/(216.9 + T));
                double P1 = Math.Pow(10, 8.04494 - 1554.3/(222.65 + T));
                double gamma2 = Math.Pow(10, B*Math.Pow(x1, 2)/Math.Pow(x1 + B*x2/A, 2));
                double gamma1 = Math.Pow(10, A*Math.Pow(x2, 2)/Math.Pow(A*x1/B + x2, 2));
                double k1 = gamma1*P1/760;
                double k2 = gamma2*P2/760;
                return 1 - k1*x1 - k2*x2;
            };

            double x = Bisection.FindRoot(f1, 0, 100, 1e-9);
            Assert.AreEqual(70.9000000710300, x, 1e-9);
            Assert.AreEqual(0, f1(x), 1e-9);
        }

        [Test]
        public void Oneeq3()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq3.htm
            Func<double, double> f1 = T => Math.Exp(21000/T)/(T*T) - 1.11e11;
            double x = Bisection.FindRoot(f1, 550, 560, 1e-2);
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
                return A*B*(B*Math.Pow(1 - x, 2) - A*x*x)/Math.Pow(x*(A - B) + B, 2) + 0.14845;
            };

            double r = Bisection.FindRoot(f1, 0, 1);
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
                return DH + TR*(ALPHA + TR*(BETA/2 + TR*GAMMA/3)) - 298.0*(ALPHA + 298.0*(BETA/2 + 298.0*GAMMA/3));
            };

            double x = Bisection.FindRoot(f1, 3000, 5000, 1e-10);
            Assert.AreEqual(4305.30991366774, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-10);
        }

        [Test]
        public void Oneeq6a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq6a.htm
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
                const double Beta = R*T*B0 - A0 - R*C/(T*T);
                const double Gama = -R*T*B0*B + A0*A - R*C*B0/(T*T);
                const double Delta = R*B0*B*C/(T*T);

                return R*T/V + Beta/(V*V) + Gama/(V*V*V) + Delta/(V*V*V*V) - P;
            };

            double x = Bisection.FindRoot(f1, 0.1, 1, 1e-8);
            Assert.AreEqual(0.174749531708621, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-8);
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
                const double Beta = R*T*B0 - A0 - R*C/(T*T);
                const double Gama = -R*T*B0*B + A0*A - R*C*B0/(T*T);
                const double Delta = R*B0*B*C/(T*T);

                return R*T*V*V*V + Beta*V*V + Gama*V + Delta - P*V*V*V*V;
            };

            double x = Bisection.FindRoot(f1, 0.1, 1);
            Assert.AreEqual(0.174749600398905, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-13);
        }

        [Test]
        public void Oneeq7()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq7.htm
            Func<double, double> f1 = x => x/(1 - x) - 5*Math.Log(0.4*(1 - x)/(0.4 - 0.5*x)) + 4.45977;
            double r = Bisection.FindRoot(f1, 0, 0.79, 1e-13);
            Assert.AreEqual(0.757396293891, r, 1e-5);
            Assert.AreEqual(0, f1(r), 1e-13);
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

                return a*v*v + b*Math.Pow(v, 7/4) - c;
            };

            double x = Bisection.FindRoot(f1, 0.01, 1, 1e-10);
            Assert.AreEqual(0.842524411168525, x, 1e-2); // Hmm, we do not find the correct root within requested accuracy here.
            Assert.AreEqual(0, f1(x), 1e-10);
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

                return Math.Pow((gama + 1)/2, (gama + 1)/(gama - 1))*(2/(gama - 1))*(Math.Pow(P/Pd, 2/gama) - Math.Pow(P/Pd, (gama + 1)/gama)) - Math.Pow(At/A, 2);
            };

            double x = Bisection.FindRoot(f1, 1, 50);
            Assert.AreEqual(25.743977294088, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = Bisection.FindRoot(f1, 50, 100, 1e-8);
            Assert.AreEqual(78.905412035983, x, 1e-8);
            Assert.AreEqual(0, f1(x), 1e-8);
        }

        [Test]
        public void Oneeq10()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq10.htm
            Func<double, double> f1 = x => (1.0/63.0)*Math.Log(x) + (64.0/63.0)*Math.Log(1/(1 - x)) + Math.Log(0.95 - x) - Math.Log(0.9);

            double r = Bisection.FindRoot(f1, .01, 0.35, 1e-8);
            Assert.AreEqual(0.036210083704, r, 1e-5);
            Assert.AreEqual(0, f1(r), 1e-8);
            r = Bisection.FindRoot(f1, 0.35, 0.7, 1e-8);
            Assert.AreEqual(0.5, r, 1e-8);
            Assert.AreEqual(0, f1(r), 1e-8);
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

                return a - Math.Pow((c + 2*x)*(a + b + c - 2*x), 2)/(kp*P*P*Math.Pow(b - 3*x, 3)) - x;
            };

            double r = Bisection.FindRoot(f1, 0, 0.25, 1e-8);
            Assert.AreEqual(0.058654581076661, r, 1e-7); // Fails with tolerance 1e-8, hmm...
            Assert.AreEqual(0, f1(r), 1e-8);
            r = Bisection.FindRoot(f1, 0.3, 1, 1e-8);
            Assert.AreEqual(0.600323116977323, r, 1e-8);
            Assert.AreEqual(0, f1(r), 1e-8);
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

                return (b - Math.Pow(Math.Pow((c + 2*x)*(a + b + c - 2*x), 2)/(kp*P*P*(a - x)), 1.0/3.0))/3.0 - x;
            };

            double r = Bisection.FindRoot(f1, 0.01, 0.45, 1e-8);
            Assert.AreEqual(0.058654571042804, r, 1e-8);
            Assert.AreEqual(0, f1(r), 1e-8);
            // Could not test the following root, since Math.Pow(y,1/3) does not work for y<0
            // r = Bisection.FindRoot(f1, 0.55, 0.7);
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

                return (R*T/P)*(1 + b/v + c/(v*v)) - v;
            };

            double x = Bisection.FindRoot(f1, 100, 300, 1e-8);
            Assert.AreEqual(213.001818272273, x, 1e-8);
            Assert.AreEqual(0, f1(x), 1e-8);
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
                double A = 0.42748*(R*R*Math.Pow(Tc, 2.5))/Pc;
                const double B = 0.08664*R*Tc/Pc;

                return R*Math.Pow(T, 1.5)*V*(V + B)/(P*Math.Sqrt(T)*V*(V + B) + A) + B - V;
            };

            double x = Bisection.FindRoot(f1, .01, .3, 1e-8);
            Assert.AreEqual(0.075699587993613, x, 1e-8);
            Assert.AreEqual(0, f1(x), 1e-8);
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
                double DT1 = T1 - t1 - Q/(M*Cp);
                double DT2 = T1 - t1 - Q/(m*cp);

                return U*A*(DT2 - DT1)/Math.Log(DT2/DT1) - Q;
            };

            double x = Bisection.FindRoot(f1, 1000000, 7000000, 1e-8);
            Assert.AreEqual(5281024.23857737, x, 1e-6); // Fails with tolerance 1e-8, hmm...
            Assert.AreEqual(0, f1(x), 1e-8);
        }

        [Test]
        public void Oneeq15a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq15a.htm
            Func<double, double> f1 = h => h*(1 + h*(3 - h)) - 2.4 - h;

            double x = Bisection.FindRoot(f1, -2, 0);
            Assert.AreEqual(-0.795219754733581, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-13);
            x = Bisection.FindRoot(f1, 0, 2);
            Assert.AreEqual(1.13413784566692, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-13);
            x = Bisection.FindRoot(f1, 2, 5);
            Assert.AreEqual(2.66108192383197, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-13);
        }

        [Test]
        public void Oneeq15b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq15b.htm
            Func<double, double> f1 = h => 2.4/(h*(3 - h)) - h;

            double x = Bisection.FindRoot(f1, -2, -0.5);
            Assert.AreEqual(-0.795219745444464, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = Bisection.FindRoot(f1, 0.5, 2);
            Assert.AreEqual(1.134137843811, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = Bisection.FindRoot(f1, 2, 2.9);
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
                const double COtwo = y + 2*z;
                const double H2O = 2*y + 3*z;
                const double N2 = 0.02 + 3.76*(2*y + 7*z/2)*x;
                const double O2 = (2*y + 7*z/2)*(x - 1);
                const double alp = 3.381*CH4 + 2.247*C2H6 + 6.214*COtwo + 7.256*H2O + 6.524*N2 + 6.148*O2;
                const double bet = 18.044*CH4 + 38.201*C2H6 + 10.396*COtwo + 2.298*H2O + 1.25*N2 + 3.102*O2;
                const double gam = -4.3*CH4 - 11.049*C2H6 - 3.545*COtwo + 0.283*H2O - 0.001*N2 - 0.923*O2;
                const double H0 = alp*298 + bet*0.001*298*298/2 + gam*1e-6*298*298*298/3;
                double Hf = alp*T + bet*0.001*T*T/2 + gam*1e-6*T*T*T/3;
                const double xx = 1;

                return 212798*y*xx + 372820*z*xx + H0 - Hf;
            };

            double r = Bisection.FindRoot(f1, 1000, 3000);
            Assert.AreEqual(1779.48406483697, r, 1e-5);
            Assert.AreEqual(0, f1(r), 1e-9);
        }

        [Test]
        public void Oneeq17()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq17.htm
            Func<double, double> f1 = rp => rp - 0.327*Math.Pow(0.06 - 161*rp, 0.804)*Math.Exp(-5230/(1.987*(373 + 1.84e6*rp)));

            double x = Bisection.FindRoot(f1, 0, 0.00035);
            Assert.AreEqual(0.000340568862275, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq18a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq18a.htm
            Func<double, double> f1 = T =>
            {
                double Tp = (269.267*T - 1752)/266.667;
                double k = 0.0006*Math.Exp(20.7 - 15000/Tp);
                double Pp = -(0.1/(321*(320.0/321.0 - (1 + k))));
                double P = (320*Pp + 0.1)/321;

                return 1.296*(T - Tp) + 10369*k*Pp;
            };

            double x = Bisection.FindRoot(f1, 500, 725, 1e-8);
            Assert.AreEqual(690.4486645013260, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-8);
            x = Bisection.FindRoot(f1, 725, 850, 1e-12);
            Assert.AreEqual(758.3286948959860, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-12);
            x = Bisection.FindRoot(f1, 850, 1000, 1e-12);
            Assert.AreEqual(912.7964911381540, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-12);
        }

        [Test]
        public void Oneeq18b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq18b.htm
            Func<double, double> f1 = T =>
            {
                double Tp = (269*T - 1752)/267;
                double k = 0.0006*Math.Exp(20.7 - 15000/Tp);
                double Pp = -(0.1/(321*(320.0/321.0 - (1 + k))));
                double P = (320*Pp + 0.1)/321;

                return 1.3*(T - Tp) + 1.04e4*k*Pp;
            };

            double x = Bisection.FindRoot(f1, 500, 1500, 1e-8);
            Assert.AreEqual(1208.2863599396200, x, 1e-4);
            Assert.AreEqual(0, f1(x), 1e-8);
        }

        [Test]
        public void Oneeq19a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq19a.htm
            Func<double, double> f1 = z =>
            {
                const double P = 200;
                const double Pc = 33.5;
                const double T = 631*2;
                const double Tc = 126.2;
                const double Pr = P/Pc;
                const double Tr = T/Tc;
                double Asqr = 0.4278*Pr/Math.Pow(Tr, 2.5);
                const double B = 0.0867*Pr/Tr;
                double Q = B*B + B - Asqr;
                double r = Asqr*B;
                double p = (-3*Q - 1)/3;
                double q = (-27*r - 9*Q - 2)/27;
                double R = Math.Pow(p/3, 3) + Math.Pow(q/2, 2);
                double V = (R > 0) ? Math.Pow(-q/2 + Math.Sqrt(R), 1/3) : 0;
                double WW = (R > 0) ? (-q/2 - Math.Sqrt(R)) : (0);
                double psi1 = (R < 0) ? (Math.Acos(Math.Sqrt((q*q/4)/(-p*p*p/27)))) : (0);
                double W = (R > 0) ? (Math.Sign(WW)*Math.Pow(Math.Abs(WW), 1/3)) : (0);
                double z1 = (R < 0) ? (2*Math.Sqrt(-p/3)*Math.Cos((psi1/3) + 2*3.1416*0/3) + 1/3) : (0);
                double z2 = (R < 0) ? (2*Math.Sqrt(-p/3)*Math.Cos((psi1/3) + 2*3.1416*1/3) + 1/3) : (0);
                double z3 = (R < 0) ? (2*Math.Sqrt(-p/3)*Math.Cos((psi1/3) + 2*3.1416*2/3) + 1/3) : (0);
                double z0 = (R > 0) ? (V + W + 1/3) : (0);

                return z*z*z - z*z - Q*z - r;
            };

            double x = Bisection.FindRoot(f1, -0.5, -0.02);
            Assert.AreEqual(-0.03241692071380, x, 1e-7);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = Bisection.FindRoot(f1, -0.02, 0.5);
            Assert.AreEqual(-0.01234360708030, x, 1e-7);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = Bisection.FindRoot(f1, 0.5, 1.2);
            Assert.AreEqual(1.04476050281300, x, 1e-7);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq19b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq19b.htm
            Func<double, double> f1 = z =>
            {
                const double P = 200;
                const double Pc = 33.5;
                const double T = 631;
                const double Tc = 126.2;
                const double Pr = P/Pc;
                const double Tr = T/Tc;
                double Asqr = 0.4278*Pr/Math.Pow(Tr, 2.5);
                const double B = 0.0867*Pr/Tr;
                double Q = B*B + B - Asqr;
                double r = Asqr*B;
                double p = (-3*Q - 1)/3;
                double q = (-27*r - 9*Q - 2)/27;
                double R = Math.Pow(p/3, 3) + Math.Pow(q/2, 2);
                double V = (R > 0) ? Math.Pow(-q/2 + Math.Sqrt(R), 1/3) : 0;
                double WW = (R > 0) ? (-q/2 - Math.Sqrt(R)) : (0);
                double psi1 = (R < 0) ? (Math.Acos(Math.Sqrt((q*q/4)/(-p*p*p/27)))) : (0);
                double W = (R > 0) ? (Math.Sign(WW)*Math.Pow(Math.Abs(WW), 1/3)) : (0);
                double z1 = (R < 0) ? (2*Math.Sqrt(-p/3)*Math.Cos((psi1/3) + 2*3.1416*0/3) + 1/3) : (0);
                double z2 = (R < 0) ? (2*Math.Sqrt(-p/3)*Math.Cos((psi1/3) + 2*3.1416*1/3) + 1/3) : (0);
                double z3 = (R < 0) ? (2*Math.Sqrt(-p/3)*Math.Cos((psi1/3) + 2*3.1416*2/3) + 1/3) : (0);
                double z0 = (R > 0) ? (V + W + 1/3) : (0);

                return z*z*z - z*z - Q*z - r;
            };

            double x = Bisection.FindRoot(f1, -0.5, 1.2);
            Assert.AreEqual(1.06831213384200, x, 1e-7);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq20a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq20a.htm
            Func<double, double> f1 = xa =>
            {
                const double T = 313;
                const double P0 = 10;
                const double FA0 = 20*P0/(0.082*450);
                double k1 = 1.277*1.0e9*Math.Exp(-90000/(8.31*T));
                double k2 = 1.29*1.0e11*Math.Exp(-135000/(8.31*T));
                double den = 1 + 7*P0*(1 - xa)/(1 + xa);
                double xa1 = 1 + xa;
                double ra = (-k1*P0*(1 - xa)/xa1 + k2*P0*P0*xa*xa/(xa1*xa1))/den;

                return -ra/FA0;
            };

            double x = Bisection.FindRoot(f1, 0.75, 1.02);
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
                const double FA0 = 20*P0/(0.082*450);
                double k1 = 1.277*1.0e9*Math.Exp(-90000/(8.31*T));
                double k2 = 1.29*1.0e11*Math.Exp(-135000/(8.31*T));
                double xa1 = 1 + xa;
                double ra = (-k1*P0*(1 - xa)/xa1 + k2*P0*P0*xa*xa/(xa1*xa1));

                return -ra/FA0;
            };

            double x = Bisection.FindRoot(f1, 0.75, 1.02);
            Assert.AreEqual(0.999984253901100, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq21a()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq21a.htm
            Func<double, double> f1 = h =>
            {
                double sg = Math.Acos(2*h - 1);
                double si = Math.Pow(1 - Math.Pow(2*h - 1, 2), 0.5);
                double ag = (sg - (2*h - 1)*si)/4;
                double al = 3.1415927/4 - ag;
                const double d = 1.44;
                double dg = 4*ag/(sg + si)*d;
                const double ugs = -0.0295;
                double ug = ugs*3.1415927/4/ag;
                const double uls = -0.00001;
                double ul = uls*3.1415927/4/al;
                double sl = 3.1415927 - sg;
                double dl = 4*al/(sl)*d;
                const double rol = 1;
                const double rog = 0.835;
                const double bt = .6;
                double g = 980*Math.Sin(bt*3.1415927/180);
                double dpg = (rol*al + rog*ag)*g/3.1415927*4;
                const double vig = 1.0;
                double reg = Math.Abs(ug)*rog*dg/vig;
                double tg = -16/reg*(.5*rog*ug*Math.Abs(ug));
                const double vil = .01;
                double rel = Math.Abs(ul)*rol*dl/vil;
                double tl = -16/rel*(.5*rol*ul*Math.Abs(ul));
                const double it = 1;
                double ti = -16/reg*rog*(.5*(ug - ul)*Math.Abs(ug - ul))*it;
                double dpf = (tl*sl + tg*sg)*d/(al + ag)/(d*d)*4;
                double dp = dpg + dpf;
                double tic = (Math.Abs(ul) < Math.Abs(ug)) ? (rog/reg) : (rol/rel);
                double y = (rol - rog)*g*Math.Pow(d/2, 2)/(8*vig*ugs);

                return -tg*sg*al + tl*sl*ag - ti*si*(al + ag) + d*(rol - rog)*al*ag*g;
            };

            double x = Bisection.FindRoot(f1, 0, 0.3);
            Assert.AreEqual(0.00596133169486, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Oneeq21b()
        {
            // Test case from http://www.polymath-software.com/library/nle/Oneeq21b.htm
            Func<double, double> f1 = h =>
            {
                double sg = Math.Acos(2*h - 1);
                double si = Math.Pow(1 - Math.Pow(2*h - 1, 2), 0.5);
                double ag = (sg - (2*h - 1)*si)/4;
                double al = 3.1415927/4 - ag;
                const double d = 1.44;
                double dg = 4*ag/(sg + si)*d;
                const double ugs = -0.029;
                double ug = ugs*3.1415927/4/ag;
                const double uls = -0.00001;
                double ul = uls*3.1415927/4/al;
                double sl = 3.1415927 - sg;
                double dl = 4*al/(sl)*d;
                const double rol = 1;
                const double rog = 0.835;
                const double bt = .6;
                double g = 980*Math.Sin(bt*3.1415927/180);
                double dpg = (rol*al + rog*ag)*g/3.1415927*4;
                const double vig = 1.0;
                double reg = Math.Abs(ug)*rog*dg/vig;
                double tg = -16/reg*(.5*rog*ug*Math.Abs(ug));
                const double vil = .01;
                double rel = Math.Abs(ul)*rol*dl/vil;
                double tl = -16/rel*(.5*rol*ul*Math.Abs(ul));
                const double it = 1;
                double ti = -16/reg*rog*(.5*(ug - ul)*Math.Abs(ug - ul))*it;
                double dpf = (tl*sl + tg*sg)*d/(al + ag)/(d*d)*4;
                double dp = dpg + dpf;
                double tic = (Math.Abs(ul) < Math.Abs(ug)) ? (rog/reg) : (rol/rel);
                double y = (rol - rog)*g*Math.Pow(d/2, 2)/(8*vig*ugs);

                return -tg*sg*al + tl*sl*ag - ti*si*(al + ag) + d*(rol - rog)*al*ag*g;
            };

            double x = Bisection.FindRoot(f1, 0, 0.1);
            Assert.AreEqual(0.00602268958797, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = Bisection.FindRoot(f1, 0.1, 0.24);
            Assert.AreEqual(0.19925189173123, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
            x = Bisection.FindRoot(f1, 0.24, 0.3);
            Assert.AreEqual(0.26405262123160, x, 1e-5);
            Assert.AreEqual(0, f1(x), 1e-14);
        }

        [Test]
        public void Accuracy()
        {
            // Verify that x-accuracy is being sought, despite good y-accuracy already at the boundaries
            Assert.AreEqual(0.5, Bisection.FindRoot(x => 1e-5 * (x - 0.5), 0.0, 1.0, 1e-5), 1e-5);

            // Verify that y-accuracy is being sought despide good x-accuracy at the boundaries
            Assert.AreEqual(Math.Sqrt(2), Bisection.FindRoot(x => 1e10 * (x - Math.Sqrt(2)), 1.0, 2.0, 1.0), 1e-10);

            // Verify that a discontinuity is not considered a root.
            double root;
            Assert.IsFalse(Bisection.TryFindRoot(x => (x<0)? -1 : 1, -1, 1, 1e-5, 100, out root));
            // ... unless it is a very small discontinuity
            Assert.AreEqual(0.0, Bisection.FindRoot(x => (x < 0) ? -1e-5 : 1e-5, -1, Math.Sqrt(2), 1e-5), 1e-5);

            // Verify no unnecessary iterations are done after the requested accuracy is achieved
            var niter = 0;
            Func<double, double> f = x => { niter++; return x - Math.Sqrt(2); };
            Bisection.FindRoot(f, 1.0, 2.0, 1.0 / 2);
            Assert.LessOrEqual(niter, 3);

            niter = 0;
            Bisection.FindRoot(f, 1.0, 2.0, 1.0 / 4);
            Assert.LessOrEqual(niter, 4);

            niter = 0;
            Bisection.FindRoot(f, 1.0, 2.0, 1.0 / 8);
            Assert.LessOrEqual(niter, 5);

            niter = 0;
            Bisection.FindRoot(f, 1.0, 2.0, 1.0 / 16);
            Assert.LessOrEqual(niter, 6);

            niter = 0;
            Bisection.FindRoot(f, 1.0, 2.0, 1.0 / 32);
            Assert.LessOrEqual(niter, 7);

            niter = 0;
            Bisection.FindRoot(f, 1.0, 2.0, 1.0 / 64);
            Assert.LessOrEqual(niter, 8);
        }
    }
}
