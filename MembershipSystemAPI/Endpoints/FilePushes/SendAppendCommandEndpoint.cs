using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.FilePushes;

public class SendAppendCommandEndpoint : Endpoint<SendAppendRequest, SendAppendResponse>
{
    private readonly IMediator _mediator;
    public SendAppendCommandEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("file-push/append");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "向当前用户的客户端发送文件追加指令";
            s.Description = "此端点需要用户认证。它会查找与当前用户关联的 API Key，并向使用该 Key 连接的 SignalR 客户端发送一条文件追加命令。";
        });
    }
    public override async Task HandleAsync(SendAppendRequest req, CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var command = new SendAppendCommand(
            CurrentUserId: currentUserGuid,
            FilePath: req.FilePath,
            Content: req.Content,
            LogMessage: req.LogMessage
        );
        var result = await _mediator.Send(command, ct);
        if (result.Success)
        {
            await Send.OkAsync(new SendAppendResponse
            (
                result.Success, result.Message
            ), ct);
            return;
        }
        else
        {
            await Send.ForbiddenAsync(ct);
            return;
        }
    }
}

