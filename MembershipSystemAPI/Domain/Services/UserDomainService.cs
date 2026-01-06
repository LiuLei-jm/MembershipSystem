using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.Domain.Enums;
using MembershipSystemAPI.Repositories;

namespace MembershipSystemAPI.Domain.Services;

public interface IUserDomainService
{
    Task<bool> IsUsernameUniqueAsync(string username);
    Task<bool> ValidateUserCredentials(string username, string password);
    Task<bool> LockUserAsync(Guid userId, DateTimeOffset lockoutEnd);
    Task<bool> UnlockUserAsync(Guid userId);
    Task<bool> ToggleUserStatusAsync(Guid userId);
    Task<bool> UpdateLastLoginAsync(Guid userId);
    Task<bool> IncrementAccessFailedCountAsync(Guid userId);
    Task<bool> UpdatePasswordAsync(Guid userId, string newPasswordHash);
    Task<Guid> CreateUserAsync(string username, string passwordHash, string role, string? createdBy = null);
}

public class UserDomainService : IUserDomainService
{
    private readonly IUserRepository _userRepository;

    public UserDomainService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> IsUsernameUniqueAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username) == null;
    }

    public async Task<bool> ValidateUserCredentials(string username, string password)
    {
        User? user = await _userRepository.GetByUsernameAsync(username);
        if (user == null) return false;
        if (!user.IsActive) return false;
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow) return false;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<bool> LockUserAsync(Guid userId, DateTimeOffset lockoutEnd)
    {
        return await _userRepository.LockUserAsync(userId, lockoutEnd);
    }

    public async Task<bool> UnlockUserAsync(Guid userId)
    {
        return await _userRepository.UnlockUserAsync(userId);
    }

    public async Task<bool> ToggleUserStatusAsync(Guid userId)
    {
        return await _userRepository.ToggleUserStatusAsync(userId);
    }

    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    {
        return await _userRepository.UpdateLastLoginAsync(userId);
    }

    public async Task<bool> IncrementAccessFailedCountAsync(Guid userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.AccessFailedCount++;
        if (user.AccessFailedCount >= 5)
        {
            return await _userRepository.LockUserAsync(userId, DateTimeOffset.UtcNow.AddMinutes(15));
        }
        else
        {
            return await _userRepository.UpdateAccessFailedCountAsync(userId, user.AccessFailedCount);
        }
    }

    public async Task<bool> UpdatePasswordAsync(Guid userId, string newPasswordHash)
    {
        User? user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.PasswordHash = newPasswordHash;
        // user.UpdatedAt = DateTimeOffset.UtcNow; // Models.User没有UpdatedAt属性

        var result = await _userRepository.UpdateAsync(user);
        return result != null;
    }

    public async Task<Guid> CreateUserAsync(string username, string passwordHash, string role, string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("用户名不能为空", nameof(username));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("密码哈希不能为空", nameof(passwordHash));

        var user = User.Create(username, passwordHash, Enum.TryParse<UserRole>(role, true, out var parsedRole) ? parsedRole : UserRole.User);

        await _userRepository.AddAsync(user);
        return user.Id;
    }
}