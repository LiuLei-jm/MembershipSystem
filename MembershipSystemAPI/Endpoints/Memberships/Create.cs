
using MembershipSystemAPI.Data;
using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Hubs;
using MembershipSystemAPI.Services;

namespace MembershipSystemAPI.Endpoints.Memberships;

public class CreateMembershipEndpoint : Endpoint<CreateMembershipRequest, CreateMembershipResponse>
{
    private readonly IMediator _mediator;
    public CreateMembershipEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("membership/create");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "创建会员";
            s.Description = "用户可以通过此接口创建新的会员。";
            s.Responses[200] = "成功创建会员";
            s.Responses[400] = "请求无效，例如缺少必需字段或字段格式错误";
            s.Responses[401] = "未授权访问";
        });
    }

    public override async Task HandleAsync(CreateMembershipRequest req, CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            await Send.ResponseAsync(new CreateMembershipResponse
            (
                false,
                Guid.Empty,
                string.Empty,
                DateTimeOffset.MinValue,
                 "无效的用户标识"
            ), 401, ct);
        }
        var command = new CreateMembershipCommand
        (
            currentUserGuid,
            req.MembershipName,
            req.Cdk,
            req.DurationInDays,
            req.Amount,
            req.StartTime,
            req.Notes
        );
        var result =  await _mediator.Send(command, ct);
        if (result.Success)
        {
            await Send.OkAsync(new CreateMembershipResponse
            (
                result.Success,
                result.CardId,
                result.Cdk,
                result.EndTime,
                 result.Message
            ), ct);
        }
        else
        {
            await Send.ResponseAsync(new CreateMembershipResponse
            (
                result.Success,
                Guid.Empty,
                string.Empty,
                DateTimeOffset.MinValue,
                 result.Message
            ), 400, ct);
        }
    }
}

