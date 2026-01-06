using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.FilePushes;

public class SendDeleteCommandEndpoint : Endpoint<SendDeleteRequest, SendDeleteResponse>
{
    private readonly IMediator _mediator;
    public SendDeleteCommandEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("file-push/delete");
        Roles("User", "Admin");
        Summary(s =>
                {
                    s.Summary = "向当前用户的客户端发送文件删除指令";
                    s.Description = "此端点需要用户认证。它会查找与当前用户关联的 API Key，并向使用该 Key 连接的 SignalR 客户端发送一条文件删除命令。";
                });
    }

    public override async Task HandleAsync(SendDeleteRequest req, CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var command = new SendDeleteCommand(currentUserGuid, req.FilePath, req.ContentToRemove, req.LogMessage);
        var result = await _mediator.Send(command, ct);
        if (result.Success)
        {
            await Send.OkAsync(new SendDeleteResponse
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

