namespace ParallelDistributedSeqBenchmark.Seq;

/// <summary>
/// Represents an event to be sent to Seq or saved to a log file.
/// </summary>
public class SeqEvent
{
    public string Tag { get; set; }
    public int Count { get; set; }
    public string Method { get; set; }
    public string MaxTimeUs { get; set; }
    public List<string> Parameters { get; set; }
    public double? AllocatedMemoryBytes { get; set; }
}
