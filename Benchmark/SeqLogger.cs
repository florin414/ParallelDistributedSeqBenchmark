using BenchmarkDotNet.Loggers;
using Serilog;
using ILogger = BenchmarkDotNet.Loggers.ILogger;

namespace ParallelDistributedSeqBenchmark.Benchmark;
internal class SeqLogger : ILogger
{
    private readonly Serilog.ILogger _logger;

    public string Id => "SeqLogger";

    public int Priority => 0;

    public SeqLogger()
    {
        _logger = new LoggerConfiguration()
           .MinimumLevel.Verbose()
           .WriteTo.Async(a => a.Seq("http://localhost:5341", apiKey: "gqnDDZvsMrAtT9VuabP4"))
           .CreateLogger();
    }

    public void Write(LogKind logKind, string text)
    {
        switch (logKind)
        {
            case LogKind.Error:
                _logger.Error(text);
                break;
            case LogKind.Info:
                _logger.Information(text);
                break;
            case LogKind.Warning:
                _logger.Warning(text);
                break;
            default:
                _logger.Debug(text);
                break;
        }
    }

    public void WriteLine() => _logger.Information("");
    public void WriteLine(LogKind logKind, string text) => Write(logKind, text);
    public void Flush() { }
}