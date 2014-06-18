#I "../../out/lib/Net40"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"

// ***MathNet.Numerics.fsx*** (DO NOT REMOVE THIS COMMENT, everything below is copied to the output)

fsi.AddPrinter(fun (matrix:MathNet.Numerics.LinearAlgebra.Matrix<_>) -> matrix.ToString())
fsi.AddPrinter(fun (vector:MathNet.Numerics.LinearAlgebra.Vector<_>) -> vector.ToString())
