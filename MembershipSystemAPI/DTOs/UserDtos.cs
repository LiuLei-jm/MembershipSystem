namespace MembershipSystemAPI.DTOs;


// User Request DTOs
/// <summary>
/// DTO for creating a new user
/// </summary>
/// <param name="Username">The username for the new user</param>
/// <param name="Password">The password for the new user</param>
/// <param name="Role">The role of the user (default is "User")</param>
public record CreateUserRequest(
    string Username,
    string Password,
    string Role = "User"
);

/// <summary>
/// DTO for updating an existing user
/// </summary>
/// <param name="Password">The new password for the user (optional)</param>
/// <param name="IsActive">The active status of the user (optional)</param>
public record UpdateUserRequest(
    string? Password = null,
    bool? IsActive = null
);
/// <summary>
/// DTO for changing a user's password
/// </summary>
/// <param name="CurrentPassword">The user's current password</param>
/// <param name="NewPassword">The new password to set</param>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
/// <summary>
/// DTO for updating path configuration
/// </summary>
public class UpdatePathConfigurationRequest
{
    /// <summary>
    /// The base path for file operations
    /// </summary>
    public string BasePath { get; set; } = "D:";

    /// <summary>
    /// The file path for membership card storage
    /// </summary>
    public string MembershipCardFilePath { get; set; } = "CDK.txt";

    /// <summary>
    /// Whether custom paths are allowed
    /// </summary>
    public bool AllowCustomPaths { get; set; } = true;

    /// <summary>
    /// Validates the path configuration
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BasePath))
            throw new ArgumentException("Base path cannot be empty", nameof(BasePath));

        if (string.IsNullOrWhiteSpace(MembershipCardFilePath))
            throw new ArgumentException("Membership card file path cannot be empty", nameof(MembershipCardFilePath));

        // Validate file name doesn't contain invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        if (MembershipCardFilePath.Any(c => invalidChars.Contains(c)))
            throw new ArgumentException("Membership card file path contains invalid characters", nameof(MembershipCardFilePath));
    }
}

// Admin Request DTOs
/// <summary>
/// DTO for an admin changing a user's password
/// </summary>
/// <param name="UserId">The ID of the user whose password is being changed</param>
/// <param name="NewPassword">The new password to set for the user</param>
public record ChangeUserPasswordRequest
(
    Guid UserId,
     string NewPassword = ""
);

/// <summary>
/// DTO for an admin deleting a user
/// </summary>
/// <param name="UserId">The ID of the user to delete</param>
public record DeleteUserRequest
(
     Guid UserId);

/// <summary>
/// DTO for an admin toggling a user's active status
/// </summary>
/// <param name="UserId">The ID of the user whose status is being toggled</param>
/// <param name="IsActive">The new active status for the user</param>
public record ToggleStatusRequest
(
     Guid UserId,
     bool IsActive
);



// User Response DTOs

/// <summary>
/// DTO for user summary information
/// </summary>
/// <param name="Id">The unique identifier of the user</param>
/// <param name="Username">The username of the user</param>
/// <param name="Role">The role of the user</param>
/// <param name="IsActive">Whether the user account is active</param>
public record UserSummaryResponse(
    Guid Id,
    string Username,
    string Role,
    bool IsActive
);

/// <summary>
/// DTO for change password response
/// </summary>
/// <param name="Message">The response message</param>
public record ChangePasswordResponse(
    string? Message
    );

/// <summary>
/// DTO for validation result
/// </summary>
/// <param name="IsValid">Whether the validation was successful</param>
/// <param name="Message">The validation message</param>
/// <param name="IsLockedOut">Whether the user is locked out</param>
public record ValidationResult(
    bool IsValid,
    string Message = "",
    bool IsLockedOut = false
);

// User Response DTOs
/// <summary>
/// DTO for detailed user information
/// </summary>
public record UserDto(Guid Id, string Username, bool IsActive, string Role, DateTimeOffset CreateAt, DateTimeOffset? LastLoginAt);


// Admin Response DTOs
/// <summary>
/// DTO for admin change user password response
/// </summary>
/// <param name="Success">Whether the password change was successful</param>
/// <param name="Message">The response message</param>
public record ChangeUserPasswordResponse
(
    bool Success,
     string Message = ""
);
/// <summary>
/// DTO for admin delete user response
/// </summary>
/// <param name="UserId">The ID of the user that was deleted</param>
/// <param name="Message">The response message</param>
public record DeleteUserResponse
(
     Guid UserId,
     string Message = ""
);
/// <summary>
/// DTO for admin toggle user status response
/// </summary>
/// <param name="UserId">The ID of the user whose status was toggled</param>
/// <param name="IsActive">The new active status of the user</param>
/// <param name="Message">The response message</param>
public record ToggleStatusResponse
(
     Guid UserId,
     bool IsActive,
     string Message
);

