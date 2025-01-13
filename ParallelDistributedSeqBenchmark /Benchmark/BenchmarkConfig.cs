using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Exporters.Plotting;
using BenchmarkDotNet.Loggers;
using ParallelDistributedSeqBenchmark.Seq;

namespace ParallelDistributedSeqBenchmark.Benchmark;
internal class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddExporter(new CsvExporter(CsvSeparator.Comma));
        AddExporter(new JsonExporter("", true));
        AddExporter(HtmlExporter.Default);
        AddLogger(ConsoleLogger.Default);
        //AddLogger(new SeqLogger());
        AddExporter(ScottPlotExporter.Default);
        AddColumn(
            TargetMethodColumn.Method,
            StatisticColumn.Max,
            StatisticColumn.Min,
            StatisticColumn.Mean
        );

        AddColumnProvider(DefaultColumnProviders.Metrics);
        AddColumnProvider(DefaultColumnProviders.Params);

        AddAnalyser(new SeqAnalyser());

        ArtifactsPath = Path.Combine("\\home\\florinliviu\\Desktop\\ParallelDistributedSeqBenchmark\\", "BenchmarkResults");
    }
}