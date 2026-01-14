using System.ComponentModel.DataAnnotations.Schema;

namespace MembershipSystemAPI.Domain.Entities;

public class ApiKey
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    // 导航属性
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    // 这些属性用于代码逻辑但不映射到数据库
    [NotMapped]
    public DateTimeOffset? UpdatedAt { get; set; }

    // 默认构造函数
    public ApiKey()
    {
        Id = Guid.NewGuid();
        Key = GenerateSecurityApiKey();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // 重载构造函数，用于创建指定用户ID的ApiKey
    public ApiKey(Guid userId) : this()
    {
        UserId = userId;
    }

    private string GenerateSecurityApiKey()
    {
        var bytes = new byte[512];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes)
            .Replace("+", "=")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public void RegenerateKey()
    {
        Key = GenerateSecurityApiKey();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static ApiKey Create(Guid userId)
    {
        return new ApiKey(userId);
    }
}