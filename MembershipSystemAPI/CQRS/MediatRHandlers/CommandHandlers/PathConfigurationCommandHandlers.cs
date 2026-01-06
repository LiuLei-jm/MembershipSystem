using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Repositories;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.CommandHandlers;

public class UpdatePathConfigurationCommandHandler : IRequestHandler<UpdatePathConfigurationCommand, PathConfigurationResponse?>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePathConfigurationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PathConfigurationResponse?> Handle(UpdatePathConfigurationCommand command, CancellationToken cancellationToken)
    {
        // 验证请求参数
        if (string.IsNullOrWhiteSpace(command.BasePath))
            throw new ArgumentException("基础路径不能为空", nameof(command.BasePath));

        if (string.IsNullOrWhiteSpace(command.MembershipCardFilePath))
            throw new ArgumentException("会员卡文件路径不能为空", nameof(command.MembershipCardFilePath));

        // 验证文件名不包含无效字符
        var invalidChars = Path.GetInvalidFileNameChars();
        if (command.MembershipCardFilePath.Any(c => invalidChars.Contains(c)))
            throw new ArgumentException("会员卡文件路径包含无效字符", nameof(command.MembershipCardFilePath));

        var user = await _unitOfWork.Users.GetByIdWithDetailsAsync(command.UserId);
        if (user == null)
        {
            throw new ArgumentException("用户未找到", nameof(command.UserId));
        }

        // 确保 PathConfiguration 不为空
        if (user.PathConfiguration == null)
        {
            user.PathConfiguration = PathConfiguration.Create(user.Id);
        }

        // 验证路径安全性
        if (!IsPathValid(command.BasePath))
        {
            throw new ArgumentException("无效的基础路径", nameof(command.BasePath));
        }

        // 更新路径配置
        user.PathConfiguration.BasePath = command.BasePath;
        user.PathConfiguration.MembershipCardFilePath = command.MembershipCardFilePath;
        user.PathConfiguration.AllowCustomPaths = command.AllowCustomPaths;
        user.PathConfiguration.UpdatedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new PathConfigurationResponse(
            user.PathConfiguration.Id,
            user.PathConfiguration.BasePath,
            user.PathConfiguration.MembershipCardFilePath,
            user.PathConfiguration.AllowCustomPaths,
            user.PathConfiguration.CreatedAt,
            user.PathConfiguration.UpdatedAt
        );
    }

    private bool IsPathValid(string path)
    {
        // 安全检查：防止路径遍历攻击
        if (string.IsNullOrWhiteSpace(path))
            return false;

        // 检查无效字符
        var invalidChars = Path.GetInvalidPathChars();
        if (path.Any(c => invalidChars.Contains(c)))
            return false;

        // 防止路径遍历攻击
        if (path.Contains(".."))
            return false;

        // 检查是否为有效的Windows路径格式
        // 允许驱动器字母如"C:", "D:"等
        if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
        {
            // 有效的驱动器字母格式
            return true;
        }

        // 允许UNC路径
        if (path.StartsWith(@"\\"))
        {
            return true;
        }

        // 对于其他路径，确保它们不以特殊字符开头
        if (path.StartsWith("/") || path.StartsWith("\\"))
        {
            return false;
        }

        return true;
    }
}

public class UpdateBasePathCommandHandler : IRequestHandler<UpdateBasePathCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBasePathCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateBasePathCommand command, CancellationToken cancellationToken)
    {
        return await _unitOfWork.PathConfigurations.UpdateBasePathAsync(command.UserId, command.NewBasePath);
    }
}

public class UpdateMembershipCardFilePathCommandHandler : IRequestHandler<UpdateMembershipCardFilePathCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMembershipCardFilePathCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateMembershipCardFilePathCommand command, CancellationToken cancellationToken)
    {
        return await _unitOfWork.PathConfigurations.UpdateMembershipCardFilePathAsync(command.UserId, command.NewFilePath);
    }
}