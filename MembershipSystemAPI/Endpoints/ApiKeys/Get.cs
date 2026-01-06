using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.ApiKeys;

public class Get : EndpointWithoutRequest<GetApiKeyResponse>
{
    private readonly IMediator _mediator;
    public Get(IMediator mediator)
    {
        _mediator = mediator;
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

        try
        {
            var query = new GetApiKeyQueries(userId);
            var apiKey = await _mediator.Send(query, ct);
            if (apiKey == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            await Send.OkAsync(apiKey, ct);

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "获取 API Key 时发生错误");
            await Send.ErrorsAsync(500, ct);
            return;
        }
    }

}

