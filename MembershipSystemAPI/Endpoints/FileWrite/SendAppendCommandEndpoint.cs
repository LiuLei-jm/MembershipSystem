using FastEndpoints;
using MembershipSystemAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MembershipSystemAPI.Endpoints.FileWrite;

public class SendAppendCommandEndpoint : Endpoint<SendAppendRequest, SendAppendResponse>
{
    public IHubContext<CommandHub> HubContext { get; set; } = null!;
    public override void Configure()
    {
        Post("api/append");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "触发向客户端发送文件写入指令";
            s.Description = "此端点会向所有连接的 SignalR 客户端广播一个文件写入命令。";
        });
    }
    public override async Task HandleAsync(SendAppendRequest req, CancellationToken ct)
    {
        var fileName = req.FilePath;

        var filePath = Path.Combine("E:", "Temp", fileName);
        var command = new SendAppendRequest
        {
            FilePath = filePath,
            Content = req.Content + Environment.NewLine,
            LogMessage = req.LogMessage
        };
        await HubContext.Clients.All.SendAsync("ReceiveWriteCommand", command, ct);
        await Send.OkAsync(new SendAppendResponse
        {
            Message = "命令已发送至所有连接的客户端."
        }, ct);
    }
}

public class SendAppendRequest
{
    public required string FilePath { get; set; }
    public required string Content { get; set; }
    public required string LogMessage { get; set; }
}

public class SendAppendResponse
{
    public required string Message { get; set; }
}
