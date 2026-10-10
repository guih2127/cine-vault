using CineVault.Application.Movies.ListMovies;
using CineVault.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace CineVault.Infrastructure.Tests.Caching;

public class MemoryMovieCacheTests
{
    private readonly MemoryMovieCache _cache = new(
        new MemoryCache(new MemoryCacheOptions()),
        NullLogger<MemoryMovieCache>.Instance);

    private readonly MovieResponse _movie = new(Guid.NewGuid(), "The Matrix", 1999, null);

    [Fact]
    public async Task GetAsync_WhenNotCached_ReturnsNull()
    {
        var cached = await _cache.GetAsync(_movie.Id);

        cached.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_AfterSet_ReturnsMovie()
    {
        await _cache.SetAsync(_movie);

        var cached = await _cache.GetAsync(_movie.Id);

        cached.Should().Be(_movie);
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
