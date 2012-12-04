// First version copied from the F# Power Pack
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/math/complex.fsi
// (c) Microsoft Corporation 2005-2009.

namespace MathNet.Numerics

    open System
    open System.Numerics

    /// The type of complex numbers
    type complex = Complex

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Complex =

        /// Create a complex number using real and imaginary parts
        val mkRect : float * float -> complex
        /// Create a complex number using magnitude/phase polar coordinates
        val mkPolar : float * float -> complex
        /// A complex of magnitude 1 and the given phase and , i.e. cis x = mkPolar 1.0 x
        val cis : float -> complex

        /// The complex number 0+0i
        val zero : complex
        /// The complex number 1+0i
        val one : complex
        /// The complex number 0+1i
        val onei : complex
        /// pi
        val pi : Complex

        /// The real part of a complex number
        val realPart : complex -> float
        /// The imaginary part of a complex number
        val imagPart : complex -> float
        /// The polar-coordinate magnitude of a complex number
        val magnitude : complex -> float
        /// The polar-coordinate phase of a complex number
        val phase : complex -> float

        /// Unary negation of a complex number
        val neg : complex -> complex
        /// The conjugate of a complex number, i.e. x-yi
        val conjugate : complex -> complex

        /// Add two complex numbers
        val add : complex -> complex -> complex
        /// Subtract one complex number from another
        val sub : complex -> complex -> complex
        /// Multiply two complex numbers
        val mul : complex -> complex -> complex
        /// Complex division of two complex numbers
        val div : complex -> complex -> complex
        
        /// Multiply a scalar by a complex number
        val smul : float -> complex -> complex
        /// Multiply a complex number by a scalar
        val muls : complex -> float -> complex

        /// exp(x) = e^x
        val exp : Complex -> Complex
        /// ln(x) is natural log (base e)
        val ln : Complex -> Complex
        /// log10(x) is common log (base 10)
        val log10 : Complex -> Complex
        /// log(base,x) is log with custom base
        val log : float -> Complex -> Complex
        /// pow(power,x) is the complex power
        val pow : Complex -> Complex -> Complex
        /// pow(power,x) is the float power
        val powf : float -> Complex -> Complex
        /// sqr(x) is the square (power 2)
        val sqr : Complex -> Complex
        /// sqrt(x) and 0 <= phase(x) < pi
        val sqrt : Complex -> Complex

        /// Sine
        val sin : Complex -> Complex
        /// Cosine
        val cos : Complex -> Complex
        /// Tagent
        val tan : Complex -> Complex
        /// Arc Sine
        val asin : Complex -> Complex
        /// Arc Cosine
        val acos : Complex -> Complex
        /// Arc Tagent
        val atan : Complex -> Complex
        /// Hyperbolic Sine
        val sinh : Complex -> Complex
        /// Hyperbolic Cosine
        val cosh : Complex -> Complex
        /// Hyperbolic Tagent
        val tanh : Complex -> Complex

        /// Secant
        val sec : Complex -> Complex
        /// Cosecant
        val csc : Complex -> Complex
        /// Cotangent
        val cot : Complex -> Complex
        /// Arc Secant
        val asec : Complex -> Complex
        /// Arc Cosecant
        val acsc : Complex -> Complex
        /// Arc Cotangent
        val acot : Complex -> Complex
        /// Hyperbolic Secant
        val sech : Complex -> Complex
        /// Hyperbolic Cosecant
        val csch : Complex -> Complex
        /// Hyperbolic Cotangent
        val coth : Complex -> Complex

    [<AutoOpen>]
    module ComplexExtensions =

        /// Constructs a complex number from both the real and imaginary part.
        val complex : float -> float -> complex

        /// The type of complex numbers stored as pairs of 64-bit floating point numbers in rectangular coordinates
        type Complex with

            /// Create a complex number x+ij using rectangular coordinates
            static member Create      : float * float -> Complex
            /// Create a complex number using magnitude/phase polar coordinates
            static member CreatePolar : float * float -> Complex

            /// The real part of a complex number
            member r: float
            /// The imaginary part of a complex number
            member i: float