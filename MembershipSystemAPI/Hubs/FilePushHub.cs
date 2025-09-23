namespace MembershipSystemAPI.Hubs;

public class FilePushHub : Hub
{
    private readonly ILogger<FilePushHub> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnectionManager _connectionManager;
    public FilePushHub(ILogger<FilePushHub> logger, IServiceProvider serviceProvider, IConnectionManager connectionManager)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _connectionManager = connectionManager;
    }

    public override async Task OnConnectedAsync()
    {
        var apiKey = Context.Items["ApiKey"] as string;
        var deviceName = Context.Items["DeviceName"] as string;


        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning($"客户端未使用 API 密钥连接。连接已中止。ConnectionId：{Context.ConnectionId}");
            Context.Abort();
            return;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MemDbContext>();
            var userExists = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey.Key == apiKey && u.IsActive);
            if (userExists is null)
            {
                _logger.LogWarning($"没有此用户或用户未激活。连接已中止。ConnectionId：{Context.ConnectionId}");
                Context.Abort();
                return;
            }
            if (string.IsNullOrEmpty(deviceName)) deviceName = Context.ConnectionId;
            _connectionManager.AddConnection(apiKey, Context.ConnectionId, deviceName);
            _logger.LogInformation($"用户: {userExists.Username} 已连接到 FilePushHub，连接ID: {Context.ConnectionId}");
        }
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"客户端断开连接：{Context.ConnectionId}");
        _connectionManager.RemoveConnection(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
