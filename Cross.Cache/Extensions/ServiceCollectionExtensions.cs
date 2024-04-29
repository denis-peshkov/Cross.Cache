namespace Cross.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCacheService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CacheOptions>(configuration.GetSection(nameof(CacheOptions)));

        var useCache = configuration["CacheOptions:UseCache"];

        switch (useCache)
        {
            case "InMemory":
                services.TryAddSingleton<ICacheService, CacheInMemoryService>();
                break;

            case "InRedis":
                services.AddStackExchangeRedisCache(
                    options =>
                    {
                        options.Configuration = configuration["CacheOptions:CacheInRedisSetting:ConnectionStringRedis"];
                        options.ConfigurationOptions = new ConfigurationOptions
                        {
                            AsyncTimeout = 6000,
                            SyncTimeout = 6000,
                        };
                    });
                services.TryAddSingleton<ICacheService, CacheInRedisService>();
                break;

            default:
                throw new ApplicationException("Ошибка регистрации модуля кэша: неверная конфигурация.");
        }

        return services;
    }
}
