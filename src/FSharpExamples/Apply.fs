// <copyright file="Apply.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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
module MathNet.Numerics.FSharp.Examples.Apply

open System.Numerics
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic
/// Flag to specify wether we want pretty printing or tab separated output.
let prettyPrint = false

/// The size of the vector we want to map things for.
let N = 1000000
/// The number of times we repeat a call.
let T = 10
/// The list of all functions we want to test.
let FunctionList : (string * (float -> float) * (float -> float)) [] =
    [| ("Cosine", cos, System.Math.Cos);
       ("Sine", sin, System.Math.Sin);
       ("Tangent", tan, System.Math.Tan);
       ("Inverse Cosine", acos, System.Math.Acos);
       ("Inverse Sine", asin, System.Math.Asin);
       ("Inverse Tangent", atan, System.Math.Atan);
       ("Hyperbolic Cosine", cosh, System.Math.Cosh);
       ("Hyperbolic Sine", sinh, System.Math.Sinh);
       ("Hyperbolic Tangent", tanh, System.Math.Tanh);
       ("Abs", abs, System.Math.Abs);
       ("Exp", exp, System.Math.Exp);
       ("Log", log, System.Math.Log);
       ("Sqrt", sqrt, System.Math.Sqrt);
       ("Error Function", SpecialFunctions.Erf, SpecialFunctions.Erf);
       ("Error Function Complement", SpecialFunctions.Erfc, SpecialFunctions.Erfc);
       ("Inverse Error Function", SpecialFunctions.ErfInv, SpecialFunctions.ErfInv);
       ("Inverse Error Function Complement", SpecialFunctions.ErfcInv, SpecialFunctions.ErfcInv) |]

/// A vector with random entries.
let w =
    let rnd = new Random.MersenneTwister()
    (new DenseVector(Array.init N (fun _ -> rnd.NextDouble() * 10.0))) :> Vector<float>

/// A stopwatch to time the execution.
let sw = new System.Diagnostics.Stopwatch()


for (name, fs, dotnet) in FunctionList do
    if prettyPrint then printfn "Running %s on an %d dimensional vector for %d iterations:" name N T
    else printf "%s" name

    /// Perform the standard F# map function.
    do
        let v = w.Clone()
        sw.Start()
        for t in 1 .. T do Vector.mapInPlace fs v
        sw.Stop()
        if prettyPrint then printfn "\tVector.map (F#): %d milliseconds." sw.ElapsedMilliseconds
        else printf "\t%d" sw.ElapsedMilliseconds
        sw.Reset()
    (*
    /// Perform the Apply.Map function.
    do
        let v = w.Clone()
        sw.Start()
        for t in 1 .. T do v.Map(fun x -> dotnet x)
        sw.Stop()
        if prettyPrint then printfn "\tApply.Map (MKL): %d milliseconds." sw.ElapsedMilliseconds
        else printf "\t%d" sw.ElapsedMilliseconds
        sw.Reset()*)

    printfn ""