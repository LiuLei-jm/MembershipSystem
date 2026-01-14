using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Services;

namespace MembershipSystemAPI.Hubs;

public class FilePushHub : Hub
{
    private readonly ILogger<FilePushHub> _logger;
    private readonly IConnectionManager _connectionManager;
    private readonly IUserConnectionService _userConnectionService;
    private readonly IHubContext<FilePushHub> _hubContext;

    public FilePushHub(
        ILogger<FilePushHub> logger,
        IConnectionManager connectionManager,
        IUserConnectionService userConnectionService,
        IHubContext<FilePushHub> hubContext)
    {
        _logger = logger;
        _connectionManager = connectionManager;
        _userConnectionService = userConnectionService;
        _hubContext = hubContext;
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

        // Validate API key and get user through the service
        var user = await _userConnectionService.ValidateApiKeyAndGetUserAsync(apiKey);
        if (user is null)
        {
            _logger.LogWarning($"没有此用户或用户未激活。连接已中止。ConnectionId：{Context.ConnectionId}");
            Context.Abort();
            return;
        }

        if (string.IsNullOrEmpty(deviceName)) deviceName = Context.ConnectionId;
        _connectionManager.AddConnection(apiKey, Context.ConnectionId, deviceName);
        _logger.LogInformation($"用户: {user.Username} 已连接到 FilePushHub，连接ID: {Context.ConnectionId}");

        // Send pending expired membership notifications
        await _userConnectionService.SendPendingExpiredMembershipNotificationsAsync(apiKey, user, _hubContext);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"客户端断开连接：{Context.ConnectionId}");
        _connectionManager.RemoveConnection(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
