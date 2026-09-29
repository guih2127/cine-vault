using CineVault.Application.Abstractions;

namespace CineVault.Application.MovieReviews.ListMovieReviews;

public class ListMovieReviewsUseCase
{
    private readonly IMovieReviewRepository _movieReviewRepository;

    public ListMovieReviewsUseCase(IMovieReviewRepository movieReviewRepository)
    {
        _movieReviewRepository = movieReviewRepository;
    }

    public async Task<IReadOnlyList<MovieReviewResponse>> Execute(ListMovieReviewsQuery query)
    {
        var reviews = await _movieReviewRepository.GetByUserAsync(query.UserId);

        return reviews
            .Select(review => new MovieReviewResponse(review.Id, review.MovieId, review.Rating))
            .ToList();
    }
}
