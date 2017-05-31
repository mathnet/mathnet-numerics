### 3.20.0-beta01 - 2017-05-31
* Optimization: non-linear optimization algorithms *~Scott Stephens, Erik Ovegard, bdodson, et al.*
* BUG: Special Functions: allow more iterations in BetaRegularized *~Elias Abou Jaoude*

### 3.19.0 - 2017-04-29
* Statistics: RunningStatistics.Combine to better handle empty statistics *~Lucas Godshalk*
* Linear Algebra: Cholesky.Factorize to reuse the factorization matrix *~mjmckp*
* Linear Algebra: Fix docs for DoPointwiseMultiply *~Jakub Arnold*

### 3.18.0 - 2017-04-09
* FFT: single-precision support *~AlexHild*

### 3.17.0 - 2017-01-15
* Random: random sources (all except crypto) now support ephemeral serialization.
* Linear Algebra: explicit impl to copy a range of a row of a sparse matrix to a range of a sparse vector *~arthurvb*
* Linear Algebra: explicitly demand fully modifiable matrix where needed, fixes issues with diagonal matrices.
* FFT: leverage new matrix internal array access approach in 2D matrix transformations.

### 3.16.0 - 2017-01-03
* Root Finding: improve accuracy handling *~Konstantin Tretyakov*
* Regression: GoodnessOfFit StandardError *~David Falkner*

