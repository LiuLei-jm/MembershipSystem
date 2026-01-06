using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Hubs;
using MembershipSystemAPI.Repositories;
using MembershipSystemAPI.Services;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.CommandHandlers;

public class PushToConnectionCommandHandler : IRequestHandler<PushToConnectionCommand, PushToConnectionCommandResult>
{
    public IHubContext<FilePushHub> HubContext { get; set; } = null!;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<PushToConnectionCommandHandler> _logger;

    public PushToConnectionCommandHandler(
        IHubContext<FilePushHub> hubContext,
        IUnitOfWork unitOfWork,
        IConnectionManager connectionManager,
        ILogger<PushToConnectionCommandHandler> logger)
    {
        HubContext = hubContext;
        _unitOfWork = unitOfWork;
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<PushToConnectionCommandResult> Handle(PushToConnectionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var apiKeyObj = await _unitOfWork.ApiKeys.GetByUserIdAsync(command.CurrentUserId);

            if (apiKeyObj == null)
            {
                _logger.LogWarning($"未找到与用户 ID 关联的 API Key. UserId: {command.CurrentUserId}");
                return new PushToConnectionCommandResult(false, "未找到与用户关联的 API Key");
            }

            var userConnections = _connectionManager.GetConnections(apiKeyObj.Key);
            if (!userConnections.Any(c => c.ConnectionId == command.TargetConnectionId))
            {
                _logger.LogWarning($"尝试访问不属于用户的连接. UserId: {command.CurrentUserId}, ConnectionId: {command.TargetConnectionId}");
                return new PushToConnectionCommandResult(false, "无权访问指定的连接");
            }

            var fileWriteRequest = new SendAppendRequest
            (
                 command.FilePath,
                 command.Content + Environment.NewLine,
                 command.LogMessage
            );

            await HubContext.Clients.Client(command.TargetConnectionId)
                .SendAsync("ReceiveWriteCommand", fileWriteRequest, cancellationToken);

            return new PushToConnectionCommandResult(true, "命令已发送至指定的客户端");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "推送命令到连接时发生错误. UserId: {UserId}, ConnectionId: {ConnectionId}",
                command.CurrentUserId, command.TargetConnectionId);
            return new PushToConnectionCommandResult(false, "推送命令时发生错误");
        }
    }
}

public class SendAppendCommandHandler : IRequestHandler<SendAppendCommand, SendAppendCommandResult>
{
    public IHubContext<FilePushHub> HubContext { get; set; } = null!;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<SendAppendCommandHandler> _logger;
    public SendAppendCommandHandler(
        IHubContext<FilePushHub> hubContext,
        IUnitOfWork unitOfWork,
        IConnectionManager connectionManager,
        ILogger<SendAppendCommandHandler> logger)
    {
        HubContext = hubContext;
        _unitOfWork = unitOfWork;
        _connectionManager = connectionManager;
        _logger = logger;
    }
    public async Task<SendAppendCommandResult> Handle(SendAppendCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var apiKeyObj = await _unitOfWork.ApiKeys.GetByUserIdAsync(command.CurrentUserId);
            if (apiKeyObj == null)
            {
                _logger.LogWarning($"未找到与用户 ID 关联的 API Key. UserId: {command.CurrentUserId}");
                return new SendAppendCommandResult(false, "未找到与用户关联的 API Key");
            }
            var userConnections = _connectionManager.GetConnections(apiKeyObj.Key);
            if (!userConnections.Any())
            {
                _logger.LogWarning($"用户没有活动连接. UserId: {command.CurrentUserId}");
                return new SendAppendCommandResult(false, "用户没有活动连接");
            }
            var fileWriteRequest = new SendAppendRequest
            (
                 command.FilePath,
                 command.Content + Environment.NewLine,
                 command.LogMessage
            );
            var sendTasks = userConnections.Select(c =>
                HubContext.Clients.Client(c.ConnectionId)
                    .SendAsync("ReceiveWriteCommand", fileWriteRequest, cancellationToken));
            await Task.WhenAll(sendTasks);
            return new SendAppendCommandResult(true, "命令已发送至所有活动客户端");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "向客户端发送追加命令时发生错误. UserId: {UserId}", command.CurrentUserId);
            return new SendAppendCommandResult(false, "发送追加命令时发生错误");
        }

    }
}

public class SendDeleteCommandHandler : IRequestHandler<SendDeleteCommand, SendDeleteCommandResult>
{
    public IHubContext<FilePushHub> HubContext { get; set; } = null!;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<SendDeleteCommandHandler> _logger;
    public SendDeleteCommandHandler(
        IHubContext<FilePushHub> hubContext,
        IUnitOfWork unitOfWork,
        IConnectionManager connectionManager,
        ILogger<SendDeleteCommandHandler> logger)
    {
        HubContext = hubContext;
        _unitOfWork = unitOfWork;
        _connectionManager = connectionManager;
        _logger = logger;
    }
    public async Task<SendDeleteCommandResult> Handle(SendDeleteCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var apiKeyObj = await _unitOfWork.ApiKeys.GetByUserIdAsync(command.CurrentUserId);
            if (apiKeyObj == null)
            {
                _logger.LogWarning($"未找到与用户 ID 关联的 API Key. UserId: {command.CurrentUserId}");
                return new SendDeleteCommandResult(false, "未找到与用户关联的 API Key");
            }
            var userConnections = _connectionManager.GetConnections(apiKeyObj.Key);
            if (!userConnections.Any())
            {
                _logger.LogWarning($"用户没有活动连接. UserId: {command.CurrentUserId}");
                return new SendDeleteCommandResult(false, "用户没有活动连接");
            }
            var fileDeleteRequest = new SendDeleteRequest
            (
                 command.FilePath,
                 command.ContentToRemove + Environment.NewLine,
                 command.LogMessage
            );
            var sendTasks = userConnections.Select(c =>
                HubContext.Clients.Client(c.ConnectionId)
                    .SendAsync("ReceiveDeleteCommand", fileDeleteRequest, cancellationToken));
            await Task.WhenAll(sendTasks);
            return new SendDeleteCommandResult(true, "删除命令已发送至所有活动客户端");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "向客户端发送删除命令时发生错误. UserId: {UserId}", command.CurrentUserId);
            return new SendDeleteCommandResult(false, "发送删除命令时发生错误");
        }
    }
}