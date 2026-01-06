using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Repositories;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.QueryHandlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        User? user = null;
        if (query.IncludeDetails)
        {
            user = await _userRepository.GetByIdWithDetailsAsync(query.UserId);
        }
        else
        {
            user = await _userRepository.GetByIdAsync(query.UserId);
        }

        if (user == null)
            return null;

        return new UserDto(user.Id, user.Username, user.IsActive, user.Role, user.CreatedAt, user.LastLoginAt);
    }
}

public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByUsernameQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByUsernameQuery query, CancellationToken cancellationToken)
    {
        User? user = null;
        if (query.IncludeApiKey)
        {
            user = await _userRepository.GetByUsernameWithApiKeyAsync(query.Username);
        }
        else
        {
            user = await _userRepository.GetByUsernameAsync(query.Username);
        }

        if (user == null)
            return null;

        return new UserDto(user.Id, user.Username, user.IsActive, user.Role, user.CreatedAt, user.LastLoginAt);
    }
}

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        IEnumerable<User> users;
        if (query.OnlyActive)
        {
            users = await _userRepository.GetActiveUsersAsync();
        }
        else
        {
            users = await _userRepository.GetAllAsync();
        }

        return users.Select(user => new UserDto(Id: user.Id, Username: user.Username, IsActive: user.IsActive, Role: user.Role, CreateAt: user.CreatedAt, LastLoginAt: user.LastLoginAt));
    }
}

public class ValidateUserCredentialsQueryHandler : IRequestHandler<ValidateUserCredentialsQuery, ValidateUserResult>
{
    private readonly IUserRepository _userRepository;

    public ValidateUserCredentialsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ValidateUserResult> Handle(ValidateUserCredentialsQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(query.Username);

        if (user == null)
        {
            return new ValidateUserResult(null!, false, "用户名或密码错误");
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            return new ValidateUserResult(null!, false, "用户已锁定", true);
        }

        if (!user.IsActive)
        {
            return new ValidateUserResult(null!, false, "用户未激活");
        }

        if (!BCrypt.Net.BCrypt.Verify(query.Password, user.PasswordHash))
        {
            return new ValidateUserResult(user, false, "用户名或密码错误");
        }

        return new ValidateUserResult(user, true, "验证成功");
    }
}