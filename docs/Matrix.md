    [hide]
    #I "../../out/lib/net40"
    #r "MathNet.Numerics.dll"
    #r "MathNet.Numerics.FSharp.dll"
    open System.Numerics
    open MathNet.Numerics
    open MathNet.Numerics.LinearAlgebra
    open MathNet.Numerics.Distributions

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
spatial problems, geography and geometry have quite different usage patterns and requirements
to linear algebra. All places where Math.NET Numerics can be used have a strong
programming language with their own data structures. For example, if you have a collection of vectors,
consider to store them in a list or array of vectors, not in a matrix (unless you need matrix operations, of course).

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

In F# we can use the builders just like in C#, but we can also use the F# modules:

    [lang=fsharp]
    let m1 = matrix [[ 2.0; 3.0 ]
                     [ 4.0; 5.0 ]]

    let v1 = vector [ 1.0; 2.0; 3.0 ]

    // dense 3x4 matrix filled with zeros.
    // (usually the type is inferred, but not for zero matrices)
    let m2 = DenseMatrix.zero<float> 3 4

    // dense 3x4 matrix initialized by a function
    let m3 = DenseMatrix.init 3 4 (fun i j -> float (i+j))

    // diagonal 4x4 identity matrix of single precision
    let m4 = DiagonalMatrix.identity<float32> 4

    // dense 3x4 matrix created from a sequence of sequence-columns
    let x = Seq.init 4 (fun c -> Seq.init 3 (fun r -> float (100*r + c)))
    let m5 = DenseMatrix.ofColumnSeq x

    // random matrix with standard distribution:
    let m6 = DenseMatrix.randomStandard<float> 3 4

    // random matrix with a uniform and one with a Gamma distribution:
    let m7a = DenseMatrix.random<float> 3 4 (ContinuousUniform(-2.0, 4.0))
    let m7b = DenseMatrix.random<float> 3 4 (Gamma(1.0, 2.0))

Or using any other of all the available functions.


Arithmetics
-----------

All the common arithmetic operators like `+`, `-`, `*`, `/` and `%` are provided,
between matrices, vectors and scalars. In F# there are additional pointwise
operators `.*`, `./` and `.%` available for convenience.

    [lang=fsharp]
    let m = matrix [[ 1.0; 4.0; 7.0 ]
                    [ 2.0; 5.0; 8.0 ]
                    [ 3.0; 6.0; 9.0 ]]

    let v = vector [ 10.0; 20.0; 30.0 ]

    let v' = m * v
    let m' = m + 2.0*m

### Arithmetic Instance Methods

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

### Shortcut Methods

A typical linear algebra problem is the regression normal equation
$\mathbf{X}^T\mathbf y = \mathbf{X}^T\mathbf X \mathbf p$ which we would like to solve
for $p$. By matrix inversion we get $\mathbf p = (\mathbf{X}^T\mathbf X)^{-1}(\mathbf{X}^T\mathbf y)$.
This can directly be translated to the following code:

    [lang=csharp]
    (X.Transpose() * X).Inverse() * (X.Transpose() * y)

Since products where one of the arguments is transposed are common, there are a few shortcut routines
that are more efficient:

    [lang=csharp]
    X.TransposeThisAndMultiply(X).Inverse() * X.TransposeThisAndMultiply(y)

Of course in practice you would not use the matrix inverse but a decomposition:

    [lang=csharp]
    X.TransposeThisAndMultiply(X).Cholesky().Solve(X.TransposeThisAndMultiply(y))
    
    // or if the problem is small enough, simply:
    X.Solve(y);


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


Sums
----

Closely related to the norms are sum functions. Vectors have a `Sum` function
that returns the sum of all vector elements, and `SumMagnitudes` that returns
the sum of the absolute vector elements (and is identical to the L1-norm).

Matrices provide `RowSums` and `ColumnSums` functions that return the sum of each
row or column vector, and `RowAbsoluteSums` and `ColumnAbsoluteSums` for the
sums of the absolute elements.


Condition Number
----------------

