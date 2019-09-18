namespace MathNet.Numerics.Integration.GaussRule
{
    /// <summary>
    /// Contains two set of abscissas, weights, and order.
    /// </summary>
    internal class GaussPoints
    {
        internal int Order { get; private set; }

        internal double[] Abscissas { get; private set; }

        internal double[] Weights { get; private set; }
        
        internal int SecondOrder { get; private set; }

        internal double[] SecondAbscissas { get; private set; }

        internal double[] SecondWeights { get; private set; }

        internal GaussPoints(int order, double[] abscissas, double[] weights, int secondOrder, double[] secondAbscissas, double[] secondWeights)
        {
            Order = order;
            Abscissas = abscissas;
            Weights = weights;
            SecondOrder = secondOrder;
            SecondAbscissas = secondAbscissas;
            SecondWeights = secondWeights;
            
        }

        internal GaussPoints(int order, double[] abscissas, double[] weights, int secondOrder, double[] secondWeights)
            : this(order, abscissas, weights, secondOrder, null, secondWeights)
        { }
    }
}
