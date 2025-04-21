namespace Cross.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheService(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(nameof(CacheOptions));
        services.Configure<CacheOptions>(section);

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
                        options.Configuration = section["CacheInRedis:ConnectionString"];
                        options.ConfigurationOptions = new ConfigurationOptions
                        {
                            AsyncTimeout = 6000,
                            SyncTimeout = 6000,
                        };
                    });
                services.TryAddSingleton<ICacheProvider, CacheInRedisProvider>();
                break;

            default:
                throw new ApplicationException("Ошибка регистрации модуля кэша: неверная конфигурация.");
        }

        return services;
    }
}
