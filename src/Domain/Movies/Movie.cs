using CineVault.Domain.Shared;

namespace CineVault.Domain.Movies;

public class Movie
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public int Year { get; private set; }
    public string? PosterUrl { get; private set; }

    private Movie(Guid id, string title, int year, string? posterUrl)
    {
        Id = id;
        Title = title;
        Year = year;
        PosterUrl = posterUrl;
    }

    public static Movie Create(string title, int year, string? posterUrl)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Movie title is required.");
        }

        return new Movie(Guid.NewGuid(), title.Trim(), year, posterUrl);
    }
}
