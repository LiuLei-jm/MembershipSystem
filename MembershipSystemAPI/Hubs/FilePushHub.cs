using MembershipSystemAPI.Endpoints.FilePushes;
using MembershipSystemAPI.Models;

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

        User? user = null;
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MemDbContext>();
            user = await dbContext.Users.FirstOrDefaultAsync(u => u.ApiKey.Key == apiKey && u.IsActive);
            if (user is null)
            {
                _logger.LogWarning($"没有此用户或用户未激活。连接已中止。ConnectionId：{Context.ConnectionId}");
                Context.Abort();
                return;
            }
            if (string.IsNullOrEmpty(deviceName)) deviceName = Context.ConnectionId;
            _connectionManager.AddConnection(apiKey, Context.ConnectionId, deviceName);
            _logger.LogInformation($"用户: {user.Username} 已连接到 FilePushHub，连接ID: {Context.ConnectionId}");
        }

        // Send pending expired membership notifications
        await SendPendingExpiredMembershipNotificationsAsync(apiKey, user!);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"客户端断开连接：{Context.ConnectionId}");
        _connectionManager.RemoveConnection(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    private async Task SendPendingExpiredMembershipNotificationsAsync(string apiKey, User user)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MemDbContext>();
        var pathService = scope.ServiceProvider.GetRequiredService<IPathService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<FilePushHub>>();

        // Get all expired membership cards for this user that haven't had notifications sent
        var utcNow = DateTimeOffset.UtcNow;
        var pendingExpiredCards = await dbContext.MembershipCards
            .Where(c => c.UserId == user.Id)
            .ToListAsync();

        var expiredCards = pendingExpiredCards
            .Where(c => c.EndTime <= utcNow && c.EndTime >= utcNow.AddDays(-30))
            .ToList();

        var cardsToSend = new List<MembershipCard>();

        foreach (var card in expiredCards)
        {
            // Check if we should send this notification
            // Send if:
            // 1. This is the first time we're checking (LastCheckedForConnection is null), or
            // 2. It's been more than 1 hour since we last tried to send it
            if (card.LastCheckedForConnection == null || (utcNow - card.LastCheckedForConnection.Value).TotalMinutes >= 1)
            {
                cardsToSend.Add(card);
                card.LastCheckedForConnection = utcNow;
            }
        }

        foreach (var card in cardsToSend)
        {
            _logger.LogInformation($"向刚连接的用户 {user.Username} 发送会员卡 {card.Id} 的过期通知");

            // Use PathService to get the correct file path
            var pathConfig = await pathService.GetUserPathConfigurationAsync(user.Id);
            string filePath = Path.Combine(pathConfig.BasePath, pathConfig.MembershipCardFilePath);

            var command = new SendDeleteRequest
            {
                FilePath = filePath,
                ContentToRemove = card.Cdk,
                LogMessage = $"会员卡 {card.Cdk} 已过期"
            };
            await hubContext.Clients.User(apiKey).SendAsync("ReceiveDeleteCommand", command);
            card.IsExpiredNotificationSent = true;
        }

        if (cardsToSend.Any())
        {
            await dbContext.SaveChangesAsync();
            _logger.LogInformation($"向用户 {user.Username} 发送了 {cardsToSend.Count} 条过期会员卡通知");
        }
    }
}
