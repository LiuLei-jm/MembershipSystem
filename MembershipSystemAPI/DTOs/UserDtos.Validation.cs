using FluentValidation;

namespace MembershipSystemAPI.DTOs;

// User Request DTOs Validators
/// <summary>
/// Validator for CreateUserRequest DTO
/// </summary>
public class CreateUserRequestValidator : Validator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .MinimumLength(3).WithMessage("用户名至少需要3个字符")
            .MaximumLength(50).WithMessage("用户名不能超过50个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MinimumLength(6).WithMessage("密码至少需要6个字符")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("密码必须包含小写字母、大写字母和数字");
    }
}

/// <summary>
/// Validator for UpdateUserRequest DTO
/// </summary>
public class UpdateUserRequestValidator : Validator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        When(x => !string.IsNullOrEmpty(x.Password), () =>
        {
            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage("密码至少需要6个字符")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("密码必须包含小写字母、大写字母和数字");
        });
    }
}

/// <summary>
/// Validator for ChangePasswordRequest DTO
/// </summary>
public class ChangePasswordRequestValidator : Validator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("当前密码不能为空");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("新密码不能为空")
            .MinimumLength(6).WithMessage("密码至少需要6个字符")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("密码必须包含小写字母、大写字母和数字")
            .NotEqual(x => x.CurrentPassword).WithMessage("新密码不能与当前密码相同");
    }
}

/// <summary>
/// Validator for UpdatePathConfigurationRequest DTO
/// </summary>
public class UpdatePathConfigurationRequestValidator : Validator<UpdatePathConfigurationRequest>
{
    public UpdatePathConfigurationRequestValidator()
    {
        RuleFor(x => x.BasePath)
            .NotEmpty().WithMessage("基础路径不能为空");

        RuleFor(x => x.MembershipCardFilePath)
            .NotEmpty().WithMessage("会员卡文件路径不能为空");

        RuleFor(x => x)
            .Custom((request, context) =>
            {
                // Validate file name doesn't contain invalid characters
                var invalidChars = Path.GetInvalidFileNameChars();
                if (request.MembershipCardFilePath.Any(c => invalidChars.Contains(c)))
                    context.AddFailure("MembershipCardFilePath", "会员卡文件路径包含无效字符");
            });
    }
}

// Admin Request DTOs Validators
/// <summary>
/// Validator for ChangeUserPasswordRequest DTO
/// </summary>
public class ChangeUserPasswordRequestValidator : Validator<ChangeUserPasswordRequest>
{
    public ChangeUserPasswordRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("用户ID不能为空");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("新密码不能为空")
            .MinimumLength(6).WithMessage("密码至少需要6个字符")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("密码必须包含小写字母、大写字母和数字");
    }
}

/// <summary>
/// Validator for DeleteUserRequest DTO
/// </summary>
public class DeleteUserRequestValidator : Validator<DeleteUserRequest>
{
    public DeleteUserRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("用户ID不能为空");
    }
}

/// <summary>
/// Validator for ToggleStatusRequest DTO
/// </summary>
public class ToggleStatusRequestValidator : Validator<ToggleStatusRequest>
{
    public ToggleStatusRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("用户ID不能为空");
    }
}