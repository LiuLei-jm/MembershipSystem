using MembershipSystemAPI.Data;
using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Hubs;
using MembershipSystemAPI.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace MembershipSystemAPI.Services;

/// <summary>
/// 用户连接服务实现类，处理用户API密钥验证和连接相关的通知
/// </summary>
public class UserConnectionService : IUserConnectionService
{
    private readonly IUserRepository _userRepository;
    private readonly IPathService _pathService;
    private readonly ILogger<UserConnectionService> _logger;

    /// <summary>
    /// 初始化用户连接服务
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="pathService">路径服务</param>
    /// <param name="logger">日志记录器</param>
    public UserConnectionService(
        IUserRepository userRepository,
        IPathService pathService,
        ILogger<UserConnectionService> logger)
    {
        _userRepository = userRepository;
        _pathService = pathService;
        _logger = logger;
    }

    /// <summary>
/// 验证API密钥并获取用户信息
/// </summary>
/// <param name="apiKey">API密钥</param>
/// <returns>用户信息，如果验证失败则返回null</returns>
public async Task<User?> ValidateApiKeyAndGetUserAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return null;
        }

        // Get user with API key details
        var user = await _userRepository.GetByIdWithDetailsAsync(
            u => u.ApiKey != null && u.ApiKey.Key == apiKey && u.IsActive);

        return user;
    }

    /// <summary>
/// 发送待处理的过期会员通知
/// </summary>
/// <param name="apiKey">用户API密钥</param>
/// <param name="user">用户信息</param>
/// <param name="hubContext">SignalR Hub上下文</param>
public async Task SendPendingExpiredMembershipNotificationsAsync(string apiKey, User user, IHubContext<FilePushHub> hubContext)
    {
        try
        {
            // Get all membership cards for this user
            var userWithCards = await _userRepository.GetByIdWithDetailsAsync(user.Id);
            if (userWithCards?.MembershipCards == null || !userWithCards.MembershipCards.Any())
            {
                return;
            }

            var utcNow = DateTimeOffset.UtcNow;
            var cardsToSend = new List<Domain.Entities.MembershipCard>();

            foreach (var card in userWithCards.MembershipCards)
            {
                // Check if card is expired (within the last 30 days)
                if (card.EndTime <= utcNow && card.EndTime >= utcNow.AddDays(-30))
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
            }

            var updatedCards = new List<Domain.Entities.MembershipCard>();
            foreach (var card in cardsToSend)
            {
                _logger.LogInformation($"向刚连接的用户 {user.Username} 发送会员卡 {card.Id} 的过期通知");

                // Use PathService to get the correct file path
                var pathConfig = await _pathService.GetUserPathConfigurationAsync(user.Id);
                string filePath = Path.Combine(pathConfig.BasePath, pathConfig.MembershipCardFilePath);

                var deleteContentRequest = new SendDeleteRequest
                (
                     filePath,
                     card.Cdk,
                     $"会员卡 {card.Cdk} 已过期"
                );
                await hubContext.Clients.User(apiKey).SendAsync("ReceiveDeleteCommand", deleteContentRequest);
                card.IsExpiredNotificationSent = true;
                updatedCards.Add(card);
            }

            // Save changes for updated cards
            if (updatedCards.Any())
            {
                foreach (var card in updatedCards)
                {
                    await _userRepository.UpdateMembershipCardAsync(card);
                }
                _logger.LogInformation($"向用户 {user.Username} 发送了 {updatedCards.Count} 条过期会员卡通知");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"发送过期会员卡通知时发生错误，用户: {user.Username}");
        }
    }
}