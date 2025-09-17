
namespace MembershipSystemAPI.Endpoints.FilePushes;

public class PushToConnectionEndpoint : Endpoint<PushToConnectionRequest, PushToConnectionResponse>
{
    public IHubContext<FilePushHub> HubContext { get; set; } = null!;
    private readonly MemDbContext _dbContext;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<PushToConnectionEndpoint> _logger;

    public PushToConnectionEndpoint(MemDbContext dbContext, IConnectionManager connectionManager, ILogger<PushToConnectionEndpoint> logger)
    {
        _dbContext = dbContext;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/file-push/push-to-connection");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "向指定连接的客户端发送文件写入指令";
            s.Description = "此端点需要用户认证。它会查找与当前用户关联的 API Key，并向指定连接 ID 的 SignalR 客户端发送一条文件写入命令。";
        });
    }
    public override async Task HandleAsync(PushToConnectionRequest req, CancellationToken ct)
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
            _logger.LogWarning($"未找到与用户 ID 关联的 API Key. UserId: {currentUserGuid}");
            await Send.ForbiddenAsync(ct);
            return;
        }

        var userConnections = _connectionManager.GetConnections(apiKeyObj.Key);
        if (!userConnections.Any(c => c.ConnectionId == req.TargetConnectionId))
        {
            _logger.LogWarning($"尝试访问不属于用户的连接. UserId: {currentUserGuid}, ConnectionId: {req.TargetConnectionId}");
            await Send.ForbiddenAsync(ct);
            return;
        }

        var command = new PushToConnectionRequest
        {
            FilePath = req.FilePath,
            Content = req.Content + Environment.NewLine,
            LogMessage = req.LogMessage
        };
        await HubContext.Clients.Client(req.TargetConnectionId).SendAsync("ReceiveWriteCommand", command, ct);
        await Send.OkAsync(new PushToConnectionResponse
        {
            Message = "命令已发送至指定的客户端."
        }, ct);
    }
}

public class PushToConnectionRequest
{
    public string TargetConnectionId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string LogMessage { get; set; } = string.Empty;
}

public class PushToConnectionResponse
{
    public string Message { get; set; } = "命令已发送至指定的客户端.";
}
