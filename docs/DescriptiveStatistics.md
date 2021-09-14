    [hide]
    #I "../../out/lib/net40"
    #r "MathNet.Numerics.dll"
    #r "MathNet.Numerics.FSharp.dll"
    open System.Numerics
    open MathNet.Numerics
    open MathNet.Numerics.Statistics

Descriptive Statistics
======================

Initialization
--------------

We need to reference Math.NET Numerics and open the statistics namespace:

    [lang=csharp]
    using MathNet.Numerics.Statistics;

Univariate Statistical Analysis
-------------------------------

The primary class for statistical analysis is `Statistics` which provides common
descriptive statics as static extension methods to `IEnumerable<double>` sequences.
However, various statistics can be computed much more efficiently if the data source
has known properties or structure, that's why the following classes provide specialized
static implementations:

* **ArrayStatistics** provides routines optimized for single-dimensional arrays. Some
  of these routines end with the `Inplace` suffix, indicating that they reorder the
  input array slightly towards being sorted during execution - without fully sorting
  them, which could be expensive.
* **SortedArrayStatistics** provides routines optimized for an array sorting ascendingly.
  Especially order-statistics are very efficient this way, some even with constant time complexity.
* **StreamingStatistics** processes large amounts of data without keeping them in memory.
  Useful if data larger than local memory is streamed directly from a disk or network.

Another alternative, in case you need to gather a whole set of statistical characteristics
in one pass, is provided by the `DescriptiveStatistics` class:

    [lang=csharp]
    var samples = new ChiSquare(5).Samples().Take(1000);
    var statistics = new DescriptiveStatistics(samples);

    var largestElement = statistics.Maximum;
    var smallestElement = statistics.Minimum;
    var median = statistics.Median;

    var mean = statistics.Mean;
    var variance = statistics.Variance;
    var stdDev = statistics.StandardDeviation;

    var kurtosis = statistics.Kurtosis;
    var skewness = statistics.Skewness;


Minimum & Maximum
-----------------

The minimum and maximum values of a sample set can be evaluated with the `Minimum` and `Maximum`
functions of all four classes: `Statistics`, `ArrayStatistics`, `SortedArrayStatistics`
and `StreamingStatistics`. The one in `SortedArrayStatistics` is the fastest with constant
time complexity, but expects the array to be sorted ascendingly.

Both min and max are directly affected by outliers and are therefore no robust statistics at all.
For a more robust alternative, consider using Quantiles instead.

    [lang=csharp]
    var samples = new ChiSquare(5).Samples().Take(1000).ToArray();
    var largestElement = samples.Maximum();
    var smallestElement = samples.Minimum();


Mean
----

The *arithmetic mean* or *average* of the provided samples. In statistics, the sample mean is
a measure of the central tendency and estimates the expected value of the distribution.
The mean is affected by outliers, so if you need a more robust estimate consider to use the Median instead.

`Statistics.Mean(data)`
`StreamingStatistics.Mean(stream)`
`ArrayStatistics.Mean(data)`

$$$
\overline{x} = \frac{1}{N}\sum_{i=1}^N x_i

    [lang=fsharp]
    let whiteNoise = Generate.Normal(1000, mean=10.0, standardDeviation=2.0)
    // [fsi:val samples : float [] = [|12.90021939; 9.631515037; 7.810008046; 14.13301053; ...|] ]
    Statistics.Mean whiteNoise
    // [fsi:val it : float = 10.02162347]

    let wave = Generate.Sinusoidal(1000, samplingRate=100., frequency=5., amplitude=0.5)
    Statistics.Mean wave
    // [fsi:val it : float = -4.133520783e-17]

Variance and Standard Deviation
-------------------------------

Variance $\sigma^2$ and the Standard Deviation $\sigma$ are measures of how far the samples are spread out.

If the whole population is available, the functions with the Population-prefix
 will evaluate the respective measures with an $N$ normalizer for a population of size $N$.

`Statistics.PopulationVariance(population)`
`Statistics.PopulationStandardDeviation(population)`

$$$
\sigma^2 = \frac{1}{N}\sum_{i=1}^N (x_i - \mu)^2

On the other hand, if only a sample of the full population is available, the functions
without the Population-prefix will estimate unbiased population measures by applying
Bessel's correction with an $N-1$ normalizer to a sample set of size $N$.

`Statistics.Variance(samples)`
`Statistics.StandardDeviation(samples)`

$$$
s^2 = \frac{1}{N-1}\sum_{i=1}^N (x_i - \overline{x})^2

    [lang=fsharp]
    Statistics.Variance whiteNoise
    // [fsi:val it : float = 3.819436094]
    Statistics.StandardDeviation whiteNoise
    // [fsi:val it : float = 1.954337764]

    Statistics.Variance wave
    // [fsi:val it : float = 0.1251251251]

