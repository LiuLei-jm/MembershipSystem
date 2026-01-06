namespace MembershipSystemAPI.DTOs;

// Authentication Request DTOs
/// <summary>
/// DTO for user login request
/// </summary>
/// <param name="Username">The username of the user</param>
/// <param name="Password">The password of the user</param>
public record LoginRequest(
    string Username,
    string Password
);

/// <summary>
/// DTO for user registration request
/// </summary>
/// <param name="Username">The username for the new user</param>
/// <param name="Password">The password for the new user</param>
public record RegisterRequest(
    string Username,
    string Password
);

// Authentication Response DTOs

/// <summary>
/// DTO for login response
/// </summary>
/// <param name="Token">The authentication token</param>
/// <param name="Message">The response message</param>
public record LoginResponse(
    string Token,
    string Message = ""
);
/// <summary>
/// DTO for authentication login response
/// </summary>
/// <param name="Token">The authentication token</param>
/// <param name="Message">The response message</param>
public record AuthenticationLoginResponse(
    string Token,
    string Message = ""
);

/// <summary>
/// DTO for user registration response
/// </summary>
/// <param name="UserId">The unique identifier of the registered user</param>
/// <param name="Username">The username of the registered user</param>
/// <param name="Message">The response message</param>
public record RegisterResponse(
    Guid UserId,
    string Username,
    string Message = ""
);