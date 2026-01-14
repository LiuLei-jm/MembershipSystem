using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.CQRS.MediatRCommands;

// Membership card related commands
public record CreateMembershipCommand(
    Guid UserId,
    string MembershipName,
    string? Cdk,
    int DurationInDays,
    decimal Amount,
    DateTimeOffset StartTime,
    string? Notes
) : IRequest<CreateMembershipResult>;

public record UpdateMembershipCommand(
    Guid CardId,
    Guid UserId,
    int? DurationInDays = null,
    decimal? Amount = null,
    DateTimeOffset? StartTime = null,
    string? Notes = null
) : IRequest<UpdateMembershipResult>;

public record DeleteMembershipCommand(
    Guid CardId,
    Guid UserId
) : IRequest<DeleteMembershipResult>;

// Command results
public record CreateMembershipResult(
    bool Success,
    Guid CardId,
    string Cdk,
    DateTimeOffset EndTime,
    string Message = ""
);

public record UpdateMembershipResult(
    bool Success,
    string Message = ""
);

public record DeleteMembershipResult(
    bool Success,
    string Message = ""
);