
using MembershipSystemAPI.Endpoints.FilePushes;

namespace MembershipSystemAPI.Endpoints.Memberships;

public class CreateMembershipEndpoint : Endpoint<CreateMembershipRequest, CreateMembershipResponse>
{
    private readonly IHubContext<FilePushHub> _hubContext;
    private readonly MemDbContext _dbContext;
    private readonly ILogger<CreateMembershipEndpoint> _logger;
    private readonly ICdkService _cdkService;

    public CreateMembershipEndpoint(MemDbContext dbContext, ILogger<CreateMembershipEndpoint> logger, ICdkService cdkService, IHubContext<FilePushHub> hubContext)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cdkService = cdkService;
        _hubContext = hubContext;
    }

    public override void Configure()
    {
        Post("membership/create");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "创建会员";
            s.Description = "用户可以通过此接口创建新的会员。";
            s.Responses[200] = "成功创建会员";
            s.Responses[400] = "请求无效，例如缺少必需字段或字段格式错误";
            s.Responses[401] = "未授权访问";
        });
    }

    public override async Task HandleAsync(CreateMembershipRequest req, CancellationToken ct)
    {
        var response = new CreateMembershipResponse();
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            response.Message = "未能获取当前用户ID";
            await Send.ResponseAsync(response, 401, ct);
            return;
        }
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == currentUserGuid, ct);
        if (user is null)
        {
            response.Message = "用户不存在";
            await Send.ResponseAsync(response, 401, ct);
            return;
        }

        var cdk = _cdkService.GenerateCdk(req.Amount);
        var endTime = req.StartTime.AddDays(req.DurationInDays);
        var newCard = new MembershipCard
        {
            UserId = user.Id,
            MembershipName = req.MembershipName,
            DurationInDays = req.DurationInDays,
            Cdk = cdk,
            Amount = req.Amount,
            StartTime = req.StartTime,
            EndTime = endTime,
            Notes = req.Notes!
        };
        await _dbContext.MembershipCards.AddAsync(newCard, ct);
        await _dbContext.SaveChangesAsync(ct);

        var apiKeyObj = await _dbContext.ApiKeys.FirstOrDefaultAsync(a => a.UserId == currentUserGuid, ct);
        if (apiKeyObj != null)
        {
            var command = new SendAppendRequest
            {
                FilePath = Path.Combine(req.CdkFilePath, "CDK.txt"),
                Content = cdk + Environment.NewLine,
                LogMessage = $"写入新的会员CDK: {cdk}"
            };
            await _hubContext.Clients.User(apiKeyObj.Key).SendAsync("ReceiveWriteCommand", command, ct);
        }

        response.Id = newCard.Id;
        response.Cdk = newCard.Cdk;
        response.EndTime = newCard.EndTime;
        response.Message = "会员卡创建成功";
        await Send.OkAsync(response, ct);
    }
}

public class CreateMembershipRequest
{
    public string MembershipName { get; set; } = string.Empty;
    public int DurationInDays { get; set; }
    public decimal Amount { get; set; }
    public string CdkFilePath { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    public string? Notes { get; set; }
}

public class CreateMembershipResponse
{
    public Guid Id { get; set; }
    public string Cdk { get; set; } = string.Empty;
    public DateTimeOffset EndTime { get; set; }
    public string Message { get; set; } = string.Empty;
}
