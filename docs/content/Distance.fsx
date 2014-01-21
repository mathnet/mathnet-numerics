(*** hide ***)
#I "../../out/lib/net40"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"

(**
Distance Metrics
================

A metric or distance function is a function $d(x,y)$ that defines the distance
between elements of a set as a non-negative real number. If the distance is zero, both elements are equivalent
under that specific metric. Distance functions thus provide a way to measure how close two elements are, where elements
do not have to be numbers but can also be vectors, matrices or arbitrary objects. Distance functions are often used
as error or cost functions to be minimized in an optimization problem.

There are multiple ways to define a metric on a set. A typical distance for real numbers is the absolute difference,
$ d : (x, y) \mapsto |x-y| $. But a scaled version of the absolute difference, or even $d(x, y) = \begin{cases} 0 &\mbox{if } x = y \\ 1 & \mbox{if } x \ne y. \end{cases}$
are valid metrics as well. Every normed vector space induces a distance given by $d(\vec x, \vec y) = \|\vec x - \vec y\|$.

Math.NET Numerics provides the following distance functions on vectors and arrays:


Sum of Absolute Difference (SAD)
--------------------------------

The sum of absolute difference is equivalent to the $L_1$-norm of the difference, also known as Manhattan- or Taxicab-norm.
The `abs` function makes this metric a bit complicated to deal with analytically, but it is more robust than SSD.

$$$
d_{\mathbf{SAD}} : (x, y) \mapsto \|x-y\|_1 = \sum_{i=1}^{n} |x_i-y_i|

    [lang=csharp]
    double d = Distance.SAD(a, b);


Sum of Squared Difference (SSD)
-------------------------------

The sum of squared difference is equivalent to the squared $L_2$-norm, also known as Euclidean norm.
This is the fundamental metric in least squares problems and linear algebra. The absence of the `abs`
function makes this metric convenient to deal with analytically, but the squares cause it to be very
sensitive to large outliers.

$$$
d_{\mathbf{SSD}} : (x, y) \mapsto \|x-y\|_2^2 = \langle x-y, x-y\rangle = \sum_{i=1}^{n} (x_i-y_i)^2

    [lang=csharp]
    double d = Distance.SSD(a, b);


Mean-Absolute Error (MAE)
-------------------------

The mean absolute error is a normalized version of the sum of absolute difference:

$$$
d_{\mathbf{MAD}} : (x, y) \mapsto \frac{d_{\mathbf{SAD}}}{n} = \frac{\|x-y\|_1}{n} = \frac{1}{n}\sum_{i=1}^{n} |x_i-y_i|

    [lang=csharp]
    double d = Distance.MAE(a, b);


Mean-Squared Error (MSE)
------------------------

The mean squared error is a normalized version of the sum of squared difference:

$$$
d_{\mathbf{MSE}} : (x, y) \mapsto \frac{d_{\mathbf{SSD}}}{n} = \frac{\|x-y\|_2^2}{n} = \frac{1}{n}\sum_{i=1}^{n} (x_i-y_i)^2

    [lang=csharp]
    double d = Distance.MSE(a, b);


Euclidean Distance
------------------

The euclidean distance is the $L_2$-norm of the difference:

$$$
d_{\mathbf{2}} : (x, y) \mapsto \|x-y\|_2 = \sqrt{d_{\mathbf{SSD}}} = \sqrt{\sum_{i=1}^{n} (x_i-y_i)^2}

    [lang=csharp]
    double d = Distance.Euclidean(a, b);


Manhattan Distance
------------------

The manhattan distance is the $L_1$-norm of the difference and equivalent to the sum of absolute difference:

$$$
d_{\mathbf{1}} \equiv d_{\mathbf{SAD}} : (x, y) \mapsto \|x-y\|_1 = \sum_{i=1}^{n} |x_i-y_i|

    [lang=csharp]
    double d = Distance.Manhattan(a, b);


Chebyshev Distance
------------------

The chebyshev distance is the $L_\infty$-norm of the difference:

$$$
d_{\mathbf{\infty}} : (x, y) \mapsto \|x-y\|_\infty = \lim_{k \rightarrow \infty}\bigg(\sum_{i=1}^{n} |x_i-y_i|^k\bigg)^\frac{1}{k} = \max_{i} |x_i-y_i|

    [lang=csharp]
    double d = Distance.Chebyshev(a, b);


Minkowski Distance
------------------

The minkovski distance is the generalized $L_p$-norm of the difference:

$$$
d_{\mathbf{p}} : (x, y) \mapsto \|x-y\|_p = \bigg(\sum_{i=1}^{n} |x_i-y_i|^p\bigg)^\frac{1}{p}

    [lang=csharp]
    double d = Distance.Minkowski(p, a, b);


Canberra Distance
-----------------

The canberra distance is a weighted version of the manhattan distance:

$$$
d_{\mathbf{Canberra}} : (x, y) \mapsto \sum_{i=1}^{n} \frac{|x_i-y_i|}{|x_i|+|y_i|}

    [lang=csharp]
    double d = Distance.Canberra(a, b);


Pearson's Distance
------------------

The pearson's distance is based on pearson's product-momentum correlation coefficient of the two sample vectors:

$$$
d_{\mathbf{Pearson}} : (x, y) \mapsto 1 - \mathbf{Corr}(x, y)

    [lang=csharp]
    double d = Distance.Pearson(a, b);


Hamming Distance
----------------

The hamming distance represents the number of entries in the two sample vectors which are different.

    [lang=csharp]
    double d = Distance.Hamming(a, b);
*)