The condition number of a function measures how much the output value can change
for a small change in the input arguments. A problem with a low condition number
is said to be *well-conditioned*, with a high condition number *ill-conditioned*.
For a linear equation $Ax=b$ the condition number is the maximum ratio of the
relative error in $x$ divided by the relative error in $b$. It therefore gives a bound on how
inaccurate the solution $x$ will be after approximation.

    [lang=csharp]
    M.Random(4,4).ConditionNumber(); // e.g. 14.829


Trace and Determinant
---------------------

For a square matrix, the trace of a matrix is the sum of the elements on the main diagonal,
which is equal to the sum of all its eigenvalues with multiplicities. Similarly, the determinant
of a square matrix is the product of all its eigenvalues with multiplicities.
A matrix is said to be *singular* if its determinant is zero and *non-singular* otherwise.
In the latter case the matrix is invertible and the linear equation system it
represents has a single unique solution.

    [lang=csharp]
    var m = M.DenseOfArray(new[,] {{ 1.0,  2.0, 1.0},
                                   {-2.0, -3.0, 1.0},
                                   { 3.0,  5.0, 0.0}});

    m.Trace();       // -2
    m.Determinant(); // ~0 hence not invertible, either none or multiple solutions


Column Space, Rank and Range
-----------------------------

The rank of a matrix is the dimension of its column and row space, i.e. the maximum
number of linearly independent column and row vectors of the matrix. It is a measure
of the non-degenerateness of the linear equation system the matrix represents.

An orthonormal basis of the column space can be computed with the range method.

    [lang=csharp]
    // with the same m as above
    m.Rank();  // 2
    m.Range(); // [-0.30519,0.503259,-0.808449], [-0.757315,-0.64296,-0.114355]


Null Space, Nullity and Kernel
------------------------------

The null space or kernel of a matrix $A$ is the set of solutions to the equation $Ax=0$.
It is the orthogonal complement to the row space of the matrix.

The nullity of a matrix is the dimension of its null space.
An orthonormal basis of the null space can be computed with the kernel method.

    [lang=csharp]
    // with the same m as above
    m.Nullity(); // 1
    m.Kernel();  // [0.845154,-0.507093,0.169031]

    // verify:
    (m * (10*m.Kernel()[0])); // ~[0,0,0]


Matrix Decompositions
---------------------

Most common matrix decompositions are directly available as instance methods.
Computing a decomposition can be expensive for large matrices, so if you need
to access multiple properties of a decomposition, consider to reuse the returned instance.

All decompositions provide Solve methods than can be used to solve linear
equations of the form $Ax=b$ or $AX=B$. For simplicity the Matrix class
also provides direct `Solve` methods that automatically choose
a decomposition. See [Linear Equation Systems](LinearEquations.html) for details.

Currently these decompositions are optimized for dense matrices only,
and can leverage native providers like Intel MKL if available.
For sparse data consider to use the iterative solvers instead if appropriate,
or convert to dense if small enough.

* **Cholesky**: Cholesky decomposition of symmetric positive definite matrices
* **LU**: LU decomposition of square matrices
* **QR(method)**: QR by Householder transformation.
  Thin by default (Q: mxn, R: nxn) but can optionally be computed fully (Q: mxm, R: mxn).
* **GramSchmidt**: QR by Modified Gram-Schmidt Orthogonalization
* **Svd(computeVectors)**: Singular Value Decomposition.
  Computation of the singular U and VT vectors can optionally be disabled.
* **Evd(symmetricity)**: Eigenvalue Decomposition.
  If the symmetricity of the matrix is known, the algorithm can optionally skip its own check.


Manipulating Matrices and Vectors
---------------------------------

Individual values can be get and set in matrices and vectors using the indexers
or the `At` methods. Using `At` instead of the indexers is slightly faster but
skips some range checks, so use it only after checking the range yourself.

    [lang=csharp]
    var m = Matrix<double>.Build.Dense(3,4,(i,j) => 10*i + j);
    m[0,0]; // 0   (row 0, column 0)
    m[2,0]; // 20 (row 2, column 0)
    m[0,2]; // 2   (row 0, column 2)
    m[0,2] = -1.0;
    m[0,2]; // -1

