namespace MembershipSystemAPI.DTOs;

// DTO for push to connection request

/// <summary>
/// DTO for push to connection request
/// </summary>
public record PushToConnectionRequest
{
    public string TargetConnectionId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string LogMessage { get; set; } = string.Empty;
}
public record SendAppendRequest
(
     string FilePath,
     string Content,
     string LogMessage);

public record SendDeleteRequest
(
     string FilePath,
     string ContentToRemove,
     string LogMessage);


// DTO for push to connection response

/// <summary>
/// DTO for push to connection response
/// </summary>
/// <param name="Message"></param>
public record PushToConnectionResponse
(
    bool Success,
     string Message = "命令已发送至指定的客户端."
);

public record SendAppendResponse
(
    bool Success,
      string Message = ""
);

public record SendDeleteResponse
(
    bool Success,
      string Message = ""
);
