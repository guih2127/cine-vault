using CineVault.Application.Shared;
using FluentAssertions;

namespace CineVault.Application.Tests.Shared;

public class ResultTests
{
    [Fact]
    public void Success_HoldsValueAndHasNoError()
    {
        var result = Result<string>.Success("token");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("token");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_HoldsErrorAndHasNoValue()
    {
        var error = new Error(ErrorType.Unauthorized, "auth.invalid_credentials", "Invalid email or password.");

        var result = Result<string>.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
        result.Value.Should().BeNull();
    }
}
