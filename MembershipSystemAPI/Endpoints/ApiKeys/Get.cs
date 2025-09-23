namespace MembershipSystemAPI.Endpoints.ApiKeys;

public class Get : EndpointWithoutRequest<GetApiKeyResponse>
{
    private readonly MemDbContext _dbContext;

    public Get(MemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/user/apikey");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "获取当前用户的 API Key";
            s.Description = "此端点需要用户认证。它会查找与当前用户关联的 API Key，并返回该 Key 及其创建时间。如果用户没有 API Key，则返回 404 状态码。";
            s.Responses[200] = "成功返回 API Key 及其创建时间";
            s.Responses[401] = "未授权的访问";
            s.Responses[404] = "未找到与当前用户关联的 API Key";
        });
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("UserId");
        var userId = Guid.Parse(userIdClaim!.Value);

        var apiKey = await _dbContext.ApiKeys.Where(k => k.UserId == userId)
            .Select(a => new GetApiKeyResponse
            {
                ApiKey = a.Key,
                CreatedAt = a.CreatedAt.ToString("o")
            }).FirstOrDefaultAsync(ct);
        if (apiKey == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(apiKey, ct);
    }

}

public class GetApiKeyResponse
{
    public string ApiKey { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}