In F#:

    [lang=fsharp]
    m.[2,0] // 20

We can also get entire column or row vectors, or a new matrix from parts of an existing one.

    [lang=csharp]
    var m = M.Dense(6,4,(i,j) => 10*i + j);
    m.Column(2);          // [2,12,22,32,42,52]
    m.Row(3);             // [30,31,32,33]
    m.SubMatrix(1,2,1,2); // [11,12; 21,22]

For each of these methods there is also a variant prefixed with `Set` that can be used
to overwrite those elements with the provided data.

    [lang=csharp]
    m.SetRow(3, V.Random(4));

In F# we can also use its slicing syntax:

    [lang=fsharp]
    let m = DenseMatrix.init 6 4 (fun i j -> float (10*i + j))
    m.[0,0..3]    // vector [0,1,2,3]
    m.[1..2,0..3] // matrix [10,11,12,13; 20,21,22,23]
    // overwrite a sub-matrix with the content of another matrix:
    m.[0..1,1..2] <- matrix [[ 3.0; 4.0 ]; [ 5.0; 6.0 ]]

To set the whole matrix or some of its columns or rows to zero, use one of the clear methods:

    [lang=csharp]
    m.Clear(); // set all elements to 0
    m.ClearColumn(2); // set the 3rd column to 0 (0-based indexing)
    m.ClearColumns(1,3); // set the 2nd and 4th columns to 0 (params-array)
    m.ClearSubMatrix(1,2,1,2); // set the 2x2 submatrix with offset 1,1 to zero

Because of the limitations of floating point numbers, we may want to set very small numbers to zero:

    [lang=csharp]
    m.CoerceZero(1e-14); // set all elements smaller than 1e-14 to 0
    m.CoerceZero(x => x < 10); // set all elements that match a predicate function to 0.

Even though matrices and vectors are mutable, their dimension is fixed and cannot be changed
after creation. However, we can still insert or remove rows or columns, or concatenate matrices together.
But all these operations will create and return a new instance.

    [lang=csharp]
    var m2 = m.RemoveRow(2); // remove the 3rd rows
    var m3 = m2.RemoveColumn(3); // remove the 4th column

    var m4 = m.Stack(m2); // new matrix with m on top and m2 on the bottom
    var m5 = m2.Append(m3); // new matrix with m2 on the left and m3 on the right
    var m6 = m.DiagonalStack(m3); // m on the top left and m3 on the bottom right


Enumerators and Higher Order Functions
--------------------------------------

Since looping over all entries of a matrix or vector with direct access is inefficient,
especially with a sparse storage layout, and working with the raw structures is non-trivial,
both vectors and matrices provide specialized enumerators and higher order functions that
understand the actual layout and can use it more efficiently.

Most of these functions can optionally skip zero-value entries. If you do not need to handle
zero-value elements, skipping them can massively speed up execution on sparse layouts.

### Iterate

Both vectors and matrices have Enumerate methods that return an `IEnumerable<T>`,
that can be used to iterate through all elements. All these methods optionally
accept a `Zeros` enumeration to control whether zero-values may be skipped or not.

* **Enumerate**: returns a straight forward enumerator over all values.
* **EnumerateIndexed**: returns an enumerable with index-value-tuples.

Matrices can also enumerate over all column or row vectors, or all of them
within a range:

* **EnumerateColumns**: returns an enumerable with all or a range of the column vectors.
* **EnumerateColumnsIndexed**: like EnumerateColumns buth returns index-column tuples.
* **EnumerateRows**: returns an enumerable with all or a range of the row vectors.
* **EnumerateRowsIndexed**: like EnumerateRows buth returns index-row tuples.

### Map

Similarly there are also Map methods that replace each element with the result
of applying a function to its value. Or, if indexed, to its index and value.

