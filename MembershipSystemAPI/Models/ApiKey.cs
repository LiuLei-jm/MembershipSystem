namespace MembershipSystemAPI.Models;

public class ApiKey
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ApiKey()
    {
        Key = GenerateSecurityApiKey();
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
        CreatedAt = DateTime.Now;
    }
}