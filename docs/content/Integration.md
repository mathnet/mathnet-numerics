    [hide]
    #I "../../out/lib/net40"
    #r "MathNet.Numerics.dll"
    #r "MathNet.Numerics.FSharp.dll"
    open System.Numerics
    open MathNet.Numerics
    open MathNet.Numerics.Integration

Numerical Integration
=====================

The following double precision numerical integration or quadrature rules are supported in Math.NET Numerics under the `MathNet.Numerics.Integration` namespace. Unless stated otherwise, the examples below evaluate the integral $\int_0^{10} x^2 \, dx = \frac{1000}{3} \approx 333.\overline{3}$.

Simpson's Rule
--------------

    [lang=csharp]
    // Composite approximation with 4 partitions
    double composite = SimpsonRule.IntegrateComposite(x => x * x, 0.0, 10.0, 4);

    // Approximate value using IntegrateComposite with 4 partitions is: 333.33333333333337
    Console.WriteLine("Approximate value using IntegrateComposite with 4 partitions is: " + composite);

    // Three point approximation
    double threePoint = SimpsonRule.IntegrateThreePoint(x => x * x, 0.0, 10.0);

    // Approximate value using IntegrateThreePoint is: 333.333333333333
    Console.WriteLine("Approximate value using IntegrateThreePoint is: " + threePoint);

Newton Cotes Trapezium Rule
---------------------------

    [lang=csharp]
    // Adaptive approximation with a relative error of 1e-5
    double adaptive = NewtonCotesTrapeziumRule.IntegrateAdaptive(x => x * x, 0.0, 10.0, 1e-5);

    // Approximate value of the integral using IntegrateAdaptive with a relative error of 1e-5 is: 333.333969116211
    Console.WriteLine("Approximate value using IntegrateAdaptive with a relative error of 1e-5: " + adaptive);

    // Composite approximation with 15 partitions
    double composite = NewtonCotesTrapeziumRule.IntegrateComposite(x => x * x, 0.0, 10.0, 15);

    //Approximate value of the integral using IntegrateComposite with 15 partitions is: 334.074074074074
    Console.WriteLine("Approximate value using IntegrateComposite with 15 partitions is: " + composite);

    // Two point approximation
    double twoPoint = NewtonCotesTrapeziumRule.IntegrateTwoPoint(x => x * x, 0.0, 10.0);

    //Approximate value using IntegrateTwoPoint is: 500
    Console.WriteLine("Approximate value using IntegrateTwoPoint is: " + twoPoint);

Double-Exponential Transformation
---------------------------------
The Double-Exponential Transformation is suited for integration of smooth functions with no discontinuities, derivative discontinuities, and poles inside the interval.

    [lang=csharp]
    // Approximate using a relative error of 1e-5.
    double integrate = DoubleExponentialTransformation.Integrate(x => x * x, 0.0, 10.0, 1e-5);

    // Approximate value using a relative error of 1e-5 is: 333.333333333332
    Console.WriteLine("Approximate value using a relative error of 1e-5 is: " + integrate);

Gauss-Legendre Rule
-------------------
A fixed-order Gauss-Legendre integration routine is provided for fast integration of smooth functions with known polynomial order. The N-point Gauss-Legendre rule is exact for polynomials of order $2N-1$ or less. For example, these rules are useful when integrating basis functions to form mass matrices for the Galerkin method [[GSL]](https://www.gnu.org/software/gsl/).

The basic idea of Gauss-Legendre integration is to approximate the integral of a function $f(x)$ using $N$ Weights $w_i$ and abscissas (or nodes) $x_i$.

$$$
\int_a^b f(x) \, dx \approx \sum_{i = 0}^{N - 1} w_i f(x_i)

This algorithm calculates the abscissas and weights for a given order and integration interval. For efficiency, pre-computed abscissas and weights for the orders $ N = 2 - 20, \, 32, \, 64, \, 96, 100, \, 128, \, 256, \, 512, \, 1024$ are used. Otherwise, they are calculated on the fly using Newton's method. For more information on the algorithm see [[Holoborodko, Pavel] ](http://www.holoborodko.com/pavel/numerical-methods/numerical-integration/).

### Abscissas and Weights

We'll first use the abscissas and weights to approximate an integral using a 5-point Gauss-Legendre rule

    [lang=csharp]
    // Create a 5-point Gauss-Legendre rule over the integration interval [0, 10]
    GaussLegendreRule rule = new GaussLegendreRule(0.0, 10.0, 5);

    double sum = 0; // Will hold the approximate value of the integral
    for (int i = 0; i < rule.Order; i++) // rule.Order = 5
    {
        // Access the ith abscissa and weight
        sum += rule.GetWeight(i) * rule.GetAbscissa(i) * rule.GetAbscissa(i);
    }

    // Approximate value is: 333.333333333333
    Console.WriteLine("Approximate value is: " + sum);

If you prefer direct access to the abscissas and weights, as opposed to using the methods

- ```double GetAbscissa(int i)```
- ```double GetWeight(int i)```

then use the properties `Abscissas` and `Weights`

    [lang=csharp]
    // Create a 5-point Gauss-Legendre rule over the integration interval [0, 10]
    GaussLegendreRule rule = new GaussLegendreRule(0.0, 10.0, 5);

    double[] x = rule.Abscissas; // Creates a clone and returns array of abscissas
    double[] w = rule.Weights; // Creates a clone and returns array of weights

    double sum = 0; // Will hold the approximate value of the integral
    for (int i = 0; i < rule.Order; i++) // rule.Order = 5
    {
        // Access the ith abscissa and weight
        sum += w[i] * x[i] * x[i];
    }

    // Approximate value is: 333.333333333333
    Console.WriteLine("Approximate value is: " + sum);;

In addition to obtaining the abscissas and weights, the order and integration interval can be obtained

    [lang=csharp]
    // Create a 5-point Gauss-Legendre rule over the integration interval [0, 10]
    GaussLegendreRule rule = new GaussLegendreRule(0.0, 10.0, 5);

    // The order of the rule is: 5
    Console.WriteLine("The order of the rule is: " + rule.Order);

    // The lower integral bound is 0
    Console.WriteLine("The lower integral bound is: " + rule.IntervalBegin);

    // The upper integral bound is 10
    Console.WriteLine("The upper integral bound is: " + rule.IntervalEnd);

### Integrate Method

For convenience, we provide an overloaded static method `double Integrate(...)` which preforms 1D and 2D integration of a function. The first parameter to the method is a delegate of type `Func<double, double>` or `Func<double, double, double>` for 1D and 2D integration respectively. So for example

    [lang=csharp]
    // 1D integration using a 5-point Gauss-Legendre rule over the integration interval [0, 10]
    double integrate1D = GaussLegendreRule.Integrate(x => x * x, 0.0, 10.0, 5);

    // Approximate value of the 1D integral is: 333.333333333333
    Console.WriteLine("Approximate value of the 1D integral is: " + integrate1D);

    // 2D integration using a 5-point Gauss-Legendre rule over the integration interval [0, 10] X [1, 2]
    double integrate2D = GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 5);

    // Approximate value of the 2D integral is: 777.777777777778
    Console.WriteLine("Approximate value of the 2D integral is: " + integrate2D);

where we used $\int_0^{10}\int_1^2 x^2 y^2 \,dydx = \frac{7000}{9} \approx 777.\overline{7}$ for the 2D integral example.
