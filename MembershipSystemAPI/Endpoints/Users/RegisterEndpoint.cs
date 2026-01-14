using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.Users;


[EnableRateLimiting("register-policy")]
public class RegisterEndpoint : Endpoint<RegisterRequest, EmptyResponse>
{
    private readonly IMediator _mediator;

    public RegisterEndpoint(IMediator mediator)
    {
        _mediator = mediator;
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

        var command = new RegisterUserCommand(req.Username, req.Password);
        var result = await _mediator.Send(command, ct);

        if (!result.Success)
        {
            await Send.ErrorsAsync(409, ct);
            return;
        }

        await Send.OkAsync(ct);
    }
}

