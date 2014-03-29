Math.NET Numerics Release Notes
===============================

- **Website: [numerics.mathdotnet.com](http://numerics.mathdotnet.com)**
- GitHub/Mainline: [github.com/mathnet/mathnet-numerics](https://github.com/mathnet/mathnet-numerics)
- CodePlex: [mathnetnumerics.codeplex.com](http://mathnetnumerics.codeplex.com)
- License: MIT/X11 Open Source

NuGet Packages, available in the [NuGet Gallery](https://nuget.org/profiles/mathnet/):

- `MathNet.Numerics` - core package, including .Net 4, .Net 3.5 and portable/PCL builds
- `MathNet.Numerics.FSharp` - optional extensions for a better F# experience
- `MathNet.Numerics.Data.Text` - NEW: optional extensions for text-based matrix input/output (CSV for now)
- `MathNet.Numerics.Data.Matlab` - NEW: optional extensions for MATLAB matrix file input/output
- `MathNet.Numerics.MKL.Win-x86` - optional Linear Algebra MKL native provider
- `MathNet.Numerics.MKL.Win-x64` - optional Linear Algebra MKL native provider
- `MathNet.Numerics.MKL.Linux-x86` - optional Linear Algebra MKL native provider
- `MathNet.Numerics.MKL.Linux-x64` - optional Linear Algebra MKL native provider
- `MathNet.Numerics.Signed` - strong-named version of the core package *(not recommended)*
- `MathNet.Numerics.FSharp.Signed` - strong-named version of the F# package *(not recommended)*
- `MathNet.Numerics.Sample` - code samples in C#
- `MathNet.Numerics.FSharp.Sample` - code samples in F#

Zip Packages, available on [CodePlex](http://mathnetnumerics.codeplex.com/releases):

- Binaries - core package and F# extensions, including both .Net 4 and portable builds
- Signed Binaries - strong-named version of the core package *(not recommended)*

Supported Platforms:

- .Net 4.0, .Net 3.5 and Mono: Windows, Linux and Mac.
- PCL Portable Profiles 47 and 136: Silverlight 5, Windows Phone 8, .NET for Windows Store apps (Metro).
- PCL/Xamarin: Andoid, iOS

Over time some members and classes have been replaced with more suitable alternatives. In order to maintain compatibility, such parts are not removed immediately but instead marked with the **Obsolete**-attribute. We strongly recommend to follow the instructions in the attribute text whenever you find any code calling an obsolete member, since we *do* intend to remove them at the next *major* release.

v3.0.0 - To Be Announced
------------------------

See also: [Roadmap](http://sdrv.ms/17wPFlW) and [Towards Math.NET Numerics Version 3](http://christoph.ruegg.name/blog/towards-mathnet-numerics-v3.html).

Multiple alpha builds have been made available as NuGet pre-release packages. There are likely more to come as we still have a lot to do; and at least a beta before the final release. All information provided here regarding v3 is preliminary and incomplete.

- All obsolete code has been removed.
- Reworked redundancies, inconsistencies and unfortunate past design choices.
- Significant namespace simplifications (-30%).

Changes as of now:

### Linear Algebra

- Favor and optimize for generic types, e.g. `Vector<double>`.
- Drop the `.Generic` in the namespaces and flattened solver namespaces.
- F#: all functions in the modules now fully generic, including the `matrix` function.
- F#: `SkipZeros` instead of the cryptic `nz` suffix for clarity.
- Add missing scalar-matrix routines.
- Optimized mixed dense-diagonal and diagonal-dense operations (500x faster on 250k set).
- More reasonable choice of return structure on mixed operations (e.g. dense+diagonal).
- Add point-wise infix operators `.*`, `./`, `.%` where supported (F#)
- Vectors explicitly provide proper L1, L2 and L-infinity norms.
- All norms return the result as double (instead of the specific value type of the matrix/vector).
- Matrix L-infinity norm now cache-optimized (8-10x faster).
- Vectors have a `ConjugateDotProduct` in addition to `DotProduct`.
- `Matrix.ConjugateTransposeAndMultiply` and variants.
- Matrix Factorization types fully generic, easily accessed by new `Matrix<T>` member methods (replacing the extension methods). Discrete implementations no longer visible.
- QR factorization is thin by default.
- Matrix factorizations no longer clone their results at point of access.
- Add direct factorization-based `Solve` methods to matrix type.
- Massive iterative solver implementation/design simplification, now mostly generic and a bit more functional-style.
- New MILU(0) iterative solver preconditioner that is much more efficient and fully leverages sparse data. *~Christian Woltering*
- Matrices/Vectors now have more consistent enumerators, with a variant that skips zeros (useful if sparse).
- Matrix/Vector creation routines have been simplified and usually no longer require explicit dimensions. New variants to create diagonal matrices, or such where all fields have the same value. All functions that take a params array now have an overload accepting an enumerable (e.g. `OfColumnVectors`).
- Generic Matrix/Vector creation using builders, e.g. `Matrix<double>.Build.DenseOfEnumerable(...)`
- Create a matrix from a 2D-array of matrices (top-left aligned within the grid).
- Create a matrix or vector with the same structural type as an example (`.Build.SameAs(...)`)
- Removed non-static Matrix/Vector.CreateMatrix/CreateVector routines (no longer needed)
- Add Vector.OfArray (copying the array, consistent with Matrix.OfArray - you can still use the dense vector constructor if you want to use the array directly without copying).
- More convenient and one more powerful overload of `Matrix.SetSubMatrix`.
- Matrices/Vectors expose whether storage is dense with a new IsDense property.
- Various minor performance work.
- Matrix.ClearSubMatrix no longer throws on 0 or negative col/row count (nop)
- BUG: Fix bug in routine to copy a vector into a sub-row of a matrix.
- Both canonical modulus and remainder operations on matrices and vectors.
- Matrix kernel (null space) and range (column space)

### Linear Algebra MKL Native Provider

- Thin QR factorization uses MKL if enabled for all types (previously just `double`)
- Sparse matrix CSR storage format now uses the much more common row pointer convention and is fully compatible with MKL (so there is nothing in the way to add native provider support).
- Providers have been moved to a `Providers` namespace and are fully generic again.
- Simpler provider usage: `Control.UseNativeMKL()`, `Control.UseManaged()`.
- MKL native provider now supports capability querying (so we can extend it much more reliably without breaking your code).
- MKL native provider consistency, precision and accuracy now configurable (trade-off).
- Native Provider development has been reintegrated into the main repository; we can now directly run all unit tests against local native provider builds. Covered by FAKE builds.

### Statistics

- Pearson and Spearman correlation matrix of a set of arrays.
- Spearman ranked correlation optimized (4x faster on 100k set)
- Single-pass `MeanVariance` method (as used often together).
- Some overloads for single-precision values.
- Add `Ranks`, `QuantileRank` and `EmpiricalCDF`.
- F# module for higher order functions.

### Probability Distributions

- Direct static distributions functions (PDF, CDF, sometimes also InvCDF).
- Direct static sample functions.
- New Trigangular distributionb *~Superbest*
- Add InvCDF to Gamma, Student-T, FisherSnedecor (F), and Beta distributions.
- Major API cleanup, including xml docs
- Xml doc and ToString now use well-known symbols for the parameters.
- Maximum-likelihood parameter estimation for a couple distributions.
- All constructors now optionally accept a random source as last argument.
- Use less problematic RNG-seeds by default, if no random source is provided.
- Simpler and more composable random sampling from distributions.
- Much more distribution's actual sample distribution is verified in tests (all continuous, most discrete).
- Binomial.CDF now properly leverages BetaRegularized.
- BUG: Fix hyper-geometric CDF semantics, clarify distribution parameters.
- BUG: Fix Zipf CDF at x=1.
- BUG: Fix Geometric distribution sampling.

### Random Number Generators

- All RNGs provide static Sample(values) functions to fill an existing array.
- Thread-safe System.Random available again as `SystemRandomSource`.
- Fast and simple to use static `SystemRandomSource.Doubles` routine with lower randomness guarantees.
- Shared `SystemRandomSource.Default` and `MersenneTwister.Default` instances to skip expensive initialization.
- Using thread-safe random source by default in distributions, Generate, linear algebra etc.
- Tests always use seeded RNGs for reproducability.
- F#: direct sampling routines in the `Random` module, also including default and shared instances.

### Linear Regression

- Reworked `Fit` class, supporting more simple scenarios.
- New `.LinearRegression` namespace with more options.
- Better support for simple regression in multiple dimensions.
- Goodness of Fit: R, RSquared *~Ethar Alali*
- Weighted polynomial and multi-dim fitting.
- Use more efficient LA routines *~Thomas Ibel*

### Interpolation

- Return tuples instead of out parameter.
- Reworked splines, drop complicated and limiting inheritance design. More functional approach.
- More efficient implementation for non-cubic splines (especially linear spline).
- `Differentiate2` instead of `DifferentiateAll`.
- Definite `Integrate(a,b)` in addition to existing indefinite `Integrate(t)`.
- Use more common names in `Interpolate` facade, e.g. "Spline" is a well known name.

### Root Finding

- Chebychev polynomial roots.
- Cubic polynomials roots. *~Candy Chiu*

### Functions

- Trig functions: common short names instead of very long names. Add sinc function.
- Excel functions: TDIST, TINV, BETADIST, BETAINV, GAMMADIST, GAMMAINV, NORMDIST, NORMINV, NORMSDIST, NORMSINV QUARTILE, PERCENTILE, PERCENTRANK.
- Special functions: BetaRegularized more robust for large arguments.
- Special functions: new `GammaLowerRegularizedInv`.
- New distance functions in `Distance`: euclidean, manhattan, chebychev distance of arrays or generic vectors. SAD, MAE, SSD, MSE metrics. Pearson's, Canberra and Minkowski distance. Hamming distance.
- Windows: ported windowing functions from Neodym (Hamming, Hann, Cosine, Lanczos, Gauss, Blackmann, Bartlett, ...)
- BigInteger factorial

### Build & Packages

- FAKE-based build (in addition to existing Visual Studio solutions) to clean, build, test, document and package independently of the CI server.
- Finally proper documentation using FSharp.Formatting with sources included in the repository so it is versioned and can be contributed to with pull requests.
- NuGet packages now also include the PCL portable profile 47 (.Net 4.5, Silverlight 5, Windows 8) in addition to the normal .Net 4.0 build and PCL profile 136 (.Net 4.0, WindowsPhone 8, Silverlight 5, Windows 8) as before. Profile 47 uses `System.Numerics` for complex numbers, among others, which is not available in profile 136.
- NuGet packages now also include a .Net 3.5 build of the core library.
- IO libraries have been removed, replaced with new `.Data` packages (see list on top).
- Alternative strong-named versions of more NuGet packages (mostly the F# extensions for now), with the `.Signed` suffix.
- Reworked solution structure so it works in both Visual Studio 11 (2012) and 12 (2013).
- We can now run the full unit test suite against the portable builds as well.
- Builds should now also work properly on recent Mono on Linux (including F# projects).
- Fixed builds on platforms with case sensitive file systems. *~Gauthier Segay*

### Misc

- Integration: simplification of the double-exponential transformation api design.
- FFT: converted to static class design and shorter names for simpler usage. Drop now redundant `Transform` class.
- Generate: ported synthetic data generation and sampling routines from Neodym (includes all from old Signals namespace). F# module for higher order functions.
- Euclid: modulus vs remainder (also BigInteger), integer theory (includes all from old NumberTheory namespace).
- Complex: common short names for Exp, Ln, Log10, Log.
- Complex: fix issue where a *negative zero* may flip the sign in special cases (like `Atanh(2)`, where incidentally MATLAB and Mathematica do not agree on the sign either).
- Complex: routines to return all two square and three cubic roots of a complex number.
- Complex: More robust complex Asin/Acos for large real numbers.
- Evaluate: routine to evaluate complex polynomials, or real polynomials at a complex point.
- CommonParallel now also supported in .Net 3.5 and portable profiles; TaskScheduler can be replaced with custom implementation *~Thomas Ibel*
- F# BigRational type cleaned up and optimized *~Jack Pappas*
- F# BigRational IsZero, IsOne, IsInteger, Reciprocal, Power operator support (**), create from fraction.
- F# functions now use the clearer `Func` suffix instead of just `F` if they return a function.
- Precision: reworked, now much more consistent. **If you use `AlmostEqual` with numbers-between/ULP semantics, please do review your code to make sure you're still using the expected variant!**. If you use the decimal-places semantics, you may need to decrement the digits argument to get the same behavior as before.
- Much less null checks, our code generally only throws `ArgumentNullException` if an unexpected null argument would *not* have caused an immediate `NullReferenceException`.
- Cases where `ArgumentOutOfRangeExceptions` where thrown with wrong arguments (i.e. no parameter name) now throw `ArgumentException` instead.
- Tests now have category attributes (to selectively run or skip categories).

v2.6.2 - October 21, 2013
-------------------------

Patch release, fixing the NuGet package to work better in WindowsPhone 8 projects. Assemblies are not changed.

v2.6.1 - August 13, 2013
------------------------

Patch release, fixing a bug in `ArrayStatistics.Variance` on arrays longer than 46341 entries.

v2.6.0 - July 26, 2013
----------------------

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
