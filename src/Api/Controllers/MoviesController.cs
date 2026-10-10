using CineVault.Api.Contracts;
using CineVault.Api.Extensions;
using CineVault.Application.Movies.GetMovieById;
using CineVault.Application.Movies.ListMovies;
using CineVault.Application.Movies.RenameMovie;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly ListMoviesUseCase _listMoviesUseCase;
    private readonly GetMovieByIdUseCase _getMovieByIdUseCase;
    private readonly RenameMovieUseCase _renameMovieUseCase;

    public MoviesController(
        ListMoviesUseCase listMoviesUseCase,
        GetMovieByIdUseCase getMovieByIdUseCase,
        RenameMovieUseCase renameMovieUseCase)
    {
        _listMoviesUseCase = listMoviesUseCase;
        _getMovieByIdUseCase = getMovieByIdUseCase;
        _renameMovieUseCase = renameMovieUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var movies = await _listMoviesUseCase.Execute();

        return Ok(movies);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getMovieByIdUseCase.Execute(new GetMovieByIdQuery(id));

        return result.ToActionResult();
    }

    [HttpPut("{id:guid}/title")]
    public async Task<IActionResult> Rename(Guid id, RenameMovieRequest request)
    {
        var result = await _renameMovieUseCase.Execute(new RenameMovieCommand(id, request.Title));

        return result.ToActionResult();
    }
}
