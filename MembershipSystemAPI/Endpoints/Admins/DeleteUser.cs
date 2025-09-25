
namespace MembershipSystemAPI.Endpoints.Admins;

public class DeleteUser : Endpoint<DeleteUserRequest, DeleteUserResponse>
{
    private readonly ILogger<DeleteUser> _logger;
    private readonly MemDbContext _dbContext;

    public DeleteUser(ILogger<DeleteUser> logger, MemDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete("/admin/{UserId}");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "删除用户";
            s.Description = "管理员可以通过此接口删除指定用户。";
            s.Responses[200] = "成功删除用户";
            s.Responses[400] = "请求无效，例如用户ID不存在";
            s.Responses[401] = "未授权访问";
            s.Responses[403] = "禁止访问，当前用户没有管理员权限";
        });
    }
    public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
    {
        var userToDelete = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == req.UserId);
        if (userToDelete == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (currentUserId == req.UserId.ToString())
        {
            _logger.LogWarning("管理员尝试删除自己，操作被阻止。");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        _dbContext.Users.Remove(userToDelete);
        await _dbContext.SaveChangesAsync(ct);
        var response = new DeleteUserResponse
        {
            UserId = userToDelete.Id,
            Message = "用户已删除"
        };
        await Send.OkAsync(response, ct);
    }
}

public class DeleteUserRequest
{
    public Guid UserId { get; set; }
}

public class DeleteUserResponse
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
}
