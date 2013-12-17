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

    /// Returns the default mersenne twister, thread-safe and also thread-locally shared
    let shared = MersenneTwister.Default :> System.Random
    let samples length = MersenneTwister.Samples(length, RandomSeed.Guid())
    let sampleSeq () = MersenneTwister.SampleSequence(RandomSeed.Guid())

    /// Creates a default .Net system pRNG with a custom seed based on uinque GUIDs
    let system () = new SystemRandomSource() :> System.Random
    let systemSeed (seed:int) = new SystemRandomSource(seed) :> System.Random
    let systemSamples (seed:int) length = SystemRandomSource.Samples(length, seed)
    let systemSampleSeq (seed:int) = SystemRandomSource.SampleSequence(seed)

#if PORTABLE
#else
    /// Creates a default .Net cryptographic system pRNG
    let crypto () = new CryptoRandomSource() :> System.Random
    let cryptoWith (threadSafe:bool) = new CryptoRandomSource(threadSafe) :> System.Random
    let cryptoSamples length = CryptoRandomSource.Samples(length)
    let cryptoSampleSeq () = CryptoRandomSource.SampleSequence()
#endif

    /// Creates a Mersenne Twister 19937 pRNG with a custom seed based on uinque GUIDs
    let mersenneTwister () = new MersenneTwister() :> System.Random
    let mersenneTwisterSeed (seed:int) = new MersenneTwister(seed) :> System.Random
    let mersenneTwisterWith seed threadSafe = new MersenneTwister(seed, threadSafe) :> System.Random
    let mersenneTwisterSamples (seed:int) length = MersenneTwister.Samples(length, seed)
    let mersenneTwisterSampleSeq (seed:int) = MersenneTwister.SampleSequence(seed)

    /// Creates a multiply-with-carry Xorshift (Xn = a * Xn−3 + c mod 2^32) pRNG with a custom seed based on uinque GUIDs
    let xorshift () = new Xorshift() :> System.Random
    let xorshiftSeed (seed:int) = new Xorshift(seed) :> System.Random
    let xorshiftWith seed threadSafe = new Xorshift(seed, threadSafe) :> System.Random
    let xorshiftCustom seed threadSafe a c x1 x2 = new Xorshift(seed, threadSafe, a, c, x1, x2) :> System.Random
    let xorshiftSamples (seed:int) length = Xorshift.Samples(length, seed)
    let xorshiftSampleSeq (seed:int) = Xorshift.SampleSequence(seed)

    /// Creates a Wichmann-Hill’s 1982 combined multiplicative congruential pRNG with a custom seed based on uinque GUIDs
    let wh1982 () = new WH1982() :> System.Random
    let wh1982Seed (seed:int) = new WH1982(seed) :> System.Random
    let wh1982With seed threadSafe = new WH1982(seed, threadSafe) :> System.Random
    let wh1982Samples (seed:int) length = WH1982.Samples(length, seed)
    let wh1982SampleSeq (seed:int) = WH1982.SampleSequence(seed)

    /// Creates a Wichmann-Hill’s 2006 combined multiplicative congruential pRNG with a custom seed based on uinque GUIDs
    let wh2006 () = new WH2006() :> System.Random
    let wh2006Seed (seed:int) = new WH2006(seed) :> System.Random
    let wh2006With seed threadSafe = new WH2006(seed, threadSafe) :> System.Random
    let wh2006Samples (seed:int) length = WH2006.Samples(length, seed)
    let wh2006SampleSeq (seed:int) = WH2006.SampleSequence(seed)

    /// Creates a Parallel Additive Lagged Fibonacci pRNG with a custom seed based on uinque GUIDs
    let palf () = new Palf() :> System.Random
    let palfSeed (seed:int) = new Palf(seed) :> System.Random
    let palfWith seed threadSafe = new Palf(seed, threadSafe, 418, 1279) :> System.Random
    let palfCustom seed threadSafe shortLag longLag = new Palf(seed, threadSafe, shortLag, longLag) :> System.Random
    let palfSamples (seed:int) length = Palf.Samples(length, seed)
    let palfSampleSeq (seed:int) = Palf.SampleSequence(seed)

    /// Creates a Multiplicative congruential generator using a modulus of 2^59 and a multiplier of 13^13 pRNG with a custom seed based on uinque GUIDs
    let mcg59 () = new Mcg59() :> System.Random
    let mcg59Seed (seed:int) = new Mcg59(seed) :> System.Random
    let mcg59With seed threadSafe = new Mcg59(seed, threadSafe) :> System.Random
    let mcg59Samples (seed:int) length = Mcg59.Samples(length, seed)
    let mcg59SampleSeq (seed:int) = Mcg59.SampleSequence(seed)

    /// Creates a Multiplicative congruential generator using a modulus of 2^31-1 and a multiplier of 1132489760 pRNG with a custom seed based on uinque GUIDs
    let mcg31m1 () = new Mcg31m1() :> System.Random
    let mcg31m1Seed (seed:int) = new Mcg31m1(seed) :> System.Random
    let mcg31m1With seed threadSafe = new Mcg31m1(seed, threadSafe) :> System.Random
    let mcg31m1Samples (seed:int) length = Mcg31m1.Samples(length, seed)
    let mcg31m1SampleSeq (seed:int) = Mcg31m1.SampleSequence(seed)

    /// Creates a 32-bit combined multiple recursive generator with 2 components of order 3 pRNG with a custom seed based on uinque GUIDs
    let mrg32k3a () = new Mrg32k3a() :> System.Random
    let mrg32k3aSeed (seed:int) = new Mrg32k3a(seed) :> System.Random
    let mrg32k3aWith seed threadSafe = new Mrg32k3a(seed, threadSafe) :> System.Random
    let mrg32k3aSamples (seed:int) length = Mrg32k3a.Samples(length, seed)
    let mrg32k3aSampleSeq (seed:int) = Mrg32k3a.SampleSequence(seed)
