using CineVault.Application.Movies.ListMovies;
using CineVault.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CineVault.Infrastructure.Tests.Caching;

public class RedisMovieCacheTests
{
    private readonly MemoryDistributedCache _distributedCache = new(Options.Create(new MemoryDistributedCacheOptions()));
    private readonly RedisMovieCache _cache;

    private readonly MovieResponse _movie = new(Guid.NewGuid(), "The Matrix", 1999, "https://posters/matrix.jpg");

    public RedisMovieCacheTests()
        => _cache = new RedisMovieCache(_distributedCache, NullLogger<RedisMovieCache>.Instance);

    [Fact]
    public async Task GetAsync_WhenNotCached_ReturnsNull()
    {
        var cached = await _cache.GetAsync(_movie.Id);

        cached.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_AfterSet_ReturnsDeserializedMovie()
    {
        await _cache.SetAsync(_movie);

        var cached = await _cache.GetAsync(_movie.Id);

        cached.Should().Be(_movie);
    }

    [Fact]
    public async Task SetAsync_StoresJsonUnderMovieKey()
    {
        await _cache.SetAsync(_movie);

        var json = await _distributedCache.GetStringAsync($"movie:{_movie.Id}");

        json.Should().Contain("\"Title\":\"The Matrix\"");
    }

    [Fact]
    public async Task GetAsync_AfterRemove_ReturnsNull()
    {
        await _cache.SetAsync(_movie);

        await _cache.RemoveAsync(_movie.Id);

        var cached = await _cache.GetAsync(_movie.Id);
        cached.Should().BeNull();
    }
}
