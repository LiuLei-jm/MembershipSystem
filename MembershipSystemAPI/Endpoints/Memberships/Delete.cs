
using MembershipSystemAPI.CQRS.MediatRCommands;
using MembershipSystemAPI.DTOs;
using MediatR;

namespace MembershipSystemAPI.Endpoints.Memberships;

public class DeleteMembershipEndpoint : Endpoint<DeleteMembershipRequest, DeleteMembershipResponse>
{
    private readonly IMediator _mediator;

    public DeleteMembershipEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    public override void Configure()
    {
        Delete("/membership/{cardId}");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "删除会员";
            s.Description = "用户可以通过此接口删除指定的会员。";
            s.Responses[200] = "成功删除会员";
            s.Responses[400] = "请求无效，例如缺少必需字段或字段格式错误";
            s.Responses[401] = "未授权访问";
        });
    }
    public override async Task HandleAsync(DeleteMembershipRequest req, CancellationToken ct)
    {
        var cardId = Route<Guid>("cardId");
        if (cardId == Guid.Empty)
        {
            await Send.ResponseAsync(new DeleteMembershipResponse("无效的会员卡ID:{cardId}"), 404, ct);
            return;
        }
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            await Send.ResponseAsync(new DeleteMembershipResponse($"用户不存在：{currentUserId}"), 404, ct);
            return;
        }

        var command = new DeleteMembershipCommand(
            CardId: cardId,
            UserId: currentUserGuid
        );

        var result = await _mediator.Send(command, ct);

        if (result.Success)
        {
            await Send.OkAsync(new DeleteMembershipResponse(result.Message), ct);
        }
        else
        {
            await Send.ResponseAsync(new DeleteMembershipResponse(result.Message), 404, ct);
        }
    }
}
