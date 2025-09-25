namespace MembershipSystemAPI.Endpoints.Users;

[EnableRateLimiting("register-policy")]
public class RegisterEndpoint : Endpoint<RegisterRequest, EmptyResponse>
{
    private readonly MemDbContext _dbContext;

    public RegisterEndpoint(MemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    override public void Configure()
    {
        Post("/user/register");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "用户注册";
            s.Description = "允许新用户注册账号";
            s.Responses[200] = "注册成功";
            s.Responses[409] = "用户名已存在";
        });
    }
    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var userExists = _dbContext.Users.Any(u => u.Username == req.Username);
        if (userExists)
        {
            await Send.ErrorsAsync(409, ct);
            return;
        }
        var newUser = new User
        {
            Username = req.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            ApiKey = new ApiKey()
        };
        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync(ct);
        await Send.OkAsync(ct);
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
