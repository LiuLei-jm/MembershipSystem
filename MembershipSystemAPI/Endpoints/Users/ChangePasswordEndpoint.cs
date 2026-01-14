using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.Users;

public class ChangePasswordEndpoint : Endpoint<ChangePasswordRequest, ChangePasswordResponse>
{
    private readonly IMediator _mediator;
    public ChangePasswordEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/user/change-password");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "修改密码";
            s.Description = "允许用户自己修改密码";
            s.Responses[200] = "密码修改成功";
            s.Responses[400] = "请求无效，当前密码不正确或新密码不符合要求";
            s.Responses[401] = "未授权的访问";
        });
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (Guid.TryParse(currentUserId, out var userId))
        {
            if (userId == Guid.Empty)
            {
                await Send.UnauthorizedAsync(ct);
                return;
            }
        }
        else
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        try
        {
            var command = new ChangePasswordCommand(userId, req.CurrentPassword, req.NewPassword);
            var result = await _mediator.Send(command, ct);
            if (!result)
            {
                await Send.ResponseAsync(new ChangePasswordResponse("原密码错误"), 400, ct);
                return;
            }
            await Send.OkAsync(new ChangePasswordResponse("密码修改成功"), ct);
        }
        catch (Exception ex)
        {
            var response = new ChangePasswordResponse($"密码修改失败:{ex.Message}");
            await Send.ResponseAsync(response, 500, ct);
            return;
        }
    }
}


