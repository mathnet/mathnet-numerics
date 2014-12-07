
namespace MathNet.Numerics.UnitTests.SpecialFunctionsTests
{
    using System;
    using NUnit.Framework;

    /// <summary>
    /// Factorial tests.
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class ExponentialIntegralTests
    {

        [TestCase(0.001d, 6.33153936413614904)]
        [TestCase(0.1d, 1.82292395841939059)]
        [TestCase(1.0d, 0.219383934395520286d)]
        [TestCase(2.0d, 0.0489005107080611248d)]
        [TestCase(2.5d, 0.0249149178702697399)]
        [TestCase(10.0d, 4.15696892968532464e-06)]
        public void ExponentialIntegral_Matches_MATLAB_and_R_expint_E1(double x, double result)
        {
            double actual = SpecialFunctions.ExponentialIntegral( x, 1 );
            double delta = Math.Abs( result - actual );
            AssertHelpers.AlmostEqualRelative( result, actual, 13 );
        }

        [TestCase(0.001d, 2, 0.992668960469238915)]
        [TestCase(0.1d, 2, 0.722545022194020392)]
        [TestCase(1.0d, 2, 0.148495506775922048)]
        [TestCase(2.0d, 2, 0.0375342618204904527)]
        [TestCase(10.0d, 2, 3.830240465631608e-06)]
        public void ExponentialIntegral_Matches_R_expint_En(double x, int n, double result)
        {
            double actual = SpecialFunctions.ExponentialIntegral(x, n);
            double delta = Math.Abs(result - actual);
            AssertHelpers.AlmostEqualRelative(result, actual, 13);
        }


        [TestCase(0.001d, 0, 999.000499833375)]
        [TestCase(0.1d, 0, 9.048374180359595)]
        [TestCase(1.0d, 0, 0.3678794411714423)]
        [TestCase(2.0d, 0, 0.06766764161830635)]
        [TestCase(10.0d, 0, 4.539992976248485e-06)]
        public void ExponentialIntegral_SpecialCase_EXP_Matches_from_R_expint_En(double x, int n, double result)
        {
            double actual = SpecialFunctions.ExponentialIntegral(x, n);
            double delta = Math.Abs(result - actual);
            AssertHelpers.AlmostEqualRelative(result, actual, 13);
        }

    }
}
