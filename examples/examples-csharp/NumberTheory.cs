// <copyright file="NumberTheory.cs" company="Math.NET">
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
using MathNet.Numerics;

namespace Examples
{
    /// <summary>
    /// Number theory utility functions
    /// </summary>
    public class NumberTheory : IExample
    {
        /// <summary>
        /// Gets the name of this example
        /// </summary>
        public string Name
        {
            get
            {
                return "Number theory utility functions";
            }
        }

        /// <summary>
        /// Gets the description of this example
        /// </summary>
        public string Description
        {
            get
            {
                return "Usage of the number theory utility functions and extention methods";
            }
        }

        /// <summary>
        /// Run example
        /// </summary>
        public void Run()
        {
            // 1. Find out whether the provided number is an even number
            Console.WriteLine(@"1. Find out whether the provided number is an even number");
            Console.WriteLine(@"{0} is even = {1}. {2} is even = {3}", 1, Euclid.IsEven(1), 2, 2.IsEven());
            Console.WriteLine();

            // 2. Find out whether the provided number is an odd number
            Console.WriteLine(@"2. Find out whether the provided number is an odd number");
            Console.WriteLine(@"{0} is odd = {1}. {2} is odd = {3}", 1, 1.IsOdd(), 2, Euclid.IsOdd(2));
            Console.WriteLine();

            // 3. Find out whether the provided number is a perfect power of two
            Console.WriteLine(@"2. Find out whether the provided number is a perfect power of two");
            Console.WriteLine(@"{0} is power of two = {1}. {2} is power of two = {3}", 5, 5.IsPowerOfTwo(), 16, Euclid.IsPowerOfTwo(16));
            Console.WriteLine();

            // 4. Find the closest perfect power of two that is larger or equal to 97
            Console.WriteLine(@"4. Find the closest perfect power of two that is larger or equal to 97");
            Console.WriteLine(97.CeilingToPowerOfTwo());
            Console.WriteLine();

            // 5. Raise 2 to the 16
            Console.WriteLine(@"5. Raise 2 to the 16");
            Console.WriteLine(16.PowerOfTwo());
            Console.WriteLine();

            // 6. Find out whether the number is a perfect square
            Console.WriteLine(@"6. Find out whether the number is a perfect square");
            Console.WriteLine(@"{0} is perfect square = {1}. {2} is perfect square = {3}", 37, 37.IsPerfectSquare(), 81, Euclid.IsPerfectSquare(81));
            Console.WriteLine();

            // 7. Compute the greatest common divisor of 32 and 36 
            Console.WriteLine(@"7. Returns the greatest common divisor of 32 and 36");
            Console.WriteLine(Euclid.GreatestCommonDivisor(32, 36));
            Console.WriteLine();

            // 8. Compute the greatest common divisor of 492, -984, 123, 246
            Console.WriteLine(@"8. Returns the greatest common divisor of 492, -984, 123, 246");
            Console.WriteLine(Euclid.GreatestCommonDivisor(492, -984, 123, 246));
            Console.WriteLine();

            // 9. Compute the extended greatest common divisor "z", such that 45*x + 18*y = z
            Console.WriteLine(@"9. Compute the extended greatest common divisor Z, such that 45*x + 18*y = Z");
            long x, y;
            var z = Euclid.ExtendedGreatestCommonDivisor(45, 18, out x, out y);
            Console.WriteLine(@"z = {0}, x = {1}, y = {2}. 45*{1} + 18*{2} = {0}", z, x, y);
            Console.WriteLine();

            // 10. Compute the least common multiple of 16 and 12
            Console.WriteLine(@"10. Compute the least common multiple of 16 and 12");
            Console.WriteLine(Euclid.LeastCommonMultiple(16, 12));
            Console.WriteLine();
        }
    }
}
