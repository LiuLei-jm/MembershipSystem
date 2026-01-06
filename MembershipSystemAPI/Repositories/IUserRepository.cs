using MembershipSystemAPI.Domain.Entities;
using System.Linq.Expressions;

namespace MembershipSystemAPI.Repositories;

public interface IUserRepository : IAsyncRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByUsernameWithApiKeyAsync(string username);
    Task<User?> GetByIdWithDetailsAsync(Guid userId);
    Task<User?> GetByIdWithDetailsAsync(Expression<Func<User, bool>> predicate);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetUsersWithPendingMembershipsAsync();
    Task<bool> UpdateLastLoginAsync(Guid userId);
    Task<bool> UpdateAccessFailedCountAsync(Guid userId, int count);
    Task<bool> LockUserAsync(Guid userId, DateTimeOffset lockoutEnd);
    Task<bool> UnlockUserAsync(Guid userId);
    Task<bool> ToggleUserStatusAsync(Guid userId);
    Task<bool> UpdateMembershipCardAsync(MembershipCard card);
}