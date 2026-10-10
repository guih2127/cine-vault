using CineVault.Application.Movies.GetMovieById;
using CineVault.Application.Movies.ListMovies;
using CineVault.Application.Movies.RenameMovie;
using CineVault.Application.MovieReviews.CreateMovieReview;
using CineVault.Application.MovieReviews.DeleteMovieReview;
using CineVault.Application.MovieReviews.ListMovieReviews;
using CineVault.Application.MovieReviews.UpdateMovieReview;
using CineVault.Application.Users.Login;
using CineVault.Application.Users.RegisterUser;
using Microsoft.Extensions.DependencyInjection;

namespace CineVault.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterUserUseCase>();
        services.AddScoped<LoginUseCase>();
        services.AddScoped<CreateMovieReviewUseCase>();
        services.AddScoped<UpdateMovieReviewUseCase>();
        services.AddScoped<DeleteMovieReviewUseCase>();
        services.AddScoped<ListMovieReviewsUseCase>();
        services.AddScoped<ListMoviesUseCase>();
        services.AddScoped<GetMovieByIdUseCase>();
        services.AddScoped<RenameMovieUseCase>();

        return services;
    }
}
