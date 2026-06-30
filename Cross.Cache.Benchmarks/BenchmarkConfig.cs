namespace Cross.Cache.Benchmarks;

public static class BenchmarkConfig
{
    /// <summary>
    /// Быстрый прогон для локальной разработки.
    /// InvocationCount задаётся на каждом методе через [InvocationCount].
    /// </summary>
    public static IConfig Fast { get; } = ManualConfig
        .Create(DefaultConfig.Instance)
        .AddJob(Job.Default
            .WithId("Fast")
            .WithLaunchCount(1)
            .WithWarmupCount(1)
            .WithIterationCount(3)
            .WithUnrollFactor(1));

    /// <summary>
    /// Более точный прогон.
    /// Запуск: dotnet run -c Release -- --full
    /// </summary>
    public static IConfig Full { get; } = ManualConfig
        .Create(DefaultConfig.Instance)
        .AddJob(Job.Default
            .WithId("Full")
            .WithLaunchCount(2)
            .WithWarmupCount(3)
            .WithIterationCount(10)
            .WithUnrollFactor(1));
}
