    [hide]
    #I "../../out/lib/net40"
    #r "MathNet.Numerics.dll"
    #r "MathNet.Numerics.FSharp.dll"
    open System.Numerics
    open MathNet.Numerics
    open MathNet.Numerics.LinearAlgebra

Linear Equation Systems
=======================

A system of linear equations is a collection of linear equations involving the same set of variables:

$$$
\begin{alignat}{7}
3x &\; + \;& 2y             &\; - \;& z  &\; = \;& 1 & \\
2x &\; - \;& 2y             &\; + \;& 4z &\; = \;& -2 & \\
-x &\; + \;& \tfrac{1}{2} y &\; - \;& z  &\; = \;& 0 &
\end{alignat}

More generally, we can write

$$$
\begin{alignat}{7}
a_{11} x_1 &&\; + \;&& a_{12} x_2   &&\; + \cdots + \;&& a_{1n} x_n &&\; = \;&&& b_1 \\
a_{21} x_1 &&\; + \;&& a_{22} x_2   &&\; + \cdots + \;&& a_{2n} x_n &&\; = \;&&& b_2 \\
\vdots\;\;\; &&     && \vdots\;\;\; &&                && \vdots\;\;\; &&     &&& \;\vdots \\
a_{m1} x_1 &&\; + \;&& a_{m2} x_2   &&\; + \cdots + \;&& a_{mn} x_n &&\; = \;&&& b_m \\
\end{alignat}

where we all parameters $a_{ij}$ and $b_i$ are known and we would like to find $x_j$ that satisfy
all these equations. If we have the same number $n$ of unknown variables $x_j$ as number of
equations $m$, and all these equations are independent, then there is a unique solution.

This is a fundamental problem in the domain of linear algebra, and we can use its power to find the solution.
Accordingly we can write the equivalent problem with matrices and vectors:

$$$
\mathbf{A}=
\begin{bmatrix}
a_{11} & a_{12} & \cdots & a_{1n} \\
a_{21} & a_{22} & \cdots & a_{2n} \\
\vdots & \vdots & \ddots & \vdots \\
a_{m1} & a_{m2} & \cdots & a_{mn}
\end{bmatrix},\quad
\mathbf{x}=\begin{bmatrix}x_1\\x_2\\ \vdots \\x_n\end{bmatrix},\quad
\mathbf{b}=\begin{bmatrix}b_1\\b_2\\ \vdots \\b_m\end{bmatrix}

such that

$$$
\mathbf{A}\mathbf{x}=\mathbf{b}

The initial example system would then look like this:

$$$
\begin{bmatrix}3 & 2 & -1 \\2 & -2 & 4 \\-1 & \tfrac{1}{2} & -1\end{bmatrix}
\begin{bmatrix}x\\y\\z\end{bmatrix}
\;=\;
\begin{bmatrix}1\\-2\\0\end{bmatrix}

Which we can solve explicitly with the LU-decomposition, or simply by using the Solve method:

    [lang=csharp]
    var A = Matrix<double>.Build.DenseOfArray(new double[,] {
        { 3, 2, -1 },
        { 2, -2, 4 },
        { -1, 0.5, -1 }
    });
    var b = Vector<double>.Build.Dense(new double[] { 1, -2, 0 });
    var x = A.Solve(b);

The resulting $\mathbf{x}$ is $[1,\;-2,\;-2]$, hence the solution $x=1,\;y=-2,\;z=-2$.

In F# the syntax is a bit lighter:

    [lang=fsharp]
    let A = matrix [[ 3.0; 2.0; -1.0 ]
                    [ 2.0; -2.0; 4.0 ]
                    [ -1.0; 0.5; -1.0 ]]
    let b = vector [ 1.0; -2.0; 0.0 ]
    let x = A.Solve(b) // 1;-2;-2


Normalizing Equation Systems
----------------------------

In practice, a linear equation system to be solved is often not in the standard form required
to use the linear algebra approach. For example, let's have a look at the following system:

$$$
\begin{bmatrix}1 & 2 & 3 & 4\\2 & 3 & 4 & 5\\3 & 4 & 5 & 6\\4 & 5 & 6 & 7\end{bmatrix}
\begin{bmatrix}0\\0\\V\\T\end{bmatrix}
\;=\;
\begin{bmatrix}F\\M\\20\\0\end{bmatrix}

The first two values of the solution vector $[0,\;0,\;V,\;T]$ are constant zero, so we can simplify
the system to:

$$$
\begin{bmatrix}3 & 4\\4 & 5\\5 & 6\\6 & 7\end{bmatrix}
\begin{bmatrix}V\\T\end{bmatrix}
\;=\;
\begin{bmatrix}F\\M\\20\\0\end{bmatrix}

Then we need to subtract the two unknowns from the right side back from the left (so that they
become zero on the right side), by introducing a new column each. First we subtract
$[F,\;0,\;0,\;0]^T$ from both sides:

$$$
\begin{bmatrix}3 & 4 & -1\\4 & 5 & 0\\5 & 6 & 0\\6 & 7 & 0\end{bmatrix}
\begin{bmatrix}V\\T\\F\end{bmatrix}
\;=\;
\begin{bmatrix}0\\M\\20\\0\end{bmatrix}

Then we subtract $[0,\;M,\;0,\;0]^T$ from both sides the same way:

$$$
\begin{bmatrix}3 & 4 & -1 & 0\\4 & 5 & 0 & -1\\5 & 6 & 0 & 0\\6 & 7 & 0 & 0\end{bmatrix}
\begin{bmatrix}V\\T\\F\\M\end{bmatrix}
\;=\;
\begin{bmatrix}0\\0\\20\\0\end{bmatrix}

Which is in standard from, so we can solve normally:

    [lang=fsharp]
    let A' = matrix [[ 3.0; 4.0; -1.0; 0.0 ]
                     [ 4.0; 5.0; 0.0; -1.0 ]
                     [ 5.0; 6.0; 0.0; 0.0; ]
                     [ 6.0; 7.0; 0.0; 0.0 ]]
    let b' = vector [ 0.0; 0.0; 20.0; 0.0 ]
    let x' = A'.Solve(b') // -140; 120; 60; 40
