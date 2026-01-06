using MembershipSystemAPI.Data;
using MembershipSystemAPI.Domain.Entities;

namespace MembershipSystemAPI.Repositories;

public class PathConfigurationRepository : BaseRepository<PathConfiguration>, IPathConfigurationRepository
{
    public PathConfigurationRepository(MemDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<PathConfiguration?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(pc => pc.UserId == userId);
    }

    public async Task<PathConfiguration?> GetByUserIdWithUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(pc => pc.User)
            .FirstOrDefaultAsync(pc => pc.UserId == userId);
    }

    public async Task<bool> UpdateBasePathAsync(Guid userId, string newBasePath)
    {
        var config = await _dbSet.FirstOrDefaultAsync(pc => pc.UserId == userId);
        if (config != null)
        {
            config.BasePath = newBasePath;
            config.UpdatedAt = DateTimeOffset.UtcNow;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> UpdateMembershipCardFilePathAsync(Guid userId, string newFilePath)
    {
        var config = await _dbSet.FirstOrDefaultAsync(pc => pc.UserId == userId);
        if (config != null)
        {
            config.MembershipCardFilePath = newFilePath;
            config.UpdatedAt = DateTimeOffset.UtcNow;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        return false;
    }
}