* **MapInplace(f,zeros)**: map in-place with a function on the element's value
* **MapIndexedInplace(f,zeros)**: map in-place with a function on the element's index and value.
* **Map(f,result,zeros)**: map into a result structure provided as argument.
* **MapIndexed(f,result,zeros)**: indexed variant of Map.
* **MapConvert(f,result,zeros)**: variant where the function can return a different type
* **MapIndexedConvert(f,result,zeros)**: indexed variant of MapConvert.
* **Map(f,zeros)**: like MapConvert but returns a new structure instead of the result argument.
* **MapIndexed(f,zeros)**: indexed variant of Map.

Example: Convert a complex vector to a real vector containing only the real parts in C#:

    [lang=csharp]
    Vector<Complex> u = Vector<Complex>.Build.Random(10);
    Vector<Double> v = u.Map(c => c.Real);

Or in F#:

    [lang=fsharp]
    let u = DenseVector.randomStandard<Complex> 10
    let v = u |> Vector.map (fun c -> c.Real)

### Fold and Reduce

Matrices also provide column/row fold and reduce routines:

* **FoldByRow(f,state,zeros)**: fold through the values of each row, returns an column-array.
* **FoldRows(f,state)**: fold over all row vectors, returns a row vector.
* **ReduceRows(f)**: reduce all row vectors, returns a row vector.


Printing and Strings
--------------------

Matrices and vectors try to print themselves to a string with the `ToString`
in a reasonable way, without overflowing the output device on a large matrix.

Note that this function is not intended to export a data structure to a string or
file, but to give an informative summary about it. For data import/export,
use one of the MathNet.Numerics.Data packages instead.

Some matrix examples:

    [lang=text]
    // Matrix<double>.Build.Dense(3,4,(i,j) => i*10*j).ToString()
    DenseMatrix 3x4-Double
    0   0   0   0
    0  10  20  30
    0  20  40  60

    // Matrix<double>.Build.Dense(100,100,(i,j) => i*10*j).ToString()
    DenseMatrix 100x100-Double
     0    0     0     0     0     0     0     0     0     0     0  ..      0      0
     0   10    20    30    40    50    60    70    80    90   100  ..    980    990
     0   20    40    60    80   100   120   140   160   180   200  ..   1960   1980
     0   30    60    90   120   150   180   210   240   270   300  ..   2940   2970
     0   40    80   120   160   200   240   280   320   360   400  ..   3920   3960
     0   50   100   150   200   250   300   350   400   450   500  ..   4900   4950
     0   60   120   180   240   300   360   420   480   540   600  ..   5880   5940
     0   70   140   210   280   350   420   490   560   630   700  ..   6860   6930
    ..   ..    ..    ..    ..    ..    ..    ..    ..    ..    ..  ..     ..     ..
     0  960  1920  2880  3840  4800  5760  6720  7680  8640  9600  ..  94080  95040
     0  970  1940  2910  3880  4850  5820  6790  7760  8730  9700  ..  95060  96030
     0  980  1960  2940  3920  4900  5880  6860  7840  8820  9800  ..  96040  97020
     0  990  1980  2970  3960  4950  5940  6930  7920  8910  9900  ..  97020  98010

    // Matrix<double>.Build.Random(4,4).ToString()
    DenseMatrix 4x4-Double
      1.6286    -1.1126    1.95526  0.950545
    0.537503  -0.465534    2.00984   1.90885
    -1.62816    1.04109   -2.06876  0.812197
    0.452355  -0.689394  -0.277921   2.72224

    // Matrix<double>.Build.SparseOfIndexed(4,100,new[] {Tuple.Create(1,2,3.0)})
    SparseMatrix 4x100-Double 0.25% Filled
    0  0    0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  ..  0  0
    0  0  3.5  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  ..  0  0
    0  0    0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  ..  0  0
    0  0    0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  0  ..  0  0

