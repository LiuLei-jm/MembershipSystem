using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.CQRS.MediatRCommands;

// PathConfiguration相关的命令
public record UpdatePathConfigurationCommand(
    Guid UserId,
    string BasePath,
    string MembershipCardFilePath,
    bool AllowCustomPaths
) : IRequest<PathConfigurationResponse?>;

public record UpdateBasePathCommand(
    Guid UserId,
    string NewBasePath
) : IRequest<bool>;

public record UpdateMembershipCardFilePathCommand(
    Guid UserId,
    string NewFilePath
) : IRequest<bool>;