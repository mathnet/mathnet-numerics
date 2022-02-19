using System;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.SpecialFunctionsTests
{
    /// <summary>
    /// Marcum Q functions tests.
    /// </summary>
    [TestFixture, Category("Functions")]
    public class MarcumQTests
    {
        [TestCase(1.0, 0.3, 0.01, 0.9926176915580, 0.570420447e-14)]
        [TestCase(3.0, 2.0, 0.1, 0.9999780077720, 0.321972267e-13)]
        [TestCase(4.0, 8.0, 50.0, 0.2311934913546e-07, 0.218106999e-12)]
        [TestCase(6.0, 25.0, 10.0, 0.9998253130004, 0.222087973e-14)]
        [TestCase(8.0, 13.0, 15.0, 0.8516869957363, 0.338927020e-14)]
        [TestCase(10.0, 45.0, 25.0, 0.9998251671677, 0.234298258e-13)]
        [TestCase(20.0, 47.0, 30.0, 0.9999865923082, 0.470741068e-13)]
        [TestCase(22.0, 100.0, 150.0, 0.3534087845586e-01, 0.114859944e-12)]
        [TestCase(25.0, 85.0, 60.0, 0.9999821600833, 0.304206530e-13)]
        [TestCase(27.0, 120.0, 205.0, 0.5457593568564e-03, 0.880060998e-13)]
        [TestCase(30.0, 130.0, 90.0, 0.9999987797684, 0.366374140e-13)]
        [TestCase(32.0, 140.0, 100.0, 0.9999982425123, 0.317739108e-12)]
        [TestCase(40.0, 30.0, 120.0, 0.1052462813144e-04, 0.940623989e-13)]
        [TestCase(50.0, 40.0, 150.0, 0.3165262228904e-05, 0.200960125e-13)]
        [TestCase(200.0, 0.01, 190.0, 0.7568702241292, 0.422994972e-13)]
        [TestCase(350.0, 100.0, 320.0, 0.9999999996149, 0.235374329e-13)]
        [TestCase(570.0, 1.0, 480.0, 0.9999701550685, 0.854871729e-14)]
        [TestCase(1000.0, 0.08, 799.0, 0.9999999999958, 0.237242084e-12)]
        public void MarcumQSomeValuesApprox(double nu,double a,double b,double q,double err)
        {
            AssertHelpers.AlmostEqual(err, Math.Abs(1 - SpecialFunctions.MarcumQ(nu, a, b) / q), 12);
        }

        [Test]
        public void MarcumQRecurrenceRelationApprox()
        {
            double delta = 0;
            double d0 = -1;

            foreach (double mu in Generate.LinearRange(10, 50, 510))
            {
                foreach (double x in Generate.LinearSpaced(55, 50, 505))
                {
                    foreach (double y in Generate.LinearRange(2, 19.15, 193.5))
                    {
                        SpecialFunctions.MarcumQFunction.Marcum(mu, x, y, out double p0, out double q0, out int ierr1);
                        SpecialFunctions.MarcumQFunction.Marcum(mu - 1, x, y, out double pm1, out double qm1, out int ierr2);
                        SpecialFunctions.MarcumQFunction.Marcum(mu + 1, x, y, out double p1, out double q1, out int ierr3);
                        SpecialFunctions.MarcumQFunction.Marcum(mu + 2, x, y, out double p2, out double q2, out int ierr4);

                        if (((ierr1 == 0) && (ierr2 == 0)) && ((ierr3 == 0) && (ierr4 == 0)))
                        {
                            if (y > x + mu)
                            {
                                delta = Math.Abs(((x - mu) * q1 + (y + mu) * q0) / (x * q2 + y * qm1) - 1.0);
                            }
                            else
                            {
                                delta = Math.Abs(((x - mu) * p1 + (y + mu) * p0) / (x * p2 + y * pm1) - 1.0);
                            }

                            if (delta > d0)
                            {
                                d0 = delta;
                            }
                        }
                    }
                }
            }



            AssertHelpers.AlmostEqual(0, d0, 12);
        }
    }
}
