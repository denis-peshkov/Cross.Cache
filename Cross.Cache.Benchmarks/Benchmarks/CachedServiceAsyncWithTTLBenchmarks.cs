namespace Cross.Cache.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class CachedServiceAsyncWithTTLBenchmarks
{
    // Подобрано под Fast: итерация >= 100 ms.
    private const int GetOrAddHitInvocationCount = 2_370_000;
    private const int GetOrAddMissInvocationCount = 145_000;
    private const int TryGetHitInvocationCount = 4_200_000;
    private const int TryGetMissInvocationCount = 4_250_000;
    private const int SetNewEntryInvocationCount = 252_000;

    private CachedServiceAsyncWithTTL _service = null!;
    private const string CachedKey = "cached-key";
    private const string MissKey = "miss-key";

    [GlobalSetup]
    public void Setup()
    {
        _service = new CachedServiceAsyncWithTTL();
        _service.Set(CachedKey, 42, TimeSpan.FromMinutes(5));
    }

    [Benchmark]
    [InvocationCount(GetOrAddHitInvocationCount)]
    public Task<int> GetOrAddAsync_Hit()
        => _service.GetOrAddAsync(CachedKey, () => Task.FromResult(99), TimeSpan.FromMinutes(5));

    [Benchmark]
    [InvocationCount(GetOrAddMissInvocationCount)]
    public Task<int> GetOrAddAsync_Miss()
    {
        var key = Guid.NewGuid().ToString("N");
        return _service.GetOrAddAsync(key, () => Task.FromResult(1), TimeSpan.FromMinutes(5));
    }

    [Benchmark]
    [InvocationCount(TryGetHitInvocationCount)]
    public bool TryGetValue_Hit()
        => _service.TryGetValue(CachedKey, out int _);

    [Benchmark]
    [InvocationCount(TryGetMissInvocationCount)]
    public bool TryGetValue_Miss()
        => _service.TryGetValue(MissKey, out int _);

    [Benchmark]
    [InvocationCount(SetNewEntryInvocationCount)]
    public void Set_NewEntry()
    {
        var key = Guid.NewGuid().ToString("N");
        _service.Set(key, 100, TimeSpan.FromMinutes(5));
    }
}
