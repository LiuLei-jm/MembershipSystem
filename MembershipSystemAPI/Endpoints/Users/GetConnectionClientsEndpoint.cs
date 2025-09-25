namespace MembershipSystemAPI.Endpoints.Users;

using System.Threading;
using System.Threading.Tasks;
using ConnectionInfo = Models.ConnectionInfo;

public class GetConnectionClientsEndpoint : EndpointWithoutRequest<List<ConnectionInfo>>
{
    private readonly MemDbContext _dbContext;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<GetConnectionClientsEndpoint> _logger;

    public GetConnectionClientsEndpoint(MemDbContext dbContext, IConnectionManager connectionManager, ILogger<GetConnectionClientsEndpoint> logger)
    {
        _dbContext = dbContext;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("user/connections");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "获取当前用户的所有连接客户端信息";
            s.Description = "此端点需要用户认证。它会查找与当前用户关联的 API Key，并返回使用该 Key 连接的所有 SignalR 客户端的信息列表。";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            _logger.LogWarning($"无效的用户 ID. {currentUserId}");
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var apiKeyObj = await _dbContext.ApiKeys.AsNoTracking().FirstOrDefaultAsync(k => k.UserId == currentUserGuid, ct);
        if (apiKeyObj == null)
        {
            await Send.OkAsync(new List<ConnectionInfo>(), ct);
            return;
        }
        var connectedClients = _connectionManager.GetConnections(apiKeyObj.Key).ToList();
        await Send.OkAsync(connectedClients, ct);
    }
}
