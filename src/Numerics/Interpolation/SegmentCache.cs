namespace MathNet.Numerics.Interpolation
{
    internal class SegmentCache
    {
        public SegmentCache()
        {
            LeftSegmentIndex = -1;
            LeftSegment = 0;
            RightSegment = 0;
        }

        public double LeftSegment { get; set; }
        public int LeftSegmentIndex { get; set; }
        public double RightSegment { get; set; }

    }
}