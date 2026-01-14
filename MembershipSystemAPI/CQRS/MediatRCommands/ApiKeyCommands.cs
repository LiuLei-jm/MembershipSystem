using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.CQRS.MediatRCommands;

public record GenerateApiKeyCommand(
    Guid UserId
) : IRequest<GenerateApiKeyResponse>;
