using System;

namespace MathNet.Numerics.RootFinding
{
    public class RootFindingException : Exception
    {
        public RootFindingException(string message, int iteration, double rangeMin, double rangeMax, double accuracy)
            : base(message)
        {
            Iteration = iteration;
            RangeMin = rangeMin;
            RangeMax = rangeMax;
            Accuracy = accuracy;
        }

        public int Iteration { get; set; }
        public double RangeMin { get; set; }
        public double RangeMax { get; set; }
        public double Accuracy { set; get; }
    }
}