using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Reports;
using Serilog;

namespace ParallelDistributedSeqBenchmark.Benchmark;
internal class SeqAnalyser : IAnalyser
{
    private readonly ILogger _logger;

    public SeqAnalyser()
    {
        _logger = new LoggerConfiguration()
           .MinimumLevel.Verbose()
           .WriteTo.Async(a => a.Seq("http://localhost:5341", apiKey: "gqnDDZvsMrAtT9VuabP4"))
           .CreateLogger();
    }

    public string Id => "SerilogAnalyser";

    public IEnumerable<Conclusion> Analyse(Summary summary)
    {
        const string benchmarkTag = "Benchmark";
        int count = 1;

        foreach (var report in summary.Reports)
        {
            var parametersInfo = new List<string>();

            foreach (var param in report.BenchmarkCase.Parameters.Items)
            {
                parametersInfo.Add($"{param.Name}: {param.Value} ");
            }

            _logger.Information($"{benchmarkTag}[{count++}]: " +
                $"Method: {report.BenchmarkCase.Descriptor.WorkloadMethod.Name}, " +
                $"Max time: {(report.ResultStatistics.Max / 1000).ToString("#,0.0", System.Globalization.CultureInfo.InvariantCulture)} us, " +
                $"{string.Join(", ", parametersInfo)}, " +
                $"Allocated Memory: {report.Metrics.FirstOrDefault(m => m.Key == "Allocated Memory").Value.Value} B");
        }

        return [];
    }
}

