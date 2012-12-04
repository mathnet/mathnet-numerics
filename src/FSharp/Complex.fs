// First version copied from the F# Power Pack
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/math/complex.fs
// (c) Microsoft Corporation 2005-2009.

namespace MathNet.Numerics

    open Microsoft.FSharp.Math
    open System
    open System.Globalization
    open System.Numerics

    type complex = Complex
    type complex32 = Complex32

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Complex =

        let mkRect(a,b) = new Complex(a,b)
        let mkPolar(a,b) = Complex.FromPolarCoordinates(a,b)
        let cis b = mkPolar(1.0,b)
        let ofComplex32 (x:complex32) = new Complex(float x.Real, float x.Imaginary)

        let zero = Complex.Zero
        let one = Complex.One
        let onei = Complex.ImaginaryOne
        let pi = mkRect (Math.PI,0.0)

        let realPart (c:complex) = c.Real
        let imagPart (c:complex) = c.Imaginary
        let magnitude (c:complex) = c.Magnitude
        let phase (c:complex) = c.Phase

        let neg (a:complex) = -a
        let conjugate (c:complex) = c.Conjugate()

        let add (a:complex) (b:complex) = a + b
        let sub (a:complex) (b:complex) = a - b
        let mul (a:complex) (b:complex) = a * b
        let div (x:complex) (y:complex) = x / y

        let smul (a:float) (b:complex) = new Complex(a * b.Real, a * b.Imaginary)
        let muls (a:complex) (b:float) = new Complex(a.Real * b, a.Imaginary * b)

        let exp (x:complex) = Complex.Exp(x)
        let ln x = Complex.Log(x)
        let log10 x = Complex.Log10(x)
        let log b x = Complex.Log(x,b)
        let pow (power:complex) x = Complex.Pow(x,power)
        let powf (power:float) x = Complex.Pow(x,power)
        let sqr (x:complex) = x.Square()
        let sqrt (x:complex) = x.SquareRoot() // numerically more stable than Complex.Sqrt

        let sin x = Complex.Sin(x)
        let cos x = Complex.Cos(x)
        let tan x = Complex.Tan(x)
        let asin x = Complex.Asin(x)
        let acos x = Complex.Acos(x)
        let atan x = Complex.Atan(x)
        let sinh x = Complex.Sinh(x)
        let cosh x = Complex.Cosh(x)
        let tanh x = Complex.Tanh(x)

        let sec (x:complex) = Trig.Secant(x)
        let csc (x:complex) = Trig.Cosecant(x)
        let cot (x:complex) = Trig.Cotangent(x)
        let asec (x:complex) = Trig.InverseSecant(x)
        let acsc (x:complex) = Trig.InverseCosecant(x)
        let acot (x:complex) = Trig.InverseCotangent(x)
        let sech (x:complex) = Trig.HyperbolicSecant(x)
        let csch (x:complex) = Trig.HyperbolicCosecant(x)
        let coth (x:complex) = Trig.HyperbolicCotangent(x)

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Complex32 =

        let mkRect(a,b) = new Complex32(a,b)
        let mkPolar(a,b) = Complex32.FromPolarCoordinates(a,b)
        let cis b = mkPolar(1.0f,b)
        let ofComplex (x:complex) = new Complex32(float32 x.Real, float32 x.Imaginary)

        let zero = Complex32.Zero
        let one = Complex32.One
        let onei = Complex32.ImaginaryOne
        let pi = mkRect (float32 Math.PI,0.0f)

        let realPart (c:complex32) = c.Real
        let imagPart (c:complex32) = c.Imaginary
        let magnitude (c:complex32) = c.Magnitude
        let phase (c:complex32) = c.Phase

        let neg (a:complex32) = -a
        let conjugate (c:complex32) = c.Conjugate()

        let add (a:complex32) (b:complex32) = a + b
        let sub (a:complex32) (b:complex32) = a - b
        let mul (a:complex32) (b:complex32) = a * b
        let div (x:complex32) (y:complex32) = x / y

        let smul (a:float32) (b:complex32) = new Complex32(a * b.Real, a * b.Imaginary)
        let muls (a:complex32) (b:float32) = new Complex32(a.Real * b, a.Imaginary * b)

        let exp (x:complex32) = Complex32.Exp(x)
        let ln x = Complex32.Log(x)
        let log10 x = Complex32.Log10(x)
        let log b x = Complex32.Log(x,b)
        let pow (power:complex32) x = Complex32.Pow(x,power)
        let powf (power:float32) x = Complex32.Pow(x,power)
        let sqr (x:complex32) = x.Square()
        let sqrt (x:complex32) = x.SquareRoot() // numerically more stable than Complex.Sqrt

        let sin x = Complex32.Sin(x)
        let cos x = Complex32.Cos(x)
        let tan x = Complex32.Tan(x)
        let asin x = Complex32.Asin(x)
        let acos x = Complex32.Acos(x)
        let atan x = Complex32.Atan(x)
        let sinh x = Complex32.Sinh(x)
        let cosh x = Complex32.Cosh(x)
        let tanh x = Complex32.Tanh(x)

        // no complex32 implementations available yet, fix once available
        let sec (x:complex32) = ofComplex <| Trig.Secant(x.ToComplex())
        let csc (x:complex32) = ofComplex <| Trig.Cosecant(x.ToComplex())
        let cot (x:complex32) = ofComplex <| Trig.Cotangent(x.ToComplex())
        let asec (x:complex32) = ofComplex <| Trig.InverseSecant(x.ToComplex())
        let acsc (x:complex32) = ofComplex <| Trig.InverseCosecant(x.ToComplex())
        let acot (x:complex32) = ofComplex <| Trig.InverseCotangent(x.ToComplex())
        let sech (x:complex32) = ofComplex <| Trig.HyperbolicSecant(x.ToComplex())
        let csch (x:complex32) = ofComplex <| Trig.HyperbolicCosecant(x.ToComplex())
        let coth (x:complex32) = ofComplex <| Trig.HyperbolicCotangent(x.ToComplex())

    [<AutoOpen>]
    module ComplexExtensions =

        let complex x y = Complex.mkRect (x,y)
        let complex32 x y = Complex32.mkRect (x,y)

        type Complex with
            member x.r = x.Real
            member x.i = x.Imaginary

            static member Create(a,b) = Complex.mkRect (a,b)
            static member CreatePolar(a,b) = Complex.mkPolar (a,b)

        type Complex32 with
            member x.r = x.Real
            member x.i = x.Imaginary

            static member Create(a,b) = Complex32.mkRect (a,b)
            static member CreatePolar(a,b) = Complex32.mkPolar (a,b)
