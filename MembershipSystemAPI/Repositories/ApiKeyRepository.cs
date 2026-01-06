using MembershipSystemAPI.Data;
using MembershipSystemAPI.Domain.Entities;

namespace MembershipSystemAPI.Repositories;

public class ApiKeyRepository : BaseRepository<ApiKey>, IApiKeyRepository
{
    public ApiKeyRepository(MemDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<ApiKey?> GetByKeyAsync(string key)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Key == key);
    }

    public async Task<ApiKey?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<ApiKey?> GetByKeyWithUserAsync(string key)
    {
        return await _dbSet
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Key == key);
    }

    public async Task<bool> RevokeByUserIdAsync(Guid userId)
    {
        var apiKey = await _dbSet.FirstOrDefaultAsync(a => a.UserId == userId);
        if (apiKey != null)
        {
            _dbSet.Remove(apiKey);
            return await _dbContext.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> IsKeyUniqueAsync(string key)
    {
        return !await _dbSet.AnyAsync(a => a.Key == key);
    }
}