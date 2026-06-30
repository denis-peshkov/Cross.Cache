namespace Cross.Cache.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class CacheValueConverterHelperBenchmarks
{
    // Подобрано под Fast: итерация >= 100 ms.
    private const int JsonObjectInvocationCount = 200_000;
    private const int IntInvocationCount = 1_300_000;
    private const int NullInvocationCount = 37_500_000;
    private const int EmptyJsonArrayInvocationCount = 410_000;

    private string _jsonDto = null!;
    private string _intValue = null!;
    private string _emptyJsonArray = null!;

    [GlobalSetup]
    public void Setup()
    {
        _jsonDto = JsonSerializer.Serialize(new BenchmarkSampleDto
        {
            Id = 42,
            Code = "code",
            Description = "description"
        });
        _intValue = "123";
        _emptyJsonArray = "[]";
    }

    [Benchmark]
    [InvocationCount(JsonObjectInvocationCount)]
    public BenchmarkSampleDto? GetConvertedValue_JsonObject()
        => CacheValueConverterHelper.GetConvertedValue<BenchmarkSampleDto>(_jsonDto);

    [Benchmark]
    [InvocationCount(IntInvocationCount)]
    public int? GetConvertedValue_Int()
        => CacheValueConverterHelper.GetConvertedValue<int>(_intValue);

    [Benchmark]
    [InvocationCount(NullInvocationCount)]
    public BenchmarkSampleDto? GetConvertedValue_Null()
        => CacheValueConverterHelper.GetConvertedValue<BenchmarkSampleDto>(null);

    [Benchmark]
    [InvocationCount(EmptyJsonArrayInvocationCount)]
    public List<int>? GetConvertedValue_EmptyJsonArray()
        => CacheValueConverterHelper.GetConvertedValue<List<int>>(_emptyJsonArray);
}
