Special Functions
=================

All the following special functions are available in the static `SpecialFunctions` class:


Factorial
---------

* `Factorial(x)`

$$$
x \mapsto x! = \prod_{k=1}^{x} k = \Gamma(x+1)

Code Sample:

    [lang=csharp]
    double x = SpecialFunctions.Factorial(14); // 87178291200.0
    double y = SpecialFunctions.Factorial(31); // 8.2228386541779224E+33

* `FactorialLn(x)`

$$$
x \mapsto \ln x! = \ln\Gamma(x+1)

* `Binomial(n,k)`

Binomial Coefficient

$$$
\binom{n}{k} = \mathrm{C}_n^k = \frac{n!}{k! (n-k)!}

* `BinomialLn(n,k)`

$$$
\ln \binom{n}{k} = \ln n! - \ln k! - \ln(n-k)!

* `Multinomial(n,k[])`

Multinomial Coefficient

$$$
\binom{n}{k_1,k_2,\dots,k_r} = \frac{n!}{k_1! k_2! \cdots k_r!} = \frac{n!}{\prod_{i=1}^{r}k_i!}


Exponential Integral
--------------------

* `ExponentialIntegral(x,n)`

Generalized Exponential Integral

$$$
E_n(x) = \int_1^\infty t^{-n} e^{-xt}\,\mathrm{d}t


Gamma functions
---------------

#### Gamma
* `Gamma(a)`

$$$
\Gamma(a) = \int_0^\infty t^{a-1} e^{-t}\,\mathrm{d}t

* `GammaLn(a)`

$$$
\ln\Gamma(a)


#### Incomplete Gamma
* `GammaLowerIncomplete(a,x)`

Lower incomplete Gamma function, unregularized.

$$$
\gamma(a,x) = \int_0^x t^{a-1} e^{-t}\,\mathrm{d}t

* `GammaUpperIncomplete(a,x)`

Upper incomplete Gamma function, unregularized.

$$$
\Gamma(a,x) = \int_x^\infty t^{a-1} e^{-t}\,\mathrm{d}t


#### Regularized Gamma
* `GammaLowerRegularized(a,x)`

Lower regularized incomplete Gamma function.

$$$
\mathrm{P}(a,x) = \frac{\gamma(a,x)}{\Gamma(a)}

* `GammaUpperRegularized(a,x)`

Upper regularized incomplete Gamma function.

$$$
\mathrm{Q}(a,x) = \frac{\Gamma(a,x)}{\Gamma(a)}

* `GammaLowerRegularizedInv(a, y)`

Inverse $x$ of the lower regularized Gamma function, such that $\mathrm{P}(a,x) = y$.

$$$
\mathrm{P}^{-1}(a,y)


#### Psi: Derivative of Logarithmic Gamma
* `DiGamma(x)`

$$$
\psi(x) = \frac{\mathrm{d}}{\mathrm{d}x}\ln\Gamma(x)

* `DiGammaInv(p)`

Inverse $x$ of the DiGamma function, such that $\psi(x) = p$.

$$$
\psi^{-1}(p)


Euler Beta functions
--------------------

#### Euler Beta
* `Beta(a,b)`

$$$
\mathrm{B}(a,b) = \int_0^1 t^{a-1} (1-t)^{b-1}\,\mathrm{d}t = \frac{\Gamma(a)\Gamma(b)}{\Gamma(a+b)}

* `BetaLn(a,b)`

$$$
\ln\mathrm{B}(a,b) = \Gamma(a) + \Gamma(b) - \Gamma(a+b)


#### Incomplete Beta
* `BetaIncomplete(a,b,x)`

Lower incomplete Beta function (unregularized).

$$$
\mathrm{B}_x(a,b) = \int_0^x t^{a-1} (1-t)^{b-1}\,\mathrm{d}t


#### Regularized Beta
* `BetaRegularized(a,b,x)`

Lower incomplete regularized Beta function.

$$$
\mathrm{I}_x(a,b) = \frac{\mathrm{B}(a,b,x)}{\mathrm{B}(a,b)}


Error functions
---------------

#### Error Function
* `Erf(x)`

$$$
\mathrm{erf}(x) = \frac{2}{\sqrt{\pi}}\int_0^x e^{-t^2}\,\mathrm{d}t

* `ErfInv(z)`

Inverse $x$ of the Error function, such that $\mathrm{erf}(x) = z$.

$$$
z \mapsto \mathrm{erf}^{-1}(z)


#### Complementary Error function.
* `Erfc(x)`

$$$
\mathrm{erfc}(x) = 1-\mathrm{erf}(x) = \frac{2}{\sqrt{\pi}}\int_x^\infty e^{-t^2}\,\mathrm{d}t

* `ErfcInv(z)`

Inverse $x$ of the complementary Error function, such that $\mathrm{erfc}(x) = z$.

$$$
z \mapsto \mathrm{erfc}^{-1}(z)

Code Sample:

    [lang=csharp]
    double erf = SpecialFunctions.Erf(0.9); // 0.7969082124


Sigmoid: Logistic function
--------------------------

* `Logistic(x)`

$$$
x \mapsto \frac{1}{1+e^{-x}}

* `Logit(y)`

Inverse of the Logistic function, for $y$ between 0 and 1 (where the function is real-valued).

$$$
y \mapsto \ln \frac{y}{1-y}


Harmonic Numbers
----------------

* `Harmonic(t)`

