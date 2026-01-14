using FluentValidation.TestHelper;
using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Tests.DTOs;

public class AuthenticationDtosValidationTests
{
    private readonly LoginRequestValidator _loginValidator;
    private readonly RegisterRequestValidator _registerValidator;

    public AuthenticationDtosValidationTests()
    {
        _loginValidator = new LoginRequestValidator();
        _registerValidator = new RegisterRequestValidator();
    }

    [Fact]
    public void LoginRequest_WithValidData_ShouldNotHaveValidationError()
    {
        // Arrange
        var model = new LoginRequest("validuser", "validpassword");

        // Act
        var result = _loginValidator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void LoginRequest_WithInvalidUsername_ShouldHaveValidationError(string username)
    {
        // Arrange
        var model = new LoginRequest(username, "validpassword");

        // Act
        var result = _loginValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void LoginRequest_WithInvalidPassword_ShouldHaveValidationError(string password)
    {
        // Arrange
        var model = new LoginRequest("validuser", password);

        // Act
        var result = _loginValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterRequest_WithValidData_ShouldNotHaveValidationError()
    {
        // Arrange
        var model = new RegisterRequest("validuser", "ValidPass123");

        // Act
        var result = _registerValidator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData(null)]
    public void RegisterRequest_WithInvalidUsername_ShouldHaveValidationError(string username)
    {
        // Arrange
        var model = new RegisterRequest(username, "ValidPass123");

        // Act
        var result = _registerValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("nouppercase1")]
    [InlineData("NOLOWERCASE1")]
    [InlineData("NoDigit")]
    [InlineData(null)]
    public void RegisterRequest_WithInvalidPassword_ShouldHaveValidationError(string password)
    {
        // Arrange
        var model = new RegisterRequest("validuser", password);

        // Act
        var result = _registerValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}