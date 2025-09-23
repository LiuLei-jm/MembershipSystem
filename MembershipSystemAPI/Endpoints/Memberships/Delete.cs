
using MembershipSystemAPI.Endpoints.FilePushes;

namespace MembershipSystemAPI.Endpoints.Memberships;

public class DeleteMembershipEndpoint : Endpoint<DeleteMembershipRequest, DeleteMembershipResponse>
{
    private readonly IHubContext<FilePushHub> _hubContext;
    private readonly MemDbContext _dbContext;

    public DeleteMembershipEndpoint(MemDbContext dbContext, IHubContext<FilePushHub> hubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
    }
    public override void Configure()
    {
        Delete("/membership/{cardId}");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "删除会员";
            s.Description = "用户可以通过此接口删除指定的会员。";
            s.Responses[200] = "成功删除会员";
            s.Responses[400] = "请求无效，例如缺少必需字段或字段格式错误";
            s.Responses[401] = "未授权访问";
        });
    }
    public override async Task HandleAsync(DeleteMembershipRequest req, CancellationToken ct)
    {
        var response = new DeleteMembershipResponse();
        var cardId = Route<Guid>("cardId");
        if (cardId == Guid.Empty)
        {
            response.Message = "无效的会员卡ID:{cardId}";
            await Send.ResponseAsync(response, 404, ct);
            return;
        }
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            response.Message = $"用户不存在：{currentUserId}";
            await Send.ResponseAsync(response, 404, ct);
            return;
        }
        var card = await _dbContext.MembershipCards.FirstOrDefaultAsync(c => c.Id == cardId && c.User.Id == currentUserGuid);
        if (card == null)
        {
            response.Message = $"未找到指定的会员卡：{cardId}";
            await Send.ResponseAsync(response, 404, ct);
            return;
        }

        var apiKeyObj = await _dbContext.ApiKeys.Include(a => a.User).FirstOrDefaultAsync(a => a.UserId == currentUserGuid, ct);
        if (apiKeyObj != null)
        { 
            var command = new SendDeleteRequest
            {
                FilePath = Path.Combine(apiKeyObj.User.MembershipCardPath, "CDK.txt"),
                ContentToRemove = card.Cdk,
                LogMessage = $"删除会员卡 {card.Cdk} "
            };
            await _hubContext.Clients.User(apiKeyObj.Key).SendAsync("ReceiveDeleteCommand", command);
        }
        _dbContext.MembershipCards.Remove(card);
        await _dbContext.SaveChangesAsync(ct);
        response.Message = "会员卡删除成功";
        await Send.OkAsync(response, ct);
    }
}

public class DeleteMembershipRequest
{
    public Guid CardId { get; set; }
}

public class DeleteMembershipResponse
{
    public string Message { get; set; } = string.Empty;
}
