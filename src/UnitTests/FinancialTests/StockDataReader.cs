using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace MathNet.Numerics.UnitTests.FinancialTests
{
    /// <summary>
    /// Class reads a file with stock data
    /// </summary>
    internal class StockDataReader
    {
        /// <summary>
        /// Reads a file with stock quotes
        /// </summary>
        /// <param name="filePath">Path of the file</param>
        /// <returns>StockData</returns>
        public IEnumerable<StockData> ReadFile(string filePath)
        {
            List<StockData> resultList = new List<StockData>();

            var dateFormat = new CultureInfo("de-DE", false).DateTimeFormat;
            var numberFormat = CultureInfo.InvariantCulture.NumberFormat;

            using (var reader = new StreamReader(filePath))
            {
                var firstLine = reader.ReadLine();

                string line = string.Empty;

                while(true)
                {
                    line = reader.ReadLine();
                    if (line == null)
                        break;

                    line = line.Trim();
                    if (line.Equals(string.Empty))
                        continue;

                    var stringValues = line.Split(';');

                    DateTime date = DateTime.Parse(stringValues[0], dateFormat);
                    double open = double.Parse(stringValues[1], numberFormat);
                    double high = double.Parse(stringValues[2], numberFormat);
                    double low = double.Parse(stringValues[3], numberFormat);
                    double close = double.Parse(stringValues[4], numberFormat);
                    int volume = int.Parse(stringValues[5], numberFormat);

                    StockData stockData = new StockData(date, open, close, high, low, volume);
                    resultList.Add(stockData);
                }
            }

            return resultList;
        }
    }
    /// <summary>
    /// Entity class for holding stock data
    /// </summary>
    internal class StockData
    {
        /// <param name="dateTime">Date</param>
        /// <param name="open">Open quote</param>
        /// <param name="close">Open quote</param>
        /// <param name="high">Highest quote</param>
        /// <param name="low">Lower quote</param>
        /// <param name="volume">Volume</param>
        public StockData(DateTime dateTime,double open,double close, double high,double low,int volume)
        {
            Date = dateTime;
            Open = open;
            Close = close;
            High = high;
            Low = low;
            Volume = volume;
        }
        /// <summary>
        /// Date
        /// </summary>
        public DateTime Date { get; private set; }
        /// <summary>
        /// Open quote
        /// </summary>
        public double Open { get; private set; }
        /// <summary>
        /// Close quote
        /// </summary>
        public double Close { get; private set; }
        /// <summary>
        /// Highest quote
        /// </summary>
        public double High { get; private set; }
        /// <summary>
        /// Lower quote
        /// </summary>
        public double Low { get; private set; }
        /// <summary>
        /// Volume
        /// </summary>
        public int Volume { get; private set; }
    }
}
