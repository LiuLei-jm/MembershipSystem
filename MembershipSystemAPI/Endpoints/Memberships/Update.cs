
using MembershipSystemAPI.CQRS.MediatRCommands;
using MembershipSystemAPI.DTOs;
using MediatR;

namespace MembershipSystemAPI.Endpoints.Memberships;

public class UpdateMembershipEndpoint : Endpoint<UpdateMembershipRequest, EmptyResponse>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateMembershipEndpoint> _logger;

    public UpdateMembershipEndpoint(IMediator mediator, ILogger<UpdateMembershipEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    public override void Configure()
    {
        Put("membership/{cardId:guid}/update");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "更新会员信息";
            s.Description = "用户可以通过此接口更新现有会员的信息，如续费或修改备注。";
            s.Responses[200] = "成功更新会员信息";
            s.Responses[400] = "请求无效，例如缺少必需字段或字段格式错误";
            s.Responses[401] = "未授权访问";
            s.Responses[404] = "未找到指定的会员";
        });
    }
    public override async Task HandleAsync(UpdateMembershipRequest req, CancellationToken ct)
    {
        var cardId = Route<Guid>("cardId");
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var command = new UpdateMembershipCommand(
            CardId: cardId,
            UserId: currentUserGuid,
            DurationInDays: req.DurationInDays,
            Amount: req.Amount,
            StartTime: req.StartTime,
            Notes: req.Notes
        );

        var result = await _mediator.Send(command, ct);

        if (result.Success)
        {
            await Send.OkAsync(new EmptyResponse(), ct);
        }
        else
        {
            await Send.NotFoundAsync(ct);
        }
    }
}
