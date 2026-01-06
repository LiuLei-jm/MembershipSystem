using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.CQRS.MediatRQueries;

// Membership card related queries
public record GetAllMembershipCardsQuery(
    Guid UserId
) : IRequest<List<MembershipCardSummaryResponse>>;

public record GetUnexpiredMembershipCardsQuery(
    Guid UserId
) : IRequest<List<MembershipCardSummaryResponse>>;