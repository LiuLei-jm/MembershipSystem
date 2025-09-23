
namespace MembershipSystemAPI.Endpoints.Admins;

public class GetAllUsers : EndpointWithoutRequest<IEnumerable<UserDto>>
{
    private readonly MemDbContext _dbContext;

    public GetAllUsers(MemDbContext dbContext)
    {
        _dbContext = dbContext;
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
        var users = await _dbContext.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                IsActive = u.IsActive,
                Role = u.Role
            }).ToListAsync();

        await Send.OkAsync(users, ct);
    }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Role { get; set; } = string.Empty;

}
