#I "../../out/lib/Net40"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"

// ***MathNet.Numerics.IfSharp.fsx*** (DO NOT REMOVE THIS COMMENT, everything below is copied to the output)

// This file is intended for the IfSharp F# profile for iPython only. See:
// http://numerics.mathdotnet.com/docs/IFSharpNotebook.html
// http://github.com/BayardRock/IfSharp
// http://ipython.org/

// Assumption: MathNet.Numerics and MathNet.Numerics.FSharp have been referenced already, using
// #N "MathNet.Numerics"
// #N "MathNet.Numerics.FSharp"

open System
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra

let inline (|Float|_|) (v:obj) = if v :? float then Some(v :?> float) else None
let inline (|Float32|_|) (v:obj) = if v :? float32 then Some(v :?> float32) else None
let inline (|PositiveInfinity|_|) (v: ^T) = if (^T : (static member IsPositiveInfinity: 'T -> bool) (v)) then Some PositiveInfinity else None
let inline (|NegativeInfinity|_|) (v: ^T) = if (^T : (static member IsNegativeInfinity: 'T -> bool) (v)) then Some NegativeInfinity else None
let inline (|NaN|_|) (v: ^T) = if (^T : (static member IsNaN: 'T -> bool) (v)) then Some NaN else None

let inline formatMathValue (floatFormat:string) = function
  | PositiveInfinity -> "\\infty"
  | NegativeInfinity -> "-\\infty"
  | NaN -> "\\times"
  | Float v -> v.ToString(floatFormat)
  | Float32 v -> v.ToString(floatFormat)
  | v -> v.ToString()

let inline formatMatrix (matrix: Matrix<'T>) =
  String.concat Environment.NewLine
    [ "\\begin{bmatrix}"
      matrix.ToMatrixString(10, 4, 7, 2, "\\cdots", "\\vdots", "\\ddots", " & ", "\\\\ " + Environment.NewLine, (fun x -> formatMathValue "G4" x))
      "\\end{bmatrix}" ]

let inline formatVector (vector: Vector<'T>) =
  String.concat Environment.NewLine
    [ "\\begin{bmatrix}"
      vector.ToVectorString(12, 80, "\\vdots", " & ", "\\\\ " + Environment.NewLine, (fun x -> formatMathValue "G4" x))
      "\\end{bmatrix}" ]
      
App.AddDisplayPrinter (fun (x:Matrix<float>) -> { ContentType = "text/latex"; Data = formatMatrix x })
App.AddDisplayPrinter (fun (x:Matrix<float32>) -> { ContentType = "text/latex"; Data = formatMatrix x })
App.AddDisplayPrinter (fun (x:Vector<float>) -> { ContentType = "text/latex"; Data = formatVector x })
App.AddDisplayPrinter (fun (x:Vector<float32>) -> { ContentType = "text/latex"; Data = formatVector x })
