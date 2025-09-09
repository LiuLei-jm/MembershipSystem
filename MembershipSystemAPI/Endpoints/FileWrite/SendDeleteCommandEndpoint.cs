using FastEndpoints;
using MembershipSystemAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MembershipSystemAPI.Endpoints.FileWrite;

public class SendDeleteCommandEndpoint : Endpoint<SendDeleteRequest, SendDeleteResponse>
{
    public IHubContext<CommandHub> HubContext { get; set; } = null!;
    public override void Configure()
    {
        Post("api/delete");
        AllowAnonymous();
        Summary(s =>
                {
                    s.Summary = "触发向客户端发送文件删除指令";
                    s.Description = "此端点会向所有连接的 SignalR 客户端广播一个文件删除命令。";
                });
    }

    public override async Task HandleAsync(SendDeleteRequest req, CancellationToken ct)
    {
        var fileName = req.FilePath;

        var filePath = Path.Combine("E:", "Temp", fileName);
        var command = new SendDeleteRequest
        {
            FilePath = filePath,
            ContentToRemove = req.ContentToRemove + Environment.NewLine,
            LogMessage = req.LogMessage
        };
        await HubContext.Clients.All.SendAsync("ReceiveDeleteCommand", command, ct);
        await Send.OkAsync(new SendDeleteResponse
        {
            Message = "命令已发送至所有连接的客户端."
        }, ct);

    }
}

public class SendDeleteRequest
{
    public required string FilePath { get; set; }
    public required string ContentToRemove { get; set; }
    public required string LogMessage { get; set; }
}
public class SendDeleteResponse
{
    public required string Message { get; set; }
}
