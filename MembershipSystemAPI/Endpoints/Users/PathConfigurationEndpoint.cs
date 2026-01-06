using MembershipSystemAPI.DTOs;

namespace MembershipSystemAPI.Endpoints.Users;

public class GetPathConfigurationEndpoint : EndpointWithoutRequest<PathConfigurationResponse>
{
    private readonly IMediator _mediator;

    public GetPathConfigurationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
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
            var query = new GetPathConfigurationQuery(userId);
            var pathConfig = await _mediator.Send(query, ct);

            if (pathConfig == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            await Send.OkAsync(pathConfig, cancellation: ct);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "获取用户路径配置时发生错误");
            await Send.ErrorsAsync(500, ct);
        }
    }
}

public class UpdatePathConfigurationEndpoint : Endpoint<UpdatePathConfigurationRequest, PathConfigurationResponse>
{
    private readonly IMediator _mediator;

    public UpdatePathConfigurationEndpoint(IMediator mediator)
    {
        _mediator = mediator;
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

    public override async Task HandleAsync(UpdatePathConfigurationRequest req, CancellationToken ct)
    {
        var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

        if (string.IsNullOrEmpty(currentUserId) || !Guid.TryParse(currentUserId, out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        try
        {
            req.Validate(); // 验证请求参数

            var command = new UpdatePathConfigurationCommand(
                userId,
                req.BasePath,
                req.MembershipCardFilePath,
                req.AllowCustomPaths
            );

            var updatedConfig = await _mediator.Send(command, ct);

            if (updatedConfig == null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            await Send.OkAsync(updatedConfig, cancellation: ct);
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
