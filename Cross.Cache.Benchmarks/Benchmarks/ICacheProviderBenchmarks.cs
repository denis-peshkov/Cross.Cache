namespace Cross.Cache.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[BenchmarkCategory("ICacheProvider")]
public class ICacheProviderBenchmarks
{
    // Сетевые операции: одинаковый InvocationCount для сравнения InMemory vs Redis (~100 ms на Redis).
    private const int GetCacheInvocationCount = 3_000;
    private const int SetNewKeyInvocationCount = 1_000;
    private const int SetExistingKeyInvocationCount = 3_000;
    private const int DeserializeInvocationCount = 1_500;
    private const int KeyExistsInvocationCount = 3_000;

    // Локальная операция без I/O — подобрано под Fast: итерация >= 100 ms.
    private const int BuildCacheKeyInvocationCount = 2_440_000;

    private ICacheProvider _provider = null!;
    private string _keyPrefix = null!;
    private string _key = null!;
    private string _dtoKey = null!;
    private string _value = null!;
    private string _jsonValue = null!;

    [ParamsSource(nameof(GetProviderKinds))]
    public BenchmarkCacheProviderKind ProviderKind { get; set; }

    public IEnumerable<BenchmarkCacheProviderKind> GetProviderKinds()
        => BenchmarkCacheProviderFactory.GetAvailableProviderKinds();

    [GlobalSetup]
    public void Setup()
    {
        _provider = BenchmarkCacheProviderFactory.Create(ProviderKind);
        _keyPrefix = $"cross-cache-bench:{ProviderKind}:";
        _key = $"{_keyPrefix}hit";
        _dtoKey = $"{_keyPrefix}dto";
        _value = "benchmark-value";
        _jsonValue = JsonSerializer.Serialize(new BenchmarkSampleDto
        {
            Id = 1,
            Code = "code",
            Description = "description"
        });

        _provider.SetCacheAsync(_key, _value).GetAwaiter().GetResult();
        _provider.SetCacheAsync(_dtoKey, _jsonValue).GetAwaiter().GetResult();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (ProviderKind != BenchmarkCacheProviderKind.Redis)
            return;

        _provider.RemoveCachesByPatternAsync($"{_keyPrefix}*").GetAwaiter().GetResult();
    }

    [Benchmark]
    [InvocationCount(GetCacheInvocationCount)]
    public Task<string> GetCacheAsync_Hit()
        => _provider.GetCacheAsync(_key);

    [Benchmark]
    [InvocationCount(GetCacheInvocationCount)]
    public Task<string> GetCacheAsync_Miss()
        => _provider.GetCacheAsync($"{_keyPrefix}missing");

    [Benchmark]
    [InvocationCount(SetNewKeyInvocationCount)]
    public Task SetCacheAsync_NewKey()
    {
        var key = $"{_keyPrefix}{Guid.NewGuid():N}";
        return _provider.SetCacheAsync(key, _value);
    }

    [Benchmark]
    [InvocationCount(SetExistingKeyInvocationCount)]
    public Task SetCacheAsync_ExistingKey()
        => _provider.SetCacheAsync(_key, _value);

    [Benchmark]
    [InvocationCount(DeserializeInvocationCount)]
    public Task<BenchmarkSampleDto?> GetValueAsync_Deserialize()
        => _provider.GetValueAsync<BenchmarkSampleDto>(_dtoKey);

    [Benchmark]
    [InvocationCount(BuildCacheKeyInvocationCount)]
    public string BuildCacheKey()
        => _provider.BuildCacheKey(nameof(BenchmarkSampleDto), _key);

    [Benchmark]
    [InvocationCount(KeyExistsInvocationCount)]
    public Task<bool> KeyExistsAsync_Hit()
        => _provider.KeyExistsAsync(_key);
}
