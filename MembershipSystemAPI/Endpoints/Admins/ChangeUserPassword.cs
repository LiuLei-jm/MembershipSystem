
namespace MembershipSystemAPI.Endpoints.Admins;

public class ChangeUserPassword : Endpoint<ChangeUserPasswordRequest, ChangeUserPasswordResponse>
{
    private readonly MemDbContext _dbContext ;

    public ChangeUserPassword(MemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Put("/admin/{UserId}/change-password");
        Roles("Admin");
        Summary(s =>
        {
            s.Summary = "更改用户密码";
            s.Description = "管理员可以通过此接口更改指定用户的密码。";
            s.Responses[200] = "成功更改用户密码";
            s.Responses[400] = "请求无效，例如用户ID不存在或新密码不符合要求";
            s.Responses[401] = "未授权访问";
            s.Responses[403] = "禁止访问，当前用户没有管理员权限";
        });
    }
    public override async Task HandleAsync(ChangeUserPasswordRequest req, CancellationToken ct)
    {
        var userToChange = _dbContext.Users.FirstOrDefault(u => u.Id == req.UserId);
        if (userToChange == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }


        if (string.IsNullOrEmpty(req.NewPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
        {
            req.NewPassword = "88888888";
        }

        if (req.NewPassword.Length < 6)
        {
            await Send.ErrorsAsync(400, ct);
            return;
        }

        userToChange.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await _dbContext.SaveChangesAsync(ct);
        var response = new ChangeUserPasswordResponse
        {
            UserId = userToChange.Id,
            Message = "用户密码已更改"
        };
        await Send.OkAsync(response, ct);
    }
}

public class ChangeUserPasswordRequest
{
    public Guid UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangeUserPasswordResponse
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
}
