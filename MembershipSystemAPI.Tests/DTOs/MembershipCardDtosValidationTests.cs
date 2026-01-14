using FluentValidation.TestHelper;
using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Tests.DTOs;

public class MembershipCardDtosValidationTests
{
    private readonly CreateMembershipRequestValidator _createMembershipValidator;
    private readonly UpdateMembershipCardRequestValidator _updateMembershipCardValidator;

    public MembershipCardDtosValidationTests()
    {
        _createMembershipValidator = new CreateMembershipRequestValidator();
        _updateMembershipValidator = new UpdateMembershipCardRequestValidator();
    }

    [Fact]
    public void CreateMembershipRequest_WithValidData_ShouldNotHaveValidationError()
    {
        // Arrange
        var model = new CreateMembershipRequest
        {
            MembershipName = "Valid Membership",
            DurationInDays = 30,
            Amount = 100,
            StartTime = DateTimeOffset.UtcNow
        };

        // Act
        var result = _createMembershipValidator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateMembershipRequest_WithEmptyMembershipName_ShouldHaveValidationError()
    {
        // Arrange
        var model = new CreateMembershipRequest
        {
            MembershipName = "",
            DurationInDays = 30,
            Amount = 100,
            StartTime = DateTimeOffset.UtcNow
        };

        // Act
        var result = _createMembershipValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MembershipName);
    }

    [Fact]
    public void CreateMembershipRequest_WithZeroDuration_ShouldHaveValidationError()
    {
        // Arrange
        var model = new CreateMembershipRequest
        {
            MembershipName = "Valid Membership",
            DurationInDays = 0,
            Amount = 100,
            StartTime = DateTimeOffset.UtcNow
        };

        // Act
        var result = _createMembershipValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationInDays);
    }

    [Fact]
    public void CreateMembershipRequest_WithNegativeAmount_ShouldHaveValidationError()
    {
        // Arrange
        var model = new CreateMembershipRequest
        {
            MembershipName = "Valid Membership",
            DurationInDays = 30,
            Amount = -100,
            StartTime = DateTimeOffset.UtcNow
        };

        // Act
        var result = _createMembershipValidator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void UpdateMembershipCardRequest_WithValidData_ShouldNotHaveValidationError()
    {
        // Arrange
        var model = new UpdateMembershipCardRequest(
            MembershipName: "Updated Membership",
            Notes: "Updated notes"
        );

        // Act
        var result = _updateMembershipCardValidator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateMembershipCardRequest_WithNullValues_ShouldNotHaveValidationError()
    {
        // Arrange
        var model = new UpdateMembershipCardRequest(
            MembershipName: null,
            Notes: null
        );

        // Act
        var result = _updateMembershipCardValidator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}