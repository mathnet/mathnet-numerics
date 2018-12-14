// <copyright file="Factorial.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using System;
using MathNet.Numerics.Properties;
using BigInteger = System.Numerics.BigInteger;

// ReSharper disable CheckNamespace
namespace MathNet.Numerics
// ReSharper restore CheckNamespace
{
    public partial class SpecialFunctions
    {
        const int FactorialMaxArgument = 170;
        static readonly double[] _factorialCache =
        {
            0x3FF0000000000000, 0x3FF0000000000000, 0x4000000000000000, 0x4018000000000000, 0x4038000000000000, 0x405E000000000000, 0x4086800000000000, 0x40B3B00000000000, 0x40E3B00000000000, 0x4116260000000000, 0x414BAF8000000000, 0x418308A800000000, 0x41BC8CFC00000000, 0x41F7328CC0000000, 0x42344C3B28000000, 0x4273077775800000, 0x42B3077775800000, 0x42F437EEECD80000, 0x4336BEECCA730000, 0x437B02B930689000, 0x43C0E1B3BE415A00, 0x4406283BE9B5C620, 0x444E77526159F06C, 0x4495E5C335F8A4CE, 0x44E06C52687A7B9A, 0x4529A940C33F6121, 0x4574D9849EA37EEB, 0x45C19787E5D9F316, 0x460EC92DD23D6966, 0x465BE6518687A784, 0x46AA27EC6E1F2D0C, 0x46F956AD0AAE33A4, 0x474956AD0AAE33A4, 0x479A21627303A541, 0x47EBC3789A33DF95, 0x483E5DCBE8A8BC8B, 0x489114C2B2DEEA0E, 0x48E3C0011ED1BEA0, 0x493774015499125E, 0x498C95619F1A8E63, 0x49E1DD5D037098FE, 0x4A36E39F2C684405, 0x4A8E0AC0EA48D947, 0x4AE42F399D68F1FC, 0x4B3BC0EF38704CBA, 0x4B9383A833AEF5F3, 0x4BEC0D41CA4B818D, 0x4C4499BC508F7324, 0x4C9EE69A78D72CB6, 0x4CF7A88E4484BE3B, 0x4D527BAF2587B49E, 0x4DAD751F23D047DC, 0x4E07EF294D193A63, 0x4E63D20E33D8E45A, 0x4EC0B93BFBBF00AC, 0x4F1CBE5F18B04928, 0x4F792693359A4003, 0x4FD6665B1BBD6103, 0x50344CC291239FEB, 0x5092B6C35DCCD76D, 0x50F18B5727F009F6, 0x5150B8CF1210C97E, 0x51B0330899804332, 0x520FE478EE34844A, 0x526FE478EE34844A, 0x52D0320568F6AB2E, 0x5330B395943E6087, 0x53917C0097314D0D, 0x53F293C0A0A461DE, 0x5454074BAD313983, 0x54B5E7FAC56DD6E7, 0x55184D5A3305DA68, 0x557B5705796695B5, 0x55DF2F423E7902C2, 0x564207524C1DF598, 0x56A5209471331BCE, 0x570916B0466CB105, 0x576E2F4C14BAC4FA, 0x57D264D25CA1D008, 0x5836B473AA57BCCA, 0x589C619094EDABFC, 0x5901F5BD7E3E66D5, 0x596702DAC9BFF3C1, 0x59CDD7B3BDA4F01E, 0x5A33958DF4743D94, 0x5A9A02A088AA61C9, 0x5B0179C3DBD279B3, 0x5B67C1863ED21D6F, 0x5BD0550C4B30743C, 0x5C36B645188F61A3, 0x5C9FF0512A89A14D, 0x5D06B4D9B43DD8AD, 0x5D7051FC798C73BC, 0x5DD7B722E0A0182D, 0x5E416A7D9CF591C1, 0x5EA9DA1274FC845A, 0x5F13638DD7BD6344, 0x5F7D62E2FAFB0A73, 0x5FE67FB5C8283400, 0x605166C698CF1838, 0x60BB30964EC395D8, 0x612574569A26543C, 0x619118B502D68B20, 0x61FB83C3509147E8, 0x62665B0EB1760A6C, 0x62D256B20D92D48D, 0x633E5F96E67B300A, 0x63A963E824AAFA28, 0x64156C4BDEF04312, 0x64823E389BD8991D, 0x64EF5AF14BDC472A, 0x655B30DD3FC905B6, 0x65C7CAC197CFE4FF, 0x663500FEE8058829, 0x66A2B4E306A4ED45, 0x6710CE83F7F82D2C, 0x677E764F3171D1E0, 0x67EBD824633209D7, 0x6859AB418B722112, 0x68C7DD36EFA41ABF, 0x69365F6380A9D913, 0x69A5262C0FA08F34, 0x6A142861FEE5087E, 0x6A835ECE2AF01629, 0x6AF2C3D7B9989578, 0x6B625340AB3F01F7, 0x6BD209F3A89205EF, 0x6C41E5DFC140E1E3, 0x6CB1E5DFC140E1E3, 0x6D2209AB80C363A7, 0x6D9251D22EC67136, 0x6E02BFBD1BDF17DD, 0x6E7355BB04BE109C, 0x6EE4171452ED7D42, 0x6F55082946D09F21, 0x6FC62E9B88B007D5, 0x70379185413B0852, 0x70A939C09FD12EE8, 0x711B3243AC4D8692, 0x718D88957D1C3023, 0x720026B1C06B6A53, 0x7271CA9FCDF6531F, 0x72E3BCC9487D4436, 0x73560CE8DEFBF234, 0x73C8CE85FADB707A, 0x743C19F3C62C956A, 0x74B006CD07056D36, 0x752267CF76103B6C, 0x75954807E082C4B5, 0x7608C5D92B5838FB, 0x767D07DA7ECB62C6, 0x76F11FA1E0C9F743, 0x776455903AEFD5A0, 0x77D84E466672AD59, 0x784D3E2CB341F88F, 0x78C1B4A51088F17F, 0x793594292C26E653, 0x79AA77BA8027B682, 0x7A2055E51B1882A4, 0x7A944AB297A87248, 0x7B095D5F3D928EDA, 0x7B7FE771CB7257AE, 0x7BF4307602BE5B7C, 0x7C69B5B6477E6880, 0x7CE07868C5CCFAF2, 0x7D553B370EFA3B7C, 0x7DCB88CB676C8525, 0x7E41F63CB077CADB, 0x7EB7932FA79D3A3F, 0x7F2F2054EB4D96E7, 0x7FA4AB7864418635
        };

        /// <summary>
        /// Computes the factorial function x -> x! of an integer number > 0. The function can represent all number up
        /// to 22! exactly, all numbers up to 170! using a double representation. All larger values will overflow.
        /// </summary>
        /// <returns>A value value! for value > 0</returns>
        /// <remarks>
        /// If you need to multiply or divide various such factorials, consider using the logarithmic version
        /// <see cref="FactorialLn"/> instead so you can add instead of multiply and subtract instead of divide, and
        /// then exponentiate the result using <see cref="System.Math.Exp"/>. This will also circumvent the problem that
        /// factorials become very large even for small parameters.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException" />
        public static double Factorial(int x)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x), Resources.ArgumentPositive);
            }

            if (x < _factorialCache.Length)
            {
                return _factorialCache[x];
            }

            return double.PositiveInfinity;
        }

        /// <summary>
        /// Computes the factorial of an integer.
        /// </summary>
        public static BigInteger Factorial(BigInteger x)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x), Resources.ArgumentPositive);
            }

            if (x == 0)
            {
                return BigInteger.One;
            }

            BigInteger r = x;
            while (--x > 1)
            {
                r *= x;
            }

            return r;
        }

        /// <summary>
        /// Computes the logarithmic factorial function x -> ln(x!) of an integer number > 0.
        /// </summary>
        /// <returns>A value value! for value > 0</returns>
        public static double FactorialLn(int x)
        {
            if (x < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x), Resources.ArgumentPositive);
            }

            if (x <= 1)
            {
                return 0d;
            }

            if (x < _factorialCache.Length)
            {
                return Math.Log(_factorialCache[x]);
            }

            return GammaLn(x + 1.0);
        }

        /// <summary>
        /// Computes the binomial coefficient: n choose k.
        /// </summary>
        /// <param name="n">A nonnegative value n.</param>
        /// <param name="k">A nonnegative value h.</param>
        /// <returns>The binomial coefficient: n choose k.</returns>
        public static double Binomial(int n, int k)
        {
            if (k < 0 || n < 0 || k > n)
            {
                return 0.0;
            }

            return Math.Floor(0.5 + Math.Exp(FactorialLn(n) - FactorialLn(k) - FactorialLn(n - k)));
        }

        /// <summary>
        /// Computes the natural logarithm of the binomial coefficient: ln(n choose k).
        /// </summary>
        /// <param name="n">A nonnegative value n.</param>
        /// <param name="k">A nonnegative value h.</param>
        /// <returns>The logarithmic binomial coefficient: ln(n choose k).</returns>
        public static double BinomialLn(int n, int k)
        {
            if (k < 0 || n < 0 || k > n)
            {
                return double.NegativeInfinity;
            }

            return FactorialLn(n) - FactorialLn(k) - FactorialLn(n - k);
        }

        /// <summary>
        /// Computes the multinomial coefficient: n choose n1, n2, n3, ...
        /// </summary>
        /// <param name="n">A nonnegative value n.</param>
        /// <param name="ni">An array of nonnegative values that sum to <paramref name="n"/>.</param>
        /// <returns>The multinomial coefficient.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="ni"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="n"/> or any of the <paramref name="ni"/> are negative.</exception>
        /// <exception cref="ArgumentException">If the sum of all <paramref name="ni"/> is not equal to <paramref name="n"/>.</exception>
        public static double Multinomial(int n, int[] ni)
        {
            if (n < 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, nameof(n));
            }

            if (ni == null)
            {
                throw new ArgumentNullException(nameof(ni));
            }

            int sum = 0;
            double ret = FactorialLn(n);
            for (int i = 0; i < ni.Length; i++)
            {
                if (ni[i] < 0)
                {
                    throw new ArgumentException(Resources.ArgumentMustBePositive, "ni[" + i + "]");
                }

                ret -= FactorialLn(ni[i]);
                sum += ni[i];
            }

            // Before returning, check that the sum of all elements was equal to n.
            if (sum != n)
            {
                throw new ArgumentException(Resources.ArgumentParameterSetInvalid, nameof(ni));
            }

            return Math.Floor(0.5 + Math.Exp(ret));
        }
    }
}
