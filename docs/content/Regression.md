    [hide]
    #I "../../out/lib/net40"
    #r "MathNet.Numerics.dll"
    #r "MathNet.Numerics.FSharp.dll"
    open System.Numerics
    open MathNet.Numerics
    open MathNet.Numerics.LinearRegression
    open MathNet.Numerics.LinearAlgebra

Curve Fitting: Linear Regression
================================

Regression is all about fitting a low order parametric model or curve to data, so we can
reason about it or make predictions on points not covered by the data. Both data and
model are known, but we'd like to find the model parameters that make the model fit best
or good enough to the data according to some metric.

We may also be interested in how well the model supports the data or whether we better
look for another more appropriate model.

In a regression, a lot of data is reduced and generalized into a few parameters.
The resulting model can obviously no longer reproduce all the original data exactly -
if you need the data to be reproduced exactly, have a look at interpolation instead.


Simple Regression: Fit to a Line
--------------------------------

In the simplest yet still common form of regression we would like to fit a line
$y : x \mapsto a + b x$ to a set of points $(x_j,y_j)$, where $x_j$ and $y_j$ are scalars.
Assuming we have two double arrays for x and y, we can use `Fit.Line` to evaluate the $a$ and $b$
parameters of the least squares fit:

    [lang=csharp]
    double[] xdata = new double[] { 10, 20, 30 };
    double[] ydata = new double[] { 15, 20, 25 };

    Tuple<double, double> p = Fit.Line(xdata, ydata);
    double a = p.Item1; // == 10; intercept
    double b = p.Item2; // == 0.5; slope

Or in F#:

    [lang=fsharp]
    let a, b = Fit.Line ([|10.0;20.0;30.0|], [|15.0;20.0;25.0|])