#### Combined Routines

Since mean and variance are often needed together, there are routines
that evaluate both in a single pass:

`Statistics.MeanVariance(samples)`
`ArrayStatistics.MeanVariance(samples)`
`StreamingStatistics.MeanVariance(samples)`

    [lang=fsharp]
    Statistics.MeanVariance whiteNoise
    // [fsi:val it : float * float = (10.02162347, 3.819436094)]

Covariance
----------

The sample covariance is an estimation of the Covariance, a measure of how much two random
variables change together. Similarly to the variance above, there are two versions in order to
apply Bessel's correction to bias in case of sample data.

`Statistics.Covariance(samples1, samples2)`

$$$
q = \frac{1}{N-1}\sum_{i=1}^N (x_i - \overline{x})(y_i - \overline{y})

`Statistics.PopulationCovariance(population1, population2)`

$$$
q = \frac{1}{N}\sum_{i=1}^N (x_i - \mu_x)(y_i - \mu_y)

    [lang=fsharp]
    Statistics.Covariance(whiteNoise, whiteNoise)
    // [fsi:val it : float = 3.819436094]
    Statistics.Covariance(whiteNoise, wave)
    // [fsi:val it : float = 0.04397985084]

Order Statistics
----------------

#### Order Statistic

The k-th order statistic of a sample set is the k-th smallest value. Note that,
as an exception to most of Math.NET Numerics, the order k is one-based, meaning
the smallest value is the order statistic of order 1 (there is no order 0).

`Statistics.OrderStatistic(data, order)`
`SortedArrayStatistics.OrderStatistic(data, order)`

If the samples are sorted ascendingly, this is trivial and can be evaluated in constant time,
which is what the `SortedArrayStatistics` implementation does.

If you have the samples in an array which is not (guaranteed to be) sorted,
but if it is fine if the array does incrementally get sorted over multiple calls,
you can also use the following in-place implementation. It is usually faster
than fully sorting the array, unless you need to compute it for more than a handful orders.

`ArrayStatistics.OrderStatisticInplace(data, order)`

For convenience there's also an option that returns a function `Func<int, double>`,
mapping from order to the resulting order statistic. Internally it sorts a copy of the
provided data and then on each invocation uses efficient sorted algorithms:

`Statistics.OrderStatisticFunc(data)`

Such Inplace and Func variants are a common pattern throughout the Statistics class
and also the rest of the library.

    [lang=fsharp]
    Statistics.OrderStatistic(whiteNoise, 1)
    // [fsi:val it : float = 3.633070184]
    Statistics.OrderStatistic(whiteNoise, 1000)
    // [fsi:val it : float = 16.65183566]

    let os = Statistics.orderStatisticFunc whiteNoise
    os 250
    // [fsi:val it : float = 8.645491746]
    os 500
    // [fsi:val it : float = 10.11872428]
    os 750
    // [fsi:val it : float = 11.33170746]

#### Median

Median is a robust indicator of central tendency and much less affected by outliers
than the sample mean. The median is estimated by the value exactly in the middle of
the sorted set of samples and thus separating the higher half of the data from the lower half.

`Statistics.Median(data)`
`SortedArrayStatistics.Median(data)`
`ArrayStatistics.MedianInplace(data)`

The median is only unique if the sample size is odd. This implementation internally
uses the default quantile definition, which is equivalent to mode 8 in R and is approximately
median-unbiased regardless of the sample distribution. If you need another convention, use
`QuantileCustom` instead, see below for details.

    [lang=fsharp]
    Statistics.Median whiteNoise
    // [fsi:val it : float = 10.11872428]
    Statistics.Median wave
    // [fsi:val it : float = -2.452600839e-16]

#### Quartiles and the 5-number summary

Quartiles group the ascendingly sorted data into four equal groups, where each
group represents a quarter of the data. The lower quartile is estimated by
the middle number between the first two groups and the upper quartile by the middle
number between the remaining two groups. The middle number between the two middle groups
estimates the median as discussed above.

`Statistics.LowerQuartile(data)`
`Statistics.UpperQuartile(data)`
`SortedArrayStatistics.LowerQuartile(data)`
`SortedArrayStatistics.UpperQuartile(data)`
`ArrayStatistics.LowerQuartileInplace(data)`
`ArrayStatistics.UpperQuartileInplace(data)`

    [lang=fsharp]
    Statistics.LowerQuartile whiteNoise
    // [fsi:val it : float = 8.645491746]
    Statistics.UpperQuartile whiteNoise
    // [fsi:val it : float = 11.33213732]

