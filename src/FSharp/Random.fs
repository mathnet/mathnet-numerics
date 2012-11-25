// <copyright file="Random.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2012 Math.NET
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

    /// Provides a seed based on unique GUIDs
    let seed () = System.Guid.NewGuid().GetHashCode()

    /// Provides a time-dependent seed value (caution, can produce the same value on quick repeated execution)
    let timeSeed () = System.Environment.TickCount

    /// Creates a default .Net system pRNG with a custom seed based on uinque GUIDs
    let system () = new System.Random(seed())
    let systemWith seed = new System.Random(seed)

#if PORTABLE
#else
    /// Creates a default .Net cryptographic system pRNG
    let crypto () = new SystemCryptoRandomNumberGenerator() :> System.Random
    let cryptoWith (threadSafe:bool) = new SystemCryptoRandomNumberGenerator(threadSafe) :> System.Random
#endif

    /// Creates a Mersenne Twister 19937 pRNG with a custom seed based on uinque GUIDs
    let mersenneTwister () = new MersenneTwister(seed()) :> System.Random
    let mersenneTwisterWith seed threadSafe = new MersenneTwister(seed, threadSafe) :> System.Random

    /// Creates a multiply-with-carry Xorshift (Xn = a * Xn−3 + c mod 2^32) pRNG with a custom seed based on uinque GUIDs
    let xorshift () = new Xorshift(seed()) :> System.Random
    let xorshiftWith seed threadSafe = new Xorshift(seed, threadSafe) :> System.Random
    let xorshiftCustom seed threadSafe a c x1 x2 = new Xorshift(seed, threadSafe, a, c, x1, x2) :> System.Random

    /// Creates a Wichmann-Hill’s 1982 combined multiplicative congruential pRNG with a custom seed based on uinque GUIDs
    let wh1982 () = new WH1982(seed()) :> System.Random
    let wh1982With seed threadSafe = new WH1982(seed, threadSafe) :> System.Random

    /// Creates a Wichmann-Hill’s 2006 combined multiplicative congruential pRNG with a custom seed based on uinque GUIDs
    let wh2006 () = new WH2006(seed()) :> System.Random
    let wh2006With seed threadSafe = new WH2006(seed, threadSafe) :> System.Random

    /// Creates a Parallel Additive Lagged Fibonacci pRNG with a custom seed based on uinque GUIDs
    let palf () = new Palf(seed()) :> System.Random
    let palfWith seed threadSafe = new Palf(seed, threadSafe, 418, 1279) :> System.Random
    let palfCustom seed threadSafe shortLag longLag = new Palf(seed, threadSafe, shortLag, longLag) :> System.Random

    /// Creates a Multiplicative congruential generator using a modulus of 2^59 and a multiplier of 13^13 pRNG with a custom seed based on uinque GUIDs
    let mcg59 () = new Mcg59(seed()) :> System.Random
    let mcg59With seed threadSafe = new Mcg59(seed, threadSafe) :> System.Random

    /// Creates a Multiplicative congruential generator using a modulus of 2^31-1 and a multiplier of 1132489760 pRNG with a custom seed based on uinque GUIDs
    let mcg31m1 () = new Mcg31m1(seed()) :> System.Random
    let mcg31m1With seed threadSafe = new Mcg31m1(seed, threadSafe) :> System.Random

    /// Creates a 32-bit combined multiple recursive generator with 2 components of order 3 pRNG with a custom seed based on uinque GUIDs
    let mrg32k3a () = new Mrg32k3a(seed()) :> System.Random
    let mrg32k3aWith seed threadSafe = new Mrg32k3a(seed, threadSafe) :> System.Random
