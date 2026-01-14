using MembershipSystemAPI.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace MembershipSystemAPI.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly MemDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(MemDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

        Users = new UserRepository(_dbContext);
        MembershipCards = new MembershipCardRepository(_dbContext);
        ApiKeys = new ApiKeyRepository(_dbContext);
        PathConfigurations = new PathConfigurationRepository(_dbContext);
    }

    public IUserRepository Users { get; }
    public IMembershipCardRepository MembershipCards { get; }
    public IApiKeyRepository ApiKeys { get; }
    public IPathConfigurationRepository PathConfigurations { get; }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle concurrency conflicts
            foreach (var entry in ex.Entries)
            {
                if (entry.Entity is not null)
                {
                    // Refresh the original values
                    entry.Reload();
                }
            }
            throw;
        }
        catch (DbUpdateException ex)
        {
            // Handle database update errors
            throw new RepositoryException("Error saving changes in UnitOfWork", ex);
        }
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await this.SaveChangesAsync();
            await _transaction?.CommitAsync()!;
        }
        catch (Exception)
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _dbContext?.Dispose();
    }
}

public class RepositoryException : Exception
{
    public RepositoryException(string message) : base(message) { }
    public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
}