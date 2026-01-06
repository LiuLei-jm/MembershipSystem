using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.Admins;

public class GetAllUsers : EndpointWithoutRequest<IEnumerable<UserDto>>
{
    private readonly IMediator _mediator;

    public GetAllUsers(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/admin/users");
        Roles("Admin");
        Summary(s =>
        {
            s.Summary = "获取所有用户的信息";
            s.Description = "此端点需要管理员权限。它会返回系统中所有用户的基本信息列表，包括用户 ID、用户名、激活状态和角色。";
            s.Responses[200] = "成功返回用户信息列表";
            s.Responses[401] = "未授权的访问";
            s.Responses[403] = "禁止访问，缺少管理员权限";
        });
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetAllUsersQuery();
        var users = await _mediator.Send(query, ct);

        await Send.OkAsync(users, ct);
    }
}

