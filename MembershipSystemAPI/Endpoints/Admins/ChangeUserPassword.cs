
using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.Admins;

public class ChangeUserPassword : Endpoint<ChangeUserPasswordRequest, ChangeUserPasswordResponse>
{
    private readonly IMediator _mediator;
    public ChangeUserPassword(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/admin/{UserId}/change-password");
        Roles("Admin");
        Summary(s =>
        {
            s.Summary = "更改用户密码";
            s.Description = "管理员可以通过此接口更改指定用户的密码。";
            s.Responses[200] = "成功更改用户密码";
            s.Responses[400] = "请求无效，例如用户ID不存在或新密码不符合要求";
            s.Responses[401] = "未授权访问";
            s.Responses[403] = "禁止访问，当前用户没有管理员权限";
        });
    }
    public override async Task HandleAsync(ChangeUserPasswordRequest req, CancellationToken ct)
    {
        try
        {
            var command = new ChangeUserPasswordCommand(req.UserId, req.NewPassword);
            var result = await _mediator.Send(command, ct);
            if (!result.Success)
            {
                await Send.ErrorsAsync(400, ct);
                return;
            }
            var response = new ChangeUserPasswordResponse(result.Success, result.Message);
            await Send.OkAsync(response, ct);
        }
        catch (Exception)
        {
            await Send.ErrorsAsync(500, ct);
        }
    }
}

