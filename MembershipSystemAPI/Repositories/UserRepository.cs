using MembershipSystemAPI.Data;
using MembershipSystemAPI.Domain.Entities;
using System.Linq.Expressions;

namespace MembershipSystemAPI.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(MemDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByUsernameWithApiKeyAsync(string username)
    {
        return await _dbSet
            .Include(u => u.ApiKey)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByIdWithDetailsAsync(Guid userId)
    {
        return await _dbSet
            .Include(u => u.ApiKey)
            .Include(u => u.PathConfiguration)
            .Include(u => u.MembershipCards)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByIdWithDetailsAsync(Expression<Func<User, bool>> predicate)
    {
        return await _dbSet
            .Include(u => u.ApiKey)
            .Include(u => u.PathConfiguration)
            .Include(u => u.MembershipCards)
            .FirstOrDefaultAsync(predicate);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet
            .Where(u => u.IsActive)
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersWithPendingMembershipsAsync()
    {
        return await _dbSet
            .Include(u => u.MembershipCards)
            .Where(u => u.MembershipCards != null && u.MembershipCards.Any(mc => !mc.IsExpiredNotificationSent))
            .ToListAsync();
    }

    public async Task<bool> UpdateLastLoginAsync(Guid userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTimeOffset.UtcNow;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> UpdateAccessFailedCountAsync(Guid userId, int count)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.AccessFailedCount = count;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> LockUserAsync(Guid userId, DateTimeOffset lockoutEnd)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.LockoutEnd = lockoutEnd;
            user.AccessFailedCount = 0;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> UnlockUserAsync(Guid userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> ToggleUserStatusAsync(Guid userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> UpdateMembershipCardAsync(MembershipCard card)
    {
        _dbContext.MembershipCards.Update(card);
        return await _dbContext.SaveChangesAsync() > 0;
    }
}