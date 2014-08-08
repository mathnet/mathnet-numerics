using System.Linq;
using Binarysharp.Benchmark;
using ConsoleDump;

namespace Performance
{
    public class Program
    {
        public static void Main()
        {
            Run(new LinearAlgebra.DenseVectorAdd(10000000), 10, "Large (10'000'000)");
            Run(new LinearAlgebra.DenseVectorAdd(100), 10000, "Small (100)");
        }

        static void Run<T>(uint iterations, string suffix = null) where T:new()
        {
            var bench = new BenchShark();
            var result = bench.EvaluateDecoratedTasks<T>(iterations);
            var label = string.IsNullOrEmpty(suffix) ? typeof (T).FullName : string.Concat(typeof (T).FullName, ": ", suffix);
            result.FastestEvaluations.Select(x => new { x.Name, x.BestExecutionTime, x.AverageExecutionTime, x.WorstExecutionTime }).Dump(label);
        }

        static void Run(object obj, uint iterations, string suffix = null)
        {
            var bench = new BenchShark();
            var result = bench.EvaluateDecoratedTasks(obj, iterations);
            var label = string.IsNullOrEmpty(suffix) ? obj.GetType().FullName : string.Concat(obj.GetType().FullName, ": ", suffix);
            result.FastestEvaluations.Select(x => new { x.Name, x.BestExecutionTime, x.AverageExecutionTime, x.WorstExecutionTime }).Dump(label);
        }
    }
}
