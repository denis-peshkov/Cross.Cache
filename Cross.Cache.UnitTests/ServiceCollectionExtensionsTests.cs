namespace Cross.Cache.UnitTests;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddCacheProvider_ShouldRegisterInMemoryProvider_WhenUseCacheIsInMemory()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CacheOptions:UseCache"] = "InMemory",
            })
            .Build();

        // Act
        services.AddCacheProvider(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var cacheProvider = provider.GetRequiredService<ICacheProvider>();
        cacheProvider.Should().BeOfType<CacheInMemoryProvider>();
    }

    [Test]
    public void AddCacheProvider_ShouldRegisterRedisProvider_WhenUseCacheIsInRedis()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CacheOptions:UseCache"] = "InRedis",
                ["CacheOptions:CacheInRedis:ConnectionString"] = "localhost:6379"
            })
            .Build();

        // Act
        services.AddCacheProvider(configuration);

        // Assert
        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ICacheProvider) &&
            descriptor.ImplementationType == typeof(CacheInRedisProvider));
    }

    [Test]
    public void AddCacheProvider_ShouldThrow_WhenUseCacheIsInvalid()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CacheOptions:UseCache"] = "UnknownCache"
            })
            .Build();

        // Act
        Action act = () => services.AddCacheProvider(configuration);

        // Assert
        act.Should().Throw<ApplicationException>()
            .WithMessage("Cache module registration error: invalid configuration.");
    }
}

