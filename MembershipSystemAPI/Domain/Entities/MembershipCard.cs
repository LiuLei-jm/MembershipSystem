using System.ComponentModel.DataAnnotations.Schema;

namespace MembershipSystemAPI.Domain.Entities;

public class MembershipCard
{
    public Guid Id { get; set; }
    public string MembershipName { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public int DurationInDays { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public decimal Amount { get; set; }
    public string Cdk { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsExpiredNotificationSent { get; set; } = false;
    public DateTimeOffset? LastCheckedForConnection { get; set; }

    // 导航属性
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    [NotMapped]
    public bool IsExpired => DateTimeOffset.UtcNow > EndTime;

    // 这些属性用于代码逻辑但不映射到数据库
    [NotMapped]
    public DateTimeOffset CreatedAt { get; set; }

    [NotMapped]
    public DateTimeOffset? UpdatedAt { get; set; }

    // 默认构造函数
    public MembershipCard()
    {
    }

    public static MembershipCard Create(string name, int durationInDays, decimal amount, string cdk, Guid userId, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("会员名称不能为空");
        if (durationInDays <= 0)
            throw new DomainException("持续时间必须大于0");
        if (amount < 0)
            throw new DomainException("金额不能为负数");
        if (string.IsNullOrWhiteSpace(cdk))
            throw new DomainException("CDK不能为空");

        var startTime = DateTimeOffset.UtcNow;
        var endTime = startTime.AddDays(durationInDays);

        return new MembershipCard
        {
            Id = Guid.NewGuid(),
            MembershipName = name,
            StartTime = startTime,
            DurationInDays = durationInDays,
            EndTime = endTime,
            Amount = amount,
            Cdk = cdk,
            Notes = notes ?? string.Empty,
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateNotes(string? newNotes)
    {
        Notes = newNotes ?? string.Empty;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateCdk(string newCdk)
    {
        if (string.IsNullOrWhiteSpace(newCdk))
            throw new DomainException("CDK不能为空");

        Cdk = newCdk;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsExpired()
    {
        IsExpiredNotificationSent = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordConnectionCheck()
    {
        LastCheckedForConnection = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // 现有属性 IsExpired 已定义为[NotMapped]计算属性，无需重复

    public bool IsActive()
    {
        var now = DateTimeOffset.UtcNow;
        return StartTime <= now && EndTime > now;
    }

    public TimeSpan GetRemainingTime()
    {
        return IsExpired ? TimeSpan.Zero : EndTime - DateTimeOffset.UtcNow;
    }
}