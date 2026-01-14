namespace MembershipSystemAPI.DTOs;

// API Key Request DTOs
/// <summary>
/// DTO for generating a new API key
/// </summary>
public record GenerateApiKeyRequest();

// API Key Response DTOs
/// <summary>
/// DTO for retrieving an API key
/// </summary>
/// <param name="Key">The API key</param>
/// <param name="CreatedAt">The creation time of the API key</param>
public record GetApiKeyResponse(
    string ApiKey,
    DateTimeOffset CreatedAt
);

/// <summary>
/// DTO for generating an API key response
/// </summary>
/// <param name="Key">The generated API key</param>
/// <param name="CreatedAt">The creation time of the API key</param>
public record GenerateApiKeyResponse(
    string Key,
    DateTimeOffset CreatedAt
);

/// <summary>
/// DTO for API key generated response
/// </summary>
/// <param name="Key">The generated API key</param>
/// <param name="Message">The response message</param>
public record ApiKeyGeneratedResponse(
    string Key,
    string Message = "API密钥已生成"
);