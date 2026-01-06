using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Hubs;
using MembershipSystemAPI.Repositories;
using MembershipSystemAPI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.CommandHandlers;

public class CreateMembershipCommandHandler : IRequestHandler<CreateMembershipCommand, CreateMembershipResult>
{
    public IHubContext<FilePushHub> HubContext { get; set; } = null!;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPathService _pathService;
    private readonly ILogger<CreateMembershipCommandHandler> _logger;
    private readonly ICdkService _cdkService;
    public CreateMembershipCommandHandler(
        IHubContext<FilePushHub> hubContext,
        IUnitOfWork unitOfWork,
        IPathService pathService,
        ILogger<CreateMembershipCommandHandler> logger,
        ICdkService cdkService)
    {
        HubContext = hubContext;
        _unitOfWork = unitOfWork;
        _pathService = pathService;
        _logger = logger;
        _cdkService = cdkService;
    }
    public async Task<CreateMembershipResult> Handle(CreateMembershipCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(command.UserId);
            if (user == null)
            {
                _logger.LogWarning("未找到指定的用户. UserId: {UserId}", command.UserId);
                return new CreateMembershipResult(false, Guid.Empty, string.Empty, DateTimeOffset.MinValue, "未找到指定的用户");
            }
            var cdk = string.IsNullOrWhiteSpace(command.Cdk) ? _cdkService.GenerateCdk(command.Amount) : command.Cdk;
            var membershipCard = Domain.Entities.MembershipCard.Create(command.MembershipName, command.DurationInDays, command.Amount, cdk, command.UserId, command.Notes);
            var apiKeyObj = await _unitOfWork.ApiKeys.GetByUserIdAsync(command.UserId);
            if (apiKeyObj != null)
            {
                var pathConfig = await _pathService.GetUserPathConfigurationAsync(command.UserId);
                var writeContentRequest = new SendAppendRequest
                (
                    Path.Combine(pathConfig.BasePath, pathConfig.MembershipCardFilePath),
                    membershipCard.Cdk + Environment.NewLine,
                    $"增加会员卡 {membershipCard.Cdk} "
                );

                await HubContext.Clients.User(apiKeyObj.Key).SendAsync("ReceiveWriteCommand", writeContentRequest, cancellationToken);

            }
            await _unitOfWork.MembershipCards.AddAsync(membershipCard);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("会员卡创建成功. CardId: {CardId}, UserId: {UserId}", membershipCard.Id, command.UserId);
            return new CreateMembershipResult(true, membershipCard.Id, membershipCard.Cdk, membershipCard.EndTime, "会员卡创建成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建会员卡时发生错误. UserId: {UserId}", command.UserId);
            return new CreateMembershipResult(false, Guid.Empty, string.Empty, DateTimeOffset.MinValue, "创建会员卡时发生错误");
        }
    }
}
public class DeleteMembershipCommandHandler : IRequestHandler<DeleteMembershipCommand, DeleteMembershipResult>
{
    public IHubContext<FilePushHub> HubContext { get; set; } = null!;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPathService _pathService;
    private readonly ILogger<DeleteMembershipCommandHandler> _logger;

    public DeleteMembershipCommandHandler(
        IHubContext<FilePushHub> hubContext,
        IUnitOfWork unitOfWork,
        IPathService pathService,
        ILogger<DeleteMembershipCommandHandler> logger)
    {
        HubContext = hubContext;
        _unitOfWork = unitOfWork;
        _pathService = pathService;
        _logger = logger;
    }

    public async Task<DeleteMembershipResult> Handle(DeleteMembershipCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var card = await _unitOfWork.MembershipCards.GetByIdAsync(command.CardId);
            if (card == null || card.UserId != command.UserId)
            {
                _logger.LogWarning("未找到指定的会员卡或无权限访问. CardId: {CardId}, UserId: {UserId}", command.CardId, command.UserId);
                return new DeleteMembershipResult(false, $"未找到指定的会员卡：{command.CardId}");
            }

            var apiKeyObj = await _unitOfWork.ApiKeys.GetByUserIdAsync(command.UserId);
            if (apiKeyObj != null)
            {
                var pathConfig = await _pathService.GetUserPathConfigurationAsync(command.UserId);
                var deleteContentRequest = new SendDeleteRequest
                (
                    Path.Combine(pathConfig.BasePath, pathConfig.MembershipCardFilePath),
                    card.Cdk,
                    $"删除会员卡 {card.Cdk} "
                );

                _logger.LogInformation("会员卡删除成功. CardId: {CardId}, UserId: {UserId}", command.CardId, command.UserId);
                await HubContext.Clients.User(apiKeyObj.Key).SendAsync("ReceiveDeleteCommand", deleteContentRequest, cancellationToken);
            }

            await _unitOfWork.MembershipCards.DeleteAsync(card.Id);
            await _unitOfWork.SaveChangesAsync();

            return new DeleteMembershipResult(true, "会员卡删除成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除会员卡时发生错误. CardId: {CardId}, UserId: {UserId}", command.CardId, command.UserId);
            return new DeleteMembershipResult(false, "删除会员卡时发生错误");
        }
    }
}

public class UpdateMembershipCommandHandler : IRequestHandler<UpdateMembershipCommand, UpdateMembershipResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateMembershipCommandHandler> _logger;

    public UpdateMembershipCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateMembershipCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateMembershipResult> Handle(UpdateMembershipCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var card = await _unitOfWork.MembershipCards.GetByIdAsync(command.CardId);
            if (card == null || card.UserId != command.UserId)
            {
                _logger.LogWarning("未找到指定的会员卡或无权限访问. CardId: {CardId}, UserId: {UserId}", command.CardId, command.UserId);
                return new UpdateMembershipResult(false, "未找到指定的会员卡");
            }

            if (command.DurationInDays is not null)
            {
                card.DurationInDays = command.DurationInDays.Value;
                card.EndTime = card.StartTime.AddDays(command.DurationInDays.Value);
            }

            if (command.Amount is not null)
            {
                card.Amount = command.Amount.Value;
            }

            if (command.StartTime is not null)
            {
                card.StartTime = command.StartTime.Value;
                card.EndTime = card.StartTime.AddDays(card.DurationInDays);
            }

            if (!string.IsNullOrWhiteSpace(command.Notes))
            {
                card.Notes = command.Notes;
            }


            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("会员卡更新成功. CardId: {CardId}, UserId: {UserId}", command.CardId, command.UserId);
            return new UpdateMembershipResult(true, "会员卡更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新会员卡时发生错误. CardId: {CardId}, UserId: {UserId}", command.CardId, command.UserId);
            return new UpdateMembershipResult(false, "更新会员卡时发生错误");
        }
    }
}