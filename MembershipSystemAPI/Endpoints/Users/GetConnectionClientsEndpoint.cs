namespace MembershipSystemAPI.Endpoints.Users;

using MediatR;
using MembershipSystemAPI.CQRS.MediatRQueries;
using System.Threading;
using System.Threading.Tasks;
using ConnectionInfo = MembershipSystemAPI.Domain.Entities.ConnectionInfo;

public class GetConnectionClientsEndpoint : EndpointWithoutRequest<List<ConnectionInfo>>
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetConnectionClientsEndpoint> _logger;

    public GetConnectionClientsEndpoint(IMediator mediator, ILogger<GetConnectionClientsEndpoint> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("user/connections");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "获取当前用户的所有连接客户端信息";
            s.Description = "此端点需要用户认证。它会查找与当前用户关联的 API Key，并返回使用该 Key 连接的所有 SignalR 客户端的信息列表。";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            _logger.LogWarning($"无效的用户 ID. {currentUserId}");
            await Send.UnauthorizedAsync(ct);
            return;
        }

        try
        {
            var query = new GetUserConnectionClientsQuery(currentUserGuid);
            var connectedClients = await _mediator.Send(query, ct);
            await Send.OkAsync(connectedClients.ToList(), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户连接客户端时发生错误，用户ID: {UserId}", currentUserId);
            await Send.ErrorsAsync(500, ct);
        }
    }
}
