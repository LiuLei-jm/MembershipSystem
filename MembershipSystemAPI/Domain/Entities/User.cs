using MembershipSystemAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MembershipSystemAPI.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = UserRole.User.ToString();
    public bool IsActive { get; set; } = true;
    public int AccessFailedCount { get; set; } = 0;
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    [NotMapped]
    public DateTimeOffset? UpdatedAt { get; set; }

    // 导航属性 - 初始化为null以避免不必要的内存分配
    public virtual ApiKey? ApiKey { get; set; }
    public virtual List<MembershipCard>? MembershipCards { get; set; }
    public virtual PathConfiguration? PathConfiguration { get; set; }

    // 默认构造函数 - 与原始Models版本保持一致
    public User()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // 枚举属性的包装器，便于编程但不会影响数据库
    [NotMapped]
    public UserRole UserRole
    {
        get => Enum.TryParse<UserRole>(Role, true, out var role) ? role : UserRole.User;
        set => Role = value.ToString();
    }

    public static User Create(string username, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("用户名不能为空");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("密码哈希不能为空");

        var Id = Guid.NewGuid(); // 预生成Id以供ApiKey使用
        return new User
        {
            Id = Id,
            Username = username,
            PasswordHash = passwordHash,
            Role = role.ToString(),
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            ApiKey = ApiKey.Create(Id),
            PathConfiguration = PathConfiguration.Create(Id)
        };
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("密码哈希不能为空");

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Lock(DateTimeOffset lockoutEnd)
    {
        LockoutEnd = lockoutEnd;
        AccessFailedCount = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Unlock()
    {
        LockoutEnd = null;
        AccessFailedCount = 0;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordFailedLogin()
    {
        AccessFailedCount++;

        if (AccessFailedCount >= 5)
        {
            Lock(DateTimeOffset.UtcNow.AddMinutes(15));
        }
        else
        {
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
        AccessFailedCount = 0;
        LockoutEnd = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ToggleStatus()
    {
        IsActive = !IsActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AssignRole(UserRole newRole)
    {
        Role = newRole.ToString();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd > DateTimeOffset.UtcNow;
    }

    // 辅助方法：获取或创建ApiKey
    public ApiKey GetOrCreateApiKey()
    {
        if (ApiKey == null)
        {
            ApiKey = ApiKey.Create(Id);
        }
        return ApiKey;
    }

    // 辅助方法：获取或创建PathConfiguration
    public PathConfiguration GetOrCreatePathConfiguration()
    {
        if (PathConfiguration == null)
        {
            PathConfiguration = PathConfiguration.Create(Id);
        }
        return PathConfiguration;
    }
}