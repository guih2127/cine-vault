using System.Text.Json;
using CineVault.Application.Abstractions;
using CineVault.Application.Movies.ListMovies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CineVault.Infrastructure.Caching;

public class RedisMovieCache : IMovieCache
{
    private static readonly DistributedCacheEntryOptions EntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = MovieCacheEntry.TimeToLive
    };

    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisMovieCache> _logger;

    public RedisMovieCache(IDistributedCache distributedCache, ILogger<RedisMovieCache> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<MovieResponse?> GetAsync(Guid movieId)
    {
        var key = MovieCacheEntry.KeyFor(movieId);
        var json = await _distributedCache.GetStringAsync(key);

        _logger.LogInformation("Redis cache {Outcome} for {Key}", json is null ? "MISS" : "HIT", key);

        return json is null ? null : JsonSerializer.Deserialize<MovieResponse>(json);
    }

    public Task SetAsync(MovieResponse movie)
        => _distributedCache.SetStringAsync(
            MovieCacheEntry.KeyFor(movie.Id),
            JsonSerializer.Serialize(movie),
            EntryOptions);

    public Task RemoveAsync(Guid movieId)
        => _distributedCache.RemoveAsync(MovieCacheEntry.KeyFor(movieId));
}
