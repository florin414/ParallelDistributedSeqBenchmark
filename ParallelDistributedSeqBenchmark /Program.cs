﻿#if DEBUG
    using ParallelDistributedSeqBenchmark.Seq;

    var seqIngestor = SeqEventIngestor.FromJsonFile("seqconfig.json");
    await seqIngestor.SendEventsFromFileAsync("seqeventslogs.json");

#else
    using BenchmarkDotNet.Running;
    using ParallelDistributedSeqBenchmark.Benchmark;
    using Serilog;

    try
    {
        Log.Information("Starting benchmark...");

        await Task.Run(() => BenchmarkRunner.Run<BenchmarkDiagnoserSort>(new BenchmarkConfig()));

        Log.Information("Benchmark completed.");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An unexpected error occurred.");
    }
    finally
    {
        await Log.CloseAndFlushAsync();
    }

#endif
