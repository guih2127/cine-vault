using CineVault.Domain.Shared;

namespace CineVault.Domain.MovieReviews;

public class MovieReview
{
    public const int MinRating = 0;
    public const int MaxRating = 10;

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MovieId { get; private set; }
    public int? Rating { get; private set; }

    private MovieReview(Guid id, Guid userId, Guid movieId, int? rating)
    {
        Id = id;
        UserId = userId;
        MovieId = movieId;
        Rating = rating;
    }

    public static MovieReview Create(Guid userId, Guid movieId, int? rating = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("A user is required.");
        }

        if (movieId == Guid.Empty)
        {
            throw new DomainException("A movie is required.");
        }

        if (rating.HasValue)
        {
            EnsureValidRating(rating.Value);
        }

        return new MovieReview(Guid.NewGuid(), userId, movieId, rating);
    }

    public void Rate(int rating)
    {
        EnsureValidRating(rating);
        Rating = rating;
    }

    private static void EnsureValidRating(int rating)
    {
        if (rating < MinRating || rating > MaxRating)
        {
            throw new DomainException($"Rating must be between {MinRating} and {MaxRating}.");
        }
    }
}
