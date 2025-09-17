
namespace MembershipSystemAPI.Endpoints.Admins;

public class ToggleUserStatus : Endpoint<ToggleStatusRequest,ToggleStatusResponse>
{
    private readonly ILogger<ToggleUserStatus> _logger;
    private readonly MemDbContext _dbContext ;

    public ToggleUserStatus(ILogger<ToggleUserStatus> logger, MemDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }


    public override void Configure()
    {
        Put("/admin/{UserId}/toggle-status");
        Roles("Admin");
        Summary(s =>
        {
            s.Summary = "切换用户激活状态";
            s.Description = "管理员可以通过此接口切换指定用户的激活状态（启用/禁用）。";
            s.Responses[200] = "成功切换用户状态";
            s.Responses[400] = "请求无效，例如用户ID不存在";
            s.Responses[401] = "未授权访问";
            s.Responses[403] = "禁止访问，当前用户没有管理员权限";
        });
    }

    public override async Task HandleAsync(ToggleStatusRequest req,CancellationToken ct)
    {
        var userIdToToggle = Route<Guid>("UserId");
        var userToToggle = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdToToggle, ct);

        if (userToToggle == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (currentUserId == userIdToToggle.ToString())
        {
            _logger.LogWarning("管理员尝试禁用自己，操作被阻止。");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        userToToggle.IsActive = req.IsActive;
        await _dbContext.SaveChangesAsync(ct);
        var response = new ToggleStatusResponse
        {
            UserId = userToToggle.Id,
            IsActive = userToToggle.IsActive,
            Message = userToToggle.IsActive ? "用户已启用" : "用户已禁用"
        };
        await Send.OkAsync(response, ct);
    }
}

public class ToggleStatusRequest
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
}

public class ToggleStatusResponse
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
    public string Message { get; set; } = string.Empty;
}
