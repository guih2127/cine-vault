using CineVault.Domain.Movies;
using CineVault.Domain.MovieReviews;
using CineVault.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CineVault.Infrastructure.Persistence.Configurations;

public class MovieReviewConfiguration : IEntityTypeConfiguration<MovieReview>
{
    public void Configure(EntityTypeBuilder<MovieReview> builder)
    {
        builder.HasKey(review => review.Id);
        builder.Property(review => review.Rating);
        builder.HasIndex(review => new { review.UserId, review.MovieId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(review => review.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Movie>()
            .WithMany()
            .HasForeignKey(review => review.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
