
namespace MembershipSystemAPI.Endpoints.Users;

public class ChangePasswordEndpoint : Endpoint<ChangePasswordRequest, ChangePasswordResponse>
{
    private readonly MemDbContext _dbContext;
    private readonly ILogger<ChangePasswordEndpoint> _logger;

    public ChangePasswordEndpoint(MemDbContext dbContext, ILogger<ChangePasswordEndpoint> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/user/change-password");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "修改密码";
            s.Description = "允许用户自己修改密码";
            s.Responses[200] = "密码修改成功";
            s.Responses[400] = "请求无效，当前密码不正确或新密码不符合要求";
            s.Responses[401] = "未授权的访问";
        });
    }

    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        var response = new ChangePasswordResponse();
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        if (!Guid.TryParse(currentUserId, out var currentUserGuid))
        {
            _logger.LogWarning($"无效的用户 ID. {currentUserId}");
            response.UserId = Guid.Empty;
            response.Message = $"无效的用户 ID: {currentUserId}";
            await Send.ResponseAsync(response,400,ct);
            return;
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == currentUserGuid, ct);
        if (user == null)
        {
            _logger.LogWarning($"用户未找到. ID: {currentUserGuid}");
            response.UserId = currentUserGuid;
            response.Message = $"用户未找到: {currentUserGuid}";
            await Send.ResponseAsync(response,404,ct);
            return;
        }
        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
        {
            response.UserId = currentUserGuid;
            response.Message = "当前密码不正确";
            await Send.ResponseAsync(response, 400, ct);
            return;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await _dbContext.SaveChangesAsync(ct);
        response.UserId = user.Id;
        response.Message = "密码修改成功";
        await Send.OkAsync(response, ct);
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordResponse
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
}
