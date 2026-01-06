namespace MembershipSystemAPI.DTOs;

// PathConfiguration Request DTOs
/// <summary>
/// DTO for getting path configuration
/// </summary>
/// <param name="UserId">The ID of the user to get path configuration for</param>
public record GetPathConfigurationRequest(
    Guid UserId
);


// PathConfiguration Response DTOs
/// <summary>
/// DTO for path configuration response
/// </summary>
/// <param name="Id">The unique identifier of the path configuration</param>
/// <param name="BasePath">The base path for file operations</param>
/// <param name="MembershipCardFilePath">The file path for membership card storage</param>
/// <param name="AllowCustomPaths">Whether custom paths are allowed</param>
/// <param name="CreatedAt">The creation time of the path configuration</param>
/// <param name="UpdatedAt">The last update time of the path configuration</param>
public record PathConfigurationResponse(
    Guid Id,
    string BasePath,
    string MembershipCardFilePath,
    bool AllowCustomPaths,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);