using FluentValidation;

namespace MembershipSystemAPI.DTOs;

// Authentication Request DTOs Validators
/// <summary>
/// Validator for LoginRequest DTO
/// </summary>
public class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .MaximumLength(50).WithMessage("用户名不能超过50个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空");
    }
}

/// <summary>
/// Validator for RegisterRequest DTO
/// </summary>
public class RegisterRequestValidator : Validator<RegisterRequest>
{
    public RegisterRequestValidator()
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