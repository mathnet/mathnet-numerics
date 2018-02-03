// <copyright file="RandomNumberGeneration.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
using MathNet.Numerics.Random;

namespace Examples
{
    /// <summary>
    /// Random number generation
    /// </summary>
    /// <seealso cref="http://reference.wolfram.com/mathematica/tutorial/RandomNumberGeneration.html">Random number generation</seealso>
    public class RandomNumberGeneration : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Random number generation";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Usage examples of random number generators (RNG)";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        /// <seealso cref="http://en.wikipedia.org/wiki/Random_number_generation">Random number generation</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Linear_congruential_generator">Linear congruential generator</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Mersenne_twister">Mersenne twister</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Lagged_Fibonacci_generator">Lagged Fibonacci generator</seealso>
        /// <seealso cref="http://en.wikipedia.org/wiki/Xorshift">Xorshift</seealso>
        public void Run()
        {
            // All RNG classes in MathNet have next counstructors:
            // - RNG(int seed, bool threadSafe): initializes a new instance with specific seed value and thread safe property
            // - RNG(int seed): iуууnitializes a new instance with specific seed value. Thread safe property is set to Control.ThreadSafeRandomNumberGenerators
            // - RNG(bool threadSafe) : initializes a new instance with the seed value set to DateTime.Now.Ticks and specific thread safe property
            // - RNG(bool threadSafe) : initializes a new instance with the seed value set to DateTime.Now.Ticks and thread safe property set to Control.ThreadSafeRandomNumberGenerators

            // All RNG classes in MathNet have next methods to produce random values:
            // - double[] NextDouble(int n): returns an "n"-size array of uniformly distributed random doubles in the interval [0.0,1.0];
            // - int Next(): returns a nonnegative random number;
            // - int Next(int maxValue): returns a random number less then a specified maximum;
            // - int Next(int minValue, int maxValue): returns a random number within a specified range;
            // - void NextBytes(byte[] buffer): fills the elements of a specified array of bytes with random numbers;

            // All RNG classes in MathNet have next extension methods to produce random values:
            // - long NextInt64(): returns a nonnegative random number less than "Int64.MaxValue";
            // - int NextFullRangeInt32(): returns a random number of the full Int32 range;
            // - long NextFullRangeInt64(): returns a random number of the full Int64 range;
            // - decimal NextDecimal(): returns a nonnegative decimal floating point random number less than 1.0;

            // 1. Multiplicative congruential generator using a modulus of 2^31-1 and a multiplier of 1132489760
            var mcg31M1 = new Mcg31m1(1);
            Console.WriteLine(@"1. Generate 10 random double values using Multiplicative congruential generator with a modulus of 2^31-1 and a multiplier of 1132489760");
            var randomValues = mcg31M1.NextDoubles(10);
            for (var i = 0; i < randomValues.Length; i++)
            {
                Console.Write(randomValues[i].ToString("N") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 2. Multiplicative congruential generator using a modulus of 2^59 and a multiplier of 13^13
            var mcg59 = new Mcg59(1);
            Console.WriteLine(@"2. Generate 10 random integer values using Multiplicative congruential generator with a modulus of 2^59 and a multiplier of 13^13");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(mcg59.Next() + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 3. Random number generator using Mersenne Twister 19937 algorithm
            var mersenneTwister = new MersenneTwister(1);
            Console.WriteLine(@"3. Generate 10 random integer values less then 100 using Mersenne Twister 19937 algorithm");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(mersenneTwister.Next(100) + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 4. Multiple recursive generator with 2 components of order 3
            var mrg32K3A = new Mrg32k3a(1);
            Console.WriteLine(@"4. Generate 10 random integer values in range [50;100] using multiple recursive generator with 2 components of order 3");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(mrg32K3A.Next(50, 100) + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 5. Parallel Additive Lagged Fibonacci pseudo-random number generator
            var palf = new Palf(1);
            Console.WriteLine(@"5. Generate 10 random bytes using Parallel Additive Lagged Fibonacci pseudo-random number generator");
            var bytes = new byte[10];
            palf.NextBytes(bytes);
            for (var i = 0; i < bytes.Length; i++)
            {
                Console.Write(bytes[i] + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 6. A random number generator based on the "System.Security.Cryptography.RandomNumberGenerator" class in the .NET library
            var systemCrypto = new CryptoRandomSource();
            Console.WriteLine(@"6. Generate 10 random decimal values using RNG based on the 'System.Security.Cryptography.RandomNumberGenerator'");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(systemCrypto.NextDecimal().ToString("N") + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 7. Wichmann-Hill’s 1982 combined multiplicative congruential generator
            var rngWh1982 = new WH1982();
            Console.WriteLine(@"7. Generate 10 random full Int32 range values using Wichmann-Hill’s 1982 combined multiplicative congruential generator");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(rngWh1982.NextFullRangeInt32() + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 8. Wichmann-Hill’s 2006 combined multiplicative congruential generator.
            var rngWh2006 = new WH2006();
            Console.WriteLine(@"8. Generate 10 random full Int64 range values using Wichmann-Hill’s 2006 combined multiplicative congruential generator");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(rngWh2006.NextFullRangeInt32() + @" ");
            }

            Console.WriteLine();
            Console.WriteLine();

            // 9. Multiply-with-carry Xorshift pseudo random number generator
            var xorshift = new Xorshift();
            Console.WriteLine(@"9. Generate 10 random nonnegative values less than Int64.MaxValue using Multiply-with-carry Xorshift pseudo random number generator");
            for (var i = 0; i < 10; i++)
            {
                Console.Write(xorshift.NextInt64() + @" ");
            }

            Console.WriteLine();
        }
    }
}
