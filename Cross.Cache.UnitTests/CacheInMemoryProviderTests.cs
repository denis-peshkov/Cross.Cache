namespace Cross.Cache.UnitTests;

[TestFixture]
public class CacheInMemoryProviderTests
{
    private CacheInMemoryProvider _provider;

    [SetUp]
    public void SetUp()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new CacheOptions
        {
            UseCache = "InMemory",
            MaxCachedSearchPageSize = 100,
            MaxCacheSize = 10,
            CacheWarmPopularCount = 0,
            CacheWarmLatestCount = 0,
            CacheInRedis = null
        });

        _provider = new CacheInMemoryProvider(options);
    }

    [Test]
    public async Task SetCacheAsync_ShouldStoreAndReturnStringValue()
    {
        // Arrange
        var key = "key";
        var value = "value";

        // Act
        await _provider.SetCacheAsync(key, value);
        var result = await _provider.GetCacheAsync(key);

        // Assert
        result.Should().Be(value);
        _provider.GetCacheCount().Should().Be(1);
    }

    [Test]
    public async Task GetCacheAsync_ShouldReturnEmptyString_WhenKeyMissing()
    {
        // Act
        var result = await _provider.GetCacheAsync("missing");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetValueAsync_ShouldDeserializeStoredJson()
    {
        // Arrange
        var key = "dto-key";
        var dto = new SampleTestDto { Id = 1, Code = "code", Description = "desc" };
        var json = JsonSerializer.Serialize(dto);

        await _provider.SetCacheAsync(key, json);

        // Act
        var result = await _provider.GetValueAsync<SampleTestDto>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(dto.Id);
        result.Code.Should().Be(dto.Code);
        result.Description.Should().Be(dto.Description);
    }

    [Test]
    public async Task CheckCacheFull_ShouldReturnTrue_WhenCountGreaterOrEqualMax()
    {
        // Arrange
        for (var i = 0; i < 10; i++)
        {
            await _provider.SetCacheAsync($"key-{i}", $"value-{i}");
        }

        // Act & Assert
        _provider.CheckCacheFull(10).Should().BeTrue();
        _provider.CheckCacheFull(5).Should().BeTrue();
        _provider.CheckCacheFull(11).Should().BeFalse();
    }

    [Test]
    public async Task RemoveCachesByPattern_ShouldRemoveMatchingKeys()
    {
        // Arrange
        await _provider.SetCacheAsync("order/1", "v1");
        await _provider.SetCacheAsync("order/2", "v2");
        await _provider.SetCacheAsync("user/1", "u1");

        // Act
        await _provider.RemoveCachesByPatternAsync("order/");

        // Assert
        _provider.GetCacheCount().Should().Be(1);
        (await _provider.GetCacheAsync("user/1")).Should().Be("u1");
        (await _provider.GetCacheAsync("order/1")).Should().BeEmpty();
        (await _provider.GetCacheAsync("order/2")).Should().BeEmpty();
    }

    [Test]
    public async Task BuildCacheKey_ShouldConcatenateTypeAndKey()
    {
        // Act
        var key = _provider.BuildCacheKey("TypeName", "123");

        // Assert
        key.Should().Be("TypeName/123");
    }

    [Test]
    public async Task SetCacheAsync_WithExpiry_ShouldRemoveKeyAfterExpiry()
    {
        // Arrange
        var key = "expiring-key";
        await _provider.SetCacheAsync(key, "value", TimeSpan.FromMilliseconds(50));

        // Act
        await Task.Delay(100);
        var result = await _provider.GetCacheAsync(key);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task SetAndGetCacheInBytes_ShouldRoundtripValue()
    {
        // Arrange
        var key = "bytes-key";
        var bytes = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        await _provider.SetCacheAsync(key, bytes, TimeSpan.FromMinutes(1));
        var result = await _provider.GetCacheInBytesAsync(key);

        // Assert
        result.Should().NotBeNull();
        result!.Should().Equal(bytes);
    }

    [Test]
    public async Task KeyExistsAsync_ShouldReflectPresenceOfKey()
    {
        // Arrange
        var key = "exists-key";

        // Act & Assert
        (await _provider.KeyExistsAsync(key)).Should().BeFalse();

        await _provider.SetCacheAsync(key, "value");

        (await _provider.KeyExistsAsync(key)).Should().BeTrue();
    }

    [Test]
    public void ClearCache_ShouldRemoveAllEntries()
    {
        _provider.SetCacheAsync("a", "1").GetAwaiter().GetResult();
        _provider.SetCacheAsync("b", "2").GetAwaiter().GetResult();
        _provider.GetCacheCount().Should().Be(2);

        _provider.ClearCache();

        _provider.GetCacheCount().Should().Be(0);
        _provider.GetCacheAsync("a").GetAwaiter().GetResult().Should().BeEmpty();
    }

    [Test]
    public void CacheOptions_ShouldReturnConfiguredOptions()
    {
        _provider.CacheOptions.Should().NotBeNull();
        _provider.CacheOptions.UseCache.Should().Be("InMemory");
        _provider.CacheOptions.MaxCacheSize.Should().Be(10);
    }

    [Test]
    public void RemoveCachesByPatternAsync_WithDatabaseOverload_ShouldThrowNotSupportedException()
    {
        Func<Task> act = () => _provider.RemoveCachesByPatternAsync("x", null);

        act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("In-memory cache provider does not support multiple databases");
    }

    [Test]
    public void GetDatabase_ShouldThrowNotSupportedException()
    {
        Func<Task> act = () => _provider.GetDatabase(-1);

        act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("In-memory cache provider does not support multiple databases");
    }

    [Test]
    public async Task SetCacheAsync_ShouldOverwrite_WhenKeyAlreadyExists()
    {
        await _provider.SetCacheAsync("same", "v1");
        await _provider.SetCacheAsync("same", "v2");

        (await _provider.GetCacheAsync("same")).Should().Be("v2");
        _provider.GetCacheCount().Should().Be(1);
    }
}

