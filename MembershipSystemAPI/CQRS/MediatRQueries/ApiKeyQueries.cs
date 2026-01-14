using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.CQRS.MediatRQueries;

public record GetApiKeyQueries(
    Guid UserId
    ) : IRequest<GetApiKeyResponse>;
