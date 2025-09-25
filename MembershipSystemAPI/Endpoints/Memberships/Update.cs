
namespace MembershipSystemAPI.Endpoints.Memberships;

public class UpdateMembershipEndpoint : Endpoint<UpdateMembershipRequest, EmptyResponse>
{
    private readonly MemDbContext _dbContext;
    private readonly ILogger<UpdateMembershipEndpoint> _logger;
    private readonly ICdkService _cdkService;

    public UpdateMembershipEndpoint(MemDbContext dbContext, ILogger<UpdateMembershipEndpoint> logger, ICdkService cdkService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cdkService = cdkService;
    }
    public override void Configure()
    {
        Put("membership/{cardId:guid}/update");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "更新会员信息";
            s.Description = "用户可以通过此接口更新现有会员的信息，如续费或修改备注。";
            s.Responses[200] = "成功更新会员信息";
            s.Responses[400] = "请求无效，例如缺少必需字段或字段格式错误";
            s.Responses[401] = "未授权访问";
            s.Responses[404] = "未找到指定的会员";
        });
    }
    public override async Task HandleAsync(UpdateMembershipRequest req, CancellationToken ct)
    {
        var cardId = Route<Guid>("cardId");
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        var card = await _dbContext.MembershipCards.FirstOrDefaultAsync(c => c.Id == cardId && c.UserId.ToString() == currentUserId);
        if (card == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        if (req.DurationInDays is not null && req.DurationInDays > 0)
        {
            card.DurationInDays = req.DurationInDays.Value;
            card.EndTime = card.StartTime.AddDays(req.DurationInDays.Value);
        }
        if (req.Amount is not null && req.Amount > 0)
        {
            card.Amount = req.Amount.Value;
        }
        if (req.StartTime is not null)
        {
            card.StartTime = req.StartTime.Value;
            card.EndTime = card.StartTime.AddDays(card.DurationInDays);
        }
        if (!string.IsNullOrWhiteSpace(req.Notes))
        {
            card.Notes = req.Notes;
        }
        await _dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new EmptyResponse(), ct);
    }
}

public class UpdateMembershipRequest
{
    public int? DurationInDays { get; set; }
    public decimal? Amount { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public string? Notes { get; set; }
}
