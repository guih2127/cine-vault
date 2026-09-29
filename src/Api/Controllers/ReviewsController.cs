using CineVault.Api.Contracts;
using CineVault.Api.Extensions;
using CineVault.Application.MovieReviews.CreateMovieReview;
using CineVault.Application.MovieReviews.DeleteMovieReview;
using CineVault.Application.MovieReviews.ListMovieReviews;
using CineVault.Application.MovieReviews.UpdateMovieReview;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly CreateMovieReviewUseCase _createMovieReviewUseCase;
    private readonly UpdateMovieReviewUseCase _updateMovieReviewUseCase;
    private readonly DeleteMovieReviewUseCase _deleteMovieReviewUseCase;
    private readonly ListMovieReviewsUseCase _listMovieReviewsUseCase;

    public ReviewsController(
        CreateMovieReviewUseCase createMovieReviewUseCase,
        UpdateMovieReviewUseCase updateMovieReviewUseCase,
        DeleteMovieReviewUseCase deleteMovieReviewUseCase,
        ListMovieReviewsUseCase listMovieReviewsUseCase)
    {
        _createMovieReviewUseCase = createMovieReviewUseCase;
        _updateMovieReviewUseCase = updateMovieReviewUseCase;
        _deleteMovieReviewUseCase = deleteMovieReviewUseCase;
        _listMovieReviewsUseCase = listMovieReviewsUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var reviews = await _listMovieReviewsUseCase.Execute(new ListMovieReviewsQuery(User.GetUserId()));

        return Ok(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReviewRequest request)
    {
        var result = await _createMovieReviewUseCase.Execute(
            new CreateMovieReviewCommand(User.GetUserId(), request.MovieId, request.Rating));

        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateReviewRequest request)
    {
        var result = await _updateMovieReviewUseCase.Execute(
            new UpdateMovieReviewCommand(id, User.GetUserId(), request.Rating));

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _deleteMovieReviewUseCase.Execute(
            new DeleteMovieReviewCommand(id, User.GetUserId()));

        return result.ToActionResult();
    }
}
