using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.CQRS.MediatRQueries;

// PathConfiguration相关的查询
public record GetPathConfigurationQuery(
    Guid UserId
) : IRequest<PathConfigurationResponse?>;

public record GetPathConfigurationWithUserQuery(
    Guid UserId
) : IRequest<PathConfigurationResponse?>;