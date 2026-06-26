[![License](https://img.shields.io/github/license/denis-peshkov/Cross.Cache)](LICENSE)
[![GitHub Release Date](https://img.shields.io/github/release-date/denis-peshkov/Cross.Cache?label=released)](https://github.com/denis-peshkov/Cross.Cache/releases)
[![NuGetVersion](https://img.shields.io/nuget/v/Cross.Cache.svg)](https://nuget.org/packages/Cross.Cache/)
[![NugetDownloads](https://img.shields.io/nuget/dt/Cross.Cache.svg)](https://nuget.org/packages/Cross.Cache/)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Cross.Cache&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Cross.Cache)
[![issues](https://img.shields.io/github/issues/denis-peshkov/Cross.Cache)](https://github.com/denis-peshkov/Cross.Cache/issues)
[![.NET PR](https://github.com/denis-peshkov/Cross.Cache/actions/workflows/dotnet.yml/badge.svg?event=pull_request)](https://github.com/denis-peshkov/Cross.Cache/actions/workflows/dotnet.yml)

![Size](https://img.shields.io/github/repo-size/denis-peshkov/Cross.Cache)
[![GitHub contributors](https://img.shields.io/github/contributors/denis-peshkov/Cross.Cache)](https://github.com/denis-peshkov/Cross.Cache/contributors)
[![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/denis-peshkov/Cross.Cache/latest?label=new+commits)](https://github.com/denis-peshkov/Cross.Cache/commits/master)
![Activity](https://img.shields.io/github/commit-activity/w/denis-peshkov/Cross.Cache)
![Activity](https://img.shields.io/github/commit-activity/m/denis-peshkov/Cross.Cache)
![Activity](https://img.shields.io/github/commit-activity/y/denis-peshkov/Cross.Cache)

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
