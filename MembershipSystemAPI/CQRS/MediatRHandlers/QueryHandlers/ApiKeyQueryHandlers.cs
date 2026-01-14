using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Repositories;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.QueryHandlers;

public class ApiKeyQueryHandlers : IRequestHandler<GetApiKeyQueries, GetApiKeyResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApiKeyQueryHandlers(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetApiKeyResponse> Handle(GetApiKeyQueries request, CancellationToken cancellationToken)
    {
        var apiKey = await _unitOfWork.ApiKeys.GetByUserIdAsync(request.UserId);
        if (apiKey == null)
        {
            return null!;
        }
        return new GetApiKeyResponse
        (
         apiKey.Key,
            apiKey.CreatedAt
        );
    }
}
