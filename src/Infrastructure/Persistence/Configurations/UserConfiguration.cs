using CineVault.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CineVault.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Name).IsRequired().HasMaxLength(200);
        builder.Property(user => user.Email).IsRequired().HasMaxLength(320);
        builder.Property(user => user.PasswordHash).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
    }
}
