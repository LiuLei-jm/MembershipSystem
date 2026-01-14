using System.ComponentModel.DataAnnotations.Schema;

namespace MembershipSystemAPI.Domain.Entities;

public class PathConfiguration
{
    public Guid Id { get; set; }
    public string BasePath { get; set; } = "D:";
    public string MembershipCardFilePath { get; set; } = "CDK.txt";
    public bool AllowCustomPaths { get; set; } = true;

    // 导航属性
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    // 这些属性用于代码逻辑但不映射到数据库
    [NotMapped]
    public DateTimeOffset CreatedAt { get; set; }

    [NotMapped]
    public DateTimeOffset? UpdatedAt { get; set; }

    // 默认构造函数
    public PathConfiguration()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // 重载构造函数，用于创建指定用户ID的PathConfiguration
    public PathConfiguration(Guid userId) : this()
    {
        UserId = userId;
    }

    public static PathConfiguration Create(Guid userId, string basePath = "D:", string membershipCardFilePath = "CDK.txt")
    {
        if (string.IsNullOrWhiteSpace(basePath))
            throw new DomainException("基础路径不能为空");
        if (string.IsNullOrWhiteSpace(membershipCardFilePath))
            throw new DomainException("会员卡文件路径不能为空");

        return new PathConfiguration(userId)
        {
            BasePath = basePath,
            MembershipCardFilePath = membershipCardFilePath
        };
    }

    public void UpdateBasePath(string newBasePath)
    {
        if (string.IsNullOrWhiteSpace(newBasePath))
            throw new DomainException("基础路径不能为空");

        BasePath = newBasePath;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateMembershipCardFilePath(string newFilePath)
    {
        if (string.IsNullOrWhiteSpace(newFilePath))
            throw new DomainException("会员卡文件路径不能为空");

        MembershipCardFilePath = newFilePath;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public string GetFullMembershipCardPath()
    {
        return Path.Combine(BasePath, MembershipCardFilePath);
    }
}