The n-th Harmonic number is the sum of the reciprocals of the first n natural numbers.
With $\gamma$ as the Euler-Mascheroni constant and the DiGamma function:

$$$
\mathrm{H}_n = \sum_{k=1}^{n}\frac{1}{k} = \gamma - \psi(n+1)

* `GeneralHarmonic(n, m)`

Generalized harmonic number of order n of m.

$$$
\mathrm{H}_{n,m} = \sum_{k=1}^{n}\frac{1}{k^m}


Bessel and Struve Functions
---------------------------

#### Bessel functions

Bessel functions are canonical solutions $y(x)$ of Bessel's differential equation

$$$
x^2\frac{\mathrm{d}^2y}{\mathrm{d}x^2}+x\frac{\mathrm{d}y}{\mathrm{d}x}+(x^2-\alpha^2)y = 0

#### Modified Bessel functions

Modified Bessel's equation:

$$$
x^2\frac{\mathrm{d}^2y}{\mathrm{d}x^2}+x\frac{\mathrm{d}y}{\mathrm{d}x}-(x^2+\alpha^2)y = 0

Modified Bessel functions:

$$$
\begin{align}
\mathrm{I}_\alpha(x) &= \imath^{-\alpha}\mathrm{J}_\alpha(\imath x) = \sum_{m=0}^\infty \frac{1}{m!\Gamma(m+\alpha+1)}\left(\frac{x}{2}\right)^{2m+\alpha} \\
\mathrm{K}_\alpha(x) &= \frac{\pi}{2} \frac{\mathrm{I}_{-\alpha}(x)-\mathrm{I}_\alpha(x)}{\sin(\alpha\pi)}
\end{align}

* `BesselI0(x)`

Modified or hyperbolic Bessel function of the first kind, order 0.

$$$
x \mapsto \mathrm{I}_0(x)

* `BesselI1(x)`

Modified or hyperbolic Bessel function of the first kind, order 1.

$$$
x \mapsto \mathrm{I}_1(x)

* `BesselK0(x)`

Modified or hyperbolic Bessel function of the second kind, order 0.

$$$
x \mapsto \mathrm{K}_0(x)

* `BesselK0e(x)`

Exponentionally scaled modified Bessel function of the second kind, order 0.

$$$
x \mapsto e^x\mathrm{K}_0(x)

* `BesselK1(x)`

Modified or hyperbolic Bessel function of the second kind, order 1.

$$$
x \mapsto \mathrm{K}_1(x)

* `BesselK1e(x)`

Exponentially scaled modified Bessel function of the second kind, order 1.

$$$
x \mapsto e^x\mathrm{K}_1(x)

#### Struve functions

Struve functions are solutions $y(x)$ of the non-homogeneous Bessel's differential equation

$$$
x^2\frac{\mathrm{d}^2y}{\mathrm{d}x^2}+x\frac{\mathrm{d}y}{\mathrm{d}x}+(x^2-\alpha^2)y = \frac{4(\frac{x}{2})^{\alpha+1}}{\sqrt{\pi}\Gamma(\alpha+\frac{1}{2})}


#### Modified Struve functions

Modified equation:

$$$
x^2\frac{\mathrm{d}^2y}{\mathrm{d}x^2}+x\frac{\mathrm{d}y}{\mathrm{d}x}-(x^2+\alpha^2)y = \frac{4(\frac{x}{2})^{\alpha+1}}{\sqrt{\pi}\Gamma(\alpha+\frac{1}{2})}

Modified Struve functions:

$$$
\mathrm{L}_\alpha(x) = \left(\frac{x}{2}\right)^{\alpha+1}\sum_{k=0}^\infty \frac{1}{\Gamma(\frac{3}{2}+k)\Gamma(\frac{3}{2}+k+\alpha)}\left(\frac{x}{2}\right)^{2k}

* `StruveL0(x)`

Modified Struve function of order 0.

$$$
x \mapsto \mathrm{L}_0(x)

* `StruveL1(x)`

Modified Struve function of order 1.

$$$
x \mapsto \mathrm{L}_1(x)


#### Misc

* `BesselI0MStruveL0(x)`

Difference between the Bessel $I_0$ and the Struve $L_0$ functions.

$$$
x \mapsto I_0(x) - L_0(x)

* `BesselI1MStruveL1(x)`

Difference between the Bessel $I_1$ and the Struve $L_1$ functions.

$$$
x \mapsto I_1(x) - L_1(x)


Numeric Stability
-----------------

* `ExponentialMinusOne(power)`

$\exp x-1$ is a typical case where a subtraction can be fatal for accuracy.
For example, at $10^{-13}$ the naive expression is 0.08% off, at $10^{-15}$
roughly 11% and at $10^{-18}$ it just returns 0.

$$$
x \mapsto e^x - 1

* `Hypotenuse(a, b)`

$$$
(a,b) \mapsto \sqrt{a^2 + b^2}


Trigonometry
------------

The `Trig` class provides the complete set of fundamental trigonometric functions
for both real and complex arguments.

* **Trigonometric**: Sin, Cos, Tan, Cot, Sec, Csc
* **Trigonometric Inverse**: Asin, Acos, Atan, Acot, Asec, Acsc
* **Hyperbolic**: Sinh, Cosh, Tanh, Coth, Sech, Csch
* **Hyperbolic Area**: Asinh, Acosh, Atanh, Acoth, Asech, Acsch
* **Sinc**: Normalized sinc function $x \mapsto \frac{\sin\pi x}{\pi x}$
* Conversion routines between radian, degree and grad.
