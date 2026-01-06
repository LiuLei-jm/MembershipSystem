using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Repositories;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.QueryHandlers;

public class GetAllMembershipCardsQueryHandler : IRequestHandler<GetAllMembershipCardsQuery, List<MembershipCardSummaryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllMembershipCardsQueryHandler> _logger;

    public GetAllMembershipCardsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllMembershipCardsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<MembershipCardSummaryResponse>> Handle(GetAllMembershipCardsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var cards = await _unitOfWork.MembershipCards.GetByUserIdAsync(query.UserId);

            var orderedCards = cards.Select(c => new MembershipCardSummaryResponse(c.Id, c.MembershipName, c.DurationInDays, c.StartTime, c.EndTime, c.Amount, c.Cdk, c.IsExpired)).OrderByDescending(c => c.StartTime).ToList();

            _logger.LogInformation("成功获取用户的所有会员卡. UserId: {UserId}, Count: {Count}", query.UserId, orderedCards.Count);
            return orderedCards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户会员卡时发生错误. UserId: {UserId}", query.UserId);
            return new List<MembershipCardSummaryResponse>();
        }
    }
}

public class GetUnexpiredMembershipCardsQueryHandler : IRequestHandler<GetUnexpiredMembershipCardsQuery, List<MembershipCardSummaryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetUnexpiredMembershipCardsQueryHandler> _logger;

    public GetUnexpiredMembershipCardsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetUnexpiredMembershipCardsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<MembershipCardSummaryResponse>> Handle(GetUnexpiredMembershipCardsQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var cards = await _unitOfWork.MembershipCards.GetByUserIdAsync(query.UserId);

            var unexpiredCards = cards.Select(c => new MembershipCardSummaryResponse(c.Id, c.MembershipName,c.DurationInDays, c.StartTime, c.EndTime, c.Amount, c.Cdk, c.IsExpired)).Where(c => c.EndTime > DateTimeOffset.Now).ToList();
            if (unexpiredCards.Count() == 0)
            {
                _logger.LogInformation("用户没有未过期的会员卡. UserId: {UserId}", query.UserId);
                return new List<MembershipCardSummaryResponse>();
            }

            return unexpiredCards.Select(c => new MembershipCardSummaryResponse(c.Id, c.MembershipName,c.durationInDays, c.StartTime, c.EndTime, c.Amount, c.Cdk, c.IsExpired)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户未过期会员卡时发生错误. UserId: {UserId}", query.UserId);
            return new List<MembershipCardSummaryResponse>();
        }
    }
}