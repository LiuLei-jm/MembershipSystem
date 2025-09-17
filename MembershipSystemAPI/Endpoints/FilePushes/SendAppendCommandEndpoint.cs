namespace MembershipSystemAPI.Endpoints.FilePushes;

public class SendAppendCommandEndpoint : Endpoint<SendAppendRequest, SendAppendResponse>
{
    public IHubContext<FilePushHub> HubContext { get; set; } = null!;
    private readonly MemDbContext _dbContext;
    public SendAppendCommandEndpoint(MemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("file-push/append");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "向当前用户的客户端发送文件删除指令";
            s.Description = "此端点需要用户认证。它会查找与当前用户关联的 API Key，并向使用该 Key 连接的 SignalR 客户端发送一条文件删除命令。";
        });
    }
    public override async Task HandleAsync(SendAppendRequest req, CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

        // Convert currentUserId to Guid before comparison
        if (Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            var apiKeyObj = await _dbContext.ApiKeys.FirstOrDefaultAsync(k => k.UserId == currentUserGuid);
            var apiKey = apiKeyObj?.Key;
            if (string.IsNullOrEmpty(apiKey))
            {
                await Send.NotFoundAsync(ct);
                return;
            }
            var fileName = req.FilePath;

            var command = new SendAppendRequest
            {
                FilePath = req.FilePath,
                Content = req.Content + Environment.NewLine,
                LogMessage = req.LogMessage
            };
            await HubContext.Clients.User(apiKey).SendAsync("ReceiveWriteCommand", command, ct);
            await Send.OkAsync(new SendAppendResponse
            {
                Message = "命令已发送至与你的 API Key 关联的客户端."
            }, ct);
        }
        else
        {
            await Send.StringAsync("无效的用户 ID.", 400);
        }
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
