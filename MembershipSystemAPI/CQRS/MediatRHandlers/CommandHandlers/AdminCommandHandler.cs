using MembershipSystemAPI.Repositories;

namespace MembershipSystemAPI.CQRS.MediatRHandlers.CommandHandlers;

public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, ChangeUserPasswordResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangeUserPasswordCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChangeUserPasswordResult> Handle(ChangeUserPasswordCommand req, CancellationToken cancellationToken)
    {
        var userToChange = await _unitOfWork.Users.GetByIdAsync(req.UserId);
        if (userToChange == null)
        {
            return new ChangeUserPasswordResult(false, "用户不存在");
        }
        var newPassword = req.NewPassword.Trim();

        if (string.IsNullOrEmpty(req.NewPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
        {
            newPassword = "88888888";
        }

        if (req.NewPassword.Length < 6)
        {
            return new ChangeUserPasswordResult(false, "密码长度不能低于6位");
        }

        userToChange.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _unitOfWork.SaveChangesAsync();
        return new ChangeUserPasswordResult(true, "密码修改成功");
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, DeleteUserResult>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<DeleteUserResult> Handle(DeleteUserCommand req, CancellationToken cancellationToken)
    {
        var userToDelete = await _unitOfWork.Users.GetByIdAsync(req.UserId);
        if (userToDelete == null)
        {
            return new DeleteUserResult(false, "用户不存在");
        }
        if (userToDelete.Role == Domain.Enums.UserRole.Admin.ToString())
        {
            return new DeleteUserResult(false, "无法删除管理员账户");
        }
        await _unitOfWork.Users.DeleteAsync(userToDelete.Id);
        await _unitOfWork.SaveChangesAsync();
        return new DeleteUserResult(true, "用户已删除");
    }
}

public class ToggleStatusCommandHandler : IRequestHandler<ToggleStatusCommand, ToggleStatusResult>
{
    private readonly IUnitOfWork _unitOfWork;
    public ToggleStatusCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<ToggleStatusResult> Handle(ToggleStatusCommand req, CancellationToken cancellationToken)
    {
        var userToToggle = await _unitOfWork.Users.GetByIdAsync(req.UserId);
        if (userToToggle == null)
        {
            return new ToggleStatusResult(false, "用户不存在");
        }
        if (userToToggle.Role == Domain.Enums.UserRole.Admin.ToString())
        {
            return new ToggleStatusResult(false, "无法禁用管理员账户");
        }
        userToToggle.IsActive = req.IsActive;
        await _unitOfWork.SaveChangesAsync();
        var message = userToToggle.IsActive ? "用户已启用" : "用户已禁用";
        return new ToggleStatusResult(true, message);
    }
}
