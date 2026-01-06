using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Repositories;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.CommandHandlers;

public class GenerateApiKeyCommandHandler : IRequestHandler<GenerateApiKeyCommand, GenerateApiKeyResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GenerateApiKeyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GenerateApiKeyResponse> Handle(GenerateApiKeyCommand command, CancellationToken cancellationToken)
    {
        var existingKey = await _unitOfWork.ApiKeys.GetByUserIdAsync(command.UserId);

        if (existingKey != null)
        {
            existingKey.RegenerateKey();
            existingKey.CreatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            var newKey = ApiKey.Create(command.UserId);
            await _unitOfWork.ApiKeys.AddAsync(newKey);
            existingKey = newKey;
        }

        while (!await _unitOfWork.ApiKeys.IsKeyUniqueAsync(existingKey.Key))
        {
            existingKey.RegenerateKey();
        }

        await _unitOfWork.SaveChangesAsync();

        return new GenerateApiKeyResponse(
            existingKey.Key,
            existingKey.CreatedAt
        );
    }
}