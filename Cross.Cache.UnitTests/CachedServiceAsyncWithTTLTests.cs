namespace Cross.Cache.UnitTests;

[TestFixture]
public class CachedServiceAsyncWithTTLTests
{
    private CachedServiceAsyncWithTTL _service;

    [SetUp]
    public void SetUp()
    {
        _service = new CachedServiceAsyncWithTTL();
    }

    [Test]
    public async Task GetOrAddAsync_ShouldUseFactoryOnce_ForSameKeyBeforeTtlExpires()
    {
        // Arrange
        var key = "key";
        var calls = 0;

        Task<int> Factory()
        {
            calls++;
            return Task.FromResult(42);
        }

        // Act
        var first = await _service.GetOrAddAsync(key, Factory, TimeSpan.FromMinutes(1));
        var second = await _service.GetOrAddAsync(key, Factory, TimeSpan.FromMinutes(1));

        // Assert
        calls.Should().Be(1);
        first.Should().Be(42);
        second.Should().Be(42);

        var stats = _service.GetStats();
        stats.TotalAccesses.Should().Be(2);
        stats.PerKey.Should().ContainKey(key);
        stats.PerKey[key].Misses.Should().Be(1);
        stats.PerKey[key].Hits.Should().Be(1);
        stats.PerKey[key].Expired.Should().Be(0);
    }

    [Test]
    public async Task GetOrAddAsync_ShouldRecreateValueAndMarkExpired_WhenTtlElapsed()
    {
        // Arrange
        var key = "expired-key";
        var calls = 0;

        Task<int> Factory()
        {
            calls++;
            return Task.FromResult(100 + calls);
        }

        // Act
        var first = await _service.GetOrAddAsync(key, Factory, TimeSpan.FromMilliseconds(50));
        await Task.Delay(100);
        var second = await _service.GetOrAddAsync(key, Factory, TimeSpan.FromMinutes(1));

        // Assert
        calls.Should().Be(2);
        first.Should().Be(101);
        second.Should().Be(102);

        var stats = _service.GetStats();
        stats.TotalAccesses.Should().Be(2);
        stats.PerKey.Should().ContainKey(key);
        stats.PerKey[key].Misses.Should().Be(2);
        stats.PerKey[key].Hits.Should().Be(0);
        stats.PerKey[key].Expired.Should().Be(1);
    }

    [Test]
    public void TryGetValue_ShouldReturnFalseAndIncreaseMisses_WhenKeyIsMissing()
    {
        // Arrange
        var key = "missing";

        // Act
        var result = _service.TryGetValue<int>(key, out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().Be(0);

        var stats = _service.GetStats();
        stats.TotalAccesses.Should().Be(1);
        stats.PerKey.Should().ContainKey(key);
        stats.PerKey[key].Misses.Should().Be(1);
        stats.PerKey[key].Hits.Should().Be(0);
        stats.PerKey[key].Expired.Should().Be(0);
    }

    [Test]
    public async Task Set_ShouldPopulateCache_ForSubsequentGetOrAdd()
    {
        // Arrange
        var key = "set-key";
        _service.Set(key, 77, TimeSpan.FromMinutes(1));
        var calls = 0;

        Task<int> Factory()
        {
            calls++;
            return Task.FromResult(99);
        }

        // Act
        var valueFromCache = await _service.GetOrAddAsync(key, Factory, TimeSpan.FromMinutes(1));

        // Assert
        calls.Should().Be(0);
        valueFromCache.Should().Be(77);

        var stats = _service.GetStats();
        stats.TotalAccesses.Should().Be(1);
        stats.PerKey.Should().ContainKey(key);
        stats.PerKey[key].Misses.Should().Be(0);
        stats.PerKey[key].Hits.Should().Be(1);
        stats.PerKey[key].Expired.Should().Be(0);
    }

    [Test]
    public async Task TryGetValue_ShouldReturnTrueAndValue_WhenKeyExistsAndNotExpired()
    {
        var key = "hit-key";
        await _service.GetOrAddAsync(key, () => Task.FromResult(123), TimeSpan.FromMinutes(1));

        var result = _service.TryGetValue<int>(key, out var value);

        result.Should().BeTrue();
        value.Should().Be(123);
        _service.GetStats().PerKey[key].Hits.Should().Be(1);
    }

    [Test]
    public async Task TryGetValue_ShouldReturnFalseAndIncrementExpired_WhenEntryExpired()
    {
        var key = "expired-try";
        await _service.GetOrAddAsync(key, () => Task.FromResult(456), TimeSpan.FromMilliseconds(50));
        await Task.Delay(100);

        var result = _service.TryGetValue<int>(key, out var value);

        result.Should().BeFalse();
        value.Should().Be(0);
        _service.GetStats().PerKey[key].Expired.Should().Be(1);
    }
}

