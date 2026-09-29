using CineVault.Domain.Shared;

namespace CineVault.Domain.Movies;

public class Movie
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public string? PosterUrl { get; private set; }

    private Movie(Guid id, string title, string? posterUrl)
    {
        Id = id;
        Title = title;
        PosterUrl = posterUrl;
    }

    public static Movie Create(string title, string? posterUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Movie title is required.");
        }

        return new Movie(Guid.NewGuid(), title.Trim(), posterUrl);
    }
}