### 3.15.0 - 2016-12-27
* FFT: MKL native provider backend.
* FFT: 2D and multi-dimensional FFT (only supported by MKL provider, managed provider pending).
* FFT: real conjugate-even FFT (only leveraging symmetry in MKL provider).
* FFT: managed provider significantly faster on x64.
* Linear Algebra: pointwise trigonometric and basic functions *~Albert Pang*
* Linear Algebra: better support for F# built-in operators (sqrt, sin, exp, ..) *~Albert Pang*
* Linear Algebra: pointwise power operator (F#)
* Linear Algebra: enable experimental matrix product implementation
* Linear Algebra: better support for matrix to/from row-major arrays and enumerables
* Linear Algebra: transport allows specifying a result matrix to transpose into, inplace if square
* Linear Algebra: vector and matrix AsArray and similar to access internal arrays if applicable
* Linear Algebra: vector and matrix pointwise min/max and absmin/absmax
* Linear Algebra: dot-power on vectors and matrices, supporting native providers.
* Linear Algebra: matrix Moore-Penrose pseudo-inverse (SVD backed).
* Provider Control: separate Control classes for LA and FFT Providers.
* Provider Control: avoid internal exceptions on provider discovery.
* Distributions: fix misleading inline docs on Negative-Binomial.
* Generate: linear integer ranges
* Root Finding: extend zero-crossing bracketing in derivative-free algorithms.
* Window: periodic versions of Hamming, Hann, Cosine and Lanczos windows.
* Special Functions: more robust GammaLowerRegularizedInv (and Gamma.InvCDF).
* BUG: ODE Solver: fix bug in Runge-Kutta second order routine *~Ksero*

### 3.13.1 - 2016-09-06
* BUG: Random: Next(x,x+1) must always return x *~Juri*

### 3.13.0 - 2016-08-18
* Linear Algebra: faster tall, wide managed matrix multiplication. *~Aixile*
* Euclid: Integer Log2 (DeBruijn sequencences algorithm).
* Integration: Gauss-Legendre documentation, cleanup. *~Larz White*
* Random: Integer sub-range sampling to use rejection sampling to avoid bias.
* Random: Improvements on integer and byte sampling.
* BUG: Random: CryptoRandomSource must not generate 1.0 samples.
* BUG: Statistics: fixed bug in WeightedPearson Correlation. *~Jon Smit*

### 3.12.0 - 2016-07-03
* ODE Solver: Runge-Kutta (order 2, 4) and Adams-Bashforth (order 1-4) algorithms *~Yoonku Hwang*
* Linear Algebra: faster multiplication of sparse with dense matrices *~Arthur*
* BUG: Integration: Gauss-Legendre on order 256 *~Sergey Kosukhin*
* BUG: Distributions: ChiSquared sampling was taking a square root where it should not *~Florian Wechsung*

### 3.11.1 - 2016-04-24
* BUG: Linear Algebra: sparse vector pointwise multiply/divide to itself
* BUG: Linear Algebra: Vector.ToVectorString if the first column is wider than maxWidth

### 3.11.0 - 2016-02-13
* Special Functions: error functions to use static coefficient arrays (perf) *~Joel Sleppy*
* Integration: Gauss-Legendre Rule (1D, 2D) *~Larz White*
* Complex: more robust magnitude and division for numbers close to MaxValue or Epsilon *~MaLiN2223*
* Native Providers: lazy default provider discovery & initialization *~Kuan Bartel*
* FSharp Package: Quaternion type *~Phil Cleveland*

### 3.10.0 - 2015-12-30
* Statistics: single-precision floating point support.
* Statistics: very limited support for int32 and complex numbers.
* Statistics: Min/Max Absolute, MagnitudePhase (complex).
* Statistics: FiveNumberSummary to use actual Median instead of R8 quantile.
* Linear Algebra: matrix Rank to use relative epsilon.
* Linear Algebra: extensions to convert between single/double precision, complex/real.
* Linear Algebra: Vector/Matrix storage DataContracts for ephemeral serialization.
* Regression: more helpful exceptions and messages.
* Random: 'Next' integer sampling no longer involves floating points, avoids one-off error in MersenneTwister.
* Precision: EpsilonOf for single-precision numbers, drop no longer needed portable fallbacks.

### 3.9.0 - 2015-11-25
* Distributions: Normal.CDF avoids problematic subtraction by using Erfc instead of Erf.
* Statistics: geometric and harmonic mean.
* Statistics: DataContracts for ephemeral serialization on RunningStatistics, DescriptiveStatistics and Histogram.
* BUG: Statistics: Histogram did not adjust lower bound correctly when value was equal to the bound *~Volker Breuer*
* Linear Algebra: minor optimization on how we call Array.Copy.
* BUG: Linear Algebra: fix bug in Complex and Complex32 SparseMatrix.ToTypeString.

### 3.8.0 - 2015-09-26
* Distributions: PDF and CDF more robust for large distribution parameters.
* Distributions: BetaScaled distribution.
* Distributions: method to create a PERT distribution (based on BetaScaled) *~John C Barstow*
* Distributions: Weibull.Estimate *~Jon Larborn*
* Random: NextBoolean extensions.
* Root Finding: RootFinding.Secant (based on NewtonRaphson) *~grovesNL*
* Linear Algebra: Matrix Rank calculation now uses a tolerance based on the matrix size.
* Linear Algebra: Alternative CreateMatrix/Vector functions with type parameter on functions instead of type.
* Linear Algebra: MKL LinearAlgebra provider requires at least native provider r9 (linear algebra v2.0).
* Native Providers: automatic handling of intermediate work arrays/buffers in MKL and OpenBLAS providers *~Marcus Cuda, Kuan Bartel*
* Native Providers: automatically use native provider if available.
* Native Providers: new Control.TryUse* to make it simpler to use providers if available but without failing if not.
* Native Providers: improved error state checking and handling *~Marcus Cuda, Kuan Bartel*
* Combinatorics: generate or select random permutation, combination or variation (shuffling)
* Finance: rename CompoundMonthlyReturn to CompoundReturn (old now obsolete).

### 3.7.1 - 2015-09-10
* BUG: Linear Algebra: fix optimized path of adding a sparse matrix to itself.

### 3.7.0 - 2015-05-09
* Statistics: RunningStatistics now propagates min/max on Combine, handles NaN on Push.
* Statistics: new MovingStatistics providing descriptive statistics over a moving window *~Marcus Cuda*
* Statistics: new Statistics.MovingAverage.
* Statistics: Improved Histogram handling of small-width buckets *~Justin Needham*
* Distributions: ChiSquare.InvCDF *~logophobia*
* FFT: Fourier.FrequencyScale to generate the frequency corresponding to each index in frequency space.
* BUG: FFT: fix Bluestein algorithm for sequences with more than 46341 samples but not power-of-two.
* Linear Algebra: SparseVector.AbsoluteMaximumIndex *~Matt Heffron*
* MKL Native Provider: OSX build script *~Marcus Cuda*
* MKL Native Provider: new combined NuGet package with a proper build target (no more manual file handling needed).
* OpenBLAS Native Provider: a new linear algebra provider using OpenBLAS *~Kuan Bartel*
* CUDA Native Provider: a new GPU-based linear algebra provider using Nvidia CUDA *~Matthew A. Johnson*
* Native Providers: now versioned separately for each kind (MKL, CUDA, OpenBLAS).

### 3.6.0 - 2015-03-22
* Distributions: ChiSquare.CDF more robust for large numbers *~Baltazar Bieniek*
* Linear Algebra: MatrixStorage.Map2 equivalent to VectorStorage.Map2
* Linear Algebra: Matrix and Vector Find/Find2, Exists/Exists2, ForAll/ForAll2
* Linear Algebra: more consistent range checking in MatrixStorage.Clear and related
* Linear Algebra: mixed-storage fall back implementations now leverage higher-order functions
* BUG: Linear Algebra: fix loop range in MatrixStorage.ClearColumns (built-in storage not affected)
* BUG: Linear Algebra: fix sparse matrix equality.
* BUG: Linear Algebra: ArgumentException instead of index exception when trying to create an empty matrix.
* Generate: Unfold, Fibonacci; Normal and Standard replacing Gaussian and Stable.
* Native Providers: NativeProviderLoader to automatically load the provider for the matching processor architecture (x86, x64) *~Kuan Bartel*
* Native Providers: Control.NativeProviderPath allowing to explicitly declare where to load binaries from.
* MKL Native Provider: support for native complex eigen-value decomposition *~Marcus Cuda*
* MKL Native Provider: non-convergence checks in singular-value and eigen-value decompositions *~Marcus Cuda*

### 3.5.0 - 2015-01-10
* Differentiation: derivative, partial and mixed partial; hessian & jacobian *~Hythem Sidky*
* Differentiation: Differentiate facade class for simple use cases
* Differentiation: F# module for better F# function support.
* Linear Algebra: matrix ToRowArrays/ToColumnArrays
* Linear Algebra: F# insertRow, appendRow, prependRow and same also for columns
* Linear Algebra: F# append, stack and ofMatrixList2
* Precision: measured machine epsilon, positive vs negative epsilon

### 3.4.0 - 2015-01-04
* Special Functions: Generalized Exponential Integral *~Ashley Messer*
* Special Functions: Regularized Incomplete Gamma domain extended to a=0 *~Ashley Messer*
* Statistics: weighted Pearson correlation *~ViK*
* MKL Native Provider: memory functions to free buffers and gather usage statistics *~Marcus Cuda*
* F#: depend on new official FSharp.Core NuGet package instead of FSharp.Core.Microsoft.Signed
* F#: simpler NuGet package dependencies (no more need for framework groups)
* Build: vagrant bootstrap now uses the latest xamarin mono packages

### 3.3.0 - 2014-11-26
* Linear Algebra: Vector.Fold2 (fold2 in F#), storage optimized
* Linear Algebra: Minor change how matrix products call the LA provider
* Linear Algebra: Random generation now leveraging array sampling routines
* BUG: Linear Algebra: fix bug when manually assigning System.Random to random distribution
* Root Finding: Change Brent tolerance check, add bracket check *~Hythen Sidky*
* Root Finding: Auto zero-crossing bracketing in FindRoots facade (not in algorithms)
* Statistics: RootMeanSquare (RMS)
* Distributions: Array sampling routines now available through interface
* Distributions: Categorical sampling now explicitly skips p=0 categories
* Generate: leverage array sampling routines for random data generation
* Generate: square, triangle and sawtooth waves
* Distance: Jaccard Index
* F#: explicitly depend on official FSharp.Core NuGet packages
* F#: NuGet package with iPython IfSharp F# module integration load script
* F#: load scripts with better packet support (and NuGet with -ExcludeVersion)
* Build: unified build.sh and buildn.sh into combined build.sh
* Build: use Paket instead of NuGet to maintain NuGet dependencies
* Build: for core add PCL profiles 7, 78 and 259; for F# extensions drop PCL profile 328

### 3.2.3 - 2014-09-06
* BUG: MatrixNormal distribution: fix density for non-square matrices *~Evelina Gabasova*

### 3.2.2 - 2014-09-05
* BUG: MatrixNormal distribution: density computation switched row and column covariance *~Evelina Gabasova*

### 3.2.1 - 2014-08-05
* Package fix: make sure .Net 3.5-only dependencies are not installed on .Net 4 and newer.

### 3.2.0 - 2014-08-05
* Linear Algebra: Vector.Map2 (map2 in F#), storage-optimized
* Linear Algebra: fix RemoveColumn/Row early index bound check (was not strict enough)
* Statistics: Entropy *~Jeff Mastry*
* Interpolation: use Array.BinarySearch instead of local implementation *~Candy Chiu*
* Resources: fix a corrupted exception message string
* Portable Build: support .Net 4.0 as well by using profile 328 instead of 344.
* .Net 3.5: F# extensions now support .Net 3.5 as well
* .Net 3.5: NuGet package now contains proper 3.5-only TPL package dependency; also in Zip package

### 3.1.0 - 2014-07-20
* Random: generate a sequence of integers within a range in one go
* Distributions: all distributions must have static routines to sample an array in one go
* Linear Algebra: fix Matrix.StrictlyLowerTriangle
* Linear Algebra: fix vector DoOuterProduct *~mjmckp*
* Linear Algebra: enumerators accept Zeros-parameter (like map/fold already does)
* Linear Algebra: Vector.MapConvert (consistency)
* Linear Algebra: proper term for 'conjugate symmetric' is 'Hermitian'
* Interpolation: new Step, LogLinear and transformed interpolators *~Candy Chiu*
* Interpolation: check for min required number of data points, throw ArgumentException if not.
* Root Finding: F# FindRoots.broyden module function *~teramonagi*
* Misc docs fixes

### 3.0.2 - 2014-06-26
* BUG: fixing a bug in Matrix.RemoveRow range checks.

### 3.0.1 - 2014-06-24
* BUG: fixing a bug in new Matrix.ToMatrixString and Vector.ToVectorString routines.

### 3.0.0 - 2014-06-21
* First stable v3 release:
   * [Upgrade Notes](https://github.com/mathnet/mathnet-numerics/wiki/Upgrading-to-Version-3)
   * Stable API, no more breaking changes for all future v3 releases (except previews).
   * Finally unlocks development and contributions around non-linear optimization and native providers over the next few minor releases.
* Native Providers: option to control max number of threads used by MKL.
* F#: Fit.multiDim; Matrix.qr, svd, eigen, lu and cholesky.

### 3.0.0-beta05 - 2014-06-20
* 2nd Candidate for v3.0 Release
* BUG: Distance: fix bug in Hamming distance that skipped the first pair.
* F#: packages now include a MathNet.Numerics.fsx script that includes FSI printers and references the assemblies.
* Linear Algebra: improved matrix and vector ToString formatting, more compact, adaptive to actual numbers.
* Linear Algebra: CoerceZero for matrix and vector to replace small numbers with zero.
* Regression: DirectRegressionMethod option to specify as argument which direct method should be used.
* Control: drop MaxToStringRows/Columns properties (no longer used)
* Random: clarify bad randomness properties of SystemRandomSource.FastDoubles (trade off)

### 3.0.0-beta04 - 2014-06-16
* Candidate for v3.0 Release
* Linear Algebra:
   * FoldRows renamed to FoldByRow, now operates on and returns arrays; same for columns. **Breaking.**
   * New FoldRows and ReduceRows that operate on row vectors; same for columns
   * Split Map into Map and MapConvert (allows optimization in common in-place case)
   * Row and column sums and absolute-sums
   * F# DiagonalMatrix module to create diagonal matrices without using the builder
   * F# Matrix module extended with sumRows, sumAbsRows, normRows; same for columns
* Build: extend build and release automation, automatic releases also for data extensions and native providers

### 3.0.0-beta03 - 2014-06-05
* Linear Algebra: vector outer product now follows common style, supports explicit result argument, more efficient.
* Interpolation: must not modify/sort original data; alternative Sorted and Inplace functions.
* Distributions: static IsValidParameterSet functions.
* Distributions: all distributions are now immutable in their distribution parameters. **Breaking.**
* NuGet: attempt to create proper symbol+source packages on symbolsource; primary packages smaller, w/o pdbs
* Build: skip long tests with new "quick" argument (FAKE)
* Build: clearing is more explicit, fixes most locking issues if solution is also open in IDE.
* Build: automated publishing docs, api, git release tag (maintainer)

### 3.0.0-beta02 - 2014-05-29
* Linear Algebra:
   * optimized sparse-sparse and sparse-diagonal matrix products. *~Christian Woltering*
   * transpose at storage level, optimized sparse transpose. *~Christian Woltering*
   * optimized inplace-map, indexed submatrix-map.
   * optimized clearing a set of rows or columns.
   * matrix FoldRows/FoldColumns.
   * matrix column/row norms, normalization.
   * prefer enums over boolean parameters (e.g. `Zeros.AllowSkip`).
   * IsSymmetric is now a method, add IsConjugateSymmetric. **Breaking.**
   * Eigenvalue decomposition can optionally skip symmetry test.
   * Direct diagonal-scalar division implementation
* Test Functions: Rosenbrock, Rastrigin, DropWave, Ackley, Bohachevsky, Matyas, SixHumpCamel, Himmelblau
* Statistics: DescriptiveStatistics support for larger datasets.
* MKL: native providers must not require MFC to compile.
* Sorting helpers support sub-range sorting, use insertion sort on very small sets.
* Build: extend usage of FAKE, automate docs, api, Zip and NuGet package generation.
* Portable: replace PCL profile 136 with profile 344, support for WP8.1
* Xamarin: prepare for better Xamarin Android/iOS support and for adding to the Xamarin store (free).
* Misc code style fixes.
* Update Vagrant setup to official Ubuntu 14.04 LTS box and proper apt-style Mono+F# provisioning.

### 3.0.0-beta01 - 2014-04-01
* See also: [Roadmap](https://sdrv.ms/17wPFlW) and [Towards Math.NET Numerics Version 3](http://christoph.ruegg.name/blog/towards-mathnet-numerics-v3.html).
* **Major release with breaking changes**
* All obsolete code has been removed
* Reworked redundancies, inconsistencies and unfortunate past design choices.
* Significant namespace simplifications (-30%).
* Linear Algebra:
   * Favor and optimize for generic types, e.g. `Vector<double>`.
   * Drop the `.Generic` in the namespaces and flattened solver namespaces.
   * F#: all functions in the modules now fully generic, including the `matrix` function.
   * F#: `SkipZeros` instead of the cryptic `nz` suffix for clarity.
   * Add missing scalar-matrix routines.
   * Optimized mixed dense-diagonal and diagonal-dense operations (500x faster on 250k set).
   * More reasonable choice of return structure on mixed operations (e.g. dense+diagonal).
   * Add point-wise infix operators `.*`, `./`, `.%` where supported (F#)
   * Vectors explicitly provide proper L1, L2 and L-infinity norms.
   * All norms return the result as double (instead of the specific value type of the matrix/vector).
   * Matrix L-infinity norm now cache-optimized (8-10x faster).
   * Vectors have a `ConjugateDotProduct` in addition to `DotProduct`.
   * `Matrix.ConjugateTransposeAndMultiply` and variants.
   * Matrix Factorization types fully generic, easily accessed by new `Matrix<T>` member methods (replacing the extension methods). Discrete implementations no longer visible.
   * QR factorization is thin by default.
   * Matrix factorizations no longer clone their results at point of access.
   * Add direct factorization-based `Solve` methods to matrix type.
   * Massive iterative solver implementation/design simplification, now mostly generic and a bit more functional-style.
   * Renamed iterative solver stop criteria from 'criterium' to 'criterion'.
   * New MILU(0) iterative solver preconditioner that is much more efficient and fully leverages sparse data. *~Christian Woltering*
   * Matrices/Vectors now have more consistent enumerators, with a variant that skips zeros (useful if sparse).
   * Matrix/Vector creation routines have been simplified and usually no longer require explicit dimensions. New variants to create diagonal matrices, or such where all fields have the same value. All functions that take a params array now have an overload accepting an enumerable (e.g. `OfColumnVectors`).
   * Generic Matrix/Vector creation using builders, e.g. `Matrix<double>.Build.DenseOfEnumerable(...)`
   * Create a matrix from a 2D-array of matrices (top-left aligned within the grid).
   * Create a matrix or vector with the same structural type as an example (`.Build.SameAs(...)`)
   * Removed non-static Matrix/Vector.CreateMatrix/CreateVector routines (no longer needed)
   * Add Vector.OfArray (copying the array, consistent with Matrix.OfArray - you can still use the dense vector constructor if you want to use the array directly without copying).
   * More convenient and one more powerful overload of `Matrix.SetSubMatrix`.
   * Matrices/Vectors expose whether storage is dense with a new IsDense property.
   * Various minor performance work.
   * Matrix.ClearSubMatrix no longer throws on 0 or negative col/row count (nop)
   * BUG: Fix bug in routine to copy a vector into a sub-row of a matrix.
   * Both canonical modulus and remainder operations on matrices and vectors.
   * Matrix kernel (null space) and range (column space)
   * Storage-aware non-inplace functional map on vectors and matrices
   * Pointwise power, exponential and natural logarithm for vectors and matrices.
   * Matrix positive-integer power
   * Matrix RemoveRow/RemoveColumn; more efficient InsertRow/InsertColumn
* Native Linear Algebra/Intel MKL:
   * Thin QR factorization uses MKL if enabled for all types (previously just `double`)
   * Sparse matrix CSR storage format now uses the much more common row pointer convention and is fully compatible with MKL (so there is nothing in the way to add native provider support).
   * Providers have been moved to a `Providers` namespace and are fully generic again.
   * Simpler provider usage: `Control.UseNativeMKL()`, `Control.UseManaged()`.
   * MKL native provider now supports capability querying (so we can extend it much more reliably without breaking your code).
   * MKL native provider consistency, precision and accuracy now configurable (trade-off).
   * Native Provider development has been reintegrated into the main repository; we can now directly run all unit tests against local native provider builds. Covered by FAKE builds.
* Statistics:
   * Pearson and Spearman correlation matrix of a set of arrays.
   * Spearman ranked correlation optimized (4x faster on 100k set)
   * Skewness and PopulationSkewness; Kurtosis and PopulationKurtosis.
   * Single-pass `MeanVariance` and `MeanStandardDeviation` methods (often used together).
   * Some overloads for single-precision values.
   * Add `Ranks`, `QuantileRank` and `EmpiricalCDF`.
   * F# module for higher order functions.
   * Median direct implementation (instead of R8-compatible 0.5-quantile)
   * New RunningStatistics that can be updated and merged
   * BUG: DescriptiveStatistics must return NaN if not enough data for a specific statistic.
* Probability Distributions:
   * Direct static distributions functions (PDF, CDF, sometimes also InvCDF).
   * Direct static sample functions, including such to fill an existing array in one call.
   * New Trigangular distribution *~Superbest, David Prince*
   * Add InvCDF to Gamma, Student-T, FisherSnedecor (F), and Beta distributions.
   * Major API cleanup, including xml docs
   * Xml doc and ToString now use well-known symbols for the parameters.
   * Maximum-likelihood parameter estimation for a couple distributions.
   * All constructors now optionally accept a random source as last argument.
   * Use less problematic RNG-seeds by default, if no random source is provided.
   * Simpler and more composable random sampling from distributions.
   * Much more distribution's actual sample distribution is verified in tests (all continuous, most discrete).
   * Binomial.CDF now properly leverages BetaRegularized.
   * BUG: Fix hyper-geometric CDF semantics, clarify distribution parameters.
   * BUG: Fix Zipf CDF at x=1.
   * BUG: Fix Geometric distribution sampling.
   * BUG: Fix Categorical distribution properties. *~David Prince*
* Random Numbers:
   * All RNGs provide static Sample(values) functions to fill an existing array.
   * Thread-safe System.Random available again as `SystemRandomSource`.
   * Fast and simple to use static `SystemRandomSource.Doubles` routine with lower randomness guarantees.
   * Shared `SystemRandomSource.Default` and `MersenneTwister.Default` instances to skip expensive initialization.
   * Using thread-safe random source by default in distributions, Generate, linear algebra etc.
   * Tests always use seeded RNGs for reproducability.
   * F#: direct sampling routines in the `Random` module, also including default and shared instances.
* Linear Regression:
   * Reworked `Fit` class, supporting more simple scenarios.
   * New `.LinearRegression` namespace with more options.
   * Better support for simple regression in multiple dimensions.
   * Goodness of Fit: R, RSquared *~Ethar Alali*
   * Weighted polynomial and multi-dim fitting.
   * Use more efficient LA routines *~Thomas Ibel*
* Interpolation:
   * Return tuples instead of out parameter.
   * Reworked splines, drop complicated and limiting inheritance design. More functional approach.
   * More efficient implementation for non-cubic splines (especially linear spline).
   * `Differentiate2` instead of `DifferentiateAll`.
   * Definite `Integrate(a,b)` in addition to existing indefinite `Integrate(t)`.
   * Use more common names in `Interpolate` facade, e.g. "Spline" is a well known name.
* Root Finding: Chebychev polynomial roots.
* Root Finding: Cubic polynomials roots. *~Candy Chiu*
* Trig functions: common short names instead of very long names. Add sinc function.
* Excel functions: TDIST, TINV, BETADIST, BETAINV, GAMMADIST, GAMMAINV, NORMDIST, NORMINV, NORMSDIST, NORMSINV QUARTILE, PERCENTILE, PERCENTRANK.
* Special functions: BetaRegularized more robust for large arguments.
* Special functions: new `GammaLowerRegularizedInv`.
* New distance functions in `Distance`: euclidean, manhattan, chebychev distance of arrays or generic vectors. SAD, MAE, SSD, MSE metrics. Pearson's, Canberra and Minkowski distance. Hamming distance.
* Windows: ported windowing functions from Neodym (Hamming, Hann, Cosine, Lanczos, Gauss, Blackmann, Bartlett, ...)
* BigInteger factorial
* Build:
   * FAKE-based build (in addition to existing Visual Studio solutions) to clean, build, test, document and package independently of the CI server.
   * Finally proper documentation using FSharp.Formatting with sources included in the repository so it is versioned and can be contributed to with pull requests.
   * NuGet packages now also include the PCL portable profile 47 (.Net 4.5, Silverlight 5, Windows 8) in addition to the normal .Net 4.0 build and PCL profile 136 (.Net 4.0, WindowsPhone 8, Silverlight 5, Windows 8) as before. Profile 47 uses `System.Numerics` for complex numbers, among others, which is not available in profile 136.
   * NuGet packages now also include a .Net 3.5 build of the core library.
   * IO libraries have been removed, replaced with new `.Data` packages (see list on top).
   * Alternative strong-named versions of more NuGet packages (mostly the F# extensions for now), with the `.Signed` suffix.
   * Reworked solution structure so it works in both Visual Studio 11 (2012) and 12 (2013).
   * We can now run the full unit test suite against the portable builds as well.
   * Builds should now also work properly on recent Mono on Linux (including F# projects).
   * Fixed builds on platforms with case sensitive file systems. *~Gauthier Segay*
* Integration: simplification of the double-exponential transformation api design.
* FFT: converted to static class design and shorter names for simpler usage. Drop now redundant `Transform` class.
* Generate: ported synthetic data generation and sampling routines from Neodym (includes all from old Signals namespace). F# module for higher order functions.
* Euclid: modulus vs remainder (also BigInteger), integer theory (includes all from old NumberTheory namespace).
* Complex: common short names for Exp, Ln, Log10, Log.
* Complex: fix issue where a *negative zero* may flip the sign in special cases (like `Atanh(2)`, where incidentally MATLAB and Mathematica do not agree on the sign either).
* Complex: routines to return all two square and three cubic roots of a complex number.
* Complex: More robust complex Asin/Acos for large real numbers.
* Evaluate: routine to evaluate complex polynomials, or real polynomials at a complex point.
* CommonParallel now also supported in .Net 3.5 and portable profiles; TaskScheduler can be replaced with custom implementation *~Thomas Ibel*
* F# BigRational type cleaned up and optimized *~Jack Pappas*
* F# BigRational IsZero, IsOne, IsInteger, create from fraction.
* F# BigRational Reciprocal, Power operator support (**), support for negative integer powers.
* F# functions now use the clearer `Func` suffix instead of just `F` if they return a function.
* Precision: reworked, now much more consistent. **If you use `AlmostEqual` with numbers-between/ULP semantics, please do review your code to make sure you're still using the expected variant!**. If you use the decimal-places semantics, you may need to decrement the digits argument to get the same behavior as before.
* Much less null checks, our code generally only throws `ArgumentNullException` if an unexpected null argument would *not* have caused an immediate `NullReferenceException`.
* Cases where `ArgumentOutOfRangeExceptions` where thrown with wrong arguments (i.e. no parameter name) now throw `ArgumentException` instead.
* Tests now have category attributes (to selectively run or skip categories).

### 2.6.2 - 2013-10-21
* Patch release, fixing the NuGet package to work better in WindowsPhone 8 projects. Assemblies are not changed.

### 2.6.1 - 2013-08-13
* BUG: fixing a bug in `ArrayStatistics.Variance` on arrays longer than 46341 entries.

### 2.6.0 - 2013-07-26
* See also: [What's New in Math.NET Numerics 2.6](http://christoph.ruegg.name/blog/new-in-mathnet-numerics-2-6.html)
* Linear Curve Fitting: Linear least-squares fitting (regression) to lines, polynomials and linear combinations of arbitrary functions. Multi-dimensional fitting. Also works well in F# with the F# extensions.
* Root Finding:
   * Brent's method. *~Candy Chiu, Alexander Täschner*
   * Bisection method. *~Scott Stephens, Alexander Täschner*
   * Broyden's method, for multi-dimensional functions. *~Alexander Täschner*
   * Newton-Raphson method.
   * Robust Newton-Raphson variant that tries to recover automatically in cases where it would fail or converge too slowly. This modification makes it more robust e.g. in the presence of singularities and less sensitive to the search range/interval.
   * All algorithms support a TryFind-pattern which returns success instead of throwing an exception.
   * Special case for quadratic functions, in the future to be extended e.g. to polynomials.
   * Basic bracketing algorithm
   * Also works well in F# with the F# extensions.
* Linear Algebra:
   * Native eigenvalue decomposition (EVD) support with our MKL packages *~Marcus Cuda*
   * Add missing scalar-vector operations (s-v, s/v, s%v) *~Thomas Ibel*
   * Support for new F# 3.1 row/column slicing syntax on matrices
   * Matrices learned proper OfColumn/RowVectors, analog also in F#.
   * Documentation Fixes *~Robin Neatherway*
   * BUG: Fixed exception text message when creating a matrix from enumerables (rows vs columns) *~Thomas Ibel*
   * We're phasing out MathNet.Numerics.IO that used to be included in the main package for matrix file I/O for text and Matlab formats. Use the new .Data.Text and .Data.Matlab packages instead.
* Statistics: Spearman Rank Correlation Coefficient *~Iain McDonald*
* Statistics: Covariance function, in Array-, Streaming- and common Statistics.
* Distributions: Categorical: distribution more consistent, no longer requires normalized pdf/cdf parameters
* Distributions: Categorical: inverse CDF function *~Paul Varkey*
* BUG: Distributions: Fixed static sampling methods of the `Stable` distribution. *~Artyom Baranovskiy*
* BUG: Fixed a bug in the Gamma Regularized special function where in some cases with large values it returned 1 instead of 0 and vice versa.
* The F# extensions now have a strong name in (and only in) the signed package as well (previously had not been signed). *~Gauthier Segay*
* Evaluate.Polynomial with new overload which is easier to use.
* Fixed a couple badly designed unit tests that failed on Mono.
* Repository now Vagrant-ready for easy testing against recent Mono on Debian.

### 2.5.0 - 2013-04-14
* See also: [What's New in Math.NET Numerics 2.5](http://christoph.ruegg.name/blog/new-in-mathnet-numerics-2-5.html)
* Statistics: Empty statistics now return NaN instead of either 0 or throwing an exception. *This may break code in case you relied upon the previous unusual and inconsistent behavior.*
* Linear Algebra: More reasonable ToString behavior for matrices and vectors. *This may break code if you relied upon ToString to export your full data to text form intended to be parsed again later. Note that the classes in the MathNet.Numerics.IO library are more appropriate for storing and loading data.*
* Statistics:
   * More consistent behavior for empty and single-element data sets: Min, Max, Mean, Variance, Standard Deviation etc. no longer throw exceptions if the data set is empty but instead return NaN. Variance and Standard Deviation will also return NaN if the set contains only a single entry. Population Variance and Population Standard Deviation will return 0 in this case.
   * Reworked order statistics (Quantile, Quartile, Percentile, IQR, Fivenum, etc.), now much easier to use and supporting compatibility with all 9 R-types, Excel and Mathematica. The obsolete Percentile class now leverages the new order statistics, fixing a range check bug as side effect.
   * New Hybrid Monte Carlo sampler for multivariate distributions. *~manyue*
   * New financial statistics: absolute risk and return measures. *~Phil Cleveland*
   * Explicit statistics for sorted arrays, unsorted arrays and sequences/streams. Faster algorithms on sorted data, also avoids multiple enumerations.
   * Some statistics like Quantile or empirical inverse CDF can optionally return a parametric function when multiple evaluations are needed, like for plotting.
* Linear Algebra:
   * More reasonable ToString behavior for matrices and vectors: `ToString` methods no longer render the whole structure to a string for large data, among others because they used to wreak havoc in debugging and interactive scenarios like F# FSI. Instead, ToString now only renders an excerpt of the data, together with a line about dimension, type and in case of sparse data a sparseness indicator. The intention is to give a good idea about the data in a visually useful way. How much data is shown can be adjusted in the Control class. See also ToTypeString and ToVector/MatrixString.
   * Performance: reworked and tuned common parallelization. Some operations are up to 3 magnitudes faster in some extreme cases. Replaced copy loops with native routines. More algorithms are storage-aware (and should thus perform better especially on sparse data). *~Thomas Ibel, Iain McDonald, Marcus Cuda*
   * Fixed range checks in the Thin-QR decomposition. *~Marcus Cuda*
   * BUG: Fixed bug in Gram Schmidt for solving tall matrices. *~Marcus Cuda*
   * Vectors now implement the BCL IList interfaces (fixed-length) for better integration with existing .Net code. *~Scott Stephens*
   * Matrix/Vector parsing has been updated to be able to parse the new visual format as well (see ToMatrixString).
   * DebuggerDisplay attributes for matrices and vectors.
   * Map/IndexedMap combinators with storage-aware and partially parallelized implementations for both dense and sparse data.
   * Reworked Matrix/Vector construction from arrays, enumerables, indexed enumerables, nested enumerables or by providing an init function/lambda. Non-obsolete constructors now always use the raw data array directly without copying, while static functions always return a matrix/vector independent of the provided data source.
   * F#: Improved extensions for matrix and vector construction: create, zeroCreate, randomCreate, init, ofArray2, ofRows/ofRowsList, ofColumns/ofColumnsList, ofSeqi/Listi (indexed). Storage-aware for performance.
   * F#: Updated map/mapi and other combinators to leverage core implementation, added -nz variants where zero-values may be skipped (relevant mostly for sparse matrices).
   * F#: Idiomatic slice setters for sub-matrices and sub-vectors
   * F#: More examples for matrix/vector creation and linear regression in the F# Sample-package.
* Control: Simpler usage with new static ConfigureAuto and ConfigureSingleThread methods. Resolved misleading configuration logic and naming around disabling parallelization.
* Control: New settings for linear algebra ToString behavior.
* Fixed range check in the Xor-shift pseudo-RNG.
* Parallelization: Reworked our common logic to avoid expensive lambda calls in inner loops. Tunable.
* F#: Examples (and thus the NuGet Sample package) are now F# scripts prepared for experimenting interactively in FSI, instead of normal F# files. Tries to get the assembly references right for most users, both within the Math.NET Numerics solution and the NuGet package.
* Various minor improvements on consistency, performance, tests, xml docs, obsolete attributes, redundant code, argument checks, resources, cleanup, nuget, etc.

### 2.4.0 - 2013-02-03
* Drops the dependency on the zlib library. We thus no longer have any dependencies on other packages. *~Marcus Cuda, Thomas Ibel*
* Adds Modified Bessel & Struve special functions *~Wei Wu*
* BUG: Fixes a bug in our iterative kurtosis statistics formula *~Artyom Baranovskiy*
* Linear Algebra: Performance work, this time mostly around accessing matrix rows/columns as vectors. Opting out from targeted patching in our matrix and vector indexers to allow inlining.
* Linear Algebra: Fixes an issue around Thin-QR solve *~Marcus Cuda*
* Linear Algebra: Simplifications around using native linear algebra providers (see Math.NET Numerics With Native Linear Algebra)
* F#: Adds the BigRational module from the F# PowerPack, now to be maintained here instead. *~Gustavo Guerra*
* F#: Better support for our Complex types (close to the F# PowerPack Complex type) *~Gustavo Guerra*

### 2.3.0 - 2013-11-25
* Portable Library: Adds support for WP8 (.Net 4.0 and higher, SL5, WP8 and .NET for Windows Store apps)
* Portable Library: New: portable build also for F# extensions (.Net 4.5, SL5 and .NET for Windows Store apps)
* Portable Library: NuGet: portable builds are now included in the main packages, no more need for special portable packages
* Linear Algebra: Continued major storage rework, in this release focusing on vectors (previous release was on matrices)
* Linear Algebra: Thin QR decomposition (in addition to existing full QR)
* Linear Algebra: Static CreateRandom for all dense matrix and vector types
* Linear Algebra: F#: slicing support for matrices and vectors
* Distributions: Consistent static Sample methods for all continuous and discrete distributions (was previously missing on a few)
* F#: better usability for random numbers and distributions.
* F# extensions are now using F# 3.0
* Updated Intel MKL references for our native linear algebra providers
* Various bug, performance and usability fixes
