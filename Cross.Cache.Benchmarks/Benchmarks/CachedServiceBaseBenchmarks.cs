namespace Cross.Cache.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class CachedServiceBaseBenchmarks
{
    // Подобрано под Fast: итерация >= 100 ms.
    private const int GetAllCachedInvocationCount = 33_700_000;
    private const int GetAllForceReloadInvocationCount = 4_560_000;
    private const int InvalidateInvocationCount = 10_600_000;

    private BenchmarkCachedService _service = null!;

    [GlobalSetup]
    public void Setup()
    {
        _service = new BenchmarkCachedService();
        _service.GetAll(force: false);
    }

    [Benchmark]
    [InvocationCount(GetAllCachedInvocationCount)]
    public BenchmarkSampleDto GetAll_Cached()
        => _service.GetAll(force: false);

    [Benchmark]
    [InvocationCount(GetAllForceReloadInvocationCount)]
    public BenchmarkSampleDto GetAll_ForceReload()
        => _service.GetAll(force: true);

    [Benchmark]
    [InvocationCount(InvalidateInvocationCount)]
    public void Invalidate()
        => _service.Invalidate();

    private sealed class BenchmarkCachedService : CachedServiceBase<BenchmarkSampleDto>
    {
        protected override Task<BenchmarkSampleDto> Load()
            => Task.FromResult(new BenchmarkSampleDto
            {
                Id = 1,
                Code = "code",
                Description = "description"
            });
    }
}
