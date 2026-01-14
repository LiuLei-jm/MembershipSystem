
using MembershipSystemAPI.CQRS.MediatRQueries;
using MembershipSystemAPI.Domain.Entities;
using MediatR;
using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.Memberships;

public class GetUnexpiredCardsEndpoint : Endpoint<EmptyRequest, List<MembershipCardSummaryResponse>>
{
    private readonly IMediator _mediator;

    public GetUnexpiredCardsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    public override void Configure()
    {
        Get("membership/cards/unexpired");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "获取当前用户的所有未过期会员卡";
            s.Description = "此端点需要用户认证。它会查找与当前用户关联的所有未过期会员卡，并返回这些卡的信息列表。如果用户没有任何未过期会员卡，则返回一个空列表。";
            s.Responses[200] = "成功返回未过期会员卡列表";
            s.Responses[401] = "未授权的访问";
        });
    }
    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if(!Guid.TryParse(currentUserId, out Guid currentUserGuid))
        {
            await Send.ErrorsAsync( 401, ct);
            return;
        }

        var query = new GetUnexpiredMembershipCardsQuery(UserId: currentUserGuid);
        var cards = await _mediator.Send(query, ct);

        await Send.OkAsync(cards, ct);
    }
}
