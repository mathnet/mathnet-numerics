namespace MathNet.Numerics.Integration.GaussRule
{
    /// <summary>
    /// Contains two GaussPoint.
    /// </summary>
    internal class GaussPointPair
    {
        internal int Order { get; }

        internal double[] Abscissas { get; }

        internal double[] Weights { get; }

        internal int SecondOrder { get; }

        internal double[] SecondAbscissas { get; }

        internal double[] SecondWeights { get; }

        internal double IntervalBegin { get; }

        internal double IntervalEnd { get; }

        internal GaussPointPair(double intervalBegin, double intervalEnd, int order, double[] abscissas, double[] weights, int secondOrder, double[] secondAbscissas, double[] secondWeights)
        {
            IntervalBegin = intervalBegin;
            IntervalEnd = intervalEnd;
            Order = order;
            Abscissas = abscissas;
            Weights = weights;
            SecondOrder = secondOrder;
            SecondAbscissas = secondAbscissas;
            SecondWeights = secondWeights;
        }

        internal GaussPointPair(int order, double[] abscissas, double[] weights, int secondOrder, double[] secondWeights)
            : this(-1, 1, order, abscissas, weights, secondOrder, null, secondWeights)
        { }
    }
}
