(*** hide ***)
#I "../../out/lib/net40"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"
open MathNet.Numerics.LinearAlgebra

(**
Matrices and Vectors
====================

Math.NET Numerics includes rich types for matrices and vectors.
They support both single and double precision, real and complex floating point numbers.


$$$
\mathbf{A}=
\begin{bmatrix}
a_{0,0} & a_{0,1} & \cdots & a_{0,(n-1)} \\
a_{1,0} & a_{1,1} & \cdots & a_{1,(n-1)} \\
\vdots & \vdots & \ddots & \vdots \\
a_{(m-1),0} & a_{(m-1),1} & \cdots & a_{(m-1),(n-1)}
\end{bmatrix},\quad
\mathbf{v}=\begin{bmatrix}v_0\\v_1\\ \vdots \\v_{n-1}\end{bmatrix}

Like all data structures in .Net they are 0-indexed, i.e. the top left cell has index (0,0). In matrices,
the first index always refers to the row and the second index to the column.
Empty matrices or vectors are not supported, i.e. each dimension must have a length of at least 1.

### Context: Linear Algebra

The context and primary scenario for these types is linear algebra. Their API is broad enough
to use them in other contexts as well, but they are *not* optimized for geometry or
as general purpose storage structure as common in MATLAB. This is intentional, as
spatial problems, geography and geometry have very different usage patterns and requirements
than linear algebra. Also, all places where Math.NET Numerics can be used have a strong
programming language with their own data structures. For example, if you have a collection of vectors,
consider to store them in a list or array, not in a matrix (unless you need matrix operations, of course).
 
Storage Layout
--------------

Both dense and sparse vectors are supported:

* **Dense Vector** uses a single array of the same length as the vector.
* **Sparse Vector** uses two arrays which are usually much shorter than the vector.
  One array stores all values that are not zero, the other stores their indices.
  They are sorted ascendingly by index.

Matrices can be either dense, diagonal or sparse:

* **Dense Matrix** uses a single array in column-major order.
* **Diagonal Matrix** stores only the diagonal values, in a single array.
* **Sparse Matrix** stores non-zero values in 3 arrays in the standard compressed sparse row (CSR) format.
  One array stores all values that are not zero, another array of the same length stores
  the their corresponding column index. The third array of the length of the number of rows plus one,
  stores the offsets where each row starts, and the total number of non-zero values in the last field.

If your data  contains only very few zeros, using the sparse variant is orders of magnitudes
slower than their dense counterparts, so consider to use dense types unless the data is very sparse (i.e. almost all zeros).

Creating Matrices and Vectors
-----------------------------

The `Matrix<T>` and `Vector<T>` types are defined in the `MathNet.Numerics.LinearAlgebra` namespace.

For technical and performance reasons there are distinct implementations for each data type.
For example, for double precision numbers there is a `DenseMatrix` class in the `MathNet.Numerics.LinearAlgebra.Double`
namespace. You do not normally need to be aware of that, but as consequence the generic `Matrix<T>` type is abstract
and we need other ways to create a matrix or vector instance.

The matrix and vector builder provide functions to create instances from a variety of formats or approaches.

    [lang=csharp]
    // create a dense matrix with 3 rows and 4 columns
    // filled with random numbers sampled from the standard distribution
    Matrix<double> m = Matrix<double>.Build.Random(3, 4);

    // create a dense zero-vector of length 10
    Vector<double> v = Vector<double>.Build.Dense(10);

Since within an application you often only work with one specific data type, a common trick to keep this a bit shorter
is to define shortcuts to the builders:

    [lang=csharp]
    var M = Matrix<double>.Build;
    var V = Vector<double>.Build;

    // build the same as above
    var m = M.Random(3, 4);
    var v = V.Dense(10);

The builder functions usually start with the layout (Dense, Sparse, Diagonal),
so if we'd like to build a sparse matrix, intellisense will list all available options
together once you type `M.Sparse`.

There are variants to generate synthetic matrices, for example:

    [lang=csharp]
    // 3x4 dense matrix filled with zeros
    M.Dense(3, 4);

    // 3x4 dense matrix filled with 1.0.
    M.Dense(3, 4, 1.0);

    // 3x4 dense matrix where each field is initialized using a function
    M.Dense(3, 4, (i,j) => 100*i + j);

    // 3x4 square dense matrix with each diagonal value set to 2.0
    M.DenseDiagonal(3, 4, 2.0);

    // 3x3 dense identity matrix
    M.DenseIdentity(3);

    // 3x4 dense random matrix sampled from a Gamma distribution
    M.Random(3, 4, new Gamma(1.0, 5.0));


But often we already have data available in some format and
need a matrix representing the same data. Whenever a function contains
"Of" in its name it does create a copy of the original data.

    [lang=csharp]
    // Copy of an existing matrix (can also be sparse or diagonal)
    Matrix<double> x = ...
    M.DenseOfMatrix(x);

    // Directly bind to an existing column-major array without copying (note: no "Of")
    double[] x = existing...
    M.Dense(3, 4, x);

    // From a 2D-array
    double[,] x = {{ 1.0, 2.0 },
                   { 3.0, 4.0 }};
    M.DenseOfArray(x);

    // From an enumerable of values and their coordinates
    Tuple<int,int,double>[] x = {Tuple.Create(0,0,2.0), Tuple.Create(0,1,-3.0)};
    M.DenseOfIndexed(3,4,x);

    // From an enumerable in column major order (column by column)
    double[] x = {1.0, 2.0, 3.0, 4.0};
    M.DenseOfColumnMajor(2, 2, x);

    // From an enumerable of enumerable-columns (optional with explicit size)
    IEnumerable<IEnumerable<double>> x = ...
    M.DenseOfColumns(x);

    // From a params-array of array-columns (or an enumerable of them)
    M.DenseOfColumnArrays(new[] {2.0, 3.0}, new[] {4.0, 5.0});

    // From a params-array of column vectors (or an enumerable of them)
    M.DenseOfColumnVectors(V.Random(3), V.Random(3));

    // Equivalent variants also for rows or diagonals:
    M.DenseOfRowArrays(new[] {2.0, 3.0}, new[] {4.0, 5.0});
    M.DenseOfDiagonalArray(new[] {2.0, 3.0, 4.0});

    // if you already have existing matrices and want to concatenate them
    Matrix<double>[,] x = ...
    M.DenseOfMatrixArray(x);

Very similar variants also exist for sparse and diagonal matrices, prefixed
with `Sparse` and `Diagonal` respectively.

The approach for vectors is exactly the same:

    [lang=csharp]
    // Standard-distributed random vector of length 10
    V.Random(10);

    // All-zero vector of length 10
    V.Dense(10);

    // Each field is initialized using a function
    V.Dense(10, i => i*i);

    // From an enumerable of values and their index
    Tuple<int,double>[] x = {Tuple.Create(3,2.0), Tuple.Create(1,-3.0)};
    V.DenseOfIndexed(x);

    // Directly bind to an existing array without copying (note: no "Of")
    double[] x = existing...
    V.Dense(x);

### Creating matrices and vectors in F#

In F# we can use the builds just like in C#, but we can also use the F# modules:
*)

