using CineVault.Application.Abstractions;
using CineVault.Application.Movies.ListMovies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CineVault.Infrastructure.Caching;

public class MemoryMovieCache : IMovieCache
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryMovieCache> _logger;

    public MemoryMovieCache(IMemoryCache memoryCache, ILogger<MemoryMovieCache> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<MovieResponse?> GetAsync(Guid movieId)
    {
        var key = MovieCacheEntry.KeyFor(movieId);
        var found = _memoryCache.TryGetValue(key, out MovieResponse? movie);

        _logger.LogInformation("Memory cache {Outcome} for {Key}", found ? "HIT" : "MISS", key);

        return Task.FromResult(movie);
    }

    public Task SetAsync(MovieResponse movie)
    {
        _memoryCache.Set(MovieCacheEntry.KeyFor(movie.Id), movie, MovieCacheEntry.TimeToLive);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid movieId)
    {
        _memoryCache.Remove(MovieCacheEntry.KeyFor(movieId));

        return Task.CompletedTask;
    }
}
