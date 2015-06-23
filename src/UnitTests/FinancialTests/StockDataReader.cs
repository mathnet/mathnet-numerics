using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathNet.Numerics.UnitTests.FinancialTests
{
    /// <summary>
    /// Class reads a file with stockdata
    /// </summary>
    internal class StockDataReader
    {
        /// <summary>
        /// Reads a file with stockquotes
        /// </summary>
        /// <param name="filePath">Path of the file</param>
        /// <returns>Stockdata</returns>
        public IEnumerable<StockData> ReadFile(string filePath)
        {
            List<StockData> resultList = new List<StockData>();

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

                    //first Datetime:
                    DateTime date = DateTime.Parse(stringValues[0], CultureInfo.InvariantCulture);
                    double open = double.Parse(stringValues[1], CultureInfo.InvariantCulture);
                    double high = double.Parse(stringValues[2], CultureInfo.InvariantCulture);
                    double low = double.Parse(stringValues[3], CultureInfo.InvariantCulture);
                    double close = double.Parse(stringValues[4], CultureInfo.InvariantCulture);
                    int volume = int.Parse(stringValues[5], CultureInfo.InvariantCulture);

                    StockData stockData = new StockData(date, open, close, high, low, volume);
                    resultList.Add(stockData);
                }
            }

            return resultList;
        }
    }
    /// <summary>
    /// Entity class for holding stockdata
    /// </summary>
    internal class StockData
    {
        /// <summary>
        /// Ctor
        /// </summary>
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

