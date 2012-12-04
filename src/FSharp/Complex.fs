// First version copied from the F# Power Pack
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/math/complex.fs
// (c) Microsoft Corporation 2005-2009.

namespace MathNet.Numerics

    open Microsoft.FSharp.Math
    open System
    open System.Globalization
    open System.Numerics

    type complex = Complex

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Complex =

        let mkRect(a,b) = new Complex(a,b)
        let mkPolar(a,b) = Complex.FromPolarCoordinates(a,b)
        let cis b = mkPolar(1.0,b)

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
        let pow (power:Complex) x = Complex.Pow(x,power)
        let powf (power:float) x = Complex.Pow(x,power)
        let sqr (x:Complex) = x.Square()
        let sqrt (x:Complex) = x.SquareRoot() // numerically more stable than Complex.Sqrt

        let sin x = Complex.Sin(x)
        let cos x = Complex.Cos(x)
        let tan x = Complex.Tan(x)
        let asin x = Complex.Asin(x)
        let acos x = Complex.Acos(x)
        let atan x = Complex.Atan(x)
        let sinh x = Complex.Sinh(x)
        let cosh x = Complex.Cosh(x)
        let tanh x = Complex.Tanh(x)

        let sec (x:Complex) = Trig.Secant(x)
        let csc (x:Complex) = Trig.Cosecant(x)
        let cot (x:Complex) = Trig.Cotangent(x)
        let asec (x:Complex) = Trig.InverseSecant(x)
        let acsc (x:Complex) = Trig.InverseCosecant(x)
        let acot (x:Complex) = Trig.InverseCotangent(x)
        let sech (x:Complex) = Trig.HyperbolicSecant(x)
        let csch (x:Complex) = Trig.HyperbolicCosecant(x)
        let coth (x:Complex) = Trig.HyperbolicCotangent(x)

        let fmt_of_string numstyle fmtprovider (s:string) =
            mkRect (System.Double.Parse(s,numstyle,fmtprovider),0.0)
        let of_string s = fmt_of_string NumberStyles.Any CultureInfo.InvariantCulture s

    [<AutoOpen>]
    module ComplexExtensions =

        let complex x y = Complex.mkRect (x,y)

        type Complex with
            member x.r = x.Real
            member x.i = x.Imaginary

            static member Create(a,b) = Complex.mkRect (a,b)
            static member CreatePolar(a,b) = Complex.mkPolar (a,b)