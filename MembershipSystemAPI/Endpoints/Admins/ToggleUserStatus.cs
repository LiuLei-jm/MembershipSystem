using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.Admins;

public class ToggleUserStatus : Endpoint<ToggleStatusRequest, ToggleStatusResponse>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ToggleUserStatus> _logger;
    public ToggleUserStatus(IMediator mediator, ILogger<ToggleUserStatus> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/admin/{UserId}/toggle-status");
        Roles("Admin");
        Summary(s =>
        {
            s.Summary = "切换用户激活状态";
            s.Description = "管理员可以通过此接口切换指定用户的激活状态（启用/禁用）。";
            s.Responses[200] = "成功切换用户状态";
            s.Responses[400] = "请求无效，例如用户ID不存在";
            s.Responses[401] = "未授权访问";
            s.Responses[403] = "禁止访问，当前用户没有管理员权限";
        });
    }

    public override async Task HandleAsync(ToggleStatusRequest req, CancellationToken ct)
    {
        try
        {
            var command = new ToggleStatusCommand(req.UserId, req.IsActive);
            var result = await _mediator.Send(command, ct);
            if (!result.Success)
            {
                _logger.LogWarning("切换用户状态失败: {Message}", result.Message);
                await Send.ErrorsAsync(400, ct);
            }
            await Send.OkAsync(new ToggleStatusResponse(req.UserId, req.IsActive, result.Message), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切换用户状态时发生错误");
            await Send.ErrorsAsync(500, ct);
            return;
        }
    }
}

