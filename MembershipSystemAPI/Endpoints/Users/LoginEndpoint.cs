using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.Users;


[EnableRateLimiting("login-policy")]
public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _appConfig;

    public LoginEndpoint(IMediator mediator, IConfiguration appConfig)
    {
        _mediator = mediator;
        _appConfig = appConfig;
    }

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
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new AuthenticateUserCommand(req.Username, req.Password, ipAddress);
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var token = JwtBearer.CreateToken(
            o =>
            {
                o.SigningKey = _appConfig["Jwt:SecretKey"]!;
                o.Issuer = _appConfig["Jwt:Issuer"];
                o.Audience = _appConfig["Jwt:Audience"];
                o.ExpireAt = DateTime.UtcNow.AddDays(1);
                o.User.Claims.Add(new("UserId", result.UserId?.ToString()!));
                o.User.Claims.Add(new("UserName", result.Username!));
                o.User.Roles.Add(result.Role!);
            }
        );

        var response = new LoginResponse(token, result.Message ?? "登录成功");

        await Send.OkAsync(response, ct);
    }
}