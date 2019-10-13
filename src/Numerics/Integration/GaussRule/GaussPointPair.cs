namespace MathNet.Numerics.Integration.GaussRule
{
    /// <summary>
    /// Contains two GaussPoint.
    /// </summary>
    internal class GaussPointPair
    {
        internal int Order { get; private set; }

        internal double[] Abscissas { get; private set; }

        internal double[] Weights { get; private set; }
        
        internal int SecondOrder { get; private set; }

        internal double[] SecondAbscissas { get; private set; }

        internal double[] SecondWeights { get; private set; }

        internal double IntervalBegin { get; private set; }

        internal double IntervalEnd { get; private set; }

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
