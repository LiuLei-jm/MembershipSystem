namespace MembershipSystemAPI.Endpoints.Admins;
using ConnectionInfo = MembershipSystemAPI.Models.ConnectionInfo;

public class GetAllConnectionClientsEndpoint : EndpointWithoutRequest<List<ConnectionInfo>>
{
    private readonly IConnectionManager _connectionManager;

    public GetAllConnectionClientsEndpoint(IConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public override void Configure()
    {
        Get("/admin/connections");
        Roles("Admin");
        Summary(s =>
        {
            s.Summary = "获取所有连接客户端信息";
            s.Description = "此端点需要管理员权限。它会返回系统中所有通过 API Key 连接的 SignalR 客户端的信息列表。";
            s.Responses[200] = "成功返回连接客户端信息列表";
            s.Responses[401] = "未授权的访问";
            s.Responses[403] = "禁止访问，缺少管理员权限";
        });
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var allConnections = _connectionManager.GetAllConnections().ToList();
        await Send.OkAsync(allConnections, ct);
    }
}
