using MembershipSystemAPI.Data;
using MembershipSystemAPI.Services;
using ConnectionInfoEntity = MembershipSystemAPI.Domain.Entities.ConnectionInfo;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.QueryHandlers;

public class GetUserConnectionClientsQueryHandler : IRequestHandler<GetUserConnectionClientsQuery, IEnumerable<ConnectionInfoEntity>>
{
    private readonly MemDbContext _dbContext;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<GetUserConnectionClientsQueryHandler> _logger;

    public GetUserConnectionClientsQueryHandler(
        MemDbContext dbContext,
        IConnectionManager connectionManager,
        ILogger<GetUserConnectionClientsQueryHandler> logger)
    {
        _dbContext = dbContext;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<IEnumerable<ConnectionInfoEntity>> Handle(GetUserConnectionClientsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var apiKeyObj = await _dbContext.ApiKeys.AsNoTracking()
                .FirstOrDefaultAsync(k => k.UserId == request.CurrentUserId, cancellationToken);

            if (apiKeyObj == null)
            {
                return new List<ConnectionInfoEntity>();
            }

            var connectedClients = _connectionManager.GetConnections(apiKeyObj.Key).ToList();
            return connectedClients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户连接客户端时发生错误，用户ID: {UserId}", request.CurrentUserId);
            throw;
        }
    }
}

public class GetAllConnectionClientsQueryHandler : IRequestHandler<GetAllConnectionClientsQuery, IEnumerable<ConnectionInfoEntity>>
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<GetAllConnectionClientsQueryHandler> _logger;

    public GetAllConnectionClientsQueryHandler(
        IConnectionManager connectionManager,
        ILogger<GetAllConnectionClientsQueryHandler> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<IEnumerable<ConnectionInfoEntity>> Handle(GetAllConnectionClientsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var allConnections = _connectionManager.GetAllConnections().ToList();
            return await Task.FromResult(allConnections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有连接客户端时发生错误");
            throw;
        }
    }
}