using System.ComponentModel.DataAnnotations;

namespace MembershipSystemAPI.Configuration;

public class JwtSettings : IJwtSettings
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT密钥不能为空")]
    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = "MembershipSystemAPI";

    public string Audience { get; set; } = "MembershipSystemClients";

    public int ExpiryInHours { get; set; } = 24;
}

public class DatabaseSettings : IDatabaseSettings
{
    public const string SectionName = "Database";

    [Required(ErrorMessage = "数据库连接字符串不能为空")]
    public string ConnectionString { get; set; } = string.Empty;

    public bool EnableSensitiveDataLogging { get; set; } = false;
}

public class RateLimitSettings : IRateLimitSettings
{
    public const string SectionName = "RateLimiting";

    public int LoginAttemptsPerMinute { get; set; } = 5;

    public int LoginQueueLimit { get; set; } = 2;

    public int RegistrationAttemptsPerHour { get; set; } = 3;

    public int GlobalRequestsPerMinute { get; set; } = 100;
}

public class SecuritySettings : ISecuritySettings
{
    public const string SectionName = "Security";

    public bool RequireConfirmedAccount { get; set; } = false;

    public int MaxFailedAccessAttempts { get; set; } = 5;

    public int LockoutTimeInMinutes { get; set; } = 15;

    public bool EnableTwoFactor { get; set; } = false;
}

public class MembershipSettings : IMembershipSettings
{
    public const string SectionName = "Membership";

    public bool AutoProcessExpiredMemberships { get; set; } = true;

    public int ExpiredMembershipsBatchSize { get; set; } = 100;

    public int ExpiredMembershipsProcessingIntervalMinutes { get; set; } = 60;
}