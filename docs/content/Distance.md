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

<img src="img/DistanceSAD.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The sum of absolute difference is equivalent to the $L_1$-norm of the difference, also known as Manhattan- or Taxicab-norm.
The `abs` function makes this metric a bit complicated to deal with analytically, but it is more robust than SSD.

$$$
d_{\mathbf{SAD}} : (x, y) \mapsto \|x-y\|_1 = \sum_{i=1}^{n} |x_i-y_i|

    [lang=csharp]
    double d = Distance.SAD(x, y);


Sum of Squared Difference (SSD)
-------------------------------

<img src="img/DistanceSSD.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The sum of squared difference is equivalent to the squared $L_2$-norm, also known as Euclidean norm.
It is therefore also known as Squared Euclidean distance.
This is the fundamental metric in least squares problems and linear algebra. The absence of the `abs`
function makes this metric convenient to deal with analytically, but the squares cause it to be very
sensitive to large outliers.

$$$
d_{\mathbf{SSD}} : (x, y) \mapsto \|x-y\|_2^2 = \langle x-y, x-y\rangle = \sum_{i=1}^{n} (x_i-y_i)^2

    [lang=csharp]
    double d = Distance.SSD(x, y);


Mean-Absolute Error (MAE)
-------------------------

<img src="img/DistanceMAE.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The mean absolute error is a normalized version of the sum of absolute difference.

$$$
d_{\mathbf{MAE}} : (x, y) \mapsto \frac{d_{\mathbf{SAD}}}{n} = \frac{\|x-y\|_1}{n} = \frac{1}{n}\sum_{i=1}^{n} |x_i-y_i|

    [lang=csharp]
    double d = Distance.MAE(x, y);


Mean-Squared Error (MSE)
------------------------

<img src="img/DistanceMSE.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The mean squared error is a normalized version of the sum of squared difference.

$$$
d_{\mathbf{MSE}} : (x, y) \mapsto \frac{d_{\mathbf{SSD}}}{n} = \frac{\|x-y\|_2^2}{n} = \frac{1}{n}\sum_{i=1}^{n} (x_i-y_i)^2

    [lang=csharp]
    double d = Distance.MSE(x, y);


Euclidean Distance
------------------

<img src="img/DistanceEuclidean.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The euclidean distance is the $L_2$-norm of the difference, a special case of the Minkowski distance with p=2.
It is the natural distance in a geometric interpretation.

$$$
d_{\mathbf{2}} : (x, y) \mapsto \|x-y\|_2 = \sqrt{d_{\mathbf{SSD}}} = \sqrt{\sum_{i=1}^{n} (x_i-y_i)^2}

    [lang=csharp]
    double d = Distance.Euclidean(x, y);


Manhattan Distance
------------------

<img src="img/DistanceManhattan.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The Manhattan distance is the $L_1$-norm of the difference, a special case of the Minkowski distance with p=1
and equivalent to the sum of absolute difference.

$$$
d_{\mathbf{1}} \equiv d_{\mathbf{SAD}} : (x, y) \mapsto \|x-y\|_1 = \sum_{i=1}^{n} |x_i-y_i|

    [lang=csharp]
    double d = Distance.Manhattan(x, y);


Chebyshev Distance
------------------

<img src="img/DistanceChebyshev.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The Chebyshev distance is the $L_\infty$-norm of the difference, a special case of the Minkowski distance
where p goes to infinity. It is also known as Chessboard distance. 

$$$
d_{\mathbf{\infty}} : (x, y) \mapsto \|x-y\|_\infty = \lim_{p \rightarrow \infty}\bigg(\sum_{i=1}^{n} |x_i-y_i|^p\bigg)^\frac{1}{p} = \max_{i} |x_i-y_i|

    [lang=csharp]
    double d = Distance.Chebyshev(x, y);


Minkowski Distance
------------------

<img src="img/DistanceMinkowski3.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The Minkowski distance is the generalized $L_p$-norm of the difference.
The contour plot on the left demonstrates the case of p=3.

$$$
d_{\mathbf{p}} : (x, y) \mapsto \|x-y\|_p = \bigg(\sum_{i=1}^{n} |x_i-y_i|^p\bigg)^\frac{1}{p}

    [lang=csharp]
    double d = Distance.Minkowski(p, x, y);


Canberra Distance
-----------------

<img src="img/DistanceCanberra.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The Canberra distance is a weighted version of the Manhattan distance, introduced and refined 1967 by Lance, Williams and Adkins.
It is often used for data scattered around an origin, as it is biased for measures around the origin and very sensitive for values close to zero.

$$$
d_{\mathbf{CAD}} : (x, y) \mapsto \sum_{i=1}^{n} \frac{|x_i-y_i|}{|x_i|+|y_i|}

    [lang=csharp]
    double d = Distance.Canberra(x, y);


Cosine Distance
---------------

<img src="img/DistanceCosine.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The cosine distance contains the dot product scaled by the product of the Euclidean distances from the origin.
It represents the angular distance of two vectors while ignoring their scale.

$$$
d_{\mathbf{cos}} : (x, y) \mapsto 1-\frac{\langle x, y\rangle}{\|x\|_2\|y\|_2} = 1-\frac{\sum_{i=1}^{n} x_i y_i}{\sqrt{\sum_{i=1}^{n} x_i^2}\sqrt{\sum_{i=1}^{n} y_i^2}}

    [lang=csharp]
    double d = Distance.Cosine(x, y);


Pearson's Distance
------------------

<img src="img/DistancePearson.png" style="width:87px; height:87px; float:left; margin:10px 10px 10px 0;" />

The Pearson distance is a correlation distance based on Pearson's product-momentum correlation coefficient
of the two sample vectors. Since the correlation coefficient falls between [-1, 1], the Pearson distance
lies in [0, 2] and measures the linear relationship between the two vectors.

$$$
d_{\mathbf{Pearson}} : (x, y) \mapsto 1 - \mathbf{Corr}(x, y)

    [lang=csharp]
    double d = Distance.Pearson(x, y);


Hamming Distance
----------------

The hamming distance represents the number of entries in the two sample vectors which are different.
It is a fundamental distance measure in information theory but less relevant in non-integer numerical problems.

    [lang=csharp]
    double d = Distance.Hamming(x, y);
