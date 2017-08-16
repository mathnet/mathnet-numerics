    [hide]
    #I "../../out/lib/net40"
    #r "MathNet.Numerics.dll"
    #r "MathNet.Numerics.FSharp.dll"
    open System.Numerics
    open MathNet.Numerics
    open MathNet.Numerics.LinearAlgebra

IF# Notebook
============

[iPython](https://ipython.org/) provides a rich browser-based interactive notebook with support for code, text, mathematical expressions,
inline plots and other rich media. [IfSharp](https://github.com/BayardRock/IfSharp), developed by Bayard Rock, is an F# profile
for iPython with IntelliSense and embedded FSharp.Charting. Thanks to its NuGet support it can load other packages like Math.NET Numerics on demand.

![Screenshot](img/IfSharp-GenerateIS.png)


Installing IF# Notebook
-----------------------

Follow the instructions at [IfSharp/Installation](http://bayardrock.github.io/IfSharp/installation.html).

Essentially:

1. Install [Anaconda](https://continuum.io/downloads)
2. In a shell, run

    conda update conda
    conda update ipython

3. Install [IfSharp](https://github.com/BayardRock/IfSharp/releases).


Display Printers for Matrices and Vectors
-----------------------------------------

By itself IfSharp does not know how to display matrices and vectors in a nice way, but we can tell it how to do so by providing our own display printers for them.
Since v3.3 the Math.NET Numerics F# package includes a script `MathNet.Numerics.IfSharp.fsx` to do so.
Unfortunately loading this script requires the exact version in the path - if you know a way to avoid this please let us know.

![Screenshot](img/IfSharp-MatrixVector.png)

Alternatively you can also use the code below and adapt it to your needs, e.g. if you want it to show more rows.

    [lang=fsharp]
    open MathNet.Numerics.LinearAlgebra

    let inline (|Float|_|) (v:obj) =
        if v :? float then Some(v :?> float) else None
    let inline (|Float32|_|) (v:obj) =
        if v :? float32 then Some(v :?> float32) else None
    let inline (|PositiveInfinity|_|) (v: ^T) =
        if (^T : (static member IsPositiveInfinity: 'T -> bool) (v))
        then Some PositiveInfinity else None
    let inline (|NegativeInfinity|_|) (v: ^T) =
        if (^T : (static member IsNegativeInfinity: 'T -> bool) (v))
        then Some NegativeInfinity else None
    let inline (|NaN|_|) (v: ^T) =
        if (^T : (static member IsNaN: 'T -> bool) (v))
        then Some NaN else None

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
          matrix.ToMatrixString(10,4,7,2,"\\cdots","\\vdots","\\ddots",
            " & ", "\\\\ " + Environment.NewLine, (fun x -> formatMathValue "G4" x))
          "\\end{bmatrix}" ]

    let inline formatVector (vector: Vector<'T>) =
      String.concat Environment.NewLine
        [ "\\begin{bmatrix}"
          vector.ToVectorString(12, 80, "\\vdots", " & ", "\\\\ " + Environment.NewLine,
            (fun x -> formatMathValue "G4" x))
          "\\end{bmatrix}" ]

    App.AddDisplayPrinter (fun (x:Matrix<float>) ->
        { ContentType = "text/latex"; Data = formatMatrix x })
    App.AddDisplayPrinter (fun (x:Matrix<float32>) ->
        { ContentType = "text/latex"; Data = formatMatrix x })
    App.AddDisplayPrinter (fun (x:Vector<float>) ->
        { ContentType = "text/latex"; Data = formatVector x })
    App.AddDisplayPrinter (fun (x:Vector<float32>) ->
        { ContentType = "text/latex"; Data = formatVector x })
