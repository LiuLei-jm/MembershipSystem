using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.ApiKeys;

public class Generate : EndpointWithoutRequest<GenerateApiKeyResponse>
{
    private readonly IMediator _mediator;

    public Generate(IMediator mediator)
    {
        _mediator = mediator;
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

        try
        {
            var command = new GenerateApiKeyCommand(userId);
            var result = await _mediator.Send(command, ct);

            await Send.OkAsync(new GenerateApiKeyResponse(result.Key, result.CreatedAt), ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "生成 API Key 时发生错误");
            await Send.ErrorsAsync(500, ct);
            return;
        }
    }

}