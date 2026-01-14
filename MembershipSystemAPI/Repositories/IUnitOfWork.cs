namespace MembershipSystemAPI.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IMembershipCardRepository MembershipCards { get; }
    IApiKeyRepository ApiKeys { get; }
    IPathConfigurationRepository PathConfigurations { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}