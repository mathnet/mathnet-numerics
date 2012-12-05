// First version copied from the F# Power Pack
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack/math/complex.fsi
// (c) Microsoft Corporation 2005-2009.

namespace MathNet.Numerics

    open System
    open System.Numerics

    /// The type of complex numbers
    type complex = Complex
    type complex32 = Complex32

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
        val pi : complex

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
        val exp : complex -> complex
        /// ln(x) is natural log (base e)
        val ln : complex -> complex
        /// log10(x) is common log (base 10)
        val log10 : complex -> complex
        /// log(base,x) is log with custom base
        val log : float -> complex -> complex
        /// pow(power,x) is the complex power
        val pow : complex -> complex -> complex
        /// pow(power,x) is the float power
        val powf : float -> complex -> complex
        /// sqr(x) is the square (power 2)
        val sqr : complex -> complex
        /// sqrt(x) and 0 <= phase(x) < pi
        val sqrt : complex -> complex

        /// Sine
        val sin : complex -> complex
        /// Cosine
        val cos : complex -> complex
        /// Tagent
        val tan : complex -> complex
        /// Arc Sine
        val asin : complex -> complex
        /// Arc Cosine
        val acos : complex -> complex
        /// Arc Tagent
        val atan : complex -> complex
        /// Hyperbolic Sine
        val sinh : complex -> complex
        /// Hyperbolic Cosine
        val cosh : complex -> complex
        /// Hyperbolic Tagent
        val tanh : complex -> complex

        /// Secant
        val sec : complex -> complex
        /// Cosecant
        val csc : complex -> complex
        /// Cotangent
        val cot : complex -> complex
        /// Arc Secant
        val asec : complex -> complex
        /// Arc Cosecant
        val acsc : complex -> complex
        /// Arc Cotangent
        val acot : complex -> complex
        /// Hyperbolic Secant
        val sech : complex -> complex
        /// Hyperbolic Cosecant
        val csch : complex -> complex
        /// Hyperbolic Cotangent
        val coth : complex -> complex

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Complex32 =

        /// Create a complex number using real and imaginary parts
        val mkRect : float32 * float32 -> complex32
        /// Create a complex number using magnitude/phase polar coordinates
        val mkPolar : float32 * float32 -> complex32
        /// A complex of magnitude 1 and the given phase and , i.e. cis x = mkPolar 1.0 x
        val cis : float32 -> complex32

        /// The complex number 0+0i
        val zero : complex32
        /// The complex number 1+0i
        val one : complex32
        /// The complex number 0+1i
        val onei : complex32
        /// pi
        val pi : complex32

        /// The real part of a complex number
        val realPart : complex32 -> float32
        /// The imaginary part of a complex number
        val imagPart : complex32 -> float32
        /// The polar-coordinate magnitude of a complex number
        val magnitude : complex32 -> float32
        /// The polar-coordinate phase of a complex number
        val phase : complex32 -> float32

        /// Unary negation of a complex number
        val neg : complex32 -> complex32
        /// The conjugate of a complex number, i.e. x-yi
        val conjugate : complex32 -> complex32

        /// Add two complex numbers
        val add : complex32 -> complex32 -> complex32
        /// Subtract one complex number from another
        val sub : complex32 -> complex32 -> complex32
        /// Multiply two complex numbers
        val mul : complex32 -> complex32 -> complex32
        /// Complex division of two complex numbers
        val div : complex32 -> complex32 -> complex32

        /// Multiply a scalar by a complex number
        val smul : float32 -> complex32 -> complex32
        /// Multiply a complex number by a scalar
        val muls : complex32 -> float32 -> complex32

        /// exp(x) = e^x
        val exp : complex32 -> complex32
        /// ln(x) is natural log (base e)
        val ln : complex32 -> complex32
        /// log10(x) is common log (base 10)
        val log10 : complex32 -> complex32
        /// log(base,x) is log with custom base
        val log : float32 -> complex32 -> complex32
        /// pow(power,x) is the complex power
        val pow : complex32 -> complex32 -> complex32
        /// pow(power,x) is the float power
        val powf : float32 -> complex32 -> complex32
        /// sqr(x) is the square (power 2)
        val sqr : complex32 -> complex32
        /// sqrt(x) and 0 <= phase(x) < pi
        val sqrt : complex32 -> complex32

        /// Sine
        val sin : complex32 -> complex32
        /// Cosine
        val cos : complex32 -> complex32
        /// Tagent
        val tan : complex32 -> complex32
        /// Arc Sine
        val asin : complex32 -> complex32
        /// Arc Cosine
        val acos : complex32 -> complex32
        /// Arc Tagent
        val atan : complex32 -> complex32
        /// Hyperbolic Sine
        val sinh : complex32 -> complex32
        /// Hyperbolic Cosine
        val cosh : complex32 -> complex32
        /// Hyperbolic Tagent
        val tanh : complex32 -> complex32

        /// Secant
        val sec : complex32 -> complex32
        /// Cosecant
        val csc : complex32 -> complex32
        /// Cotangent
        val cot : complex32 -> complex32
        /// Arc Secant
        val asec : complex32 -> complex32
        /// Arc Cosecant
        val acsc : complex32 -> complex32
        /// Arc Cotangent
        val acot : complex32 -> complex32
        /// Hyperbolic Secant
        val sech : complex32 -> complex32
        /// Hyperbolic Cosecant
        val csch : complex32 -> complex32
        /// Hyperbolic Cotangent
        val coth : complex32 -> complex32

    [<AutoOpen>]
    module ComplexExtensions =

        /// Constructs a double precision complex number from both the real and imaginary part.
        val complex : float -> float -> complex

        /// Constructs a single precision complex number from both the real and imaginary part.
        val complex32 : float32 -> float32 -> complex32

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

        /// The type of complex numbers stored as pairs of 32-bit floating point numbers in rectangular coordinates
        type Complex32 with

            /// Create a complex number x+ij using rectangular coordinates
            static member Create      : float32 * float32 -> Complex32
            /// Create a complex number using magnitude/phase polar coordinates
            static member CreatePolar : float32 * float32 -> Complex32

            /// The real part of a complex number
            member r: float32
            /// The imaginary part of a complex number
            member i: float32
