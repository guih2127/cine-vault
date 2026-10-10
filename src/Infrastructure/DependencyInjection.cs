using CineVault.Application.Abstractions;
using CineVault.Infrastructure.Caching;
using CineVault.Infrastructure.Persistence;
using CineVault.Infrastructure.Persistence.Repositories;
using CineVault.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CineVault.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CineVaultDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IMovieReviewRepository, MovieReviewRepository>();

        services.AddMemoryCache();
        services.AddSingleton<IMovieCache, MemoryMovieCache>();

        services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        services.AddSingleton(jwtOptions);
        services.AddSingleton<ITokenService, JwtTokenService>();

        return services;
    }
}
