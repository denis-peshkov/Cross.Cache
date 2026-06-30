namespace Cross.Cache.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        var useFull = Array.Exists(args, static a => string.Equals(a, "--full", StringComparison.OrdinalIgnoreCase));
        var benchmarkArgs = useFull
            ? Array.FindAll(args, static a => !string.Equals(a, "--full", StringComparison.OrdinalIgnoreCase))
            : args;

        var config = useFull ? BenchmarkConfig.Full : BenchmarkConfig.Fast;
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(benchmarkArgs, config);
    }
}
