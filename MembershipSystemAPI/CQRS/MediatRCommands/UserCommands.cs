namespace MembershipSystemAPI.CQRS.MediatRCommands;

// 用户相关的命令
public record CreateUserCommand(
    string Username,
    string Password,
    string Role,
    string? CreatedBy = null
) : IRequest<CreateUserResult>;

public record UpdateUserCommand(
    Guid UserId,
    string? Password = null,
    bool? IsActive = null,
    string? UpdatedBy = null
) : IRequest<bool>;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword,
    string? UpdatedBy = null
) : IRequest<bool>;

public record LockUserCommand(
    Guid UserId,
    DateTimeOffset LockoutEnd,
    string? UpdatedBy = null
) : IRequest<bool>;

// command results
public record CreateUserResult(
    Guid UserId,
    string Username,
    bool Success,
    string Message = ""
);

// Registration-specific command
public record RegisterUserCommand(
    string Username,
    string Password
) : IRequest<RegisterUserResult>;

public record RegisterUserResult(
    Guid UserId,
    string Username,
    bool Success,
    string Message = ""
);

// Authentication command for login
public record AuthenticateUserCommand(
    string Username,
    string Password,
    string IpAddress
) : IRequest<AuthenticateUserResult>;

public record AuthenticateUserResult
{
    public bool Success { get; init; }
    public Guid? UserId { get; init; }
    public string? Username { get; init; }
    public string? Role { get; init; }
    public string? Message { get; init; }
    public bool IsLocked { get; init; }
    public bool IsNotActive { get; init; }
    public bool IsInvalidCredentials { get; init; }

    public static AuthenticateUserResult InvalidCredentials => new()
    {
        Success = false,
        IsInvalidCredentials = true,
        Message = "用户名或密码错误"
    };

    public static AuthenticateUserResult AccountLocked => new()
    {
        Success = false,
        IsLocked = true,
        Message = "账户被锁定"
    };

    public static AuthenticateUserResult AccountNotActive => new()
    {
        Success = false,
        IsNotActive = true,
        Message = "账户未激活"
    };

    public static AuthenticateUserResult SuccessResult(Guid userId, string username, string role) => new()
    {
        Success = true,
        UserId = userId,
        Username = username,
        Role = role,
        Message = "登录成功"
    };
}
// Admin-specific commands
public record ChangeUserPasswordCommand(
    Guid UserId,
    string NewPassword,
    string? UpdatedBy = null
) : IRequest<ChangeUserPasswordResult>;

public record ChangeUserPasswordResult(
    bool Success,
    string Message = ""
);

public record DeleteUserCommand(
    Guid UserId
) : IRequest<DeleteUserResult>;

public record DeleteUserResult(
    bool Success,
    string Message = ""
);

public record ToggleStatusCommand(
    Guid UserId,
    bool IsActive
) : IRequest<ToggleStatusResult>;

public record ToggleStatusResult(
    bool Success,
    string Message = ""
);
