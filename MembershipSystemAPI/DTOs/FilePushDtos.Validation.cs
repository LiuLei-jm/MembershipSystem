using FluentValidation;

namespace MembershipSystemAPI.DTOs;

// File Push Request DTOs Validators
/// <summary>
/// Validator for PushToConnectionRequest DTO
/// </summary>
public class PushToConnectionRequestValidator : Validator<PushToConnectionRequest>
{
    public PushToConnectionRequestValidator()
    {
        RuleFor(x => x.TargetConnectionId)
            .NotEmpty().WithMessage("目标连接ID不能为空");

        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("文件路径不能为空")
            .MaximumLength(260).WithMessage("文件路径不能超过260个字符");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("内容不能为空");

        RuleFor(x => x.LogMessage)
            .MaximumLength(500).WithMessage("日志消息不能超过500个字符");
    }
}

/// <summary>
/// Validator for SendAppendRequest DTO
/// </summary>
public class SendAppendRequestValidator : Validator<SendAppendRequest>
{
    public SendAppendRequestValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("文件路径不能为空")
            .MaximumLength(260).WithMessage("文件路径不能超过260个字符");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("内容不能为空");

        RuleFor(x => x.LogMessage)
            .MaximumLength(500).WithMessage("日志消息不能超过500个字符");
    }
}

/// <summary>
/// Validator for SendDeleteRequest DTO
/// </summary>
public class SendDeleteRequestValidator : Validator<SendDeleteRequest>
{
    public SendDeleteRequestValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty().WithMessage("文件路径不能为空")
            .MaximumLength(260).WithMessage("文件路径不能超过260个字符");

        RuleFor(x => x.ContentToRemove)
            .NotEmpty().WithMessage("要删除的内容不能为空");

        RuleFor(x => x.LogMessage)
            .MaximumLength(500).WithMessage("日志消息不能超过500个字符");
    }
}