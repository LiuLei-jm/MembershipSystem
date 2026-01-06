namespace MembershipSystemAPI.CQRS.MediatRCommands;

// 文件推送相关的命令
public record PushToConnectionCommand(
    Guid CurrentUserId,
    string TargetConnectionId,
    string FilePath,
    string Content,
    string LogMessage
) : IRequest<PushToConnectionCommandResult>;

// 文件推送命令结果
public record PushToConnectionCommandResult(
    bool Success,
    string Message = ""
);

public record SendAppendCommand(
    Guid CurrentUserId,
    string FilePath,
    string Content,
    string LogMessage
) : IRequest<SendAppendCommandResult>;

public record SendAppendCommandResult(
    bool Success,
    string Message = ""
);

public record SendDeleteCommand(
    Guid CurrentUserId,
    string FilePath,
    string ContentToRemove,
    string LogMessage
) : IRequest<SendDeleteCommandResult>;

public record SendDeleteCommandResult(
    bool Success,
    string Message = ""
);