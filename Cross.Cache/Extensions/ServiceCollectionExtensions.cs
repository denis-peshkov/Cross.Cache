namespace Cross.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var cacheOptions = configuration.GetSection(nameof(CacheOptions));
        services.Configure<CacheOptions>(cacheOptions);

        var useCache = configuration["CacheOptions:UseCache"];

        switch (useCache)
        {
            case "InMemory":
                services.TryAddSingleton<ICacheProvider, CacheInMemoryProvider>();
                break;

            case "InRedis":
                services.AddStackExchangeRedisCache(
                    options =>
                    {
                        options.Configuration = cacheOptions["CacheInRedis:ConnectionString"];
                        options.ConfigurationOptions = new ConfigurationOptions
                        {
                            AsyncTimeout = 6000,
                            SyncTimeout = 6000,
                        };
                    });
                services.TryAddSingleton<ICacheProvider, CacheInRedisProvider>();
                break;

            default:
                throw new ApplicationException("Cache module registration error: invalid configuration.");
        }

        return services;
    }
}
