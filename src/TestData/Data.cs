using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MathNet.Numerics.TestData
{
    public static class Data
    {
#if NETSTANDARD1_1
        static readonly Assembly DataAssembly = typeof(Data).GetTypeInfo().Assembly;
#else
        static readonly Assembly DataAssembly = typeof (Data).Assembly;
#endif

        public static Stream ReadStream(string name)
        {
            return DataAssembly.GetManifestResourceStream("MathNet.Numerics.TestData.Data." + name);
        }

        public static TextReader ReadText(string name)
        {
            var stream = ReadStream(name);
            return new StreamReader(stream);
        }

        public static string[] ReadAllLines(string name)
        {
            List<string> lines = new List<string>();

            using (TextReader reader = ReadText(name))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines.ToArray();
        }
    }
}
