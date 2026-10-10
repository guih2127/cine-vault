namespace CineVault.Infrastructure.Caching;

internal static class MovieCacheEntry
{
    public static readonly TimeSpan TimeToLive = TimeSpan.FromMinutes(10);

    public static string KeyFor(Guid movieId) => $"movie:{movieId}";
}
