namespace MembershipSystemAPI.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public string MembershipCardPath { get; set; } = "D:";

    public int AccessFailedCount { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }

    public ApiKey ApiKey { get; set; } = new();
    public List<MembershipCard> MembershipCards { get; set; } = new();
}
