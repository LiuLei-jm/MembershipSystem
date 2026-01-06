using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.Domain.Enums;
using MembershipSystemAPI.Domain.Services;
using MembershipSystemAPI.Repositories;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.CommandHandlers;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateUserResult> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // 检查用户名是否已存在
        var existingUser = await _unitOfWork.Users.GetByUsernameAsync(command.Username);
        if (existingUser != null)
        {
            return new CreateUserResult(Guid.Empty, command.Username, false, "用户名已存在");
        }

        var Id = Guid.NewGuid();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
        var user = User.Create(command.Username, passwordHash, Enum.TryParse<UserRole>(command.Role, true, out var role) ? role : UserRole.User);
        while (!await _unitOfWork.ApiKeys.IsKeyUniqueAsync(user.ApiKey!.Key))
        {
            user.ApiKey!.RegenerateKey();
        }

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new CreateUserResult(user.Id, user.Username, true, "用户创建成功");
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(command.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
        }

        if (command.IsActive.HasValue)
        {
            user.IsActive = command.IsActive.Value;
        }

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(command.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword);
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}

public class LockUserCommandHandler : IRequestHandler<LockUserCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public LockUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(LockUserCommand command, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Users.LockUserAsync(command.UserId, command.LockoutEnd);
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // 检查用户名是否已存在
        var existingUser = await _unitOfWork.Users.GetByUsernameAsync(command.Username);
        if (existingUser != null)
        {
            return new RegisterUserResult(Guid.Empty, command.Username, false, "用户名已存在");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
        var user = User.Create(command.Username, passwordHash, UserRole.User);
        while (!await _unitOfWork.ApiKeys.IsKeyUniqueAsync(user.ApiKey!.Key))
        {
            user.ApiKey!.RegenerateKey();
        }
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new RegisterUserResult(user.Id, user.Username, true, "注册成功");
    }
}

public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, AuthenticateUserResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserDomainService _userDomainService;
    private readonly ILogger<AuthenticateUserCommandHandler> _logger;

    public AuthenticateUserCommandHandler(IUnitOfWork unitOfWork, IUserDomainService userDomainService, ILogger<AuthenticateUserCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userDomainService = userDomainService;
        _logger = logger;
    }

    public async Task<AuthenticateUserResult> Handle(AuthenticateUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"用户登录尝试: {command.Username}", command.IpAddress);

        var user = await _unitOfWork.Users.GetByUsernameAsync(command.Username);
        if (user == null)
        {
            _logger.LogWarning($"登录失败: 用户不存在. Username: {command.Username}, IP: {command.IpAddress}");
            return AuthenticateUserResult.InvalidCredentials;
        }

        // 验证用户状态
        if (!user.IsActive)
        {
            _logger.LogWarning($"登录失败: 账户未激活. Username: {command.Username}, UserId: {user.Id}");
            return AuthenticateUserResult.AccountNotActive;
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            _logger.LogWarning($"登录失败: 账户被锁定. Username: {command.Username}, UserId: {user.Id}, LockoutEnd: {user.LockoutEnd}");
            return AuthenticateUserResult.AccountLocked;
        }

        // 验证密码
        if (!BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
        {
            _logger.LogWarning($"登录失败: 密码错误. Username: {command.Username}, UserId: {user.Id}");
            // 增加失败次数
            await _userDomainService.IncrementAccessFailedCountAsync(user.Id);
            return AuthenticateUserResult.InvalidCredentials;
        }

        // 登录成功，更新相关信息
        await _userDomainService.UpdateLastLoginAsync(user.Id);
        await _userDomainService.UnlockUserAsync(user.Id); // 清除任何锁定状态

        _logger.LogInformation($"登录成功: {command.Username}, UserId: {user.Id}");
        return AuthenticateUserResult.SuccessResult(user.Id, user.Username, user.Role);
    }
}