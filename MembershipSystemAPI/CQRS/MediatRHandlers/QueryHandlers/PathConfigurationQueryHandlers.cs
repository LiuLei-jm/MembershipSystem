using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Repositories;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.QueryHandlers;

public class GetPathConfigurationQueryHandler : IRequestHandler<GetPathConfigurationQuery, PathConfigurationResponse?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPathConfigurationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PathConfigurationResponse?> Handle(GetPathConfigurationQuery query, CancellationToken cancellationToken)
    {
        var pathConfig = await _unitOfWork.PathConfigurations.GetByUserIdAsync(query.UserId);
        if (pathConfig == null)
        {
            return null;
        }

        return new PathConfigurationResponse(
            pathConfig.Id,
            pathConfig.BasePath,
            pathConfig.MembershipCardFilePath,
            pathConfig.AllowCustomPaths,
            pathConfig.CreatedAt,
            pathConfig.UpdatedAt
        );
    }
}

public class GetPathConfigurationWithUserQueryHandler : IRequestHandler<GetPathConfigurationWithUserQuery, PathConfigurationResponse?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPathConfigurationWithUserQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PathConfigurationResponse?> Handle(GetPathConfigurationWithUserQuery query, CancellationToken cancellationToken)
    {
        var pathConfig = await _unitOfWork.PathConfigurations.GetByUserIdWithUserAsync(query.UserId);
        if (pathConfig == null)
        {
            return null;
        }

        return new PathConfigurationResponse(
            pathConfig.Id,
            pathConfig.BasePath,
            pathConfig.MembershipCardFilePath,
            pathConfig.AllowCustomPaths,
            pathConfig.CreatedAt,
            pathConfig.UpdatedAt
        );
    }
}