using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.Admins;

public class DeleteUser : Endpoint<DeleteUserRequest, DeleteUserResponse>
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteUser> _logger;

    public DeleteUser(IMediator mediator, ILogger<DeleteUser> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Delete("/admin/{UserId}");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "删除用户";
            s.Description = "管理员可以通过此接口删除指定用户。";
            s.Responses[200] = "成功删除用户";
            s.Responses[400] = "请求无效，例如用户ID不存在";
            s.Responses[401] = "未授权访问";
            s.Responses[403] = "禁止访问，当前用户没有管理员权限";
        });
    }
    public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
    {
        try
        {
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (currentUserId == req.UserId.ToString())
            {
                _logger.LogWarning("管理员尝试删除自己，操作被阻止。");
                await Send.ErrorsAsync(400, ct);
                return;
            }
            var command = new DeleteUserCommand(req.UserId);
            var result = await _mediator.Send(command, ct);
            if (!result.Success)
            {
                _logger.LogWarning("尝试删除不存在的用户，用户ID: {UserId}", req.UserId);
                await Send.NotFoundAsync(ct);
            }
            await Send.OkAsync(new DeleteUserResponse
            (
                 req.UserId,
                 result.Message
            ), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理删除用户请求时出错");
            await Send.ErrorsAsync(500, ct);
            return;
        }
    }
}
