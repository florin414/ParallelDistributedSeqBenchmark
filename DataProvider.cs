namespace ParallelDistributedSeqBenchmark;

internal static class DataProvider
{
    public static List<int> RetrieveData(string dataSource, int size)
    {
        Random rand = new();

        return dataSource switch
        {
            "Random" => Enumerable.Range(0, size).Select(_ => rand.Next()).ToList(),
            "Sorted" => Enumerable.Range(0, size).ToList(),
            "Reversed" => Enumerable.Range(0, size).Reverse().ToList(),
            "Duplicates" => Enumerable.Range(0, size / 10).SelectMany(_ => Enumerable.Repeat(rand.Next(), 10)).ToList(),
            _ => throw new ArgumentException("Unknown data source"),
        };
    }
}
