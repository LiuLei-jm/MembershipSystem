
namespace MembershipSystemAPI.Services;

public class ExpiredMembershipProcessor : IHostedService, IDisposable
{
    private readonly ILogger<ExpiredMembershipProcessor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private Timer? _timer = null;
    public ExpiredMembershipProcessor(ILogger<ExpiredMembershipProcessor> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("会员卡过期处理服务启动。");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogInformation("检查过期会员卡...");
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MemDbContext>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<FilePushHub>>();
        var expiredCards = await dbContext.MembershipCards
            .Include(c => c.User)
            .ThenInclude(u => u.ApiKey)
            .Where(c => c.EndTime < DateTime.Now && !c.IsExpiredNotificationSent)
            .ToListAsync();
        foreach(var card in expiredCards)
        {
            var userApiKey = card.User.ApiKey?.Key;
            var user = card.User;
            if(string.IsNullOrEmpty(userApiKey))
            {
                _logger.LogWarning($"用户 {user.Username} 的会员卡 {card.Id} 已过期，但找不到 API Key");
                continue;
            }
            if(user is null)
            {
                _logger.LogWarning($"找不到会员卡 {card.Id} 关联的用户");
                continue;
            }
            _logger.LogInformation($"会员卡 {card.Id} 已过期，向用户 {user.Username} 发送通知");
            var command = new {
                FilePath = Path.Combine(user.MembershipCardPath,"CDK.txt"),
                Content = card.Cdk,
                LogMessage = $"会员卡 {card.Cdk} 已过期"
            };
            await hubContext.Clients.User(userApiKey).SendAsync("ReceiveDeleteCommand", command);
            card.IsExpiredNotificationSent = true;
        }
        if (expiredCards.Any())
        {
            await dbContext.SaveChangesAsync();
            _logger.LogInformation($"处理了 {expiredCards.Count} 张过期会员卡");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("会员卡过期处理服务停止。");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

}
