using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MembershipSystemAPI.Services;

/// <summary>
/// 用户连接服务接口，处理用户API密钥验证和连接相关的通知
/// </summary>
public interface IUserConnectionService
{
    /// <summary>
    /// 验证API密钥并获取用户信息
    /// </summary>
    /// <param name="apiKey">API密钥</param>
    /// <returns>用户信息，如果验证失败则返回null</returns>
    Task<User?> ValidateApiKeyAndGetUserAsync(string apiKey);

    /// <summary>
    /// 发送待处理的过期会员通知
    /// </summary>
    /// <param name="apiKey">用户API密钥</param>
    /// <param name="user">用户信息</param>
    /// <param name="hubContext">SignalR Hub上下文</param>
    Task SendPendingExpiredMembershipNotificationsAsync(string apiKey, User user, IHubContext<FilePushHub> hubContext);
}