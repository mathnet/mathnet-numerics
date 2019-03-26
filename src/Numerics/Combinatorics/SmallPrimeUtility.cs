// Copyright 2008 Adrian Akison
// Distributed under license terms of CPOL http://www.codeproject.com/info/cpol10.aspx
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace MathNet.Numerics.Combinations
{
    /// <summary>
    /// Utility class that maintains a small table of prime numbers and provides
    /// simple implementations of Prime Factorization algorithms.  
    /// This is a quick and dirty utility class to support calculations of permutation
    /// sets with indexes under 2^31.
    /// The prime table contains all primes up to Sqrt(2^31) which are all of the primes
    /// requires to factorize any Int32 positive integer.
    /// </summary>
    public class SmallPrimeUtility {
        /// <summary>
        /// Utility class, no instances allowed.
        /// </summary>
        private SmallPrimeUtility() {
            ;
        }

        /// <summary>
        /// Performs a prime factorization of a given integer using the table of primes in PrimeTable.
        /// Since this will only factor Int32 sized integers, a simple list of factors is returned instead
        /// of the more scalable, but more difficult to consume, list of primes and associated exponents.
        /// </summary>
        /// <param name="i">The number to factorize, must be positive.</param>
        /// <returns>A simple list of factors.</returns>
        public static List<int> Factor(int i) {
            int primeIndex = 0;
            int prime = PrimeTable[primeIndex];
            List<int> factors = new List<int>();
            while(i > 1) {
                if(i % prime == 0) {
                    factors.Add(prime);
                    i /= prime;
                }
                else {
                    ++primeIndex;
                    prime = PrimeTable[primeIndex];
                }
            }
            return factors;
        }

        /// <summary>
        /// Given two integers expressed as a list of prime factors, multiplies these numbers
        /// together and returns an integer also expressed as a set of prime factors.
        /// This allows multiplication to overflow well beyond a Int64 if necessary.  
        /// </summary>
        /// <param name="lhs">Left Hand Side argument, expressed as list of prime factors.</param>
        /// <param name="rhs">Right Hand Side argument, expressed as list of prime factors.</param>
        /// <returns>Product, expressed as list of prime factors.</returns>
        public static List<int> MultiplyPrimeFactors(IList<int> lhs, IList<int> rhs) {
            List<int> product = new List<int>();
            foreach(int prime in lhs) {
                product.Add(prime);
            }
            foreach(int prime in rhs) {
                product.Add(prime);
            }
            product.Sort();
            return product;
        }

        /// <summary>
        /// Given two integers expressed as a list of prime factors, divides these numbers
        /// and returns an integer also expressed as a set of prime factors.
        /// If the result is not a integer, then the result is undefined.  That is, 11 / 5
        /// when divided by this function will not yield a correct result.
        /// As such, this function is ONLY useful for division with combinatorial results where 
        /// the result is known to be an integer AND the division occurs as the last operation(s).
        /// </summary>
        /// <param name="numerator">Numerator argument, expressed as list of prime factors.</param>
        /// <param name="denominator">Denominator argument, expressed as list of prime factors.</param>
        /// <returns>Resultant, expressed as list of prime factors.</returns>
        public static List<int> DividePrimeFactors(IList<int> numerator, IList<int> denominator) {
            List<int> product = new List<int>();
            foreach(int prime in numerator) {
                product.Add(prime);
            }
            foreach(int prime in denominator) {
                product.Remove(prime);
            }
            return product;
        }

        /// <summary>
        /// Given a list of prime factors returns the long representation.
        /// </summary>
        /// <param name="value">Integer, expressed as list of prime factors.</param>
        /// <returns>Standard long representation.</returns>
        public static long EvaluatePrimeFactors(IList<int> value) {
            long accumulator = 1;
            foreach(int prime in value) {
                accumulator *= prime;
            }
            return accumulator;
        }

        /// <summary>
        /// Static initializer, set up prime table.
        /// </summary>
        static SmallPrimeUtility() {
            CalculatePrimes();
        }

        /// <summary>
        /// Calculate all primes up to Sqrt(2^32) = 2^16.  
        /// This table will be large enough for all factorizations for Int32's.
        /// Small tables are best built using the Sieve Of Eratosthenes,
        /// Reference: http://primes.utm.edu/glossary/page.php?sort=SieveOfEratosthenes
        /// </summary>
        private static void CalculatePrimes() {
            // Build Sieve Of Eratosthenes
            BitArray sieve = new BitArray(65536, true);
            for(int possiblePrime = 2; possiblePrime <= 256; ++possiblePrime) {
                if(sieve[possiblePrime] == true) {
                    // It is prime, so remove all future factors...
                    for(int nonPrime = 2 * possiblePrime; nonPrime < 65536; nonPrime += possiblePrime) {
                        sieve[nonPrime] = false;
                    }
                }
            }
            // Scan sieve for primes...
            myPrimes = new List<int>();
            for(int i = 2; i < 65536; ++i) {
                if(sieve[i] == true) {
                    myPrimes.Add(i);
                }
            }

        }

        /// <summary>
        /// A List of all primes from 2 to 2^16.
        /// </summary>
        public static IList<int> PrimeTable {
            get {
                return myPrimes;
            }
        }

        private static List<int> myPrimes = new List<int>();

    }
}
