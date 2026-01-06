using MembershipSystemAPI.Domain.Entities;

namespace MembershipSystemAPI.Repositories;

public interface IPathConfigurationRepository : IAsyncRepository<PathConfiguration>
{
    Task<PathConfiguration?> GetByUserIdAsync(Guid userId);
    Task<PathConfiguration?> GetByUserIdWithUserAsync(Guid userId);
    Task<bool> UpdateBasePathAsync(Guid userId, string newBasePath);
    Task<bool> UpdateMembershipCardFilePathAsync(Guid userId, string newFilePath);
}