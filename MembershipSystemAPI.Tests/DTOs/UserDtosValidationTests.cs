using FluentValidation.TestHelper;
using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Tests.DTOs;

public class UserDtosValidationTests
{
    private readonly CreateUserRequestValidator _createUserValidator;
    private readonly UpdateUserRequestValidator _updateUserValidator;
    private readonly ChangePasswordRequestValidator _changePasswordValidator;

    public UserDtosValidationTests()
    {
        _createUserValidator = new CreateUserRequestValidator();
        _updateUserValidator = new UpdateUserRequestValidator();
        _changePasswordValidator = new ChangePasswordRequestValidator();
    }

    [Fact]
    public void CreateUserRequest_WithValidData_ShouldNotHaveValidationError()
    {
        // Arrange
        var model = new CreateUserRequest("validuser", "ValidPass123");

        // Act
        var result = _createUserValidator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData(null)]
    public void CreateUserRequest_WithInvalidUsername_ShouldHaveValidationError(string username)
    {
        // Arrange
        var model = new CreateUserRequest(username, "ValidPass123");

        // Act
        var result = _createUserValidator.TestValidate(model);

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
    public void CreateUserRequest_WithInvalidPassword_ShouldHaveValidationError(string password)
    {
        // Arrange
        var model = new CreateUserRequest("validuser", password);

        // Act
        var result = _createUserValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void UpdateUserRequest_WithValidPassword_ShouldNotHaveValidationError()
    {
        // Arrange
        var model = new UpdateUserRequest(Password: "ValidPass123");

        // Act
        var result = _updateUserValidator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("short")]
    [InlineData("nouppercase1")]
    [InlineData("NOLOWERCASE1")]
    [InlineData("NoDigit")]
    public void UpdateUserRequest_WithInvalidPassword_ShouldHaveValidationError(string password)
    {
        // Arrange
        var model = new UpdateUserRequest(Password: password);

        // Act
        var result = _updateUserValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ChangePasswordRequest_WithValidData_ShouldNotHaveValidationError()
    {
        // Arrange
        var model = new ChangePasswordRequest("CurrentPass123", "NewPass456");

        // Act
        var result = _changePasswordValidator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ChangePasswordRequest_WithSameCurrentAndNewPassword_ShouldHaveValidationError()
    {
        // Arrange
        var password = "SamePass123";
        var model = new ChangePasswordRequest(password, password);

        // Act
        var result = _changePasswordValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ChangePasswordRequest_WithEmptyCurrentPassword_ShouldHaveValidationError()
    {
        // Arrange
        var model = new ChangePasswordRequest("", "NewPass456");

        // Act
        var result = _changePasswordValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }
}