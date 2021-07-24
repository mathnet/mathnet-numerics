    [hide]
    #I "../../out/lib/net40"
    #r "MathNet.Numerics.dll"
    #r "MathNet.Numerics.FSharp.dll"
    open System.Numerics
    open MathNet.Numerics
    open MathNet.Numerics.IntegralTransforms

Fourier and related linear integral transforms
==============================================

Math.NET Numerics currently supports two linear integral transforms: The discrete Fourier
transform and the discrete Hartley transform. Both are strongly localized in the frequency
spectrum, but while the Fourier transform operates on complex values, the Hartley transform
operates on real values only.

The transforms implement a separate forward and inverse transform method.
How the forward and inverse methods are related to each other and what exact definition
is to be used can be specified by an additional _options_ parameter.


Fourier Space: Discrete Fourier Transform and FFT
-------------------------------------------------

Wikipedia has an extensive [article on the discrete Fourier transform (DFT)](https://en.wikipedia.org/wiki/Discrete_Fourier_transform).
We provide implementations of the following algorithms:

* *Naive Discrete Fourier Transform (DFT):* Out-place transform for arbitrary vector lengths. Mainly intended for verifying faster algorithms: _[NaiveForward](https://numerics.mathdotnet.com/api/MathNet.Numerics.IntegralTransforms/Fourier.htm#NaiveForward)_, _[NaiveInverse](https://numerics.mathdotnet.com/api/MathNet.Numerics.IntegralTransforms/Fourier.htm#NaiveInverse)_

* *Radix-2 Fast Fourier Transform (FFT):* In-place fast Fourier transform for vectors with a power-of-two length (Radix-2): _[Radix2Forward](https://numerics.mathdotnet.com/api/MathNet.Numerics.IntegralTransforms/Fourier.htm#Radix2Forward)_, _[url:Radix2Inverse](https://numerics.mathdotnet.com/api/MathNet.Numerics.IntegralTransforms/Fourier.htm#Radix2Inverse)_

* *Bluestein Fast Fourier Transform (FFT):* In-place fast Fourier transform for arbitrary vector lengths: _[BluesteinForward](https://numerics.mathdotnet.com/api/MathNet.Numerics.IntegralTransforms/Fourier.htm#BluesteinForward)_, _[url:BluesteinInverse](https://numerics.mathdotnet.com/api/MathNet.Numerics.IntegralTransforms/Fourier.htm#BluesteinInverse)_

Furthermore, the _[Transform](https://numerics.mathdotnet.com/api/MathNet.Numerics.IntegralTransforms/Fourier.htm)_ class provides a shortcut for the Bluestein FFT using static methods which are even easier to use: _[FourierForward](https://numerics.mathdotnet.com/api/MathNet.Numerics.IntegralTransforms/Transform.htm#FourierForward)_, _[FourierInverse](https://numerics.mathdotnet.com/api/MathNet.Numerics.IntegralTransforms/Transform.htm#FourierInverse)_.

Code Sample using the Transform class:

    [lang=csharp]
    // create a complex sample vector of length 96
    Complex[] samples = SignalGenerator.EquidistantInterval(
         t => new Complex(1.0 / (t * t + 1.0), t / (t * t + 1.0)),
         -16, 16, 96);

    // inplace bluestein FFT with default options
    Transform.FourierForward(samples);

Fourier Options:

* *Default:* Uses a negative exponent sign in forward transformations, and symmetric scaling (that is, sqrt(1/N) for both forward and inverse transformation). This is the convention used in Maple and is widely accepted in the educational sector (due to the symmetry).
* *AsymmetricScaling:* Set this flag to suppress scaling on the forward transformation but scale the inverse transform with 1/N.
* *NoScaling:* Set this flag to suppress scaling for both forward and inverse transformation. Note that in this case if you apply first the forward and then inverse transformation you won't get back the original signal (by factor N/2).
* *InverseExponent:* Uses the positive instead of the negative sign in the forward exponent, and the negative (instead of positive) exponent in the inverse transformation.
* *Matlab:* Use this flag if you need MATLAB compatibility. Equals to setting the _AsymmetricScaling_ flag. This matches the definition used in the [url:wikipedia article|https://en.wikipedia.org/wiki/Discrete_Fourier_transform].
* *NumericalRecipes:* Use this flag if you need Numerical Recipes compatibility. Equal to setting both the _InverseExponent_ and the _NoScaling_ flags.

Useful symmetries of the Fourier transform:

* h(t) is real valued <=> real part of H(f) is even, imaginary part of H(f) is odd
* h(t) is imaginary valued <=> real part of H(f) is odd, imaginary part of H(f) is even
* h(t) is even <=> H(f) is even
* h(t) is odd <=> H(f) is odd
* h(t) is real-valued even <=> H(f) is real-valued even
* h(t) is real-valued odd <=> H(f) is imaginary-valued odd
* h(t) is imaginary-valued even <=> H(f) is imaginary-valued even
* h(t) is imaginary-valued odd <=> H(f) is real-valued odd


Hartley Space: Discrete Hartley Transform
-----------------------------------------

...
