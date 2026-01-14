using MembershipSystemAPI.Domain.Entities;

namespace MembershipSystemAPI.Repositories;

public interface IApiKeyRepository : IAsyncRepository<ApiKey>
{
    Task<ApiKey?> GetByKeyAsync(string key);
    Task<ApiKey?> GetByUserIdAsync(Guid userId);
    Task<ApiKey?> GetByKeyWithUserAsync(string key);
    Task<bool> RevokeByUserIdAsync(Guid userId);
    Task<bool> IsKeyUniqueAsync(string key);
}