Vectors are printed as a column that can wrap over to multiple columns if needed:

    [lang=text]
    // Vector<double>.Build.Random(15).ToString()
    DenseVector 15-Double
     0.519184  0.0950414
      1.65913    2.56783
     0.743408   0.574037
     -1.73394           
    -0.906662           
     0.853759           
    -0.162181           
    -0.231667           
     -1.26393           
    -0.434873           
     0.693421           
    -0.513683           

    // Vector<double>.Build.Dense(500,i => i).ToString()
    DenseVector 500-Double
     0  12  24  36  48  60  72  84   96  108  120  132  144  156  168  180  192
     1  13  25  37  49  61  73  85   97  109  121  133  145  157  169  181  193
     2  14  26  38  50  62  74  86   98  110  122  134  146  158  170  182  194
     3  15  27  39  51  63  75  87   99  111  123  135  147  159  171  183  195
     4  16  28  40  52  64  76  88  100  112  124  136  148  160  172  184  196
     5  17  29  41  53  65  77  89  101  113  125  137  149  161  173  185  197
     6  18  30  42  54  66  78  90  102  114  126  138  150  162  174  186  198
     7  19  31  43  55  67  79  91  103  115  127  139  151  163  175  187  199
     8  20  32  44  56  68  80  92  104  116  128  140  152  164  176  188   ..
     9  21  33  45  57  69  81  93  105  117  129  141  153  165  177  189   ..
    10  22  34  46  58  70  82  94  106  118  130  142  154  166  178  190  498
    11  23  35  47  59  71  83  95  107  119  131  143  155  167  179  191  499

The format is customizable to some degree, for example we can choose the
floating point format and culture, or how many rows or columns should be shown:

    [lang=text]
    // var m = Matrix<double>.Build.Random(5,100,42); // 42 = random seed
    
    // m.ToString()
    DenseMatrix 5x100-Double
     0.408388  -0.847291  -0.320552   0.162242    2.46434  ..   0.180466   -0.278793
     -1.06988   0.063008  -0.527378    1.40716    -0.5962  ..  -0.622447   -0.488186
    -0.734176  -0.703003    1.33158   0.286498    1.44158  ..  -0.834335  -0.0756724
      1.78532   0.020217    1.94275  -0.742821  -0.790251  ..    1.52823     2.49427
    -0.660645    1.28166   -1.71351   -1.33282  -0.328162  ..   0.110989    0.252272

    // m.ToString("G2", CultureInfo.GetCultureInfo("de-DE"))
    DenseMatrix 5x100-Double
     0,41  -0,85  -0,32   0,16    2,5     -0,77   0,12   0,58  ..   0,18   -0,28
     -1,1  0,063  -0,53    1,4   -0,6      -2,8  -0,35    0,3  ..  -0,62   -0,49
    -0,73   -0,7    1,3   0,29    1,4  -0,00022   -0,3   0,51  ..  -0,83  -0,076
      1,8   0,02    1,9  -0,74  -0,79     0,088   0,78  -0,94  ..    1,5     2,5
    -0,66    1,3   -1,7   -1,3  -0,33     -0,69  -0,27  -0,68  ..   0,11    0,25

    // m.ToString(3,5) // max 3 rows, 5 columns
    DenseMatrix 5x100-Double
     0.408388  -0.847291  -0.320552  ..   0.180466   -0.278793
     -1.06988   0.063008  -0.527378  ..  -0.622447   -0.488186
    -0.734176  -0.703003    1.33158  ..  -0.834335  -0.0756724
           ..         ..         ..  ..         ..          ..

    // Matrix<double>.Build.Random(100,100,42)
    // .ToMatrixString(2,4,3,4,"=","||",@"\\"," ",Environment.NewLine,x=>x.ToString("G2"))
     0.41   0.36  0.29  =  0.43 0.56   -0.56  0.98
     -1.1  -0.64   0.9  =  0.49 -0.3       2  -0.5
       ||     ||    || \\    ||   ||      ||    ||
    -0.87   -2.2  0.79  =  0.96  1.8     1.4 0.067
    -0.14 -0.016 -0.55  = -0.36 0.33    0.24  0.52
     -1.3     -1 -0.81  =   1.3    1    -1.1 -0.28
    -0.21   -1.7   2.6  =  -1.5 -1.2 -0.0014   3.4

If you are using Math.NET Numerics from within F# interactive, you may want
to load the MathNet.Numerics.fsx script of the F# package. Besides loading
the assemblies it also adds proper FSI printers for both matrices and vectors.
