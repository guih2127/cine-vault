namespace CineVault.Api.Contracts;

public record CreateReviewRequest(Guid MovieId, int? Rating);