Using that data we can provide a useful set of indicators usually named 5-number summary,
which consists of the minimum value, the lower quartile, the median, the upper quartile and
the maximum value. All these values can be visualized in the popular box plot diagrams.

`Statistics.FiveNumberSummary(data)`
`SortedArrayStatistics.FiveNumberSummary(data)`
`ArrayStatistics.FiveNumberSummaryInplace(data)`

    [lang=fsharp]
    Statistics.FiveNumberSummary whiteNoise
    // [fsi:val it : float [] = [|3.633070184; 8.645937823; 10.12165054; 11.33213732; 16.65183566|] ]
    Statistics.FiveNumberSummary wave
    // [fsi:val it : float [] = [|-0.5; -0.3584185509; -2.452600839e-16; 0.3584185509; 0.5|] ]

The difference between the upper and the lower quartile is called inter-quartile range (IQR)
and is a robust indicator of spread. In box plots the IQR is the total height of the box.

`Statistics.InterquartileRange(data)`
`SortedArrayStatistics.InterquartileRange(data)`
`ArrayStatistics.InterquartileRangeInplace(data)`

Just like median, quartiles use the default R8 quantile definition internally.

    [lang=fsharp]
    Statistics.InterquartileRange whiteNoise
    // [fsi:val it : float = 2.686199498]

#### Percentiles

Percentiles extend the concept further by grouping the sorted values into 100
equal groups and looking at the 101 places (0,1,..,100) between and around them.
The 0-percentile represents the minimum value, 25 the first quartile, 50 the median,
75 the upper quartile and 100 the maximum value.

`Statistics.Percentile(data, p)`
`Statistics.PercentileFunc(data)`
`SortedArrayStatistics.Percentile(data, p)`
`ArrayStatistics.PercentileInplace(data, p)`

Just like median, percentiles use the default R8 quantile definition internally.

    [lang=fsharp]
    Statistics.Percentile(whiteNoise, 5)
    // [fsi:val it : float = 6.693373507]
    Statistics.Percentile(whiteNoise, 98)
    // [fsi:val it : float = 13.97580653]

#### Quantiles

Instead of grouping into 4 or 100 boxes, quantiles generalize the concept to an infinite number
of boxes and thus to arbitrary real numbers $\tau$ between 0.0 and 1.0, where 0.0 represents the
minimum value, 0.5 the median and 1.0 the maximum value. Quantiles are closely related to
the inverse cumulative distribution function of the sample distribution.

`Statistics.Quantile(data, tau)`
`Statistics.QuantileFunc(data)`
`SortedArrayStatistics.Quantile(data, tau)`
`ArrayStatistics.QuantileInplace(data, tau)`

    [lang=fsharp]
    Statistics.Quantile(whiteNoise, 0.98)
    // [fsi:val it : float = 13.97580653]

#### Quantile Conventions and Compatibility

Remember that all these descriptive statistics do not *compute* but merely *estimate*
statistical indicators of the value distribution. In the case of quantiles,
there is usually not a single number between the two groups specified by $\tau$.
There are multiple ways to deal with this: the R project supports 9 modes and Mathematica
and SciPy have their own way to parametrize the behavior.

The `QuantileCustom` functions support all 9 modes from the R-project, which includes the one
used by Microsoft Excel, and also the 4-parameter variant of Mathematica:

`Statistics.QuantileCustom(data, tau, definition)`
`Statistics.QuantileCustomFunc(data, definition)`
`SortedArrayStatistics.QuantileCustom(data, tau, a, b, c, d)`
`SortedArrayStatistics.QuantileCustom(data, tau, definition)`
`ArrayStatistics.QuantileCustomInplace(data, tau, a, b, c, d)`
`ArrayStatistics.QuantileCustomInplace(data, tau, definition)`

The `QuantileDefinition` enumeration has the following options:

* **R1**, SAS3, EmpiricalInvCDF
* **R2**, SAS5, EmpiricalInvCDFAverage
* **R3**, SAS2, Nearest
* **R4**, SAS1, California
* **R5**, Hydrology, Hazen
* **R6**, SAS4, Nist, Weibull, SPSS
* **R7**, Excel, Mode, S
* **R8**, Median, Default
* **R9**, Normal

    [lang=fsharp]
    Statistics.QuantileCustom(whiteNoise, 0.98, QuantileDefinition.R3)
    // [fsi:val it : float = 13.97113209]
    Statistics.QuantileCustom(whiteNoise, 0.98, QuantileDefinition.Excel)
    // [fsi:val it : float = 13.97127374]

Rank Statistics
---------------

#### Ranks

