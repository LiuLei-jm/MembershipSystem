namespace MembershipSystemAPI.Models;

public class ConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DateTime ConenctionAt { get; set; } = DateTime.Now;
}
