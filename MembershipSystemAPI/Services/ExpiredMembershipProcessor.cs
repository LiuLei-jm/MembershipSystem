
using MembershipSystemAPI.Data;
using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Hubs;
using MembershipSystemAPI.Repositories;

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
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            _logger.LogInformation("检查过期会员卡...");
            using var scope = _serviceProvider.CreateScope();
            var membershipCardRepository = scope.ServiceProvider.GetRequiredService<IMembershipCardRepository>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<FilePushHub>>();
            var pathService = scope.ServiceProvider.GetRequiredService<IPathService>();
            var connectionManager = scope.ServiceProvider.GetRequiredService<IConnectionManager>();
            var utcNow = DateTimeOffset.UtcNow;

            // Process expired cards in batches to avoid loading too many into memory
            var expiredCards = await membershipCardRepository.GetExpiredMembershipsAsync(100);
            var expiredCardsList = expiredCards.Where(c => c.EndTime < utcNow).ToList();

            var cardsWithSentNotifications = new List<Domain.Entities.MembershipCard>();

            foreach (var card in expiredCardsList)
            {
                var userApiKey = card.User?.ApiKey?.Key;
                var user = card.User;
                if (string.IsNullOrEmpty(userApiKey))
                {
                    _logger.LogWarning($"用户 {user?.Username} 的会员卡 {card.Id} 已过期，但找不到 API Key");
                    continue;
                }
                if (user is null)
                {
                    _logger.LogWarning($"找不到会员卡 {card.Id} 关联的用户");
                    continue;
                }

                // Check if the user has any active connections before sending notification
                var userConnections = connectionManager.GetConnections(userApiKey);
                if (!userConnections.Any())
                {
                    _logger.LogInformation($"用户 {user.Username} 的会员卡 {card.Id} 已过期，但客户端未连接，将等待客户端连接后发送通知");
                    // Update the LastCheckedForConnection time to track how long we've been waiting
                    card.LastCheckedForConnection = utcNow;
                    continue; // Don't mark as sent, will try again next time
                }

                _logger.LogInformation($"会员卡 {card.Id} 已过期，向用户 {user.Username} 发送通知");

                // Use PathService to get the correct file path
                var pathConfig = await pathService.GetUserPathConfigurationAsync(user.Id);
                string filePath = Path.Combine(pathConfig.BasePath, pathConfig.MembershipCardFilePath);

                var deleteContentRequest = new SendDeleteRequest
                (
                     filePath,
                     card.Cdk,
                     $"会员卡 {card.Cdk} 已过期"
                );
                await hubContext.Clients.User(userApiKey).SendAsync("ReceiveDeleteCommand", deleteContentRequest);
                cardsWithSentNotifications.Add(card); // Only mark cards where notification was actually sent
            }

            // Only update cards where notifications were actually sent
            foreach (var card in cardsWithSentNotifications)
            {
                card.IsExpiredNotificationSent = true;
                // Update the card in the repository
                await membershipCardRepository.UpdateAsync(card);
            }

            if (cardsWithSentNotifications.Any())
            {
                _logger.LogInformation($"处理了 {cardsWithSentNotifications.Count} 张过期会员卡");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理过期会员卡时发生错误");
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