let m1 = matrix [[ 2.0; 3.0 ]
                [ 4.0; 5.0 ]]

let v1 = vector [ 1.0; 2.0; 3.0 ]

// dense 3x4 matrix filled with zeros
let m2 = DenseMatrix.zero<float> 3 4

// dense 3x4 matrix initialized by a function
let m3 = DenseMatrix.init 3 4 (fun i j -> float (i+j))

// diagonal 4x4 identity matrix of single precision
let m4 = DiagonalMatrix.identity<float32> 4

// dense 3x4 matrix created from a sequence of sequence-columns
let x = Seq.init 4 (fun c -> Seq.init 3 (fun r -> float (100*r + c)))
let m5 = DenseMatrix.ofColumnSeq x

(**
Or using any other of all the available functions.


Arithmetics
-----------

All the common arithmetic operators like `+`, `-`, `*`, `/` and `%` are provided,
between matrices, vectors and scalars. In F# there are additional pointwise
operators `.*`, `./` and `.%` available for convenience.
*)

let m = matrix [[ 1.0; 4.0; 7.0 ]
                [ 2.0; 5.0; 8.0 ]
                [ 3.0; 6.0; 9.0 ]]

let v = vector [ 10.0; 20.0; 30.0 ]

let v2 = m * v
let m2 = m + 2.0*m

(**
All other operations are covered by methods, like `Transpose` and `Conjugate`,
or in F# as functions in the Matrix module, e.g. `Matrix.transpose`.
But even the operators have equivalent methods. The equivalent code from
above when using instance methods:

    [lang=csharp]
    var v2 = m.Multiply(v);
    var m2 = m.Add(m.Multiply(2));

These methods also have an overload that accepts the result data structure as last argument,
allowing to avoid allocating new structures for every single operation. Provided the
dimensions match, most also allow one of the arguments to be passed as result,
resulting in an in-place application. For example, an in-place version of the code above:

    [lang=csharp]
    m.Multiply(v, v); // v <- m*v
    m.Multiply(3, m); // m <- 3*m


Norms
-----

With norms we assign a "size" to vectors and matrices, satisfying certain
properties pertaining to scalability and additivity. Except for the zero element,
the norm is strictly positive.

Vectors support the following norms:

* **L1Norm** or Manhattan norm (p=1): the sum of the absolute values.
* **L2Norm** or Euclidean norm (p=2): the square root of the sum of the squared values.
  This is the most common norm and assumed if nothing else is stated.
* **InfinityNorm** (p=infinity): the maximum absolute value.
* **Norm(p)**: generalized norm, essentially the p-th root of the sum of the absolute p-power of the values.

Similarly, matrices support the following norms:

* **L1Norm** (induced): the maximum absolute column sum.
* **L2Norm** (induced): the largest singular value of the matrix (expensive).
* **InfinityNorm** (induced): the maximum absolute row sum.
* **FrobeniusNorm** (entry-wise): the square root of the sum of the squared values.
* **RowNorms(p)**: the generalized p-norm for each row vector.
* **ColumnNorms(p)**: the generalized p-norm for each column vector.

Vectors can be normalized to unit p-norm with the `Normalize` method, matrices can
normalize all rows or all columns to unit p-norm with `NormalizeRows` and `NormalizeColumns`.


Rank, Trace, Determinant & Condition
------------------------------------

Kernel and Range
----------------


Matrix Decompositions
---------------------

### Cholesky Decomposition

### LU Decomposition

### QR Decomposition

### Singular Value Decomposition

### Eigenvalue Decomposition

Manipulating Matrices and Vectors
---------------------------------


Higher Order Functions
----------------------


Printing and Strings
--------------------



*)