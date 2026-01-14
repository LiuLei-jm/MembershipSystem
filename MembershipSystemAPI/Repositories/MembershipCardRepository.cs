using MembershipSystemAPI.Data;
using MembershipSystemAPI.Domain.Entities;

namespace MembershipSystemAPI.Repositories;

public class MembershipCardRepository : BaseRepository<MembershipCard>, IMembershipCardRepository
{
    public MembershipCardRepository(MemDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<MembershipCard?> GetByCdkAsync(string cdk)
    {
        return await _dbSet
            .Include(mc => mc.User)
            .FirstOrDefaultAsync(mc => mc.Cdk == cdk);
    }

    public async Task<IEnumerable<MembershipCard>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(mc => mc.User)
            .Where(mc => mc.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<MembershipCard>> GetActiveMembershipsByUserIdAsync(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        var cards = await _dbSet
            .Include(mc => mc.User)
            .Where(mc => mc.UserId == userId)
            .ToListAsync();
        return cards.Where(c => c.StartTime <= now && c.EndTime > now).ToList();
    }

    public async Task<IEnumerable<MembershipCard>> GetExpiredMembershipsAsync(int batchSize = 100)
    {
        var now = DateTimeOffset.UtcNow;
        return await _dbSet
            .Include(mc => mc.User)
            .ThenInclude(u => u.ApiKey)
            .Where(mc => !mc.IsExpiredNotificationSent)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<MembershipCard>> GetMembershipsExpiringSoonAsync(TimeSpan timeWindow)
    {
        var now = DateTimeOffset.UtcNow;
        var futureTime = now.Add(timeWindow);

        var cards = await _dbSet
            .Include(mc => mc.User)
            .Where(mc => !mc.IsExpiredNotificationSent)
            .ToListAsync();
        return cards.Where(c => c.EndTime > now && c.EndTime <= futureTime).ToList();
    }

    public async Task<bool> MarkAsExpiredNotificationSentAsync(Guid cardId)
    {
        var card = await _dbSet.FindAsync(cardId);
        if (card != null)
        {
            card.IsExpiredNotificationSent = true;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<bool> UpdateCdkAsync(Guid cardId, string newCdk)
    {
        var card = await _dbSet.FindAsync(cardId);
        if (card != null)
        {
            card.Cdk = newCdk;
            // TODO: 如果需要更新时间戳字段，需要先在模型中添加
            // card.UpdatedAt = DateTimeOffset.UtcNow;
            return await _dbContext.SaveChangesAsync() > 0;
        }
        return false;
    }

    public async Task<int> CountMembershipsByUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(mc => mc.UserId == userId)
            .CountAsync();
    }

    public async Task<bool> IsCdkUniqueAsync(string cdk)
    {
        return !await _dbSet.AnyAsync(mc => mc.Cdk == cdk);
    }
}