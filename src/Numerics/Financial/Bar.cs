using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Financial
{
    /// <summary>
    /// Repressentation of a stock bar 
    /// </summary>
    public struct Bar
    {
        private readonly double _high;
        private readonly double _low;
        private readonly double _open;
        private readonly double _close;

        /// <summary>
        /// High of the bar
        /// </summary>
        public double High
        {
            get { return _high; }
        }

        /// <summary>
        /// Low of the bar
        /// </summary>
        public double Low
        {
            get { return _low; }
        }

        /// <summary>
        /// Open of the bar
        /// </summary>
        public double Open
        {
            get { return _open; }
        }

        /// <summary>
        /// Close of the bar
        /// </summary>
        public double Close
        {
            get { return _close; }
        }
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="high"> High of the bar</param>
        /// <param name="low"> Low of the bar</param>
        /// <param name="open"> Open of the bar</param>
        /// <param name="close"> Close of the bar</param>
        public Bar(double high,double low,double open,double close)
        {
            _high = high;
            _low = low;
            _open = open;
            _close = close;
        }
    }
}
