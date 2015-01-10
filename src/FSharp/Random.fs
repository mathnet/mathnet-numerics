// <copyright file="Random.fs" company="Math.NET">
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

namespace MathNet.Numerics.Random

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Random =

    /// Returns the default random source, thread-safe and also thread-locally shared
    let shared = SystemRandomSource.Default :> System.Random

    /// Default sampling, efficient but without custom seed (uses robust seeds internally)
    let inline doubles (length:int) = SystemRandomSource.FastDoubles(length)
    let inline doubleSeq () = SystemRandomSource.DoubleSequence()
    let inline doubleFill (values:float[]) = SystemRandomSource.FastDoubles(values)

    let inline doublesSeed (seed:int) (length:int) = SystemRandomSource.Doubles(length, seed)
    let inline doubleSeqSeed (seed:int) = SystemRandomSource.DoubleSequence(seed)
    let inline doubleFillSeed (seed:int) (values:float[]) = SystemRandomSource.Doubles(values, seed)

    /// Creates a default .Net system pRNG with a robust seed
    let systemShared = shared
    let inline system () = SystemRandomSource() :> System.Random
    let inline systemSeed (seed:int) = SystemRandomSource(seed) :> System.Random

#if PORTABLE
#else
    /// Creates a default .Net cryptographic system pRNG
    let inline crypto () = new CryptoRandomSource() :> System.Random
    let inline cryptoWith (threadSafe:bool) = new CryptoRandomSource(threadSafe) :> System.Random
    let inline cryptoDoubles (length:int) = CryptoRandomSource.Doubles(length)
    let inline cryptoDoubleSeq () = CryptoRandomSource.DoubleSequence()
#endif

    /// Creates a Mersenne Twister 19937 pRNG with a robust seed
    let mersenneTwisterShared = MersenneTwister.Default :> System.Random
    let inline mersenneTwister () = MersenneTwister() :> System.Random
    let inline mersenneTwisterSeed (seed:int) = MersenneTwister(seed) :> System.Random
    let inline mersenneTwisterWith (seed:int) threadSafe = MersenneTwister(seed, threadSafe) :> System.Random

    /// Creates a multiply-with-carry Xorshift (Xn = a * Xn−3 + c mod 2^32) pRNG with a robust seed
    let inline xorshift () = Xorshift() :> System.Random
    let inline xorshiftSeed (seed:int) = Xorshift(seed) :> System.Random
    let inline xorshiftWith (seed:int) threadSafe = Xorshift(seed, threadSafe) :> System.Random
    let inline xorshiftCustom (seed:int) threadSafe a c x1 x2 = Xorshift(seed, threadSafe, a, c, x1, x2) :> System.Random

    /// Creates a Wichmann-Hill’s 1982 combined multiplicative congruential pRNG with a robust seed
    let inline wh1982 () = WH1982() :> System.Random
    let inline wh1982Seed (seed:int) = WH1982(seed) :> System.Random
    let inline wh1982With (seed:int) threadSafe = WH1982(seed, threadSafe) :> System.Random

    /// Creates a Wichmann-Hill’s 2006 combined multiplicative congruential pRNG with a robust seed
    let inline wh2006 () = WH2006() :> System.Random
    let inline wh2006Seed (seed:int) = WH2006(seed) :> System.Random
    let inline wh2006With (seed:int) threadSafe = WH2006(seed, threadSafe) :> System.Random

    /// Creates a Parallel Additive Lagged Fibonacci pRNG with a robust seed
    let inline palf () = Palf() :> System.Random
    let inline palfSeed (seed:int) = Palf(seed) :> System.Random
    let inline palfWith (seed:int) threadSafe = Palf(seed, threadSafe, 418, 1279) :> System.Random
    let inline palfCustom (seed:int) threadSafe shortLag longLag = Palf(seed, threadSafe, shortLag, longLag) :> System.Random

    /// Creates a Multiplicative congruential generator using a modulus of 2^59 and a multiplier of 13^13 pRNG with a robust seed
    let inline mcg59 () = Mcg59() :> System.Random
    let inline mcg59Seed (seed:int) = Mcg59(seed) :> System.Random
    let inline mcg59With (seed:int) threadSafe = Mcg59(seed, threadSafe) :> System.Random

    /// Creates a Multiplicative congruential generator using a modulus of 2^31-1 and a multiplier of 1132489760 pRNG with a robust seed
    let inline mcg31m1 () = Mcg31m1() :> System.Random
    let inline mcg31m1Seed (seed:int) = Mcg31m1(seed) :> System.Random
    let inline mcg31m1With (seed:int) threadSafe = Mcg31m1(seed, threadSafe) :> System.Random

    /// Creates a 32-bit combined multiple recursive generator with 2 components of order 3 pRNG with a robust seed
    let inline mrg32k3a () = Mrg32k3a() :> System.Random
    let inline mrg32k3aSeed (seed:int) = Mrg32k3a(seed) :> System.Random
    let inline mrg32k3aWith (seed:int) threadSafe = Mrg32k3a(seed, threadSafe) :> System.Random
