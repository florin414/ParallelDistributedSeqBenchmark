using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotMemory;
using Serilog;

namespace ParallelDistributedSeqBenchmark.Benchmark;

[MemoryDiagnoser(true)]
[GcServer(true)]
[DotMemoryDiagnoser] 
public class BenchmarkDiagnoserSort
{
    private static readonly Serilog.ILogger Logger = Log.ForContext<BenchmarkDiagnoserSort>();
    
    [Params(1_000, 10_000, 100_000)]
    public int Size { get; set; }

    [ParamsSource(nameof(DataSources))]
    public string? DataSource { get; set; }
    public static IEnumerable<string> DataSources => ["Random", "Reversed", "Duplicates"];

    [Params(2, 4, 8)]
    public int ParallelismDegree { get; set; }

    private List<int>? list;
    private Sort? sort;

    [GlobalSetup]
    public void Setup()
    {
        Logger.Information("Setting up benchmark with Size={Size}, DataSource={DataSource}, ParallelismDegree={ParallelismDegree}",
            Size, DataSource, ParallelismDegree);

        list = DataProvider.RetrieveData(DataSource, Size);
        sort = new Sort();
    }

    [Benchmark]
    public List<int> PLINQMergeSort()
    {
        Logger.Debug("Starting PLINQMergeSort with ParallelismDegree={ParallelismDegree}", ParallelismDegree);

        return sort.PLINQMergeSort(list, ParallelismDegree);
    }

    [Benchmark(Baseline = true)]
    public List<int> MergeSort()
    {
        Logger.Debug("Starting MergeSort...");

        return sort.MergeSort(list);
    }
}