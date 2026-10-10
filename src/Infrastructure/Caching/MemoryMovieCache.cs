using CineVault.Application.Abstractions;
using CineVault.Application.Movies.ListMovies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CineVault.Infrastructure.Caching;

public class MemoryMovieCache : IMovieCache
{
    private static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(10);

    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryMovieCache> _logger;

    public MemoryMovieCache(IMemoryCache memoryCache, ILogger<MemoryMovieCache> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<MovieResponse?> GetAsync(Guid movieId)
    {
        var key = KeyFor(movieId);
        var found = _memoryCache.TryGetValue(key, out MovieResponse? movie);

        _logger.LogInformation("Memory cache {Outcome} for {Key}", found ? "HIT" : "MISS", key);

        return Task.FromResult(movie);
    }

    public Task SetAsync(MovieResponse movie)
    {
        _memoryCache.Set(KeyFor(movie.Id), movie, TimeToLive);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid movieId)
    {
        _memoryCache.Remove(KeyFor(movieId));

        return Task.CompletedTask;
    }

    private static string KeyFor(Guid movieId) => $"movie:{movieId}";
}
