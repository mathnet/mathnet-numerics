// First version copied from the F# Power Pack
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/math/complex.fs
// (c) Microsoft Corporation 2005-2009.

namespace MathNet.Numerics

open Microsoft.FSharp.Math
open System
open System.Globalization

#if NOSYSNUMERICS
#else
open System.Numerics
#endif

//
type complex = Complex
//
type complex32 = Complex32

//
[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Complex =
    /// Create a complex number using real and imaginary parts
    let mkRect(a,b) = Complex(a,b)

    /// Create a complex number using magnitude/phase polar coordinates
    let mkPolar(a,b) = Complex.FromPolarCoordinates(a,b)

    /// A complex of magnitude 1 and the given phase and , i.e. cis x = mkPolar 1.0 x
    let cis b = mkPolar(1.0,b)

    //
    let private ofComplex32 (x : complex32) =
        Complex(float x.Real, float x.Imaginary)

    /// The complex number 0+0i
    let zero = Complex.Zero

    /// The complex number 1+0i
    let one = Complex.One

    /// The complex number 0+1i
    let onei = Complex.ImaginaryOne

    /// pi
    let pi = mkRect (Math.PI,0.0)


    /// The real part of a complex number
    let realPart (c:complex) = c.Real

    /// The imaginary part of a complex number
    let imagPart (c:complex) = c.Imaginary

    /// The polar-coordinate magnitude of a complex number
    let magnitude (c:complex) = c.Magnitude

    /// The polar-coordinate phase of a complex number
    let phase (c:complex) = c.Phase


    /// Unary negation of a complex number
    let neg (a:complex) = -a

    /// The conjugate of a complex number, i.e. x-yi
    let conjugate (c:complex) = c.Conjugate()


    /// Add two complex numbers
    let add (a:complex) (b:complex) = a + b

    /// Subtract one complex number from another
    let sub (a:complex) (b:complex) = a - b

    /// Multiply two complex numbers
    let mul (a:complex) (b:complex) = a * b

    /// Complex division of two complex numbers
    let div (x:complex) (y:complex) = x / y


    /// Multiply a scalar by a complex number
    let smul (a:float) (b:complex) = new Complex(a * b.Real, a * b.Imaginary)

    /// Multiply a complex number by a scalar
    let muls (a:complex) (b:float) = new Complex(a.Real * b, a.Imaginary * b)


    /// exp(x) = e^x
    let exp (x:complex) = Complex.Exp(x)

    /// ln(x) is natural log (base e)
    let ln x = Complex.Log(x)

    /// log10(x) is common log (base 10)
    let log10 x = Complex.Log10(x)

    /// log(base,x) is log with custom base
    let log b x = Complex.Log(x,b)

    /// pow(power,x) is the complex power
    let pow (power : complex) x = Complex.Pow(x,power)

    /// pow(power,x) is the scalar power
    let powf (power : float) x = Complex.Pow(x,power)

    /// sqr(x) is the square (power 2)
    let sqr (x : complex) = x.Square()

    /// sqrt(x) and 0 <= phase(x) < pi
    let sqrt (x : complex) = x.SquareRoot() // numerically more stable than Complex.Sqrt


    /// Sine
    let sin x = Complex.Sin(x)

    /// Cosine
    let cos x = Complex.Cos(x)

    /// Tangent
    let tan x = Complex.Tan(x)

    /// Cotangent
    let cot (x : complex) = Trig.Cot(x)

    /// Secant
    let sec (x : complex) = Trig.Sec(x)

    /// Cosecant
    let csc (x : complex) = Trig.Csc(x)


    /// Arc Sine
    let asin (x : complex) =
        // numerically more stable than Complex.Asin
        Trig.Asin(x)

    /// Arc Cosine
    let acos (x : complex) =
        // numerically more stable than Complex.Acos
        Trig.Acos(x)

    /// Arc Tangent
    let atan x = Complex.Atan(x)

    /// Arc Cotangent
    let acot (x : complex) = Trig.Acot(x)

    /// Arc Secant
    let asec (x : complex) = Trig.Asec(x)

    /// Arc Cosecant
    let acsc (x : complex) = Trig.Acsc(x)


    /// Hyperbolic Sine
    let sinh x = Complex.Sinh(x)

    /// Hyperbolic Cosine
    let cosh x = Complex.Cosh(x)

    /// Hyperbolic Tangent
    let tanh x = Complex.Tanh(x)

    /// Hyperbolic Cotangent
    let coth (x : complex) = Trig.Coth(x)

    /// Hyperbolic Secant
    let sech (x : complex) = Trig.Sech(x)

    /// Hyperbolic Cosecant
    let csch (x : complex) = Trig.Csch(x)


    /// Inverse Hyperbolic Sine
    let asinh (x : complex) = Trig.Asinh(x)

    /// Inverse Hyperbolic Cosine
    let acosh (x : complex) = Trig.Acosh(x)

    /// Inverse Hyperbolic Tangent
    let atanh (x : complex) = Trig.Atanh(x)

    /// Inverse Hyperbolic Cotangent
    let acoth (x : complex) = Trig.Acoth(x)

    /// Inverse Hyperbolic Secant
    let asech (x : complex) = Trig.Asech(x)

    /// Inverse Hyperbolic Cosecant
    let acsch (x : complex) = Trig.Acsch(x)


//
[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Complex32 =
    /// Create a complex number using real and imaginary parts
    let mkRect(a,b) = new Complex32(a,b)

    /// Create a complex number using magnitude/phase polar coordinates
    let mkPolar(a,b) = Complex32.FromPolarCoordinates(a,b)

    /// A complex of magnitude 1 and the given phase and , i.e. cis x = mkPolar 1.0 x
    let cis b = mkPolar(1.0f,b)

    //
    let private ofComplex (x : complex) =
        Complex32 (float32 x.Real, float32 x.Imaginary)


    /// The complex number 0+0i
    let zero = Complex32.Zero

    /// The complex number 1+0i
    let one = Complex32.One

    /// The complex number 0+1i
    let onei = Complex32.ImaginaryOne

    /// pi
    let pi = mkRect (float32 Math.PI,0.0f)


    /// The real part of a complex number
    let realPart (c:complex32) = c.Real

    /// The imaginary part of a complex number
    let imagPart (c:complex32) = c.Imaginary

    /// The polar-coordinate magnitude of a complex number
    let magnitude (c:complex32) = c.Magnitude

    /// The polar-coordinate phase of a complex number
    let phase (c:complex32) = c.Phase


    /// Unary negation of a complex number
    let neg (a:complex32) = -a

    /// The conjugate of a complex number, i.e. x-yi
    let conjugate (c:complex32) = c.Conjugate()


    /// Add two complex numbers
    let add (a:complex32) (b:complex32) = a + b

    /// Subtract one complex number from another
    let sub (a:complex32) (b:complex32) = a - b

    /// Multiply two complex numbers
    let mul (a:complex32) (b:complex32) = a * b

    /// Complex division of two complex numbers
    let div (x:complex32) (y:complex32) = x / y


    /// Multiply a scalar by a complex number
    let smul (a:float32) (b:complex32) =
        Complex32(a * b.Real, a * b.Imaginary)

    /// Multiply a complex number by a scalar
    let muls (a:complex32) (b:float32) =
        Complex32(a.Real * b, a.Imaginary * b)


    /// exp(x) = e^x
    let exp (x:complex32) = Complex32.Exp(x)

    /// ln(x) is natural log (base e)
    let ln x = Complex32.Log(x)

    /// log10(x) is common log (base 10)
    let log10 x = Complex32.Log10(x)

    /// log(base,x) is log with custom base
    let log b x = Complex32.Log(x,b)

    /// pow(power,x) is the complex power
    let pow (power:complex32) x = Complex32.Pow(x,power)

    /// pow(power,x) is the scalar power
    let powf (power:float32) x = Complex32.Pow(x,power)


    /// sqr(x) is the square (power 2)
    let sqr (x:complex32) = x.Square()

    /// sqrt(x) and 0 <= phase(x) < pi
    let sqrt (x:complex32) =
        // numerically more stable than Complex.Sqrt
        x.SquareRoot()


    (* Complex32 implementations are not yet available for some of the functions below.
       TODO : Fix the functions below to use the Complex32 implementations once available. *)

    /// Sine
    let sin x = Complex32.Sin(x)

    /// Cosine
    let cos x = Complex32.Cos(x)

    /// Tangent
    let tan x = Complex32.Tan(x)

    /// Cotangent
    let cot (x:complex32) = ofComplex <| Trig.Cot(x.ToComplex())

    /// Secant
    let sec (x:complex32) = ofComplex <| Trig.Sec(x.ToComplex())

    /// Cosecant
    let csc (x:complex32) = ofComplex <| Trig.Csc(x.ToComplex())


    /// Arc Sine
    let asin (x:complex32) =
        // numerically more stable than Complex.Asin
        ofComplex <| Trig.Asin(x.ToComplex())

    /// Arc Cosine
    let acos (x:complex32) =
        // numerically more stable than Complex.Acos
        ofComplex <| Trig.Acos(x.ToComplex())

    /// Arc Tangent
    let atan x = Complex32.Atan(x)

    /// Arc Cotangent
    let acot (x:complex32) = ofComplex <| Trig.Acot(x.ToComplex())

    /// Arc Secant
    let asec (x:complex32) = ofComplex <| Trig.Asec(x.ToComplex())

    /// Arc Cosecant
    let acsc (x:complex32) = ofComplex <| Trig.Acsc(x.ToComplex())


    /// Hyperbolic Sine
    let sinh x = Complex32.Sinh(x)

    /// Hyperbolic Cosine
    let cosh x = Complex32.Cosh(x)

    /// Hyperbolic Tangent
    let tanh x = Complex32.Tanh(x)

    /// Hyperbolic Cotangent
    let coth (x:complex32) = ofComplex <| Trig.Coth(x.ToComplex())

    /// Hyperbolic Secant
    let sech (x:complex32) = ofComplex <| Trig.Sech(x.ToComplex())

    /// Hyperbolic Cosecant
    let csch (x:complex32) = ofComplex <| Trig.Csch(x.ToComplex())


    /// Inverse Hyperbolic Sine
    let asinh (x:complex32) = ofComplex <| Trig.Asinh(x.ToComplex())

    /// Inverse Hyperbolic Cosine
    let acosh (x:complex32) = ofComplex <| Trig.Acosh(x.ToComplex())

    /// Inverse Hyperbolic Tangent
    let atanh (x:complex32) = ofComplex <| Trig.Atanh(x.ToComplex())

    /// Inverse Hyperbolic Cotangent
    let acoth (x:complex32) = ofComplex <| Trig.Acoth(x.ToComplex())

    /// Inverse Hyperbolic Secant
    let asech (x:complex32) = ofComplex <| Trig.Asech(x.ToComplex())

    /// Inverse Hyperbolic Cosecant
    let acsch (x:complex32) = ofComplex <| Trig.Acsch(x.ToComplex())


//
[<AutoOpen>]
module ComplexExtensions =
    /// Constructs a double precision complex number from both the real and imaginary part.
    let complex x y =
        Complex.mkRect (x,y)

    /// Constructs a single precision complex number from both the real and imaginary part.
    let complex32 x y =
        Complex32.mkRect (x,y)

    // The type of complex numbers stored as pairs of 64-bit floating point numbers in rectangular coordinates
    type Complex with
        /// The real part of a complex number
        member x.r = x.Real
        /// The imaginary part of a complex number
        member x.i = x.Imaginary

        /// Create a complex number x+ij using rectangular coordinates
        static member Create(a,b) =
            Complex.mkRect (a,b)

        /// Create a complex number using magnitude/phase polar coordinates
        static member CreatePolar(a,b) =
            Complex.mkPolar (a,b)

    /// The type of complex numbers stored as pairs of 32-bit floating point numbers in rectangular coordinates
    type Complex32 with
        /// The real part of a complex number
        member x.r = x.Real
        /// The imaginary part of a complex number
        member x.i = x.Imaginary

        /// Create a complex number x+ij using rectangular coordinates
        static member Create(a,b) =
            Complex32.mkRect (a,b)

        /// Create a complex number using magnitude/phase polar coordinates
        static member CreatePolar(a,b) =
            Complex32.mkPolar (a,b)
