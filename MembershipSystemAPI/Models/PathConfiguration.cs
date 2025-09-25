namespace MembershipSystemAPI.Models;

public class PathConfiguration
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Base path for membership card files
    public string BasePath { get; set; } = "D:";

    // Specific paths for different file types
    public string MembershipCardFilePath { get; set; } = "CDK.txt";

    // Allow user to set custom paths
    public bool AllowCustomPaths { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}