namespace MembershipSystemAPI.Endpoints.Users;

[EnableRateLimiting("login-policy")]
public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    public required MemDbContext DbContext { get; set; } = null!;

    public override void Configure()
    {
        Post("/user/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "用户登录";
            s.Description = "允许用户使用用户名和密码登录，成功后返回 JWT 令牌";
            s.Responses[200] = "登录成功，返回 JWT 令牌";
            s.Responses[401] = "用户名或密码错误，或用户未激活";
        });
    }
    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = DbContext.Users.FirstOrDefault(u => u.Username == req.Username);
        if (user != null)
        {
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                await Send.UnauthorizedAsync(ct);
                return;
            }

            if (user.IsActive && BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            {
                user.LastLoginAt = DateTimeOffset.UtcNow;
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
                var token = JwtBearer.CreateToken(
        o =>
                        {
                            o.SigningKey = Config["Jwt:SecretKey"]!;
                            o.Issuer = Config["Jwt:Issuer"];
                            o.Audience = Config["Jwt:Audience"];
                            o.ExpireAt = DateTime.UtcNow.AddDays(1);
                            o.User.Claims.Add(new("UserId", user.Id.ToString()));
                            o.User.Claims.Add(new("UserName", user.Username));
                            o.User.Roles.Add(user.Role);
                        }
                    );
                var response = new LoginResponse()
                {
                    Token = token,
                    Message = "登录成功"
                };
                await Send.OkAsync(response, ct);
                await DbContext.SaveChangesAsync(ct);
                return;
            }

            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
                user.AccessFailedCount = 0;
                await Send.UnauthorizedAsync(ct);
            }
            await DbContext.SaveChangesAsync(ct);
        }
        await Send.UnauthorizedAsync(ct);
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
