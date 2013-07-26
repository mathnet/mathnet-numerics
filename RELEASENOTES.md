Math.NET Numerics Release Notes
===============================

- **Website: [numerics.mathdotnet.com](http://numerics.mathdotnet.com)**
- GitHub/Mainline: [github.com/mathnet/mathnet-numerics](https://github.com/mathnet/mathnet-numerics)
- CodePlex: [mathnetnumerics.codeplex.com](http://mathnetnumerics.codeplex.com)
- License: MIT/X11 Open Source

NuGet Packages, available in the [NuGet Gallery](https://nuget.org/profiles/mathnet/):

- `MathNet.Numerics` - core package, including both .Net 4 and portable builds
- `MathNet.Numerics.FSharp` - optional extensions for a better F# experience
- `MathNet.Numerics.Data.Text` - NEW: optional extensions for text-based matrix input/output (CSV for now)
- `MathNet.Numerics.Data.Matlab` - NEW: optional extensions for Matlab matrix file input/output
- `MathNet.Numerics.MKL.Win-x86` - optional Linear Algebra MKL native provider
- `MathNet.Numerics.MKL.Win-x64` - optional Linear Algebra MKL native provider
- `MathNet.Numerics.MKL.Linux-x86` - optional Linear Algebra MKL native provider
- `MathNet.Numerics.MKL.Linux-x64` - optional Linear Algebra MKL native provider
- `MathNet.Numerics.Signed` - strong-named version of the core package (not recommended)
- `MathNet.Numerics.Sample` - code samples in C#
- `MathNet.Numerics.FSharp.Sample` - code samples in F#

Zip Packages, available on [CodePlex](http://mathnetnumerics.codeplex.com/releases):

- Binaries - core package and F# extensions, including both .Net 4 and portable builds
- Signed Binaries - strong-named version of the core package (not recommended)

Over time some members and classes have been replaced with more suitable alternatives. In order to maintain compatibility, such parts are not removed immediately but instead marked with the **Obsolete**-attribute. We strongly recommend to follow the instructions in the attribute text whenever you find any code calling an obsolete member, since we *do* intend to remove them at the next *major* release, v3.0.

v2.6.0 - July 26, 2013
------------------------------

See also: [What's New in Math.NET Numerics 2.6](http://christoph.ruegg.name/blog/new-in-mathnet-numerics-2-6.html): Announcement, Explanations and Sample Code.

### New: Linear Curve Fitting

- Linear least-squares fitting (regression) to lines, polynomials and linear combinations of arbitrary functions.
- Multi-dimensional fitting.
- Also works well in F# with the F# extensions.

### New: Root Finding

- Brent's method. *~Candy Chiu, Alexander Täschner*
- Bisection method. *~Scott Stephens, Alexander Täschner*
- Broyden's method, for multi-dimensional functions. *~Alexander Täschner*
- Newton-Raphson method.
- Robust Newton-Raphson variant that tries to recover automatically in cases where it would fail or converge too slowly. This modification makes it more robust e.g. in the presence of singularities and less sensitive to the search range/interval.
- All algorithms support a TryFind-pattern which returns success instead of throwing an exception.
- Special case for quadratic functions, in the future to be extended e.g. to polynomials.
- Basic bracketing algorithm
- Also works well in F# with the F# extensions.

### Linear Algebra

- Native eigenvalue decomposition (EVD) support with our MKL packages *~Marcus Cuda*
- Add missing scalar-vector operations (s-v, s/v, s%v) *~Thomas Ibel*
- Support for new F# 3.1 row/column slicing syntax on matrices
- Matrices learned proper OfColumn/RowVectors, analog also in F#.
- Documentation Fixes *~Robin Neatherway*
- BUG: Fixed exception text message when creating a matrix from enumerables (rows vs columns) *~Thomas Ibel*
- We're phasing out MathNet.Numerics.IO that used to be included in the main package for matrix file I/O for text and Matlab formats. Use the new .Data.Text and .Data.Matlab packages instead.

### Statistics & Distributions

- Spearman Rank Correlation Coefficient *~Iain McDonald*
- Covariance function, in Array-, Streaming- and common Statistics.
- Categorical: distribution more consistent, no longer requires normalized pdf/cdf parameters
- Categorical: inverse CDF function *~Paul Varkey*
- BUG: Fixed static sampling methods of the `Stable` distribution. *~Artyom Baranovskiy*

### Misc

- BUG: Fixed a bug in the Gamma Regularized special function where in some cases with large values it returned 1 instead of 0 and vice versa.
- The F# extensions now have a strong name in (and only in) the signed package as well (previously had not been signed). *~Gauthier Segay*
- Evaluate.Polynomial with new overload which is easier to use.
- Fixed a couple badly designed unit tests that failed on Mono.
- Repository now Vagrant-ready for easy testing against recent Mono on Debian.

v2.5.0 - April 14, 2013
-----------------------

See also: [What's New in Math.NET Numerics 2.5](http://christoph.ruegg.name/blog/new-in-mathnet-numerics-2-5.html): Announcement, Explanations and Sample Code.

### *Potentially Breaking Changes:*

Despite semver this release contains two changes that may break code but without triggering a major version number change. The changes fix semantic bugs and a major usability issue without changing the formal API itself. Most users are not expected to be affected negatively. Nevertheless, this is an exceptional case and we try hard to avoid such changes in the future.

- Statistics: Empty statistics now return NaN instead of either 0 or throwing an exception. *This may break code in case you relied upon the previous unusual and inconsistent behavior.*

- Linear Algebra: More reasonable ToString behavior for matrices and vectors. *This may break code if you relied upon ToString to export your full data to text form intended to be parsed again later. Note that the classes in the MathNet.Numerics.IO library are more appropriate for storing and loading data.*

### Statistics:

- More consistent behavior for empty and single-element data sets: Min, Max, Mean, Variance, Standard Deviation etc. no longer throw exceptions if the data set is empty but instead return NaN. Variance and Standard Deviation will also return NaN if the set contains only a single entry. Population Variance and Population Standard Deviation will return 0 in this case.
- Reworked order statistics (Quantile, Quartile, Percentile, IQR, Fivenum, etc.), now much easier to use and supporting compatibility with all 9 R-types, Excel and Mathematica. The obsolete Percentile class now leverages the new order statistics, fixing a range check bug as side effect.
- New Hybrid Monte Carlo sampler for multivariate distributions. *~manyue*
- New financial statistics: absolute risk and return measures. *~Phil Cleveland*
- Explicit statistics for sorted arrays, unsorted arrays and sequences/streams. Faster algorithms on sorted data, also avoids multiple enumerations.
- Some statistics like Quantile or empirical inverse CDF can optionally return a parametric function when multiple evaluations are needed, like for plotting.

### Linear Algebra:

- More reasonable ToString behavior for matrices and vectors: `ToString` methods no longer render the whole structure to a string for large data, among others because they used to wreak havoc in debugging and interactive scenarios like F# FSI. Instead, ToString now only renders an excerpt of the data, together with a line about dimension, type and in case of sparse data a sparseness indicator. The intention is to give a good idea about the data in a visually useful way. How much data is shown can be adjusted in the Control class. See also ToTypeString and ToVector/MatrixString.
- Performance: reworked and tuned common parallelization. Some operations are up to 3 magnitudes faster in some extreme cases. Replaced copy loops with native routines. More algorithms are storage-aware (and should thus perform better especially on sparse data). *~Thomas Ibel, Iain McDonald, Marcus Cuda*
- Fixed range checks in the Thin-QR decomposition. *~Marcus Cuda*
- Fixed bug in Gram Schmidt for solving tall matrices. *~Marcus Cuda*
- Vectors now implement the BCL IList interfaces (fixed-length) for better integration with existing .Net code. *~Scott Stephens*
- Matrix/Vector parsing has been updated to be able to parse the new visual format as well (see ToMatrixString).
- DebuggerDisplay attributes for matrices and vectors.
- Map/IndexedMap combinators with storage-aware and partially parallelized implementations for both dense and sparse data.
- Reworked Matrix/Vector construction from arrays, enumerables, indexed enumerables, nested enumerables or by providing an init function/lambda. Non-obsolete constructors now always use the raw data array directly without copying, while static functions always return a matrix/vector independent of the provided data source.
- F#: Improved extensions for matrix and vector construction: create, zeroCreate, randomCreate, init, ofArray2, ofRows/ofRowsList, ofColumns/ofColumnsList, ofSeqi/Listi (indexed). Storage-aware for performance.
- F#: Updated map/mapi and other combinators to leverage core implementation, added -nz variants where zero-values may be skipped (relevant mostly for sparse matrices).
- F#: Idiomatic slice setters for sub-matrices and sub-vectors
- F#: More examples for matrix/vector creation and linear regression in the F# Sample-package.

### Misc:

- Control: Simpler usage with new static ConfigureAuto and ConfigureSingleThread methods. Resolved misleading configuration logic and naming around disabling parallelization.
- Control: New settings for linear algebra ToString behavior.
- Fixed range check in the Xor-shift pseudo-RNG.
- Parallelization: Reworked our common logic to avoid expensive lambda calls in inner loops. Tunable.
- F#: Examples (and thus the NuGet Sample package) are now F# scripts prepared for experimenting interactively in FSI, instead of normal F# files. Tries to get the assembly references right for most users, both within the Math.NET Numerics solution and the NuGet package.
- Various minor improvements on consistency, performance, tests, xml docs, obsolete attributes, redundant code, argument checks, resources, cleanup, nuget, etc.


v2.4.0 - February 3, 2013
-------------------------

- Drops the dependency on the zlib library. We thus no longer have any dependencies on other packages. *~Marcus Cuda, Thomas Ibel*
- Adds Modified Bessel & Struve special functions *~Wei Wu*
- Fixes a bug in our iterative kurtosis statistics formula *~Artyom Baranovskiy*

### Linear Algebra:

- Performance work, this time mostly around accessing matrix rows/columns as vectors. Opting out from targeted patching in our matrix and vector indexers to allow inlining.
- Fixes an issue around Thin-QR solve *~Marcus Cuda*
- Simplifications around using native linear algebra providers (see Math.NET Numerics With Native Linear Algebra)

### F#:

- Adds the BigRational module from the F# PowerPack, now to be maintained here instead. *~Gustavo Guerra*
- Better support for our Complex types (close to the F# PowerPack Complex type) *~Gustavo Guerra*


v2.3.0 - November 25, 2012
--------------------------

### Portable Library Build:

- Adds support for WP8 (.Net 4.0 and higher, SL5, WP8 and .NET for Windows Store apps)
- New: portable build also for F# extensions (.Net 4.5, SL5 and .NET for Windows Store apps)
- NuGet: portable builds are now included in the main packages, no more need for special portable packages

### Linear Algebra:

- Continued major storage rework, in this release focusing on vectors (previous release was on matrices)
- Thin QR decomposition (in addition to existing full QR)
- Static CreateRandom for all dense matrix and vector types
- F#: slicing support for matrices and vectors

### Random and Probability Distributions:

- Consistent static Sample methods for all continuous and discrete distributions (was previously missing on a few)
- F#: better usability for random numbers and distributions.

### Misc:

- F# extensions are now using F# 3.0
- Updated Intel MKL references for our native linear algebra providers
- Various bug, performance and usability fixes
