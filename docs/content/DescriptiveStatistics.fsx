(*** hide ***)
#I "../../out/lib/net40"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"

(**
Descriptive Statistics
======================

Univariate Statistical Analysis
-------------------------------

To compute descriptive statistical characteristics of a sample set you can either call
the extension methods of the [Statistics][stats] class directly, or create a new
[DescriptiveStatistics][dstats] instance and pass your samples to its constructor to compute
all the characteristics in one pass.

  [stats]: http://numerics.mathdotnet.com/api/MathNet.Numerics.Statistics/Statistics.htm
  [dstats]: http://numerics.mathdotnet.com/api/MathNet.Numerics.Statistics/DescriptiveStatistics.htm

Code Sample using _DescriptiveStatistics_:

    [lang=csharp]
    using MathNet.Numerics.Statistics;

    var samples = new ChiSquare(5).Samples().Take(1000);
    var statistics = new DescriptiveStatistics(samples);

    // Order Statistics
    var largestElement = statistics.Maximum;
    var smallestElement = statistics.Minimum;
    var median = statistics.Median;

    // Central Tendency
    var mean = statistics.Mean;

    // Dispersion
    var variance = statistics.Variance;
    var stdDev = statistics.StandardDeviation;

    // Other Statistics
    var kurtosis = statistics.Kurtosis;
    var skewness = statistics.Skewness;

Code Sample using the extensions methods:

    [lang=csharp]
    using MathNet.Numerics.Statistics;

    // Extension methods are defined on IEnumerable<double>,
    // yet we call ToArray so all the methods operate on the same data
    var samples = new ChiSquare(5).Samples().Take(1000).ToArray();

    // Order Statistics
    var largestElement = samples.Maximum();
    var smallestElement = samples.Minimum();
    var median = samples.Median();
    var 250thOrderStatistic = samples.OrderStatistic(250);

    // Central Tendency
    var mean = samples.Mean();

    // Dispersion
    var variance = samples.Variance();
    var biasedPopulationVariance = samples.PopulationVariance();
    var stdDev = samples.StandardDeviation();
    var biasedPopulationStdDev = samples.PopulationStandardDeviation();


Histograms
----------

A histrogram can be computed using the [Histogram][hist] class. Its constructor takes
the samples enumerable. the number of buckets to create, plus optionally the range
(minimum, maximum) of the sample data if available.

  [hist]: http://numerics.mathdotnet.com/api/MathNet.Numerics.Statistics/Histogram.htm

    [lang=csharp]
    var histogram = new Histogram(samples, 10);
    var bucket3count = histogram[2].Count;


Percentiles
-----------

Percentiles can be computed using the [Percentile][percentile] class.
It supports four methods, which can be chosen using the _Methods_ property:

  [percentile]: http://numerics.mathdotnet.com/api/MathNet.Numerics.Statistics/Percentile.htm

* _Nist_: Using the method [recommended](http://www.itl.nist.gov/div898/handbook/prc/section2/prc252.htm) by NIST. This is the default method.
* _Nearest_: Using the [nearest rank](http://en.wikipedia.org/wiki/Percentile#Nearest_Rank) method.
* _Excel_: Using the [method](http://www.itl.nist.gov/div898/handbook/prc/section2/prc252.htm) that is also used by Microsoft Excel.
* _Interpolation_: Using linear interpolation between the two nearest ranks, see [wikipedia](http://en.wikipedia.org/wiki/Percentile#Linear_Interpolation_Between_Closest_Ranks).

    [lang=csharp]
    var percentile = new Percentile(samples) { Method = PercentileMethod.Nearest };
    var percentile90 = percentile.Compute(0.9);
    var percentiles = percentile.Compute(new[] { .25, .5, .75 });


Correlation
-----------

The [Correlation][corr] class supports computing Pearson product-momentum correlation coefficients:

  [corr]: http://numerics.mathdotnet.com/api/MathNet.Numerics.Statistics/Correlation.htm

Code Sample: Computing the correlation coefficient between 1000 samples of f(x) = 2x and g(x) = x^2:

    [lang=csharp]
    double[] dataF = SignalGenerator.EquidistantInterval(x => x * 2, 0, 100, 1000);
    double[] dataG = SignalGenerator.EquidistantInterval(x => x * x, 0, 100, 1000);
    double correlation = Correlation.Pearson(dataF, dataG);
*)
