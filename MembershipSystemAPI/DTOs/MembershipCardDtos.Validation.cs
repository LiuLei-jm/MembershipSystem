using FluentValidation;

namespace MembershipSystemAPI.DTOs;

// Membership Card Request DTOs Validators
/// <summary>
/// Validator for CreateMembershipRequest DTO
/// </summary>
public class CreateMembershipRequestValidator : Validator<CreateMembershipRequest>
{
    public CreateMembershipRequestValidator()
    {
        RuleFor(x => x.MembershipName)
            .NotEmpty().WithMessage("会员名称不能为空")
            .MaximumLength(100).WithMessage("会员名称不能超过100个字符");

        RuleFor(x => x.DurationInDays)
            .GreaterThan(0).WithMessage("会员期限必须大于0天");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("金额不能为负数");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("开始时间不能为空");

        When(x => !string.IsNullOrEmpty(x.Notes), () =>
        {
            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("备注不能超过500个字符");
        });
    }
}

/// <summary>
/// Validator for UpdateMembershipCardRequest DTO
/// </summary>
public class UpdateMembershipCardRequestValidator : Validator<UpdateMembershipCardRequest>
{
    public UpdateMembershipCardRequestValidator()
    {
        When(x => !string.IsNullOrEmpty(x.MembershipName), () =>
        {
            RuleFor(x => x.MembershipName)
                .MaximumLength(100).WithMessage("会员名称不能超过100个字符");
        });

        When(x => !string.IsNullOrEmpty(x.Notes), () =>
        {
            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("备注不能超过500个字符");
        });
    }
}

/// <summary>
/// Validator for UpdateMembershipRequest DTO
/// </summary>
public class UpdateMembershipRequestValidator : Validator<UpdateMembershipRequest>
{
    public UpdateMembershipRequestValidator()
    {
        When(x => x.DurationInDays.HasValue, () =>
        {
            RuleFor(x => x.DurationInDays)
                .GreaterThan(0).WithMessage("会员期限必须大于0天");
        });

        When(x => x.Amount.HasValue, () =>
        {
            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("金额不能为负数");
        });

        When(x => x.StartTime.HasValue, () =>
        {
            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("开始时间不能为空");
        });

        When(x => !string.IsNullOrEmpty(x.Notes), () =>
        {
            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("备注不能超过500个字符");
        });
    }
}

/// <summary>
/// Validator for DeleteMembershipRequest DTO
/// </summary>
public class DeleteMembershipRequestValidator : Validator<DeleteMembershipRequest>
{
    public DeleteMembershipRequestValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage("会员卡ID不能为空");
    }
}