namespace MembershipSystemAPI.Services;

public class ApiKeyBasedUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // 1. 从连接的查询字符串中获取 "apiKey"
        var apiKey = connection.GetHttpContext()?.Request.Query["apiKey"].ToString();
        var deviceName = connection.GetHttpContext()?.Request.Query["deviceName"].ToString() ?? "Unknown Device";
        if (string.IsNullOrEmpty(apiKey))
        {
            // 如果没有提供 apiKey，则该连接是匿名的，返回 null
            return null;
        }
        // 2. 将 apiKey "暂存" 在连接的上下文项中，以便 Hub 稍后使用
        // 这样做的好处是避免在 Hub 中再次解析 HttpContext
        connection.Items["ApiKey"] = apiKey;
        connection.Items["DeviceName"] = deviceName;
        // 【关键】: 我们在这里直接返回 apiKey 作为临时的 "UserId"
        // 为什么？因为在这个阶段直接查询数据库是复杂的（服务生命周期问题）。
        // SignalR 将使用这个返回的值（即 apiKey）作为 Clients.User() 的标识符。
        // 这意味着，当我们稍后调用 Clients.User("the_actual_api_key")... 时，SignalR 知道该推送到哪个连接。
        return apiKey;
    }
}
