using MembershipSystemAPI.Domain.Entities;

namespace MembershipSystemAPI.Repositories;

public interface IMembershipCardRepository : IAsyncRepository<MembershipCard>
{
    Task<MembershipCard?> GetByCdkAsync(string cdk);
    Task<IEnumerable<MembershipCard>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<MembershipCard>> GetActiveMembershipsByUserIdAsync(Guid userId);
    Task<IEnumerable<MembershipCard>> GetExpiredMembershipsAsync(int batchSize = 100);
    Task<IEnumerable<MembershipCard>> GetMembershipsExpiringSoonAsync(TimeSpan timeWindow);
    Task<bool> MarkAsExpiredNotificationSentAsync(Guid cardId);
    Task<bool> UpdateCdkAsync(Guid cardId, string newCdk);
    Task<int> CountMembershipsByUserAsync(Guid userId);
    Task<bool> IsCdkUniqueAsync(string cdk);
}