using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.FilePushes;

public class PushToConnectionEndpoint : Endpoint<PushToConnectionRequest, PushToConnectionResponse>
{
    private readonly IMediator _mediator;

    public PushToConnectionEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/file-push/push-to-connection");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "向指定连接的客户端发送文件写入指令";
            s.Description = "此端点需要用户认证。它会查找与当前用户关联的 API Key，并向指定连接 ID 的 SignalR 客户端发送一条文件写入命令。";
        });
    }

    public override async Task HandleAsync(PushToConnectionRequest req, CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var command = new PushToConnectionCommand(
            CurrentUserId: currentUserGuid,
            TargetConnectionId: req.TargetConnectionId,
            FilePath: req.FilePath,
            Content: req.Content,
            LogMessage: req.LogMessage
        );

        var result = await _mediator.Send(command, ct);

        if (result.Success)
        {
            await Send.OkAsync(new PushToConnectionResponse
            (
                result.Success, result.Message
            ), ct);
        }
        else
        {
            await Send.ForbiddenAsync(ct);
        }
    }
}