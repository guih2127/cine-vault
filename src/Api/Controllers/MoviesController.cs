using CineVault.Application.Movies.ListMovies;
using Microsoft.AspNetCore.Mvc;

namespace CineVault.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly ListMoviesUseCase _listMoviesUseCase;

    public MoviesController(ListMoviesUseCase listMoviesUseCase)
    {
        _listMoviesUseCase = listMoviesUseCase;
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var movies = await _listMoviesUseCase.Execute();

        return Ok(movies);
    }
}