How well do these parameters fit the data? The data points happen to be positioned
exactly on a line. Indeed, the [coefficient of determination](https://en.wikipedia.org/wiki/Coefficient_of_determination)
confirms the perfect fit:

    [lang=csharp]
    GoodnessOfFit.RSquared(xdata.Select(x => a+b*x), ydata); // == 1.0


Linear Model
------------

In practice, a line is often not an adequate model. But if we can choose a model that is linear,
we can leverage the power of linear algebra; otherwise we have to resort to iterative methods
(see Nonlinear Optimization).

A linear model can be described as linear combination of $N$ arbitrary but known
functions $f_i(x)$, scaled by the model parameters $p_i$. Note that none of the functions
$f_i$ depends on any of the $p_i$ parameters.

$$$
y : x \mapsto p_1 f_1(x) + p_2 f_2(x) + \cdots + p_N f_N(x)

If we have $M$ data points $(x_j,y_j)$, then we can write the regression problem as an
overdefined system of $M$ equations:

$$$
\begin{eqnarray}
  y_1 &=& p_1 f_1(x_1) + p_2 f_2(x_1) + \cdots + p_N f_N(x_1)  \\
  y_2 &=& p_1 f_1(x_2) + p_2 f_2(x_2) + \cdots + p_N f_N(x_2)  \\
  &\vdots&  \\
  y_M &=& p_1 f_1(x_M) + p_2 f_2(x_M) + \cdots + p_N f_N(x_M)
\end{eqnarray}

Or in matrix notation with the predictor matrix $X$ and the response $y$:

$$$
\begin{eqnarray}
  \mathbf y &=& \mathbf X \mathbf p  \\
  \begin{bmatrix}y_1\\y_2\\ \vdots \\y_M\end{bmatrix} &=&
  \begin{bmatrix}f_1(x_1) & f_2(x_1) & \cdots & f_N(x_1)\\f_1(x_2) & f_2(x_2) & \cdots & f_N(x_2)\\ \vdots & \vdots & \ddots & \vdots\\f_1(x_M) & f_2(x_M) & \cdots & f_N(x_M)\end{bmatrix}
  \begin{bmatrix}p_1\\p_2\\ \vdots \\p_N\end{bmatrix}
\end{eqnarray}

Provided the dataset is small enough, if transformed to the normal equation
$\mathbf{X}^T\mathbf y = \mathbf{X}^T\mathbf X \mathbf p$ this can be solved efficiently by the
Cholesky decomposition (do not use matrix inversion!).

    [lang=csharp]
    Vector<double> p = MultipleRegression.NormalEquations(X, y);

Using normal equations is comparably fast as it can dramatically reduce the linear algebra problem
to be solved, but that comes at the cost of less precision. If you need more precision, try using
`MultipleRegression.QR` or `MultipleRegression.Svd` instead, with the same arguments.


Polynomial Regression
---------------------

To fit to a polynomial we can choose the following linear model with $f_i(x) := x^i$:

$$$
y : x \mapsto p_0 + p_1 x + p_2 x^2 + \cdots + p_N x^N

The predictor matrix of this model is the [Vandermonde matrix](https://en.wikipedia.org/wiki/Vandermonde_matrix).
There is a special function in the `Fit` class for regressions to a polynomial,
but note that regression to high order polynomials is numerically problematic.

    [lang=csharp]
    double[] p = Fit.Polynomial(xdata, ydata, 3); // polynomial of order 3


Multiple Regression
-------------------

The $x$ in the linear model can also be a vector $\mathbf x = [x^{(1)}\; x^{(2)} \cdots x^{(k)}]$
and the arbitrary functions $f_i(\mathbf x)$ can accept vectors instead of scalars.

If we use $f_i(\mathbf x) := x^{(i)}$ and add an intercept term $f_0(\mathbf x) := 1$
we end up at the simplest form of ordinary multiple regression:

$$$
y : x \mapsto p_0 + p_1 x^{(1)} + p_2 x^{(2)} + \cdots + p_N x^{(N)}

For example, for the data points $(\mathbf{x}_j = [x^{(1)}_j\; x^{(2)}_j], y_j)$ with values
`([1,4],15)`, `([2,5],20)` and `([3,2],10)` we can evaluate the best fitting parameters with:

    [lang=csharp]
    double[] p = Fit.MultiDim(
        new[] {new[] { 1.0, 4.0 }, new[] { 2.0, 5.0 }, new[] { 3.0, 2.0 }},
        new[] { 15.0, 20, 10 },
        intercept: true);

The `Fit.MultiDim` routine uses normal equations, but you can always choose to explicitly use e.g.
the QR decomposition for more precision by using the `MultipleRegression` class directly:

    [lang=csharp]
    double[] p = MultipleRegression.QR(
        new[] {new[] { 1.0, 4.0 }, new[] { 2.0, 5.0 }, new[] { 3.0, 2.0 }},
        new[] { 15.0, 20, 10 },
        intercept: true);


Arbitrary Linear Combination
----------------------------

In multiple regression, the functions $f_i(\mathbf x)$ can also operate on the whole
vector or mix its components arbitrarily and apply any functions on them, provided they are
defined at all the data points. For example, let's have a look at the following complicated but still linear
model in two dimensions:

$$$
z : (x, y) \mapsto p_0 + p_1 \mathrm{tanh}(x) + p_2 \psi(x y) + p_3 x^y

Since we map (x,y) to (z) we need to organize the tuples in two arrays:

    [lang=csharp]
    double[][] xy = new[] { new[]{x1,y1}, new[]{x2,y2}, new[]{x3,y3}, ...  };
    double[] z = new[] { z1, z2, z3, ... };

Then we can call Fit.LinearMultiDim with our model, which will return an array with the best fitting 4 parameters $p_0, p_1, p_2, p_3$:

    [lang=csharp]
    double[] p = Fit.LinearMultiDim(xy, z,
        d => 1.0,                                  // p0*1.0
        d => Math.Tanh(d[0]),                      // p1*tanh(x)
        d => SpecialFunctions.DiGamma(d[0]*d[1]),  // p2*psi(x*y)
        d => Math.Pow(d[0], d[1]));                // p3*x^y


Evaluating the model at specific data points
--------------------------------------------

Let's say we have the following model:

$$$
y : x \mapsto a + b \ln x

For this case we can use the `Fit.LinearCombination` function:

    [lang=csharp]
    double[] p = Fit.LinearCombination(
        new[] {61.0, 62.0, 63.0, 65.0},
        new[] {3.6,3.8, 4.8, 4.1},
        x => 1.0,
        x => Math.Log(x)); // -34.481, 9.316

In order to evaluate the resulting model at specific data points we can manually apply
the values of p to the model function, or we can use an alternative function with the `Func`
suffix that returns a function instead of the model parameters. The returned function
can then be used to evaluate the parametrized model:

    [lang=csharp]
    Func<double,double> f = Fit.LinearCombinationFunc(
        new[] {61.0, 62.0, 63.0, 65.0},
        new[] {3.6, 3.8, 4.8, 4.1},
        x => 1.0,
        x => Math.Log(x));
    f(66.0); // 4.548


Linearizing non-linear models by transformation
-----------------------------------------------

Sometimes it is possible to transform a non-linear model into a linear one.
For example, the following power function

$$$
z : (x, y) \mapsto u x^v y^w

can be transformed into the following linear model with $\hat{z} = \ln z$ and $t = \ln u$

$$$
\hat{z} : (x, y) \mapsto t + v \ln x + w \ln y

    [lang=csharp]
    var xy = new[] {new[] { 1.0, 4.0 }, new[] { 2.0, 5.0 }, new[] { 3.0, 2.0 }};
    var z = new[] { 15.0, 20, 10 };

    var z_hat = z.Select(r => Math.Log(r)).ToArray(); // transform z_hat = ln(z)
    double[] p_hat = Fit.LinearMultiDim(xy, z_hat,
        d => 1.0,
        d => Math.Log(d[0]),
        d => Math.Log(d[1]));
    double u = Math.Exp(p_hat[0]); // transform t = ln(u)
    double v = p_hat[1];
    double w = p_hat[2];


Weighted Regression
-------------------

Sometimes the regression error can be reduced by dampening specific data points.
We can achieve this by introducing a weight matrix $W$ into the normal equations
$\mathbf{X}^T\mathbf{y} = \mathbf{X}^T\mathbf{X}\mathbf{p}$. Such weight matrices
are often diagonal, with a separate weight for each data point on the diagonal.

$$$
\mathbf{X}^T\mathbf{W}\mathbf{y} = \mathbf{X}^T\mathbf{W}\mathbf{X}\mathbf{p}

    [lang=csharp]
    var p = WeightedRegression.Weighted(X,y,W);

Weighter regression becomes interesting if we can adapt them to the point of interest
and e.g. dampen all data points far away. Unfortunately this way the model parameters
are dependent on the point of interest $t$.

    [lang=csharp]
    // warning: preliminary api
    var p = WeightedRegression.Local(X,y,t,radius,kernel);


Regularization
--------------


Iterative Methods
-----------------
