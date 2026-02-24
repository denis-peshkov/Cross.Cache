![CI](https://github.com/LuckyPennySoftware/MediatR/workflows/CI/badge.svg)
[![MyGet (dev)](https://img.shields.io/myget/mediatr-ci/v/MediatR.svg)](https://myget.org/gallery/mediatr-ci)

[![Nuget](https://img.shields.io/nuget/dt/Cross.Cache.svg)](https://nuget.org/packages/Cross.Cache/)
[![Nuget](https://img.shields.io/nuget/v/Cross.Cache.svg)](https://nuget.org/packages/Cross.Cache/) 
[![Documentation](https://img.shields.io/badge/docs-wiki-yellow.svg)](https://github.com/denis-peshkov/Cross.Cache/wiki)

# Cross.Cache
A flexible and efficient caching library for .NET applications that provides:

### Key Features
- Multiple cache provider support (In-Memory and Redis)
- Easy integration with dependency injection
- Asynchronous operations with TTL support
- Built-in monitoring and metrics (OpenTelemetry integration)
- Thread-safe operations
- Configurable through standard .NET configuration system

### Storage Options
- In-Memory caching for single-server scenarios
- Redis support for distributed caching
- Automatic failover and connection management for Redis

### Performance
- Thread-safe concurrent operations
- Optimized for high-performance scenarios
- Efficient memory usage

### Integration
- Simple setup through extension methods
- Compatible with .NET 7.0 and higher
- Built-in Microsoft DI container support

### Monitoring
- Detailed cache statistics
- OpenTelemetry metrics support
- Per-key performance tracking

This library is designed to provide a robust caching solution for both simple and complex .NET applications, offering flexibility in cache storage choices while maintaining high performance and reliability.

## Roadmap:

- Implement functionality of `SlidingExpiration` from `DistributedCacheEntryOptions options`
