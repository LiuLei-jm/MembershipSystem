using MembershipSystemAPI.Models;
using MembershipSystemAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace MembershipSystemAPI.Endpoints.Users;

public class GetPathConfigurationEndpoint : EndpointWithoutRequest<PathConfigurationResponse>
{
    private readonly IPathService _pathService;

    public GetPathConfigurationEndpoint(IPathService pathService)
    {
        _pathService = pathService;
    }

    public override void Configure()
    {
        Get("/user/path-config");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "获取用户路径配置";
            s.Description = "获取当前用户的路径配置信息";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        try
        {
            var pathConfig = await _pathService.GetUserPathConfigurationAsync(userId);
            await Send.OkAsync(new PathConfigurationResponse
            {
                BasePath = pathConfig.BasePath,
                MembershipCardFilePath = pathConfig.MembershipCardFilePath,
                AllowCustomPaths = pathConfig.AllowCustomPaths
            }, cancellation: ct);
        }
        catch (ArgumentException)
        {
            await Send.NotFoundAsync(ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "获取用户路径配置时发生错误");
            await Send.ErrorsAsync(500, ct);
        }
    }
}

public class UpdatePathConfigurationEndpoint : Endpoint<PathConfigurationUpdateRequest, PathConfigurationResponse>
{
    private readonly IPathService _pathService;

    public UpdatePathConfigurationEndpoint(IPathService pathService)
    {
        _pathService = pathService;
    }

    public override void Configure()
    {
        Put("/user/path-config");
        Roles("User", "Admin");
        Summary(s =>
        {
            s.Summary = "更新用户路径配置";
            s.Description = "更新当前用户的路径配置信息";
        });
    }

    public override async Task HandleAsync(PathConfigurationUpdateRequest req, CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        try
        {
            var updatedConfig = await _pathService.UpdateUserPathConfigurationAsync(userId, req);
            await Send.OkAsync(new PathConfigurationResponse
            {
                BasePath = updatedConfig.BasePath,
                MembershipCardFilePath = updatedConfig.MembershipCardFilePath,
                AllowCustomPaths = updatedConfig.AllowCustomPaths
            }, cancellation: ct);
        }
        catch (ArgumentException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(400, ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "更新用户路径配置时发生错误");
            await Send.ErrorsAsync(500, ct);
        }
    }
}

public class PathConfigurationResponse
{
    public string BasePath { get; set; } = "D:";
    public string MembershipCardFilePath { get; set; } = "CDK.txt";
    public bool AllowCustomPaths { get; set; } = true;
}