Rank statistics are the counterpart to order statistics. The `Ranks` function evaluates the rank
of each sample and returns them as an array of doubles. The return type is double instead of int
in order to deal with ties, if one of the values appears multiple times.
Similar to `QuantileDefinition`, the `RankDefinition` enumeration controls how ties should be handled:

* **Average**, Default: Replace ties with their mean (causing non-integer ranks).
* **Min**, Sports: Replace ties with their minimum, as typical in sports ranking.
* **Max**: Replace ties with their maximum.
* **First**: Permutation with increasing values at each index of ties.
* **EmpiricalCDF**

`Statistics.Ranks(data, definition)`
`SortedArrayStatistics.Ranks(data, definition)`
`ArrayStatistics.RanksInplace(data, definition)`

    [lang=fsharp]
    Statistics.Ranks(whiteNoise)
    // [fsi:val it : float [] = [|634.0; 736.0; 405.0; 395.0; 197.0; 167.0; 722.0; 44.0; ...|] ]
    Statistics.Ranks([| 13.0; 14.0; 11.0; 12.0; 13.0 |], RankDefinition.Average)
    // [fsi:val it : float [] = [|3.5; 5.0; 1.0; 2.0; 3.5|] ]
    Statistics.Ranks([| 13.0; 14.0; 11.0; 12.0; 13.0 |], RankDefinition.Sports)
    // [fsi:val it : float [] = [|3.0; 5.0; 1.0; 2.0; 3.0|] ]

#### Quantile Rank

Counterpart of the `Quantile` function, estimates $\tau$ of the provided $\tau$-quantile value
$x$ from the provided samples. The $\tau$-quantile is the data value where the cumulative distribution
function crosses $\tau$.

`Statistics.QuantileRank(data, x, definition)`
`Statistics.QuantileRankFunc(data, definition)`
`SortedArrayStatistics.QuantileRank(data, x, definition)`

    [lang=fsharp]
    Statistics.QuantileRank(whiteNoise, 13.0)
    // [fsi:val it : float = 0.9370045563]
    Statistics.QuantileRank(whiteNoise, 6.7, RankDefinition.Average)
    // [fsi:val it : float = 0.04960610389]

Empirical Distribution Functions
--------------------------------

`Statistics.EmpiricalCDF(data, x)`
`Statistics.EmpiricalCDFFunc(data)`
`Statistics.EmpiricalInvCDF(data, tau)`
`Statistics.EmpiricalInvCDFFunc(data)`
`SortedArrayStatistics.EmpiricalCDF(data, x)`

    [lang=fsharp]
    let ecdf = Statistics.EmpiricalCDFFunc whiteNoise
    Generate.LinearSpacedMap(20, start=3.0, stop=17.0, map=ecdf)
    // [fsi:val it : float [] =]
    // [fsi:    [|0.0; 0.001; 0.002; 0.005; 0.022; 0.05; 0.094; 0.172; 0.278; 0.423; 0.555; ]
    // [fsi:      0.705; 0.843; 0.921; 0.944; 0.983; 0.992; 0.997; 0.999; 1.0|] ]

    let eicdf = Statistics.empiricalInvCDFFunc whiteNoise
    [ for tau in 0.0..0.05..1.0 -> eicdf tau ]
    // [fsi:val it : float [] =]
    // [fsi:    [3.633070184; 6.682142043; 7.520000817; 8.040513497; 8.347587493; ]
    // [fsi:     8.645491746; 9.02681611; 9.298987151; 9.522627142; 9.819352699; 10.11872428; ]
    // [fsi:     10.35991046; 10.57530906; 10.8259542; 11.08605473; 11.33170746; 11.54356436; ]
    // [fsi:     11.90973541; 12.4294346; 13.36889423; 16.65183566] ]

Histograms
----------

A histogram can be computed using the [Histogram][hist] class. Its constructor takes
the samples enumerable, the number of buckets to create, plus optionally the range
(minimum, maximum) of the sample data if available.

  [hist]: https://numerics.mathdotnet.com/api/MathNet.Numerics.Statistics/Histogram.htm

    [lang=csharp]
    var histogram = new Histogram(samples, 10);
    var bucket3count = histogram[2].Count;


Correlation
-----------

The `Correlation` class supports computing Pearson's product-momentum and Spearman's ranked
correlation coefficient, as well as their correlation matrix for a set of vectors.

Code Sample: Computing the correlation coefficient of 1000 samples of f(x) = 2x and g(x) = x^2:

    [lang=csharp]
    double[] dataF = Generate.LinearSpacedMap(1000, 0, 100, x => 2*x);
    double[] dataG = Generate.LinearSpacedMap(1000, 0, 100, x => x*x);
    double correlation = Correlation.Pearson(dataF, dataG);
