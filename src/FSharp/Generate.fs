// <copyright file="Generate.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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

namespace MathNet.Numerics

open System

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Generate =

    let inline private tobcl (f:'a->'b) = Func<'a,'b>(f)
    let inline private tobcl2 (f:'a->'b->'c) = Func<'a,'b,'c>(f)

    let inline map map points = Array.map map points
    let inline mapSeq map points = Seq.map map points

    let inline map2 map pointsA pointsB = Array.map2 map pointsA pointsB
    let inline map2Seq map pointsA pointsB = Seq.map2 map pointsA pointsB

    let inline linearSpacedMap length start stop map = Generate.LinearSpacedMap(length, start, stop, tobcl map)
    let inline logSpacedMap length startExp stopExp map = Generate.LogSpacedMap(length, startExp, stopExp, tobcl map)
    let inline linearRangeMap start step stop map = [| for x in start .. step .. stop -> map x |]

    let inline periodicMap length map samplingRate frequency amplitude phase delay =
        Generate.PeriodicMap(length, tobcl map, samplingRate, frequency, amplitude, phase, delay)
    let inline periodicMapSeq map samplingRate frequency amplitude phase delay =
        Generate.PeriodicMapSequence(tobcl map, samplingRate, frequency, amplitude, phase, delay)

    let inline randomMap length distribution map = Generate.RandomMap(length, distribution, tobcl map)
    let inline randomMapSeq distribution map = Generate.RandomMapSequence(distribution, tobcl map)
    let inline randomMap2 length distribution map = Generate.RandomMap2(length, distribution, tobcl2 map)
    let inline randomMap2Seq distribution map = Generate.RandomMap2Sequence(distribution, tobcl2 map)

    let inline uniformMap length map = Generate.UniformMap(length, tobcl map)
    let inline uniformMapSeq map = Generate.UniformMapSequence(tobcl map)
    let inline uniformMap2 length map = Generate.UniformMap2(length, tobcl2 map)
    let inline uniformMap2Seq map = Generate.UniformMap2Sequence(tobcl2 map)
