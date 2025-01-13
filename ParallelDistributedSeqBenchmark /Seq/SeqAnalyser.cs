using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Reports;
using Serilog;
using System.Text.Json;

namespace ParallelDistributedSeqBenchmark.Seq
{
    internal class SeqAnalyser : IAnalyser
    {
        private readonly ILogger _logger;
        private readonly string _logFilePath = "seqeventslogs.json";
        private readonly object _fileLock = new object();

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
                    parametersInfo.Add($"{param.Name}: {param.Value}");
                }

                var seqEvent = new SeqEvent
                {
                    Tag = benchmarkTag,
                    Count = count++,
                    Method = report.BenchmarkCase.Descriptor.WorkloadMethod.Name,
                    MaxTimeUs = (report.ResultStatistics.Max / 1000).ToString("#,0.0", System.Globalization.CultureInfo.InvariantCulture),
                    Parameters = parametersInfo,
                    AllocatedMemoryBytes = report.Metrics.FirstOrDefault(m => m.Key == "Allocated Memory").Value?.Value
                };
                
                _logger.Information("{@SeqEvent}", seqEvent);
                
                WriteLogToFile(seqEvent);
            }

            return Array.Empty<Conclusion>();
        }

        private void WriteLogToFile(object logEntry)
        {
            lock (_fileLock)
            {
                try
                {
                    var existingLogs = File.Exists(_logFilePath)
                        ? JsonSerializer.Deserialize<List<object>>(File.ReadAllText(_logFilePath)) ?? new List<object>()
                        : new List<object>();
                    
                    existingLogs.Add(logEntry);
                    
                    File.WriteAllText(_logFilePath, JsonSerializer.Serialize(existingLogs, new JsonSerializerOptions { WriteIndented = true }));
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to write log to file: {ErrorMessage}", ex.Message);
                }
            }
        }
    }
}
