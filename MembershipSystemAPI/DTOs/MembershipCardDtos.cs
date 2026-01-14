namespace MembershipSystemAPI.DTOs;

// Membership Card Request DTOs
/// <summary>
/// DTO for creating a new membership card
/// </summary>
/// <param name="MembershipName">The name of the membership</param>
/// <param name="DurationInDays">The duration of the membership in days</param>
/// <param name="Amount">The amount of the membership</param>
/// <param name="Notes">Optional notes for the membership card</param>

public record CreateMembershipRequest
{
    public string MembershipName { get; set; } = string.Empty;
    public string? Cdk { get; set; }
    public int DurationInDays { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing membership card
/// </summary>
/// <param name="MembershipName">The updated name of the membership (optional)</param>
/// <param name="Notes">Updated notes for the membership card (optional)</param>
public record UpdateMembershipCardRequest(
    string? MembershipName = null,
    string? Notes = null
);

// Membership Card Response DTOs

public record CreateMembershipResponse(bool Success,Guid CardId, string Cdk, DateTimeOffset EndTime,  string Message);

/// <summary>
/// DTO for detailed membership card information
/// </summary>
/// <param name="Id">The unique identifier of the membership card</param>
/// <param name="MembershipName">The name of the membership</param>
/// <param name="StartTime">The start time of the membership</param>
/// <param name="DurationInDays">The duration of the membership in days</param>
/// <param name="EndTime">The end time of the membership</param>
/// <param name="Amount">The amount of the membership</param>
/// <param name="Cdk">The CDK code of the membership card</param>
/// <param name="Notes">Notes for the membership card</param>
/// <param name="IsExpired">Whether the membership card is expired</param>
/// <param name="IsActive">Whether the membership card is active</param>
/// <param name="IsExpiredNotificationSent">Whether the expiration notification has been sent</param>
/// <param name="LastCheckedForConnection">The last time the card was checked for connection</param>
/// <param name="CreatedAt">The creation time of the membership card</param>
/// <param name="UpdatedAt">The last update time of the membership card</param>
/// <param name="UserId">The ID of the user associated with this membership card</param>
public record MembershipCardResponse(
    Guid Id,
    string MembershipName,
    DateTimeOffset StartTime,
    int DurationInDays,
    DateTimeOffset EndTime,
    decimal Amount,
    string Cdk,
    string Notes,
    bool IsExpired,
    bool IsActive,
    bool IsExpiredNotificationSent,
    DateTimeOffset? LastCheckedForConnection,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Guid UserId
);

/// <summary>
/// DTO for membership card summary information
/// </summary>
/// <param name="Id">The unique identifier of the membership card</param>
/// <param name="MembershipName">The name of the membership</param>
/// <param name="StartTime">The start time of the membership</param>
/// <param name="EndTime">The end time of the membership</param>
/// <param name="Amount">The amount of the membership</param>
/// <param name="Cdk">The CDK code of the membership card</param>
/// <param name="IsExpired">Whether the membership card is expired</param>
/// <param name="IsActive">Whether the membership card is active</param>
public record MembershipCardSummaryResponse(
    Guid Id,
    string MembershipName,
    int durationInDays,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal Amount,
    string Cdk,
    bool IsExpired
);

// Additional Request and Response DTOs
/// <summary>
/// DTO for deleting a membership card
/// </summary>
/// <param name="CardId">The ID of the membership card to delete</param>
public record DeleteMembershipRequest(
    Guid CardId
);

/// <summary>
/// DTO for delete membership card response
/// </summary>
/// <param name="Message">The response message</param>
public record DeleteMembershipResponse(
    string Message = ""
);

/// <summary>
/// DTO for updating a membership card
/// </summary>
public class UpdateMembershipRequest
{
    /// <summary>
    /// The duration of the membership in days (optional)
    /// </summary>
    public int? DurationInDays { get; set; }

    /// <summary>
    /// The amount of the membership (optional)
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// The start time of the membership (optional)
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>
    /// Notes for the membership card (optional)
    /// </summary>
    public string? Notes { get; set; }
}

