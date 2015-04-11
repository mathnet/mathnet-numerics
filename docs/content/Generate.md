    [hide]
    #I "../../out/lib/net40"
    #r "MathNet.Numerics.dll"
    #r "MathNet.Numerics.FSharp.dll"
    open System.Numerics
    open MathNet.Numerics
    open MathNet.Numerics.Statistics
    let a = [| 2.0; 4.0; 3.0; 6.0 |]

Generating Data
===============

Numerics is all about analyzing and manipulating numeric data. But unless you can read in data from an external
file, source or e.g. with the excellent [F# Type Providers](http://fsharp.github.io/FSharp.Data/),
you may need to generate synthetic or random data locally, or transform existing data into a new form.
The `Generate` class can help you in all these scenarios with a set of static functions generating either
an array or an IEnumerable sequence.

There is some overlap with LINQ, in case of F# also with some integrated language features and its fundamental types.
This is intended for simplicity and consistency between array and sequence operations, as LINQ only supports sequences.


Linear Range
------------

Generates a linearly spaced array within the inclusive interval between start and stop,
and either a provided step or a step of 1.0. Linear range is equivalent to the
single colon `:` and double colon `::` operators in MATLAB.

F# has built in linear range support in array comprehensions with the colon operator:

    [lang=fsharp]
    [ 10.0 .. 2.0 .. 15.0 ]
    // [fsi:val it : float list = [10.0; 12.0; 14.0] ]
    [ for x in 10.0 .. 2.0 .. 15.0 -> sin x ]
    // [fsi:val it : float list = [-0.5440211109; -0.536572918; 0.9906073557] ]

In C# you can get the same result with `LinearRange`:

    [lang=csharp]
    Generate.LinearRange(10, 2, 15); // returns array { 10.0, 12.0, 14.0 }
    Generate.LinearRangeMap(10, 2, 15, Math.Sin); // applies sin(x) to each value

Most of the routines in the `Generate` class have variants with a `Map` suffix.
Instead of returning an array with the generated numbers, these routines instead
apply the generated numbers to a custom function and return an array with the results.
Similarly, some routines have variants with a `Sequence` suffix that return
lazy enumerable sequences instead of arrays.


Linear-Spaced and Log-Spaced
----------------------------

Generates a linearly or log-spaced array within an interval, but other than linear range
where the step is provided, here we instead provide the number of values we want.
This is equivalent to the linspace and logspace operators in MATLAB.

    [lang=csharp]
    Generate.LinearSpaced(11, 0.0, 1.0); // returns array { 0.0, 0.1, 0.2, .., 1.0 }
    Generate.LinearSpacedMap(15, 0.0, Math.Pi, Math.Sin); // applies sin(x) to each value

In F# you can also use:

    [lang=fsharp]
    Generate.linearSpacedMap 15 0.0 Math.PI sin
    // [fsi:val it : float [] = ]
    // [fsi:  [|0.0; 0.222520934; 0.4338837391; 0.6234898019; 0.7818314825; 0.9009688679; ]
    // [fsi:    0.9749279122; 1.0; 0.9749279122; 0.9009688679; 0.7818314825; 0.6234898019; ]
    // [fsi:    0.4338837391; 0.222520934; 1.224606354e-16|] ]

`LogSpaced` works the same way but instead of the values $10^x$ it spaces the decade exponents $x$ linearly
between the provided two exponents.

    [lang=csharp]
    Generate.LogSpaced(4,0,3); // returns array { 1, 10, 100, 1000 }


Kronecker Delta Impulse
-----------------------

The Kronecker delta $\delta[n]$ is a fundamental signal in time-discrete signal processing,
often referred to as *unit impulse*. When applied to a system, the resulting output is the system's *impulse response*.
It is closely related to the Dirac delta impulse function $\delta(x)$ in continuous signal processing.

$$$
\delta[n] = \begin{cases} 0 &\mbox{if } n \ne 0 \\ 1 & \mbox{if } n = 0\end{cases}

The `Impulse` routine generates a Kronecker delta impulse, but also accepts a sample delay
parameter $d$ and amplitude $A$ such that the resulting generated signal is

$$$
s[n] = A\cdot\delta[n-d] = \begin{cases} 0 &\mbox{if } n \ne d \\ A & \mbox{if } n = d\end{cases}

There is also a periodic version in `PeriodicImpulse` which accepts an additional `period` parameter.

    [lang=fsharp]
    Generate.Impulse(8, 2.0, 3)
    // [fsi:val it : float [] = [|0.0; 0.0; 0.0; 2.0; 0.0; 0.0; 0.0; 0.0|] ]

    Generate.PeriodicImpulse(8, 3, 10.0, 1)
    // [fsi:val it : float [] = [|0.0; 10.0; 0.0; 0.0; 10.0; 0.0; 0.0; 10.0|] ]


Heaviside Step
--------------

Another fundamental signal in signal processing, the Heaviside step function $H[n]$
is the integral of the Dirac delta impulse and represents a signal that switches on
at a specified time and then stays on indefinitely. In discrete time:

$$$
H[n] = \begin{cases} 0 &\mbox{if } n < 0 \\ 1 & \mbox{if } n \ge 0\end{cases}

The `Step` routines generates a Heaviside step, but just like the Kronecker Delta impulse
 also accepts a sample delay parameter $d$ and amplitude $A$ such that the resulting generated signal is

$$$
s[n] = A\cdot H[n-d] = \begin{cases} 0 &\mbox{if } n < d \\ A & \mbox{if } n \ge d\end{cases}

    [lang=fsharp]
    Generate.Step(8, 2.0, 3)
    // [fsi:val it : float [] = [|0.0; 0.0; 0.0; 2.0; 2.0; 2.0; 2.0; 2.0|] ]


Periodic Sawtooth
-----------------

Generates an array of the given length with a periodic upper forward sawtooth signal,
i.e. a line starting at zero up to some amplitude $A$, then drop back to zero instantly and start afresh.
The sawtooth can be used to turn any arbitrary function defined over the interval $[0,A)$ into a
periodic function by repeating it continuously.

Mathematically, the sawtooth can be described using the fractional part function
$\mathrm{frac}(x) \equiv x - \lfloor x \rfloor$, frequency $\nu$ and phase $\theta$ as

$$$
s(x) = A\cdot\mathrm{frac}\left(x\nu+\frac{\theta}{A}\right)

In a trigonometric interpretation the signal represents the angular position $\alpha$ of a point moving endlessly
around a circle with radius $\frac{A}{2\pi}$ (and thus circumference $A$) in constant speed,
normalized to strictly $0\le\alpha < A$.

`Generate.Periodic(length,samplingRate,frequency,amplitude,phase,delay)`

* **Sampling Rate**: Number of samples $r$ per time unit. If the time unit is 1s, the sampling rate has unit Hz.
* **Frequency**: Frequency $\nu$ of the signal, in sawtooth-periods per time unit. If the time unit is 1s, the frequency has unit Hz.
  For a desired number of samples $n$ per sawtooth-period and sampling rate $r$ choose $\nu=\frac{r}{n}$.
* **Amplitude**: The theoretical maximum value $A$, which is never reached and logically equivalent to zero.
  The circumference of the circle. Typically $1$ or $2\pi$.
* **Phase**: Optional initial value or phase offset. Contributes to $\theta$.
* **Delay**: Optional initial delay, in samples. Contributes to $\theta$.

The equivalent map function accepts a custom map lambda as second argument after the length:

    [lang=fsharp]
    Generate.periodicMap 15 ((+) 100.0) 1000.0 100.0 10.0 0.0 0
    // [fsi:val it : float [] = ]
    // [fsi:  [|100.0; 101.0; 102.0; 103.0; 104.0; 105.0; 106.0; 107.0; 108.0; 109.0; ]
    // [fsi:    100.0; 101.0; 102.0; 103.0; 104.0|] ]


Sinusoidal
----------

Generates a Sine wave array of the given length. This is equivalent to applying a scaled
trigonometric Sine function to a periodic sawtooth of amplitude $2\pi$.

$$$
s(x) = A\cdot\sin(2\pi\nu x + \theta)

`Generate.Sinusoidal(length,samplingRate,frequency,amplitude,mean,phase,delay)`

    [lang=csharp]
    Generate.Sinusoidal(15, 1000.0, 100.0, 10.0);
    // returns array { 0, 5.9, 9.5, 9.5, 5.9, 0, -5.9, ... }


Random
------

Generate random sequences by sampling from a random distribution.


#### Uniform Distribution

Generate sample sequences distributed uniformly between 0 and 1.

    [lang=csharp]
    Generate.Uniform(100); // e.g. 0.867421787170424, 0.236744313145403, ... 

Uniform supports mapping to functions with not only one but also two uniform variables
as arguments, with `UniformMap` and `UniformMap2`. As usual, lazy sequences can be
generated using the variants with the `Sequence` suffix, e.g. `UniformMap2Sequence`.


#### Non-Uniform Distributions

Instead of uniform we can also sample from other distributions.

* `Normal` - sample an array or sequence form a normal distribution
* `Standard` - sample an array or sequence form a standard distribution

In addition, the `Random` functions accept a custom distribution instance to sample
from. See the section about random numbers and probability distributions for details.


Map
---

Generates a new array or sequence where each new values is the result of applying the provided function
the the corresponding value in the input data.

    [lang=csharp]
    var a = new double[] { 2.0, 4.0, 3.0, 6.0 };
    Generate.Map(a, x => x + 1.0); // returns array { 3.0, 5.0, 4.0, 7.0 }

In F# you'd typically use the Array module to the same effect (and should continue to do so):

    [lang=fsharp]
    Array.map ((+) 1.0) a
    // [fsi:val it : float [] = [|3.0; 5.0; 4.0; 7.0|] ]

...but no equivalent operation is available in the .NET base class libraries (BCL) for C#.
You can use LINQ, but that operates on sequences instead of arrays:

    [lang=csharp]
    a.Select(x => x + 1.0).ToArray();

Similarly, with `Map2` you can also map a function accepting two inputs to two input arrays:

    [lang=fsharp]
    let b = [| 1.0; -1.0; 2.0; -2.0 |]
    Generate.Map2(a, b, fun x y -> x + y)
    // [fsi:val it : float [] = [|3.0; 3.0; 5.0; 4.0|] ]

Typical F# equivalent:

    [lang=fsharp]
    Array.map2 (+) a b
    // [fsi:val it : float [] = [|3.0; 3.0; 5.0; 4.0|] ]

And in C# with LINQ:

    [lang=csharp]
    a.Zip(b, (x, y) => x + y).ToArray();
