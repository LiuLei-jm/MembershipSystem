namespace MembershipSystemAPI.Endpoints.Memberships;

public class GetAllCardsEndpoint : Endpoint<EmptyRequest, List<MembershipCard>>
{
    private readonly MemDbContext _dbContext;

    public GetAllCardsEndpoint(MemDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public override void Configure()
    {
        Get("/membership/cards");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "获取当前用户的所有会员卡";
            s.Description = "此端点需要用户认证。它会查找与当前用户关联的所有会员卡，并返回这些卡的信息列表。如果用户没有任何会员卡，则返回一个空列表。";
            s.Responses[200] = "成功返回会员卡列表";
            s.Responses[401] = "未授权的访问";
        });
    }
    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("UserId");
        var userId = Guid.Parse(userIdClaim!.Value);
        var cards = await _dbContext.MembershipCards
            .Where(c => c.UserId == userId)
            .ToListAsync(ct);
        var orderedCards = cards.OrderByDescending(c => c.StartTime).ToList();
        await Send.OkAsync(orderedCards, ct);
    }
}


