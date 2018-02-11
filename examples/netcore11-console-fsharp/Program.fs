open System
open System.Diagnostics
open MathNet.Numerics
open MathNet.Numerics.IntegralTransforms
open MathNet.Numerics.LinearAlgebra

type Complex = System.Numerics.Complex

[<EntryPoint>]
let main argv =

    // Code touching all providers
    let matrix : Matrix<Complex> = DenseMatrix.randomSeed 10 10 100
    let vector = (matrix |> Matrix.svd).S
    vector.AsArray() |> Fourier.Forward
    printfn "%s" (Control.Describe())
    printfn "DC=%f; Low=%f; Hight=%f" vector.[0].Magnitude vector.[1].Magnitude vector.[5].Magnitude

    if Debugger.IsAttached then
        Console.ReadKey() |> ignore

    0
