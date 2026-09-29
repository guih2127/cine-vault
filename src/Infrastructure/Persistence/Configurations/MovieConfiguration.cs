using CineVault.Domain.Movies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CineVault.Infrastructure.Persistence.Configurations;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(movie => movie.Id);
        builder.Property(movie => movie.Title).IsRequired().HasMaxLength(300);
        builder.Property(movie => movie.PosterUrl).HasMaxLength(2048);
    }
}
