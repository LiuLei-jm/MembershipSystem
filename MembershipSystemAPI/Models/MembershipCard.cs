using NJsonSchema.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MembershipSystemAPI.Models;

public class MembershipCard
{
    public Guid Id { get; set; }
    public string MembershipName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int DurationInDays { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Amount { get; set; }
    public string Cdk { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public bool IsExpiredNotificationSent { get; set; } = false;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [NotMapped]
    public bool IsExpired => DateTime.Now > EndTime;
}
