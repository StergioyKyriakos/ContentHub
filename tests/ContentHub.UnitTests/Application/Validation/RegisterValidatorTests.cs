using ContentHub.Api.Features.Auth.Register;
using FluentAssertions;

namespace ContentHub.UnitTests.Application.Validation;

public sealed class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_For_Valid_Command()
    {
        var command = new RegisterCommand
        {
            Email = "test@contenthub.local",
            Username = "testuser",
            DisplayName = "testuser",
            Password = "Password123!"
            
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_Email_Is_Invalid()
    {
        var command = new RegisterCommand
        {
            Email = "not-an-email",
            Username = "testuser",
            DisplayName = "testuser",
            Password = "Password123!"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterCommand.Email));
    }

    [Fact]
    public void Validate_Should_Fail_When_Password_Is_Too_Short()
    {
        var command = new RegisterCommand
        {
            Email = "test@contenthub.local",
            Username = "testuser",
            DisplayName = "testuser",
            Password = "123"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterCommand.Password));
    }
}