
namespace MembershipSystemAPI.Endpoints.ApiKeys;

public class Generate : EndpointWithoutRequest<GenerateApiKeyResponse>
{
    private readonly MemDbContext _dbContext ;

    public Generate(MemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/user/apikey");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "生成或更新当前用户的 API Key";
            s.Description = "此端点需要用户认证。它会为当前用户生成一个新的 API Key。如果用户已经有一个 API Key，则会更新该 Key 并返回新的值。";
            s.Responses[200] = "成功返回新的 API Key";
            s.Responses[401] = "未授权的访问";
        });
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("UserId");
        var userId = Guid.Parse(userIdClaim!.Value);

        var existingKey = await _dbContext.ApiKeys.FirstOrDefaultAsync(k => k.UserId == userId, ct);

        if (existingKey != null)
        {
            existingKey.RegenerateKey();
            existingKey.CreatedAt = DateTime.Now;
        }
        else
        {
            var newKey = new ApiKey
            {
                UserId = userId,
            };
            _dbContext.ApiKeys.Add(newKey);
            existingKey = newKey;
        }
        var newKeyValue = existingKey.Key;
        await _dbContext.SaveChangesAsync(ct);
        await Send.OkAsync(new GenerateApiKeyResponse { ApiKey = newKeyValue }, ct);
    }

}

public class GenerateApiKeyResponse
{
    public string ApiKey { get; set; } = string.Empty;
}
